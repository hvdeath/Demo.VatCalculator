namespace Demo.VatCalculator.Core.VatCalculation;

public static class Money
{
    public static decimal Round(decimal value) =>
        decimal.Round(value, MoneyConstants.Scale, MidpointRounding.AwayFromZero);

    public static bool HasExcessivePrecision(decimal value) =>
        decimal.Round(value, MoneyConstants.Scale) != value;
}
