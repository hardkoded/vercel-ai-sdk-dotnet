// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenResponses;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.OpenResponsesLanguageModelTests;

/// <summary>Helpers that <c>open-responses-language-model.test.ts</c> declares inside its <c>describe</c> blocks.</summary>
internal static class OpenResponsesLanguageModelTestSupport
{
    public static LanguageModelCallOptions TestPrompt()
    {
        return UpstreamChat.Prompt("Hello");
    }

    public static OpenResponsesLanguageModel CreateModel(UpstreamCapture capture, string modelId = "gemma-7b-it", params OpenResponsesExtension[] extensions)
    {
        var options = new OpenResponsesOptions
        {
            Name = "lmstudio",
            ApiKey = "test-key",
            BaseUrl = "https://localhost:1234/v1",
            Extensions = extensions,
        };
        return (OpenResponsesLanguageModel)OpenResponsesProvider.Create(options, capture).LanguageModel(modelId);
    }

    public static string ReadFixture(string name)
    {
        return File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "OpenResponses", name));
    }

    public static void PrepareJsonFixtureResponse(UpstreamCapture capture, string filename)
    {
        capture.ResponseBody = ReadFixture(filename + ".json");
    }

    public static void PrepareChunksFixtureResponse(UpstreamCapture capture, string filename)
    {
        var builder = new StringBuilder();
        foreach (var line in ReadFixture(filename + ".chunks.txt").Split('\n'))
        {
            if (line.Trim().Length > 0)
            {
                builder.Append("data: ").Append(line).Append("\n\n");
            }
        }

        builder.Append("data: [DONE]\n\n");
        capture.ResponseBody = builder.ToString();
        capture.MediaType = "text/event-stream";
    }

    public static void PrepareOutputResponse(UpstreamCapture capture, params JsonObject[] output)
    {
        capture.ResponseBody = new JsonObject
        {
            ["id"] = "resp_1",
            ["object"] = "response",
            ["created_at"] = 0,
            ["model"] = "test-model",
            ["status"] = "completed",
            ["output"] = new JsonArray(output.Select(item => (JsonNode)item.DeepClone()).ToArray()),
            ["usage"] = new JsonObject { ["input_tokens"] = 0, ["output_tokens"] = 0, ["total_tokens"] = 0 },
        }.ToJsonString();
    }

    public static void PrepareStreamChunks(UpstreamCapture capture, params JsonObject[] events)
    {
        capture.ResponseBody = UpstreamChat.Sse(events.Select(item => item.ToJsonString()).ToArray());
        capture.MediaType = "text/event-stream";
    }

    public static JsonObject RequestBody(UpstreamCapture capture, int call)
    {
        return JsonNode.Parse(capture.Requests[call].Body)!.AsObject();
    }

    public static JsonObject Obj(string json)
    {
        return JsonNode.Parse(json)!.AsObject();
    }

    public static JsonArray Arr(string json)
    {
        return JsonNode.Parse(json)!.AsArray();
    }

    public static void AssertJson(JsonNode? expected, JsonNode? actual)
    {
        Assert.True(
            JsonNode.DeepEquals(expected, actual),
            "Expected: " + (expected?.ToJsonString() ?? "null") + "\nActual:   " + (actual?.ToJsonString() ?? "null"));
    }

    public static void AssertMetadata(string expectedJson, JsonElement? actual)
    {
        Assert.NotNull(actual);
        AssertJson(JsonNode.Parse(expectedJson), JsonNode.Parse(actual!.Value.GetRawText()));
    }

    public static IReadOnlyDictionary<string, JsonElement>? Options(JsonElement? metadata)
    {
        return metadata is { ValueKind: JsonValueKind.Object } bag
            ? bag.EnumerateObject().ToDictionary(property => property.Name, property => property.Value)
            : null;
    }

    public static OpenResponsesExtension CreateDocumentSearchExtension(bool providerExecuted)
    {
        bool ProviderExecuted(JsonObject item)
        {
            return item["provider_executed"] is JsonValue flag && flag.TryGetValue<bool>(out var value) ? value : providerExecuted;
        }

        return new OpenResponsesExtension
        {
            Id = "acme.document_search",
            ToolType = "acme:document_search",
            ItemTypes = new[] { "acme:document_search_call", "acme:document_search_result", "acme:document_search_receipt" },
            EventTypes = new[] { "acme:document_search_input" },
            EncodeTool = context =>
            {
                var tool = new JsonObject { ["name"] = context.Name };
                foreach (var property in context.Args.EnumerateObject())
                {
                    tool[property.Name] = JsonNode.Parse(property.Value.GetRawText());
                }

                return tool;
            },
            EncodeToolChoice = context => new JsonObject { ["name"] = context.Name },
            DecodeItem = context =>
            {
                var item = context.Item;
                var type = (string)item["type"]!;
                if (type == "acme:document_search_call")
                {
                    return new GeneratedContent[]
                    {
                        new GeneratedToolCall((string)item["call_id"]!, (string)item["name"]!, item["query"]!.ToJsonString()) { ProviderExecuted = ProviderExecuted(item) },
                    };
                }

                if (type == "acme:document_search_result")
                {
                    return new GeneratedContent[]
                    {
                        new GeneratedToolResult((string)item["call_id"]!, (string)item["name"]!, item["result"]!.ToJsonString()),
                    };
                }

                return new GeneratedContent[]
                {
                    new GeneratedToolCall((string)item["call_id"]!, (string)item["name"]!, item["query"]!.ToJsonString()) { ProviderExecuted = ProviderExecuted(item) },
                    new GeneratedToolResult((string)item["call_id"]!, (string)item["name"]!, item["result"]!.ToJsonString()),
                };
            },
            EncodeInputItem = context =>
            {
                if (context.Part is AssistantToolCallPart call)
                {
                    return new[]
                    {
                        new JsonObject
                        {
                            ["type"] = "acme:document_search_call",
                            ["id"] = "call_item_" + call.ToolCallId,
                            ["status"] = "completed",
                            ["call_id"] = call.ToolCallId,
                            ["name"] = call.ToolName,
                            ["query"] = JsonNode.Parse(call.InputJson),
                        },
                    };
                }

                if (context.Part is AssistantToolResultPart result)
                {
                    return new[]
                    {
                        new JsonObject
                        {
                            ["type"] = "acme:document_search_result",
                            ["id"] = "result_item_" + result.ToolCallId,
                            ["status"] = "completed",
                            ["call_id"] = result.ToolCallId,
                            ["name"] = result.ToolName,
                            ["result"] = new JsonObject
                            {
                                ["type"] = result.Output.OutputType,
                                ["value"] = JsonNode.Parse(result.Output.OutputJson),
                            },
                        },
                    };
                }

                return null;
            },
            DecodeEvent = context =>
            {
                var evt = context.Event;
                var id = (string)evt["call_id"]!;
                return new LanguageModelStreamPart[]
                {
                    new ToolInputStartStreamPart(id, (string)evt["name"]!, null, ProviderExecuted(evt)),
                    new ToolInputDeltaStreamPart(id, (string)evt["delta"]!),
                    new ToolInputEndStreamPart(id),
                };
            },
        };
    }
}
