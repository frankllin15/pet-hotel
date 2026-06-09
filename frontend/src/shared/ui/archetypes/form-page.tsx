import { type FormHTMLAttributes, type ReactNode } from "react";
import { PageHeader } from "../page-header";

/**
 * Arquétipo Form criar/editar (docs/08 §3): mesma estrutura de campos,
 * validação e botões. Telas: cadastro tutor/pet, nova reserva.
 */
export function FormPage({
  title,
  description,
  footer,
  children,
  ...formProps
}: {
  title: ReactNode;
  description?: ReactNode;
  /** Botões de ação (Salvar / Cancelar). */
  footer?: ReactNode;
  children: ReactNode;
} & FormHTMLAttributes<HTMLFormElement>) {
  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <PageHeader title={title} description={description} />
      <form className="space-y-6" {...formProps}>
        <div className="space-y-4">{children}</div>
        {footer && <div className="flex items-center justify-end gap-2 border-t pt-4">{footer}</div>}
      </form>
    </div>
  );
}
