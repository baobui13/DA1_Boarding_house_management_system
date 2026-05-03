import { useEffect, useMemo, useState } from "react";
import { Users, Search, Lock, Unlock, TriangleAlert } from "lucide-react";
import { useApp } from "../../context/AppContext";
import { blockUser, getUsers } from "../../lib/users";
import { normalizeRole } from "../../lib/auth";
import type { Role, UserResponse } from "../../lib/types";

type RoleFilter = "all" | Role;
type StatusFilter = "all" | "active" | "locked";

const roleLabels: Record<Role, { label: string; color: string }> = {
  tenant: { label: "Khách thuê", color: "text-blue-600 bg-blue-100" },
  landlord: { label: "Chủ trọ", color: "text-orange-600 bg-orange-100" },
  admin: { label: "Admin", color: "text-purple-600 bg-purple-100" },
};

export default function UserManagement() {
  const { token } = useApp();
  const [users, setUsers] = useState<UserResponse[]>([]);
  const [search, setSearch] = useState("");
  const [roleFilter, setRoleFilter] = useState<RoleFilter>("all");
  const [statusFilter, setStatusFilter] = useState<StatusFilter>("all");
  const [statusOverrides, setStatusOverrides] = useState<Record<string, "active" | "locked">>({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    let cancelled = false;

    (async () => {
      setLoading(true);
      setError("");

      try {
        const response = await getUsers();
        if (!cancelled) {
          setUsers(response.items);
        }
      } catch (err) {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : "Khong tai duoc danh sach nguoi dung.");
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
  }, []);

  const getStatus = (user: UserResponse) => statusOverrides[user.id] || "active";

  const filtered = useMemo(() => {
    return users
      .filter((user) => {
        if (!search) return true;
        const q = search.toLowerCase();
        return (
          user.fullName.toLowerCase().includes(q) ||
          user.email.toLowerCase().includes(q) ||
          (user.phoneNumber || "").includes(q)
        );
      })
      .filter((user) => roleFilter === "all" || normalizeRole(user.role) === roleFilter)
      .filter((user) => statusFilter === "all" || getStatus(user) === statusFilter);
  }, [roleFilter, search, statusFilter, users, statusOverrides]);

  const handleToggleLock = async (user: UserResponse) => {
    if (!token) {
      setError("Thiếu token đăng nhập admin để khóa/mở khóa.");
      return;
    }

    const nextStatus = getStatus(user) === "active" ? "locked" : "active";

    try {
      await blockUser(token, user.id, nextStatus === "locked");
      setStatusOverrides((prev) => ({ ...prev, [user.id]: nextStatus }));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Khóa/Mở khóa thất bại.");
    }
  };

  const totalActive = users.filter((user) => getStatus(user) === "active").length;
  const totalLocked = users.filter((user) => getStatus(user) === "locked").length;
  const totalLandlords = users.filter((user) => normalizeRole(user.role) === "landlord").length;

  return (
    <div className="max-w-5xl mx-auto px-4 py-6">
      <div className="mb-6">
        <h1 className="text-gray-900" style={{ fontSize: "22px", fontWeight: 700 }}>
          Quản Lý Người Dùng
        </h1>
        <p className="text-gray-500 mt-0.5" style={{ fontSize: "14px" }}>
          Danh sách lấy từ `User/GetUsersByFilter`.
        </p>
      </div>

      <div className="mb-4 rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-amber-800">
        <div className="flex items-start gap-3">
          <TriangleAlert className="w-5 h-5 mt-0.5 shrink-0" />
          <p style={{ fontSize: "13px" }}>
            Backend hiện không trả trạng thái khóa trong `UserResponse`, nên trạng thái ban đầu đang được frontend mặc định là `active`. Sau khi bấm khóa/mở khóa, frontend chỉ biết trạng thái mới từ thao tác vừa gửi.
          </p>
        </div>
      </div>

      {error && <div className="mb-4 rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-red-600">{error}</div>}

      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mb-6">
        <StatCard color="blue" label="Tổng người dùng" value={String(users.length)} />
        <StatCard color="green" label="Đang hoạt động" value={String(totalActive)} />
        <StatCard color="red" label="Bị khóa" value={String(totalLocked)} />
        <StatCard color="orange" label="Chủ trọ" value={String(totalLandlords)} />
      </div>

      <div className="bg-white rounded-2xl border border-gray-100 p-4 mb-4 flex flex-col sm:flex-row gap-3">
        <div className="flex-1 flex items-center gap-2 bg-gray-50 rounded-xl px-3 py-2 border border-gray-200 focus-within:border-orange-300">
          <Search className="w-4 h-4 text-gray-400 shrink-0" />
          <input
            type="text"
            placeholder="Tìm theo tên, email, số điện thoại..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="flex-1 bg-transparent focus:outline-none text-gray-700"
            style={{ fontSize: "14px" }}
          />
        </div>

        <div className="flex items-center gap-2 flex-wrap">
          <div className="flex gap-1">
            {(["all", "tenant", "landlord", "admin"] as RoleFilter[]).map((role) => (
              <button
                key={role}
                onClick={() => setRoleFilter(role)}
                className={`px-3 py-1.5 rounded-lg border transition-colors ${
                  roleFilter === role ? "border-orange-400 bg-orange-50 text-orange-600" : "border-gray-200 text-gray-500 hover:border-gray-300"
                }`}
                style={{ fontSize: "12px" }}
              >
                {role === "all" ? "Tất cả" : roleLabels[role].label}
              </button>
            ))}
          </div>

          <div className="flex gap-1">
            {(["all", "active", "locked"] as StatusFilter[]).map((status) => (
              <button
                key={status}
                onClick={() => setStatusFilter(status)}
                className={`px-3 py-1.5 rounded-lg border transition-colors ${
                  statusFilter === status ? "border-blue-400 bg-blue-50 text-blue-600" : "border-gray-200 text-gray-500 hover:border-gray-300"
                }`}
                style={{ fontSize: "12px" }}
              >
                {status === "all" ? "Tất cả" : status === "active" ? "Hoạt động" : "Bị khóa"}
              </button>
            ))}
          </div>
        </div>
      </div>

      <div className="bg-white rounded-2xl border border-gray-100 overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="bg-gray-50 border-b border-gray-100">
                <th className="text-left px-4 py-3 text-gray-500" style={{ fontSize: "12px", fontWeight: 600 }}>NGƯỜI DÙNG</th>
                <th className="text-left px-4 py-3 text-gray-500" style={{ fontSize: "12px", fontWeight: 600 }}>LIÊN HỆ</th>
                <th className="text-center px-4 py-3 text-gray-500" style={{ fontSize: "12px", fontWeight: 600 }}>VAI TRÒ</th>
                <th className="text-center px-4 py-3 text-gray-500" style={{ fontSize: "12px", fontWeight: 600 }}>TRẠNG THÁI</th>
                <th className="text-center px-4 py-3 text-gray-500" style={{ fontSize: "12px", fontWeight: 600 }}>NGÀY THAM GIA</th>
                <th className="text-center px-4 py-3 text-gray-500" style={{ fontSize: "12px", fontWeight: 600 }}>THAO TÁC</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-50">
              {loading ? (
                Array.from({ length: 6 }).map((_, index) => (
                  <tr key={index}>
                    <td colSpan={6} className="px-4 py-4">
                      <div className="h-10 rounded-lg bg-gray-100 animate-pulse" />
                    </td>
                  </tr>
                ))
              ) : (
                filtered.map((user) => {
                  const role = normalizeRole(user.role);
                  const roleCfg = roleLabels[role];
                  const status = getStatus(user);
                  return (
                    <tr key={user.id} className="hover:bg-gray-50/50 transition-colors">
                      <td className="px-4 py-3">
                        <div className="flex items-center gap-3">
                          <img
                            src={user.avatarUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(user.fullName)}&background=f97316&color=fff`}
                            alt=""
                            className="w-9 h-9 rounded-full object-cover"
                          />
                          <div>
                            <p className="text-gray-800" style={{ fontSize: "13px", fontWeight: 600 }}>
                              {user.fullName}
                            </p>
                            <p className="text-gray-400" style={{ fontSize: "11px" }}>
                              ID: {user.id}
                            </p>
                          </div>
                        </div>
                      </td>
                      <td className="px-4 py-3">
                        <p className="text-gray-600" style={{ fontSize: "12px" }}>{user.email}</p>
                        <p className="text-gray-400" style={{ fontSize: "11px" }}>{user.phoneNumber || "Chưa có"}</p>
                      </td>
                      <td className="px-4 py-3 text-center">
                        <span className={`px-2.5 py-1 rounded-xl ${roleCfg.color}`} style={{ fontSize: "11px", fontWeight: 600 }}>
                          {roleCfg.label}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-center">
                        <span
                          className={`px-2.5 py-1 rounded-xl ${status === "active" ? "text-green-600 bg-green-100" : "text-red-500 bg-red-100"}`}
                          style={{ fontSize: "11px", fontWeight: 600 }}
                        >
                          {status === "active" ? "Hoạt động" : "Bị khóa"}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-center">
                        <span className="text-gray-500" style={{ fontSize: "12px" }}>
                          {new Date(user.createdAt).toLocaleDateString("vi-VN")}
                        </span>
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex items-center justify-center gap-1">
                          <button
                            onClick={() => handleToggleLock(user)}
                            className={`flex items-center gap-1 px-2.5 py-1 rounded-lg transition-colors ${
                              status === "active" ? "bg-red-50 text-red-500 hover:bg-red-100" : "bg-green-50 text-green-600 hover:bg-green-100"
                            }`}
                            style={{ fontSize: "11px" }}
                          >
                            {status === "active" ? <Lock className="w-3.5 h-3.5" /> : <Unlock className="w-3.5 h-3.5" />}
                            {status === "active" ? "Khóa" : "Mở khóa"}
                          </button>
                        </div>
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>

        {!loading && filtered.length === 0 && (
          <div className="text-center py-12 text-gray-400">
            <Users className="w-8 h-8 mx-auto mb-2 opacity-40" />
            <p style={{ fontSize: "14px" }}>Không tìm thấy người dùng</p>
          </div>
        )}
      </div>
    </div>
  );
}

function StatCard({ color, label, value }: { color: string; label: string; value: string }) {
  const palette = {
    blue: "bg-blue-50 border-blue-100 text-blue-700 text-blue-600",
    green: "bg-green-50 border-green-100 text-green-700 text-green-600",
    red: "bg-red-50 border-red-100 text-red-700 text-red-600",
    orange: "bg-orange-50 border-orange-100 text-orange-700 text-orange-600",
  }[color];

  const [bg, border, valueColor, labelColor] = palette.split(" ");

  return (
    <div className={`${bg} border ${border} rounded-2xl p-4 text-center`}>
      <p className={valueColor} style={{ fontSize: "22px", fontWeight: 700 }}>{value}</p>
      <p className={labelColor} style={{ fontSize: "12px" }}>{label}</p>
    </div>
  );
}
