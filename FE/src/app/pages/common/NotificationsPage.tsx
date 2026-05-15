import { useEffect, useMemo, useState } from "react";
import { Bell, Check, RefreshCw } from "lucide-react";
import { useApp } from "../../context/AppContext";
import { getNotifications, type NotificationResponse, updateNotificationRead } from "../../lib/notifications";

export default function NotificationsPage() {
  const { token, currentUser } = useApp();
  const [notifications, setNotifications] = useState<NotificationResponse[]>([]);
  const [filter, setFilter] = useState<"all" | "unread">("all");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [updatingId, setUpdatingId] = useState<string | null>(null);

  const loadNotifications = async () => {
    if (!token || !currentUser) {
      setError("Thiếu phiên đăng nhập để tải thông báo.");
      setLoading(false);
      return;
    }

    setLoading(true);
    setError("");

    try {
      const response = await getNotifications(token, { userId: currentUser.id });
      setNotifications(response.items);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Không tải được thông báo.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadNotifications();
  }, [token, currentUser?.id]);

  const filtered = useMemo(
    () => notifications.filter((item) => filter === "all" || !item.isRead),
    [filter, notifications],
  );

  const unreadCount = notifications.filter((item) => !item.isRead).length;

  const handleMarkAsRead = async (notification: NotificationResponse) => {
    if (!token || notification.isRead) return;

    setUpdatingId(notification.id);
    try {
      await updateNotificationRead(token, notification.id, true);
      setNotifications((prev) =>
        prev.map((item) =>
          item.id === notification.id
            ? { ...item, isRead: true }
            : item,
        ),
      );
    } catch (err) {
      setError(err instanceof Error ? err.message : "Không cập nhật được trạng thái thông báo.");
    } finally {
      setUpdatingId(null);
    }
  };

  return (
    <div className="max-w-3xl mx-auto px-4 py-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-gray-900" style={{ fontSize: "22px", fontWeight: 700 }}>
            Thông Báo
          </h1>
          <p className="text-gray-500 mt-0.5" style={{ fontSize: "14px" }}>
            {unreadCount > 0 ? `Bạn có ${unreadCount} thông báo chưa đọc` : "Không có thông báo chưa đọc"}
          </p>
        </div>
        <button
          onClick={() => void loadNotifications()}
          className="flex items-center gap-2 px-3 py-1.5 text-gray-600 hover:bg-gray-50 rounded-xl transition-colors"
          style={{ fontSize: "13px", fontWeight: 500 }}
        >
          <RefreshCw className="w-4 h-4" />
          Tải lại
        </button>
      </div>

      <div className="flex gap-2 mb-5">
        {(["all", "unread"] as const).map((value) => (
          <button
            key={value}
            onClick={() => setFilter(value)}
            className={`px-4 py-1.5 rounded-xl border transition-colors ${
              filter === value ? "border-orange-400 bg-orange-50 text-orange-600" : "border-gray-200 text-gray-500"
            }`}
            style={{ fontSize: "13px", fontWeight: filter === value ? 600 : 400 }}
          >
            {value === "all" ? "Tất cả" : "Chưa đọc"}
          </button>
        ))}
      </div>

      <div className="space-y-3">
        {error ? (
          <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-red-600">{error}</div>
        ) : loading ? (
          Array.from({ length: 4 }).map((_, index) => (
            <div key={index} className="h-24 rounded-2xl bg-gray-100 animate-pulse" />
          ))
        ) : filtered.length === 0 ? (
          <div className="text-center py-16 text-gray-300">
            <Bell className="w-12 h-12 mx-auto mb-3" />
            <p className="text-gray-400" style={{ fontSize: "15px" }}>
              Không có thông báo nào
            </p>
          </div>
        ) : (
          filtered.map((notification) => (
            <div key={notification.id} className={`bg-white rounded-2xl border p-4 ${notification.isRead ? "opacity-70" : "shadow-sm"}`}>
              <div className="flex items-start gap-3">
                <div className="w-10 h-10 rounded-xl border flex items-center justify-center shrink-0 text-blue-500 bg-blue-50 border-blue-100">
                  <Bell className="w-5 h-5" />
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-gray-900" style={{ fontSize: "14px", fontWeight: notification.isRead ? 500 : 600 }}>
                    {notification.type}
                  </p>
                  <p className="text-gray-500 mt-1" style={{ fontSize: "13px", lineHeight: 1.5 }}>
                    {notification.content}
                  </p>
                  <p className="text-gray-300 mt-2" style={{ fontSize: "12px" }}>
                    {new Date(notification.timestamp).toLocaleString("vi-VN")}
                  </p>
                </div>
                {!notification.isRead && (
                  <button
                    onClick={() => void handleMarkAsRead(notification)}
                    disabled={updatingId === notification.id}
                    className="inline-flex items-center gap-1 text-orange-600 bg-orange-50 px-2 py-1 rounded-lg disabled:opacity-60"
                    style={{ fontSize: "11px" }}
                  >
                    <Check className="w-3 h-3" />
                    {updatingId === notification.id ? "Đang lưu" : "Đánh dấu đã đọc"}
                  </button>
                )}
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
}
