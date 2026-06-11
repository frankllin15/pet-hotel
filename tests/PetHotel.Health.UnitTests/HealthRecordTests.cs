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
    public void Adicionar_controle_de_parasitas_valido_levanta_evento()
    {
        var record = NewRecord();

        var result = record.AddParasiteTreatment(
            ParasiteTreatmentType.FleaTick, " Bravecto ", Today.AddDays(-10), Today.AddMonths(3), Today);

        Assert.True(result.IsSuccess);
        var treatment = Assert.Single(record.ParasiteTreatments);
        Assert.Equal("Bravecto", treatment.ProductName); // trim
        Assert.True(treatment.IsUpToDateOn(Today));
        Assert.Contains(record.DomainEvents, e => e is ParasiteTreatmentRegistered);
    }

    [Fact]
    public void Controle_de_parasitas_aplicado_no_futuro_falha()
    {
        var record = NewRecord();

        var result = record.AddParasiteTreatment(
            ParasiteTreatmentType.Dewormer, null, Today.AddDays(1), null, Today);

        Assert.True(result.IsFailure);
        Assert.Equal("parasite_treatment.applied_future", result.Error.Code);
    }

    [Fact]
    public void Controle_de_parasitas_com_proxima_dose_invalida_falha()
    {
        var record = NewRecord();

        var result = record.AddParasiteTreatment(
            ParasiteTreatmentType.Dewormer, null, Today.AddDays(-5), Today.AddDays(-5), Today);

        Assert.True(result.IsFailure);
        Assert.Equal("parasite_treatment.invalid_next_due", result.Error.Code);
    }

    [Fact]
    public void Controle_de_parasitas_vencido_ou_sem_proxima_dose()
    {
        var record = NewRecord();
        record.AddParasiteTreatment(ParasiteTreatmentType.FleaTick, null, Today.AddMonths(-4), Today.AddMonths(-1), Today);
        record.AddParasiteTreatment(ParasiteTreatmentType.Dewormer, null, Today.AddMonths(-1), null, Today);

        var treatments = record.ParasiteTreatments.ToList();

        Assert.False(treatments[0].IsUpToDateOn(Today)); // próxima dose já passou
        Assert.Null(treatments[1].IsUpToDateOn(Today)); // sem próxima dose informada
    }

    [Fact]
    public void Definir_veterinario_substitui_o_anterior()
    {
        var record = NewRecord();
        record.SetVetContact(VetContact.Create("Dra. Ana", "11 99999-0000", "Clínica Patas").Value);

        record.SetVetContact(VetContact.Create(" Dr. Bruno ", " 11 98888-0000 ", null).Value);

        Assert.NotNull(record.VetContact);
        Assert.Equal("Dr. Bruno", record.VetContact.Name); // trim
        Assert.Equal("11 98888-0000", record.VetContact.Phone);
        Assert.Null(record.VetContact.Clinic);
    }

    [Fact]
    public void Criar_veterinario_sem_nome_ou_telefone_falha()
    {
        Assert.Equal("vet_contact.name_required", VetContact.Create("  ", "11 99999-0000", null).Error.Code);
        Assert.Equal("vet_contact.phone_required", VetContact.Create("Dra. Ana", "", null).Error.Code);
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
