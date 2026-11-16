// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicServiceTier;

/// <summary>The <c>message</c> and <c>events</c> fixtures of <c>anthropic-service-tier.test.ts</c>.</summary>
internal static class AnthropicServiceTierSupport
{
    public static JsonObject Message() =>
        new JsonObject
        {
            ["id"] = "msg_tier",
            ["type"] = "message",
            ["role"] = "assistant",
            ["model"] = "claude-haiku-4-5",
            ["content"] = new JsonArray(),
            ["stop_reason"] = "end_turn",
            ["usage"] = new JsonObject { ["input_tokens"] = 1, ["output_tokens"] = 1 },
        };

    public static string Events(JsonObject message)
    {
        return Sse(new JsonObject { ["type"] = "message_start", ["message"] = message })
            + Sse(
                JsonNode.Parse(
                    "{\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"end_turn\"},\"usage\":{\"output_tokens\":1}}"
                )!
            )
            + Sse(JsonNode.Parse("{\"type\":\"message_stop\"}")!);
    }

    public static string Sse(JsonNode node) => "data: " + node.ToJsonString() + "\n\n";

    public static CaptureHandler Handler(string mode, JsonObject message)
    {
        var handler = AnthropicParity.Ok(
            mode == "generate" ? message.ToJsonString() : Events(message)
        );
        if (mode != "generate")
        {
            handler.ContentType = "text/event-stream";
        }

        return handler;
    }

    public static async Task<(
        IReadOnlyList<CallWarning> Warnings,
        System.Text.Json.JsonElement? Metadata
    )> Run(string mode, CaptureHandler handler, LanguageModelCallOptions options)
    {
        var model = AnthropicParity.Client(handler).LanguageModel("claude-haiku-4-5");
        if (mode == "generate")
        {
            var result = await model.DoGenerateAsync(options, CancellationToken.None);
            return (result.Warnings, result.ProviderMetadata);
        }

        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in await model.DoStreamAsync(options, CancellationToken.None))
        {
            parts.Add(part);
        }

        return (
            Assert.Single(parts.OfType<StreamStartStreamPart>()).Warnings,
            parts.OfType<FinishStreamPart>().FirstOrDefault()?.ProviderMetadata
        );
    }

    public static LanguageModelCallOptions Options(string? providerOptions = null)
    {
        var options = AnthropicParity.Hello("Hi");
        options.MaxOutputTokens = 16;
        if (providerOptions != null)
        {
            options.ProviderOptions = AnthropicParity
                .Json(providerOptions)
                .EnumerateObject()
                .ToDictionary(p => p.Name, p => p.Value.Clone());
        }

        return options;
    }
}
