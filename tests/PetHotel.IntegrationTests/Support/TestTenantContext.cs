using PetHotel.SharedKernel;

namespace PetHotel.IntegrationTests.Support;

/// <summary>Tenant context mutável para alternar o tenant entre asserts nos testes.</summary>
public sealed class TestTenantContext : ITenantContext
{
    public TenantId Current { get; set; }
    public bool HasTenant => Current.Value != Guid.Empty;
}

/// <summary>Usuário fixo para auditoria nos testes.</summary>
public sealed class TestCurrentUser : ICurrentUser
{
    public string? UserId => "integration-test";
}
