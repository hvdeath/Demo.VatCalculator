var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.Demo_VatCalculator_Api>("api")
    .WithHttpHealthCheck("/health");

// Add JavaScript app using Aspire's built-in integration. This will run the
// provided npm script during local development and integrate the app with
// the AppHost resource graph. Register an HTTP endpoint so the Aspire
// dashboard shows a link to the running UI.
var webfrontend = builder.AddJavaScriptApp("frontend", "../Demo.VatCalculator.Ui", "start")
    .WithReference(apiService)
    .WithHttpEndpoint(port: 4200, env: "PORT");

builder.Build().Run();
