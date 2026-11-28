// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIChatLanguageModel.MoonshotLanguageModelSupportA;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIChatLanguageModel;

/// <summary>
/// Port of <c>moonshotai-chat-language-model.test.ts</c> &gt; <c>doGenerate</c> &gt; <c>provider options passthrough</c>.
/// </summary>
public sealed class DoGenerateProviderOptionsPassthroughTests
{
    private const string Prefix =
        MoonshotLanguageModelSupportA.Id + "provider options passthrough::";

    [Fact]
    [UpstreamTest(
        Prefix + "should forward string predicted content unchanged",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_forward_string_predicted_content_unchanged()
    {
        var (model, handler) = Model("kimi-k3");
        var call = TestPrompt();
        call.ProviderOptions = Opts(
            "{\"moonshotai\":{\"prediction\":{\"type\":\"content\",\"content\":\"Hello, world!\"}}}"
        );

        await model.DoGenerateAsync(call, CancellationToken.None);

        Strict(Body(handler)["prediction"], "{\"type\":\"content\",\"content\":\"Hello, world!\"}");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit prediction when it is not configured",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_prediction_when_it_is_not_configured()
    {
        var (model, handler) = Model("kimi-k3");

        await model.DoGenerateAsync(TestPrompt(), CancellationToken.None);

        Assert.False(Body(handler).ContainsKey("prediction"));
    }

    [Theory]
    [InlineData("{\"type\":\"text\",\"content\":\"Hello, world!\"}")]
    [InlineData(
        "{\"type\":\"content\",\"content\":[{\"type\":\"image\",\"text\":\"Hello, world!\"}]}"
    )]
    [InlineData("{\"type\":\"content\",\"content\":123}")]
    [UpstreamTest(
        Prefix + "should reject invalid predicted content before making a request",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reject_invalid_predicted_content_before_making_a_request(
        string prediction
    )
    {
        var (model, handler) = Model("kimi-k3");
        var call = TestPrompt();
        call.ProviderOptions = Opts("{\"moonshotai\":{\"prediction\":" + prediction + "}}");

        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            model.DoGenerateAsync(call, CancellationToken.None)
        );

        Assert.Equal("providerOptions", error.Parameter);
        Assert.Contains("invalid moonshotai provider options", error.Message);
        Assert.Empty(handler.Bodies);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should forward promptCacheKey and safetyIdentifier",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_forward_promptCacheKey_and_safetyIdentifier()
    {
        var (model, handler) = Model("kimi-k3");
        var call = TestPrompt();
        call.ProviderOptions = Opts(
            "{\"moonshotai\":{\"promptCacheKey\":\"session-42\",\"safetyIdentifier\":\"user-hash-7\"}}"
        );

        await model.DoGenerateAsync(call, CancellationToken.None);

        var body = Body(handler);
        Assert.Equal("session-42", (string?)body["prompt_cache_key"]);
        Assert.Equal("user-hash-7", (string?)body["safety_identifier"]);
    }
}
