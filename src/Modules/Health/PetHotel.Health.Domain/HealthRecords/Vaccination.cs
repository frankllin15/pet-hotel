using PetHotel.SharedKernel;

namespace PetHotel.Health.Domain.HealthRecords;

/// <summary>
/// Vacinação registrada na ficha. Entidade interna do agregado <see cref="HealthRecord"/>:
/// só é acessada (e carregada) pela raiz, que já é filtrada por tenant — por isso não
/// carrega TenantId próprio.
/// </summary>
public sealed class Vaccination : Entity<VaccinationId>
{
    public VaccineType Type { get; private set; }
    public DateOnly AppliedOn { get; private set; }
    public DateOnly ExpiresOn { get; private set; }

    /// <summary>Chave da foto da carteira no storage (tenant-scoped). Null = sem foto.</summary>
    public string? PhotoKey { get; private set; }

    private Vaccination() { } // EF

    internal Vaccination(VaccinationId id, VaccineType type, DateOnly appliedOn, DateOnly expiresOn)
        : base(id)
    {
        Type = type;
        AppliedOn = appliedOn;
        ExpiresOn = expiresOn;
    }

    /// <summary>Vacina válida (vigente) na data informada.</summary>
    public bool IsValidOn(DateOnly date) => AppliedOn <= date && date <= ExpiresOn;

    /// <summary>Define/remove a foto da carteira; devolve a chave anterior (para apagar o órfão).</summary>
    internal string? SetPhoto(string? key)
    {
        var previous = PhotoKey;
        PhotoKey = string.IsNullOrWhiteSpace(key) ? null : key;
        return previous;
    }
}
