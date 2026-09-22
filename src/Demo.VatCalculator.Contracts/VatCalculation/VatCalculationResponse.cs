namespace Demo.VatCalculator.Contracts.VatCalculation;

public sealed record VatCalculationResponse(
    int Rate,
    decimal Net,
    decimal Vat,
    decimal Gross);