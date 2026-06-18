import { Component, type ErrorInfo, type ReactNode } from "react";
import { AlertTriangle } from "lucide-react";
import { Button } from "./button";
import { captureBoundaryError } from "@/shared/observability/sentry";

interface Props {
  children: ReactNode;
  /** Nome da feature, para log/telemetria. */
  feature?: string;
  fallback?: ReactNode;
}

interface State {
  error: Error | null;
}

/**
 * Error boundary por feature (docs/08 §Confiabilidade): uma tela quebrada não
 * derruba o app inteiro. Em produção, plugar Sentry no componentDidCatch.
 */
export class FeatureErrorBoundary extends Component<Props, State> {
  state: State = { error: null };

  static getDerivedStateFromError(error: Error): State {
    return { error };
  }

  componentDidCatch(error: Error, info: ErrorInfo): void {
    const feature = this.props.feature ?? "app";
    captureBoundaryError(error, feature, info.componentStack);
    console.error(`[${feature}]`, error, info.componentStack);
  }

  private reset = () => this.setState({ error: null });

  render() {
    if (this.state.error) {
      if (this.props.fallback) return this.props.fallback;
      return (
        <div className="flex min-h-60 flex-col items-center justify-center gap-3 text-center">
          <AlertTriangle className="size-7 text-destructive" />
          <div>
            <p className="font-medium">Algo deu errado nesta tela.</p>
            <p className="text-sm text-muted-foreground">O restante do app continua funcionando.</p>
          </div>
          <Button variant="outline" size="sm" onClick={this.reset}>
            Recarregar a tela
          </Button>
        </div>
      );
    }
    return this.props.children;
  }
}
