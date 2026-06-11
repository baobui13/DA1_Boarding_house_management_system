import { Fragment, useEffect, useMemo, useRef, useState } from "react";
import { CircleAlert, CircleCheck, Clock3, Droplets, Filter, LoaderCircle, Plus, X, Zap } from "lucide-react";
import { useApp } from "../../context/AppContext";
import { formatCurrency } from "../../lib/format";
import { createInvoice, getInvoices, updateInvoice, type InvoiceResponse } from "../../lib/invoices";
import { getContracts } from "../../lib/contracts";
import { isOccupyingContractStatus } from "../../lib/contractStatus";
import { getUserByEmail, getUsers } from "../../lib/users";
import { getPropertyListing, getPropertyListings } from "../../lib/properties";
import { getAreas } from "../../lib/areas";

type InvoiceViewStatus = "all" | "unpaid" | "paid" | "overdue";

type InvoiceRow = {
  invoice: InvoiceResponse;
  propertyName: string;
  areaName: string;
  tenantName: string;
  displayStatus: Exclude<InvoiceViewStatus, "all">;
};

type BillableRoom = {
  contractId: string;
  propertyId: string;
  propertyName: string;
  areaId: string | null;
  areaName: string;
  tenantName: string;
  rentAmount: number;
  electricPrice: number;
  waterPrice: number;
};

type DraftInvoiceForm = {
  enabled: boolean;
  oldElectricityReading: string;
  newElectricityReading: string;
  oldWaterReading: string;
  newWaterReading: string;
  otherFees: string;
  penalty: string;
  dueDate: string;
  note: string;
};

const INVOICE_PAGE_SIZE = 12;

export default function BillingPage() {
  const { token, currentUser } = useApp();
  const [invoices, setInvoices] = useState<InvoiceResponse[]>([]);
  const [latestInvoiceByContractId, setLatestInvoiceByContractId] = useState<Map<string, InvoiceResponse>>(new Map());
  const [billableRooms, setBillableRooms] = useState<BillableRoom[]>([]);
  const [contractsById, setContractsById] = useState<Map<string, Awaited<ReturnType<typeof getContracts>>["items"][number]>>(new Map());
  const [usersById, setUsersById] = useState<Map<string, Awaited<ReturnType<typeof getUsers>>["items"][number]>>(new Map());
  const [propertiesById, setPropertiesById] = useState<Map<string, Awaited<ReturnType<typeof getPropertyListings>>["items"][number]>>(new Map());
  const [areasById, setAreasById] = useState<Map<string, Awaited<ReturnType<typeof getAreas>>["items"][number]>>(new Map());
  const [pageNumber, setPageNumber] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [statusFilter, setStatusFilter] = useState<InvoiceViewStatus>("all");
  const [expandedInvoiceId, setExpandedInvoiceId] = useState<string | null>(null);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [creatingInvoices, setCreatingInvoices] = useState(false);
  const [updatingInvoiceId, setUpdatingInvoiceId] = useState<string | null>(null);
  const [createError, setCreateError] = useState("");
  const [draftPeriod, setDraftPeriod] = useState(getCurrentMonthValue());
  const [drafts, setDrafts] = useState<Record<string, DraftInvoiceForm>>({});

  const loadReferenceData = async () => {
    if (!token || !currentUser) {
      setError("Thiếu thông tin đăng nhập để tải hóa đơn.");
      setLoading(false);
      return;
    }

    setLoading(true);
    setError("");

    try {
      const landlord = currentUser.email ? await getUserByEmail(currentUser.email, token) : null;
      const landlordId = landlord?.id || currentUser.id;

      const [contractResponse, userResponse, propertyResponse, invoiceResponse] = await Promise.all([
        // Generous but not unlimited page; we scope further client-side using owned properties
        getContracts(token, { pageSize: 300 }),
        getUsers({ role: "Tenant", pageSize: 300 }, token),
        getPropertyListings({ landlordId, pageSize: 300 }),
        getInvoices(token, { page: 1, pageSize: 300 }),
      ]);
      const areaResponse = await getAreas({ landlordId, pageSize: 1000 });

      const ownedPropertyIds = new Set(propertyResponse.items.map((item) => item.id));
      const missingPropertyIds = Array.from(new Set(contractResponse.items.map((item) => item.propertyId))).filter((id) => !ownedPropertyIds.has(id));
      const missingOwnedProperties = await Promise.all(
        missingPropertyIds.map(async (id) => {
          try {
            const property = await getPropertyListing(id);
            return property.landlordId === currentUser.id ? property : null;
          } catch {
            return null;
          }
        }),
      );

      const mergedProperties = [
        ...propertyResponse.items,
        ...missingOwnedProperties.filter((item): item is NonNullable<typeof item> => Boolean(item)),
      ];
      const mergedOwnedPropertyIds = new Set(mergedProperties.map((item) => item.id));
      const scopedContracts = contractResponse.items.filter((item) => mergedOwnedPropertyIds.has(item.propertyId));
      const scopedContractIds = new Set(scopedContracts.map((item) => item.id));
      const scopedInvoices = invoiceResponse.items.filter((item) => scopedContractIds.has(item.contractId));

      const nextContractsById = new Map(scopedContracts.map((item) => [item.id, item]));
      const nextUsersById = new Map(userResponse.items.map((item) => [item.id, item]));
      const nextPropertiesById = new Map(mergedProperties.map((item) => [item.id, item]));
      const nextAreasById = new Map(areaResponse.items.map((item) => [item.id, item]));
      const nextLatestInvoiceByContractId = buildLatestInvoiceByContractMap(scopedInvoices);

      const activeContracts = scopedContracts.filter((item) => isOccupyingContractStatus(item.status));
      const refreshedActiveProperties = await Promise.all(
        activeContracts.map(async (contract) => {
          try {
            const property = await getPropertyListing(contract.propertyId);
            return property.landlordId === landlordId ? property : null;
          } catch {
            return nextPropertiesById.get(contract.propertyId) || null;
          }
        }),
      );

      const refreshedPropertyMap = new Map(
        refreshedActiveProperties
          .filter((item): item is NonNullable<typeof item> => Boolean(item))
          .map((item) => [item.id, item]),
      );

      refreshedPropertyMap.forEach((property, propertyId) => {
        nextPropertiesById.set(propertyId, property);
      });

      setContractsById(nextContractsById);
      setUsersById(nextUsersById);
      setPropertiesById(nextPropertiesById);
      setAreasById(nextAreasById);
      setLatestInvoiceByContractId(nextLatestInvoiceByContractId);
      setInvoices(scopedInvoices);
      setTotalCount(scopedInvoices.length);

      const nextBillableRooms = activeContracts
        .map((contract) => {
          const property = nextPropertiesById.get(contract.propertyId);
          const tenant = nextUsersById.get(contract.tenantId);
          if (!property) return null;

          return {
            contractId: contract.id,
            propertyId: property.id,
            propertyName: property.propertyName,
            areaId: property.areaId || null,
            areaName: (property.areaId ? nextAreasById.get(property.areaId)?.name : null) || "Chưa gán khu",
            tenantName: tenant?.fullName || "Khách thuê chưa rõ",
            rentAmount: property.price,
            electricPrice: Number(property.electricPrice || 0),
            waterPrice: Number(property.waterPrice || 0),
          } satisfies BillableRoom;
        })
        .filter((item): item is BillableRoom => Boolean(item));

      setBillableRooms(nextBillableRooms);
      setDrafts(buildDrafts(nextBillableRooms, nextLatestInvoiceByContractId));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Không tải được hóa đơn.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadReferenceData();
  }, [currentUser?.email, token]);

  useEffect(() => {
    setPageNumber(1);
  }, [statusFilter]);

  const rows = useMemo(
    () =>
      invoices.map((invoice) => {
        const contract = contractsById.get(invoice.contractId);
        const property = contract ? propertiesById.get(contract.propertyId) : null;
        const tenant = contract ? usersById.get(contract.tenantId) : null;

        return {
          invoice,
          propertyName: property?.propertyName || invoice.note || "Phòng không xác định",
          areaName: (property?.areaId ? areasById.get(property.areaId)?.name : null) || property?.address || "Chưa rõ khu",
          tenantName: tenant?.fullName || "Khách thuê chưa rõ",
          displayStatus: resolveInvoiceStatus(invoice),
        } satisfies InvoiceRow;
      }),
    [areasById, contractsById, invoices, propertiesById, usersById],
  );

  const filteredRows = useMemo(
    () => rows.filter((row) => statusFilter === "all" || row.displayStatus === statusFilter),
    [rows, statusFilter],
  );

  const paginatedRows = useMemo(() => {
    const start = (pageNumber - 1) * INVOICE_PAGE_SIZE;
    return filteredRows.slice(start, start + INVOICE_PAGE_SIZE);
  }, [filteredRows, pageNumber]);

  const paidTotal = rows.filter((row) => row.displayStatus === "paid").reduce((sum, row) => sum + row.invoice.total, 0);
  const unpaidTotal = rows.filter((row) => row.displayStatus === "unpaid").reduce((sum, row) => sum + row.invoice.total, 0);
  const overdueCount = rows.filter((row) => row.displayStatus === "overdue").length;

  const openCreateModal = () => {
    setCreateError("");
    setDrafts(buildDrafts(billableRooms, latestInvoiceByContractId));
    setShowCreateModal(true);
  };

  const closeCreateModal = () => {
    setShowCreateModal(false);
    setCreateError("");
  };

  const updateDraft = (contractId: string, patch: Partial<DraftInvoiceForm>) => {
    setDrafts((prev) => ({
      ...prev,
      [contractId]: {
        ...prev[contractId],
        ...patch,
      },
    }));
  };

  const handleCreateInvoices = async () => {
    if (!token) return;

    const selectedRooms = billableRooms.filter((room) => drafts[room.contractId]?.enabled);
    if (selectedRooms.length === 0) {
      setCreateError("Chọn ít nhất một phòng để tạo hóa đơn.");
      return;
    }

    setCreatingInvoices(true);
    setCreateError("");

    try {
      for (const room of selectedRooms) {
        const draft = drafts[room.contractId];
        const oldElectricityReading = Number(draft.oldElectricityReading || 0);
        const newElectricityReading = Number(draft.newElectricityReading || 0);
        const oldWaterReading = Number(draft.oldWaterReading || 0);
        const newWaterReading = Number(draft.newWaterReading || 0);
        const electricityUsage = Math.max(newElectricityReading - oldElectricityReading, 0);
        const waterUsage = Math.max(newWaterReading - oldWaterReading, 0);
        const otherFees = Number(draft.otherFees || 0);
        const penalty = Number(draft.penalty || 0);
        const electricityCost = electricityUsage * room.electricPrice;
        const waterCost = waterUsage * room.waterPrice;
        const total = room.rentAmount + electricityCost + waterCost + otherFees + penalty;

        await createInvoice(token, {
          contractId: room.contractId,
          period: `${draftPeriod}-01T00:00:00.000Z`,
          rentAmount: room.rentAmount,
          oldElectricityReading,
          newElectricityReading,
          electricityCost,
          oldWaterReading,
          newWaterReading,
          waterCost,
          otherFees,
          penalty,
          total,
          note: draft.note || `${room.propertyName} - ${room.tenantName}`,
          dueDate: `${draft.dueDate}T00:00:00.000Z`,
          status: "Pending",
        });
      }

      closeCreateModal();
      await loadReferenceData();
    } catch (err) {
      setCreateError(err instanceof Error ? err.message : "Tạo hóa đơn thất bại.");
    } finally {
      setCreatingInvoices(false);
    }
  };

  const handleMarkPaid = async (row: InvoiceRow) => {
    if (!token || updatingInvoiceId) return;

    setUpdatingInvoiceId(row.invoice.id);
    setError("");
    try {
      await updateInvoice(token, {
        id: row.invoice.id,
        status: "Paid",
      });
      await loadReferenceData();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Không cập nhật được trạng thái hóa đơn.");
    } finally {
      setUpdatingInvoiceId(null);
    }
  };

  const totalPages = Math.max(1, Math.ceil(filteredRows.length / INVOICE_PAGE_SIZE));

  const changePage = (nextPage: number) => {
    if (nextPage === pageNumber) {
      return;
    }

    setPageNumber(nextPage);
    window.scrollTo({ top: 0, behavior: "smooth" });
  };

  return (
    <div className="max-w-7xl mx-auto px-4 py-6">
      <div className="mb-8 flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
        <div>
          <h1 className="text-gray-900" style={{ fontSize: "24px", fontWeight: 800 }}>
            Quản Lý Hóa Đơn
          </h1>
          <p className="mt-1 text-gray-500" style={{ fontSize: "14px", fontWeight: 500 }}>
            Chốt điện nước và gửi hóa đơn hàng tháng
          </p>
        </div>

        <button
          onClick={openCreateModal}
          className="inline-flex items-center gap-2 rounded-2xl bg-orange-500 px-5 py-3 text-white shadow-sm transition-colors hover:bg-orange-600"
          style={{ fontSize: "14px", fontWeight: 700 }}
        >
          <Plus className="h-4 w-4" />
          Tạo hóa đơn
        </button>
      </div>

      <div className="mb-6 grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-4">
        <SummaryCard tone="green" label="Đã thu" value={compactCurrency(paidTotal)} />
        <SummaryCard tone="yellow" label="Chưa thu" value={compactCurrency(unpaidTotal)} />
        <SummaryCard tone="red" label="Quá hạn" value={`${overdueCount} hóa đơn`} />
        <SummaryCard tone="blue" label="Tổng hóa đơn" value={String(totalCount)} />
      </div>

      <div className="mb-5 flex flex-wrap items-center gap-3">
        <div className="rounded-2xl border border-gray-200 bg-white p-3 text-gray-400">
          <Filter className="h-4 w-4" />
        </div>
        {(
          [
            ["all", "Tất cả"],
            ["unpaid", "Chưa TT"],
            ["paid", "Đã TT"],
            ["overdue", "Quá hạn"],
          ] as const
        ).map(([value, label]) => (
          <button
            key={value}
            onClick={() => setStatusFilter(value)}
            className={`rounded-2xl border px-5 py-3 transition-colors ${
              statusFilter === value
                ? "border-orange-300 bg-orange-50 text-orange-600"
                : "border-gray-200 bg-white text-gray-500 hover:border-gray-300"
            }`}
            style={{ fontSize: "14px", fontWeight: 700 }}
          >
            {label}
          </button>
        ))}
      </div>

      <div className="mb-4 flex items-center justify-between gap-3 rounded-2xl border border-gray-100 bg-white px-4 py-3">
        <p className="text-gray-500" style={{ fontSize: "13px" }}>
          {loading ? "Đang tải dữ liệu..." : `Trang ${pageNumber}/${totalPages} · hiển thị ${paginatedRows.length} / ${filteredRows.length} hóa đơn`}
        </p>
        <p className="text-gray-400" style={{ fontSize: "12px" }}>
          Hóa đơn đã được lọc theo đúng tài khoản chủ trọ hiện tại
        </p>
      </div>

      <div className="overflow-hidden rounded-[28px] border border-gray-100 bg-white shadow-sm">
        {error ? (
          <div className="p-6">
            <div className="rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-red-600">{error}</div>
          </div>
        ) : loading ? (
          <div className="space-y-3 p-6">
            {Array.from({ length: 5 }).map((_, index) => (
              <div key={index} className="h-20 animate-pulse rounded-2xl bg-gray-100" />
            ))}
          </div>
        ) : filteredRows.length === 0 ? (
          <div className="flex flex-col items-center justify-center px-6 py-16 text-center text-gray-400">
            <LoaderCircle className="mb-3 h-8 w-8" />
            <p style={{ fontSize: "14px", fontWeight: 600 }}>Không có hóa đơn khớp bộ lọc hiện tại</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full">
              <thead>
                <tr className="border-b border-gray-100 bg-gray-50/70 text-left">
                  <th className="px-5 py-4 text-gray-500" style={{ fontSize: "13px", fontWeight: 800 }}>
                    PHÒNG / KHÁCH THUÊ
                  </th>
                  <th className="px-5 py-4 text-gray-500" style={{ fontSize: "13px", fontWeight: 800 }}>
                    THÁNG
                  </th>
                  <th className="px-5 py-4 text-gray-500" style={{ fontSize: "13px", fontWeight: 800 }}>
                    TỔNG TIỀN
                  </th>
                  <th className="px-5 py-4 text-gray-500" style={{ fontSize: "13px", fontWeight: 800 }}>
                    TRẠNG THÁI
                  </th>
                  <th className="px-5 py-4 text-gray-500" style={{ fontSize: "13px", fontWeight: 800 }}>
                    THAO TÁC
                  </th>
                </tr>
              </thead>
              <tbody>
                {paginatedRows.map((row) => (
                  <Fragment key={row.invoice.id}>
                    <tr className="border-b border-gray-100 last:border-b-0">
                      <td className="px-5 py-4">
                        <p className="text-gray-900" style={{ fontSize: "16px", fontWeight: 700 }}>
                          {row.tenantName}
                        </p>
                        <p className="mt-1 text-gray-400" style={{ fontSize: "13px", fontWeight: 600 }}>
                          {row.propertyName} · {row.areaName}
                        </p>
                      </td>
                      <td className="px-5 py-4">
                        <p className="text-gray-700" style={{ fontSize: "16px", fontWeight: 700 }}>
                          {formatBillingPeriod(row.invoice.period)}
                        </p>
                        <p className="mt-1 text-gray-400" style={{ fontSize: "13px", fontWeight: 600 }}>
                          Hạn: {formatDate(row.invoice.dueDate)}
                        </p>
                      </td>
                      <td className="px-5 py-4">
                        <p className="text-gray-900" style={{ fontSize: "18px", fontWeight: 800 }}>
                          {formatCurrency(row.invoice.total)}
                        </p>
                      </td>
                      <td className="px-5 py-4">
                        <StatusBadge status={row.displayStatus} />
                      </td>
                      <td className="px-5 py-4">
                        <div className="flex flex-wrap items-center gap-2">
                          <button
                            onClick={() => setExpandedInvoiceId((current) => (current === row.invoice.id ? null : row.invoice.id))}
                            className="rounded-2xl bg-blue-50 px-4 py-2 text-blue-600"
                            style={{ fontSize: "13px", fontWeight: 700 }}
                          >
                            Chi tiết
                          </button>
                          {row.displayStatus !== "paid" ? (
                            <button
                              onClick={() => void handleMarkPaid(row)}
                              disabled={updatingInvoiceId === row.invoice.id}
                              className="rounded-2xl bg-green-50 px-4 py-2 text-green-600 disabled:opacity-70"
                              style={{ fontSize: "13px", fontWeight: 700 }}
                            >
                              {updatingInvoiceId === row.invoice.id ? "Đang cập nhật..." : "Đã TT"}
                            </button>
                          ) : null}
                        </div>
                      </td>
                    </tr>
                    {expandedInvoiceId === row.invoice.id ? (
                      <tr className="bg-slate-50/60">
                        <td colSpan={5} className="px-5 pb-5 pt-0">
                          <InvoiceDetailPanel row={row} />
                        </td>
                      </tr>
                    ) : null}
                  </Fragment>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {totalPages > 1 ? (
        <div className="mt-4 flex items-center justify-center gap-2">
          <button
            onClick={() => changePage(pageNumber - 1)}
            disabled={pageNumber === 1}
            className="rounded-xl border border-gray-200 bg-white px-4 py-2 text-gray-600 disabled:opacity-50"
            style={{ fontSize: "13px", fontWeight: 600 }}
          >
            Trước
          </button>
          <span className="text-gray-500" style={{ fontSize: "13px" }}>
            {pageNumber} / {totalPages}
          </span>
          <button
            onClick={() => changePage(pageNumber + 1)}
            disabled={pageNumber === totalPages}
            className="rounded-xl border border-gray-200 bg-white px-4 py-2 text-gray-600 disabled:opacity-50"
            style={{ fontSize: "13px", fontWeight: 600 }}
          >
            Sau
          </button>
        </div>
      ) : null}

      {showCreateModal ? (
        <CreateInvoiceModal
          period={draftPeriod}
          onPeriodChange={setDraftPeriod}
          rooms={billableRooms}
          latestInvoiceByContractId={latestInvoiceByContractId}
          drafts={drafts}
          onDraftChange={updateDraft}
          onClose={closeCreateModal}
          onSubmit={() => void handleCreateInvoices()}
          loading={creatingInvoices}
          error={createError}
        />
      ) : null}
    </div>
  );
}

function buildDrafts(rooms: BillableRoom[], latestInvoiceByContractId: Map<string, InvoiceResponse>) {
  const dueDate = getDefaultDueDate();
  return Object.fromEntries(
    rooms.map((room) => {
      const latestInvoice = latestInvoiceByContractId.get(room.contractId);
      const oldElectricityReading = formatReadingValue(latestInvoice?.newElectricityReading);
      const oldWaterReading = formatReadingValue(latestInvoice?.newWaterReading);

      return [
        room.contractId,
        {
          enabled: false,
          oldElectricityReading,
          newElectricityReading: oldElectricityReading,
          oldWaterReading,
          newWaterReading: oldWaterReading,
          otherFees: "0",
          penalty: "0",
          dueDate,
          note: "",
        } satisfies DraftInvoiceForm,
      ];
    }),
  );
}

function CreateInvoiceModal({
  period,
  onPeriodChange,
  rooms,
  latestInvoiceByContractId,
  drafts,
  onDraftChange,
  onClose,
  onSubmit,
  loading,
  error,
}: {
  period: string;
  onPeriodChange: (value: string) => void;
  rooms: BillableRoom[];
  latestInvoiceByContractId: Map<string, InvoiceResponse>;
  drafts: Record<string, DraftInvoiceForm>;
  onDraftChange: (contractId: string, patch: Partial<DraftInvoiceForm>) => void;
  onClose: () => void;
  onSubmit: () => void;
  loading: boolean;
  error: string;
}) {
  const selectedSectionRef = useRef<HTMLDivElement | null>(null);
  const [searchTerm, setSearchTerm] = useState("");
  const periodOptions = getPeriodOptions();
  const selectedCount = rooms.filter((room) => drafts[room.contractId]?.enabled).length;

  const setRoomsEnabled = (targetRooms: BillableRoom[], enabled: boolean) => {
    targetRooms.forEach((room) => {
      onDraftChange(room.contractId, { enabled });
    });
  };

  const groupRooms = (sourceRooms: BillableRoom[]) =>
    Object.entries(
      sourceRooms.reduce<Record<string, { areaName: string; rooms: BillableRoom[] }>>((acc, room) => {
        const key = room.areaId || "ungrouped";
        const current = acc[key] || {
          areaName: room.areaName || "Chưa gán khu",
          rooms: [],
        };
        current.rooms.push(room);
        acc[key] = current;
        return acc;
      }, {}),
    ).sort((a, b) => a[1].areaName.localeCompare(b[1].areaName, "vi"));

  const normalizedSearch = searchTerm.trim().toLowerCase();
  const filteredRooms = rooms.filter((room) => {
    if (!normalizedSearch) return true;
    return (
      room.propertyName.toLowerCase().includes(normalizedSearch) ||
      room.tenantName.toLowerCase().includes(normalizedSearch)
    );
  });
  const filteredSelectedCount = filteredRooms.filter((room) => drafts[room.contractId]?.enabled).length;
  const allSelected = filteredRooms.length > 0 && filteredSelectedCount === filteredRooms.length;

  const orderedGroups = groupRooms(filteredRooms);
  const selectedRooms = rooms.filter((room) => drafts[room.contractId]?.enabled);
  const selectedGroups = groupRooms(selectedRooms);

  const scrollToSelectedSection = () => {
    selectedSectionRef.current?.scrollIntoView({ behavior: "smooth", block: "start" });
  };

  return (
    <div className="fixed inset-0 z-50 overflow-y-auto bg-black/40 p-4">
      <div className="flex min-h-full items-center justify-center">
        <div className="flex w-full max-w-6xl max-h-[calc(100vh-2rem)] flex-col overflow-hidden rounded-[32px] bg-white shadow-2xl">
          <div className="flex items-center justify-between border-b border-gray-100 px-6 py-5">
            <div>
              <h2 className="text-gray-900" style={{ fontSize: "22px", fontWeight: 800 }}>
                Tạo hóa đơn theo danh sách phòng
              </h2>
              <p className="mt-1 text-gray-500" style={{ fontSize: "14px", fontWeight: 500 }}>
                Chỉ hiển thị các phòng đang có hợp đồng để tạo hóa đơn đúng `ContractId`.
              </p>
            </div>
            <button onClick={onClose} className="rounded-full p-2 text-gray-400 transition-colors hover:bg-gray-100 hover:text-gray-600">
              <X className="h-5 w-5" />
            </button>
          </div>

          <div className="space-y-5 overflow-y-auto px-6 py-5">
            <div className="grid gap-4 md:grid-cols-3">
              <Field label="Kỳ hóa đơn">
                <select
                  value={period}
                  onChange={(e) => onPeriodChange(e.target.value)}
                  className="w-full rounded-2xl border border-gray-200 bg-gray-50 px-4 py-3 focus:outline-none"
                >
                  {periodOptions.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </select>
              </Field>
              <Field label="Tìm phòng / người thuê">
                <input
                  type="text"
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  placeholder="Nhập tên phòng hoặc tên người thuê"
                  className="w-full rounded-2xl border border-gray-200 bg-gray-50 px-4 py-3 focus:outline-none"
                />
              </Field>
            </div>

            <div className="rounded-[28px] border border-orange-100 bg-orange-50/60 p-4">
              <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
                <div>
                  <p className="text-gray-900" style={{ fontSize: "15px", fontWeight: 800 }}>
                    Chọn phòng tạo hóa đơn
                  </p>
                  <p className="mt-1 text-gray-500" style={{ fontSize: "13px", fontWeight: 600 }}>
                    Đã chọn {selectedCount}/{rooms.length} phòng. Bạn có thể chọn riêng từng phòng hoặc áp dụng cho tất cả phòng đang có hợp đồng.
                  </p>
                  {normalizedSearch ? (
                    <p className="mt-1 text-gray-400" style={{ fontSize: "12px", fontWeight: 600 }}>
                      Đang lọc {filteredRooms.length} phòng theo từ khóa: "{searchTerm}"
                    </p>
                  ) : null}
                </div>

                <div className="flex flex-wrap gap-2">
                  {selectedCount > 0 ? (
                    <button
                      type="button"
                      onClick={scrollToSelectedSection}
                      className="rounded-2xl bg-orange-500 px-4 py-2 text-white transition-colors hover:bg-orange-600"
                      style={{ fontSize: "13px", fontWeight: 700 }}
                    >
                      Nhập dữ liệu cho {selectedCount} phòng
                    </button>
                  ) : null}
                  <button
                    type="button"
                    onClick={() => setRoomsEnabled(filteredRooms, true)}
                    className={`rounded-2xl px-4 py-2 transition-colors ${
                      allSelected ? "bg-orange-500 text-white" : "bg-white text-orange-600 border border-orange-200 hover:bg-orange-100"
                    }`}
                    style={{ fontSize: "13px", fontWeight: 700 }}
                  >
                    Tất cả phòng
                  </button>
                  <button
                    type="button"
                    onClick={() => setRoomsEnabled(filteredRooms, false)}
                    className="rounded-2xl border border-gray-200 bg-white px-4 py-2 text-gray-600 transition-colors hover:bg-gray-50"
                    style={{ fontSize: "13px", fontWeight: 700 }}
                  >
                    Bỏ chọn tất cả
                  </button>
                </div>
              </div>
            </div>

            {error ? <div className="rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-red-600">{error}</div> : null}

            <div className="space-y-4">
              {rooms.length === 0 ? (
                <div className="rounded-2xl border border-dashed border-gray-200 px-4 py-10 text-center text-gray-400">
                  Chưa có phòng nào đủ điều kiện tạo hóa đơn.
                </div>
              ) : filteredRooms.length === 0 ? (
                <div className="rounded-2xl border border-dashed border-gray-200 px-4 py-10 text-center text-gray-400">
                  Không tìm thấy phòng hoặc khách thuê phù hợp.
                </div>
              ) : (
                <div className="overflow-hidden rounded-[30px] border border-gray-100 bg-white shadow-sm">
                  <div className="border-b border-gray-100 bg-white px-4 py-4">
                    <p className="text-gray-900" style={{ fontSize: "15px", fontWeight: 800 }}>
                      Danh sách phòng
                    </p>
                    <p className="mt-1 text-gray-500" style={{ fontSize: "13px", fontWeight: 600 }}>
                      Tick để chọn nhiều phòng, hoặc dùng nút trong từng khu để chọn nhanh cả khu.
                    </p>
                  </div>
                  <div className="max-h-[420px] overflow-y-auto">
                    {orderedGroups.map(([groupKey, group]) => {
                      const groupSelectedCount = group.rooms.filter((room) => drafts[room.contractId]?.enabled).length;
                      const groupAllSelected = group.rooms.length > 0 && groupSelectedCount === group.rooms.length;

                      return (
                        <div key={groupKey} className="border-b border-gray-100 last:border-b-0">
                          <div className="flex flex-col gap-3 bg-gray-50/80 px-4 py-4 lg:flex-row lg:items-center lg:justify-between">
                            <div>
                              <p className="text-gray-900" style={{ fontSize: "16px", fontWeight: 800 }}>
                                {group.areaName}
                              </p>
                              <p className="text-gray-500" style={{ fontSize: "12px", fontWeight: 600 }}>
                                {groupSelectedCount}/{group.rooms.length} phòng đang được chọn
                              </p>
                            </div>
                            <div className="flex flex-wrap gap-2">
                              <button
                                type="button"
                                onClick={() => setRoomsEnabled(group.rooms, true)}
                                className={`rounded-2xl px-3 py-2 transition-colors ${
                                  groupAllSelected ? "bg-orange-500 text-white" : "border border-orange-200 bg-white text-orange-600 hover:bg-orange-50"
                                }`}
                                style={{ fontSize: "12px", fontWeight: 700 }}
                              >
                                Chọn cả khu
                              </button>
                              <button
                                type="button"
                                onClick={() => setRoomsEnabled(group.rooms, false)}
                                className="rounded-2xl border border-gray-200 bg-white px-3 py-2 text-gray-600 transition-colors hover:bg-gray-50"
                                style={{ fontSize: "12px", fontWeight: 700 }}
                              >
                                Bỏ chọn khu
                              </button>
                            </div>
                          </div>

                          <div className="divide-y divide-gray-100">
                            {group.rooms.map((room) => {
                              const draft = drafts[room.contractId];
                              return (
                                <label
                                  key={room.contractId}
                                  className={`flex cursor-pointer items-center gap-3 px-4 py-4 transition-colors ${
                                    draft?.enabled ? "bg-orange-50/70" : "bg-white hover:bg-gray-50"
                                  }`}
                                >
                                  <input
                                    type="checkbox"
                                    checked={draft?.enabled || false}
                                    onChange={(e) => onDraftChange(room.contractId, { enabled: e.target.checked })}
                                    className="mt-1 h-4 w-4 rounded border-gray-300 text-orange-500 focus:ring-orange-500"
                                  />
                                  <div className="min-w-0 flex-1">
                                    <div className="grid gap-2 lg:grid-cols-[minmax(0,1.3fr)_minmax(0,1fr)_auto] lg:items-center">
                                      <div className="min-w-0">
                                        <p className="truncate text-gray-900" style={{ fontSize: "15px", fontWeight: 800 }}>
                                          {room.propertyName}
                                        </p>
                                        <p className="mt-1 text-gray-400" style={{ fontSize: "12px", fontWeight: 600 }}>
                                          {group.areaName}
                                        </p>
                                      </div>
                                      <div className="min-w-0">
                                        <p className="truncate text-gray-500" style={{ fontSize: "13px", fontWeight: 700 }}>
                                          {room.tenantName}
                                        </p>
                                        <p className="mt-1 text-gray-400" style={{ fontSize: "12px", fontWeight: 600 }}>
                                          Khách thuê
                                        </p>
                                      </div>
                                      <div className="text-left lg:text-right">
                                        <p className="text-gray-900" style={{ fontSize: "13px", fontWeight: 700 }}>
                                          {formatCurrency(room.rentAmount)}
                                        </p>
                                        <p className="text-gray-400" style={{ fontSize: "12px", fontWeight: 600 }}>
                                          Tiền phòng / tháng
                                        </p>
                                      </div>
                                    </div>
                                  </div>
                                </label>
                              );
                            })}
                          </div>
                        </div>
                      );
                    })}
                  </div>
                </div>
              )}
            </div>

            <div ref={selectedSectionRef} className="border-t border-gray-100 pt-1">
              <div className="mb-4">
                <p className="text-gray-900" style={{ fontSize: "18px", fontWeight: 800 }}>
                  Nhập dữ liệu cho phòng đã chọn
                </p>
                <p className="mt-1 text-gray-500" style={{ fontSize: "13px", fontWeight: 600 }}>
                  Sau khi chọn phòng ở trên, phần nhập chỉ hiển thị đúng các phòng đã chọn.
                </p>
              </div>

              {selectedRooms.length === 0 ? (
                <div className="rounded-2xl border border-dashed border-gray-200 px-4 py-10 text-center text-gray-400">
                  Chưa chọn phòng nào để nhập hóa đơn.
                </div>
              ) : (
                <div className="space-y-4">
                  {selectedGroups.map(([groupKey, group]) => (
                    <div key={groupKey} className="rounded-[30px] border border-gray-100 bg-white p-5 shadow-sm">
                      <div className="mb-4 border-b border-gray-100 pb-4">
                        <p className="text-gray-900" style={{ fontSize: "18px", fontWeight: 800 }}>
                          {group.areaName}
                        </p>
                        <p className="text-gray-500" style={{ fontSize: "13px", fontWeight: 600 }}>
                          {group.rooms.length} phòng sẽ được tạo hóa đơn
                        </p>
                      </div>

                      <div className="space-y-4">
                        {group.rooms.map((room) => {
                        const draft = drafts[room.contractId];
                        const latestInvoice = latestInvoiceByContractId.get(room.contractId);
                        const oldElectricityReading = Number(draft?.oldElectricityReading || 0);
                        const newElectricityReading = Number(draft?.newElectricityReading || 0);
                        const oldWaterReading = Number(draft?.oldWaterReading || 0);
                        const newWaterReading = Number(draft?.newWaterReading || 0);
                        const electricityUsage = Math.max(newElectricityReading - oldElectricityReading, 0);
                        const waterUsage = Math.max(newWaterReading - oldWaterReading, 0);
                        const otherFees = Number(draft?.otherFees || 0);
                        const penalty = Number(draft?.penalty || 0);
                        const electricityCost = electricityUsage * room.electricPrice;
                        const waterCost = waterUsage * room.waterPrice;
                        const total =
                          room.rentAmount +
                          electricityCost +
                          waterCost +
                          otherFees +
                          penalty;

                        return (
                          <div key={room.contractId} className="rounded-[28px] border border-gray-100 bg-gray-50 p-5">
                            <div className="mb-4 flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
                              <div>
                                <p className="text-gray-900" style={{ fontSize: "17px", fontWeight: 800 }}>
                                  {room.propertyName}
                                </p>
                                <p className="text-gray-500" style={{ fontSize: "13px", fontWeight: 600 }}>
                                  {room.tenantName}
                                </p>
                                <p className="mt-1 text-gray-400" style={{ fontSize: "12px", fontWeight: 600 }}>
                                  Giá điện {formatCurrency(room.electricPrice)}/số · Giá nước {formatCurrency(room.waterPrice)}/khối
                                </p>
                              </div>
                              <div className="text-gray-500" style={{ fontSize: "13px", fontWeight: 600 }}>
                                Tiền phòng: <span className="text-gray-900">{formatCurrency(room.rentAmount)}</span>
                              </div>
                            </div>

                            {latestInvoice ? (
                              <div className="mb-4 rounded-2xl border border-blue-100 bg-blue-50/70 px-4 py-3">
                                <p className="text-blue-700" style={{ fontSize: "12px", fontWeight: 700 }}>
                                  Chỉ số cũ đã tự lấy từ hóa đơn gần nhất {formatBillingPeriod(latestInvoice.period)}.
                                </p>
                              </div>
                            ) : null}

                            <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-5">
                              <Field label="Điện cũ">
                                <input type="number" value={draft?.oldElectricityReading || ""} onChange={(e) => onDraftChange(room.contractId, { oldElectricityReading: e.target.value })} className="w-full rounded-2xl border border-gray-200 bg-white px-4 py-3 focus:outline-none" />
                              </Field>
                              <Field label="Điện mới">
                                <input type="number" value={draft?.newElectricityReading || ""} onChange={(e) => onDraftChange(room.contractId, { newElectricityReading: e.target.value })} className="w-full rounded-2xl border border-gray-200 bg-white px-4 py-3 focus:outline-none" />
                                <p className="mt-1 text-gray-400" style={{ fontSize: "11px", fontWeight: 600 }}>
                                  {electricityUsage} số · {formatCurrency(electricityCost)}
                                </p>
                              </Field>
                              <Field label="Nước cũ">
                                <input type="number" value={draft?.oldWaterReading || ""} onChange={(e) => onDraftChange(room.contractId, { oldWaterReading: e.target.value })} className="w-full rounded-2xl border border-gray-200 bg-white px-4 py-3 focus:outline-none" />
                              </Field>
                              <Field label="Nước mới">
                                <input type="number" value={draft?.newWaterReading || ""} onChange={(e) => onDraftChange(room.contractId, { newWaterReading: e.target.value })} className="w-full rounded-2xl border border-gray-200 bg-white px-4 py-3 focus:outline-none" />
                                <p className="mt-1 text-gray-400" style={{ fontSize: "11px", fontWeight: 600 }}>
                                  {waterUsage} khối · {formatCurrency(waterCost)}
                                </p>
                              </Field>
                              <Field label="Phí khác">
                                <input type="number" value={draft?.otherFees || ""} onChange={(e) => onDraftChange(room.contractId, { otherFees: e.target.value })} className="w-full rounded-2xl border border-gray-200 bg-white px-4 py-3 focus:outline-none" />
                              </Field>
                              <Field label="Phạt">
                                <input type="number" value={draft?.penalty || ""} onChange={(e) => onDraftChange(room.contractId, { penalty: e.target.value })} className="w-full rounded-2xl border border-gray-200 bg-white px-4 py-3 focus:outline-none" />
                              </Field>
                              <Field label="Hạn thanh toán">
                                <input type="date" value={draft?.dueDate || ""} onChange={(e) => onDraftChange(room.contractId, { dueDate: e.target.value })} className="w-full rounded-2xl border border-gray-200 bg-white px-4 py-3 focus:outline-none" />
                              </Field>
                            </div>

                            <div className="mt-4 grid gap-4 lg:grid-cols-[1fr_auto] lg:items-end">
                              <Field label="Ghi chú">
                                <input type="text" value={draft?.note || ""} onChange={(e) => onDraftChange(room.contractId, { note: e.target.value })} className="w-full rounded-2xl border border-gray-200 bg-white px-4 py-3 focus:outline-none" placeholder={`${room.propertyName} - ${room.tenantName}`} />
                              </Field>
                              <div className="rounded-2xl bg-white px-5 py-4 text-right">
                                <p className="text-gray-400" style={{ fontSize: "12px", fontWeight: 700 }}>
                                  Tổng tạm tính
                                </p>
                                <p className="mt-1 text-orange-600" style={{ fontSize: "20px", fontWeight: 800 }}>
                                  {formatCurrency(total)}
                                </p>
                                <p className="mt-1 text-gray-400" style={{ fontSize: "11px", fontWeight: 600 }}>
                                  Phòng {formatCurrency(room.rentAmount)} · Điện {formatCurrency(electricityCost)} · Nước {formatCurrency(waterCost)}
                                </p>
                              </div>
                            </div>
                          </div>
                        );
                      })}
                    </div>
                  </div>
                  ))}
                </div>
              )}
            </div>
          </div>

          <div className="flex items-center justify-end gap-3 border-t border-gray-100 bg-white px-6 py-4">
            <button onClick={onClose} className="rounded-2xl border border-gray-200 px-5 py-3 text-gray-600 transition-colors hover:bg-gray-50" style={{ fontSize: "14px", fontWeight: 700 }}>
              Hủy
            </button>
            <button onClick={onSubmit} disabled={loading} className="inline-flex items-center gap-2 rounded-2xl bg-orange-500 px-5 py-3 text-white transition-colors hover:bg-orange-600 disabled:opacity-70" style={{ fontSize: "14px", fontWeight: 700 }}>
              {loading ? <LoaderCircle className="h-4 w-4 animate-spin" /> : <Plus className="h-4 w-4" />}
              Lưu hóa đơn
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <label className="block">
      <span className="mb-1.5 block text-gray-600" style={{ fontSize: "13px", fontWeight: 700 }}>
        {label}
      </span>
      {children}
    </label>
  );
}

function buildLatestInvoiceByContractMap(invoices: InvoiceResponse[]) {
  const latestByContractId = new Map<string, InvoiceResponse>();

  invoices.forEach((invoice) => {
    const current = latestByContractId.get(invoice.contractId);
    if (!current) {
      latestByContractId.set(invoice.contractId, invoice);
      return;
    }

    const currentTime = new Date(current.period || current.createdAt).getTime();
    const nextTime = new Date(invoice.period || invoice.createdAt).getTime();
    if (nextTime >= currentTime) {
      latestByContractId.set(invoice.contractId, invoice);
    }
  });

  return latestByContractId;
}

function formatReadingValue(value?: number | null) {
  return value == null ? "" : String(value);
}

function resolveInvoiceStatus(invoice: InvoiceResponse): Exclude<InvoiceViewStatus, "all"> {
  const normalized = invoice.status.toLowerCase();
  if (normalized === "paid") return "paid";
  if (new Date(invoice.dueDate).getTime() < Date.now()) return "overdue";
  return "unpaid";
}

function formatBillingPeriod(period: string) {
  const date = new Date(period);
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const year = date.getFullYear();
  return `${month}/${year}`;
}

function formatDate(value: string) {
  return new Date(value).toLocaleDateString("vi-VN");
}

function compactCurrency(value: number) {
  if (value >= 1_000_000_000) return `${(value / 1_000_000_000).toFixed(1)}t`;
  if (value >= 1_000_000) return `${(value / 1_000_000).toFixed(1)}tr`;
  return formatCurrency(value);
}

function getCurrentMonthValue() {
  const now = new Date();
  const month = String(now.getMonth() + 1).padStart(2, "0");
  return `${now.getFullYear()}-${month}`;
}

function getPeriodOptions() {
  const now = new Date();
  return Array.from({ length: 12 }).map((_, index) => {
    const date = new Date(now.getFullYear(), now.getMonth() - index, 1);
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const year = date.getFullYear();
    return {
      value: `${year}-${month}`,
      label: `Tháng ${month}/${year}`,
    };
  });
}

function getDefaultDueDate() {
  const now = new Date();
  const date = new Date(now.getFullYear(), now.getMonth(), 5);
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, "0")}-${String(date.getDate()).padStart(2, "0")}`;
}

function SummaryCard({ tone, label, value }: { tone: "green" | "yellow" | "red" | "blue"; label: string; value: string }) {
  const palette = {
    green: "border-green-100 bg-green-50 text-green-700",
    yellow: "border-yellow-100 bg-yellow-50 text-yellow-700",
    red: "border-red-100 bg-red-50 text-red-600",
    blue: "border-blue-100 bg-blue-50 text-blue-600",
  }[tone];

  return (
    <div className={`rounded-[28px] border p-6 ${palette}`}>
      <p style={{ fontSize: "14px", fontWeight: 700 }}>{label}</p>
      <p className="mt-3" style={{ fontSize: "20px", fontWeight: 800 }}>
        {value}
      </p>
    </div>
  );
}

function StatusBadge({ status }: { status: Exclude<InvoiceViewStatus, "all"> }) {
  if (status === "paid") {
    return (
      <span className="inline-flex items-center gap-1.5 rounded-full bg-green-100 px-4 py-2 text-green-700" style={{ fontSize: "13px", fontWeight: 700 }}>
        <CircleCheck className="h-4 w-4" />
        Đã TT
      </span>
    );
  }

  if (status === "overdue") {
    return (
      <span className="inline-flex items-center gap-1.5 rounded-full bg-red-100 px-4 py-2 text-red-600" style={{ fontSize: "13px", fontWeight: 700 }}>
        <CircleAlert className="h-4 w-4" />
        Quá hạn
      </span>
    );
  }

  return (
    <span className="inline-flex items-center gap-1.5 rounded-full bg-yellow-100 px-4 py-2 text-yellow-700" style={{ fontSize: "13px", fontWeight: 700 }}>
      <Clock3 className="h-4 w-4" />
      Chưa TT
    </span>
  );
}

function InvoiceDetailPanel({ row }: { row: InvoiceRow }) {
  const electricityUsage =
    row.invoice.oldElectricityReading != null && row.invoice.newElectricityReading != null
      ? Math.max(row.invoice.newElectricityReading - row.invoice.oldElectricityReading, 0)
      : 0;
  const waterUsage =
    row.invoice.oldWaterReading != null && row.invoice.newWaterReading != null
      ? Math.max(row.invoice.newWaterReading - row.invoice.oldWaterReading, 0)
      : 0;
  const electricityUnitPrice =
    electricityUsage > 0
      ? Math.round((row.invoice.electricityCost || 0) / electricityUsage)
      : 0;
  const waterUnitPrice =
    waterUsage > 0
      ? Math.round((row.invoice.waterCost || 0) / waterUsage)
      : 0;

  return (
    <div className="mt-3 rounded-[28px] border border-blue-100 bg-white p-6 shadow-sm">
      <h3 className="mb-6 text-slate-700" style={{ fontSize: "18px", fontWeight: 800 }}>
        Chi tiết hóa đơn - {row.tenantName} tháng {formatBillingPeriod(row.invoice.period)}
      </h3>

      <div className="grid gap-4 xl:grid-cols-[1.1fr_1.1fr_1fr]">
        <MetricCard
          tone="yellow"
          icon={<Zap className="h-6 w-6" />}
          title="Điện"
          rows={[
            { label: "Chỉ số cũ", value: `${row.invoice.oldElectricityReading || 0} kWh` },
            { label: "Chỉ số mới", value: `${row.invoice.newElectricityReading || 0} kWh` },
            { label: "Sản lượng", value: `${electricityUsage} kWh` },
            { label: "Đơn giá", value: electricityUnitPrice ? `${formatCurrency(electricityUnitPrice)}/kWh` : "-" },
            { label: "Thành tiền", value: formatCurrency(row.invoice.electricityCost || 0), strong: true },
          ]}
        />

        <MetricCard
          tone="blue"
          icon={<Droplets className="h-6 w-6" />}
          title="Nước"
          rows={[
            { label: "Chỉ số cũ", value: `${row.invoice.oldWaterReading || 0} m³` },
            { label: "Chỉ số mới", value: `${row.invoice.newWaterReading || 0} m³` },
            { label: "Sản lượng", value: `${waterUsage} m³` },
            { label: "Đơn giá", value: waterUnitPrice ? `${formatCurrency(waterUnitPrice)}/m³` : "-" },
            { label: "Thành tiền", value: formatCurrency(row.invoice.waterCost || 0), strong: true },
          ]}
        />

        <div className="rounded-[28px] bg-slate-50 p-6">
          <p className="mb-5 text-slate-600" style={{ fontSize: "18px", fontWeight: 800 }}>
            TỔNG KẾT
          </p>
          <SummaryLine label="Tiền phòng" value={formatCurrency(row.invoice.rentAmount)} />
          <SummaryLine label="Điện" value={formatCurrency(row.invoice.electricityCost || 0)} />
          <SummaryLine label="Nước" value={formatCurrency(row.invoice.waterCost || 0)} />
          <SummaryLine label="Dịch vụ" value={formatCurrency(row.invoice.otherFees || 0)} />
          <SummaryLine label="Phạt" value={formatCurrency(row.invoice.penalty || 0)} />
          <div className="mt-4 border-t border-slate-200 pt-4">
            <SummaryLine label="Tổng cộng" value={formatCurrency(row.invoice.total)} emphasize />
          </div>
        </div>
      </div>
    </div>
  );
}

function MetricCard({
  tone,
  icon,
  title,
  rows,
}: {
  tone: "yellow" | "blue";
  icon: React.ReactNode;
  title: string;
  rows: Array<{ label: string; value: string; strong?: boolean }>;
}) {
  const palette = tone === "yellow" ? "bg-yellow-50 text-amber-700 border-yellow-100" : "bg-blue-50 text-blue-700 border-blue-100";

  return (
    <div className={`rounded-[28px] border p-6 ${palette}`}>
      <div className="mb-5 flex items-center gap-3">
        {icon}
        <p style={{ fontSize: "18px", fontWeight: 800 }}>{title}</p>
      </div>
      <div className="space-y-4">
        {rows.map((row) => (
          <div key={row.label} className="flex items-center justify-between gap-4">
            <span className="text-slate-500" style={{ fontSize: "15px", fontWeight: 600 }}>
              {row.label}
            </span>
            <span className={row.strong ? "text-current" : "text-slate-900"} style={{ fontSize: row.strong ? "18px" : "16px", fontWeight: row.strong ? 800 : 700 }}>
              {row.value}
            </span>
          </div>
        ))}
      </div>
    </div>
  );
}

function SummaryLine({ label, value, emphasize = false }: { label: string; value: string; emphasize?: boolean }) {
  return (
    <div className="flex items-center justify-between gap-4 py-1.5">
      <span className={emphasize ? "text-slate-900" : "text-slate-400"} style={{ fontSize: emphasize ? "16px" : "15px", fontWeight: emphasize ? 800 : 700 }}>
        {label}
      </span>
      <span className={emphasize ? "text-orange-600" : "text-slate-900"} style={{ fontSize: emphasize ? "18px" : "16px", fontWeight: 800 }}>
        {value}
      </span>
    </div>
  );
}
