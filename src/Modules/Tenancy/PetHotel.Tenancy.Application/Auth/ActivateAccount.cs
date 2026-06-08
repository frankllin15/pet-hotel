namespace PetHotel.Tenancy.Application.Auth;

/// <summary>Primeiro acesso: define a senha a partir do token de ativação/convite.</summary>
public sealed record ActivateAccount(string Email, string Token, string Password);
