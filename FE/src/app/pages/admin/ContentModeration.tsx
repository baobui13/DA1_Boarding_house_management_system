import { useEffect, useMemo, useState } from "react";
import { ShieldCheck, CheckCircle2, XCircle, Clock, Eye, MapPin, X } from "lucide-react";
import { useApp } from "../../context/AppContext";
import { getPropertyListings, updateProperty } from "../../lib/properties";
import type { PropertyListing } from "../../lib/types";
import { formatCurrency } from "../../lib/format";

type StatusFilter = "all" | "pendingapproval" | "approved" | "rejected";

export default function ContentModeration() {
  const { token } = useApp();
  const [posts, setPosts] = useState<PropertyListing[]>([]);
  const [selectedPost, setSelectedPost] = useState<string | null>(null);
  const [rejectReason, setRejectReason] = useState("");
  const [showRejectModal, setShowRejectModal] = useState<string | null>(null);
  const [statusFilter, setStatusFilter] = useState<StatusFilter>("all");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const loadPosts = async () => {
    setLoading(true);
    setError("");
    try {
      const response = await getPropertyListings();
      setPosts(response.items);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Không tải được danh sách tin.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadPosts();
  }, []);

  const filtered = useMemo(
    () =>
      posts.filter((item) => statusFilter === "all" || item.status.toLowerCase() === statusFilter),
    [posts, statusFilter],
  );

  const handleApprove = async (id: string) => {
    if (!token) {
      setError("Thiếu token admin để duyệt bài.");
      return;
    }
    try {
      await updateProperty(token, { id, status: "Approved", rejectionReason: "" });
      await loadPosts();
      if (selectedPost === id) setSelectedPost(null);
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
      await updateProperty(token, { id, status: "Rejected", rejectionReason: rejectReason || "Nội dung không đạt yêu cầu." });
      setShowRejectModal(null);
      setRejectReason("");
      await loadPosts();
      if (selectedPost === id) setSelectedPost(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Từ chối thất bại.");
    }
  };

  const pendingCount = posts.filter((item) => item.status.toLowerCase() === "pendingapproval").length;
  const approvedCount = posts.filter((item) => item.status.toLowerCase() === "approved").length;
  const rejectedCount = posts.filter((item) => item.status.toLowerCase() === "rejected").length;
  const viewedPost = posts.find((item) => item.id === selectedPost);

  return (
    <div className="max-w-6xl mx-auto px-4 py-6">
      <div className="mb-6">
        <h1 className="text-gray-900" style={{ fontSize: "22px", fontWeight: 700 }}>
          Kiểm Duyệt Tin Đăng
        </h1>
        <p className="text-gray-500 mt-0.5" style={{ fontSize: "14px" }}>
          Trang này đã được nối với `Property` thật qua `status` và `rejectionReason`.
        </p>
      </div>

      {error && <div className="mb-4 rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-red-600">{error}</div>}

      <div className="grid grid-cols-3 gap-4 mb-6">
        <StatCard tone="yellow" icon={Clock} value={String(pendingCount)} label="Chờ duyệt" />
        <StatCard tone="green" icon={CheckCircle2} value={String(approvedCount)} label="Đã duyệt" />
        <StatCard tone="red" icon={XCircle} value={String(rejectedCount)} label="Từ chối" />
      </div>

      <div className="flex gap-2 mb-4">
        {(["all", "pendingapproval", "approved", "rejected"] as const).map((status) => (
          <button
            key={status}
            onClick={() => setStatusFilter(status)}
            className={`px-3 py-1.5 rounded-lg border transition-colors ${
              statusFilter === status ? "border-orange-400 bg-orange-50 text-orange-600" : "border-gray-200 text-gray-500 hover:border-gray-300"
            }`}
            style={{ fontSize: "13px", fontWeight: statusFilter === status ? 600 : 400 }}
          >
            {status === "all" ? "Tất cả" : status}
          </button>
        ))}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="space-y-4">
          {loading ? (
            Array.from({ length: 4 }).map((_, index) => <div key={index} className="h-32 rounded-2xl bg-gray-100 animate-pulse" />)
          ) : filtered.length === 0 ? (
            <div className="text-center py-12 text-gray-400 bg-white rounded-2xl border border-gray-100">
              <ShieldCheck className="w-10 h-10 mx-auto mb-3 opacity-30" />
              <p style={{ fontSize: "14px" }}>Không có tin đăng nào</p>
            </div>
          ) : (
            filtered.map((post) => {
              const statusCfg = {
                pendingapproval: { label: "Chờ duyệt", color: "text-yellow-600 bg-yellow-100", icon: Clock },
                approved: { label: "Đã duyệt", color: "text-green-600 bg-green-100", icon: CheckCircle2 },
                rejected: { label: "Từ chối", color: "text-red-500 bg-red-100", icon: XCircle },
              }[post.status.toLowerCase() as Exclude<StatusFilter, "all">] || { label: post.status, color: "text-gray-500 bg-gray-100", icon: Clock };
              const StatusIcon = statusCfg.icon;
              const isSelected = selectedPost === post.id;

              return (
                <div
                  key={post.id}
                  className={`bg-white rounded-2xl border overflow-hidden transition-all cursor-pointer ${
                    isSelected ? "border-orange-400 shadow-md" : "border-gray-100 hover:border-gray-200"
                  }`}
                  onClick={() => setSelectedPost(isSelected ? null : post.id)}
                >
                  <div className="flex gap-3 p-4">
                    <img
                      src={post.images[0] || "https://placehold.co/240x180?text=No+Image"}
                      alt=""
                      className="w-24 h-18 rounded-xl object-cover shrink-0"
                      style={{ height: "72px" }}
                    />
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

                  {post.status.toLowerCase() === "pendingapproval" && (
                    <div className="flex gap-2 px-4 pb-4">
                      <button
                        onClick={(e) => {
                          e.stopPropagation();
                          void handleApprove(post.id);
                        }}
                        className="flex-1 flex items-center justify-center gap-1.5 py-2 bg-green-500 text-white rounded-xl hover:bg-green-600 transition-colors"
                        style={{ fontSize: "13px", fontWeight: 600 }}
                      >
                        <CheckCircle2 className="w-4 h-4" />
                        Phê duyệt
                      </button>
                      <button
                        onClick={(e) => {
                          e.stopPropagation();
                          setShowRejectModal(post.id);
                        }}
                        className="flex-1 flex items-center justify-center gap-1.5 py-2 bg-red-50 text-red-500 border border-red-200 rounded-xl hover:bg-red-100 transition-colors"
                        style={{ fontSize: "13px", fontWeight: 600 }}
                      >
                        <XCircle className="w-4 h-4" />
                        Từ chối
                      </button>
                    </div>
                  )}
                </div>
              );
            })
          )}
        </div>

        <div className="hidden lg:block">
          {viewedPost ? (
            <div className="bg-white rounded-2xl border border-gray-100 p-5 sticky top-20">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-gray-900" style={{ fontSize: "16px", fontWeight: 700 }}>
                  Chi tiết tin đăng
                </h3>
                <button onClick={() => setSelectedPost(null)} className="text-gray-400 hover:text-gray-600">
                  <X className="w-4 h-4" />
                </button>
              </div>

              <img src={viewedPost.images[0] || "https://placehold.co/600x400?text=No+Image"} alt="" className="w-full rounded-xl object-cover mb-4" style={{ height: "200px" }} />
              <h4 className="text-gray-900 mb-2" style={{ fontSize: "15px", fontWeight: 700 }}>{viewedPost.propertyName}</h4>
              <div className="space-y-2 mb-4" style={{ fontSize: "13px" }}>
                <div className="flex justify-between"><span className="text-gray-400">Địa chỉ</span><span className="text-gray-700 font-medium text-right max-w-[200px]">{viewedPost.address || "Chưa có"}</span></div>
                <div className="flex justify-between"><span className="text-gray-400">Giá thuê</span><span className="text-orange-600 font-semibold">{formatCurrency(viewedPost.price)}/tháng</span></div>
                <div className="flex justify-between"><span className="text-gray-400">Status</span><span className="text-gray-700 font-medium">{viewedPost.status}</span></div>
              </div>
              <div className="bg-gray-50 rounded-xl p-4">
                <p className="text-gray-500 mb-2" style={{ fontSize: "12px", fontWeight: 600 }}>MÔ TẢ</p>
                <p className="text-gray-700" style={{ fontSize: "13px", lineHeight: 1.6 }}>
                  {viewedPost.description || "Backend chưa có mô tả."}
                </p>
                {viewedPost.rejectionReason && (
                  <div className="mt-3 rounded-lg bg-red-50 p-3">
                    <p className="text-red-600" style={{ fontSize: "12px", fontWeight: 600 }}>Lý do từ chối</p>
                    <p className="text-red-500 mt-1" style={{ fontSize: "12px" }}>{viewedPost.rejectionReason}</p>
                  </div>
                )}
              </div>
            </div>
          ) : (
            <div className="bg-white rounded-2xl border border-gray-100 p-8 text-center text-gray-400 sticky top-20">
              <Eye className="w-10 h-10 mx-auto mb-3 opacity-30" />
              <p style={{ fontSize: "14px" }}>Chọn một tin để xem chi tiết</p>
            </div>
          )}
        </div>
      </div>

      {showRejectModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
          <div className="bg-white rounded-2xl shadow-2xl w-full max-w-md">
            <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100">
              <h3 className="text-gray-900" style={{ fontSize: "17px", fontWeight: 700 }}>Lý do từ chối</h3>
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

function StatCard({ tone, icon: Icon, value, label }: { tone: "yellow" | "green" | "red"; icon: React.ElementType; value: string; label: string }) {
  const styles = {
    yellow: "bg-yellow-50 border-yellow-200 text-yellow-700 text-yellow-600 bg-yellow-100",
    green: "bg-green-50 border-green-100 text-green-700 text-green-600 bg-green-100",
    red: "bg-red-50 border-red-100 text-red-600 text-red-500 bg-red-100",
  }[tone].split(" ");
  return (
    <div className={`${styles[0]} border ${styles[1]} rounded-2xl p-4 flex items-center gap-3`}>
      <div className={`w-10 h-10 rounded-xl ${styles[4]} flex items-center justify-center`}>
        <Icon className={`w-5 h-5 ${styles[3]}`} />
      </div>
      <div>
        <p className={styles[2]} style={{ fontSize: "22px", fontWeight: 700 }}>{value}</p>
        <p className={styles[3]} style={{ fontSize: "12px" }}>{label}</p>
      </div>
    </div>
  );
}
