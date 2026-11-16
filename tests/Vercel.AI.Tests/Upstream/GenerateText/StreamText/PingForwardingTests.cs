// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AspNetCore;
using Vercel.AI.Gateway;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>
/// Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; ping forwarding</c>. Not ported: <c>does not hold heartbeats behind buffered tool input during retries</c>
/// and <c>delivers a heartbeat once while discarding failed-attempt tool input and content</c>, because <c>StreamTextOptions</c> has no <c>streamRetries</c> and no <c>onChunk</c>.
/// </summary>
public sealed class PingForwardingTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > ping forwarding::";

    [Fact]
    [UpstreamTest(
        Prefix + "should forward ping chunks without enabling raw chunks",
        Coverage = UpstreamCoverage.Covered,
        Commit = "97540149254736e8d566d432ccb7abf7b1cebbb3"
    )]
    public async Task Should_forward_ping_chunks_without_enabling_raw_chunks()
    {
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new StreamStartStreamPart(null),
                new PingStreamPart(),
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), "stop"),
            },
        };

        var result = Client()
            .StreamTextAsync(new StreamTextOptions { Model = model, Prompt = "test prompt" });

        var chunks = await ReadAsync(result);

        Assert.Single(chunks.OfType<PingPart>());
        Assert.Empty(chunks.OfType<RawPart>());
        Assert.False(model.Calls[0].IncludeRawChunks);
        Assert.Equal("ping", chunks.OfType<PingPart>().Single().Type);
        Assert.Empty((await result.Steps)[0].Content);
        var uiChunks = new List<System.Text.Json.Nodes.JsonObject>();
        await foreach (var chunk in UIMessageStreamConverter.ToUIMessageStream(Replay(chunks)))
        {
            uiChunks.Add(chunk);
        }

        Assert.DoesNotContain(uiChunks, chunk => chunk["type"]?.GetValue<string>() == "ping");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "does not treat a ping-only incomplete stream as generated output",
        Coverage = UpstreamCoverage.Covered,
        Commit = "97540149254736e8d566d432ccb7abf7b1cebbb3"
    )]
    public async Task Does_not_treat_a_ping_only_incomplete_stream_as_generated_output()
    {
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[] { new PingStreamPart() },
        };

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Prompt = "test",
                    OnError = (_, _) => Task.CompletedTask,
                }
            );

        // The port rethrows the no-output error from the stream after yielding the parts.
        var chunks = new List<TextStreamPart>();
        await Assert.ThrowsAsync<NoOutputGeneratedException>(async () =>
        {
            await foreach (var part in result.Stream())
            {
                chunks.Add(part);
            }
        });

        Assert.Contains(chunks, chunk => chunk is PingPart);
        Assert.Contains(chunks, chunk => chunk is ErrorPart);
        await Assert.ThrowsAsync<NoOutputGeneratedException>(() => result.Steps);
    }

    private static async IAsyncEnumerable<TextStreamPart> Replay(IEnumerable<TextStreamPart> parts)
    {
        foreach (var part in parts)
        {
            yield return part;
        }

        await Task.CompletedTask;
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
