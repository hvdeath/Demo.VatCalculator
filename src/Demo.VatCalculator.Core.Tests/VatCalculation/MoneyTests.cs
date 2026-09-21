using Demo.VatCalculator.Core.VatCalculation;

namespace Demo.VatCalculator.Core.Tests.VatCalculation;

public class MoneyTests
{
    [Fact]
    public void Round_RoundsHalfAwayFromZero()
    {
        Assert.Equal(1.23m, Money.Round(1.225m));
        Assert.Equal(-1.23m, Money.Round(-1.225m));
    }

    [Fact]
    public void HasExcessivePrecision_ReturnsTrueForThreeDecimals()
    {
        Assert.True(Money.HasExcessivePrecision(1.234m));
    }

    [Fact]
    public void HasExcessivePrecision_ReturnsFalseForTwoDecimals()
    {
        Assert.False(Money.HasExcessivePrecision(1.23m));
    }
}
