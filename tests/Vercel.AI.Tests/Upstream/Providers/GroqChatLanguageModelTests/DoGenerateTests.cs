// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Groq;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.GroqChatLanguageModelTests.GroqChatTestSupport;

namespace Vercel.AI.Tests.GroqChatLanguageModelTests;

/// <summary>Port of <c>groq-chat-language-model.test.ts</c> &gt; <c>doGenerate</c>.</summary>
public sealed class DoGenerateTests
{
    private const string Prefix = TestFile + "::doGenerate::";

    private const string Minimal =
        """{"id":"chatcmpl-test","object":"chat.completion","created":1711115037,"model":"gemma2-9b-it","choices":[{"index":0,"message":{"role":"assistant","content":""},"finish_reason":"stop"}],"usage":{"prompt_tokens":20,"total_tokens":20}}""";

    private static string WithUsage(string usage, string finishReason = "stop")
    {
        var body = Json(Minimal);
        body["choices"]![0]!["finish_reason"] = finishReason;
        body["usage"] = JsonNode.Parse(usage);
        return body.ToJsonString();
    }

    private static LanguageModelCallOptions JsonFormat(
        string schema = ValueSchema,
        string? name = "test-name",
        string? description = "test description"
    )
    {
        var options = TestPrompt();
        options.JsonResponse = true;
        options.JsonSchema = UpstreamChat.Json(schema);
        options.JsonSchemaName = name;
        options.JsonSchemaDescription = description;
        return options;
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject a response without choices",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reject_a_response_without_choices()
    {
        var capture = new UpstreamCapture
        {
            ResponseBody =
                """{"id":"chatcmpl-empty","object":"chat.completion","created":1711115037,"model":"gemma2-9b-it","choices":[],"usage":{"prompt_tokens":4,"total_tokens":4,"completion_tokens":0}}""",
        };

        var error = await Assert.ThrowsAsync<InvalidResponseDataException>(() =>
            Model(capture).DoGenerateAsync(TestPrompt(), CancellationToken.None)
        );

        Assert.Equal("Response did not contain any choices.", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should extract usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_usage()
    {
        var result = await Model(JsonFixture("groq-text"))
            .DoGenerateAsync(TestPrompt(), CancellationToken.None);

        AssertUsage(
            result.Usage,
            45,
            45,
            null,
            607,
            607,
            null,
            """{"prompt_tokens":45,"completion_tokens":607,"total_tokens":652}"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send additional response information",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_additional_response_information()
    {
        var result = await Model(JsonFixture("groq-text"))
            .DoGenerateAsync(TestPrompt(), CancellationToken.None);

        Assert.Equal("chatcmpl-09d64d2a-ed1c-4473-829f-78db43f45d13", result.ResponseId);
        Assert.Equal("llama-3.3-70b-versatile", result.ResponseModelId);
        Assert.Equal(
            new DateTimeOffset(2026, 2, 11, 0, 46, 38, TimeSpan.Zero),
            result.ResponseTimestamp
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should support partial usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_support_partial_usage()
    {
        var result = await Model(new UpstreamCapture { ResponseBody = Minimal })
            .DoGenerateAsync(TestPrompt(), CancellationToken.None);

        AssertUsage(
            result.Usage,
            20,
            20,
            null,
            0,
            0,
            null,
            """{"prompt_tokens":20,"total_tokens":20}"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should extract cached input tokens",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_cached_input_tokens()
    {
        var usage =
            """{"prompt_tokens":20,"total_tokens":25,"completion_tokens":5,"prompt_tokens_details":{"cached_tokens":15}}""";
        var result = await Model(new UpstreamCapture { ResponseBody = WithUsage(usage) })
            .DoGenerateAsync(TestPrompt(), CancellationToken.None);

        AssertUsage(result.Usage, 20, 5, 15, 5, 5, null, usage);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should extract reasoning tokens from completion_tokens_details",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_reasoning_tokens_from_completion_tokens_details()
    {
        var result = await Model(JsonFixture("groq-reasoning"))
            .DoGenerateAsync(TestPrompt(), CancellationToken.None);

        AssertUsage(
            result.Usage,
            17,
            17,
            null,
            649,
            79,
            570,
            """{"prompt_tokens":17,"completion_tokens":649,"total_tokens":666,"completion_tokens_details":{"reasoning_tokens":570}}"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should support unknown finish reason",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_support_unknown_finish_reason()
    {
        var body = WithUsage(
            """{"prompt_tokens":4,"total_tokens":34,"completion_tokens":30}""",
            "eos"
        );

        var result = await Model(new UpstreamCapture { ResponseBody = body })
            .DoGenerateAsync(TestPrompt(), CancellationToken.None);

        Assert.Equal(FinishReason.Other, result.FinishReason);
        Assert.Equal("eos", result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should expose the raw response headers",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_expose_the_raw_response_headers()
    {
        var capture = JsonFixture("groq-text", "test-header", "test-value");

        var result = await Model(capture).DoGenerateAsync(TestPrompt(), CancellationToken.None);

        Assert.Equal("test-value", result.ResponseHeaders["test-header"]);
        Assert.StartsWith("application/json", result.ResponseHeaders["content-type"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass provider options", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_provider_options()
    {
        var capture = JsonFixture("groq-text");
        var options = TestPrompt();
        options.ProviderOptions = UpstreamChat.Bag(
            "groq",
            """{"reasoningFormat":"hidden","user":"test-user-id","parallelToolCalls":false}"""
        );

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        AssertJson(
            """{"messages":[{"content":"Hello","role":"user"}],"model":"gemma2-9b-it","parallel_tool_calls":false,"reasoning_format":"hidden","user":"test-user-id"}""",
            UpstreamChat.Body(capture)
        );
    }

    [Theory]
    [InlineData("flex", "should pass serviceTier provider option")]
    [InlineData("performance", "should pass performance serviceTier provider option")]
    [UpstreamTest(
        Prefix + "should pass serviceTier provider option",
        Coverage = UpstreamCoverage.Covered
    )]
    [UpstreamTest(
        Prefix + "should pass performance serviceTier provider option",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_serviceTier_provider_option(string tier, string title)
    {
        Assert.NotEmpty(title);
        var capture = JsonFixture("groq-text");
        var options = TestPrompt();
        options.ProviderOptions = UpstreamChat.Bag("groq", $$"""{"serviceTier":"{{tier}}"}""");

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        AssertJson(
            $$"""{"messages":[{"content":"Hello","role":"user"}],"model":"gemma2-9b-it","service_tier":"{{tier}}"}""",
            UpstreamChat.Body(capture)
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass tools and toolChoice", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_tools_and_toolChoice()
    {
        var capture = JsonFixture("groq-text");
        var options = TestPrompt();
        options.Tools = [new LanguageModelTool("test-tool", null, Schema(ValueSchema))];
        options.ToolChoice = ToolChoice.Tool("test-tool");

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        AssertJson(
            $$$"""
            {"messages":[{"content":"Hello","role":"user"}],"model":"gemma2-9b-it",
             "tool_choice":{"function":{"name":"test-tool"},"type":"function"},
             "tools":[{"function":{"name":"test-tool","parameters":{{{ValueSchema}}}},"type":"function"}]}
            """,
            UpstreamChat.Body(capture)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use json_schema format when structuredOutputs explicitly enabled",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_json_schema_format_when_structuredOutputs_explicitly_enabled()
    {
        var capture = JsonFixture("groq-text");
        var options = JsonFormat();
        options.ProviderOptions = UpstreamChat.Bag("groq", """{"structuredOutputs":true}""");

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        AssertJson(
            $$$"""
            {"messages":[{"content":"Hello","role":"user"}],"model":"gemma2-9b-it",
             "response_format":{"json_schema":{"description":"test description","name":"test-name","schema":{{{ValueSchema}}},"strict":true},"type":"json_schema"}}
            """,
            UpstreamChat.Body(capture)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should allow explicit structuredOutputs override",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_allow_explicit_structuredOutputs_override()
    {
        var capture = JsonFixture("groq-text");
        var options = JsonFormat(name: null, description: null);
        options.ProviderOptions = UpstreamChat.Bag("groq", """{"structuredOutputs":true}""");

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        AssertJson(
            $$$"""
            {"messages":[{"content":"Hello","role":"user"}],"model":"gemma2-9b-it",
             "response_format":{"json_schema":{"name":"response","schema":{{{ValueSchema}}},"strict":true},"type":"json_schema"}}
            """,
            UpstreamChat.Body(capture)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send strict: false when strictJsonSchema is explicitly disabled",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_strict_false_when_strictJsonSchema_is_explicitly_disabled()
    {
        var capture = JsonFixture("groq-text");
        var options = JsonFormat();
        options.ProviderOptions = UpstreamChat.Bag("groq", """{"strictJsonSchema":false}""");

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        AssertJson(
            $$$"""
            {"messages":[{"content":"Hello","role":"user"}],"model":"gemma2-9b-it",
             "response_format":{"json_schema":{"description":"test description","name":"test-name","schema":{{{ValueSchema}}},"strict":false},"type":"json_schema"}}
            """,
            UpstreamChat.Body(capture)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle structured outputs with Kimi K2 model",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_structured_outputs_with_Kimi_K2_model()
    {
        const string recipeSchema =
            """{"type":"object","properties":{"recipe":{"type":"object","properties":{"name":{"type":"string"},"ingredients":{"type":"array","items":{"type":"string"}},"instructions":{"type":"array","items":{"type":"string"}}},"required":["name","ingredients","instructions"]}},"required":["recipe"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}""";
        var capture = JsonFixture("groq-text");
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Generate a simple pasta recipe") },
            JsonResponse = true,
            JsonSchema = UpstreamChat.Json(recipeSchema),
            JsonSchemaName = "recipe_response",
            JsonSchemaDescription = "A recipe with ingredients and instructions",
            ProviderOptions = UpstreamChat.Bag("groq", """{"structuredOutputs":true}"""),
        };

        await Model(capture, "moonshotai/kimi-k2-instruct-0905")
            .DoGenerateAsync(options, CancellationToken.None);

        AssertJson(
            $$$"""
            {"messages":[{"content":"Generate a simple pasta recipe","role":"user"}],"model":"moonshotai/kimi-k2-instruct-0905",
             "response_format":{"json_schema":{"description":"A recipe with ingredients and instructions","name":"recipe_response","schema":{{{recipeSchema}}},"strict":true},"type":"json_schema"}}
            """,
            UpstreamChat.Body(capture)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should include warnings when structured outputs explicitly disabled but schema provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_warnings_when_structured_outputs_explicitly_disabled_but_schema_provided()
    {
        var options = JsonFormat(name: null, description: null);
        options.ProviderOptions = UpstreamChat.Bag("groq", """{"structuredOutputs":false}""");

        var result = await Model(JsonFixture("groq-text"))
            .DoGenerateAsync(options, CancellationToken.None);

        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("responseFormat", warning.Feature);
        Assert.Equal(
            "JSON response format schema is only supported with structuredOutputs",
            warning.Message
        );
    }
}
