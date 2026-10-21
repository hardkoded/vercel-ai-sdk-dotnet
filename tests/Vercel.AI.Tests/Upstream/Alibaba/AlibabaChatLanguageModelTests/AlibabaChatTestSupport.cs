// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Alibaba;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Alibaba.AlibabaChatLanguageModelTests;

/// <summary>Shared setup for the ports of <c>alibaba-chat-language-model.test.ts</c>.</summary>
internal static class AlibabaChatTestSupport
{
    public const string ChatCompletionsUrl = "https://dashscope-intl.aliyuncs.com/compatible-mode/v1/chat/completions";

    public static string Fixture(string name)
    {
        return File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name + ".json"));
    }

    public static UpstreamCapture CaptureFixture(string name)
    {
        return new UpstreamCapture { ResponseBody = Fixture(name) };
    }

    public static OpenAICompatibleLanguageModel Model(HttpMessageHandler handler, string modelId = "qwen-plus", Action<OpenAICompatibleOptions>? configure = null)
    {
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-key" };
        configure?.Invoke(options);
        return (OpenAICompatibleLanguageModel)AlibabaProvider.Create(options, handler).LanguageModel(modelId);
    }

    /// <summary>The reasoning id upstream gets from its mocked <c>generateId</c>.</summary>
    public const string ReasoningId = "test-reasoning-id";

    /// <summary>A model whose reasoning block ids are <see cref="ReasoningId"/>, like the upstream test file mocks <c>generateId</c>.</summary>
    public static OpenAICompatibleLanguageModel StreamModel(HttpMessageHandler handler, string modelId = "qwen-plus")
    {
        return Model(handler, modelId, options => options.ReasoningBlockId = () => ReasoningId);
    }

    public static SseStreamHandler FixtureStream(string name)
    {
        return new SseStreamHandler(SseStreamHandler.FixtureChunks(name, done: true));
    }

    public static async Task<List<LanguageModelStreamPart>> ReadStream(OpenAICompatibleLanguageModel model, LanguageModelCallOptions options)
    {
        var result = await model.DoStreamAsync(options, CancellationToken.None);
        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in result.ConfigureAwait(false))
        {
            parts.Add(part);
        }

        return parts;
    }

    /// <summary>Asserts a stream against an upstream <c>toMatchSnapshot()</c> entry, which the port keeps as JSON in <c>Fixtures</c>.</summary>
    public static void AssertStreamSnapshot(IReadOnlyList<LanguageModelStreamPart> parts, string fixture)
    {
        var expected = JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", fixture + ".stream.snap.json")))!.AsArray();
        var actual = new JsonArray();
        foreach (var part in parts)
        {
            actual.Add(SnapshotOf(part));
        }

        Assert.Equal(expected.Count, actual.Count);
        for (var index = 0; index < expected.Count; index++)
        {
            Assert.True(
                JsonNode.DeepEquals(expected[index], actual[index]),
                "Part " + index + ": expected " + expected[index]!.ToJsonString() + " but got " + actual[index]!.ToJsonString());
        }
    }

    public static JsonNode SnapshotOf(LanguageModelStreamPart part)
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
                node["timestamp"] = metadata.Timestamp!.Value.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", System.Globalization.CultureInfo.InvariantCulture);
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
            case ReasoningStartStreamPart reasoningStart:
                node["id"] = reasoningStart.Id;
                break;
            case ReasoningDeltaStreamPart reasoningDelta:
                node["id"] = reasoningDelta.Id;
                node["delta"] = reasoningDelta.Delta;
                break;
            case ReasoningEndStreamPart reasoningEnd:
                node["id"] = reasoningEnd.Id;
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
                break;
            case FinishStreamPart finish:
                node["finishReason"] = new JsonObject { ["raw"] = finish.RawFinishReason, ["unified"] = finish.FinishReason == FinishReason.ToolCalls ? "tool-calls" : finish.FinishReason.ToString().ToLowerInvariant() };
                node["usage"] = new JsonObject
                {
                    ["inputTokens"] = new JsonObject
                    {
                        ["cacheRead"] = finish.Usage.CacheReadTokens,
                        ["cacheWrite"] = finish.Usage.CacheWriteTokens,
                        ["noCache"] = finish.Usage.NoCacheInputTokens,
                        ["total"] = finish.Usage.InputTokens,
                    },
                    ["outputTokens"] = new JsonObject
                    {
                        ["reasoning"] = finish.Usage.ReasoningTokens,
                        ["text"] = finish.Usage.TextTokens,
                        ["total"] = finish.Usage.OutputTokens,
                    },
                    ["raw"] = JsonNode.Parse(finish.Usage.Raw!.Value.GetRawText()),
                };
                break;
            default:
                throw new InvalidOperationException("Unexpected stream part " + part.Type);
        }

        return node;
    }

    public static IReadOnlyDictionary<string, JsonElement> AlibabaOptions(string json)
    {
        return new Dictionary<string, JsonElement> { ["alibaba"] = JsonDocument.Parse(json).RootElement.Clone() };
    }

    public static JsonObject Body(UpstreamCapture capture, int call = 0)
    {
        return JsonNode.Parse(capture.Requests[call].Body)!.AsObject();
    }

    public static void AssertJson(string expected, JsonNode? actual)
    {
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), actual), "Expected " + expected + " but got " + actual?.ToJsonString());
    }

    /// <summary>Asserts the request, response, usage, and warnings parts of an upstream result snapshot.</summary>
    public static void AssertResultSnapshot(
        LanguageModelGenerateResult result,
        UpstreamCapture capture,
        string fixture,
        string rawFinishReason,
        FinishReason finishReason,
        string id,
        DateTimeOffset timestamp,
        int promptTokens,
        int completionTokens,
        int reasoningTokens)
    {
        Assert.Equal(finishReason, result.FinishReason);
        Assert.Equal(rawFinishReason, result.RawFinishReason);
        AssertJson("{\"model\":\"qwen-plus\",\"messages\":[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Hello\"}]}]}", JsonNode.Parse(capture.Requests[0].Body));
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(Fixture(fixture)), JsonNode.Parse(result.RawResponse!)));
        Assert.StartsWith("application/json", result.ResponseHeaders["content-type"]);
        Assert.Equal(id, result.ResponseId);
        Assert.Equal("qwen3-max", result.ResponseModelId);
        Assert.Equal(timestamp, result.ResponseTimestamp);
        Assert.Equal(promptTokens, result.Usage.InputTokens);
        Assert.Equal(0, result.Usage.CacheReadTokens);
        Assert.Equal(0, result.Usage.CacheWriteTokens);
        Assert.Equal(promptTokens, result.Usage.NoCacheInputTokens);
        Assert.Equal(completionTokens, result.Usage.OutputTokens);
        Assert.Equal(reasoningTokens, result.Usage.ReasoningTokens);
        Assert.Equal(completionTokens - reasoningTokens, result.Usage.TextTokens);
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(Fixture(fixture))!["usage"], JsonNode.Parse(result.Usage.Raw!.Value.GetRawText())));
        Assert.Empty(result.Warnings);
    }
}
