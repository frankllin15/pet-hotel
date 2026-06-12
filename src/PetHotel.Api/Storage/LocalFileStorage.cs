using Microsoft.Extensions.Options;
using PetHotel.SharedKernel;

namespace PetHotel.Api.Storage;

/// <summary>
/// Adaptador de <see cref="IFileStorage"/> em disco local — suficiente para o MVP
/// (trocável por S3/Azure Blob sem tocar no domínio). Toda chave é prefixada pelo
/// tenant corrente; leituras/exclusões de outro tenant são recusadas e o caminho
/// resolvido é validado contra a raiz para impedir path traversal.
/// </summary>
public sealed class LocalFileStorage(
    IOptions<FileStorageOptions> options,
    ITenantContext tenantContext,
    IWebHostEnvironment environment) : IFileStorage
{
    private static readonly Dictionary<string, string> ExtensionByContentType = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp",
        ["image/gif"] = ".gif",
        ["application/pdf"] = ".pdf",
    };

    private readonly FileStorageOptions _options = options.Value;

    public async Task<StoredFile> SaveAsync(
        string category,
        Stream content,
        string contentType,
        string? originalFileName,
        CancellationToken cancellationToken = default)
    {
        var tenant = tenantContext.Current.Value.ToString("N");
        var extension = ResolveExtension(contentType, originalFileName);
        var key = $"{tenant}/{Sanitize(category)}/{Guid.NewGuid():N}{extension}";

        var fullPath = ResolvePath(key)
            ?? throw new InvalidOperationException("Chave de arquivo inválida.");

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var file = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(file, cancellationToken);

        return new StoredFile(key, contentType, file.Length);
    }

    public Task<FileContent?> OpenAsync(string key, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolvePathForCurrentTenant(key);
        if (fullPath is null || !File.Exists(fullPath))
        {
            return Task.FromResult<FileContent?>(null);
        }

        var contentType = ContentTypeFor(fullPath);
        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<FileContent?>(new FileContent(stream, contentType, stream.Length));
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolvePathForCurrentTenant(key);
        if (fullPath is not null && File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string Root =>
        Path.IsPathRooted(_options.RootPath)
            ? _options.RootPath
            : Path.Combine(environment.ContentRootPath, _options.RootPath);

    /// <summary>Resolve a chave em caminho absoluto, garantindo que não escapa da raiz.</summary>
    private string? ResolvePath(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        var root = Path.GetFullPath(Root);
        var combined = Path.GetFullPath(Path.Combine(root, key.Replace('/', Path.DirectorySeparatorChar)));

        // Defesa contra path traversal: o caminho final precisa ficar dentro da raiz.
        return combined.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            ? combined
            : null;
    }

    /// <summary>Como <see cref="ResolvePath"/>, mas só aceita chaves do tenant corrente.</summary>
    private string? ResolvePathForCurrentTenant(string key)
    {
        var tenant = tenantContext.Current.Value.ToString("N");
        return key.StartsWith($"{tenant}/", StringComparison.Ordinal) ? ResolvePath(key) : null;
    }

    private static string ResolveExtension(string contentType, string? originalFileName)
    {
        if (ExtensionByContentType.TryGetValue(contentType, out var byType))
        {
            return byType;
        }

        var byName = Path.GetExtension(originalFileName ?? string.Empty);
        return string.IsNullOrWhiteSpace(byName) ? ".bin" : byName.ToLowerInvariant();
    }

    private static string ContentTypeFor(string path)
    {
        var extension = Path.GetExtension(path);
        foreach (var (type, ext) in ExtensionByContentType)
        {
            if (string.Equals(ext, extension, StringComparison.OrdinalIgnoreCase))
            {
                return type;
            }
        }

        return "application/octet-stream";
    }

    private static string Sanitize(string segment) =>
        string.Concat(segment.Where(c => char.IsLetterOrDigit(c) || c is '-' or '_'));
}
