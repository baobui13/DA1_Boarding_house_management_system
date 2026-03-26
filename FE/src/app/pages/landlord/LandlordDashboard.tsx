import { useEffect, useMemo, useState } from "react";
import { Building2, DoorOpen, TrendingUp, AlertTriangle, Calendar, FileText, TriangleAlert } from "lucide-react";
import { useNavigate } from "react-router";
import { useApp } from "../../context/AppContext";
import { getPropertyListings } from "../../lib/properties";
import { getInvoices } from "../../lib/invoices";
import { getContracts } from "../../lib/contracts";
import type { PropertyListing } from "../../lib/types";
import type { InvoiceResponse } from "../../lib/invoices";
import type { ContractResponse } from "../../lib/contracts";
import { formatCurrency } from "../../lib/format";

export default function LandlordDashboard() {
  const navigate = useNavigate();
  const { currentUser, token } = useApp();
  const [properties, setProperties] = useState<PropertyListing[]>([]);
  const [invoices, setInvoices] = useState<InvoiceResponse[]>([]);
  const [contracts, setContracts] = useState<ContractResponse[]>([]);
  const [propertyError, setPropertyError] = useState("");
  const [invoiceError, setInvoiceError] = useState("");
  const [contractError, setContractError] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;

    (async () => {
      setLoading(true);

      const propertyTask = getPropertyListings(currentUser ? { landlordId: currentUser.id } : {})
        .then((response) => {
          if (!cancelled) setProperties(response.items);
        })
        .catch((err) => {
          if (!cancelled) setPropertyError(err instanceof Error ? err.message : "Không tải được tài sản.");
        });

      const invoiceTask = token
        ? getInvoices(token)
            .then((response) => {
              if (!cancelled) setInvoices(response.items);
            })
            .catch((err) => {
              if (!cancelled) setInvoiceError(err instanceof Error ? err.message : "Không tải được hóa đơn.");
            })
        : Promise.resolve();

      const contractTask = token
        ? getContracts(token)
            .then((response) => {
              if (!cancelled) setContracts(response.items);
            })
            .catch((err) => {
              if (!cancelled) setContractError(err instanceof Error ? err.message : "Không tải được hợp đồng.");
            })
        : Promise.resolve();

      await Promise.all([propertyTask, invoiceTask, contractTask]);

      if (!cancelled) setLoading(false);
    })();

    return () => {
      cancelled = true;
    };
  }, [currentUser?.id, token]);

  const totalRooms = properties.length;
  const availableRooms = properties.filter((item) => item.status.toLowerCase() === "available").length;
  const rentedRooms = properties.filter((item) => item.status.toLowerCase() === "rented").length;
  const currentMonthRevenue = useMemo(() => {
    const now = new Date();
    return invoices
      .filter((invoice) => {
        const period = new Date(invoice.period);
        return period.getMonth() === now.getMonth() && period.getFullYear() === now.getFullYear();
      })
      .reduce((sum, invoice) => sum + invoice.total, 0);
  }, [invoices]);

  const unpaidInvoices = invoices.filter((invoice) => invoice.status.toLowerCase() !== "paid");
  const activeContracts = contracts.filter((contract) => contract.status.toLowerCase() === "active");

  return (
    <div className="max-w-7xl mx-auto px-4 py-6">
      <div className="mb-6">
        <h1 className="text-gray-900" style={{ fontSize: "22px", fontWeight: 700 }}>
          Tổng Quan Kinh Doanh
        </h1>
        <p className="text-gray-500 mt-0.5" style={{ fontSize: "14px" }}>
          Dashboard đang tổng hợp từ API thật; các phần backend chưa implement sẽ hiện cảnh báo thay vì mock.
        </p>
      </div>

      {(invoiceError || contractError) && (
        <div className="mb-6 rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-amber-800">
          <div className="flex items-start gap-3">
            <TriangleAlert className="w-5 h-5 mt-0.5 shrink-0" />
            <div style={{ fontSize: "13px" }}>
              {invoiceError && <p>Hóa đơn: {invoiceError}</p>}
              {contractError && <p>Hợp đồng: {contractError}</p>}
            </div>
          </div>
        </div>
      )}

      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        <KpiCard icon={Building2} tone="blue" label="Tổng tài sản" value={String(totalRooms)} hint={propertyError ? "Lỗi tải dữ liệu" : "Từ Property API"} />
        <KpiCard icon={DoorOpen} tone="green" label="Đang available" value={String(availableRooms)} hint="Theo status property" />
        <KpiCard icon={FileText} tone="purple" label="HĐ hiệu lực" value={contractError ? "--" : String(activeContracts.length)} hint={contractError ? "Contract API lỗi" : "Từ Contract API"} />
        <KpiCard icon={TrendingUp} tone="orange" label="Doanh thu tháng" value={invoiceError ? "--" : formatCurrency(currentMonthRevenue)} hint={invoiceError ? "Invoice API lỗi" : "Từ Invoice API"} />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 bg-white rounded-2xl border border-gray-100 p-5">
          <div className="flex items-center justify-between mb-4">
            <div>
              <h2 className="text-gray-900" style={{ fontSize: "16px", fontWeight: 700 }}>
                Tài sản gần đây
              </h2>
              <p className="text-gray-400" style={{ fontSize: "13px" }}>
                Danh sách lấy từ Property API
              </p>
            </div>
            <button
              onClick={() => navigate("/landlord/properties")}
              className="text-orange-600 hover:text-orange-700"
              style={{ fontSize: "13px", fontWeight: 600 }}
            >
              Quản lý →
            </button>
          </div>

          {propertyError ? (
            <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-red-600">{propertyError}</div>
          ) : loading ? (
            <div className="space-y-3">
              {Array.from({ length: 5 }).map((_, index) => (
                <div key={index} className="h-14 rounded-xl bg-gray-100 animate-pulse" />
              ))}
            </div>
          ) : properties.length === 0 ? (
            <div className="text-center py-12 text-gray-400">
              <Building2 className="w-8 h-8 mx-auto mb-2 opacity-40" />
              <p style={{ fontSize: "14px" }}>Chưa có tài sản</p>
            </div>
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
              {properties.slice(0, 6).map((property) => (
                <div key={property.id} className="flex items-center gap-3 bg-gray-50 rounded-xl p-3">
                  <div
                    className={`w-2.5 h-2.5 rounded-full shrink-0 ${
                      property.status.toLowerCase() === "available"
                        ? "bg-green-400"
                        : property.status.toLowerCase() === "rented"
                        ? "bg-blue-400"
                        : "bg-yellow-400"
                    }`}
                  />
                  <div className="flex-1 min-w-0">
                    <p className="truncate text-gray-700" style={{ fontSize: "13px", fontWeight: 500 }}>
                      {property.propertyName}
                    </p>
                    <p className="text-gray-400" style={{ fontSize: "11px" }}>
                      {formatCurrency(property.price)}/tháng
                    </p>
                  </div>
                  <span className="shrink-0 px-2 py-0.5 rounded-md bg-white text-gray-600 capitalize" style={{ fontSize: "10px", fontWeight: 600 }}>
                    {property.status}
                  </span>
                </div>
              ))}
            </div>
          )}
        </div>

        <div className="space-y-4">
          <InfoPanel
            icon={AlertTriangle}
            tone="red"
            title="Hóa đơn chưa thanh toán"
            count={invoiceError ? "--" : String(unpaidInvoices.length)}
            body={
              invoiceError
                ? invoiceError
                : unpaidInvoices.length === 0
                ? "Không có hóa đơn chưa thanh toán."
                : unpaidInvoices.slice(0, 3).map((invoice) => `${invoice.contractId} · ${formatCurrency(invoice.total)}`).join("\n")
            }
            actionLabel="Quản lý hóa đơn"
            onClick={() => navigate("/landlord/billing")}
          />

          <InfoPanel
            icon={Calendar}
            tone="blue"
            title="Hợp đồng active"
            count={contractError ? "--" : String(activeContracts.length)}
            body={
              contractError
                ? contractError
                : activeContracts.length === 0
                ? "Không có hợp đồng hiệu lực."
                : activeContracts
                    .slice(0, 3)
                    .map((contract) => `${roomNamesOrId(properties, contract.roomId)} · ${new Date(contract.endDate).toLocaleDateString("vi-VN")}`)
                    .join("\n")
            }
            actionLabel="Quản lý hợp đồng"
            onClick={() => navigate("/landlord/contracts")}
          />
        </div>
      </div>
    </div>
  );
}

function roomNamesOrId(properties: PropertyListing[], roomId: string) {
  return properties.find((item) => item.id === roomId)?.propertyName || roomId;
}

function KpiCard({
  icon: Icon,
  tone,
  label,
  value,
  hint,
}: {
  icon: React.ElementType;
  tone: "blue" | "green" | "purple" | "orange";
  label: string;
  value: string;
  hint: string;
}) {
  const styles = {
    blue: "bg-blue-100 text-blue-600",
    green: "bg-green-100 text-green-600",
    purple: "bg-purple-100 text-purple-600",
    orange: "bg-orange-100 text-orange-600",
  }[tone];

  return (
    <div className="bg-white rounded-2xl border border-gray-100 p-5">
      <div className="w-10 h-10 rounded-xl flex items-center justify-center mb-3">
        <div className={`w-10 h-10 rounded-xl flex items-center justify-center ${styles}`}>
          <Icon className="w-5 h-5" />
        </div>
      </div>
      <p className="text-gray-900" style={{ fontSize: "22px", fontWeight: 700 }}>{value}</p>
      <p className="text-gray-500" style={{ fontSize: "13px" }}>{label}</p>
      <p className="text-gray-400 mt-1" style={{ fontSize: "11px" }}>{hint}</p>
    </div>
  );
}

function InfoPanel({
  icon: Icon,
  tone,
  title,
  count,
  body,
  actionLabel,
  onClick,
}: {
  icon: React.ElementType;
  tone: "red" | "blue";
  title: string;
  count: string;
  body: string;
  actionLabel: string;
  onClick: () => void;
}) {
  const badge = tone === "red" ? "bg-red-100 text-red-600" : "bg-blue-100 text-blue-600";
  const icon = tone === "red" ? "text-red-500" : "text-blue-500";

  return (
    <div className="bg-white rounded-2xl border border-gray-100 p-5">
      <div className="flex items-center gap-2 mb-4">
        <Icon className={`w-5 h-5 ${icon}`} />
        <h3 className="text-gray-900" style={{ fontSize: "15px", fontWeight: 600 }}>
          {title}
        </h3>
        <span className={`ml-auto px-2 py-0.5 rounded-lg ${badge}`} style={{ fontSize: "11px", fontWeight: 600 }}>
          {count}
        </span>
      </div>
      <div className="whitespace-pre-line text-gray-500 min-h-[68px]" style={{ fontSize: "13px", lineHeight: 1.6 }}>
        {body}
      </div>
      <button
        onClick={onClick}
        className="w-full mt-4 py-2 text-orange-600 hover:bg-orange-50 rounded-xl transition-colors"
        style={{ fontSize: "13px", fontWeight: 500 }}
      >
        {actionLabel} →
      </button>
    </div>
  );
}
