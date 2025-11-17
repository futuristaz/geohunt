using System;
using Polly;
using Polly.Extensions.Http;
using Serilog;

namespace psi25_project.Configuration
{
    public static class HttpPolicyConfiguration
    {
        /// <summary>
        /// Creates a combined retry policy for Google Maps API calls.
        /// Handles both rate limiting (429) and transient errors (5xx, 408).
        /// </summary>
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            var random = new Random();

            // Policy for 429 rate limiting errors - longer delays (8s, 16s, 32s)
            var rateLimitPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => r.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt =>
                    {
                        var baseDelay = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt + 2)); // 8s, 16s, 32s
                        var jitter = TimeSpan.FromMilliseconds(random.Next(0, 1000));
                        return baseDelay + jitter;
                    },
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        Log.Warning("Google Maps API rate limit hit (429). Retry {RetryCount} after {Delay}s.",
                            retryCount, timespan.TotalSeconds);
                    });

            // Policy for transient errors (5xx, 408) - standard delays (2s, 4s, 8s)
            var transientErrorPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt =>
                    {
                        var baseDelay = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)); // 2s, 4s, 8s
                        var jitter = TimeSpan.FromMilliseconds(random.Next(0, 1000));
                        return baseDelay + jitter;
                    },
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        Log.Warning("Google Maps API call failed. Retry {RetryCount} after {Delay}s. Status: {StatusCode}",
                            retryCount, timespan.TotalSeconds, outcome.Result?.StatusCode);
                    });

            // Wrap both policies - rate limit policy runs first, then transient error policy
            return Policy.WrapAsync(rateLimitPolicy, transientErrorPolicy);
        }

        /// <summary>
        /// Creates a circuit breaker policy for Google Maps API calls.
        /// Opens circuit after 5 consecutive failures, stays open for 30 seconds.
        /// </summary>
        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests) // 429 - consistent with retry policy
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (outcome, duration) =>
                    {
                        Log.Error("Google Maps API circuit breaker opened for {Duration}s due to consecutive failures", duration.TotalSeconds);
                    },
                    onReset: () =>
                    {
                        Log.Information("Google Maps API circuit breaker reset - resuming normal operations");
                    });
        }
    }
}
