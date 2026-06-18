using PetHotel.Operations.Domain.Tasks;
using PetHotel.SharedKernel;

namespace PetHotel.Operations.UnitTests;

public class OperationalTaskTests
{
    private static readonly TenantId Tenant = TenantId.New();
    private static readonly DateOnly Day = new(2026, 6, 14);
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-06-14T12:00:00Z");

    [Fact]
    public void Criar_tarefa_valida_fica_pendente()
    {
        var responsible = Guid.NewGuid();

        var result = OperationalTask.Create(Tenant, " Limpar ala A ", Day, TaskCategory.Cleaning, responsible);

        Assert.True(result.IsSuccess);
        Assert.Equal("Limpar ala A", result.Value.Title); // trim
        Assert.Equal(TaskCategory.Cleaning, result.Value.Category);
        Assert.Equal(responsible, result.Value.AssignedTo);
        Assert.False(result.Value.Done);
        Assert.Null(result.Value.CompletedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_sem_titulo_falha(string title)
    {
        var result = OperationalTask.Create(Tenant, title, Day, TaskCategory.Feeding, null);

        Assert.True(result.IsFailure);
        Assert.Equal("task.title_required", result.Error.Code);
    }

    [Fact]
    public void Guid_vazio_de_responsavel_vira_nulo()
    {
        var result = OperationalTask.Create(Tenant, "Recreação", Day, TaskCategory.Recreation, Guid.Empty);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.AssignedTo);
    }

    [Fact]
    public void Marcar_feita_carimba_conclusao_e_desmarcar_limpa()
    {
        var task = OperationalTask.Create(Tenant, "Limpar", Day, TaskCategory.Cleaning, null).Value;

        task.SetDone(true, Now);
        Assert.True(task.Done);
        Assert.Equal(Now, task.CompletedAt);

        task.SetDone(false, Now);
        Assert.False(task.Done);
        Assert.Null(task.CompletedAt);
    }

    [Fact]
    public void Editar_atualiza_titulo_categoria_e_responsavel()
    {
        var task = OperationalTask.Create(Tenant, "Limpar", Day, TaskCategory.Cleaning, null).Value;
        var responsible = Guid.NewGuid();

        var result = task.Update(" Alimentação manhã ", TaskCategory.Feeding, responsible);

        Assert.True(result.IsSuccess);
        Assert.Equal("Alimentação manhã", task.Title);
        Assert.Equal(TaskCategory.Feeding, task.Category);
        Assert.Equal(responsible, task.AssignedTo);
    }
}
