namespace Demo.VatCalculator.Core.VatCalculation;

public sealed record VatCalculationRequest(int Rate, decimal? Net, decimal? Gross, decimal? Vat);