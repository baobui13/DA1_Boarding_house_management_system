import { apiRequest } from "./api";
import type { PagedResponse } from "./types";

export interface SearchHistoryResponse {
  id: string;
  userId: string;
  filters: string;
  createdAt: string;
}

export async function getSearchHistories(token: string, query: Record<string, string | number | boolean | undefined> = {}) {
  return apiRequest<PagedResponse<SearchHistoryResponse>>("SearchHistory/GetSearchHistoriesByFilter", {
    authToken: token,
    query: { pageSize: 50, ...query },
  });
}

export async function createSearchHistory(
  token: string,
  input: {
    userId: string;
    filters: string; // JSON string of search criteria, e.g. {"q":"...","district":"..."}
  },
) {
  return apiRequest<any>("SearchHistory/CreateSearchHistory", {
    method: "POST",
    authToken: token,
    body: input,
  });
}
