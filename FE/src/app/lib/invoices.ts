import { apiRequest } from "./api";
import type { PagedResponse } from "./types";

export interface InvoiceResponse {
  id: string;
  contractId: string;
  period: string;
  rentAmount: number;
  oldElectricityReading?: number | null;
  newElectricityReading?: number | null;
  electricityCost?: number | null;
  oldWaterReading?: number | null;
  newWaterReading?: number | null;
  waterCost?: number | null;
  otherFees?: number | null;
  penalty: number;
  total: number;
  note?: string | null;
  status: string;
  invoiceUrl?: string | null;
  receiptUrl?: string | null;
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

export async function getMyInvoices(token: string, query: Record<string, string | number | boolean | undefined> = {}) {
  return apiRequest<PagedResponse<InvoiceResponse>>("Invoice/GetMyInvoices", {
    authToken: token,
    query: { pageSize: 12, ...query },
  });
}

export async function getInvoiceById(token: string, id: string) {
  return apiRequest<InvoiceResponse>("Invoice/GetInvoiceById", {
    authToken: token,
    query: { id },
  });
}

export async function createInvoice(
  token: string,
  input: {
    contractId: string;
    period: string;
    rentAmount: number;
    oldElectricityReading?: number | null;
    newElectricityReading?: number | null;
    electricityCost?: number | null;
    oldWaterReading?: number | null;
    newWaterReading?: number | null;
    waterCost?: number | null;
    otherFees?: number | null;
    penalty?: number;
    total: number;
    note?: string | null;
    status?: string;
    receiptUrl?: string | null;
    dueDate: string;
  },
) {
  return apiRequest<InvoiceResponse>("Invoice/CreateInvoice", {
    method: "POST",
    authToken: token,
    body: {
      ...input,
      penalty: input.penalty ?? 0,
      status: input.status ?? "Pending",
    },
  });
}

export async function updateInvoice(
  token: string,
  input: {
    id: string;
    oldElectricityReading?: number | null;
    newElectricityReading?: number | null;
    electricityCost?: number | null;
    oldWaterReading?: number | null;
    newWaterReading?: number | null;
    waterCost?: number | null;
    otherFees?: number | null;
    penalty?: number | null;
    total?: number | null;
    note?: string | null;
    status?: string | null;
    invoiceUrl?: string | null;
    receiptUrl?: string | null;
    dueDate?: string | null;
  },
) {
  return apiRequest<void>("Invoice/UpdateInvoice", {
    method: "PUT",
    authToken: token,
    body: input,
  });
}
