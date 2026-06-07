namespace PetHotel.Registry.Application.Tutors;

/// <summary>Projeção de leitura de um tutor (docs/04).</summary>
public sealed record TutorDto(Guid Id, string FullName, string Email, string Phone, DateTimeOffset CreatedAt);
