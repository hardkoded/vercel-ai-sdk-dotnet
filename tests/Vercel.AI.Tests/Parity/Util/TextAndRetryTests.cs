// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class TextAndRetryTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/get-potential-start-index.test.ts::getPotentialStartIndex::should return null when searchedText is empty",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_null_for_an_empty_search()
    {
        Assert.Null(TextIndex.GetPotentialStartIndex("1234567890", string.Empty));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/get-potential-start-index.test.ts::getPotentialStartIndex::should return null when searchedText is not in text",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_null_when_the_search_is_absent()
    {
        Assert.Null(TextIndex.GetPotentialStartIndex("1234567890", "a"));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/get-potential-start-index.test.ts::getPotentialStartIndex::should return index when searchedText is in text",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_the_index_of_a_full_match()
    {
        Assert.Equal(0, TextIndex.GetPotentialStartIndex("1234567890", "1234567890"));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/get-potential-start-index.test.ts::getPotentialStartIndex::should return index when searchedText might start in text",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_a_suffix_that_starts_the_search()
    {
        Assert.Equal(9, TextIndex.GetPotentialStartIndex("1234567890", "0123"));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/get-potential-start-index.test.ts::getPotentialStartIndex::should return index when searchedText might start in text #2",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_a_longer_suffix_prefix()
    {
        Assert.Equal(8, TextIndex.GetPotentialStartIndex("1234567890", "90123"));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/get-potential-start-index.test.ts::getPotentialStartIndex::should return index when searchedText might start in text #3",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_the_longest_suffix_prefix()
    {
        Assert.Equal(7, TextIndex.GetPotentialStartIndex("1234567890", "890123"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/split-array.test.ts::should split an array into chunks of the specified size", Coverage = UpstreamCoverage.Covered)]
    public void Splits_into_chunks()
    {
        var chunks = Arrays.SplitArray(new[] { 1, 2, 3, 4, 5 }, 2);
        Assert.Equal(3, chunks.Count);
        Assert.Equal(new[] { 1, 2 }, chunks[0]);
        Assert.Equal(new[] { 3, 4 }, chunks[1]);
        Assert.Equal(new[] { 5 }, chunks[2]);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/split-array.test.ts::should return an empty array when the input array is empty", Coverage = UpstreamCoverage.Covered)]
    public void Returns_no_chunks_for_an_empty_array()
    {
        Assert.Empty(Arrays.SplitArray(Array.Empty<int>(), 2));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/split-array.test.ts::should return the original array when the chunk size is greater than the array length",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_one_chunk_when_the_size_is_larger()
    {
        var chunks = Arrays.SplitArray(new[] { 1, 2, 3 }, 5);
        Assert.Single(chunks);
        Assert.Equal(new[] { 1, 2, 3 }, chunks[0]);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/split-array.test.ts::should return the original array when the chunk size is equal to the array length",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_one_chunk_when_the_size_matches()
    {
        var chunks = Arrays.SplitArray(new[] { 1, 2, 3 }, 3);
        Assert.Single(chunks);
        Assert.Equal(new[] { 1, 2, 3 }, chunks[0]);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/split-array.test.ts::should handle chunk size of 1 correctly", Coverage = UpstreamCoverage.Covered)]
    public void Splits_into_single_element_chunks()
    {
        var chunks = Arrays.SplitArray(new[] { 1, 2, 3 }, 1);
        Assert.Equal(3, chunks.Count);
        Assert.Equal(new[] { 1 }, chunks[0]);
        Assert.Equal(new[] { 2 }, chunks[1]);
        Assert.Equal(new[] { 3 }, chunks[2]);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/split-array.test.ts::should throw InvalidArgumentError for chunk size %s", Coverage = UpstreamCoverage.Covered)]
    public void Throws_for_a_non_positive_chunk_size()
    {
        foreach (var size in new[] { 0, -1 })
        {
            var error = Assert.Throws<InvalidArgumentException>(() => Arrays.SplitArray(new[] { 1, 2, 3 }, size));
            Assert.True(InvalidArgumentException.IsInstance(error));
            Assert.Equal("chunkSize", error.Parameter);
            Assert.Equal(size, error.Value);
            Assert.Equal("Invalid argument for parameter chunkSize: chunkSize must be greater than 0", error.Message);
        }
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/split-array.test.ts::should handle non-integer chunk size by flooring the size", Coverage = UpstreamCoverage.Covered)]
    public void Floors_a_non_integer_chunk_size()
    {
        var chunks = Arrays.SplitArray(new[] { 1, 2, 3, 4, 5 }, (int)Math.Floor(2.5));
        Assert.Equal(3, chunks.Count);
        Assert.Equal(new[] { 1, 2 }, chunks[0]);
        Assert.Equal(new[] { 3, 4 }, chunks[1]);
        Assert.Equal(new[] { 5 }, chunks[2]);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/prepare-headers.test.ts::prepareHeaders::should set Content-Type header if not present", Coverage = UpstreamCoverage.Covered)]
    public void Sets_a_missing_content_type()
    {
        var headers = HttpHeaderPreparation.PrepareHeaders(
            new Dictionary<string, string>(),
            new Dictionary<string, string> { ["content-type"] = "application/json" });
        Assert.Equal("application/json", headers.Get("Content-Type"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/prepare-headers.test.ts::prepareHeaders::should not overwrite existing Content-Type header", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_overwrite_content_type()
    {
        var headers = HttpHeaderPreparation.PrepareHeaders(
            new Dictionary<string, string> { ["Content-Type"] = "text/html" },
            new Dictionary<string, string> { ["content-type"] = "application/json" });
        Assert.Equal("text/html", headers.Get("Content-Type"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/prepare-headers.test.ts::prepareHeaders::should handle undefined init", Coverage = UpstreamCoverage.Covered)]
    public void Applies_defaults_when_init_is_null()
    {
        var headers = HttpHeaderPreparation.PrepareHeaders(
            null,
            new Dictionary<string, string> { ["content-type"] = "application/json" });
        Assert.Equal("application/json", headers.Get("Content-Type"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/prepare-headers.test.ts::prepareHeaders::should handle init headers as Headers object", Coverage = UpstreamCoverage.Covered)]
    public void Copies_a_header_map()
    {
        var headers = HttpHeaderPreparation.PrepareHeaders(
            new Dictionary<string, string> { ["init"] = "foo" },
            new Dictionary<string, string> { ["content-type"] = "application/json" });
        Assert.Equal("foo", headers.Get("init"));
        Assert.Equal("application/json", headers.Get("Content-Type"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/prepare-headers.test.ts::prepareHeaders::should handle Response object headers", Coverage = UpstreamCoverage.Covered)]
    public void Copies_response_headers()
    {
        var headers = HttpHeaderPreparation.PrepareHeaders(
            new Dictionary<string, string> { ["init"] = "foo", ["extra"] = "bar" },
            new Dictionary<string, string> { ["content-type"] = "application/json" });
        Assert.Equal("foo", headers.Get("init"));
        Assert.Equal("bar", headers.Get("extra"));
        Assert.Equal("application/json", headers.Get("Content-Type"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/prepare-retries.test.ts::should set default values correctly when no input is provided", Coverage = UpstreamCoverage.Covered)]
    public void Prepare_retries_defaults_to_two()
    {
        var prepared = Retries.PrepareRetries(null);
        Assert.Equal(2, prepared.MaxRetries);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/retry-with-exponential-backoff.test.ts::retryWithExponentialBackoffRespectingRetryHeaders::should use rate limit header delay when present and reasonable",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_a_reasonable_retry_after_ms_header()
    {
        var delays = new List<int>();
        var result = await Run(attempt =>
        {
            if (attempt == 1)
            {
                throw RateLimit("retry-after-ms", "3000");
            }

            return Task.FromResult("success");
        }, delays);
        Assert.Equal("success", result);
        Assert.Equal(new[] { 3000 }, delays);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/retry-with-exponential-backoff.test.ts::retryWithExponentialBackoffRespectingRetryHeaders::should parse retry-after header in seconds",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Parses_retry_after_seconds()
    {
        var delays = new List<int>();
        var result = await Run(attempt =>
        {
            if (attempt == 1)
            {
                throw RateLimit("retry-after", "5");
            }

            return Task.FromResult("success");
        }, delays);
        Assert.Equal("success", result);
        Assert.Equal(new[] { 5000 }, delays);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/retry-with-exponential-backoff.test.ts::retryWithExponentialBackoffRespectingRetryHeaders::should use exponential backoff when rate limit delay is too long",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Ignores_a_rate_limit_delay_over_sixty_seconds()
    {
        var delays = new List<int>();
        var result = await Run(attempt =>
        {
            if (attempt == 1)
            {
                throw RateLimit("retry-after-ms", "70000");
            }

            return Task.FromResult("success");
        }, delays, initialDelayInMs: 2000);
        Assert.Equal("success", result);
        Assert.Equal(new[] { 2000 }, delays);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/retry-with-exponential-backoff.test.ts::retryWithExponentialBackoffRespectingRetryHeaders::should fall back to exponential backoff when no rate limit headers",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_exponential_backoff_without_headers()
    {
        var delays = new List<int>();
        var result = await Run(attempt =>
        {
            if (attempt == 1)
            {
                throw new APICallError("Temporary error", "https://api.example.com", isRetryable: true, responseHeaders: new Dictionary<string, string>());
            }

            return Task.FromResult("success");
        }, delays, initialDelayInMs: 2000);
        Assert.Equal("success", result);
        Assert.Equal(new[] { 2000 }, delays);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/retry-with-exponential-backoff.test.ts::retryWithExponentialBackoffRespectingRetryHeaders::should handle invalid rate limit header values",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Falls_back_when_rate_limit_headers_are_invalid()
    {
        var delays = new List<int>();
        var result = await Run(attempt =>
        {
            if (attempt == 1)
            {
                throw new APICallError(
                    "Rate limited",
                    "https://api.example.com",
                    isRetryable: true,
                    responseHeaders: new Dictionary<string, string>
                    {
                        ["retry-after-ms"] = "invalid",
                        ["retry-after"] = "not-a-number",
                    });
            }

            return Task.FromResult("success");
        }, delays, initialDelayInMs: 2000);
        Assert.Equal("success", result);
        Assert.Equal(new[] { 2000 }, delays);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/retry-with-exponential-backoff.test.ts::retryWithExponentialBackoffRespectingRetryHeaders > with mocked provider responses::should handle Anthropic 429 response with retry-after-ms header",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_an_anthropic_retry_after_ms_header()
    {
        var delays = new List<int>();
        var result = await Run(attempt =>
        {
            if (attempt == 1)
            {
                throw new APICallError(
                    "Rate limit exceeded",
                    "https://api.anthropic.com/v1/messages",
                    statusCode: 429,
                    isRetryable: true,
                    responseHeaders: new Dictionary<string, string> { ["retry-after-ms"] = "5000", ["x-request-id"] = "req_123456" });
            }

            return Task.FromResult("Hello from Claude!");
        }, delays);
        Assert.Equal("Hello from Claude!", result);
        Assert.Equal(new[] { 5000 }, delays);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/retry-with-exponential-backoff.test.ts::retryWithExponentialBackoffRespectingRetryHeaders > with mocked provider responses::should handle OpenAI 429 response with retry-after header",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_an_openai_retry_after_header()
    {
        var delays = new List<int>();
        var result = await Run(attempt =>
        {
            if (attempt == 1)
            {
                throw new APICallError(
                    "Rate limit reached for requests",
                    "https://api.openai.com/v1/chat/completions",
                    statusCode: 429,
                    isRetryable: true,
                    responseHeaders: new Dictionary<string, string> { ["retry-after"] = "30" });
            }

            return Task.FromResult("Hello from GPT!");
        }, delays);
        Assert.Equal("Hello from GPT!", result);
        Assert.Equal(new[] { 30000 }, delays);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/retry-with-exponential-backoff.test.ts::retryWithExponentialBackoffRespectingRetryHeaders > with mocked provider responses::should prefer retry-after-ms over retry-after when both present",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Prefers_retry_after_ms()
    {
        var delays = new List<int>();
        var result = await Run(attempt =>
        {
            if (attempt == 1)
            {
                throw new APICallError(
                    "Rate limited",
                    "https://api.example.com/v1/messages",
                    statusCode: 429,
                    isRetryable: true,
                    responseHeaders: new Dictionary<string, string> { ["retry-after-ms"] = "3000", ["retry-after"] = "10" });
            }

            return Task.FromResult("success");
        }, delays);
        Assert.Equal("success", result);
        Assert.Equal(new[] { 3000 }, delays);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/retry-with-exponential-backoff.test.ts::retryWithExponentialBackoffRespectingRetryHeaders > with mocked provider responses::should handle retry-after header with HTTP date format",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Parses_an_http_date_retry_after_header()
    {
        var delays = new List<int>();
        var header = DateTimeOffset.UtcNow.AddMilliseconds(5000).ToString("r", CultureInfo.InvariantCulture);
        var result = await Run(attempt =>
        {
            if (attempt == 1)
            {
                throw new APICallError(
                    "Rate limit exceeded",
                    "https://api.example.com/v1/endpoint",
                    statusCode: 429,
                    isRetryable: true,
                    responseHeaders: new Dictionary<string, string> { ["retry-after"] = header });
            }

            return Task.FromResult("success");
        }, delays);
        Assert.Equal("success", result);
        Assert.Single(delays);
        Assert.InRange(delays[0], 3000, 6000);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/retry-with-exponential-backoff.test.ts::retryWithExponentialBackoffRespectingRetryHeaders > with mocked provider responses::should fall back to exponential backoff when rate limit delay is negative",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Ignores_a_negative_rate_limit_delay()
    {
        var delays = new List<int>();
        var result = await Run(attempt =>
        {
            if (attempt == 1)
            {
                throw RateLimit("retry-after-ms", "-1000");
            }

            return Task.FromResult("success");
        }, delays, initialDelayInMs: 2000);
        Assert.Equal("success", result);
        Assert.Equal(new[] { 2000 }, delays);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/retry-with-exponential-backoff.test.ts::retryWithExponentialBackoffRespectingRetryHeaders > with Gateway errors::should retry on GatewayInternalServerError",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Retries_a_gateway_internal_server_error()
    {
        var delays = new List<int>();
        var result = await Run(attempt =>
        {
            if (attempt == 1)
            {
                throw new GatewayInternalServerError("Internal server error", 503);
            }

            return Task.FromResult("success");
        }, delays, initialDelayInMs: 2000);
        Assert.Equal("success", result);
        Assert.Equal(new[] { 2000 }, delays);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/retry-with-exponential-backoff.test.ts::retryWithExponentialBackoffRespectingRetryHeaders > with Gateway errors::should retry on GatewayRateLimitError",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Retries_a_gateway_rate_limit_error()
    {
        var delays = new List<int>();
        var result = await Run(attempt =>
        {
            if (attempt == 1)
            {
                throw new GatewayRateLimitError("Rate limit exceeded");
            }

            return Task.FromResult("success");
        }, delays, initialDelayInMs: 2000);
        Assert.Equal("success", result);
        Assert.Equal(new[] { 2000 }, delays);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/retry-with-exponential-backoff.test.ts::retryWithExponentialBackoffRespectingRetryHeaders > with Gateway errors::should not retry on non-retryable GatewayAuthenticationError",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_retry_a_gateway_authentication_error()
    {
        var attempts = 0;
        var error = await Assert.ThrowsAsync<GatewayAuthenticationError>(() => Run<string>(_ =>
        {
            attempts++;
            return Task.FromException<string>(new GatewayAuthenticationError("Invalid API key"));
        }, new List<int>()));
        Assert.Equal("Invalid API key", error.Message);
        Assert.Equal(1, attempts);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/retry-with-exponential-backoff.test.ts::retryWithExponentialBackoffRespectingRetryHeaders > with Gateway errors::should use retry-after headers from APICallError cause",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_retry_headers_from_the_cause()
    {
        var delays = new List<int>();
        var result = await Run(attempt =>
        {
            if (attempt == 1)
            {
                var cause = new APICallError(
                    "Service unavailable",
                    "https://api.example.com",
                    statusCode: 503,
                    isRetryable: true,
                    responseHeaders: new Dictionary<string, string> { ["retry-after-ms"] = "3000" });
                throw new GatewayInternalServerError("Internal server error", 503, cause);
            }

            return Task.FromResult("success");
        }, delays);
        Assert.Equal("success", result);
        Assert.Equal(new[] { 3000 }, delays);
    }

    private static APICallError RateLimit(string header, string value)
    {
        return new APICallError(
            "Rate limited",
            "https://api.example.com",
            isRetryable: true,
            responseHeaders: new Dictionary<string, string> { [header] = value });
    }

    private static Task<T> Run<T>(Func<int, Task<T>> operation, List<int> delays, int? maxRetries = null, int initialDelayInMs = 2000)
    {
        var attempt = 0;
        var retry = Retries.RetryWithExponentialBackoffRespectingRetryHeaders(
            maxRetries ?? 2,
            initialDelayInMs,
            delay: (milliseconds, _) =>
            {
                delays.Add(milliseconds);
                return Task.CompletedTask;
            });
        return retry.ExecuteAsync(() =>
        {
            attempt++;
            return operation(attempt);
        });
    }
}
