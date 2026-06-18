using PetHotel.SharedKernel;

namespace PetHotel.Operations.Domain.Tasks;

/// <summary>
/// Tarefa operacional do dia (limpeza, alimentação, recreação) do hotel — NÃO atrelada a um pet.
/// Agregado tenant-scoped e auditável. Pode ter um responsável (usuário) e um estado feito/não-feito.
/// </summary>
public sealed class OperationalTask : AggregateRoot<OperationalTaskId>, IHasTenant, IAuditable
{
    public TenantId TenantId { get; private set; }
    public string Title { get; private set; } = null!;
    /// <summary>Dia ao qual a tarefa pertence (a escala é por data).</summary>
    public DateOnly Date { get; private set; }
    public TaskCategory Category { get; private set; }
    /// <summary>Usuário responsável (Id do diretório). Nulo = sem responsável definido.</summary>
    public Guid? AssignedTo { get; private set; }
    public bool Done { get; private set; }
    /// <summary>Momento em que foi concluída (quem concluiu fica na auditoria). Nulo se pendente.</summary>
    public DateTimeOffset? CompletedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    private OperationalTask() { } // EF

    private OperationalTask(
        OperationalTaskId id, TenantId tenantId, string title, DateOnly date, TaskCategory category, Guid? assignedTo)
        : base(id)
    {
        TenantId = tenantId;
        Title = title;
        Date = date;
        Category = category;
        AssignedTo = assignedTo;
    }

    public static Result<OperationalTask> Create(
        TenantId tenantId, string? title, DateOnly date, TaskCategory category, Guid? assignedTo)
    {
        if (tenantId.Value == Guid.Empty)
        {
            return Error.Validation("task.tenant_required", "Tenant é obrigatório.");
        }

        var validation = Validate(title, category);
        if (validation.IsFailure)
        {
            return validation.Error;
        }

        return new OperationalTask(
            OperationalTaskId.New(), tenantId, title!.Trim(), date, category, Normalize(assignedTo));
    }

    /// <summary>Edita título, categoria e responsável (a conclusão é gerida à parte por <see cref="SetDone"/>).</summary>
    public Result Update(string? title, TaskCategory category, Guid? assignedTo)
    {
        var validation = Validate(title, category);
        if (validation.IsFailure)
        {
            return validation.Error;
        }

        Title = title!.Trim();
        Category = category;
        AssignedTo = Normalize(assignedTo);
        return Result.Success();
    }

    /// <summary>Marca como feita/não-feita; carimba (ou limpa) o momento da conclusão.</summary>
    public void SetDone(bool done, DateTimeOffset now)
    {
        Done = done;
        CompletedAt = done ? now : null;
    }

    private static Result Validate(string? title, TaskCategory category)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Error.Validation("task.title_required", "O título da tarefa é obrigatório.");
        }

        if (!Enum.IsDefined(category))
        {
            return Error.Validation("task.category_invalid", "Categoria inválida.");
        }

        return Result.Success();
    }

    private static Guid? Normalize(Guid? assignedTo) =>
        assignedTo is { } id && id != Guid.Empty ? id : null;
}
