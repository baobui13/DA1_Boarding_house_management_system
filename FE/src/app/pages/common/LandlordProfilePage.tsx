import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router";
import { useApp } from "../../context/AppContext";
import { getUserById } from "../../lib/users";
import { getProperties } from "../../lib/properties";
import type { PropertyListing, UserResponse } from "../../lib/types";
import { formatCurrency } from "../../lib/format";
import { MapPin, Phone, Mail, ChevronLeft, Shield, ChevronRight } from "lucide-react";

export default function LandlordProfilePage() {
  const { id = "" } = useParams();
  const navigate = useNavigate();
  const { token } = useApp();

  const [landlord, setLandlord] = useState<UserResponse | null>(null);
  const [properties, setProperties] = useState<PropertyListing[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    let cancelled = false;

    const loadData = async () => {
      setLoading(true);
      try {
        // Fetch landlord info (backend endpoint was updated to allow anonymous lookup by id or email)
        const userRes = await getUserById(id, token);
        if (!cancelled) setLandlord(userRes);

        // Fetch landlord's properties
        const propRes = await getProperties({ landlordId: id, status: "Available" });
        if (!cancelled) {
          const { getPropertyImages } = await import("../../lib/properties");
          
          const listings: PropertyListing[] = await Promise.all(
            propRes.items.map(async (p) => {
              let images = ["https://placehold.co/600x400?text=Room"];
              try {
                const imgRes = await getPropertyImages(p.id);
                if (imgRes && imgRes.length > 0) {
                  images = imgRes.sort((a, b) => Number(b.isPrimary) - Number(a.isPrimary)).map((img) => img.imageUrl);
                }
              } catch (e) {
                // Ignore error and use placeholder
              }
              return { ...p, images, amenities: [] };
            })
          );
          setProperties(listings);
        }
      } catch (err) {
        if (!cancelled) {
          setError("Không thể tải thông tin chủ trọ.");
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    };

    loadData();

    return () => {
      cancelled = true;
    };
  }, [id, token]);

  if (loading) {
    return <div className="max-w-6xl mx-auto px-4 py-10 text-gray-500">Đang tải thông tin...</div>;
  }

  if (error || !landlord) {
    return (
      <div className="max-w-6xl mx-auto px-4 py-10">
        <div className="rounded-2xl border border-red-200 bg-red-50 px-5 py-4 text-red-600">
          {error || "Không tìm thấy thông tin chủ trọ."}
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-6xl mx-auto px-4 py-6">
      <button
        onClick={() => navigate(-1)}
        className="flex items-center gap-1.5 text-gray-500 hover:text-gray-700 mb-6 transition-colors"
        style={{ fontSize: "14px" }}
      >
        <ChevronLeft className="w-4 h-4" />
        Quay lại
      </button>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Left Column: Landlord Info */}
        <div className="lg:col-span-1">
          <div className="bg-white rounded-2xl p-6 border border-gray-100 shadow-sm sticky top-24 text-center">
            <img
              src={landlord.avatarUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(landlord.fullName)}&background=f97316&color=fff`}
              alt={landlord.fullName}
              className="w-24 h-24 rounded-full mx-auto mb-4 border-4 border-orange-50 object-cover"
            />
            <h1 className="text-xl font-bold text-gray-900 mb-1">{landlord.fullName}</h1>
            <div className="flex items-center justify-center gap-1.5 text-green-600 mb-4 bg-green-50 w-max mx-auto px-3 py-1 rounded-full text-xs font-semibold">
              <Shield className="w-3.5 h-3.5" />
              Chủ trọ uy tín
            </div>

            <div className="space-y-3 text-left border-t border-gray-100 pt-5">
              {landlord.phoneNumber && (
                <div className="flex items-center gap-3 text-gray-600 text-sm">
                  <Phone className="w-4 h-4 text-gray-400" />
                  <span>{landlord.phoneNumber}</span>
                </div>
              )}
              {landlord.email && (
                <div className="flex items-center gap-3 text-gray-600 text-sm">
                  <Mail className="w-4 h-4 text-gray-400" />
                  <span>{landlord.email}</span>
                </div>
              )}
            </div>
            
            <button
              onClick={() => navigate(`/messages?userId=${landlord.id}`)}
              className="w-full mt-6 bg-orange-500 hover:bg-orange-600 text-white font-semibold py-2.5 rounded-xl transition-colors text-sm"
            >
              Nhắn tin ngay
            </button>
          </div>
        </div>

        {/* Right Column: Properties */}
        <div className="lg:col-span-2">
          <h2 className="text-xl font-bold text-gray-900 mb-6">Các phòng đang cho thuê ({properties.length})</h2>
          
          {properties.length === 0 ? (
            <div className="bg-gray-50 rounded-2xl p-10 text-center border border-dashed border-gray-200">
              <p className="text-gray-500">Chủ trọ này hiện chưa có phòng nào đang trống.</p>
            </div>
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-5">
              {properties.map((property) => (
                <div
                  key={property.id}
                  onClick={() => navigate(`/rooms/${property.id}`)}
                  className="bg-white rounded-2xl border border-gray-100 overflow-hidden cursor-pointer hover:shadow-md transition-shadow group flex flex-col"
                >
                  <div className="aspect-[4/3] bg-gray-100 relative overflow-hidden">
                    <img
                      src={property.images[0]}
                      alt={property.propertyName}
                      className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                    />
                    <div className="absolute top-3 left-3 bg-white/90 backdrop-blur text-xs font-semibold px-2.5 py-1 rounded-lg text-gray-700">
                      {property.size}m²
                    </div>
                  </div>
                  <div className="p-4 flex flex-col flex-1">
                    <div className="flex items-start justify-between gap-2 mb-2">
                      <h3 className="font-semibold text-gray-900 line-clamp-2 leading-tight">
                        {property.propertyName}
                      </h3>
                    </div>
                    <div className="flex items-center gap-1.5 text-gray-500 mb-3 text-sm line-clamp-1">
                      <MapPin className="w-3.5 h-3.5 shrink-0" />
                      <span>{property.address || "Chưa cập nhật địa chỉ"}</span>
                    </div>
                    <div className="mt-auto flex items-center justify-between">
                      <div className="font-bold text-orange-500">
                        {formatCurrency(property.price)}<span className="text-xs text-gray-400 font-normal">/tháng</span>
                      </div>
                      <div className="w-8 h-8 rounded-full bg-orange-50 flex items-center justify-center group-hover:bg-orange-100 transition-colors">
                        <ChevronRight className="w-4 h-4 text-orange-500" />
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
