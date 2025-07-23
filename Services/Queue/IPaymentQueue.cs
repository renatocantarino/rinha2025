using rinhabeckend2025.Models;
using System.Threading.Channels;

namespace rinhabeckend2025.Services.Queue;

public interface IPaymentQueue
{
    ValueTask EnqueueAsync(PaymentRequest paymentRequest);

    ChannelReader<PaymentRequest> Reader { get; }
}

public class PaymentQueue : IPaymentQueue
{
    private readonly Channel<PaymentRequest> _channel = Channel.CreateUnbounded<PaymentRequest>(new UnboundedChannelOptions
    {
        SingleReader = false,
        SingleWriter = false,
        AllowSynchronousContinuations = false,
    });

    public async ValueTask EnqueueAsync(PaymentRequest paymentRequest)
    {
        await _channel.Writer.WriteAsync(paymentRequest);
    }

    public ChannelReader<PaymentRequest> Reader => _channel.Reader;
}