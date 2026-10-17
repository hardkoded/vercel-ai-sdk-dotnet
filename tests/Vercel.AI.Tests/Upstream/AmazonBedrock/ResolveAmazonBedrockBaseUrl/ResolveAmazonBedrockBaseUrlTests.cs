// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AmazonBedrock;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.ResolveAmazonBedrockBaseUrl;

[Collection("BedrockEndpointEnvironment")]
public sealed class ResolveAmazonBedrockBaseUrlTests : BedrockEndpointEnvironment
{
    private const string File = "packages/amazon-bedrock/src/resolve-amazon-bedrock-base-url.test.ts::resolveAmazonBedrockBaseURL::";

    private static string Resolve(string region = "us-east-1", string? baseUrl = null) =>
        AmazonBedrockEndpoints.Resolve(baseUrl, region, "bedrock-runtime");

    [Fact]
    [UpstreamTest(File + "generates the regional endpoint for a DNS-label region", Coverage = UpstreamCoverage.Covered)]
    public void Generates_the_regional_endpoint_for_a_DNS_label_region()
    {
        Assert.Equal("https://bedrock-runtime.us-east-1.amazonaws.com", Resolve("us-east-1"));
    }

    [Fact]
    [UpstreamTest(File + "uses the partition DNS suffix of the region", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_partition_DNS_suffix_of_the_region()
    {
        Assert.Equal("https://bedrock-runtime.cn-north-1.amazonaws.com.cn", Resolve("cn-north-1"));
        Assert.Equal("https://bedrock-runtime.us-iso-east-1.c2s.ic.gov", Resolve("us-iso-east-1"));
        Assert.Equal("https://bedrock-runtime.eusc-de-east-1.amazonaws.eu", Resolve("eusc-de-east-1"));
    }

    [Fact]
    [UpstreamTest(File + "prefers an explicit base URL over the generated endpoint", Coverage = UpstreamCoverage.Covered)]
    public void Prefers_an_explicit_base_URL_over_the_generated_endpoint()
    {
        Assert.Equal("https://localhost:4566", Resolve("us-east-1", "https://localhost:4566"));
    }

    [Fact]
    [UpstreamTest(File + "prefers the service endpoint environment variable over the generated endpoint", Coverage = UpstreamCoverage.Covered)]
    public void Prefers_the_service_endpoint_environment_variable_over_the_generated_endpoint()
    {
        Environment.SetEnvironmentVariable("AWS_ENDPOINT_URL_BEDROCK_RUNTIME", "https://runtime.example.com/");

        Assert.Equal("https://runtime.example.com", Resolve());
    }

    [Theory]
    [InlineData("evil.example.com/#")]
    [InlineData("user@internal:8080/#")]
    [InlineData("169.254.169.254:80/x#")]
    [InlineData("us-east-1/../..")]
    [InlineData("us east 1")]
    [InlineData("")]
    [UpstreamTest(File + "rejects region %j because it would rewrite the request host", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_region_because_it_would_rewrite_the_request_host(string region)
    {
        var error = Assert.Throws<ArgumentException>(() => Resolve(region));

        Assert.Equal("region", error.ParamName);
    }

    [Fact]
    [UpstreamTest(File + "does not reject the region when an explicit endpoint is configured", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_reject_the_region_when_an_explicit_endpoint_is_configured()
    {
        // custom endpoints do not use the region as part of the request host
        Environment.SetEnvironmentVariable("AWS_ENDPOINT_URL", "https://localhost:4566");

        Assert.Equal("https://localhost:4566", Resolve("not a region"));
    }
}
