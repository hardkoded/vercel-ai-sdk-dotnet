// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel.GatewayLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel;

/// <summary>Port of <c>gateway-language-model.test.ts</c> &gt; <c>GatewayLanguageModel &gt; raw chunks filtering</c>.</summary>
public sealed class GatewayLanguageModelRawChunksFilteringTests
{
    private const string Prefix = "packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > raw chunks filtering::";

    [Fact]
    [UpstreamTest(Prefix + "should filter raw chunks based on includeRawChunks option", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_filter_raw_chunks_based_on_includeRawChunks_option()
    {
        var server = StreamChunks(
            "{\"type\":\"stream-start\",\"warnings\":[]}",
            "{\"type\":\"raw\",\"rawValue\":{\"id\":\"test-chunk\",\"object\":\"chat.completion.chunk\",\"choices\":[{\"delta\":{\"content\":\"Hello\"}}]}}",
            "{\"type\":\"text-delta\",\"textDelta\":\"Hello\"}",
            "{\"type\":\"raw\",\"rawValue\":{\"id\":\"test-chunk-2\",\"object\":\"chat.completion.chunk\",\"choices\":[{\"delta\":{\"content\":\" world\"}}]}}",
            "{\"type\":\"text-delta\",\"textDelta\":\" world\"}",
            "{\"type\":\"finish\",\"finishReason\":\"stop\",\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":5}}");
        var options = new LanguageModelCallOptions { Prompt = TestPrompt.Prompt, IncludeRawChunks = false };

        var result = await CreateTestModel(server).DoStreamAsync(options, CancellationToken.None);
        var chunks = await ReadAll(result);

        Assert.Collection(
            chunks,
            part => Assert.Empty(Assert.IsType<StreamStartStreamPart>(part).Warnings),
            part => Assert.Equal("Hello", Assert.IsType<TextDeltaStreamPart>(part).Delta),
            part => Assert.Equal(" world", Assert.IsType<TextDeltaStreamPart>(part).Delta),
            part =>
            {
                Assert.Equal(FinishReason.Stop, Assert.IsType<FinishStreamPart>(part).FinishReason);
                JsonAssert.Equal(part.Chunk!.Value, "{\"finishReason\":\"stop\",\"type\":\"finish\",\"usage\":{\"completion_tokens\":5,\"prompt_tokens\":10}}");
            });
    }

    [Fact]
    [UpstreamTest(Prefix + "should include raw chunks when includeRawChunks is true", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_raw_chunks_when_includeRawChunks_is_true()
    {
        var server = StreamChunks(
            "{\"type\":\"stream-start\",\"warnings\":[]}",
            "{\"type\":\"raw\",\"rawValue\":{\"id\":\"test-chunk\",\"object\":\"chat.completion.chunk\",\"choices\":[{\"delta\":{\"content\":\"Hello\"}}]}}",
            "{\"type\":\"text-delta\",\"textDelta\":\"Hello\"}",
            "{\"type\":\"finish\",\"finishReason\":\"stop\",\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":5}}");
        var options = new LanguageModelCallOptions { Prompt = TestPrompt.Prompt, IncludeRawChunks = true };

        var result = await CreateTestModel(server).DoStreamAsync(options, CancellationToken.None);
        var chunks = await ReadAll(result);

        Assert.Collection(
            chunks,
            part => Assert.Empty(Assert.IsType<StreamStartStreamPart>(part).Warnings),
            part => JsonAssert.Equal(
                JsonNode.Parse(Assert.IsType<RawStreamPart>(part).RawJson),
                "{\"id\":\"test-chunk\",\"object\":\"chat.completion.chunk\",\"choices\":[{\"delta\":{\"content\":\"Hello\"}}]}"),
            part => Assert.Equal("Hello", Assert.IsType<TextDeltaStreamPart>(part).Delta),
            part =>
            {
                Assert.Equal(FinishReason.Stop, Assert.IsType<FinishStreamPart>(part).FinishReason);
                JsonAssert.Equal(part.Chunk!.Value, "{\"finishReason\":\"stop\",\"type\":\"finish\",\"usage\":{\"completion_tokens\":5,\"prompt_tokens\":10}}");
            });
    }
}
