using PetHotel.SharedKernel;
using PetHotel.Tenancy.Domain.Tenants;
using PetHotel.Tenancy.Domain.Tenants.Events;

namespace PetHotel.Tenancy.UnitTests;

public class TenantTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 6, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Registrar_tenant_valido_levanta_evento_e_fica_ativo()
    {
        var result = Tenant.Register("Hotel do Bicho", "hotel-do-bicho", Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(TenantStatus.Active, result.Value.Status);
        Assert.Equal("hotel-do-bicho", result.Value.Slug.Value);
        Assert.Contains(result.Value.DomainEvents, e => e is TenantRegistered);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Registrar_tenant_sem_nome_falha_com_validacao(string? name)
    {
        var result = Tenant.Register(name, "slug-valido", Now);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Theory]
    [InlineData("Slug Invalido")]
    [InlineData("-comeca-com-hifen")]
    [InlineData("ACENTUAÇÃO")]
    public void Registrar_tenant_com_slug_invalido_falha(string slug)
    {
        var result = Tenant.Register("Nome", slug, Now);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void Suspender_tenant_ja_suspenso_retorna_conflito()
    {
        var tenant = Tenant.Register("Nome", "slug", Now).Value;
        Assert.True(tenant.Suspend().IsSuccess);

        var again = tenant.Suspend();

        Assert.True(again.IsFailure);
        Assert.Equal(ErrorType.Conflict, again.Error.Type);
    }
}
