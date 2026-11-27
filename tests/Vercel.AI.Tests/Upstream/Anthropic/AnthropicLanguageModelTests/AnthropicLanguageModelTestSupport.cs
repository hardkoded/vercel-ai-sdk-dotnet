// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>
/// Helpers of <c>anthropic-language-model.test.ts</c>: the <c>prepareJsonFixtureResponse</c> and <c>prepareChunksFixtureResponse</c>
/// functions, the test server calls, and the JSON shape that <c>toMatchInlineSnapshot</c> and <c>toMatchSnapshot</c> compare.
/// </summary>
internal static class AnthropicLanguageModelTestSupport
{
    public const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > ";

    public const string Haiku = "claude-3-haiku-20240307";

    private static readonly Lazy<JsonObject> Snapshots = new(() =>
        JsonNode
            .Parse(
                File.ReadAllText(
                    Path.Combine(
                        AppContext.BaseDirectory,
                        "Fixtures",
                        "anthropic-language-model.tools-and-options.snap.json"
                    )
                )
            )!
            .AsObject()
    );

    public static string FixtureText(string name, string extension = ".json")
    {
        return File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "Fixtures", name + extension)
        );
    }

    /// <summary>Maps to <c>prepareJsonFixtureResponse</c>.</summary>
    public static CaptureHandler JsonFixture(string name)
    {
        // The upstream helper parses the fixture, and the test server stringifies it again.
        return new CaptureHandler
        {
            ResponseBody = JsonNode
                .Parse(FixtureText(name))!
                .ToJsonString(
                    new JsonSerializerOptions
                    {
                        Encoder = System
                            .Text
                            .Encodings
                            .Web
                            .JavaScriptEncoder
                            .UnsafeRelaxedJsonEscaping,
                    }
                ),
        };
    }

    /// <summary>Maps to <c>server.urls[...].response = { type: 'json-value', body }</c>.</summary>
    public static CaptureHandler JsonValue(string body)
    {
        return new CaptureHandler { ResponseBody = body };
    }

    /// <summary>Maps to <c>prepareChunksFixtureResponse</c>.</summary>
    public static CaptureHandler ChunksFixture(string name)
    {
        var chunks = FixtureText(name, ".chunks.txt")
            .Split('\n')
            .Select(line => $"data: {line}\n\n");
        return StreamChunks(chunks.Append("data: [DONE]\n\n"));
    }

    /// <summary>Maps to <c>server.urls[...].response = { type: 'stream-chunks', chunks }</c>.</summary>
    public static CaptureHandler StreamChunks(IEnumerable<string> chunks)
    {
        return new CaptureHandler
        {
            ContentType = "text/event-stream",
            ResponseBody = string.Concat(chunks),
        };
    }

    public static AnthropicProvider Provider(CaptureHandler handler)
    {
        var next = 0;
        return AnthropicProvider.Create(
            new AnthropicOptions
            {
                ApiKey = "test-api-key",
                GenerateId = () => "id-" + next++.ToString(CultureInfo.InvariantCulture),
            },
            handler
        );
    }

    public static Task<LanguageModelGenerateResult> DoGenerate(
        CaptureHandler handler,
        LanguageModelCallOptions options,
        string model = Haiku
    )
    {
        return Provider(handler)
            .LanguageModel(model)
            .DoGenerateAsync(options, CancellationToken.None);
    }

    /// <summary>Maps to <c>convertReadableStreamToArray(stream)</c> of <c>doStream</c>.</summary>
    public static async Task<List<LanguageModelStreamPart>> DoStream(
        CaptureHandler handler,
        LanguageModelCallOptions options,
        string model = Haiku
    )
    {
        var parts = new List<LanguageModelStreamPart>();
        await foreach (
            var part in await Provider(handler)
                .LanguageModel(model)
                .DoStreamAsync(options, CancellationToken.None)
        )
        {
            parts.Add(part);
        }

        return parts;
    }

    /// <summary>Maps to <c>TEST_PROMPT</c> with the given text.</summary>
    public static LanguageModelCallOptions Options(
        string? providerOptions = null,
        IReadOnlyList<LanguageModelTool>? tools = null,
        string text = "Hello"
    )
    {
        var options = AnthropicParity.Hello(text);
        options.Tools = tools;
        if (providerOptions != null)
        {
            options.ProviderOptions = new Dictionary<string, JsonElement>
            {
                ["anthropic"] = AnthropicParity.Json(providerOptions),
            };
        }

        return options;
    }

    /// <summary>Maps to <c>{ type: 'provider', id, name, args }</c>.</summary>
    public static LanguageModelTool ProviderTool(string id, string name, string args = "{}")
    {
        return LanguageModelTool.ProviderDefined(id, name, AnthropicParity.Json(args));
    }

    /// <summary>Maps to <c>{ type: 'function', name, description, inputSchema, providerOptions }</c>.</summary>
    public static LanguageModelTool FunctionTool(
        string name,
        string? description,
        string inputSchema,
        string? providerOptions = null
    )
    {
        return new LanguageModelTool(name, description, AnthropicParity.Json(inputSchema))
        {
            ProviderOptions =
                providerOptions == null ? null : AnthropicParity.Json(providerOptions),
        };
    }

    /// <summary>The request body of <c>TEST_PROMPT</c> that most inline snapshots start from, with the given extra properties.</summary>
    public static JsonObject HelloBody(
        string extra = "{}",
        string model = Haiku,
        int maxTokens = 4096
    )
    {
        var body = new JsonObject
        {
            ["max_tokens"] = maxTokens,
            ["messages"] = JsonNode.Parse(
                "[{\"content\":[{\"text\":\"Hello\",\"type\":\"text\"}],\"role\":\"user\"}]"
            ),
            ["model"] = model,
        };
        foreach (var (key, value) in JsonNode.Parse(extra)!.AsObject().ToList())
        {
            body[key] = value?.DeepClone();
        }

        return body;
    }

    /// <summary>The stored value of an upstream <c>toMatchSnapshot()</c> entry.</summary>
    public static JsonNode Snapshot(string key)
    {
        return Snapshots.Value[key]!.DeepClone();
    }

    public static JsonObject RequestBody(CaptureHandler handler)
    {
        return JsonNode.Parse(handler.Body)!.AsObject();
    }

    /// <summary>Asserts the request headers the way <c>toMatchInlineSnapshot</c> of <c>requestHeaders</c> does.</summary>
    public static void AssertRequestHeaders(
        CaptureHandler handler,
        params (string Name, string Value)[] expected
    )
    {
        var actual = new JsonObject();
        foreach (var header in handler.Headers.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            if (!ExcludedHeaders.Contains(header.Key))
            {
                actual[header.Key.ToLowerInvariant()] = header.Key.Equals(
                    "content-type",
                    StringComparison.OrdinalIgnoreCase
                )
                    ? header.Value.Split(';')[0]
                    : header.Value;
            }
        }

        var expectedNode = new JsonObject();
        foreach (var (name, value) in expected)
        {
            expectedNode[name] = value;
        }

        AssertJson(expectedNode, actual);
    }

    public static string Beta(CaptureHandler handler)
    {
        return handler.Headers["anthropic-beta"];
    }

    public static void AssertJson(string expected, JsonNode? actual)
    {
        AssertJson(JsonNode.Parse(expected), actual);
    }

    /// <summary>Maps to <c>toEqual</c> and <c>toMatchInlineSnapshot</c> on a JSON value. Object key order does not matter.</summary>
    public static void AssertJson(JsonNode? expected, JsonNode? actual)
    {
        Assert.True(
            JsonNode.DeepEquals(expected, actual),
            "First difference at " + Difference(expected, actual, "$")
        );
    }

    private static string Difference(JsonNode? expected, JsonNode? actual, string path)
    {
        if (expected is JsonObject expectedObject && actual is JsonObject actualObject)
        {
            foreach (
                var key in expectedObject
                    .Select(pair => pair.Key)
                    .Union(actualObject.Select(pair => pair.Key))
            )
            {
                expectedObject.TryGetPropertyValue(key, out var expectedValue);
                actualObject.TryGetPropertyValue(key, out var actualValue);
                if (!JsonNode.DeepEquals(expectedValue, actualValue))
                {
                    return Difference(expectedValue, actualValue, path + "." + key);
                }
            }
        }

        if (expected is JsonArray expectedArray && actual is JsonArray actualArray)
        {
            for (var index = 0; index < Math.Max(expectedArray.Count, actualArray.Count); index++)
            {
                var expectedValue = index < expectedArray.Count ? expectedArray[index] : null;
                var actualValue = index < actualArray.Count ? actualArray[index] : null;
                if (!JsonNode.DeepEquals(expectedValue, actualValue))
                {
                    return Difference(expectedValue, actualValue, path + "[" + index + "]");
                }
            }
        }

        return path
            + ": expected "
            + Truncate(expected?.ToJsonString())
            + " but got "
            + Truncate(actual?.ToJsonString());
    }

    private static string Truncate(string? value)
    {
        return value == null ? "<missing>"
            : value.Length > 400 ? value[..400] + "..."
            : value;
    }

    /// <summary>Maps to <c>toMatchObject</c> on a JSON object: every property of <paramref name="expected"/> must match.</summary>
    public static void AssertJsonContains(JsonNode? expected, JsonNode? actual)
    {
        Assert.True(
            Contains(expected, actual),
            "Expected to contain "
                + expected?.ToJsonString()
                + Environment.NewLine
                + "but got  "
                + actual?.ToJsonString()
        );
    }

    public static JsonNode ContentJson(IEnumerable<GeneratedContent> content)
    {
        return new JsonArray(content.Select(part => (JsonNode?)ContentPartJson(part)).ToArray());
    }

    public static JsonNode WarningsJson(IEnumerable<CallWarning> warnings)
    {
        return new JsonArray(
            warnings
                .Select(warning =>
                {
                    var node = new JsonObject { ["type"] = warning.Type };
                    if (warning.Feature != null)
                    {
                        node["feature"] = warning.Feature;
                        if (warning.Message != warning.Feature)
                        {
                            node["details"] = warning.Message;
                        }
                    }
                    else
                    {
                        node["message"] = warning.Message;
                    }

                    return (JsonNode?)node;
                })
                .ToArray()
        );
    }

    public static JsonNode UsageJson(LanguageModelUsage usage)
    {
        var node = new JsonObject
        {
            ["inputTokens"] = new JsonObject
            {
                ["cacheRead"] = usage.CacheReadTokens,
                ["cacheWrite"] = usage.CacheWriteTokens,
                ["noCache"] = usage.NoCacheInputTokens,
                ["total"] = usage.InputTokens,
            },
            ["outputTokens"] = new JsonObject
            {
                ["reasoning"] = usage.ReasoningTokens,
                ["text"] = usage.TextTokens,
                ["total"] = usage.OutputTokens,
            },
        };
        node["raw"] = usage.Raw == null ? null : JsonNode.Parse(usage.Raw.Value.GetRawText());
        return RemoveNulls(node, keepNullKeys: ["raw"]);
    }

    public static JsonNode FinishReasonJson(FinishReason reason, string? raw)
    {
        return new JsonObject { ["raw"] = raw, ["unified"] = UnifiedFinishReason(reason) };
    }

    public static string UnifiedFinishReason(FinishReason reason)
    {
        return reason switch
        {
            FinishReason.Stop => "stop",
            FinishReason.Length => "length",
            FinishReason.ContentFilter => "content-filter",
            FinishReason.ToolCalls => "tool-calls",
            FinishReason.Error => "error",
            FinishReason.Other => "other",
            _ => "interrupted",
        };
    }

    public static JsonNode StreamJson(IEnumerable<LanguageModelStreamPart> parts)
    {
        return new JsonArray(parts.Select(part => (JsonNode?)StreamPartJson(part)).ToArray());
    }

    public static JsonNode StreamPartJson(LanguageModelStreamPart part)
    {
        var node = new JsonObject { ["type"] = part.Type };
        switch (part)
        {
            case StreamStartStreamPart start:
                node["warnings"] = WarningsJson(start.Warnings);
                break;
            case ResponseMetadataStreamPart metadata:
                node["id"] = metadata.Id;
                node["modelId"] = metadata.ModelId;
                break;
            case CustomStreamPart custom:
                node["kind"] = custom.Kind;
                node["providerMetadata"] = Parse(custom.ProviderMetadata);
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
                node["input"] = call.ArgumentsJson;
                node["toolCallId"] = call.ToolCallId;
                node["toolName"] = call.ToolName;
                if (call.ProviderExecuted)
                {
                    node["providerExecuted"] = true;
                }

                if (call.Dynamic)
                {
                    node["dynamic"] = true;
                }

                if (call.ProviderMetadata != null)
                {
                    node["providerMetadata"] = Parse(call.ProviderMetadata);
                }

                break;
            case FinishStreamPart finish:
                node["finishReason"] = FinishReasonJson(
                    finish.FinishReason,
                    finish.RawFinishReason
                );
                node["usage"] = UsageJson(finish.Usage);
                if (finish.ProviderMetadata != null)
                {
                    node["providerMetadata"] = Parse(finish.ProviderMetadata);
                }

                break;
            default:
                throw new InvalidOperationException("Unexpected stream part " + part.Type);
        }

        return node;
    }

    private static readonly HashSet<string> ExcludedHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "user-agent",
        "accept",
        "content-length",
    };

    private static JsonNode? Parse(JsonElement? element)
    {
        return element == null ? null : JsonNode.Parse(element.Value.GetRawText());
    }

    private static JsonNode? Parse(string json)
    {
        return JsonNode.Parse(json);
    }

    private static JsonObject ContentPartJson(GeneratedContent part)
    {
        var node = new JsonObject { ["type"] = part.Type };
        switch (part)
        {
            case AnthropicReasoningContent reasoning:
                node["text"] = reasoning.Text;
                node["providerMetadata"] = Parse(reasoning.ProviderMetadata);
                break;
            case GeneratedText text:
                node["text"] = text.Text;
                if (text.ProviderMetadata != null)
                {
                    node["providerMetadata"] = Parse(text.ProviderMetadata);
                }

                break;
            case GeneratedToolCall call:
                node["input"] = call.ArgumentsJson;
                node["toolCallId"] = call.ToolCallId;
                node["toolName"] = call.ToolName;
                if (call.ProviderExecuted)
                {
                    node["providerExecuted"] = true;
                }

                if (call.Dynamic)
                {
                    node["dynamic"] = true;
                }

                if (call.ProviderMetadata != null)
                {
                    node["providerMetadata"] = Parse(call.ProviderMetadata);
                }

                break;
            case AnthropicToolResultContent result:
                node["toolCallId"] = result.ToolCallId;
                node["toolName"] = result.ToolName;
                node["result"] = Parse(result.ResultJson);

                // An MCP result maps `isError: part.is_error`, so it always carries the flag.
                if (result.IsError || result.Dynamic)
                {
                    node["isError"] = result.IsError;
                }

                if (result.Dynamic)
                {
                    node["dynamic"] = true;
                }

                if (result.ProviderMetadata != null)
                {
                    node["providerMetadata"] = Parse(result.ProviderMetadata);
                }

                break;
            case GeneratedToolResult result:
                node["toolCallId"] = result.ToolCallId;
                node["toolName"] = result.ToolName;
                node["result"] = Parse(result.ResultJson);
                if (result.IsError)
                {
                    node["isError"] = true;
                }

                if (result.ProviderMetadata != null)
                {
                    node["providerMetadata"] = Parse(result.ProviderMetadata);
                }

                break;
            case GeneratedSource source:
                node["sourceType"] = "url";
                node["id"] = source.Id;
                node["url"] = source.Url;
                node["title"] = source.Title;
                if (source.ProviderMetadata != null)
                {
                    node["providerMetadata"] = Parse(source.ProviderMetadata);
                }

                break;
            case AnthropicCustomContent custom:
                node["kind"] = custom.Kind;
                if (custom.ProviderMetadata != null)
                {
                    node["providerMetadata"] = Parse(custom.ProviderMetadata);
                }

                break;
            default:
                throw new InvalidOperationException("Unexpected content " + part.Type);
        }

        return node;
    }

    private static JsonObject RemoveNulls(JsonObject node, string[] keepNullKeys)
    {
        foreach (var (key, value) in node.ToList())
        {
            if (value is JsonObject child)
            {
                RemoveNulls(child, keepNullKeys);
            }
            else if (value == null && !keepNullKeys.Contains(key))
            {
                node.Remove(key);
            }
        }

        return node;
    }

    private static bool Contains(JsonNode? expected, JsonNode? actual)
    {
        switch (expected)
        {
            case JsonObject expectedObject:
                return actual is JsonObject actualObject
                    && expectedObject.All(pair =>
                        actualObject.TryGetPropertyValue(pair.Key, out var value)
                        && Contains(pair.Value, value)
                    );
            case JsonArray expectedArray:
                return actual is JsonArray actualArray
                    && actualArray.Count == expectedArray.Count
                    && expectedArray
                        .Select((item, index) => Contains(item, actualArray[index]))
                        .All(x => x);
            default:
                return JsonNode.DeepEquals(expected, actual);
        }
    }
}
