// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Converter = Vercel.AI.Groq.ConvertToGroqChatMessages;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Groq.ConvertToGroqChatMessages;

/// <summary>Helpers shared by the <c>convert-to-groq-chat-messages.test.ts</c> ports.</summary>
internal static class ConvertToGroqChatMessagesSupport
{
    internal const string TestFile = "packages/groq/src/convert-to-groq-chat-messages.test.ts";

    /// <summary>The bytes of upstream's <c>new Uint8Array([0, 1, 2, 3])</c>, which is base64 <c>AAECAw==</c>.</summary>
    internal static readonly byte[] Bytes0123 = { 0, 1, 2, 3 };

    /// <summary>The bytes of upstream's <c>iVBORw0KGgo=</c>, a PNG signature.</summary>
    internal static readonly byte[] PngBytes = System.Convert.FromBase64String("iVBORw0KGgo=");

    internal static JsonArray Run(params ModelMessage[] prompt) =>
        Converter.Convert(prompt);

    internal static UserModelMessage User(params UserContentPart[] parts) => new(parts);

    internal static AssistantModelMessage Assistant(params AssistantContentPart[] parts) =>
        new(parts);

    internal static AssistantToolCallPart Call(string id, string name, string arguments) =>
        new(new GeneratedToolCall(id, name, arguments));

    internal static ToolModelMessage ToolJson(string id, string name, string json) =>
        new(id, name, json, false);

    internal static JsonNode? Content(JsonArray result, int index = 0) =>
        result[0]!["content"]![index];
}
