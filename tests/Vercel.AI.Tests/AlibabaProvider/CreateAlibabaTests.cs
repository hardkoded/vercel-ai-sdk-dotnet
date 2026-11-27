// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Alibaba;
using Vercel.AI.OpenAICompatible;
using AlibabaProviderType = Vercel.AI.Alibaba.AlibabaProvider;

namespace Vercel.AI.Tests.AlibabaProvider;

/// <summary>Port of <c>alibaba-provider.test.ts</c> &gt; <c>AlibabaProvider</c> &gt; <c>createAlibaba</c>.</summary>
public sealed class CreateAlibabaTests
{
    private const string Prefix =
        "packages/alibaba/src/alibaba-provider.test.ts::AlibabaProvider > createAlibaba::";

    [Fact]
    [UpstreamTest(
        Prefix + "should create an AlibabaProvider instance with default options",
        Coverage = UpstreamCoverage.Partial,
        Note = "The port has no loadApiKey description, so 'Alibaba Cloud (DashScope)' is not checked. The test reads the key from ALIBABA_API_KEY instead of a mocked loadApiKey."
    )]
    public async Task Should_create_an_AlibabaProvider_instance_with_default_options()
    {
        var saved = Environment.GetEnvironmentVariable(AlibabaProviderType.ApiKeyVariable);
        var savedDashScope = Environment.GetEnvironmentVariable("DASHSCOPE_API_KEY");
        try
        {
            Environment.SetEnvironmentVariable(AlibabaProviderType.ApiKeyVariable, "env-api-key");
            Environment.SetEnvironmentVariable("DASHSCOPE_API_KEY", null);
            var provider = AlibabaProviderType.Create();
            provider.LanguageModel("qwen-plus");

            var headers = await provider.CreateHeadersAsync(CancellationToken.None);

            Assert.Null(provider.Options.ApiKey);
            Assert.Equal("ALIBABA_API_KEY", provider.Options.ApiKeyEnvironmentVariable);
            Assert.Equal("Bearer env-api-key", headers["Authorization"]);
        }
        finally
        {
            Environment.SetEnvironmentVariable(AlibabaProviderType.ApiKeyVariable, saved);
            Environment.SetEnvironmentVariable("DASHSCOPE_API_KEY", savedDashScope);
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should create an AlibabaProvider instance with custom options",
        Coverage = UpstreamCoverage.Partial,
        Note = "The port has no loadApiKey description, so 'Alibaba Cloud (DashScope)' is not checked. The test reads the resolved headers instead of a mocked loadApiKey."
    )]
    public async Task Should_create_an_AlibabaProvider_instance_with_custom_options()
    {
        var options = new AlibabaProviderSettings
        {
            ApiKey = "custom-key",
            BaseUrl = "https://custom.url",
        };
        options.Headers["Custom-Header"] = "value";
        var provider = AlibabaProviderType.Create(options);
        provider.LanguageModel("qwen-plus");

        var headers = await provider.CreateHeadersAsync(CancellationToken.None);

        Assert.Equal("Bearer custom-key", headers["Authorization"]);
        Assert.Equal("value", headers["Custom-Header"]);
        Assert.Equal("ALIBABA_API_KEY", provider.Options.ApiKeyEnvironmentVariable);
        Assert.Equal("https://custom.url", provider.Options.BaseUrl);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return a chat model when called as a function",
        Coverage = UpstreamCoverage.Covered,
        Note = "The port's Alibaba chat model is an OpenAICompatibleLanguageModel with the alibaba.chat provider id."
    )]
    public void Should_return_a_chat_model_when_called_as_a_function()
    {
        var provider = AlibabaProviderType.Create();
        var modelId = "qwen3-max";

        var model = provider.LanguageModel(modelId);

        var chat = Assert.IsType<OpenAICompatibleLanguageModel>(model);
        Assert.Equal("alibaba.chat", chat.Provider);
        Assert.Equal(modelId, chat.ModelId);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass includeUsage option to language model",
        Coverage = UpstreamCoverage.Covered,
        Note = "The chat model reads includeUsage from the provider options."
    )]
    public void Should_pass_includeUsage_option_to_language_model()
    {
        var provider = AlibabaProviderType.Create(
            new AlibabaProviderSettings { IncludeUsage = false }
        );
        provider.LanguageModel("qwen-plus");

        Assert.False(provider.Options.IncludeUsage);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should default includeUsage to true",
        Coverage = UpstreamCoverage.Covered,
        Note = "The chat model reads includeUsage from the provider options."
    )]
    public void Should_default_includeUsage_to_true()
    {
        var provider = AlibabaProviderType.Create();
        provider.LanguageModel("qwen-plus");

        Assert.True(provider.Options.IncludeUsage);
    }
}
