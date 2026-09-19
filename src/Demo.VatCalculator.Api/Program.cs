using System.Text.Json.Serialization;
using Demo.VatCalculator.Api.Endpoints;
using Demo.VatCalculator.Api.ErrorHandling;
using Demo.VatCalculator.Core.VatCalculation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<VatCalculationService>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
});

var app = builder.Build();

app.UseMiddleware<ExceptionToProblemDetailsMiddleware>();

app.MapOpenApi();
app.UseSwaggerUI(options =>
    options.SwaggerEndpoint("/openapi/v1.json", "Demo.VatCalculator API v1"));

app.MapVatEndpoints();

app.Run();

public partial class Program;