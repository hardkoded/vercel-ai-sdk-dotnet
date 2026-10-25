// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; doGenerate &gt; phase</c>.</summary>
public sealed class DoGeneratePhaseTests
{
    private const string Id = "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > phase::";

    private sealed record Case(string Name, string[] Messages, string[] ExpectedTexts, string? StructuredOutputText);

    // The messages are "phase:text|text" entries. An empty phase means the message has none.
    private static readonly Case[] Cases =
    {
        new(
            "JSON commentary before a final answer",
            new[] { "commentary:{\"items\":[]}", "final_answer:{\"items\":[\"result\"]}" },
            new[] { "{\"items\":[]}", "{\"items\":[\"result\"]}" },
            "{\"items\":[\"result\"]}"),
        new(
            "non-JSON commentary before a final answer",
            new[] { "commentary:Let me think.", "final_answer:{\"items\":[\"result\"]}" },
            new[] { "Let me think.", "{\"items\":[\"result\"]}" },
            "{\"items\":[\"result\"]}"),
        new(
            "identical final answers",
            new[] { "final_answer:{\"items\":[\"result\"]}", "final_answer:{\"items\":[\"result\"]}" },
            new[] { "{\"items\":[\"result\"]}", "{\"items\":[\"result\"]}" },
            "{\"items\":[\"result\"]}"),
        new(
            "different final answers followed by commentary",
            new[] { "final_answer:{\"items\":[]}", "final_answer:{\"items\":[\"result\"]}", "commentary:Finished." },
            new[] { "{\"items\":[]}", "{\"items\":[\"result\"]}", "Finished." },
            "{\"items\":[\"result\"]}"),
        new(
            "a final answer split across text parts",
            new[] { "commentary:{\"items\":[]}", "final_answer:{\"items\":|[\"result\"]}" },
            new[] { "{\"items\":[]}", "{\"items\":", "[\"result\"]}" },
            "{\"items\":[\"result\"]}"),
        new(
            "a malformed final answer without falling back to a draft",
            new[] { "final_answer:{\"items\":[]}", "final_answer:{\"items\":" },
            new[] { "{\"items\":[]}", "{\"items\":" },
            "{\"items\":"),
        new(
            "an empty final answer without falling back to a draft",
            new[] { "final_answer:{\"items\":[]}", "final_answer:" },
            new[] { "{\"items\":[]}", string.Empty },
            string.Empty),
        new(
            "a final answer without text parts",
            new[] { "final_answer:{\"items\":[]}", "final_answer" },
            new[] { "{\"items\":[]}" },
            string.Empty),
        new(
            "messages without phases",
            new[] { ":{\"items\":", ":[\"result\"]}" },
            new[] { "{\"items\":", "[\"result\"]}" },
            null),
        new(
            "commentary without an explicit final answer",
            new[] { "commentary:{\"items\":[]}" },
            new[] { "{\"items\":[]}" },
            null),
    };

    public static TheoryData<string> CaseNames => new(Cases.Select(testCase => testCase.Name));

    private static JsonObject Message(string id, string? phase, params string[] texts)
    {
        return new JsonObject
        {
            ["type"] = "message",
            ["id"] = id,
            ["role"] = "assistant",
            ["status"] = "completed",
            ["phase"] = phase,
            ["content"] = new JsonArray(texts.Select(text => (JsonNode)new JsonObject
            {
                ["type"] = "output_text",
                ["text"] = text,
                ["annotations"] = new JsonArray(),
            }).ToArray()),
        };
    }

    // Splits a "phase:text|text" entry. Without a colon the message has a final-answer phase and no text parts.
    private static JsonObject ParseMessage(string entry, int index)
    {
        var colon = entry.IndexOf(':');
        if (colon < 0)
        {
            return Message("m" + index, entry);
        }

        var phase = colon == 0 ? null : entry.Substring(0, colon);
        var texts = entry.Substring(colon + 1).Split('|');
        return Message("m" + index, phase, texts);
    }

    private static async Task<LanguageModelGenerateResult> Generate(JsonNode body)
    {
        var capture = new OpenAICapture { ResponseJson = body.ToJsonString() };
        var options = OpenAIUpstream.Hello();
        options.JsonResponse = true;
        return await OpenAIUpstream.Provider(capture).ResponsesModel("gpt-5.3-codex").DoGenerateAsync(options, CancellationToken.None);
    }

    [Fact]
    [UpstreamTest(Id + "should include phase in provider metadata for message output items", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_phase_in_provider_metadata_for_message_output_items()
    {
        var capture = new OpenAICapture { ResponseJson = OpenAIResponsesSupport.Fixture("openai-phase.1.json") };

        var result = await OpenAIUpstream.Provider(capture).ResponsesModel("gpt-5.3-codex").DoGenerateAsync(OpenAIUpstream.Hello(), CancellationToken.None);

        var textParts = result.Content.OfType<GeneratedText>().ToList();
        Assert.Equal(2, textParts.Count);
        Assert.Null(result.StructuredOutputText);
        var first = ((OpenAIText)textParts[0]).ProviderMetadata.GetProperty("openai");
        Assert.Equal("msg_0465b6d1ae1f97c500699f883243a481a3b50b985223592984", first.GetProperty("itemId").GetString());
        Assert.Equal("commentary", first.GetProperty("phase").GetString());
        var second = ((OpenAIText)textParts[1]).ProviderMetadata.GetProperty("openai");
        Assert.Equal("msg_0465b6d1ae1f97c500699f8835e09c81a3b91e9d502ff18555", second.GetProperty("itemId").GetString());
        Assert.Equal("final_answer", second.GetProperty("phase").GetString());
    }

    [Theory]
    [MemberData(nameof(CaseNames))]
    [UpstreamTest(Id + "should preserve all text and select structured output for $name", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_all_text_and_select_structured_output_for_name(string name)
    {
        var testCase = Cases.Single(candidate => candidate.Name == name);
        var output = new JsonArray(testCase.Messages.Select((entry, index) => (JsonNode)ParseMessage(entry, index)).ToArray());

        var result = await Generate(new JsonObject
        {
            ["id"] = "resp_json",
            ["model"] = "gpt-5.3-codex",
            ["output"] = output,
            ["usage"] = new JsonObject { ["input_tokens"] = 13, ["output_tokens"] = 7 },
        });

        Assert.All(result.Content, part => Assert.IsAssignableFrom<GeneratedText>(part));
        Assert.Equal(testCase.ExpectedTexts, result.Content.OfType<GeneratedText>().Select(part => part.Text).ToArray());
        Assert.Equal(testCase.StructuredOutputText, result.StructuredOutputText);
    }

    [Fact]
    [UpstreamTest(
        Id + "should preserve all content and response details while selecting structured output for %s",
        Coverage = UpstreamCoverage.Partial,
        Note = "Covers the openai provider. The port reports provider metadata under openai for Azure too, and does not report logprobs in the response provider metadata.")]
    public void Should_preserve_all_content_and_response_details_while_selecting_structured_output_for_openai()
    {
        var annotations = new JsonArray(new JsonObject
        {
            ["type"] = "url_citation",
            ["start_index"] = 0,
            ["end_index"] = 5,
            ["url"] = "https://example.com/draft",
            ["title"] = "Draft source",
        });
        var body = new JsonObject
        {
            ["id"] = "resp_json",
            ["model"] = "gpt-4o",
            ["output"] = new JsonArray(
                new JsonObject
                {
                    ["type"] = "message",
                    ["id"] = "draft",
                    ["role"] = "assistant",
                    ["status"] = "completed",
                    ["phase"] = "commentary",
                    ["content"] = new JsonArray(new JsonObject
                    {
                        ["type"] = "output_text",
                        ["text"] = "Draft.",
                        ["annotations"] = annotations.DeepClone(),
                    }),
                },
                new JsonObject
                {
                    ["type"] = "reasoning",
                    ["id"] = "reasoning",
                    ["encrypted_content"] = "encrypted",
                    ["summary"] = new JsonArray(new JsonObject { ["type"] = "summary_text", ["text"] = "Thinking." }),
                },
                new JsonObject
                {
                    ["type"] = "function_call",
                    ["id"] = "function",
                    ["call_id"] = "call",
                    ["name"] = "weather",
                    ["arguments"] = "{\"location\":\"Paris\"}",
                },
                new JsonObject
                {
                    ["type"] = "message",
                    ["id"] = "final",
                    ["role"] = "assistant",
                    ["status"] = "completed",
                    ["phase"] = "final_answer",
                    ["content"] = new JsonArray(
                        new JsonObject { ["type"] = "output_text", ["text"] = "{\"items\":", ["annotations"] = new JsonArray() },
                        new JsonObject { ["type"] = "output_text", ["text"] = "[\"result\"]}", ["annotations"] = new JsonArray() }),
                }),
            ["usage"] = new JsonObject
            {
                ["input_tokens"] = 13,
                ["input_tokens_details"] = new JsonObject { ["cached_tokens"] = 3 },
                ["output_tokens"] = 7,
                ["output_tokens_details"] = new JsonObject { ["reasoning_tokens"] = 2 },
            },
        };
        var raw = body.ToJsonString();
        using var document = JsonDocument.Parse(raw);
        var next = 0;

        var result = OpenAIResponsesLanguageModel.Parse(
            document.RootElement,
            Array.Empty<OpenAICallWarning>(),
            raw,
            new Dictionary<string, string>(),
            () => "id-" + next++,
            OpenAIParallelToolCall.FunctionToolNames(OpenAIResponsesSupport.TestTools),
            jsonResponse: true);

        Assert.Collection(
            result.Content,
            part =>
            {
                var text = Assert.IsAssignableFrom<GeneratedText>(part);
                Assert.Equal("Draft.", text.Text);
                var citation = Assert.Single(text.Citations!);
                Assert.Equal(0, citation.StartIndex);
                Assert.Equal(5, citation.EndIndex);
                Assert.Equal("https://example.com/draft", Assert.IsType<GeneratedSource>(citation.Source).Url);
                JsonAssert.Equal(
                    ((OpenAIText)text).ProviderMetadata,
                    new JsonObject { ["openai"] = new JsonObject { ["itemId"] = "draft", ["phase"] = "commentary", ["annotations"] = annotations.DeepClone() } }.ToJsonString());
            },
            part =>
            {
                var source = Assert.IsType<GeneratedSource>(part);
                Assert.Equal("id-0", source.Id);
                Assert.Equal("https://example.com/draft", source.Url);
                Assert.Equal("Draft source", source.Title);
            },
            part =>
            {
                var reasoning = Assert.IsType<GeneratedReasoning>(part);
                Assert.Equal("Thinking.", reasoning.Text);
                JsonAssert.Equal(reasoning.ProviderMetadata!.Value, """{"openai":{"itemId":"reasoning","reasoningEncryptedContent":"encrypted"}}""");
            },
            part =>
            {
                var call = Assert.IsType<GeneratedToolCall>(part);
                Assert.Equal("call", call.ToolCallId);
                Assert.Equal("weather", call.ToolName);
                Assert.Equal("{\"location\":\"Paris\"}", call.ArgumentsJson);
                JsonAssert.Equal(call.ProviderMetadata!.Value, """{"openai":{"itemId":"function"}}""");
            },
            part => AssertFinalText(part, "{\"items\":"),
            part => AssertFinalText(part, "[\"result\"]}"));
        Assert.Equal("{\"items\":[\"result\"]}", result.StructuredOutputText);
        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
        Assert.Null(result.RawFinishReason);
        Assert.Equal(13, result.Usage.InputTokens);
        Assert.Equal(10, result.Usage.NoCacheInputTokens);
        Assert.Equal(3, result.Usage.CacheReadTokens);
        Assert.Equal(7, result.Usage.OutputTokens);
        Assert.Equal(5, result.Usage.TextTokens);
        Assert.Equal(2, result.Usage.ReasoningTokens);
        Assert.Equal(body.ToJsonString(), result.RawResponse);
    }

    private static void AssertFinalText(GeneratedContent part, string expected)
    {
        var text = Assert.IsAssignableFrom<GeneratedText>(part);
        Assert.Equal(expected, text.Text);
        JsonAssert.Equal(((OpenAIText)text).ProviderMetadata, """{"openai":{"itemId":"final","phase":"final_answer"}}""");
    }
}
