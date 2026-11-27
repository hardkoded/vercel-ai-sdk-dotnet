// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; transformRequestBody</c>.</summary>
public sealed class TransformRequestBodyTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > transformRequestBody::";

    private const string TransformJsonResponse =
        """{"id":"msg_test","type":"message","role":"assistant","content":[{"type":"text","text":"Hello!"}],"model":"claude-3-haiku-20240307","stop_reason":"end_turn","stop_sequence":null,"usage":{"input_tokens":4,"output_tokens":30}}""";

    private static AnthropicProvider Provider(
        HttpMessageHandler handler,
        Func<JsonObject, ISet<string>, JsonObject>? transformRequestBody = null
    )
    {
        return AnthropicProvider.Create(
            new AnthropicOptions
            {
                ApiKey = "test-api-key",
                Name = "test-provider",
                TransformRequestBody = transformRequestBody,
            },
            handler
        );
    }

    private static SseStreamHandler PrepareTransformStreamResponse()
    {
        return DoStreamSupport.Stream(
            """{"type":"message_start","message":{"id":"msg_test","type":"message","role":"assistant","content":[],"model":"claude-3-haiku-20240307","stop_reason":null,"stop_sequence":null,"usage":{"input_tokens":4,"output_tokens":1}}}""",
            """{"type":"content_block_start","index":0,"content_block":{"type":"text","text":""}}""",
            """{"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":"Hello!"}}""",
            """{"type":"content_block_stop","index":0}""",
            """{"type":"message_delta","delta":{"stop_reason":"end_turn","stop_sequence":null},"usage":{"output_tokens":2}}""",
            """{"type":"message_stop"}"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should transform request body in doGenerate when transformRequestBody is provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_transform_request_body_in_doGenerate_when_transformRequestBody_is_provided()
    {
        var calls = new List<(JsonObject Body, ISet<string> Betas)>();
        var handler = AnthropicParity.Ok(TransformJsonResponse);

        await AnthropicParity.Generate(Provider(handler, (body, betas) =>
            {
                calls.Add((body.DeepClone().AsObject(), betas));
                body["custom_field"] = "added-by-transform";
                return body;
            }), "claude-3-haiku-20240307");

        // Verify transform was called
        var call = Assert.Single(calls);
        Assert.Equal("claude-3-haiku-20240307", (string?)call.Body["model"]);
        JsonAssert.Equal(
            call.Body["messages"],
            """[{"role":"user","content":[{"type":"text","text":"Hello"}]}]"""
        );
        Assert.IsAssignableFrom<ISet<string>>(call.Betas);

        // Verify transformed body was sent
        Assert.Equal("added-by-transform", (string?)JsonNode.Parse(handler.Body)!["custom_field"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should transform request body in doStream when transformRequestBody is provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_transform_request_body_in_doStream_when_transformRequestBody_is_provided()
    {
        var calls = new List<(JsonObject Body, ISet<string> Betas)>();
        var handler = PrepareTransformStreamResponse();

        // Consume the stream
        await DoStreamSupport.Collect(Provider(handler, (body, betas) =>
            {
                calls.Add((body.DeepClone().AsObject(), betas));
                body["custom_field"] = "added-by-transform";
                return body;
            }), "claude-3-haiku-20240307");

        // Verify transform was called
        var call = Assert.Single(calls);
        Assert.Equal("claude-3-haiku-20240307", (string?)call.Body["model"]);
        JsonAssert.Equal(
            call.Body["messages"],
            """[{"role":"user","content":[{"type":"text","text":"Hello"}]}]"""
        );
        Assert.True((bool?)call.Body["stream"]);
        Assert.IsAssignableFrom<ISet<string>>(call.Betas);

        // Verify transformed body was sent
        Assert.Equal("added-by-transform", (string?)JsonNode.Parse(handler.Body)!["custom_field"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should work without transformRequestBody",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_work_without_transformRequestBody()
    {
        var handler = AnthropicParity.Ok(TransformJsonResponse);

        await AnthropicParity.Generate(Provider(handler), "claude-3-haiku-20240307");

        var requestBody = JsonNode.Parse(handler.Body)!.AsObject();
        Assert.Equal("claude-3-haiku-20240307", (string?)requestBody["model"]);
        Assert.False(requestBody.ContainsKey("custom_field"));
    }
}
