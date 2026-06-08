namespace PetHotel.SharedKernel;

/// <summary>
/// Categoria de falha esperada, mapeada para status HTTP no adaptador da API (docs/02).
/// </summary>
public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Forbidden,
    Unauthorized,
    Unexpected
}

/// <summary>
/// Falha esperada (validação, regra de negócio, não encontrado, conflito).
/// Fluxo de negócio usa <see cref="Result"/>/<see cref="Error"/>, nunca exceção (docs/02).
/// </summary>
public readonly record struct Error(string Code, string Message, ErrorType Type)
{
    /// <summary>Ausência de erro — usada por resultados de sucesso.</summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Unexpected);

    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);
    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);
    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);
    public static Error Forbidden(string code, string message) => new(code, message, ErrorType.Forbidden);
    public static Error Unauthorized(string code, string message) => new(code, message, ErrorType.Unauthorized);
    public static Error Unexpected(string code, string message) => new(code, message, ErrorType.Unexpected);
}
