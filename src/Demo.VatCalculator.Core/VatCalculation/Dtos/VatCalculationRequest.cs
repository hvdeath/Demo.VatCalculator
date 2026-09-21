namespace Demo.VatCalculator.Core.VatCalculation.Dtos;

public sealed record VatCalculationRequest(int Rate, decimal? Net, decimal? Gross, decimal? Vat);