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

    public static OpenAICompatibleLanguageModel Model(UpstreamCapture capture, string modelId = "qwen-plus")
    {
        return (OpenAICompatibleLanguageModel)AlibabaProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, capture).LanguageModel(modelId);
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
