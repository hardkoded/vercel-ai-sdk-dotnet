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
    [UpstreamTest("packages/minimax/src/minimax-provider.test.ts::MiniMaxProvider > createMiniMax::should create a MiniMaxProvider instance with default options", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_create_a_MiniMaxProvider_instance_with_default_options()
    {
        var server = new MiniMaxServer();
        var provider = Vercel.AI.MiniMax.MiniMaxProvider.Create(new MiniMaxOptions { ApiKey = "secret" }, server);

        await provider.LanguageModel("minimax-m3").DoGenerateAsync(MiniMaxParity.Hello(), CancellationToken.None);

        Assert.Equal("MINIMAX_API_KEY", Vercel.AI.MiniMax.MiniMaxProvider.ApiKeyVariable);
        Assert.Equal(MiniMaxServer.Url, server.Calls[0].Url);
        Assert.Equal("secret", server.Calls[0].Headers["x-api-key"]);
        Assert.Equal("2023-06-01", server.Calls[0].Headers["anthropic-version"]);
        Assert.False(server.Calls[0].Headers.ContainsKey("Authorization"));
    }

    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-provider.test.ts::MiniMaxProvider > createMiniMax::should create a MiniMaxProvider instance with custom options", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_create_a_MiniMaxProvider_instance_with_custom_options()
    {
        var server = new MiniMaxServer();
        var options = new MiniMaxOptions { ApiKey = "custom-key", BaseUrl = "https://custom.url/anthropic/v1" };
        options.Headers["Custom-Header"] = "value";
        var provider = Vercel.AI.MiniMax.MiniMaxProvider.Create(options, server);

        await provider.LanguageModel("minimax-m3").DoGenerateAsync(MiniMaxParity.Hello(), CancellationToken.None);

        Assert.Equal("https://custom.url/anthropic/v1/messages", server.Calls[0].Url);
        Assert.Equal("custom-key", server.Calls[0].Headers["x-api-key"]);
        Assert.Equal("value", server.Calls[0].Headers["Custom-Header"]);
    }
}
