import { useState } from "react";
import { Link, useLocation, Outlet, useNavigate } from "react-router";
import {
  Home,
  Search,
  Building2,
  FileText,
  Bell,
  User,
  LogOut,
  Menu,
  X,
  BarChart3,
  Users,
  ShieldCheck,
  Receipt,
  LayoutDashboard,
  TrendingUp,
} from "lucide-react";
import { useApp } from "../context/AppContext";
import type { Role } from "../lib/types";

const tenantNav = [
  { path: "/", label: "Trang Chủ", icon: Home },
  { path: "/search", label: "Tìm Kiếm", icon: Search },
  { path: "/tenant/dashboard", label: "Quản Lý Cá Nhân", icon: LayoutDashboard },
];

const landlordNav = [
  { path: "/landlord/dashboard", label: "Tổng Quan", icon: BarChart3 },
  { path: "/landlord/properties", label: "Quản Lý Phòng", icon: Building2 },
  { path: "/landlord/billing", label: "Hóa Đơn", icon: Receipt },
  { path: "/landlord/contracts", label: "Hợp Đồng", icon: FileText },
];

const adminNav = [
  { path: "/admin/users", label: "Người Dùng", icon: Users },
  { path: "/admin/moderation", label: "Kiểm Duyệt", icon: ShieldCheck },
  { path: "/admin/analytics", label: "Báo Cáo", icon: TrendingUp },
];

const roleLabels: Record<Role, string> = {
  tenant: "Khách Thuê",
  landlord: "Chủ Trọ",
  admin: "Quản Trị Viên",
};

const roleColors: Record<Role, string> = {
  tenant: "bg-blue-500",
  landlord: "bg-orange-500",
  admin: "bg-purple-500",
};

export function Layout() {
  const { currentUser, logout } = useApp();
  const location = useLocation();
  const navigate = useNavigate();
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const unreadCount = 0;

  const getNavItems = () => {
    if (!currentUser) return tenantNav;
    if (currentUser.role === "landlord") return landlordNav;
    if (currentUser.role === "admin") return adminNav;
    return tenantNav;
  };

  const navItems = getNavItems();

  const handleLogout = async () => {
    await logout();
    navigate("/login");
  };

  return (
    <div className="min-h-screen bg-gray-50 flex">
      {/* Sidebar */}
      <aside
        className={`fixed inset-y-0 left-0 z-50 w-64 bg-white shadow-xl transform transition-transform duration-300 flex flex-col
        ${sidebarOpen ? "translate-x-0" : "-translate-x-full"} lg:translate-x-0 lg:static lg:shadow-none lg:border-r border-gray-100`}
      >
        {/* Logo */}
        <div className="flex items-center gap-3 px-6 py-5 border-b border-gray-100">
          <div className="w-9 h-9 rounded-xl bg-orange-500 flex items-center justify-center">
            <Building2 className="w-5 h-5 text-white" />
          </div>
          <div>
            <p className="font-semibold text-gray-900" style={{ fontSize: "15px" }}>
              TroViet
            </p>
            <p className="text-gray-400" style={{ fontSize: "11px" }}>
              Quản lý phòng trọ
            </p>
          </div>
          <button
            className="ml-auto lg:hidden text-gray-400 hover:text-gray-600"
            onClick={() => setSidebarOpen(false)}
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Role Badge */}
        {currentUser && (
          <div className="px-4 py-3 mx-4 mt-3 rounded-xl bg-orange-50 border border-orange-100">
            <div className="flex items-center gap-2">
              <div className={`w-2 h-2 rounded-full ${roleColors[currentUser.role]}`} />
              <span className="text-orange-700" style={{ fontSize: "12px", fontWeight: 600 }}>
                {roleLabels[currentUser.role]}
              </span>
            </div>
            <p className="text-gray-500 truncate mt-0.5" style={{ fontSize: "12px" }}>
              {currentUser.name}
            </p>
          </div>
        )}

        {/* Nav Items */}
        <nav className="flex-1 px-4 py-4 space-y-1 overflow-y-auto">
          {navItems.map((item) => {
            const Icon = item.icon;
            const active = location.pathname === item.path;
            return (
              <Link
                key={item.path}
                to={item.path}
                onClick={() => setSidebarOpen(false)}
                className={`flex items-center gap-3 px-3 py-2.5 rounded-xl transition-all duration-150
                  ${active ? "bg-orange-500 text-white shadow-sm shadow-orange-200" : "text-gray-600 hover:bg-gray-50 hover:text-gray-900"}`}
              >
                <Icon className="w-4.5 h-4.5 shrink-0" style={{ width: "18px", height: "18px" }} />
                <span style={{ fontSize: "14px", fontWeight: active ? 600 : 500 }}>{item.label}</span>
              </Link>
            );
          })}

          <div className="pt-2 border-t border-gray-100 mt-2 space-y-1">
            <Link
              to="/notifications"
              onClick={() => setSidebarOpen(false)}
              className={`flex items-center gap-3 px-3 py-2.5 rounded-xl transition-all duration-150
                ${location.pathname === "/notifications" ? "bg-orange-500 text-white" : "text-gray-600 hover:bg-gray-50 hover:text-gray-900"}`}
            >
              <Bell className="shrink-0" style={{ width: "18px", height: "18px" }} />
              <span style={{ fontSize: "14px", fontWeight: 500 }}>Thông Báo</span>
              {unreadCount > 0 && (
                <span className="ml-auto bg-red-500 text-white text-xs rounded-full px-1.5 py-0.5 min-w-[20px] text-center">
                  {unreadCount}
                </span>
              )}
            </Link>
            <Link
              to="/profile"
              onClick={() => setSidebarOpen(false)}
              className={`flex items-center gap-3 px-3 py-2.5 rounded-xl transition-all duration-150
                ${location.pathname === "/profile" ? "bg-orange-500 text-white" : "text-gray-600 hover:bg-gray-50 hover:text-gray-900"}`}
            >
              <User className="shrink-0" style={{ width: "18px", height: "18px" }} />
              <span style={{ fontSize: "14px", fontWeight: 500 }}>Hồ Sơ</span>
            </Link>
          </div>
        </nav>

        {/* Bottom User */}
        <div className="px-4 py-4 border-t border-gray-100">
          {currentUser ? (
            <div className="flex items-center gap-3">
              <img src={currentUser.avatar} alt="" className="w-9 h-9 rounded-full object-cover" />
              <div className="flex-1 min-w-0">
                <p className="truncate text-gray-800" style={{ fontSize: "13px", fontWeight: 600 }}>
                  {currentUser.name}
                </p>
                <p className="truncate text-gray-400" style={{ fontSize: "11px" }}>
                  {currentUser.email}
                </p>
              </div>
              <button onClick={handleLogout} className="text-gray-400 hover:text-red-500 transition-colors">
                <LogOut className="w-4 h-4" />
              </button>
            </div>
          ) : (
            <Link
              to="/login"
              className="flex items-center justify-center gap-2 w-full py-2 rounded-xl bg-orange-500 text-white hover:bg-orange-600 transition-colors"
              style={{ fontSize: "14px" }}
            >
              Đăng Nhập
            </Link>
          )}
        </div>
      </aside>

      {/* Overlay */}
      {sidebarOpen && (
        <div className="fixed inset-0 z-40 bg-black/30 lg:hidden" onClick={() => setSidebarOpen(false)} />
      )}

      {/* Main Content */}
      <div className="flex-1 flex flex-col min-w-0">
        {/* Header */}
        <header className="bg-white border-b border-gray-100 px-4 lg:px-6 py-3 flex items-center gap-4 sticky top-0 z-30">
          <button
            className="lg:hidden p-2 rounded-lg text-gray-500 hover:bg-gray-100"
            onClick={() => setSidebarOpen(true)}
          >
            <Menu className="w-5 h-5" />
          </button>

          <div className="flex-1" />

          <Link to="/notifications" className="relative p-2 rounded-lg text-gray-500 hover:bg-gray-100 transition-colors">
            <Bell className="w-5 h-5" />
            {unreadCount > 0 && (
              <span className="absolute top-1 right-1 w-2 h-2 bg-red-500 rounded-full" />
            )}
          </Link>

          {currentUser && (
            <Link to="/profile">
              <img
                src={currentUser.avatar}
                alt={currentUser.name}
                className="w-8 h-8 rounded-full object-cover border-2 border-orange-200 hover:border-orange-400 transition-colors"
              />
            </Link>
          )}
        </header>

        {/* Page Content */}
        <main className="flex-1 overflow-auto">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
