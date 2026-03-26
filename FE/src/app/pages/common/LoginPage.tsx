import { useState } from "react";
import { useNavigate, Link } from "react-router";
import { Building2, Eye, EyeOff, Mail, Lock, Users, Home } from "lucide-react";
import { useApp } from "../../context/AppContext";

export default function LoginPage() {
  const { login } = useApp();
  const navigate = useNavigate();
  const [tab, setTab] = useState<"login" | "register">("login");
  const [showPassword, setShowPassword] = useState(false);
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    setError("");

    try {
      const user = await login(email, password);
      if (user.role === "landlord") navigate("/landlord/dashboard");
      else if (user.role === "admin") navigate("/admin/users");
      else navigate("/");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Dang nhap that bai.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-orange-50 via-white to-amber-50 flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-orange-500 mb-4 shadow-lg shadow-orange-200">
            <Building2 className="w-7 h-7 text-white" />
          </div>
          <h1 className="text-gray-900" style={{ fontSize: "24px", fontWeight: 700 }}>
            TroViet
          </h1>
          <p className="text-gray-500 mt-1" style={{ fontSize: "14px" }}>
            Nền tảng quản lý phòng trọ thông minh
          </p>
        </div>

        <div className="bg-white rounded-2xl shadow-xl shadow-gray-100 overflow-hidden border border-gray-100">
          <div className="flex border-b border-gray-100">
            {(["login", "register"] as const).map((t) => (
              <button
                key={t}
                onClick={() => setTab(t)}
                className={`flex-1 py-4 transition-colors ${
                  tab === t ? "text-orange-600 border-b-2 border-orange-500 bg-orange-50/50" : "text-gray-500 hover:text-gray-700"
                }`}
                style={{ fontSize: "14px", fontWeight: tab === t ? 600 : 500 }}
              >
                {t === "login" ? "Đăng Nhập" : "Đăng Ký"}
              </button>
            ))}
          </div>

          <div className="p-6">
            {tab === "login" ? (
              <form onSubmit={handleLogin} className="space-y-4">
                <div>
                  <label className="block text-gray-700 mb-1.5" style={{ fontSize: "13px", fontWeight: 500 }}>
                    Email
                  </label>
                  <div className="relative">
                    <Mail className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                    <input
                      type="email"
                      value={email}
                      onChange={(e) => setEmail(e.target.value)}
                      className="w-full pl-10 pr-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none focus:ring-2 focus:ring-orange-300 focus:border-orange-400 transition-all"
                      style={{ fontSize: "14px" }}
                      required
                    />
                  </div>
                </div>

                <div>
                  <label className="block text-gray-700 mb-1.5" style={{ fontSize: "13px", fontWeight: 500 }}>
                    Mật khẩu
                  </label>
                  <div className="relative">
                    <Lock className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                    <input
                      type={showPassword ? "text" : "password"}
                      value={password}
                      onChange={(e) => setPassword(e.target.value)}
                      className="w-full pl-10 pr-10 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none focus:ring-2 focus:ring-orange-300 focus:border-orange-400 transition-all"
                      style={{ fontSize: "14px" }}
                      required
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

                {error && (
                  <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-red-600" style={{ fontSize: "13px" }}>
                    {error}
                  </div>
                )}

                <button
                  type="submit"
                  disabled={submitting}
                  className="w-full py-3 rounded-xl bg-orange-500 text-white hover:bg-orange-600 active:bg-orange-700 transition-colors shadow-sm shadow-orange-200 disabled:opacity-70"
                  style={{ fontSize: "15px", fontWeight: 600 }}
                >
                  {submitting ? "Dang nhap..." : "Đăng Nhập"}
                </button>
              </form>
            ) : (
              <div className="space-y-3">
                <p className="text-center text-gray-600" style={{ fontSize: "14px", fontWeight: 500 }}>
                  Chọn loại tài khoản bạn muốn đăng ký
                </p>

                <Link
                  to="/register/tenant"
                  className="flex items-center gap-3 p-4 rounded-xl border-2 border-gray-200 hover:border-orange-400 hover:bg-orange-50 transition-all group"
                >
                  <div className="flex items-center justify-center w-12 h-12 rounded-xl bg-orange-100 text-orange-600 group-hover:bg-orange-200">
                    <Users className="w-6 h-6" />
                  </div>
                  <div className="flex-1 text-left">
                    <h3 className="text-gray-900 group-hover:text-orange-600" style={{ fontSize: "14px", fontWeight: 600 }}>
                      Đăng ký Khách Thuê
                    </h3>
                    <p className="text-gray-500" style={{ fontSize: "12px" }}>
                      Tìm phòng trọ phù hợp với bạn
                    </p>
                  </div>
                </Link>

                <Link
                  to="/register/landlord"
                  className="flex items-center gap-3 p-4 rounded-xl border-2 border-gray-200 hover:border-orange-400 hover:bg-orange-50 transition-all group"
                >
                  <div className="flex items-center justify-center w-12 h-12 rounded-xl bg-blue-100 text-blue-600 group-hover:bg-blue-200">
                    <Home className="w-6 h-6" />
                  </div>
                  <div className="flex-1 text-left">
                    <h3 className="text-gray-900 group-hover:text-orange-600" style={{ fontSize: "14px", fontWeight: 600 }}>
                      Đăng ký Chủ Trọ
                    </h3>
                    <p className="text-gray-500" style={{ fontSize: "12px" }}>
                      Quản lý phòng trọ chuyên nghiệp
                    </p>
                  </div>
                </Link>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
