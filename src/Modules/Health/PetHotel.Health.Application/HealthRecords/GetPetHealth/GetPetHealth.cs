namespace PetHotel.Health.Application.HealthRecords.GetPetHealth;

/// <summary>Consulta a ficha de saúde de um pet (com aptidão sanitária na data de hoje).</summary>
public sealed record GetPetHealth(Guid PetId);
