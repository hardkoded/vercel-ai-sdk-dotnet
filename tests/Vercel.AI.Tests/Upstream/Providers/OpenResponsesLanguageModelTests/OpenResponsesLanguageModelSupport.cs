// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenResponses;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.OpenResponsesLanguageModelTests;

/// <summary>Fixtures and helpers shared by the <c>open-responses-language-model.test.ts</c> ports.</summary>
internal static class OpenResponsesLanguageModelSupport
{
    /// <summary>A model named <c>lmstudio</c>, like the upstream <c>createModel</c>.</summary>
    internal static OpenResponsesLanguageModel Model(UpstreamCapture capture, string modelId = "gemma-7b-it")
    {
        var options = new OpenResponsesOptions { ApiKey = "test-key", BaseUrl = "https://localhost:1234/v1", Name = "lmstudio" };
        return (OpenResponsesLanguageModel)OpenResponsesProvider.Create(options, capture).LanguageModel(modelId);
    }

    /// <summary>Replays a recorded JSON response, like the upstream <c>prepareJsonFixtureResponse</c>.</summary>
    internal static UpstreamCapture JsonFixture(string name)
    {
        return new UpstreamCapture { ResponseBody = File.ReadAllText(Fixture(name + ".json")) };
    }

    /// <summary>Replays a recorded <c>.chunks.txt</c> fixture, like the upstream <c>prepareChunksFixtureResponse</c>.</summary>
    internal static UpstreamCapture ChunksFixture(string name)
    {
        var lines = File.ReadAllLines(Fixture(name + ".chunks.txt")).Where(line => line.Trim().Length > 0);
        return StreamResponse(lines.ToArray());
    }

    /// <summary>A server-sent-event response. Each argument is the JSON of one event.</summary>
    internal static UpstreamCapture StreamResponse(params string[] events)
    {
        return new UpstreamCapture { MediaType = "text/event-stream", ResponseBody = UpstreamChat.Sse(events) };
    }

    internal static Task<List<LanguageModelStreamPart>> Collect(OpenResponsesLanguageModel model, LanguageModelCallOptions options)
    {
        return UpstreamChat.Read(model.DoStreamAsync(options, CancellationToken.None));
    }

    /// <summary>The expected stream parts of an upstream snapshot, stored as JSON next to the fixtures.</summary>
    internal static JsonNode ExpectedParts(string name)
    {
        return JsonNode.Parse(File.ReadAllText(Fixture(name + ".stream.json")))!;
    }

    /// <summary>Stream parts in the upstream shape. Unset fields are left out, like <c>undefined</c> in a snapshot.</summary>
    internal static JsonArray ToJson(IEnumerable<LanguageModelStreamPart> parts)
    {
        var array = new JsonArray();
        foreach (var part in parts)
        {
            array.Add(ToJson(part));
        }

        return array;
    }

    internal static JsonObject ToJson(LanguageModelStreamPart part)
    {
        var json = new JsonObject { ["type"] = part.Type };
        switch (part)
        {
            case StreamStartStreamPart start:
                var warnings = new JsonArray();
                foreach (var warning in start.Warnings)
                {
                    warnings.Add(new JsonObject { ["type"] = warning.Type, ["message"] = warning.Message });
                }

                json["warnings"] = warnings;
                break;
            case TextStartStreamPart text:
                Add(json, text.Id, null, text.ProviderMetadata);
                break;
            case TextDeltaStreamPart text:
                Add(json, text.Id, text.Delta, text.ProviderMetadata);
                break;
            case TextEndStreamPart text:
                Add(json, text.Id, null, text.ProviderMetadata);
                break;
            case ReasoningStartStreamPart reasoning:
                Add(json, reasoning.Id, null, reasoning.ProviderMetadata);
                break;
            case ReasoningDeltaStreamPart reasoning:
                Add(json, reasoning.Id, reasoning.Delta, null);
                break;
            case ReasoningEndStreamPart reasoning:
                Add(json, reasoning.Id, null, reasoning.ProviderMetadata);
                break;
            case ToolCallStreamPart call:
                json["toolCallId"] = call.ToolCallId;
                json["toolName"] = call.ToolName;
                json["input"] = call.ArgumentsJson;
                Add(json, null, null, call.ProviderMetadata);
                break;
            case ErrorStreamPart error:
                json["error"] = error.Message;
                break;
            case FinishStreamPart finish:
                var finishReason = new JsonObject { ["unified"] = Unified(finish.FinishReason) };
                Set(finishReason, "raw", finish.RawFinishReason);
                json["finishReason"] = finishReason;
                json["usage"] = Usage(finish.Usage);
                Add(json, null, null, finish.ProviderMetadata);
                break;
        }

        return json;
    }

    /// <summary>Generated content in the upstream shape.</summary>
    internal static JsonArray ToJson(IEnumerable<GeneratedContent> content)
    {
        var array = new JsonArray();
        foreach (var part in content)
        {
            var json = new JsonObject { ["type"] = part.Type };
            switch (part)
            {
                case GeneratedText text:
                    json["text"] = text.Text;
                    break;
                case GeneratedReasoning reasoning:
                    json["text"] = reasoning.Text;
                    Add(json, null, null, reasoning.ProviderMetadata);
                    break;
                case GeneratedToolCall call:
                    json["toolCallId"] = call.ToolCallId;
                    json["toolName"] = call.ToolName;
                    json["input"] = call.ArgumentsJson;
                    Add(json, null, null, call.ProviderMetadata);
                    break;
            }

            array.Add(json);
        }

        return array;
    }

    /// <summary>A function tool with a JSON schema.</summary>
    internal static LanguageModelTool Tool(string name, string description, string schema, bool? strict = null)
    {
        return new LanguageModelTool(name, description, UpstreamChat.Json(schema), strict);
    }

    internal static string Unified(FinishReason finishReason)
    {
        return finishReason switch
        {
            FinishReason.Stop => "stop",
            FinishReason.Length => "length",
            FinishReason.ContentFilter => "content-filter",
            FinishReason.ToolCalls => "tool-calls",
            FinishReason.Error => "error",
            _ => "other",
        };
    }

    /// <summary>Usage in the upstream shape.</summary>
    internal static JsonObject Usage(LanguageModelUsage usage)
    {
        var input = new JsonObject();
        Set(input, "total", usage.InputTokens);
        Set(input, "noCache", usage.NoCacheInputTokens);
        Set(input, "cacheRead", usage.CacheReadTokens);
        Set(input, "cacheWrite", usage.CacheWriteTokens);
        var output = new JsonObject();
        Set(output, "total", usage.OutputTokens);
        Set(output, "text", usage.TextTokens);
        Set(output, "reasoning", usage.ReasoningTokens);
        var json = new JsonObject { ["inputTokens"] = input, ["outputTokens"] = output };
        if (usage.Raw is { } raw)
        {
            json["raw"] = JsonNode.Parse(raw.GetRawText());
        }

        return json;
    }

    private static void Add(JsonObject json, string? id, string? delta, JsonElement? providerMetadata)
    {
        Set(json, "id", id);
        Set(json, "delta", delta);
        if (providerMetadata is { } metadata)
        {
            json["providerMetadata"] = JsonNode.Parse(metadata.GetRawText());
        }
    }

    private static void Set(JsonObject json, string name, JsonNode? value)
    {
        if (value != null)
        {
            json[name] = value;
        }
    }

    private static string Fixture(string name)
    {
        return Path.Combine(AppContext.BaseDirectory, "Fixtures", "open-responses-" + name);
    }
}
