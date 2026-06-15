import { useEffect, useMemo, useState } from "react";
import {
  CalendarDays,
  Clock3,
  CheckCircle2,
  XCircle,
  Search,
  Trash2,
  Calendar,
  Phone,
  Mail,
  Ban,
  UserRound,
  Building2,
  RefreshCw,
} from "lucide-react";
import { useApp } from "../../context/AppContext";
import { getPropertyListings } from "../../lib/properties";
import { getUsers } from "../../lib/users";
import { getAppointments, updateAppointment, deleteAppointment } from "../../lib/appointments";
import type { PropertyListing, UserResponse } from "../../lib/types";
import type { AppointmentResponse } from "../../lib/appointments";

type ActiveTab = "all" | "pending" | "confirmed" | "rejected" | "cancelled";

export default function AppointmentManagement() {
  const { currentUser, token } = useApp();
  const [properties, setProperties] = useState<PropertyListing[]>([]);
  const [users, setUsers] = useState<UserResponse[]>([]);
  const [appointments, setAppointments] = useState<AppointmentResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  // Filter/Search states
  const [activeTab, setActiveTab] = useState<ActiveTab>("all");
  const [searchQuery, setSearchQuery] = useState("");

  // Modal states
  const [rescheduleAppointment, setRescheduleAppointment] = useState<AppointmentResponse | null>(null);
  const [rescheduleDate, setRescheduleDate] = useState("");
  const [rescheduleNote, setRescheduleNote] = useState("");
  const [savingReschedule, setSavingReschedule] = useState(false);

  const [rejectAppointment, setRejectAppointment] = useState<AppointmentResponse | null>(null);
  const [rejectReason, setRejectReason] = useState("");
  const [savingReject, setSavingReject] = useState(false);

  const loadData = async () => {
    if (!currentUser || !token) return;
    setLoading(true);
    setError("");

    try {
      const [propertyResponse, userResponse, appointmentResponse] = await Promise.all([
        getPropertyListings({ landlordId: currentUser.id, pageSize: 1000 }, token),
        getUsers({ page: 1, pageSize: 1000 }, token),
        getAppointments(token, { pageSize: 1000 }),
      ]);

      setProperties(propertyResponse.items);
      setUsers(userResponse.items);
      setAppointments(appointmentResponse.items);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Không tải được dữ liệu lịch hẹn.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadData();
  }, [currentUser?.id, token]);

  const propertiesById = useMemo(
    () => Object.fromEntries(properties.map((p) => [p.id, p])),
    [properties],
  );

  const usersById = useMemo(
    () => Object.fromEntries(users.map((u) => [u.id, u])),
    [users],
  );

  const landlordPropertyIds = useMemo(
    () => new Set(properties.map((p) => p.id)),
    [properties],
  );

  // Filter only appointments belonging to the landlord's properties
  const landlordAppointments = useMemo(() => {
    return appointments.filter((a) => landlordPropertyIds.has(a.propertyId));
  }, [appointments, landlordPropertyIds]);

  // Statistics
  const stats = useMemo(() => {
    return {
      total: landlordAppointments.length,
      pending: landlordAppointments.filter((a) => a.status === "Pending").length,
      confirmed: landlordAppointments.filter((a) => a.status === "Confirmed").length,
      inactive: landlordAppointments.filter((a) => a.status === "Cancelled" || a.status === "Rejected").length,
    };
  }, [landlordAppointments]);

  // Handle Tab and Search filtering
  const filteredAppointments = useMemo(() => {
    return landlordAppointments
      .filter((a) => {
        // Tab filtering
        if (activeTab === "pending") return a.status === "Pending";
        if (activeTab === "confirmed") return a.status === "Confirmed";
        if (activeTab === "rejected") return a.status === "Rejected";
        if (activeTab === "cancelled") return a.status === "Cancelled";
        return true;
      })
      .filter((a) => {
        // Search filtering
        if (!searchQuery) return true;
        const query = searchQuery.toLowerCase();
        const tenant = usersById[a.userId];
        const property = propertiesById[a.propertyId];

        const tenantName = tenant?.fullName?.toLowerCase() || "";
        const propertyName = property?.propertyName?.toLowerCase() || "";
        const tenantPhone = tenant?.phoneNumber || "";

        return tenantName.includes(query) || propertyName.includes(query) || tenantPhone.includes(query);
      })
      .sort((a, b) => new Date(b.appointmentDateTime).getTime() - new Date(a.appointmentDateTime).getTime());
  }, [landlordAppointments, activeTab, searchQuery, usersById, propertiesById]);

  // Status handlers
  const handleConfirm = async (id: string) => {
    if (!token) return;
    if (!confirm("Xác nhận cuộc hẹn xem phòng này?")) return;

    try {
      await updateAppointment(token, { id, status: "Confirmed" });
      await loadData();
    } catch (err) {
      alert(err instanceof Error ? err.message : "Xác nhận lịch hẹn thất bại.");
    }
  };

  const handleCancel = async (id: string) => {
    if (!token) return;
    if (!confirm("Bạn có chắc chắn muốn hủy lịch hẹn này?")) return;

    try {
      await updateAppointment(token, { id, status: "Cancelled", note: "Chủ trọ chủ động hủy lịch." });
      await loadData();
    } catch (err) {
      alert(err instanceof Error ? err.message : "Hủy lịch hẹn thất bại.");
    }
  };

  const handleDelete = async (id: string) => {
    if (!token) return;
    if (!confirm("Bạn có chắc chắn muốn xóa lịch hẹn này? Việc xóa sẽ mất vĩnh viễn log.")) return;

    try {
      await deleteAppointment(token, id);
      await loadData();
    } catch (err) {
      alert(err instanceof Error ? err.message : "Xóa lịch hẹn thất bại.");
    }
  };

  const openRescheduleModal = (appointment: AppointmentResponse) => {
    setRescheduleAppointment(appointment);
    setRescheduleDate(toLocalDateTimeString(appointment.appointmentDateTime));
    setRescheduleNote(appointment.note || "");
  };

  const closeRescheduleModal = () => {
    setRescheduleAppointment(null);
    setRescheduleDate("");
    setRescheduleNote("");
  };

  const handleRescheduleSubmit = async () => {
    if (!token || !rescheduleAppointment) return;
    if (!rescheduleDate) {
      alert("Vui lòng chọn ngày giờ mới.");
      return;
    }

    setSavingReschedule(true);
    try {
      const isoDate = new Date(rescheduleDate).toISOString();
      await updateAppointment(token, {
        id: rescheduleAppointment.id,
        appointmentDateTime: isoDate,
        note: rescheduleNote || null,
        status: "Pending", // Reset back to pending so tenant can re-review or acknowledge
      });
      closeRescheduleModal();
      await loadData();
    } catch (err) {
      alert(err instanceof Error ? err.message : "Đổi lịch hẹn thất bại.");
    } finally {
      setSavingReschedule(false);
    }
  };

  const openRejectModal = (appointment: AppointmentResponse) => {
    setRejectAppointment(appointment);
    setRejectReason("");
  };

  const closeRejectModal = () => {
    setRejectAppointment(null);
    setRejectReason("");
  };

  const handleRejectSubmit = async () => {
    if (!token || !rejectAppointment) return;

    setSavingReject(true);
    try {
      await updateAppointment(token, {
        id: rejectAppointment.id,
        status: "Rejected",
        note: rejectReason ? `Từ chối: ${rejectReason}` : "Chủ trọ từ chối lịch hẹn xem phòng.",
      });
      closeRejectModal();
      await loadData();
    } catch (err) {
      alert(err instanceof Error ? err.message : "Từ chối lịch hẹn thất bại.");
    } finally {
      setSavingReject(false);
    }
  };

  return (
    <div className="max-w-7xl mx-auto px-4 py-6 space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-gray-900" style={{ fontSize: "24px", fontWeight: 700 }}>
            Quản Lý Lịch Hẹn
          </h1>
          <p className="text-gray-500 mt-1" style={{ fontSize: "14px" }}>
            Theo dõi và xét duyệt lịch xem phòng từ khách hàng tiềm năng.
          </p>
        </div>
        <button
          onClick={() => void loadData()}
          disabled={loading}
          className="inline-flex items-center justify-center gap-2 rounded-2xl border border-gray-200 bg-white px-4 py-2.5 text-gray-700 hover:bg-gray-50 disabled:opacity-50 transition-colors shadow-sm"
          style={{ fontSize: "14px", fontWeight: 600 }}
        >
          <RefreshCw className={`w-4 h-4 ${loading ? "animate-spin" : ""}`} />
          Tải lại
        </button>
      </div>

      {error && (
        <div className="rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-red-600">
          {error}
        </div>
      )}

      {/* Summary Stats Grid */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <SummaryCard label="Tổng lịch hẹn" value={loading ? "..." : String(stats.total)} tone="blue" />
        <SummaryCard label="Đang chờ duyệt" value={loading ? "..." : String(stats.pending)} tone="amber" />
        <SummaryCard label="Đã xác nhận" value={loading ? "..." : String(stats.confirmed)} tone="green" />
        <SummaryCard label="Đã hủy / Từ chối" value={loading ? "..." : String(stats.inactive)} tone="gray" />
      </div>

      {/* Filters and Search */}
      <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between bg-white rounded-3xl border border-gray-100 p-4 shadow-sm">
        {/* Tabs */}
        <div className="flex flex-wrap gap-1">
          <TabButton active={activeTab === "all"} onClick={() => setActiveTab("all")} label="Tất cả" />
          <TabButton active={activeTab === "pending"} onClick={() => setActiveTab("pending")} label="Chờ duyệt" count={stats.pending} badgeTone="amber" />
          <TabButton active={activeTab === "confirmed"} onClick={() => setActiveTab("confirmed")} label="Đã xác nhận" count={stats.confirmed} badgeTone="green" />
          <TabButton active={activeTab === "rejected"} onClick={() => setActiveTab("rejected")} label="Bị từ chối" />
          <TabButton active={activeTab === "cancelled"} onClick={() => setActiveTab("cancelled")} label="Đã hủy" />
        </div>

        {/* Search */}
        <div className="relative w-full md:max-w-xs">
          <Search className="absolute left-3.5 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            placeholder="Tìm tên khách, tên phòng..."
            className="w-full pl-10 pr-4 py-2 border border-gray-200 rounded-2xl focus:border-orange-500 focus:outline-none placeholder-gray-400"
            style={{ fontSize: "14px" }}
          />
        </div>
      </div>

      {/* Appointment Listings */}
      {loading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {Array.from({ length: 4 }).map((_, index) => (
            <div key={index} className="h-56 rounded-3xl bg-gray-100 animate-pulse" />
          ))}
        </div>
      ) : filteredAppointments.length === 0 ? (
        <div className="rounded-3xl border border-dashed border-gray-200 bg-white px-6 py-16 text-center text-gray-400">
          <CalendarDays className="w-12 h-12 mx-auto mb-3 opacity-30" />
          <p className="text-gray-700" style={{ fontSize: "16px", fontWeight: 600 }}>
            Không tìm thấy lịch hẹn nào
          </p>
          <p className="text-gray-400 mt-1" style={{ fontSize: "14px" }}>
            Các cuộc hẹn xem phòng mới từ khách hàng sẽ xuất hiện tại đây.
          </p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {filteredAppointments.map((appointment) => {
            const tenant = usersById[appointment.userId];
            const property = propertiesById[appointment.propertyId];

            return (
              <div
                key={appointment.id}
                className="group relative flex flex-col justify-between overflow-hidden rounded-3xl border border-gray-100 bg-white p-5 shadow-sm hover:border-orange-200 transition-all duration-300"
              >
                {/* Delete Button */}
                <button
                  onClick={() => void handleDelete(appointment.id)}
                  className="absolute top-4 right-4 flex h-8 w-8 items-center justify-center rounded-xl bg-gray-50 text-gray-400 hover:bg-red-50 hover:text-red-500 opacity-0 group-hover:opacity-100 transition-all duration-300"
                >
                  <Trash2 className="w-4 h-4" />
                </button>

                {/* Appointment Content */}
                <div className="space-y-4">
                  {/* Property Details */}
                  <div className="flex items-start gap-3">
                    <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-2xl bg-orange-100 text-orange-500">
                      <Building2 className="w-5 h-5" />
                    </div>
                    <div className="min-w-0 pr-8">
                      <p className="truncate text-gray-900" style={{ fontSize: "16px", fontWeight: 700 }}>
                        {property?.propertyName || `Tài sản #${appointment.propertyId}`}
                      </p>
                      <p className="truncate text-gray-400 mt-0.5" style={{ fontSize: "12px" }}>
                        {property?.address || "Không rõ địa chỉ"}
                      </p>
                    </div>
                  </div>

                  {/* Tenant Details */}
                  <div className="border-t border-b border-gray-50 py-3 space-y-2">
                    <div className="flex items-center gap-2">
                      <UserRound className="w-4 h-4 text-gray-400 shrink-0" />
                      <span className="text-gray-700 truncate" style={{ fontSize: "13px", fontWeight: 600 }}>
                        {tenant?.fullName || `Khách hàng #${appointment.userId}`}
                      </span>
                    </div>

                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-2 text-gray-500" style={{ fontSize: "12px" }}>
                      <div className="flex items-center gap-1.5 min-w-0">
                        <Phone className="w-3.5 h-3.5 text-gray-400 shrink-0" />
                        <span className="truncate">{tenant?.phoneNumber || "Chưa cập nhật số"}</span>
                      </div>
                      <div className="flex items-center gap-1.5 min-w-0">
                        <Mail className="w-3.5 h-3.5 text-gray-400 shrink-0" />
                        <span className="truncate">{tenant?.email || "Chưa cập nhật mail"}</span>
                      </div>
                    </div>
                  </div>

                  {/* Date, Time and Status */}
                  <div className="flex flex-wrap items-center justify-between gap-3">
                    <div className="flex items-center gap-2 text-orange-600">
                      <Calendar className="w-4 h-4 shrink-0" />
                      <span style={{ fontSize: "13px", fontWeight: 700 }}>
                        {formatDateTime(appointment.appointmentDateTime)}
                      </span>
                    </div>
                    <StatusBadge status={appointment.status} />
                  </div>

                  {/* Note block if present */}
                  {appointment.note && (
                    <div className="rounded-2xl bg-gray-50 border border-gray-100 px-3.5 py-2.5 text-gray-600" style={{ fontSize: "12px", lineHeight: 1.5 }}>
                      <p className="font-semibold text-gray-800 mb-0.5">Ghi chú:</p>
                      {appointment.note}
                    </div>
                  )}
                </div>

                {/* Inline Actions based on status */}
                {appointment.status === "Pending" && (
                  <div className="mt-5 flex flex-wrap gap-2 pt-4 border-t border-gray-100">
                    <button
                      onClick={() => void handleConfirm(appointment.id)}
                      className="flex-1 min-w-[100px] inline-flex items-center justify-center gap-1.5 rounded-xl bg-green-500 hover:bg-green-600 text-white px-3 py-2 transition-colors"
                      style={{ fontSize: "13px", fontWeight: 600 }}
                    >
                      <CheckCircle2 className="w-4 h-4" />
                      Xác nhận
                    </button>
                    <button
                      onClick={() => openRescheduleModal(appointment)}
                      className="flex-1 min-w-[100px] inline-flex items-center justify-center gap-1.5 rounded-xl border border-gray-200 bg-white hover:bg-gray-50 text-gray-700 px-3 py-2 transition-colors"
                      style={{ fontSize: "13px", fontWeight: 600 }}
                    >
                      Đổi lịch
                    </button>
                    <button
                      onClick={() => openRejectModal(appointment)}
                      className="flex-1 min-w-[100px] inline-flex items-center justify-center gap-1.5 rounded-xl border border-red-200 bg-red-50 hover:bg-red-100 text-red-600 px-3 py-2 transition-colors"
                      style={{ fontSize: "13px", fontWeight: 600 }}
                    >
                      Từ chối
                    </button>
                  </div>
                )}

                {appointment.status === "Confirmed" && (
                  <div className="mt-5 flex flex-wrap gap-2 pt-4 border-t border-gray-100">
                    <button
                      onClick={() => openRescheduleModal(appointment)}
                      className="flex-1 inline-flex items-center justify-center gap-1.5 rounded-xl border border-gray-200 bg-white hover:bg-gray-50 text-gray-700 px-3 py-2 transition-colors"
                      style={{ fontSize: "13px", fontWeight: 600 }}
                    >
                      Đổi lịch hẹn
                    </button>
                    <button
                      onClick={() => void handleCancel(appointment.id)}
                      className="flex-1 inline-flex items-center justify-center gap-1.5 rounded-xl border border-red-200 bg-red-50 hover:bg-red-100 text-red-600 px-3 py-2 transition-colors"
                      style={{ fontSize: "13px", fontWeight: 600 }}
                    >
                      <Ban className="w-4 h-4" />
                      Hủy lịch hẹn
                    </button>
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}

      {/* Reschedule Modal Dialog */}
      {rescheduleAppointment && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm p-4 animate-fade-in">
          <div className="w-full max-w-md overflow-hidden rounded-3xl border border-gray-100 bg-white p-6 shadow-2xl space-y-4">
            <div>
              <h3 className="text-gray-900" style={{ fontSize: "18px", fontWeight: 700 }}>
                Đổi Lịch Hẹn Xem Phòng
              </h3>
              <p className="text-gray-400 mt-1" style={{ fontSize: "13px" }}>
                Chọn lại ngày giờ xem phòng mới và gửi tới khách hàng.
              </p>
            </div>

            <div className="space-y-3">
              <div>
                <label className="block text-gray-700 mb-1" style={{ fontSize: "13px", fontWeight: 600 }}>
                  Thời gian mới
                </label>
                <input
                  type="datetime-local"
                  value={rescheduleDate}
                  onChange={(e) => setRescheduleDate(e.target.value)}
                  className="w-full px-4 py-2.5 border border-gray-200 rounded-2xl focus:border-orange-500 focus:outline-none"
                  style={{ fontSize: "14px" }}
                />
              </div>

              <div>
                <label className="block text-gray-700 mb-1" style={{ fontSize: "13px", fontWeight: 600 }}>
                  Lý do đổi lịch / Ghi chú thêm
                </label>
                <textarea
                  value={rescheduleNote}
                  onChange={(e) => setRescheduleNote(e.target.value)}
                  placeholder="Ví dụ: Anh bận chút việc đột xuất vào giờ cũ, chuyển sang buổi chiều được không em?..."
                  rows={3}
                  className="w-full px-4 py-2.5 border border-gray-200 rounded-2xl focus:border-orange-500 focus:outline-none placeholder-gray-400"
                  style={{ fontSize: "14px" }}
                />
              </div>
            </div>

            <div className="flex items-center justify-end gap-2 pt-2">
              <button
                onClick={closeRescheduleModal}
                disabled={savingReschedule}
                className="rounded-xl border border-gray-200 bg-white hover:bg-gray-50 px-4 py-2 text-gray-700"
                style={{ fontSize: "13px", fontWeight: 600 }}
              >
                Hủy bỏ
              </button>
              <button
                onClick={() => void handleRescheduleSubmit()}
                disabled={savingReschedule}
                className="rounded-xl bg-orange-500 hover:bg-orange-600 disabled:opacity-50 text-white px-5 py-2 inline-flex items-center gap-1.5"
                style={{ fontSize: "13px", fontWeight: 600 }}
              >
                {savingReschedule ? "Đang lưu..." : "Đổi lịch"}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Reject Modal Dialog */}
      {rejectAppointment && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm p-4 animate-fade-in">
          <div className="w-full max-w-md overflow-hidden rounded-3xl border border-gray-100 bg-white p-6 shadow-2xl space-y-4">
            <div>
              <h3 className="text-gray-900" style={{ fontSize: "18px", fontWeight: 700 }}>
                Từ Chối Lịch Hẹn
              </h3>
              <p className="text-gray-400 mt-1" style={{ fontSize: "13px" }}>
                Nhập lý do từ chối lịch xem phòng trọ này.
              </p>
            </div>

            <div>
              <label className="block text-gray-700 mb-1" style={{ fontSize: "13px", fontWeight: 600 }}>
                Lý do từ chối (Không bắt buộc)
              </label>
              <textarea
                value={rejectReason}
                onChange={(e) => setRejectReason(e.target.value)}
                placeholder="Ví dụ: Phòng trọ hiện tại vừa được cọc thuê sáng nay..."
                rows={3}
                className="w-full px-4 py-2.5 border border-gray-200 rounded-2xl focus:border-orange-500 focus:outline-none placeholder-gray-400"
                style={{ fontSize: "14px" }}
              />
            </div>

            <div className="flex items-center justify-end gap-2 pt-2">
              <button
                onClick={closeRejectModal}
                disabled={savingReject}
                className="rounded-xl border border-gray-200 bg-white hover:bg-gray-50 px-4 py-2 text-gray-700"
                style={{ fontSize: "13px", fontWeight: 600 }}
              >
                Hủy bỏ
              </button>
              <button
                onClick={() => void handleRejectSubmit()}
                disabled={savingReject}
                className="rounded-xl bg-red-500 hover:bg-red-600 disabled:opacity-50 text-white px-5 py-2"
                style={{ fontSize: "13px", fontWeight: 600 }}
              >
                {savingReject ? "Đang xử lý..." : "Từ chối lịch"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

// Helpers
function SummaryCard({ label, value, tone }: { label: string; value: string; tone: "blue" | "amber" | "green" | "gray" }) {
  const palette = {
    blue: "bg-blue-50 border-blue-100 text-blue-600",
    amber: "bg-amber-50 border-amber-100 text-amber-600",
    green: "bg-green-50 border-green-100 text-green-600",
    gray: "bg-gray-50 border-gray-100 text-gray-600",
  }[tone];

  return (
    <div className={`rounded-3xl border px-5 py-4 text-center ${palette} shadow-sm`}>
      <p style={{ fontSize: "32px", fontWeight: 700, lineHeight: 1 }}>{value}</p>
      <p className="mt-2 text-gray-500" style={{ fontSize: "13px", fontWeight: 600 }}>
        {label}
      </p>
    </div>
  );
}

function TabButton({
  active,
  onClick,
  label,
  count,
  badgeTone = "gray",
}: {
  active: boolean;
  onClick: () => void;
  label: string;
  count?: number;
  badgeTone?: "gray" | "amber" | "green";
}) {
  const style = active
    ? "bg-orange-500 text-white shadow-sm shadow-orange-100"
    : "text-gray-500 hover:bg-gray-50 hover:text-gray-800";

  const badgeStyle = {
    gray: active ? "bg-white/20 text-white" : "bg-gray-100 text-gray-600",
    amber: active ? "bg-white/20 text-white" : "bg-amber-100 text-amber-600",
    green: active ? "bg-white/20 text-white" : "bg-green-100 text-green-600",
  }[badgeTone];

  return (
    <button
      onClick={onClick}
      className={`inline-flex items-center gap-2 rounded-2xl px-4 py-2 transition-all duration-200 ${style}`}
      style={{ fontSize: "13px", fontWeight: 600 }}
    >
      {label}
      {count !== undefined ? (
        <span className={`rounded-full px-2 py-0.5 text-xs font-bold ${badgeStyle}`}>
          {count}
        </span>
      ) : null}
    </button>
  );
}

function StatusBadge({ status }: { status: string }) {
  const meta = {
    Pending: {
      label: "Chờ duyệt",
      style: "bg-amber-100 text-amber-700",
      icon: Clock3,
    },
    Confirmed: {
      label: "Đã xác nhận",
      style: "bg-green-100 text-green-700",
      icon: CheckCircle2,
    },
    Rejected: {
      label: "Bị từ chối",
      style: "bg-red-100 text-red-700",
      icon: XCircle,
    },
    Cancelled: {
      label: "Đã hủy",
      style: "bg-gray-100 text-gray-600",
      icon: Ban,
    },
  }[status as keyof typeof meta] || {
    label: status,
    style: "bg-slate-100 text-slate-600",
    icon: Clock3,
  };

  const Icon = meta.icon;

  return (
    <span className={`inline-flex items-center gap-1 rounded-full px-3 py-1 font-bold ${meta.style}`} style={{ fontSize: "11px" }}>
      <Icon className="w-3.5 h-3.5" />
      {meta.label}
    </span>
  );
}

function formatDateTime(value: string) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return date.toLocaleString("vi-VN", {
    weekday: "long",
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

function toLocalDateTimeString(utcString: string) {
  const date = new Date(utcString);
  if (Number.isNaN(date.getTime())) return "";
  const tzOffset = date.getTimezoneOffset() * 60000;
  const localISOTime = new Date(date.getTime() - tzOffset).toISOString().slice(0, 16);
  return localISOTime;
}
