namespace rinhabeckend2025.Services.HC;

public interface ITimeBasedHealthCheck : IHealthCheck
{
    TimeSpan CacheDuration { get; }
    DateTime LastCheckTime { get; }
    bool LastStatus { get; }

    void UpdateFromPaymentResult(bool paymentSucceeded);
}

public class TimeBasedHealthCheck(IHealthCheck innerHealthCheck, TimeSpan cacheDuration) : ITimeBasedHealthCheck
{
    private readonly SemaphoreSlim _lock = new(1);

    private bool _lastPaymentStatus;

    public TimeSpan CacheDuration { get; } = cacheDuration;
    public DateTime LastCheckTime { get; private set; } = DateTime.MinValue;

    public bool LastStatus { get; private set; }

    public async Task<bool> IsHealthyAsync()
    {
        var now = DateTime.UtcNow;

        if (_lastPaymentStatus)
        {
            return true;
        }
        // If recent payment FAILED → assume timeout, check frequently
        var effectiveCacheDuration = _lastPaymentStatus == false
            ? TimeSpan.FromSeconds(1) : CacheDuration;

        if (now - LastCheckTime < effectiveCacheDuration)
            return LastStatus;

        await _lock.WaitAsync();
        try
        {
            if (now - LastCheckTime >= effectiveCacheDuration)
            {
                LastStatus = await innerHealthCheck.IsHealthyAsync();
                _lastPaymentStatus = LastStatus;
                LastCheckTime = now;
            }
        }
        finally
        {
            _lock.Release();
        }

        return LastStatus;
    }

    public void UpdateFromPaymentResult(bool paymentSucceeded)
    {
        var now = DateTime.UtcNow;
        _lastPaymentStatus = paymentSucceeded;
        LastStatus = paymentSucceeded;
        LastCheckTime = now;
    }
}