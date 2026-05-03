import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router";
import { Search, MapPin, Star, ChevronRight, ArrowRight } from "lucide-react";
import { getPropertyListings } from "../../lib/properties";
import type { PropertyListing } from "../../lib/types";
import { formatCurrency } from "../../lib/format";

const districts = ["Bình Thạnh", "Quận 7", "Quận 10", "Gò Vấp", "Thủ Đức", "Quận 3"];

function isVisibleListing(item: PropertyListing) {
  return !["rejected", "unavailable"].includes(item.status.toLowerCase());
}

export default function LandingPage() {
  const navigate = useNavigate();
  const [searchQuery, setSearchQuery] = useState("");
  const [priceRange, setPriceRange] = useState("");
  const [listings, setListings] = useState<PropertyListing[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    let cancelled = false;

    (async () => {
      setLoading(true);
      setError("");

      try {
        const response = await getPropertyListings();
        if (!cancelled) {
          setListings(response.items.filter(isVisibleListing));
        }
      } catch (err) {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : "Khong tai duoc danh sach phong.");
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
  }, []);

  const suggestedRooms = useMemo(() => listings.slice(0, 4), [listings]);

  const handleSearch = () => {
    const params = new URLSearchParams();
    if (searchQuery) params.set("q", searchQuery);
    if (priceRange) params.set("price", priceRange);
    navigate(`/search?${params.toString()}`);
  };

  return (
    <div className="min-h-screen bg-white">
      <div
        className="relative min-h-[520px] flex items-center justify-center overflow-hidden"
        style={{ background: "linear-gradient(135deg, #1e3a5f 0%, #f97316 100%)" }}
      >
        <div className="absolute inset-0 opacity-10">
          <img
            src="https://images.unsplash.com/photo-1602646994030-464f98de5e5c?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=1200"
            alt=""
            className="w-full h-full object-cover"
          />
        </div>

        <div className="relative z-10 w-full max-w-3xl mx-auto px-4 text-center">
          <div className="inline-block bg-white/20 backdrop-blur-sm text-white px-4 py-1.5 rounded-full mb-4 border border-white/30">
            <span style={{ fontSize: "13px" }}>Nguồn dữ liệu đang lấy trực tiếp từ backend</span>
          </div>
          <h1 className="text-white mb-4" style={{ fontSize: "42px", fontWeight: 700, lineHeight: 1.2 }}>
            Tìm Phòng Trọ
            <br />
            <span className="text-orange-300">Nhanh - Dễ - Uy Tín</span>
          </h1>
          <p className="text-white/80 mb-8 max-w-lg mx-auto" style={{ fontSize: "16px" }}>
            Khám phá các tin đang hiển thị từ API quản lý phòng trọ.
          </p>

          <div className="bg-white rounded-2xl shadow-2xl p-3 flex flex-col sm:flex-row gap-3">
            <div className="flex-1 flex items-center gap-3 px-3 bg-gray-50 rounded-xl">
              <MapPin className="w-5 h-5 text-orange-400 shrink-0" />
              <input
                type="text"
                placeholder="Khu vực, quận, đường..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                onKeyDown={(e) => e.key === "Enter" && handleSearch()}
                className="flex-1 bg-transparent py-3 focus:outline-none text-gray-700"
                style={{ fontSize: "15px" }}
              />
            </div>
            <div className="flex items-center gap-3 px-3 bg-gray-50 rounded-xl sm:w-44">
              <span className="text-gray-400" style={{ fontSize: "13px" }}>💰</span>
              <select
                value={priceRange}
                onChange={(e) => setPriceRange(e.target.value)}
                className="flex-1 bg-transparent py-3 focus:outline-none text-gray-600"
                style={{ fontSize: "14px" }}
              >
                <option value="">Khoảng giá</option>
                <option value="0-3000000">Dưới 3 triệu</option>
                <option value="3000000-5000000">3 - 5 triệu</option>
                <option value="5000000-8000000">5 - 8 triệu</option>
                <option value="8000000-999999999">Trên 8 triệu</option>
              </select>
            </div>
            <button
              onClick={handleSearch}
              className="flex items-center justify-center gap-2 px-6 py-3 bg-orange-500 text-white rounded-xl hover:bg-orange-600 active:bg-orange-700 transition-colors shadow-md shadow-orange-200"
              style={{ fontSize: "15px", fontWeight: 600 }}
            >
              <Search className="w-5 h-5" />
              Tìm kiếm
            </button>
          </div>

          <div className="flex flex-wrap justify-center gap-2 mt-4">
            {districts.map((district) => (
              <button
                key={district}
                onClick={() => navigate(`/search?q=${encodeURIComponent(district)}`)}
                className="px-3 py-1 bg-white/20 backdrop-blur-sm text-white border border-white/30 rounded-full hover:bg-white/30 transition-colors"
                style={{ fontSize: "13px" }}
              >
                {district}
              </button>
            ))}
          </div>
        </div>
      </div>

      <div className="max-w-6xl mx-auto px-4 py-10">
        <div className="flex items-center justify-between mb-6">
          <div>
            <h2 className="text-gray-900" style={{ fontSize: "22px", fontWeight: 700 }}>
              Tin Đang Hiển Thị
            </h2>
            <p className="text-gray-500 mt-0.5" style={{ fontSize: "14px" }}>
              {loading ? "Đang tải từ API..." : `${listings.length} tin lấy từ backend`}
            </p>
          </div>
          <button
            onClick={() => navigate("/search")}
            className="flex items-center gap-1.5 text-orange-600 hover:text-orange-700 transition-colors"
            style={{ fontSize: "14px", fontWeight: 500 }}
          >
            Xem tất cả <ChevronRight className="w-4 h-4" />
          </button>
        </div>

        {error ? (
          <div className="rounded-2xl border border-red-200 bg-red-50 px-5 py-4 text-red-600">{error}</div>
        ) : loading ? (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
            {Array.from({ length: 4 }).map((_, index) => (
              <div key={index} className="bg-gray-100 rounded-2xl animate-pulse aspect-[4/5]" />
            ))}
          </div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
            {suggestedRooms.map((room) => (
              <div
                key={room.id}
                onClick={() => navigate(`/rooms/${room.id}`)}
                className="bg-white rounded-2xl border border-gray-100 overflow-hidden hover:shadow-lg hover:shadow-gray-100 transition-all cursor-pointer group"
              >
                <div className="relative overflow-hidden aspect-[4/3]">
                  <img
                    src={room.images[0] || "https://placehold.co/800x600?text=No+Image"}
                    alt={room.propertyName}
                    className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
                  />
                  <div className="absolute top-3 left-3">
                    <span className="bg-green-500 text-white px-2 py-0.5 rounded-lg" style={{ fontSize: "11px", fontWeight: 600 }}>
                      {room.status}
                    </span>
                  </div>
                  <div className="absolute top-3 right-3">
                    <span className="bg-white/90 backdrop-blur-sm text-gray-700 px-2 py-0.5 rounded-lg" style={{ fontSize: "11px" }}>
                      {room.size}m²
                    </span>
                  </div>
                </div>
                <div className="p-4">
                  <h3 className="text-gray-900 mb-1 line-clamp-2" style={{ fontSize: "14px", fontWeight: 600, lineHeight: 1.4 }}>
                    {room.propertyName}
                  </h3>
                  <div className="flex items-center gap-1 text-gray-400 mb-2">
                    <MapPin className="w-3.5 h-3.5 shrink-0" />
                    <span className="truncate" style={{ fontSize: "12px" }}>
                      {room.address || "Chưa có địa chỉ"}
                    </span>
                  </div>
                  <div className="flex items-center gap-1 mb-3">
                    <Star className="w-3.5 h-3.5 text-amber-400 fill-amber-400" />
                    <span className="text-gray-600" style={{ fontSize: "12px" }}>
                      Dữ liệu thật
                    </span>
                  </div>
                  <div className="flex items-center justify-between">
                    <div>
                      <span className="text-orange-600" style={{ fontSize: "16px", fontWeight: 700 }}>
                        {formatCurrency(room.price)}
                      </span>
                      <span className="text-gray-400" style={{ fontSize: "12px" }}>
                        /tháng
                      </span>
                    </div>
                    <div className="flex gap-1">
                      {room.amenities.slice(0, 2).map((amenity) => (
                        <span key={amenity} className="bg-gray-100 text-gray-500 px-2 py-0.5 rounded-lg" style={{ fontSize: "10px" }}>
                          {amenity}
                        </span>
                      ))}
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}

        <div className="mt-12 rounded-2xl bg-gradient-to-r from-orange-500 to-amber-500 p-8 flex flex-col sm:flex-row items-center justify-between gap-6">
          <div>
            <h3 className="text-white" style={{ fontSize: "20px", fontWeight: 700 }}>
              Bạn là chủ nhà trọ?
            </h3>
            <p className="text-orange-100 mt-1" style={{ fontSize: "14px" }}>
              Đăng ký tài khoản chủ trọ để tạo và quản lý tin trực tiếp qua API.
            </p>
          </div>
          <button
            className="flex items-center gap-2 bg-white text-orange-600 px-6 py-3 rounded-xl hover:bg-orange-50 transition-colors whitespace-nowrap"
            style={{ fontSize: "14px", fontWeight: 600 }}
            onClick={() => navigate("/register/landlord")}
          >
            Đăng ký ngay <ArrowRight className="w-4 h-4" />
          </button>
        </div>
      </div>
    </div>
  );
}
