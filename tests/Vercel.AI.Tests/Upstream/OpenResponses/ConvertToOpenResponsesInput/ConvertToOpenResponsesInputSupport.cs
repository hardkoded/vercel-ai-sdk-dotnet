// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.OpenResponses;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.OpenResponses.ConvertToOpenResponsesInput;

/// <summary>Helpers shared by the <c>convert-to-open-responses-input.test.ts</c> ports.</summary>
internal static class ConvertToOpenResponsesInputSupport
{
    internal static OpenResponsesInputResult Convert(params ModelMessage[] prompt)
    {
        return OpenResponsesInput.Convert(prompt);
    }

    internal static OpenResponsesInputResult Convert(string providerOptionsName, params ModelMessage[] prompt)
    {
        return OpenResponsesInput.Convert(prompt, providerOptionsName);
    }

    internal static OpenResponsesInputResult ConvertStrict(params ModelMessage[] prompt)
    {
        return OpenResponsesInput.Convert(prompt, strictResponseInput: true);
    }

    internal static UserModelMessage User(params UserContentPart[] parts)
    {
        return new UserModelMessage(parts);
    }

    internal static AssistantModelMessage AssistantText(string text)
    {
        return new AssistantModelMessage(text, null, null);
    }

    internal static AssistantModelMessage AssistantToolCalls(string? text, params GeneratedToolCall[] calls)
    {
        return new AssistantModelMessage(text, calls, null);
    }

    internal static FileContentPart File(string mediaType, string base64, string? fileName = null, string? providerOptions = null)
    {
        return new FileContentPart(mediaType, null, System.Convert.FromBase64String(base64), fileName) { ProviderOptions = Options(providerOptions) };
    }

    internal static FileContentPart UrlFile(string mediaType, string url, string? providerOptions = null)
    {
        return new FileContentPart(mediaType, url, null, null) { ProviderOptions = Options(providerOptions) };
    }

    private static Dictionary<string, JsonElement>? Options(string? json)
    {
        return json == null ? null : UpstreamChat.Bag("test-provider", json);
    }
}
