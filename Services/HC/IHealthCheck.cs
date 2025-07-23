﻿using System.Net;

namespace rinhabeckend2025.Services.HC;

public interface IHealthCheck
{
    Task<bool> IsHealthyAsync();
}

public class HttpEndpointHealthCheck(IHttpClientFactory httpClientFactory, string healthCheckUrl)
    : IHealthCheck
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(healthCheckUrl);
            return response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.TooManyRequests;
        }
        catch
        {
            return false;
        }
    }
}