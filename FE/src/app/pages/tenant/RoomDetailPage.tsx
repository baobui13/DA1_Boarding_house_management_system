import { useEffect, useState } from "react";
import { useParams, useNavigate, useSearchParams } from "react-router";
import {
  MapPin,
  ChevronLeft,
  ChevronRight,
  Calendar,
  Ruler,
  Shield,
  Pencil,
  Check,
  X,
} from "lucide-react";
import { createAppointment } from "../../lib/appointments";
import { getPropertyListing, deleteProperty } from "../../lib/properties";
import type { PropertyListing } from "../../lib/types";
import { formatCurrency } from "../../lib/format";
import { useApp } from "../../context/AppContext";

export default function RoomDetailPage() {
  const { id = "" } = useParams();
  const { token, currentUser } = useApp();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const isLandlordView = searchParams.get("view") === "landlord";
  const [room, setRoom] = useState<PropertyListing | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [activeImage, setActiveImage] = useState(0);
  const [showBookingForm, setShowBookingForm] = useState(false);
  const [bookingDate, setBookingDate] = useState("");
  const [bookingTime, setBookingTime] = useState("14:00");
  const [bookingNote, setBookingNote] = useState("");
  const [bookingError, setBookingError] = useState("");
  const [bookingSuccess, setBookingSuccess] = useState("");
  const [submittingBooking, setSubmittingBooking] = useState(false);

  useEffect(() => {
    let cancelled = false;

    (async () => {
      setLoading(true);
      setError("");

      try {
        const listing = await getPropertyListing(id);
        if (!cancelled) {
          setRoom(listing);
        }
      } catch (err) {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : "Khong tai duoc chi tiet phong.");
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [id]);

  if (loading) {
    return <div className="max-w-6xl mx-auto px-4 py-10 text-gray-500">Đang tải chi tiết...</div>;
  }

  if (error || !room) {
    return (
      <div className="max-w-6xl mx-auto px-4 py-10">
        <div className="rounded-2xl border border-red-200 bg-red-50 px-5 py-4 text-red-600">
          {error || "Không tìm thấy dữ liệu."}
        </div>
      </div>
    );
  }

  const images = room.images.length > 0 ? room.images : ["https://placehold.co/1200x800?text=No+Image"];
  const handlePrevImage = () => setActiveImage((prev) => (prev - 1 + images.length) % images.length);
  const handleNextImage = () => setActiveImage((prev) => (prev + 1) % images.length);

  const handleDelete = async () => {
    if (!token) {
      alert("Bạn cần đăng nhập lại để xóa tin.");
      return;
    }

    if (!confirm("Bạn có chắc muốn xóa tin này?")) return;

    try {
      await deleteProperty(token, room.id);
      navigate("/landlord/properties");
    } catch (err) {
      alert(err instanceof Error ? err.message : "Xóa tin thất bại.");
    }
  };

  const resetBookingState = () => {
    setBookingError("");
    setBookingSuccess("");
  };

  const handleOpenBookingForm = () => {
    resetBookingState();
    setShowBookingForm(true);
  };

  const handleCloseBookingForm = () => {
    resetBookingState();
    setShowBookingForm(false);
  };

  const handleCreateAppointment = async () => {
    if (!token || !currentUser) {
      setBookingError("Bạn cần đăng nhập để đặt lịch xem phòng.");
      return;
    }

    if (!bookingDate) {
      setBookingError("Vui lòng chọn ngày hẹn.");
      return;
    }

    const appointmentDate = new Date(`${bookingDate}T${bookingTime}:00`);
    if (Number.isNaN(appointmentDate.getTime())) {
      setBookingError("Thời gian hẹn không hợp lệ.");
      return;
    }

    if (appointmentDate.getTime() <= Date.now()) {
      setBookingError("Vui lòng chọn thời gian trong tương lai.");
      return;
    }

    setSubmittingBooking(true);
    setBookingError("");
    setBookingSuccess("");

    try {
      await createAppointment(token, {
        propertyId: room.id,
        userId: currentUser.id,
        appointmentDateTime: appointmentDate.toISOString(),
        note: bookingNote.trim() || undefined,
      });

      setBookingSuccess("Đã gửi lịch hẹn thành công.");
      setBookingDate("");
      setBookingTime("14:00");
      setBookingNote("");
    } catch (err) {
      setBookingError(err instanceof Error ? err.message : "Không gửi được lịch hẹn.");
    } finally {
      setSubmittingBooking(false);
    }
  };

  return (
    <div className="max-w-6xl mx-auto px-4 py-6">
      <button
        onClick={() => navigate(isLandlordView ? "/landlord/properties" : "/search")}
        className="flex items-center gap-1.5 text-gray-500 hover:text-gray-700 mb-4 transition-colors"
        style={{ fontSize: "14px" }}
      >
        <ChevronLeft className="w-4 h-4" />
        {isLandlordView ? "Quay lại quản lý tài sản" : "Quay lại tìm kiếm"}
      </button>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <div className="lg:col-span-2 space-y-6">
          <div className="relative rounded-2xl overflow-hidden bg-gray-100">
            <img src={images[activeImage]} alt={room.propertyName} className="w-full aspect-[16/9] object-cover" />
            {images.length > 1 && (
              <>
                <button
                  onClick={handlePrevImage}
                  className="absolute left-3 top-1/2 -translate-y-1/2 w-9 h-9 rounded-full bg-black/40 text-white hover:bg-black/60 flex items-center justify-center transition-colors"
                >
                  <ChevronLeft className="w-5 h-5" />
                </button>
                <button
                  onClick={handleNextImage}
                  className="absolute right-3 top-1/2 -translate-y-1/2 w-9 h-9 rounded-full bg-black/40 text-white hover:bg-black/60 flex items-center justify-center transition-colors"
                >
                  <ChevronRight className="w-5 h-5" />
                </button>
              </>
            )}
          </div>

          {images.length > 1 && (
            <div className="flex gap-2">
              {images.map((img, index) => (
                <button
                  key={img}
                  onClick={() => setActiveImage(index)}
                  className={`w-20 h-14 rounded-lg overflow-hidden border-2 transition-all ${
                    activeImage === index ? "border-orange-400" : "border-transparent opacity-60 hover:opacity-80"
                  }`}
                >
                  <img src={img} alt="" className="w-full h-full object-cover" />
                </button>
              ))}
            </div>
          )}

          <div>
            <div className="flex items-start justify-between gap-4 mb-2">
              <h1 className="text-gray-900" style={{ fontSize: "22px", fontWeight: 700, lineHeight: 1.3 }}>
                {room.propertyName}
              </h1>
              <span className="shrink-0 px-3 py-1 rounded-xl bg-green-100 text-green-700" style={{ fontSize: "12px", fontWeight: 600 }}>
                {room.status}
              </span>
            </div>
            <div className="flex items-center gap-1.5 text-gray-500 mb-3">
              <MapPin className="w-4 h-4 text-orange-400" />
              <span style={{ fontSize: "14px" }}>{room.address || "Chưa có địa chỉ chi tiết"}</span>
            </div>
          </div>

          <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
            {[
              { icon: Ruler, label: "Diện tích", value: `${room.size}m²` },
              { icon: Shield, label: "Trạng thái", value: room.status },
              { icon: Calendar, label: "Ngày đăng", value: new Date(room.createdAt).toLocaleDateString("vi-VN") },
            ].map((stat) => (
              <div key={stat.label} className="bg-gray-50 rounded-xl p-3 text-center">
                <stat.icon className="w-5 h-5 text-orange-400 mx-auto mb-1" />
                <p className="text-gray-500" style={{ fontSize: "11px" }}>
                  {stat.label}
                </p>
                <p className="text-gray-900" style={{ fontSize: "14px", fontWeight: 600 }}>
                  {stat.value}
                </p>
              </div>
            ))}
          </div>

          <div>
            <h2 className="text-gray-900 mb-3" style={{ fontSize: "17px", fontWeight: 700 }}>
              Mô tả
            </h2>
            <p className="text-gray-600 leading-relaxed" style={{ fontSize: "14px", lineHeight: 1.7 }}>
              {room.description || "Backend chưa có mô tả cho tin này."}
            </p>
          </div>

          <div>
            <h2 className="text-gray-900 mb-3" style={{ fontSize: "17px", fontWeight: 700 }}>
              Tiện nghi
            </h2>
            {room.amenities.length === 0 ? (
              <div className="rounded-xl border border-amber-200 bg-amber-50 px-4 py-3 text-amber-700" style={{ fontSize: "14px" }}>
                Backend chưa trả danh sách tiện nghi cho tin này.
              </div>
            ) : (
              <div className="grid grid-cols-2 gap-2">
                {room.amenities.map((amenity) => (
                  <div key={amenity} className="flex items-center gap-2">
                    <div className="w-5 h-5 rounded-full bg-green-100 flex items-center justify-center shrink-0">
                      <Check className="w-3 h-3 text-green-600" />
                    </div>
                    <span className="text-gray-600" style={{ fontSize: "14px" }}>
                      {amenity}
                    </span>
                  </div>
                ))}
              </div>
            )}
          </div>

        </div>

        <div className="lg:col-span-1">
          <div className="sticky top-20 space-y-4">
            <div className="bg-white rounded-2xl border border-gray-100 shadow-lg p-5">
              <div className="mb-4">
                <div className="flex items-baseline gap-1">
                  <span className="text-orange-600" style={{ fontSize: "28px", fontWeight: 700 }}>
                    {formatCurrency(room.price)}
                  </span>
                  <span className="text-gray-400" style={{ fontSize: "14px" }}>
                    /tháng
                  </span>
                </div>
              </div>

              <div className="space-y-3">
                {isLandlordView ? (
                  <>
                    <button
                      onClick={() => navigate("/landlord/properties")}
                      className="w-full flex items-center justify-center gap-2 py-3 bg-orange-500 text-white rounded-xl hover:bg-orange-600 transition-colors"
                      style={{ fontSize: "15px", fontWeight: 600 }}
                    >
                      <Pencil className="w-4.5 h-4.5" style={{ width: "18px", height: "18px" }} />
                      Về trang quản lý để sửa
                    </button>
                    <button
                      onClick={handleDelete}
                      className="w-full flex items-center justify-center gap-2 py-2.5 border-2 border-red-200 text-red-600 rounded-xl hover:bg-red-50 transition-colors"
                      style={{ fontSize: "14px", fontWeight: 600 }}
                    >
                      Xóa tin
                    </button>
                  </>
                ) : (
                  <>
                    <button
                      onClick={handleOpenBookingForm}
                      className="w-full flex items-center justify-center gap-2 py-3 bg-orange-500 text-white rounded-xl hover:bg-orange-600 transition-colors"
                      style={{ fontSize: "15px", fontWeight: 600 }}
                    >
                      <Calendar className="w-4.5 h-4.5" style={{ width: "18px", height: "18px" }} />
                      Đặt lịch xem phòng
                    </button>
                  </>
                )}
              </div>
            </div>
          </div>
        </div>
      </div>

      {showBookingForm && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
          <div className="bg-white rounded-2xl shadow-2xl w-full max-w-md overflow-hidden">
            <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100">
              <h3 className="text-gray-900" style={{ fontSize: "17px", fontWeight: 700 }}>
                Đặt lịch xem phòng
              </h3>
              <button onClick={handleCloseBookingForm} className="text-gray-400 hover:text-gray-600">
                <X className="w-5 h-5" />
              </button>
            </div>

            <div className="p-6 space-y-4">
              <input
                type="date"
                value={bookingDate}
                onChange={(e) => setBookingDate(e.target.value)}
                min={new Date().toISOString().split("T")[0]}
                className="w-full px-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none"
              />
              <input
                type="time"
                value={bookingTime}
                onChange={(e) => setBookingTime(e.target.value)}
                className="w-full px-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none"
              />
              <textarea
                value={bookingNote}
                onChange={(e) => setBookingNote(e.target.value)}
                rows={3}
                placeholder="Ghi chú"
                className="w-full px-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none resize-none"
              />
              {bookingError ? (
                <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-red-600" style={{ fontSize: "13px" }}>
                  {bookingError}
                </div>
              ) : null}
              {bookingSuccess ? (
                <div className="rounded-xl border border-green-200 bg-green-50 px-4 py-3 text-green-700" style={{ fontSize: "13px" }}>
                  {bookingSuccess}
                </div>
              ) : null}
              <div className="flex gap-3">
                <button
                  onClick={handleCloseBookingForm}
                  className="flex-1 py-3 rounded-xl border border-gray-200 text-gray-600 hover:bg-gray-50 transition-colors"
                  style={{ fontSize: "14px" }}
                >
                  Hủy
                </button>
                <button
                  onClick={() => void handleCreateAppointment()}
                  disabled={submittingBooking}
                  className="flex-1 py-3 rounded-xl bg-orange-500 text-white hover:bg-orange-600 transition-colors disabled:opacity-60"
                  style={{ fontSize: "14px", fontWeight: 600 }}
                >
                  {submittingBooking ? "Đang gửi..." : "Gửi lịch hẹn"}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
