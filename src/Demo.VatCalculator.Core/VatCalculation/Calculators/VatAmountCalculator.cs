using Demo.VatCalculator.Core.VatCalculation.Dtos;

namespace Demo.VatCalculator.Core.VatCalculation.Calculators;

public static class VatAmountCalculator
{
    public static VatCalculationResponse Calculate(decimal amount, VatAmountKind kind, VatPercent rate)
    {
        var ratePercent = (int)rate;
        var factor = ratePercent / 100m;

        return kind switch
        {
            VatAmountKind.Net => FromNet(amount, factor, ratePercent),
            VatAmountKind.Gross => FromGross(amount, factor, ratePercent),
            VatAmountKind.Vat => FromVat(amount, factor, ratePercent),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
        };
    }

    private static VatCalculationResponse FromNet(decimal net, decimal factor, int ratePercent)
    {
        var vat = Money.Round(net * factor);
        return new(ratePercent, net, vat, net + vat);
    }

    private static VatCalculationResponse FromGross(decimal gross, decimal factor, int ratePercent)
    {
        var net = Money.Round(gross / (1m + factor));
        return new(ratePercent, net, gross - net, gross);
    }

    private static VatCalculationResponse FromVat(decimal vat, decimal factor, int ratePercent)
    {
        var net = Money.Round(vat / factor);
        return new(ratePercent, net, vat, net + vat);
    }
}