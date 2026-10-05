// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AspNetCore;

namespace Vercel.AI.Tests;

public sealed class UiMessageChunkSchemaTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/ui-message-chunks.test.ts::uiMessageChunkSchema::returns UI message chunks",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_ui_message_chunks()
    {
        using var document = JsonDocument.Parse("{\"type\":\"text-delta\",\"delta\":\"Hello, world!\",\"id\":\"123\"}");
        var chunk = UIMessageChunks.Parse(document.RootElement);

        Assert.Equal("text-delta", chunk["type"]!.GetValue<string>());
        Assert.Equal("Hello, world!", chunk["delta"]!.GetValue<string>());
        Assert.Equal("123", chunk["id"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/ui-message-chunks.test.ts::uiMessageChunkSchema::accepts reset-step chunks",
        Coverage = UpstreamCoverage.Covered)]
    public void Accepts_reset_step_chunks()
    {
        using var document = JsonDocument.Parse("{\"type\":\"reset-step\"}");
        var chunk = UIMessageChunks.Parse(document.RootElement);

        Assert.Equal("reset-step", chunk["type"]!.GetValue<string>());
        Assert.Single(chunk);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/ui-message-chunks.test.ts::uiMessageChunkSchema::accepts known chunks with fields added by newer servers",
        Coverage = UpstreamCoverage.Covered)]
    public void Accepts_known_chunks_with_fields_added_by_newer_servers()
    {
        var sse = "data: {\"type\":\"tool-output-available\",\"toolCallId\":\"call-123\",\"output\":{\"ok\":true},\"optionalFieldFromNewerServer\":{\"addedIn\":\"future-ai-sdk-version\"}}\n\n";
        var chunks = UIMessageChunks.ParseSse(sse);

        var chunk = Assert.Single(chunks);
        Assert.Equal("tool-output-available", chunk["type"]!.GetValue<string>());
        Assert.Equal("call-123", chunk["toolCallId"]!.GetValue<string>());
        Assert.True(chunk["output"]!["ok"]!.GetValue<bool>());
        Assert.Equal("future-ai-sdk-version", chunk["optionalFieldFromNewerServer"]!["addedIn"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/ui-message-chunks.test.ts::uiMessageChunkSchema::rejects chunk types unknown to the client",
        Coverage = UpstreamCoverage.Covered)]
    public void Rejects_chunk_types_unknown_to_the_client()
    {
        using var document = JsonDocument.Parse("{\"type\":\"future-control-chunk\"}");
        var error = Assert.Throws<UIMessageChunkException>(() => UIMessageChunks.Parse(document.RootElement));

        Assert.Contains("future-control-chunk", error.Message, StringComparison.Ordinal);
    }
}
