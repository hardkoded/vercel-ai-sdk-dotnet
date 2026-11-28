// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Moonshot;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIChatLanguageModel;

/// <summary>Helpers shared by the <c>doGenerate</c> ports of <c>moonshotai-chat-language-model.test.ts</c>.</summary>
internal static class MoonshotLanguageModelSupportA
{
    public const string Id =
        "packages/moonshotai/src/moonshotai-chat-language-model.test.ts::doGenerate > ";

    /// <summary>Maps <c>TEST_PROMPT</c>.</summary>
    public static LanguageModelCallOptions TestPrompt()
    {
        return new LanguageModelCallOptions { Prompt = [new UserModelMessage("Hello")] };
    }

    /// <summary>Maps <c>readJsonFixture</c>.</summary>
    public static string Fixture(string filename)
    {
        return File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "Fixtures", filename + ".json")
        );
    }

    /// <summary>Maps <c>createMoonshotAI({ apiKey })</c> with <c>prepareJsonFixtureResponse(fixture)</c> and <c>chatModel(modelId)</c>.</summary>
    public static (OpenAICompatibleLanguageModel Model, SequencedHandler Handler) Model(
        string modelId,
        string fixture = "moonshotai-text"
    )
    {
        return Model(modelId, new SequencedHandler().Json(Fixture(fixture)));
    }

    /// <summary>Maps <c>provider.chatModel(modelId)</c> against a scripted server.</summary>
    public static (OpenAICompatibleLanguageModel Model, SequencedHandler Handler) Model(
        string modelId,
        SequencedHandler handler
    )
    {
        var provider = MoonshotProvider.Create(
            new OpenAICompatibleOptions { ApiKey = "test-api-key" },
            handler
        );
        return (provider.CreateChatModel(modelId), handler);
    }

    /// <summary>Maps <c>await server.calls[index].requestBodyJson</c>.</summary>
    public static JsonObject Body(SequencedHandler handler, int index = 0)
    {
        return JsonNode.Parse(handler.Bodies[index])!.AsObject();
    }

    /// <summary>Maps <c>providerOptions</c>.</summary>
    public static Dictionary<string, JsonElement> Opts(string json)
    {
        return GoogleUpstream.ProviderOptions(json);
    }

    /// <summary>Maps a single-message prompt.</summary>
    public static LanguageModelCallOptions Messages(params ModelMessage[] messages)
    {
        return new LanguageModelCallOptions { Prompt = messages };
    }

    /// <summary>Maps a function tool whose input schema is <paramref name="schema"/>.</summary>
    public static LanguageModelTool Tool(string name, string? description, string schema)
    {
        return GoogleUpstream.Function(name, schema, description);
    }

    /// <summary>Maps <c>toStrictEqual</c> on JSON.</summary>
    public static void Strict(JsonNode? actual, string expected)
    {
        JsonAssert.Equal(actual, expected);
    }

    /// <summary>Maps <c>toMatchObject</c> on JSON: every property of <paramref name="expected"/> is in <paramref name="actual"/>.</summary>
    public static void Match(JsonNode? actual, string expected)
    {
        Assert.True(Matches(actual, JsonNode.Parse(expected)), actual?.ToJsonString());
    }

    /// <summary>Maps <c>expect(warnings).toStrictEqual([...])</c> for feature warnings.</summary>
    public static void AssertWarnings(
        IReadOnlyList<CallWarning> warnings,
        params (string Type, string Feature, string? Details)[] expected
    )
    {
        Assert.Equal(expected.Length, warnings.Count);
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i].Type, warnings[i].Type);
            Assert.Equal(expected[i].Feature, warnings[i].Feature);
            Assert.Equal(expected[i].Details ?? expected[i].Feature, warnings[i].Message);
        }
    }

    private static bool Matches(JsonNode? actual, JsonNode? expected)
    {
        if (expected is JsonObject expectedObject)
        {
            if (actual is not JsonObject actualObject)
            {
                return false;
            }

            foreach (var pair in expectedObject)
            {
                if (!actualObject.TryGetPropertyValue(pair.Key, out var value))
                {
                    return false;
                }

                if (!Matches(value, pair.Value))
                {
                    return false;
                }
            }

            return true;
        }

        if (expected is JsonArray expectedArray)
        {
            if (actual is not JsonArray actualArray || actualArray.Count != expectedArray.Count)
            {
                return false;
            }

            for (var i = 0; i < expectedArray.Count; i++)
            {
                if (!Matches(actualArray[i], expectedArray[i]))
                {
                    return false;
                }
            }

            return true;
        }

        return JsonNode.DeepEquals(actual, expected);
    }
}
