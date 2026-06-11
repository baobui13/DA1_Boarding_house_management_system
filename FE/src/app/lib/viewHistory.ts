import { apiRequest } from "./api";
import type { PagedResponse } from "./types";

export interface ViewHistoryResponse {
  id: string;
  userId: string;
  propertyId: string;
  createdAt: string;
}

export async function getViewHistories(token: string, query: Record<string, string | number | boolean | undefined> = {}) {
  return apiRequest<PagedResponse<ViewHistoryResponse>>("ViewHistory/GetViewHistoriesByFilter", {
    authToken: token,
    query: { pageSize: 50, ...query },
  });
}

export async function createViewHistory(
  token: string,
  input: {
    userId: string;
    propertyId: string;
  },
) {
  return apiRequest<any>("ViewHistory/CreateViewHistory", {
    method: "POST",
    authToken: token,
    body: input,
  });
}
