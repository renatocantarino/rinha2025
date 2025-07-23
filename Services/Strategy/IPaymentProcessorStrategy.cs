using Microsoft.Extensions.Options;
using rinhabeckend2025.Config;
using rinhabeckend2025.Models;
using rinhabeckend2025.Services.Client;
using rinhabeckend2025.Services.HC;

namespace rinhabeckend2025.Services.Strategy;

public interface IPaymentProcessorStrategy
{
    ProcessorType ProcessorType { get; }

    Task<bool> CanProcessAsync();

    Task<bool> ProcessAsync(PaymentProcessorRequest request, CancellationToken token);
}

public class DefaultPaymentProcessorStrategy(IHealthPaymentProcessorService healthService,
                                             IOptions<PaymentProcessorSettings> settings,
                                             IPaymentProcessorClient processorClient)
                                           : IPaymentProcessorStrategy
{
    public ProcessorType ProcessorType => ProcessorType.Default;
    private readonly string _processorUrl = settings.Value.DefaultUrl;

    public Task<bool> CanProcessAsync() => healthService.IsDefaultOnlineAsync();

    public async Task<bool> ProcessAsync(PaymentProcessorRequest request, CancellationToken token)
    {
        var sucess = await processorClient.ProcessPaymentAsync(request, _processorUrl, token);
        healthService.UpdateDefaultHealth(sucess);
        return sucess;
    }
}

public class FallbackPaymentProcessorStrategy(IHealthPaymentProcessorService healthService,
                                             IOptions<PaymentProcessorSettings> settings,
                                             IPaymentProcessorClient processorClient)
                                           : IPaymentProcessorStrategy
{
    public ProcessorType ProcessorType => ProcessorType.Fallback;
    private readonly string _processorUrl = settings.Value.FallbackUrl;

    public Task<bool> CanProcessAsync() => healthService.IsFallbackOnlineAsync();

    public async Task<bool> ProcessAsync(PaymentProcessorRequest request, CancellationToken token)
    {
        var sucess = await processorClient.ProcessPaymentAsync(request, _processorUrl, token);
        healthService.UpdateDefaultHealth(sucess);
        return sucess;
    }
}