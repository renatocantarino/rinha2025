using Microsoft.Extensions.Options;
using rinhabeckend2025.Config;

namespace rinhabeckend2025.Services.HC;

public class HealthCheckFactory(
    IHttpClientFactory httpClientFactory,
    IOptions<PaymentProcessorSettings> settings)
{
    private readonly PaymentProcessorSettings _settings = settings.Value;
    private const string HealthEndpoint = "/payments/service-health";

    public ITimeBasedHealthCheck CreateDefaultProcessor()
    {
        var healthCheck = new HttpEndpointHealthCheck(
            httpClientFactory,
            _settings.DefaultUrl + HealthEndpoint);
        return new TimeBasedHealthCheck(
            healthCheck,
            TimeSpan.FromMilliseconds(_settings.HealthCheckIntervalMilliseconds));
    }

    public ITimeBasedHealthCheck CreateFallbackProcessor()
    {
        var healthCheck = new HttpEndpointHealthCheck(
            httpClientFactory,
            _settings.FallbackUrl + HealthEndpoint);
        return new TimeBasedHealthCheck(
            healthCheck,
            TimeSpan.FromMilliseconds(_settings.HealthCheckIntervalMilliseconds));
    }
}