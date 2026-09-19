using Demo.VatCalculator.Core.VatCalculation;
using Microsoft.AspNetCore.Mvc;

namespace Demo.VatCalculator.Api.ErrorHandling;

internal static class ProblemFactory
{
    public const string ValidationFailedType = "urn:demo-vat:validation-failed";
    public const string MalformedBodyType = "urn:demo-vat:malformed-body";
    public const string UnknownFieldType = "urn:demo-vat:unknown-field";
    public const string ProblemJsonMediaType = "application/problem+json";

    public static ProblemDetails ValidationFailed(
        IReadOnlyDictionary<string, IReadOnlyList<VatCalculationError>> errors) =>
        new ErrorProblemDetails
        {
            Type = ValidationFailedType,
            Title = "Validation failed.",
            Status = StatusCodes.Status400BadRequest,
            Detail = "The request did not pass validation. See the 'errors' member for field-level details.",
            Errors = errors,
        };

    public static ProblemDetails MalformedBody(string detail) =>
        new ErrorProblemDetails
        {
            Type = MalformedBodyType,
            Title = "Malformed request body.",
            Status = StatusCodes.Status400BadRequest,
            Detail = detail,
            Errors = new Dictionary<string, IReadOnlyList<VatCalculationError>>
            {
                ["request"] = [new VatCalculationError("vat.malformedBody", detail)],
            },
        };

    public static ProblemDetails UnknownField(string property) =>
        new ErrorProblemDetails
        {
            Type = UnknownFieldType,
            Title = "Unknown field.",
            Status = StatusCodes.Status400BadRequest,
            Detail = $"The field '{property}' is not part of the API contract for this request.",
            Errors = new Dictionary<string, IReadOnlyList<VatCalculationError>>
            {
                [property] =
                [
                    new VatCalculationError("vat.unknownField", $"The field '{property}' is not supported."),
                ],
            },
        };
}