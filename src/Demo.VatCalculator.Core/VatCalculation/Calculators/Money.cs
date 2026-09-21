namespace Demo.VatCalculator.Core.VatCalculation.Calculators;

public static class Money
{
    public static decimal Round(decimal value) =>
        decimal.Round(value, MoneyConstants.Scale, MidpointRounding.AwayFromZero);

    public static bool HasExcessivePrecision(decimal value) =>
        decimal.Round(value, MoneyConstants.Scale) != value;
}
