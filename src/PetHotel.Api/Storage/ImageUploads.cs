using Microsoft.Extensions.Options;
using PetHotel.SharedKernel;

namespace PetHotel.Api.Storage;

/// <summary>
/// Validação e gravação de uploads de imagem reusada pelos endpoints (foto de pet,
/// carteira de vacina). Falhas esperadas voltam como <see cref="Error"/> (docs/02).
/// </summary>
public static class ImageUploads
{
    private static readonly HashSet<string> AllowedContentTypes =
        new(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/png", "image/webp" };

    public static async Task<Result<StoredFile>> SaveAsync(
        IFormFile? file,
        string category,
        IFileStorage storage,
        IOptions<FileStorageOptions> options,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return Error.Validation("file.required", "Envie um arquivo de imagem.");
        }

        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            return Error.Validation("file.invalid_type", "Formato inválido. Use JPEG, PNG ou WebP.");
        }

        if (file.Length > options.Value.MaxFileSizeBytes)
        {
            var maxMb = options.Value.MaxFileSizeBytes / (1024 * 1024);
            return Error.Validation("file.too_large", $"Arquivo acima do limite de {maxMb} MB.");
        }

        await using var stream = file.OpenReadStream();
        return await storage.SaveAsync(category, stream, file.ContentType, file.FileName, cancellationToken);
    }
}
