// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.AspNetCore;

namespace Vercel.AI.Tests;

public sealed class UIMessageChunkSchemaTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/ui-message-chunks.test.ts::uiMessageChunkSchema::returns UI message chunks",
        Coverage = UpstreamCoverage.Covered)]
    public void Validates_a_text_delta_chunk()
    {
        ParityAssert.JsonEqual(
            UIMessageChunkSchema.Validate(JsonNode.Parse("{\"type\":\"text-delta\",\"delta\":\"Hello, world!\",\"id\":\"123\"}")),
            "{\"type\":\"text-delta\",\"delta\":\"Hello, world!\",\"id\":\"123\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/ui-message-chunks.test.ts::uiMessageChunkSchema::accepts reset-step chunks",
        Coverage = UpstreamCoverage.Covered)]
    public void Accepts_reset_step_chunks()
    {
        ParityAssert.JsonEqual(
            UIMessageChunkSchema.Validate(JsonNode.Parse("{\"type\":\"reset-step\"}")),
            "{\"type\":\"reset-step\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/ui-message-chunks.test.ts::uiMessageChunkSchema::accepts known chunks with fields added by newer servers",
        Coverage = UpstreamCoverage.Covered)]
    public void Keeps_fields_added_by_newer_servers()
    {
        var payload = "{\"type\":\"tool-output-available\",\"toolCallId\":\"call-123\",\"output\":{\"ok\":true},\"optionalFieldFromNewerServer\":{\"addedIn\":\"future-ai-sdk-version\"}}";
        var parsed = UIMessageChunkSchema.ParseJsonEventStream("data: " + payload + "\n\n");

        Assert.Single(parsed);
        Assert.True(parsed[0].Success);
        ParityAssert.JsonEqual(parsed[0].Value, payload);
        ParityAssert.JsonEqual(parsed[0].RawValue, payload);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/ui-message-chunks.test.ts::uiMessageChunkSchema::rejects chunk types unknown to the client",
        Coverage = UpstreamCoverage.Covered)]
    public void Rejects_unknown_chunk_types()
    {
        Assert.Throws<UIMessageChunkValidationException>(() =>
            UIMessageChunkSchema.Validate(JsonNode.Parse("{\"type\":\"future-control-chunk\"}")));
    }
}
