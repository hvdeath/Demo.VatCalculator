using Demo.VatCalculator.Contracts.VatCalculation;
using Demo.VatCalculator.Core.VatCalculation.Dtos;

namespace Demo.VatCalculator.Core.VatCalculation.Services;

public interface IVatCalculationService
{
    VatCalculationResult Execute(VatCalculationRequest request);
}
