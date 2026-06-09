namespace PetHotel.IntegrationTests.Support;

/// <summary>
/// Serializa os testes baseados em WebApplicationFactory: eles configuram a connection
/// string/Jwt por variável de ambiente (processo-global), então não podem rodar em paralelo.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class WebApplicationCollection
{
    public const string Name = "WebApplication";
}
