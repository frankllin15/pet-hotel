namespace PetHotel.Registry.Application.Tutors.DeleteTutor;

/// <summary>Exclui um tutor do tenant corrente (somente se não houver pets vinculados).</summary>
public sealed record DeleteTutor(Guid Id);
