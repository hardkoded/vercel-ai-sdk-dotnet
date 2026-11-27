// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AmazonBedrock;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockApiTypes;

public sealed class CreateAmazonBedrockCachePointTests
{
    private const string Prefix =
        "packages/amazon-bedrock/src/amazon-bedrock-api-types.test.ts::createAmazonBedrockCachePoint::";

    [Fact]
    [UpstreamTest(
        Prefix + "should return default cache point when no TTL is provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_return_default_cache_point_when_no_TTL_is_provided()
    {
        Assert.Equal(
            "{\"cachePoint\":{\"type\":\"default\"}}",
            global::Vercel
                .AI.AmazonBedrock.AmazonBedrockApiTypes.CreateAmazonBedrockCachePoint()
                .ToJsonString()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should create cache point with 5m TTL",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_create_cache_point_with_5m_TTL()
    {
        Assert.Equal(
            "{\"cachePoint\":{\"type\":\"default\",\"ttl\":\"5m\"}}",
            global::Vercel
                .AI.AmazonBedrock.AmazonBedrockApiTypes.CreateAmazonBedrockCachePoint("5m")
                .ToJsonString()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should create cache point with 1h TTL",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_create_cache_point_with_1h_TTL()
    {
        Assert.Equal(
            "{\"cachePoint\":{\"type\":\"default\",\"ttl\":\"1h\"}}",
            global::Vercel
                .AI.AmazonBedrock.AmazonBedrockApiTypes.CreateAmazonBedrockCachePoint("1h")
                .ToJsonString()
        );
    }
}
