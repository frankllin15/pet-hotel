using PetHotel.SharedKernel;
using PetHotel.Tenancy.Application.Abstractions;

namespace PetHotel.Api.Endpoints;

/// <summary>
/// Diretório de usuários do hotel (id + nome) para qualquer usuário autenticado resolver autoria
/// (quem registrou diário/medicação/incidente). Diferente de /v1/invitations (admin), expõe só o
/// mínimo — sem e-mail, papéis ou status.
/// </summary>
public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/users", async (ITenantContext tenantContext, IUserDirectory directory, CancellationToken ct) =>
            {
                var users = await directory.ListByTenantAsync(tenantContext.Current, ct);
                return Results.Ok(users.Select(u => new UserSummaryResponse(u.Id, u.DisplayName)).ToList());
            })
            .RequireAuthorization()
            .WithTags("Users")
            .WithName("ListTenantUsers")
            .WithSummary("Diretório de usuários do hotel (id + nome) para resolver autoria.")
            .Produces<IReadOnlyList<UserSummaryResponse>>(StatusCodes.Status200OK);

        return app;
    }

    /// <summary>Usuário no diretório de autoria: só id e nome.</summary>
    public sealed record UserSummaryResponse(Guid Id, string DisplayName);
}
