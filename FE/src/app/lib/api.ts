const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL || "http://localhost:5046/api").replace(/\/+$/, "");

export class ApiError extends Error {
  status: number;

  constructor(message: string, status: number) {
    super(message);
    this.name = "ApiError";
    this.status = status;
  }
}

type QueryValue = string | number | boolean | null | undefined;

interface RequestOptions {
  method?: "GET" | "POST" | "PUT" | "DELETE";
  body?: BodyInit | Record<string, unknown>;
  headers?: Record<string, string>;
  query?: Record<string, QueryValue>;
  authToken?: string | null;
  _isRetry?: boolean;
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

export async function apiRequest<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const { method = "GET", body, headers = {}, query, authToken } = options;

  const requestHeaders = new Headers(headers);
  let requestBody: BodyInit | undefined;

  if (authToken) {
    requestHeaders.set("Authorization", `Bearer ${authToken}`);
  }

  if (body instanceof FormData) {
    requestBody = body;
  } else if (body !== undefined) {
    requestHeaders.set("Content-Type", "application/json");
    requestBody = JSON.stringify(body);
  }

  let response: Response;

  try {
    response = await fetch(buildUrl(path, query), {
      method,
      headers: requestHeaders,
      body: requestBody,
    });
  } catch (error) {
    const message =
      error instanceof Error && /fetch|load failed|network/i.test(error.message)
        ? "Khong the ket noi backend. Hay kiem tra backend dang chay va CORS da duoc bat."
        : error instanceof Error
        ? error.message
        : "Khong the ket noi backend.";
    throw new ApiError(message, 0);
  }

  // Handle automatic token refresh on 401 Unauthorized
  if (response.status === 401 && authToken && !options._isRetry) {
    try {
      const rawSession = localStorage.getItem("qlt.session");
      if (rawSession) {
        const session = JSON.parse(rawSession);
        if (session.token && session.refreshToken) {
          const refreshRes = await fetch(`${API_BASE_URL}/Auth/refresh-token`, {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
            },
            body: JSON.stringify({
              token: session.token,
              refreshToken: session.refreshToken,
            }),
          });

          if (refreshRes.ok) {
            const authData = await refreshRes.json();
            // Update session
            session.token = authData.token;
            if (authData.refreshToken) {
              session.refreshToken = authData.refreshToken;
            }
            localStorage.setItem("qlt.session", JSON.stringify(session));

            // Dispatch event to update AppContext state
            window.dispatchEvent(new CustomEvent("qlt-session-refreshed", { detail: session }));

            // Retry original request with new token
            return apiRequest<T>(path, {
              ...options,
              authToken: authData.token,
              _isRetry: true,
            });
          } else {
            // Refresh failed, clean up session
            localStorage.removeItem("qlt.session");
            window.dispatchEvent(new CustomEvent("qlt-session-refreshed", { detail: null }));
            window.location.href = "/login";
          }
        }
      }
    } catch (refreshErr) {
      console.error("Failed to auto-refresh token", refreshErr);
    }
  }

  const contentType = response.headers.get("content-type") || "";
  const payload = contentType.includes("application/json") ? await response.json().catch(() => null) : await response.text();

  if (!response.ok) {
    const message =
      typeof payload === "string"
        ? payload
        : payload?.message || payload?.title || "Yeu cau API that bai.";
    throw new ApiError(message, response.status);
  }

  return payload as T;
}

export { API_BASE_URL };
