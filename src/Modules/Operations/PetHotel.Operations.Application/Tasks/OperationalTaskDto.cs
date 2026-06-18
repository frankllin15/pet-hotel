namespace PetHotel.Operations.Application.Tasks;

/// <summary>Projeção de leitura de uma tarefa operacional do dia (docs/04).</summary>
public sealed record OperationalTaskDto(
    Guid Id,
    string Title,
    DateOnly Date,
    string Category,
    Guid? AssignedTo,
    bool Done,
    DateTimeOffset? CompletedAt);
