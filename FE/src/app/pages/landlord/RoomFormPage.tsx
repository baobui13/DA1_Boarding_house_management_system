import { useEffect, useState } from "react";
import { useParams, useNavigate, Link } from "react-router";
import { ArrowLeft, Save, Home, MapPin, DollarSign, Maximize2, AlertTriangle } from "lucide-react";
import { useApp } from "../../context/AppContext";
import { createProperty, getPropertyById, updateProperty } from "../../lib/properties";

export default function RoomFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { token, currentUser } = useApp();
  const isEdit = !!id;
  const [loading, setLoading] = useState(isEdit);
  const [error, setError] = useState("");
  const [formData, setFormData] = useState({
    propertyName: "",
    address: "",
    price: 0,
    size: 0,
    status: "Available",
    description: "",
  });

  useEffect(() => {
    if (!isEdit || !id) return;

    let cancelled = false;
    (async () => {
      try {
        const property = await getPropertyById(id);
        if (!cancelled) {
          setFormData({
            propertyName: property.propertyName,
            address: property.address || "",
            price: Number(property.price),
            size: Number(property.size),
            status: property.status,
            description: property.description || "",
          });
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
  }, [id, isEdit]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: ["price", "size"].includes(name) ? Number(value) || 0 : value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!token || !currentUser) {
      setError("Thiếu token hoặc người dùng đăng nhập.");
      return;
    }

    try {
      if (isEdit && id) {
        await updateProperty(token, { id, ...formData });
      } else {
        await createProperty(token, { landlordId: currentUser.id, ...formData });
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
            Backend chưa có model/API `Room` riêng, nên form này chỉ lưu các field có thật của `Property`: tên, địa chỉ, giá, diện tích, status, mô tả.
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
