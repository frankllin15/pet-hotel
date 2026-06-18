namespace PetHotel.Operations.Domain.Tasks;

/// <summary>Identificador tipado de uma tarefa operacional.</summary>
public readonly record struct OperationalTaskId(Guid Value)
{
    public static OperationalTaskId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
