// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Moonshot;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class MoonshotUpstreamTests
{
    [Fact]
    [UpstreamTest("packages/moonshotai/src/moonshotai-provider.test.ts::MoonshotAIProvider > createMoonshotAI::should create a MoonshotAIProvider instance with default options", Coverage = UpstreamCoverage.Covered)]
    public async Task Default_options_target_the_moonshot_api()
    {
        var capture = new UpstreamCapture();
        var provider = MoonshotProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture);
        await ((OpenAICompatibleLanguageModel)provider.LanguageModel("kimi-k2.5")).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        Assert.Equal("https://api.moonshot.ai/v1/chat/completions", capture.Requests[0].Uri!.GetLeftPart(UriPartial.Path));
        Assert.Equal("Bearer secret", capture.Requests[0].Headers["Authorization"]);
        Assert.Contains("ai-sdk/moonshotai/" + AiSdkVersion.Version, capture.Requests[0].Headers["User-Agent"]);
        Assert.Equal("MOONSHOT_API_KEY", provider.Options.ApiKeyEnvironmentVariable);
        Assert.True(provider.Options.IncludeUsage);
    }

    [Fact]
    [UpstreamTest("packages/moonshotai/src/moonshotai-provider.test.ts::MoonshotAIProvider > createMoonshotAI::should create a MoonshotAIProvider instance with custom options", Coverage = UpstreamCoverage.Covered)]
    public async Task Custom_base_url_is_kept()
    {
        var capture = new UpstreamCapture();
        var provider = MoonshotProvider.Create(new OpenAICompatibleOptions { ApiKey = "custom", BaseUrl = "https://custom.moonshot.test/v1" }, capture);
        await ((OpenAICompatibleLanguageModel)provider.LanguageModel("kimi-k2.5")).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        Assert.Equal("https://custom.moonshot.test/v1/chat/completions", capture.Requests[0].Uri!.GetLeftPart(UriPartial.Path));
        Assert.Equal("Bearer custom", capture.Requests[0].Headers["Authorization"]);
    }

    [Fact]
    [UpstreamTest("packages/moonshotai/src/moonshotai-provider.test.ts::MoonshotAIProvider > createMoonshotAI::should return a chat model when called as a function", Coverage = UpstreamCoverage.Covered)]
    public void Language_model_is_a_chat_model()
    {
        var model = (OpenAICompatibleLanguageModel)MoonshotProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).LanguageModel("kimi-k2.5");
        Assert.Equal("moonshotai.chat", model.Provider);
        Assert.True(model.SupportsStructuredOutputs);
    }

    [Fact]
    [UpstreamTest("packages/moonshotai/src/moonshotai-provider.test.ts::getMoonshotAILanguageModelCapabilities::supportsStructuredOutputs for %s is %s", Coverage = UpstreamCoverage.Covered)]
    public void Structured_output_support_matches_the_model_list()
    {
        Assert.True(MoonshotProvider.SupportsStructuredOutputs("kimi-k2.5"));
        Assert.True(MoonshotProvider.SupportsStructuredOutputs("kimi-k2.6"));
        Assert.True(MoonshotProvider.SupportsStructuredOutputs("kimi-k2.7-code"));
        Assert.True(MoonshotProvider.SupportsStructuredOutputs("kimi-k3"));
        Assert.True(MoonshotProvider.SupportsStructuredOutputs("moonshot-v1-8k"));
        Assert.True(MoonshotProvider.SupportsStructuredOutputs("moonshot-v1-32k"));
        Assert.True(MoonshotProvider.SupportsStructuredOutputs("moonshot-v1-128k"));
        Assert.True(MoonshotProvider.SupportsStructuredOutputs("moonshot-v1-auto"));
        Assert.True(MoonshotProvider.SupportsStructuredOutputs("moonshot-v1-8k-vision-preview"));
        Assert.True(MoonshotProvider.SupportsStructuredOutputs("moonshot-v1-32k-vision-preview"));
        Assert.True(MoonshotProvider.SupportsStructuredOutputs("moonshot-v1-128k-vision-preview"));
        Assert.True(MoonshotProvider.SupportsStructuredOutputs("moonshot-v1-custom"));
        Assert.True(MoonshotProvider.SupportsStructuredOutputs("custom-model-id"));
    }
}
