// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel;

/// <summary>Port of <c>xai-responses-language-model.test.ts</c> &gt; <c>XaiResponsesLanguageModel &gt; doStream &gt; citation streaming</c>.</summary>
public sealed class DoStreamCitationStreamingTests
{
    private const string Prefix = TestFile + " > doStream > citation streaming::";

    [Fact]
    [UpstreamTest(Prefix + "should stream citations as sources", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_citations_as_sources()
    {
        var capture = StreamResponse(
            """{"type":"response.created","response":{"id":"resp_123","object":"response","model":"grok-4-fast-non-reasoning","status":"in_progress","output":[]}}""",
            """{"type":"response.output_text.annotation.added","item_id":"msg_123","output_index":0,"content_index":0,"annotation_index":0,"annotation":{"type":"url_citation","url":"https://example.com","title":"example"}}""",
            """{"type":"response.done","response":{"id":"resp_123","object":"response","status":"completed","output":[],"usage":{"input_tokens":10,"output_tokens":5}}}""");

        var parts = await Collect(Model(capture), HelloPrompt());

        var source = Assert.Single(parts.OfType<SourceStreamPart>());
        Assert.Equal(("id-0", "https://example.com", "example"), (source.Id, source.Url, source.Title));
    }
}
