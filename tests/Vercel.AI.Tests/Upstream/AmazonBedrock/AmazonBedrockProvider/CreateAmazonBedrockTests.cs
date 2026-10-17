// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AmazonBedrock;

using BedrockProvider = Vercel.AI.AmazonBedrock.AmazonBedrockProvider;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockProvider;

[Collection("BedrockEndpointEnvironment")]
public sealed class CreateAmazonBedrockTests : BedrockEndpointEnvironment
{
    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/amazon-bedrock-provider.test.ts::AmazonBedrockProvider > createAmazonBedrock::should pass headers to embedding model",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_headers_to_embedding_model()
    {
        var handler = new UpstreamRecordingHandler("{\"embedding\":[0.1,0.2],\"inputTextTokenCount\":3}");
        var provider = global::Vercel.AI.AmazonBedrock.AmazonBedrockProvider.Create(new AmazonBedrockOptions
        {
            AccessKeyId = "test-access-key",
            SecretAccessKey = "test-secret",
            Headers = new Dictionary<string, string?> { ["x-custom-header"] = "custom-value" },
        }, handler);

        await provider.EmbeddingModel("amazon.titan-embed-text-v2:0").DoEmbedAsync(new[] { "hello" }, null, CancellationToken.None);

        Assert.Equal("custom-value", handler.Headers["x-custom-header"]);
        Assert.Contains("ai-sdk-amazon-bedrock", handler.Headers["user-agent"]);
    }

    private const string File = "packages/amazon-bedrock/src/amazon-bedrock-provider.test.ts::AmazonBedrockProvider > createAmazonBedrock::";

    private static string RuntimeBaseUrl(AmazonBedrockOptions options)
    {
        var uri = BedrockProvider.Create(options).ConverseUri("anthropic.claude-v2");
        return uri.GetLeftPart(UriPartial.Authority);
    }

    [Theory]
    [InlineData("cn-north-1", "amazonaws.com.cn")]
    [InlineData("us-gov-west-1", "amazonaws.com")]
    [InlineData("us-iso-east-1", "c2s.ic.gov")]
    [InlineData("us-isob-east-1", "sc2s.sgov.gov")]
    [InlineData("eu-isoe-west-1", "cloud.adc-e.uk")]
    [InlineData("us-isof-south-1", "csp.hci.ic.gov")]
    [InlineData("eusc-de-east-1", "amazonaws.eu")]
    [UpstreamTest(File + "resolves the Bedrock Runtime endpoint for %s", Coverage = UpstreamCoverage.Covered)]
    public void Resolves_the_Bedrock_Runtime_endpoint_for(string region, string dnsSuffix)
    {
        Assert.Equal($"https://bedrock-runtime.{region}.{dnsSuffix}", RuntimeBaseUrl(new AmazonBedrockOptions { Region = region }));
    }

    [Fact]
    [UpstreamTest(File + "uses an explicit base URL without loading a region", Coverage = UpstreamCoverage.Covered)]
    public void Uses_an_explicit_base_URL_without_loading_a_region()
    {
        Environment.SetEnvironmentVariable("AWS_ENDPOINT_URL_BEDROCK_RUNTIME", "https://runtime.example.com");
        Environment.SetEnvironmentVariable("AWS_ENDPOINT_URL", "https://global.example.com");

        Assert.Equal(
            "https://explicit.example.com",
            RuntimeBaseUrl(new AmazonBedrockOptions { Region = "not a region", BaseUrl = "https://explicit.example.com/" }));
    }

    [Fact]
    [UpstreamTest(File + "prefers the service-specific Bedrock Runtime endpoint over the global endpoint", Coverage = UpstreamCoverage.Covered)]
    public void Prefers_the_service_specific_Bedrock_Runtime_endpoint_over_the_global_endpoint()
    {
        Environment.SetEnvironmentVariable("AWS_ENDPOINT_URL_BEDROCK_RUNTIME", "https://runtime.example.com/");
        Environment.SetEnvironmentVariable("AWS_ENDPOINT_URL", "https://global.example.com");

        Assert.Equal("https://runtime.example.com", RuntimeBaseUrl(new AmazonBedrockOptions { Region = "not a region" }));
    }

    [Fact]
    [UpstreamTest(File + "uses the global endpoint override when no service-specific endpoint is set", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_global_endpoint_override_when_no_service_specific_endpoint_is_set()
    {
        Environment.SetEnvironmentVariable("AWS_ENDPOINT_URL", "https://global.example.com/");

        Assert.Equal("https://global.example.com", RuntimeBaseUrl(new AmazonBedrockOptions { Region = "not a region" }));
    }

    [Fact]
    [UpstreamTest(File + "uses a separate service-specific endpoint for Bedrock Agent Runtime", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_a_separate_service_specific_endpoint_for_Bedrock_Agent_Runtime()
    {
        Environment.SetEnvironmentVariable("AWS_ENDPOINT_URL_BEDROCK_RUNTIME", "https://runtime.example.com");
        Environment.SetEnvironmentVariable("AWS_ENDPOINT_URL_BEDROCK_AGENT_RUNTIME", "https://agent-runtime.example.com");

        var handler = await Rerank(new AmazonBedrockOptions { Region = "us-east-1" });

        Assert.StartsWith("https://agent-runtime.example.com/rerank", handler.Uri);
    }

    [Fact]
    [UpstreamTest(File + "resolves the Bedrock Agent Runtime endpoint for a non-standard AWS partition", Coverage = UpstreamCoverage.Covered)]
    public async Task Resolves_the_Bedrock_Agent_Runtime_endpoint_for_a_non_standard_AWS_partition()
    {
        var handler = await Rerank(new AmazonBedrockOptions { Region = "eu-isoe-west-1" });

        Assert.StartsWith("https://bedrock-agent-runtime.eu-isoe-west-1.cloud.adc-e.uk/rerank", handler.Uri);
    }

    private static async Task<UpstreamRecordingHandler> Rerank(AmazonBedrockOptions options)
    {
        var handler = new UpstreamRecordingHandler("{\"results\":[{\"index\":0,\"relevanceScore\":0.5}]}");
        options.AccessKeyId = "AKIA";
        options.SecretAccessKey = "secret";
        var provider = BedrockProvider.Create(options, handler);
        using var providerOptions = System.Text.Json.JsonDocument.Parse("{}");
        var model = (AmazonBedrockRerankingModel)provider.RerankingModel("amazon.rerank-v1:0");
        await model.DoRerankAsync("q", new[] { "a" }, 1, providerOptions.RootElement, CancellationToken.None);
        return handler;
    }
}
