// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.MiniMax;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class MiniMaxUpstreamTests
{
    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-provider.test.ts::MiniMaxProvider > createMiniMax::should create a MiniMaxProvider instance with default options", Coverage = UpstreamCoverage.Covered)]
    public async Task Default_chat_uses_the_openai_compatible_endpoint()
    {
        var capture = new UpstreamCapture();
        var provider = MiniMaxProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture);
        await ((OpenAICompatibleLanguageModel)provider.LanguageModel("minimax-m3")).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        Assert.Equal("MINIMAX_API_KEY", provider.Options.ApiKeyEnvironmentVariable);
        Assert.Equal("https://api.minimax.io/v1/chat/completions", capture.Requests[0].Uri!.GetLeftPart(UriPartial.Path));
        Assert.Equal("Bearer secret", capture.Requests[0].Headers["Authorization"]);
        Assert.False(capture.Requests[0].Headers.ContainsKey("x-api-key"));
    }

    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-provider.test.ts::MiniMaxProvider > createMiniMax::should create a MiniMaxProvider instance with custom options", Coverage = UpstreamCoverage.Covered)]
    public async Task Custom_key_base_url_and_headers_are_kept()
    {
        var capture = new UpstreamCapture();
        var provider = MiniMaxProvider.Create(
            new OpenAICompatibleOptions { ApiKey = "custom-key", BaseUrl = "https://custom.minimax.test/v1" },
            capture);
        provider.Options.Headers["Custom-Header"] = "value";
        await ((OpenAICompatibleLanguageModel)provider.LanguageModel("minimax-m3")).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        Assert.Equal("https://custom.minimax.test/v1/chat/completions", capture.Requests[0].Uri!.GetLeftPart(UriPartial.Path));
        Assert.Equal("Bearer custom-key", capture.Requests[0].Headers["Authorization"]);
        Assert.Equal("value", capture.Requests[0].Headers["Custom-Header"]);
    }

    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-provider.test.ts::MiniMaxProvider > unsupported model types::should throw NoSuchModelError for embeddingModel", Coverage = UpstreamCoverage.Covered)]
    public void Embeddings_are_rejected()
    {
        var error = Assert.Throws<AiSdkException>(() => MiniMaxProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).EmbeddingModel("embed"));
        Assert.Equal("Provider 'minimax' does not support embedding model 'embed'.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-provider.test.ts::MiniMaxProvider > unsupported model types::should throw NoSuchModelError for imageModel", Coverage = UpstreamCoverage.Covered)]
    public void Images_are_rejected()
    {
        var error = Assert.Throws<AiSdkException>(() => MiniMaxProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).ImageModel("image"));
        Assert.Equal("Provider 'minimax' does not support image model 'image'.", error.Message);
    }
}
