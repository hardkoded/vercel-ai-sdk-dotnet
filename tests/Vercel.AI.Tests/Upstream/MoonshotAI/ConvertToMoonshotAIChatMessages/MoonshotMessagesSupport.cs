// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Moonshot;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.ConvertToMoonshotAIChatMessages;

/// <summary>Helpers shared by the <c>convert-to-moonshotai-chat-messages.test.ts</c> ports.</summary>
internal static class MoonshotMessagesSupport
{
    internal const string Prefix =
        "packages/moonshotai/src/convert-to-moonshotai-chat-messages.test.ts::";

    internal static readonly byte[] Bytes0123 = { 0, 1, 2, 3 };

    /// <summary>Parses a JSON object of provider options, keyed by provider.</summary>
    internal static IReadOnlyDictionary<string, JsonElement> Options(string json)
    {
        using var document = JsonDocument.Parse(json);
        var options = new Dictionary<string, JsonElement>();
        foreach (var property in document.RootElement.EnumerateObject())
        {
            options[property.Name] = property.Value.Clone();
        }

        return options;
    }

    internal static MoonshotChatMessages.Result Convert(
        IReadOnlyList<ModelMessage> prompt,
        string? modelId = null,
        string providerOptionsName = "moonshotai",
        string? responseFormatType = null
    ) =>
        MoonshotChatMessages.ConvertToMoonshotAIChatMessages(
            prompt,
            modelId,
            providerOptionsName,
            responseFormatType
        );

    /// <summary>Upstream's <c>convertMessages</c> helper.</summary>
    internal static JsonArray ConvertMessages(params ModelMessage[] prompt) =>
        Convert(prompt).Messages;

    internal static UserModelMessage User(params UserContentPart[] parts) => new(parts);

    internal static UserModelMessage UserFile(FileContentPart part) =>
        new(new UserContentPart[] { part });

    internal static FileContentPart DataFile(string mediaType) =>
        new(mediaType, null, Bytes0123, null);

    internal static FileContentPart UrlFile(string mediaType, string url) =>
        new(mediaType, url, null, null);

    internal static AssistantModelMessage AssistantText(
        string text,
        IReadOnlyDictionary<string, JsonElement>? providerOptions = null
    ) =>
        new(new AssistantContentPart[] { new AssistantTextPart(text) })
        {
            ProviderOptions = providerOptions,
        };

    internal static void Equal(JsonNode? actual, string expected) =>
        JsonAssert.Equal(actual, expected);
}
