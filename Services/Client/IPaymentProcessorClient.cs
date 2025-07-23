using rinhabeckend2025.Models;

namespace rinhabeckend2025.Services.Client;

public interface IPaymentProcessorClient
{
    Task<bool> ProcessPaymentAsync(PaymentProcessorRequest request, string processorUrl, CancellationToken token);
}

public class HttpPaymentProcessorClient(
    IHttpClientFactory httpClientFactory,
    ILogger<HttpPaymentProcessorClient> logger)
    : IPaymentProcessorClient
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

    public async Task<bool> ProcessPaymentAsync(PaymentProcessorRequest request, string processorUrl, CancellationToken token)
    {
        try
        {
            var result = await _httpClient.PostAsJsonAsync(
                processorUrl + "/payments",
                request,
                AppJsonSerializerContext.Default.PaymentProcessorRequest,
                token);

            return result.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing payment through {ProcessorUrl}", processorUrl);
            return false;
        }
    }
}