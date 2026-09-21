namespace Demo.VatCalculator.Core.VatCalculation.Dtos;

public sealed record VatCalculationResponse(
    int Rate,
    decimal Net,
    decimal Vat,
    decimal Gross);