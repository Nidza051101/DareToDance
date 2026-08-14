using ErrorOr;

namespace DareToDance.Api.Common.Results;

public static class ErrorOrProblemExtensions
{
    public static IResult ToProblem(this List<Error> errors)
    {
        if (errors.Count == 0)
        {
            return Microsoft.AspNetCore.Http.Results.Problem();
        }

        if (errors.TrueForAll(error => error.Type == ErrorType.Validation))
        {
            return ValidationProblem(errors);
        }

        return Problem(errors[0]);
    }

    private static IResult Problem(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,
        };

        return Microsoft.AspNetCore.Http.Results.Problem(
            statusCode: statusCode,
            title: error.Code,
            detail: error.Description);
    }

    private static IResult ValidationProblem(List<Error> errors)
    {
        var errorsByProperty = errors
            .GroupBy(error => error.Code)
            .ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray());

        return Microsoft.AspNetCore.Http.Results.ValidationProblem(errorsByProperty);
    }
}
