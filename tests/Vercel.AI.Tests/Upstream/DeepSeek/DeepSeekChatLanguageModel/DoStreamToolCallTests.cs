// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.DeepSeek;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel.DeepSeekChatLanguageModelStreamSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel;

/// <summary>Port of <c>deepseek-chat-language-model.test.ts</c> &gt; <c>DeepSeekChatLanguageModel &gt; doStream &gt; tool call</c>.</summary>
public sealed class DoStreamToolCallTests
{
    private const string Prefix = TestFile + " > doStream > tool call::";

    [Fact]
    [UpstreamTest(Prefix + "should stream tool call", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_tool_call()
    {
        var handler = ChunksFixture("deepseek-tool-call");
        var options = TestPrompt();
        options.Tools = new[]
        {
            new LanguageModelTool(
                "weather",
                null,
                UpstreamChat.Json(
                    """
                    {
                      "type": "object",
                      "properties": { "location": { "type": "string" } },
                      "required": ["location"],
                      "additionalProperties": false,
                      "$schema": "http://json-schema.org/draft-07/schema#"
                    }
                    """
                )
            ),
        };
        options.ProviderOptions = new DeepSeekLanguageModelChatOptions
        {
            Thinking = new DeepSeekThinkingOptions { Type = "enabled" },
        }.ToProviderOptions();

        var parts = await Stream(Chat(handler, "deepseek-reasoner"), options);

        AssertStreamSnapshot(parts, "deepseek-chat-tool-call.stream.snap.json");
    }
}
