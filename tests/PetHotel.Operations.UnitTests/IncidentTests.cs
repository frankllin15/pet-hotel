using PetHotel.Operations.Domain.CareLog;
using PetHotel.Operations.Domain.Incidents;
using PetHotel.Operations.Domain.Incidents.Events;
using PetHotel.SharedKernel;

namespace PetHotel.Operations.UnitTests;

public class IncidentTests
{
    private static readonly TenantId Tenant = TenantId.New();
    private static readonly PetReference Pet = new(Guid.NewGuid());
    private static readonly Guid Stay = Guid.NewGuid();
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-06-13T12:00:00Z");

    [Fact]
    public void Registrar_incidente_valido_levanta_evento()
    {
        var result = Incident.Report(Tenant, Pet, CareContextType.HotelStay, Stay, IncidentSeverity.High, " brigou com outro pet ", Now.AddHours(-1), Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(IncidentSeverity.High, result.Value.Severity);
        Assert.Equal("brigou com outro pet", result.Value.Description); // trim
        var evt = Assert.Single(result.Value.DomainEvents, e => e is IncidentReported);
        Assert.Equal(IncidentSeverity.High, ((IncidentReported)evt).Severity);
    }

    [Fact]
    public void Sem_descricao_falha()
    {
        var result = Incident.Report(Tenant, Pet, CareContextType.HotelStay, Stay, IncidentSeverity.Low, "  ", Now, Now);
        Assert.True(result.IsFailure);
        Assert.Equal("incident.description_required", result.Error.Code);
    }

    [Fact]
    public void Gravidade_invalida_falha()
    {
        var result = Incident.Report(Tenant, Pet, CareContextType.HotelStay, Stay, (IncidentSeverity)99, "x", Now, Now);
        Assert.True(result.IsFailure);
        Assert.Equal("incident.severity_invalid", result.Error.Code);
    }

    [Fact]
    public void Incidente_no_futuro_falha()
    {
        var result = Incident.Report(Tenant, Pet, CareContextType.HotelStay, Stay, IncidentSeverity.Medium, "x", Now.AddMinutes(5), Now);
        Assert.True(result.IsFailure);
        Assert.Equal("incident.occurred_in_future", result.Error.Code);
    }
}
