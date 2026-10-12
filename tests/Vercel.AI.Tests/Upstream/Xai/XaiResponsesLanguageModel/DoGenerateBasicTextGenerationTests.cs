// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel;

/// <summary>Port of <c>xai-responses-language-model.test.ts</c> &gt; <c>XaiResponsesLanguageModel &gt; doGenerate &gt; basic text generation</c>.</summary>
public sealed class DoGenerateBasicTextGenerationTests
{
    private const string Prefix = TestFile + " > doGenerate > basic text generation::";

    [Fact]
    [UpstreamTest(Prefix + "should generate text content", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_generate_text_content()
    {
        var capture = JsonResponse("""
            {"id":"resp_123","object":"response","created_at":1700000000,"status":"completed","model":"grok-4-fast-non-reasoning",
             "output":[{"type":"message","id":"msg_123","status":"completed","role":"assistant","content":[{"type":"output_text","text":"hello world","annotations":[]}]}],
             "usage":{"input_tokens":10,"output_tokens":5,"total_tokens":15}}
            """);

        var result = await Model(capture).DoGenerateAsync(HelloPrompt(), CancellationToken.None);

        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("hello world", text.Text);
    }

    [Fact]
    [UpstreamTest(Prefix + "should extract usage correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_usage_correctly()
    {
        var capture = JsonResponse("""
            {"id":"resp_123","object":"response","status":"completed","model":"grok-4-fast-non-reasoning","output":[],
             "usage":{"input_tokens":345,"output_tokens":538,"total_tokens":883,"output_tokens_details":{"reasoning_tokens":123}}}
            """);

        var usage = (await Model(capture).DoGenerateAsync(HelloPrompt(), CancellationToken.None)).Usage;

        Assert.Equal(345, usage.InputTokens);
        Assert.Equal(345, usage.NoCacheInputTokens);
        Assert.Equal(0, usage.CacheReadTokens);
        Assert.Null(usage.CacheWriteTokens);
        Assert.Equal(538, usage.OutputTokens);
        Assert.Equal(415, usage.TextTokens);
        Assert.Equal(123, usage.ReasoningTokens);
        JsonAssert.Equal(usage.Raw!.Value, """{"input_tokens":345,"output_tokens":538,"output_tokens_details":{"reasoning_tokens":123},"total_tokens":883}""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should expose cost_in_usd_ticks in providerMetadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_expose_cost_in_usd_ticks_in_providerMetadata()
    {
        var capture = JsonResponse("""
            {"id":"resp_123","object":"response","status":"completed","model":"grok-4-fast-non-reasoning","output":[],
             "usage":{"input_tokens":10,"output_tokens":5,"cost_in_usd_ticks":113500}}
            """);

        var result = await Model(capture).DoGenerateAsync(HelloPrompt(), CancellationToken.None);

        JsonAssert.Equal(result.ProviderMetadata!.Value, """{"xai":{"costInUsdTicks":113500}}""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should not include providerMetadata when cost_in_usd_ticks is missing", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_include_providerMetadata_when_cost_in_usd_ticks_is_missing()
    {
        var result = await Model(EmptyResponse()).DoGenerateAsync(HelloPrompt(), CancellationToken.None);

        Assert.Null(result.ProviderMetadata);
    }

    [Fact]
    [UpstreamTest(Prefix + "should expose echoed request identifiers in providerMetadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_expose_echoed_request_identifiers_in_providerMetadata()
    {
        var capture = EmptyResponse(extra: ",\"prompt_cache_key\":\"conversation-123\",\"safety_identifier\":\"hashed-user-123\"");

        var result = await Model(capture).DoGenerateAsync(HelloPrompt(), CancellationToken.None);

        JsonAssert.Equal(result.ProviderMetadata!.Value, """{"xai":{"promptCacheKey":"conversation-123","safetyIdentifier":"hashed-user-123"}}""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should extract finish reason from status", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_finish_reason_from_status()
    {
        var result = await Model(EmptyResponse()).DoGenerateAsync(HelloPrompt(), CancellationToken.None);

        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("completed", result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(Prefix + "should return tool-calls finish reason when function_call is present", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_tool_calls_finish_reason_when_function_call_is_present()
    {
        var capture = JsonResponse("""
            {"id":"resp_123","object":"response","status":"completed","model":"grok-4-fast-non-reasoning",
             "output":[{"type":"function_call","id":"fc_123","name":"weather","arguments":"{\"location\":\"sf\"}","call_id":"call_123"}],
             "usage":{"input_tokens":10,"output_tokens":5}}
            """);
        var options = HelloPrompt();
        options.Tools = new[] { WeatherTool() };

        var result = await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
        Assert.Equal("completed", result.RawFinishReason);
    }
}
