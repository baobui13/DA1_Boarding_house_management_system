import { useEffect, useMemo, useState } from "react";
import { Receipt, CheckCircle2, AlertCircle, Clock, TriangleAlert, RefreshCw } from "lucide-react";
import { useApp } from "../../context/AppContext";
import { getInvoices, type InvoiceResponse } from "../../lib/invoices";
import { formatCurrency } from "../../lib/format";

type StatusFilter = "all" | "paid" | "pending" | "partial";

function normalizeInvoiceStatus(status: string): StatusFilter | "other" {
  const normalized = status.toLowerCase();
  if (normalized === "paid") return "paid";
  if (normalized === "pending") return "pending";
  if (normalized === "partial") return "partial";
  return "other";
}

export default function BillingPage() {
  const { token } = useApp();
  const [invoices, setInvoices] = useState<InvoiceResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [statusFilter, setStatusFilter] = useState<StatusFilter>("all");

  const loadInvoices = async () => {
    if (!token) {
      setError("Thiếu token đăng nhập để tải hóa đơn.");
      setLoading(false);
      return;
    }

    setLoading(true);
    setError("");

    try {
      const response = await getInvoices(token);
      setInvoices(response.items);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Không tải được hóa đơn.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadInvoices();
  }, [token]);

  const filtered = useMemo(() => {
    return invoices.filter((invoice) => statusFilter === "all" || normalizeInvoiceStatus(invoice.status) === statusFilter);
  }, [invoices, statusFilter]);

  const totalPaid = invoices.filter((invoice) => normalizeInvoiceStatus(invoice.status) === "paid").reduce((sum, invoice) => sum + invoice.total, 0);
  const totalPending = invoices.filter((invoice) => normalizeInvoiceStatus(invoice.status) !== "paid").reduce((sum, invoice) => sum + invoice.total, 0);
  const partialCount = invoices.filter((invoice) => normalizeInvoiceStatus(invoice.status) === "partial").length;

  return (
    <div className="max-w-5xl mx-auto px-4 py-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-gray-900" style={{ fontSize: "22px", fontWeight: 700 }}>
            Quản Lý Hóa Đơn
          </h1>
          <p className="text-gray-500 mt-0.5" style={{ fontSize: "14px" }}>
            Dữ liệu đang lấy từ `Invoice/GetInvoicesByFilter`.
          </p>
        </div>
        <button
          onClick={() => void loadInvoices()}
          className="flex items-center gap-2 px-4 py-2.5 border border-gray-200 text-gray-700 rounded-xl hover:bg-gray-50 transition-colors"
          style={{ fontSize: "14px", fontWeight: 600 }}
        >
          <RefreshCw className="w-4 h-4" />
          Tải lại
        </button>
      </div>

      <div className="mb-4 rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-amber-800">
        <div className="flex items-start gap-3">
          <TriangleAlert className="w-5 h-5 mt-0.5 shrink-0" />
          <p style={{ fontSize: "13px" }}>
            Backend đang khai báo endpoint hóa đơn nhưng [InvoiceController](/Users/trinhngocanh/Downloads/QLT/DA1_Boarding_house_management_system/Backend_Boarding_house_management_system/Backend_Boarding_house_management_system/Controllers/InvoiceController.cs#L13) hiện vẫn `throw NotImplementedException()`. Màn này đã bỏ mock và sẽ hiển thị lỗi thật từ backend.
          </p>
        </div>
      </div>

      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mb-6">
        <SummaryCard tone="green" label="Đã thu" value={formatCurrency(totalPaid)} />
        <SummaryCard tone="yellow" label="Chưa thu" value={formatCurrency(totalPending)} />
        <SummaryCard tone="red" label="Thanh toán dở dang" value={String(partialCount)} />
        <SummaryCard tone="blue" label="Tổng hóa đơn" value={String(invoices.length)} />
      </div>

      <div className="flex items-center gap-2 mb-4">
        {(["all", "pending", "paid", "partial"] as StatusFilter[]).map((status) => (
          <button
            key={status}
            onClick={() => setStatusFilter(status)}
            className={`px-3 py-1.5 rounded-lg border transition-colors ${
              statusFilter === status
                ? "border-orange-400 bg-orange-50 text-orange-600"
                : "border-gray-200 text-gray-500 hover:border-gray-300"
            }`}
            style={{ fontSize: "13px", fontWeight: statusFilter === status ? 600 : 400 }}
          >
            {status === "all" ? "Tất cả" : status === "pending" ? "Pending" : status === "paid" ? "Paid" : "Partial"}
          </button>
        ))}
      </div>

      <div className="bg-white rounded-2xl border border-gray-100 overflow-hidden">
        {error ? (
          <div className="p-6">
            <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-red-600">{error}</div>
          </div>
        ) : loading ? (
          <div className="p-6 space-y-3">
            {Array.from({ length: 5 }).map((_, index) => (
              <div key={index} className="h-14 rounded-xl bg-gray-100 animate-pulse" />
            ))}
          </div>
        ) : filtered.length === 0 ? (
          <div className="text-center py-12 text-gray-400">
            <Receipt className="w-8 h-8 mx-auto mb-2 opacity-40" />
            <p style={{ fontSize: "14px" }}>Không có hóa đơn để hiển thị</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="bg-gray-50 border-b border-gray-100">
                  <th className="text-left px-4 py-3 text-gray-500" style={{ fontSize: "12px", fontWeight: 600 }}>KỲ HÓA ĐƠN</th>
                  <th className="text-left px-4 py-3 text-gray-500" style={{ fontSize: "12px", fontWeight: 600 }}>HỢP ĐỒNG</th>
                  <th className="text-right px-4 py-3 text-gray-500" style={{ fontSize: "12px", fontWeight: 600 }}>TỔNG TIỀN</th>
                  <th className="text-center px-4 py-3 text-gray-500" style={{ fontSize: "12px", fontWeight: 600 }}>TRẠNG THÁI</th>
                  <th className="text-left px-4 py-3 text-gray-500" style={{ fontSize: "12px", fontWeight: 600 }}>CHI TIẾT</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-50">
                {filtered.map((invoice) => (
                  <tr key={invoice.id} className="hover:bg-gray-50/50 transition-colors">
                    <td className="px-4 py-3">
                      <p className="text-gray-800" style={{ fontSize: "13px", fontWeight: 600 }}>
                        {new Date(invoice.period).toLocaleDateString("vi-VN", { month: "2-digit", year: "numeric" })}
                      </p>
                      <p className="text-gray-400" style={{ fontSize: "11px" }}>
                        Hạn: {new Date(invoice.dueDate).toLocaleDateString("vi-VN")}
                      </p>
                    </td>
                    <td className="px-4 py-3">
                      <span className="text-gray-600" style={{ fontSize: "13px" }}>
                        {invoice.contractId}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-right">
                      <span className="text-gray-900" style={{ fontSize: "14px", fontWeight: 700 }}>
                        {formatCurrency(invoice.total)}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-center">
                      <StatusBadge status={invoice.status} />
                    </td>
                    <td className="px-4 py-3">
                      <div className="grid grid-cols-2 gap-2 text-gray-600" style={{ fontSize: "12px" }}>
                        <span>Tiền phòng: {formatCurrency(invoice.rentAmount)}</span>
                        <span>Điện: {formatCurrency(invoice.electricityCost || 0)}</span>
                        <span>Nước: {formatCurrency(invoice.waterCost || 0)}</span>
                        <span>Phí khác: {formatCurrency(invoice.otherFees || 0)}</span>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}

function SummaryCard({ tone, label, value }: { tone: "green" | "yellow" | "red" | "blue"; label: string; value: string }) {
  const styles = {
    green: "bg-green-50 border-green-100 text-green-700 text-green-600",
    yellow: "bg-yellow-50 border-yellow-100 text-yellow-700 text-yellow-600",
    red: "bg-red-50 border-red-100 text-red-700 text-red-600",
    blue: "bg-blue-50 border-blue-100 text-blue-700 text-blue-600",
  }[tone].split(" ");

  return (
    <div className={`${styles[0]} border ${styles[1]} rounded-2xl p-4`}>
      <p className={`${styles[3]} mb-1`} style={{ fontSize: "12px" }}>{label}</p>
      <p className={styles[2]} style={{ fontSize: "18px", fontWeight: 700 }}>{value}</p>
    </div>
  );
}

function StatusBadge({ status }: { status: string }) {
  const normalized = status.toLowerCase();

  if (normalized === "paid") {
    return (
      <span className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-xl bg-green-100 text-green-700" style={{ fontSize: "11px", fontWeight: 600 }}>
        <CheckCircle2 className="w-3 h-3" />
        Paid
      </span>
    );
  }

  if (normalized === "partial") {
    return (
      <span className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-xl bg-red-100 text-red-600" style={{ fontSize: "11px", fontWeight: 600 }}>
        <AlertCircle className="w-3 h-3" />
        Partial
      </span>
    );
  }

  return (
    <span className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-xl bg-yellow-100 text-yellow-700" style={{ fontSize: "11px", fontWeight: 600 }}>
      <Clock className="w-3 h-3" />
      {status}
    </span>
  );
}
