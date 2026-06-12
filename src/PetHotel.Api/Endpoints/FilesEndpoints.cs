using PetHotel.Api.Storage;

namespace PetHotel.Api.Endpoints;

/// <summary>
/// Download autenticado e tenant-scoped de arquivos. A chave já carrega o tenant; o
/// adaptador recusa chaves de outro tenant (devolvendo 404 aqui).
/// </summary>
public static class FilesEndpoints
{
    public static IEndpointRouteBuilder MapFilesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/files/{**key}", async (string key, IFileStorage storage, CancellationToken ct) =>
            {
                var file = await storage.OpenAsync(key, ct);
                if (file is null)
                {
                    return Results.NotFound();
                }

                // Imutável: a chave é única por upload, então pode cachear agressivamente.
                return Results.Stream(file.Content, file.ContentType, enableRangeProcessing: true);
            })
            .WithTags("Files")
            .RequireAuthorization()
            .WithName("DownloadFile")
            .WithSummary("Baixa um arquivo (foto) do tenant corrente pela chave.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
