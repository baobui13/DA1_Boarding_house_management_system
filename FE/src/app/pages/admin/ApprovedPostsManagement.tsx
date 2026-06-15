import { useEffect, useMemo, useState } from "react";
import {
  ShieldCheck,
  CheckCircle2,
  XCircle,
  Clock,
  Eye,
  MapPin,
  X,
  Home,
  Ruler,
  Image as ImageIcon,
  Building2,
  Search,
  UserRound,
  Trash2,
  Lock,
} from "lucide-react";
import { useApp } from "../../context/AppContext";
import { getModerationProperties, getPropertyListing, updateProperty, deleteProperty } from "../../lib/properties";
import { getUsers } from "../../lib/users";
import type { PropertyListing, PropertyResponse, UserResponse } from "../../lib/types";
import { formatCurrency } from "../../lib/format";
import { getPropertyStatusMeta } from "../../lib/propertyStatus";

const PAGE_SIZE = 8;

export default function ApprovedPostsManagement() {
  const { token } = useApp();
  const [posts, setPosts] = useState<PropertyResponse[]>([]);
  const [users, setUsers] = useState<UserResponse[]>([]);
  const [selectedPostId, setSelectedPostId] = useState<string | null>(null);
  const [selectedPostDetail, setSelectedPostDetail] = useState<PropertyListing | null>(null);
  const [rejectReason, setRejectReason] = useState("");
  const [showRejectModal, setShowRejectModal] = useState<string | null>(null);
  
  const [pageNumber, setPageNumber] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [searchQuery, setSearchQuery] = useState("");
  
  const [loading, setLoading] = useState(true);
  const [detailLoading, setDetailLoading] = useState(false);
  const [error, setError] = useState("");

  const loadData = async () => {
    if (!token) {
      setError("Thiếu token admin để truy cập trang này.");
      setLoading(false);
      return;
    }

    setLoading(true);
    setError("");
    try {
      const [propertyResponse, userResponse] = await Promise.all([
        getModerationProperties(token, {
          status: "Approved",
          pageNumber: 1, // Fetch larger set to support full local search and metrics or keep it paginated
          pageSize: 1000,
        }),
        getUsers({ page: 1, pageSize: 1000 }, token),
      ]);

      setPosts(propertyResponse.items);
      setUsers(userResponse.items);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Không tải được danh sách tin đã duyệt.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadData();
  }, [token]);

  useEffect(() => {
    if (!selectedPostId) {
      setSelectedPostDetail(null);
      return;
    }

    let cancelled = false;

    const loadDetail = async () => {
      setDetailLoading(true);
      try {
        const detail = await getPropertyListing(selectedPostId);
        if (!cancelled) {
          setSelectedPostDetail(detail);
        }
      } catch (err) {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : "Không tải được chi tiết tin.");
        }
      } finally {
        if (!cancelled) {
          setDetailLoading(false);
        }
      }
    };

    void loadDetail();

    return () => {
      cancelled = true;
    };
  }, [selectedPostId]);

  const usersById = useMemo(
    () => Object.fromEntries(users.map((u) => [u.id, u])),
    [users],
  );

  // Filter approved posts based on search query
  const searchedPosts = useMemo(() => {
    return posts.filter((post) => {
      if (!searchQuery) return true;
      const query = searchQuery.toLowerCase();
      const landlord = usersById[post.landlordId];
      
      const title = post.propertyName?.toLowerCase() || "";
      const address = post.address?.toLowerCase() || "";
      const ownerName = landlord?.fullName?.toLowerCase() || "";
      const ownerEmail = landlord?.email?.toLowerCase() || "";

      return title.includes(query) || address.includes(query) || ownerName.includes(query) || ownerEmail.includes(query);
    });
  }, [posts, searchQuery, usersById]);

  // Pagination on filtered posts
  const paginatedPosts = useMemo(() => {
    const startIndex = (pageNumber - 1) * PAGE_SIZE;
    return searchedPosts.slice(startIndex, startIndex + PAGE_SIZE);
  }, [searchedPosts, pageNumber]);

  // Statistics counters
  const stats = useMemo(() => {
    return {
      total: searchedPosts.length,
      available: searchedPosts.filter((p) => p.status.toLowerCase() === "available" || p.status === "Trống").length,
      rented: searchedPosts.filter((p) => p.status.toLowerCase() === "rented" || p.status === "Đã cho thuê").length,
      maintenance: searchedPosts.filter((p) => p.status.toLowerCase() === "maintenance" || p.status === "Đang sửa chữa").length,
    };
  }, [searchedPosts]);

  const handleRevokeApproval = async (id: string) => {
    if (!token) return;
    if (!rejectReason) {
      alert("Vui lòng điền lý do khóa tin đăng.");
      return;
    }

    try {
      await updateProperty(token, {
        id,
        moderationStatus: "Rejected",
        rejectionReason: `Bị khóa bởi Admin: ${rejectReason}`,
      });

      setShowRejectModal(null);
      setRejectReason("");
      setSelectedPostId(null);
      setSelectedPostDetail(null);
      await loadData();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Khóa tin đăng thất bại.");
    }
  };

  const handleDelete = async (id: string) => {
    if (!token) return;
    if (!confirm("Bạn có chắc chắn muốn xóa vĩnh viễn tin đăng này khỏi cơ sở dữ liệu?")) return;

    try {
      await deleteProperty(token, id);
      setSelectedPostId(null);
      setSelectedPostDetail(null);
      await loadData();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Xóa tin đăng thất bại.");
    }
  };

  const pageCount = Math.max(1, Math.ceil(searchedPosts.length / PAGE_SIZE));
  const changePage = (nextPage: number) => {
    if (nextPage === pageNumber) return;
    setPageNumber(nextPage);
    window.scrollTo({ top: 0, behavior: "smooth" });
  };

  const viewedPost = selectedPostDetail;
  const viewedLandlord = viewedPost ? usersById[viewedPost.landlordId] : null;

  return (
    <div className="max-w-7xl mx-auto px-4 py-6 space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-gray-900" style={{ fontSize: "24px", fontWeight: 700 }}>
            Quản Lý Bài Đăng Đã Duyệt
          </h1>
          <p className="text-gray-500 mt-1" style={{ fontSize: "14px" }}>
            Giám sát, tìm kiếm, khóa hoặc gỡ bỏ các tin đăng phòng trọ đã được phê duyệt trên hệ thống.
          </p>
        </div>
      </div>

      {error && (
        <div className="rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-red-600">
          {error}
        </div>
      )}

      {/* Stats Counter Grid */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <SummaryCard label="Tổng bài đăng" value={loading ? "..." : String(stats.total)} tone="blue" />
        <SummaryCard label="Phòng đang trống" value={loading ? "..." : String(stats.available)} tone="green" />
        <SummaryCard label="Phòng đã thuê" value={loading ? "..." : String(stats.rented)} tone="purple" />
        <SummaryCard label="Đang sửa chữa" value={loading ? "..." : String(stats.maintenance)} tone="amber" />
      </div>

      {/* Filter and Live Search input */}
      <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between bg-white rounded-3xl border border-gray-100 p-4 shadow-sm">
        <div className="flex items-center gap-2">
          <span className="rounded-full bg-green-100 px-3.5 py-1 text-green-700" style={{ fontSize: "12px", fontWeight: 700 }}>
            Trạng thái: Đã Phê Duyệt
          </span>
        </div>

        <div className="relative w-full md:max-w-xs">
          <Search className="absolute left-3.5 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => {
              setSearchQuery(e.target.value);
              setPageNumber(1); // Reset back to first page
            }}
            placeholder="Tìm phòng, chủ trọ, địa chỉ..."
            className="w-full pl-10 pr-4 py-2 border border-gray-200 rounded-2xl focus:border-orange-500 focus:outline-none placeholder-gray-400"
            style={{ fontSize: "14px" }}
          />
        </div>
      </div>

      {/* Main layout grid */}
      <div className="grid grid-cols-1 lg:grid-cols-[1.25fr_1fr] gap-6 items-start">
        {/* Left Column list */}
        <div className="space-y-4">
          {loading ? (
            Array.from({ length: 4 }).map((_, index) => (
              <div key={index} className="h-28 rounded-3xl bg-gray-100 animate-pulse" />
            ))
          ) : paginatedPosts.length === 0 ? (
            <div className="text-center py-16 text-gray-400 bg-white rounded-3xl border border-gray-100">
              <Building2 className="w-12 h-12 mx-auto mb-3 opacity-30" />
              <p className="text-gray-700" style={{ fontSize: "15px", fontWeight: 600 }}>
                Không tìm thấy bài đăng nào đã duyệt
              </p>
              <p className="text-gray-400 mt-1" style={{ fontSize: "13px" }}>
                Thử đổi từ khóa tìm kiếm khác.
              </p>
            </div>
          ) : (
            <div className="space-y-3">
              {paginatedPosts.map((post) => {
                const landlord = usersById[post.landlordId];
                const isSelected = selectedPostId === post.id;
                const statusMeta = getPropertyStatusMeta(post.status);

                return (
                  <div
                    key={post.id}
                    onClick={() => setSelectedPostId(isSelected ? null : post.id)}
                    className={`bg-white rounded-3xl border p-4 cursor-pointer transition-all duration-200 ${
                      isSelected ? "border-orange-500 shadow-md shadow-orange-50/50" : "border-gray-100 hover:border-gray-200"
                    }`}
                  >
                    <div className="flex gap-4">
                      <div className="w-24 h-24 rounded-2xl bg-orange-50 shrink-0 flex items-center justify-center text-orange-500 overflow-hidden">
                        <Home className="w-6 h-6" />
                      </div>
                      <div className="flex-1 min-w-0 flex flex-col justify-between py-0.5">
                        <div>
                          <div className="flex items-start justify-between gap-2">
                            <h4 className="text-gray-900 line-clamp-1" style={{ fontSize: "15px", fontWeight: 700 }}>
                              {post.propertyName}
                            </h4>
                            <span className={`shrink-0 rounded-full px-2.5 py-0.5 font-bold ${
                              statusMeta.tone === "green"
                                ? "bg-green-100 text-green-700"
                                : statusMeta.tone === "blue"
                                  ? "bg-blue-100 text-blue-700"
                                  : "bg-amber-100 text-amber-700"
                            }`} style={{ fontSize: "10px" }}>
                              {statusMeta.label}
                            </span>
                          </div>
                          <div className="flex items-center gap-1 text-gray-400 mt-1">
                            <MapPin className="w-3.5 h-3.5 shrink-0" />
                            <p className="truncate" style={{ fontSize: "12px" }}>
                              {post.address || "Không rõ địa chỉ"}
                            </p>
                          </div>
                        </div>

                        <div className="flex items-center justify-between mt-2 pt-2 border-t border-gray-50">
                          <div className="flex items-center gap-3">
                            <span className="text-orange-600" style={{ fontSize: "14px", fontWeight: 700 }}>
                              {new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(post.price)}/tháng
                            </span>
                            {post.totalRatings && post.totalRatings > 0 ? (
                              <div className="flex items-center gap-1 text-orange-500" style={{ fontSize: "12px", fontWeight: 600 }}>
                                <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="currentColor" stroke="none"><polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/></svg>
                                <span>{post.averageRating?.toFixed(1)}</span>
                                <span className="text-gray-400 font-normal">({post.totalRatings})</span>
                              </div>
                            ) : null}
                          </div>
                          <span className="text-gray-400" style={{ fontSize: "11px" }}>
                            Chủ: {landlord?.fullName || "Chưa gán"}
                          </span>
                        </div>
                      </div>
                    </div>
                  </div>
                );
              })}
            </div>
          )}

          {/* Pagination bar */}
          <div className="flex items-center justify-between rounded-3xl border border-gray-100 bg-white px-5 py-4 shadow-sm">
            <p className="text-gray-500" style={{ fontSize: "13px" }}>
              Trang {pageNumber}/{pageCount} · hiển thị {paginatedPosts.length} / {searchedPosts.length} tin
            </p>
            <div className="flex gap-2">
              <button
                type="button"
                onClick={() => changePage(Math.max(1, pageNumber - 1))}
                disabled={pageNumber <= 1 || loading}
                className="rounded-xl border border-gray-200 px-4 py-2 text-sm text-gray-600 disabled:opacity-40 transition-colors hover:bg-gray-50"
              >
                Trước
              </button>
              <button
                type="button"
                onClick={() => changePage(Math.min(pageCount, pageNumber + 1))}
                disabled={pageNumber >= pageCount || loading}
                className="rounded-xl border border-gray-200 px-4 py-2 text-sm text-gray-600 disabled:opacity-40 transition-colors hover:bg-gray-50"
              >
                Sau
              </button>
            </div>
          </div>
        </div>

        {/* Right Column details viewer */}
        <div>
          {detailLoading ? (
            <div className="bg-white rounded-3xl border border-gray-100 p-5 animate-pulse space-y-4">
              <div className="h-6 w-48 rounded bg-gray-100 mb-4" />
              <div className="h-52 rounded-2xl bg-gray-100 mb-4" />
              <div className="h-32 rounded-2xl bg-gray-100" />
            </div>
          ) : viewedPost ? (
            <div className="bg-white rounded-3xl border border-gray-100 p-5 space-y-4 sticky top-20 shadow-sm">
              <div className="flex items-center justify-between pb-3 border-b border-gray-50">
                <h3 className="text-gray-900" style={{ fontSize: "16px", fontWeight: 700 }}>
                  Chi Tiết Bài Đăng
                </h3>
                <button onClick={() => setSelectedPostId(null)} className="text-gray-400 hover:text-gray-600">
                  <X className="w-5 h-5" />
                </button>
              </div>

              <div>
                <h4 className="text-gray-900" style={{ fontSize: "18px", fontWeight: 700 }}>
                  {viewedPost.propertyName}
                </h4>
                <div className="flex items-center gap-2 text-gray-500 mt-1" style={{ fontSize: "13px" }}>
                  <MapPin className="w-4 h-4 text-orange-500 shrink-0" />
                  <span>{viewedPost.address || "Chưa có địa chỉ"}</span>
                </div>
              </div>

              <img
                src={viewedPost.images[0] || "https://placehold.co/600x400?text=No+Image"}
                alt=""
                className="w-full rounded-2xl object-cover"
                style={{ height: "200px" }}
              />

              {viewedPost.images.length > 1 && (
                <div className="grid grid-cols-4 gap-2">
                  {viewedPost.images.slice(0, 4).map((image, index) => (
                    <img key={`${viewedPost.id}-${index}`} src={image} alt="" className="w-full h-16 rounded-xl object-cover border border-gray-50" />
                  ))}
                </div>
              )}

              {/* Technical indicators */}
              <div className="grid grid-cols-4 gap-3">
                <InfoCard icon={Home} label="Mã tin" value={viewedPost.id.slice(0, 8)} />
                <InfoCard icon={Ruler} label="Diện tích" value={`${viewedPost.size} m²`} />
                <InfoCard icon={ImageIcon} label="Số ảnh" value={`${viewedPost.images.length}`} />
                <InfoCard icon={() => <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="currentColor" stroke="none"><polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/></svg>} label="Đánh giá" value={viewedPost.totalRatings && viewedPost.totalRatings > 0 ? `${viewedPost.averageRating?.toFixed(1)} (${viewedPost.totalRatings})` : "Chưa có"} />
              </div>

              {/* Cost specifications */}
              <div className="bg-gray-50 rounded-2xl p-4 space-y-2.5" style={{ fontSize: "13px" }}>
                <div className="flex justify-between gap-4">
                  <span className="text-gray-400">Giá thuê tháng</span>
                  <span className="text-orange-600 font-bold">{formatCurrency(viewedPost.price)}/tháng</span>
                </div>
                <div className="flex justify-between gap-4">
                  <span className="text-gray-400">Đơn giá điện</span>
                  <span className="text-gray-700 font-semibold">{viewedPost.electricPrice ? `${formatCurrency(viewedPost.electricPrice)}/kWh` : "Chưa cấu hình"}</span>
                </div>
                <div className="flex justify-between gap-4">
                  <span className="text-gray-400">Đơn giá nước</span>
                  <span className="text-gray-700 font-semibold">{viewedPost.waterPrice ? `${formatCurrency(viewedPost.waterPrice)}/m³` : "Chưa cấu hình"}</span>
                </div>
                <div className="flex justify-between gap-4 pt-2 border-t border-gray-200/60">
                  <span className="text-gray-400">Ngày phê duyệt</span>
                  <span className="text-gray-700 font-semibold">{viewedPost.approvedAt ? new Date(viewedPost.approvedAt).toLocaleDateString("vi-VN") : "Không rõ"}</span>
                </div>
              </div>

              {/* Owner details */}
              <div className="rounded-2xl border border-gray-100 p-4 space-y-2">
                <p className="text-gray-500 font-bold" style={{ fontSize: "12px" }}>
                  THÔNG TIN CHỦ TRỌ
                </p>
                <div className="flex items-center gap-3">
                  <div className="w-10 h-10 rounded-full bg-orange-100 text-orange-600 flex items-center justify-center shrink-0">
                    <UserRound className="w-5 h-5" />
                  </div>
                  <div className="min-w-0">
                    <p className="text-gray-900 font-semibold truncate" style={{ fontSize: "13px" }}>
                      {viewedLandlord?.fullName || "Không rõ tên"}
                    </p>
                    <p className="text-gray-400 truncate" style={{ fontSize: "11px" }}>
                      {viewedLandlord?.email || "Không rõ mail"}
                    </p>
                    {viewedLandlord?.phoneNumber && (
                      <p className="text-orange-600 font-medium" style={{ fontSize: "11px" }}>
                        SĐT: {viewedLandlord.phoneNumber}
                      </p>
                    )}
                  </div>
                </div>
              </div>

              {/* Description and Amenities */}
              <div className="bg-gray-50 rounded-2xl p-4 space-y-4">
                <div>
                  <p className="text-gray-500 font-bold mb-1.5" style={{ fontSize: "11px" }}>
                    MÔ TẢ CHI TIẾT
                  </p>
                  <p className="text-gray-700" style={{ fontSize: "13px", lineHeight: 1.6 }}>
                    {viewedPost.description || "Chủ trọ không cung cấp mô tả chi tiết."}
                  </p>
                </div>

                {viewedPost.amenities.length > 0 && (
                  <div>
                    <p className="text-gray-500 font-bold mb-2" style={{ fontSize: "11px" }}>
                      TIỆN ÍCH PHÒNG
                    </p>
                    <div className="flex flex-wrap gap-1.5">
                      {viewedPost.amenities.map((amenity) => (
                        <span key={amenity} className="px-2.5 py-1 rounded-xl bg-white border border-gray-100 text-gray-600" style={{ fontSize: "12px" }}>
                          {amenity}
                        </span>
                      ))}
                    </div>
                  </div>
                )}
              </div>

              {/* Administrative actions */}
              <div className="rounded-2xl border border-red-100 bg-red-50/50 p-4 space-y-3">
                <p className="text-red-800 font-bold" style={{ fontSize: "13px" }}>
                  Quyền Quản Trị Viên (Admin)
                </p>
                <div className="flex flex-col sm:flex-row gap-2">
                  <button
                    onClick={() => setShowRejectModal(viewedPost.id)}
                    className="flex-1 inline-flex items-center justify-center gap-1.5 py-2.5 bg-red-100 border border-red-200 hover:bg-red-200 text-red-600 rounded-xl transition-colors"
                    style={{ fontSize: "13px", fontWeight: 600 }}
                  >
                    <Lock className="w-4 h-4" />
                    Khóa tin đăng
                  </button>
                  <button
                    onClick={() => void handleDelete(viewedPost.id)}
                    className="flex-1 inline-flex items-center justify-center gap-1.5 py-2.5 bg-red-600 hover:bg-red-700 text-white rounded-xl transition-colors"
                    style={{ fontSize: "13px", fontWeight: 600 }}
                  >
                    <Trash2 className="w-4 h-4" />
                    Xóa bài
                  </button>
                </div>
              </div>
            </div>
          ) : (
            <div className="bg-white rounded-3xl border border-gray-100 p-8 text-center text-gray-400 lg:sticky lg:top-20 shadow-sm">
              <Eye className="w-10 h-10 mx-auto mb-3 opacity-30" />
              <p className="text-gray-700 mb-1" style={{ fontSize: "14px", fontWeight: 600 }}>
                Chọn một tin đã duyệt để quản lý
              </p>
              <p style={{ fontSize: "13px", lineHeight: 1.5 }}>
                Bấm chọn tin đăng từ danh sách bên trái để tải thông tin kỹ thuật, hồ sơ chủ nhà, chi phí và thực hiện các quyết định khóa hoặc xóa.
              </p>
            </div>
          )}
        </div>
      </div>

      {/* Custom lock/reject modal */}
      {showRejectModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/40 backdrop-blur-sm animate-fade-in">
          <div className="bg-white rounded-3xl shadow-2xl w-full max-w-md overflow-hidden border border-gray-100">
            <div className="flex items-center justify-between px-6 py-4 border-b border-gray-50">
              <h3 className="text-gray-900" style={{ fontSize: "17px", fontWeight: 700 }}>
                Lý Do Khóa Bài Đăng
              </h3>
              <button onClick={() => setShowRejectModal(null)} className="text-gray-400 hover:text-gray-600">
                <X className="w-5 h-5" />
              </button>
            </div>
            <div className="p-6 space-y-4">
              <textarea
                value={rejectReason}
                onChange={(e) => setRejectReason(e.target.value)}
                rows={4}
                placeholder="Ví dụ: Tin đăng chứa nội dung quảng cáo spam, giá thuê không hợp lệ hoặc thông tin sai lệch..."
                className="w-full px-4 py-2.5 rounded-2xl border border-gray-200 focus:border-red-500 focus:outline-none placeholder-gray-400 resize-none text-gray-700"
                style={{ fontSize: "14px" }}
              />
              <div className="flex gap-2">
                <button
                  onClick={() => setShowRejectModal(null)}
                  className="flex-1 py-2.5 rounded-xl border border-gray-200 text-gray-700 hover:bg-gray-50 transition-colors font-semibold"
                  style={{ fontSize: "13px" }}
                >
                  Hủy bỏ
                </button>
                <button
                  onClick={() => void handleRevokeApproval(showRejectModal)}
                  className="flex-1 py-2.5 rounded-xl bg-red-600 hover:bg-red-700 text-white transition-colors font-semibold"
                  style={{ fontSize: "13px" }}
                >
                  Xác nhận khóa
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

// Inline helper components
function SummaryCard({ label, value, tone }: { label: string; value: string; tone: "blue" | "green" | "purple" | "amber" }) {
  const palette = {
    blue: "bg-blue-50 border-blue-100 text-blue-600",
    green: "bg-green-50 border-green-100 text-green-600",
    purple: "bg-purple-50 border-purple-100 text-purple-600",
    amber: "bg-amber-50 border-amber-100 text-amber-600",
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

function InfoCard({ icon: Icon, label, value }: { icon: React.ElementType; label: string; value: string }) {
  return (
    <div className="rounded-2xl border border-gray-100 bg-gray-50 px-4 py-3">
      <div className="flex items-center gap-1.5 text-gray-400 mb-1">
        <Icon className="w-4 h-4 text-orange-500" />
        <span style={{ fontSize: "11px", fontWeight: 600 }}>{label}</span>
      </div>
      <p className="text-gray-900" style={{ fontSize: "13px", fontWeight: 700 }}>
        {value}
      </p>
    </div>
  );
}
