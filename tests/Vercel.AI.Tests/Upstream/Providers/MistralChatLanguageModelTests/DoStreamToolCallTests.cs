// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.MistralChatLanguageModelTests.MistralChatTestSupport;

namespace Vercel.AI.Tests.MistralChatLanguageModelTests;

/// <summary>Port of <c>mistral-chat-language-model.test.ts</c> &gt; <c>doStream &gt; tool call</c>.</summary>
public sealed class DoStreamToolCallTests
{
    private const string Prefix =
        "packages/mistral/src/mistral-chat-language-model.test.ts::doStream > tool call::";

    [Fact]
    [UpstreamTest(Prefix + "should stream tool call", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_tool_call()
    {
        var handler = ChunksFixture("mistral-tool-call");
        var model = Model(handler, "mistral-small-latest", options =>
            options.GenerateId = MockId()
        );

        var result = await model.DoStreamAsync(TestPrompt(), CancellationToken.None);
        var parts = Describe(await UpstreamChat.Read(result.Stream));

        Assert.Equal(
            new[]
            {
                "stream-start:0",
                "response-metadata:b3999b8c93e04e11bcbff7bcab829667:mistral-small-latest:2026-01-22T13:34:14.000Z",
                "tool-input-start:gSIMJiOkT:weather",
                "tool-input-delta:gSIMJiOkT:{\"location\": \"San Francisco\"}",
                "tool-input-end:gSIMJiOkT",
                "tool-call:gSIMJiOkT:weather:{\"location\": \"San Francisco\"}",
                "finish:ToolCalls:tool_calls:124/124/:22/22",
            },
            parts
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should accumulate incremental tool call arguments",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_accumulate_incremental_tool_call_arguments()
    {
        var handler = ChunksFixture("mistral-incremental-tool-call");
        var model = Model(handler, "mistral-small-latest", options =>
            options.GenerateId = MockId()
        );

        var result = await model.DoStreamAsync(TestPrompt(), CancellationToken.None);
        var parts = Describe(await UpstreamChat.Read(result.Stream));

        Assert.Equal(
            new[]
            {
                "tool-input-start:chatcmpl-tool-9f149c74c42f265b:webSearchTool",
                "tool-input-delta:chatcmpl-tool-9f149c74c42f265b:{\"query\": \"current Berlin weather\"}",
                "tool-input-end:chatcmpl-tool-9f149c74c42f265b",
                "tool-call:chatcmpl-tool-9f149c74c42f265b:webSearchTool:{\"query\": \"current Berlin weather\"}",
            },
            parts
                .Where(part =>
                    part == "error" || part.StartsWith("tool-", StringComparison.Ordinal)
                )
                .ToList()
        );
    }
}
