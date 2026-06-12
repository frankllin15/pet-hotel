namespace PetHotel.Api.Storage;

/// <summary>
/// Porta de armazenamento de arquivos (fotos de pet, carteira de vacina etc.).
/// Vive na API por ser uma preocupação de adaptador (composition root): o domínio
/// guarda apenas a <c>chave</c> resultante como string; quem orquestra o upload é o
/// endpoint. As chaves são sempre tenant-scoped (docs/04) — o adaptador prefixa o
/// tenant corrente e recusa leituras de outro tenant.
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Persiste o conteúdo e devolve a chave tenant-scoped para guardar no agregado.
    /// </summary>
    Task<StoredFile> SaveAsync(
        string category,
        Stream content,
        string contentType,
        string? originalFileName,
        CancellationToken cancellationToken = default);

    /// <summary>Abre o arquivo da chave informada, ou null se não existir / não pertencer ao tenant.</summary>
    Task<FileContent?> OpenAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>Remove o arquivo da chave (idempotente). Ignora chaves de outro tenant.</summary>
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
}

/// <summary>Resultado de uma escrita: a chave a guardar e os metadados.</summary>
public sealed record StoredFile(string Key, string ContentType, long Size);

/// <summary>Conteúdo aberto para download. O chamador é dono do <see cref="Content"/> e deve descartá-lo.</summary>
public sealed record FileContent(Stream Content, string ContentType, long Size);
