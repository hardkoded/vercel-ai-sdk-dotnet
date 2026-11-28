// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.ParseGoogleInteractionsOutputs;

/// <summary>Shared setup for the ports of <c>parse-google-interactions-outputs.test.ts</c>.</summary>
internal static class ParseGoogleInteractionsOutputsSupport
{
    public const string Id =
        "packages/google/src/interactions/parse-google-interactions-outputs.test.ts::parseGoogleInteractionsOutputs > ";

    /// <summary>Upstream's <c>const generateId = () =&gt; 'gen-id'</c>.</summary>
    public static string GenerateId()
    {
        return "gen-id";
    }

    public static (IReadOnlyList<GeneratedContent> Content, bool HasFunctionCall) Parse(
        string steps,
        string? interactionId = null
    )
    {
        return GoogleInteractionsOutput.Parse(
            JsonNode.Parse(steps)!.AsArray(),
            GenerateId,
            interactionId
        );
    }

    public static JsonArray Snapshot(IEnumerable<GeneratedContent> content)
    {
        var array = new JsonArray();
        foreach (var item in content)
        {
            array.Add(Snapshot(item));
        }

        return array;
    }

    /// <summary>Renders a content part the way upstream's snapshots show it. A value that upstream leaves undefined is left out.</summary>
    public static JsonObject Snapshot(GeneratedContent item)
    {
        var node = new JsonObject { ["type"] = item.Type };
        switch (item)
        {
            case GeneratedText text:
                node["text"] = text.Text;
                Metadata(node, text.ProviderMetadata);
                break;
            case GeneratedReasoning reasoning:
                node["text"] = reasoning.Text;
                Metadata(node, reasoning.ProviderMetadata);
                break;
            case GeneratedToolCall call:
                node["toolCallId"] = call.ToolCallId;
                node["toolName"] = call.ToolName;
                node["input"] = call.ArgumentsJson;
                Metadata(node, call.ProviderMetadata);
                if (call.ProviderExecuted)
                {
                    node["providerExecuted"] = true;
                }

                break;
            case GoogleGeneratedFile file:
                node["mediaType"] = file.MediaType;
                node["data"] =
                    file.Url != null
                        ? new JsonObject { ["type"] = "url", ["url"] = file.Url.ToString() }
                        : new JsonObject
                        {
                            ["type"] = "data",
                            ["data"] = System.Convert.ToBase64String(file.Data),
                        };
                Metadata(node, file.ProviderMetadata);
                break;
            case GeneratedCustom custom:
                node["kind"] = custom.Kind;
                Metadata(node, custom.ProviderMetadata);
                break;
            default:
                throw new InvalidOperationException("Unexpected content " + item.Type);
        }

        return node;
    }

    private static void Metadata(JsonObject node, JsonElement? metadata)
    {
        if (metadata != null)
        {
            node["providerMetadata"] = JsonNode.Parse(metadata.Value.GetRawText());
        }
    }
}
