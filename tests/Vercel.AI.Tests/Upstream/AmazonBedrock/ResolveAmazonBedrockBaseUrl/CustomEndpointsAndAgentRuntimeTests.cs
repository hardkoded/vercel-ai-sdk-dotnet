// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AmazonBedrock;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.ResolveAmazonBedrockBaseUrl;

[Collection("BedrockEndpointEnvironment")]
public sealed class CustomEndpointsAndAgentRuntimeTests : BedrockEndpointEnvironment
{
    private const string File = "packages/amazon-bedrock/src/resolve-amazon-bedrock-base-url.test.ts::custom endpoints and agent runtime::";

    [Theory]
    [InlineData("AWS_ENDPOINT_URL")]
    [InlineData("AWS_ENDPOINT_URL_BEDROCK_RUNTIME")]
    [UpstreamTest(File + "does not validate an unused region with %s", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_validate_an_unused_region_with(string name)
    {
        Environment.SetEnvironmentVariable(name, "https://proxy.example/");

        Assert.Equal("https://proxy.example", AmazonBedrockEndpoints.Resolve(null, "user@internal/#", "bedrock-runtime"));
    }

    [Fact]
    [UpstreamTest(File + "does not load the region with an explicit endpoint", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_load_the_region_with_an_explicit_endpoint()
    {
        Assert.Equal(
            "https://proxy.example",
            AmazonBedrockEndpoints.Resolve("https://proxy.example/", () => throw new InvalidOperationException("must not load"), "bedrock-agent-runtime"));
    }

    [Fact]
    [UpstreamTest(File + "validates the agent runtime region", Coverage = UpstreamCoverage.Covered)]
    public void Validates_the_agent_runtime_region()
    {
        Assert.Throws<ArgumentException>(() => AmazonBedrockEndpoints.Resolve(null, "user@internal/#", "bedrock-agent-runtime"));
    }
}
