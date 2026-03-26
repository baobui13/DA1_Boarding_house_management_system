import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router";
import { FileText, Settings, RefreshCw, TriangleAlert, CheckCircle2, Clock, XCircle } from "lucide-react";
import { useApp } from "../../context/AppContext";
import { getContracts, type ContractResponse } from "../../lib/contracts";
import { getPropertyListings } from "../../lib/properties";
import { formatCurrency } from "../../lib/format";

type StatusKey = "all" | "active" | "expired" | "terminated";

function normalizeContractStatus(status: string): StatusKey | "other" {
  const normalized = status.toLowerCase();
  if (normalized === "active") return "active";
  if (normalized === "expired") return "expired";
  if (normalized === "terminated") return "terminated";
  return "other";
}

export default function ContractManagement() {
  const navigate = useNavigate();
  const { token } = useApp();
  const [contracts, setContracts] = useState<ContractResponse[]>([]);
  const [roomNames, setRoomNames] = useState<Record<string, string>>({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [statusFilter, setStatusFilter] = useState<StatusKey>("all");

  const loadContracts = async () => {
    if (!token) {
      setError("Thiếu token đăng nhập để tải hợp đồng.");
      setLoading(false);
      return;
    }

    setLoading(true);
    setError("");

    try {
      const [contractResponse, propertyResponse] = await Promise.all([
        getContracts(token),
        getPropertyListings().catch(() => ({ items: [] })),
      ]);

      setContracts(contractResponse.items);
      setRoomNames(
        Object.fromEntries(propertyResponse.items.map((item) => [item.id, item.propertyName])),
      );
    } catch (err) {
      setError(err instanceof Error ? err.message : "Không tải được hợp đồng.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadContracts();
  }, [token]);

  const filtered = useMemo(() => {
    return contracts.filter((contract) => statusFilter === "all" || normalizeContractStatus(contract.status) === statusFilter);
  }, [contracts, statusFilter]);

  const activeCount = contracts.filter((contract) => normalizeContractStatus(contract.status) === "active").length;
  const expiredCount = contracts.filter((contract) => normalizeContractStatus(contract.status) === "expired").length;
  const terminatedCount = contracts.filter((contract) => normalizeContractStatus(contract.status) === "terminated").length;

  return (
    <div className="max-w-5xl mx-auto px-4 py-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-gray-900" style={{ fontSize: "22px", fontWeight: 700 }}>
            Quản Lý Hợp Đồng
          </h1>
          <p className="text-gray-500 mt-0.5" style={{ fontSize: "14px" }}>
            Dữ liệu đang lấy từ `Contract/GetContractsByFilter`.
          </p>
        </div>
        <div className="flex gap-2">
          <button
            onClick={() => navigate("/landlord/contract-templates")}
            className="flex items-center gap-2 px-4 py-2.5 border border-purple-200 bg-purple-50 text-purple-600 rounded-xl hover:bg-purple-100 transition-colors"
            style={{ fontSize: "14px", fontWeight: 600 }}
          >
            <Settings className="w-4 h-4" />
            Quản lý mẫu
          </button>
          <button
            onClick={() => void loadContracts()}
            className="flex items-center gap-2 px-4 py-2.5 border border-gray-200 text-gray-700 rounded-xl hover:bg-gray-50 transition-colors"
            style={{ fontSize: "14px", fontWeight: 600 }}
          >
            <RefreshCw className="w-4 h-4" />
            Tải lại
          </button>
        </div>
      </div>

      <div className="mb-4 rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-amber-800">
        <div className="flex items-start gap-3">
          <TriangleAlert className="w-5 h-5 mt-0.5 shrink-0" />
          <p style={{ fontSize: "13px" }}>
            Backend đang khai báo endpoint hợp đồng nhưng [ContractController](/Users/trinhngocanh/Downloads/QLT/DA1_Boarding_house_management_system/Backend_Boarding_house_management_system/Backend_Boarding_house_management_system/Controllers/ContractController.cs#L13) hiện vẫn `throw NotImplementedException()`. Màn này đã bỏ mock và sẽ phản ánh lỗi thật.
          </p>
        </div>
      </div>

      <div className="grid grid-cols-3 gap-4 mb-6">
        <StatCard tone="green" label="Đang hiệu lực" value={String(activeCount)} />
        <StatCard tone="gray" label="Hết hạn" value={String(expiredCount)} />
        <StatCard tone="red" label="Đã chấm dứt" value={String(terminatedCount)} />
      </div>

      <div className="flex gap-2 mb-4">
        {(["all", "active", "expired", "terminated"] as StatusKey[]).map((status) => (
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
            {status === "all" ? "Tất cả" : status}
          </button>
        ))}
      </div>

      <div className="space-y-4">
        {error ? (
          <div className="rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-red-600">{error}</div>
        ) : loading ? (
          Array.from({ length: 4 }).map((_, index) => (
            <div key={index} className="h-40 rounded-2xl bg-gray-100 animate-pulse" />
          ))
        ) : filtered.length === 0 ? (
          <div className="rounded-2xl border border-gray-100 bg-white px-6 py-10 text-center text-gray-400">
            <FileText className="w-8 h-8 mx-auto mb-2 opacity-40" />
            <p style={{ fontSize: "14px" }}>Không có hợp đồng để hiển thị</p>
          </div>
        ) : (
          filtered.map((contract) => (
            <div key={contract.id} className="bg-white rounded-2xl border border-gray-100 overflow-hidden">
              <div className="p-5">
                <div className="flex items-start justify-between gap-3 mb-4">
                  <div className="flex items-start gap-3">
                    <div className="w-11 h-11 rounded-xl bg-gray-100 flex items-center justify-center shrink-0">
                      <FileText className="w-5 h-5 text-gray-500" />
                    </div>
                    <div>
                      <p className="text-gray-900" style={{ fontSize: "15px", fontWeight: 700 }}>
                        Hợp đồng {contract.id}
                      </p>
                      <p className="text-gray-500 mt-0.5" style={{ fontSize: "13px" }}>
                        {roomNames[contract.roomId] || `RoomId: ${contract.roomId}`}
                      </p>
                      <p className="text-gray-400 mt-1.5" style={{ fontSize: "12px" }}>
                        TenantId: {contract.tenantId}
                      </p>
                    </div>
                  </div>
                  <StatusPill status={contract.status} />
                </div>

                <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 mb-4">
                  <InfoBox label="Tiền cọc" value={formatCurrency(contract.deposit)} />
                  <InfoBox label="Ngày bắt đầu" value={new Date(contract.startDate).toLocaleDateString("vi-VN")} />
                  <InfoBox label="Ngày kết thúc" value={new Date(contract.endDate).toLocaleDateString("vi-VN")} />
                  <InfoBox label="Hoàn cọc" value={formatCurrency(contract.refundAmount)} />
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 text-gray-600" style={{ fontSize: "13px" }}>
                  <div className="bg-gray-50 rounded-xl px-4 py-3">
                    <p className="text-gray-400 mb-1" style={{ fontSize: "11px" }}>Điều khoản</p>
                    <p>{contract.terms || "Backend chưa trả terms."}</p>
                  </div>
                  <div className="bg-gray-50 rounded-xl px-4 py-3">
                    <p className="text-gray-400 mb-1" style={{ fontSize: "11px" }}>Khấu trừ / bàn giao</p>
                    <p>
                      Khấu trừ: {formatCurrency(contract.deductionAmount)}
                      {contract.deductionReason ? ` · ${contract.deductionReason}` : ""}
                    </p>
                    <p className="mt-1">{contract.handoverNote || "Chưa có ghi chú bàn giao."}</p>
                  </div>
                </div>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
}

function StatCard({ tone, label, value }: { tone: "green" | "gray" | "red"; label: string; value: string }) {
  const palette = {
    green: "bg-green-50 border-green-100 text-green-700 text-green-600",
    gray: "bg-gray-50 border-gray-200 text-gray-700 text-gray-500",
    red: "bg-red-50 border-red-100 text-red-700 text-red-500",
  }[tone].split(" ");

  return (
    <div className={`${palette[0]} border ${palette[1]} rounded-2xl p-4 text-center`}>
      <p className={palette[2]} style={{ fontSize: "24px", fontWeight: 700 }}>{value}</p>
      <p className={palette[3]} style={{ fontSize: "12px" }}>{label}</p>
    </div>
  );
}

function InfoBox({ label, value }: { label: string; value: string }) {
  return (
    <div className="bg-gray-50 rounded-xl px-3 py-2">
      <p className="text-gray-400" style={{ fontSize: "11px" }}>{label}</p>
      <p className="text-gray-800" style={{ fontSize: "13px", fontWeight: 600 }}>{value}</p>
    </div>
  );
}

function StatusPill({ status }: { status: string }) {
  const normalized = status.toLowerCase();

  if (normalized === "active") {
    return (
      <span className="flex items-center gap-1.5 px-3 py-1 rounded-xl shrink-0 text-green-600 bg-green-100" style={{ fontSize: "12px", fontWeight: 600 }}>
        <CheckCircle2 className="w-3.5 h-3.5" />
        Đang hiệu lực
      </span>
    );
  }

  if (normalized === "expired") {
    return (
      <span className="flex items-center gap-1.5 px-3 py-1 rounded-xl shrink-0 text-gray-500 bg-gray-100" style={{ fontSize: "12px", fontWeight: 600 }}>
        <Clock className="w-3.5 h-3.5" />
        Hết hạn
      </span>
    );
  }

  return (
    <span className="flex items-center gap-1.5 px-3 py-1 rounded-xl shrink-0 text-red-500 bg-red-100" style={{ fontSize: "12px", fontWeight: 600 }}>
      <XCircle className="w-3.5 h-3.5" />
      {status}
    </span>
  );
}
