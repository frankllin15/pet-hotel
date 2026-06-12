using PetHotel.SharedKernel;

namespace PetHotel.LoadTest.Seeder;

/// <summary>
/// <see cref="ITenantContext"/> mutável para o seeder: o tenant corrente é trocado a
/// cada tenant semeado, antes de abrir os DbContexts. Alimenta o global query filter e o
/// interceptor de carimbo de tenant (mesmo caminho do runtime).
/// </summary>
public sealed class SeedTenantContext : ITenantContext
{
    private TenantId _current;

    public TenantId Current => _current;
    public bool HasTenant => _current.Value != Guid.Empty;

    public void Use(TenantId tenant) => _current = tenant;
}

/// <summary>Usuário fixo para a auditoria das linhas semeadas.</summary>
public sealed class SeedCurrentUser : ICurrentUser
{
    public string? UserId => "loadtest-seeder";
}
