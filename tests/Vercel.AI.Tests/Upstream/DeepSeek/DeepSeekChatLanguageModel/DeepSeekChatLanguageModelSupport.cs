// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.DeepSeek;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using ChatModel = Vercel.AI.DeepSeek.DeepSeekChatLanguageModel;
using ResponsesSupport = Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekResponsesLanguageModel.DeepSeekResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel;

/// <summary>Fixtures and helpers shared by the <c>deepseek-chat-language-model.test.ts</c> doGenerate ports.</summary>
internal static class DeepSeekChatLanguageModelSupport
{
    internal const string TestFile =
        "packages/deepseek/src/chat/deepseek-chat-language-model.test.ts::DeepSeekChatLanguageModel";

    /// <summary>Upstream commit that has these tests. The parity catalog is older.</summary>
    internal const string Commit = "3bea041f28e65f371e640d2c70c302776a46387f";

    /// <summary>The upstream <c>weather</c> tool of the tool call tests.</summary>
    internal const string WeatherSchema =
        """{"type":"object","properties":{"location":{"type":"string"}},"required":["location"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}""";

    /// <summary><c>createDeepSeek({ apiKey: 'test-api-key' })</c>, talking to <paramref name="capture"/>.</summary>
    internal static DeepSeekProvider CreateProvider(UpstreamCapture capture)
    {
        return DeepSeekProvider.Create(
            new OpenAICompatibleOptions { ApiKey = "test-api-key" },
            capture
        );
    }

    /// <summary><c>createDeepSeek({ apiKey: 'test-api-key', baseURL: 'https://api.deepseek.com/beta' })</c>.</summary>
    internal static DeepSeekProvider CreateBetaProvider(UpstreamCapture capture)
    {
        return DeepSeekProvider.Create(
            new OpenAICompatibleOptions
            {
                ApiKey = "test-api-key",
                BaseUrl = "https://api.deepseek.com/beta",
            },
            capture
        );
    }

    /// <summary><c>provider.chat(modelId)</c>.</summary>
    internal static ChatModel Model(UpstreamCapture capture, string modelId)
    {
        return CreateProvider(capture).ChatModel(modelId);
    }

    /// <summary><c>betaProvider.chat(modelId)</c>.</summary>
    internal static ChatModel BetaModel(UpstreamCapture capture, string modelId)
    {
        return CreateBetaProvider(capture).ChatModel(modelId);
    }

    /// <summary>The <c>TEST_PROMPT</c> of the upstream file.</summary>
    internal static LanguageModelCallOptions TestPrompt()
    {
        return UpstreamChat.Prompt("Hello");
    }

    /// <summary>The upstream <c>prepareJsonFixtureResponse</c>. The test server sends the minified JSON.</summary>
    internal static UpstreamCapture JsonFixture(string name)
    {
        return ResponsesSupport.JsonFixture(name);
    }

    /// <summary>The parsed JSON of an upstream fixture, as <c>JSON.parse(fs.readFileSync(...))</c>.</summary>
    internal static JsonObject FixtureBody(string name)
    {
        return JsonNode
            .Parse(
                File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name + ".json"))
            )!
            .AsObject();
    }

    /// <summary>A snapshot stored next to the fixtures. The upstream <c>toMatchSnapshot</c>, with unset fields left out.</summary>
    internal static JsonNode Snapshot(string fileName)
    {
        return ResponsesSupport.Snapshot(fileName);
    }

    /// <summary>Asserts the generate result and its request equal the stored upstream snapshot.</summary>
    internal static void MatchSnapshot(
        string fileName,
        LanguageModelGenerateResult result,
        UpstreamCapture capture
    )
    {
        var actual = ResponsesSupport.GenerateSnapshot(result, capture);
        Assert.True(JsonNode.DeepEquals(Snapshot(fileName), actual), actual.ToJsonString());
    }

    /// <summary>The upstream <c>weather</c> function tool.</summary>
    internal static LanguageModelTool WeatherTool()
    {
        return new LanguageModelTool("weather", null, UpstreamChat.Json(WeatherSchema));
    }

    /// <summary><c>providerOptions: { deepseek: ... }</c>.</summary>
    internal static Dictionary<string, JsonElement> DeepSeekOptions(string json)
    {
        return UpstreamChat.Bag("deepseek", json);
    }

    /// <summary>The JSON of the first request, like <c>await server.calls[0].requestBodyJson</c>.</summary>
    internal static JsonObject RequestBody(UpstreamCapture capture)
    {
        return UpstreamChat.Body(capture);
    }

    /// <summary>Like <c>toStrictEqual</c> and <c>toMatchInlineSnapshot</c> on JSON: the same keys and values.</summary>
    internal static void AssertJson(JsonNode? actual, string expected)
    {
        JsonAssert.Equal(actual, expected);
    }

    /// <summary>Like <c>toMatchObject</c>: every expected property is in <paramref name="actual"/>. Arrays match element by element.</summary>
    internal static void AssertMatchObject(JsonNode? actual, string expected)
    {
        var expectedNode = JsonNode.Parse(expected);
        Assert.True(
            IsSubset(expectedNode, actual),
            (actual?.ToJsonString() ?? "null") + " does not match " + expected
        );
    }

    private static bool IsSubset(JsonNode? expected, JsonNode? actual)
    {
        switch (expected)
        {
            case JsonObject expectedObject:
                if (actual is not JsonObject actualObject)
                {
                    return false;
                }

                foreach (var property in expectedObject)
                {
                    if (
                        !actualObject.TryGetPropertyValue(property.Key, out var value)
                        || !IsSubset(property.Value, value)
                    )
                    {
                        return false;
                    }
                }

                return true;
            case JsonArray expectedArray:
                if (actual is not JsonArray actualArray || actualArray.Count != expectedArray.Count)
                {
                    return false;
                }

                for (var index = 0; index < expectedArray.Count; index++)
                {
                    if (!IsSubset(expectedArray[index], actualArray[index]))
                    {
                        return false;
                    }
                }

                return true;
            default:
                return JsonNode.DeepEquals(expected, actual);
        }
    }

    /// <summary>Like <c>toStrictEqual</c> on the warnings: the same warnings in the same order.</summary>
    internal static void AssertWarnings(
        LanguageModelGenerateResult result,
        params (string Type, string? Feature, string Message)[] expected
    )
    {
        Assert.Equal(
            expected,
            result
                .Warnings.Select(warning => (warning.Type, warning.Feature, warning.Message))
                .ToArray()
        );
    }

    /// <summary>Checks the <c>setting</c> of each <c>deprecated</c> warning, in order.</summary>
    internal static void AssertDeprecatedSettings(
        LanguageModelGenerateResult result,
        params string[] expected
    )
    {
        Assert.Equal(
            expected,
            result
                .Warnings.Where(warning => warning.Type == "deprecated")
                .Select(warning => warning.Setting)
                .ToArray()
        );
    }

    /// <summary>Like <c>toContainEqual</c> on the warnings.</summary>
    internal static void AssertContainsWarning(
        LanguageModelGenerateResult result,
        string type,
        string feature,
        string details
    )
    {
        Assert.Contains(result.Warnings, warning =>
            warning.Type == type && warning.Feature == feature && warning.Message == details
        );
    }

    /// <summary>The <c>deepseek</c> provider metadata of a result, as <c>result.providerMetadata?.deepseek</c>.</summary>
    internal static JsonObject DeepSeekMetadata(LanguageModelGenerateResult result)
    {
        return JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())!["deepseek"]!.AsObject();
    }
}
