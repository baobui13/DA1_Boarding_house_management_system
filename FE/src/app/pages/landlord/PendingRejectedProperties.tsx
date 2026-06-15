import { useEffect, useState, useMemo } from "react";
import { useNavigate } from "react-router";
import {
  Building2,
  Clock,
  ShieldAlert,
  Trash2,
  Pencil,
  MapPin,
  ChevronRight,
} from "lucide-react";
import { useApp } from "../../context/AppContext";
import { getPropertyListings, deleteProperty } from "../../lib/properties";
import { formatCurrency } from "../../lib/format";

export default function PendingRejectedProperties() {
  const { currentUser, token } = useApp();
  const navigate = useNavigate();

  const [properties, setProperties] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [activeTab, setActiveTab] = useState<"pending" | "rejected">("pending");

  const loadData = async () => {
    if (!currentUser) return;
    setLoading(true);
    setError("");

    try {
      const response = await getPropertyListings({
        landlordId: currentUser.id,
        pageSize: 1000,
      }, token);

      // Lọc các phòng chưa duyệt hoặc bị từ chối
      const filtered = response.items.filter(
        (p) => p.moderationStatus === "Pending" || p.moderationStatus === "Rejected"
      );
      setProperties(filtered);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Không tải được dữ liệu phòng.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadData();
  }, [currentUser]);

  const pendingList = useMemo(() => {
    return properties.filter((p) => p.moderationStatus === "Pending");
  }, [properties]);

  const rejectedList = useMemo(() => {
    return properties.filter((p) => p.moderationStatus === "Rejected");
  }, [properties]);

  const activeList = activeTab === "pending" ? pendingList : rejectedList;

  const handleDelete = async (id: string, name: string) => {
    if (!token) return;
    if (!confirm(`Bạn có chắc chắn muốn xóa tin đăng "${name}" không?`)) return;

    try {
      await deleteProperty(token, id);
      setProperties((prev) => prev.filter((p) => p.id !== id));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Xóa thất bại.");
    }
  };

  return (
    <div className="max-w-7xl mx-auto px-4 py-6">
      {/* Header section */}
      <div className="mb-8">
        <h1 className="text-gray-900" style={{ fontSize: "24px", fontWeight: 700 }}>
          Kiểm Duyệt Tin Đăng
        </h1>
        <p className="text-gray-500 mt-1" style={{ fontSize: "14px" }}>
          Quản lý trạng thái phê duyệt của các phòng trọ/tài sản mới đăng tải hoặc cần chỉnh sửa thông tin.
        </p>
      </div>

      {error && (
        <div className="mb-6 rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-red-600">
          {error}
        </div>
      )}

      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
        <div className="rounded-3xl border border-amber-100 bg-amber-50/50 p-6 flex items-center justify-between shadow-sm">
          <div className="space-y-1">
            <p className="text-gray-500 font-medium" style={{ fontSize: "14px" }}>
              Đang chờ duyệt
            </p>
            <p className="text-amber-600 font-bold" style={{ fontSize: "36px", lineHeight: 1 }}>
              {pendingList.length}
            </p>
          </div>
          <div className="h-14 w-14 rounded-2xl bg-amber-100/80 text-amber-600 flex items-center justify-center">
            <Clock className="w-7 h-7" />
          </div>
        </div>

        <div className="rounded-3xl border border-rose-100 bg-rose-50/50 p-6 flex items-center justify-between shadow-sm">
          <div className="space-y-1">
            <p className="text-gray-500 font-medium" style={{ fontSize: "14px" }}>
              Bị từ chối duyệt
            </p>
            <p className="text-rose-600 font-bold" style={{ fontSize: "36px", lineHeight: 1 }}>
              {rejectedList.length}
            </p>
          </div>
          <div className="h-14 w-14 rounded-2xl bg-rose-100/80 text-rose-600 flex items-center justify-center">
            <ShieldAlert className="w-7 h-7" />
          </div>
        </div>
      </div>

      {/* Interactive Tabs */}
      <div className="border-b border-gray-100 flex gap-6 mb-8">
        <button
          onClick={() => setActiveTab("pending")}
          className={`pb-4 relative font-semibold transition-colors ${
            activeTab === "pending" ? "text-orange-500" : "text-gray-400 hover:text-gray-600"
          }`}
          style={{ fontSize: "16px" }}
        >
          Đang chờ duyệt ({pendingList.length})
          {activeTab === "pending" && (
            <span className="absolute bottom-0 left-0 right-0 h-0.5 bg-orange-500 rounded-full" />
          )}
        </button>
        <button
          onClick={() => setActiveTab("rejected")}
          className={`pb-4 relative font-semibold transition-colors ${
            activeTab === "rejected" ? "text-orange-500" : "text-gray-400 hover:text-gray-600"
          }`}
          style={{ fontSize: "16px" }}
        >
          Bị từ chối ({rejectedList.length})
          {activeTab === "rejected" && (
            <span className="absolute bottom-0 left-0 right-0 h-0.5 bg-orange-500 rounded-full" />
          )}
        </button>
      </div>

      {/* Content Section */}
      {loading ? (
        <div className="space-y-4">
          {Array.from({ length: 2 }).map((_, idx) => (
            <div key={idx} className="h-40 rounded-3xl bg-gray-100 animate-pulse" />
          ))}
        </div>
      ) : activeList.length === 0 ? (
        <div className="rounded-3xl border border-dashed border-gray-200 bg-white px-6 py-16 text-center">
          <Building2 className="w-12 h-12 mx-auto mb-4 text-gray-300" />
          <p className="text-gray-700 font-bold" style={{ fontSize: "16px" }}>
            Không tìm thấy phòng nào
          </p>
          <p className="text-gray-400 mt-1" style={{ fontSize: "14px" }}>
            {activeTab === "pending"
              ? "Hiện tại bạn không có phòng nào đang chờ duyệt từ quản trị viên."
              : "Tuyệt vời! Không có phòng nào của bạn bị từ chối phê duyệt."}
          </p>
        </div>
      ) : (
        <div className="space-y-6">
          {activeList.map((property) => (
            <div
              key={property.id}
              className="overflow-hidden rounded-3xl border border-gray-100 bg-white p-6 shadow-sm hover:shadow-md transition-shadow"
            >
              <div className="flex flex-col gap-6 lg:flex-row lg:items-start lg:justify-between">
                {/* Information Area */}
                <div className="flex flex-col md:flex-row gap-5 min-w-0 flex-1">
                  {/* Property Image Placeholder or Real image */}
                  <div className="h-28 w-44 rounded-2xl bg-gray-50 border border-gray-100 overflow-hidden shrink-0 flex items-center justify-center text-gray-400 relative">
                    {property.images && property.images.length > 0 ? (
                      <img
                        src={property.images[0]}
                        alt={property.propertyName}
                        className="w-full h-full object-cover"
                      />
                    ) : (
                      <Building2 className="w-8 h-8 text-gray-300" />
                    )}
                  </div>

                  <div className="min-w-0 space-y-1">
                    <h3 className="text-gray-900 truncate" style={{ fontSize: "20px", fontWeight: 700 }}>
                      {property.propertyName}
                    </h3>
                    <div className="flex items-center gap-1 text-gray-500" style={{ fontSize: "13px" }}>
                      <MapPin className="w-4 h-4 text-gray-400" />
                      <span className="truncate">{property.address || "Chưa cập nhật địa chỉ"}</span>
                    </div>
                    <div className="pt-1 flex flex-wrap gap-x-4 gap-y-1 text-gray-600" style={{ fontSize: "13px", fontWeight: 500 }}>
                      <span>Diện tích: <strong className="text-gray-900">{property.size}m²</strong></span>
                      <span>Giá thuê: <strong className="text-orange-500">{formatCurrency(property.price)}/tháng</strong></span>
                    </div>
                    {property.amenities && property.amenities.length > 0 && (
                      <div className="pt-2 flex flex-wrap gap-1.5">
                        {property.amenities.slice(0, 4).map((amenity: string) => (
                          <span
                            key={amenity}
                            className="rounded-full bg-gray-50 border border-gray-100 px-2.5 py-0.5 text-gray-500"
                            style={{ fontSize: "11px", fontWeight: 600 }}
                          >
                            {amenity}
                          </span>
                        ))}
                      </div>
                    )}
                  </div>
                </div>

                {/* Status and Action Buttons */}
                <div className="flex flex-col items-start gap-4 lg:items-end shrink-0">
                  {property.moderationStatus === "Pending" ? (
                    <span className="inline-flex items-center gap-1.5 rounded-full bg-amber-50 border border-amber-100 px-3 py-1.5 text-amber-700 font-semibold" style={{ fontSize: "12px" }}>
                      <Clock className="w-4 h-4 animate-pulse" />
                      Đang chờ duyệt
                    </span>
                  ) : (
                    <span className="inline-flex items-center gap-1.5 rounded-full bg-rose-50 border border-rose-100 px-3 py-1.5 text-rose-700 font-semibold" style={{ fontSize: "12px" }}>
                      <ShieldAlert className="w-4 h-4" />
                      Bị từ chối duyệt
                    </span>
                  )}

                  <div className="flex flex-wrap gap-2.5">
                    <button
                      onClick={() => navigate(`/landlord/rooms/${property.id}/edit`)}
                      className="inline-flex items-center gap-1.5 rounded-2xl border border-orange-200 px-4 py-2.5 text-orange-600 hover:bg-orange-50 transition-colors"
                      style={{ fontSize: "13px", fontWeight: 700 }}
                    >
                      <Pencil className="w-4 h-4" />
                      Chỉnh sửa
                    </button>
                    <button
                      onClick={() => handleDelete(property.id, property.propertyName)}
                      className="inline-flex items-center gap-1.5 rounded-2xl border border-red-200 px-4 py-2.5 text-red-600 hover:bg-red-50 transition-colors"
                      style={{ fontSize: "13px", fontWeight: 700 }}
                    >
                      <Trash2 className="w-4 h-4" />
                      Xóa phòng
                    </button>
                  </div>
                </div>
              </div>

              {/* Status Specific Description Blocks */}
              {property.moderationStatus === "Rejected" ? (
                <div className="mt-5 rounded-2xl border border-red-100 bg-red-50/50 p-4 text-red-700 space-y-1.5">
                  <div className="flex items-center gap-2 font-bold" style={{ fontSize: "13px" }}>
                    <ShieldAlert className="w-4 h-4" />
                    Lý do từ chối kiểm duyệt của quản trị viên:
                  </div>
                  <p style={{ fontSize: "13px", lineHeight: 1.6 }}>
                    {property.rejectionReason || "Không có lý do cụ thể. Bạn vui lòng kiểm tra lại thông tin, ảnh và mức giá của phòng để chỉnh sửa cho hợp lệ."}
                  </p>
                </div>
              ) : (
                <div className="mt-5 rounded-2xl border border-amber-100 bg-amber-50/50 p-4 text-amber-700 space-y-1.5">
                  <div className="flex items-center gap-2 font-bold" style={{ fontSize: "13px" }}>
                    <Clock className="w-4 h-4" />
                    Báo cáo tiến độ:
                  </div>
                  <p style={{ fontSize: "13px", lineHeight: 1.6 }}>
                    Tin đăng đang được hệ thống phân tích và duyệt tự động kết hợp thủ công bởi ban quản trị. Chúng tôi sẽ nhanh chóng phê duyệt và hiển thị phòng lên trang tìm kiếm công khai nếu thông tin hoàn toàn hợp lệ.
                  </p>
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
