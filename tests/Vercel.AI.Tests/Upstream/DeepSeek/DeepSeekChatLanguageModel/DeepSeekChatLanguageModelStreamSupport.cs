// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.DeepSeek;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using DeepSeekChatLanguageModelType = Vercel.AI.DeepSeek.DeepSeekChatLanguageModel;
using XaiSupport = Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel;

/// <summary>Fixtures and helpers shared by the <c>doStream</c> ports of <c>deepseek-chat-language-model.test.ts</c>.</summary>
internal static class DeepSeekChatLanguageModelStreamSupport
{
    internal const string TestFile =
        "packages/deepseek/src/chat/deepseek-chat-language-model.test.ts::DeepSeekChatLanguageModel";

    /// <summary>The upstream <c>createDeepSeek({ apiKey: 'test-api-key' })</c>.</summary>
    internal static DeepSeekProvider Provider(HttpMessageHandler handler)
    {
        return DeepSeekProvider.Create(
            new OpenAICompatibleOptions { ApiKey = "test-api-key" },
            handler
        );
    }

    /// <summary>The upstream <c>betaProvider</c>, with <c>baseURL: 'https://api.deepseek.com/beta'</c>.</summary>
    internal static DeepSeekProvider BetaProvider(HttpMessageHandler handler)
    {
        return DeepSeekProvider.Create(
            new OpenAICompatibleOptions
            {
                ApiKey = "test-api-key",
                BaseUrl = "https://api.deepseek.com/beta",
            },
            handler
        );
    }

    /// <summary>The upstream <c>provider.chat(modelId)</c>.</summary>
    internal static DeepSeekChatLanguageModelType Chat(
        HttpMessageHandler handler,
        string modelId = "deepseek-chat"
    )
    {
        return Provider(handler).ChatModel(modelId);
    }

    /// <summary>The <c>TEST_PROMPT</c> of the upstream file.</summary>
    internal static LanguageModelCallOptions TestPrompt()
    {
        return UpstreamChat.Prompt("Hello");
    }

    /// <summary>A stream of server-sent events. Each argument is one raw chunk, like the upstream <c>stream-chunks</c> response.</summary>
    internal static SseStreamHandler Chunks(params string[] chunks)
    {
        return new SseStreamHandler(chunks);
    }

    /// <summary>The upstream <c>prepareChunksFixtureResponse</c>: replays a recorded <c>.chunks.txt</c> fixture and ends with <c>[DONE]</c>.</summary>
    internal static SseStreamHandler ChunksFixture(string name)
    {
        return new SseStreamHandler(SseStreamHandler.FixtureChunks(name, done: true));
    }

    /// <summary>Runs <c>doStream</c> and reads every part, like <c>convertReadableStreamToArray(stream)</c>.</summary>
    internal static async Task<List<LanguageModelStreamPart>> Stream(
        DeepSeekChatLanguageModelType model,
        LanguageModelCallOptions options
    )
    {
        var result = await model.DoStreamAsync(options, CancellationToken.None);
        return await UpstreamChat.Read(result.Stream);
    }

    /// <summary>The JSON of the first request body, like <c>server.calls[0].requestBodyJson</c>.</summary>
    internal static JsonObject RequestBody(SseStreamHandler handler)
    {
        return JsonNode.Parse(handler.Body)!.AsObject();
    }

    /// <summary>A recorded stream snapshot stored next to the fixtures. The upstream snapshot, with unset fields left out.</summary>
    internal static JsonNode Snapshot(string fileName)
    {
        return JsonNode.Parse(
            File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName))
        )!;
    }

    /// <summary>The upstream <c>toMatchSnapshot()</c> of a stream.</summary>
    internal static void AssertStreamSnapshot(
        IEnumerable<LanguageModelStreamPart> parts,
        string fileName
    )
    {
        var actual = XaiSupport.ToJson(parts);
        Assert.True(JsonNode.DeepEquals(Snapshot(fileName), actual), actual.ToJsonString());
    }

    /// <summary>The finish part of a stream, like <c>parts.find(part =&gt; part.type === 'finish')</c>.</summary>
    internal static FinishStreamPart Finish(IEnumerable<LanguageModelStreamPart> parts)
    {
        return parts.OfType<FinishStreamPart>().First();
    }

    /// <summary>
    /// A call warning in the upstream shape: <c>setting</c> and <c>message</c> for a deprecated setting,
    /// <c>feature</c> and <c>details</c> for the other warnings.
    /// </summary>
    internal static JsonObject WarningJson(CallWarning warning)
    {
        var json = new JsonObject { ["type"] = warning.Type };
        if (warning.Setting != null)
        {
            json["setting"] = warning.Setting;
            json["message"] = warning.Message;
        }
        else
        {
            json["feature"] = warning.Feature;
            json["details"] = warning.Message;
        }

        return json;
    }

    /// <summary>The first part as <c>{ type: 'stream-start', warnings }</c>, for <c>toStrictEqual</c>.</summary>
    internal static JsonObject StreamStartJson(IReadOnlyList<LanguageModelStreamPart> parts)
    {
        var start = Assert.IsType<StreamStartStreamPart>(parts[0]);
        var warnings = new JsonArray();
        foreach (var warning in start.Warnings)
        {
            warnings.Add(WarningJson(warning));
        }

        return new JsonObject { ["type"] = start.Type, ["warnings"] = warnings };
    }

    /// <summary>Checks a JSON value against an expected JSON text.</summary>
    internal static void AssertJson(JsonNode? actual, string expected)
    {
        JsonAssert.Equal(actual, expected);
    }
}
