namespace PetHotel.Health.Application.Contracts;

/// <summary>
/// Contrato PÚBLICO de aptidão sanitária consumível por outros módulos (ex.: Booking).
/// Livre de tipos internos do Health — pendências são nomes de vacina (docs/01).
/// </summary>
public sealed record PetHealthClearance(Guid PetId, bool IsCleared, IReadOnlyList<string> Pendencies);
