import { useEffect } from "react";
import { useFieldArray, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useNavigate, useParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { Plus, Trash2 } from "lucide-react";
import { ApiError } from "@/shared/lib/problem-details";
import { Button } from "@/shared/ui/button";
import { Field } from "@/shared/ui/field";
import { FormPage } from "@/shared/ui/archetypes/form-page";
import { Input } from "@/shared/ui/input";
import { Spinner } from "@/shared/ui/spinner";
import { getTutor } from "../api";
import { registryKeys, useRegisterTutor, useUpdateTutor } from "../queries";
import { tutorFormSchema, type TutorFormInput } from "../schemas";

export function TutorFormPage() {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEdit = !!id;

  const registerMutation = useRegisterTutor();
  const updateMutation = useUpdateTutor(id ?? "");
  const mutation = isEdit ? updateMutation : registerMutation;

  const tutorQuery = useQuery({
    queryKey: registryKeys.tutor(id ?? ""),
    queryFn: () => getTutor(id!),
    enabled: isEdit,
  });

  const {
    register,
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<TutorFormInput>({
    resolver: zodResolver(tutorFormSchema),
    defaultValues: {
      emergencyContacts: [],
      authorizedPickups: [],
      billingDocument: "",
      billingEmail: "",
      billingAddressLine1: "",
      billingAddressLine2: "",
      billingCity: "",
      billingState: "",
      billingPostalCode: "",
    },
  });

  const contacts = useFieldArray({ control, name: "emergencyContacts" });
  const pickups = useFieldArray({ control, name: "authorizedPickups" });

  // Pré-preenche o formulário (inclusive as coleções) quando o tutor é carregado.
  useEffect(() => {
    const tutor = tutorQuery.data;
    if (!tutor) return;
    reset({
      fullName: tutor.fullName,
      email: tutor.email,
      phone: tutor.phone,
      emergencyContacts: tutor.emergencyContacts.map((c) => ({
        name: c.name,
        phone: c.phone,
        relationship: c.relationship ?? "",
      })),
      authorizedPickups: tutor.authorizedPickups.map((p) => ({ name: p.name, document: p.document ?? "" })),
      billingDocument: tutor.billing?.document ?? "",
      billingEmail: tutor.billing?.billingEmail ?? "",
      billingAddressLine1: tutor.billing?.addressLine1 ?? "",
      billingAddressLine2: tutor.billing?.addressLine2 ?? "",
      billingCity: tutor.billing?.city ?? "",
      billingState: tutor.billing?.state ?? "",
      billingPostalCode: tutor.billing?.postalCode ?? "",
    });
  }, [tutorQuery.data, reset]);

  const submit = handleSubmit((values) => {
    const common = {
      fullName: values.fullName,
      email: values.email,
      phone: values.phone,
      emergencyContacts: values.emergencyContacts.map((c) => ({
        name: c.name,
        phone: c.phone,
        relationship: c.relationship ? c.relationship : null,
      })),
      authorizedPickups: values.authorizedPickups.map((p) => ({
        name: p.name,
        document: p.document ? p.document : null,
      })),
      billing: values.billingDocument
        ? {
            document: values.billingDocument,
            billingEmail: values.billingEmail ? values.billingEmail : null,
            addressLine1: values.billingAddressLine1 ? values.billingAddressLine1 : null,
            addressLine2: values.billingAddressLine2 ? values.billingAddressLine2 : null,
            city: values.billingCity ? values.billingCity : null,
            state: values.billingState ? values.billingState : null,
            postalCode: values.billingPostalCode ? values.billingPostalCode : null,
          }
        : null,
    };

    if (isEdit) {
      updateMutation.mutate(
        { id: id!, ...common },
        { onSuccess: () => navigate(`/registry/tutors/${id}`, { replace: true }) },
      );
    } else {
      registerMutation.mutate(common, {
        onSuccess: ({ id: newId }) => navigate(`/registry/tutors/${newId}`, { replace: true }),
      });
    }
  });

  const formError = mutation.error instanceof ApiError ? mutation.error.message : null;

  if (isEdit && tutorQuery.isPending) {
    return (
      <div className="flex justify-center py-16">
        <Spinner />
      </div>
    );
  }

  return (
    <FormPage
      title={isEdit ? "Editar tutor" : "Novo tutor"}
      description={isEdit ? "Atualize os dados do tutor." : "Cadastre o dono dos pets."}
      onSubmit={submit}
      footer={
        <>
          <Button type="button" variant="ghost" onClick={() => navigate(-1)}>
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

      <div className="space-y-3 rounded-lg border p-4">
        <div>
          <h3 className="text-sm font-semibold">Faturamento</h3>
          <p className="text-xs text-muted-foreground">
            Dados de cobrança/nota. Deixe o CPF/CNPJ em branco se não houver.
          </p>
        </div>
        <div className="grid gap-3 sm:grid-cols-2">
          <Field label="CPF/CNPJ" htmlFor="billingDocument" error={errors.billingDocument?.message}>
            <Input id="billingDocument" aria-invalid={!!errors.billingDocument} {...register("billingDocument")} />
          </Field>
          <Field label="E-mail de cobrança" htmlFor="billingEmail" error={errors.billingEmail?.message}>
            <Input
              id="billingEmail"
              type="email"
              placeholder="se diferente do principal"
              {...register("billingEmail")}
            />
          </Field>
        </div>
        <div className="grid gap-3 sm:grid-cols-2">
          <Field label="Endereço" htmlFor="billingAddressLine1" error={errors.billingAddressLine1?.message}>
            <Input id="billingAddressLine1" placeholder="rua, número" {...register("billingAddressLine1")} />
          </Field>
          <Field label="Complemento" htmlFor="billingAddressLine2" error={errors.billingAddressLine2?.message}>
            <Input id="billingAddressLine2" {...register("billingAddressLine2")} />
          </Field>
        </div>
        <div className="grid gap-3 sm:grid-cols-3">
          <Field label="Cidade" htmlFor="billingCity" error={errors.billingCity?.message}>
            <Input id="billingCity" {...register("billingCity")} />
          </Field>
          <Field label="UF" htmlFor="billingState" error={errors.billingState?.message}>
            <Input id="billingState" {...register("billingState")} />
          </Field>
          <Field label="CEP" htmlFor="billingPostalCode" error={errors.billingPostalCode?.message}>
            <Input id="billingPostalCode" {...register("billingPostalCode")} />
          </Field>
        </div>
      </div>

      <FieldArraySection
        title="Contatos de emergência"
        description="Acionados se o tutor não for localizado."
        addLabel="Adicionar contato"
        onAdd={() => contacts.append({ name: "", phone: "", relationship: "" })}
        fields={contacts.fields}
        onRemove={contacts.remove}
      >
        {(index) => (
          <>
            <Field label="Nome" htmlFor={`ec-name-${index}`} error={errors.emergencyContacts?.[index]?.name?.message}>
              <Input id={`ec-name-${index}`} {...register(`emergencyContacts.${index}.name`)} />
            </Field>
            <Field label="Telefone" htmlFor={`ec-phone-${index}`} error={errors.emergencyContacts?.[index]?.phone?.message}>
              <Input id={`ec-phone-${index}`} {...register(`emergencyContacts.${index}.phone`)} />
            </Field>
            <Field
              label="Vínculo"
              htmlFor={`ec-rel-${index}`}
              error={errors.emergencyContacts?.[index]?.relationship?.message}
            >
              <Input id={`ec-rel-${index}`} placeholder="ex.: cônjuge" {...register(`emergencyContacts.${index}.relationship`)} />
            </Field>
          </>
        )}
      </FieldArraySection>

      <FieldArraySection
        title="Autorizados a retirar"
        description="Pessoas que podem retirar o pet em nome do tutor."
        addLabel="Adicionar autorizado"
        onAdd={() => pickups.append({ name: "", document: "" })}
        fields={pickups.fields}
        onRemove={pickups.remove}
      >
        {(index) => (
          <>
            <Field label="Nome" htmlFor={`ap-name-${index}`} error={errors.authorizedPickups?.[index]?.name?.message}>
              <Input id={`ap-name-${index}`} {...register(`authorizedPickups.${index}.name`)} />
            </Field>
            <Field
              label="Documento"
              htmlFor={`ap-doc-${index}`}
              error={errors.authorizedPickups?.[index]?.document?.message}
            >
              <Input id={`ap-doc-${index}`} placeholder="CPF/RG" {...register(`authorizedPickups.${index}.document`)} />
            </Field>
          </>
        )}
      </FieldArraySection>

      {formError && <p className="text-sm text-destructive">{formError}</p>}
    </FormPage>
  );
}

/** Bloco de lista dinâmica (adicionar/remover itens) reutilizado nas duas coleções do tutor. */
function FieldArraySection({
  title,
  description,
  addLabel,
  onAdd,
  fields,
  onRemove,
  children,
}: {
  title: string;
  description: string;
  addLabel: string;
  onAdd: () => void;
  fields: { id: string }[];
  onRemove: (index: number) => void;
  children: (index: number) => React.ReactNode;
}) {
  return (
    <div className="space-y-3 rounded-lg border p-4">
      <div className="flex items-start justify-between gap-2">
        <div>
          <h3 className="text-sm font-semibold">{title}</h3>
          <p className="text-xs text-muted-foreground">{description}</p>
        </div>
        <Button type="button" variant="outline" size="sm" onClick={onAdd}>
          <Plus /> {addLabel}
        </Button>
      </div>
      {fields.length === 0 ? (
        <p className="text-sm text-muted-foreground">Nenhum item.</p>
      ) : (
        fields.map((field, index) => (
          <div key={field.id} className="flex items-end gap-2 border-t pt-3 first:border-t-0 first:pt-0">
            <div className="grid flex-1 gap-3 sm:grid-cols-3">{children(index)}</div>
            <Button type="button" variant="ghost" size="icon" aria-label="Remover" onClick={() => onRemove(index)}>
              <Trash2 />
            </Button>
          </div>
        ))
      )}
    </div>
  );
}
