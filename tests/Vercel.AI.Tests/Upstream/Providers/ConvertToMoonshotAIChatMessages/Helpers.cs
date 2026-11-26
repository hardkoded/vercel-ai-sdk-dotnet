// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Moonshot;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.ConvertToMoonshotAIChatMessages;

/// <summary>Shared helpers of the <c>convert-to-moonshotai-chat-messages.test.ts</c> port.</summary>
internal static class Helpers
{
    public const string Source =
        "packages/moonshotai/src/convert-to-moonshotai-chat-messages.test.ts::";

    /// <summary>Maps to the <c>convertMessages</c> helper of the upstream test file.</summary>
    public static JsonArray ConvertMessages(params ModelMessage[] prompt)
    {
        return MoonshotChatMessages.ConvertToMoonshotAIChatMessages(prompt).Messages;
    }

    public static IReadOnlyDictionary<string, JsonElement> Options(string json)
    {
        return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)!;
    }

    public static void AssertJson(string expected, JsonNode? actual)
    {
        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(expected), actual),
            "Expected " + expected + " but was " + actual?.ToJsonString()
        );
    }
}
