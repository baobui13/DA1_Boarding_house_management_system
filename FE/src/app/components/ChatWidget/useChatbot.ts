import { useState, useRef, useCallback } from "react";
import { sendChatMessage } from "../../lib/chatbot";
import type { ChatMessage, EmotionResult } from "../../lib/chatbot";
import { useApp } from "../../context/AppContext";

export interface DisplayMessage {
  id: string;
  role: "user" | "bot";
  text: string;
  emotion?: EmotionResult;
  prompt?: string;
  context?: string;
  timestamp: Date;
}

export function useChatbot() {
  const { token, currentUser } = useApp();
  const [messages, setMessages] = useState<DisplayMessage[]>([
    {
      id: "welcome",
      role: "bot",
      text: "Xin chào! Tôi là trợ lý AI của hệ thống quản lý phòng trọ.\nTôi có thể giúp bạn về hóa đơn, hợp đồng, khiếu nại và lịch hẹn.",
      timestamp: new Date(),
    },
  ]);
  const [suggestions, setSuggestions] = useState<string[]>([
    "Hỏi về hóa đơn",
    "Xem hợp đồng",
    "Đặt lịch hẹn",
    "Gửi khiếu nại",
  ]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const historyRef = useRef<ChatMessage[]>([]);

  const sendMessage = useCallback(
    async (text: string) => {
      if (!text.trim() || isLoading) return;

      const userMsg: DisplayMessage = {
        id: `user-${Date.now()}`,
        role: "user",
        text: text.trim(),
        timestamp: new Date(),
      };

      setMessages((prev) => [...prev, userMsg]);
      setIsLoading(true);
      setError(null);

      // Thêm vào lịch sử chat cho Gemini
      historyRef.current = [
        ...historyRef.current,
        { role: "user", text: text.trim() },
      ];

      try {
        const roleLower = currentUser?.role?.toLowerCase();
        const userRole = roleLower === "landlord" ? "landlord" : "tenant";

        const response = await sendChatMessage(
          text.trim(),
          historyRef.current.slice(0, -1), // lịch sử không bao gồm tin nhắn hiện tại
          userRole,
          token
        );

        const botMsg: DisplayMessage = {
          id: `bot-${Date.now()}`,
          role: "bot",
          text: response.reply,
          timestamp: new Date(),
        };

        // Cập nhật emotion, prompt và context cho tin nhắn user vừa gửi, và thêm tin nhắn của bot
        setMessages((prev) => {
          const updated = prev.map(m => 
            m.id === userMsg.id ? { 
              ...m, 
              emotion: response.emotion,
              prompt: response.prompt,
              context: response.context 
            } : m
          );
          return [...updated, botMsg];
        });
        setSuggestions(response.suggestions ?? []);

        // Thêm phản hồi AI vào lịch sử
        historyRef.current = [
          ...historyRef.current,
          { role: "model", text: response.reply },
        ];

        // Giữ lịch sử tối đa 10 lượt (20 tin nhắn)
        if (historyRef.current.length > 20) {
          historyRef.current = historyRef.current.slice(-20);
        }
      } catch {
        setError("Không thể kết nối với trợ lý AI. Vui lòng thử lại.");
        setMessages((prev) => prev.filter((m) => m.id !== userMsg.id));
        historyRef.current = historyRef.current.slice(0, -1);
      } finally {
        setIsLoading(false);
      }
    },
    [isLoading, token, currentUser?.role]
  );

  const clearHistory = useCallback(() => {
    historyRef.current = [];
    setMessages([
      {
        id: "welcome-new",
        role: "bot",
        text: "Cuộc trò chuyện mới đã bắt đầu. Tôi có thể giúp gì cho bạn?",
        timestamp: new Date(),
      },
    ]);
    setSuggestions([
      "Hỏi về hóa đơn",
      "Xem hợp đồng",
      "Đặt lịch hẹn",
      "Gửi khiếu nại",
    ]);
    setError(null);
  }, []);

  return {
    messages,
    suggestions,
    isLoading,
    error,
    sendMessage,
    clearHistory,
  };
}
