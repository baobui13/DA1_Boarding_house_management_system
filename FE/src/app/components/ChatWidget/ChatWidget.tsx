import { useState, useRef, useEffect, KeyboardEvent } from "react";
import "./ChatWidget.css";
import { useChatbot } from "./useChatbot";
import type { EmotionResult } from "../../lib/chatbot";

// Emotion Badge
function EmotionBadge({ emotion }: { emotion: EmotionResult }) {
  if (emotion.label === "neutral" || emotion.score < 0.3) return null;

  const urgencyClass =
    emotion.urgency === "high"
      ? "emotion-high"
      : emotion.urgency === "medium"
      ? "emotion-medium"
      : "emotion-low";

  const urgencyIcon =
    emotion.urgency === "high" ? "🔴" : emotion.urgency === "medium" ? "🟡" : "🟢";

  return (
    <span className={`chat-emotion-badge ${urgencyClass}`}>
      {urgencyIcon} {emotion.label_vi}
    </span>
  );
}

// Typing Indicator
function TypingIndicator() {
  return (
    <div className="chat-message bot">
      <div className="chat-typing">
        <span className="typing-dot" />
        <span className="typing-dot" />
        <span className="typing-dot" />
      </div>
    </div>
  );
}

// Time Formatter
function formatTime(date: Date) {
  return date.toLocaleTimeString("vi-VN", {
    hour: "2-digit",
    minute: "2-digit",
  });
}

// Main ChatWidget
export default function ChatWidget() {
  const [isOpen, setIsOpen] = useState(false);
  const [isInspectorOpen, setIsInspectorOpen] = useState(false);
  const [inputText, setInputText] = useState("");
  const [hasNewMessage, setHasNewMessage] = useState(true);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLTextAreaElement>(null);
  const isSendingRef = useRef(false); // Khóa đồng bộ ngăn chặn click kép gửi tin trùng lặp

  const { messages, suggestions, isLoading, error, sendMessage, clearHistory } =
    useChatbot();

  // Tìm tin nhắn gần nhất có data cảm xúc để hiển thị lên Inspector
  const latestEmotionMsg = [...messages]
    .reverse()
    .find((m) => m.role === "user" && m.emotion);
  const emotionData = latestEmotionMsg?.emotion;
  const promptData = latestEmotionMsg?.prompt;
  const contextData = latestEmotionMsg?.context;

  // Scroll xuống cuối khi có tin nhắn mới
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages, isLoading]);

  // Focus input khi mở chat
  useEffect(() => {
    if (isOpen) {
      setTimeout(() => inputRef.current?.focus(), 100);
      setHasNewMessage(false);
    }
  }, [isOpen]);

  const handleSend = async () => {
    if (!inputText.trim() || isLoading || isSendingRef.current) return;
    
    try {
      isSendingRef.current = true; // Kích hoạt khóa ngay lập tức (đồng bộ)
      const text = inputText;
      setInputText("");
      await sendMessage(text);
      
      // Tự động mở inspector khi người dùng gửi tin nhắn đầu tiên (nếu chưa mở)
      if (!isInspectorOpen && messages.length === 0) {
        setIsInspectorOpen(true);
      }
    } finally {
      isSendingRef.current = false; // Mở khóa khi quá trình gửi hoàn tất
    }
  };

  const handleKeyDown = (e: KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  const handleSuggestion = (text: string) => {
    const cleanText = text.replace(/^[^\w\s]+\s*/, "");
    setInputText(cleanText);
    inputRef.current?.focus();
  };

  return (
    <>
      {isOpen && (
        <div className="chat-wrapper">
          {/* AI Inspector Panel */}
          {isInspectorOpen && (
            <div className="chat-inspector" role="complementary" aria-label="AI Analysis Inspector">
              <div className="inspector-header">
                <h3>AI Inspector</h3>
                <button 
                  className="inspector-close-btn" 
                  onClick={() => setIsInspectorOpen(false)}
                  title="Đóng Inspector"
                >
                  ✕
                </button>
              </div>

              <div className="inspector-content">
                {/* Status Box */}
                <div>
                  <div className="inspector-section-title">Hệ thống đang chạy</div>
                  <div className="inspector-box">
                    <div className="inspector-row">
                      <span className="inspector-label">Generative AI:</span>
                      <span className="inspector-value text-green-600">Gemini 2.5 Flash</span>
                    </div>
                    <div className="inspector-row">
                      <span className="inspector-label">NPL Encoder:</span>
                      <span className="inspector-value text-blue-600">BERT Multilingual</span>
                    </div>
                    <div className="inspector-row">
                      <span className="inspector-label">Trạng thái:</span>
                      <span className="inspector-value">
                        {isLoading ? "Đang phân tích..." : "Sẵn sàng"}
                      </span>
                    </div>
                  </div>
                </div>

                {/* Prompt & Context Injected */}
                {promptData && (
                  <div>
                    <div className="inspector-section-title">System Prompt (Chỉ dẫn AI)</div>
                    <details className="inspector-details">
                      <summary>Xem chi tiết System Prompt</summary>
                      <div className="inspector-box-code">
                        <pre className="inspector-pre">{promptData}</pre>
                      </div>
                    </details>
                  </div>
                )}

                {contextData && (
                  <div>
                    <div className="inspector-section-title">Ngữ cảnh CSDL gửi AI (RAG)</div>
                    <details className="inspector-details" open>
                      <summary>Xem chi tiết Context</summary>
                      <div className="inspector-box-code">
                        <pre className="inspector-pre">{contextData}</pre>
                      </div>
                    </details>
                  </div>
                )}

                {!promptData && !contextData && (
                  <div>
                    <div className="inspector-section-title">Dữ liệu phân tích</div>
                    <div className="inspector-box" style={{ textAlign: 'center', color: 'var(--muted-foreground)' }}>
                      Chưa có dữ liệu. Hãy gửi tin nhắn để xem chi tiết.
                    </div>
                  </div>
                )}
              </div>
            </div>
          )}

          {/* Main Chat Window */}
          <div className="chat-window" role="dialog" aria-label="AI Chat Assistant">
            {/* Header */}
            <div className="chat-header">
              <div className="chat-header-avatar">
                <svg viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
                  <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 14.5v-9l6 4.5-6 4.5z" />
                </svg>
              </div>
              <div className="chat-header-info">
                <div className="chat-header-name">Trợ lý AI Phòng Trọ</div>
                <div className="chat-header-status">
                  <span className="chat-status-dot" />
                  Đang hoạt động
                </div>
              </div>
              <div className="chat-header-actions">
                <button
                  className={`chat-header-btn ${isInspectorOpen ? "active" : ""}`}
                  onClick={() => setIsInspectorOpen(!isInspectorOpen)}
                  title="Bật/Tắt AI Inspector"
                  aria-label="Toggle Inspector"
                >
                  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <rect x="3" y="3" width="18" height="18" rx="2" ry="2"></rect>
                    <line x1="9" y1="3" x2="9" y2="21"></line>
                  </svg>
                </button>
                <button
                  className="chat-header-btn"
                  onClick={clearHistory}
                  title="Bắt đầu cuộc trò chuyện mới"
                  aria-label="Xóa lịch sử chat"
                >
                  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <path d="M3 12a9 9 0 1 0 9-9 9.75 9.75 0 0 0-6.74 2.74L3 8"></path>
                    <path d="M3 3v5h5"></path>
                  </svg>
                </button>
                <button
                  className="chat-header-btn"
                  onClick={() => setIsOpen(false)}
                  title="Đóng chat"
                  aria-label="Đóng cửa sổ chat"
                >
                  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <line x1="18" y1="6" x2="6" y2="18"></line>
                    <line x1="6" y1="6" x2="18" y2="18"></line>
                  </svg>
                </button>
              </div>
            </div>

            {/* Messages */}
            <div className="chat-messages" role="log" aria-live="polite">
              {messages.map((msg) => (
                <div
                  key={msg.id}
                  className={`chat-message ${msg.role}`}
                >
                  <div className="chat-bubble">{msg.text}</div>
                  <span className="chat-time">{formatTime(msg.timestamp)}</span>
                </div>
              ))}

              {isLoading && <TypingIndicator />}
              <div ref={messagesEndRef} />
            </div>

            {/* Suggestions */}
            {suggestions.length > 0 && !isLoading && (
              <div className="chat-suggestions">
                {suggestions.map((s) => (
                  <button
                    key={s}
                    className="chat-suggestion-btn"
                    onClick={() => handleSuggestion(s)}
                  >
                    {s}
                  </button>
                ))}
              </div>
            )}

            {/* Error */}
            {error && <div className="chat-error">{error}</div>}

            {/* Input */}
            <div className="chat-input-area">
              <div className="chat-input-row">
                <textarea
                  ref={inputRef}
                  id="chat-input"
                  className="chat-input"
                  value={inputText}
                  onChange={(e) => setInputText(e.target.value)}
                  onKeyDown={handleKeyDown}
                  placeholder="Nhắn tin với trợ lý AI..."
                  rows={1}
                  disabled={isLoading}
                  aria-label="Nhập tin nhắn"
                />
                <button
                  id="chat-send-btn"
                  className="chat-send-btn"
                  onClick={handleSend}
                  disabled={!inputText.trim() || isLoading}
                  aria-label="Gửi tin nhắn"
                >
                  <svg viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
                    <path d="M2.01 21L23 12 2.01 3 2 10l15 2-15 2z" />
                  </svg>
                </button>
              </div>
              <p className="chat-input-hint">Enter để gửi · Shift+Enter xuống dòng</p>
            </div>
          </div>
        </div>
      )}

      {/* FAB Button */}
      <button
        id="chat-fab-btn"
        className={`chat-fab ${isOpen ? "open" : ""}`}
        onClick={() => setIsOpen((o) => !o)}
        aria-label={isOpen ? "Đóng chat" : "Mở chat với trợ lý AI"}
        title="Trợ lý AI Phòng Trọ"
      >
        {isOpen ? (
          <svg viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
            <path d="M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z" />
          </svg>
        ) : (
          <svg viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
            <path d="M20 2H4c-1.1 0-2 .9-2 2v18l4-4h14c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2zm-2 12H6v-2h12v2zm0-3H6V9h12v2zm0-3H6V6h12v2z" />
          </svg>
        )}
        {!isOpen && hasNewMessage && <span className="chat-fab-dot" />}
      </button>
    </>
  );
}
