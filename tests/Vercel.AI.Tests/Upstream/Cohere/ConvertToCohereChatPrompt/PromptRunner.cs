// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Converter = Vercel.AI.Cohere.ConvertToCohereChatPrompt;

namespace Vercel.AI.Tests.Upstream.Cohere.ConvertToCohereChatPrompt;

/// <summary>Runs the converter and serializes its result like the upstream <c>{ messages, documents, warnings }</c> object.</summary>
internal static class PromptRunner
{
    public const string Prefix =
        "packages/cohere/src/convert-to-cohere-chat-prompt.test.ts::convert to cohere chat prompt > ";

    public static JsonObject Run(params ModelMessage[] prompt)
    {
        var (messages, documents, warnings) = Converter.Convert(prompt);
        return new JsonObject
        {
            ["messages"] = messages,
            ["documents"] = documents,
            ["warnings"] = new JsonArray(warnings.Select(w => (JsonNode?)w.Message).ToArray()),
        };
    }

    public static UserModelMessage User(params UserContentPart[] parts) => new(parts);

    public static FileContentPart File(string mediaType, byte[] data, string? name = null) =>
        new(mediaType, null, data, name);

    public static FileContentPart File(string mediaType, string text, string? name = null) =>
        new(mediaType, null, Encoding.UTF8.GetBytes(text), name);

    public static readonly byte[] Png = { 0, 1, 2, 3 };

    public static JsonElement Json(string json) => JsonSerializer.Deserialize<JsonElement>(json);
}
