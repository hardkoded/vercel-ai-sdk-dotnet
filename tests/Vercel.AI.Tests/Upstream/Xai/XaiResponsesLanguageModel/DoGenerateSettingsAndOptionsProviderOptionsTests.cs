// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel;

/// <summary>Port of <c>xai-responses-language-model.test.ts</c> &gt; <c>XaiResponsesLanguageModel &gt; doGenerate &gt; settings and options &gt; provider options</c>.</summary>
public sealed class DoGenerateSettingsAndOptionsProviderOptionsTests
{
    private const string Prefix = TestFile + " > doGenerate > settings and options > provider options::";

    [Fact]
    [UpstreamTest(Prefix + "reasoningEffort", Coverage = UpstreamCoverage.Covered)]
    public async Task ReasoningEffort()
    {
        var capture = EmptyResponse();

        await Model(capture).DoGenerateAsync(WithOptions("""{"reasoningEffort":"high"}"""), CancellationToken.None);

        Assert.Equal("high", (string?)Body(capture)["reasoning"]!["effort"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "reasoningEffort: \"xhigh\" for grok-4.6", Coverage = UpstreamCoverage.Covered)]
    public async Task ReasoningEffort_xhigh_for_grok_4_6()
    {
        var capture = EmptyResponse("grok-4.6");

        await Model(capture, "grok-4.6").DoGenerateAsync(WithOptions("""{"reasoningEffort":"xhigh"}"""), CancellationToken.None);

        Assert.Equal("xhigh", (string?)Body(capture)["reasoning"]!["effort"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "reasoningEffort: \"none\"", Coverage = UpstreamCoverage.Covered)]
    public async Task ReasoningEffort_none()
    {
        var capture = EmptyResponse("grok-4.3");

        await Model(capture).DoGenerateAsync(WithOptions("""{"reasoningEffort":"none"}"""), CancellationToken.None);

        Assert.Equal("none", (string?)Body(capture)["reasoning"]!["effort"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "accepts deprecated reasoningSummary without sending it", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_deprecated_reasoningSummary_without_sending_it()
    {
        var capture = EmptyResponse("grok-4.7");

        await Model(capture).DoGenerateAsync(WithOptions("""{"reasoningSummary":"concise"}"""), CancellationToken.None);

        Assert.False(Body(capture).ContainsKey("reasoning"));
    }

    [Fact]
    [UpstreamTest(Prefix + "logprobs and topLogprobs", Coverage = UpstreamCoverage.Covered)]
    public async Task Logprobs_and_topLogprobs()
    {
        var capture = EmptyResponse("grok-4-fast");

        await Model(capture).DoGenerateAsync(WithOptions("""{"logprobs":true,"topLogprobs":6}"""), CancellationToken.None);

        Assert.True((bool?)Body(capture)["logprobs"]);
        Assert.Equal(6, (int?)Body(capture)["top_logprobs"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "topLogprobs enables logprobs", Coverage = UpstreamCoverage.Covered)]
    public async Task TopLogprobs_enables_logprobs()
    {
        var capture = EmptyResponse("grok-4-fast");

        await Model(capture).DoGenerateAsync(WithOptions("""{"topLogprobs":2}"""), CancellationToken.None);

        Assert.True((bool?)Body(capture)["logprobs"]);
        Assert.Equal(2, (int?)Body(capture)["top_logprobs"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "store:true", Coverage = UpstreamCoverage.Covered)]
    public async Task Store_true()
    {
        var capture = EmptyResponse();

        await Model(capture).DoGenerateAsync(WithOptions("""{"store":true}"""), CancellationToken.None);

        Assert.False(Body(capture).ContainsKey("store"));
        Assert.False(Body(capture).ContainsKey("include"));
    }

    [Fact]
    [UpstreamTest(Prefix + "store:false", Coverage = UpstreamCoverage.Covered)]
    public async Task Store_false()
    {
        var capture = EmptyResponse();

        await Model(capture).DoGenerateAsync(WithOptions("""{"store":false}"""), CancellationToken.None);

        Assert.False((bool?)Body(capture)["store"]);
        JsonAssert.Equal(Body(capture)["include"], """["reasoning.encrypted_content"]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "previousResponseId", Coverage = UpstreamCoverage.Covered)]
    public async Task PreviousResponseId()
    {
        var capture = EmptyResponse();

        await Model(capture).DoGenerateAsync(WithOptions("""{"previousResponseId":"resp_456"}"""), CancellationToken.None);

        Assert.Equal("resp_456", (string?)Body(capture)["previous_response_id"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "additional request options", Coverage = UpstreamCoverage.Covered)]
    public async Task Additional_request_options()
    {
        var capture = EmptyResponse("grok-4.7");
        var options = WithOptions("""
            {"minP":0.1,"maxTurns":5,"parallelToolCalls":false,"promptCacheKey":"conversation-123",
             "safetyIdentifier":"hashed-user-123","serviceTier":"priority","user":"user-123"}
            """);
        options.TopK = 40;

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        var body = Body(capture);
        Assert.Equal(40, (int?)body["top_k"]);
        Assert.Equal(0.1, (double?)body["min_p"]);
        Assert.Equal(5, (int?)body["max_turns"]);
        Assert.False((bool?)body["parallel_tool_calls"]);
        Assert.Equal("conversation-123", (string?)body["prompt_cache_key"]);
        Assert.Equal("hashed-user-123", (string?)body["safety_identifier"]);
        Assert.Equal("priority", (string?)body["service_tier"]);
        Assert.Equal("user-123", (string?)body["user"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "serviceTier", Coverage = UpstreamCoverage.Covered)]
    public async Task ServiceTier()
    {
        var capture = EmptyResponse("grok-4.6", ",\"service_tier\":\"priority\"");

        var result = await Model(capture, "grok-4.6").DoGenerateAsync(WithOptions("""{"serviceTier":"priority"}"""), CancellationToken.None);

        Assert.Equal("priority", (string?)Body(capture)["service_tier"]);
        Assert.Equal("priority", result.ProviderMetadata!.Value.GetProperty("xai").GetProperty("serviceTier").GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "serviceTier reports a downgrade to default", Coverage = UpstreamCoverage.Covered)]
    public async Task ServiceTier_reports_a_downgrade_to_default()
    {
        var capture = EmptyResponse("grok-4.6", ",\"service_tier\":\"default\"");

        var result = await Model(capture, "grok-4.6").DoGenerateAsync(WithOptions("""{"serviceTier":"priority"}"""), CancellationToken.None);

        Assert.Equal("default", result.ProviderMetadata!.Value.GetProperty("xai").GetProperty("serviceTier").GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "include with file_search_call.results", Coverage = UpstreamCoverage.Covered)]
    public async Task Include_with_file_search_call_results()
    {
        var capture = EmptyResponse();

        await Model(capture).DoGenerateAsync(WithOptions("""{"include":["file_search_call.results"]}"""), CancellationToken.None);

        JsonAssert.Equal(Body(capture)["include"], """["file_search_call.results"]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "include with no_inline_citations", Coverage = UpstreamCoverage.Covered)]
    public async Task Include_with_no_inline_citations()
    {
        var capture = EmptyResponse("grok-4.7");

        await Model(capture).DoGenerateAsync(WithOptions("""{"include":["no_inline_citations"]}"""), CancellationToken.None);

        JsonAssert.Equal(Body(capture)["include"], """["no_inline_citations"]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "include with server-side tool outputs", Coverage = UpstreamCoverage.Covered)]
    public async Task Include_with_server_side_tool_outputs()
    {
        var capture = EmptyResponse("grok-4.7");

        await Model(capture).DoGenerateAsync(
            WithOptions("""{"include":["web_search_call.action.sources","code_interpreter_call.outputs"]}"""),
            CancellationToken.None);

        JsonAssert.Equal(Body(capture)["include"], """["web_search_call.action.sources","code_interpreter_call.outputs"]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "include with file_search_call.results and store:false", Coverage = UpstreamCoverage.Covered)]
    public async Task Include_with_file_search_call_results_and_store_false()
    {
        var capture = EmptyResponse();

        await Model(capture).DoGenerateAsync(
            WithOptions("""{"include":["file_search_call.results"],"store":false}"""),
            CancellationToken.None);

        Assert.False((bool?)Body(capture)["store"]);
        JsonAssert.Equal(Body(capture)["include"], """["file_search_call.results","reasoning.encrypted_content"]""");
    }

    private static LanguageModelCallOptions WithOptions(string xaiOptions)
    {
        var options = HelloPrompt();
        options.ProviderOptions = UpstreamChat.Bag("xai", xaiOptions);
        return options;
    }
}
