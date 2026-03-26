import { useState } from "react";
import {
  User,
  Mail,
  Phone,
  Lock,
  Camera,
  CheckCircle2,
  Shield,
  Bell,
  Eye,
  EyeOff,
} from "lucide-react";
import { useApp } from "../../context/AppContext";

export default function ProfilePage() {
  const { currentUser } = useApp();
  const [tab, setTab] = useState<"profile" | "security" | "notifications">("profile");
  const [saved, setSaved] = useState(false);
  const [showOldPw, setShowOldPw] = useState(false);
  const [showNewPw, setShowNewPw] = useState(false);

  const [profileForm, setProfileForm] = useState({
    name: currentUser?.name || "",
    email: currentUser?.email || "",
    phone: currentUser?.phone || "",
    bio: "Đang tìm phòng trọ tại TP.HCM",
  });

  const [passwordForm, setPasswordForm] = useState({
    oldPassword: "",
    newPassword: "",
    confirmPassword: "",
  });

  const [notifSettings, setNotifSettings] = useState({
    invoiceReminder: true,
    viewingConfirm: true,
    contractExpiry: true,
    systemNews: false,
    marketingEmail: false,
  });

  const handleSaveProfile = (e: React.FormEvent) => {
    e.preventDefault();
    setSaved(true);
    setTimeout(() => setSaved(false), 2000);
  };

  const tabs = [
    { id: "profile", label: "Hồ sơ", icon: User },
    { id: "security", label: "Bảo mật", icon: Shield },
    { id: "notifications", label: "Thông báo", icon: Bell },
  ];

  return (
    <div className="max-w-3xl mx-auto px-4 py-6">
      {/* Header */}
      <div className="bg-white rounded-2xl border border-gray-100 p-6 mb-6">
        <div className="flex items-center gap-5">
          <div className="relative">
            <img
              src={currentUser?.avatar || "https://i.pravatar.cc/80?img=11"}
              alt=""
              className="w-20 h-20 rounded-2xl object-cover"
            />
            <button className="absolute -bottom-1 -right-1 w-7 h-7 bg-orange-500 text-white rounded-xl flex items-center justify-center shadow-md hover:bg-orange-600 transition-colors">
              <Camera className="w-3.5 h-3.5" />
            </button>
          </div>
          <div>
            <h1 className="text-gray-900" style={{ fontSize: "20px", fontWeight: 700 }}>
              {currentUser?.name}
            </h1>
            <p className="text-gray-400 mt-0.5" style={{ fontSize: "14px" }}>
              {currentUser?.email}
            </p>
            <div className="flex items-center gap-2 mt-2">
              <span
                className={`px-3 py-1 rounded-xl text-white ${
                  currentUser?.role === "admin"
                    ? "bg-purple-500"
                    : currentUser?.role === "landlord"
                    ? "bg-orange-500"
                    : "bg-blue-500"
                }`}
                style={{ fontSize: "11px", fontWeight: 600 }}
              >
                {currentUser?.role === "admin" ? "Quản Trị Viên" : currentUser?.role === "landlord" ? "Chủ Trọ" : "Khách Thuê"}
              </span>
              <span className="flex items-center gap-1 text-green-600" style={{ fontSize: "12px" }}>
                <CheckCircle2 className="w-3.5 h-3.5" />
                Đã xác thực
              </span>
            </div>
          </div>
        </div>
      </div>

      {/* Tabs */}
      <div className="flex gap-1 bg-gray-100 p-1 rounded-xl mb-6">
        {tabs.map((t) => {
          const Icon = t.icon;
          return (
            <button
              key={t.id}
              onClick={() => setTab(t.id as any)}
              className={`flex-1 flex items-center justify-center gap-2 py-2 rounded-lg transition-all ${
                tab === t.id ? "bg-white text-gray-900 shadow-sm" : "text-gray-500 hover:text-gray-700"
              }`}
              style={{ fontSize: "13px", fontWeight: tab === t.id ? 600 : 400 }}
            >
              <Icon className="w-4 h-4" />
              {t.label}
            </button>
          );
        })}
      </div>

      {/* Profile Tab */}
      {tab === "profile" && (
        <div className="bg-white rounded-2xl border border-gray-100 p-6">
          <h2 className="text-gray-900 mb-5" style={{ fontSize: "16px", fontWeight: 700 }}>
            Thông tin cá nhân
          </h2>
          <form onSubmit={handleSaveProfile} className="space-y-4">
            <div>
              <label className="block text-gray-700 mb-1.5" style={{ fontSize: "13px", fontWeight: 500 }}>
                Họ và tên
              </label>
              <div className="relative">
                <User className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                <input
                  type="text"
                  value={profileForm.name}
                  onChange={(e) => setProfileForm({ ...profileForm, name: e.target.value })}
                  className="w-full pl-10 pr-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none focus:ring-2 focus:ring-orange-300 focus:border-orange-400 transition-all"
                  style={{ fontSize: "14px" }}
                />
              </div>
            </div>
            <div>
              <label className="block text-gray-700 mb-1.5" style={{ fontSize: "13px", fontWeight: 500 }}>
                Email
              </label>
              <div className="relative">
                <Mail className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                <input
                  type="email"
                  value={profileForm.email}
                  onChange={(e) => setProfileForm({ ...profileForm, email: e.target.value })}
                  className="w-full pl-10 pr-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none focus:ring-2 focus:ring-orange-300 focus:border-orange-400 transition-all"
                  style={{ fontSize: "14px" }}
                />
              </div>
            </div>
            <div>
              <label className="block text-gray-700 mb-1.5" style={{ fontSize: "13px", fontWeight: 500 }}>
                Số điện thoại
              </label>
              <div className="relative">
                <Phone className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                <input
                  type="tel"
                  value={profileForm.phone}
                  onChange={(e) => setProfileForm({ ...profileForm, phone: e.target.value })}
                  className="w-full pl-10 pr-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none focus:ring-2 focus:ring-orange-300 focus:border-orange-400 transition-all"
                  style={{ fontSize: "14px" }}
                />
              </div>
            </div>
            <div>
              <label className="block text-gray-700 mb-1.5" style={{ fontSize: "13px", fontWeight: 500 }}>
                Giới thiệu bản thân
              </label>
              <textarea
                value={profileForm.bio}
                onChange={(e) => setProfileForm({ ...profileForm, bio: e.target.value })}
                rows={3}
                className="w-full px-4 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none focus:ring-2 focus:ring-orange-300 focus:border-orange-400 transition-all resize-none"
                style={{ fontSize: "14px" }}
              />
            </div>
            <div className="flex gap-3 pt-2">
              <button
                type="button"
                className="flex-1 py-2.5 rounded-xl border border-gray-200 text-gray-600 hover:bg-gray-50 transition-colors"
                style={{ fontSize: "14px" }}
              >
                Hủy thay đổi
              </button>
              <button
                type="submit"
                className={`flex-1 py-2.5 rounded-xl transition-colors ${
                  saved ? "bg-green-500 text-white" : "bg-orange-500 text-white hover:bg-orange-600"
                }`}
                style={{ fontSize: "14px", fontWeight: 600 }}
              >
                {saved ? "✓ Đã lưu!" : "Lưu thay đổi"}
              </button>
            </div>
          </form>
        </div>
      )}

      {/* Security Tab */}
      {tab === "security" && (
        <div className="bg-white rounded-2xl border border-gray-100 p-6">
          <h2 className="text-gray-900 mb-5" style={{ fontSize: "16px", fontWeight: 700 }}>
            Đổi mật khẩu
          </h2>
          <form
            onSubmit={(e) => {
              e.preventDefault();
              setSaved(true);
              setTimeout(() => setSaved(false), 2000);
            }}
            className="space-y-4"
          >
            <div>
              <label className="block text-gray-700 mb-1.5" style={{ fontSize: "13px", fontWeight: 500 }}>
                Mật khẩu hiện tại
              </label>
              <div className="relative">
                <Lock className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                <input
                  type={showOldPw ? "text" : "password"}
                  value={passwordForm.oldPassword}
                  onChange={(e) => setPasswordForm({ ...passwordForm, oldPassword: e.target.value })}
                  placeholder="••••••••"
                  className="w-full pl-10 pr-10 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none focus:ring-2 focus:ring-orange-300"
                  style={{ fontSize: "14px" }}
                />
                <button type="button" onClick={() => setShowOldPw(!showOldPw)} className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400">
                  {showOldPw ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                </button>
              </div>
            </div>
            <div>
              <label className="block text-gray-700 mb-1.5" style={{ fontSize: "13px", fontWeight: 500 }}>
                Mật khẩu mới
              </label>
              <div className="relative">
                <Lock className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                <input
                  type={showNewPw ? "text" : "password"}
                  value={passwordForm.newPassword}
                  onChange={(e) => setPasswordForm({ ...passwordForm, newPassword: e.target.value })}
                  placeholder="Ít nhất 8 ký tự"
                  className="w-full pl-10 pr-10 py-2.5 rounded-xl border border-gray-200 bg-gray-50 focus:outline-none focus:ring-2 focus:ring-orange-300"
                  style={{ fontSize: "14px" }}
                />
                <button type="button" onClick={() => setShowNewPw(!showNewPw)} className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400">
                  {showNewPw ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                </button>
              </div>
              {/* Password strength */}
              {passwordForm.newPassword && (
                <div className="mt-2">
                  <div className="flex gap-1">
                    {[1, 2, 3, 4].map((i) => (
                      <div
                        key={i}
                        className={`flex-1 h-1 rounded-full ${
                          i <= (passwordForm.newPassword.length >= 12 ? 4 : passwordForm.newPassword.length >= 8 ? 3 : passwordForm.newPassword.length >= 6 ? 2 : 1)
                            ? i <= 2
                              ? "bg-red-400"
                              : i === 3
                              ? "bg-yellow-400"
                              : "bg-green-400"
                            : "bg-gray-100"
                        }`}
                      />
                    ))}
                  </div>
                  <p className="text-gray-400 mt-1" style={{ fontSize: "11px" }}>
                    {passwordForm.newPassword.length < 6 ? "Quá ngắn" : passwordForm.newPassword.length < 8 ? "Yếu" : passwordForm.newPassword.length < 12 ? "Trung bình" : "Mạnh"}
                  </p>
                </div>
              )}
            </div>
            <div>
              <label className="block text-gray-700 mb-1.5" style={{ fontSize: "13px", fontWeight: 500 }}>
                Xác nhận mật khẩu mới
              </label>
              <div className="relative">
                <Lock className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                <input
                  type="password"
                  value={passwordForm.confirmPassword}
                  onChange={(e) => setPasswordForm({ ...passwordForm, confirmPassword: e.target.value })}
                  placeholder="Nhập lại mật khẩu mới"
                  className={`w-full pl-10 pr-4 py-2.5 rounded-xl border bg-gray-50 focus:outline-none focus:ring-2 focus:ring-orange-300 ${
                    passwordForm.confirmPassword && passwordForm.newPassword !== passwordForm.confirmPassword
                      ? "border-red-300"
                      : "border-gray-200"
                  }`}
                  style={{ fontSize: "14px" }}
                />
              </div>
              {passwordForm.confirmPassword && passwordForm.newPassword !== passwordForm.confirmPassword && (
                <p className="text-red-500 mt-1" style={{ fontSize: "12px" }}>
                  Mật khẩu không khớp
                </p>
              )}
            </div>
            <button
              type="submit"
              className={`w-full py-2.5 rounded-xl transition-colors ${
                saved ? "bg-green-500 text-white" : "bg-orange-500 text-white hover:bg-orange-600"
              }`}
              style={{ fontSize: "14px", fontWeight: 600 }}
            >
              {saved ? "✓ Đổi mật khẩu thành công!" : "Đổi mật khẩu"}
            </button>
          </form>
        </div>
      )}

      {/* Notifications Tab */}
      {tab === "notifications" && (
        <div className="bg-white rounded-2xl border border-gray-100 p-6">
          <h2 className="text-gray-900 mb-5" style={{ fontSize: "16px", fontWeight: 700 }}>
            Cài đặt thông báo
          </h2>
          <div className="space-y-4">
            {[
              { key: "invoiceReminder", label: "Nhắc hóa đơn", desc: "Thông báo khi có hóa đơn mới hoặc sắp hết hạn" },
              { key: "viewingConfirm", label: "Xác nhận lịch xem", desc: "Thông báo khi lịch xem phòng được xác nhận/từ chối" },
              { key: "contractExpiry", label: "Hợp đồng sắp hết hạn", desc: "Nhắc khi hợp đồng còn 30 ngày" },
              { key: "systemNews", label: "Tin tức hệ thống", desc: "Cập nhật tính năng và bảo trì hệ thống" },
              { key: "marketingEmail", label: "Email marketing", desc: "Các ưu đãi và chương trình khuyến mãi" },
            ].map((setting) => (
              <div key={setting.key} className="flex items-center justify-between p-4 bg-gray-50 rounded-xl">
                <div className="flex-1 mr-4">
                  <p className="text-gray-800" style={{ fontSize: "14px", fontWeight: 500 }}>
                    {setting.label}
                  </p>
                  <p className="text-gray-400 mt-0.5" style={{ fontSize: "12px" }}>
                    {setting.desc}
                  </p>
                </div>
                <button
                  onClick={() =>
                    setNotifSettings((prev) => ({
                      ...prev,
                      [setting.key]: !prev[setting.key as keyof typeof prev],
                    }))
                  }
                  className={`relative w-12 h-6 rounded-full transition-colors shrink-0 ${
                    notifSettings[setting.key as keyof typeof notifSettings]
                      ? "bg-orange-500"
                      : "bg-gray-200"
                  }`}
                >
                  <div
                    className={`absolute top-0.5 w-5 h-5 bg-white rounded-full shadow-sm transition-transform ${
                      notifSettings[setting.key as keyof typeof notifSettings] ? "translate-x-6" : "translate-x-0.5"
                    }`}
                  />
                </button>
              </div>
            ))}
          </div>
          <button
            onClick={() => setSaved(true)}
            className={`w-full mt-5 py-2.5 rounded-xl transition-colors ${saved ? "bg-green-500 text-white" : "bg-orange-500 text-white hover:bg-orange-600"}`}
            style={{ fontSize: "14px", fontWeight: 600 }}
          >
            {saved ? "✓ Đã lưu cài đặt!" : "Lưu cài đặt"}
          </button>
        </div>
      )}
    </div>
  );
}
