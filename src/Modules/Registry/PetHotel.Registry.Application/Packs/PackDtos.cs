namespace PetHotel.Registry.Application.Packs;

/// <summary>Projeção de leitura de uma matilha com os membros e os alertas de compatibilidade.</summary>
public sealed record PackDto(
    Guid Id,
    string Name,
    string? Notes,
    IReadOnlyList<PackMemberDto> Members,
    bool NeedsAttention,
    DateTimeOffset CreatedAt);

/// <summary>Membro da matilha (leitura) com os sinais de compatibilidade do pet.</summary>
public sealed record PackMemberDto(
    Guid PetId,
    string Name,
    string? Species,
    bool Found,
    IReadOnlyList<string> Flags);

/// <summary>Resumo de matilha para listagem.</summary>
public sealed record PackSummaryDto(
    Guid Id,
    string Name,
    int MemberCount,
    bool NeedsAttention);
