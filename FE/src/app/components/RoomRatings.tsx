import { useEffect, useState } from "react";
import { getRatings, type RatingResponse } from "../lib/ratings";
import { Star, MessageSquarePlus } from "lucide-react";
import { useApp } from "../context/AppContext";
import { RatingFormModal } from "./RatingFormModal";
import { getContracts } from "../lib/contracts";

interface RoomRatingsProps {
  propertyId: string;
}

export function RoomRatings({ propertyId }: RoomRatingsProps) {
  const { currentUser, token } = useApp();
  const [ratings, setRatings] = useState<RatingResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [hasContract, setHasContract] = useState(false);

  const loadRatings = async () => {
    try {
      const response = await getRatings({ PropertyId: propertyId });
      const filteredRatings = response.items.filter((r) => r.propertyId === propertyId);
      setRatings(filteredRatings);
    } catch (error) {
      console.error("Failed to load ratings:", error);
    }
  };

  useEffect(() => {
    let mounted = true;
    (async () => {
      await loadRatings();
      if (mounted) setLoading(false);
    })();
    return () => {
      mounted = false;
    };
  }, [propertyId]);

  useEffect(() => {
    let mounted = true;
    if (currentUser?.role === "Tenant" && token) {
      (async () => {
        try {
          const contractsRes = await getContracts(token);
          const userContracts = contractsRes.items.filter(
            (c) => c.tenantId === currentUser.id && c.propertyId === propertyId
          );
          if (mounted) setHasContract(userContracts.length > 0);
        } catch (err) {
          console.error("Failed to check contract:", err);
        }
      })();
    }
    return () => {
      mounted = false;
    };
  }, [currentUser, token, propertyId]);

  if (loading) {
    return <div className="text-gray-500 text-sm mt-4">Đang tải đánh giá...</div>;
  }

  const canRate = currentUser?.role === "Tenant" && hasContract;
  const userRating = currentUser ? ratings.find(r => r.tenantId === currentUser.id) : undefined;

  if (ratings.length === 0) {
    return (
      <div className="mt-8">
        <div className="flex items-center justify-between mb-3">
          <h2 className="text-gray-900" style={{ fontSize: "17px", fontWeight: 700 }}>
            Đánh giá từ người thuê
          </h2>
          {canRate && (
            <button
              onClick={() => setShowModal(true)}
              className="flex items-center gap-1.5 px-3 py-1.5 bg-orange-100 text-orange-600 rounded-lg hover:bg-orange-200 transition-colors text-sm font-semibold"
            >
              <MessageSquarePlus className="w-4 h-4" />
              {userRating ? "Sửa đánh giá" : "Viết đánh giá"}
            </button>
          )}
        </div>
        <div className="rounded-xl border border-gray-100 bg-gray-50 px-4 py-3 text-gray-500 text-sm">
          Chưa có đánh giá nào cho phòng này.
        </div>
        {showModal && (
          <RatingFormModal
            propertyId={propertyId}
            initialData={userRating}
            onClose={() => setShowModal(false)}
            onSuccess={() => {
              setShowModal(false);
              void loadRatings();
            }}
          />
        )}
      </div>
    );
  }

  const averageRating = (ratings.reduce((acc, r) => acc + r.stars, 0) / ratings.length).toFixed(1);

  return (
    <div className="mt-8">
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center gap-3">
          <h2 className="text-gray-900" style={{ fontSize: "17px", fontWeight: 700 }}>
            Đánh giá từ người thuê ({ratings.length})
          </h2>
          <div className="flex items-center gap-1 bg-orange-50 px-2.5 py-1 rounded-full">
            <Star className="w-4 h-4 text-orange-500 fill-current" />
            <span className="text-orange-700 font-bold text-sm">{averageRating}</span>
          </div>
        </div>
        {canRate && (
          <button
            onClick={() => setShowModal(true)}
            className="flex items-center gap-1.5 px-3 py-1.5 bg-orange-100 text-orange-600 rounded-lg hover:bg-orange-200 transition-colors text-sm font-semibold"
          >
            <MessageSquarePlus className="w-4 h-4" />
            {userRating ? "Sửa đánh giá" : "Viết đánh giá"}
          </button>
        )}
      </div>
      <div className="space-y-4">
        {ratings.map((rating) => (
          <div key={rating.id} className="bg-white border border-gray-100 rounded-xl p-4 shadow-sm">
            <div className="flex items-center justify-between mb-2">
              <div className="flex items-center gap-1">
                {Array.from({ length: 5 }).map((_, i) => (
                  <Star
                    key={i}
                    className={`w-4 h-4 ${
                      i < rating.stars ? "text-orange-400 fill-current" : "text-gray-200"
                    }`}
                  />
                ))}
              </div>
              <span className="text-gray-400 text-xs">
                {new Date(rating.createdAt).toLocaleDateString("vi-VN")}
              </span>
            </div>
            {rating.content && (
              <p className="text-gray-600 text-sm leading-relaxed mt-2">{rating.content}</p>
            )}
            {rating.aIAttitude && (
              <div className="mt-3 inline-block px-2 py-1 rounded-md bg-gray-50 border border-gray-100 text-xs text-gray-500">
                Thái độ: {rating.aIAttitude}
              </div>
            )}
          </div>
        ))}
      </div>
      {showModal && (
        <RatingFormModal
          propertyId={propertyId}
          initialData={userRating}
          onClose={() => setShowModal(false)}
          onSuccess={() => {
            setShowModal(false);
            void loadRatings();
          }}
        />
      )}
    </div>
  );
}
