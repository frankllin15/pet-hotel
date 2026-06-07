namespace PetHotel.SharedKernel;

/// <summary>
/// Resultado de uma operação que pode falhar de forma esperada (docs/02).
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        switch (isSuccess)
        {
            case true when error != Error.None:
                throw new InvalidOperationException("Um resultado de sucesso não pode carregar erro.");
            case false when error == Error.None:
                throw new InvalidOperationException("Um resultado de falha precisa de um erro.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);

    public static Result<T> Success<T>(T value) => new(value, true, Error.None);
    public static Result<T> Failure<T>(Error error) => new(default, false, error);

    // Permite `return Error.NotFound(...);` onde se espera um Result.
    public static implicit operator Result(Error error) => Failure(error);
}

/// <summary>
/// Resultado com valor de retorno em caso de sucesso.
/// </summary>
public class Result<T> : Result
{
    private readonly T? _value;

    protected internal Result(T? value, bool isSuccess, Error error)
        : base(isSuccess, error) => _value = value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Não há valor em um resultado de falha.");

    // Permite `return value;` e `return Error.NotFound(...);`.
    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure<T>(error);
}
