// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json.Nodes;
using Vercel.AI.AspNetCore;

namespace Vercel.AI.Tests;

public sealed class TextUiMessageStreamTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/transform-text-to-ui-message-stream.test.ts::transformTextToUiMessageStream::should transform text stream into UI message stream with correct sequence",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Transforms_text_into_the_ui_message_sequence()
    {
        var chunks = await Read(TextUIMessageStream.TransformAsync(Text("Hello", " ", "World")));

        Assert.Equal(
            new[]
            {
                "start",
                "start-step",
                "text-start",
                "text-delta",
                "text-delta",
                "text-delta",
                "text-end",
                "finish-step",
                "finish",
            },
            chunks.Select(chunk => chunk["type"]!.GetValue<string>()).ToArray());
        Assert.Equal("text-1", chunks[2]["id"]!.GetValue<string>());
        Assert.Equal(new[] { "Hello", " ", "World" }, chunks.Where(chunk => chunk["type"]!.GetValue<string>() == "text-delta").Select(chunk => chunk["delta"]!.GetValue<string>()).ToArray());
        Assert.Equal("text-1", chunks[6]["id"]!.GetValue<string>());
        Assert.False(chunks[8].ContainsKey("finishReason"));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/transform-text-to-ui-message-stream.test.ts::transformTextToUiMessageStream::should handle empty streams correctly",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Transforms_an_empty_text_stream_without_deltas()
    {
        var chunks = await Read(TextUIMessageStream.TransformAsync(Text()));

        Assert.Equal(
            new[] { "start", "start-step", "text-start", "text-end", "finish-step", "finish" },
            chunks.Select(chunk => chunk["type"]!.GetValue<string>()).ToArray());
        Assert.Equal("text-1", chunks[2]["id"]!.GetValue<string>());
        Assert.Equal("text-1", chunks[3]["id"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/transform-text-to-ui-message-stream.test.ts::transformTextToUiMessageStream::should handle single chunk streams",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Transforms_a_single_text_chunk()
    {
        var chunks = await Read(TextUIMessageStream.TransformAsync(Text("Complete message")));

        Assert.Equal("text-delta", chunks[3]["type"]!.GetValue<string>());
        Assert.Equal("text-1", chunks[3]["id"]!.GetValue<string>());
        Assert.Equal("Complete message", chunks[3]["delta"]!.GetValue<string>());
        Assert.Equal(7, chunks.Count);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/process-text-stream.test.ts::processTextStream::should process stream chunks correctly",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Processes_text_chunks_in_order()
    {
        var seen = new List<string>();
        await TextUIMessageStream.ProcessAsync(Bytes("Hello", " ", "World"), text =>
        {
            seen.Add(text);
            return Task.CompletedTask;
        });

        Assert.Equal(new[] { "Hello", " ", "World" }, seen);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/process-text-stream.test.ts::processTextStream::should handle empty streams",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Processes_an_empty_byte_stream_without_calling_back()
    {
        var calls = 0;
        await TextUIMessageStream.ProcessAsync(Bytes(), _ =>
        {
            calls++;
            return Task.CompletedTask;
        });

        Assert.Equal(0, calls);
    }

    private static async Task<List<JsonObject>> Read(IAsyncEnumerable<JsonObject> chunks)
    {
        var list = new List<JsonObject>();
        await foreach (var chunk in chunks)
        {
            list.Add(chunk);
        }

        return list;
    }

    private static async IAsyncEnumerable<string> Text(params string[] parts)
    {
        foreach (var part in parts)
        {
            yield return part;
            await Task.Yield();
        }
    }

    private static async IAsyncEnumerable<byte[]> Bytes(params string[] parts)
    {
        foreach (var part in parts)
        {
            yield return Encoding.UTF8.GetBytes(part);
            await Task.Yield();
        }
    }
}
