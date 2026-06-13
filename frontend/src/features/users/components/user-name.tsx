import { useUsers } from "../queries";

/**
 * Resolve um Id de usuário (auditoria) para o nome de exibição. Usa o diretório cacheado;
 * cai para "—" quando vazio e para um id curto se o usuário não for encontrado.
 */
export function UserName({ userId }: { userId: string | null | undefined }) {
  const { data } = useUsers();

  if (!userId) return <>—</>;

  const user = data?.find((u) => u.id === userId);
  if (user) return <>{user.displayName}</>;

  // Sem o diretório carregado ainda (ou usuário removido): mostra um id curto, não o GUID inteiro.
  return <>{userId.slice(0, 8)}</>;
}
