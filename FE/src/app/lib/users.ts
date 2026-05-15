import { apiRequest } from "./api";
import type { PagedResponse, UserResponse } from "./types";

export interface UserSummaryResponse {
  totalUsers: number;
  totalActive: number;
  totalLocked: number;
  totalLandlords: number;
}

export async function getUsers(query: Record<string, string | number | boolean | undefined> = {}) {
  return apiRequest<PagedResponse<UserResponse>>("User/GetUsersByFilter", {
    query,
  });
}

export async function getUserSummary() {
  return apiRequest<UserSummaryResponse>("User/GetUserSummary");
}

export async function getUserByEmail(email: string) {
  return apiRequest<UserResponse>("User/GetUserByIdOrEmail", {
    query: { email },
  });
}

export async function blockUser(token: string, id: string, isBlocked: boolean) {
  return apiRequest<void>("User/BlockUser", {
    method: "PUT",
    authToken: token,
    body: { id, isBlocked },
  });
}
