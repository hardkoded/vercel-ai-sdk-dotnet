// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Alibaba.AlibabaChatLanguageModelTests.AlibabaChatTestSupport;

namespace Vercel.AI.Tests.Upstream.Alibaba.AlibabaChatLanguageModelTests;

/// <summary>Port of <c>alibaba-chat-language-model.test.ts</c> &gt; <c>doGenerate &gt; tool call</c>.</summary>
public sealed class DoGenerateToolCallTests
{
    [Fact]
    [UpstreamTest("packages/alibaba/src/alibaba-chat-language-model.test.ts::doGenerate > tool call::should extract tool call content", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_tool_call_content()
    {
        var capture = CaptureFixture("alibaba-tool-call");
        var model = Model(capture);

        var result = await model.DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);

        var call = Assert.IsType<GeneratedToolCall>(Assert.Single(result.Content));
        Assert.Equal("{\"location\": \"San Francisco\"}", call.ArgumentsJson);
        Assert.Equal("call_962bfd2ab8f54b89a1161356", call.ToolCallId);
        Assert.Equal("weather", call.ToolName);
        AssertResultSnapshot(result, capture, "alibaba-tool-call", "tool_calls", FinishReason.ToolCalls, "chatcmpl-bc7fc58d-c03f-9c9f-af73-91bea326c99f", new DateTimeOffset(2026, 2, 10, 23, 7, 37, TimeSpan.Zero), 295, 22, 0);
    }
}
