namespace PetHotel.Tenancy.Infrastructure.Auth;

/// <summary>Opções do JWT (bind da seção "Jwt" da configuração).</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "PetHotel";
    public string Audience { get; set; } = "PetHotel";
    public string SigningKey { get; set; } = null!;
    public int LifetimeMinutes { get; set; } = 60;
}
