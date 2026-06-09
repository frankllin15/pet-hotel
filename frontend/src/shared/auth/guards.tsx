import { Navigate, Outlet, useLocation } from "react-router-dom";
import type { Role } from "./roles";
import { useAuth } from "./auth-context";

/** Exige sessão autenticada; senão redireciona ao login preservando o destino. */
export function RequireAuth() {
  const { isAuthenticated } = useAuth();
  const location = useLocation();
  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }
  return <Outlet />;
}

/** Exige um dos papéis informados; senão mostra acesso negado (RBAC, docs/08). */
export function RequireRole({ roles }: { roles: Role[] }) {
  const { hasRole } = useAuth();
  if (!hasRole(...roles)) {
    return <Forbidden />;
  }
  return <Outlet />;
}

function Forbidden() {
  return (
    <div className="flex min-h-[60vh] flex-col items-center justify-center gap-2 text-center">
      <h1 className="text-2xl font-semibold">Acesso negado</h1>
      <p className="text-muted-foreground">Você não tem permissão para acessar esta área.</p>
    </div>
  );
}
