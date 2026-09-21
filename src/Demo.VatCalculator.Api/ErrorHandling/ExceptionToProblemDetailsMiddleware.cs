using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Demo.VatCalculator.Api.ErrorHandling;

public sealed partial class ExceptionToProblemDetailsMiddleware(RequestDelegate next, ILogger<ExceptionToProblemDetailsMiddleware> logger)
{
    private static readonly JsonSerializerOptions ProblemJsonOptions = new(JsonSerializerDefaults.Web);

    [GeneratedRegex(@"The JSON property '([^']+)' could not be mapped to any \.NET member")]
    private static partial Regex UnknownPropertyPattern();

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex) when (ex is BadHttpRequestException or JsonException or NotSupportedException)
        {
            var statusCode = ex is BadHttpRequestException badRequest
                ? badRequest.StatusCode
                : StatusCodes.Status400BadRequest;

            var problem = TryGetUnknownProperty(ex, out var property)
                ? (ProblemDetails)ProblemFactory.UnknownField(property)
                : ProblemFactory.MalformedBody(UnwrapMessage(ex));

            logger.LogWarning(ex, "Request parsing failed: {Message}", ex.Message);
            await WriteProblemAsync(context, problem, statusCode);
        }
        catch (Exception ex)
        {
            // Unexpected exception: log and return a generic 500 ProblemDetails
            logger.LogError(ex, "Unhandled exception while processing request");
            var problem = ProblemFactory.InternalServerError("An unexpected error occurred.");
            await WriteProblemAsync(context, problem, StatusCodes.Status500InternalServerError);
        }
    }

    private static string UnwrapMessage(Exception exception) =>
        (exception as BadHttpRequestException)?.InnerException?.Message ?? exception.Message;

    private static bool TryGetUnknownProperty(Exception exception, out string property)
    {
        var inner = (exception as BadHttpRequestException)?.InnerException;
        var candidate = inner ?? exception;

        var match = UnknownPropertyPattern().Match(candidate.Message);
        property = match.Success ? match.Groups[1].Value : string.Empty;
        return match.Success;
    }

    private static async Task WriteProblemAsync(
        HttpContext context,
        ProblemDetails problem,
        int statusCode)
    {
        problem.Status = statusCode;
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = ProblemFactory.ProblemJsonMediaType;
        await JsonSerializer.SerializeAsync(
            context.Response.Body,
            problem,
            problem.GetType(),
            ProblemJsonOptions);
    }
}
