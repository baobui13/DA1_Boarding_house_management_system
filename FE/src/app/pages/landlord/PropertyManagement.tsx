import { useEffect, useState } from "react";
import { useNavigate } from "react-router";
import { Plus, Building2, Pencil, Trash2, X, MapPin, LoaderCircle } from "lucide-react";
import { useApp } from "../../context/AppContext";
import { createProperty, deleteProperty, getPropertyListings, updateProperty } from "../../lib/properties";
import type { PropertyListing } from "../../lib/types";
import { formatCurrency } from "../../lib/format";

type PropertyForm = {
  propertyName: string;
  address: string;
  size: string;
  price: string;
  description: string;
  status: string;
};

const initialForm: PropertyForm = {
  propertyName: "",
  address: "",
  size: "",
  price: "",
  description: "",
  status: "Available",
};

export default function PropertyManagement() {
  const { currentUser, token } = useApp();
  const navigate = useNavigate();
  const [properties, setProperties] = useState<PropertyListing[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);
  const [showAddModal, setShowAddModal] = useState(false);
  const [editingProperty, setEditingProperty] = useState<PropertyListing | null>(null);
  const [form, setForm] = useState<PropertyForm>(initialForm);

  const loadProperties = async () => {
    if (!currentUser) return;

    setLoading(true);
    setError("");

    try {
      const response = await getPropertyListings({ landlordId: currentUser.id });
      setProperties(response.items);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Khong tai duoc du lieu.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadProperties();
  }, [currentUser?.id]);

  const openCreate = () => {
    setForm(initialForm);
    setEditingProperty(null);
    setShowAddModal(true);
  };

  const openEdit = (property: PropertyListing) => {
    setEditingProperty(property);
    setForm({
      propertyName: property.propertyName,
      address: property.address || "",
      size: String(property.size),
      price: String(property.price),
      description: property.description || "",
      status: property.status,
    });
    setShowAddModal(true);
  };

  const closeModal = () => {
    setShowAddModal(false);
    setEditingProperty(null);
    setForm(initialForm);
  };

  const handleSubmit = async () => {
    if (!token || !currentUser) {
      setError("Bạn cần đăng nhập lại để thao tác.");
      return;
    }

    setSaving(true);
    setError("");

    try {
      if (editingProperty) {
        await updateProperty(token, {
          id: editingProperty.id,
          propertyName: form.propertyName,
          address: form.address,
          size: Number(form.size),
          price: Number(form.price),
          description: form.description,
          status: form.status,
        });
      } else {
        await createProperty(token, {
          landlordId: currentUser.id,
          propertyName: form.propertyName,
          address: form.address,
          size: Number(form.size),
          price: Number(form.price),
          description: form.description,
          status: form.status,
        });
      }

      closeModal();
      await loadProperties();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Luu tai san that bai.");
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (property: PropertyListing) => {
    if (!token) {
      setError("Bạn cần đăng nhập lại để xóa.");
      return;
    }

    if (!confirm(`Xóa tin "${property.propertyName}"?`)) return;

    try {
      await deleteProperty(token, property.id);
      await loadProperties();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Xoa that bai.");
    }
  };

  return (
    <div className="max-w-5xl mx-auto px-4 py-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-gray-900" style={{ fontSize: "22px", fontWeight: 700 }}>
            Quản Lý Tài Sản
          </h1>
          <p className="text-gray-500 mt-0.5" style={{ fontSize: "14px" }}>
            Dữ liệu đang bám đúng `PropertyController` của backend.
          </p>
        </div>
        <button
          onClick={openCreate}
          className="flex items-center gap-2 px-4 py-2.5 bg-orange-500 text-white rounded-xl hover:bg-orange-600 transition-colors shadow-sm shadow-orange-200"
          style={{ fontSize: "14px", fontWeight: 600 }}
        >
          <Plus className="w-4 h-4" />
          Thêm tài sản
        </button>
      </div>

      {error && (
        <div className="mb-4 rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-red-600">{error}</div>
      )}

      {loading ? (
        <div className="space-y-4">
          {Array.from({ length: 3 }).map((_, index) => (
            <div key={index} className="h-32 rounded-2xl bg-gray-100 animate-pulse" />
          ))}
        </div>
      ) : properties.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-gray-300 bg-white px-6 py-10 text-center">
          <Building2 className="w-10 h-10 mx-auto mb-3 text-gray-300" />
          <p className="text-gray-700" style={{ fontSize: "16px", fontWeight: 600 }}>
            Chưa có tài sản nào
          </p>
          <p className="text-gray-500 mt-1" style={{ fontSize: "14px" }}>
            Tạo mới từ modal phía trên để gửi trực tiếp tới API.
          </p>
        </div>
      ) : (
        <div className="grid grid-cols-1 gap-4">
          {properties.map((property) => (
            <div key={property.id} className="bg-white rounded-2xl border border-gray-100 overflow-hidden">
              <div className="grid grid-cols-1 md:grid-cols-[220px,1fr]">
                <img
                  src={property.images[0] || "https://placehold.co/600x400?text=No+Image"}
                  alt={property.propertyName}
                  className="w-full h-full object-cover min-h-[180px]"
                />
                <div className="p-5">
                  <div className="flex items-start justify-between gap-4">
                    <div>
                      <h3 className="text-gray-900" style={{ fontSize: "18px", fontWeight: 700 }}>
                        {property.propertyName}
                      </h3>
                      <div className="flex items-center gap-1 text-gray-500 mt-1">
                        <MapPin className="w-4 h-4 text-orange-400" />
                        <span style={{ fontSize: "13px" }}>{property.address || "Chưa có địa chỉ"}</span>
                      </div>
                    </div>
                    <span className="px-3 py-1 rounded-xl bg-orange-50 text-orange-600 capitalize" style={{ fontSize: "12px", fontWeight: 600 }}>
                      {property.status}
                    </span>
                  </div>

                  <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 mt-4">
                    <Metric label="Giá thuê" value={formatCurrency(property.price)} />
                    <Metric label="Diện tích" value={`${property.size}m²`} />
                    <Metric label="Tiện nghi" value={String(property.amenities.length)} />
                    <Metric label="Ngày tạo" value={new Date(property.createdAt).toLocaleDateString("vi-VN")} />
                  </div>

                  <p className="text-gray-600 mt-4 line-clamp-3" style={{ fontSize: "14px", lineHeight: 1.7 }}>
                    {property.description || "Backend chưa lưu mô tả cho tài sản này."}
                  </p>

                  <div className="flex flex-wrap gap-2 mt-4">
                    {property.amenities.slice(0, 5).map((amenity) => (
                      <span key={amenity} className="px-2.5 py-1 rounded-lg bg-gray-100 text-gray-600" style={{ fontSize: "12px" }}>
                        {amenity}
                      </span>
                    ))}
                  </div>

                  <div className="flex flex-wrap gap-2 mt-5">
                    <button
                      onClick={() => navigate(`/rooms/${property.id}?view=landlord`)}
                      className="px-4 py-2 rounded-xl border border-gray-200 text-gray-700 hover:bg-gray-50 transition-colors"
                      style={{ fontSize: "13px", fontWeight: 600 }}
                    >
                      Xem chi tiết
                    </button>
                    <button
                      onClick={() => openEdit(property)}
                      className="flex items-center gap-2 px-4 py-2 rounded-xl border border-orange-200 text-orange-600 hover:bg-orange-50 transition-colors"
                      style={{ fontSize: "13px", fontWeight: 600 }}
                    >
                      <Pencil className="w-4 h-4" />
                      Chỉnh sửa
                    </button>
                    <button
                      onClick={() => handleDelete(property)}
                      className="flex items-center gap-2 px-4 py-2 rounded-xl border border-red-200 text-red-600 hover:bg-red-50 transition-colors"
                      style={{ fontSize: "13px", fontWeight: 600 }}
                    >
                      <Trash2 className="w-4 h-4" />
                      Xóa
                    </button>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {showAddModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
          <div className="bg-white rounded-2xl shadow-2xl w-full max-w-lg">
            <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100">
              <h3 className="text-gray-900" style={{ fontSize: "17px", fontWeight: 700 }}>
                {editingProperty ? "Chỉnh sửa tài sản" : "Thêm tài sản"}
              </h3>
              <button onClick={closeModal} className="text-gray-400 hover:text-gray-600">
                <X className="w-5 h-5" />
              </button>
            </div>
            <div className="p-6 space-y-4">
              <Input label="Tên tài sản" value={form.propertyName} onChange={(value) => setForm({ ...form, propertyName: value })} />
              <Input label="Địa chỉ" value={form.address} onChange={(value) => setForm({ ...form, address: value })} />
              <div className="grid grid-cols-2 gap-3">
                <Input label="Diện tích (m²)" type="number" value={form.size} onChange={(value) => setForm({ ...form, size: value })} />
                <Input label="Giá thuê" type="number" value={form.price} onChange={(value) => setForm({ ...form, price: value })} />
              </div>
              <div>
                <label className="block text-gray-700 mb-1.5" style={{ fontSize: "13px", fontWeight: 500 }}>
                  Trạng thái
                </label>
                <select
                  value={form.status}
                  onChange={(e) => setForm({ ...form, status: e.target.value })}
                  className="w-full px-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none"
                >
                  <option value="Available">Available</option>
                  <option value="Approved">Approved</option>
                  <option value="Rented">Rented</option>
                  <option value="Unavailable">Unavailable</option>
                </select>
              </div>
              <div>
                <label className="block text-gray-700 mb-1.5" style={{ fontSize: "13px", fontWeight: 500 }}>
                  Mô tả
                </label>
                <textarea
                  value={form.description}
                  onChange={(e) => setForm({ ...form, description: e.target.value })}
                  rows={4}
                  className="w-full px-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none resize-none"
                />
              </div>
              <div className="flex gap-3 pt-2">
                <button
                  onClick={closeModal}
                  className="flex-1 py-3 rounded-xl border border-gray-200 text-gray-600 hover:bg-gray-50 transition-colors"
                  style={{ fontSize: "14px" }}
                >
                  Hủy
                </button>
                <button
                  onClick={handleSubmit}
                  disabled={saving}
                  className="flex-1 py-3 rounded-xl bg-orange-500 text-white hover:bg-orange-600 transition-colors disabled:opacity-70 inline-flex items-center justify-center gap-2"
                  style={{ fontSize: "14px", fontWeight: 600 }}
                >
                  {saving && <LoaderCircle className="w-4 h-4 animate-spin" />}
                  {editingProperty ? "Lưu thay đổi" : "Tạo tài sản"}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

function Metric({ label, value }: { label: string; value: string }) {
  return (
    <div className="bg-gray-50 rounded-xl px-4 py-3">
      <p className="text-gray-500" style={{ fontSize: "11px" }}>
        {label}
      </p>
      <p className="text-gray-900 mt-1" style={{ fontSize: "14px", fontWeight: 600 }}>
        {value}
      </p>
    </div>
  );
}

function Input({
  label,
  value,
  onChange,
  type = "text",
}: {
  label: string;
  value: string;
  onChange: (value: string) => void;
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
        className="w-full px-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none"
      />
    </div>
  );
}
