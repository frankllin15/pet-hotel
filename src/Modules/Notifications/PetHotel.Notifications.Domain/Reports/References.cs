namespace PetHotel.Notifications.Domain.Reports;

/// <summary>Referência ao pet (agregado do Registry) por Id — módulos não compartilham domínio.</summary>
public readonly record struct PetReference(Guid Value)
{
    public override string ToString() => Value.ToString();
}

/// <summary>Referência ao tutor (agregado do Registry) por Id — destinatário do relatório.</summary>
public readonly record struct TutorReference(Guid Value)
{
    public override string ToString() => Value.ToString();
}
