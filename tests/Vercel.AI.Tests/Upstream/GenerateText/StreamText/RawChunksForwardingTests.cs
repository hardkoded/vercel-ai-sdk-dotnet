// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>
/// Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; raw chunks forwarding</c>. Not ported: <c>should call onChunk with raw chunks when include.rawChunks is enabled</c>,
/// because <c>StreamTextOptions</c> has no <c>onChunk</c> callback.
/// </summary>
public sealed class RawChunksForwardingTests
{
    private const string Prefix = "packages/ai/src/generate-text/stream-text.test.ts::streamText > raw chunks forwarding::";

    [Fact]
    [UpstreamTest(Prefix + "should forward raw chunks when include.rawChunks is enabled", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_forward_raw_chunks_when_include_rawChunks_is_enabled()
    {
        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = ModelWithRawChunks("should appear"),
            Prompt = "test prompt",
            Include = new StreamTextInclude { RawChunks = true },
        });

        var raw = (await ReadAsync(result)).OfType<RawPart>().ToList();

        var chunk = Assert.Single(raw);
        Assert.Equal("raw", chunk.Type);
        Assert.Equal("{\"type\":\"raw-data\",\"content\":\"should appear\"}", chunk.RawValue.GetRawText());
    }

    [Fact]
    [UpstreamTest(Prefix + "should not forward raw chunks when includeRawChunks is disabled", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_forward_raw_chunks_when_includeRawChunks_is_disabled()
    {
        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = ModelWithRawChunks("should not appear"),
            Prompt = "test prompt",
            Include = new StreamTextInclude { RawChunks = false },
        });

        Assert.Empty((await ReadAsync(result)).OfType<RawPart>());
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass through the include.rawChunks flag correctly to the model", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_through_the_include_rawChunks_flag_correctly_to_the_model()
    {
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new StreamStartStreamPart(null),
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), "stop"),
            },
        };

        await ReadAsync(Client().StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "test prompt",
            Include = new StreamTextInclude { RawChunks = true },
        }));

        Assert.True(model.Calls[0].IncludeRawChunks);
    }

    private static TestLanguageModel ModelWithRawChunks(string content)
    {
        return new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new StreamStartStreamPart(null),
                new RawStreamPart(JsonSerializer.Serialize(new { type = "raw-data", content })),
                new ResponseMetadataStreamPart("test-id", "test-model", DateTimeOffset.FromUnixTimeMilliseconds(0)),
                new TextStartStreamPart("1"),
                new TextDeltaStreamPart("1", "Hello, world!"),
                new TextEndStreamPart("1"),
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), "stop"),
            },
        };
    }

    private static async Task<List<TextStreamPart>> ReadAsync(StreamTextResult result)
    {
        var parts = new List<TextStreamPart>();
        await foreach (var part in result.Stream())
        {
            parts.Add(part);
        }

        return parts;
    }

    private static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
    }
}
