// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AmazonBedrock;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockProvider;

public sealed class CreateAmazonBedrockTests
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
}
