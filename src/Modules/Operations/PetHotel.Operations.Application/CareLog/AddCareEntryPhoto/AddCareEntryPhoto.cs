namespace PetHotel.Operations.Application.CareLog.AddCareEntryPhoto;

/// <summary>Anexa uma foto (já gravada no storage) a uma ocorrência do diário.</summary>
public sealed record AddCareEntryPhoto(Guid EntryId, string Key);
