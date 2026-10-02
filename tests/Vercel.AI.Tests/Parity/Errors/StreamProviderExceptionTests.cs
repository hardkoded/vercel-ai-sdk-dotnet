// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Error;

namespace Vercel.AI.Tests;

public sealed class StreamProviderExceptionTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/error/stream-provider-error.test.ts::StreamProviderError::exposes provider metadata and preserves the raw payload",
        Coverage = UpstreamCoverage.Covered)]
    public void Exposes_provider_metadata_and_preserves_the_raw_payload()
    {
        var data = new Dictionary<string, object>
        {
            ["message"] = "Overloaded",
            ["type"] = "overloaded_error",
            ["code"] = "provider_overloaded",
        };

        var error = new StreamProviderException(
            (string)data["message"],
            (string)data["type"],
            (string)data["code"],
            529,
            data: data);

        Assert.IsAssignableFrom<Exception>(error);
        Assert.Equal("Overloaded", error.Message);
        Assert.Equal("overloaded_error", error.Type ?? string.Empty);
        Assert.Equal("provider_overloaded", error.Code as string ?? string.Empty);
        Assert.Equal(529, error.StatusCode ?? 0);
        Assert.True(error.IsRetryable);
        Assert.True(ReferenceEquals(data, error.Data));
        Assert.True(StreamProviderException.IsInstance(error));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/error/stream-provider-error.test.ts::StreamProviderError::uses an explicit retryability value",
        Coverage = UpstreamCoverage.Covered)]
    public void Uses_an_explicit_retryability_value()
    {
        var error = new StreamProviderException("Do not retry", statusCode: 503, isRetryable: false);

        Assert.False(error.IsRetryable);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/error/stream-provider-error.test.ts::StreamProviderError::supports marker-based identification across package copies",
        Coverage = UpstreamCoverage.Covered)]
    public void Supports_marker_based_identification_across_package_copies()
    {
        var foreign = new Dictionary<string, object>
        {
            ["vercel.ai.error.AI_StreamProviderError"] = true,
        };

        Assert.True(StreamProviderException.IsInstance(foreign));
    }
}
