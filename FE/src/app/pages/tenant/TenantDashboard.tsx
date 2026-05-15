import { useEffect, useMemo, useState } from "react";
import {
  CalendarDays,
  CheckCircle2,
  Clock3,
  Download,
  Eye,
  FileText,
  Home,
  Receipt,
  RefreshCw,
  XCircle,
} from "lucide-react";
import { useApp } from "../../context/AppContext";
import { getAppointments, type AppointmentResponse } from "../../lib/appointments";
import { getInvoices, type InvoiceResponse } from "../../lib/invoices";
import { getContracts, type ContractResponse } from "../../lib/contracts";
import { getPropertyListing, getPropertyListings } from "../../lib/properties";
import { getAreas } from "../../lib/areas";
import { formatCurrency } from "../../lib/format";

const tabs = [
  { id: "overview", label: "Tổng Quan", icon: Home },
  { id: "viewing", label: "Lịch Xem Phòng", icon: CalendarDays },
  { id: "invoices", label: "Hóa Đơn", icon: Receipt },
  { id: "contracts", label: "Hợp Đồng", icon: FileText },
] as const;

type TabKey = (typeof tabs)[number]["id"];

type PropertyMeta = {
  name: string;
  areaName: string;
  price: number;
};

type EnrichedContract = {
  contract: ContractResponse;
  propertyName: string;
  areaName: string;
  rentAmount: number;
};

type EnrichedInvoice = {
  invoice: InvoiceResponse;
  propertyName: string;
  areaName: string;
  rentAmount: number;
  statusKey: "paid" | "unpaid" | "overdue";
};

export default function TenantDashboard() {
  const { token, currentUser } = useApp();
  const [activeTab, setActiveTab] = useState<TabKey>("overview");
  const [expandedInvoiceId, setExpandedInvoiceId] = useState<string | null>(null);
  const [expandedContractId, setExpandedContractId] = useState<string | null>(null);
  const [appointments, setAppointments] = useState<AppointmentResponse[]>([]);
  const [invoices, setInvoices] = useState<InvoiceResponse[]>([]);
  const [contracts, setContracts] = useState<ContractResponse[]>([]);
  const [propertyMeta, setPropertyMeta] = useState<Record<string, PropertyMeta>>({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const loadData = async () => {
    if (!token || !currentUser) {
      setError("Thiếu thông tin đăng nhập để tải dashboard.");
      setLoading(false);
      return;
    }

    setLoading(true);
    setError("");

    try {
      const [appointmentResponse, invoiceResponse, contractResponse, propertyResponse, areaResponse] = await Promise.all([
        getAppointments(token, { userId: currentUser.id, pageSize: 1000 }),
        getInvoices(token, { pageSize: 1000 }),
        getContracts(token, { pageSize: 1000 }),
        getPropertyListings({ page: 1, pageSize: 1000 }),
        getAreas({ page: 1, pageSize: 1000 }).catch(() => ({ items: [] })),
      ]);

      const tenantContracts = contractResponse.items.filter((item) => item.tenantId === currentUser.id);
      const contractIds = new Set(tenantContracts.map((item) => item.id));
      const tenantInvoices = invoiceResponse.items.filter((item) => contractIds.has(item.contractId));
      const areasById = new Map(areaResponse.items.map((item) => [item.id, item.name]));
      const relatedPropertyIds = new Set([
        ...tenantContracts.map((item) => item.propertyId),
        ...appointmentResponse.items.map((item) => item.propertyId),
      ]);
      const fetchedPropertyIds = new Set(propertyResponse.items.map((item) => item.id));
      const missingPropertyIds = Array.from(relatedPropertyIds).filter((id) => !fetchedPropertyIds.has(id));
      const missingProperties = await Promise.all(
        missingPropertyIds.map(async (id) => {
          try {
            return await getPropertyListing(id);
          } catch {
            return null;
          }
        }),
      );
      const allProperties = [...propertyResponse.items, ...missingProperties.filter((item): item is NonNullable<typeof item> => Boolean(item))];

      setAppointments(appointmentResponse.items);
      setContracts(tenantContracts);
      setInvoices(tenantInvoices);
      setPropertyMeta(
        Object.fromEntries(
          allProperties.map((item) => [
            item.id,
            {
              name: item.propertyName,
              areaName: item.areaId ? areasById.get(item.areaId) || "Chưa gán khu" : "Chưa gán khu",
              price: item.price,
            },
          ]),
        ),
      );
    } catch (err) {
      setError(err instanceof Error ? err.message : "Không tải được dashboard cá nhân.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadData();
  }, [token, currentUser?.id]);

  const currentContract = useMemo(
    () => contracts.find((item) => item.status.toLowerCase() === "active") || null,
    [contracts],
  );

  const enrichedContracts = useMemo<EnrichedContract[]>(
    () =>
      contracts.map((contract) => ({
        contract,
        propertyName: propertyMeta[contract.propertyId]?.name || "Phòng không xác định",
        areaName: propertyMeta[contract.propertyId]?.areaName || "Chưa rõ khu",
        rentAmount: propertyMeta[contract.propertyId]?.price || 0,
      })),
    [contracts, propertyMeta],
  );

  const enrichedInvoices = useMemo<EnrichedInvoice[]>(
    () =>
      invoices
        .map((invoice) => {
          const contract = contracts.find((item) => item.id === invoice.contractId);
          const property = contract ? propertyMeta[contract.propertyId] : null;
          return {
            invoice,
            propertyName: property?.name || invoice.note || "Phòng không xác định",
            areaName: property?.areaName || "Chưa rõ khu",
            rentAmount: invoice.rentAmount,
            statusKey: resolveInvoiceStatus(invoice),
          };
        })
        .sort((a, b) => new Date(b.invoice.period).getTime() - new Date(a.invoice.period).getTime()),
    [contracts, invoices, propertyMeta],
  );

  const unpaidInvoices = enrichedInvoices.filter((item) => item.statusKey !== "paid");
  const pendingAppointments = appointments.filter((item) => item.status.toLowerCase().includes("pending"));
  const recentInvoices = enrichedInvoices.slice(0, 3);
  const currentContractMeta = currentContract ? propertyMeta[currentContract.propertyId] : null;

  return (
    <div className="mx-auto max-w-7xl px-4 py-6">
      <div className="mb-6 flex items-start justify-between gap-4">
        <div>
          <h1 className="text-gray-900" style={{ fontSize: "22px", fontWeight: 800 }}>
            Quản Lý Cá Nhân
          </h1>
          <p className="mt-1 text-gray-500" style={{ fontSize: "14px", fontWeight: 500 }}>
            Theo dõi hóa đơn, lịch xem phòng và hợp đồng của bạn
          </p>
        </div>
        <button
          onClick={() => void loadData()}
          className="inline-flex items-center gap-2 rounded-2xl border border-gray-200 px-4 py-3 text-gray-600 transition-colors hover:bg-gray-50"
          style={{ fontSize: "14px", fontWeight: 700 }}
        >
          <RefreshCw className="h-4 w-4" />
          Tải lại
        </button>
      </div>

      <div className="mb-6 flex gap-1 overflow-x-auto rounded-[28px] bg-gray-100 p-2">
        {tabs.map((tab) => {
          const Icon = tab.icon;
          return (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={`inline-flex items-center gap-3 rounded-2xl px-5 py-4 whitespace-nowrap transition-all ${
                activeTab === tab.id ? "bg-white text-gray-900 shadow-sm" : "text-gray-500 hover:text-gray-700"
              }`}
              style={{ fontSize: "14px", fontWeight: 700 }}
            >
              <Icon className="h-5 w-5" />
              {tab.label}
            </button>
          );
        })}
      </div>

      {error ? <div className="mb-5 rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-red-600">{error}</div> : null}

      {activeTab === "overview" ? (
        <div className="space-y-6">
          <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
            <StatCard
              tone="blue"
              icon={<CalendarDays className="h-5 w-5" />}
              label="Lịch xem"
              value={loading ? "..." : String(appointments.length)}
              hint={`${pendingAppointments.length} đang chờ xác nhận`}
            />
            <StatCard
              tone="red"
              icon={<Receipt className="h-5 w-5" />}
              label="Hóa đơn chưa đóng"
              value={loading ? "..." : String(unpaidInvoices.length)}
              hint={`Tổng: ${formatCurrency(unpaidInvoices.reduce((sum, item) => sum + item.invoice.total, 0))}`}
            />
            <StatCard
              tone="green"
              icon={<FileText className="h-5 w-5" />}
              label="Hợp đồng"
              value={currentContract ? "Đang thuê" : "Chưa có"}
              hint={currentContract ? `Hết hạn: ${formatShortDate(currentContract.endDate)}` : "Chưa có hợp đồng hiệu lực"}
            />
          </div>

          <div className="rounded-[28px] border border-amber-200 bg-gradient-to-r from-amber-50 to-white px-5 py-6">
            <div className="mb-3 flex items-center justify-between gap-3">
              <div>
                <p className="text-gray-500" style={{ fontSize: "12px", fontWeight: 800 }}>
                  HỢP ĐỒNG HIỆN TẠI
                </p>
                <p className="mt-1 text-gray-900" style={{ fontSize: "18px", fontWeight: 800 }}>
                  {currentContract ? `${currentContractMeta?.name || "Phòng không xác định"} - ${currentContractMeta?.areaName || "Chưa rõ khu"}` : "Chưa có hợp đồng đang hiệu lực"}
                </p>
              </div>
              {currentContract ? <StatusPill tone="green">Đang hiệu lực</StatusPill> : null}
            </div>

            {currentContract ? (
              <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
                <ContractInfo label="Giá thuê" value={`${formatCurrency(currentContractMeta?.price || 0)}/tháng`} />
                <ContractInfo label="Bắt đầu" value={formatShortDate(currentContract.startDate)} />
                <ContractInfo label="Hết hạn" value={formatShortDate(currentContract.endDate)} />
              </div>
            ) : (
              <p className="text-gray-400" style={{ fontSize: "14px", fontWeight: 600 }}>
                Bạn chưa có hợp đồng đang hiệu lực.
              </p>
            )}
          </div>

          <Panel
            title="Hóa đơn gần đây"
            actionLabel="Xem tất cả"
            onAction={() => setActiveTab("invoices")}
            loading={loading}
            emptyText="Chưa có hóa đơn gần đây."
          >
            {recentInvoices.map((item) => (
              <RecentInvoiceRow
                key={item.invoice.id}
                month={formatBillingPeriod(item.invoice.period)}
                propertyLabel={`${item.propertyName} - ${item.areaName}`}
                amount={formatCurrency(item.invoice.total)}
                statusKey={item.statusKey}
              />
            ))}
          </Panel>
        </div>
      ) : null}

      {activeTab === "viewing" ? (
        <Section title="Lịch xem phòng">
          {loading ? <LoadingBlock /> : null}
          {!loading && appointments.length === 0 ? <EmptyBlock text="Bạn chưa có lịch xem phòng." /> : null}
          {!loading ? (
            <div className="space-y-5">
              {appointments
                .slice()
                .sort((a, b) => new Date(a.appointmentDateTime).getTime() - new Date(b.appointmentDateTime).getTime())
                .map((appointment) => (
                  <ViewingCard
                    key={appointment.id}
                    title={propertyMeta[appointment.propertyId]?.name || "Phòng không xác định"}
                    dateTime={formatDateTime(appointment.appointmentDateTime)}
                    note={appointment.note || ""}
                    status={appointment.status}
                  />
                ))}
            </div>
          ) : null}
        </Section>
      ) : null}

      {activeTab === "invoices" ? (
        <Section title="Hóa đơn & Thanh toán">
          {loading ? <LoadingBlock /> : null}
          {!loading && enrichedInvoices.length === 0 ? <EmptyBlock text="Bạn chưa có hóa đơn." /> : null}
          {!loading ? (
            <div className="space-y-5">
              {enrichedInvoices.map((item) => (
                <InvoiceCard
                  key={item.invoice.id}
                  item={item}
                  expanded={expandedInvoiceId === item.invoice.id}
                  onToggle={() => setExpandedInvoiceId((current) => (current === item.invoice.id ? null : item.invoice.id))}
                />
              ))}
            </div>
          ) : null}
        </Section>
      ) : null}

      {activeTab === "contracts" ? (
        <Section title="Hợp đồng thuê phòng">
          {loading ? <LoadingBlock /> : null}
          {!loading && enrichedContracts.length === 0 ? <EmptyBlock text="Bạn chưa có hợp đồng thuê phòng." /> : null}
          {!loading ? (
            <div className="space-y-5">
              {enrichedContracts.map((item) => (
                <ContractCard
                  key={item.contract.id}
                  item={item}
                  expanded={expandedContractId === item.contract.id}
                  onToggle={() => setExpandedContractId((current) => (current === item.contract.id ? null : item.contract.id))}
                />
              ))}
            </div>
          ) : null}
        </Section>
      ) : null}
    </div>
  );
}

function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div>
      <h2 className="mb-6 text-gray-900" style={{ fontSize: "18px", fontWeight: 800 }}>
        {title}
      </h2>
      {children}
    </div>
  );
}

function StatCard({
  tone,
  icon,
  label,
  value,
  hint,
}: {
  tone: "blue" | "red" | "green";
  icon: React.ReactNode;
  label: string;
  value: string;
  hint: string;
}) {
  const palette = {
    blue: "bg-blue-50 text-blue-600",
    red: "bg-red-50 text-red-500",
    green: "bg-green-50 text-green-600",
  }[tone];

  return (
    <div className="rounded-[28px] border border-gray-100 bg-white p-6">
      <div className={`mb-4 inline-flex h-12 w-12 items-center justify-center rounded-2xl ${palette}`}>{icon}</div>
      <p className="text-gray-500" style={{ fontSize: "14px", fontWeight: 700 }}>
        {label}
      </p>
      <p className="mt-1 text-gray-900" style={{ fontSize: "20px", fontWeight: 800 }}>
        {value}
      </p>
      <p className="mt-3 text-gray-400" style={{ fontSize: "13px", fontWeight: 600 }}>
        {hint}
      </p>
    </div>
  );
}

function ContractInfo({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <p className="text-gray-400" style={{ fontSize: "12px", fontWeight: 700 }}>
        {label}
      </p>
      <p className="mt-1 text-gray-900" style={{ fontSize: "16px", fontWeight: 800 }}>
        {value}
      </p>
    </div>
  );
}

function Panel({
  title,
  children,
  loading,
  emptyText,
  actionLabel,
  onAction,
}: {
  title: string;
  children: React.ReactNode;
  loading: boolean;
  emptyText: string;
  actionLabel?: string;
  onAction?: () => void;
}) {
  const items = Array.isArray(children) ? children.filter(Boolean) : [children].filter(Boolean);
  return (
    <div className="rounded-[28px] border border-gray-100 bg-white p-5">
      <div className="mb-4 flex items-center justify-between gap-3">
        <h2 className="text-gray-900" style={{ fontSize: "18px", fontWeight: 800 }}>
          {title}
        </h2>
        {actionLabel && onAction ? (
          <button onClick={onAction} className="text-orange-500 hover:text-orange-600" style={{ fontSize: "14px", fontWeight: 700 }}>
            {actionLabel}
          </button>
        ) : null}
      </div>
      {loading ? <LoadingBlock /> : null}
      {!loading && items.length === 0 ? <EmptyBlock text={emptyText} /> : null}
      {!loading && items.length > 0 ? <div>{children}</div> : null}
    </div>
  );
}

function RecentInvoiceRow({
  month,
  propertyLabel,
  amount,
  statusKey,
}: {
  month: string;
  propertyLabel: string;
  amount: string;
  statusKey: "paid" | "unpaid" | "overdue";
}) {
  const tone = statusKey === "paid" ? "bg-green-500" : statusKey === "overdue" ? "bg-red-400" : "bg-amber-400";
  const label = statusKey === "paid" ? "Đã thanh toán" : statusKey === "overdue" ? "Quá hạn" : "Chưa thanh toán";
  const textTone = statusKey === "paid" ? "text-green-600" : statusKey === "overdue" ? "text-red-500" : "text-amber-500";

  return (
    <div className="flex items-center justify-between gap-4 border-b border-gray-100 py-4 last:border-0">
      <div className="flex items-start gap-4">
        <span className={`mt-2 h-2.5 w-2.5 rounded-full ${tone}`} />
        <div>
          <p className="text-gray-800" style={{ fontSize: "16px", fontWeight: 800 }}>
            {month}
          </p>
          <p className="mt-1 text-gray-400" style={{ fontSize: "14px", fontWeight: 600 }}>
            {propertyLabel}
          </p>
        </div>
      </div>
      <div className="text-right">
        <p className="text-gray-900" style={{ fontSize: "16px", fontWeight: 800 }}>
          {amount}
        </p>
        <p className={`mt-1 ${textTone}`} style={{ fontSize: "14px", fontWeight: 700 }}>
          {label}
        </p>
      </div>
    </div>
  );
}

function ViewingCard({
  title,
  dateTime,
  note,
  status,
}: {
  title: string;
  dateTime: string;
  note: string;
  status: string;
}) {
  return (
    <div className="rounded-[28px] border border-gray-100 bg-white p-6">
      <div className="mb-3 flex items-start justify-between gap-4">
        <div>
          <p className="text-gray-900" style={{ fontSize: "18px", fontWeight: 800 }}>
            {title}
          </p>
          <div className="mt-3 flex items-center gap-3 text-gray-500" style={{ fontSize: "14px", fontWeight: 600 }}>
            <CalendarDays className="h-4 w-4" />
            {dateTime}
          </div>
        </div>
        <AppointmentStatus status={status} />
      </div>
      {note ? (
        <div className="rounded-2xl bg-gray-50 px-4 py-3 text-gray-500" style={{ fontSize: "14px", fontWeight: 600 }}>
          {note}
        </div>
      ) : null}
    </div>
  );
}

function InvoiceCard({
  item,
  expanded,
  onToggle,
}: {
  item: EnrichedInvoice;
  expanded: boolean;
  onToggle: () => void;
}) {
  const electricityUnitPrice =
    item.invoice.oldElectricityReading != null && item.invoice.newElectricityReading != null && item.invoice.newElectricityReading > item.invoice.oldElectricityReading
      ? Math.round((item.invoice.electricityCost || 0) / (item.invoice.newElectricityReading - item.invoice.oldElectricityReading))
      : 0;
  const waterUnitPrice =
    item.invoice.oldWaterReading != null && item.invoice.newWaterReading != null && item.invoice.newWaterReading > item.invoice.oldWaterReading
      ? Math.round((item.invoice.waterCost || 0) / (item.invoice.newWaterReading - item.invoice.oldWaterReading))
      : 0;
  const electricityUsage =
    item.invoice.oldElectricityReading != null && item.invoice.newElectricityReading != null
      ? Math.max(item.invoice.newElectricityReading - item.invoice.oldElectricityReading, 0)
      : 0;
  const waterUsage =
    item.invoice.oldWaterReading != null && item.invoice.newWaterReading != null
      ? Math.max(item.invoice.newWaterReading - item.invoice.oldWaterReading, 0)
      : 0;

  return (
    <div className="overflow-hidden rounded-[28px] border border-gray-100 bg-white">
      <div className="flex items-start justify-between gap-4 px-6 py-6">
        <div className="flex items-start gap-4">
          <div className="flex h-20 w-20 items-center justify-center rounded-[28px] bg-yellow-50 text-amber-600">
            <Receipt className="h-8 w-8" />
          </div>
          <div>
            <p className="text-gray-900" style={{ fontSize: "18px", fontWeight: 800 }}>
              Hóa đơn tháng {formatBillingPeriod(item.invoice.period)}
            </p>
            <p className="mt-1 text-gray-400" style={{ fontSize: "14px", fontWeight: 600 }}>
              Hạn: {formatShortDate(item.invoice.dueDate)}
            </p>
            <p className="mt-1 text-gray-400" style={{ fontSize: "14px", fontWeight: 600 }}>
              {item.propertyName} - {item.areaName}
            </p>
          </div>
        </div>
        <div className="flex items-start gap-5">
          <div className="text-right">
            <p className="text-gray-900" style={{ fontSize: "18px", fontWeight: 800 }}>
              {formatCurrency(item.invoice.total)}
            </p>
            <p className={`mt-2 ${invoiceStatusText(item.statusKey)}`} style={{ fontSize: "14px", fontWeight: 700 }}>
              {invoiceStatusLabel(item.statusKey)}
            </p>
          </div>
          <button onClick={onToggle} className="rounded-full p-2 text-gray-400 transition-colors hover:bg-gray-50 hover:text-gray-600">
            <Eye className="h-6 w-6" />
          </button>
        </div>
      </div>

      {expanded ? (
        <div className="grid gap-5 border-t border-gray-100 bg-gray-50 p-6 lg:grid-cols-[1fr_0.95fr]">
          <div>
            <InvoiceLine label="Tiền phòng" value={formatCurrency(item.rentAmount)} />
            <InvoiceLine
              label={`Điện (${electricityUsage} kWh x ${formatCurrency(electricityUnitPrice)})`}
              value={formatCurrency(item.invoice.electricityCost || 0)}
            />
            <InvoiceLine
              label={`Nước (${waterUsage} m³ x ${formatCurrency(waterUnitPrice)})`}
              value={formatCurrency(item.invoice.waterCost || 0)}
            />
            <InvoiceLine label="Phí dịch vụ" value={formatCurrency(item.invoice.otherFees || 0)} />
            <div className="mt-4 border-t border-gray-200 pt-4">
              <InvoiceLine label="Tổng cộng" value={formatCurrency(item.invoice.total)} emphasize />
            </div>
          </div>

          <div className="rounded-[28px] bg-white p-6">
            <p className="mb-5 text-gray-600" style={{ fontSize: "18px", fontWeight: 800 }}>
              CHỈ SỐ ĐIỆN NƯỚC
            </p>
            <MetricLine label="Điện cũ" value={`${item.invoice.oldElectricityReading || 0} kWh`} />
            <MetricLine label="Điện mới" value={`${item.invoice.newElectricityReading || 0} kWh`} />
            <MetricLine label="Điện sử dụng" value={`${electricityUsage} kWh`} />
            <MetricLine label="Đơn giá điện" value={electricityUnitPrice ? `${formatCurrency(electricityUnitPrice)}/kWh` : "-"} />
            <MetricLine label="Nước cũ" value={`${item.invoice.oldWaterReading || 0} m³`} />
            <MetricLine label="Nước mới" value={`${item.invoice.newWaterReading || 0} m³`} />
            <MetricLine label="Nước sử dụng" value={`${waterUsage} m³`} />
            <MetricLine label="Đơn giá nước" value={waterUnitPrice ? `${formatCurrency(waterUnitPrice)}/m³` : "-"} />
          </div>
        </div>
      ) : null}
    </div>
  );
}

function ContractCard({
  item,
  expanded,
  onToggle,
}: {
  item: EnrichedContract;
  expanded: boolean;
  onToggle: () => void;
}) {
  const progress = getContractProgress(item.contract.startDate, item.contract.endDate);

  return (
    <div className="rounded-[28px] border border-gray-100 bg-white p-6">
      <div className="mb-4 flex items-start justify-between gap-4">
        <div>
          <p className="text-gray-900" style={{ fontSize: "18px", fontWeight: 800 }}>
            {item.propertyName} - {item.areaName}
          </p>
          <p className="mt-2 text-gray-400" style={{ fontSize: "14px", fontWeight: 600 }}>
            Giá thuê: {formatCurrency(item.rentAmount)}/tháng · Cọc: {formatCurrency(item.contract.deposit)}
          </p>
        </div>
        <StatusPill tone={contractTone(item.contract.status)}>{contractLabel(item.contract.status)}</StatusPill>
      </div>

      <div className="mb-4 flex items-center justify-between text-gray-400" style={{ fontSize: "14px", fontWeight: 600 }}>
        <span>{formatShortDate(item.contract.startDate)}</span>
        <span>{formatShortDate(item.contract.endDate)}</span>
      </div>

      <div className="mb-6 h-4 rounded-full bg-orange-100">
        <div className="h-4 rounded-full bg-orange-500" style={{ width: `${progress}%` }} />
      </div>

      <div className="flex flex-wrap gap-3">
        <button
          onClick={onToggle}
          className="inline-flex items-center gap-3 rounded-2xl bg-gray-50 px-5 py-4 text-gray-600 transition-colors hover:bg-gray-100"
          style={{ fontSize: "14px", fontWeight: 700 }}
        >
          <Eye className="h-5 w-5" />
          {expanded ? "Ẩn hợp đồng" : "Xem hợp đồng"}
        </button>
        <button
          onClick={() => window.print()}
          className="inline-flex items-center gap-3 rounded-2xl bg-gray-50 px-5 py-4 text-gray-600 transition-colors hover:bg-gray-100"
          style={{ fontSize: "14px", fontWeight: 700 }}
        >
          <Download className="h-5 w-5" />
          Tải PDF
        </button>
      </div>

      {expanded ? (
        <div className="mt-5 rounded-[24px] border border-gray-100 bg-gray-50 px-5 py-6">
          <p className="mb-4 text-center text-gray-700" style={{ fontSize: "18px", fontWeight: 800 }}>
            HỢP ĐỒNG THUÊ PHÒNG
          </p>
          <pre className="whitespace-pre-wrap text-gray-600" style={{ fontFamily: "inherit", fontSize: "14px", lineHeight: 1.9 }}>
            {item.contract.terms ||
              [
                `Bên thuê đang sử dụng phòng: ${item.propertyName} - ${item.areaName}.`,
                `Thời hạn hợp đồng: ${formatShortDate(item.contract.startDate)} đến ${formatShortDate(item.contract.endDate)}.`,
                `Giá thuê: ${formatCurrency(item.rentAmount)}/tháng.`,
                `Tiền cọc: ${formatCurrency(item.contract.deposit)}.`,
              ].join("\n")}
          </pre>
        </div>
      ) : null}
    </div>
  );
}

function InvoiceLine({ label, value, emphasize = false }: { label: string; value: string; emphasize?: boolean }) {
  return (
    <div className="flex items-center justify-between gap-4 py-2">
      <span className={emphasize ? "text-gray-900" : "text-gray-500"} style={{ fontSize: emphasize ? "17px" : "16px", fontWeight: emphasize ? 800 : 600 }}>
        {label}
      </span>
      <span className={emphasize ? "text-orange-600" : "text-gray-700"} style={{ fontSize: emphasize ? "18px" : "16px", fontWeight: 800 }}>
        {value}
      </span>
    </div>
  );
}

function MetricLine({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex items-center justify-between gap-4 py-2">
      <span className="text-gray-400" style={{ fontSize: "16px", fontWeight: 600 }}>
        {label}
      </span>
      <span className="text-gray-700" style={{ fontSize: "16px", fontWeight: 700 }}>
        {value}
      </span>
    </div>
  );
}

function LoadingBlock() {
  return <div className="h-24 animate-pulse rounded-2xl bg-gray-100" />;
}

function EmptyBlock({ text }: { text: string }) {
  return (
    <div className="rounded-[28px] border border-gray-100 bg-white px-6 py-12 text-center text-gray-400">
      <p style={{ fontSize: "14px", fontWeight: 600 }}>{text}</p>
    </div>
  );
}

function StatusPill({ tone, children }: { tone: "green" | "blue" | "gray" | "red" | "yellow"; children: React.ReactNode }) {
  const palette = {
    green: "bg-green-100 text-green-700",
    blue: "bg-blue-100 text-blue-700",
    gray: "bg-gray-100 text-gray-500",
    red: "bg-red-100 text-red-600",
    yellow: "bg-yellow-100 text-amber-600",
  }[tone];
  return (
    <span className={`inline-flex items-center rounded-full px-4 py-2 ${palette}`} style={{ fontSize: "13px", fontWeight: 700 }}>
      {children}
    </span>
  );
}

function AppointmentStatus({ status }: { status: string }) {
  const normalized = status.toLowerCase();
  if (normalized.includes("confirm") || normalized.includes("approved")) {
    return (
      <StatusPill tone="green">
        <span className="inline-flex items-center gap-2">
          <CheckCircle2 className="h-4 w-4" />
          Đã xác nhận
        </span>
      </StatusPill>
    );
  }
  if (normalized.includes("reject") || normalized.includes("cancel")) {
    return (
      <StatusPill tone="red">
        <span className="inline-flex items-center gap-2">
          <XCircle className="h-4 w-4" />
          Từ chối
        </span>
      </StatusPill>
    );
  }
  return (
    <StatusPill tone="yellow">
      <span className="inline-flex items-center gap-2">
        <Clock3 className="h-4 w-4" />
        Chờ xác nhận
      </span>
    </StatusPill>
  );
}

function resolveInvoiceStatus(invoice: InvoiceResponse) {
  const normalized = invoice.status.toLowerCase();
  if (normalized === "paid") return "paid";
  if (new Date(invoice.dueDate).getTime() < Date.now()) return "overdue";
  return "unpaid";
}

function invoiceStatusLabel(statusKey: "paid" | "unpaid" | "overdue") {
  if (statusKey === "paid") return "Đã thanh toán";
  if (statusKey === "overdue") return "Quá hạn";
  return "Chưa thanh toán";
}

function invoiceStatusText(statusKey: "paid" | "unpaid" | "overdue") {
  if (statusKey === "paid") return "text-green-600";
  if (statusKey === "overdue") return "text-red-500";
  return "text-amber-600";
}

function formatShortDate(value: string) {
  return new Date(value).toLocaleDateString("vi-VN");
}

function formatDateTime(value: string) {
  return new Date(value).toLocaleString("vi-VN");
}

function formatBillingPeriod(period: string) {
  const date = new Date(period);
  return `${String(date.getMonth() + 1).padStart(2, "0")}/${date.getFullYear()}`;
}

function contractTone(status: string): "green" | "gray" | "red" {
  const normalized = status.toLowerCase();
  if (normalized === "active") return "green";
  if (normalized === "expired") return "gray";
  return "red";
}

function contractLabel(status: string) {
  const normalized = status.toLowerCase();
  if (normalized === "active") return "Đang hiệu lực";
  if (normalized === "expired") return "Hết hạn";
  if (normalized === "terminated") return "Đã chấm dứt";
  return status;
}

function getContractProgress(startDate: string, endDate: string) {
  const start = new Date(startDate).getTime();
  const end = new Date(endDate).getTime();
  const now = Date.now();
  if (Number.isNaN(start) || Number.isNaN(end) || end <= start) return 100;
  if (now <= start) return 0;
  if (now >= end) return 100;
  return ((now - start) / (end - start)) * 100;
}
