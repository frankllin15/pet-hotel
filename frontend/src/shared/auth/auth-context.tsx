import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import type { Role } from "./roles";
import {
  getStoredToken,
  setStoredToken,
  parseClaims,
  isExpired,
  type AuthClaims,
} from "./token";
import { setObservabilityUser } from "@/shared/observability/sentry";

interface AuthState {
  token: string | null;
  claims: AuthClaims | null;
  isAuthenticated: boolean;
  /** Define a sessão a partir de um access token (ex.: após login). */
  signIn: (token: string) => void;
  signOut: () => void;
  hasRole: (...roles: Role[]) => boolean;
}

const AuthContext = createContext<AuthState | null>(null);

/** Evento de janela usado pelo client de API para forçar logout em 401. */
export const AUTH_SIGNOUT_EVENT = "pethotel:signout";

function load(): { token: string | null; claims: AuthClaims | null } {
  const token = getStoredToken();
  if (!token) return { token: null, claims: null };
  const claims = parseClaims(token);
  if (!claims || isExpired(claims)) {
    setStoredToken(null);
    return { token: null, claims: null };
  }
  return { token, claims };
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [{ token, claims }, setSession] = useState(load);

  const signIn = useCallback((next: string) => {
    setStoredToken(next);
    setSession({ token: next, claims: parseClaims(next) });
  }, []);

  const signOut = useCallback(() => {
    setStoredToken(null);
    setSession({ token: null, claims: null });
  }, []);

  // O client de API dispara este evento ao receber 401 (token inválido/expirado).
  useEffect(() => {
    window.addEventListener(AUTH_SIGNOUT_EVENT, signOut);
    return () => window.removeEventListener(AUTH_SIGNOUT_EVENT, signOut);
  }, [signOut]);

  // Associa (ou limpa) o usuário nos eventos de observabilidade — só id/tenant,
  // sem PII (e-mail/nome). No-op se o Sentry não estiver inicializado.
  useEffect(() => {
    setObservabilityUser(claims ? { id: claims.sub, tenantId: claims.tenantId } : null);
  }, [claims]);

  const hasRole = useCallback(
    (...roles: Role[]) => !!claims && roles.some((r) => claims.roles.includes(r)),
    [claims],
  );

  const value = useMemo<AuthState>(
    () => ({
      token,
      claims,
      isAuthenticated: !!token && !!claims,
      signIn,
      signOut,
      hasRole,
    }),
    [token, claims, signIn, signOut, hasRole],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthState {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth deve ser usado dentro de <AuthProvider>.");
  return ctx;
}
