import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { useLocation, useNavigate } from "react-router-dom";
import { PawPrint } from "lucide-react";
import { useAuth } from "@/shared/auth/auth-context";
import { ApiError } from "@/shared/lib/problem-details";
import { Button } from "@/shared/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/shared/ui/card";
import { Input } from "@/shared/ui/input";
import { Label } from "@/shared/ui/label";
import { login } from "../api";
import { loginSchema, type LoginInput } from "../schemas";

export function LoginPage() {
  const { signIn } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const from = (location.state as { from?: Location } | null)?.from?.pathname ?? "/";

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginInput>({ resolver: zodResolver(loginSchema) });

  const mutation = useMutation({
    mutationFn: login,
    onSuccess: ({ token }) => {
      signIn(token);
      navigate(from, { replace: true });
    },
  });

  const errorMessage =
    mutation.error instanceof ApiError && mutation.error.kind === "unauthorized"
      ? "E-mail ou senha inválidos."
      : mutation.error
        ? "Não foi possível entrar. Tente novamente."
        : null;

  return (
    <div className="flex min-h-screen items-center justify-center bg-muted/30 p-4">
      <Card className="w-full max-w-sm">
        <CardHeader className="items-center text-center">
          <PawPrint className="size-8 text-primary" />
          <CardTitle className="text-xl">Entrar no PetHotel</CardTitle>
          <CardDescription>Acesso restrito à equipe.</CardDescription>
        </CardHeader>
        <CardContent>
          <form className="space-y-4" onSubmit={handleSubmit((v) => mutation.mutate(v))} noValidate>
            <div className="space-y-1.5">
              <Label htmlFor="email">E-mail</Label>
              <Input
                id="email"
                type="email"
                autoComplete="username"
                aria-invalid={!!errors.email}
                {...register("email")}
              />
              {errors.email && <p className="text-xs text-destructive">{errors.email.message}</p>}
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="password">Senha</Label>
              <Input
                id="password"
                type="password"
                autoComplete="current-password"
                aria-invalid={!!errors.password}
                {...register("password")}
              />
              {errors.password && (
                <p className="text-xs text-destructive">{errors.password.message}</p>
              )}
            </div>
            {errorMessage && <p className="text-sm text-destructive">{errorMessage}</p>}
            <Button type="submit" className="w-full" disabled={mutation.isPending}>
              {mutation.isPending ? "Entrando…" : "Entrar"}
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
