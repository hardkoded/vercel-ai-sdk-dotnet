// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Azure;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests.AzureResponsesToolSupport;
using AzureProvider = Vercel.AI.Azure.AzureOpenAIProvider;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>Port of <c>azure-openai-provider.test.ts</c> &gt; <c>responses (default language model)</c> &gt; <c>doGenerate</c>.</summary>
public sealed class ResponsesDefaultLanguageModelDoGenerateTests
{
    private const string Id =
        "packages/azure/src/azure-openai-provider.test.ts::responses (default language model) > doGenerate::";

    private const string ResponsesResponse =
        "{\"id\":\"resp_67c97c0203188190a025beb4a75242bc\",\"object\":\"response\",\"created_at\":1741257730,\"status\":\"completed\",\"model\":\"test-deployment\",\"output\":[{\"id\":\"msg_67c97c02656c81908e080dfdf4a03cd1\",\"type\":\"message\",\"status\":\"completed\",\"role\":\"assistant\",\"content\":[{\"type\":\"output_text\",\"text\":\"\",\"annotations\":[]}]}],\"usage\":{\"input_tokens\":4,\"output_tokens\":30,\"total_tokens\":34},\"incomplete_details\":null}";

    private static LanguageModelCallOptions Prompt() =>
        new() { Prompt = new ModelMessage[] { new UserModelMessage("Hello") } };

    private static AzureProvider Create(
        UpstreamRecordingHandler handler,
        Action<AzureOpenAIOptions>? configure = null
    )
    {
        var options = new AzureOpenAIOptions
        {
            ApiKey = "test-api-key",
            ResourceName = "test-resource",
        };
        configure?.Invoke(options);
        return AzureProvider.Create(options, handler);
    }

    [Fact]
    [UpstreamTest(
        Id + "should set the correct default api version",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_set_the_correct_default_api_version()
    {
        var handler = new UpstreamRecordingHandler(ResponsesResponse);

        await Create(handler)
            .LanguageModel("test-deployment")
            .DoGenerateAsync(Prompt(), CancellationToken.None);

        Assert.Equal(
            "https://test-resource.openai.azure.com/openai/v1/responses?api-version=v1",
            handler.Uri
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should set the correct modified api version",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_set_the_correct_modified_api_version()
    {
        var handler = new UpstreamRecordingHandler(ResponsesResponse);

        await Create(handler, options => options.ApiVersion = "2025-04-01-preview")
            .LanguageModel("test-deployment")
            .DoGenerateAsync(Prompt(), CancellationToken.None);

        Assert.Contains("api-version=2025-04-01-preview", handler.Uri);
    }

    [Fact]
    [UpstreamTest(Id + "should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_headers()
    {
        var handler = new UpstreamRecordingHandler(ResponsesResponse);
        var provider = Create(handler, options =>
            options.Headers["Custom-Provider-Header"] = "provider-header-value"
        );
        var call = Prompt();
        call.Headers = new Dictionary<string, string?>
        {
            ["Custom-Request-Header"] = "request-header-value",
        };

        await provider
            .LanguageModel("test-deployment")
            .DoGenerateAsync(call, CancellationToken.None);

        AssertRequestHeaders(handler.Headers, ProviderAndRequestHeaders);
    }

    [Fact]
    [UpstreamTest(
        Id + "should use tokenProvider for Microsoft Entra ID auth",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_tokenProvider_for_Microsoft_Entra_ID_auth()
    {
        var handler = new UpstreamRecordingHandler(ResponsesResponse);
        var provider = AzureProvider.Create(
            new AzureOpenAIOptions
            {
                ResourceName = "test-resource",
                TokenProvider = () => "test-azure-ad-token",
            },
            handler
        );

        await provider
            .LanguageModel("test-deployment")
            .DoGenerateAsync(Prompt(), CancellationToken.None);

        AssertRequestHeaders(
            handler.Headers,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Authorization"] = "Bearer test-azure-ad-token",
            }
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should call tokenProvider for every request",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_tokenProvider_for_every_request()
    {
        var handler = new UpstreamRecordingHandler(ResponsesResponse);
        var count = 0;
        var provider = AzureProvider.Create(
            new AzureOpenAIOptions
            {
                ResourceName = "test-resource",
                TokenProvider = () => "token-" + (++count),
            },
            handler
        );

        await provider
            .LanguageModel("test-deployment")
            .DoGenerateAsync(Prompt(), CancellationToken.None);
        Assert.Equal("Bearer token-1", handler.Headers["Authorization"]);
        await provider
            .LanguageModel("test-deployment")
            .DoGenerateAsync(Prompt(), CancellationToken.None);

        Assert.Equal(2, count);
        Assert.Equal("Bearer token-2", handler.Headers["Authorization"]);
    }

    [Fact]
    [UpstreamTest(
        Id + "should reject explicit apiKey with tokenProvider",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_reject_explicit_apiKey_with_tokenProvider()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            AzureProvider.Create(
                new AzureOpenAIOptions
                {
                    ResourceName = "test-resource",
                    ApiKey = "test-api-key",
                    TokenProvider = () => "test-azure-ad-token",
                }
            )
        );

        Assert.Equal(
            "Both apiKey and tokenProvider were provided. Please use only one authentication method.",
            exception.Message
        );
    }

    [Fact]
    [UpstreamTest(Id + "should use the baseURL correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_the_baseURL_correctly()
    {
        var handler = new UpstreamRecordingHandler(ResponsesResponse);
        var provider = AzureProvider.Create(
            new AzureOpenAIOptions
            {
                BaseUrl = "https://test-resource.openai.azure.com/openai",
                ApiKey = "test-api-key",
            },
            handler
        );

        await provider
            .LanguageModel("test-deployment")
            .DoGenerateAsync(Prompt(), CancellationToken.None);

        Assert.Equal(
            "https://test-resource.openai.azure.com/openai/v1/responses?api-version=v1",
            handler.Uri
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should include explicit message item types for Foundry project endpoints",
        Coverage = UpstreamCoverage.Covered,
        Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d"
    )]
    public async Task Should_include_explicit_message_item_types_for_Foundry_project_endpoints()
    {
        var handler = new UpstreamRecordingHandler(ResponsesResponse);
        var provider = AzureProvider.Create(
            new AzureOpenAIOptions
            {
                BaseUrl =
                    "https://test-resource.services.ai.azure.com/api/projects/test-project/openai/v1",
                ApiKey = "test-api-key",
            },
            handler
        );

        await provider
            .LanguageModel("test-deployment")
            .DoGenerateAsync(
                new LanguageModelCallOptions
                {
                    Prompt = new ModelMessage[]
                    {
                        new SystemModelMessage("You are concise."),
                        new UserModelMessage("Say hi."),
                        new AssistantModelMessage("Hi.", null, null),
                        new UserModelMessage("Say it again."),
                    },
                },
                CancellationToken.None
            );

        Assert.Equal(
            "https://test-resource.services.ai.azure.com/api/projects/test-project/openai/v1/responses",
            handler.Uri
        );
        var expected = JsonNode.Parse(
            "[{\"type\":\"message\",\"role\":\"system\",\"content\":\"You are concise.\"},"
                + "{\"type\":\"message\",\"role\":\"user\",\"content\":[{\"type\":\"input_text\",\"text\":\"Say hi.\"}]},"
                + "{\"type\":\"message\",\"role\":\"assistant\",\"content\":\"Hi.\"},"
                + "{\"type\":\"message\",\"role\":\"user\",\"content\":[{\"type\":\"input_text\",\"text\":\"Say it again.\"}]}]"
        )!;
        Assert.True(JsonNode.DeepEquals(expected, JsonNode.Parse(handler.Body)!["input"]));
    }
}
