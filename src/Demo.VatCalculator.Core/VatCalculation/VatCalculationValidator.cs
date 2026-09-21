namespace Demo.VatCalculator.Core.VatCalculation;

public static class VatCalculationValidator
{
    public static readonly int[] SupportedRates =
        Enum.GetValues<VatPercent>().Select(p => (int)p).ToArray();

    public static VatValidationResult Validate(VatCalculationRequest request)
    {
        var errors = new Dictionary<string, List<VatCalculationError>>();

        if (!SupportedRates.Contains(request.Rate))
        {
            Add(errors, "rate", "vat.rateNotSupported",
                "Rate must be one of 10, 13, or 20 percent.");
        }

        var provided = new List<(string Field, decimal Amount)>();
        if (request.Net is { } net)
        {
            provided.Add(("net", net));
        }

        if (request.Gross is { } gross)
        {
            provided.Add(("gross", gross));
        }

        if (request.Vat is { } vat)
        {
            provided.Add(("vat", vat));
        }

        if (provided.Count == 0)
        {
            Add(errors, "amount", "vat.noAmount",
                "Exactly one amount (net, gross, or vat) must be provided.");
        }
        else
        {
            if (provided.Count > 1)
            {
                foreach (var (field, _) in provided)
                {
                    Add(errors, field, "vat.multipleInputs",
                        "Only one amount may be provided; more than one amount is ambiguous.");
                }
            }

            foreach (var (field, amount) in provided)
            {
                if (amount <= 0m)
                {
                    Add(errors, field, "vat.amountNotPositive", "Amount must be greater than zero.");
                }
                else if (Money.HasExcessivePrecision(amount))
                {
                    Add(errors, field, "vat.excessivePrecision",
                        $"Amount must have at most {MoneyConstants.Scale} decimal places.");
                }
                else if (amount > MoneyConstants.MaxAmount)
                {
                    Add(errors, field, "vat.amountTooLarge",
                        $"Amount must not exceed {MoneyConstants.MaxAmount:0.00}.");
                }
            }
        }

        return errors.Count == 0
            ? VatValidationResult.Valid
            : VatValidationResult.Invalid(AsReadOnly(errors));
    }

    private static void Add(
        Dictionary<string, List<VatCalculationError>> errors,
        string field,
        string code,
        string message)
    {
        if (!errors.TryGetValue(field, out var list))
        {
            list = [];
            errors[field] = list;
        }

        list.Add(new VatCalculationError(code, message));
    }

    private static Dictionary<string, IReadOnlyList<VatCalculationError>> AsReadOnly(
        IDictionary<string, List<VatCalculationError>> errors) =>
        errors.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyList<VatCalculationError>)kvp.Value);
}