using PetHotel.Booking.Domain.Accommodations;
using PetHotel.SharedKernel;

namespace PetHotel.Booking.UnitTests;

public class AccommodationTests
{
    private static readonly TenantId Tenant = TenantId.New();

    [Fact]
    public void Criar_acomodacao_valida_fica_disponivel()
    {
        var result = Accommodation.Create(Tenant, "Suíte 1", 120m, 3);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsAvailable);
        Assert.Equal(120m, result.Value.DailyRate);
        Assert.Equal(3, result.Value.Capacity);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_sem_nome_falha(string name)
    {
        var result = Accommodation.Create(Tenant, name, 100m, 1);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void Criar_com_diaria_negativa_falha()
    {
        var result = Accommodation.Create(Tenant, "Box", -10m, 1);

        Assert.True(result.IsFailure);
        Assert.Equal("accommodation.daily_rate_invalid", result.Error.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Criar_com_capacidade_invalida_falha(int capacity)
    {
        var result = Accommodation.Create(Tenant, "Box", 80m, capacity);

        Assert.True(result.IsFailure);
        Assert.Equal("accommodation.capacity_invalid", result.Error.Code);
    }

    [Fact]
    public void Desativar_torna_indisponivel()
    {
        var accommodation = Accommodation.Create(Tenant, "Box", 80m, 1).Value;

        Assert.True(accommodation.Deactivate().IsSuccess);
        Assert.False(accommodation.IsAvailable);
        Assert.True(accommodation.Deactivate().IsFailure);
    }

    [Fact]
    public void Reativar_volta_a_ficar_disponivel()
    {
        var accommodation = Accommodation.Create(Tenant, "Box", 80m, 1).Value;
        accommodation.Deactivate();

        Assert.True(accommodation.Activate().IsSuccess);
        Assert.True(accommodation.IsAvailable);
        Assert.True(accommodation.Activate().IsFailure); // já ativa
    }

    [Fact]
    public void Editar_atualiza_nome_e_diaria()
    {
        var accommodation = Accommodation.Create(Tenant, "Box", 80m, 1).Value;

        var result = accommodation.Update(" Suíte Premium ", 250m, 4);

        Assert.True(result.IsSuccess);
        Assert.Equal("Suíte Premium", accommodation.Name); // trim
        Assert.Equal(250m, accommodation.DailyRate);
        Assert.Equal(4, accommodation.Capacity);
    }

    [Fact]
    public void Editar_com_diaria_negativa_falha_e_preserva_estado()
    {
        var accommodation = Accommodation.Create(Tenant, "Box", 80m, 1).Value;

        var result = accommodation.Update("Box", -5m, 1);

        Assert.True(result.IsFailure);
        Assert.Equal("accommodation.daily_rate_invalid", result.Error.Code);
        Assert.Equal(80m, accommodation.DailyRate);
    }

    [Fact]
    public void Editar_com_capacidade_invalida_falha()
    {
        var accommodation = Accommodation.Create(Tenant, "Box", 80m, 2).Value;

        var result = accommodation.Update("Box", 80m, 0);

        Assert.True(result.IsFailure);
        Assert.Equal("accommodation.capacity_invalid", result.Error.Code);
        Assert.Equal(2, accommodation.Capacity); // preserva
    }
}
