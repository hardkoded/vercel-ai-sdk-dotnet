// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;

namespace Vercel.AI.Tests.ConvertToAnthropicPrompt;

/// <summary>Runs <c>convertToAnthropicPrompt</c> on a prompt written in the SDK message JSON shape.</summary>
internal static class PromptRunner
{
    public static AnthropicPromptResult Run(
        string promptJson,
        bool sendReasoning = true,
        AnthropicCacheControlValidator? cacheControl = null,
        IReadOnlyDictionary<string, string>? providerToolNames = null,
        IReadOnlyDictionary<string, string>? toolsetNames = null
    )
    {
        using var document = JsonDocument.Parse(promptJson);
        return AnthropicPrompt.ConvertJson(
            document.RootElement.Clone(),
            sendReasoning,
            cacheControl,
            providerToolNames,
            toolsetNames
        );
    }

    public static JsonNode? Node(string json) => JsonNode.Parse(json);

    public static void AssertJson(string expected, JsonNode? actual) =>
        Assert.True(
            JsonNode.DeepEquals(Node(expected), actual),
            "Expected " + expected + " but was " + (actual?.ToJsonString() ?? "null")
        );

    public static void AssertWarnings(AnthropicPromptResult result, params string[] messages)
    {
        Assert.Equal(messages.Length, result.Warnings.Count);
        for (var i = 0; i < messages.Length; i++)
        {
            Assert.Equal("other", result.Warnings[i].Type);
            Assert.Equal(messages[i], result.Warnings[i].Message);
        }
    }
}
