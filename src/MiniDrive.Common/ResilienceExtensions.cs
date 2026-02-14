using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

namespace MiniDrive.Common;

public static class ResilienceExtensions
{
    public static IHttpClientBuilder AddDefaultResilience(this IHttpClientBuilder builder)
    {
        // Remove .Builder and just return the original builder
        builder.AddStandardResilienceHandler(options =>
        {
            options.Retry.MaxRetryAttempts = 3;
            options.Retry.Delay = TimeSpan.FromSeconds(2);

            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
            options.CircuitBreaker.FailureRatio = 0.5;
        });
        return builder;
    }
}