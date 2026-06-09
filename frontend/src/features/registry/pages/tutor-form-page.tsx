import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useNavigate } from "react-router-dom";
import { ApiError } from "@/shared/lib/problem-details";
import { Button } from "@/shared/ui/button";
import { Field } from "@/shared/ui/field";
import { FormPage } from "@/shared/ui/archetypes/form-page";
import { Input } from "@/shared/ui/input";
import { useRegisterTutor } from "../queries";
import { tutorFormSchema, type TutorFormInput } from "../schemas";

export function TutorFormPage() {
  const navigate = useNavigate();
  const mutation = useRegisterTutor();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<TutorFormInput>({ resolver: zodResolver(tutorFormSchema) });

  const submit = handleSubmit((values) =>
    mutation.mutate(values, {
      onSuccess: ({ id }) => navigate(`/registry/tutors/${id}`, { replace: true }),
    }),
  );

  const formError = mutation.error instanceof ApiError ? mutation.error.message : null;

  return (
    <FormPage
      title="Novo tutor"
      description="Cadastre o dono dos pets."
      onSubmit={submit}
      footer={
        <>
          <Button type="button" variant="ghost" onClick={() => navigate("/registry/tutors")}>
            Cancelar
          </Button>
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? "Salvando…" : "Salvar"}
          </Button>
        </>
      }
    >
      <Field label="Nome completo" htmlFor="fullName" error={errors.fullName?.message}>
        <Input id="fullName" aria-invalid={!!errors.fullName} {...register("fullName")} />
      </Field>
      <Field label="E-mail" htmlFor="email" error={errors.email?.message}>
        <Input id="email" type="email" aria-invalid={!!errors.email} {...register("email")} />
      </Field>
      <Field label="Telefone" htmlFor="phone" error={errors.phone?.message}>
        <Input id="phone" aria-invalid={!!errors.phone} {...register("phone")} />
      </Field>
      {formError && <p className="text-sm text-destructive">{formError}</p>}
    </FormPage>
  );
}
