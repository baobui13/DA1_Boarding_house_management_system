import { useEffect, useState } from "react";
import { ShieldCheck, CheckCircle2, XCircle, Clock, Eye, MapPin, X, Home, Ruler, Image as ImageIcon } from "lucide-react";
import { useApp } from "../../context/AppContext";
import { approveProperty, getModerationProperties, getPropertyListing, rejectProperty } from "../../lib/properties";
import type { PropertyListing, PropertyResponse } from "../../lib/types";
import { formatCurrency } from "../../lib/format";

type StatusFilter = "pendingapproval" | "rejected";

const PAGE_SIZE = 8;

export default function ContentModeration() {
  const { token } = useApp();
  const [posts, setPosts] = useState<PropertyResponse[]>([]);
  const [selectedPostId, setSelectedPostId] = useState<string | null>(null);
  const [selectedPostDetail, setSelectedPostDetail] = useState<PropertyListing | null>(null);
  const [rejectReason, setRejectReason] = useState("");
  const [showRejectModal, setShowRejectModal] = useState<string | null>(null);
  const [statusFilter, setStatusFilter] = useState<StatusFilter>("pendingapproval");
  const [pageNumber, setPageNumber] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [detailLoading, setDetailLoading] = useState(false);
  const [error, setError] = useState("");

  const normalizeModerationStatus = (status?: string | null) => {
    const normalized = status?.trim().toLowerCase() || "";
    return normalized === "pending" ? "pendingapproval" : normalized;
  };

  const loadPosts = async () => {
    if (!token) {
      setError("Thiếu token admin để tải danh sách kiểm duyệt.");
      setPosts([]);
      setTotalCount(0);
      setLoading(false);
      return;
    }

    setLoading(true);
    setError("");
    try {
      const response = await getModerationProperties(token, {
        status: statusFilter === "pendingapproval" ? "Pending" : "Rejected",
        pageNumber,
        pageSize: PAGE_SIZE,
      });
      setPosts(response.items);
      setTotalCount(response.totalCount);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Không tải được danh sách tin.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadPosts();
  }, [pageNumber, statusFilter, token]);

  useEffect(() => {
    if (!selectedPostId) {
      setSelectedPostDetail(null);
      return;
    }

    let cancelled = false;

    const loadDetail = async () => {
      setDetailLoading(true);
      setError("");
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

  const handleApprove = async (id: string) => {
    if (!token) {
      setError("Thiếu token admin để duyệt bài.");
      return;
    }

    try {
      await approveProperty(token, id);
      setPosts((current) => current.filter((post) => post.id !== id));
      setTotalCount((current) => Math.max(0, current - 1));
      if (selectedPostId === id) {
        setSelectedPostId(null);
        setSelectedPostDetail(null);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Phê duyệt thất bại.");
    }
  };

  const handleReject = async (id: string) => {
    if (!token) {
      setError("Thiếu token admin để từ chối bài.");
      return;
    }

    try {
      await rejectProperty(token, id, rejectReason || "Nội dung không đạt yêu cầu.");

      setShowRejectModal(null);
      setRejectReason("");

      if (statusFilter === "pendingapproval") {
        setPosts((current) => current.filter((post) => post.id !== id));
        setTotalCount((current) => Math.max(0, current - 1));
        if (selectedPostId === id) {
          setSelectedPostId(null);
          setSelectedPostDetail(null);
        }
      } else {
        await loadPosts();
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Từ chối thất bại.");
    }
  };

  const pageCount = Math.max(1, Math.ceil(totalCount / PAGE_SIZE));
  const changePage = (nextPage: number) => {
    if (nextPage === pageNumber) {
      return;
    }

    setPageNumber(nextPage);
    window.scrollTo({ top: 0, behavior: "smooth" });
  };
  const viewedPost = selectedPostDetail;
  const viewedStatus = viewedPost
    ? {
        pendingapproval: { label: "Chờ duyệt", color: "text-yellow-600 bg-yellow-100", icon: Clock },
        rejected: { label: "Từ chối", color: "text-red-500 bg-red-100", icon: XCircle },
      }[normalizeModerationStatus(viewedPost.moderationStatus) as StatusFilter] || {
        label: viewedPost.moderationStatus,
        color: "text-gray-500 bg-gray-100",
        icon: Clock,
      }
    : null;

  return (
    <div className="max-w-7xl mx-auto px-4 py-6">
      <div className="mb-6">
        <h1 className="text-gray-900" style={{ fontSize: "24px", fontWeight: 700 }}>
          Kiểm Duyệt Tin Đăng
        </h1>
        <p className="text-gray-500 mt-1" style={{ fontSize: "14px" }}>
          Xem xét và phê duyệt các tin đăng phòng trọ chờ kiểm duyệt
        </p>
      </div>

      {error && <div className="mb-4 rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-red-600">{error}</div>}

      <div className="grid grid-cols-3 gap-4 mb-6">
        <StatCard tone="yellow" icon={Clock} value={statusFilter === "pendingapproval" ? String(totalCount) : "0"} label="Chờ duyệt" />
        <StatCard tone="red" icon={XCircle} value={statusFilter === "rejected" ? String(totalCount) : "0"} label="Từ chối" />
        <StatCard tone="slate" icon={ShieldCheck} value={String(totalCount)} label="Đang tải theo trang" />
      </div>

      <div className="flex gap-2 mb-4">
        {(["pendingapproval", "rejected"] as const).map((status) => (
          <button
            key={status}
            onClick={() => {
              setStatusFilter(status);
              setPageNumber(1);
              setSelectedPostId(null);
              setSelectedPostDetail(null);
            }}
            className={`px-3 py-1.5 rounded-xl border transition-colors ${
              statusFilter === status ? "border-orange-400 bg-orange-50 text-orange-600" : "border-gray-200 text-gray-500 hover:border-gray-300"
            }`}
            style={{ fontSize: "13px", fontWeight: statusFilter === status ? 600 : 400 }}
          >
            {status === "pendingapproval" ? "Chờ duyệt" : "Bị từ chối"}
          </button>
        ))}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="space-y-4">
          {loading ? (
            Array.from({ length: 4 }).map((_, index) => <div key={index} className="h-28 rounded-2xl bg-gray-100 animate-pulse" />)
          ) : posts.length === 0 ? (
            <div className="text-center py-12 text-gray-400 bg-white rounded-2xl border border-gray-100">
              <ShieldCheck className="w-10 h-10 mx-auto mb-3 opacity-30" />
              <p style={{ fontSize: "14px" }}>Không có tin đăng nào</p>
            </div>
          ) : (
            posts.map((post) => {
              const statusCfg = {
                pendingapproval: { label: "Chờ duyệt", color: "text-yellow-600 bg-yellow-100", icon: Clock },
                rejected: { label: "Từ chối", color: "text-red-500 bg-red-100", icon: XCircle },
              }[normalizeModerationStatus(post.moderationStatus) as StatusFilter] || {
                label: post.moderationStatus,
                color: "text-gray-500 bg-gray-100",
                icon: Clock,
              };

              const StatusIcon = statusCfg.icon;
              const isSelected = selectedPostId === post.id;

              return (
                <div
                  key={post.id}
                  className={`bg-white rounded-2xl border overflow-hidden transition-all cursor-pointer ${
                    isSelected ? "border-orange-400 shadow-md" : "border-gray-100 hover:border-gray-200"
                  }`}
                  onClick={() => setSelectedPostId(isSelected ? null : post.id)}
                >
                  <div className="flex gap-3 p-4">
                    <div className="w-24 rounded-xl bg-orange-50 text-orange-500 shrink-0 flex items-center justify-center" style={{ height: "72px" }}>
                      <Home className="w-6 h-6" />
                    </div>
                    <div className="flex-1 min-w-0">
                      <div className="flex items-start justify-between gap-2 mb-1">
                        <p className="text-gray-900 line-clamp-1" style={{ fontSize: "14px", fontWeight: 600 }}>
                          {post.propertyName}
                        </p>
                        <span className={`flex items-center gap-1 px-2 py-0.5 rounded-lg shrink-0 ${statusCfg.color}`} style={{ fontSize: "10px", fontWeight: 600 }}>
                          <StatusIcon className="w-3 h-3" />
                          {statusCfg.label}
                        </span>
                      </div>
                      <div className="flex items-center gap-1 text-gray-400 mb-1">
                        <MapPin className="w-3 h-3" />
                        <span className="truncate" style={{ fontSize: "12px" }}>
                          {post.address || "Chưa có địa chỉ"}
                        </span>
                      </div>
                      <div className="flex items-center justify-between">
                        <span className="text-orange-600" style={{ fontSize: "13px", fontWeight: 600 }}>
                          {formatCurrency(post.price)}/tháng
                        </span>
                        <span className="text-gray-400" style={{ fontSize: "11px" }}>
                          {new Date(post.createdAt).toLocaleDateString("vi-VN")}
                        </span>
                      </div>
                    </div>
                  </div>
                </div>
              );
            })
          )}

          <div className="flex items-center justify-between rounded-2xl border border-gray-100 bg-white px-4 py-3">
            <p className="text-gray-500" style={{ fontSize: "13px" }}>
              Trang {pageNumber}/{pageCount} · hiển thị {posts.length} / {totalCount} tin
            </p>
            <div className="flex gap-2">
              <button
                type="button"
                onClick={() => changePage(Math.max(1, pageNumber - 1))}
                disabled={pageNumber <= 1 || loading}
                className="rounded-lg border border-gray-200 px-3 py-1.5 text-sm text-gray-600 disabled:cursor-not-allowed disabled:opacity-40"
              >
                Trước
              </button>
              <button
                type="button"
                onClick={() => changePage(Math.min(pageCount, pageNumber + 1))}
                disabled={pageNumber >= pageCount || loading}
                className="rounded-lg border border-gray-200 px-3 py-1.5 text-sm text-gray-600 disabled:cursor-not-allowed disabled:opacity-40"
              >
                Sau
              </button>
            </div>
          </div>
        </div>

        <div className="lg:block">
          {detailLoading ? (
            <div className="bg-white rounded-2xl border border-gray-100 p-5 sticky top-20 animate-pulse">
              <div className="h-6 w-48 rounded bg-gray-100 mb-4" />
              <div className="h-56 rounded-xl bg-gray-100 mb-4" />
              <div className="grid grid-cols-3 gap-3 mb-4">
                {Array.from({ length: 3 }).map((_, index) => (
                  <div key={index} className="h-20 rounded-xl bg-gray-100" />
                ))}
              </div>
              <div className="h-32 rounded-xl bg-gray-100" />
            </div>
          ) : viewedPost ? (
            <div className="bg-white rounded-2xl border border-gray-100 p-5 sticky top-20">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-gray-900" style={{ fontSize: "16px", fontWeight: 700 }}>
                  Chi tiết tin đăng kiểm duyệt
                </h3>
                <button onClick={() => setSelectedPostId(null)} className="text-gray-400 hover:text-gray-600">
                  <X className="w-4 h-4" />
                </button>
              </div>

              <div className="flex items-start justify-between gap-3 mb-4">
                <div>
                  <h4 className="text-gray-900 mb-1" style={{ fontSize: "18px", fontWeight: 700 }}>
                    {viewedPost.propertyName}
                  </h4>
                  <div className="flex items-center gap-2 text-gray-500" style={{ fontSize: "13px" }}>
                    <MapPin className="w-4 h-4" />
                    <span>{viewedPost.address || "Chưa có địa chỉ"}</span>
                  </div>
                </div>
                {viewedStatus ? (
                  <span className={`flex items-center gap-1.5 px-3 py-1 rounded-xl shrink-0 ${viewedStatus.color}`} style={{ fontSize: "12px", fontWeight: 600 }}>
                    <viewedStatus.icon className="w-4 h-4" />
                    {viewedStatus.label}
                  </span>
                ) : null}
              </div>

              <img
                src={viewedPost.images[0] || "https://placehold.co/600x400?text=No+Image"}
                alt=""
                className="w-full rounded-xl object-cover mb-4"
                style={{ height: "220px" }}
              />

              {viewedPost.images.length > 1 && (
                <div className="grid grid-cols-4 gap-2 mb-4">
                  {viewedPost.images.slice(0, 4).map((image, index) => (
                    <img key={`${viewedPost.id}-${index}`} src={image} alt="" className="w-full h-16 rounded-lg object-cover" />
                  ))}
                </div>
              )}

              <div className="grid grid-cols-1 sm:grid-cols-3 gap-3 mb-4">
                <InfoCard icon={Home} label="Mã tin" value={viewedPost.id.slice(0, 8)} />
                <InfoCard icon={Ruler} label="Diện tích" value={`${viewedPost.size} m²`} />
                <InfoCard icon={ImageIcon} label="Số ảnh" value={`${viewedPost.images.length}`} />
              </div>

              <div className="space-y-2 mb-4" style={{ fontSize: "13px" }}>
                <div className="flex justify-between gap-4">
                  <span className="text-gray-400">Giá thuê</span>
                  <span className="text-orange-600 font-semibold text-right">{formatCurrency(viewedPost.price)}/tháng</span>
                </div>
                <div className="flex justify-between gap-4">
                  <span className="text-gray-400">Ngày đăng</span>
                  <span className="text-gray-700 font-medium text-right">{new Date(viewedPost.createdAt).toLocaleDateString("vi-VN")}</span>
                </div>
                <div className="flex justify-between gap-4">
                  <span className="text-gray-400">Trạng thái kiểm duyệt</span>
                  <span className="text-gray-700 font-medium text-right">{viewedPost.moderationStatus}</span>
                </div>
              </div>

              <div className="bg-gray-50 rounded-xl p-4">
                <p className="text-gray-500 mb-2" style={{ fontSize: "12px", fontWeight: 600 }}>
                  MÔ TẢ
                </p>
                <p className="text-gray-700" style={{ fontSize: "13px", lineHeight: 1.6 }}>
                  {viewedPost.description || "Backend chưa có mô tả."}
                </p>
                {viewedPost.amenities.length > 0 && (
                  <div className="mt-4">
                    <p className="text-gray-500 mb-2" style={{ fontSize: "12px", fontWeight: 600 }}>
                      TIỆN ÍCH
                    </p>
                    <div className="flex flex-wrap gap-2">
                      {viewedPost.amenities.map((amenity) => (
                        <span key={amenity} className="px-2.5 py-1 rounded-lg bg-white border border-gray-200 text-gray-600" style={{ fontSize: "12px" }}>
                          {amenity}
                        </span>
                      ))}
                    </div>
                  </div>
                )}
                {viewedPost.rejectionReason && (
                  <div className="mt-3 rounded-lg bg-red-50 p-3">
                    <p className="text-red-600" style={{ fontSize: "12px", fontWeight: 600 }}>
                      Lý do từ chối
                    </p>
                    <p className="text-red-500 mt-1" style={{ fontSize: "12px" }}>
                      {viewedPost.rejectionReason}
                    </p>
                  </div>
                )}
              </div>

              {normalizeModerationStatus(viewedPost.moderationStatus) === "pendingapproval" && (
                <div className="mt-4 rounded-2xl border border-orange-100 bg-orange-50/70 p-4">
                  <p className="text-gray-900 mb-3" style={{ fontSize: "14px", fontWeight: 700 }}>
                    Quyết định kiểm duyệt
                  </p>
                  <div className="flex flex-col sm:flex-row gap-3">
                    <button
                      onClick={() => void handleApprove(viewedPost.id)}
                      className="flex-1 flex items-center justify-center gap-2 py-3 bg-green-500 text-white rounded-xl hover:bg-green-600 transition-colors"
                      style={{ fontSize: "14px", fontWeight: 600 }}
                    >
                      <CheckCircle2 className="w-4 h-4" />
                      Duyệt tin đăng
                    </button>
                    <button
                      onClick={() => setShowRejectModal(viewedPost.id)}
                      className="flex-1 flex items-center justify-center gap-2 py-3 bg-red-50 text-red-500 border border-red-200 rounded-xl hover:bg-red-100 transition-colors"
                      style={{ fontSize: "14px", fontWeight: 600 }}
                    >
                      <XCircle className="w-4 h-4" />
                      Từ chối tin đăng
                    </button>
                  </div>
                </div>
              )}
            </div>
          ) : (
            <div className="bg-white rounded-2xl border border-gray-100 p-8 text-center text-gray-400 lg:sticky lg:top-20">
              <Eye className="w-10 h-10 mx-auto mb-3 opacity-30" />
              <p className="text-gray-700 mb-1" style={{ fontSize: "14px", fontWeight: 600 }}>
                Chọn một tin đăng để kiểm duyệt
              </p>
              <p style={{ fontSize: "13px" }}>
                Danh sách bên trái chỉ tải theo từng phần. Khi bấm vào một tin, hệ thống mới tải chi tiết để admin duyệt hoặc từ chối.
              </p>
            </div>
          )}
        </div>
      </div>

      {showRejectModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
          <div className="bg-white rounded-2xl shadow-2xl w-full max-w-md">
            <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100">
              <h3 className="text-gray-900" style={{ fontSize: "17px", fontWeight: 700 }}>
                Lý do từ chối
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
                placeholder="Nhập lý do từ chối..."
                className="w-full px-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none resize-none"
              />
              <div className="flex gap-3">
                <button onClick={() => setShowRejectModal(null)} className="flex-1 py-3 rounded-xl border border-gray-200 text-gray-600 hover:bg-gray-50 transition-colors" style={{ fontSize: "14px" }}>
                  Hủy
                </button>
                <button
                  onClick={() => void handleReject(showRejectModal)}
                  className="flex-1 py-3 rounded-xl bg-red-500 text-white hover:bg-red-600 transition-colors"
                  style={{ fontSize: "14px", fontWeight: 600 }}
                >
                  Xác nhận từ chối
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

function InfoCard({
  icon: Icon,
  label,
  value,
}: {
  icon: typeof Home;
  label: string;
  value: string;
}) {
  return (
    <div className="rounded-xl border border-gray-100 bg-gray-50 px-4 py-3">
      <div className="flex items-center gap-2 text-gray-400 mb-1">
        <Icon className="w-4 h-4" />
        <span style={{ fontSize: "12px", fontWeight: 600 }}>{label}</span>
      </div>
      <p className="text-gray-900" style={{ fontSize: "14px", fontWeight: 700 }}>
        {value}
      </p>
    </div>
  );
}

function StatCard({
  tone,
  icon: Icon,
  value,
  label,
}: {
  tone: "yellow" | "red" | "slate";
  icon: React.ElementType;
  value: string;
  label: string;
}) {
  const styles = {
    yellow: "bg-yellow-50 border-yellow-200 text-yellow-700 text-yellow-600 bg-yellow-100",
    red: "bg-red-50 border-red-100 text-red-600 text-red-500 bg-red-100",
    slate: "bg-slate-50 border-slate-200 text-slate-700 text-slate-600 bg-slate-100",
  }[tone].split(" ");

  return (
    <div className={`${styles[0]} border ${styles[1]} rounded-2xl p-4 flex items-center gap-3`}>
      <div className={`w-10 h-10 rounded-xl ${styles[4]} flex items-center justify-center`}>
        <Icon className={`w-5 h-5 ${styles[3]}`} />
      </div>
      <div>
        <p className={styles[2]} style={{ fontSize: "22px", fontWeight: 700 }}>
          {value}
        </p>
        <p className={styles[3]} style={{ fontSize: "12px" }}>
          {label}
        </p>
      </div>
    </div>
  );
}
