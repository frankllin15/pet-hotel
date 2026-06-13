using PetHotel.Operations.Application.CareLog;
using PetHotel.SharedKernel;

namespace PetHotel.Operations.Application.Abstractions;

/// <summary>Porta de leitura do diário de bordo (AsNoTracking + query filter de tenant, docs/04).</summary>
public interface ICareLogQueries
{
    /// <summary>Timeline de ocorrências de um contexto (estadia), mais recentes primeiro (cursor).</summary>
    Task<CursorPage<CareLogEntryDto>> GetByContextAsync(
        Guid contextId,
        Cursor? after,
        int limit,
        CancellationToken cancellationToken = default);
}
