using PetHotel.Registry.Domain.Packs;
using PetHotel.Registry.Domain.Packs.Events;
using PetHotel.Registry.Domain.Pets;
using PetHotel.SharedKernel;

namespace PetHotel.Registry.UnitTests;

public class PackTests
{
    private static readonly TenantId Tenant = TenantId.New();

    [Fact]
    public void Criar_matilha_valida_normaliza_e_deduplica_membros()
    {
        var pet = PetId.New();

        var result = Pack.Create(Tenant, " Matilha Calma ", "  ", [pet, pet]);

        Assert.True(result.IsSuccess);
        Assert.Equal("Matilha Calma", result.Value.Name); // trim
        Assert.Null(result.Value.Notes); // em branco vira null
        Assert.Single(result.Value.Members); // duplicata removida
        Assert.Equal(pet.Value, result.Value.Members[0].PetId);
    }

    [Fact]
    public void Criar_matilha_sem_nome_falha()
    {
        var result = Pack.Create(Tenant, "  ", null, null);

        Assert.True(result.IsFailure);
        Assert.Equal("pack.name_required", result.Error.Code);
    }

    [Fact]
    public void Editar_substitui_composicao_e_dados()
    {
        var pack = Pack.Create(Tenant, "Original", null, [PetId.New()]).Value;
        var novos = new[] { PetId.New(), PetId.New() };

        var result = pack.Update("Editada", "observação", novos);

        Assert.True(result.IsSuccess);
        Assert.Equal("Editada", pack.Name);
        Assert.Equal("observação", pack.Notes);
        Assert.Equal(2, pack.Members.Count);
    }

    [Fact]
    public void Excluir_levanta_evento()
    {
        var pack = Pack.Create(Tenant, "Matilha", null, null).Value;

        pack.Delete();

        var evt = Assert.Single(pack.DomainEvents, e => e is PackDeleted);
        Assert.Equal(pack.Id, ((PackDeleted)evt).PackId);
    }

    [Fact]
    public void Compatibilidade_sinaliza_reativo_e_pouco_sociavel()
    {
        var flags = PackCompatibility.Evaluate(BehaviorLevel.Low, BehaviorLevel.High);

        Assert.Contains(PackCompatibilityFlag.Reactive, flags);
        Assert.Contains(PackCompatibilityFlag.LowSociability, flags);
    }

    [Fact]
    public void Compatibilidade_sem_sinais_para_pet_calmo_ou_sem_avaliacao()
    {
        Assert.Empty(PackCompatibility.Evaluate(BehaviorLevel.High, BehaviorLevel.Low)); // sociável e pouco reativo
        Assert.Empty(PackCompatibility.Evaluate(null, null)); // sem avaliação não sinaliza
    }
}
