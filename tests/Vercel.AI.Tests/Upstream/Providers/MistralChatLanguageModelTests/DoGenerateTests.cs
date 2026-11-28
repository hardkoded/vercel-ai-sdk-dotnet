// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.MistralChatLanguageModelTests.MistralChatTestSupport;

namespace Vercel.AI.Tests.MistralChatLanguageModelTests;

/// <summary>Port of <c>mistral-chat-language-model.test.ts</c> &gt; <c>doGenerate</c>.</summary>
public sealed class DoGenerateTests
{
    private const string Prefix =
        "packages/mistral/src/mistral-chat-language-model.test.ts::doGenerate::";

    private const string RawUsage =
        "{\"prompt_tokens\":20,\"completion_tokens\":2,\"total_tokens\":22,\"prompt_audio_seconds\":1,\"request_count\":1,\"service_tier\":\"standard\",\"num_cached_tokens\":0,\"prompt_tokens_details\":{\"cached_tokens\":0,\"audio_tokens\":1,\"messages\":[{\"role\":\"user\",\"total_tokens\":20,\"settings_tokens\":null,\"truncated\":false,\"usage_count\":1}],\"additional_prompt_detail\":{\"value\":true}},\"prompt_token_details\":{\"cached_tokens\":0,\"audio_tokens\":1,\"additional_prompt_token_detail\":[\"value\"]},\"completion_tokens_details\":{\"reasoning_tokens\":7,\"additional_completion_detail\":\"value\"},\"additional_usage_field\":{\"nested\":true}}";

    [Fact]
    [UpstreamTest(Prefix + "should pass tools and toolChoice", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_tools_and_toolChoice()
    {
        var capture = JsonFixture("mistral-text");
        var schema =
            "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false,\"$schema\":\"http://json-schema.org/draft-07/schema#\"}";
        var options = TestPrompt();
        options.Tools = new[] { Tool("test-tool", schema) };
        options.ToolChoice = ToolChoice.Tool("test-tool");

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        var expected = JsonNode.Parse(
            "{\"model\":\"mistral-small-latest\",\"messages\":[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Hello\"}]}],"
                + "\"tools\":[{\"type\":\"function\",\"function\":{\"name\":\"test-tool\",\"parameters\":"
                + schema
                + "}}],\"tool_choice\":{\"type\":\"function\",\"function\":{\"name\":\"test-tool\"}}}"
        );
        Assert.True(JsonNode.DeepEquals(Body(capture), expected), Body(capture).ToJsonString());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should forward stopSequences as the Mistral stop parameter and not warn",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_forward_stopSequences_as_the_Mistral_stop_parameter_and_not_warn()
    {
        var capture = JsonFixture("mistral-text");
        var options = TestPrompt();
        options.StopSequences = new[] { "foo", "bar" };

        var result = await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        var expected = JsonNode.Parse(
            "{\"model\":\"mistral-small-latest\",\"messages\":[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Hello\"}]}],\"stop\":[\"foo\",\"bar\"]}"
        );
        Assert.True(JsonNode.DeepEquals(Body(capture), expected), Body(capture).ToJsonString());
        Assert.DoesNotContain(result.Warnings, warning =>
            warning.Type == "unsupported" && warning.Feature == "stopSequences"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should forward presencePenalty and frequencyPenalty without unsupported warnings",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_forward_presencePenalty_and_frequencyPenalty_without_unsupported_warnings()
    {
        var capture = JsonFixture("mistral-text");
        var options = TestPrompt();
        options.PresencePenalty = 0.1;
        options.FrequencyPenalty = 0.2;

        var result = await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        Assert.DoesNotContain(result.Warnings, warning =>
            warning.Type == "unsupported" && warning.Feature == "presencePenalty"
        );
        Assert.DoesNotContain(result.Warnings, warning =>
            warning.Type == "unsupported" && warning.Feature == "frequencyPenalty"
        );
        Assert.Equal(0.1, Body(capture)["presence_penalty"]!.GetValue<double>());
        Assert.Equal(0.2, Body(capture)["frequency_penalty"]!.GetValue<double>());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should expose the raw response headers",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_expose_the_raw_response_headers()
    {
        var capture = JsonFixture("mistral-text");
        capture.ResponseHeaders["test-header"] = "test-value";

        var result = await Model(capture).DoGenerateAsync(TestPrompt(), CancellationToken.None);

        Assert.StartsWith("application/json", result.ResponseHeaders["content-type"]);
        Assert.Equal("test-value", result.ResponseHeaders["test-header"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should extract usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_usage()
    {
        var capture = JsonFixture("mistral-text");

        var result = await Model(capture).DoGenerateAsync(TestPrompt(), CancellationToken.None);

        var usage = result.Usage;
        Assert.Equal(13, usage.InputTokens);
        Assert.Equal(13, usage.NoCacheInputTokens);
        Assert.Null(usage.CacheReadTokens);
        Assert.Null(usage.CacheWriteTokens);
        Assert.Equal(434, usage.OutputTokens);
        Assert.Equal(434, usage.TextTokens);
        Assert.Null(usage.ReasoningTokens);
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(usage.Raw!.Value.GetRawText()),
                JsonNode.Parse(
                    "{\"completion_tokens\":434,\"prompt_tokens\":13,\"total_tokens\":447}"
                )
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve the complete raw usage object without changing normalized usage",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_the_complete_raw_usage_object_without_changing_normalized_usage()
    {
        var capture = JsonFixture("mistral-usage-details");

        var result = await Model(capture).DoGenerateAsync(TestPrompt(), CancellationToken.None);

        var usage = result.Usage;
        Assert.Equal(20, usage.InputTokens);
        Assert.Equal(20, usage.NoCacheInputTokens);
        Assert.Null(usage.CacheReadTokens);
        Assert.Null(usage.CacheWriteTokens);
        Assert.Equal(2, usage.OutputTokens);
        Assert.Equal(2, usage.TextTokens);
        Assert.Null(usage.ReasoningTokens);
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(usage.Raw!.Value.GetRawText()),
                JsonNode.Parse(RawUsage)
            ),
            usage.Raw!.Value.GetRawText()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send additional response information",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_additional_response_information()
    {
        var capture = JsonFixture("mistral-text");

        var result = await Model(capture).DoGenerateAsync(TestPrompt(), CancellationToken.None);

        Assert.Equal("5319bd0299614c679a0068a4f2c8ffd0", result.ResponseId);
        Assert.Equal("mistral-small-latest", result.ResponseModelId);
        Assert.Equal(
            new DateTimeOffset(2026, 1, 22, 13, 32, 0, TimeSpan.Zero),
            result.ResponseTimestamp
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should send request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_request_body()
    {
        var capture = JsonFixture("mistral-text");

        var result = await Model(capture).DoGenerateAsync(TestPrompt(), CancellationToken.None);

        var expected = JsonNode.Parse(
            "{\"messages\":[{\"content\":[{\"text\":\"Hello\",\"type\":\"text\"}],\"role\":\"user\"}],\"model\":\"mistral-small-latest\"}"
        );
        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(result.Request!.Body!.Value.GetRawText()), expected),
            result.Request!.Body!.Value.GetRawText()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should inject JSON instruction for JSON response format",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_inject_JSON_instruction_for_JSON_response_format()
    {
        var capture = JsonFixture("mistral-text");
        var options = TestPrompt();
        options.JsonResponse = true;

        var result = await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        var expected = JsonNode.Parse(
            "{\"messages\":[{\"content\":\"You MUST answer with JSON.\",\"role\":\"system\"},{\"content\":[{\"text\":\"Hello\",\"type\":\"text\"}],\"role\":\"user\"}],\"model\":\"mistral-small-latest\",\"response_format\":{\"type\":\"json_object\"}}"
        );
        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(result.Request!.Body!.Value.GetRawText()), expected),
            result.Request!.Body!.Value.GetRawText()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should inject JSON instruction for JSON response format with schema",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_inject_JSON_instruction_for_JSON_response_format_with_schema()
    {
        var capture = JsonFixture("mistral-text");
        var options = TestPrompt();
        options.JsonResponse = true;
        options.JsonSchema = UpstreamChat.Json(
            "{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}}}"
        );

        var result = await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        var expected = JsonNode.Parse(
            "{\"messages\":[{\"content\":[{\"text\":\"Hello\",\"type\":\"text\"}],\"role\":\"user\"}],\"model\":\"mistral-small-latest\","
                + "\"response_format\":{\"json_schema\":{\"name\":\"response\",\"schema\":{\"properties\":{\"name\":{\"type\":\"string\"}},\"type\":\"object\"},\"strict\":false},\"type\":\"json_schema\"}}"
        );
        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(result.Request!.Body!.Value.GetRawText()), expected),
            result.Request!.Body!.Value.GetRawText()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should inject JSON schema when structured outputs are disabled",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_inject_JSON_schema_when_structured_outputs_are_disabled()
    {
        var capture = JsonFixture("mistral-text");
        const string schema =
            "{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}}}";
        var options = TestPrompt();
        options.JsonResponse = true;
        options.JsonSchema = UpstreamChat.Json(schema);
        options.ProviderOptions = UpstreamChat.Bag("mistral", "{\"structuredOutputs\":false}");

        var result = await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        var body = JsonNode.Parse(result.Request!.Body!.Value.GetRawText())!;
        var system = body["messages"]![0]!;
        Assert.Equal("system", system["role"]!.GetValue<string>());
        Assert.Equal(
            "JSON schema:\n"
                + schema
                + "\nYou MUST answer with a JSON object that matches the JSON schema above.",
            system["content"]!.GetValue<string>()
        );
        Assert.Equal("user", body["messages"]![1]!["role"]!.GetValue<string>());
        Assert.Equal("json_object", body["response_format"]!["type"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass parallelToolCalls option",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_parallelToolCalls_option()
    {
        var capture = JsonFixture("mistral-text");
        var schema =
            "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false,\"$schema\":\"http://json-schema.org/draft-07/schema#\"}";
        var options = TestPrompt();
        options.Tools = new[] { Tool("test-tool", schema) };
        options.ProviderOptions = UpstreamChat.Bag("mistral", "{\"parallelToolCalls\":false}");

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        var body = Body(capture);
        Assert.Equal("mistral-small-latest", body["model"]!.GetValue<string>());
        Assert.Equal("test-tool", body["tools"]![0]!["function"]!["name"]!.GetValue<string>());
        Assert.True(
            JsonNode.DeepEquals(
                body["tools"]![0]!["function"]!["parameters"],
                JsonNode.Parse(schema)
            )
        );
        Assert.False(body["parallel_tool_calls"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass promptCacheKey as prompt_cache_key",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_promptCacheKey_as_prompt_cache_key()
    {
        var capture = JsonFixture("mistral-text");
        var options = TestPrompt();
        options.ProviderOptions = UpstreamChat.Bag(
            "mistral",
            "{\"promptCacheKey\":\"classification-workflow-123\"}"
        );

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal(
            "classification-workflow-123",
            Body(capture)["prompt_cache_key"]!.GetValue<string>()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should avoid duplication when trailing assistant message",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_avoid_duplication_when_trailing_assistant_message()
    {
        var capture = new UpstreamCapture
        {
            ResponseBody =
                "{\"object\":\"chat.completion\",\"id\":\"16362f24e60340d0994dd205c267a43a\",\"created\":1711113008,\"model\":\"mistral-small-latest\",\"choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":\"prefix and more content\",\"tool_calls\":null},\"finish_reason\":\"stop\",\"logprobs\":null}],\"usage\":{\"prompt_tokens\":4,\"total_tokens\":34,\"completion_tokens\":30}}",
        };
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage("Hello"),
                new AssistantModelMessage("prefix ", null, null),
            },
        };

        var result = await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("prefix and more content", text.Text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve ordering of mixed thinking and text",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_ordering_of_mixed_thinking_and_text()
    {
        var capture = new UpstreamCapture
        {
            ResponseBody =
                "{\"id\":\"mixed-content-test\",\"object\":\"chat.completion\",\"created\":1722349660,\"model\":\"magistral-medium-2507\",\"choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":[{\"type\":\"thinking\",\"thinking\":[{\"type\":\"text\",\"text\":\"First thought.\"}]},{\"type\":\"text\",\"text\":\"Partial answer.\"},{\"type\":\"thinking\",\"thinking\":[{\"type\":\"text\",\"text\":\"Second thought.\"}]},{\"type\":\"text\",\"text\":\"Final answer.\"}]},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":10,\"total_tokens\":30,\"completion_tokens\":20}}",
        };

        var result = await Model(capture, "magistral-medium-2507")
            .DoGenerateAsync(TestPrompt(), CancellationToken.None);

        Assert.Equal(4, result.Content.Count);
        Assert.Equal("First thought.", Assert.IsType<GeneratedReasoning>(result.Content[0]).Text);
        Assert.Equal("Partial answer.", Assert.IsType<GeneratedText>(result.Content[1]).Text);
        Assert.Equal("Second thought.", Assert.IsType<GeneratedReasoning>(result.Content[2]).Text);
        Assert.Equal("Final answer.", Assert.IsType<GeneratedText>(result.Content[3]).Text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle empty thinking content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_empty_thinking_content()
    {
        var capture = new UpstreamCapture
        {
            ResponseBody =
                "{\"id\":\"mixed-content-test\",\"object\":\"chat.completion\",\"created\":1722349660,\"model\":\"magistral-medium-2507\",\"choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":[{\"type\":\"thinking\",\"thinking\":[]},{\"type\":\"text\",\"text\":\"Just the answer.\"}]},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":10,\"total_tokens\":30,\"completion_tokens\":20}}",
        };

        var result = await Model(capture, "magistral-medium-2507")
            .DoGenerateAsync(TestPrompt(), CancellationToken.None);

        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("Just the answer.", text.Text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should extract content when message content is a content object",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_content_when_message_content_is_a_content_object()
    {
        var capture = new UpstreamCapture
        {
            ResponseBody =
                "{\"id\":\"mixed-content-test\",\"object\":\"chat.completion\",\"created\":1722349660,\"model\":\"mistral-small-latest\",\"choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"Hello from object\"}]},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":10,\"total_tokens\":30,\"completion_tokens\":20}}",
        };

        var result = await Model(capture, "mistral-small-latest")
            .DoGenerateAsync(TestPrompt(), CancellationToken.None);

        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("Hello from object", text.Text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return raw text with think tags",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_return_raw_text_with_think_tags()
    {
        var capture = new UpstreamCapture
        {
            ResponseBody =
                "{\"id\":\"mixed-content-test\",\"object\":\"chat.completion\",\"created\":1722349660,\"model\":\"magistral-small-2506\",\"choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":\"<think>\\nLet me think about this problem step by step.\\nFirst, I need to understand what the user is asking.\\nThen I can provide a helpful response.\\n</think>\\n\\nHello! I'm ready to help you with your question.\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":10,\"total_tokens\":30,\"completion_tokens\":20}}",
        };

        var result = await Model(capture, "magistral-small-2506")
            .DoGenerateAsync(TestPrompt(), CancellationToken.None);

        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal(
            "<think>\nLet me think about this problem step by step.\nFirst, I need to understand what the user is asking.\nThen I can provide a helpful response.\n</think>\n\nHello! I'm ready to help you with your question.",
            text.Text
        );
    }
}
