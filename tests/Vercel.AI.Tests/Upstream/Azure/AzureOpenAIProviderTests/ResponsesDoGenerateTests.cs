// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Azure;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests.AzureResponsesToolSupport;
using AzureProvider = Vercel.AI.Azure.AzureOpenAIProvider;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>Port of <c>azure-openai-provider.test.ts</c> &gt; <c>responses</c> &gt; <c>doGenerate</c>.</summary>
public sealed class ResponsesDoGenerateTests
{
    private const string Id =
        "packages/azure/src/azure-openai-provider.test.ts::responses > doGenerate::";

    private static LanguageModelCallOptions Prompt() =>
        new() { Prompt = new ModelMessage[] { new UserModelMessage("Hello") } };

    [Fact]
    [UpstreamTest(Id + "should extract usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_usage()
    {
        var result = await Generate(JsonFixture("azure-text.1"), Call());

        var usage = result.Usage;
        Assert.Equal(11, usage.InputTokens);
        Assert.Equal(11, usage.NoCacheInputTokens);
        Assert.Equal(0, usage.CacheReadTokens);
        Assert.Null(usage.CacheWriteTokens);
        Assert.Equal(11, usage.OutputTokens);
        Assert.Equal(0, usage.ReasoningTokens);
        Assert.Equal(11, usage.TextTokens);
        JsonAssert.Equal(
            usage.Raw!.Value,
            "{\"input_tokens\":11,\"input_tokens_details\":{\"cached_tokens\":0},\"output_tokens\":11,\"output_tokens_details\":{\"reasoning_tokens\":0},\"total_tokens\":22}"
        );
    }

    [Fact]
    [UpstreamTest(Id + "should extract response metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_response_metadata()
    {
        var result = await Generate(JsonFixture("azure-text.1"), Call());

        Assert.Equal("resp_0d6bb044bb6ff37200698c51948054819385e24e2ad931ae6e", result.ResponseId);
        Assert.Equal("gpt-5.1", result.ResponseModelId);
        Assert.Equal(DateTimeOffset.Parse("2026-02-11T09:53:24.000Z"), result.ResponseTimestamp);
    }

    [Fact]
    [UpstreamTest(Id + "should extract response headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_response_headers()
    {
        var capture = JsonFixture(
            "azure-text.1",
            new Dictionary<string, string> { ["test-header"] = "test-value" }
        );

        var result = await Generate(capture, Call());

        Assert.Equal("1978", result.ResponseHeaders["content-length"]);
        Assert.StartsWith("application/json", result.ResponseHeaders["content-type"]);
        Assert.Equal("test-value", result.ResponseHeaders["test-header"]);
        Assert.Equal(3, result.ResponseHeaders.Count);
    }

    [Fact]
    [UpstreamTest(Id + "should set the correct api version", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_set_the_correct_api_version()
    {
        var capture = JsonFixture("azure-text.1");

        await AzureResponsesToolSupport
            .Provider(capture)
            .ResponsesModel("test-deployment")
            .DoGenerateAsync(Prompt(), CancellationToken.None);

        Assert.Contains("api-version=v1", capture.Uri);
    }

    [Fact]
    [UpstreamTest(Id + "should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_headers()
    {
        var capture = JsonFixture("azure-text.1");
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
            .Create(options, capture)
            .ResponsesModel("test-deployment")
            .DoGenerateAsync(call, CancellationToken.None);

        AssertRequestHeaders(capture.Headers, ProviderAndRequestHeaders);
    }

    [Fact]
    [UpstreamTest(Id + "should use the baseURL correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_the_baseURL_correctly()
    {
        var capture = JsonFixture("azure-text.1");
        var provider = AzureProvider.Create(
            new AzureOpenAIOptions
            {
                BaseUrl = "https://test-resource.openai.azure.com/openai",
                ApiKey = "test-api-key",
            },
            capture
        );

        await provider
            .ResponsesModel("test-deployment")
            .DoGenerateAsync(Prompt(), CancellationToken.None);

        Assert.Equal(
            "https://test-resource.openai.azure.com/openai/v1/responses?api-version=v1",
            capture.Uri
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should send include provider option for file search results",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_include_provider_option_for_file_search_results()
    {
        var capture = JsonFixture("azure-text.1");

        var result = await Generate(
            capture,
            Call(
                ProviderTool(
                    "openai.file_search",
                    "file_search",
                    "{\"vectorStoreIds\":[\"vs_123\",\"vs_456\"],\"maxNumResults\":10,\"ranking\":{\"ranker\":\"auto\"}}"
                )
            )
        );

        var body = RequestBody(capture);
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    "{\"input\":[{\"content\":[{\"text\":\"Hello\",\"type\":\"input_text\"}],\"role\":\"user\"}],\"model\":\"test-deployment\",\"tools\":[{\"max_num_results\":10,\"ranking_options\":{\"ranker\":\"auto\"},\"type\":\"file_search\",\"vector_store_ids\":[\"vs_123\",\"vs_456\"]}]}"
                ),
                body
            ),
            body.ToJsonString()
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Id + "should forward include provider options to request body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_forward_include_provider_options_to_request_body()
    {
        var capture = JsonFixture("azure-text.1");

        var result = await Generate(
            capture,
            Call(azureOptions: "{\"include\":[\"file_search_call.results\"]}")
        );

        var body = RequestBody(capture);
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    "{\"include\":[\"file_search_call.results\"],\"input\":[{\"content\":[{\"text\":\"Hello\",\"type\":\"input_text\"}],\"role\":\"user\"}],\"model\":\"test-deployment\"}"
                ),
                body
            ),
            body.ToJsonString()
        );
        Assert.Empty(result.Warnings);
    }
}
