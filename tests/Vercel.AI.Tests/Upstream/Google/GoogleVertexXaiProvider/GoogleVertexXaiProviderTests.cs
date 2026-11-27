// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Util;
using GoogleVertexXaiProviderType = Vercel.AI.Google.GoogleVertexXaiProvider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleVertexXaiProvider;

public sealed class GoogleVertexXaiProviderTests
{
    private const string Prefix =
        "packages/google-vertex/src/xai/google-vertex-xai-provider.test.ts::google-vertex-xai-provider::";

    private const string ModelId = "xai/grok-4.1-fast-reasoning";

    [Fact]
    [UpstreamTest(
        Prefix + "should not call createOpenAICompatible at provider creation time",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_not_call_createOpenAICompatible_at_provider_creation_time()
    {
        var provider = new GoogleVertexXaiProviderType("test-project", "global");

        Assert.False(provider.ClientCreated);
        Assert.Null(provider.CompatibleOptions);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should create a provider with correct base URL for global location",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_create_a_provider_with_correct_base_URL_for_global_location()
    {
        var options = Options("global");

        Assert.Equal(
            "https://aiplatform.googleapis.com/v1/projects/test-project/locations/global/endpoints/openapi",
            options.BaseUrl
        );
        AssertGrokSettings(options);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should construct the default base URL when baseURL is an empty string",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_construct_the_default_base_URL_when_baseURL_is_an_empty_string()
    {
        var provider = new GoogleVertexXaiProviderType("test-project", "global", baseUrl: "");
        provider.LanguageModel(ModelId);

        Assert.Equal(
            "https://aiplatform.googleapis.com/v1/projects/test-project/locations/global/endpoints/openapi",
            provider.CompatibleOptions!.BaseUrl
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should create a provider with correct base URL for regional location",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_create_a_provider_with_correct_base_URL_for_regional_location()
    {
        var options = Options("us-central1");

        Assert.Equal(
            "https://aiplatform.googleapis.com/v1/projects/test-project/locations/us-central1/endpoints/openapi",
            options.BaseUrl
        );
        AssertGrokSettings(options);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should configure OpenAI-compatible behavior for Vertex Grok",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_configure_OpenAI_compatible_behavior_for_Vertex_Grok()
    {
        var options = Options("global");

        Assert.Equal(
            "https://aiplatform.googleapis.com/v1/projects/test-project/locations/global/endpoints/openapi",
            options.BaseUrl
        );
        AssertGrokSettings(options);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should strip reasoning_effort from request bodies",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_strip_reasoning_effort_from_request_bodies()
    {
        var options = Options("global");

        var body = options.TransformRequestBody!(
            new JsonObject
            {
                ["model"] = ModelId,
                ["reasoning_effort"] = "high",
                ["messages"] = new JsonArray(),
            },
            []
        );

        Assert.Equal(
            "{\"model\":\"xai/grok-4.1-fast-reasoning\",\"messages\":[]}",
            body.ToJsonString()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should count Grok reasoning tokens separately from completion tokens",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_count_Grok_reasoning_tokens_separately_from_completion_tokens()
    {
        var options = Options("global");
        using var document = JsonDocument.Parse(
            "{\"prompt_tokens\":663,\"prompt_tokens_details\":{\"cached_tokens\":654},\"completion_tokens\":50,\"completion_tokens_details\":{\"reasoning_tokens\":124}}"
        );

        var usage = options.ConvertUsage!(document.RootElement);

        Assert.Equal(663, usage.InputTokens);
        Assert.Equal(654, usage.CacheReadTokens);
        Assert.Null(usage.CacheWriteTokens);
        Assert.Equal(9, usage.NoCacheInputTokens);
        Assert.Equal(174, usage.OutputTokens);
        Assert.Equal(50, usage.TextTokens);
        Assert.Equal(124, usage.ReasoningTokens);
        Assert.Equal(document.RootElement.GetRawText(), usage.Raw!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(Prefix + "should support HTTP image URLs", Coverage = UpstreamCoverage.Covered)]
    public void Should_support_HTTP_image_URLs()
    {
        var urls = Assert.Single(Options("global").SupportedUrls!);

        Assert.Equal("image/*", urls.Key);
        Assert.Equal("^https?://.*\\z", urls.Value);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw NoSuchModelError for embedding and image models",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_throw_NoSuchModelError_for_embedding_and_image_models()
    {
        var provider = new GoogleVertexXaiProviderType("test-project", null);

        Assert.Throws<NoSuchModelError>(() => provider.EmbeddingModel("invalid-model-id"));
        Assert.Throws<NoSuchModelError>(() => provider.ImageModel("invalid-model-id"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should cache the OpenAI-compatible provider after first access",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_cache_the_OpenAI_compatible_provider_after_first_access()
    {
        var provider = new GoogleVertexXaiProviderType("test-project", null);

        provider.LanguageModel(ModelId);
        var first = provider.CompatibleOptions;
        provider.LanguageModel("xai/grok-4.20-reasoning");

        Assert.NotNull(first);
        Assert.Same(first, provider.CompatibleOptions);
    }

    private static OpenAICompatibleOptions Options(string location)
    {
        var provider = new GoogleVertexXaiProviderType("test-project", location);
        provider.LanguageModel(ModelId);
        return provider.CompatibleOptions!;
    }

    private static void AssertGrokSettings(OpenAICompatibleOptions options)
    {
        Assert.Equal("googleVertex.xai", options.ProviderName);
        Assert.True(options.IncludeUsage);
        Assert.True(options.SupportsStructuredOutputs);
        Assert.NotNull(options.ConvertUsage);
        Assert.NotNull(options.SupportedUrls);
        Assert.NotNull(options.TransformRequestBody);
    }
}
