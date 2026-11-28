// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Moonshot;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIChatLanguageModel;

/// <summary>Helpers shared by the <c>doStream</c>, <c>supportedUrls</c> and <c>errors</c> ports of <c>moonshotai-chat-language-model.test.ts</c>.</summary>
internal static class MoonshotLanguageModelSupportB
{
    internal const string TestFile =
        "packages/moonshotai/src/moonshotai-chat-language-model.test.ts";

    /// <summary>The upstream <c>createMoonshotAI({ apiKey: 'test-api-key' })</c>.</summary>
    internal static MoonshotProvider CreateProvider(HttpMessageHandler handler)
    {
        return MoonshotProvider.Create(
            new OpenAICompatibleOptions { ApiKey = "test-api-key" },
            handler
        );
    }

    /// <summary>The upstream <c>TEST_PROMPT</c>.</summary>
    internal static LanguageModelCallOptions TestPrompt()
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
        };
    }

    /// <summary>Provider options under the <c>moonshotai</c> key.</summary>
    internal static Dictionary<string, JsonElement> Options(string json)
    {
        return UpstreamChat.Bag("moonshotai", json);
    }

    /// <summary>The upstream <c>prepareChunksFixtureResponse</c>.</summary>
    internal static SseStreamHandler ChunksFixture(string name)
    {
        return new SseStreamHandler(SseStreamHandler.FixtureChunks(name, done: true));
    }

    /// <summary>The upstream <c>getStreamParts</c>.</summary>
    internal static async Task<List<LanguageModelStreamPart>> GetStreamParts(string fileName)
    {
        var handler = ChunksFixture(fileName);
        return await Stream(handler, TestPrompt());
    }

    /// <summary>Runs <c>doStream</c> on <c>kimi-k3</c> and reads every part.</summary>
    internal static async Task<List<LanguageModelStreamPart>> Stream(
        HttpMessageHandler handler,
        LanguageModelCallOptions options,
        string modelId = "kimi-k3"
    )
    {
        var result = await CreateProvider(handler)
            .CreateChatModel(modelId)
            .DoStreamAsync(options, CancellationToken.None);
        return await UpstreamChat.Read(result.Stream);
    }

    /// <summary>The JSON of the request body, like <c>server.calls[0].requestBodyJson</c>.</summary>
    internal static JsonObject RequestBody(SseStreamHandler handler)
    {
        return JsonNode.Parse(handler.Body)!.AsObject();
    }

    /// <summary>The finish part of a stream.</summary>
    internal static FinishStreamPart Finish(IEnumerable<LanguageModelStreamPart> parts)
    {
        return parts.OfType<FinishStreamPart>().First();
    }

    /// <summary>The metadata of the finish part, as JSON.</summary>
    internal static JsonNode? FinishMetadata(IEnumerable<LanguageModelStreamPart> parts)
    {
        var metadata = Finish(parts).ProviderMetadata;
        return metadata is null ? null : JsonNode.Parse(metadata.Value.GetRawText());
    }

    internal static void AssertJson(JsonNode? actual, string expected)
    {
        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(expected), actual),
            actual?.ToJsonString() ?? "null"
        );
    }

    /// <summary>The SSE events of a raw chunk list.</summary>
    internal static SseStreamHandler Chunks(params string[] chunks)
    {
        return new SseStreamHandler(chunks);
    }
}
