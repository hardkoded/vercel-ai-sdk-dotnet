// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Testing;

namespace Vercel.AI.Tests;

public sealed class MockModelTests
{
    [UpstreamTest("packages/ai/src/test/mock-language-model.test.ts::MockLanguageModelV2::returns array-backed generate results from the first entry", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Language_v2_returns_generate_results_in_order()
    {
        var model = new MockLanguageModelV2(doGenerate: new[] { new MockGenerateResult("first"), new MockGenerateResult("second") });
        Assert.Equal("v2", model.SpecificationVersion);
        var first = await model.DoGenerateAsync(new MockLanguageModelCallOptions());
        var second = await model.DoGenerateAsync(new MockLanguageModelCallOptions());
        Assert.Equal("text", first.Content[0].Type);
        Assert.Equal("first", first.Content[0].Text);
        Assert.Equal("second", second.Content[0].Text);
    }

    [UpstreamTest("packages/ai/src/test/mock-language-model.test.ts::MockLanguageModelV2::returns array-backed stream results from the first entry", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Language_v2_returns_stream_results_in_order()
    {
        var model = new MockLanguageModelV2(doStream: new[] { new MockStreamResult("first"), new MockStreamResult("second") });
        Assert.Equal("first", TextOf(await model.DoStreamAsync(new MockLanguageModelCallOptions())));
        Assert.Equal("second", TextOf(await model.DoStreamAsync(new MockLanguageModelCallOptions())));
        Assert.Equal(2, model.DoStreamCalls.Count);
    }

    [UpstreamTest("packages/ai/src/test/mock-language-model.test.ts::MockLanguageModelV3::returns array-backed generate results from the first entry", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Language_v3_returns_generate_results_in_order()
    {
        var model = new MockLanguageModelV3(doGenerate: new[] { new MockGenerateResult("first"), new MockGenerateResult("second") });
        Assert.Equal("v3", model.SpecificationVersion);
        Assert.Equal("first", (await model.DoGenerateAsync(new MockLanguageModelCallOptions())).Content[0].Text);
        Assert.Equal("second", (await model.DoGenerateAsync(new MockLanguageModelCallOptions())).Content[0].Text);
    }

    [UpstreamTest("packages/ai/src/test/mock-language-model.test.ts::MockLanguageModelV3::returns array-backed stream results from the first entry", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Language_v3_returns_stream_results_in_order()
    {
        var model = new MockLanguageModelV3(doStream: new[] { new MockStreamResult("first"), new MockStreamResult("second") });
        Assert.Equal("first", TextOf(await model.DoStreamAsync(new MockLanguageModelCallOptions())));
        Assert.Equal("second", TextOf(await model.DoStreamAsync(new MockLanguageModelCallOptions())));
        Assert.Equal(2, model.DoStreamCalls.Count);
    }

    [UpstreamTest("packages/ai/src/test/mock-language-model.test.ts::MockLanguageModelV4::returns array-backed generate results from the first entry", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Language_v4_returns_generate_results_in_order()
    {
        var model = new MockLanguageModelV4(doGenerate: new[] { new MockGenerateResult("first"), new MockGenerateResult("second") });
        Assert.Equal("v4", model.SpecificationVersion);
        Assert.Equal("first", (await model.DoGenerateAsync(new MockLanguageModelCallOptions())).Content[0].Text);
        Assert.Equal("second", (await model.DoGenerateAsync(new MockLanguageModelCallOptions())).Content[0].Text);
    }

    [UpstreamTest("packages/ai/src/test/mock-language-model.test.ts::MockLanguageModelV4::returns array-backed stream results from the first entry", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Language_v4_returns_stream_results_in_order()
    {
        var model = new MockLanguageModelV4(doStream: new[] { new MockStreamResult("first"), new MockStreamResult("second") });
        Assert.Equal("first", TextOf(await model.DoStreamAsync(new MockLanguageModelCallOptions())));
        Assert.Equal("second", TextOf(await model.DoStreamAsync(new MockLanguageModelCallOptions())));
        Assert.Equal(2, model.DoStreamCalls.Count);
    }

    [UpstreamTest("packages/ai/src/test/mock-embedding-model.test.ts::MockEmbeddingModelV3::returns array-backed embed results from the first entry", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Embedding_v3_returns_results_in_order()
    {
        var model = new MockEmbeddingModelV3(doEmbed: new[] { new MockEmbedResult(new[] { 1f }), new MockEmbedResult(new[] { 2f }) });
        Assert.Equal("v3", model.SpecificationVersion);
        Assert.Equal(new[] { 1f }, (await model.DoEmbedAsync(new MockEmbedOptions(new[] { "first" }))).Embeddings[0]);
        Assert.Equal(new[] { 2f }, (await model.DoEmbedAsync(new MockEmbedOptions(new[] { "second" }))).Embeddings[0]);
    }

    [UpstreamTest("packages/ai/src/test/mock-embedding-model.test.ts::MockEmbeddingModelV4::returns array-backed embed results from the first entry", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Embedding_v4_returns_results_in_order()
    {
        var model = new MockEmbeddingModelV4(doEmbed: new[] { new MockEmbedResult(new[] { 1f }), new MockEmbedResult(new[] { 2f }) });
        Assert.Equal("v4", model.SpecificationVersion);
        Assert.Equal(new[] { 1f }, (await model.DoEmbedAsync(new MockEmbedOptions(new[] { "first" }))).Embeddings[0]);
        Assert.Equal(new[] { 2f }, (await model.DoEmbedAsync(new MockEmbedOptions(new[] { "second" }))).Embeddings[0]);
    }

    private static string TextOf(MockStreamResult result)
    {
        var text = string.Empty;
        for (var i = 0; i < result.Chunks.Count; i++)
        {
            if (result.Chunks[i].Type == "text-delta")
            {
                text += result.Chunks[i].Delta;
            }
        }

        return text;
    }
}
