namespace PetHotel.Operations.Domain.CareLog;

/// <summary>Referência ao pet (agregado do Registry) por Id — módulos não compartilham domínio.</summary>
public readonly record struct PetReference(Guid Value)
{
    public override string ToString() => Value.ToString();
}
