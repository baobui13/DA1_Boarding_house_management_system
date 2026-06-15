import { useState, useEffect } from "react";
import { Star, MessageSquare, Search } from "lucide-react";
import { getRatings, getRatingDetailById, type RatingResponse, type RatingDetailResponse } from "../../lib/ratings";
import { useApp } from "../../context/AppContext";
import { Link } from "react-router";

export default function RatingManagement() {
  const { token } = useApp();
  const [ratings, setRatings] = useState<RatingResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [search, setSearch] = useState("");
  const [attitudeFilter, setAttitudeFilter] = useState("all");

  const [selectedRatingId, setSelectedRatingId] = useState<string | null>(null);
  const [selectedRatingDetail, setSelectedRatingDetail] = useState<RatingDetailResponse | null>(null);
  const [detailLoading, setDetailLoading] = useState(false);

  const loadData = async () => {
    if (!token) return;
    setLoading(true);
    try {
      const response = await getRatings({}, token);
      setRatings(response.items);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Không tải được danh sách đánh giá.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadData();
  }, [token]);

  useEffect(() => {
    if (!selectedRatingId) {
      setSelectedRatingDetail(null);
      return;
    }

    let cancelled = false;
    const loadDetail = async () => {
      setDetailLoading(true);
      try {
        const detail = await getRatingDetailById(selectedRatingId);
        if (!cancelled) {
          setSelectedRatingDetail(detail);
        }
      } catch (err) {
        if (!cancelled) {
          console.error("Failed to load rating details", err);
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
  }, [selectedRatingId]);

  const filteredRatings = ratings.filter((r) => {
    const matchesFilter = attitudeFilter === "all" || (r.aiAttitude?.toLowerCase() || "") === attitudeFilter.toLowerCase();
    const matchesSearch =
      r.content.toLowerCase().includes(search.toLowerCase()) ||
      r.propertyId.toLowerCase().includes(search.toLowerCase()) ||
      r.tenantId.toLowerCase().includes(search.toLowerCase());
    return matchesFilter && matchesSearch;
  }).sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());

  return (
    <div className="max-w-7xl mx-auto px-4 py-6 space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-gray-900" style={{ fontSize: "24px", fontWeight: 700 }}>
            Quản Lý Đánh Giá
          </h1>
          <p className="text-gray-500 mt-1" style={{ fontSize: "14px" }}>
            Theo dõi đánh giá từ người thuê
          </p>
        </div>
      </div>

      <div className="flex flex-col sm:flex-row gap-4">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
          <input
            type="text"
            placeholder="Tìm kiếm theo nội dung, ID phòng, người dùng..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full pl-10 pr-4 py-2.5 rounded-xl border border-gray-200 focus:outline-none focus:ring-2 focus:ring-orange-500/20 focus:border-orange-500"
          />
        </div>
        <select
          value={attitudeFilter}
          onChange={(e) => setAttitudeFilter(e.target.value)}
          className="px-4 py-2.5 rounded-xl border border-gray-200 focus:outline-none focus:ring-2 focus:ring-orange-500/20 focus:border-orange-500 bg-white"
        >
          <option value="all">Tất cả thái độ</option>
          <option value="positive">Tích cực</option>
          <option value="neutral">Bình thường</option>
          <option value="negative">Tiêu cực</option>
        </select>
      </div>

      {error && (
        <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-red-600 text-sm">
          {error}
        </div>
      )}

      {loading ? (
        <div className="text-center py-12 text-gray-400">Đang tải dữ liệu...</div>
      ) : filteredRatings.length === 0 ? (
        <div className="text-center py-12 bg-white rounded-2xl border border-dashed border-gray-200">
          <MessageSquare className="w-12 h-12 mx-auto text-gray-300 mb-3" />
          <p className="text-gray-500">Không tìm thấy đánh giá nào.</p>
        </div>
      ) : (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {filteredRatings.map((rating) => (
            <div 
              key={rating.id} 
              onClick={() => setSelectedRatingId(rating.id)}
              className="bg-white rounded-2xl border border-gray-100 p-5 shadow-sm cursor-pointer hover:border-orange-300 transition-colors"
            >
              <div className="flex justify-between items-start mb-3">
                <div className="flex text-orange-400">
                  {Array.from({ length: 5 }).map((_, i) => (
                    <Star key={i} className={`w-4 h-4 ${i < rating.stars ? "fill-current" : "text-gray-200"}`} />
                  ))}
                </div>
                <span className="text-xs text-gray-400">
                  {new Date(rating.createdAt).toLocaleDateString("vi-VN")}
                </span>
              </div>
              
              <p className="text-gray-700 text-sm leading-relaxed mb-4 line-clamp-3">{rating.content}</p>
              
              <div className="space-y-2 pt-4 border-t border-gray-50 text-xs text-gray-500">
                <div className="flex items-center justify-between">
                  <span>Phòng:</span>
                  <Link to={`/rooms/${rating.propertyId}`} target="_blank" className="font-semibold text-blue-500 hover:underline truncate ml-2 max-w-[150px]">
                    {rating.propertyId.slice(0, 8)}...
                  </Link>
                </div>
                <div className="flex items-center justify-between">
                  <span>Người thuê:</span>
                  <span className="font-semibold text-gray-700 truncate ml-2 max-w-[150px]">{rating.tenantId.slice(0, 8)}...</span>
                </div>
                <div className="flex items-center justify-between">
                  <span>Phân tích thái độ:</span>
                  <span className={`px-2 py-0.5 rounded-full font-medium ${
                    (rating.aiAttitude || "").toLowerCase() === "positive" ? "bg-green-100 text-green-700" :
                    (rating.aiAttitude || "").toLowerCase() === "negative" ? "bg-red-100 text-red-700" :
                    "bg-gray-100 text-gray-700"
                  }`}>
                    {rating.aiAttitude || "Chưa phân tích"}
                  </span>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Modal Details */}
      {selectedRatingId && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/40 backdrop-blur-sm animate-fade-in">
          <div className="bg-white rounded-3xl shadow-2xl w-full max-w-2xl max-h-[90vh] flex flex-col overflow-hidden border border-gray-100">
            <div className="flex items-center justify-between px-6 py-4 border-b border-gray-50">
              <h3 className="text-gray-900" style={{ fontSize: "18px", fontWeight: 700 }}>
                Chi Tiết Đánh Giá
              </h3>
              <button onClick={() => setSelectedRatingId(null)} className="text-gray-400 hover:text-gray-600 transition-colors p-1 rounded-lg hover:bg-gray-100">
                <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M18 6 6 18"/><path d="m6 6 12 12"/></svg>
              </button>
            </div>
            
            <div className="p-6 overflow-y-auto">
              {detailLoading ? (
                <div className="flex justify-center py-12">
                  <div className="w-8 h-8 border-4 border-orange-500 border-t-transparent rounded-full animate-spin"></div>
                </div>
              ) : selectedRatingDetail ? (
                <div className="space-y-6">
                  {/* Rating Content Section */}
                  <div className="bg-orange-50/50 rounded-2xl p-5 border border-orange-100/50">
                    <div className="flex justify-between items-center mb-3">
                      <div className="flex text-orange-500">
                        {Array.from({ length: 5 }).map((_, i) => (
                          <Star key={i} className={`w-5 h-5 ${i < selectedRatingDetail.stars ? "fill-current" : "text-gray-200"}`} />
                        ))}
                      </div>
                      <span className={`px-2.5 py-1 rounded-full font-bold text-xs ${
                        (selectedRatingDetail.aiAttitude || "").toLowerCase() === "positive" ? "bg-green-100 text-green-700" :
                        (selectedRatingDetail.aiAttitude || "").toLowerCase() === "negative" ? "bg-red-100 text-red-700" :
                        "bg-gray-100 text-gray-700"
                      }`}>
                        AI: {selectedRatingDetail.aiAttitude || "Chưa phân tích"}
                      </span>
                    </div>
                    <p className="text-gray-800 text-sm leading-relaxed whitespace-pre-wrap">
                      "{selectedRatingDetail.content}"
                    </p>
                    <p className="text-gray-400 text-xs mt-3 font-medium">
                      Đăng lúc {new Date(selectedRatingDetail.createdAt).toLocaleString("vi-VN")}
                    </p>
                  </div>

                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    {/* Tenant Details */}
                    <div className="border border-gray-100 rounded-2xl p-4">
                      <h4 className="text-xs font-bold text-gray-400 mb-3 uppercase tracking-wider">Người Đánh Giá</h4>
                      {selectedRatingDetail.tenant ? (
                        <div className="flex items-center gap-3">
                          <img
                            src={selectedRatingDetail.tenant.avatarUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(selectedRatingDetail.tenant.fullName)}&background=f97316&color=fff`}
                            alt=""
                            className="w-12 h-12 rounded-full object-cover shadow-sm"
                          />
                          <div className="min-w-0">
                            <p className="text-gray-900 font-semibold text-sm truncate">{selectedRatingDetail.tenant.fullName}</p>
                            <p className="text-gray-500 text-xs truncate">{selectedRatingDetail.tenant.email}</p>
                            {selectedRatingDetail.tenant.phoneNumber && (
                              <p className="text-orange-600 text-xs font-medium truncate mt-0.5">{selectedRatingDetail.tenant.phoneNumber}</p>
                            )}
                          </div>
                        </div>
                      ) : (
                        <p className="text-gray-500 text-sm italic">Không tìm thấy thông tin</p>
                      )}
                    </div>

                    {/* Property Details */}
                    <div className="border border-gray-100 rounded-2xl p-4">
                      <h4 className="text-xs font-bold text-gray-400 mb-3 uppercase tracking-wider">Phòng / Căn Hộ</h4>
                      {selectedRatingDetail.property ? (
                        <div className="flex items-start gap-3">
                          <img
                            src={selectedRatingDetail.property.images?.[0] || "https://placehold.co/400x300?text=No+Image"}
                            alt=""
                            className="w-16 h-16 rounded-xl object-cover shadow-sm shrink-0"
                          />
                          <div className="min-w-0">
                            <p className="text-gray-900 font-semibold text-sm line-clamp-2">{selectedRatingDetail.property.propertyName}</p>
                            <p className="text-gray-500 text-xs truncate mt-0.5">{selectedRatingDetail.property.address}</p>
                            <p className="text-orange-600 text-xs font-bold mt-1">
                              {new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(selectedRatingDetail.property.price)}/tháng
                            </p>
                          </div>
                        </div>
                      ) : (
                        <p className="text-gray-500 text-sm italic">Không tìm thấy thông tin phòng</p>
                      )}
                    </div>
                  </div>
                </div>
              ) : (
                <div className="text-center py-12 text-gray-500">
                  Không thể tải chi tiết đánh giá.
                </div>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
