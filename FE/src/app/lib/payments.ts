import { apiRequest } from "./api";
import type { PagedResponse } from "./types";

export interface PaymentResponse {
  id: string;
  invoiceId: string;
  amount: number;
  method?: string | null;
  status: string;
  transactionRef?: string | null;
  paidAt?: string | null;
  createdAt: string;
}

export async function getPayments(token: string, query: Record<string, string | number | boolean | undefined> = {}) {
  return apiRequest<PagedResponse<PaymentResponse>>("Payment/GetPaymentsByFilter", {
    authToken: token,
    query: { pageSize: 100, ...query },
  });
}

export async function getPaymentById(token: string, id: string) {
  return apiRequest<PaymentResponse>("Payment/GetPaymentById", {
    authToken: token,
    query: { id },
  });
}

export async function createPayment(
  token: string,
  input: {
    invoiceId: string;
    amount: number;
    method?: string | null;
    transactionRef?: string | null;
  },
) {
  return apiRequest<PaymentResponse>("Payment/CreatePayment", {
    method: "POST",
    authToken: token,
    body: input,
  });
}

export async function deletePayment(token: string, id: string) {
  return apiRequest<void>("Payment/DeletePayment", {
    method: "DELETE",
    authToken: token,
    body: { id },
  });
}
