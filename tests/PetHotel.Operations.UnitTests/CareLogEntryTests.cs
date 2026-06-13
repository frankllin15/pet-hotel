using PetHotel.Operations.Domain.CareLog;
using PetHotel.Operations.Domain.CareLog.Events;
using PetHotel.SharedKernel;

namespace PetHotel.Operations.UnitTests;

public class CareLogEntryTests
{
    private static readonly TenantId Tenant = TenantId.New();
    private static readonly PetReference Pet = new(Guid.NewGuid());
    private static readonly Guid Stay = Guid.NewGuid();
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-06-13T12:00:00Z");

    [Fact]
    public void Registrar_ocorrencia_valida_normaliza_nota_e_levanta_evento()
    {
        var result = CareLogEntry.Log(Tenant, Pet, CareContextType.HotelStay, Stay, CareLogEntryType.Meal, "  comeu tudo  ", Now.AddHours(-1), Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(CareLogEntryType.Meal, result.Value.Type);
        Assert.Equal(CareContextType.HotelStay, result.Value.ContextType);
        Assert.Equal(Stay, result.Value.ContextId);
        Assert.Equal("comeu tudo", result.Value.Note); // trim
        Assert.Equal(Now.AddHours(-1), result.Value.OccurredAt);
        Assert.Contains(result.Value.DomainEvents, e => e is CareEntryLogged);
    }

    [Fact]
    public void Nota_em_branco_vira_nula()
    {
        var result = CareLogEntry.Log(Tenant, Pet, CareContextType.HotelStay, Stay, CareLogEntryType.Play, "   ", Now, Now);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.Note);
    }

    [Fact]
    public void Ocorrencia_no_futuro_falha()
    {
        var result = CareLogEntry.Log(Tenant, Pet, CareContextType.HotelStay, Stay, CareLogEntryType.Note, "x", Now.AddMinutes(1), Now);

        Assert.True(result.IsFailure);
        Assert.Equal("care_log.occurred_in_future", result.Error.Code);
    }

    [Fact]
    public void Tipo_invalido_falha()
    {
        var result = CareLogEntry.Log(Tenant, Pet, CareContextType.HotelStay, Stay, (CareLogEntryType)99, null, Now, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("care_log.type_invalid", result.Error.Code);
    }

    [Fact]
    public void Sem_contexto_de_estadia_falha()
    {
        var result = CareLogEntry.Log(Tenant, Pet, CareContextType.HotelStay, Guid.Empty, CareLogEntryType.Meal, null, Now, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("care_log.context_required", result.Error.Code);
    }

    [Fact]
    public void Anexar_e_remover_foto_na_ocorrencia()
    {
        var entry = CareLogEntry.Log(Tenant, Pet, CareContextType.HotelStay, Stay, CareLogEntryType.Play, null, Now, Now).Value;

        Assert.True(entry.AddPhoto("k1").IsSuccess);
        Assert.Single(entry.PhotoKeys);
        Assert.True(entry.RemovePhoto("k1").IsSuccess);
        Assert.Empty(entry.PhotoKeys);
        Assert.True(entry.RemovePhoto("k1").IsFailure); // já removida
    }
}
