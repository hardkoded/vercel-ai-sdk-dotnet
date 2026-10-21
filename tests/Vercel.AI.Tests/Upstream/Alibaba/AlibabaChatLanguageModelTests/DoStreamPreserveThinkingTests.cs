// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Alibaba.AlibabaChatLanguageModelTests.AlibabaChatTestSupport;

namespace Vercel.AI.Tests.Upstream.Alibaba.AlibabaChatLanguageModelTests;

/// <summary>Port of <c>alibaba-chat-language-model.test.ts</c> &gt; <c>doStream &gt; preserveThinking</c>.</summary>
public sealed class DoStreamPreserveThinkingTests
{
    [Fact]
    [UpstreamTest("packages/alibaba/src/alibaba-chat-language-model.test.ts::doStream > preserveThinking::should send explicit preserve_thinking false and omit historical reasoning", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_explicit_preserve_thinking_false_and_omit_historical_reasoning()
    {
        var handler = FixtureStream("alibaba-reasoning");
        var model = StreamModel(handler);

        await ReadStream(
            model,
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[]
                {
                    new UserModelMessage("Think before answering."),
                    new AssistantModelMessage(new AssistantContentPart[]
                    {
                        new AssistantReasoningPart("Hidden reasoning."),
                        new AssistantTextPart("Visible answer."),
                    }),
                    new UserModelMessage("Continue."),
                },
                ProviderOptions = AlibabaOptions("{\"preserveThinking\":false}"),
            });

        var body = JsonNode.Parse(handler.Body)!.AsObject();
        Assert.False(body["preserve_thinking"]!.GetValue<bool>());
        AssertJson(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Think before answering.\"}]},{\"role\":\"assistant\",\"content\":\"Visible answer.\"},{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Continue.\"}]}]",
            body["messages"]);
    }
}
