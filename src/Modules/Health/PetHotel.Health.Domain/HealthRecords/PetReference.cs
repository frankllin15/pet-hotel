namespace PetHotel.Health.Domain.HealthRecords;

/// <summary>
/// Referência ao pet (agregado do módulo Registry) por Id. Módulos não compartilham
/// domínio — aqui só guardamos o identificador (docs/01, docs/03).
/// </summary>
public readonly record struct PetReference(Guid Value)
{
    public override string ToString() => Value.ToString();
}
