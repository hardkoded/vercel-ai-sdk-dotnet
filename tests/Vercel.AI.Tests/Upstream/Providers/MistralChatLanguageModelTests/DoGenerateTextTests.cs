// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.MistralChatLanguageModelTests.MistralChatTestSupport;

namespace Vercel.AI.Tests.MistralChatLanguageModelTests;

/// <summary>Port of <c>mistral-chat-language-model.test.ts</c> &gt; <c>doGenerate &gt; text</c>.</summary>
public sealed class DoGenerateTextTests
{
    private const string Prefix =
        "packages/mistral/src/mistral-chat-language-model.test.ts::doGenerate > text::";

    [Fact]
    [UpstreamTest(Prefix + "should extract text content", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_text_content()
    {
        var capture = JsonFixture("mistral-text");

        var result = await Model(capture).DoGenerateAsync(TestPrompt(), CancellationToken.None);

        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.StartsWith("**Holiday Name: \"World Kindness Day of Sharing\"**", text.Text);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("stop", result.RawFinishReason);
        Assert.Equal("5319bd0299614c679a0068a4f2c8ffd0", result.ResponseId);
        Assert.Equal("mistral-small-latest", result.ResponseModelId);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send correct request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_correct_request_body()
    {
        var capture = JsonFixture("mistral-text");

        await Model(capture).DoGenerateAsync(TestPrompt(), CancellationToken.None);

        Assert.True(
            JsonNode.DeepEquals(
                Body(capture),
                JsonNode.Parse(
                    "{\"messages\":[{\"content\":[{\"text\":\"Hello\",\"type\":\"text\"}],\"role\":\"user\"}],\"model\":\"mistral-small-latest\"}"
                )
            )
        );
    }
}
