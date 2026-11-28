// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleInteractionsLanguageModel;

/// <summary>Helpers shared by the <c>doGenerate</c> ports of <c>google-interactions-language-model.test.ts</c>.</summary>
internal static class DoGenerateSupport
{
    public const string Id =
        "packages/google/src/interactions/google-interactions-language-model.test.ts::GoogleInteractionsLanguageModel.doGenerate > ";

    /// <summary>The concatenated text of every text part, as upstream's <c>filter(c =&gt; c.type === 'text')</c> join builds it.</summary>
    public static string Text(LanguageModelGenerateResult result)
    {
        return string.Concat(result.Content.OfType<GeneratedText>().Select(text => text.Text));
    }

    /// <summary>The first request body the handler received.</summary>
    public static JsonObject Body(SequencedHandler handler)
    {
        return JsonNode.Parse(handler.Bodies[0])!.AsObject();
    }

    /// <summary>The content parts as upstream's inline snapshots show them. A value that upstream leaves undefined is left out.</summary>
    public static JsonArray Content(IEnumerable<GeneratedContent> content)
    {
        var array = new JsonArray();
        foreach (var part in content)
        {
            var node = new JsonObject();
            switch (part)
            {
                case GeneratedReasoning reasoning:
                    Metadata(node, reasoning.ProviderMetadata);
                    node["text"] = reasoning.Text;
                    break;
                case GeneratedText text:
                    Metadata(node, text.ProviderMetadata);
                    node["text"] = text.Text;
                    break;
                case GeneratedToolCall call:
                    node["input"] = call.ArgumentsJson;
                    Metadata(node, call.ProviderMetadata);
                    node["toolCallId"] = call.ToolCallId;
                    node["toolName"] = call.ToolName;
                    break;
                default:
                    throw new InvalidOperationException("Unexpected content " + part.Type);
            }

            node["type"] = part.Type;
            array.Add(node);
        }

        return array;
    }

    /// <summary>The finish reason as upstream's snapshot shows it.</summary>
    public static JsonObject FinishReason(LanguageModelGenerateResult result)
    {
        return new JsonObject
        {
            ["raw"] = result.RawFinishReason,
            ["unified"] =
                result.FinishReason == Provider.FinishReason.ToolCalls
                    ? "tool-calls"
                    : result.FinishReason.ToString().ToLowerInvariant(),
        };
    }

    private static void Metadata(JsonObject node, JsonElement? metadata)
    {
        if (metadata != null)
        {
            node["providerMetadata"] = JsonNode.Parse(metadata.Value.GetRawText());
        }
    }
}
