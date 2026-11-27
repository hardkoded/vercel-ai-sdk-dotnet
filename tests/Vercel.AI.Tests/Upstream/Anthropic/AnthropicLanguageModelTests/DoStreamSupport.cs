// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Shared setup for the <c>doStream</c> ports of <c>anthropic-language-model.test.ts</c>.</summary>
internal static class DoStreamSupport
{
    /// <summary>One server-sent event, like the <c>data: ...\n\n</c> chunks the upstream tests write by hand.</summary>
    public static string Event(string json)
    {
        return "data: " + json + "\n\n";
    }

    /// <summary>The body of a server-sent event response with the given events.</summary>
    public static string SseBody(params string[] events)
    {
        return string.Concat(events.Select(Event));
    }

    /// <summary>Builds a handler that answers with the given server-sent events, like upstream's <c>stream-chunks</c> response.</summary>
    public static SseStreamHandler Stream(params string[] events)
    {
        return new SseStreamHandler(events.Select(Event));
    }

    /// <summary>Like upstream's <c>prepareChunksFixtureResponse</c>.</summary>
    public static SseStreamHandler FixtureStream(string name)
    {
        var chunks = File.ReadAllText(
                Path.Combine(AppContext.BaseDirectory, "Fixtures", name + ".chunks.txt")
            )
            .Split('\n')
            .Select(line => "data: " + line + "\n\n")
            .ToList();
        chunks.Add("data: [DONE]\n\n");
        return new SseStreamHandler(chunks);
    }

    /// <summary>Like upstream's <c>createAnthropic({ apiKey: 'test-api-key', generateId: mockId({ prefix: 'id' }) })</c>.</summary>
    public static AnthropicProvider Provider(
        HttpMessageHandler handler,
        Action<AnthropicOptions>? configure = null
    )
    {
        var counter = 0;
        var options = new AnthropicOptions
        {
            ApiKey = "test-api-key",
            GenerateId = () => "id-" + (counter++).ToString(CultureInfo.InvariantCulture),
        };
        configure?.Invoke(options);
        return AnthropicProvider.Create(options, handler);
    }

    public static async Task<List<LanguageModelStreamPart>> Collect(
        ILanguageModel model,
        LanguageModelCallOptions options
    )
    {
        var result = await model.DoStreamAsync(options, CancellationToken.None);
        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in result.ConfigureAwait(false))
        {
            parts.Add(part);
        }

        return parts;
    }

    /// <summary>Reads the stream of <c>model</c> for the default <c>Hello</c> prompt.</summary>
    public static Task<List<LanguageModelStreamPart>> Collect(
        AnthropicProvider provider,
        string modelId,
        LanguageModelCallOptions? options = null
    )
    {
        return Collect(provider.LanguageModel(modelId), options ?? AnthropicParity.Hello());
    }

    public static FinishStreamPart Finish(IEnumerable<LanguageModelStreamPart> parts)
    {
        return parts.OfType<FinishStreamPart>().First();
    }

    public static JsonNode? AnthropicMetadata(FinishStreamPart finish, string key)
    {
        return
            finish.ProviderMetadata is { } metadata
            && metadata.TryGetProperty("anthropic", out var anthropic)
            && anthropic.TryGetProperty(key, out var value)
            ? JsonNode.Parse(value.GetRawText())
            : null;
    }

    /// <summary>
    /// The request headers like upstream's test server records them. The HTTP client adds <c>user-agent</c> and
    /// <c>content-length</c> and a charset to <c>content-type</c>, which the server of the upstream tests does not report.
    /// </summary>
    public static JsonNode RequestHeaders(CaptureHandler handler)
    {
        var headers = new JsonObject();
        foreach (var pair in handler.Headers)
        {
            var name = pair.Key.ToLowerInvariant();
            if (name is "user-agent" or "content-length")
            {
                continue;
            }

            headers[name] = name == "content-type" ? pair.Value.Split(';')[0] : pair.Value;
        }

        return headers;
    }

    public static string Snapshot(string name)
    {
        return File.ReadAllText(
            Path.Combine(
                AppContext.BaseDirectory,
                "Fixtures",
                "anthropic-do-stream",
                name + ".json"
            )
        );
    }

    /// <summary>Asserts a value against an upstream <c>toMatchInlineSnapshot</c>, which the port keeps as JSON in <c>Fixtures/anthropic-do-stream</c>.</summary>
    public static void AssertSnapshot(JsonNode? actual, string name)
    {
        var expected = JsonNode.Parse(Snapshot(name));
        Assert.True(
            JsonNode.DeepEquals(expected, actual),
            "Snapshot "
                + name
                + ": expected "
                + (expected?.ToJsonString() ?? "null")
                + " but got "
                + (actual?.ToJsonString() ?? "null")
        );
    }

    /// <summary>Asserts a stream against an upstream <c>toMatchInlineSnapshot</c> of the parts, part by part.</summary>
    public static void AssertStreamSnapshot(
        IReadOnlyList<LanguageModelStreamPart> parts,
        string name,
        bool trailingErrorNotReported = false
    )
    {
        var expected = JsonNode.Parse(Snapshot(name))!.AsArray();

        // The stream part has a plain bool where upstream also tells an explicit false from undefined.
        foreach (var part in expected.OfType<JsonObject>())
        {
            if (
                part["isError"] is JsonValue { } isError
                && isError.TryGetValue<bool>(out var flag)
                && !flag
            )
            {
                part.Remove("isError");
            }
        }

        // Upstream shares one provider, and so one mocked id counter, across its tests, so the ids in its snapshots
        // continue from the earlier tests. Each test here has its own provider, so the ids are renumbered from 0.
        int? firstId = null;
        foreach (var part in expected.OfType<JsonObject>())
        {
            if (
                part["id"] is JsonValue idValue
                && idValue.TryGetValue<string>(out var id)
                && id.StartsWith("id-", StringComparison.Ordinal)
                && int.TryParse(id.AsSpan(3), out var number)
            )
            {
                firstId ??= number;
                part["id"] =
                    "id-" + (number - firstId.Value).ToString(CultureInfo.InvariantCulture);
            }
        }

        if (trailingErrorNotReported && expected[^1]?["type"]?.GetValue<string>() == "error")
        {
            expected.RemoveAt(expected.Count - 1);
        }

        var actual = new JsonArray();
        foreach (var part in parts)
        {
            actual.Add(SnapshotOf(part));
        }

        for (var index = 0; index < Math.Min(expected.Count, actual.Count); index++)
        {
            Assert.True(
                JsonNode.DeepEquals(expected[index], actual[index]),
                "Part "
                    + index
                    + ": expected "
                    + expected[index]!.ToJsonString()
                    + " but got "
                    + actual[index]!.ToJsonString()
            );
        }

        Assert.Equal(expected.Count, actual.Count);
    }

    /// <summary>The upstream shape of a stream part. Absent values are left out, like upstream's <c>undefined</c>.</summary>
    public static JsonNode SnapshotOf(LanguageModelStreamPart part)
    {
        var node = new JsonObject { ["type"] = part.Type };
        switch (part)
        {
            case StreamStartStreamPart start:
                node["warnings"] = new JsonArray(
                    start
                        .Warnings.Select(warning =>
                            (JsonNode?)
                                new JsonObject
                                {
                                    ["type"] = warning.Type,
                                    ["message"] = warning.Message,
                                }
                        )
                        .ToArray()
                );
                break;
            case ResponseMetadataStreamPart metadata:
                Set(node, "id", metadata.Id);
                Set(node, "modelId", metadata.ModelId);
                if (metadata.Timestamp is { } timestamp)
                {
                    node["timestamp"] = timestamp.UtcDateTime.ToString(
                        "yyyy-MM-dd'T'HH:mm:ss.fff'Z'",
                        CultureInfo.InvariantCulture
                    );
                }

                break;
            case TextStartStreamPart textStart:
                node["id"] = textStart.Id;
                Metadata(node, textStart.ProviderMetadata);
                break;
            case TextDeltaStreamPart textDelta:
                node["id"] = textDelta.Id;
                node["delta"] = textDelta.Delta;
                Metadata(node, textDelta.ProviderMetadata);
                break;
            case TextEndStreamPart textEnd:
                node["id"] = textEnd.Id;
                if (textEnd.Citations is { Count: > 0 } citations)
                {
                    node["citations"] = new JsonArray(
                        citations.Select(citation => (JsonNode?)CitationOf(citation)).ToArray()
                    );
                }

                Metadata(node, textEnd.ProviderMetadata);
                break;
            case ReasoningStartStreamPart reasoningStart:
                node["id"] = reasoningStart.Id;
                Metadata(node, reasoningStart.ProviderMetadata);
                break;
            case ReasoningDeltaStreamPart reasoningDelta:
                node["id"] = reasoningDelta.Id;
                node["delta"] = reasoningDelta.Delta;
                Metadata(node, reasoningDelta.ProviderMetadata);
                break;
            case ReasoningEndStreamPart reasoningEnd:
                node["id"] = reasoningEnd.Id;
                Metadata(node, reasoningEnd.ProviderMetadata);
                break;
            case ToolInputStartStreamPart inputStart:
                node["id"] = inputStart.Id;
                node["toolName"] = inputStart.ToolName;
                if (inputStart.ProviderExecuted is { } inputProviderExecuted)
                {
                    node["providerExecuted"] = inputProviderExecuted;
                }

                if (inputStart.Dynamic is { } inputDynamic)
                {
                    node["dynamic"] = inputDynamic;
                }

                Set(node, "title", inputStart.Title);
                Metadata(node, inputStart.ProviderMetadata);
                break;
            case ToolInputDeltaStreamPart inputDelta:
                node["id"] = inputDelta.Id;
                node["delta"] = inputDelta.Delta;
                Metadata(node, inputDelta.ProviderMetadata);
                break;
            case ToolInputEndStreamPart inputEnd:
                node["id"] = inputEnd.Id;
                Metadata(node, inputEnd.ProviderMetadata);
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

                Metadata(node, call.ProviderMetadata);
                break;
            case ToolResultStreamPart result:
                node["result"] = JsonNode.Parse(result.Result.GetRawText());
                node["toolCallId"] = result.ToolCallId;
                node["toolName"] = result.ToolName;
                if (result.Dynamic is { } resultDynamic)
                {
                    node["dynamic"] = resultDynamic;
                }

                if (result.IsError)
                {
                    node["isError"] = true;
                }

                Metadata(node, result.ProviderMetadata);
                break;
            case SourceStreamPart source:
                node["sourceType"] = "url";
                node["id"] = source.Id;
                node["url"] = source.Url;
                Set(node, "title", source.Title);
                Metadata(node, source.ProviderMetadata);
                break;
            case DocumentSourceStreamPart document:
                node["sourceType"] = "document";
                node["id"] = document.Id;
                node["mediaType"] = document.MediaType;
                node["title"] = document.Title;
                Set(node, "filename", document.Filename);
                Metadata(node, document.ProviderMetadata);
                break;
            case CustomStreamPart custom:
                node["kind"] = custom.Kind;
                Metadata(node, custom.ProviderMetadata);
                break;
            case RawStreamPart raw:
                node["rawValue"] = JsonNode.Parse(raw.RawJson);
                break;
            case ErrorStreamPart error:
                if (error.Error is { } providerError)
                {
                    var errorNode = new JsonObject
                    {
                        ["data"] = (providerError.Data as JsonNode)?.DeepClone(),
                        ["message"] = providerError.Message,
                    };
                    Set(errorNode, "type", providerError.Type);
                    Set(errorNode, "statusCode", providerError.StatusCode);
                    if (providerError.IsRetryable is { } retryable)
                    {
                        errorNode["isRetryable"] = retryable;
                    }

                    node["error"] = errorNode;
                }
                else
                {
                    node["error"] = error.Message;
                }

                break;
            case FinishStreamPart finish:
                node["finishReason"] = new JsonObject
                {
                    ["raw"] = finish.RawFinishReason,
                    ["unified"] = UnifiedFinishReason(finish.FinishReason),
                };
                if (finish.RawFinishReason is null)
                {
                    ((JsonObject)node["finishReason"]!).Remove("raw");
                }

                node["usage"] = UsageOf(finish.Usage);
                Metadata(node, finish.ProviderMetadata);
                break;
            default:
                throw new InvalidOperationException("Unexpected stream part " + part.Type);
        }

        return node;
    }

    private static JsonObject CitationOf(Citation citation)
    {
        var source = new JsonObject { ["type"] = "source" };
        switch (citation.Source)
        {
            case GeneratedSource url:
                source["sourceType"] = "url";
                source["id"] = url.Id;
                source["url"] = url.Url;
                Set(source, "title", url.Title);
                Metadata(source, url.ProviderMetadata);
                break;
            case GeneratedDocumentSource document:
                source["sourceType"] = "document";
                source["id"] = document.Id;
                source["mediaType"] = document.MediaType;
                source["title"] = document.Title;
                Set(source, "filename", document.Filename);
                Metadata(source, document.ProviderMetadata);
                break;
        }

        var node = new JsonObject { ["source"] = source };
        Set(node, "citedText", citation.CitedText);
        return node;
    }

    private static string UnifiedFinishReason(FinishReason reason)
    {
        return reason switch
        {
            FinishReason.ToolCalls => "tool-calls",
            FinishReason.ContentFilter => "content-filter",
            _ => reason.ToString().ToLowerInvariant(),
        };
    }

    private static JsonObject UsageOf(LanguageModelUsage usage)
    {
        var input = new JsonObject();
        Set(input, "cacheRead", usage.CacheReadTokens);
        Set(input, "cacheWrite", usage.CacheWriteTokens);
        Set(input, "noCache", usage.NoCacheInputTokens);
        Set(input, "total", usage.InputTokens);
        var output = new JsonObject();
        Set(output, "reasoning", usage.ReasoningTokens);
        Set(output, "text", usage.TextTokens);
        Set(output, "total", usage.OutputTokens);
        var node = new JsonObject { ["inputTokens"] = input, ["outputTokens"] = output };
        if (usage.Raw is { } raw)
        {
            node["raw"] = JsonNode.Parse(raw.GetRawText());
        }

        return node;
    }

    private static void Set(JsonObject node, string name, string? value)
    {
        if (value is not null)
        {
            node[name] = value;
        }
    }

    private static void Set(JsonObject node, string name, int? value)
    {
        if (value is { } number)
        {
            node[name] = number;
        }
    }

    private static void Metadata(JsonObject node, JsonElement? metadata)
    {
        if (metadata is { } value)
        {
            node["providerMetadata"] = JsonNode.Parse(value.GetRawText());
        }
    }
}
