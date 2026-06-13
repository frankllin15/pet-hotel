namespace PetHotel.Operations.Application.CareLog.RemoveCareEntryPhoto;

/// <summary>Remove uma foto de uma ocorrência do diário (pela chave do storage).</summary>
public sealed record RemoveCareEntryPhoto(Guid EntryId, string Key);
