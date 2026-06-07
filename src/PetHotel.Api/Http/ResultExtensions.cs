using PetHotel.SharedKernel;

namespace PetHotel.Api.Http;

/// <summary>
/// Mapeia <see cref="Result"/>/<see cref="Result{T}"/> para resposta HTTP, traduzindo
/// <see cref="ErrorType"/> em status e ProblemDetails (RFC 9457, docs/02).
/// </summary>
public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult> onSuccess) =>
        result.IsSuccess ? onSuccess(result.Value) : Problem(result.Error);

    public static IResult ToHttpResult(this Result result, IResult onSuccess) =>
        result.IsSuccess ? onSuccess : Problem(result.Error);

    private static IResult Problem(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.Problem(
            detail: error.Message,
            statusCode: statusCode,
            title: error.Code,
            extensions: new Dictionary<string, object?> { ["code"] = error.Code });
    }
}
