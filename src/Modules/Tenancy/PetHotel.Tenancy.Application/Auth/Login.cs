namespace PetHotel.Tenancy.Application.Auth;

/// <summary>Autentica por e-mail e senha e devolve o token de acesso.</summary>
public sealed record Login(string Email, string Password);
