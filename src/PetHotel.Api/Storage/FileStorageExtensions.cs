namespace PetHotel.Api.Storage;

/// <summary>Registro DI do storage de arquivos (composition root).</summary>
public static class FileStorageExtensions
{
    public static IServiceCollection AddFileStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));
        services.AddScoped<IFileStorage, LocalFileStorage>();
        return services;
    }
}
