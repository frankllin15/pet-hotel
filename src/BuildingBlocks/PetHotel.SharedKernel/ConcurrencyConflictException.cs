namespace PetHotel.SharedKernel;

/// <summary>
/// Lançada pela Unit of Work quando o token de concorrência otimista (xmin) detecta
/// escrita concorrente. O handler traduz para <see cref="ErrorType.Conflict"/> e o
/// chamador decide retry (docs/04).
/// </summary>
public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException(string message) : base(message) { }

    public ConcurrencyConflictException(string message, Exception innerException)
        : base(message, innerException) { }
}
