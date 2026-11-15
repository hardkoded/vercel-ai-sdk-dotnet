// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.DeepSeek;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.DeepSeek.ConvertToDeepSeekResponsesInput;

/// <summary>Helpers shared by the <c>convert-to-deepseek-responses-input.test.ts</c> ports.</summary>
internal static class ConvertToDeepSeekResponsesInputSupport
{
    internal const string TestFile = "packages/deepseek/src/responses/convert-to-deepseek-responses-input.test.ts::convertToDeepSeekResponsesInput";

    /// <summary>Upstream commit that added the tests. The parity catalog is older.</summary>
    internal const string Commit = "f9d43bdaa78a0564105942c7a6709469a120c3ee";

    internal static JsonArray Convert(params ModelMessage[] prompt)
    {
        return Convert(new List<CallWarning>(), "deepseek", prompt);
    }

    internal static JsonArray Convert(List<CallWarning> warnings, params ModelMessage[] prompt)
    {
        return Convert(warnings, "deepseek", prompt);
    }

    internal static JsonArray Convert(List<CallWarning> warnings, string providerOptionsName, params ModelMessage[] prompt)
    {
        return DeepSeekResponsesInput.Convert(prompt, providerOptionsName, warnings);
    }

    internal static UserModelMessage User(params UserContentPart[] parts)
    {
        return new UserModelMessage(parts);
    }

    /// <summary>Provider options from (provider name, JSON) pairs.</summary>
    internal static Dictionary<string, JsonElement> Opts(params (string Name, string Json)[] entries)
    {
        var options = new Dictionary<string, JsonElement>();
        foreach (var (name, json) in entries)
        {
            using var document = JsonDocument.Parse(json);
            options[name] = document.RootElement.Clone();
        }

        return options;
    }

    internal static FileContentPart ImageUrl(string mediaType, string url, Dictionary<string, JsonElement>? options = null)
    {
        return new FileContentPart(mediaType, url, null, null) { ProviderOptions = options };
    }

    internal static FileContentPart ImageData(string mediaType, byte[] data, Dictionary<string, JsonElement>? options = null)
    {
        return new FileContentPart(mediaType, null, data, null) { ProviderOptions = options };
    }

    /// <summary>The bytes of upstream's base64 <c>AQID</c> and <c>Uint8Array([1, 2, 3])</c>.</summary>
    internal static readonly byte[] Bytes123 = { 1, 2, 3 };

    internal static ToolModelMessage ToolText(string id, string name, string value, string type = "text")
    {
        return new ToolModelMessage(id, name, value, false, type);
    }
}
