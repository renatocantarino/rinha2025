using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;
using rinhabeckend2025.Config;
using rinhabeckend2025.Models;
using rinhabeckend2025.Services.Client;
using rinhabeckend2025.Services.HC;
using rinhabeckend2025.Services.Impl;
using rinhabeckend2025.Services.Queue;
using rinhabeckend2025.Services.Strategy;
using rinhabeckend2025.Worker;

ThreadPool.SetMinThreads(Environment.ProcessorCount * 4, Environment.ProcessorCount * 4);

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxConcurrentConnections = 1000;
    options.Limits.MaxConcurrentUpgradedConnections = 1000;
    options.Limits.MaxRequestBodySize = 1024; // 1KB - payments are small
    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(5);
    options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(120);
    options.AllowSynchronousIO = false; // Keep async-only for better performance
});

// Disable features not needed for high-performance API
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = false; // Skip URL transformation
    options.LowercaseQueryStrings = false;
    options.AppendTrailingSlash = false;
});

// Optimize logging for production (minimize allocations)
builder.Logging.Configure(options =>
{
    options.ActivityTrackingOptions = ActivityTrackingOptions.None;
});

builder.Services.Configure<PaymentProcessorSettings>(builder.Configuration.GetSection("PaymentProcessor"));

builder.Services.AddHttpClient(Options.DefaultName, (serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<PaymentProcessorSettings>>().Value;
    client.Timeout = TimeSpan.FromMilliseconds(settings.HttpClientTimeoutMilliseconds);
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddSingleton<IPaymentQueue, PaymentQueue>();
builder.Services.AddSingleton<IPaymentProcessorClient, HttpPaymentProcessorClient>();

//builder.Services.AddSingleton<IPaymentRepository, PaymentRepository>();

builder.Services.AddSingleton<IPaymentProcessorStrategy, DefaultPaymentProcessorStrategy>();
builder.Services.AddSingleton<IPaymentProcessorStrategy, FallbackPaymentProcessorStrategy>();
builder.Services.AddSingleton<PaymentService>();

// Configure health checks
builder.Services.AddSingleton<HealthCheckFactory>();
builder.Services.AddSingleton<IHealthPaymentProcessorService>(sp =>
{
    var factory = sp.GetRequiredService<HealthCheckFactory>();
    return new HealthPaymentProcessorService(
        factory.CreateDefaultProcessor(),
        factory.CreateFallbackProcessor());
});

builder.Services.AddHostedService<PaymentWorker>();

var app = builder.Build();

app.MapPost("/payments", async (PaymentRequest request, IPaymentQueue queue) =>
{
    await queue.EnqueueAsync(request);
    return Results.Accepted();
}).DisableAntiforgery();

app.MapGet("/payments/service-health", () =>
{
    return Results.NoContent();
}).DisableAntiforgery();

await app.RunAsync();