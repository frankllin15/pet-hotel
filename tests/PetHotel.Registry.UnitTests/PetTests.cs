using PetHotel.Registry.Domain.Pets;
using PetHotel.Registry.Domain.Pets.Events;
using PetHotel.Registry.Domain.Tutors;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.UnitTests;

public class PetTests
{
    private static readonly TenantId Tenant = TenantId.New();
    private static readonly TutorId Tutor = TutorId.New();
    private static readonly DateOnly Today = new(2026, 6, 6);

    [Fact]
    public void Registrar_pet_valido_levanta_evento()
    {
        var result = Pet.Register(Tenant, Tutor, "Rex", Species.Dog, "Vira-lata", new DateOnly(2020, 1, 1), null, Today);

        Assert.True(result.IsSuccess);
        Assert.Equal(Tutor, result.Value.TutorId);
        Assert.Equal(Species.Dog, result.Value.Species);
        Assert.Contains(result.Value.DomainEvents, e => e is PetRegistered);
    }

    [Fact]
    public void Registrar_pet_sem_nome_falha()
    {
        var result = Pet.Register(Tenant, Tutor, "  ", Species.Cat, null, null, null, Today);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void Registrar_pet_com_nascimento_no_futuro_falha()
    {
        var future = Today.AddDays(1);

        var result = Pet.Register(Tenant, Tutor, "Rex", Species.Dog, null, future, null, Today);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void Registrar_pet_sem_tutor_falha()
    {
        var result = Pet.Register(Tenant, default, "Rex", Species.Dog, null, null, null, Today);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }
}
