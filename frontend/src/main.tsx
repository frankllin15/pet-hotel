import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import { AppProviders } from "@/app/providers";
import { AppRouter } from "@/app/router";
import { initObservability } from "@/shared/observability/sentry";

// Inicializa antes do render para capturar erros de carregamento (no-op sem DSN).
initObservability();

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <AppProviders>
      <AppRouter />
    </AppProviders>
  </StrictMode>,
);
