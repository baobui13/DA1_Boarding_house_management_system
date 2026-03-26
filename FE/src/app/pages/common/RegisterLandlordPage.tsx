import { useState } from "react";
import { useNavigate, Link } from "react-router";
import { Building2, Eye, EyeOff, Mail, Lock, User, Phone, ArrowLeft, MapPin, FileText } from "lucide-react";
import { useApp } from "../../context/AppContext";

export default function RegisterLandlordPage() {
  const { register } = useApp();
  const navigate = useNavigate();
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");
  const [formData, setFormData] = useState({
    name: "",
    email: "",
    phone: "",
    password: "",
    confirmPassword: "",
    businessName: "",
    address: "",
    taxCode: "",
  });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (formData.password !== formData.confirmPassword) {
      setError("Mật khẩu xác nhận không khớp.");
      return;
    }

    setSubmitting(true);
    setError("");

    try {
      await register({
        email: formData.email,
        password: formData.password,
        fullName: formData.name,
        phoneNumber: formData.phone,
        address: formData.address,
        role: "landlord",
      });
      navigate("/landlord/dashboard");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Dang ky that bai.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 via-white to-indigo-50 flex items-center justify-center p-4">
      <div className="w-full max-w-2xl">
        {/* Back Button */}
        <Link
          to="/login"
          className="inline-flex items-center gap-2 text-gray-600 hover:text-gray-900 mb-6 transition-colors"
          style={{ fontSize: "14px", fontWeight: 500 }}
        >
          <ArrowLeft className="w-4 h-4" />
          Quay lại đăng nhập
        </Link>

        {/* Logo & Title */}
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-orange-500 mb-4 shadow-lg shadow-orange-200">
            <Building2 className="w-7 h-7 text-white" />
          </div>
          <h1 className="text-gray-900" style={{ fontSize: "24px", fontWeight: 700 }}>
            Đăng Ký Chủ Trọ
          </h1>
          <p className="text-gray-500 mt-1" style={{ fontSize: "14px" }}>
            Quản lý phòng trọ chuyên nghiệp và hiệu quả
          </p>
        </div>

        <div className="bg-white rounded-2xl shadow-xl shadow-gray-100 border border-gray-100 p-6">
          <form onSubmit={handleSubmit} className="space-y-4">
            {/* Personal Information */}
            <div className="pb-4 border-b border-gray-100">
              <h3 className="text-gray-700 mb-4" style={{ fontSize: "15px", fontWeight: 600 }}>
                Thông tin cá nhân
              </h3>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-gray-700 mb-1.5" style={{ fontSize: "13px", fontWeight: 500 }}>
                    Họ và tên <span className="text-red-500">*</span>
                  </label>
                  <div className="relative">
                    <User className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                    <input
                      type="text"
                      name="name"
                      value={formData.name}
                      onChange={handleChange}
                      placeholder="Nguyễn Văn A"
                      required
                      className="w-full pl-10 pr-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none focus:ring-2 focus:ring-orange-300 focus:border-orange-400 transition-all"
                      style={{ fontSize: "14px" }}
                    />
                  </div>
                </div>

                <div>
                  <label className="block text-gray-700 mb-1.5" style={{ fontSize: "13px", fontWeight: 500 }}>
                    Số điện thoại <span className="text-red-500">*</span>
                  </label>
                  <div className="relative">
                    <Phone className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                    <input
                      type="tel"
                      name="phone"
                      value={formData.phone}
                      onChange={handleChange}
                      placeholder="09xxxxxxxx"
                      required
                      className="w-full pl-10 pr-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none focus:ring-2 focus:ring-orange-300 focus:border-orange-400 transition-all"
                      style={{ fontSize: "14px" }}
                    />
                  </div>
                </div>
              </div>

              <div className="mt-4">
                <label className="block text-gray-700 mb-1.5" style={{ fontSize: "13px", fontWeight: 500 }}>
                  Email <span className="text-red-500">*</span>
                </label>
                <div className="relative">
                  <Mail className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                  <input
                    type="email"
                    name="email"
                    value={formData.email}
                    onChange={handleChange}
                    placeholder="email@example.com"
                    required
                    className="w-full pl-10 pr-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none focus:ring-2 focus:ring-orange-300 focus:border-orange-400 transition-all"
                    style={{ fontSize: "14px" }}
                  />
                </div>
              </div>
            </div>

            {/* Business Information */}
            <div className="pb-4 border-b border-gray-100">
              <h3 className="text-gray-700 mb-4" style={{ fontSize: "15px", fontWeight: 600 }}>
                Thông tin kinh doanh
              </h3>
              <div className="space-y-4">
                <div>
                  <label className="block text-gray-700 mb-1.5" style={{ fontSize: "13px", fontWeight: 500 }}>
                    Tên cơ sở kinh doanh (nếu có)
                  </label>
                  <div className="relative">
                    <Building2 className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                    <input
                      type="text"
                      name="businessName"
                      value={formData.businessName}
                      onChange={handleChange}
                      placeholder="Nhà trọ ABC"
                      className="w-full pl-10 pr-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none focus:ring-2 focus:ring-orange-300 focus:border-orange-400 transition-all"
                      style={{ fontSize: "14px" }}
                    />
                  </div>
                </div>

                <div>
                  <label className="block text-gray-700 mb-1.5" style={{ fontSize: "13px", fontWeight: 500 }}>
                    Địa chỉ cơ sở chính
                  </label>
                  <div className="relative">
                    <MapPin className="absolute left-3 top-3 w-4 h-4 text-gray-400" />
                    <textarea
                      name="address"
                      value={formData.address}
                      onChange={handleChange}
                      placeholder="Số nhà, đường, phường, quận, thành phố"
                      rows={2}
                      className="w-full pl-10 pr-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none focus:ring-2 focus:ring-orange-300 focus:border-orange-400 transition-all resize-none"
                      style={{ fontSize: "14px" }}
                    />
                  </div>
                </div>

                <div>
                  <label className="block text-gray-700 mb-1.5" style={{ fontSize: "13px", fontWeight: 500 }}>
                    Mã số thuế (nếu có)
                  </label>
                  <div className="relative">
                    <FileText className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                    <input
                      type="text"
                      name="taxCode"
                      value={formData.taxCode}
                      onChange={handleChange}
                      placeholder="0123456789"
                      className="w-full pl-10 pr-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none focus:ring-2 focus:ring-orange-300 focus:border-orange-400 transition-all"
                      style={{ fontSize: "14px" }}
                    />
                  </div>
                </div>
              </div>
            </div>

            {/* Security */}
            <div>
              <h3 className="text-gray-700 mb-4" style={{ fontSize: "15px", fontWeight: 600 }}>
                Bảo mật
              </h3>
              <div className="space-y-4">
                <div>
                  <label className="block text-gray-700 mb-1.5" style={{ fontSize: "13px", fontWeight: 500 }}>
                    Mật khẩu <span className="text-red-500">*</span>
                  </label>
                  <div className="relative">
                    <Lock className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                    <input
                      type={showPassword ? "text" : "password"}
                      name="password"
                      value={formData.password}
                      onChange={handleChange}
                      placeholder="Ít nhất 8 ký tự"
                      required
                      className="w-full pl-10 pr-10 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none focus:ring-2 focus:ring-orange-300 focus:border-orange-400 transition-all"
                      style={{ fontSize: "14px" }}
                    />
                    <button
                      type="button"
                      onClick={() => setShowPassword(!showPassword)}
                      className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
                    >
                      {showPassword ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                    </button>
                  </div>
                </div>

                <div>
                  <label className="block text-gray-700 mb-1.5" style={{ fontSize: "13px", fontWeight: 500 }}>
                    Xác nhận mật khẩu <span className="text-red-500">*</span>
                  </label>
                  <div className="relative">
                    <Lock className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                    <input
                      type={showConfirmPassword ? "text" : "password"}
                      name="confirmPassword"
                      value={formData.confirmPassword}
                      onChange={handleChange}
                      placeholder="Nhập lại mật khẩu"
                      required
                      className="w-full pl-10 pr-10 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none focus:ring-2 focus:ring-orange-300 focus:border-orange-400 transition-all"
                      style={{ fontSize: "14px" }}
                    />
                    <button
                      type="button"
                      onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                      className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
                    >
                      {showConfirmPassword ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                    </button>
                  </div>
                </div>
              </div>
            </div>

            <div className="flex items-start gap-2 pt-2">
              <input
                type="checkbox"
                id="terms"
                required
                className="mt-0.5 w-4 h-4 rounded border-gray-300 text-orange-500 focus:ring-orange-300"
              />
              <label htmlFor="terms" className="text-gray-600" style={{ fontSize: "12px" }}>
                Tôi đồng ý với{" "}
                <a href="#" className="text-orange-600 hover:text-orange-700">
                  Điều khoản dịch vụ
                </a>{" "}
                và{" "}
                <a href="#" className="text-orange-600 hover:text-orange-700">
                  Chính sách bảo mật
                </a>
              </label>
            </div>

            {error && (
              <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-red-600" style={{ fontSize: "13px" }}>
                {error}
              </div>
            )}

            <button
              type="submit"
              disabled={submitting}
              className="w-full py-3 rounded-xl bg-orange-500 text-white hover:bg-orange-600 active:bg-orange-700 transition-colors shadow-sm shadow-orange-200"
              style={{ fontSize: "15px", fontWeight: 600 }}
            >
              {submitting ? "Dang ky..." : "Tạo Tài Khoản Chủ Trọ"}
            </button>
          </form>

          <div className="mt-4 text-center">
            <span className="text-gray-600" style={{ fontSize: "13px" }}>
              Đã có tài khoản?{" "}
            </span>
            <Link
              to="/login"
              className="text-orange-600 hover:text-orange-700"
              style={{ fontSize: "13px", fontWeight: 600 }}
            >
              Đăng nhập ngay
            </Link>
          </div>
        </div>

        {/* Benefits */}
        <div className="mt-6 bg-white rounded-xl p-4 border border-gray-100">
          <h4 className="text-gray-700 mb-3" style={{ fontSize: "13px", fontWeight: 600 }}>
            Lợi ích khi đăng ký chủ trọ:
          </h4>
          <ul className="space-y-2 text-gray-600" style={{ fontSize: "12px" }}>
            <li className="flex items-start gap-2">
              <span className="text-green-500 mt-0.5">✓</span>
              <span>Đăng tin miễn phí, quản lý nhiều khu trọ</span>
            </li>
            <li className="flex items-start gap-2">
              <span className="text-green-500 mt-0.5">✓</span>
              <span>Tự động hóa hóa đơn điện nước, thu chi</span>
            </li>
            <li className="flex items-start gap-2">
              <span className="text-green-500 mt-0.5">✓</span>
              <span>Báo cáo doanh thu chi tiết với biểu đồ</span>
            </li>
            <li className="flex items-start gap-2">
              <span className="text-green-500 mt-0.5">✓</span>
              <span>Quản lý hợp đồng và khách thuê chuyên nghiệp</span>
            </li>
          </ul>
        </div>
      </div>
    </div>
  );
}
