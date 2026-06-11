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

    [Fact]
    public void Registrar_tutor_guarda_contatos_de_emergencia_e_autorizados()
    {
        var contact = EmergencyContact.Create("João", "11988887777", "Cônjuge").Value;
        var pickup = AuthorizedPickup.Create("Ana", "123.456.789-00").Value;

        var result = Tutor.Register(Tenant, "Maria", "m@b.com", "11999998888", [contact], [pickup]);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.EmergencyContacts);
        Assert.Equal("João", result.Value.EmergencyContacts[0].Name);
        Assert.Equal("Cônjuge", result.Value.EmergencyContacts[0].Relationship);
        Assert.Single(result.Value.AuthorizedPickups);
        Assert.Equal("Ana", result.Value.AuthorizedPickups[0].Name);
    }

    [Fact]
    public void Registrar_tutor_sem_coleções_inicia_listas_vazias()
    {
        var result = Tutor.Register(Tenant, "Maria", "m@b.com", "11999998888");

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.EmergencyContacts);
        Assert.Empty(result.Value.AuthorizedPickups);
    }

    [Fact]
    public void Contato_de_emergencia_sem_telefone_falha()
    {
        var result = EmergencyContact.Create("João", "  ", null);

        Assert.True(result.IsFailure);
        Assert.Equal("emergency_contact.phone_required", result.Error.Code);
    }

    [Fact]
    public void Editar_tutor_atualiza_dados_e_coleções()
    {
        var tutor = Tutor.Register(Tenant, "Maria", "m@b.com", "11999998888").Value;
        var contact = EmergencyContact.Create("João", "11988887777", "Vizinho").Value;

        var result = tutor.Update("Maria Souza", "maria@novo.com", "11900001111", [contact], null);

        Assert.True(result.IsSuccess);
        Assert.Equal("Maria Souza", tutor.FullName);
        Assert.Equal("maria@novo.com", tutor.Email.Value);
        Assert.Single(tutor.EmergencyContacts);
        Assert.Empty(tutor.AuthorizedPickups);
    }

    [Fact]
    public void Editar_tutor_com_email_invalido_falha()
    {
        var tutor = Tutor.Register(Tenant, "Maria", "m@b.com", "11999998888").Value;

        var result = tutor.Update("Maria", "nao-eh-email", "11999998888");

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal("m@b.com", tutor.Email.Value); // preservado
    }
}
