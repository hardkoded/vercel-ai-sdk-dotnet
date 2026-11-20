// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; doGenerate &gt; reasoning</c>.</summary>
public sealed class DoGenerateReasoningTests
{
    private const string Prefix =
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > reasoning::";

    private const string Rs = "rs_6808709f6fcc8191ad2e2fdd784017b3";
    private const string Msg = "msg_67c97c02656c81908e080dfdf4a03cd1";
    private const string Encrypted = "encrypted_reasoning_data_abc123";
    private const string First =
        "**Exploring burrito origins**\n\nThe user is curious about the debate regarding Taqueria La Cumbre and El Farolito.";
    private const string Second =
        "**Investigating burrito origins**\n\nThere's a fascinating debate about who created the Mission burrito.";

    [Fact]
    [UpstreamTest(
        Prefix + "should handle reasoning with summary",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_reasoning_with_summary()
    {
        var (result, capture) = await Run(
            Reasoning(Rs, null, First, Second),
            """{"reasoningEffort":"low","reasoningSummary":"auto"}"""
        );

        OpenAIResponsesSupport.AssertContent(
            result.Content,
            Expected(
                ReasoningPart(Rs, null, First),
                ReasoningPart(Rs, null, Second),
                TextPart(Msg, "answer text")
            )
        );
        AssertRequest(
            capture,
            """{"model":"o3-mini","reasoning":{"effort":"low","summary":"auto"}}"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should generate reasoning encrypted content using real fixture",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_generate_reasoning_encrypted_content_using_real_fixture()
    {
        var capture = new OpenAICapture
        {
            ResponseJson = OpenAIResponsesSupport.Fixture(
                "openai-reasoning-encrypted-content.1.json"
            ),
        };
        var options = OpenAIUpstream.Hello();
        options.Tools = new[]
        {
            LanguageModelTool.ProviderDefined(
                "openai.code_interpreter",
                "codeExecution",
                OpenAIUpstream.Json("{}")
            ),
        };

        var result = await OpenAIUpstream
            .Provider(capture)
            .ResponsesModel("gpt-5-mini")
            .DoGenerateAsync(options, CancellationToken.None);

        OpenAIResponsesSupport.AssertContentSnapshot(
            result.Content,
            "openai-reasoning-encrypted-content.1"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle reasoning with empty summary",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_reasoning_with_empty_summary()
    {
        var (result, capture) = await Run(
            Reasoning(Rs, null),
            """{"reasoningEffort":"low","reasoningSummary":null}"""
        );

        OpenAIResponsesSupport.AssertContent(
            result.Content,
            Expected(ReasoningPart(Rs, null, string.Empty), TextPart(Msg, "answer text"))
        );
        AssertRequest(capture, """{"model":"o3-mini","reasoning":{"effort":"low"}}""");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle encrypted content with summary",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_encrypted_content_with_summary()
    {
        var (result, capture) = await Run(
            Reasoning(Rs, Encrypted, First, Second),
            """{"reasoningEffort":"low","reasoningSummary":"auto","include":["reasoning.encrypted_content"]}"""
        );

        OpenAIResponsesSupport.AssertContent(
            result.Content,
            Expected(
                ReasoningPart(Rs, Encrypted, First),
                ReasoningPart(Rs, Encrypted, Second),
                TextPart(Msg, "answer text")
            )
        );
        AssertRequest(
            capture,
            """{"model":"o3-mini","reasoning":{"effort":"low","summary":"auto"},"include":["reasoning.encrypted_content"]}"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle encrypted content with empty summary",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_encrypted_content_with_empty_summary()
    {
        var (result, capture) = await Run(
            Reasoning(Rs, Encrypted),
            """{"reasoningEffort":"low","reasoningSummary":null,"include":["reasoning.encrypted_content"]}"""
        );

        OpenAIResponsesSupport.AssertContent(
            result.Content,
            Expected(ReasoningPart(Rs, Encrypted, string.Empty), TextPart(Msg, "answer text"))
        );
        AssertRequest(
            capture,
            """{"model":"o3-mini","reasoning":{"effort":"low"},"include":["reasoning.encrypted_content"]}"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle multiple reasoning blocks",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_multiple_reasoning_blocks()
    {
        const string rsFirst = "rs_first_6808709f6fcc8191ad2e2fdd784017b3";
        const string rsSecond = "rs_second_7908809g7gcc9291be3e3fee895028c4";
        const string msgFinal = "msg_final_78d08d03767d92908f25523f5ge51e77";
        const string a =
            "**Initial analysis**\n\nFirst reasoning block: analyzing the problem structure.";
        const string b =
            "**Deeper consideration**\n\nLet me think about the various approaches available.";
        const string c = "Second reasoning block: considering alternative approaches.";

        var output = new JsonArray(
            ReasoningItem(rsFirst, null, a, b),
            MessageItem(Msg, "Let me think about this step by step."),
            ReasoningItem(rsSecond, null, c),
            MessageItem(msgFinal, "Based on my analysis, here is the solution.")
        );
        var (result, capture) = await RunOutput(
            output,
            """{"reasoningEffort":"medium","reasoningSummary":"auto"}"""
        );

        OpenAIResponsesSupport.AssertContent(
            result.Content,
            Expected(
                ReasoningPart(rsFirst, null, a),
                ReasoningPart(rsFirst, null, b),
                TextPart(Msg, "Let me think about this step by step."),
                ReasoningPart(rsSecond, null, c),
                TextPart(msgFinal, "Based on my analysis, here is the solution.")
            )
        );
        AssertRequest(
            capture,
            """{"model":"o3-mini","reasoning":{"effort":"medium","summary":"auto"}}"""
        );
    }

    private static JsonNode ReasoningItem(string id, string? encrypted, params string[] summary)
    {
        var item = new JsonObject { ["id"] = id, ["type"] = "reasoning" };
        if (encrypted != null)
        {
            item["encrypted_content"] = encrypted;
        }

        var parts = new JsonArray();
        foreach (var text in summary)
        {
            parts.Add(new JsonObject { ["type"] = "summary_text", ["text"] = text });
        }

        item["summary"] = parts;
        return item;
    }

    private static JsonNode MessageItem(string id, string text) =>
        new JsonObject
        {
            ["id"] = id,
            ["type"] = "message",
            ["status"] = "completed",
            ["role"] = "assistant",
            ["content"] = new JsonArray(
                new JsonObject
                {
                    ["type"] = "output_text",
                    ["text"] = text,
                    ["annotations"] = new JsonArray(),
                }
            ),
        };

    private static JsonNode Reasoning(string id, string? encrypted, params string[] summary) =>
        new JsonArray(ReasoningItem(id, encrypted, summary), MessageItem(Msg, "answer text"));

    private static JsonObject ReasoningPart(string id, string? encrypted, string text) =>
        new()
        {
            ["type"] = "reasoning",
            ["text"] = text,
            ["providerMetadata"] = new JsonObject
            {
                ["openai"] = new JsonObject
                {
                    ["itemId"] = id,
                    ["reasoningEncryptedContent"] = encrypted,
                },
            },
        };

    private static JsonObject TextPart(string id, string text) =>
        new()
        {
            ["type"] = "text",
            ["text"] = text,
            ["providerMetadata"] = new JsonObject
            {
                ["openai"] = new JsonObject { ["itemId"] = id },
            },
        };

    private static JsonArray Expected(params JsonObject[] parts)
    {
        var array = new JsonArray();
        foreach (var part in parts)
        {
            array.Add(part);
        }

        return array;
    }

    private static void AssertRequest(OpenAICapture capture, string expectedSubset) =>
        AssertSubset(JsonNode.Parse(expectedSubset)!, JsonNode.Parse(capture.Body)!, "$");

    // The toMatchObject of the upstream test: every expected field is in the request body.
    private static void AssertSubset(JsonNode expected, JsonNode? actual, string path)
    {
        Assert.NotNull(actual);
        switch (expected)
        {
            case JsonObject expectedObject:
                foreach (var (key, value) in expectedObject)
                {
                    AssertSubset(value!, actual![key], path + "." + key);
                }

                break;
            case JsonArray expectedArray:
                Assert.Equal(expectedArray.Count, actual!.AsArray().Count);
                for (var i = 0; i < expectedArray.Count; i++)
                {
                    AssertSubset(expectedArray[i]!, actual.AsArray()[i], path + "[" + i + "]");
                }

                break;
            default:
                Assert.True(JsonNode.DeepEquals(expected, actual), path + ": " + actual);
                break;
        }
    }

    private static Task<(LanguageModelGenerateResult Result, OpenAICapture Capture)> Run(
        JsonNode output,
        string openaiOptions
    ) => RunOutput(output, openaiOptions);

    private static async Task<(
        LanguageModelGenerateResult Result,
        OpenAICapture Capture
    )> RunOutput(JsonNode output, string openaiOptions)
    {
        var body = new JsonObject
        {
            ["id"] = "resp_67c97c0203188190a025beb4a75242bc",
            ["object"] = "response",
            ["created_at"] = 1741257730,
            ["status"] = "completed",
            ["error"] = null,
            ["incomplete_details"] = null,
            ["input"] = new JsonArray(),
            ["instructions"] = null,
            ["max_output_tokens"] = null,
            ["model"] = "o3-mini-2025-01-31",
            ["output"] = output.DeepClone(),
            ["parallel_tool_calls"] = true,
            ["previous_response_id"] = null,
            ["reasoning"] = new JsonObject { ["effort"] = "low", ["summary"] = "auto" },
            ["store"] = true,
            ["temperature"] = 1,
            ["text"] = new JsonObject { ["format"] = new JsonObject { ["type"] = "text" } },
            ["tool_choice"] = "auto",
            ["tools"] = new JsonArray(),
            ["top_p"] = 1,
            ["truncation"] = "disabled",
            ["usage"] = new JsonObject
            {
                ["input_tokens"] = 34,
                ["input_tokens_details"] = new JsonObject { ["cached_tokens"] = 0 },
                ["output_tokens"] = 538,
                ["output_tokens_details"] = new JsonObject { ["reasoning_tokens"] = 320 },
                ["total_tokens"] = 572,
            },
            ["user"] = null,
            ["metadata"] = new JsonObject(),
        };
        var capture = new OpenAICapture { ResponseJson = body.ToJsonString() };
        var options = OpenAIUpstream.Hello();
        options.ProviderOptions = OpenAIUpstream.OpenAIOptionsJson(openaiOptions);
        var result = await OpenAIUpstream
            .Provider(capture)
            .ResponsesModel("o3-mini")
            .DoGenerateAsync(options, CancellationToken.None);
        return (result, capture);
    }
}
