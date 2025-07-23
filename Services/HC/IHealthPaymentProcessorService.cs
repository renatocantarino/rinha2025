namespace rinhabeckend2025.Services.HC;

public interface IHealthPaymentProcessorService
{
    Task<bool> IsDefaultOnlineAsync();

    Task<bool> IsFallbackOnlineAsync();

    void UpdateDefaultHealth(bool paymentSucceeded);

    void UpdateFallbackHealth(bool paymentSucceeded);
}

public class HealthPaymentProcessorService(
    ITimeBasedHealthCheck defaultProcessor,
    ITimeBasedHealthCheck fallbackProcessor)
    : IHealthPaymentProcessorService
{
    public Task<bool> IsDefaultOnlineAsync() => defaultProcessor.IsHealthyAsync();

    public Task<bool> IsFallbackOnlineAsync() => fallbackProcessor.IsHealthyAsync();

    public void UpdateDefaultHealth(bool paymentSucceeded) =>
        defaultProcessor.UpdateFromPaymentResult(paymentSucceeded);

    public void UpdateFallbackHealth(bool paymentSucceeded) =>
        fallbackProcessor.UpdateFromPaymentResult(paymentSucceeded);
}