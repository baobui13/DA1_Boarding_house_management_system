const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL || "http://localhost:5046/api").replace(/\/+$/, "");
const SESSION_KEY = "qlt.session";

export class ApiError extends Error {
  status: number;

  constructor(message: string, status: number) {
    super(message);
    this.name = "ApiError";
    this.status = status;
  }
}

// --- Token refresh handling (to support expired access tokens) ---
let refreshPromise: Promise<string | null> | null = null;

async function readRefreshToken(): Promise<string | null> {
  try {
    const raw = localStorage.getItem(SESSION_KEY);
    if (!raw) return null;
    const parsed = JSON.parse(raw) as { refreshToken?: string | null };
    return parsed?.refreshToken || null;
  } catch {
    return null;
  }
}

async function writeNewTokens(newAuth: { token?: string; refreshToken?: string | null }) {
  try {
    const raw = localStorage.getItem(SESSION_KEY);
    if (!raw) return;
    const session = JSON.parse(raw);
    if (newAuth.token) session.token = newAuth.token;
    if (newAuth.refreshToken !== undefined) session.refreshToken = newAuth.refreshToken;
    localStorage.setItem(SESSION_KEY, JSON.stringify(session));
  } catch {
    // ignore storage errors
  }
}

async function performRefresh(): Promise<string | null> {
  const currentRefresh = await readRefreshToken();
  if (!currentRefresh) return null;

  try {
    const res = await fetch(`${API_BASE_URL}/Auth/refresh-token`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ refreshToken: currentRefresh }),
    });

    if (!res.ok) {
      // Refresh failed (expired/invalid) → clear session so user will be forced to re-login on next protected action
      localStorage.removeItem(SESSION_KEY);
      return null;
    }

    const auth = (await res.json()) as { token?: string; refreshToken?: string | null };
    await writeNewTokens(auth);
    return auth.token || null;
  } catch {
    return null;
  }
}

async function getValidToken(originalAuthToken?: string | null): Promise<string | null> {
  // If caller passed a token and it might be fresh, prefer trying with it first (the 401 path will trigger refresh).
  // The main job of this helper is the shared refresh promise.
  if (refreshPromise) {
    const newToken = await refreshPromise;
    return newToken || originalAuthToken || null;
  }

  // No ongoing refresh — the caller will hit 401 path which starts one.
  return originalAuthToken || null;
}

type QueryValue = string | number | boolean | null | undefined;

interface RequestOptions {
  method?: "GET" | "POST" | "PUT" | "DELETE";
  body?: BodyInit | Record<string, unknown>;
  headers?: Record<string, string>;
  query?: Record<string, QueryValue>;
  authToken?: string | null;
}

function buildUrl(path: string, query?: Record<string, QueryValue>) {
  const url = new URL(`${API_BASE_URL}/${path.replace(/^\/+/, "")}`);

  if (query) {
    Object.entries(query).forEach(([key, value]) => {
      if (value === undefined || value === null || value === "") return;
      url.searchParams.set(key, String(value));
    });
  }

  return url.toString();
}

async function executeRequest(
  path: string,
  options: RequestOptions,
  authTokenToUse?: string | null,
): Promise<{ response: Response; payload: any }> {
  const { method = "GET", body, headers = {}, query } = options;

  const requestHeaders = new Headers(headers);
  let requestBody: BodyInit | undefined;

  const token = authTokenToUse ?? options.authToken;
  if (token) {
    requestHeaders.set("Authorization", `Bearer ${token}`);
  }

  if (body instanceof FormData) {
    requestBody = body;
  } else if (body !== undefined) {
    requestHeaders.set("Content-Type", "application/json");
    requestBody = JSON.stringify(body);
  }

  const response = await fetch(buildUrl(path, query), {
    method,
    headers: requestHeaders,
    body: requestBody,
  });

  const contentType = response.headers.get("content-type") || "";
  const payload = contentType.includes("application/json")
    ? await response.json().catch(() => null)
    : await response.text();

  return { response, payload };
}

export async function apiRequest<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const { authToken } = options;

  let attemptResponse: Response;
  let attemptPayload: any;

  try {
    // First attempt
    const first = await executeRequest(path, options, authToken);
    attemptResponse = first.response;
    attemptPayload = first.payload;
  } catch (error) {
    const message =
      error instanceof Error && /fetch|load failed|network/i.test(error.message)
        ? "Khong the ket noi backend. Hay kiem tra backend dang chay va CORS da duoc bat."
        : error instanceof Error
        ? error.message
        : "Khong the ket noi backend.";
    throw new ApiError(message, 0);
  }

  // Auto refresh + retry on 401 (expired access token)
  if (attemptResponse.status === 401) {
    if (!refreshPromise) {
      refreshPromise = performRefresh().finally(() => {
        refreshPromise = null;
      });
    }

    const freshToken = await refreshPromise;

    if (freshToken) {
      try {
        const retry = await executeRequest(path, options, freshToken);
        attemptResponse = retry.response;
        attemptPayload = retry.payload;
      } catch (error) {
        const message =
          error instanceof Error && /fetch|load failed|network/i.test(error.message)
            ? "Khong the ket noi backend. Hay kiem tra backend dang chay va CORS da duoc bat."
            : error instanceof Error
            ? error.message
            : "Khong the ket noi backend.";
        throw new ApiError(message, 0);
      }
    } else {
      const message =
        typeof attemptPayload === "string"
          ? attemptPayload
          : attemptPayload?.message || attemptPayload?.title || "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
      throw new ApiError(message, 401);
    }
  }

  if (!attemptResponse.ok) {
    const message =
      typeof attemptPayload === "string"
        ? attemptPayload
        : attemptPayload?.message || attemptPayload?.title || "Yeu cau API that bai.";
    throw new ApiError(message, attemptResponse.status);
  }

  return attemptPayload as T;
}

export { API_BASE_URL };
