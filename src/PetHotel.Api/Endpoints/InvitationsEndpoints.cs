using FluentValidation;
using PetHotel.Api.Http;
using PetHotel.SharedKernel;
using PetHotel.Tenancy.Application.Abstractions;
using PetHotel.Tenancy.Application.Invitations;

namespace PetHotel.Api.Endpoints;

/// <summary>
/// Convite de usuários e listagem da equipe. Restrito a admin do tenant. Convidar
/// toca Identity, então invoca o handler direto via DI (fora do codegen do Wolverine).
/// </summary>
public static class InvitationsEndpoints
{
    public static IEndpointRouteBuilder MapInvitationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/invitations").WithTags("Invitations").RequireAuthorization("TenantAdmin");

        group.MapPost("",
                async (
                    InviteUser command,
                    IValidator<InviteUser> validator,
                    ITenantContext tenantContext,
                    IUserAccountService userAccounts,
                    CancellationToken ct) =>
                {
                    var result = await InviteUserHandler.Handle(command, validator, tenantContext, userAccounts, ct);
                    // ActivationToken volta no corpo só no MVP; em produção iria por e-mail (Notifications).
                    return result.ToHttpResult(inv => Results.Created($"/v1/invitations/{inv.UserId}", inv));
                })
            .WithName("InviteUser")
            .WithSummary("Convida um usuário para o hotel já com o papel atribuído.")
            .Produces<Invitation>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("",
                async (ITenantContext tenantContext, IUserDirectory directory, CancellationToken ct) =>
                {
                    var users = await directory.ListByTenantAsync(tenantContext.Current, ct);
                    return Results.Ok(users);
                })
            .WithName("ListUsers")
            .WithSummary("Lista os usuários do hotel (ativos e pendentes).")
            .Produces<IReadOnlyList<DirectoryUser>>(StatusCodes.Status200OK);

        return app;
    }
}
