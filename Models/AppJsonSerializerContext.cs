using System.Text.Json.Serialization;

namespace rinhabeckend2025.Models;

public record PaymentRequest(Guid CorrelationId, decimal Amount);
public record PaymentProcessorRequest(Guid CorrelationId, decimal Amount, DateTime RequestedAt);

public record PaymentSummaryResponse(PaymentSummary Default, PaymentSummary Fallback
);

public record PaymentSummary(long TotalRequests, decimal TotalAmount);

public enum ProcessorType
{
    Default, Fallback
}

public class SummaryQueryResult
{
    public int DefaultTotalRequests { get; init; }
    public decimal DefaultTotalAmount { get; init; }
    public int FallbackTotalRequests { get; init; }
    public decimal FallbackTotalAmount { get; init; }
}

public record PaymentPurgeResponse(string Message);

public class Payment
{
    public Guid CorrelationId { get; init; }
    public decimal Amount { get; init; }
    public DateTime RequestedAt { get; init; }
    public ProcessorType ProcessorType { get; init; }
}

[JsonSerializable(typeof(PaymentRequest))]
[JsonSerializable(typeof(SummaryQueryResult))]
[JsonSerializable(typeof(PaymentSummaryResponse))]
[JsonSerializable(typeof(PaymentSummary))]
[JsonSerializable(typeof(PaymentProcessorRequest))]
[JsonSerializable(typeof(PaymentPurgeResponse))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{ }