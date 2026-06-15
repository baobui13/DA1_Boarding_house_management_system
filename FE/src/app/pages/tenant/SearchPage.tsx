import { useEffect, useMemo, useState } from "react";
import { useNavigate, useSearchParams } from "react-router";
import { SlidersHorizontal, MapPin, Search, X, ChevronDown, ArrowUpDown, Grid3X3 } from "lucide-react";
import { 
  getPropertyListings, 
  getMostViewedPropertyListings, 
  getTrendingPropertyListings,
  getRecommendedPropertyListings 
} from "../../lib/properties";
import { createSearchHistory } from "../../lib/searchHistory";
import type { PropertyListing } from "../../lib/types";
import { formatCurrency } from "../../lib/format";
import { useApp } from "../../context/AppContext";
import { MapContainer, TileLayer, Marker, Popup } from "react-leaflet";
import L from "leaflet";
import "leaflet/dist/leaflet.css";

// Fix leaflet default icon issue
delete (L.Icon.Default.prototype as any)._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: "https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png",
  iconUrl: "https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png",
  shadowUrl: "https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png",
});

const districts = ["Tất cả", "Bình Thạnh", "Quận 7", "Quận 10", "Gò Vấp", "Thủ Đức", "Quận 3", "Quận 12"];

type SortOption = "price_asc" | "price_desc" | "newest";

function isVisibleListing(item: PropertyListing) {
  const isApproved = item.moderationStatus?.toLowerCase() === "approved";
  const isAvailable = item.status?.toLowerCase() === "available";
  return isApproved && isAvailable;
}

function normalizeSearchText(value: string) {
  return value
    .toLowerCase()
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .trim();
}

function extractDistrict(address?: string | null) {
  if (!address) return "Khác";
  const district = districts.find((item) => item !== "Tất cả" && address.toLowerCase().includes(item.toLowerCase()));
  return district || "Khác";
}

function getStatusSearchTerms(status: string) {
  const normalized = normalizeSearchText(status);

  if (normalized === "rented") {
    return ["rented", "da thue", "thue", "occupied"];
  }

  if (normalized === "available") {
    return ["available", "con trong", "trong", "vacant"];
  }

  if (normalized === "working" || normalized === "maintenance") {
    return ["working", "maintenance", "dang sua", "bao tri", "sua chua"];
  }

  return [normalized];
}

export default function SearchPage() {
  const navigate = useNavigate();
  const { token, currentUser } = useApp();
  const [searchParams] = useSearchParams();
  const PAGE_SIZE = 12;
  const [query, setQuery] = useState(searchParams.get("q") || "");
  const [selectedDistrict, setSelectedDistrict] = useState(searchParams.get("district") || "Tất cả");
  const [priceMin, setPriceMin] = useState(() => {
    const p = searchParams.get("priceMin");
    return p ? Number(p) : 0;
  });
  const [priceMax, setPriceMax] = useState(() => {
    const p = searchParams.get("priceMax");
    return p ? Number(p) : 10000000;
  });
  const [selectedAmenities, setSelectedAmenities] = useState<string[]>([]);
  const [sortBy, setSortBy] = useState<SortOption>("newest");
  const [showFilters, setShowFilters] = useState(false);
  const [viewMode, setViewMode] = useState<"split" | "list">("split");
  const [listings, setListings] = useState<PropertyListing[]>([]);
  const [pageNumber, setPageNumber] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [recType, setRecType] = useState<string | null>(null); // most_viewed | trending | recommended

  const [boostAspects, setBoostAspects] = useState<string[]>([]);
  const [recommendationMode, setRecommendationMode] = useState<string>("PersonalMatch");

  useEffect(() => {
    let cancelled = false;

    const typeFromUrl = searchParams.get("type") || searchParams.get("rec");
    setRecType(typeFromUrl);

    (async () => {
      setLoading(true);
      setError("");

      try {
        let response;
        const fetchQuery: Record<string, any> = { page: 1, pageSize: 100 };
        if (boostAspects.length > 0) fetchQuery.boostAspect = boostAspects;
        if (recommendationMode) fetchQuery.recommendationMode = recommendationMode;

        if (typeFromUrl === "most_viewed") {
          response = await getMostViewedPropertyListings(token, fetchQuery);
        } else if (typeFromUrl === "trending") {
          response = await getTrendingPropertyListings(token, fetchQuery);
        } else if (typeFromUrl === "recommended" && token && currentUser) {
          response = await getRecommendedPropertyListings(token, fetchQuery);
        } else {
          response = await getPropertyListings(fetchQuery, token);
        }

        if (!cancelled) {
          setListings((response.items || []).filter(isVisibleListing));
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
  }, [searchParams, token, currentUser, boostAspects, recommendationMode]);

  useEffect(() => {
    setPageNumber(1);
  }, [query, selectedDistrict, priceMin, priceMax, selectedAmenities, sortBy]);

  const allAmenities = useMemo(() => {
    return Array.from(new Set(listings.flatMap((item) => item.amenities))).slice(0, 10);
  }, [listings]);

  const filteredRooms = useMemo(() => {
    return listings
      .filter((item) => {
        if (!query) return true;
        const q = normalizeSearchText(query);
        const searchFields = [
          normalizeSearchText(item.propertyName),
          normalizeSearchText(item.address || ""),
          normalizeSearchText(extractDistrict(item.address)),
          ...getStatusSearchTerms(item.status),
        ];

        return (
          searchFields.some((field) => field.includes(q))
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

  // Record search history to power personalized recommendations (only for logged-in tenants)
  useEffect(() => {
    if (!currentUser || !token) return;

    const hasActiveCriteria =
      query.trim() ||
      selectedDistrict !== "Tất cả" ||
      priceMin > 0 ||
      priceMax < 10000000 ||
      selectedAmenities.length > 0;

    if (!hasActiveCriteria) return;

    const filtersPayload = JSON.stringify({
      q: query.trim() || undefined,
      district: selectedDistrict !== "Tất cả" ? selectedDistrict : undefined,
      priceMin: priceMin > 0 ? priceMin : undefined,
      priceMax: priceMax < 10000000 ? priceMax : undefined,
      amenities: selectedAmenities.length > 0 ? selectedAmenities : undefined,
      sort: sortBy,
    });

    createSearchHistory(token, {
      userId: currentUser.id,
      filters: filtersPayload,
    }).catch(() => {
      // Non-critical background call
    });
  }, [query, selectedDistrict, priceMin, priceMax, selectedAmenities, sortBy, currentUser, token]);

  const totalCount = filteredRooms.length;
  const totalPages = Math.max(1, Math.ceil(totalCount / PAGE_SIZE));
  const pagedRooms = useMemo(
    () => filteredRooms.slice((pageNumber - 1) * PAGE_SIZE, pageNumber * PAGE_SIZE),
    [filteredRooms, pageNumber],
  );
  const pageStart = totalCount === 0 ? 0 : (pageNumber - 1) * PAGE_SIZE + 1;
  const pageEnd = totalCount === 0 ? 0 : Math.min(pageNumber * PAGE_SIZE, totalCount);

  // Banner for recommended type
  const recBanner = recType === "most_viewed" 
    ? "Hiển thị phòng được xem nhiều nhất theo đề cử" 
    : recType === "trending" 
    ? "Hiển thị phòng đang xu hướng tìm kiếm" 
    : recType === "recommended" 
    ? "Hiển thị phòng đề cử dành riêng cho bạn" 
    : null;

  const currentType = searchParams.get('type') || searchParams.get('rec');

  const changePage = (nextPage: number) => {
    if (nextPage === pageNumber) {
      return;
    }

    setPageNumber(nextPage);
    window.scrollTo({ top: 0, behavior: "smooth" });
  };

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

      {/* Recommendation type filters - "xu hướng tìm kiếm" and "xem nhiều nhất" as selectable filters on the search UI */}
      <div className="px-4 py-2 border-b border-gray-100 bg-white flex items-center gap-2 text-sm">
        <span className="text-gray-500 mr-1">Đề cử:</span>
        <button
          onClick={() => {
            const p = new URLSearchParams(searchParams);
            p.delete('type');
            p.delete('rec');
            navigate(`/search?${p.toString()}`);
          }}
          className={`px-3 py-1 rounded-full border text-xs transition ${!currentType ? 'bg-orange-500 text-white border-orange-500' : 'border-gray-200 hover:bg-gray-50'}`}
        >
          Tất cả
        </button>
        <button
          onClick={() => {
            const p = new URLSearchParams(searchParams);
            p.set('type', 'most_viewed');
            navigate(`/search?${p.toString()}`);
          }}
          className={`px-3 py-1 rounded-full border text-xs transition ${currentType === 'most_viewed' ? 'bg-blue-500 text-white border-blue-500' : 'border-gray-200 hover:bg-gray-50'}`}
        >
          Xem nhiều nhất
        </button>
        <button
          onClick={() => {
            const p = new URLSearchParams(searchParams);
            p.set('type', 'trending');
            navigate(`/search?${p.toString()}`);
          }}
          className={`px-3 py-1 rounded-full border text-xs transition ${currentType === 'trending' ? 'bg-emerald-500 text-white border-emerald-500' : 'border-gray-200 hover:bg-gray-50'}`}
        >
          Xu hướng tìm kiếm
        </button>
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

            <div>
              <p className="text-gray-600 mb-2" style={{ fontSize: "12px", fontWeight: 600 }}>
                CHẾ ĐỘ ĐỀ CỬ (AI)
              </p>
              <select
                value={recommendationMode}
                onChange={(e) => setRecommendationMode(e.target.value)}
                className="px-3 py-1.5 rounded-lg border border-gray-200 focus:outline-none focus:border-orange-300 text-gray-600 bg-white"
                style={{ fontSize: "13px" }}
              >
                <option value="PersonalMatch">Cá nhân hoá</option>
                <option value="HighAspectQuality">Ưu tiên chất lượng</option>
                <option value="Balanced">Cân bằng</option>
                <option value="PriceSensitive">Nhạy cảm giá</option>
                <option value="Explore">Khám phá</option>
                <option value="AvoidNegatives">Tránh điểm yếu</option>
              </select>
            </div>

            <div>
              <p className="text-gray-600 mb-2" style={{ fontSize: "12px", fontWeight: 600 }}>
                YẾU TỐ QUAN TÂM NHẤT
              </p>
<div className="flex flex-wrap gap-2">
                  {[
                    { label: "Chất lượng phòng", value: "RoomQuality" },
                    { label: "Tiếng ồn", value: "Noise" },
                    { label: "Wifi", value: "Wifi" },
                    { label: "Điện nước", value: "Utilities" },
                    { label: "Chỗ đậu xe", value: "Parking" },
                    { label: "An ninh", value: "Security" },
                    { label: "Môi trường", value: "Environment" },
                    { label: "Chủ trọ", value: "Landlord" },
                    { label: "Vị trí", value: "Location" },
                    { label: "Giá cả", value: "Price" },
                  ].map((aspect) => (
                  <button
                    key={aspect.value}
                    onClick={() => {
                      setBoostAspects((prev) =>
                        prev.includes(aspect.value)
                          ? prev.filter((item) => item !== aspect.value)
                          : [...prev, aspect.value]
                      );
                    }}
                    className={`px-3 py-1 rounded-lg border transition-colors ${
                      boostAspects.includes(aspect.value)
                        ? "border-orange-400 bg-orange-50 text-orange-600"
                        : "border-gray-200 text-gray-500 hover:border-gray-300"
                    }`}
                    style={{ fontSize: "13px" }}
                  >
                    {aspect.label}
                  </button>
                ))}
              </div>
            </div>
          </div>
        </div>
      )}

      <div className="bg-gray-50 px-4 py-2 border-b border-gray-100">
        <span className="text-gray-500" style={{ fontSize: "13px" }}>
          {loading ? "Đang tải dữ liệu..." : `Trang ${pageNumber}/${totalPages} · tải ${pageStart}-${pageEnd} / ${totalCount} tin`}
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
              <>
                {recBanner && (
                  <div className="mb-4 px-4 py-2 rounded-xl bg-orange-50 text-orange-700 text-sm font-medium">
                    {recBanner}
                  </div>
                )}
                {pagedRooms.map((room) => (
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
                ))}

                <div className="flex flex-wrap items-center justify-between gap-3 rounded-xl bg-white px-4 py-3 border border-gray-100">
                  <p className="text-gray-500" style={{ fontSize: "13px", fontWeight: 600 }}>
                    Hiển thị tối đa {PAGE_SIZE} tin mỗi trang để tải nhanh hơn
                  </p>
                  <div className="flex items-center gap-2">
                    <button
                      onClick={() => changePage(Math.max(1, pageNumber - 1))}
                      disabled={pageNumber === 1}
                      className="rounded-xl border border-gray-200 px-3 py-2 text-gray-600 disabled:cursor-not-allowed disabled:opacity-40"
                      style={{ fontSize: "13px", fontWeight: 700 }}
                    >
                      Trước
                    </button>
                    <span className="px-2 text-gray-500" style={{ fontSize: "13px", fontWeight: 700 }}>
                      {pageNumber}/{totalPages}
                    </span>
                    <button
                      onClick={() => changePage(Math.min(totalPages, pageNumber + 1))}
                      disabled={pageNumber >= totalPages}
                      className="rounded-xl border border-gray-200 px-3 py-2 text-gray-600 disabled:cursor-not-allowed disabled:opacity-40"
                      style={{ fontSize: "13px", fontWeight: 700 }}
                    >
                      Sau
                    </button>
                  </div>
                </div>
              </>
            )}
          </div>
        </div>

        {viewMode === "split" && (
          <div className="hidden lg:block flex-1 bg-gray-100 relative z-0">
            <MapContainer center={[10.8231, 106.6297]} zoom={11} style={{ height: "100%", width: "100%" }}>
              <TileLayer
                attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
              />
              {pagedRooms.map((room) => (
                <GeocodedMarker key={room.id} room={room} navigate={navigate} />
              ))}
            </MapContainer>
          </div>
        )}
      </div>
    </div>
  );
}

function GeocodedMarker({ room, navigate }: { room: PropertyListing; navigate: any }) {
  const [position, setPosition] = useState<[number, number] | null>(null);

  useEffect(() => {
    if (room.latitude && room.longitude) {
      setPosition([room.latitude, room.longitude]);
      return;
    }
    
    if (room.address) {
      // Delay call to avoid rate limit (1 request per second for Nominatim)
      const timer = setTimeout(() => {
        fetch(`https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(room.address + ", Hồ Chí Minh")}&limit=1`)
          .then((res) => res.json())
          .then((data) => {
            if (data && data.length > 0) {
              setPosition([parseFloat(data[0].lat), parseFloat(data[0].lon)]);
            }
          })
          .catch((err) => console.error("Geocoding failed", err));
      }, Math.random() * 2000);

      return () => clearTimeout(timer);
    }
  }, [room.latitude, room.longitude, room.address]);

  if (!position) return null;

  return (
    <Marker position={position}>
      <Popup>
        <div className="w-48 cursor-pointer" onClick={() => navigate(`/rooms/${room.id}`)}>
          <img
            src={room.images[0] || "https://placehold.co/400x300?text=No+Image"}
            alt={room.propertyName}
            className="w-full h-24 object-cover rounded mb-2"
          />
          <h4 className="font-bold text-sm mb-1 line-clamp-1">{room.propertyName}</h4>
          <p className="text-orange-600 font-semibold">{formatCurrency(room.price)}/tháng</p>
        </div>
      </Popup>
    </Marker>
  );
}
