namespace PetHotel.Registry.Application.Tutors.RegisterTutor;

/// <summary>Cadastra um tutor no tenant corrente.</summary>
public sealed record RegisterTutor(string FullName, string Email, string Phone);
