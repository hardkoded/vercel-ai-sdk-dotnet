// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel;

/// <summary>Port of <c>xai-responses-language-model.test.ts</c> &gt; <c>XaiResponsesLanguageModel &gt; doGenerate &gt; citations</c>.</summary>
public sealed class DoGenerateCitationsTests
{
    private const string Prefix = TestFile + " > doGenerate > citations::";

    [Fact]
    [UpstreamTest(Prefix + "should extract citations from annotations", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_citations_from_annotations()
    {
        var capture = JsonResponse("""
            {"id":"resp_123","object":"response","status":"completed","model":"grok-4-fast-non-reasoning",
             "output":[{"type":"message","id":"msg_123","status":"completed","role":"assistant","content":[
               {"type":"output_text","text":"based on research","annotations":[
                 {"type":"url_citation","url":"https://example.com","title":"example title"},
                 {"type":"url_citation","url":"https://test.com"}]}]}],
             "usage":{"input_tokens":10,"output_tokens":5}}
            """);

        var result = await Model(capture).DoGenerateAsync(HelloPrompt(), CancellationToken.None);

        Assert.Equal(3, result.Content.Count);
        Assert.Equal("based on research", Assert.IsType<GeneratedText>(result.Content[0]).Text);
        var first = Assert.IsType<GeneratedSource>(result.Content[1]);
        Assert.Equal(("id-0", "https://example.com", "example title"), (first.Id, first.Url, first.Title));
        var second = Assert.IsType<GeneratedSource>(result.Content[2]);
        Assert.Equal(("id-1", "https://test.com", "https://test.com"), (second.Id, second.Url, second.Title));
    }
}
