/**
 * Papéis (RBAC). O token traz claims de tenant e papel; rotas e ações se
 * adaptam ao papel — tratador não vê financeiro (docs/08 §Auth).
 * Os valores devem casar com os papéis emitidos pelo backend.
 */
export const ROLES = {
  TenantAdmin: "TenantAdmin",
  Manager: "Manager",
  Receptionist: "Receptionist",
  Caretaker: "Caretaker",
  Groomer: "Groomer",
} as const;

export type Role = (typeof ROLES)[keyof typeof ROLES];
