using Demo.VatCalculator.Core.VatCalculation.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Demo.VatCalculator.Api.ErrorHandling;

public sealed class ErrorProblemDetails : ProblemDetails
{
    public IReadOnlyDictionary<string, IReadOnlyList<VatCalculationError>> Errors { get; init; } =
        new Dictionary<string, IReadOnlyList<VatCalculationError>>(StringComparer.Ordinal);
}