// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.DeepSeek;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel.DeepSeekChatLanguageModelStreamSupport;
using XaiSupport = Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel;

/// <summary>Port of <c>deepseek-chat-language-model.test.ts</c> &gt; <c>DeepSeekChatLanguageModel &gt; doStream &gt; reasoning</c>.</summary>
public sealed class DoStreamReasoningTests
{
    private const string Prefix = TestFile + " > doStream > reasoning::";

    [Fact]
    [UpstreamTest(
        Prefix + "should keep reasoning active when deltas include empty tool calls",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_keep_reasoning_active_when_deltas_include_empty_tool_calls()
    {
        var handler = Chunks(
            "data: {\"id\":\"chatcmpl-test\",\"object\":\"chat.completion.chunk\",\"created\":1,\"model\":\"test-model\","
                + "\"choices\":[{\"index\":0,\"delta\":{\"role\":\"assistant\",\"content\":\"\",\"reasoning_content\":\"Think \",\"tool_calls\":[]},\"finish_reason\":null}]}\n\n",
            "data: {\"id\":\"chatcmpl-test\",\"object\":\"chat.completion.chunk\",\"created\":1,\"model\":\"test-model\","
                + "\"choices\":[{\"index\":0,\"delta\":{\"content\":\"\",\"reasoning_content\":\"more...\",\"tool_calls\":[]},\"finish_reason\":null}]}\n\n",
            "data: {\"id\":\"chatcmpl-test\",\"object\":\"chat.completion.chunk\",\"created\":1,\"model\":\"test-model\","
                + "\"choices\":[{\"index\":0,\"delta\":{\"content\":\"Hello\",\"reasoning_content\":\"\",\"tool_calls\":[]},\"finish_reason\":\"stop\"}]}\n\n",
            "data: [DONE]\n\n"
        );

        var events = await Stream(Chat(handler, "deepseek-reasoner"), TestPrompt());

        var actual = XaiSupport.ToJson(events.Where(part => part.Type.StartsWith("reasoning-")));
        AssertJson(
            actual,
            """
            [
              { "type": "reasoning-start", "id": "reasoning-0" },
              { "type": "reasoning-delta", "id": "reasoning-0", "delta": "Think " },
              { "type": "reasoning-delta", "id": "reasoning-0", "delta": "more..." },
              { "type": "reasoning-end", "id": "reasoning-0" }
            ]
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map legacy thinking and generic reasoning to canonical request values",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_legacy_thinking_and_generic_reasoning_to_canonical_request_values()
    {
        var handler = ChunksFixture("deepseek-reasoning");
        var options = TestPrompt();
        options.Reasoning = "medium";
        options.ProviderOptions = new DeepSeekLanguageModelChatOptions
        {
            Thinking = new DeepSeekThinkingOptions { Type = "adaptive" },
        }.ToProviderOptions();

        var streamParts = await Stream(Chat(handler, "deepseek-reasoner"), options);

        var requestBody = RequestBody(handler);
        AssertJson(requestBody["thinking"], """{ "type": "enabled" }""");
        Assert.Equal("low", (string)requestBody["reasoning_effort"]!);
        AssertJson(
            StreamStartJson(streamParts),
            """
            {
              "type": "stream-start",
              "warnings": [
                {
                  "type": "compatibility",
                  "feature": "thinking.type",
                  "details": "thinking.type \"adaptive\" is not a canonical DeepSeek value. mapped to \"enabled\"."
                },
                {
                  "type": "compatibility",
                  "feature": "reasoning",
                  "details": "reasoning \"medium\" is not directly supported by this model. mapped to effort \"low\"."
                }
              ]
            }
            """
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream reasoning", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_reasoning()
    {
        var handler = ChunksFixture("deepseek-reasoning");

        var parts = await Stream(Chat(handler, "deepseek-reasoning"), TestPrompt());

        AssertStreamSnapshot(parts, "deepseek-chat-reasoning.stream.snap.json");
    }
}
