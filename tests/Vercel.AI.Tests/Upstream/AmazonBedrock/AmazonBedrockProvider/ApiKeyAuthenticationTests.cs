// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AmazonBedrock;
using Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockImageModel;
using BedrockProvider = Vercel.AI.AmazonBedrock.AmazonBedrockProvider;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockProvider;

public sealed class ApiKeyAuthenticationTests
{
    private const string Prefix =
        "packages/amazon-bedrock/src/amazon-bedrock-provider.test.ts::AmazonBedrockProvider > createAmazonBedrock > API Key Authentication::";

    private static BedrockProvider CreateProvider(UpstreamRecordingHandler handler) =>
        BedrockProvider.Create(
            new AmazonBedrockOptions
            {
                ApiKey = "test-api-key",
                Region = "us-east-1",
                Headers = new Dictionary<string, string?> { ["custom-header"] = "value" },
            },
            handler
        );

    private static void AssertApiKeyRequest(UpstreamRecordingHandler handler)
    {
        Assert.Equal("Bearer test-api-key", handler.Headers["Authorization"]);
        Assert.Equal("value", handler.Headers["custom-header"]);
        Assert.Contains("ai-sdk-amazon-bedrock/", handler.Headers["user-agent"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should work with embedding models when using API key",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_work_with_embedding_models_when_using_API_key()
    {
        var handler = new UpstreamRecordingHandler(
            "{\"embedding\":[0.1,0.2],\"inputTextTokenCount\":3}"
        );

        await CreateProvider(handler)
            .EmbeddingModel("amazon.titan-embed-text-v1")
            .DoEmbedAsync(new[] { "hello" }, null, CancellationToken.None);

        AssertApiKeyRequest(handler);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should work with image models when using API key",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_work_with_image_models_when_using_API_key()
    {
        var handler = new UpstreamRecordingHandler(BedrockImageTestSupport.ImagesBody);

        await CreateProvider(handler)
            .Image("amazon.titan-image-generator")
            .DoGenerateAsync(BedrockImageTestSupport.Call(), CancellationToken.None);

        AssertApiKeyRequest(handler);
    }
}
