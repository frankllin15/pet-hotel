namespace PetHotel.SharedKernel;

/// <summary>
/// Marca um fato de domínio já ocorrido dentro de um agregado.
/// Despachado após o commit (docs/03, docs/05).
/// </summary>
public interface IDomainEvent;
