// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Alibaba.AlibabaChatLanguageModelTests.AlibabaChatTestSupport;

namespace Vercel.AI.Tests.Upstream.Alibaba.AlibabaChatLanguageModelTests;

/// <summary>Port of <c>alibaba-chat-language-model.test.ts</c> &gt; <c>doStream &gt; reasoning</c>.</summary>
public sealed class DoStreamReasoningTests
{
    [Fact]
    [UpstreamTest("packages/alibaba/src/alibaba-chat-language-model.test.ts::doStream > reasoning::should keep reasoning active when deltas include empty tool calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_keep_reasoning_active_when_deltas_include_empty_tool_calls()
    {
        var handler = new SseStreamHandler(new[]
        {
            "data: {\"id\":\"chatcmpl-test\",\"object\":\"chat.completion.chunk\",\"created\":1,\"model\":\"test-model\"," +
                "\"choices\":[{\"index\":0,\"delta\":{\"role\":\"assistant\",\"content\":\"\",\"reasoning_content\":\"Think \",\"tool_calls\":[]},\"finish_reason\":null}]}\n\n",
            "data: {\"id\":\"chatcmpl-test\",\"object\":\"chat.completion.chunk\",\"created\":1,\"model\":\"test-model\"," +
                "\"choices\":[{\"index\":0,\"delta\":{\"content\":\"\",\"reasoning_content\":\"more...\",\"tool_calls\":[]},\"finish_reason\":null}]}\n\n",
            "data: {\"id\":\"chatcmpl-test\",\"object\":\"chat.completion.chunk\",\"created\":1,\"model\":\"test-model\"," +
                "\"choices\":[{\"index\":0,\"delta\":{\"content\":\"Hello\",\"reasoning_content\":\"\",\"tool_calls\":[]},\"finish_reason\":\"stop\"}]}\n\n",
            "data: [DONE]\n\n",
        });
        var model = StreamModel(handler);

        var parts = await ReadStream(model, UpstreamChat.Prompt());

        var reasoning = new JsonArray();
        foreach (var part in parts.Where(part => part.Type.StartsWith("reasoning-", StringComparison.Ordinal)))
        {
            reasoning.Add(SnapshotOf(part));
        }

        AssertJson(
            "[{\"type\":\"reasoning-start\",\"id\":\"test-reasoning-id\"},{\"type\":\"reasoning-delta\",\"id\":\"test-reasoning-id\",\"delta\":\"Think \"},{\"type\":\"reasoning-delta\",\"id\":\"test-reasoning-id\",\"delta\":\"more...\"},{\"type\":\"reasoning-end\",\"id\":\"test-reasoning-id\"}]",
            reasoning);
    }

    [Fact]
    [UpstreamTest("packages/alibaba/src/alibaba-chat-language-model.test.ts::doStream > reasoning::should stream reasoning", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_reasoning()
    {
        var model = StreamModel(FixtureStream("alibaba-reasoning"));

        var parts = await ReadStream(model, UpstreamChat.Prompt());

        AssertStreamSnapshot(parts, "alibaba-reasoning");
    }
}
