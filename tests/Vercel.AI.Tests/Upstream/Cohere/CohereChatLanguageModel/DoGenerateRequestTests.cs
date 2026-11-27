// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Cohere.CohereTestSupport;

namespace Vercel.AI.Tests.Upstream.Cohere.CohereChatLanguageModel;

/// <summary>Port of <c>cohere-chat-language-model.test.ts</c> &gt; <c>doGenerate</c> &gt; <c>request</c>.</summary>
public sealed class DoGenerateRequestTests
{
    private const string Chat = "packages/cohere/src/cohere-chat-language-model.test.ts::";
    private const string Suite = Chat + "doGenerate > request::";
    private const string Messages =
        "[{\"content\":\"you are a friendly bot!\",\"role\":\"system\"},{\"content\":\"Hello\",\"role\":\"user\"}]";

    private static string Json(string messages, string extra = "") =>
        "{\"messages\":" + messages + ",\"model\":\"command-r-plus\"" + extra + "}";

    [Fact]
    [UpstreamTest(Suite + "should pass model and messages", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_model_and_messages()
    {
        var handler = JsonFixture("cohere-text");

        await Model(handler).DoGenerateAsync(TestPrompt(), CancellationToken.None);

        JsonAssert.Equal(CohereTestSupport.Json(handler.Calls[0].Text), Json(Messages));
    }

    [Fact]
    [UpstreamTest(Suite + "should pass tools", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_tools()
    {
        var handler = JsonFixture("cohere-text");
        var options = TestPrompt();
        options.ToolChoice = ToolChoice.None;
        options.Tools = new[]
        {
            FunctionTool(
                "test-tool",
                "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false,\"$schema\":\"http://json-schema.org/draft-07/schema#\"}"
            ),
        };

        await Model(handler).DoGenerateAsync(options, CancellationToken.None);

        JsonAssert.Equal(
            CohereTestSupport.Json(handler.Calls[0].Text),
            Json(
                Messages,
                ",\"tool_choice\":\"NONE\",\"tools\":[{\"function\":{\"name\":\"test-tool\",\"parameters\":{\"$schema\":\"http://json-schema.org/draft-07/schema#\",\"additionalProperties\":false,\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"type\":\"object\"}},\"type\":\"function\"}]"
            )
        );
    }

    [Fact]
    [UpstreamTest(Suite + "should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_headers()
    {
        var handler = JsonFixture("cohere-text");
        var options = TestPrompt();
        options.Headers = RequestHeaders();

        await ModelWithHeaders(handler).DoGenerateAsync(options, CancellationToken.None);

        AssertTestHeaders(handler.Calls[0]);
    }

    [Fact]
    [UpstreamTest(Suite + "should pass response format", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_response_format()
    {
        var handler = JsonFixture("cohere-text");
        var options = TestPrompt();
        options.JsonResponse = true;
        options.JsonSchema = CohereTestSupport.Json(
            "{\"type\":\"object\",\"properties\":{\"text\":{\"type\":\"string\"}},\"required\":[\"text\"]}"
        );

        await Model(handler).DoGenerateAsync(options, CancellationToken.None);

        JsonAssert.Equal(
            CohereTestSupport.Json(handler.Calls[0].Text),
            Json(
                Messages,
                ",\"response_format\":{\"json_schema\":{\"properties\":{\"text\":{\"type\":\"string\"}},\"required\":[\"text\"],\"type\":\"object\"},\"type\":\"json_object\"}"
            )
        );
    }

    [Fact]
    [UpstreamTest(Suite + "should send request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_request_body()
    {
        var result = await Model(JsonFixture("cohere-text"))
            .DoGenerateAsync(TestPrompt(), CancellationToken.None);

        JsonAssert.Equal(result.Request!.Body!.Value, Json(Messages));
    }

    [Fact]
    [UpstreamTest(
        Suite + "should expose the raw response headers",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_expose_the_raw_response_headers()
    {
        var result = await Model(JsonFixture("cohere-text", ("test-header", "test-value")))
            .DoGenerateAsync(TestPrompt(), CancellationToken.None);

        Assert.Equal("test-value", result.ResponseHeaders["test-header"]);
        Assert.Equal("application/json", result.ResponseHeaders["content-type"]);
    }

    [Fact]
    [UpstreamTest(Suite + "should extract usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_usage()
    {
        var usage = (
            await Model(JsonFixture("cohere-text"))
                .DoGenerateAsync(TestPrompt(), CancellationToken.None)
        ).Usage;

        Assert.Equal(507, usage.InputTokens);
        Assert.Equal(59, usage.NoCacheInputTokens);
        Assert.Equal(448, usage.CacheReadTokens);
        Assert.Null(usage.CacheWriteTokens);
        Assert.Equal(10, usage.OutputTokens);
        Assert.Equal(10, usage.TextTokens);
        Assert.Null(usage.ReasoningTokens);
        JsonAssert.Equal(
            usage.Raw!.Value,
            "{\"billed_units\":{\"input_tokens\":12,\"output_tokens\":7},\"cached_tokens\":448,\"tokens\":{\"input_tokens\":507,\"output_tokens\":10}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Suite + "should preserve extra top-level and nested fields in raw usage",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_extra_top_level_and_nested_fields_in_raw_usage()
    {
        const string rawUsage =
            "{\"billed_units\":{\"input_tokens\":12,\"output_tokens\":7,\"billing_tier\":\"standard\"},\"tokens\":{\"input_tokens\":507,\"output_tokens\":10,\"tokenizer\":\"command\"},\"cached_tokens\":448,\"provider_usage_id\":\"usage-123\"}";
        var handler = JsonBody(
            "{\"message\":{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"Hello\"}]},\"finish_reason\":\"COMPLETE\",\"usage\":"
                + rawUsage
                + "}"
        );

        var usage = (
            await Model(handler).DoGenerateAsync(TestPrompt(), CancellationToken.None)
        ).Usage;

        Assert.Equal(507, usage.InputTokens);
        Assert.Equal(59, usage.NoCacheInputTokens);
        Assert.Equal(448, usage.CacheReadTokens);
        Assert.Null(usage.CacheWriteTokens);
        Assert.Equal(10, usage.OutputTokens);
        Assert.Equal(10, usage.TextTokens);
        Assert.Null(usage.ReasoningTokens);
        JsonAssert.Equal(usage.Raw!.Value, rawUsage);
    }

    [Fact]
    [UpstreamTest(
        Suite + "should validate cached token usage",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_validate_cached_token_usage()
    {
        var handler = JsonBody(
            "{\"message\":{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"Hello\"}]},\"finish_reason\":\"COMPLETE\",\"usage\":{\"billed_units\":{\"input_tokens\":12,\"output_tokens\":7},\"tokens\":{\"input_tokens\":507,\"output_tokens\":10},\"cached_tokens\":\"448\"}}"
        );

        await Assert.ThrowsAnyAsync<Exception>(() =>
            Model(handler).DoGenerateAsync(TestPrompt(), CancellationToken.None)
        );
    }

    [Fact]
    [UpstreamTest(
        Suite + "should send additional response information",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_additional_response_information()
    {
        var result = await Model(JsonFixture("cohere-text"))
            .DoGenerateAsync(TestPrompt(), CancellationToken.None);

        Assert.Equal("e7592632-1e3d-424f-b129-bd5f9f980f7b", result.ResponseId);
        Assert.Null(result.ResponseModelId);
        Assert.Null(result.ResponseTimestamp);
    }
}
