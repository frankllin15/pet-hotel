using PetHotel.SharedKernel;
using PetHotel.Tenancy.Domain.Users;
using PetHotel.Tenancy.Domain.Users.Events;

namespace PetHotel.Tenancy.UnitTests;

public class UserTests
{
    private static readonly TenantId Tenant = TenantId.New();

    [Fact]
    public void Registrar_usuario_valido_normaliza_email_e_levanta_evento()
    {
        var result = User.Register(Tenant, "Dono@Hotel.COM", "Dono", UserRole.Owner);

        Assert.True(result.IsSuccess);
        Assert.Equal("dono@hotel.com", result.Value.Email.Value);
        Assert.Equal(UserStatus.Active, result.Value.Status);
        Assert.Contains(result.Value.DomainEvents, e => e is UserRegistered);
    }

    [Fact]
    public void Registrar_usuario_com_email_invalido_falha()
    {
        var result = User.Register(Tenant, "sem-arroba", "Nome", UserRole.Staff);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void Registrar_usuario_sem_tenant_falha()
    {
        var result = User.Register(default, "a@b.com", "Nome", UserRole.Staff);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void Trocar_para_mesmo_papel_retorna_conflito()
    {
        var user = User.Register(Tenant, "a@b.com", "Nome", UserRole.Staff).Value;

        var result = user.ChangeRole(UserRole.Staff);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public void Desativar_e_reativar_usuario()
    {
        var user = User.Register(Tenant, "a@b.com", "Nome", UserRole.Manager).Value;

        Assert.True(user.Deactivate().IsSuccess);
        Assert.Equal(UserStatus.Inactive, user.Status);
        Assert.True(user.Deactivate().IsFailure);
        Assert.True(user.Activate().IsSuccess);
        Assert.Equal(UserStatus.Active, user.Status);
    }
}
