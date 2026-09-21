namespace Demo.VatCalculator.Core.VatCalculation.Dtos;

public sealed record VatCalculationResult(
    VatCalculationResponse? Response,
    IReadOnlyDictionary<string, IReadOnlyList<VatCalculationError>>? Errors)
{
    public bool IsValid => Errors is null;

    public static VatCalculationResult Valid(VatCalculationResponse response) =>
        new(response, null);

    public static VatCalculationResult Invalid(
        IReadOnlyDictionary<string, IReadOnlyList<VatCalculationError>> errors) =>
        new(null, errors);
}