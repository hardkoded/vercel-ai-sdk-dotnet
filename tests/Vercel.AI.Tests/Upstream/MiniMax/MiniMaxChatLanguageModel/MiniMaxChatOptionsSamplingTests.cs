// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.MiniMaxChatLanguageModel;

/// <summary>Port of <c>minimax-chat-language-model.test.ts</c> &gt; <c>MiniMax chat options (%s)</c> &gt; <c>sampling</c>.</summary>
public sealed class MiniMaxChatOptionsSamplingTests
{
    private const string Adaptive = "{\"thinking\":{\"type\":\"adaptive\"}}";

    public static IEnumerable<object?[]> Temperatures() =>
        MiniMaxChatOptionsCall.Modes(new object?[] { 0.0 }, new object?[] { 0.5 }, new object?[] { 1.5 }, new object?[] { 2.0 });

    public static IEnumerable<object?[]> Namespaces() =>
        MiniMaxChatOptionsCall.Modes(new object?[] { "minimax" }, new object?[] { "anthropic" });

    public static IEnumerable<object?[]> Plain() => MiniMaxChatOptionsCall.Modes();

    public static IEnumerable<object?[]> Clamps() =>
        MiniMaxChatOptionsCall.Modes(new object?[] { -0.5, 0.0 }, new object?[] { 2.5, 2.0 });

    public static IEnumerable<object?[]> OlderModels() =>
        MiniMaxChatOptionsCall.Modes(new object?[] { "minimax-m3" }, new object?[] { "MiniMax-M2.7" });

    [Theory]
    [MemberData(nameof(Temperatures))]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax chat options (%s) > sampling::preserves temperature %s and topP with default thinking", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_temperature_and_topP_with_default_thinking(string mode, double temperature)
    {
        var (body, warnings, _) = await MiniMaxChatOptionsCall.Call(mode, new LanguageModelCallOptions { Temperature = temperature, TopP = 0.9 });

        Assert.Equal(temperature, body["temperature"]!.GetValue<double>());
        Assert.Equal(0.9, body["top_p"]!.GetValue<double>());
        Assert.Empty(warnings);
    }

    [Theory]
    [MemberData(nameof(Namespaces))]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax chat options (%s) > sampling::preserves sampling with explicit adaptive thinking in %s options", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_sampling_with_explicit_adaptive_thinking(string mode, string providerNamespace)
    {
        var options = new LanguageModelCallOptions
        {
            Temperature = 1.5,
            TopP = 0.9,
            ProviderOptions = MiniMaxParity.Options(providerNamespace, Adaptive),
        };

        var (body, warnings, _) = await MiniMaxChatOptionsCall.Call(mode, options);

        MiniMaxParity.AssertMatch(body, "{\"temperature\":1.5,\"top_p\":0.9,\"thinking\":{\"type\":\"adaptive\"}}");
        Assert.Empty(warnings);
    }

    [Theory]
    [MemberData(nameof(Plain))]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax chat options (%s) > sampling::preserves sampling with top-level reasoning and the canonical model ID", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_sampling_with_top_level_reasoning_and_the_canonical_model_ID(string mode)
    {
        var (body, warnings, _) = await MiniMaxChatOptionsCall.Call(
            mode,
            new LanguageModelCallOptions { Temperature = 2, TopP = 0.8, Reasoning = "high" },
            "MiniMax-M3.1-Flash");

        MiniMaxParity.AssertMatch(body, "{\"temperature\":2,\"top_p\":0.8,\"output_config\":{\"effort\":\"high\"}}");
        Assert.Empty(warnings);
    }

    [Theory]
    [MemberData(nameof(Plain))]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax chat options (%s) > sampling::leaves omitted sampling parameters unset with adaptive thinking", Coverage = UpstreamCoverage.Covered)]
    public async Task Leaves_omitted_sampling_parameters_unset_with_adaptive_thinking(string mode)
    {
        var (body, warnings, _) = await MiniMaxChatOptionsCall.Call(mode, new LanguageModelCallOptions { ProviderOptions = MiniMaxParity.Options("minimax", Adaptive) });

        Assert.Null(body["temperature"]);
        Assert.Null(body["top_p"]);
        Assert.Empty(warnings);
    }

    [Theory]
    [MemberData(nameof(Clamps))]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax chat options (%s) > sampling::clamps temperature $temperature to $expected", Coverage = UpstreamCoverage.Covered)]
    public async Task Clamps_temperature(string mode, double temperature, double expected)
    {
        var options = new LanguageModelCallOptions
        {
            Temperature = temperature,
            TopP = 0.9,
            ProviderOptions = MiniMaxParity.Options("minimax", Adaptive),
        };

        var (body, warnings, _) = await MiniMaxChatOptionsCall.Call(mode, options);

        Assert.Equal(expected, body["temperature"]!.GetValue<double>());
        Assert.Equal(0.9, body["top_p"]!.GetValue<double>());
        var warning = Assert.Single(warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("temperature", warning.Feature);
        Assert.Contains("minimax", warning.Message);
    }

    [Theory]
    [MemberData(nameof(OlderModels))]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax chat options (%s) > sampling::preserves existing sampling behavior for %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_existing_sampling_behavior(string mode, string modelId)
    {
        var (body, warnings, _) = await MiniMaxChatOptionsCall.Call(mode, new LanguageModelCallOptions { Temperature = 1.5 }, modelId);

        Assert.Equal(1, body["temperature"]!.GetValue<double>());
        Assert.Contains(warnings, warning => warning.Feature == "temperature");
    }

    [Theory]
    [MemberData(nameof(OlderModels))]
    [UpstreamTest("packages/minimax/src/minimax-chat-language-model.test.ts::MiniMax chat options (%s) > sampling::preserves sampling restrictions with adaptive thinking for %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_sampling_restrictions_with_adaptive_thinking(string mode, string modelId)
    {
        var options = new LanguageModelCallOptions
        {
            Temperature = 0.5,
            TopP = 0.9,
            ProviderOptions = MiniMaxParity.Options("minimax", Adaptive),
        };

        var (body, warnings, _) = await MiniMaxChatOptionsCall.Call(mode, options, modelId);

        Assert.Null(body["temperature"]);
        Assert.Null(body["top_p"]);
        Assert.Equal(new[] { "temperature", "topP" }, warnings.Select(warning => warning.Feature));
    }
}
