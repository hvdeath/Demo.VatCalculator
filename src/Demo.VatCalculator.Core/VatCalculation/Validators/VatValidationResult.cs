using Demo.VatCalculator.Core.VatCalculation.Dtos;

namespace Demo.VatCalculator.Core.VatCalculation.Validators;

public sealed class VatValidationResult(bool isValid, IReadOnlyDictionary<string, IReadOnlyList<VatCalculationError>>? errors)
{
    public bool IsValid { get; } = isValid;

    public IReadOnlyDictionary<string, IReadOnlyList<VatCalculationError>>? Errors { get; } = errors;

    public static VatValidationResult Valid { get; } = new(true, null);

    public static VatValidationResult Invalid(
        IReadOnlyDictionary<string, IReadOnlyList<VatCalculationError>> errors) =>
        new(false, errors);
}