namespace Demo.VatCalculator.Core.VatCalculation;

public interface IVatCalculationService
{
    VatCalculationResult Execute(VatCalculationRequest request);
}
