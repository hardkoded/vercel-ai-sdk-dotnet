// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIChatLanguageModel.MoonshotLanguageModelSupportA;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIChatLanguageModel;

/// <summary>
/// Port of <c>moonshotai-chat-language-model.test.ts</c> &gt; <c>doGenerate</c> &gt; <c>logprobs</c>.
/// </summary>
public sealed class DoGenerateLogprobsTests
{
    private const string Prefix = MoonshotLanguageModelSupportA.Id + "logprobs::";

    [Fact]
    [UpstreamTest(
        Prefix + "should send logprobs provider options",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_logprobs_provider_options()
    {
        var (model, handler) = Model("moonshot-v1-8k", "moonshotai-logprobs");
        var call = TestPrompt();
        call.ProviderOptions = Opts("{\"moonshotai\":{\"logprobs\":true,\"topLogprobs\":20}}");

        await model.DoGenerateAsync(call, CancellationToken.None);

        Match(Body(handler), "{\"logprobs\":true,\"top_logprobs\":20}");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should enable logprobs when topLogprobs is set",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_enable_logprobs_when_topLogprobs_is_set()
    {
        var (model, handler) = Model("moonshot-v1-8k", "moonshotai-logprobs");
        var call = TestPrompt();
        call.ProviderOptions = Opts("{\"moonshotai\":{\"topLogprobs\":0}}");

        await model.DoGenerateAsync(call, CancellationToken.None);

        Match(Body(handler), "{\"logprobs\":true,\"top_logprobs\":0}");
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("1.5")]
    [InlineData("21")]
    [UpstreamTest(
        Prefix + "should reject invalid topLogprobs value %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reject_invalid_topLogprobs_value(string topLogprobs)
    {
        var (model, handler) = Model("moonshot-v1-8k", "moonshotai-logprobs");
        var call = TestPrompt();
        call.ProviderOptions = Opts("{\"moonshotai\":{\"topLogprobs\":" + topLogprobs + "}}");

        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            model.DoGenerateAsync(call, CancellationToken.None)
        );

        Assert.Equal("providerOptions", error.Parameter);
        Assert.Contains("invalid moonshotai provider options", error.Message);
        Assert.Empty(handler.Bodies);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should expose logprobs in provider metadata without changing text",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_expose_logprobs_in_provider_metadata_without_changing_text()
    {
        var (model, _) = Model("moonshot-v1-8k", "moonshotai-logprobs");
        var call = TestPrompt();
        call.ProviderOptions = Opts("{\"moonshotai\":{\"logprobs\":true}}");

        var result = await model.DoGenerateAsync(call, CancellationToken.None);

        var text = Assert.Single(result.Content);
        Assert.Equal("OK!", Assert.IsType<GeneratedText>(text).Text);
        var metadata = JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())!;
        Strict(
            metadata["moonshotai"]!["logprobs"],
            "{\"content\":[{\"bytes\":[79,75],\"logprob\":-0.0004808938247151673,\"token\":\"OK\",\"top_logprobs\":[{\"bytes\":[79,75],\"logprob\":-0.0004808938247151673,\"token\":\"OK\"}]},{\"bytes\":null,\"logprob\":-0.01,\"token\":\"!\",\"top_logprobs\":[{\"bytes\":null,\"logprob\":-0.01,\"token\":\"!\"}]}]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should parse a null logprobs response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_parse_a_null_logprobs_response()
    {
        var response = JsonNode.Parse(Fixture("moonshotai-text"))!;
        response["choices"]![0]!["logprobs"] = null;
        var (model, _) = Model(
            "moonshot-v1-8k",
            new SequencedHandler().Json(response.ToJsonString())
        );

        var result = await model.DoGenerateAsync(TestPrompt(), CancellationToken.None);

        Strict(
            JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText()),
            "{\"moonshotai\":{\"responseObject\":\"chat.completion\",\"choiceIndex\":0,\"messageRole\":\"assistant\"}}"
        );
    }
}
