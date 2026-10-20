// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

public sealed class RawChunksForwardingTests
{
    private const string Suite = "packages/ai/src/generate-text/stream-text.test.ts::streamText > raw chunks forwarding::";

    [Fact]
    [UpstreamTest(Suite + "should forward raw chunks when include.rawChunks is enabled", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_forward_raw_chunks_when_include_rawChunks_is_enabled()
    {
        var result = StepModel.Stream(
            new StepModel(WithRawChunk("should appear")),
            new StreamTextOptions { Prompt = "test prompt", Include = new StreamTextInclude { RawChunks = true } });

        var raw = Assert.IsType<RawPart>(Assert.Single(await Collect(result), part => part.Type == "raw"));
        Assert.Equal("{\"type\":\"raw-data\",\"content\":\"should appear\"}", raw.RawJson);
    }

    [Fact]
    [UpstreamTest(Suite + "should not forward raw chunks when includeRawChunks is disabled", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_forward_raw_chunks_when_includeRawChunks_is_disabled()
    {
        var result = StepModel.Stream(
            new StepModel(WithRawChunk("should not appear")),
            new StreamTextOptions { Prompt = "test prompt", Include = new StreamTextInclude { RawChunks = false } });

        Assert.DoesNotContain(await Collect(result), part => part.Type == "raw");
    }

    [Fact]
    [UpstreamTest(Suite + "should pass through the include.rawChunks flag correctly to the model", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_through_the_include_rawChunks_flag_correctly_to_the_model()
    {
        var model = new StepModel(new LanguageModelStreamPart[]
        {
            new StreamStartStreamPart(Array.Empty<CallWarning>()),
            new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), "stop"),
        });

        await Collect(StepModel.Stream(model, new StreamTextOptions { Prompt = "test prompt", Include = new StreamTextInclude { RawChunks = true } }));

        Assert.True(Assert.Single(model.Calls).IncludeRawChunks);
    }

    private static LanguageModelStreamPart[] WithRawChunk(string content)
    {
        return new LanguageModelStreamPart[]
        {
            new StreamStartStreamPart(Array.Empty<CallWarning>()),
            new RawStreamPart("{\"type\":\"raw-data\",\"content\":\"" + content + "\"}"),
            new ResponseMetadataStreamPart("test-id", "test-model", DateTimeOffset.FromUnixTimeMilliseconds(0)),
            new TextStartStreamPart("1"),
            new TextDeltaStreamPart("1", "Hello, world!"),
            new TextEndStreamPart("1"),
            new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), "stop"),
        };
    }

    private static async Task<List<TextStreamPart>> Collect(StreamTextResult result)
    {
        var parts = new List<TextStreamPart>();
        await foreach (var part in result.Stream())
        {
            parts.Add(part);
        }

        return parts;
    }
}
