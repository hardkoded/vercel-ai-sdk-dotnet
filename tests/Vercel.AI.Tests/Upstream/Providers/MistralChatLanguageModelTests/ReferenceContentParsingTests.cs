// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.MistralChatLanguageModelTests.MistralChatTestSupport;

namespace Vercel.AI.Tests.MistralChatLanguageModelTests;

/// <summary>Port of <c>mistral-chat-language-model.test.ts</c> &gt; <c>reference content parsing</c>.</summary>
public sealed class ReferenceContentParsingTests
{
    private const string Prefix =
        "packages/mistral/src/mistral-chat-language-model.test.ts::reference content parsing::";

    [Fact]
    [UpstreamTest(
        Prefix + "should handle reference_ids as numbers",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_reference_ids_as_numbers()
    {
        var capture = new UpstreamCapture
        {
            ResponseBody =
                "{\"object\":\"chat.completion\",\"id\":\"test-id\",\"created\":1711113008,\"model\":\"mistral-small-latest\",\"choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"Here is the info\"},{\"type\":\"reference\",\"reference_ids\":[1,2,3]}],\"tool_calls\":null},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":4,\"total_tokens\":34,\"completion_tokens\":30}}",
        };

        var result = await Model(capture).DoGenerateAsync(TestPrompt(), CancellationToken.None);

        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("Here is the info", text.Text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle reference_ids as strings",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_reference_ids_as_strings()
    {
        var capture = new UpstreamCapture
        {
            ResponseBody =
                "{\"object\":\"chat.completion\",\"id\":\"test-id\",\"created\":1711113008,\"model\":\"mistral-small-latest\",\"choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"Here is the info\"},{\"type\":\"reference\",\"reference_ids\":[\"ref-1\",\"ref-2\",\"ref-3\"]}],\"tool_calls\":null},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":4,\"total_tokens\":34,\"completion_tokens\":30}}",
        };

        var result = await Model(capture).DoGenerateAsync(TestPrompt(), CancellationToken.None);

        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("Here is the info", text.Text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle mixed reference_ids",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_mixed_reference_ids()
    {
        var capture = new UpstreamCapture
        {
            ResponseBody =
                "{\"object\":\"chat.completion\",\"id\":\"test-id\",\"created\":1711113008,\"model\":\"mistral-small-latest\",\"choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"Here is the info\"},{\"type\":\"reference\",\"reference_ids\":[1,\"ref-2\",3]}],\"tool_calls\":null},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":4,\"total_tokens\":34,\"completion_tokens\":30}}",
        };

        var result = await Model(capture).DoGenerateAsync(TestPrompt(), CancellationToken.None);

        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("Here is the info", text.Text);
    }
}
