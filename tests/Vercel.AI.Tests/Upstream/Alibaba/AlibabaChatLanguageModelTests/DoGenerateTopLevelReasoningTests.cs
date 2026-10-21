// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using static Vercel.AI.Tests.Upstream.Alibaba.AlibabaChatLanguageModelTests.AlibabaChatTestSupport;

namespace Vercel.AI.Tests.Upstream.Alibaba.AlibabaChatLanguageModelTests;

/// <summary>Port of <c>alibaba-chat-language-model.test.ts</c> &gt; <c>doGenerate &gt; top-level reasoning</c>.</summary>
public sealed class DoGenerateTopLevelReasoningTests
{
    private const string Upstream = "packages/alibaba/src/alibaba-chat-language-model.test.ts::doGenerate > top-level reasoning::";

    [Fact]
    [UpstreamTest(Upstream + "should map top-level reasoning to enable_thinking true with budget", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_top_level_reasoning_to_enable_thinking_true_with_budget()
    {
        var capture = CaptureFixture("alibaba-text");
        var model = Model(capture);
        var options = UpstreamChat.Prompt();
        options.Reasoning = "high";

        await model.DoGenerateAsync(options, CancellationToken.None);

        var body = Body(capture);
        Assert.True(body["enable_thinking"]!.GetValue<bool>());
        Assert.True(body["thinking_budget"]!.GetValue<int>() > 0);
    }

    [Fact]
    [UpstreamTest(Upstream + "should map top-level reasoning none to enable_thinking false", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_top_level_reasoning_none_to_enable_thinking_false()
    {
        var capture = CaptureFixture("alibaba-text");
        var model = Model(capture);
        var options = UpstreamChat.Prompt();
        options.Reasoning = "none";

        await model.DoGenerateAsync(options, CancellationToken.None);

        var body = Body(capture);
        Assert.False(body["enable_thinking"]!.GetValue<bool>());
        Assert.Null(body["thinking_budget"]);
    }

    [Fact]
    [UpstreamTest(Upstream + "should prefer providerOptions over top-level reasoning", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_prefer_providerOptions_over_top_level_reasoning()
    {
        var capture = CaptureFixture("alibaba-text");
        var model = Model(capture);
        var options = UpstreamChat.Prompt();
        options.Reasoning = "none";
        options.ProviderOptions = AlibabaOptions("{\"enableThinking\":true}");

        await model.DoGenerateAsync(options, CancellationToken.None);

        Assert.True(Body(capture)["enable_thinking"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest(Upstream + "should not set thinking when reasoning is not specified", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_set_thinking_when_reasoning_is_not_specified()
    {
        var capture = CaptureFixture("alibaba-text");
        var model = Model(capture);

        await model.DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);

        var body = Body(capture);
        Assert.Null(body["enable_thinking"]);
        Assert.Null(body["thinking_budget"]);
    }
}
