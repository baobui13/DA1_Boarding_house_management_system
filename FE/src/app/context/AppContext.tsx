import React, { createContext, useContext, useEffect, useState } from "react";
import {
  buildSession,
  buildSessionFromAuth,
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

type AppContextStore = {
  appContext?: React.Context<AppContextType | null>;
};

const appContextStore = globalThis as typeof globalThis & AppContextStore;
const AppContext = appContextStore.appContext ?? createContext<AppContextType | null>(null);
appContextStore.appContext = AppContext;

export function AppProvider({ children }: { children: React.ReactNode }) {
  const [session, setSession] = useState<StoredSession | null>(null);
  const [authReady, setAuthReady] = useState(false);

  useEffect(() => {
    setSession(readStoredSession());
    setAuthReady(true);
  }, []);

  useEffect(() => {
    const handleRefreshed = (e: Event) => {
      const customEvent = e as CustomEvent<StoredSession | null>;
      setSession(customEvent.detail);
    };

    window.addEventListener("qlt-session-refreshed", handleRefreshed);
    return () => {
      window.removeEventListener("qlt-session-refreshed", handleRefreshed);
    };
  }, []);

  useEffect(() => {
    if (!session?.token || !session.user.email) {
      return;
    }

    let cancelled = false;

    (async () => {
      try {
        const userResponse = await getUserByEmail(session.user.email, session.token);
        if (cancelled) return;

        const refreshedSession = buildSession(
          {
            token: session.token,
            refreshToken: session.refreshToken,
            email: session.user.email,
            fullName: session.user.name,
            role: session.user.role,
            avatarUrl: session.user.avatar,
          },
          userResponse,
        );

        persistSession(refreshedSession);
      } catch {
        // Keep the stored session when the refresh probe fails.
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [session?.refreshToken, session?.token, session?.user.avatar, session?.user.email, session?.user.name, session?.user.role]);

  const persistSession = (nextSession: StoredSession | null) => {
    setSession(nextSession);
    writeStoredSession(nextSession);
  };

  const login = async (email: string, password: string) => {
    const authResponse = await loginRequest(email, password);
    let nextSession = buildSessionFromAuth(authResponse);

    try {
      const userResponse = await getUserByEmail(authResponse.email, authResponse.token);
      nextSession = buildSession(authResponse, userResponse);
    } catch {
      // Backend DTO currently requires Id even when endpoint name suggests email lookup.
      // Fall back to claims embedded in the login token so sign-in can still complete.
    }

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
