import { apiRequest } from "./api";
import type { PagedResponse } from "./types";

export interface ContractResponse {
  id: string;
  roomId: string;
  tenantId: string;
  startDate: string;
  endDate: string;
  deposit: number;
  terms?: string | null;
  contractFileUrl?: string | null;
  status: string;
  actualEndDate?: string | null;
  handoverNote?: string | null;
  deductionAmount: number;
  deductionReason?: string | null;
  refundAmount: number;
  handoverConfirmedBy?: string | null;
  handoverConfirmedAt?: string | null;
  createdAt: string;
  updatedAt?: string | null;
}

export async function getContracts(token: string, query: Record<string, string | number | boolean | undefined> = {}) {
  return apiRequest<PagedResponse<ContractResponse>>("Contract/GetContractsByFilter", {
    authToken: token,
    query: { pageSize: 100, ...query },
  });
}

export async function getContractById(token: string, id: string) {
  return apiRequest<ContractResponse>("Contract/GetContractById", {
    authToken: token,
    query: { id },
  });
}
