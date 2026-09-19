namespace Demo.VatCalculator.Core.VatCalculation;

public sealed record VatCalculationResponse(
    int Rate,
    decimal Net,
    decimal Vat,
    decimal Gross);