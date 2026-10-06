// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class AnthropicMessageStartTests
{
    [Theory]
    [InlineData(2)]
    [InlineData(0)]
    [InlineData(null)]
    public async Task ShouldEmitRawMessageStartUsageWithOutputTokensByDefault(int? outputTokens)
    {
        var usage = new JsonObject { ["input_tokens"] = 10 };
        if (outputTokens != null)
        {
            usage["output_tokens"] = outputTokens.Value;
        }

        usage["cache_read_input_tokens"] = 20;
        usage["cache_creation_input_tokens"] = 30;
        usage["service_tier"] = "standard";
        var expectedUsage = usage.ToJsonString();
        var start = new JsonObject
        {
            ["type"] = "message_start",
            ["message"] = new JsonObject { ["id"] = "msg_initial_usage", ["model"] = "claude-haiku-4-5", ["usage"] = usage },
        };
        var handler = AnthropicParity.Ok();
        handler.ContentType = "text/event-stream";
        handler.ResponseBody = "data: " + start.ToJsonString() + "\n\n"
            + "data: {\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"end_turn\",\"stop_sequence\":null},\"usage\":{\"input_tokens\":12,\"output_tokens\":7,\"cache_read_input_tokens\":22,\"cache_creation_input_tokens\":32}}\n\n"
            + "data: {\"type\":\"message_stop\"}\n\n";

        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in AnthropicParity.Client(handler).LanguageModel("claude-3-haiku-20240307").DoStreamAsync(AnthropicParity.Hello(), CancellationToken.None))
        {
            parts.Add(part);
        }

        Assert.Collection(
            parts,
            part => Assert.IsType<StreamStartStreamPart>(part),
            part =>
            {
                var metadata = Assert.IsType<ResponseMetadataStreamPart>(part);
                Assert.Equal("msg_initial_usage", metadata.Id);
                Assert.Equal("claude-haiku-4-5", metadata.ModelId);
            },
            part =>
            {
                var custom = Assert.IsType<CustomStreamPart>(part);
                Assert.Equal("custom", custom.Type);
                Assert.Equal("anthropic.message_start", custom.Kind);
            },
            part =>
            {
                var finish = Assert.IsType<FinishStreamPart>(part);
                Assert.Equal(66, finish.Usage.InputTokens);
                Assert.Equal(12, finish.Usage.NoCacheInputTokens);
                Assert.Equal(22, finish.Usage.CacheReadTokens);
                Assert.Equal(32, finish.Usage.CacheWriteTokens);
                Assert.Equal(7, finish.Usage.OutputTokens);
            });

        var expected = new JsonObject
        {
            ["anthropic"] = new JsonObject
            {
                ["id"] = "msg_initial_usage",
                ["model"] = "claude-haiku-4-5",
                ["usage"] = JsonNode.Parse(expectedUsage),
            },
        };
        var providerMetadata = ((CustomStreamPart)parts[2]).ProviderMetadata!.Value;
        Assert.True(JsonNode.DeepEquals(expected, JsonNode.Parse(providerMetadata.GetRawText())));
        Assert.Equal(outputTokens != null, providerMetadata.GetProperty("anthropic").GetProperty("usage").TryGetProperty("output_tokens", out _));
    }

    [Fact]
    public void ShouldIgnoreMessageStartAccountingPartsWhenReplayingAssistantContent()
    {
        var result = AnthropicPrompt.Convert(
            AnthropicParity.Json("[{\"role\":\"assistant\",\"content\":[{\"type\":\"custom\",\"kind\":\"anthropic.message_start\",\"providerOptions\":{\"anthropic\":{\"id\":\"msg_usage\",\"model\":\"claude-haiku-4-5\",\"usage\":{\"input_tokens\":13,\"output_tokens\":1}}}},{\"type\":\"text\",\"text\":\"Hi!\"}]}]"),
            sendReasoning: false);

        Assert.Equal("[{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"Hi!\"}]}]", result.Messages.ToJsonString());
        Assert.Empty(result.Warnings);
    }
}
