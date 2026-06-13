namespace PetHotel.Operations.Application.Abstractions;

/// <summary>Limite transacional do módulo Operations (o DbContext é a Unit of Work, docs/04).</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
