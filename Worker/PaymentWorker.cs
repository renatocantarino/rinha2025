using Microsoft.Extensions.Options;
using rinhabeckend2025.Config;
using rinhabeckend2025.Services.Impl;
using rinhabeckend2025.Services.Queue;

namespace rinhabeckend2025.Worker;

public class PaymentWorker(
     IPaymentQueue paymentQueue,
     IOptions<PaymentProcessorSettings> settings,
     PaymentService paymentService,
     ILogger<PaymentWorker> logger
     ) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tasks = new List<Task>();
        for (var i = 0; i < settings.Value.WorkerPoolSize; i++)
        {
            tasks.Add(Task.Run(() => ProcessPaymentAsync(stoppingToken), stoppingToken));
        }

        await Task.WhenAll(tasks);
    }

    private async Task ProcessPaymentAsync(CancellationToken stoppingToken)
    {
        while (await paymentQueue.Reader.WaitToReadAsync(stoppingToken))
        {
            while (paymentQueue.Reader.TryRead(out var paymentRequest))
            {
                try
                {                    
                    await paymentService.ProcessPaymentAsync(paymentRequest, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing payment for correlation {CorrelationId}",
                        paymentRequest.CorrelationId);
                }
            }
        }
    }
}