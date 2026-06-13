import { apiRequest } from "./api";
import type { PagedResponse } from "./types";

export interface ComplaintResponse {
  id: string;
  propertyId?: string | null;
  creatorId: string;
  subject: string;
  content: string;
  status: string;
  resolutionNote?: string | null;
  createdAt: string;
  updatedAt?: string | null;
}

export async function getComplaints(query: Record<string, string | number | boolean | undefined> = {}) {
  return apiRequest<PagedResponse<ComplaintResponse>>("Complaint/GetComplaintsByFilter", {
    query: { pageSize: 100, ...query },
  });
}

export async function getComplaintById(id: string) {
  return apiRequest<ComplaintResponse>("Complaint/GetComplaintById", {
    query: { id },
  });
}

export async function createComplaint(
  token: string,
  input: {
    propertyId?: string | null;
    subject: string;
    content: string;
  },
) {
  return apiRequest<ComplaintResponse>("Complaint/CreateComplaint", {
    method: "POST",
    authToken: token,
    body: input,
  });
}

export async function updateComplaint(
  token: string,
  input: {
    id: string;
    subject?: string;
    content?: string;
    status?: string;
    resolutionNote?: string | null;
  },
) {
  return apiRequest<void>("Complaint/UpdateComplaint", {
    method: "PUT",
    authToken: token,
    body: input,
  });
}

export async function deleteComplaint(token: string, id: string) {
  return apiRequest<void>("Complaint/DeleteComplaint", {
    method: "DELETE",
    authToken: token,
    query: { id },
  });
}
