import { useEffect, useMemo, useState } from "react";
import { useNavigate, useSearchParams } from "react-router";
import { SlidersHorizontal, MapPin, Search, X, ChevronDown, ArrowUpDown, Grid3X3 } from "lucide-react";
import { getPropertyListings } from "../../lib/properties";
import type { PropertyListing } from "../../lib/types";
import { formatCurrency } from "../../lib/format";

const districts = ["Tất cả", "Bình Thạnh", "Quận 7", "Quận 10", "Gò Vấp", "Thủ Đức", "Quận 3", "Quận 12"];

type SortOption = "price_asc" | "price_desc" | "newest";

function extractDistrict(address?: string | null) {
  if (!address) return "Khác";
  const district = districts.find((item) => item !== "Tất cả" && address.toLowerCase().includes(item.toLowerCase()));
  return district || "Khác";
}

export default function SearchPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [query, setQuery] = useState(searchParams.get("q") || "");
  const [selectedDistrict, setSelectedDistrict] = useState("Tất cả");
  const [priceMin, setPriceMin] = useState(0);
  const [priceMax, setPriceMax] = useState(10000000);
  const [selectedAmenities, setSelectedAmenities] = useState<string[]>([]);
  const [sortBy, setSortBy] = useState<SortOption>("newest");
  const [showFilters, setShowFilters] = useState(false);
  const [viewMode, setViewMode] = useState<"split" | "list">("split");
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
          setListings(response.items);
        }
      } catch (err) {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : "Khong tai duoc du lieu.");
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

  const allAmenities = useMemo(() => {
    return Array.from(new Set(listings.flatMap((item) => item.amenities))).slice(0, 10);
  }, [listings]);

  const filteredRooms = useMemo(() => {
    return listings
      .filter((item) => {
        if (!query) return true;
        const q = query.toLowerCase();
        return (
          item.propertyName.toLowerCase().includes(q) ||
          (item.address || "").toLowerCase().includes(q) ||
          extractDistrict(item.address).toLowerCase().includes(q)
        );
      })
      .filter((item) => selectedDistrict === "Tất cả" || extractDistrict(item.address) === selectedDistrict)
      .filter((item) => item.price >= priceMin && item.price <= priceMax)
      .filter((item) => selectedAmenities.every((amenity) => item.amenities.includes(amenity)))
      .sort((a, b) => {
        if (sortBy === "price_asc") return a.price - b.price;
        if (sortBy === "price_desc") return b.price - a.price;
        return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
      });
  }, [listings, priceMax, priceMin, query, selectedAmenities, selectedDistrict, sortBy]);

  const toggleAmenity = (amenity: string) => {
    setSelectedAmenities((prev) =>
      prev.includes(amenity) ? prev.filter((item) => item !== amenity) : [...prev, amenity],
    );
  };

  return (
    <div className="flex flex-col h-full">
      <div className="bg-white border-b border-gray-100 px-4 py-3 flex flex-col sm:flex-row gap-3">
        <div className="flex-1 flex items-center gap-2 bg-gray-50 rounded-xl px-3 py-2 border border-gray-200 focus-within:border-orange-300">
          <Search className="w-4 h-4 text-gray-400 shrink-0" />
          <input
            type="text"
            placeholder="Tìm theo khu vực, địa chỉ..."
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            className="flex-1 bg-transparent focus:outline-none text-gray-700"
            style={{ fontSize: "14px" }}
          />
          {query && (
            <button onClick={() => setQuery("")}>
              <X className="w-4 h-4 text-gray-400 hover:text-gray-600" />
            </button>
          )}
        </div>

        <div className="flex items-center gap-2">
          <div className="relative flex-1 sm:flex-none">
            <select
              value={sortBy}
              onChange={(e) => setSortBy(e.target.value as SortOption)}
              className="appearance-none pl-3 pr-8 py-2 rounded-xl border border-gray-200 bg-white text-gray-600 focus:outline-none focus:border-orange-300 w-full"
              style={{ fontSize: "13px" }}
            >
              <option value="newest">Mới nhất</option>
              <option value="price_asc">Giá tăng dần</option>
              <option value="price_desc">Giá giảm dần</option>
            </select>
            <ChevronDown className="absolute right-2 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
          </div>

          <button
            onClick={() => setShowFilters(!showFilters)}
            className={`flex items-center gap-2 px-3 py-2 rounded-xl border transition-colors ${
              showFilters ? "border-orange-400 bg-orange-50 text-orange-600" : "border-gray-200 text-gray-600 hover:bg-gray-50"
            }`}
            style={{ fontSize: "13px" }}
          >
            <SlidersHorizontal className="w-4 h-4" />
            Bộ lọc
          </button>

          <div className="hidden sm:flex items-center gap-1 bg-gray-100 rounded-lg p-1">
            <button
              onClick={() => setViewMode("split")}
              className={`p-1.5 rounded-md transition-colors ${viewMode === "split" ? "bg-white shadow-sm text-orange-500" : "text-gray-400 hover:text-gray-600"}`}
            >
              <ArrowUpDown className="w-4 h-4" />
            </button>
            <button
              onClick={() => setViewMode("list")}
              className={`p-1.5 rounded-md transition-colors ${viewMode === "list" ? "bg-white shadow-sm text-orange-500" : "text-gray-400 hover:text-gray-600"}`}
            >
              <Grid3X3 className="w-4 h-4" />
            </button>
          </div>
        </div>
      </div>

      {showFilters && (
        <div className="bg-white border-b border-gray-100 px-4 py-4">
          <div className="flex flex-wrap gap-6">
            <div>
              <p className="text-gray-600 mb-2" style={{ fontSize: "12px", fontWeight: 600 }}>
                KHU VỰC
              </p>
              <div className="flex flex-wrap gap-2">
                {districts.map((district) => (
                  <button
                    key={district}
                    onClick={() => setSelectedDistrict(district)}
                    className={`px-3 py-1 rounded-lg border transition-colors ${
                      selectedDistrict === district
                        ? "border-orange-400 bg-orange-50 text-orange-600"
                        : "border-gray-200 text-gray-500 hover:border-gray-300"
                    }`}
                    style={{ fontSize: "13px" }}
                  >
                    {district}
                  </button>
                ))}
              </div>
            </div>

            <div>
              <p className="text-gray-600 mb-2" style={{ fontSize: "12px", fontWeight: 600 }}>
                KHOẢNG GIÁ
              </p>
              <div className="flex items-center gap-2">
                <input
                  type="number"
                  placeholder="Từ"
                  value={priceMin || ""}
                  onChange={(e) => setPriceMin(Number(e.target.value) || 0)}
                  className="w-28 px-3 py-1.5 rounded-lg border border-gray-200 focus:outline-none focus:border-orange-300 text-gray-600"
                  style={{ fontSize: "13px" }}
                />
                <span className="text-gray-400">-</span>
                <input
                  type="number"
                  placeholder="Đến"
                  value={priceMax === 10000000 ? "" : priceMax}
                  onChange={(e) => setPriceMax(Number(e.target.value) || 10000000)}
                  className="w-28 px-3 py-1.5 rounded-lg border border-gray-200 focus:outline-none focus:border-orange-300 text-gray-600"
                  style={{ fontSize: "13px" }}
                />
              </div>
            </div>

            <div>
              <p className="text-gray-600 mb-2" style={{ fontSize: "12px", fontWeight: 600 }}>
                TIỆN NGHI
              </p>
              <div className="flex flex-wrap gap-2">
                {allAmenities.map((amenity) => (
                  <button
                    key={amenity}
                    onClick={() => toggleAmenity(amenity)}
                    className={`px-3 py-1 rounded-lg border transition-colors ${
                      selectedAmenities.includes(amenity)
                        ? "border-orange-400 bg-orange-50 text-orange-600"
                        : "border-gray-200 text-gray-500 hover:border-gray-300"
                    }`}
                    style={{ fontSize: "13px" }}
                  >
                    {amenity}
                  </button>
                ))}
              </div>
            </div>
          </div>
        </div>
      )}

      <div className="bg-gray-50 px-4 py-2 border-b border-gray-100">
        <span className="text-gray-500" style={{ fontSize: "13px" }}>
          {loading ? "Đang tải dữ liệu..." : `Tìm thấy ${filteredRooms.length} tin`}
        </span>
      </div>

      <div className="flex-1 flex overflow-hidden">
        <div className={`overflow-y-auto ${viewMode === "split" ? "w-full lg:w-1/2" : "w-full"}`} style={{ minHeight: 0 }}>
          <div className="p-4 space-y-3">
            {error ? (
              <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-red-600">{error}</div>
            ) : loading ? (
              Array.from({ length: 6 }).map((_, index) => (
                <div key={index} className="bg-gray-100 rounded-xl animate-pulse h-36" />
              ))
            ) : filteredRooms.length === 0 ? (
              <div className="text-center py-16 text-gray-400">
                <Search className="w-10 h-10 mx-auto mb-3 opacity-40" />
                <p style={{ fontSize: "15px" }}>Không tìm thấy phòng phù hợp</p>
              </div>
            ) : (
              filteredRooms.map((room) => (
                <div
                  key={room.id}
                  onClick={() => navigate(`/rooms/${room.id}`)}
                  className="bg-white rounded-xl border border-gray-100 overflow-hidden cursor-pointer transition-all hover:shadow-md flex gap-0"
                >
                  <div className="w-28 sm:w-40 shrink-0 relative">
                    <img
                      src={room.images[0] || "https://placehold.co/400x300?text=No+Image"}
                      alt={room.propertyName}
                      className="w-full h-full object-cover"
                      style={{ minHeight: "100px" }}
                    />
                  </div>
                  <div className="flex-1 p-3">
                    <h3 className="text-gray-900 mb-1" style={{ fontSize: "14px", fontWeight: 600, lineHeight: 1.4 }}>
                      {room.propertyName}
                    </h3>
                    <div className="flex items-center gap-1 text-gray-400 mb-1.5">
                      <MapPin className="w-3 h-3 shrink-0" />
                      <span className="truncate" style={{ fontSize: "12px" }}>
                        {room.address || "Chưa có địa chỉ"}
                      </span>
                    </div>
                    <div className="flex items-center gap-3 mb-2">
                      <span className="text-gray-500" style={{ fontSize: "12px" }}>
                        {room.size}m²
                      </span>
                      <span className="text-gray-500" style={{ fontSize: "12px" }}>
                        {extractDistrict(room.address)}
                      </span>
                    </div>
                    <div className="flex flex-wrap gap-1 mb-2">
                      {room.amenities.slice(0, 3).map((amenity) => (
                        <span key={amenity} className="bg-gray-100 text-gray-500 px-2 py-0.5 rounded-md" style={{ fontSize: "10px" }}>
                          {amenity}
                        </span>
                      ))}
                    </div>
                    <div className="flex items-center justify-between">
                      <span className="text-orange-600" style={{ fontSize: "16px", fontWeight: 700 }}>
                        {formatCurrency(room.price)}
                        <span className="text-gray-400" style={{ fontSize: "11px", fontWeight: 400 }}>
                          /tháng
                        </span>
                      </span>
                      <span className="text-gray-400 capitalize" style={{ fontSize: "11px" }}>
                        {room.status}
                      </span>
                    </div>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>

        {viewMode === "split" && (
          <div className="hidden lg:flex flex-1 bg-[#eef3e8] items-center justify-center p-8">
            <div className="max-w-sm text-center">
              <div className="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-white shadow-sm mb-4">
                <MapPin className="w-6 h-6 text-orange-500" />
              </div>
              <h3 className="text-gray-900 mb-2" style={{ fontSize: "18px", fontWeight: 700 }}>
                Chế độ bản đồ chưa được nối
              </h3>
              <p className="text-gray-500" style={{ fontSize: "14px" }}>
                Backend hiện trả danh sách tin nhưng chưa có API bản đồ hoặc geocoding đầy đủ để hiển thị map thật.
              </p>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
