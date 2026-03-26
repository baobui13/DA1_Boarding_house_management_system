import { useEffect, useMemo, useState } from "react";
import { TrendingUp, Users, Building2, CheckCircle2, Activity, TriangleAlert } from "lucide-react";
import { getUsers } from "../../lib/users";
import { getPropertyListings } from "../../lib/properties";
import { getInvoices } from "../../lib/invoices";
import { useApp } from "../../context/AppContext";
import { formatCurrency } from "../../lib/format";

export default function SystemAnalytics() {
  const { token } = useApp();
  const [usersCount, setUsersCount] = useState(0);
  const [properties, setProperties] = useState<{ createdAt: string; status: string; price: number }[]>([]);
  const [revenue, setRevenue] = useState<number | null>(null);
  const [errors, setErrors] = useState<string[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;

    (async () => {
      setLoading(true);
      const nextErrors: string[] = [];

      await Promise.all([
        getUsers()
          .then((response) => {
            if (!cancelled) setUsersCount(response.totalCount || response.items.length);
          })
          .catch((err) => nextErrors.push(err instanceof Error ? `Users: ${err.message}` : "Users API lỗi")),
        getPropertyListings()
          .then((response) => {
            if (!cancelled) setProperties(response.items);
          })
          .catch((err) => nextErrors.push(err instanceof Error ? `Properties: ${err.message}` : "Property API lỗi")),
        token
          ? getInvoices(token)
              .then((response) => {
                if (!cancelled) setRevenue(response.items.reduce((sum, item) => sum + item.total, 0));
              })
              .catch((err) => nextErrors.push(err instanceof Error ? `Invoices: ${err.message}` : "Invoice API lỗi"))
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

  const approvedCount = properties.filter((item) => item.status.toLowerCase() === "approved").length;
  const pendingCount = properties.filter((item) => item.status.toLowerCase() === "pendingapproval").length;
  const createdByMonth = useMemo(() => {
    const map = new Map<string, number>();
    properties.forEach((item) => {
      const key = new Date(item.createdAt).toLocaleDateString("vi-VN", { month: "2-digit", year: "2-digit" });
      map.set(key, (map.get(key) || 0) + 1);
    });
    return Array.from(map.entries()).slice(-6);
  }, [properties]);

  return (
    <div className="max-w-7xl mx-auto px-4 py-6">
      <div className="mb-6">
        <h1 className="text-gray-900" style={{ fontSize: "22px", fontWeight: 700 }}>
          Báo Cáo Hệ Thống
        </h1>
        <p className="text-gray-500 mt-0.5" style={{ fontSize: "14px" }}>
          Trang analytics đã bỏ mock và tổng hợp từ API thật hiện có.
        </p>
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
        <MetricCard icon={Users} tone="blue" value={loading ? "..." : String(usersCount)} label="Người dùng" />
        <MetricCard icon={Building2} tone="orange" value={loading ? "..." : String(properties.length)} label="Tin / tài sản" />
        <MetricCard icon={CheckCircle2} tone="green" value={loading ? "..." : String(approvedCount)} label="Đã duyệt" />
        <MetricCard icon={TrendingUp} tone="purple" value={loading ? "..." : revenue === null ? "--" : formatCurrency(revenue)} label="Doanh thu" />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
        <div className="bg-white rounded-2xl border border-gray-100 p-5">
          <h2 className="text-gray-900 mb-4" style={{ fontSize: "15px", fontWeight: 700 }}>
            Tin tạo theo tháng
          </h2>
          <div className="space-y-3">
            {createdByMonth.length === 0 ? (
              <p className="text-gray-400" style={{ fontSize: "13px" }}>Chưa có dữ liệu.</p>
            ) : (
              createdByMonth.map(([month, count]) => (
                <div key={month}>
                  <div className="flex items-center justify-between mb-1">
                    <span className="text-gray-600" style={{ fontSize: "13px" }}>{month}</span>
                    <span className="text-gray-800" style={{ fontSize: "13px", fontWeight: 600 }}>{count}</span>
                  </div>
                  <div className="h-2 bg-gray-100 rounded-full overflow-hidden">
                    <div className="h-full rounded-full bg-orange-500" style={{ width: `${Math.min(100, count * 10)}%` }} />
                  </div>
                </div>
              ))
            )}
          </div>
        </div>

        <div className="bg-white rounded-2xl border border-gray-100 p-5">
          <h2 className="text-gray-900 mb-4" style={{ fontSize: "15px", fontWeight: 700 }}>
            Tình trạng kiểm duyệt
          </h2>
          <div className="space-y-4">
            <StatusRow label="PendingApproval" count={pendingCount} tone="yellow" />
            <StatusRow label="Approved" count={approvedCount} tone="green" />
            <StatusRow label="Rejected" count={properties.filter((item) => item.status.toLowerCase() === "rejected").length} tone="red" />
            <StatusRow label="Available" count={properties.filter((item) => item.status.toLowerCase() === "available").length} tone="blue" />
          </div>
        </div>
      </div>

      <div className="bg-white rounded-2xl border border-gray-100 p-5">
        <div className="flex items-center gap-2 mb-4">
          <Activity className="w-5 h-5 text-blue-500" />
          <h2 className="text-gray-900" style={{ fontSize: "15px", fontWeight: 700 }}>
            Ghi chú hệ thống
          </h2>
        </div>
        <div className="space-y-3 text-gray-600" style={{ fontSize: "13px" }}>
          <p>User analytics đang lấy được từ `User/GetUsersByFilter`.</p>
          <p>Property analytics đang lấy được từ `Property/GetPropertiesByFilter`.</p>
          <p>Revenue hiện phụ thuộc `Invoice/GetInvoicesByFilter`; nếu backend chưa implement thì card doanh thu sẽ hiển thị `--`.</p>
        </div>
      </div>
    </div>
  );
}

function MetricCard({ icon: Icon, tone, value, label }: { icon: React.ElementType; tone: "blue" | "orange" | "green" | "purple"; value: string; label: string }) {
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

function StatusRow({ label, count, tone }: { label: string; count: number; tone: "yellow" | "green" | "red" | "blue" }) {
  const color = {
    yellow: "bg-yellow-500",
    green: "bg-green-500",
    red: "bg-red-500",
    blue: "bg-blue-500",
  }[tone];
  return (
    <div>
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
