// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.GroqChatLanguageModelTests.GroqChatTestSupport;

namespace Vercel.AI.Tests.GroqChatLanguageModelTests;

/// <summary>Port of <c>groq-chat-language-model.test.ts</c> &gt; <c>doGenerate &gt; tool call</c>.</summary>
public sealed class DoGenerateToolCallTests
{
    [Fact]
    [UpstreamTest(
        TestFile + "::doGenerate > tool call::should extract tool call content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_tool_call_content()
    {
        var capture = JsonFixture("groq-tool-call");

        var result = await Model(capture).DoGenerateAsync(TestPrompt(), CancellationToken.None);

        var toolCall = Assert.IsType<GeneratedToolCall>(Assert.Single(result.Content));
        Assert.Equal("ax9fskhev", toolCall.ToolCallId);
        Assert.Equal("weather", toolCall.ToolName);
        Assert.Equal("{}", toolCall.ArgumentsJson);
        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
        Assert.Equal("tool_calls", result.RawFinishReason);
        AssertJson(
            """{"model":"gemma2-9b-it","messages":[{"role":"user","content":"Hello"}]}""",
            JsonNode.Parse(result.Request!.Body!.Value.GetRawText())
        );
        AssertUsage(
            result.Usage,
            218,
            218,
            null,
            15,
            15,
            null,
            """{"prompt_tokens":218,"completion_tokens":15,"total_tokens":233}"""
        );
    }
}
