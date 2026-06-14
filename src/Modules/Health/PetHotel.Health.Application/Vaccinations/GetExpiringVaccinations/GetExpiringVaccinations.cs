namespace PetHotel.Health.Application.Vaccinations.GetExpiringVaccinations;

/// <summary>Vacinas vencidas ou que vencem em até <see cref="WithinDays"/> dias a partir de <see cref="AsOf"/>.</summary>
public sealed record GetExpiringVaccinations(DateOnly AsOf, int WithinDays);
