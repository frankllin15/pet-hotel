using PetHotel.Health.Domain.HealthRecords;
using PetHotel.Health.Domain.HealthRecords.Events;
using PetHotel.SharedKernel;

namespace PetHotel.Health.UnitTests;

public class HealthRecordTests
{
    private static readonly TenantId Tenant = TenantId.New();
    private static readonly PetReference Pet = new(Guid.NewGuid());
    private static readonly DateOnly Today = new(2026, 6, 7);

    private static HealthRecord NewRecord() => HealthRecord.Open(Tenant, Pet).Value;

    [Fact]
    public void Abrir_ficha_valida()
    {
        var result = HealthRecord.Open(Tenant, Pet);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Vaccinations);
    }

    [Fact]
    public void Adicionar_vacina_valida_levanta_evento()
    {
        var record = NewRecord();

        var result = record.AddVaccination(VaccineType.Rabies, Today.AddYears(-1), Today.AddYears(1), Today);

        Assert.True(result.IsSuccess);
        Assert.Single(record.Vaccinations);
        Assert.Contains(record.DomainEvents, e => e is VaccinationRegistered);
    }

    [Fact]
    public void Adicionar_vacina_aplicada_no_futuro_falha()
    {
        var record = NewRecord();

        var result = record.AddVaccination(VaccineType.Rabies, Today.AddDays(1), Today.AddYears(1), Today);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void Adicionar_vacina_com_validade_invalida_falha()
    {
        var record = NewRecord();

        var result = record.AddVaccination(VaccineType.Rabies, Today.AddYears(-1), Today.AddYears(-1), Today);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void Sem_antirrabica_nao_esta_apto()
    {
        var record = NewRecord();
        record.AddVaccination(VaccineType.Distemper, Today.AddMonths(-1), Today.AddYears(1), Today);

        var clearance = record.GetClearance(Today);

        Assert.False(clearance.IsCleared);
        Assert.Contains(VaccineType.Rabies, clearance.Pendencies);
    }

    [Fact]
    public void Antirrabica_vencida_nao_esta_apto()
    {
        var record = NewRecord();
        // Aplicada há 2 anos, validade de 1 ano: vencida hoje.
        record.AddVaccination(VaccineType.Rabies, Today.AddYears(-2), Today.AddYears(-1), Today);

        var clearance = record.GetClearance(Today);

        Assert.False(clearance.IsCleared);
        Assert.Contains(VaccineType.Rabies, clearance.Pendencies);
    }

    [Fact]
    public void Antirrabica_vigente_esta_apto()
    {
        var record = NewRecord();
        record.AddVaccination(VaccineType.Rabies, Today.AddMonths(-1), Today.AddYears(1), Today);

        var clearance = record.GetClearance(Today);

        Assert.True(clearance.IsCleared);
        Assert.Empty(clearance.Pendencies);
    }
}
