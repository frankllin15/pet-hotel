using PetHotel.Registry.Domain.Packs.Events;
using PetHotel.Registry.Domain.Pets;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.Domain.Packs;

/// <summary>
/// Matilha: grupo de pets compatíveis para conviver/ficar juntos. Agregado tenant-scoped;
/// referencia os pets por Id (fronteira de agregado, docs/03). A compatibilidade é um alerta
/// calculado no lado de leitura a partir da avaliação comportamental (<see cref="PackCompatibility"/>).
/// </summary>
public sealed class Pack : AggregateRoot<PackId>, IHasTenant, IAuditable
{
    public TenantId TenantId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Notes { get; private set; }
    /// <summary>Pets que compõem a matilha (referência por Id, sem duplicatas).</summary>
    public List<PackMember> Members { get; private set; } = [];

    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    private Pack() { } // EF

    private Pack(PackId id, TenantId tenantId, string name, string? notes) : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Notes = notes;
    }

    public static Result<Pack> Create(TenantId tenantId, string? name, string? notes, IEnumerable<PetId>? members = null)
    {
        if (tenantId.Value == Guid.Empty)
        {
            return Error.Validation("pack.tenant_required", "Tenant é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation("pack.name_required", "Nome da matilha é obrigatório.");
        }

        return new Pack(PackId.New(), tenantId, name.Trim(), Normalize(notes))
        {
            Members = ToMembers(members),
        };
    }

    /// <summary>Edita nome, observações e a composição da matilha (substituição integral).</summary>
    public Result Update(string? name, string? notes, IEnumerable<PetId>? members)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation("pack.name_required", "Nome da matilha é obrigatório.");
        }

        Name = name.Trim();
        Notes = Normalize(notes);
        Members = ToMembers(members);
        return Result.Success();
    }

    /// <summary>Marca a matilha como excluída (levanta o evento; a remoção física é do repositório).</summary>
    public void Delete() => Raise(new PackDeleted(Id, TenantId));

    private static string? Normalize(string? notes) => string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

    private static List<PackMember> ToMembers(IEnumerable<PetId>? members) =>
        members?.Select(m => m.Value).Distinct().Select(id => new PackMember(id)).ToList() ?? [];
}
