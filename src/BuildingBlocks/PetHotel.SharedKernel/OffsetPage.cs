namespace PetHotel.SharedKernel;

/// <summary>
/// Página de uma lista paginada por offset (page/pageSize) com total de itens (docs/04).
/// Alternativa ao <see cref="CursorPage{T}"/> quando o cliente precisa navegar por número de
/// página e saber o total (ex.: tabela de reservas com "Página X de Y").
/// </summary>
public sealed record OffsetPage<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize)
{
    /// <summary>Total de páginas (>= 1 quando há itens; 0 quando vazio).</summary>
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(Total / (double)PageSize);

    public static OffsetPage<T> Empty(int page, int pageSize) => new([], 0, page, pageSize);
}
