// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google;

/// <summary>Shared setup for the ports of the Interactions stream tests.</summary>
internal static class GoogleInteractionsStreamSupport
{
    /// <summary>The model upstream builds with <c>generateId: () =&gt; 'test-id'</c>.</summary>
    public static ILanguageModel Model(SequencedHandler handler)
    {
        return GoogleProvider.Create(new GoogleOptions { ApiKey = "test-api-key", GenerateId = () => "test-id" }, handler).Interactions("gemini-2.5-flash");
    }

    /// <summary>The events of a <c>__fixtures__/*.chunks.txt</c> file, one JSON object per line.</summary>
    public static List<JsonObject> FixtureEvents(string name)
    {
        return File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "Fixtures", "google-interactions-" + name + ".chunks.txt"))
            .Where(line => line.Trim().Length > 0)
            .Select(line => JsonNode.Parse(line)!.AsObject())
            .ToList();
    }

    /// <summary>The text of a <c>__fixtures__/*.json</c> file.</summary>
    public static string JsonFixture(string name)
    {
        return File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "google-interactions-" + name + ".json"));
    }

    /// <summary>The body of an event stream response that sends each event as one <c>data:</c> line.</summary>
    public static string Sse(IEnumerable<JsonObject> events)
    {
        return string.Concat(events.Select(e => "data: " + e.ToJsonString() + "\n\n"));
    }

    public static SequencedHandler FixtureHandler(string name)
    {
        return new SequencedHandler().Sse(Sse(FixtureEvents(name)));
    }

    public static async Task<List<LanguageModelStreamPart>> Stream(ILanguageModel model, LanguageModelCallOptions options)
    {
        var result = await model.DoStreamAsync(options, CancellationToken.None);
        return await GoogleUpstream.Collect(result.Stream);
    }

    /// <summary>Runs events through a transform the way upstream's <c>runTransform</c> does: stream-start, one transform per event, then flush.</summary>
    public static List<LanguageModelStreamPart> Run(params string[] events)
    {
        var transform = new GoogleInteractionsStreamTransform(new List<CallWarning>(), false, () => "test-id");
        var parts = new List<LanguageModelStreamPart>(transform.Start());
        foreach (var json in events)
        {
            parts.AddRange(transform.Transform(new GoogleInteractionsEvent(json)));
        }

        parts.AddRange(transform.Flush());
        return parts;
    }

    public static JsonArray Snapshot(IEnumerable<LanguageModelStreamPart> parts)
    {
        var array = new JsonArray();
        foreach (var part in parts)
        {
            array.Add(Snapshot(part));
        }

        return array;
    }

    /// <summary>Renders a part the way upstream's inline snapshots show it. A value that upstream leaves undefined is left out.</summary>
    public static JsonObject Snapshot(LanguageModelStreamPart part)
    {
        var node = new JsonObject { ["type"] = part.Type };
        switch (part)
        {
            case StreamStartStreamPart start:
                node["warnings"] = new JsonArray(start.Warnings.Select(warning => (JsonNode?)JsonValue.Create(warning.Message)).ToArray());
                break;
            case ResponseMetadataStreamPart metadata:
                node["id"] = metadata.Id;
                node["modelId"] = metadata.ModelId;
                break;
            case TextStartStreamPart textStart:
                node["id"] = textStart.Id;
                break;
            case TextDeltaStreamPart textDelta:
                node["id"] = textDelta.Id;
                node["delta"] = textDelta.Delta;
                break;
            case TextEndStreamPart textEnd:
                node["id"] = textEnd.Id;
                Metadata(node, textEnd.ProviderMetadata);
                break;
            case ReasoningStartStreamPart reasoningStart:
                node["id"] = reasoningStart.Id;
                Metadata(node, reasoningStart.ProviderMetadata);
                break;
            case ReasoningDeltaStreamPart reasoningDelta:
                node["id"] = reasoningDelta.Id;
                node["delta"] = reasoningDelta.Delta;
                break;
            case ReasoningEndStreamPart reasoningEnd:
                node["id"] = reasoningEnd.Id;
                Metadata(node, reasoningEnd.ProviderMetadata);
                break;
            case ToolInputStartStreamPart inputStart:
                node["id"] = inputStart.Id;
                node["toolName"] = inputStart.ToolName;
                break;
            case ToolInputDeltaStreamPart inputDelta:
                node["id"] = inputDelta.Id;
                node["delta"] = inputDelta.Delta;
                break;
            case ToolInputEndStreamPart inputEnd:
                node["id"] = inputEnd.Id;
                break;
            case ToolCallStreamPart call:
                node["toolCallId"] = call.ToolCallId;
                node["toolName"] = call.ToolName;
                node["input"] = call.ArgumentsJson;
                Metadata(node, call.ProviderMetadata);
                if (call.ProviderExecuted)
                {
                    node["providerExecuted"] = true;
                }

                break;
            case ToolResultStreamPart result:
                node["toolCallId"] = result.ToolCallId;
                node["toolName"] = result.ToolName;
                node["result"] = JsonNode.Parse(result.Result.GetRawText());
                break;
            case SourceStreamPart source:
                node["sourceType"] = "url";
                node["id"] = source.Id;
                node["url"] = source.Url;
                if (source.Title != null)
                {
                    node["title"] = source.Title;
                }

                break;
            case DocumentSourceStreamPart document:
                node["sourceType"] = "document";
                node["id"] = document.Id;
                node["mediaType"] = document.MediaType;
                node["title"] = document.Title;
                if (document.Filename != null)
                {
                    node["filename"] = document.Filename;
                }

                break;
            case CustomStreamPart custom:
                node["kind"] = custom.Kind;
                Metadata(node, custom.ProviderMetadata);
                break;
            case FinishStreamPart finish:
                node["finishReason"] = new JsonObject
                {
                    ["raw"] = finish.RawFinishReason,
                    ["unified"] = finish.FinishReason == FinishReason.ToolCalls ? "tool-calls" : finish.FinishReason.ToString().ToLowerInvariant(),
                };
                Metadata(node, finish.ProviderMetadata);
                node["usage"] = new JsonObject
                {
                    ["inputTokens"] = Without(new JsonObject
                    {
                        ["cacheRead"] = finish.Usage.CacheReadTokens,
                        ["cacheWrite"] = finish.Usage.CacheWriteTokens,
                        ["noCache"] = finish.Usage.NoCacheInputTokens,
                        ["total"] = finish.Usage.InputTokens,
                    }),
                    ["outputTokens"] = Without(new JsonObject
                    {
                        ["reasoning"] = finish.Usage.ReasoningTokens,
                        ["text"] = finish.Usage.TextTokens,
                        ["total"] = finish.Usage.OutputTokens,
                    }),
                    ["raw"] = JsonNode.Parse(finish.Usage.Raw!.Value.GetRawText()),
                };
                break;
            default:
                throw new InvalidOperationException("Unexpected stream part " + part.Type);
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

    private static JsonObject Without(JsonObject node)
    {
        foreach (var name in node.Where(pair => pair.Value == null).Select(pair => pair.Key).ToList())
        {
            node.Remove(name);
        }

        return node;
    }
}
