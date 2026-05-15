import { useEffect, useMemo, useState } from "react";
import {
  CalendarSync,
  CheckCircle2,
  Clock,
  Download,
  Eye,
  FileText,
  LoaderCircle,
  Plus,
  RefreshCw,
  UserX,
  X,
  XCircle,
} from "lucide-react";
import { useApp } from "../../context/AppContext";
import { createContract, getContracts, updateContract, type ContractResponse } from "../../lib/contracts";
import { isOccupyingContractStatus } from "../../lib/contractStatus";
import { getPropertyListing, getPropertyListings } from "../../lib/properties";
import { formatCurrency } from "../../lib/format";
import { getUserByEmail, getUsers } from "../../lib/users";
import { getAreas } from "../../lib/areas";
import type { AreaResponse, PropertyListing, UserResponse } from "../../lib/types";

type StatusKey = "all" | "active" | "expired" | "terminated";

type EnrichedContract = {
  contract: ContractResponse;
  property: PropertyListing | null;
  tenant: UserResponse | null;
  area: AreaResponse | null;
  previewText: string;
  remainingDaysText: string;
  remainingDaysValue: number;
};

type ContractFormState = {
  tenantId: string;
  tenantEmail: string;
  tenantName: string;
  tenantPhone: string;
  tenantCitizenId: string;
  propertyId: string;
  startDate: string;
  endDate: string;
  deposit: string;
  terms: string;
};

const CONTRACT_PAGE_SIZE = 8;

function normalizeContractStatus(status: string): StatusKey | "other" {
  const normalized = status.toLowerCase();
  if (normalized === "active") return "active";
  if (normalized === "expired") return "expired";
  if (normalized === "terminated") return "terminated";
  return "other";
}

export default function ContractManagement() {
  const { token, currentUser } = useApp();
  const [contracts, setContracts] = useState<ContractResponse[]>([]);
  const [properties, setProperties] = useState<PropertyListing[]>([]);
  const [areas, setAreas] = useState<AreaResponse[]>([]);
  const [tenants, setTenants] = useState<UserResponse[]>([]);
  const [pageNumber, setPageNumber] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [statusFilter, setStatusFilter] = useState<StatusKey>("all");
  const [expandedContractId, setExpandedContractId] = useState<string | null>(null);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [saving, setSaving] = useState(false);
  const [findingTenant, setFindingTenant] = useState(false);
  const [actingContractId, setActingContractId] = useState<string | null>(null);
  const [createError, setCreateError] = useState("");
  const [form, setForm] = useState<ContractFormState>(() => buildEmptyForm());

  const loadReferenceData = async () => {
    if (!token || !currentUser) {
      setError("Thiếu thông tin đăng nhập để tải hợp đồng.");
      setLoading(false);
      return;
    }

    setLoading(true);
    setError("");

    try {
      const landlord = currentUser.email ? await getUserByEmail(currentUser.email) : null;
      const landlordId = landlord?.id || currentUser.id;

      const [contractResponse, propertyResponse, userResponse, areaResponse] = await Promise.all([
        getContracts(token, { pageSize: 1000 }),
        getPropertyListings({ landlordId, pageSize: 1000 }),
        getUsers({ role: "Tenant", pageSize: 1000 }),
        getAreas({ landlordId, pageSize: 1000 }),
      ]);

      const ownedPropertyIds = new Set(propertyResponse.items.map((item) => item.id));
      const missingPropertyIds = Array.from(new Set(contractResponse.items.map((item) => item.propertyId))).filter((id) => !ownedPropertyIds.has(id));
      const missingOwnedProperties = await Promise.all(
        missingPropertyIds.map(async (id) => {
          try {
            const property = await getPropertyListing(id);
            return property.landlordId === landlordId ? property : null;
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
      const refreshedAvailableProperties = await Promise.all(
        mergedProperties.map(async (property) => {
          try {
            const latestProperty = await getPropertyListing(property.id);
            return latestProperty.landlordId === landlordId ? latestProperty : property;
          } catch {
            return property;
          }
        }),
      );

      setContracts(scopedContracts);
      setProperties(refreshedAvailableProperties);
      setAreas(areaResponse.items);
      setTenants(userResponse.items);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Không tải được hợp đồng.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadReferenceData();
  }, [token, currentUser?.email]);

  useEffect(() => {
    setPageNumber(1);
  }, [statusFilter]);

  const areasById = useMemo(() => new Map(areas.map((item) => [item.id, item])), [areas]);
  const propertiesById = useMemo(() => new Map(properties.map((item) => [item.id, item])), [properties]);
  const tenantsById = useMemo(() => new Map(tenants.map((item) => [item.id, item])), [tenants]);

  const enrichedContracts = useMemo<EnrichedContract[]>(() => {
    return contracts.map((contract) => {
      const property = propertiesById.get(contract.propertyId) || null;
      const tenant = tenantsById.get(contract.tenantId) || null;
      const area = property?.areaId ? areasById.get(property.areaId) || null : null;
      const previewText = buildContractPreview({
        landlordName: currentUser?.name || "Chủ hộ",
        tenantEmail: tenant?.email || "",
        tenantName: tenant?.fullName || `Khách thuê #${contract.tenantId}`,
        tenantPhone: tenant?.phoneNumber || "",
        tenantCitizenId: "",
        tenantAddress: tenant?.address || "",
        propertyName: property?.propertyName || `Tài sản #${contract.propertyId}`,
        rentAmount: property?.price || 0,
        electricPrice: Number(property?.electricPrice || 0),
        waterPrice: Number(property?.waterPrice || 0),
        deposit: contract.deposit,
        startDate: contract.startDate,
        endDate: contract.endDate,
        terms: contract.terms,
      });

      return {
        contract,
        property,
        tenant,
        area,
        previewText,
        ...getRemainingDays(contract),
      };
    });
  }, [areasById, contracts, currentUser?.name, propertiesById, tenantsById]);

  const filteredContracts = useMemo(
    () => enrichedContracts.filter((item) => statusFilter === "all" || normalizeContractStatus(item.contract.status) === statusFilter),
    [enrichedContracts, statusFilter],
  );

  const paginatedContracts = useMemo(() => {
    const start = (pageNumber - 1) * CONTRACT_PAGE_SIZE;
    return filteredContracts.slice(start, start + CONTRACT_PAGE_SIZE);
  }, [filteredContracts, pageNumber]);

  const activeCount = enrichedContracts.filter((item) => normalizeContractStatus(item.contract.status) === "active").length;
  const expiredCount = enrichedContracts.filter((item) => normalizeContractStatus(item.contract.status) === "expired").length;
  const terminatedCount = enrichedContracts.filter((item) => normalizeContractStatus(item.contract.status) === "terminated").length;

  const availableProperties = useMemo(() => {
    const activePropertyIds = new Set(
      contracts
        .filter((item) => isOccupyingContractStatus(item.status))
        .map((item) => item.propertyId),
    );

    return properties.filter((item) => !activePropertyIds.has(item.id));
  }, [contracts, properties]);

  const propertyOptionsByArea = useMemo(() => {
    const grouped = availableProperties.reduce<Record<string, PropertyListing[]>>((acc, property) => {
      const key = property.areaId || "ungrouped";
      acc[key] = [...(acc[key] || []), property];
      return acc;
    }, {});

    return Object.entries(grouped)
      .map(([key, items]) => ({
        key,
        label: key === "ungrouped" ? "Chưa gán khu" : areasById.get(key)?.name || "Khu trọ",
        items: items.sort((a, b) => a.propertyName.localeCompare(b.propertyName, "vi")),
      }))
      .sort((a, b) => a.label.localeCompare(b.label, "vi"));
  }, [areasById, availableProperties]);

  const selectedProperty = availableProperties.find((item) => item.id === form.propertyId) || null;
  const selectedTenant = tenants.find((item) => item.id === form.tenantId) || null;
  const previewTerms = buildContractPreview({
    landlordName: currentUser?.name || "Chủ hộ",
    tenantEmail: form.tenantEmail,
    tenantName: form.tenantName || selectedTenant?.fullName || "Tên khách thuê",
    tenantPhone: form.tenantPhone,
    tenantCitizenId: form.tenantCitizenId,
    tenantAddress: selectedTenant?.address || "",
    propertyName: selectedProperty?.propertyName || "Phòng trọ",
    rentAmount: selectedProperty?.price || 0,
    electricPrice: Number(selectedProperty?.electricPrice || 0),
    waterPrice: Number(selectedProperty?.waterPrice || 0),
    deposit: Number(form.deposit || 0),
    startDate: form.startDate,
    endDate: form.endDate,
    terms: form.terms,
  });

  const openCreateModal = () => {
    setCreateError("");
    const defaultRoom = availableProperties[0] || null;
    const defaultTenant = tenants[0] || null;
    setForm({
      tenantId: defaultTenant?.id || "",
      tenantEmail: defaultTenant?.email || "",
      tenantName: defaultTenant?.fullName || "",
      tenantPhone: defaultTenant?.phoneNumber || "",
      tenantCitizenId: "",
      propertyId: defaultRoom?.id || "",
      startDate: getTodayValue(),
      endDate: getOneYearLaterValue(),
      deposit: defaultRoom ? String(defaultRoom.price) : "",
      terms: "",
    });
    setShowCreateModal(true);
  };

  const handleFindTenantByEmail = async () => {
    if (!form.tenantEmail.trim()) {
      setCreateError("Nhập email tenant để tìm tài khoản.");
      return;
    }

    setFindingTenant(true);
    setCreateError("");
    try {
      const tenant = await getUserByEmail(form.tenantEmail.trim());
      if (tenant.role.toLowerCase() !== "tenant") {
        throw new Error("Email này không thuộc tài khoản tenant.");
      }

      setForm((current) => ({
        ...current,
        tenantId: tenant.id,
        tenantName: tenant.fullName,
        tenantPhone: tenant.phoneNumber || "",
      }));
    } catch (err) {
      setForm((current) => ({
        ...current,
        tenantId: "",
      }));
      setCreateError(err instanceof Error ? err.message : "Không tìm thấy tenant theo email.");
    } finally {
      setFindingTenant(false);
    }
  };

  const handleCreateContract = async () => {
    if (!token) return;
    if (!form.propertyId || !form.tenantId || !form.startDate || !form.endDate || !form.deposit) {
      setCreateError("Điền đầy đủ phòng, khách thuê, thời gian và tiền cọc.");
      return;
    }

    setSaving(true);
    setCreateError("");
    try {
      await createContract(token, {
        propertyId: form.propertyId,
        tenantId: form.tenantId,
        startDate: `${form.startDate}T00:00:00.000Z`,
        endDate: `${form.endDate}T00:00:00.000Z`,
        deposit: Number(form.deposit),
        terms: form.terms || previewTerms,
        status: "Active",
      });
      setShowCreateModal(false);
      await loadReferenceData();
    } catch (err) {
      setCreateError(err instanceof Error ? err.message : "Tạo hợp đồng thất bại.");
    } finally {
      setSaving(false);
    }
  };

  const handleExtend = async (item: EnrichedContract) => {
    if (!token || actingContractId) return;
    const end = new Date(item.contract.endDate);
    const nextEnd = new Date(end.getFullYear() + 1, end.getMonth(), end.getDate());
    setActingContractId(item.contract.id);
    setError("");
    try {
      await updateContract(token, {
        id: item.contract.id,
        endDate: nextEnd.toISOString(),
        status: "Active",
      });
      await loadReferenceData();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Không gia hạn được hợp đồng.");
    } finally {
      setActingContractId(null);
    }
  };

  const handleTerminate = async (item: EnrichedContract) => {
    if (!token || actingContractId) return;
    setActingContractId(item.contract.id);
    setError("");
    try {
      await updateContract(token, {
        id: item.contract.id,
        status: "Terminated",
        actualEndDate: getTodayValue(),
        refundAmount: Math.max(item.contract.deposit - item.contract.deductionAmount, 0),
      });
      await loadReferenceData();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Không chấm dứt được hợp đồng.");
    } finally {
      setActingContractId(null);
    }
  };

  const handleExport = (item: EnrichedContract) => {
    const popup = window.open("", "_blank", "width=900,height=700");
    if (!popup) return;
    popup.document.write(`
      <html>
        <head>
          <title>Hop dong ${item.property?.propertyName || item.contract.id}</title>
          <style>
            body { font-family: Arial, sans-serif; padding: 32px; line-height: 1.7; color: #1f2937; }
            h1 { font-size: 24px; text-align: center; margin-bottom: 24px; }
            pre { white-space: pre-wrap; font-family: Arial, sans-serif; font-size: 16px; }
          </style>
        </head>
        <body>
          <h1>HOP DONG THUE PHONG</h1>
          <pre>${item.previewText}</pre>
        </body>
      </html>
    `);
    popup.document.close();
    popup.focus();
    popup.print();
  };

  return (
    <div className="mx-auto max-w-6xl px-4 py-6">
      <div className="mb-8 flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
        <div>
          <h1 className="text-gray-900" style={{ fontSize: "22px", fontWeight: 800 }}>
            Quản Lý Hợp Đồng
          </h1>
          <p className="mt-1 text-gray-500" style={{ fontSize: "14px", fontWeight: 500 }}>
            Danh sách khách thuê và trạng thái pháp lý
          </p>
        </div>

        <div className="flex flex-wrap items-center gap-3">
          <button
            onClick={() => void loadReferenceData()}
            className="inline-flex items-center gap-2 rounded-2xl border border-gray-200 px-4 py-3 text-gray-600 transition-colors hover:bg-gray-50"
            style={{ fontSize: "14px", fontWeight: 700 }}
          >
            <RefreshCw className="h-4 w-4" />
            Tải lại
          </button>
          <button
            onClick={openCreateModal}
            className="inline-flex items-center gap-2 rounded-2xl bg-orange-500 px-5 py-3 text-white shadow-sm transition-colors hover:bg-orange-600"
            style={{ fontSize: "14px", fontWeight: 700 }}
          >
            <Plus className="h-4 w-4" />
            Tạo hợp đồng
          </button>
        </div>
      </div>

      <div className="mb-6 grid grid-cols-1 gap-4 md:grid-cols-3">
        <SummaryCard tone="green" label="Đang hiệu lực" value={String(activeCount)} />
        <SummaryCard tone="gray" label="Hết hạn" value={String(expiredCount)} />
        <SummaryCard tone="red" label="Đã chấm dứt" value={String(terminatedCount)} />
      </div>

      <div className="mb-5 flex flex-wrap gap-2">
        {(["all", "active", "expired", "terminated"] as StatusKey[]).map((status) => (
          <button
            key={status}
            onClick={() => setStatusFilter(status)}
            className={`rounded-2xl border px-4 py-2.5 transition-colors ${
              statusFilter === status
                ? "border-orange-300 bg-orange-50 text-orange-600"
                : "border-gray-200 bg-white text-gray-500 hover:border-gray-300"
            }`}
            style={{ fontSize: "13px", fontWeight: 700 }}
          >
            {status === "all" ? "Tất cả" : status === "active" ? "Đang hiệu lực" : status === "expired" ? "Hết hạn" : "Đã chấm dứt"}
          </button>
        ))}
      </div>

      <div className="mb-4 flex items-center justify-between gap-3 rounded-2xl border border-gray-100 bg-white px-4 py-3">
        <p className="text-gray-500" style={{ fontSize: "13px" }}>
          {loading ? "Đang tải dữ liệu..." : `Trang ${pageNumber}/${Math.max(1, Math.ceil(filteredContracts.length / CONTRACT_PAGE_SIZE))} · hiển thị ${paginatedContracts.length} / ${filteredContracts.length} hợp đồng`}
        </p>
        <p className="text-gray-400" style={{ fontSize: "12px" }}>
          Hợp đồng đã được giới hạn theo các phòng thuộc tài khoản chủ trọ hiện tại
        </p>
      </div>

      {error ? <div className="mb-4 rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-red-600">{error}</div> : null}

      <div className="space-y-5">
        {loading ? (
          Array.from({ length: 3 }).map((_, index) => <div key={index} className="h-72 animate-pulse rounded-[28px] bg-gray-100" />)
        ) : filteredContracts.length === 0 ? (
          <div className="rounded-[28px] border border-gray-100 bg-white px-6 py-12 text-center text-gray-400">
            <FileText className="mx-auto mb-3 h-8 w-8 opacity-40" />
            <p style={{ fontSize: "14px", fontWeight: 600 }}>Chưa có hợp đồng phù hợp để hiển thị.</p>
          </div>
        ) : (
          paginatedContracts.map((item) => {
            const isExpanded = expandedContractId === item.contract.id;
            const isBusy = actingContractId === item.contract.id;

            return (
              <div key={item.contract.id} className="overflow-hidden rounded-[28px] border border-gray-100 bg-white shadow-sm">
                <div className="p-5">
                  <div className="mb-5 flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
                    <div className="flex items-start gap-4">
                      <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-orange-100 text-orange-500">
                        <FileText className="h-5 w-5" />
                      </div>
                      <div>
                        <p className="text-gray-900" style={{ fontSize: "18px", fontWeight: 800 }}>
                          {item.tenant?.fullName || `Khách thuê #${item.contract.tenantId}`}
                        </p>
                        <p className="mt-1 text-gray-500" style={{ fontSize: "14px", fontWeight: 600 }}>
                          {item.property?.propertyName || `Tài sản #${item.contract.propertyId}`}
                        </p>
                        <div className="mt-2 flex flex-wrap gap-3 text-gray-400" style={{ fontSize: "12px", fontWeight: 600 }}>
                          <span>{item.tenant?.phoneNumber || "Chưa có SĐT"}</span>
                          <span>{item.area?.name || "Chưa gán khu"}</span>
                          <span>{item.contract.id}</span>
                        </div>
                      </div>
                    </div>

                    <StatusPill status={item.contract.status} />
                  </div>

                  <div className="mb-4 grid grid-cols-2 gap-3 lg:grid-cols-4">
                    <InfoBox label="Giá thuê" value={formatCurrency(item.property?.price || 0)} />
                    <InfoBox label="Giá điện" value={`${formatCurrency(item.property?.electricPrice || 0)}/kWh`} />
                    <InfoBox label="Giá nước" value={`${formatCurrency(item.property?.waterPrice || 0)}/m³`} />
                    <InfoBox label="Tiền cọc" value={formatCurrency(item.contract.deposit)} />
                    <InfoBox label="Ngày bắt đầu" value={formatDate(item.contract.startDate)} />
                    <InfoBox label="Ngày kết thúc" value={formatDate(item.contract.endDate)} />
                  </div>

                  <div className="mb-4 flex items-center justify-between gap-4 border-t border-gray-100 pt-4 text-gray-400" style={{ fontSize: "12px", fontWeight: 700 }}>
                    <span>Bắt đầu</span>
                    <span>{item.remainingDaysText}</span>
                  </div>

                  <div className="mb-4 flex flex-wrap gap-2">
                    <ActionButton onClick={() => setExpandedContractId(isExpanded ? null : item.contract.id)} tone="gray">
                      <Eye className="h-4 w-4" />
                      {isExpanded ? "Ẩn hợp đồng" : "Xem hợp đồng"}
                    </ActionButton>
                    <ActionButton onClick={() => handleExport(item)} tone="gray">
                      <Download className="h-4 w-4" />
                      Xuất PDF
                    </ActionButton>
                    <ActionButton onClick={() => void handleExtend(item)} disabled={isBusy} tone="blue">
                      <CalendarSync className="h-4 w-4" />
                      {isBusy ? "Đang xử lý..." : "Gia hạn"}
                    </ActionButton>
                    <ActionButton onClick={() => void handleTerminate(item)} disabled={isBusy || normalizeContractStatus(item.contract.status) === "terminated"} tone="red">
                      <UserX className="h-4 w-4" />
                      Kết thúc HĐ
                    </ActionButton>
                  </div>

                  {isExpanded ? (
                    <div className="rounded-[24px] border border-gray-100 bg-gray-50 px-5 py-6">
                      <p className="mb-4 text-center text-gray-700" style={{ fontSize: "18px", fontWeight: 800 }}>
                        HỢP ĐỒNG THUÊ PHÒNG
                      </p>
                      <pre className="whitespace-pre-wrap text-gray-600" style={{ fontFamily: "inherit", fontSize: "14px", lineHeight: 1.9 }}>
                        {item.previewText}
                      </pre>
                    </div>
                  ) : null}
                </div>
              </div>
            );
          })
        )}
      </div>

      {Math.ceil(filteredContracts.length / CONTRACT_PAGE_SIZE) > 1 ? (
        <div className="mt-4 flex items-center justify-center gap-2">
          <button
            onClick={() => {
              if (pageNumber > 1) {
                setPageNumber(pageNumber - 1);
                window.scrollTo({ top: 0, behavior: "smooth" });
              }
            }}
            disabled={pageNumber === 1}
            className="rounded-xl border border-gray-200 bg-white px-4 py-2 text-gray-600 disabled:opacity-50"
            style={{ fontSize: "13px", fontWeight: 600 }}
          >
            Trước
          </button>
          <span className="text-gray-500" style={{ fontSize: "13px" }}>
            {pageNumber} / {Math.max(1, Math.ceil(filteredContracts.length / CONTRACT_PAGE_SIZE))}
          </span>
          <button
            onClick={() => {
              const totalPages = Math.max(1, Math.ceil(filteredContracts.length / CONTRACT_PAGE_SIZE));
              if (pageNumber < totalPages) {
                setPageNumber(pageNumber + 1);
                window.scrollTo({ top: 0, behavior: "smooth" });
              }
            }}
            disabled={pageNumber >= Math.max(1, Math.ceil(filteredContracts.length / CONTRACT_PAGE_SIZE))}
            className="rounded-xl border border-gray-200 bg-white px-4 py-2 text-gray-600 disabled:opacity-50"
            style={{ fontSize: "13px", fontWeight: 600 }}
          >
            Sau
          </button>
        </div>
      ) : null}

      {showCreateModal ? (
        <CreateContractModal
          form={form}
          onChange={(patch) =>
            setForm((current) => {
              const next = { ...current, ...patch };
              if (patch.tenantId !== undefined) {
                const tenant = tenants.find((item) => item.id === patch.tenantId) || null;
                next.tenantName = tenant?.fullName || "";
                next.tenantPhone = tenant?.phoneNumber || "";
              }
              return next;
            })
          }
          onClose={() => setShowCreateModal(false)}
          onSubmit={() => void handleCreateContract()}
          onFindTenant={() => void handleFindTenantByEmail()}
          findingTenant={findingTenant}
          saving={saving}
          error={createError}
          propertyOptionsByArea={propertyOptionsByArea}
          previewTerms={previewTerms}
          selectedProperty={selectedProperty}
        />
      ) : null}
    </div>
  );
}

function CreateContractModal({
  form,
  onChange,
  onClose,
  onSubmit,
  onFindTenant,
  findingTenant,
  saving,
  error,
  propertyOptionsByArea,
  previewTerms,
  selectedProperty,
}: {
  form: ContractFormState;
  onChange: (patch: Partial<ContractFormState>) => void;
  onClose: () => void;
  onSubmit: () => void;
  onFindTenant: () => void;
  findingTenant: boolean;
  saving: boolean;
  error: string;
  propertyOptionsByArea: Array<{ key: string; label: string; items: PropertyListing[] }>;
  previewTerms: string;
  selectedProperty: PropertyListing | null;
}) {
  return (
    <div className="fixed inset-0 z-50 overflow-y-auto bg-black/40 p-4">
      <div className="flex min-h-full items-center justify-center">
        <div className="flex w-full max-w-5xl max-h-[calc(100vh-2rem)] flex-col overflow-hidden rounded-[32px] bg-white shadow-2xl">
          <div className="flex items-center justify-between border-b border-gray-100 px-6 py-5">
            <div>
              <h2 className="text-gray-900" style={{ fontSize: "22px", fontWeight: 800 }}>
                Tạo hợp đồng mới
              </h2>
              <p className="mt-1 text-gray-500" style={{ fontSize: "14px", fontWeight: 500 }}>
                Chọn phòng đang trống và khách thuê để tạo hợp đồng ngay từ màn quản lý.
              </p>
            </div>
            <button onClick={onClose} className="rounded-full p-2 text-gray-400 transition-colors hover:bg-gray-100 hover:text-gray-600">
              <X className="h-5 w-5" />
            </button>
          </div>

          <div className="space-y-5 overflow-y-auto px-6 py-5">
            <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
              <Field label="Email tenant">
                <div className="flex gap-2">
                  <input
                    type="email"
                    value={form.tenantEmail}
                    onChange={(e) => onChange({ tenantEmail: e.target.value })}
                    className="w-full rounded-2xl border border-gray-200 bg-gray-50 px-4 py-3 focus:outline-none"
                    placeholder="tenant@email.com"
                  />
                  <button
                    type="button"
                    onClick={onFindTenant}
                    disabled={findingTenant}
                    className="rounded-2xl border border-orange-200 bg-orange-50 px-4 py-3 text-orange-600 disabled:opacity-70"
                    style={{ fontSize: "13px", fontWeight: 700 }}
                  >
                    {findingTenant ? "Đang tìm..." : "Tìm"}
                  </button>
                </div>
              </Field>

              <Field label="Tên khách thuê">
                <input
                  type="text"
                  value={form.tenantName}
                  onChange={(e) => onChange({ tenantName: e.target.value })}
                  className="w-full rounded-2xl border border-gray-200 bg-gray-50 px-4 py-3 focus:outline-none"
                  placeholder="Nhập tên khách thuê"
                />
              </Field>

              <Field label="Phòng / tài sản">
                <select
                  value={form.propertyId}
                  onChange={(e) => onChange({ propertyId: e.target.value })}
                  className="w-full rounded-2xl border border-gray-200 bg-gray-50 px-4 py-3 focus:outline-none"
                >
                  <option value="">Chọn phòng</option>
                  {propertyOptionsByArea.map((group) => (
                    <optgroup key={group.key} label={group.label}>
                      {group.items.map((property) => (
                        <option key={property.id} value={property.id}>
                          {property.propertyName}
                        </option>
                      ))}
                    </optgroup>
                  ))}
                </select>
              </Field>

              <Field label="Ngày bắt đầu">
                <input type="date" value={form.startDate} onChange={(e) => onChange({ startDate: e.target.value })} className="w-full rounded-2xl border border-gray-200 bg-gray-50 px-4 py-3 focus:outline-none" />
              </Field>

              <Field label="Ngày kết thúc">
                <input type="date" value={form.endDate} onChange={(e) => onChange({ endDate: e.target.value })} className="w-full rounded-2xl border border-gray-200 bg-gray-50 px-4 py-3 focus:outline-none" />
              </Field>
            </div>

            <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
              <Field label="Số điện thoại">
                <input type="text" value={form.tenantPhone} onChange={(e) => onChange({ tenantPhone: e.target.value })} className="w-full rounded-2xl border border-gray-200 bg-gray-50 px-4 py-3 focus:outline-none" />
              </Field>

              <Field label="Số CCCD">
                <input type="text" value={form.tenantCitizenId} onChange={(e) => onChange({ tenantCitizenId: e.target.value })} className="w-full rounded-2xl border border-gray-200 bg-gray-50 px-4 py-3 focus:outline-none" />
              </Field>

              <Field label="Giá thuê (VNĐ/tháng)">
                <input type="text" value={selectedProperty ? formatCurrency(selectedProperty.price || 0) : ""} readOnly className="w-full rounded-2xl border border-gray-200 bg-gray-100 px-4 py-3 text-gray-600 focus:outline-none" />
              </Field>

              <Field label="Giá điện (VNĐ/kWh)">
                <input type="text" value={selectedProperty ? formatCurrency(selectedProperty.electricPrice || 0) : ""} readOnly className="w-full rounded-2xl border border-gray-200 bg-gray-100 px-4 py-3 text-gray-600 focus:outline-none" />
              </Field>

              <Field label="Giá nước (VNĐ/m³)">
                <input type="text" value={selectedProperty ? formatCurrency(selectedProperty.waterPrice || 0) : ""} readOnly className="w-full rounded-2xl border border-gray-200 bg-gray-100 px-4 py-3 text-gray-600 focus:outline-none" />
              </Field>

              <Field label="Tiền cọc">
                <input type="number" value={form.deposit} onChange={(e) => onChange({ deposit: e.target.value })} className="w-full rounded-2xl border border-gray-200 bg-gray-50 px-4 py-3 focus:outline-none" />
              </Field>
            </div>

            <div className="grid gap-4 md:grid-cols-1">
              <Field label="Điều khoản tùy chỉnh">
                <textarea value={form.terms} onChange={(e) => onChange({ terms: e.target.value })} rows={5} className="w-full rounded-2xl border border-gray-200 bg-gray-50 px-4 py-3 focus:outline-none" placeholder="Có thể để trống để dùng mẫu hợp đồng tự sinh." />
              </Field>
            </div>

            {error ? <div className="rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-red-600">{error}</div> : null}

            <div className="rounded-[28px] border border-gray-100 bg-gray-50 px-5 py-6">
              <p className="mb-4 text-center text-gray-700" style={{ fontSize: "18px", fontWeight: 800 }}>
                Xem trước nội dung hợp đồng
              </p>
              <pre className="whitespace-pre-wrap text-gray-600" style={{ fontFamily: "inherit", fontSize: "14px", lineHeight: 1.9 }}>
                {previewTerms}
              </pre>
            </div>
          </div>

          <div className="flex items-center justify-end gap-3 border-t border-gray-100 bg-white px-6 py-4">
            <button onClick={onClose} className="rounded-2xl border border-gray-200 px-5 py-3 text-gray-600 transition-colors hover:bg-gray-50" style={{ fontSize: "14px", fontWeight: 700 }}>
              Hủy
            </button>
            <button onClick={onSubmit} disabled={saving} className="inline-flex items-center gap-2 rounded-2xl bg-orange-500 px-5 py-3 text-white transition-colors hover:bg-orange-600 disabled:opacity-70" style={{ fontSize: "14px", fontWeight: 700 }}>
              {saving ? <LoaderCircle className="h-4 w-4 animate-spin" /> : <Plus className="h-4 w-4" />}
              Lưu hợp đồng
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}

function SummaryCard({ tone, label, value }: { tone: "green" | "gray" | "red"; label: string; value: string }) {
  const palette = {
    green: "border-green-100 bg-green-50 text-green-700",
    gray: "border-gray-200 bg-white text-slate-700",
    red: "border-red-100 bg-red-50 text-red-600",
  }[tone];

  return (
    <div className={`rounded-[28px] border p-6 text-center ${palette}`}>
      <p style={{ fontSize: "28px", fontWeight: 800 }}>{value}</p>
      <p className="mt-2" style={{ fontSize: "14px", fontWeight: 700 }}>
        {label}
      </p>
    </div>
  );
}

function InfoBox({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-2xl bg-gray-50 px-4 py-3">
      <p className="text-gray-400" style={{ fontSize: "12px", fontWeight: 700 }}>
        {label}
      </p>
      <p className="mt-1 text-gray-800" style={{ fontSize: "16px", fontWeight: 800 }}>
        {value}
      </p>
    </div>
  );
}

function ActionButton({
  children,
  onClick,
  tone,
  disabled = false,
}: {
  children: React.ReactNode;
  onClick: () => void;
  tone: "gray" | "blue" | "red";
  disabled?: boolean;
}) {
  const palette = {
    gray: "bg-white text-gray-600 border-gray-200 hover:bg-gray-50",
    blue: "bg-blue-50 text-blue-600 border-blue-100 hover:bg-blue-100",
    red: "bg-red-50 text-red-500 border-red-100 hover:bg-red-100",
  }[tone];

  return (
    <button
      onClick={onClick}
      disabled={disabled}
      className={`inline-flex items-center gap-2 rounded-2xl border px-4 py-2.5 transition-colors disabled:opacity-60 ${palette}`}
      style={{ fontSize: "14px", fontWeight: 700 }}
    >
      {children}
    </button>
  );
}

function StatusPill({ status }: { status: string }) {
  const normalized = status.toLowerCase();

  if (normalized === "active") {
    return (
      <span className="inline-flex items-center gap-1.5 rounded-full bg-green-100 px-4 py-2 text-green-700" style={{ fontSize: "13px", fontWeight: 700 }}>
        <CheckCircle2 className="h-4 w-4" />
        Đang hiệu lực
      </span>
    );
  }

  if (normalized === "expired") {
    return (
      <span className="inline-flex items-center gap-1.5 rounded-full bg-gray-100 px-4 py-2 text-gray-500" style={{ fontSize: "13px", fontWeight: 700 }}>
        <Clock className="h-4 w-4" />
        Hết hạn
      </span>
    );
  }

  return (
    <span className="inline-flex items-center gap-1.5 rounded-full bg-red-100 px-4 py-2 text-red-600" style={{ fontSize: "13px", fontWeight: 700 }}>
      <XCircle className="h-4 w-4" />
      Đã chấm dứt
    </span>
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

function buildEmptyForm(): ContractFormState {
  return {
    tenantId: "",
    tenantEmail: "",
    tenantName: "",
    tenantPhone: "",
    tenantCitizenId: "",
    propertyId: "",
    startDate: getTodayValue(),
    endDate: getOneYearLaterValue(),
    deposit: "",
    terms: "",
  };
}

function formatDate(value: string) {
  return new Date(value).toLocaleDateString("vi-VN");
}

function getRemainingDays(contract: ContractResponse) {
  const normalized = normalizeContractStatus(contract.status);
  if (normalized === "terminated") {
    return {
      remainingDaysText: "Đã chấm dứt",
      remainingDaysValue: -1,
    };
  }

  const ms = new Date(contract.endDate).getTime() - Date.now();
  const days = Math.ceil(ms / (1000 * 60 * 60 * 24));
  if (days < 0) {
    return {
      remainingDaysText: `Quá hạn ${Math.abs(days)} ngày`,
      remainingDaysValue: days,
    };
  }

  return {
    remainingDaysText: `Còn ${days} ngày`,
    remainingDaysValue: days,
  };
}

function getTodayValue() {
  const now = new Date();
  return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, "0")}-${String(now.getDate()).padStart(2, "0")}`;
}

function getOneYearLaterValue() {
  const now = new Date();
  const next = new Date(now.getFullYear() + 1, now.getMonth(), now.getDate());
  return `${next.getFullYear()}-${String(next.getMonth() + 1).padStart(2, "0")}-${String(next.getDate()).padStart(2, "0")}`;
}

function buildContractPreview(input: {
  landlordName: string;
  tenantEmail: string;
  tenantName: string;
  tenantPhone: string;
  tenantCitizenId: string;
  tenantAddress: string;
  propertyName: string;
  rentAmount: number;
  electricPrice: number;
  waterPrice: number;
  deposit: number;
  startDate: string;
  endDate: string;
  terms?: string | null;
}) {
  if (input.terms && input.terms.trim()) return input.terms;

  return [
    `Hôm nay, ngày ${formatDateForContract(input.startDate || new Date().toISOString())}, các bên gồm:`,
    ``,
    `Bên cho thuê (Bên A): ${input.landlordName} - Chủ hộ`,
    `Bên thuê (Bên B): ${input.tenantName}${input.tenantEmail ? ` - Email: ${input.tenantEmail}` : ""}${input.tenantPhone ? ` - SĐT: ${input.tenantPhone}` : ""}${input.tenantCitizenId ? ` - CCCD: ${input.tenantCitizenId}` : ""}${input.tenantAddress ? ` - Địa chỉ: ${input.tenantAddress}` : ""}`,
    `Tài sản cho thuê: ${input.propertyName}`,
    `Thời hạn: từ ${formatDateForContract(input.startDate)} đến ${formatDateForContract(input.endDate)}.`,
    `Giá thuê: ${formatCurrency(input.rentAmount)}/tháng, thanh toán vào ngày 5 hàng tháng.`,
    `Giá điện: ${formatCurrency(input.electricPrice)}/kWh. Giá nước: ${formatCurrency(input.waterPrice)}/m³.`,
    `Tiền cọc: ${formatCurrency(input.deposit)}, hoàn trả khi hết hợp đồng sau khi đối soát chi phí phát sinh.`,
    ``,
    `[ Điều khoản hợp đồng đầy đủ... ]`,
  ].join("\n");
}

function formatDateForContract(value: string) {
  if (!value) return "...";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return date.toLocaleDateString("vi-VN");
}
