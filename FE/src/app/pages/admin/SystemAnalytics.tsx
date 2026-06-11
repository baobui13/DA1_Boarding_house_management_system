import { useEffect, useMemo, useState } from "react";
import { Activity, Building2, Home, ReceiptText, TrendingUp, TriangleAlert, Users } from "lucide-react";
import { useApp } from "../../context/AppContext";
import { getAreas } from "../../lib/areas";
import { getContracts } from "../../lib/contracts";
import { formatCurrency } from "../../lib/format";
import { getInvoices } from "../../lib/invoices";
import { getPropertyListings } from "../../lib/properties";
import { isAvailablePropertyStatus, isMaintenancePropertyStatus, isRentedPropertyStatus } from "../../lib/propertyStatus";
import type { PropertyListing, UserResponse } from "../../lib/types";
import { getUsers } from "../../lib/users";

type InvoiceSummary = {
  total: number;
  createdAt: string;
  dueDate: string;
  status: string;
};

export default function SystemAnalytics() {
  const { token } = useApp();
  const [users, setUsers] = useState<UserResponse[]>([]);
  const [areasCount, setAreasCount] = useState(0);
  const [properties, setProperties] = useState<PropertyListing[]>([]);
  const [contractsCount, setContractsCount] = useState(0);
  const [invoices, setInvoices] = useState<InvoiceSummary[]>([]);
  const [errors, setErrors] = useState<string[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;

    (async () => {
      setLoading(true);
      const nextErrors: string[] = [];
      const toErrorMessage = (label: string, err: unknown) => {
        if (err instanceof Error && "status" in err) {
          const status = (err as Error & { status?: number }).status;
          if (status === 401 || status === 403) {
            return `${label}: Phiên đăng nhập admin không hợp lệ hoặc đã hết hạn. Hãy đăng nhập lại.`;
          }
        }

        if (err instanceof Error && err.message.trim()) {
          return `${label}: ${err.message}`;
        }

        return `${label}: Không tải được dữ liệu.`;
      };

      await Promise.all([
        getUsers({ page: 1, pageSize: 1000 }, token)
          .then((response) => {
            if (!cancelled) setUsers(response.items);
          })
          .catch((err) => nextErrors.push(toErrorMessage("Users", err))),
        getAreas({ page: 1, pageSize: 1000 })
          .then((response) => {
            if (!cancelled) setAreasCount(response.totalCount || response.items.length);
          })
          .catch((err) => nextErrors.push(toErrorMessage("Areas", err))),
        getPropertyListings({ page: 1, pageSize: 1000 })
          .then((response) => {
            if (!cancelled) setProperties(response.items);
          })
          .catch((err) => nextErrors.push(toErrorMessage("Properties", err))),
        token
          ? getContracts(token, { page: 1, pageSize: 1000 })
              .then((response) => {
                if (!cancelled) setContractsCount(response.totalCount || response.items.length);
              })
              .catch((err) => nextErrors.push(toErrorMessage("Contracts", err)))
          : Promise.resolve(),
        token
          ? getInvoices(token, { page: 1, pageSize: 1000 })
              .then((response) => {
                if (!cancelled) setInvoices(response.items);
              })
              .catch((err) => nextErrors.push(toErrorMessage("Invoices", err)))
          : Promise.resolve(),
      ]);

      if (!cancelled) {
        setErrors(nextErrors);
        setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [token]);

  const landlordsCount = users.filter((user) => user.role.toLowerCase() === "landlord").length;
  const tenantsCount = users.filter((user) => user.role.toLowerCase() === "tenant").length;
  const availableCount = properties.filter((item) => isAvailablePropertyStatus(item.status)).length;
  const rentedCount = properties.filter((item) => isRentedPropertyStatus(item.status)).length;
  const unavailableCount = properties.filter((item) => isMaintenancePropertyStatus(item.status)).length;
  const totalRevenue = invoices.reduce((sum, item) => sum + item.total, 0);
  const paidRevenue = invoices
    .filter((item) => item.status.toLowerCase() === "paid")
    .reduce((sum, item) => sum + item.total, 0);
  const pendingInvoices = invoices.filter((item) => item.status.toLowerCase() === "pending").length;
  const partialInvoices = invoices.filter((item) => item.status.toLowerCase() === "partial").length;

  const propertiesByMonth = useMemo(() => {
    const map = new Map<string, number>();
    properties.forEach((item) => {
      const key = new Date(item.createdAt).toLocaleDateString("vi-VN", { month: "2-digit", year: "2-digit" });
      map.set(key, (map.get(key) || 0) + 1);
    });
    return Array.from(map.entries()).slice(-6);
  }, [properties]);

  const revenueByMonth = useMemo(() => {
    const map = new Map<string, number>();
    invoices.forEach((item) => {
      const key = new Date(item.createdAt).toLocaleDateString("vi-VN", { month: "2-digit", year: "2-digit" });
      map.set(key, (map.get(key) || 0) + item.total);
    });
    return Array.from(map.entries()).slice(-6);
  }, [invoices]);

  const newestProperties = useMemo(
    () => [...properties].sort((a, b) => +new Date(b.createdAt) - +new Date(a.createdAt)).slice(0, 5),
    [properties],
  );

  const latestInvoices = useMemo(
    () => [...invoices].sort((a, b) => +new Date(b.createdAt) - +new Date(a.createdAt)).slice(0, 5),
    [invoices],
  );

  return (
    <div className="max-w-7xl mx-auto px-4 py-6">
      <div className="mb-6">
        <h1 className="text-gray-900" style={{ fontSize: "22px", fontWeight: 700 }}>
          Báo Cáo Hệ Thống
        </h1>
      </div>

      {errors.length > 0 && (
        <div className="mb-6 rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-amber-800">
          <div className="flex items-start gap-3">
            <TriangleAlert className="w-5 h-5 mt-0.5 shrink-0" />
            <div style={{ fontSize: "13px" }}>
              {errors.map((item) => (
                <p key={item}>{item}</p>
              ))}
            </div>
          </div>
        </div>
      )}

      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        <MetricCard icon={Users} tone="blue" value={loading ? "..." : String(users.length)} label="Tổng người dùng" />
        <MetricCard icon={Building2} tone="orange" value={loading ? "..." : String(areasCount)} label="Khu trọ" />
        <MetricCard icon={Home} tone="green" value={loading ? "..." : String(properties.length)} label="Phòng / tài sản" />
        <MetricCard icon={TrendingUp} tone="purple" value={loading ? "..." : formatCurrency(totalRevenue)} label="Tổng doanh thu hóa đơn" />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
        <Panel title="Cơ cấu người dùng">
          <StatusRow label="Khách thuê" count={tenantsCount} tone="blue" />
          <StatusRow label="Chủ trọ" count={landlordsCount} tone="orange" />
          <StatusRow label="Admin" count={users.filter((user) => user.role.toLowerCase() === "admin").length} tone="purple" />
        </Panel>

        <Panel title="Tình trạng phòng / tài sản">
          <StatusRow label="Available" count={availableCount} tone="green" />
          <StatusRow label="Rented" count={rentedCount} tone="blue" />
          <StatusRow label="Unavailable" count={unavailableCount} tone="red" />
        </Panel>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
        <Panel title="Phòng tạo theo tháng">
          {propertiesByMonth.length === 0 ? (
            <p className="text-gray-400" style={{ fontSize: "13px" }}>Chưa có dữ liệu.</p>
          ) : (
            <div className="space-y-3">
              {propertiesByMonth.map(([month, count]) => (
                <BarRow key={month} label={month} value={count} width={Math.min(100, count * 8)} formatter={(val) => String(val)} tone="orange" />
              ))}
            </div>
          )}
        </Panel>

        <Panel title="Doanh thu hóa đơn theo tháng">
          {revenueByMonth.length === 0 ? (
            <p className="text-gray-400" style={{ fontSize: "13px" }}>Chưa có dữ liệu hóa đơn.</p>
          ) : (
            <div className="space-y-3">
              {revenueByMonth.map(([month, total]) => (
                <BarRow
                  key={month}
                  label={month}
                  value={total}
                  width={Math.min(100, Math.round((total / Math.max(...revenueByMonth.map((item) => item[1]), 1)) * 100))}
                  formatter={(val) => formatCurrency(val)}
                  tone="purple"
                />
              ))}
            </div>
          )}
        </Panel>
      </div>

      <div className="mb-6">
        <Panel title="Hóa đơn & hợp đồng">
          <div className="grid grid-cols-2 gap-3">
            <MiniMetric label="Tổng hợp đồng" value={loading ? "..." : String(contractsCount)} />
            <MiniMetric label="Hóa đơn chờ thanh toán" value={loading ? "..." : String(pendingInvoices)} />
            <MiniMetric label="Hóa đơn thanh toán một phần" value={loading ? "..." : String(partialInvoices)} />
            <MiniMetric label="Doanh thu đã thu" value={loading ? "..." : formatCurrency(paidRevenue)} />
          </div>
        </Panel>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Panel title="Phòng mới cập nhật">
          {newestProperties.length === 0 ? (
            <p className="text-gray-400" style={{ fontSize: "13px" }}>Chưa có dữ liệu.</p>
          ) : (
            <div className="space-y-3">
              {newestProperties.map((property) => (
                <SimpleRow
                  key={property.id}
                  title={property.propertyName}
                  subtitle={property.address || "Chưa có địa chỉ"}
                  rightTop={formatCurrency(property.price)}
                  rightBottom={property.status}
                />
              ))}
            </div>
          )}
        </Panel>

        <Panel title="Hóa đơn gần đây">
          {latestInvoices.length === 0 ? (
            <p className="text-gray-400" style={{ fontSize: "13px" }}>Chưa có dữ liệu hóa đơn.</p>
          ) : (
            <div className="space-y-3">
              {latestInvoices.map((invoice) => (
                <SimpleRow
                  key={invoice.id}
                  title={`Kỳ ${new Date(invoice.dueDate).toLocaleDateString("vi-VN", { month: "2-digit", year: "numeric" })}`}
                  subtitle={`Hạn: ${new Date(invoice.dueDate).toLocaleDateString("vi-VN")}`}
                  rightTop={formatCurrency(invoice.total)}
                  rightBottom={invoice.status}
                />
              ))}
            </div>
          )}
        </Panel>
      </div>
    </div>
  );
}

function MetricCard({
  icon: Icon,
  tone,
  value,
  label,
}: {
  icon: React.ElementType;
  tone: "blue" | "orange" | "green" | "purple";
  value: string;
  label: string;
}) {
  const styles = {
    blue: "bg-blue-50 border-blue-100 text-blue-700 bg-blue-100 text-blue-600",
    orange: "bg-orange-50 border-orange-100 text-orange-700 bg-orange-100 text-orange-600",
    green: "bg-green-50 border-green-100 text-green-700 bg-green-100 text-green-600",
    purple: "bg-purple-50 border-purple-100 text-purple-700 bg-purple-100 text-purple-600",
  }[tone].split(" ");
  return (
    <div className={`${styles[0]} border ${styles[1]} rounded-2xl p-5`}>
      <div className={`w-10 h-10 rounded-xl ${styles[3]} flex items-center justify-center mb-3`}>
        <Icon className={`w-5 h-5 ${styles[4]}`} />
      </div>
      <p className={styles[2]} style={{ fontSize: "24px", fontWeight: 700 }}>{value}</p>
      <p className="text-gray-500 mt-0.5" style={{ fontSize: "13px" }}>{label}</p>
    </div>
  );
}

function Panel({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div className="bg-white rounded-2xl border border-gray-100 p-5">
      <div className="flex items-center gap-2 mb-4">
        <Activity className="w-5 h-5 text-blue-500" />
        <h2 className="text-gray-900" style={{ fontSize: "15px", fontWeight: 700 }}>
          {title}
        </h2>
      </div>
      {children}
    </div>
  );
}

function StatusRow({ label, count, tone }: { label: string; count: number; tone: "yellow" | "green" | "red" | "blue" | "orange" | "purple" }) {
  const color = {
    yellow: "bg-yellow-500",
    green: "bg-green-500",
    red: "bg-red-500",
    blue: "bg-blue-500",
    orange: "bg-orange-500",
    purple: "bg-purple-500",
  }[tone];
  return (
    <div className="mb-4 last:mb-0">
      <div className="flex items-center justify-between mb-1">
        <span className="text-gray-600" style={{ fontSize: "13px" }}>{label}</span>
        <span className="text-gray-800" style={{ fontSize: "13px", fontWeight: 600 }}>{count}</span>
      </div>
      <div className="h-2 bg-gray-100 rounded-full overflow-hidden">
        <div className={`h-full rounded-full ${color}`} style={{ width: `${Math.min(100, count * 10)}%` }} />
      </div>
    </div>
  );
}

function BarRow({
  label,
  value,
  width,
  formatter,
  tone,
}: {
  label: string;
  value: number;
  width: number;
  formatter: (value: number) => string;
  tone: "orange" | "purple";
}) {
  const color = tone === "orange" ? "bg-orange-500" : "bg-purple-500";
  return (
    <div>
      <div className="flex items-center justify-between mb-1">
        <span className="text-gray-600" style={{ fontSize: "13px" }}>{label}</span>
        <span className="text-gray-800" style={{ fontSize: "13px", fontWeight: 600 }}>{formatter(value)}</span>
      </div>
      <div className="h-2 bg-gray-100 rounded-full overflow-hidden">
        <div className={`h-full rounded-full ${color}`} style={{ width: `${width}%` }} />
      </div>
    </div>
  );
}

function MiniMetric({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-xl border border-gray-100 bg-gray-50 px-4 py-3">
      <p className="text-gray-500" style={{ fontSize: "12px" }}>{label}</p>
      <p className="text-gray-900 mt-1" style={{ fontSize: "16px", fontWeight: 700 }}>{value}</p>
    </div>
  );
}

function SimpleRow({
  title,
  subtitle,
  rightTop,
  rightBottom,
}: {
  title: string;
  subtitle: string;
  rightTop: string;
  rightBottom: string;
}) {
  return (
    <div className="flex items-start justify-between gap-4 rounded-xl border border-gray-100 px-4 py-3">
      <div className="min-w-0">
        <p className="text-gray-900 truncate" style={{ fontSize: "14px", fontWeight: 700 }}>{title}</p>
        <p className="text-gray-500 mt-0.5 truncate" style={{ fontSize: "12px" }}>{subtitle}</p>
      </div>
      <div className="text-right shrink-0">
        <p className="text-gray-900" style={{ fontSize: "13px", fontWeight: 700 }}>{rightTop}</p>
        <p className="text-gray-500 mt-0.5" style={{ fontSize: "12px" }}>{rightBottom}</p>
      </div>
    </div>
  );
}
