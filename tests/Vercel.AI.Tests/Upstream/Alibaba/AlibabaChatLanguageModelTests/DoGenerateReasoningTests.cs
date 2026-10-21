// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Alibaba.AlibabaChatLanguageModelTests.AlibabaChatTestSupport;

namespace Vercel.AI.Tests.Upstream.Alibaba.AlibabaChatLanguageModelTests;

/// <summary>Port of <c>alibaba-chat-language-model.test.ts</c> &gt; <c>doGenerate &gt; reasoning</c>.</summary>
public sealed class DoGenerateReasoningTests
{
    private const string Upstream = "packages/alibaba/src/alibaba-chat-language-model.test.ts::doGenerate > reasoning::";

    [Fact]
    [UpstreamTest(Upstream + "should extract reasoning content", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_reasoning_content()
    {
        var capture = CaptureFixture("alibaba-reasoning");
        var message = JsonNode.Parse(capture.ResponseBody)!["choices"]![0]!["message"]!;
        var model = Model(capture);

        var result = await model.DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);

        Assert.Collection(
            result.Content,
            part => Assert.Equal(message["reasoning_content"]!.GetValue<string>(), Assert.IsType<GeneratedReasoning>(part).Text),
            part => Assert.Equal(message["content"]!.GetValue<string>(), Assert.IsType<GeneratedText>(part).Text));
        AssertResultSnapshot(result, capture, "alibaba-reasoning", "stop", FinishReason.Stop, "chatcmpl-296c9a8c-4984-9b34-9b83-844f6208995e", new DateTimeOffset(2026, 2, 10, 23, 8, 22, TimeSpan.Zero), 24, 1668, 1353);
    }

    [Fact]
    [UpstreamTest(Upstream + "should extract usage with reasoning tokens", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_usage_with_reasoning_tokens()
    {
        var capture = CaptureFixture("alibaba-reasoning");
        var model = Model(capture);

        var usage = (await model.DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None)).Usage;

        Assert.Equal(24, usage.InputTokens);
        Assert.Equal(0, usage.CacheReadTokens);
        Assert.Equal(0, usage.CacheWriteTokens);
        Assert.Equal(24, usage.NoCacheInputTokens);
        Assert.Equal(1668, usage.OutputTokens);
        Assert.Equal(1353, usage.ReasoningTokens);
        Assert.Equal(315, usage.TextTokens);
        AssertJson("{\"completion_tokens\":1668,\"completion_tokens_details\":{\"reasoning_tokens\":1353},\"prompt_tokens\":24,\"prompt_tokens_details\":{\"cached_tokens\":0},\"total_tokens\":1692}", JsonNode.Parse(usage.Raw!.Value.GetRawText()));
    }

    [Fact]
    [UpstreamTest(Upstream + "should omit a reasoning-only response from a follow-up request", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_omit_a_reasoning_only_response_from_a_follow_up_request()
    {
        var capture = new UpstreamCapture
        {
            ResponseBody = "{\"id\":\"reasoning-only-response\",\"choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":null,\"reasoning_content\":\"Hidden reasoning.\"},\"finish_reason\":\"length\"}]}",
        };
        var model = Model(capture);
        var firstUserMessage = new UserModelMessage("Think before answering.");

        var first = await model.DoGenerateAsync(new LanguageModelCallOptions { Prompt = new ModelMessage[] { firstUserMessage } }, CancellationToken.None);
        var reasoningContent = first.Content.Where(part => part is GeneratedReasoning).ToList();

        Assert.Equal("Hidden reasoning.", Assert.IsType<GeneratedReasoning>(Assert.Single(reasoningContent)).Text);

        await model.DoGenerateAsync(
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[]
                {
                    firstUserMessage,
                    AssistantModelMessage.FromGeneratedContent(reasoningContent),
                    new UserModelMessage("Continue."),
                },
            },
            CancellationToken.None);

        AssertJson(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Think before answering.\"}]},{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Continue.\"}]}]",
            Body(capture, 1)["messages"]);
    }
}
