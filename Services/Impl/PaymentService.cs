using rinhabeckend2025.Models;
using rinhabeckend2025.Services.Queue;
using rinhabeckend2025.Services.Strategy;
using System.Diagnostics;

namespace rinhabeckend2025.Services.Impl
{
    public class PaymentService(IEnumerable<IPaymentProcessorStrategy> strategies,
                                IPaymentQueue queue,
                                ILogger<PaymentService> logger)
    {
        public async Task ProcessPaymentAsync(PaymentRequest paymentRequest, CancellationToken token)
        {
            var requestedAt = DateTime.UtcNow;
            var processorRequest = new PaymentProcessorRequest(
                paymentRequest.CorrelationId,
                paymentRequest.Amount,
                requestedAt);

            foreach (var processor in strategies)
            {
                if (!await processor.CanProcessAsync())
                {
                    continue;
                }

                if (!await processor.ProcessAsync(processorRequest, token))
                {
                    continue;
                }

                logger.LogWarning("All processors failed, re-queueing payment {CorrelationId}", paymentRequest.CorrelationId);
                await queue.EnqueueAsync(paymentRequest);
            }
        }
    }
}