// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json.Nodes;
using Vercel.AI.AspNetCore;

namespace Vercel.AI.Tests;

public sealed class TextUiStreamTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/process-text-stream.test.ts::processTextStream::should process stream chunks correctly",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Process_decodes_each_utf8_chunk()
    {
        var chunks = new List<string>();
        await TextUiStream.ProcessAsync(Bytes("Hello", " ", "World"), text =>
        {
            chunks.Add(text);
            return Task.CompletedTask;
        });

        Assert.Equal(new[] { "Hello", " ", "World" }, chunks);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/process-text-stream.test.ts::processTextStream::should handle empty streams",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Process_handles_an_empty_stream()
    {
        var calls = 0;
        await TextUiStream.ProcessAsync(Bytes(), _ =>
        {
            calls++;
            return Task.CompletedTask;
        });

        Assert.Equal(0, calls);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/transform-text-to-ui-message-stream.test.ts::transformTextToUiMessageStream::should transform text stream into UI message stream with correct sequence",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Transform_wraps_text_chunks()
    {
        var chunks = await Collect(TextUiStream.Transform(Text("Hello", " ", "World")));
        ParityAssert.JsonEqual(
            ArrayOf(chunks),
            "[{\"type\":\"start\"},{\"type\":\"start-step\"},{\"type\":\"text-start\",\"id\":\"text-1\"},{\"type\":\"text-delta\",\"id\":\"text-1\",\"delta\":\"Hello\"},{\"type\":\"text-delta\",\"id\":\"text-1\",\"delta\":\" \"},{\"type\":\"text-delta\",\"id\":\"text-1\",\"delta\":\"World\"},{\"type\":\"text-end\",\"id\":\"text-1\"},{\"type\":\"finish-step\"},{\"type\":\"finish\"}]");
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/transform-text-to-ui-message-stream.test.ts::transformTextToUiMessageStream::should handle empty streams correctly",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Transform_handles_an_empty_stream()
    {
        var chunks = await Collect(TextUiStream.Transform(Text()));
        ParityAssert.JsonEqual(
            ArrayOf(chunks),
            "[{\"type\":\"start\"},{\"type\":\"start-step\"},{\"type\":\"text-start\",\"id\":\"text-1\"},{\"type\":\"text-end\",\"id\":\"text-1\"},{\"type\":\"finish-step\"},{\"type\":\"finish\"}]");
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/transform-text-to-ui-message-stream.test.ts::transformTextToUiMessageStream::should handle single chunk streams",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Transform_handles_a_single_chunk()
    {
        var chunks = await Collect(TextUiStream.Transform(Text("Complete message")));
        ParityAssert.JsonEqual(
            ArrayOf(chunks),
            "[{\"type\":\"start\"},{\"type\":\"start-step\"},{\"type\":\"text-start\",\"id\":\"text-1\"},{\"type\":\"text-delta\",\"id\":\"text-1\",\"delta\":\"Complete message\"},{\"type\":\"text-end\",\"id\":\"text-1\"},{\"type\":\"finish-step\"},{\"type\":\"finish\"}]");
    }

    private static JsonArray ArrayOf(IReadOnlyList<JsonObject> chunks)
    {
        var array = new JsonArray();
        for (var index = 0; index < chunks.Count; index++)
        {
            array.Add(chunks[index]);
        }

        return array;
    }

    private static async Task<List<JsonObject>> Collect(IAsyncEnumerable<JsonObject> stream)
    {
        var chunks = new List<JsonObject>();
        await foreach (var chunk in stream)
        {
            chunks.Add(chunk);
        }

        return chunks;
    }

    private static async IAsyncEnumerable<string> Text(params string[] parts)
    {
        await Task.CompletedTask;
        for (var index = 0; index < parts.Length; index++)
        {
            yield return parts[index];
        }
    }

    private static async IAsyncEnumerable<byte[]> Bytes(params string[] parts)
    {
        await Task.CompletedTask;
        for (var index = 0; index < parts.Length; index++)
        {
            yield return Encoding.UTF8.GetBytes(parts[index]);
        }
    }
}
