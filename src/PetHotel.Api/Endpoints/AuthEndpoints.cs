using FluentValidation;
using PetHotel.Api.Http;
using PetHotel.Tenancy.Application.Abstractions;
using PetHotel.Tenancy.Application.Auth;

namespace PetHotel.Api.Endpoints;

/// <summary>
/// Autenticação: ativação de conta (primeiro acesso) e login. Invocam os handlers
/// direto via DI (tocam Identity — fora do codegen do Wolverine).
/// </summary>
public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/auth").WithTags("Auth");

        group.MapPost("/activate",
                async (
                    ActivateAccount command,
                    IValidator<ActivateAccount> validator,
                    IUserAccountService userAccounts,
                    CancellationToken ct) =>
                {
                    var result = await ActivateAccountHandler.Handle(command, validator, userAccounts, ct);
                    return result.ToHttpResult(Results.NoContent());
                })
            .WithName("ActivateAccount")
            .WithSummary("Define a senha no primeiro acesso a partir do token de ativação/convite.")
            .AllowAnonymous()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/login",
                async (
                    Login command,
                    IValidator<Login> validator,
                    IUserAccountService userAccounts,
                    IJwtTokenIssuer tokenIssuer,
                    CancellationToken ct) =>
                {
                    var result = await LoginHandler.Handle(command, validator, userAccounts, tokenIssuer, ct);
                    return result.ToHttpResult(Results.Ok);
                })
            .WithName("Login")
            .WithSummary("Autentica e devolve o JWT de acesso.")
            .AllowAnonymous()
            .Produces<AccessToken>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return app;
    }
}
