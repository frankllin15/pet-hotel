using PetHotel.Registry.Domain.Tutors;
using PetHotel.Registry.Domain.Tutors.Events;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.UnitTests;

public class TutorTests
{
    private static readonly TenantId Tenant = TenantId.New();

    [Fact]
    public void Registrar_tutor_valido_normaliza_contato_e_levanta_evento()
    {
        var result = Tutor.Register(Tenant, "Maria Silva", "Maria@Email.com", "+55 (11) 99999-8888");

        Assert.True(result.IsSuccess);
        Assert.Equal("maria@email.com", result.Value.Email.Value);
        Assert.Equal("+5511999998888", result.Value.Phone.Value);
        Assert.Contains(result.Value.DomainEvents, e => e is TutorRegistered);
    }

    [Theory]
    [InlineData("nao-eh-email")]
    [InlineData("")]
    public void Registrar_tutor_com_email_invalido_falha(string email)
    {
        var result = Tutor.Register(Tenant, "Nome", email, "11999998888");

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void Registrar_tutor_com_telefone_curto_falha()
    {
        var result = Tutor.Register(Tenant, "Nome", "a@b.com", "123");

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void Registrar_tutor_sem_tenant_falha()
    {
        var result = Tutor.Register(default, "Nome", "a@b.com", "11999998888");

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }
}
