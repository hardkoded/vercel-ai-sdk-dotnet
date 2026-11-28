// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.MiniMax;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.MiniMaxProvider;

/// <summary>Port of <c>minimax-provider.test.ts</c> &gt; <c>MiniMaxProvider &gt; createMiniMax</c>.</summary>
public sealed class CreateMiniMaxTests
{
    [Fact]
    [UpstreamTest(
        "packages/minimax/src/minimax-provider.test.ts::MiniMaxProvider > createMiniMax::should create a MiniMaxProvider instance with default options",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_create_a_MiniMaxProvider_instance_with_default_options()
    {
        var server = new MiniMaxServer();
        var provider = Vercel.AI.MiniMax.MiniMaxProvider.Create(
            new MiniMaxOptions { ApiKey = "secret" },
            server
        );

        await provider
            .LanguageModel("minimax-m3")
            .DoGenerateAsync(MiniMaxParity.Hello(), CancellationToken.None);

        Assert.Equal("MINIMAX_API_KEY", Vercel.AI.MiniMax.MiniMaxProvider.ApiKeyVariable);
        Assert.Equal(MiniMaxServer.Url, server.Calls[0].Url);
        Assert.Equal("secret", server.Calls[0].Headers["x-api-key"]);
        Assert.Equal("2023-06-01", server.Calls[0].Headers["anthropic-version"]);
        Assert.False(server.Calls[0].Headers.ContainsKey("Authorization"));
    }

    [Fact]
    [UpstreamTest(
        "packages/minimax/src/minimax-provider.test.ts::MiniMaxProvider > createMiniMax::should create a MiniMaxProvider instance with custom options",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_create_a_MiniMaxProvider_instance_with_custom_options()
    {
        var server = new MiniMaxServer();
        var options = new MiniMaxOptions
        {
            ApiKey = "custom-key",
            BaseUrl = "https://custom.url/anthropic/v1",
        };
        options.Headers["Custom-Header"] = "value";
        var provider = Vercel.AI.MiniMax.MiniMaxProvider.Create(options, server);

        await provider
            .LanguageModel("minimax-m3")
            .DoGenerateAsync(MiniMaxParity.Hello(), CancellationToken.None);

        Assert.Equal("https://custom.url/anthropic/v1/messages", server.Calls[0].Url);
        Assert.Equal("custom-key", server.Calls[0].Headers["x-api-key"]);
        Assert.Equal("value", server.Calls[0].Headers["Custom-Header"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/minimax/src/minimax-provider.test.ts::MiniMaxProvider > createMiniMax::should return a chat model when called as a function",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_return_a_chat_model_when_called_as_a_function()
    {
        var provider = Vercel.AI.MiniMax.MiniMaxProvider.Create();

        // The provider is not callable in C#; LanguageModel is the equivalent of provider('...').
        var model = provider.LanguageModel("minimax-m3");

        Assert.IsAssignableFrom<Vercel.AI.Anthropic.AnthropicLanguageModel>(model);
    }

    [Fact]
    [UpstreamTest(
        "packages/minimax/src/minimax-provider.test.ts::MiniMaxProvider > createMiniMax::should construct the model with the Anthropic-compatible config",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_construct_the_model_with_the_Anthropic_compatible_config()
    {
        var server = new MiniMaxServer();
        var provider = Vercel.AI.MiniMax.MiniMaxProvider.Create(
            new MiniMaxOptions { ApiKey = "mock-api-key" },
            server
        );

        var model = provider.LanguageModel("minimax-m3");
        await model.DoGenerateAsync(MiniMaxParity.Hello(), CancellationToken.None);

        Assert.Equal("minimax-m3", model.ModelId);
        // provider prefix `minimax` is the provider-options namespace
        Assert.Equal("minimax.messages", model.Provider);
        Assert.Equal("https://api.minimax.io/anthropic/v1/messages", server.Calls[0].Url);
        Assert.Equal("mock-api-key", server.Calls[0].Headers["x-api-key"]);
        Assert.Equal("2023-06-01", server.Calls[0].Headers["anthropic-version"]);
    }
}
