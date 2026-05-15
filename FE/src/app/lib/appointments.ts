import { apiRequest } from "./api";
import type { PagedResponse } from "./types";

export interface AppointmentResponse {
  id: string;
  propertyId: string;
  userId: string;
  appointmentDateTime: string;
  status: string;
  note?: string | null;
  createdAt: string;
  updatedAt?: string | null;
}

export async function getAppointments(token: string, query: Record<string, string | number | boolean | undefined> = {}) {
  return apiRequest<PagedResponse<AppointmentResponse>>("Appointment/GetAppointmentsByFilter", {
    authToken: token,
    query: { pageSize: 100, ...query },
  });
}

export async function createAppointment(
  token: string,
  input: {
    propertyId: string;
    userId: string;
    appointmentDateTime: string;
    status?: string;
    note?: string;
  },
) {
  return apiRequest<AppointmentResponse>("Appointment/CreateAppointment", {
    method: "POST",
    authToken: token,
    body: input,
  });
}
