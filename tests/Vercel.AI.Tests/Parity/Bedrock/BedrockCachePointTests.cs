// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;

namespace Vercel.AI.Tests;

/// <summary>Cache-point bodies from the Bedrock API types tests.</summary>
public sealed class BedrockCachePointTests
{
    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-api-types.test.ts::createAmazonBedrockCachePoint::should return default cache point when no TTL is provided", Coverage = UpstreamCoverage.Covered)]
    public void Omits_ttl_when_the_cache_point_uses_the_default()
    {
        var point = AmazonBedrockCachePoint.Create();

        Assert.Equal("default", point["cachePoint"]!["type"]!.GetValue<string>());
        Assert.Null(point["cachePoint"]!["ttl"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-api-types.test.ts::createAmazonBedrockCachePoint::should create cache point with 5m TTL", Coverage = UpstreamCoverage.Covered)]
    public void Creates_a_five_minute_cache_point()
    {
        var point = AmazonBedrockCachePoint.Create("5m");

        Assert.Equal("default", point["cachePoint"]!["type"]!.GetValue<string>());
        Assert.Equal("5m", point["cachePoint"]!["ttl"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-api-types.test.ts::createAmazonBedrockCachePoint::should create cache point with 1h TTL", Coverage = UpstreamCoverage.Covered)]
    public void Creates_a_one_hour_cache_point()
    {
        var point = AmazonBedrockCachePoint.Create("1h");

        Assert.Equal("default", point["cachePoint"]!["type"]!.GetValue<string>());
        Assert.Equal("1h", point["cachePoint"]!["ttl"]!.GetValue<string>());
    }
}
