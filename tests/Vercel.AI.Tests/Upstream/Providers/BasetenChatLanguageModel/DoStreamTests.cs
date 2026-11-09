// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.BasetenChatLanguageModel;

/// <summary>Port of <c>baseten-chat-language-model.test.ts</c> &gt; <c>doStream</c>.</summary>
public sealed class DoStreamTests
{
    private const bool Stream = true;
    private const string P = "packages/baseten/src/baseten-chat-language-model.test.ts::%s::";

    [Fact]
    [UpstreamTest(P + "should use the schema in the prompt without JSON mode when reasoning is enabled", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_the_schema_in_the_prompt_without_JSON_mode_when_reasoning_is_enabled()
    {
        var (body, warnings) = await BasetenChatCalls.Call(Stream, BasetenChatCalls.Options());

        Assert.Null(body["response_format"]);
        Assert.Equal("high", body["reasoning_effort"]!.GetValue<string>());
        var first = body["messages"]![0]!;
        Assert.Equal("system", first["role"]!.GetValue<string>());
        Assert.Contains(BasetenChatCalls.Schema, first["content"]!.GetValue<string>(), StringComparison.Ordinal);
        var rest = body["messages"]!.AsArray().Skip(1).ToArray();
        Assert.Equal(2, rest.Length);
        Assert.Equal("system", rest[0]!["role"]!.GetValue<string>());
        Assert.Equal("You are a helpful assistant.", rest[0]!["content"]!.GetValue<string>());
        Assert.Equal("user", rest[1]!["role"]!.GetValue<string>());
        Assert.Equal("Reply with PONG.", rest[1]!["content"]!.GetValue<string>());
        Assert.Contains(warnings, w => w.Type == "compatibility" && w.Feature == "responseFormat" && w.Message.Contains("injected into the system message", StringComparison.Ordinal));
    }

    [Fact]
    [UpstreamTest(P + "should omit JSON mode without a schema when reasoning uses its default", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_omit_JSON_mode_without_a_schema_when_reasoning_uses_its_default()
    {
        var (body, _) = await BasetenChatCalls.Call(Stream, BasetenChatCalls.Options(schema: false, reasoning: null));

        Assert.Null(body["response_format"]);
        Assert.Null(body["reasoning_effort"]);
        var first = body["messages"]![0]!;
        Assert.Equal("system", first["role"]!.GetValue<string>());
        Assert.Equal("You MUST answer with JSON.", first["content"]!.GetValue<string>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [UpstreamTest(P + "should retain JSON Schema when reasoning is disabled with %j", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_retain_JSON_Schema_when_reasoning_is_disabled(int variant)
    {
        var options = BasetenChatCalls.Options(reasoning: variant == 0 ? "none" : null);
        if (variant == 1)
        {
            options.ProviderOptions = UpstreamChat.Bag("baseten", "{\"reasoningEffort\":\"none\"}");
        }
        else if (variant == 2)
        {
            options.ProviderOptions = UpstreamChat.Bag("baseten", "{\"reasoning_effort\":\"none\"}");
        }

        var (body, warnings) = await BasetenChatCalls.Call(Stream, options);

        Assert.Equal("json_schema", body["response_format"]!["type"]!.GetValue<string>());
        Assert.Equal("none", body["reasoning_effort"]!.GetValue<string>());
        Assert.Equal(2, body["messages"]!.AsArray().Count);
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest(P + "should leave text requests unchanged", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_leave_text_requests_unchanged()
    {
        var (body, warnings) = await BasetenChatCalls.Call(Stream, BasetenChatCalls.Options(json: false));

        Assert.Null(body["response_format"]);
        Assert.Equal(2, body["messages"]!.AsArray().Count);
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest(P + "should retain JSON Schema for other models", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_retain_JSON_Schema_for_other_models()
    {
        var (body, warnings) = await BasetenChatCalls.Call(Stream, BasetenChatCalls.Options(), id: "zai-org/GLM-4.7");

        Assert.Equal("json_schema", body["response_format"]!["type"]!.GetValue<string>());
        Assert.Equal(2, body["messages"]!.AsArray().Count);
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest(P + "should leave dedicated deployments unchanged", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_leave_dedicated_deployments_unchanged()
    {
        var (body, warnings) = await BasetenChatCalls.Call(Stream, BasetenChatCalls.Options(), dedicated: true);

        Assert.Equal("json_schema", body["response_format"]!["type"]!.GetValue<string>());
        Assert.Equal(2, body["messages"]!.AsArray().Count);
        Assert.Empty(warnings);
    }
}
