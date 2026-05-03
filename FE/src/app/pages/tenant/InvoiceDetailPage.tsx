import { useEffect, useState } from "react";
import { useParams, useNavigate, Link } from "react-router";
import { ArrowLeft, Receipt, CheckCircle2, AlertCircle, XCircle, TriangleAlert } from "lucide-react";
import { useApp } from "../../context/AppContext";
import { getInvoiceById, type InvoiceResponse } from "../../lib/invoices";
import { formatCurrency } from "../../lib/format";

export default function InvoiceDetailPage() {
  const { id = "" } = useParams();
  const navigate = useNavigate();
  const { token } = useApp();
  const [invoice, setInvoice] = useState<InvoiceResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    let cancelled = false;

    (async () => {
      if (!token) {
        setError("Thiếu token đăng nhập.");
        setLoading(false);
        return;
      }

      try {
        const response = await getInvoiceById(token, id);
        if (!cancelled) setInvoice(response);
      } catch (err) {
        if (!cancelled) setError(err instanceof Error ? err.message : "Không tải được hóa đơn.");
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [id, token]);

  const statusConfig = (() => {
    const status = invoice?.status.toLowerCase();
    if (status === "paid") return { label: "Đã thanh toán", color: "text-green-700 bg-green-100", icon: CheckCircle2 };
    if (status === "partial") return { label: "Thanh toán một phần", color: "text-red-700 bg-red-100", icon: XCircle };
    return { label: invoice?.status || "Không xác định", color: "text-amber-700 bg-amber-100", icon: AlertCircle };
  })();
  const StatusIcon = statusConfig.icon;

  return (
    <div className="max-w-4xl mx-auto px-4 py-6">
      <div className="mb-6">
        <Link to="/tenant/dashboard" className="inline-flex items-center gap-2 text-gray-600 hover:text-gray-900 mb-4 transition-colors" style={{ fontSize: "14px", fontWeight: 500 }}>
          <ArrowLeft className="w-4 h-4" />
          Quay lại
        </Link>
        <div className="flex items-start justify-between">
          <div>
            <h1 className="text-gray-900" style={{ fontSize: "22px", fontWeight: 700 }}>
              Chi Tiết Hóa Đơn
            </h1>
            <p className="text-gray-500 mt-0.5" style={{ fontSize: "14px" }}>
              Mã hóa đơn: <span className="font-mono">{id}</span>
            </p>
          </div>
          {invoice && (
            <div className={`flex items-center gap-2 px-3 py-1.5 rounded-lg ${statusConfig.color}`}>
              <StatusIcon className="w-4 h-4" />
              <span style={{ fontSize: "13px", fontWeight: 600 }}>{statusConfig.label}</span>
            </div>
          )}
        </div>
      </div>

      <div className="mb-6 rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-amber-800">
        <div className="flex items-start gap-3">
          <TriangleAlert className="w-5 h-5 mt-0.5 shrink-0" />
          <p style={{ fontSize: "13px" }}>
            `InvoiceController.GetInvoiceById` hiện chưa implement ở backend. Trang này đã chuyển sang gọi API thật, nên nếu backend chưa chạy sẽ thấy lỗi thật bên dưới.
          </p>
        </div>
      </div>

      {loading ? (
        <div className="h-48 rounded-2xl bg-gray-100 animate-pulse" />
      ) : error ? (
        <div className="rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-red-600">{error}</div>
      ) : !invoice ? (
        <div className="rounded-2xl border border-gray-200 bg-white px-4 py-6 text-gray-500">Không có dữ liệu hóa đơn.</div>
      ) : (
        <div className="bg-white rounded-2xl border border-gray-100 overflow-hidden">
          <div className="bg-gradient-to-r from-orange-500 to-amber-500 p-6 text-white">
            <div className="flex items-center gap-2 mb-2">
              <Receipt className="w-6 h-6" />
              <h2 style={{ fontSize: "18px", fontWeight: 700 }}>HÓA ĐƠN</h2>
            </div>
            <p style={{ fontSize: "26px", fontWeight: 700 }}>{formatCurrency(invoice.total)}</p>
          </div>
          <div className="p-6 grid grid-cols-1 md:grid-cols-2 gap-4">
            <Info label="Kỳ hóa đơn" value={new Date(invoice.period).toLocaleDateString("vi-VN", { month: "2-digit", year: "numeric" })} />
            <Info label="ContractId" value={invoice.contractId} />
            <Info label="Tiền phòng" value={formatCurrency(invoice.rentAmount)} />
            <Info label="Tiền điện" value={formatCurrency(invoice.electricityCost || 0)} />
            <Info label="Tiền nước" value={formatCurrency(invoice.waterCost || 0)} />
            <Info label="Phí khác" value={formatCurrency(invoice.otherFees || 0)} />
            <Info label="Phạt" value={formatCurrency(invoice.penalty)} />
            <Info label="Hạn thanh toán" value={new Date(invoice.dueDate).toLocaleDateString("vi-VN")} />
          </div>
        </div>
      )}

      <button
        onClick={() => navigate("/tenant/dashboard")}
        className="mt-6 px-6 py-2.5 rounded-xl bg-orange-500 text-white hover:bg-orange-600 transition-colors"
        style={{ fontSize: "14px", fontWeight: 600 }}
      >
        Quay lại Dashboard
      </button>
    </div>
  );
}

function Info({ label, value }: { label: string; value: string }) {
  return (
    <div className="bg-gray-50 rounded-xl p-4">
      <p className="text-gray-500" style={{ fontSize: "12px" }}>{label}</p>
      <p className="text-gray-900 mt-1" style={{ fontSize: "14px", fontWeight: 600 }}>{value}</p>
    </div>
  );
}
