import { apiRequest } from "./api";
import type { PagedResponse } from "./types";

export interface RatingResponse {
  id: string;
  propertyId: string;
  tenantId: string;
  score: number;
  comment?: string | null;
  createdAt: string;
  updatedAt?: string | null;
}

export async function getRatings(query: Record<string, string | number | boolean | undefined> = {}) {
  return apiRequest<PagedResponse<RatingResponse>>("Rating/GetRatingsByFilter", {
    query: { pageSize: 100, ...query },
  });
}

export async function getRatingById(id: string) {
  return apiRequest<RatingResponse>("Rating/GetRatingById", {
    query: { id },
  });
}

export async function createRating(
  token: string,
  input: {
    propertyId: string;
    tenantId: string;
    score: number;
    comment?: string | null;
  },
) {
  return apiRequest<RatingResponse>("Rating/CreateRating", {
    method: "POST",
    authToken: token,
    body: input,
  });
}

export async function updateRating(
  token: string,
  input: {
    id: string;
    score?: number;
    comment?: string | null;
  },
) {
  return apiRequest<void>("Rating/UpdateRating", {
    method: "PUT",
    authToken: token,
    body: input,
  });
}

export async function deleteRating(token: string, id: string) {
  return apiRequest<void>("Rating/DeleteRating", {
    method: "DELETE",
    authToken: token,
    query: { id },
  });
}
