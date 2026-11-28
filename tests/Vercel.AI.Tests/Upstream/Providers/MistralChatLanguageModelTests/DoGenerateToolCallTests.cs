// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.MistralChatLanguageModelTests.MistralChatTestSupport;

namespace Vercel.AI.Tests.MistralChatLanguageModelTests;

/// <summary>Port of <c>mistral-chat-language-model.test.ts</c> &gt; <c>doGenerate &gt; tool call</c>.</summary>
public sealed class DoGenerateToolCallTests
{
    private const string Prefix =
        "packages/mistral/src/mistral-chat-language-model.test.ts::doGenerate > tool call::";

    [Fact]
    [UpstreamTest(Prefix + "should extract tool call content", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_tool_call_content()
    {
        var capture = JsonFixture("mistral-tool-call");

        var result = await Model(capture).DoGenerateAsync(TestPrompt(), CancellationToken.None);

        var call = Assert.IsType<GeneratedToolCall>(Assert.Single(result.Content));
        Assert.Equal("gSIMJiOkT", call.ToolCallId);
        Assert.Equal("weather", call.ToolName);
        Assert.Equal("{\"location\": \"San Francisco\"}", call.ArgumentsJson);
        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
        Assert.Equal("tool_calls", result.RawFinishReason);
        Assert.Equal(124, result.Usage.InputTokens);
        Assert.Equal(22, result.Usage.OutputTokens);
    }
}
