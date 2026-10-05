// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests;

namespace Vercel.AI.Tests.Upstream.ProviderUtils;

public sealed class IdAndRetryTests
{
    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/generate-id.test.ts::createIdGenerator::should generate an ID with the correct length",
        Coverage = UpstreamCoverage.Covered)]
    public void Id_generator_uses_the_requested_length()
    {
        var generate = IdGenerator.Create(size: 10);
        Assert.Equal(10, generate().Length);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/generate-id.test.ts::createIdGenerator::should generate an ID with the correct default length",
        Coverage = UpstreamCoverage.Covered)]
    public void Id_generator_defaults_to_sixteen_characters()
    {
        Assert.Equal(16, IdGenerator.Create()().Length);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/generate-id.test.ts::createIdGenerator::should throw an error if the separator is part of the alphabet",
        Coverage = UpstreamCoverage.Covered)]
    public void Id_generator_rejects_a_separator_that_is_in_the_alphabet()
    {
        var error = Assert.Throws<ArgumentException>(() => IdGenerator.Create(prefix: "b", separator: "a"));
        Assert.Contains("The separator \"a\" must not be part of the alphabet", error.Message);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/generate-id.test.ts::generateId::should generate unique IDs",
        Coverage = UpstreamCoverage.Covered)]
    public void Generate_produces_distinct_ids()
    {
        Assert.NotEqual(IdGenerator.Generate(), IdGenerator.Generate());
    }

    [Fact]
    public async Task Retry_rethrows_the_original_error_when_retries_are_disabled()
    {
        var error = new InvalidOperationException("nope");
        var caught = await Assert.ThrowsAsync<InvalidOperationException>(() => ExponentialBackoff.RetryAsync<int>(
            () => Task.FromException<int>(error),
            _ => Task.FromResult(true),
            new RetryOptions { MaxRetries = 0, InitialDelayInMs = 0 }));
        Assert.Same(error, caught);
    }

    [Fact]
    public async Task Retry_does_not_repeat_an_abort()
    {
        var calls = 0;
        await Assert.ThrowsAsync<DelayAbortedException>(() => ExponentialBackoff.RetryAsync<int>(
            () =>
            {
                calls++;
                return Task.FromException<int>(new DelayAbortedException());
            },
            _ => Task.FromResult(true),
            new RetryOptions { MaxRetries = 2, InitialDelayInMs = 0 }));
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task Retry_returns_the_value_from_a_later_attempt()
    {
        var attempts = 0;
        var value = await ExponentialBackoff.RetryAsync(
            () =>
            {
                attempts++;
                if (attempts < 2)
                {
                    return Task.FromException<int>(new InvalidOperationException("again"));
                }

                return Task.FromResult(7);
            },
            _ => Task.FromResult(true),
            new RetryOptions { MaxRetries = 2, InitialDelayInMs = 0 });
        Assert.Equal(7, value);
        Assert.Equal(2, attempts);
    }

    [Fact]
    public async Task Retry_rethrows_the_first_error_that_must_not_be_retried()
    {
        var error = new InvalidOperationException("stop");
        var caught = await Assert.ThrowsAsync<InvalidOperationException>(() => ExponentialBackoff.RetryAsync<int>(
            () => Task.FromException<int>(error),
            _ => Task.FromResult(false),
            new RetryOptions { MaxRetries = 2, InitialDelayInMs = 0 }));
        Assert.Same(error, caught);
    }

    [Fact]
    public async Task Retry_wraps_errors_after_the_budget_is_spent()
    {
        var caught = await Assert.ThrowsAsync<RetryException>(() => ExponentialBackoff.RetryAsync<int>(
            () => Task.FromException<int>(new InvalidOperationException("x")),
            _ => Task.FromResult(true),
            new RetryOptions { MaxRetries = 2, InitialDelayInMs = 0 }));
        Assert.Equal(RetryErrorReason.MaxRetriesExceeded, caught.Reason);
        Assert.Equal(3, caught.Errors.Count);
        Assert.Contains("Failed after 3 attempts", caught.Message);
    }
}
