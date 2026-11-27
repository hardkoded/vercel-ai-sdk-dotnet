// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Azure;
using Vercel.AI.Provider;
using AzureProvider = Vercel.AI.Azure.AzureOpenAIProvider;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>Port of <c>azure-openai-provider.test.ts</c> &gt; <c>completion</c> &gt; <c>doGenerate</c>.</summary>
public sealed class CompletionDoGenerateTests
{
    private const string CompletionResponse =
        "{\"id\":\"cmpl-96cAM1v77r4jXa4qb2NSmRREV5oWB\",\"object\":\"text_completion\",\"created\":1711363706,\"model\":\"test-deployment\",\"choices\":[{\"text\":\"Hello World!\",\"index\":0,\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":4,\"total_tokens\":34,\"completion_tokens\":30}}";

    private static LanguageModelCallOptions Prompt() =>
        new() { Prompt = new ModelMessage[] { new UserModelMessage("Hello") } };

    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::completion > doGenerate::should set the correct api version",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_set_the_correct_api_version()
    {
        var handler = new UpstreamRecordingHandler(CompletionResponse);
        var provider = AzureProvider.Create(
            new AzureOpenAIOptions { ApiKey = "test-api-key", ResourceName = "test-resource" },
            handler
        );

        await provider
            .CompletionModel("test-deployment", null)
            .DoGenerateAsync(Prompt(), CancellationToken.None);

        Assert.Equal(
            "https://test-resource.openai.azure.com/openai/v1/completions?api-version=v1",
            handler.Uri
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::completion > doGenerate::should pass headers",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_headers()
    {
        var handler = new UpstreamRecordingHandler(CompletionResponse);
        var options = new AzureOpenAIOptions
        {
            ApiKey = "test-api-key",
            ResourceName = "test-resource",
        };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var call = Prompt();
        call.Headers = new Dictionary<string, string?>
        {
            ["Custom-Request-Header"] = "request-header-value",
        };

        await AzureProvider
            .Create(options, handler)
            .CompletionModel("test-deployment", null)
            .DoGenerateAsync(call, CancellationToken.None);

        AzureResponsesToolSupport.AssertRequestHeaders(
            handler.Headers,
            AzureResponsesToolSupport.ProviderAndRequestHeaders
        );
    }
}
