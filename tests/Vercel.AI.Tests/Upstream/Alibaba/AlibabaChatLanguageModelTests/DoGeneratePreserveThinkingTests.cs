// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Alibaba.AlibabaChatLanguageModelTests.AlibabaChatTestSupport;

namespace Vercel.AI.Tests.Upstream.Alibaba.AlibabaChatLanguageModelTests;

/// <summary>Port of <c>alibaba-chat-language-model.test.ts</c> &gt; <c>doGenerate &gt; preserveThinking</c>.</summary>
public sealed class DoGeneratePreserveThinkingTests
{
    private const string Upstream = "packages/alibaba/src/alibaba-chat-language-model.test.ts::doGenerate > preserveThinking::";

    private const string ThinkMessage = "{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Think before answering.\"}]}";

    private const string ContinueMessage = "{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Continue.\"}]}";

    private static IReadOnlyList<ModelMessage> ReasoningHistory()
    {
        return new ModelMessage[]
        {
            new UserModelMessage("Think before answering."),
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantReasoningPart("Hidden reasoning."),
                new AssistantTextPart("Visible answer."),
            }),
            new UserModelMessage("Continue."),
        };
    }

    [Fact]
    [UpstreamTest(Upstream + "should send preserve_thinking and replay reasoning as reasoning_content", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_preserve_thinking_and_replay_reasoning_as_reasoning_content()
    {
        var capture = CaptureFixture("alibaba-reasoning");
        var model = Model(capture);

        await model.DoGenerateAsync(
            new LanguageModelCallOptions { Prompt = ReasoningHistory(), ProviderOptions = AlibabaOptions("{\"preserveThinking\":true}") },
            CancellationToken.None);

        var body = Body(capture);
        Assert.True(body["preserve_thinking"]!.GetValue<bool>());
        AssertJson("[" + ThinkMessage + ",{\"role\":\"assistant\",\"content\":\"Visible answer.\",\"reasoning_content\":\"Hidden reasoning.\"}," + ContinueMessage + "]", body["messages"]);
    }

    [Fact]
    [UpstreamTest(Upstream + "should omit preserve_thinking when the option is not set", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_omit_preserve_thinking_when_the_option_is_not_set()
    {
        var capture = CaptureFixture("alibaba-reasoning");
        var model = Model(capture);

        await model.DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);

        Assert.False(Body(capture).ContainsKey("preserve_thinking"));
    }

    [Fact]
    [UpstreamTest(Upstream + "should still map top-level reasoning when only preserveThinking is set", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_still_map_top_level_reasoning_when_only_preserveThinking_is_set()
    {
        var capture = CaptureFixture("alibaba-reasoning");
        var model = Model(capture);
        var options = UpstreamChat.Prompt();
        options.Reasoning = "high";
        options.ProviderOptions = AlibabaOptions("{\"preserveThinking\":true}");

        await model.DoGenerateAsync(options, CancellationToken.None);

        var body = Body(capture);
        Assert.True(body["enable_thinking"]!.GetValue<bool>());
        Assert.True(body["preserve_thinking"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest(Upstream + "should default preserve_thinking to true for supported models", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_default_preserve_thinking_to_true_for_supported_models()
    {
        var capture = CaptureFixture("alibaba-reasoning");
        var supportedModel = Model(capture, "qwen3.7-max");

        await supportedModel.DoGenerateAsync(new LanguageModelCallOptions { Prompt = ReasoningHistory() }, CancellationToken.None);

        var body = Body(capture);
        Assert.True(body["preserve_thinking"]!.GetValue<bool>());
        AssertJson("[" + ThinkMessage + ",{\"role\":\"assistant\",\"content\":\"Visible answer.\",\"reasoning_content\":\"Hidden reasoning.\"}," + ContinueMessage + "]", body["messages"]);
    }

    [Fact]
    [UpstreamTest(Upstream + "should let supported models opt out with preserveThinking false", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_let_supported_models_opt_out_with_preserveThinking_false()
    {
        var capture = CaptureFixture("alibaba-reasoning");
        var supportedModel = Model(capture, "qwen3.7-max");

        await supportedModel.DoGenerateAsync(
            new LanguageModelCallOptions { Prompt = ReasoningHistory(), ProviderOptions = AlibabaOptions("{\"preserveThinking\":false}") },
            CancellationToken.None);

        var body = Body(capture);
        Assert.False(body["preserve_thinking"]!.GetValue<bool>());
        AssertJson("[" + ThinkMessage + ",{\"role\":\"assistant\",\"content\":\"Visible answer.\"}," + ContinueMessage + "]", body["messages"]);
    }

    [Fact]
    [UpstreamTest(Upstream + "should keep same-round reasoning with tool calls even when preserveThinking is false", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_keep_same_round_reasoning_with_tool_calls_even_when_preserveThinking_is_false()
    {
        var capture = CaptureFixture("alibaba-reasoning");
        var model = Model(capture);
        var prompt = new ModelMessage[]
        {
            new UserModelMessage("Weather in San Francisco?"),
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantReasoningPart("Need to check the weather."),
                new AssistantToolCallPart(new GeneratedToolCall("call-1", "get_weather", "{\"location\":\"San Francisco\"}")),
            }),
            new ToolModelMessage("call-1", "get_weather", "Sunny, 72F.", false, "text"),
        };

        await model.DoGenerateAsync(
            new LanguageModelCallOptions { Prompt = prompt, ProviderOptions = AlibabaOptions("{\"preserveThinking\":false}") },
            CancellationToken.None);

        var messages = Body(capture)["messages"]!.AsArray();
        Assert.Contains(
            messages,
            message => JsonNode.DeepEquals(
                message,
                JsonNode.Parse("{\"role\":\"assistant\",\"content\":null,\"reasoning_content\":\"Need to check the weather.\",\"tool_calls\":[{\"id\":\"call-1\",\"type\":\"function\",\"function\":{\"name\":\"get_weather\",\"arguments\":\"{\\\"location\\\":\\\"San Francisco\\\"}\"}}]}")));
    }
}
