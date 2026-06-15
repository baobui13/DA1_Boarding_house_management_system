import { apiRequest } from "./api";
import type { AreaResponse, PagedResponse } from "./types";

export async function getAreas(query: Record<string, string | number | boolean | undefined> = {}) {
  return apiRequest<PagedResponse<AreaResponse>>("Area/GetAreasByFilter", {
    query: { pageSize: 100, ...query },
  });
}

export async function getMyAreas(token: string, query: Record<string, string | number | boolean | undefined> = {}) {
  return apiRequest<PagedResponse<AreaResponse>>("Area/GetMyAreas", {
    authToken: token,
    query: { pageSize: 1000, ...query },
  });
}

export async function createArea(
  token: string,
  input: {
    name: string;
    address: string;
    landlordId: string;
    description?: string;
    roomCount?: number;
  },
) {
  return apiRequest<AreaResponse>("Area/CreateArea", {
    method: "POST",
    authToken: token,
    body: input,
  });
}

export async function updateArea(
  token: string,
  input: {
    id: string;
    name?: string;
    address?: string;
    description?: string;
    roomCount?: number;
  },
) {
  return apiRequest<void>("Area/UpdateArea", {
    method: "PUT",
    authToken: token,
    body: input,
  });
}

export async function deleteArea(token: string, id: string) {
  return apiRequest<void>("Area/DeleteArea", {
    method: "DELETE",
    authToken: token,
    body: { id },
  });
}
