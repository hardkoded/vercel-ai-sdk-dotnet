// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using XaiProviderType = Vercel.AI.Xai.XaiProvider;
using XaiResponsesLanguageModelType = Vercel.AI.Xai.XaiResponsesLanguageModel;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel;

/// <summary>Fixtures and helpers shared by the <c>xai-responses-language-model.test.ts</c> ports.</summary>
internal static class XaiResponsesLanguageModelSupport
{
    internal const string TestFile = "packages/xai/src/responses/xai-responses-language-model.test.ts::XaiResponsesLanguageModel";

    /// <summary>The <c>response.created</c> event that starts most upstream stream cases.</summary>
    internal const string Created = """{"type":"response.created","response":{"id":"resp_123","object":"response","model":"grok-4-fast-non-reasoning","status":"in_progress","output":[]}}""";

    /// <summary>The <c>response.done</c> event that ends most upstream stream cases.</summary>
    internal const string Done = """{"type":"response.done","response":{"id":"resp_123","object":"response","status":"completed","output":[],"usage":{"input_tokens":10,"output_tokens":5}}}""";

    /// <summary>Creates a model whose generated ids are <c>id-0</c>, <c>id-1</c>, and so on, like the upstream <c>mockId</c>.</summary>
    internal static XaiResponsesLanguageModelType Model(UpstreamCapture capture, string modelId = "grok-4-fast-non-reasoning")
    {
        var provider = XaiProviderType.Create(
            new OpenAICompatibleOptions { ApiKey = "test-key", BaseUrl = "https://api.x.ai/v1" },
            capture);
        var counter = 0;
        return new XaiResponsesLanguageModelType(provider, modelId, () => "id-" + counter++);
    }

    internal static LanguageModelCallOptions HelloPrompt()
    {
        return UpstreamChat.Prompt("hello");
    }

    /// <summary>A scripted JSON response with the given extra properties.</summary>
    internal static UpstreamCapture JsonResponse(string json)
    {
        return new UpstreamCapture { ResponseBody = json };
    }

    /// <summary>A completed response with no output and a small usage.</summary>
    internal static UpstreamCapture EmptyResponse(string model = "grok-4-fast-non-reasoning", string extra = "")
    {
        return JsonResponse("{\"id\":\"resp_123\",\"object\":\"response\",\"status\":\"completed\",\"model\":\"" + model + "\",\"output\":[],\"usage\":{\"input_tokens\":10,\"output_tokens\":5}" + extra + "}");
    }

    /// <summary>A server-sent-event response. Each argument is the JSON of one event.</summary>
    internal static UpstreamCapture StreamResponse(params string[] events)
    {
        return new UpstreamCapture { MediaType = "text/event-stream", ResponseBody = UpstreamChat.Sse(events) };
    }

    /// <summary>A server-sent-event response that replays a recorded <c>.chunks.txt</c> fixture.</summary>
    internal static UpstreamCapture ChunksFixture(string name)
    {
        var lines = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "Fixtures", name)).Where(line => line.Trim().Length > 0);
        return StreamResponse(lines.ToArray());
    }

    /// <summary>Replays a recorded JSON response, like the upstream <c>prepareJsonFixtureResponse</c>.</summary>
    internal static UpstreamCapture JsonFixture(string name)
    {
        return JsonResponse(File.ReadAllText(Fixture(name + ".json")));
    }

    /// <summary>An upstream snapshot converted to JSON and stored next to the fixtures. Unset fields are left out.</summary>
    internal static JsonNode Expected(string fileName)
    {
        return JsonNode.Parse(File.ReadAllText(Fixture(fileName)))!;
    }

    /// <summary>A provider-defined tool, like <c>{ type: 'provider', id, name, args }</c>.</summary>
    internal static LanguageModelTool ProviderTool(string id, string name, string args = "{}")
    {
        return LanguageModelTool.ProviderDefined(id, name, UpstreamChat.Json(args));
    }

    /// <summary>Call options with the hello prompt and <paramref name="tools"/>.</summary>
    internal static LanguageModelCallOptions WithTools(params LanguageModelTool[] tools)
    {
        var options = HelloPrompt();
        options.Tools = tools;
        return options;
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
            case ResponseMetadataStreamPart metadata:
                Set(json, "id", metadata.Id);
                Set(json, "modelId", metadata.ModelId);
                Set(json, "timestamp", metadata.Timestamp?.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture));
                break;
            case TextStartStreamPart text:
                json["id"] = text.Id;
                Metadata(json, text.ProviderMetadata);
                break;
            case TextDeltaStreamPart text:
                json["id"] = text.Id;
                json["delta"] = text.Delta;
                Metadata(json, text.ProviderMetadata);
                break;
            case TextEndStreamPart text:
                json["id"] = text.Id;
                Metadata(json, text.ProviderMetadata);
                break;
            case ReasoningStartStreamPart reasoning:
                json["id"] = reasoning.Id;
                Metadata(json, reasoning.ProviderMetadata);
                break;
            case ReasoningDeltaStreamPart reasoning:
                json["id"] = reasoning.Id;
                json["delta"] = reasoning.Delta;
                Metadata(json, reasoning.ProviderMetadata);
                break;
            case ReasoningEndStreamPart reasoning:
                json["id"] = reasoning.Id;
                Metadata(json, reasoning.ProviderMetadata);
                break;
            case ToolInputStartStreamPart input:
                json["id"] = input.Id;
                json["toolName"] = input.ToolName;
                if (input.ProviderExecuted is { } inputProviderExecuted)
                {
                    json["providerExecuted"] = inputProviderExecuted;
                }

                Metadata(json, input.ProviderMetadata);
                break;
            case ToolInputDeltaStreamPart input:
                json["id"] = input.Id;
                json["delta"] = input.Delta;
                break;
            case ToolInputEndStreamPart input:
                json["id"] = input.Id;
                break;
            case ToolCallStreamPart call:
                json["toolCallId"] = call.ToolCallId;
                json["toolName"] = call.ToolName;
                json["input"] = call.ArgumentsJson;
                if (call.ProviderExecuted)
                {
                    json["providerExecuted"] = true;
                }

                Metadata(json, call.ProviderMetadata);
                break;
            case ToolResultStreamPart result:
                json["toolCallId"] = result.ToolCallId;
                json["toolName"] = result.ToolName;
                json["result"] = JsonNode.Parse(result.Result.GetRawText());
                if (result.IsError)
                {
                    json["isError"] = true;
                }

                break;
            case SourceStreamPart source:
                json["sourceType"] = "url";
                json["id"] = source.Id;
                json["url"] = source.Url;
                Set(json, "title", source.Title);
                Metadata(json, source.ProviderMetadata);
                break;
            case FinishStreamPart finish:
                var finishReason = new JsonObject { ["unified"] = Unified(finish.FinishReason) };
                Set(finishReason, "raw", finish.RawFinishReason);
                json["finishReason"] = finishReason;
                json["usage"] = Usage(finish.Usage);
                Metadata(json, finish.ProviderMetadata);
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
                    Metadata(json, text.ProviderMetadata);
                    break;
                case GeneratedReasoning reasoning:
                    json["text"] = reasoning.Text;
                    Metadata(json, reasoning.ProviderMetadata);
                    break;
                case GeneratedToolCall call:
                    json["toolCallId"] = call.ToolCallId;
                    json["toolName"] = call.ToolName;
                    json["input"] = call.ArgumentsJson;
                    if (call.ProviderExecuted)
                    {
                        json["providerExecuted"] = true;
                    }

                    Metadata(json, call.ProviderMetadata);
                    break;
                case GeneratedToolResult result:
                    json["toolCallId"] = result.ToolCallId;
                    json["toolName"] = result.ToolName;
                    json["result"] = JsonNode.Parse(result.ResultJson);
                    if (result.IsError)
                    {
                        json["isError"] = true;
                    }

                    Metadata(json, result.ProviderMetadata);
                    break;
                case GeneratedSource source:
                    json["sourceType"] = "url";
                    json["id"] = source.Id;
                    json["url"] = source.Url;
                    Set(json, "title", source.Title);
                    Metadata(json, source.ProviderMetadata);
                    break;
            }

            array.Add(json);
        }

        return array;
    }

    /// <summary>Asserts that <paramref name="actual"/> holds an item equal to <paramref name="expected"/>, like <c>toContainEqual</c>.</summary>
    internal static void ContainsEqual(JsonArray actual, string expected)
    {
        var node = JsonNode.Parse(expected);
        Assert.True(
            actual.Any(item => JsonNode.DeepEquals(item, node)),
            "Expected an item equal to " + node!.ToJsonString() + " in " + actual.ToJsonString());
    }

    private static string Unified(FinishReason finishReason)
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

    private static JsonObject Usage(LanguageModelUsage usage)
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

    private static void Metadata(JsonObject json, JsonElement? metadata)
    {
        if (metadata is { } value)
        {
            json["providerMetadata"] = JsonNode.Parse(value.GetRawText());
        }
    }

    private static void Set(JsonObject json, string name, string? value)
    {
        if (value != null)
        {
            json[name] = value;
        }
    }

    private static void Set(JsonObject json, string name, int? value)
    {
        if (value.HasValue)
        {
            json[name] = value.Value;
        }
    }

    private static string Fixture(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName);
    }

    internal static JsonObject Body(UpstreamCapture capture)
    {
        return UpstreamChat.Body(capture);
    }

    internal static async Task<List<LanguageModelStreamPart>> Collect(XaiResponsesLanguageModelType model, LanguageModelCallOptions options)
    {
        return await UpstreamChat.Read((await model.DoStreamAsync(options, CancellationToken.None)));
    }

    internal static LanguageModelTool WeatherTool()
    {
        return new LanguageModelTool(
            "weather",
            "get weather",
            UpstreamChat.Json("{\"type\":\"object\",\"properties\":{\"location\":{\"type\":\"string\"}}}"));
    }

    /// <summary>The first stream part of type <typeparamref name="T"/>.</summary>
    internal static T First<T>(IEnumerable<LanguageModelStreamPart> parts)
        where T : LanguageModelStreamPart
    {
        return parts.OfType<T>().First();
    }

}
