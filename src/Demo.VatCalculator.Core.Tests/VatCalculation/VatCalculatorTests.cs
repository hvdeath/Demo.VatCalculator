using Demo.VatCalculator.Core.VatCalculation;

namespace Demo.VatCalculator.Core.Tests.VatCalculation;

public sealed class VatAmountCalculatorTests
{
    public static TheoryData<int, VatAmountKind, decimal, decimal, decimal, decimal> CalculateData =>
        new()
        {
            { 10, VatAmountKind.Net, 100m, 100m, 10m, 110m },
            { 10, VatAmountKind.Net, 33.33m, 33.33m, 3.33m, 36.66m },
            { 10, VatAmountKind.Net, 0.05m, 0.05m, 0.01m, 0.06m },
            { 10, VatAmountKind.Net, 0.01m, 0.01m, 0.00m, 0.01m },
            { 13, VatAmountKind.Net, 100m, 100m, 13m, 113m },
            { 13, VatAmountKind.Net, 33.33m, 33.33m, 4.33m, 37.66m },
            { 20, VatAmountKind.Net, 100m, 100m, 20m, 120m },
            { 20, VatAmountKind.Net, 33.33m, 33.33m, 6.67m, 40.00m },

            { 10, VatAmountKind.Gross, 110m, 100m, 10m, 110m },
            { 10, VatAmountKind.Gross, 36.66m, 33.33m, 3.33m, 36.66m },
            { 13, VatAmountKind.Gross, 113m, 100m, 13m, 113m },
            { 13, VatAmountKind.Gross, 10m, 8.85m, 1.15m, 10m },
            { 20, VatAmountKind.Gross, 120m, 100m, 20m, 120m },
            { 20, VatAmountKind.Gross, 40m, 33.33m, 6.67m, 40m },
            { 20, VatAmountKind.Gross, 39.99m, 33.33m, 6.66m, 39.99m },
            { 20, VatAmountKind.Gross, 0.03m, 0.03m, 0.00m, 0.03m },
            { 20, VatAmountKind.Gross, 1_000_000_000m, 833_333_333.33m, 166_666_666.67m, 1_000_000_000m },

            { 10, VatAmountKind.Vat, 10m, 100m, 10m, 110m },
            { 10, VatAmountKind.Vat, 3.33m, 33.30m, 3.33m, 36.63m },
            { 13, VatAmountKind.Vat, 13m, 100m, 13m, 113m },
            { 13, VatAmountKind.Vat, 1.15m, 8.85m, 1.15m, 10m },
            { 20, VatAmountKind.Vat, 20m, 100m, 20m, 120m },
            { 20, VatAmountKind.Vat, 6.67m, 33.35m, 6.67m, 40.02m },
            { 20, VatAmountKind.Vat, 0.05m, 0.25m, 0.05m, 0.30m },
        };

    [Theory]
    [MemberData(nameof(CalculateData))]
    public void Calculate_returns_expected_amounts(
        int ratePercent,
        VatAmountKind kind,
        decimal input,
        decimal expectedNet,
        decimal expectedVat,
        decimal expectedGross)
    {
        var result = VatAmountCalculator.Calculate(input, kind, (VatPercent)ratePercent);

        Assert.Equal(expectedNet, result.Net);
        Assert.Equal(expectedVat, result.Vat);
        Assert.Equal(expectedGross, result.Gross);
        Assert.Equal(ratePercent, result.Rate);
    }

    [Fact]
    public void Calculate_always_satisfies_net_plus_vat_equals_gross()
    {
        for (var rate = VatPercent.Ten; rate <= VatPercent.Twenty; rate++)
        {
            for (var amount = 0.01m; amount <= 100m; amount += 0.01m)
            {
                var fromNet = VatAmountCalculator.Calculate(amount, VatAmountKind.Net, rate);
                Assert.Equal(fromNet.Net + fromNet.Vat, fromNet.Gross);
                Assert.Equal(amount, fromNet.Net);

                var fromGross = VatAmountCalculator.Calculate(amount, VatAmountKind.Gross, rate);
                Assert.Equal(fromGross.Net + fromGross.Vat, fromGross.Gross);
                Assert.Equal(amount, fromGross.Gross);

                var fromVat = VatAmountCalculator.Calculate(amount, VatAmountKind.Vat, rate);
                Assert.Equal(fromVat.Net + fromVat.Vat, fromVat.Gross);
                Assert.Equal(amount, fromVat.Vat);
            }
        }
    }
}