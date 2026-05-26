import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router";
import {
  AlertTriangle,
  ArrowUpRight,
  Building2,
  CalendarDays,
  CheckCircle2,
  Clock3,
  DoorOpen,
  FileText,
  Receipt,
  TriangleAlert,
  UserRound,
} from "lucide-react";
import { Bar, BarChart, CartesianGrid, Cell, XAxis, YAxis } from "recharts";
import { useApp } from "../../context/AppContext";
import { getPropertyListings } from "../../lib/properties";
import { getInvoices } from "../../lib/invoices";
import { getContracts } from "../../lib/contracts";
import { getAppointments } from "../../lib/appointments";
import { getUserByEmail, getUsers } from "../../lib/users";
import type { PropertyListing, UserResponse } from "../../lib/types";
import type { InvoiceResponse } from "../../lib/invoices";
import type { ContractResponse } from "../../lib/contracts";
import type { AppointmentResponse } from "../../lib/appointments";
import { formatCurrency } from "../../lib/format";
import { isOccupyingContractStatus } from "../../lib/contractStatus";
import {
  getPropertyStatusMeta,
  isAvailablePropertyStatus,
  isRentedPropertyStatus,
} from "../../lib/propertyStatus";
import { ChartContainer, ChartTooltip, ChartTooltipContent } from "../../components/ui/chart";

type DashboardError = {
  section: string;
  message: string;
};

export default function LandlordDashboard() {
  const navigate = useNavigate();
  const { currentUser, token } = useApp();
  const [properties, setProperties] = useState<PropertyListing[]>([]);
  const [invoices, setInvoices] = useState<InvoiceResponse[]>([]);
  const [contracts, setContracts] = useState<ContractResponse[]>([]);
  const [appointments, setAppointments] = useState<AppointmentResponse[]>([]);
  const [users, setUsers] = useState<UserResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [errors, setErrors] = useState<DashboardError[]>([]);

  useEffect(() => {
    let cancelled = false;

    (async () => {
      setLoading(true);
      const nextErrors: DashboardError[] = [];
      const landlord = currentUser?.email ? await getUserByEmail(currentUser.email).catch(() => null) : null;
      const landlordId = landlord?.id || currentUser?.id;

      await Promise.all([
        getPropertyListings(landlordId ? { landlordId, pageSize: 1000 } : {})
          .then((response) => {
            if (!cancelled) {
              const approvedProperties = response.items.filter((p) => p.moderationStatus === "Approved");
              setProperties(approvedProperties);
            }
          })
          .catch((err) => nextErrors.push({ section: "Tài sản", message: err instanceof Error ? err.message : "Không tải được tài sản." })),
        token
          ? getInvoices(token, { pageSize: 1000 })
              .then((response) => {
                if (!cancelled) setInvoices(response.items);
              })
              .catch((err) => nextErrors.push({ section: "Hóa đơn", message: err instanceof Error ? err.message : "Không tải được hóa đơn." }))
          : Promise.resolve(),
        token
          ? getContracts(token, { pageSize: 1000 })
              .then((response) => {
                if (!cancelled) setContracts(response.items);
              })
              .catch((err) => nextErrors.push({ section: "Hợp đồng", message: err instanceof Error ? err.message : "Không tải được hợp đồng." }))
          : Promise.resolve(),
        token
          ? getAppointments(token, { pageSize: 1000 })
              .then((response) => {
                if (!cancelled) setAppointments(response.items);
              })
              .catch((err) => nextErrors.push({ section: "Lịch hẹn", message: err instanceof Error ? err.message : "Không tải được lịch hẹn." }))
          : Promise.resolve(),
        getUsers({ page: 1, pageSize: 1000 })
          .then((response) => {
            if (!cancelled) setUsers(response.items);
          })
          .catch((err) => nextErrors.push({ section: "Người dùng", message: err instanceof Error ? err.message : "Không tải được người dùng." })),
      ]);

      if (!cancelled) {
        setErrors(nextErrors);
        setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [currentUser?.email, currentUser?.id, token]);


  const usersById = useMemo(
    () => Object.fromEntries(users.map((user) => [user.id, user])),
    [users],
  );

  const contractsById = useMemo(
    () => Object.fromEntries(contracts.map((contract) => [contract.id, contract])),
    [contracts],
  );

  const propertiesById = useMemo(
    () => Object.fromEntries(properties.map((property) => [property.id, property])),
    [properties],
  );

  const landlordPropertyIds = useMemo(
    () => new Set(properties.map((property) => property.id)),
    [properties],
  );

  const landlordContracts = useMemo(
    () => contracts.filter((contract) => landlordPropertyIds.has(contract.propertyId)),
    [contracts, landlordPropertyIds],
  );

  const landlordContractIds = useMemo(
    () => new Set(landlordContracts.map((contract) => contract.id)),
    [landlordContracts],
  );

  const landlordInvoices = useMemo(
    () => invoices.filter((invoice) => landlordContractIds.has(invoice.contractId)),
    [invoices, landlordContractIds],
  );

  const landlordAppointments = useMemo(
    () => appointments.filter((appointment) => landlordPropertyIds.has(appointment.propertyId)),
    [appointments, landlordPropertyIds],
  );

  const occupiedPropertyIds = useMemo(
    () =>
      new Set(
        landlordContracts
          .filter((contract) => isOccupyingContractStatus(contract.status))
          .map((contract) => contract.propertyId),
      ),
    [landlordContracts],
  );

  const totalRooms = properties.length;
  const rentedRooms = useMemo(
    () => properties.filter((property) => isRentedPropertyStatus(property.status)).length,
    [properties],
  );
  const availableRooms = useMemo(
    () => properties.filter((property) => isAvailablePropertyStatus(property.status)).length,
    [properties],
  );

  const monthlyRevenue = useMemo(() => {
    const now = new Date();
    const months = Array.from({ length: 6 }, (_, index) => {
      const date = new Date(now.getFullYear(), now.getMonth() - (5 - index), 1);
      return {
        key: `${date.getFullYear()}-${date.getMonth()}`,
        label: `T${date.getMonth() + 1}`,
        month: date.getMonth(),
        year: date.getFullYear(),
        revenue: 0,
      };
    });

    const monthMap = new Map(months.map((item) => [item.key, item]));

    landlordInvoices.forEach((invoice) => {
      const period = new Date(invoice.period);
      if (Number.isNaN(period.getTime())) return;
      const key = `${period.getFullYear()}-${period.getMonth()}`;
      const target = monthMap.get(key);
      if (!target) return;
      target.revenue += invoice.total;
    });

    return months;
  }, [landlordInvoices]);

  const revenueThisMonth = monthlyRevenue[monthlyRevenue.length - 1]?.revenue || 0;
  const revenueLastMonth = monthlyRevenue[monthlyRevenue.length - 2]?.revenue || 0;
  const revenueGrowth = revenueLastMonth > 0 ? ((revenueThisMonth - revenueLastMonth) / revenueLastMonth) * 100 : null;
  const totalSixMonthRevenue = monthlyRevenue.reduce((sum, item) => sum + item.revenue, 0);

  const unpaidInvoices = useMemo(() => {
    return landlordInvoices
      .filter((invoice) => invoice.status.toLowerCase() !== "paid")
      .sort((a, b) => new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime());
  }, [landlordInvoices]);

  const upcomingAppointments = useMemo(() => {
    const now = Date.now();
    return landlordAppointments
      .filter((appointment) => new Date(appointment.appointmentDateTime).getTime() >= now)
      .sort((a, b) => new Date(a.appointmentDateTime).getTime() - new Date(b.appointmentDateTime).getTime());
  }, [landlordAppointments]);

  const roomOverview = useMemo(() => {
    return properties.map((property) => {
      if (occupiedPropertyIds.has(property.id)) {
        return {
          id: property.id,
          name: property.propertyName,
          price: property.price,
          status: "Đã thuê",
          tone: "blue" as const,
        };
      }

      const statusMeta = getPropertyStatusMeta(property.status);

      return {
        id: property.id,
        name: property.propertyName,
        price: property.price,
        status: statusMeta.label,
        tone: statusMeta.tone,
      };
    });
  }, [occupiedPropertyIds, properties]);

  return (
    <div className="max-w-7xl mx-auto px-4 py-6 space-y-6">
      <div>
        <h1 className="text-gray-900" style={{ fontSize: "24px", fontWeight: 700 }}>
          Tổng Quan Chủ Trọ
        </h1>
      </div>

      {errors.length > 0 && (
        <div className="rounded-3xl border border-amber-200 bg-amber-50 px-5 py-4 text-amber-800">
          <div className="flex items-start gap-3">
            <TriangleAlert className="w-5 h-5 mt-0.5 shrink-0" />
            <div style={{ fontSize: "13px" }}>
              {errors.map((error) => (
                <p key={`${error.section}-${error.message}`}>
                  {error.section}: {error.message}
                </p>
              ))}
            </div>
          </div>
        </div>
      )}

      <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4">
        <StatCard
          icon={Building2}
          tone="blue"
          badge="Tổng"
          label="Tổng số phòng"
          value={loading ? "..." : String(totalRooms)}
        />
        <StatCard
          icon={DoorOpen}
          tone="green"
          badge="Trống"
          label="Phòng đang trống"
          value={loading ? "..." : String(availableRooms)}
        />
        <StatCard
          icon={UserRound}
          tone="purple"
          badge="Đang thuê"
          label="Phòng đã thuê"
          value={loading ? "..." : String(rentedRooms)}
        />
        <StatCard
          icon={ArrowUpRight}
          tone="orange"
          badge={revenueGrowth === null ? "Tháng này" : `${revenueGrowth >= 0 ? "↗" : "↘"} ${Math.abs(revenueGrowth).toFixed(1)}%`}
          label="Doanh thu tháng này"
          value={loading ? "..." : shortCurrency(revenueThisMonth)}
          badgeTone={revenueGrowth !== null && revenueGrowth < 0 ? "text-red-500" : "text-green-500"}
        />
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-[1.95fr_1fr] gap-6 items-start">
        <div className="bg-white rounded-3xl border border-gray-100 p-5 shadow-sm">
          <div className="flex items-start justify-between gap-4 mb-4">
            <div>
              <h2 className="text-gray-900" style={{ fontSize: "17px", fontWeight: 700 }}>
                Doanh thu 6 tháng gần nhất
              </h2>
              <p className="text-gray-400 mt-1" style={{ fontSize: "13px" }}>
                So sánh doanh thu theo tháng
              </p>
            </div>
            <div className="text-right">
              <p className="text-orange-600" style={{ fontSize: "16px", fontWeight: 700 }}>
                {formatCurrency(totalSixMonthRevenue)}
              </p>
              <p className="text-gray-400" style={{ fontSize: "12px" }}>
                Tổng 6 tháng
              </p>
            </div>
          </div>

          <ChartContainer
            config={{
              revenue: {
                label: "Doanh thu",
                color: "#ff6500",
              },
            }}
            className="h-[320px] w-full"
          >
            <BarChart data={monthlyRevenue} margin={{ top: 12, right: 8, left: 8, bottom: 0 }}>
              <CartesianGrid vertical={false} strokeDasharray="3 3" />
              <XAxis
                dataKey="label"
                tickLine={false}
                axisLine={false}
                tickMargin={10}
              />
              <YAxis
                tickLine={false}
                axisLine={false}
                width={44}
                tickFormatter={(value) => `${Math.round(value / 1000000)}tr`}
              />
              <ChartTooltip
                cursor={false}
                content={
                  <ChartTooltipContent
                    formatter={(value) => (
                      <span className="font-semibold text-gray-900">{formatCurrency(Number(value))}</span>
                    )}
                  />
                }
              />
              <Bar dataKey="revenue" radius={[8, 8, 0, 0]} maxBarSize={30}>
                {monthlyRevenue.map((item) => (
                  <Cell
                    key={item.key}
                    fill={item === monthlyRevenue[monthlyRevenue.length - 1] ? "#ff6500" : "#ff7a1a"}
                  />
                ))}
              </Bar>
            </BarChart>
          </ChartContainer>
        </div>

        <div className="space-y-4">
          <SidePanel
            icon={AlertTriangle}
            iconTone="red"
            title="Chưa đóng tiền"
            count={loading ? "..." : String(unpaidInvoices.length)}
            footerLabel="Quản lý hóa đơn"
            onClick={() => navigate("/landlord/billing")}
          >
            {loading ? (
              <PanelSkeleton lines={3} />
            ) : unpaidInvoices.length === 0 ? (
              <EmptyPanelText text="Không có hóa đơn chưa thanh toán." />
            ) : (
              unpaidInvoices.slice(0, 3).map((invoice) => {
                const contract = contractsById[invoice.contractId];
                const propertyName = contract ? propertiesById[contract.propertyId]?.propertyName || `Tài sản #${contract.propertyId}` : `Hợp đồng ${invoice.contractId}`;
                const overdue = new Date(invoice.dueDate).getTime() < Date.now();
                return (
                  <InvoiceRow
                    key={invoice.id}
                    name={propertyName}
                    subtext={`${formatCurrency(invoice.total)} • ${formatPeriod(invoice.period)}`}
                    tone={overdue ? "red" : "amber"}
                    badge={overdue ? "Quá hạn" : "Chưa TT"}
                  />
                );
              })
            )}
          </SidePanel>

          <SidePanel
            icon={CalendarDays}
            iconTone="blue"
            title="Lịch xem phòng mới"
            count={loading ? "..." : String(upcomingAppointments.length)}
          >
            {loading ? (
              <PanelSkeleton lines={2} />
            ) : upcomingAppointments.length === 0 ? (
              <EmptyPanelText text="Chưa có lịch xem nào sắp tới." />
            ) : (
              upcomingAppointments.slice(0, 2).map((appointment) => {
                const propertyName = propertiesById[appointment.propertyId]?.propertyName || `Tài sản #${appointment.propertyId}`;
                const userName = usersById[appointment.userId]?.fullName || `Khách #${appointment.userId}`;
                return (
                  <div key={appointment.id} className="rounded-2xl bg-blue-50 px-4 py-3 border border-blue-100">
                    <div className="flex items-center justify-between gap-3">
                      <div className="min-w-0">
                        <p className="text-gray-900 truncate" style={{ fontSize: "14px", fontWeight: 600 }}>
                          {userName}
                        </p>
                        <p className="text-gray-500 mt-0.5 truncate" style={{ fontSize: "12px" }}>
                          {propertyName}
                        </p>
                        <p className="text-blue-600 mt-1" style={{ fontSize: "12px" }}>
                          {formatDateTime(appointment.appointmentDateTime)}
                        </p>
                      </div>
                      <span className="shrink-0 rounded-full bg-green-500 px-3 py-1 text-white" style={{ fontSize: "11px", fontWeight: 600 }}>
                        {appointment.status}
                      </span>
                    </div>
                  </div>
                );
              })
            )}
          </SidePanel>
        </div>
      </div>

      <div className="bg-white rounded-3xl border border-gray-100 p-5 shadow-sm">
        <div className="flex items-center justify-between gap-4 mb-5">
          <div>
            <h2 className="text-gray-900" style={{ fontSize: "17px", fontWeight: 700 }}>
              Tổng quan phòng trọ
            </h2>
            <p className="text-gray-400 mt-1" style={{ fontSize: "13px" }}>
              Danh sách tài sản hiện có của bạn
            </p>
          </div>
          <button
            onClick={() => navigate("/landlord/properties")}
            className="text-orange-600 hover:text-orange-700 transition-colors"
            style={{ fontSize: "14px", fontWeight: 600 }}
          >
            Quản lý →
          </button>
        </div>

        {loading ? (
          <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-3">
            {Array.from({ length: 6 }).map((_, index) => (
              <div key={index} className="h-20 rounded-2xl bg-gray-100 animate-pulse" />
            ))}
          </div>
        ) : properties.length === 0 ? (
          <div className="rounded-2xl border border-dashed border-gray-200 px-6 py-10 text-center text-gray-400">
            <Building2 className="w-8 h-8 mx-auto mb-2 opacity-40" />
            <p style={{ fontSize: "14px" }}>Chưa có tài sản nào để hiển thị.</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-3">
            {roomOverview.map((room) => (
              <button
                key={room.id}
                onClick={() => navigate(`/rooms/${room.id}?view=landlord`)}
                className="text-left rounded-2xl border border-gray-100 bg-gray-50 px-4 py-3 hover:bg-white hover:border-orange-200 transition-colors"
              >
                <div className="flex items-start justify-between gap-3">
                  <div className="min-w-0">
                    <div className="flex items-center gap-2">
                      <span
                        className={`h-2.5 w-2.5 rounded-full ${
                          room.tone === "green"
                            ? "bg-green-500"
                            : room.tone === "blue"
                              ? "bg-blue-500"
                              : room.tone === "amber"
                                ? "bg-amber-500"
                                : "bg-slate-400"
                        }`}
                      />
                      <p className="truncate text-gray-900" style={{ fontSize: "14px", fontWeight: 600 }}>
                        {room.name}
                      </p>
                    </div>
                    <p className="text-gray-400 mt-1 ml-[18px]" style={{ fontSize: "12px" }}>
                      {formatCurrency(room.price)}/tháng
                    </p>
                  </div>
                  <span
                    className={`shrink-0 rounded-full px-3 py-1 ${
                      room.tone === "green"
                        ? "bg-green-100 text-green-600"
                        : room.tone === "blue"
                          ? "bg-blue-100 text-blue-600"
                          : room.tone === "amber"
                            ? "bg-amber-100 text-amber-700"
                            : "bg-slate-100 text-slate-600"
                    }`}
                    style={{ fontSize: "11px", fontWeight: 600 }}
                  >
                    {room.status}
                  </span>
                </div>
              </button>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

function StatCard({
  icon: Icon,
  tone,
  badge,
  label,
  value,
  badgeTone,
}: {
  icon: React.ElementType;
  tone: "blue" | "green" | "purple" | "orange";
  badge: string;
  label: string;
  value: string;
  badgeTone?: string;
}) {
  const palette = {
    blue: "bg-blue-100 text-blue-600",
    green: "bg-green-100 text-green-600",
    purple: "bg-purple-100 text-purple-600",
    orange: "bg-orange-100 text-orange-600",
  }[tone];

  return (
    <div className="rounded-3xl border border-gray-100 bg-white px-5 py-5 shadow-sm">
      <div className="flex items-center justify-between mb-4">
        <div className={`flex h-12 w-12 items-center justify-center rounded-2xl ${palette}`}>
          <Icon className="w-5 h-5" />
        </div>
        <span className={badgeTone || "text-gray-400"} style={{ fontSize: "12px", fontWeight: 700 }}>
          {badge}
        </span>
      </div>
      <p className="text-gray-900" style={{ fontSize: "38px", fontWeight: 700, lineHeight: 1 }}>
        {value}
      </p>
      <p className="text-gray-500 mt-2" style={{ fontSize: "14px", fontWeight: 500 }}>
        {label}
      </p>
    </div>
  );
}

function SidePanel({
  icon: Icon,
  iconTone,
  title,
  count,
  children,
  footerLabel,
  onClick,
}: {
  icon: React.ElementType;
  iconTone: "red" | "blue";
  title: string;
  count: string;
  children: React.ReactNode;
  footerLabel?: string;
  onClick?: () => void;
}) {
  return (
    <div className="rounded-3xl border border-gray-100 bg-white p-5 shadow-sm">
      <div className="flex items-center gap-2 mb-4">
        <Icon className={`w-5 h-5 ${iconTone === "red" ? "text-red-500" : "text-blue-500"}`} />
        <h3 className="text-gray-900" style={{ fontSize: "16px", fontWeight: 700 }}>
          {title}
        </h3>
        <span
          className={`ml-auto rounded-full px-2.5 py-0.5 ${iconTone === "red" ? "bg-red-100 text-red-500" : "bg-blue-100 text-blue-500"}`}
          style={{ fontSize: "11px", fontWeight: 700 }}
        >
          {count}
        </span>
      </div>

      <div className="space-y-3">{children}</div>

      {footerLabel && onClick ? (
        <button
          onClick={onClick}
          className="mt-4 w-full rounded-2xl py-2 text-orange-600 hover:bg-orange-50 transition-colors"
          style={{ fontSize: "13px", fontWeight: 600 }}
        >
          {footerLabel} →
        </button>
      ) : null}
    </div>
  );
}

function InvoiceRow({
  name,
  subtext,
  tone,
  badge,
}: {
  name: string;
  subtext: string;
  tone: "red" | "amber";
  badge: string;
}) {
  return (
    <div className="flex items-center gap-3">
      <span className={`h-2.5 w-2.5 rounded-full ${tone === "red" ? "bg-red-500" : "bg-amber-400"}`} />
      <div className="min-w-0 flex-1">
        <p className="truncate text-gray-900" style={{ fontSize: "14px", fontWeight: 600 }}>
          {name}
        </p>
        <p className="truncate text-gray-400 mt-0.5" style={{ fontSize: "12px" }}>
          {subtext}
        </p>
      </div>
      <span
        className={`shrink-0 rounded-full px-3 py-1 ${tone === "red" ? "bg-red-100 text-red-500" : "bg-amber-100 text-amber-600"}`}
        style={{ fontSize: "11px", fontWeight: 700 }}
      >
        {badge}
      </span>
    </div>
  );
}

function PanelSkeleton({ lines }: { lines: number }) {
  return (
    <div className="space-y-3">
      {Array.from({ length: lines }).map((_, index) => (
        <div key={index} className="h-14 rounded-2xl bg-gray-100 animate-pulse" />
      ))}
    </div>
  );
}

function EmptyPanelText({ text }: { text: string }) {
  return (
    <div className="rounded-2xl border border-dashed border-gray-200 px-4 py-6 text-center text-gray-400">
      <Receipt className="w-6 h-6 mx-auto mb-2 opacity-40" />
      <p style={{ fontSize: "13px" }}>{text}</p>
    </div>
  );
}

function shortCurrency(value: number) {
  if (value >= 1000000) {
    return `${(value / 1000000).toFixed(1)}tr`;
  }
  return formatCurrency(value);
}

function formatPeriod(period: string) {
  const date = new Date(period);
  if (Number.isNaN(date.getTime())) return period;
  return `T${date.getMonth() + 1}/${date.getFullYear()}`;
}

function formatDateTime(value: string) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return date.toLocaleString("vi-VN", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}
