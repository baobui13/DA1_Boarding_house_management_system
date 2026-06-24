import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router";
import {
  Building2,
  ChevronDown,
  ChevronRight,
  Hammer,
  ImagePlus,
  LoaderCircle,
  MapPin,
  Pencil,
  Plus,
  Star,
  Trash2,
  X,
} from "lucide-react";
import { useApp } from "../../context/AppContext";
import { MapContainer, TileLayer, Marker, useMapEvents, useMap } from "react-leaflet";
import L from "leaflet";
import "leaflet/dist/leaflet.css";

// Fix leaflet default icon issue
delete (L.Icon.Default.prototype as any)._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: "https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png",
  iconUrl: "https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png",
  shadowUrl: "https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png",
});

function LocationPicker({ position, onLocationSelect }: { position: [number, number] | null; onLocationSelect: (lat: number, lng: number) => void }) {
  useMapEvents({
    click(e) {
      onLocationSelect(e.latlng.lat, e.latlng.lng);
    },
  });
  return position ? <Marker position={position} /> : null;
}

function MapUpdater({ center }: { center: [number, number] }) {
  const map = useMap();
  useEffect(() => {
    map.flyTo(center, map.getZoom(), { animate: true });
  }, [center[0], center[1], map]);
  return null;
}
import { getAmenities } from "../../lib/amenities";
import { createArea, getMyAreas, updateArea } from "../../lib/areas";
import {
  createProperty,
  createPropertyAmenity,
  createPropertyImage,
  deleteProperty,
  deletePropertyImage,
  deletePropertyAmenity,
  getPropertyImages,
  getPropertyAmenities,
  getMyPropertyListings,
  replacePropertyImage,
  updatePropertyImage,
  updateProperty,
} from "../../lib/properties";
import {
  isAvailablePropertyStatus,
  isMaintenancePropertyStatus,
  isRentedPropertyStatus,
} from "../../lib/propertyStatus";
import type { AmenityResponse, AreaResponse, PropertyImageResponse, PropertyListing } from "../../lib/types";
import { formatCurrency } from "../../lib/format";

type AreaForm = {
  name: string;
  address: string;
  description: string;
};

type RoomForm = {
  propertyName: string;
  address: string;
  size: string;
  price: string;
  electricPrice: string;
  waterPrice: string;
  description: string;
  status: string;
  areaId: string;
  amenities: string[];
  latitude: number | null;
  longitude: number | null;
};

const initialAreaForm: AreaForm = {
  name: "",
  address: "",
  description: "",
};

const initialRoomForm: RoomForm = {
  propertyName: "",
  address: "",
  size: "",
  price: "",
  electricPrice: "",
  waterPrice: "",
  description: "",
  status: "Available",
  areaId: "",
  amenities: [],
  latitude: null,
  longitude: null,
};

export default function PropertyManagement() {
  const { currentUser, token } = useApp();
  const navigate = useNavigate();
  const [areas, setAreas] = useState<AreaResponse[]>([]);
  const [properties, setProperties] = useState<PropertyListing[]>([]);
  const [amenityOptions, setAmenityOptions] = useState<AmenityResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [savingArea, setSavingArea] = useState(false);
  const [savingRoom, setSavingRoom] = useState(false);
  const [showAreaModal, setShowAreaModal] = useState(false);
  const [showRoomModal, setShowRoomModal] = useState(false);
  const [editingArea, setEditingArea] = useState<AreaResponse | null>(null);
  const [editingProperty, setEditingProperty] = useState<PropertyListing | null>(null);
  const [areaForm, setAreaForm] = useState<AreaForm>(initialAreaForm);
  const [roomForm, setRoomForm] = useState<RoomForm>(initialRoomForm);
  const [roomImages, setRoomImages] = useState<PropertyImageResponse[]>([]);
  const [pendingRoomFiles, setPendingRoomFiles] = useState<File[]>([]);
  const [removedRoomImageIds, setRemovedRoomImageIds] = useState<string[]>([]);
  const [replacedRoomImageFiles, setReplacedRoomImageFiles] = useState<Record<string, File>>({});
  const [primaryRoomImageId, setPrimaryRoomImageId] = useState("");
  const [expandedAreas, setExpandedAreas] = useState<Record<string, boolean>>({});

  const loadData = async () => {
    if (!currentUser) return;

    setLoading(true);
    setError("");

    try {
      const [areaResponse, propertyResponse, amenityResponse] = await Promise.all([
        getMyAreas(token),
        getMyPropertyListings(token),
        getAmenities(),
      ]);

      setAreas(areaResponse.items);
      setProperties(propertyResponse.items);
      setAmenityOptions(amenityResponse);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Không tải được dữ liệu.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadData();
  }, [currentUser?.id]);

  const groupedAreas = useMemo(() => {
    const mappedAreas = areas.map((area) => {
      const areaProperties = properties.filter((property) => property.areaId === area.id);
      return {
        area,
        properties: areaProperties,
        totalRooms: areaProperties.length,
        availableRooms: areaProperties.filter((property) => isAvailable(property.status)).length,
        rentedRooms: areaProperties.filter((property) => isRented(property.status)).length,
        maintenanceRooms: areaProperties.filter((property) => isMaintenance(property.status)).length,
      };
    });

    const unassignedProperties = properties.filter((property) => !property.areaId);
    if (unassignedProperties.length === 0) return mappedAreas;

    return [
      ...mappedAreas,
      {
        area: {
          id: "unassigned",
          name: "Chưa gán khu",
          address: "Các phòng chưa thuộc khu nào",
          latitude: null,
          longitude: null,
          roomCount: unassignedProperties.length,
          description: "Tạm thời chưa gắn AreaId",
          landlordId: currentUser?.id || "",
          createdAt: "",
          updatedAt: null,
        },
        properties: unassignedProperties,
        totalRooms: unassignedProperties.length,
        availableRooms: unassignedProperties.filter((property) => isAvailable(property.status)).length,
        rentedRooms: unassignedProperties.filter((property) => isRented(property.status)).length,
        maintenanceRooms: unassignedProperties.filter((property) => isMaintenance(property.status)).length,
      },
    ];
  }, [areas, currentUser?.id, properties]);

  useEffect(() => {
    setExpandedAreas((prev) => {
      const next: Record<string, boolean> = {};
      groupedAreas.forEach((group, index) => {
        next[group.area.id] = prev[group.area.id] ?? index === 0;
      });
      return next;
    });
  }, [groupedAreas]);

  const totalAvailable = properties.filter((property) => isAvailable(property.status)).length;
  const totalRented = properties.filter((property) => isRented(property.status)).length;
  const totalMaintenance = properties.filter((property) => isMaintenance(property.status)).length;

  const openAreaCreate = () => {
    setEditingArea(null);
    setAreaForm(initialAreaForm);
    setShowAreaModal(true);
  };

  const openAreaEdit = (area: AreaResponse) => {
    if (area.id === "unassigned") return;
    setEditingArea(area);
    setAreaForm({
      name: area.name,
      address: area.address,
      description: area.description || "",
    });
    setShowAreaModal(true);
  };

  const openRoomCreate = (areaId?: string) => {
    setEditingProperty(null);
    setRoomForm({
      ...initialRoomForm,
      areaId: areaId && areaId !== "unassigned" ? areaId : "",
      address: areaId && areaId !== "unassigned" ? areas.find((area) => area.id === areaId)?.address || "" : "",
    });
    setRoomImages([]);
    setPendingRoomFiles([]);
    setRemovedRoomImageIds([]);
    setReplacedRoomImageFiles({});
    setPrimaryRoomImageId("");
    setShowRoomModal(true);
  };

  const openRoomEdit = async (property: PropertyListing) => {
    setEditingProperty(property);
    const rawImages = await getPropertyImages(property.id);
    // Defensive client-side filter to ensure only this property's images (in case filter endpoint leaks)
    const images = rawImages.filter((img: any) => (img.propertyId || img.PropertyId) === property.id);
    setRoomForm({
      propertyName: property.propertyName,
      address: property.address || "",
      size: String(property.size),
      price: String(property.price),
      electricPrice: property.electricPrice ? String(property.electricPrice) : "",
      waterPrice: property.waterPrice ? String(property.waterPrice) : "",
      description: property.description || "",
      status: property.status,
      areaId: property.areaId || "",
      amenities: property.amenities,
      latitude: property.latitude ?? null,
      longitude: property.longitude ?? null,
    });
    setRoomImages(images);
    setPendingRoomFiles([]);
    setRemovedRoomImageIds([]);
    setReplacedRoomImageFiles({});
    setPrimaryRoomImageId(images.find((item) => item.isPrimary)?.id || images[0]?.id || "");
    setShowRoomModal(true);
  };

  const closeAreaModal = () => {
    setShowAreaModal(false);
    setEditingArea(null);
    setAreaForm(initialAreaForm);
  };

  const closeRoomModal = () => {
    setShowRoomModal(false);
    setEditingProperty(null);
    setRoomForm(initialRoomForm);
    setRoomImages([]);
    setPendingRoomFiles([]);
    setRemovedRoomImageIds([]);
    setReplacedRoomImageFiles({});
    setPrimaryRoomImageId("");
  };

  const handleAreaSubmit = async () => {
    if (!token || !currentUser) {
      setError("Bạn cần đăng nhập lại để thao tác.");
      return;
    }

    setSavingArea(true);
    setError("");

    try {
      if (editingArea) {
        await updateArea(token, {
          id: editingArea.id,
          name: areaForm.name,
          address: areaForm.address,
          description: areaForm.description,
        });
      } else {
        await createArea(token, {
          landlordId: currentUser.id,
          name: areaForm.name,
          address: areaForm.address,
          description: areaForm.description,
          roomCount: 0,
        });
      }

      closeAreaModal();
      await loadData();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Lưu khu trọ thất bại.");
    } finally {
      setSavingArea(false);
    }
  };

  const handleRoomSubmit = async () => {
    if (!token || !currentUser) {
      setError("Bạn cần đăng nhập lại để thao tác.");
      return;
    }

    setSavingRoom(true);
    setError("");

    try {
      let propertyId = editingProperty?.id;

      if (editingProperty) {
        await updateProperty(token, {
          id: editingProperty.id,
          areaId: roomForm.areaId || null,
          propertyName: roomForm.propertyName,
          address: roomForm.address,
          size: Number(roomForm.size),
          price: Number(roomForm.price),
          description: roomForm.description,
          status: roomForm.status,
          electricPrice: roomForm.electricPrice ? Number(roomForm.electricPrice) : null,
          waterPrice: roomForm.waterPrice ? Number(roomForm.waterPrice) : null,
          latitude: roomForm.latitude,
          longitude: roomForm.longitude,
        });
      } else {
        const created = await createProperty(token, {
          landlordId: currentUser.id,
          areaId: roomForm.areaId || null,
          propertyName: roomForm.propertyName,
          address: roomForm.address,
          size: Number(roomForm.size),
          price: Number(roomForm.price),
          description: roomForm.description,
          status: roomForm.status,
          electricPrice: roomForm.electricPrice ? Number(roomForm.electricPrice) : null,
          waterPrice: roomForm.waterPrice ? Number(roomForm.waterPrice) : null,
          latitude: roomForm.latitude,
          longitude: roomForm.longitude,
        });
        propertyId = created.id;
      }

      if (propertyId) {
        await syncRoomAmenities(token, propertyId, roomForm.amenities, amenityOptions);
        await Promise.all(removedRoomImageIds.map((imageId) => deletePropertyImage(token, imageId)));

        // Replace existing images (in-place file swap via API) - parallel
        const replaceEntries = Object.entries(replacedRoomImageFiles);
        if (replaceEntries.length > 0) {
          await Promise.all(
            replaceEntries.map(([imageId, file]) => replacePropertyImage(token, { id: imageId, file }))
          );
        }

        // Add new images in parallel (major speed improvement)
        // Compute remaining existing count client-side to avoid extra refetch
        const remainingExistingCount = (roomImages || []).filter(
          (img: any) => !removedRoomImageIds.includes(img.id)
        ).length;

        if (pendingRoomFiles.length > 0) {
          await Promise.all(
            pendingRoomFiles.map((file, index) =>
              createPropertyImage(token, {
                propertyId,
                file,
                isPrimary: remainingExistingCount === 0 && index === 0,
              })
            )
          );
        }

        if (primaryRoomImageId) {
          await updatePropertyImage(token, { id: primaryRoomImageId, isPrimary: true });
        }
      }

      closeRoomModal();
      await loadData();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Lưu phòng thất bại.");
    } finally {
      setSavingRoom(false);
    }
  };

  const handleDelete = async (property: PropertyListing) => {
    if (!token) {
      setError("Bạn cần đăng nhập lại để xóa.");
      return;
    }

    if (!confirm(`Xóa phòng "${property.propertyName}"?`)) return;

    try {
      await deleteProperty(token, property.id);
      await loadData();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Xóa thất bại.");
    }
  };

  const handleAddressGeocode = async (addressStr: string) => {
    if (!addressStr.trim()) return;
    try {
      const res = await fetch(`https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(addressStr + ", Hồ Chí Minh")}&limit=1`);
      const data = await res.json();
      if (data && data.length > 0) {
        setRoomForm(prev => ({ ...prev, latitude: parseFloat(data[0].lat), longitude: parseFloat(data[0].lon) }));
      }
    } catch (e) {
      console.error(e);
    }
  };

  const handleGetCurrentLocation = () => {
    if (!navigator.geolocation) {
      alert("Trình duyệt không hỗ trợ lấy vị trí hiện tại.");
      return;
    }
    navigator.geolocation.getCurrentPosition(
      (position) => {
        setRoomForm(prev => ({
          ...prev,
          latitude: position.coords.latitude,
          longitude: position.coords.longitude,
        }));
      },
      (error) => {
        alert("Không thể lấy vị trí hiện tại. Vui lòng kiểm tra quyền truy cập vị trí.");
        console.error(error);
      }
    );
  };

  const toggleArea = (areaId: string) => {
    setExpandedAreas((prev) => ({ ...prev, [areaId]: !prev[areaId] }));
  };

  const toggleRoomAmenity = (name: string) => {
    setRoomForm((prev) => ({
      ...prev,
      amenities: prev.amenities.includes(name)
        ? prev.amenities.filter((item) => item !== name)
        : [...prev.amenities, name],
    }));
  };

  const handleRoomFilesSelected = (e: React.ChangeEvent<HTMLInputElement>) => {
    const incoming = Array.from(e.target.files || []).filter((file) => file.type.startsWith("image/"));
    setPendingRoomFiles((prev) => [...prev, ...incoming]);
    e.target.value = "";
  };

  const removePendingRoomFile = (targetFile: File) => {
    setPendingRoomFiles((prev) => prev.filter((file) => file !== targetFile));
  };

  const handleReplaceRoomImage = (imageId: string) => {
    const input = document.createElement("input");
    input.type = "file";
    input.accept = "image/*";
    input.onchange = (e) => {
      const file = (e.target as HTMLInputElement).files?.[0];
      if (file && file.type.startsWith("image/")) {
        setReplacedRoomImageFiles((prev) => ({ ...prev, [imageId]: file }));
      }
      input.value = "";
    };
    input.click();
  };

  const markRoomImageForRemoval = (imageId: string) => {
    setRemovedRoomImageIds((prev) => (prev.includes(imageId) ? prev : [...prev, imageId]));
    setReplacedRoomImageFiles((prev) => {
      const next = { ...prev };
      delete next[imageId];
      return next;
    });
    if (primaryRoomImageId === imageId) {
      const fallback = roomImages.find((item) => item.id !== imageId && !removedRoomImageIds.includes(item.id));
      setPrimaryRoomImageId(fallback?.id || "");
    }
  };

  return (
    <div className="max-w-7xl mx-auto px-4 py-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between mb-6">
        <div>
          <h1 className="text-gray-900" style={{ fontSize: "24px", fontWeight: 700 }}>
            Quản Lý Khu Trọ & Phòng
          </h1>
          <p className="text-gray-500 mt-1" style={{ fontSize: "14px" }}>
            Quản lý các khu trọ, phòng và trạng thái cho thuê
          </p>
        </div>
        <button
          onClick={openAreaCreate}
          className="inline-flex items-center justify-center gap-2 px-5 py-3 bg-orange-500 text-white rounded-2xl hover:bg-orange-600 transition-colors shadow-sm shadow-orange-200"
          style={{ fontSize: "15px", fontWeight: 600 }}
        >
          <Plus className="w-4 h-4" />
          Thêm khu mới
        </button>
      </div>

      {error && <div className="mb-4 rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-red-600">{error}</div>}

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
        <SummaryCard tone="green" label="Phòng trống" value={String(totalAvailable)} />
        <SummaryCard tone="blue" label="Đang thuê" value={String(totalRented)} />
        <SummaryCard tone="amber" label="Đang sửa" value={String(totalMaintenance)} />
      </div>

      {loading ? (
        <div className="space-y-4">
          {Array.from({ length: 3 }).map((_, index) => (
            <div key={index} className="h-44 rounded-3xl bg-gray-100 animate-pulse" />
          ))}
        </div>
      ) : groupedAreas.length === 0 ? (
        <div className="rounded-3xl border border-dashed border-gray-300 bg-white px-6 py-12 text-center">
          <Building2 className="w-10 h-10 mx-auto mb-3 text-gray-300" />
          <p className="text-gray-700" style={{ fontSize: "16px", fontWeight: 600 }}>
            Chưa có khu hoặc phòng nào
          </p>
          <p className="text-gray-500 mt-1" style={{ fontSize: "14px" }}>
            Tạo khu trước hoặc tạo phòng rồi gán vào khu sau đều được.
          </p>
        </div>
      ) : (
        <div className="space-y-4">
          {groupedAreas.map((group) => {
            const isExpanded = expandedAreas[group.area.id];
            const isUnassigned = group.area.id === "unassigned";

            return (
              <div key={group.area.id} className="overflow-hidden rounded-3xl border border-gray-100 bg-white shadow-sm">
                <div className="flex flex-col gap-4 border-b border-gray-100 px-4 py-4 md:flex-row md:items-center md:justify-between">
                  <button onClick={() => toggleArea(group.area.id)} className="flex min-w-0 flex-1 items-start gap-3 text-left">
                    <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-2xl bg-orange-100 text-orange-500">
                      <Building2 className="w-5 h-5" />
                    </div>
                    <div className="min-w-0">
                      <div className="flex flex-wrap items-center gap-2">
                        <h3 className="truncate text-gray-900" style={{ fontSize: "18px", fontWeight: 700 }}>
                          {group.area.name}
                        </h3>
                        {!isUnassigned ? (
                          <span className="rounded-full bg-green-100 px-2.5 py-1 text-green-600" style={{ fontSize: "11px", fontWeight: 700 }}>
                            Hoạt động
                          </span>
                        ) : null}
                      </div>
                      <div className="mt-1 flex items-center gap-1 text-gray-500">
                        <MapPin className="w-4 h-4 text-orange-400" />
                        <span className="truncate" style={{ fontSize: "13px" }}>
                          {group.area.address}
                        </span>
                      </div>
                    </div>
                  </button>

                  <div className="flex items-center gap-5 self-end md:self-auto">
                    <InlineStat value={String(group.totalRooms)} label="Phòng" tone="text-gray-900" />
                    <InlineStat value={String(group.availableRooms)} label="Trống" tone="text-green-600" />
                    <InlineStat value={String(group.maintenanceRooms)} label="Sửa" tone="text-amber-600" />
                    <button onClick={() => toggleArea(group.area.id)} className="text-gray-400 hover:text-gray-600">
                      {isExpanded ? <ChevronDown className="w-5 h-5" /> : <ChevronRight className="w-5 h-5" />}
                    </button>
                  </div>
                </div>

                {isExpanded && (
                  <div className="px-4 py-4">
                    <div className="mb-4 flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
                      <div className="flex flex-wrap gap-2">
                        {group.area.description ? (
                          <span className="rounded-full bg-gray-100 px-3 py-1 text-gray-500" style={{ fontSize: "12px", fontWeight: 600 }}>
                            {group.area.description}
                          </span>
                        ) : null}
                      </div>

                      <div className="flex flex-wrap gap-2">
                        {!isUnassigned ? (
                          <button
                            onClick={() => openAreaEdit(group.area)}
                            className="inline-flex items-center gap-2 rounded-2xl border border-gray-200 px-4 py-2 text-gray-700 hover:bg-gray-50 transition-colors"
                            style={{ fontSize: "13px", fontWeight: 600 }}
                          >
                            <Pencil className="w-4 h-4" />
                            Sửa khu
                          </button>
                        ) : null}
                        <button
                          onClick={() => openRoomCreate(isUnassigned ? "" : group.area.id)}
                          className="inline-flex items-center gap-2 rounded-2xl border border-orange-200 bg-orange-50 px-4 py-2 text-orange-600 hover:bg-orange-100 transition-colors"
                          style={{ fontSize: "13px", fontWeight: 700 }}
                        >
                          <Plus className="w-4 h-4" />
                          Thêm phòng
                        </button>
                      </div>
                    </div>

                    {group.properties.length === 0 ? (
                      <div className="rounded-2xl border border-dashed border-gray-200 px-4 py-8 text-center text-gray-400" style={{ fontSize: "13px" }}>
                        Khu này chưa có phòng nào.
                      </div>
                    ) : (
                      <div className="space-y-4">
                        {group.properties.map((property, index) => (
                          <div key={property.id}>
                            <div className="mb-2 flex items-center gap-2">
                              <span className="rounded-full bg-gray-100 px-2 py-1 text-gray-500" style={{ fontSize: "11px", fontWeight: 700 }}>
                                P{index + 1}
                              </span>
                            </div>

                            <div className="rounded-2xl border border-gray-100 bg-gray-50 px-4 py-4">
                              <div className="flex flex-col gap-3 lg:flex-row lg:items-start lg:justify-between">
                                <div className="min-w-0">
                                  <p className="truncate text-gray-900" style={{ fontSize: "18px", fontWeight: 700 }}>
                                    {property.propertyName}
                                  </p>
                                  <div className="flex items-center gap-2 mt-1">
                                    <p className="text-gray-400" style={{ fontSize: "13px" }}>
                                      {property.size}m² • {formatCurrency(property.price)}/tháng
                                    </p>
                                    {property.totalRatings && property.totalRatings > 0 ? (
                                      <div className="flex items-center gap-1 text-orange-500" style={{ fontSize: "12px", fontWeight: 600 }}>
                                        <span className="text-gray-300">•</span>
                                        <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="currentColor" stroke="none"><polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/></svg>
                                        <span>{property.averageRating?.toFixed(1)}</span>
                                        <span className="text-gray-400 font-normal">({property.totalRatings})</span>
                                      </div>
                                    ) : null}
                                  </div>
                                  <p className="text-gray-400 mt-1" style={{ fontSize: "12px", fontWeight: 600 }}>
                                    Điện: {property.electricPrice ? `${formatCurrency(property.electricPrice)}/kWh` : "Chưa đặt"} • Nước: {property.waterPrice ? `${formatCurrency(property.waterPrice)}/m³` : "Chưa đặt"}
                                  </p>
                                  <div className="mt-3 flex flex-wrap gap-2">
                                    {property.amenities.slice(0, 4).map((amenity) => (
                                      <span key={amenity} className="rounded-full bg-white px-2.5 py-1 text-gray-500" style={{ fontSize: "11px", fontWeight: 600 }}>
                                        {amenity}
                                      </span>
                                    ))}
                                  </div>
                                </div>

                                <div className="flex flex-col items-start gap-3 lg:items-end">
                                  <RoomStatusBadge status={property.status} />
                                  <div className="flex flex-wrap gap-2">
                                    <button
                                      onClick={() => navigate(`/rooms/${property.id}?view=landlord`)}
                                      className="rounded-xl border border-gray-200 px-3 py-2 text-gray-700 hover:bg-white transition-colors"
                                      style={{ fontSize: "12px", fontWeight: 600 }}
                                    >
                                      Chi tiết
                                    </button>
                                    <button
                                      onClick={() => openRoomEdit(property)}
                                      className="inline-flex items-center gap-1 rounded-xl border border-orange-200 px-3 py-2 text-orange-600 hover:bg-orange-50 transition-colors"
                                      style={{ fontSize: "12px", fontWeight: 600 }}
                                    >
                                      <Pencil className="w-3.5 h-3.5" />
                                      Sửa
                                    </button>
                                    <button
                                      onClick={() => void handleDelete(property)}
                                      className="inline-flex items-center gap-1 rounded-xl border border-red-200 px-3 py-2 text-red-600 hover:bg-red-50 transition-colors"
                                      style={{ fontSize: "12px", fontWeight: 600 }}
                                    >
                                      <Trash2 className="w-3.5 h-3.5" />
                                      Xóa
                                    </button>
                                  </div>
                                </div>
                              </div>
                            </div>
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}

      {showAreaModal && (
        <ModalShell title={editingArea ? "Chỉnh sửa khu" : "Thêm khu mới"} onClose={closeAreaModal}>
          <Input label="Tên khu" value={areaForm.name} onChange={(value) => setAreaForm((prev) => ({ ...prev, name: value }))} />
          <Input label="Địa chỉ khu" value={areaForm.address} onChange={(value) => setAreaForm((prev) => ({ ...prev, address: value }))} />
          <TextArea label="Mô tả" value={areaForm.description} onChange={(value) => setAreaForm((prev) => ({ ...prev, description: value }))} />
          <ModalActions
            onCancel={closeAreaModal}
            onConfirm={() => void handleAreaSubmit()}
            loading={savingArea}
            confirmLabel={editingArea ? "Lưu khu" : "Tạo khu"}
          />
        </ModalShell>
      )}

      {showRoomModal && (
        <ModalShell title={editingProperty ? "Chỉnh sửa phòng" : "Thêm phòng"} onClose={closeRoomModal}>
          <Input label="Tên phòng" value={roomForm.propertyName} onChange={(value) => setRoomForm((prev) => ({ ...prev, propertyName: value }))} />
          <SelectField
            label="Thuộc khu"
            value={roomForm.areaId}
            onChange={(value) =>
              setRoomForm((prev) => ({
                ...prev,
                areaId: value,
                address: value ? areas.find((area) => area.id === value)?.address || prev.address : prev.address,
              }))
            }
            options={[
              { value: "", label: "Chưa gán khu" },
              ...areas.map((area) => ({ value: area.id, label: area.name })),
            ]}
          />
          <div className="mb-4">
            <div className="flex items-end gap-2 mb-2">
              <div className="flex-1">
                <Input 
                  label="Địa chỉ" 
                  value={roomForm.address} 
                  onChange={(value) => setRoomForm((prev) => ({ ...prev, address: value }))} 
                  onBlur={() => handleAddressGeocode(roomForm.address)}
                />
              </div>
              <button 
                type="button" 
                onClick={handleGetCurrentLocation}
                className="px-4 py-2.5 bg-blue-50 text-blue-600 border border-blue-200 rounded-2xl hover:bg-blue-100 transition whitespace-nowrap"
                style={{ fontSize: "13px", fontWeight: 600 }}
              >
                Vị trí hiện tại
              </button>
            </div>
            <div className="rounded-xl overflow-hidden border border-gray-200 relative z-0" style={{ height: "250px" }}>
              <MapContainer center={roomForm.latitude && roomForm.longitude ? [roomForm.latitude, roomForm.longitude] : [10.8231, 106.6297]} zoom={12} style={{ height: "100%", width: "100%" }}>
                <TileLayer
                  attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
                  url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                />
                {roomForm.latitude && roomForm.longitude && (
                  <MapUpdater center={[roomForm.latitude, roomForm.longitude]} />
                )}
                <LocationPicker 
                  position={roomForm.latitude && roomForm.longitude ? [roomForm.latitude, roomForm.longitude] : null} 
                  onLocationSelect={(lat, lng) => {
                    setRoomForm(prev => ({ ...prev, latitude: lat, longitude: lng }));
                  }} 
                />
              </MapContainer>
            </div>
            <p className="text-gray-500 mt-1" style={{ fontSize: "12px" }}>
              Nhập địa chỉ để tự động tìm trên bản đồ. Nếu ghim bị lệch, nhấp chuột vào bản đồ để chọn lại vị trí chính xác (địa chỉ chữ sẽ được giữ nguyên).
            </p>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <Input label="Diện tích (m²)" type="number" value={roomForm.size} onChange={(value) => setRoomForm((prev) => ({ ...prev, size: value }))} />
            <Input label="Giá thuê" type="number" value={roomForm.price} onChange={(value) => setRoomForm((prev) => ({ ...prev, price: value }))} />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <Input label="Giá điện" type="number" value={roomForm.electricPrice} onChange={(value) => setRoomForm((prev) => ({ ...prev, electricPrice: value }))} />
            <Input label="Giá nước" type="number" value={roomForm.waterPrice} onChange={(value) => setRoomForm((prev) => ({ ...prev, waterPrice: value }))} />
          </div>
          <SelectField
            label="Trạng thái"
            value={roomForm.status}
            onChange={(value) => setRoomForm((prev) => ({ ...prev, status: value }))}
            options={[
              { value: "Available", label: "Trống (Sẵn sàng cho thuê)" },
              { value: "Rented", label: "Đã cho thuê" },
              { value: "Maintenance", label: "Đang sửa chữa" },
            ]}
          />
          <AmenitySelector
            options={amenityOptions}
            selected={roomForm.amenities}
            onToggle={toggleRoomAmenity}
          />
          <ImagePicker
            images={roomImages}
            pendingFiles={pendingRoomFiles}
            removedImageIds={removedRoomImageIds}
            replacedImageFiles={replacedRoomImageFiles}
            primaryImageId={primaryRoomImageId}
            onPickFiles={handleRoomFilesSelected}
            onRemovePending={removePendingRoomFile}
            onRemoveExisting={markRoomImageForRemoval}
            onReplaceExisting={handleReplaceRoomImage}
            onSelectPrimary={setPrimaryRoomImageId}
          />
          <TextArea label="Mô tả" value={roomForm.description} onChange={(value) => setRoomForm((prev) => ({ ...prev, description: value }))} />
          <ModalActions
            onCancel={closeRoomModal}
            onConfirm={() => void handleRoomSubmit()}
            loading={savingRoom}
            confirmLabel={editingProperty ? "Lưu phòng" : "Tạo phòng"}
          />
        </ModalShell>
      )}
    </div>
  );
}

function isAvailable(status: string) {
  return isAvailablePropertyStatus(status);
}

function isRented(status: string) {
  return isRentedPropertyStatus(status);
}

function isMaintenance(status: string) {
  return isMaintenancePropertyStatus(status);
}

function uniqueValue(value: string, index: number, array: string[]) {
  return array.indexOf(value) === index;
}

async function syncRoomAmenities(
  token: string,
  propertyId: string,
  selectedAmenityNames: string[],
  amenityOptions: AmenityResponse[],
) {
  const currentAmenities = await getPropertyAmenities(propertyId);
  const selectedSet = new Set(selectedAmenityNames);
  const currentByName = new Map(currentAmenities.map((item) => [item.amenityName, item]));
  const optionByName = new Map(amenityOptions.map((item) => [item.name, item]));

  await Promise.all(
    currentAmenities
      .filter((item) => !selectedSet.has(item.amenityName))
      .map((item) => deletePropertyAmenity(token, item.id)),
  );

  await Promise.all(
    Array.from(selectedSet)
      .filter((name) => !currentByName.has(name))
      .map((name) => {
        const amenity = optionByName.get(name);
        if (!amenity) return Promise.resolve();
        return createPropertyAmenity(token, {
          propertyId,
          amenityId: amenity.id,
          status: "Working",
        });
      }),
  );
}

function SummaryCard({ tone, label, value }: { tone: "green" | "blue" | "amber"; label: string; value: string }) {
  const palette = {
    green: "bg-green-50 border-green-100 text-green-600",
    blue: "bg-blue-50 border-blue-100 text-blue-600",
    amber: "bg-amber-50 border-amber-100 text-amber-600",
  }[tone];

  return (
    <div className={`rounded-3xl border px-5 py-4 text-center ${palette}`}>
      <p style={{ fontSize: "34px", fontWeight: 700, lineHeight: 1 }}>{value}</p>
      <p className="mt-2" style={{ fontSize: "14px", fontWeight: 600 }}>
        {label}
      </p>
    </div>
  );
}

function InlineStat({ value, label, tone }: { value: string; label: string; tone: string }) {
  return (
    <div className="text-center">
      <p className={tone} style={{ fontSize: "24px", fontWeight: 700, lineHeight: 1 }}>
        {value}
      </p>
      <p className="text-gray-400 mt-1" style={{ fontSize: "12px", fontWeight: 600 }}>
        {label}
      </p>
    </div>
  );
}

function InfoPanel({ children, tone = "gray" }: { children: string; tone?: "gray" | "amber" }) {
  const palette =
    tone === "amber"
      ? "border-amber-200 bg-amber-50 text-amber-800"
      : "border-gray-200 bg-gray-50 text-gray-600";

  return (
    <div className={`rounded-2xl border px-4 py-3 ${palette}`} style={{ fontSize: "12px", lineHeight: 1.5 }}>
      {children}
    </div>
  );
}

function AmenitySelector({
  options,
  selected,
  onToggle,
}: {
  options: AmenityResponse[];
  selected: string[];
  onToggle: (name: string) => void;
}) {
  if (options.length === 0) {
    return <InfoPanel>Chưa tải được danh mục tiện ích phòng.</InfoPanel>;
  }

  return (
    <div className="space-y-2">
      <div className="flex items-center justify-between gap-3">
        <label className="block text-gray-700" style={{ fontSize: "13px", fontWeight: 600 }}>
          Tiện ích phòng
        </label>
        <span className="text-gray-400" style={{ fontSize: "12px" }}>
          Chọn các tiện ích áp dụng riêng cho phòng này
        </span>
      </div>
      <div className="flex flex-wrap gap-2">
        {options.map((option) => {
          const active = selected.includes(option.name);
          return (
            <button
              key={option.id}
              type="button"
              onClick={() => onToggle(option.name)}
              className={`rounded-full border px-3 py-1.5 transition-colors ${
                active ? "border-orange-200 bg-orange-50 text-orange-600" : "border-gray-200 bg-white text-gray-500 hover:bg-gray-50"
              }`}
              style={{ fontSize: "12px", fontWeight: 600 }}
            >
              {option.name}
            </button>
          );
        })}
      </div>
    </div>
  );
}

function ImagePicker({
  images,
  pendingFiles,
  removedImageIds,
  replacedImageFiles = {},
  primaryImageId,
  onPickFiles,
  onRemovePending,
  onRemoveExisting,
  onReplaceExisting,
  onSelectPrimary,
}: {
  images: PropertyImageResponse[];
  pendingFiles: File[];
  removedImageIds: string[];
  replacedImageFiles?: Record<string, File>;
  primaryImageId: string;
  onPickFiles: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onRemovePending: (file: File) => void;
  onRemoveExisting: (id: string) => void;
  onReplaceExisting?: (id: string) => void;
  onSelectPrimary: (id: string) => void;
}) {
  const visibleImages = images.filter((item) => !removedImageIds.includes(item.id));

  return (
    <div className="space-y-3">
      <div className="flex items-center justify-between gap-3">
        <label className="block text-gray-700" style={{ fontSize: "13px", fontWeight: 600 }}>
          Ảnh phòng
        </label>
        <label className="inline-flex cursor-pointer items-center gap-2 rounded-xl border border-orange-200 bg-orange-50 px-3 py-2 text-orange-600 hover:bg-orange-100" style={{ fontSize: "12px", fontWeight: 700 }}>
          <ImagePlus className="w-4 h-4" />
          Thêm ảnh
          <input type="file" accept="image/*" multiple className="hidden" onChange={onPickFiles} />
        </label>
      </div>

      {visibleImages.length === 0 && pendingFiles.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-gray-200 px-4 py-8 text-center text-gray-400" style={{ fontSize: "12px" }}>
          Chưa có ảnh nào cho phòng này.
        </div>
      ) : (
        <div className="grid grid-cols-2 gap-3 md:grid-cols-3">
          {visibleImages.map((image) => {
            const replacementFile = replacedImageFiles[image.id];
            const displaySrc = replacementFile ? URL.createObjectURL(replacementFile) : image.imageUrl;
            const isReplaced = !!replacementFile;
            return (
              <div key={image.id} className="overflow-hidden rounded-2xl border border-gray-200 bg-white">
                <img src={displaySrc} alt="" className="h-28 w-full object-cover" />
                <div className="flex items-center justify-between gap-2 p-2">
                  <button
                    type="button"
                    onClick={() => onSelectPrimary(image.id)}
                    className={`inline-flex items-center gap-1 rounded-full px-2.5 py-1 ${
                      primaryImageId === image.id ? "bg-amber-100 text-amber-700" : "bg-gray-100 text-gray-500"
                    }`}
                    style={{ fontSize: "11px", fontWeight: 700 }}
                  >
                    <Star className="w-3.5 h-3.5" />
                    Ảnh chính
                  </button>
                  <div className="flex items-center gap-1.5">
                    {onReplaceExisting && (
                      <button
                        type="button"
                        onClick={() => onReplaceExisting(image.id)}
                        className="text-gray-500 hover:text-orange-600"
                        title="Thay ảnh này"
                      >
                        <Pencil className="w-4 h-4" />
                      </button>
                    )}
                    <button type="button" onClick={() => onRemoveExisting(image.id)} className="text-red-500 hover:text-red-600">
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </div>
                </div>
                {isReplaced && (
                  <div className="px-2 pb-2">
                    <span className="inline-block rounded bg-orange-100 px-1.5 py-0.5 text-[10px] font-semibold text-orange-700">
                      Sẽ thay thế
                    </span>
                  </div>
                )}
              </div>
            );
          })}

          {pendingFiles.map((file, index) => (
            <div key={`${file.name}-${index}`} className="overflow-hidden rounded-2xl border border-gray-200 bg-white">
              <img src={URL.createObjectURL(file)} alt="" className="h-28 w-full object-cover" />
              <div className="flex items-center justify-between gap-2 p-2">
                <span className="truncate text-gray-500" style={{ fontSize: "11px", fontWeight: 600 }}>
                  Ảnh mới
                </span>
                <button type="button" onClick={() => onRemovePending(file)} className="text-red-500 hover:text-red-600">
                  <Trash2 className="w-4 h-4" />
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

function RoomStatusBadge({ status }: { status: string }) {
  const normalized = status.toLowerCase();
  if (isAvailable(normalized)) {
    return (
      <span className="inline-flex items-center gap-1 rounded-full bg-green-100 px-3 py-1 text-green-600" style={{ fontSize: "12px", fontWeight: 700 }}>
        <ChevronRight className="w-3.5 h-3.5 rotate-90" />
        Còn trống
      </span>
    );
  }

  if (isRented(normalized)) {
    return (
      <span className="inline-flex items-center gap-1 rounded-full bg-blue-100 px-3 py-1 text-blue-600" style={{ fontSize: "12px", fontWeight: 700 }}>
        <Building2 className="w-3.5 h-3.5" />
        Đã thuê
      </span>
    );
  }

  return (
    <span className="inline-flex items-center gap-1 rounded-full bg-amber-100 px-3 py-1 text-amber-600" style={{ fontSize: "12px", fontWeight: 700 }}>
      <Hammer className="w-3.5 h-3.5" />
      Đang sửa chữa
    </span>
  );
}

function ModalShell({ title, onClose, children }: { title: string; onClose: () => void; children: React.ReactNode }) {
  return (
    <div className="fixed inset-0 z-50 overflow-y-auto bg-black/50 p-4 backdrop-blur-sm">
      <div className="flex min-h-full items-center justify-center">
        <div className="flex w-full max-w-lg max-h-[calc(100vh-2rem)] flex-col overflow-hidden rounded-3xl bg-white shadow-2xl">
          <div className="flex items-center justify-between border-b border-gray-100 px-6 py-4 shrink-0">
            <h3 className="text-gray-900" style={{ fontSize: "17px", fontWeight: 700 }}>
              {title}
            </h3>
            <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
              <X className="w-5 h-5" />
            </button>
          </div>
          <div className="space-y-4 overflow-y-auto p-6">{children}</div>
        </div>
      </div>
    </div>
  );
}

function ModalActions({
  onCancel,
  onConfirm,
  loading,
  confirmLabel,
}: {
  onCancel: () => void;
  onConfirm: () => void;
  loading: boolean;
  confirmLabel: string;
}) {
  return (
    <div className="sticky bottom-0 flex gap-3 border-t border-gray-100 bg-white pt-4">
      <button
        onClick={onCancel}
        className="flex-1 py-3 rounded-2xl border border-gray-200 text-gray-600 hover:bg-gray-50 transition-colors"
        style={{ fontSize: "14px" }}
      >
        Hủy
      </button>
      <button
        onClick={onConfirm}
        disabled={loading}
        className="flex-1 py-3 rounded-2xl bg-orange-500 text-white hover:bg-orange-600 transition-colors disabled:opacity-70 inline-flex items-center justify-center gap-2"
        style={{ fontSize: "14px", fontWeight: 600 }}
      >
        {loading && <LoaderCircle className="w-4 h-4 animate-spin" />}
        {confirmLabel}
      </button>
    </div>
  );
}

function Input({
  label,
  value,
  onChange,
  onBlur,
  type = "text",
}: {
  label: string;
  value: string;
  onChange: (value: string) => void;
  onBlur?: () => void;
  type?: string;
}) {
  return (
    <div>
      <label className="block text-gray-700 mb-1.5" style={{ fontSize: "13px", fontWeight: 500 }}>
        {label}
      </label>
      <input
        type={type}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        onBlur={onBlur}
        className="w-full px-4 py-2.5 rounded-2xl border border-gray-200 bg-gray-50 focus:outline-none"
      />
    </div>
  );
}

function TextArea({ label, value, onChange }: { label: string; value: string; onChange: (value: string) => void }) {
  return (
    <div>
      <label className="block text-gray-700 mb-1.5" style={{ fontSize: "13px", fontWeight: 500 }}>
        {label}
      </label>
      <textarea
        value={value}
        onChange={(e) => onChange(e.target.value)}
        rows={4}
        className="w-full px-4 py-2.5 rounded-2xl border border-gray-200 bg-gray-50 focus:outline-none resize-none"
      />
    </div>
  );
}

function SelectField({
  label,
  value,
  onChange,
  options,
}: {
  label: string;
  value: string;
  onChange: (value: string) => void;
  options: Array<{ value: string; label: string }>;
}) {
  return (
    <div>
      <label className="block text-gray-700 mb-1.5" style={{ fontSize: "13px", fontWeight: 500 }}>
        {label}
      </label>
      <select
        value={value}
        onChange={(e) => onChange(e.target.value)}
        className="w-full px-4 py-2.5 rounded-2xl border border-gray-200 bg-gray-50 focus:outline-none"
      >
        {options.map((option) => (
          <option key={option.value || "empty"} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
    </div>
  );
}
