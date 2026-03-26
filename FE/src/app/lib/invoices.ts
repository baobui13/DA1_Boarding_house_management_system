import { apiRequest } from "./api";
import type { PagedResponse } from "./types";

export interface InvoiceResponse {
  id: string;
  contractId: string;
  period: string;
  rentAmount: number;
  electricityUsage?: number | null;
  electricityCost?: number | null;
  waterUsage?: number | null;
  waterCost?: number | null;
  otherFees?: number | null;
  penalty: number;
  total: number;
  note?: string | null;
  status: string;
  invoiceUrl?: string | null;
  dueDate: string;
  createdAt: string;
  updatedAt?: string | null;
}

export async function getInvoices(token: string, query: Record<string, string | number | boolean | undefined> = {}) {
  return apiRequest<PagedResponse<InvoiceResponse>>("Invoice/GetInvoicesByFilter", {
    authToken: token,
    query: { pageSize: 100, ...query },
  });
}

export async function getInvoiceById(token: string, id: string) {
  return apiRequest<InvoiceResponse>("Invoice/GetInvoiceById", {
    authToken: token,
    query: { id },
  });
}
