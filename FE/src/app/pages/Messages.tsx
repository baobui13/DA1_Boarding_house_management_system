import React, { useEffect, useState, useRef } from "react";
import { useSearchParams } from "react-router";
import { useApp } from "../context/AppContext";
import {
  ConversationResponse,
  getConversations,
  getMessages,
  markConversationAsRead,
  sendMessage,
  setupSignalRConnection,
  MessageResponse,
} from "../lib/messages";
import { getProperties } from "../lib/properties";
import { createAppointment } from "../lib/appointments";
import type { PropertyResponse } from "../lib/types";
import { HubConnection, HubConnectionState } from "@microsoft/signalr";

export default function Messages() {
  const { token, currentUser: user } = useApp();
  const [searchParams, setSearchParams] = useSearchParams();
  const initialUserId = searchParams.get("userId");
  const initialPropertyId = searchParams.get("propertyId");

  const [showAppointmentModal, setShowAppointmentModal] = useState(false);
  const [landlordProperties, setLandlordProperties] = useState<PropertyResponse[]>([]);
  const [selectedPropertyId, setSelectedPropertyId] = useState<string>(initialPropertyId || "");
  const [appointmentDate, setAppointmentDate] = useState("");
  const [appointmentTime, setAppointmentTime] = useState("");
  const [appointmentNote, setAppointmentNote] = useState("");
  const [bookingLoading, setBookingLoading] = useState(false);

  const [conversations, setConversations] = useState<ConversationResponse[]>([]);
  const [selectedContactId, setSelectedContactId] = useState<string | null>(
    initialUserId
  );
  const [messages, setMessages] = useState<MessageResponse[]>([]);
  const [inputText, setInputText] = useState("");
  const [loadingConversations, setLoadingConversations] = useState(true);
  const [loadingMessages, setLoadingMessages] = useState(false);

  const connectionRef = useRef<HubConnection | null>(null);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  // Load conversations
  const loadConversations = async () => {
    if (!token) return;
    try {
      const data = await getConversations(token);
      setConversations(data);
      // If there's an initialUserId but it's not in conversations, it means it's a new conversation
      if (initialUserId && !data.some((c) => c.contactId === initialUserId)) {
        // We could fetch user details and prepend a dummy conversation, but let's keep it simple
        // When message is sent, it will appear.
      } else if (!initialUserId && data.length > 0) {
        // Select first conversation by default if none selected
        setSelectedContactId(data[0].contactId);
      }
    } catch (error) {
      console.error("Failed to load conversations:", error);
    } finally {
      setLoadingConversations(false);
    }
  };

  // Load messages for selected contact
  const loadMessages = async (contactId: string) => {
    if (!token || !user) return;
    setLoadingMessages(true);
    try {
      const data = await getMessages(token, user.id, contactId);
      setMessages(data);
      // Mark as read
      await markConversationAsRead(token, contactId);
      // Update unread count in conversations state
      setConversations((prev) =>
        prev.map((c) =>
          c.contactId === contactId ? { ...c, unreadCount: 0 } : c
        )
      );
    } catch (error) {
      console.error("Failed to load messages:", error);
    } finally {
      setLoadingMessages(false);
    }
  };

  useEffect(() => {
    loadConversations();
  }, [token]);

  useEffect(() => {
    if (selectedContactId) {
      loadMessages(selectedContactId);
      // Update URL silently
      setSearchParams({ userId: selectedContactId }, { replace: true });
    }
  }, [selectedContactId, token, user]);

  // Scroll to bottom when messages change
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  // SignalR Setup
  useEffect(() => {
    if (!token || !user) return;

    const connection = setupSignalRConnection(token);
    connectionRef.current = connection;

    connection.on("ReceiveMessage", (senderId: string, messageStr: string) => {
      // The backend could send the whole message object or just the string content
      // If it's a JSON string, we should parse it. Let's assume it's the raw content string for now
      // Actually, ideally backend sends JSON. But our ChatHub code sends (senderId, messageStr).
      // We will reload messages or append locally.
      
      // If the message is from the currently selected contact, reload or append
      if (senderId === selectedContactId) {
        // Simple approach: reload messages
        loadMessages(senderId);
      } else {
        // Update conversation list to show new unread
        loadConversations();
      }
    });

    const startConnection = async () => {
      try {
        await connection.start();
        console.log("SignalR Connected.");
      } catch (err) {
        console.error("SignalR Connection Error: ", err);
      }
    };

    startConnection();

    return () => {
      if (connection.state === HubConnectionState.Connected) {
        connection.stop();
      }
    };
  }, [token, user, selectedContactId]);

  useEffect(() => {
    if (showAppointmentModal && selectedContactId && token) {
      const fetchProperties = async () => {
        try {
          const res = await getProperties({ landlordId: selectedContactId, status: "Available" });
          setLandlordProperties(res.items);
          if (!selectedPropertyId && res.items.length > 0) {
            setSelectedPropertyId(res.items[0].id);
          }
        } catch (error) {
          console.error("Failed to fetch landlord properties:", error);
        }
      };
      fetchProperties();
    }
  }, [showAppointmentModal, selectedContactId, token]);

  const handleBookAppointment = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!token || !user || !selectedPropertyId || !appointmentDate || !appointmentTime) return;

    setBookingLoading(true);
    try {
      // Combine date and time
      const dateObj = new Date(`${appointmentDate}T${appointmentTime}`);
      
      const newAppt = await createAppointment(token, {
        propertyId: selectedPropertyId,
        userId: user.id,
        appointmentDateTime: dateObj.toISOString(),
        note: appointmentNote,
      });

      // Send an automated message in chat to notify landlord with appointment ID encoded
      await sendMessage(token, {
        senderId: user.id,
        receiverId: selectedContactId as string,
        content: `[APPOINTMENT:${newAppt.id}] Tôi đã đặt một lịch hẹn xem phòng vào lúc ${appointmentTime} ngày ${new Date(appointmentDate).toLocaleDateString("vi-VN")}. ${appointmentNote ? `Ghi chú: ${appointmentNote}` : ""}`
      });

      setShowAppointmentModal(false);
      setAppointmentDate("");
      setAppointmentTime("");
      setAppointmentNote("");
      loadMessages(selectedContactId as string);
      loadConversations();
      alert("Đặt lịch hẹn thành công!");
    } catch (error) {
      console.error("Failed to book appointment:", error);
      alert("Đặt lịch hẹn thất bại. Vui lòng thử lại.");
    } finally {
      setBookingLoading(false);
    }
  };

  const handleConfirmAppointment = async (appointmentId: string) => {
    if (!token || !user || !selectedContactId) return;
    try {
      await import("../lib/appointments").then((m) =>
        m.updateAppointment(token, { id: appointmentId, status: "Confirmed" })
      );
      
      // The backend now automatically sends a message upon status update
      // So we don't need to manually send one here anymore.
      
      // Tải lại tin nhắn
      await loadMessages(selectedContactId);
      loadConversations();
      alert("Đã xác nhận lịch hẹn thành công!");
    } catch (error) {
      console.error("Failed to confirm appointment:", error);
      alert("Xác nhận thất bại. Lịch hẹn có thể đã bị xóa hoặc có lỗi hệ thống.");
    }
  };

  const handleSendMessage = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!inputText.trim() || !selectedContactId || !token || !user) return;

    const content = inputText;
    setInputText(""); // Optimistic clear

    try {
      const newMsg = await sendMessage(token, {
        senderId: user.id,
        receiverId: selectedContactId,
        content: content,
      });

      // Append locally immediately
      setMessages((prev) => [...prev, newMsg]);

      // If we are connected to SignalR, we can also tell the Hub to broadcast
      if (connectionRef.current?.state === HubConnectionState.Connected) {
        await connectionRef.current.invoke(
          "SendMessage",
          selectedContactId,
          content
        );
      }

      // Refresh conversation list to bump it to top
      loadConversations();
    } catch (error) {
      console.error("Failed to send message:", error);
    }
  };

  const selectedContact = conversations.find(
    (c) => c.contactId === selectedContactId
  );

  return (
    <div className="flex h-[calc(100vh-64px)] bg-gray-50 overflow-hidden">
      {/* Left Sidebar - Conversations */}
      <div className="w-1/3 max-w-sm border-r bg-white flex flex-col">
        <div className="p-4 border-b bg-white">
          <h2 className="text-xl font-bold text-gray-800">Tin nhắn</h2>
        </div>
        <div className="flex-1 overflow-y-auto">
          {loadingConversations ? (
            <div className="p-4 text-center text-gray-500">Đang tải...</div>
          ) : conversations.length === 0 ? (
            <div className="p-4 text-center text-gray-500">
              Bạn chưa có cuộc trò chuyện nào.
            </div>
          ) : (
            <ul className="divide-y">
              {conversations.map((c) => {
                let previewContent = c.lastMessage.content;
                if (previewContent.startsWith("[APPOINTMENT:")) {
                  const match = previewContent.match(/^\[APPOINTMENT:.+?\]\s*(.*)$/s);
                  if (match) {
                    previewContent = "📅 " + match[1];
                  }
                }
                
                return (
                  <li
                    key={c.contactId}
                    onClick={() => setSelectedContactId(c.contactId)}
                    className={`p-4 cursor-pointer hover:bg-gray-50 transition-colors ${
                      selectedContactId === c.contactId ? "bg-blue-50" : ""
                    }`}
                  >
                    <div className="flex items-center space-x-3">
                      <img
                        src={
                          c.contactAvatarUrl ||
                          "https://res.cloudinary.com/dx1jnguud/image/upload/v1734027787/users/default_avatar.jpg"
                        }
                        alt={c.contactName}
                        className="w-12 h-12 rounded-full object-cover"
                      />
                      <div className="flex-1 min-w-0">
                        <div className="flex justify-between items-baseline">
                          <p className="text-sm font-medium text-gray-900 truncate">
                            {c.contactName}
                          </p>
                          <p className="text-xs text-gray-500">
                            {new Date(c.lastMessage.timestamp).toLocaleDateString(
                              "vi-VN"
                            )}
                          </p>
                        </div>
                        <div className="flex justify-between items-center mt-1">
                          <p
                            className={`text-sm truncate ${
                              c.unreadCount > 0
                                ? "font-semibold text-gray-900"
                                : "text-gray-500"
                            }`}
                          >
                            {c.lastMessage.senderId === user?.id ? "Bạn: " : ""}
                            {previewContent}
                          </p>
                          {c.unreadCount > 0 && (
                            <span className="inline-flex items-center justify-center w-5 h-5 ml-2 text-xs font-bold text-white bg-red-500 rounded-full">
                              {c.unreadCount}
                            </span>
                          )}
                        </div>
                      </div>
                    </div>
                  </li>
                );
              })}
            </ul>
          )}
        </div>
      </div>

      {/* Right Content - Chat Window */}
      <div className="flex-1 flex flex-col bg-gray-50">
        {selectedContactId ? (
          <>
            {/* Chat Header */}
            <div className="p-4 border-b bg-white flex items-center shadow-sm z-10 justify-between">
              <div className="flex items-center">
                <img
                  src={
                    selectedContact?.contactAvatarUrl ||
                    "https://res.cloudinary.com/dx1jnguud/image/upload/v1734027787/users/default_avatar.jpg"
                  }
                  alt="Avatar"
                  className="w-10 h-10 rounded-full mr-3 object-cover"
                />
                <div>
                  <h3 className="font-semibold text-gray-800">
                    {selectedContact?.contactName || "Người dùng"}
                  </h3>
                  <span className="text-xs text-gray-500">
                    {selectedContact?.contactRole || ""}
                  </span>
                </div>
              </div>
              {selectedContact?.contactRole === "Landlord" && user?.role === "tenant" && (
                <button
                  onClick={() => setShowAppointmentModal(true)}
                  className="bg-orange-500 hover:bg-orange-600 text-white px-4 py-2 rounded-lg text-sm font-medium transition-colors"
                >
                  Đặt lịch hẹn
                </button>
              )}
            </div>

            {/* Messages Area */}
            <div className="flex-1 overflow-y-auto p-4 space-y-4 relative">
              {loadingMessages ? (
                <div className="text-center text-gray-500 mt-10">
                  Đang tải tin nhắn...
                </div>
              ) : messages.length === 0 ? (
                <div className="text-center text-gray-500 mt-10">
                  Hãy bắt đầu trò chuyện!
                </div>
              ) : (
                messages.map((msg, idx) => {
                  const isMe = msg.senderId === user?.id;
                  let displayContent = msg.content;
                  let appointmentId: string | null = null;
                  
                  if (displayContent.startsWith("[APPOINTMENT:")) {
                    const match = displayContent.match(/^\[APPOINTMENT:(.+?)\]\s*(.*)$/s);
                    if (match) {
                      appointmentId = match[1];
                      displayContent = match[2];
                    }
                  }

                  return (
                    <div
                      key={msg.id || idx}
                      className={`flex ${
                        isMe ? "justify-end" : "justify-start"
                      }`}
                    >
                      <div
                        className={`max-w-[70%] rounded-2xl px-4 py-2 ${
                          isMe
                            ? "bg-blue-600 text-white rounded-br-none"
                            : "bg-gray-200 text-gray-800 rounded-bl-none"
                        }`}
                      >
                        <p className="text-sm whitespace-pre-wrap font-medium mb-1">
                          {appointmentId ? "📅 Yêu cầu đặt lịch hẹn:" : ""}
                        </p>
                        <p className="text-sm whitespace-pre-wrap">
                          {displayContent}
                        </p>
                        
                        {/* Only show "Xác nhận" if it's an appointment request, user is Landlord, and user is NOT the sender */}
                        {appointmentId && !isMe && user?.role === "landlord" && (
                          <div className="mt-3">
                            <button
                              onClick={() => handleConfirmAppointment(appointmentId as string)}
                              className="bg-orange-500 hover:bg-orange-600 text-white px-4 py-1.5 rounded-lg text-sm font-medium transition-colors w-full"
                            >
                              Xác nhận lịch hẹn
                            </button>
                          </div>
                        )}

                        <span
                          className={`text-[10px] mt-1 block ${
                            isMe ? "text-blue-100" : "text-gray-500"
                          }`}
                        >
                          {new Date(msg.timestamp).toLocaleTimeString("vi-VN", {
                            hour: "2-digit",
                            minute: "2-digit",
                          })}
                        </span>
                      </div>
                    </div>
                  );
                })
              )}
              <div ref={messagesEndRef} />
            </div>

            {/* Message Input */}
            <div className="p-4 bg-white border-t">
              <form onSubmit={handleSendMessage} className="flex space-x-2">
                <input
                  type="text"
                  value={inputText}
                  onChange={(e) => setInputText(e.target.value)}
                  placeholder="Nhập tin nhắn..."
                  className="flex-1 border rounded-full px-4 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
                <button
                  type="submit"
                  disabled={!inputText.trim()}
                  className="bg-blue-600 text-white rounded-full p-2 w-10 h-10 flex items-center justify-center hover:bg-blue-700 disabled:opacity-50 transition-colors"
                >
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    fill="none"
                    viewBox="0 0 24 24"
                    strokeWidth={2}
                    stroke="currentColor"
                    className="w-5 h-5"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      d="M6 12L3.269 3.126A59.768 59.768 0 0121.485 12 59.77 59.77 0 013.27 20.876L5.999 12zm0 0h7.5"
                    />
                  </svg>
                </button>
              </form>
            </div>
          </>
        ) : (
          <div className="flex-1 flex items-center justify-center text-gray-500">
            Chọn một cuộc trò chuyện để bắt đầu
          </div>
        )}
      </div>

      {/* Appointment Modal */}
      {showAppointmentModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
          <div className="bg-white rounded-2xl shadow-2xl w-full max-w-md overflow-hidden">
            <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100">
              <h3 className="text-gray-900" style={{ fontSize: "17px", fontWeight: 700 }}>
                Đặt lịch hẹn xem phòng
              </h3>
              <button onClick={() => setShowAppointmentModal(false)} className="text-gray-400 hover:text-gray-600">
                <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
            <form onSubmit={handleBookAppointment} className="p-6 space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Chọn phòng
                </label>
                <select
                  required
                  value={selectedPropertyId}
                  onChange={(e) => setSelectedPropertyId(e.target.value)}
                  className="w-full px-4 py-2 border border-gray-200 rounded-xl focus:ring-2 focus:ring-orange-500 focus:border-orange-500 outline-none transition-all text-sm"
                >
                  <option value="" disabled>-- Chọn phòng --</option>
                  {landlordProperties.map((p) => (
                    <option key={p.id} value={p.id}>
                      {p.propertyName} - {p.price.toLocaleString("vi-VN")}đ
                    </option>
                  ))}
                </select>
                {landlordProperties.length === 0 && (
                  <p className="text-xs text-red-500 mt-1">Chủ trọ này hiện chưa có phòng nào trống.</p>
                )}
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Ngày xem
                  </label>
                  <input
                    type="date"
                    required
                    min={new Date().toISOString().split("T")[0]}
                    value={appointmentDate}
                    onChange={(e) => setAppointmentDate(e.target.value)}
                    className="w-full px-4 py-2 border border-gray-200 rounded-xl focus:ring-2 focus:ring-orange-500 focus:border-orange-500 outline-none transition-all text-sm"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Giờ xem
                  </label>
                  <input
                    type="time"
                    required
                    value={appointmentTime}
                    onChange={(e) => setAppointmentTime(e.target.value)}
                    className="w-full px-4 py-2 border border-gray-200 rounded-xl focus:ring-2 focus:ring-orange-500 focus:border-orange-500 outline-none transition-all text-sm"
                  />
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Ghi chú thêm (Tùy chọn)
                </label>
                <textarea
                  value={appointmentNote}
                  onChange={(e) => setAppointmentNote(e.target.value)}
                  placeholder="Ví dụ: Mình có thể đến muộn 10 phút..."
                  rows={3}
                  className="w-full px-4 py-2 border border-gray-200 rounded-xl focus:ring-2 focus:ring-orange-500 focus:border-orange-500 outline-none transition-all text-sm resize-none"
                />
              </div>
              <div className="pt-2">
                <button
                  type="submit"
                  disabled={bookingLoading || landlordProperties.length === 0}
                  className="w-full py-3 bg-orange-500 text-white rounded-xl hover:bg-orange-600 transition-colors font-medium disabled:opacity-50"
                >
                  {bookingLoading ? "Đang đặt lịch..." : "Xác nhận đặt lịch"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
