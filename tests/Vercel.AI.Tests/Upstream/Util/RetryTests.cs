// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class RetryTests
{
    [UpstreamTest("packages/ai/src/util/prepare-retries.test.ts::should set default values correctly when no input is provided", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Prepare_retries_defaults_to_two()
    {
        Assert.Equal(2, PrepareRetries.Prepare(null).MaxRetries);
    }

    [UpstreamTest("packages/ai/src/util/retry-with-exponential-backoff.test.ts::retryWithExponentialBackoffRespectingRetryHeaders::should use rate limit header delay when present and reasonable", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Uses_retry_after_ms_when_it_is_short()
    {
        await AssertRetryDelay(3000, Headers(("retry-after-ms", "3000")), "success");
    }

    [UpstreamTest("packages/ai/src/util/retry-with-exponential-backoff.test.ts::retryWithExponentialBackoffRespectingRetryHeaders::should parse retry-after header in seconds", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Parses_retry_after_seconds()
    {
        await AssertRetryDelay(5000, Headers(("retry-after", "5")), "success");
    }

    [UpstreamTest("packages/ai/src/util/retry-with-exponential-backoff.test.ts::retryWithExponentialBackoffRespectingRetryHeaders::should use exponential backoff when rate limit delay is too long", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Falls_back_when_the_header_is_longer_than_a_minute()
    {
        await AssertRetryDelay(2000, Headers(("retry-after-ms", "70000")), "success");
    }

    [UpstreamTest("packages/ai/src/util/retry-with-exponential-backoff.test.ts::retryWithExponentialBackoffRespectingRetryHeaders::should fall back to exponential backoff when no rate limit headers", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Falls_back_when_headers_are_absent()
    {
        await AssertRetryDelay(2000, new Dictionary<string, string>(), "success");
    }

    [UpstreamTest("packages/ai/src/util/retry-with-exponential-backoff.test.ts::retryWithExponentialBackoffRespectingRetryHeaders::should handle invalid rate limit header values", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Falls_back_when_headers_are_invalid()
    {
        await AssertRetryDelay(2000, Headers(("retry-after-ms", "invalid"), ("retry-after", "not-a-number")), "success");
    }

    [UpstreamTest("packages/ai/src/util/retry-with-exponential-backoff.test.ts::retryWithExponentialBackoffRespectingRetryHeaders > with mocked provider responses::should handle Anthropic 429 response with retry-after-ms header", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Retries_an_anthropic_rate_limit()
    {
        var result = await AssertRetryDelay(5000, Headers(("retry-after-ms", "5000"), ("x-request-id", "req_123456")), new { content = "Hello from Claude!" });
        Assert.Equal("Hello from Claude!", result.content);
    }

    [UpstreamTest("packages/ai/src/util/retry-with-exponential-backoff.test.ts::retryWithExponentialBackoffRespectingRetryHeaders > with mocked provider responses::should handle OpenAI 429 response with retry-after header", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Retries_an_openai_rate_limit()
    {
        var result = await AssertRetryDelay(30000, Headers(("retry-after", "30"), ("x-request-id", "req_abcdef123456")), "Hello from GPT!");
        Assert.Equal("Hello from GPT!", result);
    }

    [UpstreamTest("packages/ai/src/util/retry-with-exponential-backoff.test.ts::retryWithExponentialBackoffRespectingRetryHeaders > with mocked provider responses::should handle multiple retries with exponential backoff progression", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Honors_each_retry_after_header()
    {
        var delays = new List<int>();
        var gates = new Queue<TaskCompletionSource<bool>>();
        var calls = 0;
        var retry = new RetryWithExponentialBackoffRespectingRetryHeaders(new RetryOptions
        {
            MaxRetries = 3,
            Delay = delegate (int milliseconds, CancellationToken token)
            {
                delays.Add(milliseconds);
                var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                gates.Enqueue(gate);
                return gate.Task;
            },
        });
        var task = retry.ExecuteAsync<string>(delegate
        {
            calls++;
            if (calls == 1)
            {
                throw Api("retry-after-ms", "5000");
            }

            if (calls == 2)
            {
                throw Api("retry-after-ms", "2000");
            }

            return Task.FromResult("Success after retries!");
        });

        await WaitUntil(delegate { return delays.Count == 1; });
        Assert.Equal(1, calls);
        Assert.Equal(5000, delays[0]);
        gates.Dequeue().SetResult(true);
        await WaitUntil(delegate { return delays.Count == 2; });
        Assert.Equal(2, calls);
        Assert.Equal(2000, delays[1]);
        gates.Dequeue().SetResult(true);
        Assert.Equal("Success after retries!", await task);
        Assert.Equal(3, calls);
    }

    [UpstreamTest("packages/ai/src/util/retry-with-exponential-backoff.test.ts::retryWithExponentialBackoffRespectingRetryHeaders > with mocked provider responses::should prefer retry-after-ms over retry-after when both present", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Prefers_retry_after_ms()
    {
        await AssertRetryDelay(3000, Headers(("retry-after-ms", "3000"), ("retry-after", "10")), "success");
    }

    [UpstreamTest("packages/ai/src/util/retry-with-exponential-backoff.test.ts::retryWithExponentialBackoffRespectingRetryHeaders > with mocked provider responses::should handle retry-after header with HTTP date format", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Parses_an_http_date_retry_after()
    {
        var now = DateTimeOffset.FromUnixTimeMilliseconds(1700000000000);
        var header = now.AddMilliseconds(5000).ToUniversalTime().ToString("r", CultureInfo.InvariantCulture);
        var delays = new List<int>();
        var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = 0;
        var retry = new RetryWithExponentialBackoffRespectingRetryHeaders(new RetryOptions
        {
            UtcNow = delegate { return now; },
            Delay = delegate (int milliseconds, CancellationToken token)
            {
                delays.Add(milliseconds);
                return gate.Task;
            },
        });
        var task = retry.ExecuteAsync(delegate
        {
            calls++;
            if (calls == 1)
            {
                throw Api("retry-after", header);
            }

            return Task.FromResult("success");
        });
        await WaitUntil(delegate { return delays.Count == 1; });
        Assert.Equal(1, calls);
        Assert.InRange(delays[0], 4000, 6000);
        gate.SetResult(true);
        Assert.Equal("success", await task);
        Assert.Equal(2, calls);
    }

    [UpstreamTest("packages/ai/src/util/retry-with-exponential-backoff.test.ts::retryWithExponentialBackoffRespectingRetryHeaders > with mocked provider responses::should fall back to exponential backoff when rate limit delay is negative", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Falls_back_when_the_header_is_negative()
    {
        await AssertRetryDelay(2000, Headers(("retry-after-ms", "-1000")), "success");
    }

    [UpstreamTest("packages/ai/src/util/retry-with-exponential-backoff.test.ts::retryWithExponentialBackoffRespectingRetryHeaders > with Gateway errors::should retry on GatewayInternalServerError", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Retries_a_gateway_internal_error()
    {
        await AssertGatewayRetry(new GatewayInternalServerError("Internal server error", 503));
    }

    [UpstreamTest("packages/ai/src/util/retry-with-exponential-backoff.test.ts::retryWithExponentialBackoffRespectingRetryHeaders > with Gateway errors::should retry on GatewayRateLimitError", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Retries_a_gateway_rate_limit()
    {
        await AssertGatewayRetry(new GatewayRateLimitError("Rate limit exceeded"));
    }

    [UpstreamTest("packages/ai/src/util/retry-with-exponential-backoff.test.ts::retryWithExponentialBackoffRespectingRetryHeaders > with Gateway errors::should not retry on non-retryable GatewayAuthenticationError", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Does_not_retry_gateway_authentication()
    {
        var calls = 0;
        var retry = new RetryWithExponentialBackoffRespectingRetryHeaders(new RetryOptions
        {
            Delay = delegate { throw new InvalidOperationException("delay should not run"); },
        });
        var error = await Assert.ThrowsAsync<GatewayAuthenticationError>(delegate
        {
            return retry.ExecuteAsync<string>(delegate
            {
                calls++;
                throw new GatewayAuthenticationError("Invalid API key");
            });
        });
        Assert.Equal("Invalid API key", error.Message);
        Assert.Equal(1, calls);
    }

    [UpstreamTest("packages/ai/src/util/retry-with-exponential-backoff.test.ts::retryWithExponentialBackoffRespectingRetryHeaders > with Gateway errors::should use retry-after headers from APICallError cause", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Reads_retry_after_from_the_gateway_cause()
    {
        var cause = Api("retry-after-ms", "3000");
        var delays = new List<int>();
        var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = 0;
        var retry = new RetryWithExponentialBackoffRespectingRetryHeaders(new RetryOptions
        {
            Delay = delegate (int milliseconds, CancellationToken token)
            {
                delays.Add(milliseconds);
                return gate.Task;
            },
        });
        var task = retry.ExecuteAsync(delegate
        {
            calls++;
            if (calls == 1)
            {
                throw new GatewayInternalServerError("Internal server error", 503, cause);
            }

            return Task.FromResult("success");
        });
        await WaitUntil(delegate { return delays.Count == 1; });
        Assert.Equal(3000, delays[0]);
        Assert.Equal(1, calls);
        gate.SetResult(true);
        Assert.Equal("success", await task);
    }

    private static async Task<T> AssertRetryDelay<T>(int expectedDelay, IReadOnlyDictionary<string, string> headers, T success)
    {
        var delays = new List<int>();
        var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = 0;
        var retry = new RetryWithExponentialBackoffRespectingRetryHeaders(new RetryOptions
        {
            Delay = delegate (int milliseconds, CancellationToken token)
            {
                delays.Add(milliseconds);
                return gate.Task;
            },
        });
        var task = retry.ExecuteAsync(delegate
        {
            calls++;
            if (calls == 1)
            {
                throw new ApiCallError("Rate limited", "https://api.example.com", new object(), 429, headers, isRetryable: true);
            }

            return Task.FromResult(success);
        });
        await WaitUntil(delegate { return delays.Count == 1; });
        Assert.Equal(expectedDelay, delays[0]);
        Assert.Equal(1, calls);
        gate.SetResult(true);
        var result = await task;
        Assert.Equal(2, calls);
        return result;
    }

    private static Task AssertGatewayRetry(Exception error)
    {
        var delays = new List<int>();
        var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = 0;
        var retry = new RetryWithExponentialBackoffRespectingRetryHeaders(new RetryOptions
        {
            Delay = delegate (int milliseconds, CancellationToken token)
            {
                delays.Add(milliseconds);
                return gate.Task;
            },
        });
        var task = retry.ExecuteAsync(delegate
        {
            calls++;
            if (calls == 1)
            {
                throw error;
            }

            return Task.FromResult("success");
        });
        return Finish(delays, gate, task, delegate { return calls; });
    }

    private static async Task Finish(List<int> delays, TaskCompletionSource<bool> gate, Task<string> task, Func<int> calls)
    {
        await WaitUntil(delegate { return delays.Count == 1; });
        Assert.Equal(2000, delays[0]);
        Assert.Equal(1, calls());
        gate.SetResult(true);
        Assert.Equal("success", await task);
        Assert.Equal(2, calls());
    }

    private static ApiCallError Api(string name, string value)
    {
        return new ApiCallError("Rate limited", "https://api.example.com", new object(), 429, Headers((name, value)), isRetryable: true);
    }

    private static Dictionary<string, string> Headers(params (string Name, string Value)[] headers)
    {
        var result = new Dictionary<string, string>();
        for (var i = 0; i < headers.Length; i++)
        {
            result[headers[i].Name] = headers[i].Value;
        }

        return result;
    }

    private static async Task WaitUntil(Func<bool> condition)
    {
        for (var i = 0; i < 100; i++)
        {
            if (condition())
            {
                return;
            }

            await Task.Delay(10);
        }

        Assert.True(condition());
    }
}
