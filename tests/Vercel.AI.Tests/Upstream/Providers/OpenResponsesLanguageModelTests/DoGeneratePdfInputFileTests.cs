// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;
using static Vercel.AI.Tests.OpenResponsesLanguageModelTests.OpenResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.OpenResponsesLanguageModelTests;

/// <summary>Port of <c>open-responses-language-model.test.ts</c> &gt; <c>OpenResponsesLanguageModel &gt; doGenerate &gt; pdf input file</c>.</summary>
public sealed class DoGeneratePdfInputFileTests
{
    private const string Prefix = "packages/open-responses/src/responses/open-responses-language-model.test.ts::OpenResponsesLanguageModel > doGenerate > pdf input file::";

    [Fact]
    [UpstreamTest(Prefix + "should send input_file in request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_input_file_in_request_body()
    {
        var capture = JsonFixture("openai-pdf-input-file.1");

        await Generate(capture);

        JsonAssert.Equal(UpstreamChat.Body(capture), """
            {
              "input":[
                {
                  "content":[
                    {"text":"What text does this PDF contain? Reply with just the text content, nothing else.","type":"input_text"},
                    {"file_url":"https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf","type":"input_file"}
                  ],
                  "role":"user",
                  "type":"message"
                }
              ],
              "model":"gpt-4.1-nano"
            }
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should produce correct content", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_produce_correct_content()
    {
        var result = await Generate(JsonFixture("openai-pdf-input-file.1"));

        JsonAssert.Equal(ToJson(result.Content), """[{"providerMetadata":{"lmstudio":{"itemId":"msg_048edf44633e41ae0069d4fea0d1a08194af1e491c093df1d9"}},"text":"Dummy PDF file","type":"text"}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should extract usage correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_usage_correctly()
    {
        var result = await Generate(JsonFixture("openai-pdf-input-file.1"));

        JsonAssert.Equal(Usage(result.Usage), """
            {
              "inputTokens":{"cacheRead":0,"noCache":44,"total":44},
              "outputTokens":{"reasoning":0,"text":4,"total":4},
              "raw":{"input_tokens":44,"input_tokens_details":{"cached_tokens":0},"output_tokens":4,"output_tokens_details":{"reasoning_tokens":0},"total_tokens":48}
            }
            """);
    }

    private static Task<LanguageModelGenerateResult> Generate(UpstreamCapture capture)
    {
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage(new UserContentPart[]
                {
                    new TextContentPart("What text does this PDF contain? Reply with just the text content, nothing else."),
                    new FileContentPart("application/pdf", "https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf", null, null),
                }),
            },
        };
        return Model(capture, "gpt-4.1-nano").DoGenerateAsync(options, CancellationToken.None);
    }
}
