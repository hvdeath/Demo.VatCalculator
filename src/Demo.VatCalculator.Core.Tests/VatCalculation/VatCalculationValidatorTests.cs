using Demo.VatCalculator.Core.VatCalculation;

namespace Demo.VatCalculator.Core.Tests.VatCalculation;

public sealed class VatCalculationValidatorTests
{
    private readonly VatCalculationValidator _sut = new();

    [Theory]
    [InlineData(10)]
    [InlineData(13)]
    [InlineData(20)]
    public void Validate_accepts_supported_rates(int rate)
    {
        var result = _sut.Validate(CreateRequest(rate, net: 100m));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(9)]
    [InlineData(21)]
    [InlineData(100)]
    public void Validate_rejects_unsupported_rates(int rate)
    {
        var result = _sut.Validate(CreateRequest(rate, net: 100m));

        Assert.False(result.IsValid);
        AssertSingleError(result, "rate", "vat.rateNotSupported");
    }

    [Fact]
    public void Validate_rejects_when_no_amount_was_provided()
    {
        var result = _sut.Validate(new VatCalculationRequest(20, null, null, null));

        Assert.False(result.IsValid);
        AssertSingleError(result, "amount", "vat.noAmount");
    }

    [Theory]
    [InlineData("net", "gross")]
    [InlineData("net", "vat")]
    [InlineData("gross", "vat")]
    [InlineData("net", "gross", "vat")]
    public void Validate_rejects_multiple_amounts(params string[] providedFields)
    {
        var request = new VatCalculationRequest(
            20,
            providedFields.Contains("net") ? 100m : null,
            providedFields.Contains("gross") ? 100m : null,
            providedFields.Contains("vat") ? 100m : null);

        var result = _sut.Validate(request);

        Assert.False(result.IsValid);
        Assert.Equal(providedFields.Length, result.Errors!.Count);
        foreach (var field in providedFields)
        {
            AssertSingleError(result, field, "vat.multipleInputs");
        }
    }

    [Theory]
    [InlineData("net")]
    [InlineData("gross")]
    [InlineData("vat")]
    public void Validate_rejects_zero_amounts(string field)
    {
        var result = _sut.Validate(WithAmount(field, 0m));

        Assert.False(result.IsValid);
        AssertSingleError(result, field, "vat.amountNotPositive");
    }

    public static TheoryData<string, decimal> NegativeAmounts =>
        new()
        {
            { "net", -1m },
            { "gross", -0.01m },
            { "vat", -100m },
        };

    [Theory]
    [MemberData(nameof(NegativeAmounts))]
    public void Validate_rejects_negative_amounts(string field, decimal amount)
    {
        var result = _sut.Validate(WithAmount(field, amount));

        Assert.False(result.IsValid);
        AssertSingleError(result, field, "vat.amountNotPositive");
    }

    public static TheoryData<string, decimal> ExcessivePrecisionAmounts =>
        new()
        {
            { "net", 10.005m },
            { "gross", 1.234m },
            { "vat", 0.001m },
        };

    [Theory]
    [MemberData(nameof(ExcessivePrecisionAmounts))]
    public void Validate_rejects_amounts_with_excessive_precision(string field, decimal amount)
    {
        var result = _sut.Validate(WithAmount(field, amount));

        Assert.False(result.IsValid);
        AssertSingleError(result, field, "vat.excessivePrecision");
    }

    [Theory]
    [InlineData("net")]
    [InlineData("gross")]
    [InlineData("vat")]
    public void Validate_accepts_maximum_amount(string field)
    {
        var result = _sut.Validate(WithAmount(field, MoneyConstants.MaxAmount));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("net")]
    [InlineData("gross")]
    [InlineData("vat")]
    public void Validate_rejects_amounts_exceeding_maximum(string field)
    {
        var result = _sut.Validate(WithAmount(field, MoneyConstants.MaxAmount + 0.01m));

        Assert.False(result.IsValid);
        AssertSingleError(result, field, "vat.amountTooLarge");
    }

    private static VatCalculationRequest CreateRequest(
        int rate = 20,
        decimal? net = null,
        decimal? gross = null,
        decimal? vat = null) =>
        new(rate, net, gross, vat);

    private static VatCalculationRequest WithAmount(string field, decimal amount) =>
        field switch
        {
            "net" => CreateRequest(net: amount),
            "gross" => CreateRequest(gross: amount),
            "vat" => CreateRequest(vat: amount),
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, null),
        };

    private static void AssertSingleError(VatValidationResult result, string field, string code)
    {
        var errors = result.Errors![field];
        Assert.Single(errors);
        Assert.Equal(code, errors[0].Code);
    }
}