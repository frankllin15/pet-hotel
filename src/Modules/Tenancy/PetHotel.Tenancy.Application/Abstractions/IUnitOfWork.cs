namespace PetHotel.Tenancy.Application.Abstractions;

/// <summary>
/// Limite transacional do módulo (o DbContext é a Unit of Work, docs/04).
/// Porta específica do Tenancy para evitar colisão de DI entre módulos.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
