using PetHotel.Registry.Domain.Pets.Events;
using PetHotel.Registry.Domain.Tutors;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.Domain.Pets;

/// <summary>
/// Pet hospedado. Agregado tenant-scoped; referencia o tutor por Id, nunca por
/// navegação (fronteira de agregado, docs/03).
/// </summary>
public sealed class Pet : AggregateRoot<PetId>, IHasTenant, IAuditable
{
    public TenantId TenantId { get; private set; }
    public TutorId TutorId { get; private set; }
    public string Name { get; private set; } = null!;
    public Species Species { get; private set; }
    public string? Breed { get; private set; }
    public DateOnly? BirthDate { get; private set; }
    public string? Notes { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    private Pet() { } // EF

    private Pet(
        PetId id,
        TenantId tenantId,
        TutorId tutorId,
        string name,
        Species species,
        string? breed,
        DateOnly? birthDate,
        string? notes) : base(id)
    {
        TenantId = tenantId;
        TutorId = tutorId;
        Name = name;
        Species = species;
        Breed = breed;
        BirthDate = birthDate;
        Notes = notes;
    }

    public static Result<Pet> Register(
        TenantId tenantId,
        TutorId tutorId,
        string? name,
        Species species,
        string? breed,
        DateOnly? birthDate,
        string? notes,
        DateOnly today)
    {
        if (tenantId.Value == Guid.Empty)
        {
            return Error.Validation("pet.tenant_required", "Tenant é obrigatório.");
        }

        if (tutorId.Value == Guid.Empty)
        {
            return Error.Validation("pet.tutor_required", "Tutor é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation("pet.name_required", "Nome do pet é obrigatório.");
        }

        if (birthDate is { } birth && birth > today)
        {
            return Error.Validation("pet.birthdate_future", "Data de nascimento não pode ser no futuro.");
        }

        var pet = new Pet(
            PetId.New(),
            tenantId,
            tutorId,
            name.Trim(),
            species,
            string.IsNullOrWhiteSpace(breed) ? null : breed.Trim(),
            birthDate,
            string.IsNullOrWhiteSpace(notes) ? null : notes.Trim());

        pet.Raise(new PetRegistered(pet.Id, tenantId, tutorId));
        return pet;
    }
}
