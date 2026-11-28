// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.MistralChatLanguageModelTests.MistralChatTestSupport;

namespace Vercel.AI.Tests.MistralChatLanguageModelTests;

/// <summary>Port of <c>mistral-chat-language-model.test.ts</c> &gt; <c>tool result format support</c>.</summary>
public sealed class ToolResultFormatSupportTests
{
    [Fact]
    [UpstreamTest(
        "packages/mistral/src/mistral-chat-language-model.test.ts::tool result format support::should handle new LanguageModelV4ToolResultOutput format",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_new_LanguageModelV4ToolResultOutput_format()
    {
        var capture = new UpstreamCapture
        {
            ResponseBody =
                "{\"id\":\"test-id\",\"object\":\"chat.completion\",\"created\":1234567890,\"model\":\"mistral-small\",\"choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":\"Here is the result\",\"tool_calls\":null},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":5,\"total_tokens\":15}}",
        };
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage("Hello"),
                new AssistantModelMessage(
                    new AssistantContentPart[]
                    {
                        new AssistantToolCallPart(
                            new GeneratedToolCall("call-1", "test-tool", "{\"query\":\"test\"}")
                        ),
                    }
                ),
                new ToolModelMessage("call-1", "test-tool", "{\"result\":\"success\"}", false),
            },
        };

        var result = await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("Here is the result", text.Text);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("stop", result.RawFinishReason);
    }
}
