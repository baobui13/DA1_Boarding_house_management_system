import { useState } from "react";
import { X, Star } from "lucide-react";
import { createRating, updateRating, type RatingResponse } from "../lib/ratings";
import { useApp } from "../context/AppContext";

interface RatingFormModalProps {
  propertyId?: string;
  eligibleProperties?: { id: string; name: string }[];
  initialData?: RatingResponse;
  onClose: () => void;
  onSuccess: () => void;
}

export function RatingFormModal({ propertyId, eligibleProperties, initialData, onClose, onSuccess }: RatingFormModalProps) {
  const { token, currentUser } = useApp();
  const [selectedPropertyId, setSelectedPropertyId] = useState(
    initialData ? initialData.propertyId : propertyId === "SELECT" ? (eligibleProperties?.[0]?.id || "") : propertyId
  );
  const [stars, setStars] = useState(initialData?.stars || 5);
  const [content, setContent] = useState(initialData?.content || "");
  const [attitude, setAttitude] = useState(initialData?.aiAttitude || "Positive");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!token || !currentUser) return;

    if (!selectedPropertyId || selectedPropertyId === "SELECT") {
      setError("Vui lòng chọn phòng để đánh giá.");
      return;
    }

    if (!content.trim()) {
      setError("Vui lòng nhập nội dung đánh giá.");
      return;
    }

    setSubmitting(true);
    setError("");

    try {
      if (initialData) {
        await updateRating(token, {
          id: initialData.id,
          stars,
          content: content.trim(),
          aiAttitude: attitude,
        });
      } else {
        await createRating(token, {
          propertyId: selectedPropertyId!,
          tenantId: currentUser.id,
          stars,
          content: content.trim(),
          aiAttitude: attitude,
        });
      }
      onSuccess();
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Đã xảy ra lỗi khi gửi đánh giá.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-md overflow-hidden">
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100">
          <h3 className="text-gray-900" style={{ fontSize: "17px", fontWeight: 700 }}>
            {initialData ? "Sửa đánh giá" : "Đánh giá phòng"}
          </h3>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
            <X className="w-5 h-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-4">
          {!initialData && propertyId === "SELECT" && eligibleProperties && (
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Chọn phòng cần đánh giá</label>
              <select
                value={selectedPropertyId}
                onChange={(e) => setSelectedPropertyId(e.target.value)}
                className="w-full px-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none"
              >
                {eligibleProperties.length === 0 && <option value="">Bạn chưa thuê phòng nào</option>}
                {eligibleProperties.map(p => (
                  <option key={p.id} value={p.id}>{p.name || p.id}</option>
                ))}
              </select>
            </div>
          )}

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Chất lượng</label>
            <div className="flex items-center gap-2">
              {[1, 2, 3, 4, 5].map((s) => (
                <button
                  type="button"
                  key={s}
                  onClick={() => setStars(s)}
                  className="focus:outline-none"
                >
                  <Star
                    className={`w-8 h-8 transition-colors ${
                      s <= stars ? "text-orange-400 fill-current" : "text-gray-200"
                    }`}
                  />
                </button>
              ))}
            </div>
          </div>



          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Nội dung</label>
            <textarea
              value={content}
              onChange={(e) => setContent(e.target.value)}
              rows={4}
              placeholder="Chia sẻ trải nghiệm của bạn..."
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
              {submitting ? "Đang gửi..." : "Gửi đánh giá"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
