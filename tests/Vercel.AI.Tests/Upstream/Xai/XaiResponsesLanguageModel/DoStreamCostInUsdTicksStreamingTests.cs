// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel;

/// <summary>Port of <c>xai-responses-language-model.test.ts</c> &gt; <c>XaiResponsesLanguageModel &gt; doStream &gt; cost_in_usd_ticks streaming</c>.</summary>
public sealed class DoStreamCostInUsdTicksStreamingTests
{
    private const string Prefix = TestFile + " > doStream > cost_in_usd_ticks streaming::";

    [Fact]
    [UpstreamTest(Prefix + "should expose cost_in_usd_ticks in finish providerMetadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_expose_cost_in_usd_ticks_in_finish_providerMetadata()
    {
        var capture = StreamResponse(
            """{"type":"response.created","response":{"id":"resp_123","object":"response","model":"grok-4-fast-non-reasoning","output":[]}}""",
            """{"type":"response.output_text.delta","output_index":0,"content_index":0,"delta":"Hello"}""",
            """{"type":"response.completed","response":{"id":"resp_123","object":"response","model":"grok-4-fast-non-reasoning","status":"completed","output":[{"type":"message","id":"msg_001","role":"assistant","status":"completed","content":[{"type":"output_text","text":"Hello"}]}],"usage":{"input_tokens":10,"output_tokens":5,"cost_in_usd_ticks":113500}}}""");

        var parts = await Collect(Model(capture), HelloPrompt());

        JsonAssert.Equal(First<FinishStreamPart>(parts).ProviderMetadata!.Value, """{"xai":{"costInUsdTicks":113500}}""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should expose echoed request identifiers in finish providerMetadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_expose_echoed_request_identifiers_in_finish_providerMetadata()
    {
        var capture = StreamResponse(
            """{"type":"response.created","response":{"id":"resp_123","object":"response","model":"grok-4-fast-non-reasoning","output":[]}}""",
            """{"type":"response.completed","response":{"id":"resp_123","object":"response","model":"grok-4-fast-non-reasoning","status":"completed","output":[],"usage":{"input_tokens":10,"output_tokens":5},"prompt_cache_key":"conversation-123","safety_identifier":"hashed-user-123"}}""");

        var parts = await Collect(Model(capture), HelloPrompt());

        JsonAssert.Equal(First<FinishStreamPart>(parts).ProviderMetadata!.Value, """{"xai":{"promptCacheKey":"conversation-123","safetyIdentifier":"hashed-user-123"}}""");
    }
}
