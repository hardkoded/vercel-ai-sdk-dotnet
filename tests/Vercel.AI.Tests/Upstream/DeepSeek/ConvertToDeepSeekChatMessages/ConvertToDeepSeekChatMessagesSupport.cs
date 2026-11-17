// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Converter = Vercel.AI.DeepSeek.ConvertToDeepSeekChatMessages;

namespace Vercel.AI.Tests.Upstream.DeepSeek.ConvertToDeepSeekChatMessages;

/// <summary>Helpers shared by the <c>convert-to-deepseek-chat-messages.test.ts</c> ports.</summary>
internal static class ConvertToDeepSeekChatMessagesSupport
{
    internal const string TestFile =
        "packages/deepseek/src/chat/convert-to-deepseek-chat-messages.test.ts::convertToDeepSeekChatMessages";

    /// <summary>The bytes of upstream's <c>new Uint8Array([0, 1, 2, 3])</c>, which is base64 <c>AAECAw==</c>.</summary>
    internal static readonly byte[] Bytes0123 = { 0, 1, 2, 3 };

    /// <summary>Calls the converter with <c>responseFormat: undefined</c>, as every upstream test does.</summary>
    internal static Converter.Converted Run(
        string modelId,
        IReadOnlyList<ModelMessage> prompt,
        string providerOptionsName = "deepseek",
        bool supportsAssistantPrefixCompletion = false
    )
    {
        return Converter.Convert(
            prompt,
            false,
            null,
            modelId,
            providerOptionsName,
            supportsAssistantPrefixCompletion
        );
    }

    internal static UserModelMessage User(params UserContentPart[] parts)
    {
        return new UserModelMessage(parts);
    }

    internal static AssistantModelMessage Assistant(params AssistantContentPart[] parts)
    {
        return new AssistantModelMessage(parts);
    }

    internal static AssistantToolCallPart Call(string id, string name, string arguments)
    {
        return new AssistantToolCallPart(new GeneratedToolCall(id, name, arguments));
    }

    internal static ToolModelMessage ToolJson(string id, string name, string json)
    {
        return new ToolModelMessage(id, name, json, false);
    }

    internal static ToolModelMessage ToolText(string id, string name, string value)
    {
        return new ToolModelMessage(id, name, value, false, "text");
    }

    /// <summary>Provider options from (provider name, JSON) pairs.</summary>
    internal static Dictionary<string, JsonElement> Opts(
        params (string Name, string Json)[] entries
    )
    {
        var options = new Dictionary<string, JsonElement>();
        foreach (var (name, json) in entries)
        {
            using var document = JsonDocument.Parse(json);
            options[name] = document.RootElement.Clone();
        }

        return options;
    }

    internal static FileContentPart ImageUrl(
        string mediaType,
        string url,
        Dictionary<string, JsonElement>? options = null
    )
    {
        return new FileContentPart(mediaType, url, null, null) { ProviderOptions = options };
    }

    internal static FileContentPart ImageData(
        string mediaType,
        byte[] data,
        string? fileName = null,
        Dictionary<string, JsonElement>? options = null
    )
    {
        return new FileContentPart(mediaType, null, data, fileName) { ProviderOptions = options };
    }

    /// <summary>The warnings as upstream's <c>{ type, feature }</c> objects.</summary>
    internal static string WarningsJson(Converter.Converted result)
    {
        return JsonSerializer.Serialize(
            result.Warnings.Select(warning => new
            {
                type = warning.Type,
                feature = warning.Feature,
            })
        );
    }
}
