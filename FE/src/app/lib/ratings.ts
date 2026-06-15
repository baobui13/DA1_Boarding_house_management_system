import { apiRequest } from "./api";
import type { PagedResponse } from "./types";

export interface RatingResponse {
  id: string;
  propertyId: string;
  tenantId: string;
  stars: number;
  content: string;
  aiAttitude: string;
  createdAt: string;
  updatedAt?: string | null;
}

export async function getRatings(query: Record<string, string | number | boolean | undefined> = {}, token?: string) {
  return apiRequest<PagedResponse<RatingResponse>>("Rating/GetRatingsByFilter", {
    ...(token ? { authToken: token } : {}),
    query: { pageSize: 100, ...query },
  });
}

export async function getRatingById(id: string) {
  return apiRequest<RatingResponse>("Rating/GetRatingById", {
    query: { id },
  });
}

export interface RatingDetailResponse extends RatingResponse {
  tenant?: {
    id: string;
    fullName: string;
    email: string;
    phoneNumber?: string;
    avatarUrl?: string;
  };
  property?: {
    id: string;
    propertyName: string;
    address: string;
    price: number;
    size: number;
    images: string[];
    status: string;
  };
}

export async function getRatingDetailById(id: string) {
  return apiRequest<RatingDetailResponse>("Rating/GetRatingDetailById", {
    query: { id },
  });
}

export async function createRating(
  token: string,
  input: {
    propertyId: string;
    tenantId: string;
    stars: number;
    content: string;
    aiAttitude: string;
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
    stars?: number;
    content?: string;
    aiAttitude?: string;
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
