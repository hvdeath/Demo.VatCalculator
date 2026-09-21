namespace Demo.VatCalculator.Core.VatCalculation;

public sealed class VatCalculationService : IVatCalculationService
{
    public VatCalculationResult Execute(VatCalculationRequest request)
    {
        var validation = VatCalculationValidator.Validate(request);
        if (!validation.IsValid)
        {
            return VatCalculationResult.Invalid(validation.Errors!);
        }

        var (kind, amount) = SelectAmount(request);
        var response = VatAmountCalculator.Calculate(amount, kind, (VatPercent)request.Rate);
        return VatCalculationResult.Valid(response);
    }

    private static (VatAmountKind Kind, decimal Amount) SelectAmount(VatCalculationRequest request) =>
        request.Net is { } net
            ? (VatAmountKind.Net, net)
            : request.Gross is { } gross
                ? (VatAmountKind.Gross, gross)
                : (VatAmountKind.Vat, request.Vat!.Value);
}
