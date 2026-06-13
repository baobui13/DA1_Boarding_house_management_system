import { useEffect, useState, useMemo, useRef } from "react";
import { useApp } from "../../context/AppContext";
import { createNotification, getNotifications, type NotificationResponse } from "../../lib/notifications";
import { getUsers } from "../../lib/users";
import {
  Bell,
  Search,
  User,
  Users,
  Send,
  Loader2,
  RefreshCw,
  CheckCircle2,
  AlertCircle,
  X,
  Mail,
  ChevronRight,
  TrendingUp
} from "lucide-react";
import type { Role, UserResponse } from "../../lib/types";
import { normalizeRole } from "../../lib/auth";

type TargetScope = "single" | "role" | "all";

interface SendStatus {
  userId: string;
  userName: string;
  email: string;
  status: "pending" | "sending" | "success" | "error";
  error?: string;
}

export default function AdminNotificationPage() {
  const { token } = useApp();
  const [scope, setScope] = useState<TargetScope>("single");
  
  // Single user select state
  const [searchQuery, setSearchQuery] = useState("");
  const [searchResults, setSearchResults] = useState<UserResponse[]>([]);
  const [searching, setSearching] = useState(false);
  const [selectedUser, setSelectedUser] = useState<UserResponse | null>(null);
  const [dropdownOpen, setDropdownOpen] = useState(false);

  // Role select state
  const [targetRole, setTargetRole] = useState<Role>("tenant");

  // Notification details state
  const [notificationType, setNotificationType] = useState<string>("System");
  const [content, setContent] = useState("");
  const [relatedId, setRelatedId] = useState("");

  // Recent notifications sent
  const [recentNotifications, setRecentNotifications] = useState<NotificationResponse[]>([]);
  const [loadingRecent, setLoadingRecent] = useState(true);

  // Progress/Status of sending
  const [sending, setSending] = useState(false);
  const [sendStatuses, setSendStatuses] = useState<SendStatus[]>([]);
  const [currentProgress, setCurrentProgress] = useState(0);
  const [showStatusModal, setShowStatusModal] = useState(false);

  const [formError, setFormError] = useState("");
  const [formSuccess, setFormSuccess] = useState("");

  const dropdownRef = useRef<HTMLDivElement>(null);

  // Notification types configuration
  const notificationTypesConfig = [
    { value: "System", label: "Hệ thống (Chung)", color: "text-purple-600 bg-purple-50" },
    { value: "Invoice", label: "Hóa đơn (Invoice)", color: "text-amber-600 bg-amber-50" },
    { value: "Appointment", label: "Lịch hẹn (Appointment)", color: "text-blue-600 bg-blue-50" },
    { value: "Contract", label: "Hợp đồng (Contract)", color: "text-emerald-600 bg-emerald-50" },
    { value: "Message", label: "Tin nhắn (Message)", color: "text-pink-600 bg-pink-50" },
    { value: "Rating", label: "Đánh giá (Rating)", color: "text-rose-600 bg-rose-50" },
  ];

  // Fetch recent notifications sent by admin
  const loadRecentNotifications = async () => {
    if (!token) return;
    setLoadingRecent(true);
    try {
      // Get system notifications or overall notifications list
      const response = await getNotifications(token, { pageSize: 15 });
      setRecentNotifications(response.items);
    } catch (err) {
      console.error("Failed to load recent notifications", err);
    } finally {
      setLoadingRecent(false);
    }
  };

  useEffect(() => {
    void loadRecentNotifications();
  }, [token]);

  // Click outside search dropdown
  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setDropdownOpen(false);
      }
    }
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  // Handle Autocomplete Search
  useEffect(() => {
    if (searchQuery.trim().length < 1) {
      setSearchResults([]);
      return;
    }

    const delayDebounceFn = setTimeout(async () => {
      setSearching(true);
      try {
        const response = await getUsers({
          pageSize: 20,
        });

        // Client side filtering to match the search query (name or email or phone)
        const q = searchQuery.toLowerCase();
        const matched = response.items.filter(
          (u) =>
            u.fullName.toLowerCase().includes(q) ||
            u.email.toLowerCase().includes(q) ||
            (u.phoneNumber || "").includes(q),
        );
        setSearchResults(matched);
        setDropdownOpen(matched.length > 0);
      } catch (err) {
        console.error("Error searching users", err);
      } finally {
        setSearching(false);
      }
    }, 400);

    return () => clearTimeout(delayDebounceFn);
  }, [searchQuery]);

  // Handle selected role conversion to title case for backend API
  const toBackendRole = (role: Role) => {
    return role.charAt(0).toUpperCase() + role.slice(1);
  };

  // Submit Handler
  const handleSendNotification = async (e: React.FormEvent) => {
    e.preventDefault();
    setFormError("");
    setFormSuccess("");

    if (!token) {
      setFormError("Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại.");
      return;
    }

    if (!content.trim()) {
      setFormError("Nội dung thông báo không được để trống.");
      return;
    }

    if (content.length > 255) {
      setFormError("Nội dung thông báo không được vượt quá 255 ký tự.");
      return;
    }

    if (notificationType !== "System" && !relatedId.trim()) {
      setFormError(`Bắt buộc phải điền ID liên quan (RelatedId) cho loại thông báo "${notificationType}".`);
      return;
    }

    // Step 1: Collect Target User List
    let targetUsers: { id: string; fullName: string; email: string }[] = [];

    if (scope === "single") {
      if (!selectedUser) {
        setFormError("Vui lòng chọn người nhận thông báo.");
        return;
      }
      targetUsers = [{ id: selectedUser.id, fullName: selectedUser.fullName, email: selectedUser.email }];
    } else {
      setSending(true);
      try {
        // Fetch all active users
        const response = await getUsers({
          pageSize: 5000, // Fetch a large batch to get all matching users
          role: scope === "role" ? toBackendRole(targetRole) : undefined,
        });

        targetUsers = response.items.map((u) => ({ id: u.id, fullName: u.fullName, email: u.email }));

        if (targetUsers.length === 0) {
          setFormError("Không tìm thấy người dùng nào thỏa mãn điều kiện gửi.");
          setSending(false);
          return;
        }
      } catch (err) {
        setFormError(err instanceof Error ? err.message : "Không thể tải danh sách người dùng để gửi.");
        setSending(false);
        return;
      }
    }

    // Step 2: Initialize Statuses
    const initialStatuses = targetUsers.map((u) => ({
      userId: u.id,
      userName: u.fullName,
      email: u.email,
      status: "pending" as const,
    }));
    setSendStatuses(initialStatuses);
    setCurrentProgress(0);
    setSending(true);
    setShowStatusModal(true);

    // Step 3: Send Concurrent Batches (e.g. 5 at a time)
    const chunkSize = 5;
    let completedCount = 0;

    for (let i = 0; i < targetUsers.length; i += chunkSize) {
      const chunk = targetUsers.slice(i, i + chunkSize);
      
      // Update statuses to 'sending'
      setSendStatuses((prev) =>
        prev.map((s) =>
          chunk.some((u) => u.id === s.userId) ? { ...s, status: "sending" } : s,
        ),
      );

      const promises = chunk.map(async (user) => {
        try {
          await createNotification(token, {
            userId: user.id,
            type: notificationType,
            content: content.trim(),
            relatedId: notificationType === "System" ? null : relatedId.trim(),
          });
          
          setSendStatuses((prev) =>
            prev.map((s) => (s.userId === user.id ? { ...s, status: "success" } : s)),
          );
        } catch (err) {
          console.error(`Failed to send to user ${user.id}`, err);
          const errMsg = err instanceof Error ? err.message : "Lỗi hệ thống";
          setSendStatuses((prev) =>
            prev.map((s) =>
              s.userId === user.id ? { ...s, status: "error", error: errMsg } : s,
            ),
          );
        }
      });

      await Promise.all(promises);
      completedCount += chunk.length;
      setCurrentProgress(Math.round((completedCount / targetUsers.length) * 100));
    }

    setSending(false);
    setFormSuccess(`Đã gửi thông báo thành công cho danh sách người nhận!`);
    void loadRecentNotifications();

    // Reset Form if single send
    if (scope === "single") {
      setContent("");
      setRelatedId("");
      setSelectedUser(null);
      setSearchQuery("");
    }
  };

  const totals = useMemo(() => {
    const total = sendStatuses.length;
    const success = sendStatuses.filter((s) => s.status === "success").length;
    const errorCount = sendStatuses.filter((s) => s.status === "error").length;
    const processing = sendStatuses.filter((s) => s.status === "sending" || s.status === "pending").length;
    return { total, success, errorCount, processing };
  }, [sendStatuses]);

  return (
    <div className="max-w-7xl mx-auto px-4 py-6">
      {/* Page Title */}
      <div className="mb-6">
        <h1 className="text-gray-900" style={{ fontSize: "24px", fontWeight: 700 }}>
          Quản Lý &amp; Gửi Thông Báo
        </h1>
        <p className="text-gray-500 mt-1" style={{ fontSize: "14px" }}>
          Soạn thảo và gửi thông báo hệ thống hoặc sự kiện đến các tài khoản.
        </p>
      </div>

      {formError && (
        <div className="mb-4 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-red-600 flex items-center gap-2" style={{ fontSize: "14px" }}>
          <AlertCircle className="w-4 h-4 shrink-0" />
          <span>{formError}</span>
        </div>
      )}

      {formSuccess && (
        <div className="mb-4 rounded-xl border border-green-200 bg-green-50 px-4 py-3 text-green-600 flex items-center gap-2" style={{ fontSize: "14px" }}>
          <CheckCircle2 className="w-4 h-4 shrink-0" />
          <span>{formSuccess}</span>
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Main Builder Form */}
        <div className="lg:col-span-2">
          <div className="bg-white rounded-2xl border border-gray-100 p-6 shadow-sm">
            <h2 className="text-gray-800 mb-5 pb-3 border-b border-gray-100" style={{ fontSize: "16px", fontWeight: 600 }}>
              Soạn thảo thông báo mới
            </h2>

            <form onSubmit={handleSendNotification} className="space-y-5">
              {/* Scope Selection */}
              <div>
                <label className="block text-gray-700 mb-2 font-medium" style={{ fontSize: "13px" }}>
                  Phạm vi người nhận
                </label>
                <div className="grid grid-cols-3 gap-2">
                  <button
                    type="button"
                    onClick={() => {
                      setScope("single");
                      setFormError("");
                    }}
                    className={`flex flex-col items-center gap-2 py-3 rounded-xl border-2 transition-all ${
                      scope === "single"
                        ? "border-orange-500 bg-orange-50/50 text-orange-600 shadow-sm"
                        : "border-gray-200 text-gray-500 hover:border-gray-300"
                    }`}
                  >
                    <User className="w-5 h-5" />
                    <span style={{ fontSize: "12px", fontWeight: 600 }}>Một người dùng</span>
                  </button>

                  <button
                    type="button"
                    onClick={() => {
                      setScope("role");
                      setFormError("");
                    }}
                    className={`flex flex-col items-center gap-2 py-3 rounded-xl border-2 transition-all ${
                      scope === "role"
                        ? "border-orange-500 bg-orange-50/50 text-orange-600 shadow-sm"
                        : "border-gray-200 text-gray-500 hover:border-gray-300"
                    }`}
                  >
                    <Users className="w-5 h-5" />
                    <span style={{ fontSize: "12px", fontWeight: 600 }}>Theo vai trò</span>
                  </button>

                  <button
                    type="button"
                    onClick={() => {
                      setScope("all");
                      setFormError("");
                    }}
                    className={`flex flex-col items-center gap-2 py-3 rounded-xl border-2 transition-all ${
                      scope === "all"
                        ? "border-orange-500 bg-orange-50/50 text-orange-600 shadow-sm"
                        : "border-gray-200 text-gray-500 hover:border-gray-300"
                    }`}
                  >
                    <Bell className="w-5 h-5" />
                    <span style={{ fontSize: "12px", fontWeight: 600 }}>Tất cả mọi người</span>
                  </button>
                </div>
              </div>

              {/* Target Details Depending on Scope */}
              {scope === "single" && (
                <div ref={dropdownRef} className="relative">
                  <label className="block text-gray-700 mb-1.5 font-medium" style={{ fontSize: "13px" }}>
                    Tìm kiếm người nhận
                  </label>
                  {selectedUser ? (
                    <div className="flex items-center justify-between p-3 border-2 border-orange-200 bg-orange-50/20 rounded-xl">
                      <div className="flex items-center gap-3">
                        <img
                          src={selectedUser.avatarUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(selectedUser.fullName)}&background=f97316&color=fff`}
                          alt=""
                          className="w-9 h-9 rounded-full object-cover"
                        />
                        <div>
                          <p className="text-gray-800 font-semibold" style={{ fontSize: "13px" }}>
                            {selectedUser.fullName}
                          </p>
                          <p className="text-gray-500" style={{ fontSize: "11px" }}>
                            {selectedUser.email}
                          </p>
                        </div>
                      </div>
                      <button
                        type="button"
                        onClick={() => setSelectedUser(null)}
                        className="p-1 rounded-lg hover:bg-orange-100 text-orange-500 transition-colors"
                      >
                        <X className="w-4 h-4" />
                      </button>
                    </div>
                  ) : (
                    <>
                      <div className="relative flex items-center bg-gray-50 border border-gray-200 rounded-xl px-3 py-2.5 focus-within:border-orange-300">
                        <Search className="w-4 h-4 text-gray-400 shrink-0 mr-2" />
                        <input
                          type="text"
                          placeholder="Nhập tên, email hoặc số điện thoại..."
                          value={searchQuery}
                          onChange={(e) => {
                            setSearchQuery(e.target.value);
                            setDropdownOpen(true);
                          }}
                          className="w-full bg-transparent focus:outline-none text-gray-700 text-sm"
                        />
                        {searching && <Loader2 className="w-4 h-4 text-gray-400 animate-spin shrink-0" />}
                      </div>

                      {/* Dropdown search results */}
                      {dropdownOpen && searchResults.length > 0 && (
                        <div className="absolute z-20 w-full mt-1 bg-white border border-gray-100 rounded-xl shadow-xl max-h-60 overflow-y-auto divide-y divide-gray-50">
                          {searchResults.map((user) => (
                            <button
                              key={user.id}
                              type="button"
                              onClick={() => {
                                setSelectedUser(user);
                                setDropdownOpen(false);
                                setSearchQuery("");
                              }}
                              className="w-full text-left p-3 hover:bg-gray-50 flex items-center gap-3 transition-colors"
                            >
                              <img
                                src={user.avatarUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(user.fullName)}&background=f97316&color=fff`}
                                alt=""
                                className="w-8 h-8 rounded-full object-cover"
                              />
                              <div className="flex-1 min-w-0">
                                <div className="flex items-center gap-1.5">
                                  <span className="text-gray-800 font-semibold truncate text-xs">{user.fullName}</span>
                                  <span className="text-[9px] bg-orange-100 text-orange-600 px-1.5 py-0.5 rounded font-bold uppercase shrink-0">
                                    {normalizeRole(user.role) === "tenant" ? "Khách thuê" : "Chủ trọ"}
                                  </span>
                                </div>
                                <span className="text-gray-400 text-[10px] block truncate">{user.email}</span>
                              </div>
                              <ChevronRight className="w-3.5 h-3.5 text-gray-300 shrink-0" />
                            </button>
                          ))}
                        </div>
                      )}
                    </>
                  )}
                </div>
              )}

              {scope === "role" && (
                <div>
                  <label className="block text-gray-700 mb-2 font-medium" style={{ fontSize: "13px" }}>
                    Chọn vai trò nhận tin
                  </label>
                  <div className="flex gap-4">
                    <label className="flex-1 flex items-center gap-3 p-3 rounded-xl border border-gray-200 hover:bg-gray-50 cursor-pointer transition-all">
                      <input
                        type="radio"
                        name="targetRole"
                        checked={targetRole === "tenant"}
                        onChange={() => setTargetRole("tenant")}
                        className="w-4 h-4 text-orange-500 focus:ring-orange-300"
                      />
                      <div>
                        <p className="text-gray-800 font-semibold" style={{ fontSize: "13px" }}>
                          Khách Thuê (Tenant)
                        </p>
                        <p className="text-gray-400 text-[11px]">
                          Tất cả người đi thuê phòng trọ
                        </p>
                      </div>
                    </label>

                    <label className="flex-1 flex items-center gap-3 p-3 rounded-xl border border-gray-200 hover:bg-gray-50 cursor-pointer transition-all">
                      <input
                        type="radio"
                        name="targetRole"
                        checked={targetRole === "landlord"}
                        onChange={() => setTargetRole("landlord")}
                        className="w-4 h-4 text-orange-500 focus:ring-orange-300"
                      />
                      <div>
                        <p className="text-gray-800 font-semibold" style={{ fontSize: "13px" }}>
                          Chủ Trọ (Landlord)
                        </p>
                        <p className="text-gray-400 text-[11px]">
                          Tất cả người quản lý / chủ sở hữu phòng trọ
                        </p>
                      </div>
                    </label>
                  </div>
                </div>
              )}

              {scope === "all" && (
                <div className="p-3 border border-orange-200 bg-orange-50/30 rounded-xl flex items-start gap-3">
                  <Bell className="w-5 h-5 text-orange-500 shrink-0 mt-0.5" />
                  <div>
                    <p className="text-orange-800 font-semibold text-xs">Chú ý:</p>
                    <p className="text-orange-700 text-[11px] mt-0.5 leading-relaxed">
                      Thông báo này sẽ được nhân bản và gửi tới **tất cả người dùng** có tài khoản đăng ký trên hệ thống. 
                      Hành động này có thể mất vài giây để hoàn thành do số lượng lớn các request đồng thời.
                    </p>
                  </div>
                </div>
              )}

              {/* Notification Type & RelatedId */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-gray-700 mb-1.5 font-medium" style={{ fontSize: "13px" }}>
                    Loại thông báo
                  </label>
                  <select
                    value={notificationType}
                    onChange={(e) => {
                      setNotificationType(e.target.value);
                      setRelatedId("");
                    }}
                    className="w-full bg-gray-50 border border-gray-200 rounded-xl px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-orange-300 focus:border-orange-400 text-sm text-gray-700"
                  >
                    {notificationTypesConfig.map((item) => (
                      <option key={item.value} value={item.value}>
                        {item.label}
                      </option>
                    ))}
                  </select>
                </div>

                {notificationType !== "System" && (
                  <div>
                    <label className="block text-gray-700 mb-1.5 font-medium" style={{ fontSize: "13px" }}>
                      ID tài liệu liên quan (RelatedId) <span className="text-red-500">*</span>
                    </label>
                    <input
                      type="text"
                      placeholder={`Nhập ID ${notificationType} hợp lệ...`}
                      value={relatedId}
                      onChange={(e) => setRelatedId(e.target.value)}
                      className="w-full bg-gray-50 border border-gray-200 rounded-xl px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-orange-300 focus:border-orange-400 text-sm text-gray-700"
                      required
                    />
                    <p className="text-gray-400 text-[10px] mt-1">
                      Ví dụ: Hóa đơn, lịch hẹn hay hợp đồng liên quan.
                    </p>
                  </div>
                )}
              </div>

              {/* Notification Content */}
              <div>
                <div className="flex justify-between items-center mb-1.5">
                  <label className="block text-gray-700 font-medium" style={{ fontSize: "13px" }}>
                    Nội dung thông báo
                  </label>
                  <span className={`text-[11px] ${content.length > 255 ? "text-red-500 font-bold" : "text-gray-400"}`}>
                    {content.length}/255
                  </span>
                </div>
                <textarea
                  rows={4}
                  placeholder="Soạn nội dung chi tiết của thông báo..."
                  value={content}
                  onChange={(e) => setContent(e.target.value)}
                  className="w-full bg-gray-50 border border-gray-200 rounded-xl px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-orange-300 focus:border-orange-400 text-sm text-gray-700 resize-none"
                  maxLength={255}
                  required
                />
              </div>

              {/* Submit Button */}
              <div className="pt-2">
                <button
                  type="submit"
                  disabled={sending}
                  className="w-full py-3 bg-orange-500 text-white rounded-xl hover:bg-orange-600 font-semibold text-sm transition-all shadow-sm shadow-orange-200 active:bg-orange-700 flex items-center justify-center gap-2 disabled:opacity-75"
                >
                  {sending ? (
                    <>
                      <Loader2 className="w-4.5 h-4.5 animate-spin" />
                      Đang xử lý gửi hàng loạt...
                    </>
                  ) : (
                    <>
                      <Send className="w-4 h-4" />
                      Gửi thông báo ngay
                    </>
                  )}
                </button>
              </div>
            </form>
          </div>
        </div>

        {/* Info & Instructions Panel */}
        <div className="lg:col-span-1 space-y-4">
          <div className="bg-white rounded-2xl border border-gray-100 p-5 shadow-sm">
            <h3 className="text-gray-800 mb-3 font-semibold flex items-center gap-2" style={{ fontSize: "14px" }}>
              <TrendingUp className="w-4 h-4 text-orange-500" />
              Lợi ích khi gửi tin
            </h3>
            <ul className="space-y-2 text-gray-600 text-[12px] leading-relaxed">
              <li className="flex items-start gap-2">
                <span className="text-green-500 font-bold">✓</span>
                <span>Thông báo ngay cho người dùng trên cổng thông tin</span>
              </li>
              <li className="flex items-start gap-2">
                <span className="text-green-500 font-bold">✓</span>
                <span>Hỗ trợ gửi hàng loạt qua concurrent batches</span>
              </li>
              <li className="flex items-start gap-2">
                <span className="text-green-500 font-bold">✓</span>
                <span>An toàn mạng lưới nhờ kiểm soát khối lượng gửi</span>
              </li>
            </ul>
          </div>

          <div className="bg-white rounded-2xl border border-gray-100 p-5 shadow-sm">
            <h3 className="text-gray-800 mb-3 font-semibold text-xs uppercase text-gray-400">
              Quy tắc gửi dữ liệu
            </h3>
            <div className="space-y-2 text-[11px] text-gray-500">
              <p>
                1. **Hệ thống (System)**: Cho phép bỏ trống RelatedId. Được hiển thị chung.
              </p>
              <p>
                2. **Hóa đơn/Lịch hẹn/Hợp đồng**: Yêu cầu RelatedId là khóa chính (Guid) hợp lệ tồn tại trong cơ sở dữ liệu.
              </p>
              <p>
                3. Nội dung bắt buộc tối đa 255 ký tự để tránh tràn cột trong database.
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Recent Sent Notifications Section */}
      <div className="mt-8 bg-white rounded-2xl border border-gray-100 p-6 shadow-sm">
        <div className="flex justify-between items-center mb-5 pb-3 border-b border-gray-100">
          <div>
            <h3 className="text-gray-800 font-semibold" style={{ fontSize: "16px" }}>
              Nhật ký thông báo gần đây
            </h3>
            <p className="text-gray-400 text-xs mt-0.5">
              Danh sách các thông báo trong hệ thống đã được gửi đi.
            </p>
          </div>
          <button
            onClick={() => void loadRecentNotifications()}
            className="flex items-center gap-1 text-xs text-gray-500 hover:bg-gray-50 px-2 py-1.5 rounded-lg border border-gray-200 transition-colors"
          >
            <RefreshCw className={`w-3.5 h-3.5 ${loadingRecent ? "animate-spin" : ""}`} />
            Làm mới
          </button>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full text-left">
            <thead>
              <tr className="bg-gray-50 border-b border-gray-100 text-gray-500 font-semibold" style={{ fontSize: "11px" }}>
                <th className="px-4 py-2.5">MÃ NGƯỜI NHẬN</th>
                <th className="px-4 py-2.5">PHÂN LOẠI</th>
                <th className="px-4 py-2.5">NỘI DUNG</th>
                <th className="px-4 py-2.5 text-center">TRẠNG THÁI</th>
                <th className="px-4 py-2.5 text-right">THỜI GIAN GỬI</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-50 text-gray-600" style={{ fontSize: "12.5px" }}>
              {loadingRecent ? (
                Array.from({ length: 4 }).map((_, index) => (
                  <tr key={index}>
                    <td colSpan={5} className="px-4 py-4 text-center">
                      <div className="h-6 rounded bg-gray-100 animate-pulse" />
                    </td>
                  </tr>
                ))
              ) : recentNotifications.length === 0 ? (
                <tr>
                  <td colSpan={5} className="px-4 py-8 text-center text-gray-400">
                    Chưa có lịch sử thông báo nào được lưu trữ.
                  </td>
                </tr>
              ) : (
                recentNotifications.map((notif) => {
                  const cfg = notificationTypesConfig.find((t) => t.value.toLowerCase() === notif.type.toLowerCase()) || {
                    label: notif.type,
                    color: "text-gray-600 bg-gray-50",
                  };
                  return (
                    <tr key={notif.id} className="hover:bg-gray-50/50 transition-colors">
                      <td className="px-4 py-3 font-mono text-[10px] text-gray-400 truncate max-w-[150px]">
                        {notif.userId}
                      </td>
                      <td className="px-4 py-3">
                        <span className={`inline-block px-2 py-0.5 rounded text-[10px] font-bold ${cfg.color}`}>
                          {cfg.label}
                        </span>
                      </td>
                      <td className="px-4 py-3 font-medium text-gray-800 max-w-[300px] truncate" title={notif.content}>
                        {notif.content}
                      </td>
                      <td className="px-4 py-3 text-center">
                        <span className={`inline-block px-2 py-0.5 rounded text-[10px] ${notif.isRead ? "text-gray-400 bg-gray-100" : "text-orange-600 bg-orange-50 font-semibold"}`}>
                          {notif.isRead ? "Đã xem" : "Chưa xem"}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-right text-gray-400 text-xs">
                        {new Date(notif.timestamp).toLocaleString("vi-VN")}
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Sending Batch Progress Modal */}
      {showStatusModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm p-4">
          <div className="bg-white w-full max-w-xl rounded-2xl shadow-2xl border border-gray-100 overflow-hidden animate-in fade-in zoom-in-95 duration-200">
            {/* Modal Header */}
            <div className="bg-gray-50 px-6 py-4 border-b border-gray-100 flex items-center justify-between">
              <div>
                <h3 className="text-gray-800 font-bold" style={{ fontSize: "16px" }}>
                  Trạng thái gửi hàng loạt ({totals.success + totals.errorCount}/{totals.total})
                </h3>
                <p className="text-gray-400 text-[11px] mt-0.5">
                  Đang phân phối thông báo đồng thời qua các chunk api.
                </p>
              </div>
              {!sending && (
                <button
                  onClick={() => setShowStatusModal(false)}
                  className="p-1 rounded-lg hover:bg-gray-200 text-gray-500 transition-colors"
                >
                  <X className="w-4 h-4" />
                </button>
              )}
            </div>

            {/* Modal Content */}
            <div className="p-6 space-y-4">
              {/* Progress bar */}
              <div>
                <div className="flex justify-between items-center mb-1.5">
                  <span className="text-gray-600 font-semibold text-xs">Tiến trình</span>
                  <span className="text-orange-600 font-bold text-xs">{currentProgress}%</span>
                </div>
                <div className="w-full bg-gray-100 rounded-full h-2.5 overflow-hidden">
                  <div
                    className="bg-orange-500 h-2.5 rounded-full transition-all duration-300"
                    style={{ width: `${currentProgress}%` }}
                  />
                </div>
              </div>

              {/* Stat Boxes */}
              <div className="grid grid-cols-3 gap-3 text-center">
                <div className="bg-green-50 border border-green-100 rounded-xl p-2.5">
                  <span className="text-green-600 font-bold block" style={{ fontSize: "18px" }}>{totals.success}</span>
                  <span className="text-green-700 text-[10px]">Thành công</span>
                </div>
                <div className="bg-red-50 border border-red-100 rounded-xl p-2.5">
                  <span className="text-red-500 font-bold block" style={{ fontSize: "18px" }}>{totals.errorCount}</span>
                  <span className="text-red-600 text-[10px]">Thất bại</span>
                </div>
                <div className="bg-gray-50 border border-gray-100 rounded-xl p-2.5">
                  <span className="text-gray-500 font-bold block" style={{ fontSize: "18px" }}>{totals.processing}</span>
                  <span className="text-gray-600 text-[10px]">Đang chờ</span>
                </div>
              </div>

              {/* Status List */}
              <div>
                <label className="block text-gray-700 mb-1.5 font-medium" style={{ fontSize: "12px" }}>
                  Danh sách chi tiết
                </label>
                <div className="border border-gray-100 rounded-xl divide-y divide-gray-50 max-h-48 overflow-y-auto bg-gray-50/50">
                  {sendStatuses.map((status) => (
                    <div key={status.userId} className="p-3 flex items-center justify-between text-xs">
                      <div className="min-w-0 flex-1 pr-3">
                        <p className="text-gray-800 font-semibold truncate">{status.userName}</p>
                        <p className="text-gray-400 text-[10px] truncate">{status.email}</p>
                      </div>
                      <div className="shrink-0 flex items-center gap-1.5">
                        {status.status === "pending" && (
                          <span className="text-gray-400 font-medium">Chờ gửi...</span>
                        )}
                        {status.status === "sending" && (
                          <div className="flex items-center gap-1 text-orange-500 font-semibold">
                            <Loader2 className="w-3.5 h-3.5 animate-spin" />
                            Đang gửi
                          </div>
                        )}
                        {status.status === "success" && (
                          <div className="flex items-center gap-1 text-green-600 font-bold">
                            <CheckCircle2 className="w-3.5 h-3.5" />
                            Thành công
                          </div>
                        )}
                        {status.status === "error" && (
                          <div className="flex items-center gap-1 text-red-500 font-bold" title={status.error}>
                            <AlertCircle className="w-3.5 h-3.5" />
                            Thất bại
                          </div>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>

            {/* Modal Footer */}
            <div className="bg-gray-50 px-6 py-4 border-t border-gray-100 flex justify-end">
              <button
                type="button"
                disabled={sending}
                onClick={() => setShowStatusModal(false)}
                className="px-4 py-2 bg-gray-800 text-white rounded-xl hover:bg-gray-900 transition-all font-semibold text-xs disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Đóng hộp thoại
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
