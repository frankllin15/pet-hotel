using PetHotel.Booking.Domain.Accommodations;
using PetHotel.SharedKernel;

namespace PetHotel.Booking.UnitTests;

public class AccommodationTests
{
    private static readonly TenantId Tenant = TenantId.New();

    [Fact]
    public void Criar_acomodacao_valida_fica_disponivel()
    {
        var result = Accommodation.Create(Tenant, "Suíte 1");

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsAvailable);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_sem_nome_falha(string name)
    {
        var result = Accommodation.Create(Tenant, name);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void Desativar_torna_indisponivel()
    {
        var accommodation = Accommodation.Create(Tenant, "Box").Value;

        Assert.True(accommodation.Deactivate().IsSuccess);
        Assert.False(accommodation.IsAvailable);
        Assert.True(accommodation.Deactivate().IsFailure);
    }
}
