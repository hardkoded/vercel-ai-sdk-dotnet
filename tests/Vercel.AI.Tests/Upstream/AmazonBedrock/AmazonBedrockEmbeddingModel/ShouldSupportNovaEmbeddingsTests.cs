// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockEmbeddingModel;

/// <summary>Port of <c>amazon-bedrock-embedding-model.test.ts</c> &gt; <c>should support Nova embeddings</c>.</summary>
public sealed class ShouldSupportNovaEmbeddingsTests
{
    private const string NovaResponse = "{\"embeddings\":[{\"embeddingType\":\"TEXT\",\"embedding\":[-0.09,0.05,-0.02,0.01,0.04]}],\"inputTokenCount\":8}";

    public static TheoryData<int?, int?, int?> Cases => new()
    {
        { null, null, 1024 },
        { 256, null, 256 },
        { null, 384, 384 },
        { 256, 384, 384 },
    };

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-embedding-model.test.ts::should support Nova embeddings::maps dimensions $dimensions with provider override $providerDimensions to $expected", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_dimensions_with_provider_override(int? dimensions, int? providerDimensions, int? expected)
    {
        var (model, handler) = Create();

        await model.DoEmbedAsync(new[] { "sunny day at the beach" }, EmbeddingDimensionCases.Options("amazonBedrock", "embeddingDimension", providerDimensions), CancellationToken.None, dimensions);

        var body = JsonNode.Parse(handler.Body)!;
        Assert.Equal(expected, body["singleEmbeddingParams"]!["embeddingDimension"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-embedding-model.test.ts::should support Nova embeddings::should send SINGLE_EMBEDDING payload for Nova embeddings", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_SINGLE_EMBEDDING_payload_for_Nova_embeddings()
    {
        var (model, handler) = Create();

        var result = await model.DoEmbedAsync(new[] { "sunny day at the beach" }, null, CancellationToken.None);

        Assert.Equal(new[] { -0.09f, 0.05f, -0.02f, 0.01f, 0.04f }, result.Embeddings[0]);
        JsonAssert.Equal(JsonNode.Parse(handler.Body), "{\"taskType\":\"SINGLE_EMBEDDING\",\"singleEmbeddingParams\":{\"embeddingPurpose\":\"GENERIC_INDEX\",\"embeddingDimension\":1024,\"text\":{\"truncationMode\":\"END\",\"value\":\"sunny day at the beach\"}}}");
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-embedding-model.test.ts::should support Nova embeddings::should pass embeddingDimension for Nova embeddings", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_embeddingDimension_for_Nova_embeddings()
    {
        var (model, handler) = Create();

        var result = await model.DoEmbedAsync(new[] { "sunny day at the beach" }, EmbeddingDimensionCases.Options("bedrock", "embeddingDimension", 256), CancellationToken.None);

        Assert.Equal(new[] { -0.09f, 0.05f, -0.02f, 0.01f, 0.04f }, result.Embeddings[0]);
        JsonAssert.Equal(JsonNode.Parse(handler.Body), "{\"taskType\":\"SINGLE_EMBEDDING\",\"singleEmbeddingParams\":{\"embeddingPurpose\":\"GENERIC_INDEX\",\"embeddingDimension\":256,\"text\":{\"truncationMode\":\"END\",\"value\":\"sunny day at the beach\"}}}");
    }

    private static (Vercel.AI.Provider.IEmbeddingModel Model, UpstreamRecordingHandler Handler) Create()
    {
        var handler = new UpstreamRecordingHandler(NovaResponse);
        var provider = AmazonBedrockProvider.Create(new AmazonBedrockOptions { AccessKeyId = "AKIA", SecretAccessKey = "secret" }, handler);
        return (provider.EmbeddingModel("amazon.nova-2-multimodal-embeddings-v1:0"), handler);
    }
}
