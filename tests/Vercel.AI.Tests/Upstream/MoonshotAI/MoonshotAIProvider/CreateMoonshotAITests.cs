// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Moonshot;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIProvider;

/// <summary>Port of <c>moonshotai-provider.test.ts</c> &gt; <c>MoonshotAIProvider</c> &gt; <c>createMoonshotAI</c>.</summary>
[Collection(MoonshotAIEnvironmentCollection.Name)]
public sealed class CreateMoonshotAITests
{
    private const string Prefix =
        "packages/moonshotai/src/moonshotai-provider.test.ts::MoonshotAIProvider > createMoonshotAI::";

    [Fact]
    [UpstreamTest(
        Prefix + "should create a MoonshotAIProvider instance with default options",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_create_a_MoonshotAIProvider_instance_with_default_options()
    {
        var saved = Environment.GetEnvironmentVariable(MoonshotProvider.ApiKeyVariable);
        try
        {
            Environment.SetEnvironmentVariable(MoonshotProvider.ApiKeyVariable, "mock-api-key");
            var provider = MoonshotProvider.Create();
            provider.LanguageModel("model-id");

            Assert.Equal("MOONSHOT_API_KEY", provider.Options.ApiKeyEnvironmentVariable);
            var headers = await provider.CreateHeadersAsync();
            Assert.Equal("Bearer mock-api-key", headers["Authorization"]);

            Environment.SetEnvironmentVariable(MoonshotProvider.ApiKeyVariable, null);
            var error = await Assert.ThrowsAsync<AiSdkException>(() =>
                provider.CreateHeadersAsync()
            );
            Assert.Contains("MOONSHOT_API_KEY", error.Message);
        }
        finally
        {
            Environment.SetEnvironmentVariable(MoonshotProvider.ApiKeyVariable, saved);
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should create a MoonshotAIProvider instance with custom options",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_create_a_MoonshotAIProvider_instance_with_custom_options()
    {
        var options = new OpenAICompatibleOptions
        {
            ApiKey = "custom-key",
            BaseUrl = "https://custom.url",
        };
        options.Headers["Custom-Header"] = "value";
        var provider = MoonshotProvider.Create(options);
        provider.LanguageModel("model-id");

        var headers = await provider.CreateHeadersAsync();
        Assert.Equal("Bearer custom-key", headers["Authorization"]);
        Assert.Equal("value", headers["Custom-Header"]);
        Assert.Equal("MOONSHOT_API_KEY", provider.Options.ApiKeyEnvironmentVariable);
        Assert.Equal("https://custom.url", provider.Options.BaseUrl);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return a chat model when called as a function",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_return_a_chat_model_when_called_as_a_function()
    {
        var provider = MoonshotProvider.Create();

        var model = provider.LanguageModel("kimi-k2.5");

        Assert.IsType<OpenAICompatibleLanguageModel>(model);
    }

    [Theory]
    [InlineData("moonshot-v1-auto")]
    [InlineData("moonshot-v1-8k-vision-preview")]
    [InlineData("moonshot-v1-32k-vision-preview")]
    [InlineData("moonshot-v1-128k-vision-preview")]
    [UpstreamTest(
        Prefix + "should create a model for official model ID %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_create_a_model_for_official_model_ID(string modelId)
    {
        var provider = MoonshotProvider.Create();

        Assert.Equal(modelId, provider.LanguageModel(modelId).ModelId);
    }
}
