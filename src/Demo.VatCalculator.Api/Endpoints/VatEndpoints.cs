using Demo.VatCalculator.Api.ErrorHandling;
using Demo.VatCalculator.Core.VatCalculation;

namespace Demo.VatCalculator.Api.Endpoints;

public static class VatEndpoints
{
    public static void MapVatEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/vat");

        group.MapPost(
            "/calculate",
            (VatCalculationRequest request) =>
            {
                var result = VatCalculationService.Execute(request);
                return result.IsValid
                    ? Results.Ok(result.Response)
                    : Results.Problem(ProblemFactory.ValidationFailed(result.Errors!));
            });
    }
}