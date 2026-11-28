// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.GroqChatLanguageModelTests.GroqChatTestSupport;

namespace Vercel.AI.Tests.GroqChatLanguageModelTests;

/// <summary>Port of <c>groq-chat-language-model.test.ts</c> &gt; <c>doGenerate &gt; text</c>.</summary>
public sealed class DoGenerateTextTests
{
    private const string Prefix = TestFile + "::doGenerate > text::";

    [Fact]
    [UpstreamTest(Prefix + "should extract text content", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_text_content()
    {
        var capture = JsonFixture("groq-text");
        var expected = JsonNode.Parse(Fixture("groq-text"))!["choices"]![0]!["message"]![
            "content"
        ]!.GetValue<string>();

        var result = await Model(capture).DoGenerateAsync(TestPrompt(), CancellationToken.None);

        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal(expected, text.Text);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("stop", result.RawFinishReason);
        AssertJson(
            """{"model":"gemma2-9b-it","messages":[{"role":"user","content":"Hello"}]}""",
            JsonNode.Parse(result.Request!.Body!.Value.GetRawText())
        );
        Assert.Equal(
            JsonNode.Parse(Fixture("groq-text"))!.ToJsonString(),
            JsonNode.Parse(result.RawResponse!)!.ToJsonString()
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send correct request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_correct_request_body()
    {
        var capture = JsonFixture("groq-text");

        await Model(capture).DoGenerateAsync(TestPrompt(), CancellationToken.None);

        AssertJson(
            """{"messages":[{"content":"Hello","role":"user"}],"model":"gemma2-9b-it"}""",
            UpstreamChat.Body(capture)
        );
    }
}
