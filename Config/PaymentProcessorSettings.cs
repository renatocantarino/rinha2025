namespace rinhabeckend2025.Config;

public class PaymentProcessorSettings
{
    public string DefaultUrl { get; set; } = string.Empty;
    public string FallbackUrl { get; set; } = string.Empty;
    public int WorkerPoolSize { get; set; } = 5;
    public int HealthCheckIntervalMilliseconds { get; set; } = 1000;
    public int HttpClientTimeoutMilliseconds { get; set; } = 1000;
}