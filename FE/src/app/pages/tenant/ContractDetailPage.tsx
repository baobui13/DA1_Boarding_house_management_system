import { useEffect, useState } from "react";
import { useParams, useNavigate, Link } from "react-router";
import { ArrowLeft, FileText, CheckCircle2, AlertCircle, XCircle, TriangleAlert } from "lucide-react";
import { useApp } from "../../context/AppContext";
import { getContractById, type ContractResponse } from "../../lib/contracts";
import { formatCurrency } from "../../lib/format";

export default function ContractDetailPage() {
  const { id = "" } = useParams();
  const navigate = useNavigate();
  const { token } = useApp();
  const [contract, setContract] = useState<ContractResponse | null>(null);
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
        const response = await getContractById(token, id);
        if (!cancelled) setContract(response);
      } catch (err) {
        if (!cancelled) setError(err instanceof Error ? err.message : "Không tải được hợp đồng.");
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [id, token]);

  const statusConfig = (() => {
    const status = contract?.status.toLowerCase();
    if (status === "active") return { label: "Còn hiệu lực", color: "text-green-700 bg-green-100", icon: CheckCircle2 };
    if (status === "expired") return { label: "Hết hạn", color: "text-red-700 bg-red-100", icon: XCircle };
    return { label: contract?.status || "Không xác định", color: "text-gray-700 bg-gray-100", icon: AlertCircle };
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
              Chi Tiết Hợp Đồng
            </h1>
            <p className="text-gray-500 mt-0.5" style={{ fontSize: "14px" }}>
              Mã hợp đồng: <span className="font-mono">{id}</span>
            </p>
          </div>
          {contract && (
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
            `ContractController.GetContractById` hiện chưa implement ở backend. Trang này đã chuyển sang API thật và hiển thị lỗi backend thực tế.
          </p>
        </div>
      </div>

      {loading ? (
        <div className="h-48 rounded-2xl bg-gray-100 animate-pulse" />
      ) : error ? (
        <div className="rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-red-600">{error}</div>
      ) : !contract ? (
        <div className="rounded-2xl border border-gray-200 bg-white px-4 py-6 text-gray-500">Không có dữ liệu hợp đồng.</div>
      ) : (
        <div className="bg-white rounded-2xl border border-gray-100 overflow-hidden">
          <div className="bg-gradient-to-r from-orange-500 to-amber-500 p-6 text-white">
            <div className="flex items-center gap-2 mb-2">
              <FileText className="w-6 h-6" />
              <h2 style={{ fontSize: "18px", fontWeight: 700 }}>HỢP ĐỒNG THUÊ</h2>
            </div>
            <p style={{ fontSize: "13px" }}>RoomId: {contract.roomId}</p>
          </div>
          <div className="p-6 grid grid-cols-1 md:grid-cols-2 gap-4">
            <Info label="TenantId" value={contract.tenantId} />
            <Info label="Tiền cọc" value={formatCurrency(contract.deposit)} />
            <Info label="Ngày bắt đầu" value={new Date(contract.startDate).toLocaleDateString("vi-VN")} />
            <Info label="Ngày kết thúc" value={new Date(contract.endDate).toLocaleDateString("vi-VN")} />
            <Info label="Hoàn cọc" value={formatCurrency(contract.refundAmount)} />
            <Info label="Khấu trừ" value={formatCurrency(contract.deductionAmount)} />
            <Info label="Terms" value={contract.terms || "Backend chưa trả terms"} />
            <Info label="Ghi chú bàn giao" value={contract.handoverNote || "Chưa có"} />
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
