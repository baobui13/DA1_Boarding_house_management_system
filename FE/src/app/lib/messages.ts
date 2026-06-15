import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
} from "@microsoft/signalr";

export interface MessageResponse {
  id: string;
  senderId: string;
  receiverId: string;
  content: string;
  timestamp: string;
  isRead: boolean;
  propertyId?: string;
  contractId?: string;
}

// Types based on the backend DTOs
export interface ConversationResponse {
  contactId: string;
  contactName: string;
  contactAvatarUrl?: string;
  contactRole: string;
  lastMessage: MessageResponse;
  unreadCount: number;
}

export interface SendMessageRequest {
  senderId: string;
  receiverId: string;
  content: string;
  propertyId?: string;
  contractId?: string;
}

const API_BASE_URL = "http://localhost:5046/api/Message";
const HUB_URL = "http://localhost:5046/chathub";

/**
 * Fetch list of conversations for current user
 */
export async function getConversations(
  token: string
): Promise<ConversationResponse[]> {
  const response = await fetch(`${API_BASE_URL}/GetConversations`, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });
  if (!response.ok) throw new Error("Failed to fetch conversations");
  return response.json();
}

/**
 * Fetch messages for a specific conversation
 */
export async function getMessages(
  token: string,
  currentUserId: string,
  contactId: string,
  page: number = 1
) {
  // We use Plainquire filter format.
  // sender == current & receiver == contact OR sender == contact & receiver == current
  // Since Plainquire might be tricky with OR, if we just want messages between 2 users, we can filter backend
  // For now, let's use the GetMessagesByFilter API.
  // Alternatively, we can just fetch and filter locally if there's no complex query builder.
  const filterParams = new URLSearchParams();
  // Simplified: we will just fetch sorted messages.
  // This might be better handled by a custom backend API, but we'll try to fetch all user messages and filter locally for now if API doesn't support complex OR.
  // Wait, we can just pass the filter:
  // Since we don't have a direct conversation API, we'll fetch a larger page and filter locally, or use Plainquire OR syntax.
  const response = await fetch(`${API_BASE_URL}/GetMessagesByFilter?sort=-Timestamp&pageNumber=${page}&pageSize=50`, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });
  if (!response.ok) throw new Error("Failed to fetch messages");
  const data = await response.json();
  const messages: MessageResponse[] = data.items;
  return messages.filter(m => 
    (m.senderId === currentUserId && m.receiverId === contactId) ||
    (m.senderId === contactId && m.receiverId === currentUserId)
  ).sort((a, b) => new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime());
}

/**
 * Send a message via REST
 */
export async function sendMessage(
  token: string,
  payload: SendMessageRequest
): Promise<MessageResponse> {
  const response = await fetch(`${API_BASE_URL}/CreateMessage`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify(payload),
  });
  if (!response.ok) throw new Error("Failed to send message");
  return response.json();
}

/**
 * Mark a conversation as read
 */
export async function markConversationAsRead(
  token: string,
  senderId: string
): Promise<void> {
  const response = await fetch(`${API_BASE_URL}/MarkAsRead`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify({ senderId }),
  });
  if (!response.ok) throw new Error("Failed to mark as read");
}

/**
 * Setup SignalR Connection
 */
export function setupSignalRConnection(token: string): HubConnection {
  const connection = new HubConnectionBuilder()
    .withUrl(`${HUB_URL}?access_token=${token}`)
    .configureLogging(LogLevel.Information)
    .withAutomaticReconnect()
    .build();

  return connection;
}
