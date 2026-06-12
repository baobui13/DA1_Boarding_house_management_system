import { useEffect, useState } from "react";
import { useParams, useNavigate, Link } from "react-router";
import { ArrowLeft, Save, Home, MapPin, DollarSign, Maximize2, AlertTriangle, ImagePlus, Star, Trash2, Pencil } from "lucide-react";
import { useApp } from "../../context/AppContext";
import { getAmenities } from "../../lib/amenities";
import { getAreas } from "../../lib/areas";
import {
  createProperty,
  createPropertyAmenity,
  createPropertyImage,
  deletePropertyImage,
  deletePropertyAmenity,
  getPropertyAmenities,
  getPropertyById,
  getPropertyImages,
  replacePropertyImage,
  updatePropertyImage,
  updateProperty,
} from "../../lib/properties";
import type { AmenityResponse, AreaResponse, PropertyImageResponse } from "../../lib/types";

export default function RoomFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { token, currentUser } = useApp();
  const isEdit = !!id;
  const [loading, setLoading] = useState(isEdit);
  const [error, setError] = useState("");
  const [areas, setAreas] = useState<AreaResponse[]>([]);
  const [amenities, setAmenities] = useState<AmenityResponse[]>([]);
  const [images, setImages] = useState<PropertyImageResponse[]>([]);
  const [newFiles, setNewFiles] = useState<File[]>([]);
  const [removedImageIds, setRemovedImageIds] = useState<string[]>([]);
  const [replacedImageFiles, setReplacedImageFiles] = useState<Record<string, File>>({});
  const [primaryImageId, setPrimaryImageId] = useState<string>("");
  const [formData, setFormData] = useState({
    propertyName: "",
    address: "",
    price: 0,
    size: 0,
    electricPrice: 0,
    waterPrice: 0,
    status: "Available",
    description: "",
    areaId: "",
    amenities: [] as string[],
  });

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const [areaResponse, amenityResponse, property, roomAmenities, propertyImages] = await Promise.all([
          currentUser ? getAreas({ landlordId: currentUser.id }) : Promise.resolve({ items: [] as AreaResponse[] }),
          getAmenities(),
          isEdit && id ? getPropertyById(id) : Promise.resolve(null),
          isEdit && id ? getPropertyAmenities(id) : Promise.resolve([]),
          isEdit && id ? getPropertyImages(id) : Promise.resolve([]),
        ]);
        if (!cancelled) {
          setAreas(areaResponse.items);
          setAmenities(amenityResponse);
          if (property) {
            setFormData({
              propertyName: property.propertyName,
              address: property.address || "",
              price: Number(property.price),
              size: Number(property.size),
              electricPrice: Number(property.electricPrice || 0),
              waterPrice: Number(property.waterPrice || 0),
              status: property.status,
              description: property.description || "",
              areaId: property.areaId || "",
              amenities: roomAmenities.map((item) => item.amenityName),
            });
          }
          // Defensive filter to this property only
          const safeImages = (propertyImages || []).filter((img: any) => (img.propertyId || img.PropertyId) === id);
          setImages(safeImages);
          setPrimaryImageId(safeImages.find((item) => item.isPrimary)?.id || safeImages[0]?.id || "");
        }
      } catch (err) {
        if (!cancelled) setError(err instanceof Error ? err.message : "Không tải được dữ liệu.");
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [currentUser, id, isEdit]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: ["price", "size", "electricPrice", "waterPrice"].includes(name) ? Number(value) || 0 : value,
    }));
  };

  const toggleAmenity = (name: string) => {
    setFormData((prev) => ({
      ...prev,
      amenities: prev.amenities.includes(name)
        ? prev.amenities.filter((item) => item !== name)
        : [...prev.amenities, name],
    }));
  };

  const handleFilesSelected = (e: React.ChangeEvent<HTMLInputElement>) => {
    const incoming = Array.from(e.target.files || []).filter((file) => file.type.startsWith("image/"));
    setNewFiles((prev) => [...prev, ...incoming]);
    e.target.value = "";
  };

  const removePendingFile = (targetFile: File) => {
    setNewFiles((prev) => prev.filter((file) => file !== targetFile));
  };

  const markImageForRemoval = (imageId: string) => {
    setRemovedImageIds((prev) => (prev.includes(imageId) ? prev : [...prev, imageId]));
    setReplacedImageFiles((prev) => {
      const next = { ...prev };
      delete next[imageId];
      return next;
    });
    if (primaryImageId === imageId) {
      const fallback = images.find((item) => item.id !== imageId && !removedImageIds.includes(item.id));
      setPrimaryImageId(fallback?.id || "");
    }
  };

  const handleReplaceImage = (imageId: string) => {
    const input = document.createElement("input");
    input.type = "file";
    input.accept = "image/*";
    input.onchange = (e) => {
      const file = (e.target as HTMLInputElement).files?.[0];
      if (file && file.type.startsWith("image/")) {
        setReplacedImageFiles((prev) => ({ ...prev, [imageId]: file }));
      }
      input.value = "";
    };
    input.click();
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!token || !currentUser) {
      setError("Thiếu token hoặc người dùng đăng nhập.");
      return;
    }

    try {
      let propertyId = id;
      if (isEdit && id) {
        await updateProperty(token, { id, ...formData });
      } else {
        const created = await createProperty(token, { landlordId: currentUser.id, ...formData });
        propertyId = created.id;
      }

      if (propertyId) {
        await syncRoomAmenities(token, propertyId, formData.amenities, amenities);
        await Promise.all(removedImageIds.map((imageId) => deletePropertyImage(token, imageId)));

        // Replace existing images (keeps the same image record id + primary status) - parallel
        const replaceEntries = Object.entries(replacedImageFiles);
        if (replaceEntries.length > 0) {
          await Promise.all(
            replaceEntries.map(([imageId, file]) => replacePropertyImage(token, { id: imageId, file }))
          );
        }

        // Add new images in parallel (major speed improvement)
        // Compute remaining existing count client-side to avoid extra refetch + sequential awaits
        const remainingExistingCount = (images || []).filter(
          (img: any) => !removedImageIds.includes(img.id)
        ).length;

        if (newFiles.length > 0) {
          await Promise.all(
            newFiles.map((file, index) =>
              createPropertyImage(token, {
                propertyId,
                file,
                isPrimary: remainingExistingCount === 0 && index === 0,
              })
            )
          );
        }

        if (primaryImageId) {
          await updatePropertyImage(token, {
            id: primaryImageId,
            isPrimary: true,
          });
        }
      }
      navigate("/landlord/properties");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Lưu thất bại.");
    }
  };

  return (
    <div className="max-w-4xl mx-auto px-4 py-6">
      <div className="mb-6">
        <Link to="/landlord/properties" className="inline-flex items-center gap-2 text-gray-600 hover:text-gray-900 mb-4 transition-colors" style={{ fontSize: "14px", fontWeight: 500 }}>
          <ArrowLeft className="w-4 h-4" />
          Quay lại danh sách tài sản
        </Link>
        <h1 className="text-gray-900" style={{ fontSize: "22px", fontWeight: 700 }}>
          {isEdit ? "Chỉnh Sửa Tin / Tài Sản" : "Tạo Tin / Tài Sản"}
        </h1>
        <p className="text-gray-500 mt-0.5" style={{ fontSize: "14px" }}>
          Route cũ tên `RoomFormPage`, nhưng backend hiện đang làm việc với `Property`. Màn này đã chuyển sang lưu `Property` thật.
        </p>
      </div>

      <div className="mb-6 rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-amber-800">
        <div className="flex items-start gap-3">
          <AlertTriangle className="w-5 h-5 mt-0.5 shrink-0" />
          <p style={{ fontSize: "13px" }}>
            Backend hiện có ảnh phòng qua `PropertyImage` và tiện ích phòng qua `RoomAmenity`. Ảnh khu và tiện ích khu vẫn chưa có API riêng.
          </p>
        </div>
      </div>

      {error && <div className="mb-4 rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-red-600">{error}</div>}

      {loading ? (
        <div className="h-64 rounded-2xl bg-gray-100 animate-pulse" />
      ) : (
        <form onSubmit={handleSubmit} className="space-y-6">
          <div className="bg-white rounded-2xl border border-gray-100 p-6">
            <h2 className="text-gray-900 mb-4 flex items-center gap-2" style={{ fontSize: "16px", fontWeight: 600 }}>
              <Home className="w-5 h-5 text-orange-500" />
              Thông tin cơ bản
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="md:col-span-2">
                <label className="block text-gray-700 mb-1.5" style={{ fontSize: "13px", fontWeight: 500 }}>Tên tài sản</label>
                <input name="propertyName" value={formData.propertyName} onChange={handleChange} required className="w-full px-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none" />
              </div>
              <div className="md:col-span-2">
                <label className="block text-gray-700 mb-1.5" style={{ fontSize: "13px", fontWeight: 500 }}>Địa chỉ</label>
                <div className="relative">
                  <MapPin className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                  <input name="address" value={formData.address} onChange={handleChange} className="w-full pl-10 pr-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none" />
                </div>
              </div>
              <div>
                <label className="block text-gray-700 mb-1.5" style={{ fontSize: "13px", fontWeight: 500 }}>Giá thuê</label>
                <div className="relative">
                  <DollarSign className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                  <input type="number" name="price" value={formData.price} onChange={handleChange} required className="w-full pl-10 pr-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none" />
                </div>
              </div>
              <div>
                <label className="block text-gray-700 mb-1.5" style={{ fontSize: "13px", fontWeight: 500 }}>Diện tích</label>
                <div className="relative">
                  <Maximize2 className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                  <input type="number" name="size" value={formData.size} onChange={handleChange} required className="w-full pl-10 pr-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none" />
                </div>
              </div>
              <div>
                <label className="block text-gray-700 mb-1.5" style={{ fontSize: "13px", fontWeight: 500 }}>Giá điện</label>
                <div className="relative">
                  <DollarSign className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                  <input type="number" name="electricPrice" value={formData.electricPrice} onChange={handleChange} className="w-full pl-10 pr-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none" />
                </div>
              </div>
              <div>
                <label className="block text-gray-700 mb-1.5" style={{ fontSize: "13px", fontWeight: 500 }}>Giá nước</label>
                <div className="relative">
                  <DollarSign className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                  <input type="number" name="waterPrice" value={formData.waterPrice} onChange={handleChange} className="w-full pl-10 pr-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none" />
                </div>
              </div>
              <div className="md:col-span-2">
                <label className="block text-gray-700 mb-1.5" style={{ fontSize: "13px", fontWeight: 500 }}>Thuộc khu</label>
                <select name="areaId" value={formData.areaId} onChange={handleChange} className="w-full px-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none">
                  <option value="">Chưa gán khu</option>
                  {areas.map((area) => (
                    <option key={area.id} value={area.id}>
                      {area.name}
                    </option>
                  ))}
                </select>
              </div>
              <div className="md:col-span-2">
                <label className="block text-gray-700 mb-1.5" style={{ fontSize: "13px", fontWeight: 500 }}>Trạng thái</label>
                <select name="status" value={formData.status} onChange={handleChange} className="w-full px-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none">
                  <option value="Available">Available</option>
                  <option value="PendingApproval">PendingApproval</option>
                  <option value="Approved">Approved</option>
                  <option value="Rented">Rented</option>
                  <option value="Unavailable">Unavailable</option>
                  <option value="Rejected">Rejected</option>
                </select>
              </div>
              <div className="md:col-span-2">
                <label className="block text-gray-700 mb-2" style={{ fontSize: "13px", fontWeight: 500 }}>Tiện ích phòng</label>
                <div className="flex flex-wrap gap-2">
                  {amenities.map((amenity) => {
                    const active = formData.amenities.includes(amenity.name);
                    return (
                      <button
                        key={amenity.id}
                        type="button"
                        onClick={() => toggleAmenity(amenity.name)}
                        className={`rounded-full border px-3 py-1.5 transition-colors ${
                          active ? "border-orange-200 bg-orange-50 text-orange-600" : "border-gray-200 bg-white text-gray-500 hover:bg-gray-100"
                        }`}
                        style={{ fontSize: "12px", fontWeight: 600 }}
                      >
                        {amenity.name}
                      </button>
                    );
                  })}
                </div>
              </div>
              <div className="md:col-span-2 space-y-3">
                <div className="flex items-center justify-between gap-3">
                  <label className="block text-gray-700" style={{ fontSize: "13px", fontWeight: 500 }}>Ảnh phòng</label>
                  <label className="inline-flex cursor-pointer items-center gap-2 rounded-xl border border-orange-200 bg-orange-50 px-3 py-2 text-orange-600 hover:bg-orange-100" style={{ fontSize: "12px", fontWeight: 700 }}>
                    <ImagePlus className="w-4 h-4" />
                    Thêm ảnh
                    <input type="file" accept="image/*" multiple className="hidden" onChange={handleFilesSelected} />
                  </label>
                </div>

                <div className="rounded-2xl border border-gray-100 bg-gray-50 p-4">
                  <p className="text-gray-500 mb-3" style={{ fontSize: "12px" }}>
                    Khu hiện chưa có API ảnh riêng. Ảnh bên dưới chỉ áp dụng cho phòng/tài sản.
                  </p>

                  {images.filter((item) => !removedImageIds.includes(item.id)).length === 0 && newFiles.length === 0 ? (
                    <div className="rounded-2xl border border-dashed border-gray-200 px-4 py-8 text-center text-gray-400" style={{ fontSize: "12px" }}>
                      Chưa có ảnh nào cho phòng này.
                    </div>
                  ) : (
                    <div className="grid grid-cols-2 gap-3 md:grid-cols-3">
                      {images
                        .filter((item) => !removedImageIds.includes(item.id))
                        .map((image) => {
                          const replacementFile = replacedImageFiles[image.id];
                          const displaySrc = replacementFile ? URL.createObjectURL(replacementFile) : image.imageUrl;
                          const isReplaced = !!replacementFile;
                          return (
                            <div key={image.id} className="overflow-hidden rounded-2xl border border-gray-200 bg-white">
                              <img src={displaySrc} alt="" className="h-28 w-full object-cover" />
                              <div className="flex items-center justify-between gap-2 p-2">
                                <button
                                  type="button"
                                  onClick={() => setPrimaryImageId(image.id)}
                                  className={`inline-flex items-center gap-1 rounded-full px-2.5 py-1 ${
                                    primaryImageId === image.id ? "bg-amber-100 text-amber-700" : "bg-gray-100 text-gray-500"
                                  }`}
                                  style={{ fontSize: "11px", fontWeight: 700 }}
                                >
                                  <Star className="w-3.5 h-3.5" />
                                  Ảnh chính
                                </button>
                                <div className="flex items-center gap-1.5">
                                  <button
                                    type="button"
                                    onClick={() => handleReplaceImage(image.id)}
                                    className="text-gray-500 hover:text-orange-600"
                                    title="Thay ảnh này"
                                  >
                                    <Pencil className="w-4 h-4" />
                                  </button>
                                  <button type="button" onClick={() => markImageForRemoval(image.id)} className="text-red-500 hover:text-red-600">
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

                      {newFiles.map((file, index) => (
                        <div key={`${file.name}-${index}`} className="overflow-hidden rounded-2xl border border-gray-200 bg-white">
                          <img src={URL.createObjectURL(file)} alt="" className="h-28 w-full object-cover" />
                          <div className="flex items-center justify-between gap-2 p-2">
                            <span className="truncate text-gray-500" style={{ fontSize: "11px", fontWeight: 600 }}>
                              Ảnh mới
                            </span>
                            <button type="button" onClick={() => removePendingFile(file)} className="text-red-500 hover:text-red-600">
                              <Trash2 className="w-4 h-4" />
                            </button>
                          </div>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </div>
              <div className="md:col-span-2">
                <label className="block text-gray-700 mb-1.5" style={{ fontSize: "13px", fontWeight: 500 }}>Mô tả</label>
                <textarea name="description" value={formData.description} onChange={handleChange} rows={5} className="w-full px-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none resize-none" />
              </div>
            </div>
          </div>

          <div className="flex justify-end">
            <button type="submit" className="flex items-center gap-2 px-6 py-3 rounded-xl bg-orange-500 text-white hover:bg-orange-600 transition-colors" style={{ fontSize: "14px", fontWeight: 600 }}>
              <Save className="w-4 h-4" />
              {isEdit ? "Lưu thay đổi" : "Tạo mới"}
            </button>
          </div>
        </form>
      )}
    </div>
  );
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
