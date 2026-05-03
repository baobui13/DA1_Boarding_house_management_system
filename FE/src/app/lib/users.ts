import { apiRequest } from "./api";
import type { PagedResponse, UserResponse } from "./types";

export async function getUsers(query: Record<string, string | number | boolean | undefined> = {}) {
  return apiRequest<PagedResponse<UserResponse>>("User/GetUsersByFilter", {
    query: { pageSize: 100, ...query },
  });
}

export async function blockUser(token: string, id: string, isBlocked: boolean) {
  return apiRequest<void>("User/BlockUser", {
    method: "PUT",
    authToken: token,
    body: { id, isBlocked },
  });
}
