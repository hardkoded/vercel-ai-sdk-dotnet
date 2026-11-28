// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Groq;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.GroqChatLanguageModelTests;

/// <summary>Helpers shared by the <c>groq-chat-language-model.test.ts</c> ports.</summary>
internal static class GroqChatTestSupport
{
    internal const string TestFile = "packages/groq/src/groq-chat-language-model.test.ts";

    internal static GroqProvider Provider(
        HttpMessageHandler handler,
        Action<OpenAICompatibleOptions>? configure = null
    )
    {
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-key" };
        configure?.Invoke(options);
        return GroqProvider.Create(options, handler);
    }

    internal static ILanguageModel Model(
        HttpMessageHandler handler,
        string modelId = "gemma2-9b-it"
    )
    {
        return Provider(handler).LanguageModel(modelId);
    }

    /// <summary>Upstream's <c>TEST_PROMPT</c>.</summary>
    internal static LanguageModelCallOptions TestPrompt()
    {
        return UpstreamChat.Prompt();
    }

    internal static string Fixture(string name)
    {
        return File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name + ".json"));
    }

    /// <summary>A capture whose JSON response is the upstream fixture. Maps to <c>prepareJsonFixtureResponse</c>.</summary>
    internal static UpstreamCapture JsonFixture(
        string name,
        string? headerName = null,
        string? headerValue = null
    )
    {
        var capture = new UpstreamCapture { ResponseBody = Fixture(name) };
        if (headerName != null)
        {
            capture.ResponseHeaders[headerName] = headerValue!;
        }

        return capture;
    }

    /// <summary>A stream handler for the upstream chunks fixture. Maps to <c>prepareChunksFixtureResponse</c>.</summary>
    internal static SseStreamHandler ChunksFixture(
        string name,
        IReadOnlyDictionary<string, string>? headers = null
    )
    {
        return new SseStreamHandler(SseStreamHandler.FixtureChunks(name, done: true), headers);
    }

    internal static JsonObject Json(string json)
    {
        return JsonNode.Parse(json)!.AsObject();
    }

    internal static void AssertJson(string expected, JsonNode? actual)
    {
        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(expected), actual),
            $"Expected {expected} but was {actual?.ToJsonString()}"
        );
    }

    internal static JsonElement Schema(string json)
    {
        return UpstreamChat.Json(json);
    }

    /// <summary>The value schema upstream's tests reuse for <c>test-tool</c> and the response format.</summary>
    internal const string ValueSchema =
        """{"type":"object","properties":{"value":{"type":"string"}},"required":["value"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}""";

    internal static void AssertUsage(
        LanguageModelUsage usage,
        int? total,
        int? noCache,
        int? cacheRead,
        int? output,
        int? text,
        int? reasoning,
        string? raw
    )
    {
        Assert.Equal(total, usage.InputTokens);
        Assert.Equal(noCache, usage.NoCacheInputTokens);
        Assert.Equal(cacheRead, usage.CacheReadTokens);
        Assert.Null(usage.CacheWriteTokens);
        Assert.Equal(output, usage.OutputTokens);
        Assert.Equal(text, usage.TextTokens);
        Assert.Equal(reasoning, usage.ReasoningTokens);
        if (raw == null)
        {
            Assert.Null(usage.Raw);
        }
        else
        {
            AssertJson(raw, JsonNode.Parse(usage.Raw!.Value.GetRawText()));
        }
    }

    internal static async Task<List<LanguageModelStreamPart>> Stream(
        ILanguageModel model,
        LanguageModelCallOptions? options = null
    )
    {
        var result = await model.DoStreamAsync(options ?? TestPrompt(), CancellationToken.None);
        return await UpstreamChat.Read(result.Stream);
    }
}
