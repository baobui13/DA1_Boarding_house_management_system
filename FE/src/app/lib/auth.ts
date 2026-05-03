import { apiRequest } from "./api";
import type { AppUser, AuthResponse, Role, StoredSession, UserResponse } from "./types";

const SESSION_KEY = "qlt.session";

export function normalizeRole(role: string): Role {
  const normalized = role.trim().toLowerCase();
  if (normalized === "admin") return "admin";
  if (normalized === "landlord") return "landlord";
  return "tenant";
}

export function readStoredSession(): StoredSession | null {
  const raw = localStorage.getItem(SESSION_KEY);
  if (!raw) return null;

  try {
    return JSON.parse(raw) as StoredSession;
  } catch {
    localStorage.removeItem(SESSION_KEY);
    return null;
  }
}

export function writeStoredSession(session: StoredSession | null) {
  if (!session) {
    localStorage.removeItem(SESSION_KEY);
    return;
  }

  localStorage.setItem(SESSION_KEY, JSON.stringify(session));
}

export async function loginRequest(email: string, password: string) {
  return apiRequest<AuthResponse>("Auth/login", {
    method: "POST",
    body: { email, password },
  });
}

export async function registerRequest(input: {
  email: string;
  password: string;
  fullName: string;
  phoneNumber?: string;
  address?: string;
  role: string;
}) {
  return apiRequest<AuthResponse>("Auth/register", {
    method: "POST",
    body: input,
  });
}

export async function logoutRequest(token: string) {
  return apiRequest<{ message: string }>("Auth/logout", {
    method: "POST",
    authToken: token,
  });
}

export async function getUserByEmail(email: string) {
  return apiRequest<UserResponse>("User/GetUserByIdOrEmail", {
    query: { email },
  });
}

export function buildSession(auth: AuthResponse, user: UserResponse): StoredSession {
  const appUser: AppUser = {
    id: user.id,
    name: user.fullName,
    email: user.email,
    phone: user.phoneNumber || "",
    role: normalizeRole(user.role || auth.role),
    avatar: user.avatarUrl || auth.avatarUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(user.fullName)}&background=f97316&color=fff`,
    address: user.address || undefined,
  };

  return {
    token: auth.token || "",
    refreshToken: auth.refreshToken,
    user: appUser,
  };
}
