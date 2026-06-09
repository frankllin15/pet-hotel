/**
 * Camada única que traduz ProblemDetails/Result do backend (docs/08 §Consistência
 * com o backend). A validação com zod é UX; o backend é a fonte da verdade.
 */

/** RFC 7807 ProblemDetails, com a extensão `errors` do ASP.NET para validação. */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  /** Erros de validação por campo (ValidationProblemDetails do ASP.NET). */
  errors?: Record<string, string[]>;
  /** Código de erro do domínio (Result.Error.Code), quando exposto. */
  code?: string;
  [key: string]: unknown;
}

export type ApiErrorKind = "validation" | "conflict" | "unauthorized" | "notfound" | "unexpected";

export class ApiError extends Error {
  readonly kind: ApiErrorKind;
  readonly status: number;
  readonly problem: ProblemDetails;
  /** Erros por campo de formulário, quando kind === "validation". */
  readonly fieldErrors: Record<string, string[]>;

  constructor(status: number, problem: ProblemDetails) {
    super(problem.detail || problem.title || `Erro ${status}`);
    this.name = "ApiError";
    this.status = status;
    this.problem = problem;
    this.fieldErrors = problem.errors ?? {};
    this.kind = classify(status);
  }
}

function classify(status: number): ApiErrorKind {
  if (status === 400 || status === 422) return "validation";
  if (status === 401 || status === 403) return "unauthorized";
  if (status === 404) return "notfound";
  if (status === 409) return "conflict";
  return "unexpected";
}

/** Constrói um ApiError a partir do corpo da resposta (já parseado, se houver). */
export function toApiError(status: number, body: unknown): ApiError {
  const problem =
    body && typeof body === "object" ? (body as ProblemDetails) : { title: String(body ?? "") };
  return new ApiError(status, problem);
}
