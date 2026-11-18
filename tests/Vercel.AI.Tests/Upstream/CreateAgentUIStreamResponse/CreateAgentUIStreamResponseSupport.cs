// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using Vercel.AI.AspNetCore;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.CreateAgentUIStreamResponse;

/// <summary>Shared helpers of <c>create-agent-ui-stream-response.test.ts</c>.</summary>
internal static class CreateAgentUIStreamResponseSupport
{
    public const string Schema =
        "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false}";

    public static readonly LanguageModelUsage ModelUsage = new(5, 5, 10);

    /// <summary>Maps to the <c>decoder.decode(chunk)</c> of every chunk of the response body.</summary>
    public static async Task<IReadOnlyList<string>> DecodeAsync(UIMessageStreamResponse response)
    {
        var chunks = await response.Body.ToArrayAsync();
        return chunks.Select(chunk => Encoding.UTF8.GetString(chunk)).ToList();
    }

    public static IReadOnlyList<LanguageModelStreamPart> TextParts(string id, string text) =>
        [
            new StreamStartStreamPart([]),
            new ResponseMetadataStreamPart(id, "mock-model-id", DateTimeOffset.UnixEpoch),
            new TextStartStreamPart("1"),
            new TextDeltaStreamPart("1", text),
            new TextEndStreamPart("1"),
            new FinishStreamPart(FinishReason.Stop, ModelUsage, "stop"),
        ];
}
