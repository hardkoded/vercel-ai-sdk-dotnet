// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AspNetCore;

namespace Vercel.AI.Tests.Upstream.ToUiMessageChunk;

/// <summary>Port of <c>to-ui-message-chunk.test.ts</c> &gt; <c>toUIMessageChunk</c>.</summary>
public sealed class ToUIMessageChunkTests
{
    private const string Prefix = "packages/ai/src/ui-message-stream/to-ui-message-chunk.test.ts::toUIMessageChunk::";
    private const string Commit = "ceeee29f9a7b7ca8c12a9270a542581bd760bdc7";

    private static readonly JsonElement ProviderMetadata = JsonDocument.Parse("""{"testProvider":{"signature":"sig-1"}}""").RootElement.Clone();

    [Fact]
    [UpstreamTest(Prefix + "maps text parts and preserves provider metadata", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Maps_text_parts_and_preserves_provider_metadata()
    {
        const string metadata = """{"testProvider":{"signature":"sig-1"}}""";

        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse($$"""{"type":"text-start","id":"text-1","providerMetadata":{{metadata}}}"""),
            UIMessageStreamConverter.ToUIMessageChunk(new TextStartPart("text-1", ProviderMetadata))));
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse($$"""{"type":"text-delta","id":"text-1","delta":"hello","providerMetadata":{{metadata}}}"""),
            UIMessageStreamConverter.ToUIMessageChunk(new TextDeltaPart("text-1", "hello", ProviderMetadata))));
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse($$"""{"type":"text-end","id":"text-1","providerMetadata":{{metadata}}}"""),
            UIMessageStreamConverter.ToUIMessageChunk(new TextEndPart("text-1", providerMetadata: ProviderMetadata))));
    }

    [Fact]
    [UpstreamTest(Prefix + "maps reasoning parts by default and suppresses them when disabled", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Maps_reasoning_parts_by_default_and_suppresses_them_when_disabled()
    {
        TextStreamPart[] reasoningParts =
        [
            new ReasoningStartPart("reasoning-1", ProviderMetadata),
            new ReasoningDeltaPart("reasoning-1", "thinking", ProviderMetadata),
            new ReasoningEndPart("reasoning-1", ProviderMetadata),
        ];
        const string metadata = """{"testProvider":{"signature":"sig-1"}}""";
        string[] expected =
        [
            $$"""{"type":"reasoning-start","id":"reasoning-1","providerMetadata":{{metadata}}}""",
            $$"""{"type":"reasoning-delta","id":"reasoning-1","delta":"thinking","providerMetadata":{{metadata}}}""",
            $$"""{"type":"reasoning-end","id":"reasoning-1","providerMetadata":{{metadata}}}""",
        ];

        for (var i = 0; i < reasoningParts.Length; i++)
        {
            Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected[i]), UIMessageStreamConverter.ToUIMessageChunk(reasoningParts[i])));
            Assert.Null(UIMessageStreamConverter.ToUIMessageChunk(reasoningParts[i], new ToUIMessageChunkOptions { SendReasoning = false }));
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "masks error parts by default", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Masks_error_parts_by_default()
    {
        var chunk = UIMessageStreamConverter.ToUIMessageChunk(new ErrorPart("boom"));

        Assert.Equal("""{"type":"error","errorText":"An error occurred."}""", chunk!.ToJsonString());
    }

    [Fact]
    [UpstreamTest(Prefix + "masks error parts when redactToolErrors is false", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Masks_error_parts_when_redactToolErrors_is_false()
    {
        var chunk = UIMessageStreamConverter.ToUIMessageChunk(new ErrorPart("boom"), new ToUIMessageChunkOptions { RedactToolErrors = false });

        Assert.Equal("""{"type":"error","errorText":"An error occurred."}""", chunk!.ToJsonString());
    }
}
