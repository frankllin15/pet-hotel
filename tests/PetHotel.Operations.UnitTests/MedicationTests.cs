using PetHotel.Operations.Domain.CareLog;
using PetHotel.Operations.Domain.Medications;
using PetHotel.SharedKernel;

namespace PetHotel.Operations.UnitTests;

public class MedicationTests
{
    private static readonly TenantId Tenant = TenantId.New();
    private static readonly PetReference Pet = new(Guid.NewGuid());
    private static readonly Guid Stay = Guid.NewGuid();
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-06-13T12:00:00Z");

    [Fact]
    public void Administrar_valido_normaliza_e_guarda()
    {
        var result = MedicationAdministration.Administer(
            Tenant, Pet, CareContextType.HotelStay, Stay, " Dipirona ", " 1 comprimido ", Now.AddHours(-2), Now);

        Assert.True(result.IsSuccess);
        Assert.Equal("Dipirona", result.Value.Drug);
        Assert.Equal("1 comprimido", result.Value.Dose);
        Assert.Equal(Now.AddHours(-2), result.Value.AdministeredAt);
    }

    [Fact]
    public void Sem_medicamento_falha()
    {
        var result = MedicationAdministration.Administer(Tenant, Pet, CareContextType.HotelStay, Stay, "  ", "1cp", Now, Now);
        Assert.True(result.IsFailure);
        Assert.Equal("medication.drug_required", result.Error.Code);
    }

    [Fact]
    public void Sem_dose_falha()
    {
        var result = MedicationAdministration.Administer(Tenant, Pet, CareContextType.HotelStay, Stay, "Dipirona", " ", Now, Now);
        Assert.True(result.IsFailure);
        Assert.Equal("medication.dose_required", result.Error.Code);
    }

    [Fact]
    public void Aplicacao_no_futuro_falha()
    {
        var result = MedicationAdministration.Administer(Tenant, Pet, CareContextType.HotelStay, Stay, "Dipirona", "1cp", Now.AddMinutes(5), Now);
        Assert.True(result.IsFailure);
        Assert.Equal("medication.administered_in_future", result.Error.Code);
    }
}
