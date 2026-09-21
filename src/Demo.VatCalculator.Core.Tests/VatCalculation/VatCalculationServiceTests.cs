using Demo.VatCalculator.Core.VatCalculation;

namespace Demo.VatCalculator.Core.Tests.VatCalculation;

public sealed class VatCalculationServiceTests
{
    [Theory]
    [InlineData(10)]
    [InlineData(13)]
    [InlineData(20)]
    public void Execute_returns_calculated_response_for_net_input(int rate)
    {
        var result = VatCalculationService.Execute(new VatCalculationRequest(rate, Net: 200m, null, null));

        Assert.True(result.IsValid);
        Assert.NotNull(result.Response);
        Assert.Equal(200m, result.Response!.Net);
        Assert.Equal(rate, result.Response.Rate);
        Assert.Equal(result.Response.Net + result.Response.Vat, result.Response.Gross);
    }

    [Fact]
    public void Execute_calculates_from_gross_when_only_gross_is_provided()
    {
        var result = VatCalculationService.Execute(new VatCalculationRequest(20, null, Gross: 120m, null));

        Assert.True(result.IsValid);
        Assert.Equal(100m, result.Response!.Net);
        Assert.Equal(20m, result.Response.Vat);
        Assert.Equal(120m, result.Response.Gross);
    }

    [Fact]
    public void Execute_calculates_from_vat_when_only_vat_is_provided()
    {
        var result = VatCalculationService.Execute(new VatCalculationRequest(10, null, null, Vat: 10m));

        Assert.True(result.IsValid);
        Assert.Equal(100m, result.Response!.Net);
        Assert.Equal(10m, result.Response.Vat);
        Assert.Equal(110m, result.Response.Gross);
    }

    [Fact]
    public void Execute_returns_validation_errors_without_a_response()
    {
        var result = VatCalculationService.Execute(new VatCalculationRequest(20, null, null, null));

        Assert.False(result.IsValid);
        Assert.Null(result.Response);
        Assert.NotEmpty(result.Errors!);
    }
}