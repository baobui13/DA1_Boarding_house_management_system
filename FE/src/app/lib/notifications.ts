import { apiRequest } from "./api";
import type { PagedResponse } from "./types";

export interface NotificationResponse {
  id: string;
  userId: string;
  type: string;
  content: string;
  isRead: boolean;
  timestamp: string;
  relatedId?: string | null;
}

export async function getNotifications(token: string, query: Record<string, string | number | boolean | undefined> = {}) {
  return apiRequest<PagedResponse<NotificationResponse>>("Notification/GetNotificationsByFilter", {
    authToken: token,
    query: { pageSize: 100, ...query },
  });
}
