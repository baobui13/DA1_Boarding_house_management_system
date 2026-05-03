import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router";
import { Calendar, FileText, Receipt, Home, TriangleAlert, RefreshCw } from "lucide-react";
import { useApp } from "../../context/AppContext";
import { getInvoices, type InvoiceResponse } from "../../lib/invoices";
import { getContracts, type ContractResponse } from "../../lib/contracts";
import { formatCurrency } from "../../lib/format";

const tabs = [
  { id: "overview", label: "Tổng Quan", icon: Home },
  { id: "viewing", label: "Lịch Xem Phòng", icon: Calendar },
  { id: "invoices", label: "Hóa Đơn", icon: Receipt },
  { id: "contracts", label: "Hợp Đồng", icon: FileText },
];

export default function TenantDashboard() {
  const navigate = useNavigate();
  const { token } = useApp();
  const [activeTab, setActiveTab] = useState("overview");
  const [invoices, setInvoices] = useState<InvoiceResponse[]>([]);
  const [contracts, setContracts] = useState<ContractResponse[]>([]);
  const [invoiceError, setInvoiceError] = useState("");
  const [contractError, setContractError] = useState("");
  const [loading, setLoading] = useState(true);

  const loadData = async () => {
    if (!token) {
      setInvoiceError("Thiếu token đăng nhập.");
      setContractError("Thiếu token đăng nhập.");
      setLoading(false);
      return;
    }

    setLoading(true);
    setInvoiceError("");
    setContractError("");

    await Promise.all([
      getInvoices(token)
        .then((response) => setInvoices(response.items))
        .catch((err) => setInvoiceError(err instanceof Error ? err.message : "Không tải được hóa đơn.")),
      getContracts(token)
        .then((response) => setContracts(response.items))
        .catch((err) => setContractError(err instanceof Error ? err.message : "Không tải được hợp đồng.")),
    ]);

    setLoading(false);
  };

  useEffect(() => {
    void loadData();
  }, [token]);

  const currentContract = useMemo(
    () => contracts.find((item) => item.status.toLowerCase() === "active"),
    [contracts],
  );
  const unpaidInvoices = invoices.filter((item) => item.status.toLowerCase() !== "paid");

  return (
    <div className="max-w-5xl mx-auto px-4 py-6">
      <div className="mb-6 flex items-start justify-between gap-4">
        <div>
          <h1 className="text-gray-900" style={{ fontSize: "22px", fontWeight: 700 }}>
            Quản Lý Cá Nhân
          </h1>
          <p className="text-gray-500 mt-0.5" style={{ fontSize: "14px" }}>
            Dashboard tenant đã bỏ mock và đang bám dữ liệu backend thật.
          </p>
        </div>
        <button
          onClick={() => void loadData()}
          className="flex items-center gap-2 px-4 py-2.5 border border-gray-200 text-gray-700 rounded-xl hover:bg-gray-50 transition-colors"
          style={{ fontSize: "14px", fontWeight: 600 }}
        >
          <RefreshCw className="w-4 h-4" />
          Tải lại
        </button>
      </div>

      <div className="mb-6 rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-amber-800">
        <div className="flex items-start gap-3">
          <TriangleAlert className="w-5 h-5 mt-0.5 shrink-0" />
          <div style={{ fontSize: "13px" }}>
            <p>`Appointment`, `Invoice`, `Contract` cho tenant hiện đều đang bị chặn bởi backend chưa implement đầy đủ.</p>
            {invoiceError && <p>Invoice API: {invoiceError}</p>}
            {contractError && <p>Contract API: {contractError}</p>}
          </div>
        </div>
      </div>

      <div className="flex gap-1 bg-gray-100 p-1 rounded-xl mb-6 overflow-x-auto">
        {tabs.map((tab) => {
          const Icon = tab.icon;
          return (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={`flex items-center gap-2 px-4 py-2 rounded-lg whitespace-nowrap transition-all ${
                activeTab === tab.id ? "bg-white text-gray-900 shadow-sm" : "text-gray-500 hover:text-gray-700"
              }`}
              style={{ fontSize: "13px", fontWeight: activeTab === tab.id ? 600 : 400 }}
            >
              <Icon className="w-4 h-4" />
              {tab.label}
            </button>
          );
        })}
      </div>

      {activeTab === "overview" && (
        <div className="space-y-6">
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <SummaryCard label="Lịch xem" value="--" hint="Appointment API chưa chạy" />
            <SummaryCard label="Hóa đơn chưa đóng" value={loading ? "..." : String(unpaidInvoices.length)} hint={loading ? "Đang tải" : formatCurrency(unpaidInvoices.reduce((sum, item) => sum + item.total, 0))} />
            <SummaryCard label="Hợp đồng" value={loading ? "..." : currentContract ? "Đang thuê" : "Không có"} hint={currentContract ? new Date(currentContract.endDate).toLocaleDateString("vi-VN") : "Chưa có dữ liệu"} />
          </div>
        </div>
      )}

      {activeTab === "viewing" && <BlockPanel title="Lịch Xem Phòng" body="AppointmentController chưa implement, nên chưa thể hiển thị lịch xem thật." />}
      {activeTab === "invoices" && (
        <DataPanel
          title="Hóa Đơn"
          loading={loading}
          error={invoiceError}
          emptyText="Không có hóa đơn"
          items={invoices.map((invoice) => (
            <div key={invoice.id} className="flex items-center justify-between py-3 border-b border-gray-50 last:border-0">
              <div>
                <p className="text-gray-800" style={{ fontSize: "13px", fontWeight: 600 }}>
                  {new Date(invoice.period).toLocaleDateString("vi-VN", { month: "2-digit", year: "numeric" })}
                </p>
                <p className="text-gray-400" style={{ fontSize: "12px" }}>ContractId: {invoice.contractId}</p>
              </div>
              <button
                onClick={() => navigate(`/tenant/invoices/${invoice.id}`)}
                className="text-orange-600 hover:text-orange-700"
                style={{ fontSize: "13px", fontWeight: 600 }}
              >
                {formatCurrency(invoice.total)}
              </button>
            </div>
          ))}
        />
      )}
      {activeTab === "contracts" && (
        <DataPanel
          title="Hợp Đồng"
          loading={loading}
          error={contractError}
          emptyText="Không có hợp đồng"
          items={contracts.map((contract) => (
            <div key={contract.id} className="flex items-center justify-between py-3 border-b border-gray-50 last:border-0">
              <div>
                <p className="text-gray-800" style={{ fontSize: "13px", fontWeight: 600 }}>
                  {contract.id}
                </p>
                <p className="text-gray-400" style={{ fontSize: "12px" }}>RoomId: {contract.roomId}</p>
              </div>
              <button
                onClick={() => navigate(`/tenant/contracts/${contract.id}`)}
                className="text-orange-600 hover:text-orange-700"
                style={{ fontSize: "13px", fontWeight: 600 }}
              >
                {contract.status}
              </button>
            </div>
          ))}
        />
      )}
    </div>
  );
}

function SummaryCard({ label, value, hint }: { label: string; value: string; hint: string }) {
  return (
    <div className="bg-white rounded-2xl border border-gray-100 p-5">
      <p className="text-gray-500" style={{ fontSize: "12px" }}>{label}</p>
      <p className="text-gray-900 mt-2" style={{ fontSize: "20px", fontWeight: 700 }}>{value}</p>
      <p className="text-gray-400 mt-1" style={{ fontSize: "12px" }}>{hint}</p>
    </div>
  );
}

function BlockPanel({ title, body }: { title: string; body: string }) {
  return (
    <div className="bg-white rounded-2xl border border-gray-100 p-5">
      <h2 className="text-gray-900 mb-2" style={{ fontSize: "17px", fontWeight: 700 }}>{title}</h2>
      <p className="text-gray-500" style={{ fontSize: "14px" }}>{body}</p>
    </div>
  );
}

function DataPanel({
  title,
  loading,
  error,
  emptyText,
  items,
}: {
  title: string;
  loading: boolean;
  error: string;
  emptyText: string;
  items: React.ReactNode[];
}) {
  return (
    <div className="bg-white rounded-2xl border border-gray-100 p-5">
      <h2 className="text-gray-900 mb-4" style={{ fontSize: "17px", fontWeight: 700 }}>{title}</h2>
      {error ? <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-red-600">{error}</div> : null}
      {!error && loading ? <div className="h-20 rounded-xl bg-gray-100 animate-pulse" /> : null}
      {!error && !loading && items.length === 0 ? <p className="text-gray-400" style={{ fontSize: "14px" }}>{emptyText}</p> : null}
      {!error && !loading && items.length > 0 ? <div>{items}</div> : null}
    </div>
  );
}
