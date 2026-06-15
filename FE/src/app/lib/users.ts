import { apiRequest } from "./api";
import type { PagedResponse, UserResponse } from "./types";

export interface UserSummaryResponse {
  totalUsers: number;
  totalActive: number;
  totalLocked: number;
  totalLandlords: number;
}

export async function getUsers(
  query: Record<string, string | number | boolean | undefined> = {},
  token?: string,
) {
  return apiRequest<PagedResponse<UserResponse>>("User/GetUsersByFilter", {
    query,
    ...(token ? { authToken: token } : {}),
  });
}

export async function getUserSummary(token?: string) {
  return apiRequest<UserSummaryResponse>("User/GetUserSummary", {
    ...(token ? { authToken: token } : {}),
  });
}

export async function getUserByEmail(email: string, token?: string) {
  return apiRequest<UserResponse>("User/GetUserByIdOrEmail", {
    query: { email },
    ...(token ? { authToken: token } : {}),
  });
}

export async function getUserById(id: string, token?: string) {
  return apiRequest<UserResponse>("User/GetUserByIdOrEmail", {
    query: { id },
    ...(token ? { authToken: token } : {}),
  });
}

export async function blockUser(token: string, id: string, isBlocked: boolean) {
  return apiRequest<void>("User/BlockUser", {
    method: "PUT",
    authToken: token,
    body: { id, isBlocked },
  });
}
