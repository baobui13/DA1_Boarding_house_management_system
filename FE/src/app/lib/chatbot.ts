import { apiRequest } from "../lib/api";

export interface ChatMessage {
  role: "user" | "model";
  text: string;
}

export interface EmotionResult {
  label: string;
  label_vi: string;
  score: number;
  urgency: "low" | "medium" | "high";
  source: string;
  all_scores: Record<string, number>;
}

export interface ChatResponse {
  reply: string;
  emotion: EmotionResult;
  suggestions: string[];
  prompt: string;
  context: string;
}

export interface ComplaintAnalysisResponse {
  emotion: EmotionResult;
  urgency: string;
  suggested_category: string;
  suggested_response: string;
}

export async function sendChatMessage(
  message: string,
  history: ChatMessage[],
  userRole: "tenant" | "landlord" = "tenant",
  token?: string | null
): Promise<ChatResponse> {
  return apiRequest<ChatResponse>("chatbot/Chat", {
    method: "POST",
    body: {
      message,
      history,
      userRole,
    },
    authToken: token,
  });
}

export async function analyzeComplaint(
  content: string,
  tenantId: string,
  tenantName: string,
  token?: string | null
): Promise<ComplaintAnalysisResponse> {
  return apiRequest<ComplaintAnalysisResponse>("chatbot/AnalyzeComplaint", {
    method: "POST",
    body: { content, tenantId, tenantName },
    authToken: token,
  });
}
