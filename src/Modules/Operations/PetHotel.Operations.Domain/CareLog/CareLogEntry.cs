using PetHotel.Operations.Domain.CareLog.Events;
using PetHotel.SharedKernel;

namespace PetHotel.Operations.Domain.CareLog;

/// <summary>
/// Entrada do diário de bordo: uma ocorrência registrada para um pet, vinculada ao contexto de
/// presença (estadia de hotel hoje; creche no futuro). Agregado tenant-scoped; referencia o pet
/// e o contexto por Id (fronteira de agregado, docs/03).
/// </summary>
public sealed class CareLogEntry : AggregateRoot<CareLogEntryId>, IHasTenant, IAuditable
{
    public TenantId TenantId { get; private set; }
    public PetReference Pet { get; private set; }
    /// <summary>Tipo do contexto de presença (estadia/creche).</summary>
    public CareContextType ContextType { get; private set; }
    /// <summary>Id do contexto (ex.: ReservationId da estadia).</summary>
    public Guid ContextId { get; private set; }
    public CareLogEntryType Type { get; private set; }
    /// <summary>Detalhe livre da ocorrência (opcional; o tipo já carrega o sentido).</summary>
    public string? Note { get; private set; }
    /// <summary>Momento em que a ocorrência aconteceu (pode ser anterior ao registro).</summary>
    public DateTimeOffset OccurredAt { get; private set; }

    /// <summary>Chaves das fotos anexadas à ocorrência (tenant-scoped no storage).</summary>
    public List<string> PhotoKeys { get; private set; } = [];

    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    private CareLogEntry() { } // EF

    private CareLogEntry(
        CareLogEntryId id, TenantId tenantId, PetReference pet, CareContextType contextType, Guid contextId,
        CareLogEntryType type, string? note, DateTimeOffset occurredAt)
        : base(id)
    {
        TenantId = tenantId;
        Pet = pet;
        ContextType = contextType;
        ContextId = contextId;
        Type = type;
        Note = note;
        OccurredAt = occurredAt;
    }

    public static Result<CareLogEntry> Log(
        TenantId tenantId, PetReference pet, CareContextType contextType, Guid contextId,
        CareLogEntryType type, string? note, DateTimeOffset occurredAt, DateTimeOffset now)
    {
        if (tenantId.Value == Guid.Empty)
        {
            return Error.Validation("care_log.tenant_required", "Tenant é obrigatório.");
        }

        if (pet.Value == Guid.Empty)
        {
            return Error.Validation("care_log.pet_required", "Pet é obrigatório.");
        }

        if (!Enum.IsDefined(contextType))
        {
            return Error.Validation("care_log.context_invalid", "Contexto de cuidado inválido.");
        }

        if (contextId == Guid.Empty)
        {
            return Error.Validation("care_log.context_required", "Contexto (estadia) é obrigatório.");
        }

        if (!Enum.IsDefined(type))
        {
            return Error.Validation("care_log.type_invalid", "Tipo de ocorrência inválido.");
        }

        if (occurredAt > now)
        {
            return Error.Validation("care_log.occurred_in_future", "A ocorrência não pode estar no futuro.");
        }

        var entry = new CareLogEntry(
            CareLogEntryId.New(),
            tenantId,
            pet,
            contextType,
            contextId,
            type,
            string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            occurredAt);

        entry.Raise(new CareEntryLogged(entry.Id, tenantId, pet));
        return entry;
    }

    /// <summary>Limite de fotos por ocorrência.</summary>
    public const int MaxPhotos = 8;

    /// <summary>Anexa uma foto (já gravada no storage; aqui só guardamos a chave).</summary>
    public Result AddPhoto(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return Error.Validation("care_photo.key_required", "Chave do arquivo é obrigatória.");
        }

        if (PhotoKeys.Count >= MaxPhotos)
        {
            return Error.Conflict("care_photo.limit_reached", $"Limite de {MaxPhotos} fotos por ocorrência atingido.");
        }

        PhotoKeys.Add(key);
        return Result.Success();
    }

    /// <summary>Remove uma foto pela chave (o arquivo é apagado pelo adaptador).</summary>
    public Result RemovePhoto(string key) =>
        PhotoKeys.Remove(key)
            ? Result.Success()
            : Error.NotFound("care_photo.not_found", "Foto não encontrada nesta ocorrência.");
}
