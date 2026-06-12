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

    private static Result<Pet> Register(
        string? name = "Rex",
        Species species = Species.Dog,
        string? breed = "Vira-lata",
        DateOnly? birthDate = null,
        PetSize? size = null,
        Sex? sex = null,
        bool? neutered = null,
        string? microchip = null,
        TutorId? tutor = null) =>
        Pet.Register(Tenant, tutor ?? Tutor, name, species, breed, birthDate, size, sex, neutered, microchip, null, Today);

    [Fact]
    public void Registrar_pet_valido_levanta_evento()
    {
        var result = Register(birthDate: new DateOnly(2020, 1, 1));

        Assert.True(result.IsSuccess);
        Assert.Equal(Tutor, result.Value.TutorId);
        Assert.Equal(Species.Dog, result.Value.Species);
        Assert.Contains(result.Value.DomainEvents, e => e is PetRegistered);
    }

    [Fact]
    public void Registrar_pet_guarda_porte_sexo_castracao_e_microchip()
    {
        var result = Register(size: PetSize.Large, sex: Sex.Female, neutered: true, microchip: " 982000123456789 ");

        Assert.True(result.IsSuccess);
        Assert.Equal(PetSize.Large, result.Value.Size);
        Assert.Equal(Sex.Female, result.Value.Sex);
        Assert.True(result.Value.Neutered);
        Assert.Equal("982000123456789", result.Value.MicrochipCode); // trim
    }

    [Fact]
    public void Registrar_pet_sem_atributos_opcionais_mantem_nulos()
    {
        var result = Register();

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.Size);
        Assert.Null(result.Value.Sex);
        Assert.Null(result.Value.Neutered);
        Assert.Null(result.Value.MicrochipCode);
    }

    [Fact]
    public void Registrar_pet_sem_nome_falha()
    {
        var result = Register(name: "  ", species: Species.Cat, breed: null);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void Registrar_pet_com_nascimento_no_futuro_falha()
    {
        var result = Register(breed: null, birthDate: Today.AddDays(1));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void Registrar_pet_sem_tutor_falha()
    {
        var result = Register(breed: null, tutor: default(TutorId));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void Editar_pet_atualiza_campos()
    {
        var pet = Register(size: PetSize.Small, neutered: false).Value;

        var result = pet.Update(
            "Bidu", Species.Dog, "SRD", new DateOnly(2021, 5, 1),
            PetSize.Large, Sex.Male, neutered: true, microchipCode: "999", notes: "atualizado",
            sociability: null, reactivity: null, fear: null, destructiveness: null, behaviorNotes: null, Today);

        Assert.True(result.IsSuccess);
        Assert.Equal("Bidu", pet.Name);
        Assert.Equal(PetSize.Large, pet.Size);
        Assert.Equal(Sex.Male, pet.Sex);
        Assert.True(pet.Neutered);
        Assert.Equal("999", pet.MicrochipCode);
    }

    [Fact]
    public void Editar_pet_guarda_avaliacao_comportamental()
    {
        var pet = Register().Value;

        var result = pet.Update(
            "Rex", Species.Dog, null, null, null, null, null, null, null,
            sociability: BehaviorLevel.High, reactivity: BehaviorLevel.Low, fear: BehaviorLevel.Medium,
            destructiveness: BehaviorLevel.Low, behaviorNotes: " brinca bem ", Today);

        Assert.True(result.IsSuccess);
        Assert.Equal(BehaviorLevel.High, pet.Sociability);
        Assert.Equal(BehaviorLevel.Low, pet.Reactivity);
        Assert.Equal(BehaviorLevel.Medium, pet.Fear);
        Assert.Equal("brinca bem", pet.BehaviorNotes); // trim
    }

    [Fact]
    public void Criar_rotina_alimentar_normaliza_horarios_e_textos()
    {
        var result = FeedingRoutine.Create(
            " Golden Filhote ",
            " 100 g ",
            [new TimeOnly(18, 0), new TimeOnly(8, 0), new TimeOnly(8, 0)],
            "  ",
            FoodSource.TutorProvided);

        Assert.True(result.IsSuccess);
        Assert.Equal("Golden Filhote", result.Value.FoodName); // trim
        Assert.Equal("100 g", result.Value.PortionSize); // trim
        Assert.Equal([new TimeOnly(8, 0), new TimeOnly(18, 0)], result.Value.MealTimes); // ordenado, sem repetição
        Assert.Null(result.Value.Restrictions); // em branco vira null
    }

    [Fact]
    public void Criar_rotina_alimentar_sem_racao_falha()
    {
        var result = FeedingRoutine.Create("  ", null, null, null, FoodSource.HotelProvided);

        Assert.True(result.IsFailure);
        Assert.Equal("feeding_routine.food_name_required", result.Error.Code);
    }

    [Fact]
    public void Criar_rotina_alimentar_com_origem_invalida_falha()
    {
        var result = FeedingRoutine.Create("Golden", null, null, null, (FoodSource)99);

        Assert.True(result.IsFailure);
        Assert.Equal("feeding_routine.food_source_invalid", result.Error.Code);
    }

    [Fact]
    public void Editar_pet_guarda_rotina_alimentar()
    {
        var pet = Register().Value;
        var routine = FeedingRoutine.Create(
            "Golden", "150 g", [new TimeOnly(7, 30)], "sem frango", FoodSource.TutorProvided).Value;

        var result = pet.Update(
            "Rex", Species.Dog, null, null, null, null, null, null, null,
            null, null, null, null, null, Today, routine);

        Assert.True(result.IsSuccess);
        Assert.NotNull(pet.FeedingRoutine);
        Assert.Equal("Golden", pet.FeedingRoutine.FoodName);
        Assert.Equal("sem frango", pet.FeedingRoutine.Restrictions);
        Assert.Equal(FoodSource.TutorProvided, pet.FeedingRoutine.FoodSource);
    }

    [Fact]
    public void Editar_pet_sem_rotina_alimentar_limpa_a_rotina()
    {
        var pet = Register().Value;
        var routine = FeedingRoutine.Create("Golden", null, null, null, FoodSource.HotelProvided).Value;
        pet.Update("Rex", Species.Dog, null, null, null, null, null, null, null,
            null, null, null, null, null, Today, routine);

        var result = pet.Update("Rex", Species.Dog, null, null, null, null, null, null, null,
            null, null, null, null, null, Today);

        Assert.True(result.IsSuccess);
        Assert.Null(pet.FeedingRoutine);
    }

    [Fact]
    public void Editar_pet_sem_nome_falha_e_preserva_estado()
    {
        var pet = Register(name: "Rex").Value;

        var result = pet.Update(
            "  ", Species.Dog, null, null, null, null, null, null, null,
            null, null, null, null, null, Today);

        Assert.True(result.IsFailure);
        Assert.Equal("pet.name_required", result.Error.Code);
        Assert.Equal("Rex", pet.Name);
    }

    [Fact]
    public void Excluir_pet_levanta_evento()
    {
        var pet = Register().Value;

        pet.Delete();

        var evt = Assert.Single(pet.DomainEvents, e => e is PetDeleted);
        var deleted = Assert.IsType<PetDeleted>(evt);
        Assert.Equal(pet.Id, deleted.PetId);
        Assert.Equal(Tutor, deleted.TutorId);
    }
}
