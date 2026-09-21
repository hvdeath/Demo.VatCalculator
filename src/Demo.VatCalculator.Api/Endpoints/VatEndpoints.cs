using Demo.VatCalculator.Api.ErrorHandling;
using Demo.VatCalculator.Core.VatCalculation.Calculators;
using Demo.VatCalculator.Core.VatCalculation.Dtos;
using Demo.VatCalculator.Core.VatCalculation.Services;

namespace Demo.VatCalculator.Api.Endpoints;

public static class VatEndpoints
{
    public static void MapVatEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/vat");
        group.MapPost(
            "/calculate",
            (VatCalculationRequest request, IVatCalculationService service) =>
            {
                var result = service.Execute(request);
                return result.IsValid
                    ? Results.Ok(result.Response)
                    : Results.Problem(ProblemFactory.ValidationFailed(result.Errors!));
            })
            .WithDescription($"Calculate VAT for a given request. \n\nAllowed rate values: {string.Join(", ", Enum.GetValues<VatPercent>().Select(p => (int)p).ToArray())}.");
    }
}