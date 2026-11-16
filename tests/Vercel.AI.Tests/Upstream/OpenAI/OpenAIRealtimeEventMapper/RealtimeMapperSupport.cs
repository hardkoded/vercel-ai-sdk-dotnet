// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIRealtimeEventMapper;

/// <summary>Shared helpers of the OpenAI realtime event mapper tests.</summary>
internal static class RealtimeMapperSupport
{
    internal const string Commit = "892af4dab7f1351fd2ced70dcaba288746d8a782";

    internal const string File =
        "packages/openai/src/realtime/openai-realtime-event-mapper.test.ts::";

    /// <summary>The non-null usage fields by their upstream names, like <c>toEqual</c> sees an object without undefined fields.</summary>
    internal static JsonObject UsageFields(RealtimeModelV4Usage? usage)
    {
        var fields = new JsonObject();
        if (usage == null)
        {
            return fields;
        }

        Add(fields, "inputTokens", usage.InputTokens);
        Add(fields, "outputTokens", usage.OutputTokens);
        Add(fields, "cachedInputTokens", usage.CachedInputTokens);
        Add(fields, "inputTextTokens", usage.InputTextTokens);
        Add(fields, "inputAudioTokens", usage.InputAudioTokens);
        Add(fields, "outputTextTokens", usage.OutputTextTokens);
        Add(fields, "outputAudioTokens", usage.OutputAudioTokens);
        Add(fields, "cachedInputTextTokens", usage.CachedInputTextTokens);
        Add(fields, "cachedInputAudioTokens", usage.CachedInputAudioTokens);
        if (usage.Seconds != null)
        {
            fields["seconds"] = usage.Seconds;
        }

        return fields;
    }

    internal static JsonElement Json(string json)
    {
        return OpenAIUpstream.Json(json);
    }

    private static void Add(JsonObject target, string name, int? value)
    {
        if (value != null)
        {
            target[name] = value;
        }
    }
}
