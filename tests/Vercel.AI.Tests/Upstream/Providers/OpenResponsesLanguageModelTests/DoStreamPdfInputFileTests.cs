// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;
using static Vercel.AI.Tests.OpenResponsesLanguageModelTests.OpenResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.OpenResponsesLanguageModelTests;

/// <summary>Port of <c>open-responses-language-model.test.ts</c> &gt; <c>OpenResponsesLanguageModel &gt; doStream &gt; pdf input file</c>.</summary>
public sealed class DoStreamPdfInputFileTests
{
    private const string Prefix = "packages/open-responses/src/responses/open-responses-language-model.test.ts::OpenResponsesLanguageModel > doStream > pdf input file::";

    [Fact]
    [UpstreamTest(Prefix + "should stream content from pdf input", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_content_from_pdf_input()
    {
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage(new UserContentPart[]
                {
                    new TextContentPart("What text does this PDF contain?"),
                    new FileContentPart("application/pdf", "https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf", null, null),
                }),
            },
        };

        var parts = await Collect(Model(ChunksFixture("openai-pdf-input-file.1"), "gpt-4.1-nano"), options);

        const string id = "msg_051ebd7ab60063870069d4fe8c1b7c8194b701e22f1ef094dd";
        JsonAssert.Equal(ToJson(parts), """
            [
              {"type":"stream-start","warnings":[]},
              {"id":"ID","type":"text-start"},
              {"delta":"Dummy","id":"ID","type":"text-delta"},
              {"delta":" PDF","id":"ID","type":"text-delta"},
              {"delta":" file","id":"ID","type":"text-delta"},
              {"id":"ID","providerMetadata":{"lmstudio":{"itemId":"ID"}},"type":"text-end"},
              {
                "finishReason":{"unified":"stop"},
                "type":"finish",
                "usage":{
                  "inputTokens":{"cacheRead":0,"noCache":44,"total":44},
                  "outputTokens":{"reasoning":0,"text":4,"total":4},
                  "raw":{"input_tokens":44,"input_tokens_details":{"cached_tokens":0},"output_tokens":4,"output_tokens_details":{"reasoning_tokens":0},"total_tokens":48}
                }
              }
            ]
            """.Replace("ID", id));
    }
}
