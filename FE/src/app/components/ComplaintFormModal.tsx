import { useState } from "react";
import { X } from "lucide-react";
import { createComplaint } from "../lib/complaints";
import { useApp } from "../context/AppContext";

interface ComplaintFormModalProps {
  defaultRelatedType?: string;
  defaultRelatedId?: string;
  onClose: () => void;
  onSuccess: () => void;
}

export function ComplaintFormModal({
  defaultRelatedType = "Property",
  defaultRelatedId = "",
  onClose,
  onSuccess,
}: ComplaintFormModalProps) {
  const { token, currentUser } = useApp();
  const [relatedType, setRelatedType] = useState(defaultRelatedType);
  const [relatedId, setRelatedId] = useState(defaultRelatedId);
  const [title, setTitle] = useState("");
  const [content, setContent] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!token || !currentUser) return;

    if (!relatedId.trim()) {
      setError("Vui lòng nhập ID liên quan.");
      return;
    }
    if (!title.trim() || !content.trim()) {
      setError("Vui lòng nhập tiêu đề và nội dung.");
      return;
    }

    setSubmitting(true);
    setError("");

    try {
      await createComplaint(token, {
        creatorId: currentUser.id,
        relatedType,
        relatedId: relatedId.trim(),
        title: title.trim(),
        content: content.trim(),
      });
      onSuccess();
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Đã xảy ra lỗi khi gửi khiếu nại.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-md overflow-hidden">
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100">
          <h3 className="text-gray-900" style={{ fontSize: "17px", fontWeight: 700 }}>
            Tạo khiếu nại mới
          </h3>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
            <X className="w-5 h-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Loại khiếu nại</label>
            <select
              value={relatedType}
              onChange={(e) => setRelatedType(e.target.value)}
              className="w-full px-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none"
            >
              <option value="Property">Phòng trọ</option>
              <option value="Contract">Hợp đồng</option>
              <option value="Invoice">Hóa đơn</option>
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Mã liên quan ({relatedType === "Property" ? "ID Phòng" : relatedType === "Contract" ? "ID Hợp đồng" : "ID Hóa đơn"})
            </label>
            <input
              type="text"
              value={relatedId}
              onChange={(e) => setRelatedId(e.target.value)}
              placeholder="Nhập ID..."
              className="w-full px-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Tiêu đề</label>
            <input
              type="text"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              placeholder="Tóm tắt vấn đề..."
              className="w-full px-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Nội dung chi tiết</label>
            <textarea
              value={content}
              onChange={(e) => setContent(e.target.value)}
              rows={4}
              placeholder="Mô tả chi tiết vấn đề bạn gặp phải..."
              className="w-full px-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none resize-none"
            />
          </div>

          {error && (
            <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-red-600 text-sm">
              {error}
            </div>
          )}

          <div className="flex gap-3 pt-2">
            <button
              type="button"
              onClick={onClose}
              className="flex-1 py-3 rounded-xl border border-gray-200 text-gray-600 hover:bg-gray-50 transition-colors"
              style={{ fontSize: "14px" }}
            >
              Hủy
            </button>
            <button
              type="submit"
              disabled={submitting}
              className="flex-1 py-3 rounded-xl bg-orange-500 text-white hover:bg-orange-600 transition-colors disabled:opacity-60"
              style={{ fontSize: "14px", fontWeight: 600 }}
            >
              {submitting ? "Đang gửi..." : "Gửi khiếu nại"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
