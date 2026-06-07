namespace PetHotel.SharedKernel;

/// <summary>
/// Entidade pertencente a um tenant. Recebe carimbo automático na escrita
/// e global query filter na leitura (docs/04). Contrato puro (sem EF) para
/// que o domínio possa implementá-lo sem violar a regra de dependência.
/// </summary>
public interface IHasTenant
{
    TenantId TenantId { get; }
}
