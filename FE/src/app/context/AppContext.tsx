import React, { createContext, useContext, useEffect, useState } from "react";
import {
  buildSession,
  getUserByEmail,
  loginRequest,
  logoutRequest,
  readStoredSession,
  registerRequest,
  writeStoredSession,
} from "../lib/auth";
import type { AppUser, Role, StoredSession } from "../lib/types";

interface AppContextType {
  currentUser: AppUser | null;
  isAuthenticated: boolean;
  token: string | null;
  authReady: boolean;
  login: (email: string, password: string) => Promise<AppUser>;
  register: (input: {
    email: string;
    password: string;
    fullName: string;
    phoneNumber?: string;
    address?: string;
    role: Role;
  }) => Promise<AppUser>;
  logout: () => Promise<void>;
  setRole: (role: Role) => void;
}

const AppContext = createContext<AppContextType | null>(null);

export function AppProvider({ children }: { children: React.ReactNode }) {
  const [session, setSession] = useState<StoredSession | null>(null);
  const [authReady, setAuthReady] = useState(false);

  useEffect(() => {
    setSession(readStoredSession());
    setAuthReady(true);
  }, []);

  const persistSession = (nextSession: StoredSession | null) => {
    setSession(nextSession);
    writeStoredSession(nextSession);
  };

  const login = async (email: string, password: string) => {
    const authResponse = await loginRequest(email, password);
    const userResponse = await getUserByEmail(authResponse.email);
    const nextSession = buildSession(authResponse, userResponse);
    persistSession(nextSession);
    return nextSession.user;
  };

  const register = async (input: {
    email: string;
    password: string;
    fullName: string;
    phoneNumber?: string;
    address?: string;
    role: Role;
  }) => {
    await registerRequest({
      ...input,
      role:
        input.role === "admin"
          ? "Admin"
          : input.role === "landlord"
          ? "Landlord"
          : "Tenant",
    });

    return login(input.email, input.password);
  };

  const logout = async () => {
    const token = session?.token;
    persistSession(null);

    if (!token) return;

    try {
      await logoutRequest(token);
    } catch {
      // Do not block local logout when backend token is already invalid.
    }
  };

  const setRole = (role: Role) => {
    if (!session) return;

    const nextSession: StoredSession = {
      ...session,
      user: {
        ...session.user,
        role,
      },
    };

    persistSession(nextSession);
  };

  return (
    <AppContext.Provider
      value={{
        currentUser: session?.user ?? null,
        isAuthenticated: Boolean(session?.token),
        token: session?.token ?? null,
        authReady,
        login,
        register,
        logout,
        setRole,
      }}
    >
      {children}
    </AppContext.Provider>
  );
}

export function useApp() {
  const ctx = useContext(AppContext);
  if (!ctx) throw new Error("useApp must be used within AppProvider");
  return ctx;
}
