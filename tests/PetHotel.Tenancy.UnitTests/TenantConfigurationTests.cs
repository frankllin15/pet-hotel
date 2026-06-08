using PetHotel.SharedKernel;
using PetHotel.Tenancy.Domain.Configuration;

namespace PetHotel.Tenancy.UnitTests;

public class TenantConfigurationTests
{
    private static readonly TenantId Tenant = TenantId.New();

    private static AccommodationType Suite() => AccommodationType.Create("Suíte", 2, 120m).Value;

    [Fact]
    public void Defaults_iniciam_sem_setup_concluido_e_com_antirrabica()
    {
        var config = TenantConfiguration.CreateDefaults(Tenant);

        Assert.False(config.SetupCompleted);
        Assert.Contains("Rabies", config.RequiredVaccines);
        Assert.Empty(config.AccommodationTypes);
    }

    [Fact]
    public void Update_com_tipo_de_acomodacao_conclui_o_setup()
    {
        var config = TenantConfiguration.CreateDefaults(Tenant);

        config.Update([Suite()], ["Rabies"], new TimeOnly(14, 0), new TimeOnly(12, 0));

        Assert.True(config.SetupCompleted);
        Assert.Single(config.AccommodationTypes);
    }

    [Fact]
    public void Update_sem_acomodacao_mantem_setup_incompleto()
    {
        var config = TenantConfiguration.CreateDefaults(Tenant);

        config.Update([], ["Rabies"], new TimeOnly(14, 0), new TimeOnly(12, 0));

        Assert.False(config.SetupCompleted);
    }

    [Theory]
    [InlineData("", 2, 100)]
    [InlineData("Box", 0, 100)]
    [InlineData("Box", 2, -1)]
    public void AccommodationType_invalido_falha(string name, int capacity, decimal price)
    {
        var result = AccommodationType.Create(name, capacity, price);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void AccommodationType_valido_normaliza_nome()
    {
        var result = AccommodationType.Create("  Canil  ", 1, 80m);

        Assert.True(result.IsSuccess);
        Assert.Equal("Canil", result.Value.Name);
    }
}
