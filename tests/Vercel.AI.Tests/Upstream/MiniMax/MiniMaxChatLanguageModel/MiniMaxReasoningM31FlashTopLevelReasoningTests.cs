// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;
using MiniMaxModel = Vercel.AI.MiniMax.MiniMaxChatLanguageModel;

namespace Vercel.AI.Tests.MiniMaxChatLanguageModel;

/// <summary>Port of <c>minimax-chat-language-model.test.ts</c> &gt; <c>MiniMax reasoning &gt; M3.1 Flash top-level reasoning</c>.</summary>
public sealed class MiniMaxReasoningM31FlashTopLevelReasoningTests
{
    private readonly MiniMaxServer _server = new MiniMaxServer();

    public MiniMaxReasoningM31FlashTopLevelReasoningTests()
    {
        _server.SetJson("{\"type\":\"message\",\"content\":[{\"type\":\"text\",\"text\":\"Hello!\"}],\"stop_reason\":\"end_turn\",\"usage\":{\"input_tokens\":4,\"output_tokens\":10}}");
    }

    [Theory]
    [InlineData("minimal", "low")]
    [InlineData("low", "low")]
    [InlineData("medium", "medium")]
    [InlineData("high", "high")]
    [InlineData("xhigh", "xhigh")]
    [InlineData("max", "max")]
    [InlineData("none", "low")]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax reasoning > M3.1 Flash top-level reasoning::maps %s to effort %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_reasoning_to_effort(string reasoning, string effort)
    {
        var options = MiniMaxParity.Hello();
        options.Reasoning = reasoning;
        options.MaxOutputTokens = 16384;
        options.Temperature = 1;
        options.TopP = 0.95;

        var result = await MiniMaxParity.Provider(_server).Chat("minimax-m3.1-flash").DoGenerateAsync(options, CancellationToken.None);

        var body = _server.Calls[0].Body;
        MiniMaxParity.AssertMatch(body, "{\"output_config\":{\"effort\":\"" + effort + "\"},\"max_tokens\":16384,\"temperature\":1,\"top_p\":0.95}");
        Assert.Null(body["thinking"]);
        if (reasoning == "minimal" || reasoning == "none")
        {
            var warning = Assert.Single(result.Warnings);
            Assert.Equal("compatibility", warning.Type);
            Assert.Equal("reasoning", warning.Feature);
            Assert.Contains("low", warning.Message);
        }
        else
        {
            Assert.Empty(result.Warnings);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("provider-default")]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax reasoning > M3.1 Flash top-level reasoning::preserves the server default for %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_the_server_default(string? reasoning)
    {
        var options = MiniMaxParity.Hello();
        options.Reasoning = reasoning;
        options.MaxOutputTokens = 16384;

        await MiniMaxParity.Provider(_server).Chat("minimax-m3.1-flash").DoGenerateAsync(options, CancellationToken.None);

        var body = _server.Calls[0].Body;
        Assert.Null(body["output_config"]);
        Assert.Null(body["thinking"]);
    }

    [Theory]
    [InlineData("minimax")]
    [InlineData("anthropic")]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax reasoning > M3.1 Flash top-level reasoning::preserves explicit %s effort over top-level reasoning", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_explicit_effort_over_top_level_reasoning(string providerNamespace)
    {
        var options = MiniMaxParity.Hello();
        options.Reasoning = "none";
        options.MaxOutputTokens = 16384;
        options.ProviderOptions = MiniMaxParity.Options(providerNamespace, "{\"effort\":\"max\"}");

        var result = await MiniMaxParity.Provider(_server).Chat("minimax-m3.1-flash").DoGenerateAsync(options, CancellationToken.None);

        var body = _server.Calls[0].Body;
        JsonAssert.Equal(body["output_config"], "{\"effort\":\"max\"}");
        Assert.Null(body["thinking"]);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax reasoning > M3.1 Flash top-level reasoning::recognizes the canonical mixed-case model ID", Coverage = UpstreamCoverage.Covered)]
    public async Task Recognizes_the_canonical_mixed_case_model_ID()
    {
        var options = MiniMaxParity.Hello();
        options.Reasoning = "high";
        options.MaxOutputTokens = 16384;

        await MiniMaxParity.Provider(_server).Chat("MiniMax-M3.1-Flash").DoGenerateAsync(options, CancellationToken.None);

        MiniMaxParity.AssertMatch(_server.Calls[0].Body, "{\"model\":\"MiniMax-M3.1-Flash\",\"output_config\":{\"effort\":\"high\"}}");
    }

    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax reasoning > M3.1 Flash top-level reasoning::preserves MiniMax reasoning handling after workflow serialization", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_MiniMax_reasoning_handling_after_workflow_serialization()
    {
        var model = MiniMaxParity.Provider(_server).Chat("minimax-m3.1-flash");
        var serialized = model.Serialize();
        var restored = MiniMaxModel.Deserialize(serialized, "test-api-key", _server);

        var options = MiniMaxParity.Hello();
        options.Reasoning = "high";
        options.MaxOutputTokens = 16384;
        await restored.DoGenerateAsync(options, CancellationToken.None);

        Assert.IsType<MiniMaxModel>(restored);
        var body = _server.Calls[0].Body;
        JsonAssert.Equal(body["output_config"], "{\"effort\":\"high\"}");
        Assert.Null(body["thinking"]);
    }

    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax reasoning > M3.1 Flash top-level reasoning::does not change reasoning handling for M3", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_change_reasoning_handling_for_M3()
    {
        var options = MiniMaxParity.Hello();
        options.Reasoning = "none";
        options.MaxOutputTokens = 16384;

        await MiniMaxParity.Provider(_server).Chat("minimax-m3").DoGenerateAsync(options, CancellationToken.None);

        var body = _server.Calls[0].Body;
        JsonAssert.Equal(body["thinking"], "{\"type\":\"disabled\"}");
        Assert.Null(body["output_config"]);
    }

    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax reasoning > M3.1 Flash top-level reasoning::maps reasoning and reports fallback warnings when streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_reasoning_and_reports_fallback_warnings_when_streaming()
    {
        _server.SetStream(MiniMaxParity.StreamMessageStart, MiniMaxParity.StreamEnd);
        var options = MiniMaxParity.Hello();
        options.Reasoning = "none";
        options.MaxOutputTokens = 16384;

        var result = await MiniMaxParity.Provider(_server).Chat("minimax-m3.1-flash").DoStreamAsync(options, CancellationToken.None);
        var parts = await MiniMaxParity.Collect(result);

        var body = _server.Calls[0].Body;
        JsonAssert.Equal(body["output_config"], "{\"effort\":\"low\"}");
        Assert.Null(body["thinking"]);
        var start = Assert.Single(parts.OfType<StreamStartStreamPart>());
        var warning = Assert.Single(start.Warnings);
        Assert.Equal("compatibility", warning.Type);
        Assert.Equal("reasoning", warning.Feature);
        Assert.DoesNotContain(parts, part => part is ErrorStreamPart);
    }
}
