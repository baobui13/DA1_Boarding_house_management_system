import { apiRequest } from "./api";
import type { PagedResponse } from "./types";

export interface ContractResponse {
  id: string;
  propertyId: string;
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

export async function createContract(
  token: string,
  input: {
    propertyId: string;
    tenantId: string;
    startDate: string;
    endDate: string;
    deposit: number;
    terms?: string | null;
    contractFileUrl?: string | null;
    status?: string;
  },
) {
  return apiRequest<ContractResponse>("Contract/CreateContract", {
    method: "POST",
    authToken: token,
    body: {
      ...input,
      status: input.status ?? "Active",
    },
  });
}

export async function updateContract(
  token: string,
  input: {
    id: string;
    endDate?: string | null;
    terms?: string | null;
    contractFileUrl?: string | null;
    status?: string | null;
    actualEndDate?: string | null;
    handoverNote?: string | null;
    deductionAmount?: number | null;
    deductionReason?: string | null;
    refundAmount?: number | null;
    handoverConfirmedBy?: string | null;
    handoverConfirmedAt?: string | null;
  },
) {
  return apiRequest<void>("Contract/UpdateContract", {
    method: "PUT",
    authToken: token,
    body: input,
  });
}
