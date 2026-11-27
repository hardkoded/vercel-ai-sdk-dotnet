// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Prompt.ConvertToLanguageModelPrompt.PromptTestSupport;

namespace Vercel.AI.Tests.Upstream.Prompt.ConvertToLanguageModelPrompt;

/// <summary>Port of <c>convert-to-language-model-prompt.test.ts</c> &gt; <c>convertToLanguageModelPrompt &gt; user message &gt; provider options</c>.</summary>
public sealed class UserMessageProviderOptionsTests
{
    private const string Id =
        "packages/ai/src/prompt/convert-to-language-model-prompt.test.ts::convertToLanguageModelPrompt > user message > provider options::";

    [Fact]
    [UpstreamTest(
        Id + "should add provider options to messages",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_add_provider_options_to_messages()
    {
        var result = await ConvertAsync([
            new UserModelMessage([new TextContentPart("hello, world!")])
            {
                ProviderOptions = new Dictionary<string, JsonElement>
                {
                    ["test-provider"] = JsonDocument
                        .Parse("{\"key-a\":\"test-value-1\",\"key-b\":\"test-value-2\"}")
                        .RootElement.Clone(),
                },
            },
        ]);

        var user = Assert.IsType<UserModelMessage>(Assert.Single(result));
        var text = Assert.IsType<TextContentPart>(Assert.Single(user.Content));
        Assert.Equal("hello, world!", text.Text);
        Assert.Null(text.ProviderOptions);
        Assert.Equal(
            "{\"key-a\":\"test-value-1\",\"key-b\":\"test-value-2\"}",
            user.ProviderOptions!["test-provider"].GetRawText()
        );
    }
}
