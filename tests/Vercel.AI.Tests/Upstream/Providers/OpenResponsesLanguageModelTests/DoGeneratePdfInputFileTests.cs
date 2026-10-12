// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.OpenResponsesLanguageModelTests.OpenResponsesTestSupport;

namespace Vercel.AI.Tests.OpenResponsesLanguageModelTests;

/// <summary>Port of <c>open-responses-language-model.test.ts</c> &gt; <c>OpenResponsesLanguageModel &gt; doGenerate &gt; pdf input file</c>.</summary>
public sealed class DoGeneratePdfInputFileTests
{
    private const string Prefix = "packages/open-responses/src/responses/open-responses-language-model.test.ts::OpenResponsesLanguageModel > doGenerate > pdf input file::";

    [Fact]
    [UpstreamTest(Prefix + "should send input_file in request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_input_file_in_request_body()
    {
        var (capture, _) = await Generate();

        Assert.True(JsonEquals("""
            {"input":[{"content":[{"text":"What text does this PDF contain? Reply with just the text content, nothing else.","type":"input_text"},{"file_url":"https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf","type":"input_file"}],"role":"user","type":"message"}],"model":"gpt-4.1-nano"}
            """, System.Text.Json.Nodes.JsonNode.Parse(capture.Requests[0].Body)));
    }

    [Fact]
    [UpstreamTest(Prefix + "should produce correct content", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_produce_correct_content()
    {
        var (_, result) = await Generate();

        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("Dummy PDF file", text.Text);
        Assert.True(JsonEquals("""{"open-responses":{"itemId":"msg_048edf44633e41ae0069d4fea0d1a08194af1e491c093df1d9"}}""", System.Text.Json.Nodes.JsonNode.Parse(text.ProviderMetadata!.Value.GetRawText())));
    }

    [Fact]
    [UpstreamTest(Prefix + "should extract usage correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_usage_correctly()
    {
        var (_, result) = await Generate();

        Assert.Equal(0, result.Usage.CacheReadTokens);
        Assert.Null(result.Usage.CacheWriteTokens);
        Assert.Equal(44, result.Usage.NoCacheInputTokens);
        Assert.Equal(44, result.Usage.InputTokens);
        Assert.Equal(0, result.Usage.ReasoningTokens);
        Assert.Equal(4, result.Usage.TextTokens);
        Assert.Equal(4, result.Usage.OutputTokens);
        Assert.True(JsonEquals("""
            {"input_tokens":44,"input_tokens_details":{"cached_tokens":0},"output_tokens":4,"output_tokens_details":{"reasoning_tokens":0},"total_tokens":48}
            """, System.Text.Json.Nodes.JsonNode.Parse(result.Usage.Raw!.Value.GetRawText())));
    }

    private static async Task<(UpstreamCapture Capture, LanguageModelGenerateResult Result)> Generate()
    {
        var capture = new UpstreamCapture { ResponseBody = Fixture("openai-pdf-input-file.1") };
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
        var result = await CreateModel(capture, "gpt-4.1-nano").DoGenerateAsync(options, CancellationToken.None);
        return (capture, result);
    }
}
