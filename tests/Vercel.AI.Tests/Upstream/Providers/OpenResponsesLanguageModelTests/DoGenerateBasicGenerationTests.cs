// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Tests.Upstream;
using static Vercel.AI.Tests.OpenResponsesLanguageModelTests.OpenResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.OpenResponsesLanguageModelTests;

/// <summary>Port of <c>open-responses-language-model.test.ts</c> &gt; <c>OpenResponsesLanguageModel &gt; doGenerate &gt; basic generation</c>.</summary>
public sealed class DoGenerateBasicGenerationTests
{
    private const string Prefix = "packages/open-responses/src/responses/open-responses-language-model.test.ts::OpenResponsesLanguageModel > doGenerate > basic generation::";

    [Fact]
    [UpstreamTest(Prefix + "should send correct request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_correct_request_body()
    {
        var capture = JsonFixture("lmstudio-basic.1");

        await Model(capture).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);

        JsonAssert.Equal(UpstreamChat.Body(capture), """
            {"input":[{"content":[{"text":"Hello","type":"input_text"}],"role":"user","type":"message"}],"model":"gemma-7b-it"}
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should produce correct content", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_produce_correct_content()
    {
        var result = await Model(JsonFixture("lmstudio-basic.1")).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);

        JsonAssert.Equal(ToJson(result.Content), """
            [
              {
                "providerMetadata":{"lmstudio":{"itemId":"rs_3l1z5wpifxkwxhj459ya7","reasoningContent":[{"text":"reasoning content","type":"reasoning_text"}],"reasoningSummary":[]}},
                "text":"reasoning content",
                "type":"reasoning"
              },
              {"providerMetadata":{"lmstudio":{"itemId":"msg_p1y190hl7hj1xyfqr1cir"}},"text":"text content","type":"text"}
            ]
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should extract usage correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_usage_correctly()
    {
        var result = await Model(JsonFixture("lmstudio-basic.1")).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);

        JsonAssert.Equal(Usage(result.Usage), """
            {
              "inputTokens":{"cacheRead":0,"noCache":136,"total":136},
              "outputTokens":{"reasoning":2456,"text":1221,"total":3677},
              "raw":{"input_tokens":136,"input_tokens_details":{"cached_tokens":0},"output_tokens":3677,"output_tokens_details":{"reasoning_tokens":2456},"total_tokens":3813}
            }
            """);
    }
}
