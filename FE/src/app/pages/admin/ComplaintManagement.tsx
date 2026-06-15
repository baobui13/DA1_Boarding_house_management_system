import { useState, useEffect } from "react";
import { AlertCircle, CheckCircle2, Clock, MessageSquare, Search } from "lucide-react";
import { getComplaints, updateComplaint, type ComplaintResponse } from "../../lib/complaints";
import { useApp } from "../../context/AppContext";

export default function ComplaintManagement() {
  const { token } = useApp();
  const [complaints, setComplaints] = useState<ComplaintResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [filter, setFilter] = useState("all");
  const [search, setSearch] = useState("");
  const [resolvingId, setResolvingId] = useState<string | null>(null);
  const [resolutionNote, setResolutionNote] = useState("");

  const loadData = async () => {
    if (!token) return;
    setLoading(true);
    try {
      const response = await getComplaints({}, token);
      setComplaints(response.items);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Không tải được danh sách khiếu nại.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadData();
  }, [token]);

  const handleResolve = async (id: string) => {
    if (!token) return;
    try {
      await updateComplaint(id, token, {
        status: "Resolved",
        resolutionNote,
      });
      setResolvingId(null);
      setResolutionNote("");
      void loadData();
    } catch (err) {
      alert(err instanceof Error ? err.message : "Đã xảy ra lỗi khi cập nhật.");
    }
  };

  const filteredComplaints = complaints.filter((c) => {
    const matchesFilter = filter === "all" || c.status.toLowerCase() === filter.toLowerCase();
    const matchesSearch =
      c.title.toLowerCase().includes(search.toLowerCase()) ||
      c.relatedId.toLowerCase().includes(search.toLowerCase()) ||
      c.creatorId.toLowerCase().includes(search.toLowerCase());
    return matchesFilter && matchesSearch;
  }).sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());

  return (
    <div className="max-w-7xl mx-auto px-4 py-6 space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-gray-900" style={{ fontSize: "24px", fontWeight: 700 }}>
            Quản Lý Khiếu Nại
          </h1>
          <p className="text-gray-500 mt-1" style={{ fontSize: "14px" }}>
            Theo dõi và giải quyết khiếu nại từ người dùng
          </p>
        </div>
      </div>

      <div className="flex flex-col sm:flex-row gap-4">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
          <input
            type="text"
            placeholder="Tìm kiếm theo tiêu đề, ID..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full pl-10 pr-4 py-2.5 rounded-xl border border-gray-200 focus:outline-none focus:ring-2 focus:ring-orange-500/20 focus:border-orange-500"
          />
        </div>
        <select
          value={filter}
          onChange={(e) => setFilter(e.target.value)}
          className="px-4 py-2.5 rounded-xl border border-gray-200 focus:outline-none focus:ring-2 focus:ring-orange-500/20 focus:border-orange-500 bg-white"
        >
          <option value="all">Tất cả trạng thái</option>
          <option value="pending">Chờ xử lý</option>
          <option value="processing">Đang xử lý</option>
          <option value="resolved">Đã giải quyết</option>
        </select>
      </div>

      {error && (
        <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-red-600 text-sm">
          {error}
        </div>
      )}

      {loading ? (
        <div className="text-center py-12 text-gray-400">Đang tải dữ liệu...</div>
      ) : filteredComplaints.length === 0 ? (
        <div className="text-center py-12 bg-white rounded-2xl border border-dashed border-gray-200">
          <MessageSquare className="w-12 h-12 mx-auto text-gray-300 mb-3" />
          <p className="text-gray-500">Không tìm thấy khiếu nại nào.</p>
        </div>
      ) : (
        <div className="grid gap-4">
          {filteredComplaints.map((complaint) => (
            <div key={complaint.id} className="bg-white rounded-2xl border border-gray-100 p-5 shadow-sm">
              <div className="flex flex-col lg:flex-row gap-4 justify-between">
                <div className="flex-1">
                  <div className="flex items-center gap-3 mb-2">
                    <span
                      className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold ${
                        complaint.status.toLowerCase() === "resolved"
                          ? "bg-green-50 text-green-700"
                          : complaint.status.toLowerCase() === "processing"
                          ? "bg-blue-50 text-blue-700"
                          : "bg-yellow-50 text-yellow-700"
                      }`}
                    >
                      {complaint.status.toLowerCase() === "resolved" ? (
                        <CheckCircle2 className="w-3.5 h-3.5" />
                      ) : complaint.status.toLowerCase() === "processing" ? (
                        <Clock className="w-3.5 h-3.5" />
                      ) : (
                        <AlertCircle className="w-3.5 h-3.5" />
                      )}
                      {complaint.status}
                    </span>
                    <span className="text-xs text-gray-400">
                      {new Date(complaint.createdAt).toLocaleString("vi-VN")}
                    </span>
                  </div>
                  
                  <h3 className="text-gray-900 font-bold text-lg">{complaint.title}</h3>
                  <div className="flex items-center gap-4 mt-2 text-sm text-gray-500">
                    <span>
                      <strong className="text-gray-700">Người gửi:</strong> {complaint.creatorId}
                    </span>
                    <span>
                      <strong className="text-gray-700">Liên quan:</strong> {complaint.relatedType} ({complaint.relatedId})
                    </span>
                  </div>
                  <p className="text-gray-600 mt-3">{complaint.content}</p>

                  {complaint.resolutionNote && (
                    <div className="mt-4 bg-gray-50 rounded-xl p-4 border border-gray-100">
                      <p className="text-sm text-gray-700 font-semibold mb-1">Ghi chú giải quyết:</p>
                      <p className="text-sm text-gray-600">{complaint.resolutionNote}</p>
                    </div>
                  )}
                </div>

                <div className="lg:w-72 lg:border-l lg:border-gray-100 lg:pl-6 flex flex-col justify-center">
                  {complaint.status.toLowerCase() !== "resolved" ? (
                    resolvingId === complaint.id ? (
                      <div className="space-y-3">
                        <textarea
                          value={resolutionNote}
                          onChange={(e) => setResolutionNote(e.target.value)}
                          placeholder="Nhập ghi chú giải quyết..."
                          className="w-full px-3 py-2 text-sm rounded-lg border border-gray-200 focus:outline-none resize-none"
                          rows={3}
                        />
                        <div className="flex gap-2">
                          <button
                            onClick={() => {
                              setResolvingId(null);
                              setResolutionNote("");
                            }}
                            className="flex-1 py-2 rounded-lg border border-gray-200 text-gray-600 hover:bg-gray-50 text-sm font-semibold"
                          >
                            Hủy
                          </button>
                          <button
                            onClick={() => handleResolve(complaint.id)}
                            className="flex-1 py-2 rounded-lg bg-green-500 text-white hover:bg-green-600 text-sm font-semibold"
                          >
                            Lưu
                          </button>
                        </div>
                      </div>
                    ) : (
                      <button
                        onClick={() => setResolvingId(complaint.id)}
                        className="w-full py-2.5 rounded-xl bg-orange-50 text-orange-600 hover:bg-orange-100 font-semibold text-sm transition-colors"
                      >
                        Đánh dấu đã giải quyết
                      </button>
                    )
                  ) : (
                    <div className="text-center text-sm font-medium text-green-600 bg-green-50 rounded-xl py-3 border border-green-100">
                      Khiếu nại đã được giải quyết
                    </div>
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
