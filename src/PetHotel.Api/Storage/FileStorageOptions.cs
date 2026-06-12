namespace PetHotel.Api.Storage;

/// <summary>Configuração do storage local (seção <c>FileStorage</c> em appsettings).</summary>
public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>
    /// Raiz onde os arquivos são gravados. Relativa ao ContentRoot quando não absoluta.
    /// Em produção apontar para um volume persistente fora do diretório publicado.
    /// </summary>
    public string RootPath { get; set; } = "App_Data/uploads";

    /// <summary>Tamanho máximo de upload aceito (bytes). Default 5 MiB.</summary>
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;
}
