using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace Demo.VatCalculator.Api.Tests;

public sealed class VatCalculationApiTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly string Route = "/api/v1/vat/calculate";

    public static TheoryData<int, string, decimal, decimal, decimal, decimal> CalculationData => new()
    {
        { 10, "net", 100m, 100m, 10m, 110m },
        { 13, "net", 33.33m, 33.33m, 4.33m, 37.66m },
        { 20, "net", 33.33m, 33.33m, 6.67m, 40m },
        { 20, "gross", 40m, 33.33m, 6.67m, 40m },
        { 20, "gross", 39.99m, 33.33m, 6.66m, 39.99m },
        { 10, "vat", 10m, 100m, 10m, 110m },
        { 20, "vat", 6.67m, 33.35m, 6.67m, 40.02m },
    };

    [Theory]
    [MemberData(nameof(CalculationData))]
    public async Task Calculate_returns_net_vat_gross_for_valid_request(
        int rate,
        string kind,
        decimal input,
        decimal expectedNet,
        decimal expectedVat,
        decimal expectedGross)
    {
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(Route, new Dictionary<string, object>
        {
            ["rate"] = rate,
            [kind] = input,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        using var body = await response.Content.ReadAsJsonDocument();
        Assert.Equal(expectedNet, body.RootElement.GetProperty("net").GetDecimal());
        Assert.Equal(expectedVat, body.RootElement.GetProperty("vat").GetDecimal());
        Assert.Equal(expectedGross, body.RootElement.GetProperty("gross").GetDecimal());
        Assert.Equal(rate, body.RootElement.GetProperty("rate").GetInt32());
        Assert.Equal(
            body.RootElement.GetProperty("net").GetDecimal() + body.RootElement.GetProperty("vat").GetDecimal(),
            body.RootElement.GetProperty("gross").GetDecimal());
    }

    public static TheoryData<string, string, string, string> ValidationFailureData => new()
    {
        { "missing amount", """{"rate":20}""", "amount", "vat.noAmount" },
        { "zero amount", """{"rate":20,"net":0}""", "net", "vat.amountNotPositive" },
        { "negative amount", """{"rate":20,"gross":-5}""", "gross", "vat.amountNotPositive" },
        { "too precise", """{"rate":20,"vat":0.001}""", "vat", "vat.excessivePrecision" },
        { "too large", """{"rate":20,"net":1000000000.01}""", "net", "vat.amountTooLarge" },
        { "unsupported rate", """{"rate":21,"net":100}""", "rate", "vat.rateNotSupported" },
        { "zero rate", """{"rate":0,"net":100}""", "rate", "vat.rateNotSupported" },
    };

    [Theory]
    [MemberData(nameof(ValidationFailureData))]
    public async Task Calculate_returns_validation_failed_contract_for_domain_errors(
        string _,
        string json,
        string field,
        string code)
    {
        using var client = factory.CreateClient();

        using var response = await PostJsonAsync(client, json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var body = await response.Content.ReadAsJsonDocument();
        Assert.Equal("urn:demo-vat:validation-failed", body.RootElement.GetProperty("type").GetString());
        Assert.Equal(400, body.RootElement.GetProperty("status").GetInt32());
        var error = body.RootElement.GetProperty("errors").GetProperty(field).EnumerateArray().Single();
        Assert.Equal(code, error.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Calculate_rejects_multiple_amounts_with_an_error_per_field()
    {
        using var client = factory.CreateClient();

        using var response = await PostJsonAsync(client, """{"rate":20,"net":100,"gross":110}""");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var body = await response.Content.ReadAsJsonDocument();
        var errors = body.RootElement.GetProperty("errors");
        Assert.Equal(
            "vat.multipleInputs",
            errors.GetProperty("net").EnumerateArray().Single().GetProperty("code").GetString());
        Assert.Equal(
            "vat.multipleInputs",
            errors.GetProperty("gross").EnumerateArray().Single().GetProperty("code").GetString());
    }

    [Theory]
    [InlineData("""{"rate":20,"net":100,"tax":5}""", "tax")]
    [InlineData("""{"rate":20,"net":100,"grossAmount":100}""", "grossAmount")]
    public async Task Calculate_rejects_unknown_fields_with_machine_readable_detail(string json, string field)
    {
        using var client = factory.CreateClient();

        using var response = await PostJsonAsync(client, json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var body = await response.Content.ReadAsJsonDocument();
        Assert.Equal("urn:demo-vat:unknown-field", body.RootElement.GetProperty("type").GetString());
        var error = body.RootElement.GetProperty("errors").GetProperty(field).EnumerateArray().Single();
        Assert.Equal("vat.unknownField", error.GetProperty("code").GetString());
    }

    [Theory]
    [InlineData("""{"rate":20,"net":"100"}""", "string-typed amount")]
    [InlineData("""{"rate":"20","net":100}""", "string-typed rate")]
    [InlineData("""{"rate":20,"net":"1,23"}""", "locale-style comma string")]
    [InlineData("""{"rate":20""", "truncated JSON")]
    public async Task Calculate_returns_malformed_body_contract_for_unparseable_bodies(string json, string _)
    {
        using var client = factory.CreateClient();

        using var response = await PostJsonAsync(client, json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var body = await response.Content.ReadAsJsonDocument();
        Assert.Equal("urn:demo-vat:malformed-body", body.RootElement.GetProperty("type").GetString());
        var error = body.RootElement.GetProperty("errors").GetProperty("request").EnumerateArray().Single();
        Assert.Equal("vat.malformedBody", error.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Calculate_returns_method_not_allowed_for_get()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(Route);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task OpenApi_document_contains_the_v1_calculation_path()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = await response.Content.ReadAsJsonDocument();
        var paths = body.RootElement.GetProperty("paths");
        Assert.True(paths.TryGetProperty("/api/v1/vat/calculate", out _));
    }

    [Fact]
    public async Task Swagger_ui_is_served()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/swagger/index.html");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
    }

    private static async Task<HttpResponseMessage> PostJsonAsync(HttpClient client, string json)
    {
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        return await client.PostAsync(Route, content);
    }
}

internal static class HttpContentJsonExtensions
{
    public static async Task<System.Text.Json.JsonDocument> ReadAsJsonDocument(this HttpContent content)
    {
        var stream = await content.ReadAsStreamAsync();
        return await System.Text.Json.JsonDocument.ParseAsync(stream);
    }
}