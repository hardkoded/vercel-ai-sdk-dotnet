// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayProviderTests;

/// <summary>Port of <c>gateway-provider.test.ts</c> &gt; <c>GatewayProvider &gt; createGateway</c>, with its <c>metadata caching</c> tests.</summary>
[Collection("GatewayAuthEnvironment")]
public sealed class GatewayProviderTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-provider.test.ts::GatewayProvider > createGateway";
    private const string OneModel =
        "{\"models\":[{\"id\":\"test-model\",\"name\":\"Test\",\"specification\":{\"specificationVersion\":\"v4\",\"provider\":\"p\",\"modelId\":\"test-model\"}}]}";

    [Fact]
    [UpstreamTest(
        Prefix + "::should create provider with correct configuration",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream checks the GatewayLanguageModel constructor call. The port's model reads the provider, so the test checks the provider id, the base URL and the provider's headers (config.headers). The upstream fetch: undefined check has no counterpart: the port's provider always owns an HTTP client."
    )]
    public async Task Should_create_provider_with_correct_configuration()
    {
        using var scope = new GatewayProviderTestScope();
        var provider = GatewayProvider.Create(
            new GatewayOptions
            {
                BaseUrl = "https://api.example.com",
                ApiKey = "test-api-key",
                Headers = new Dictionary<string, string> { ["Custom-Header"] = "value" },
            }
        );

        var model = provider.LanguageModel("test-model");

        Assert.Equal("gateway", model.Provider);
        Assert.Equal("https://api.example.com", provider.CreateConfig().BaseUrl);
        AssertGatewayHeaders(
            await provider.GetHeadersAsync(CancellationToken.None),
            "Bearer test-api-key",
            "api-key",
            custom: "value"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "::should create a provider-owned batch service",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream checks the GatewayBatch constructor call. The port checks the provider id and that the batch results request goes to the provider's base URL with the gateway headers."
    )]
    public async Task Should_create_a_provider_owned_batch_service()
    {
        using var scope = new GatewayProviderTestScope();
        var handler = new UpstreamRecordingHandler(
            "{\"type\":\"text\",\"id\":\"req-1\",\"status\":\"succeeded\",\"result\":{\"content\":[{\"type\":\"text\",\"text\":\"pong 1\"}],\"finishReason\":{\"unified\":\"stop\"},\"response\":{\"modelId\":\"openai/gpt-5.6-luna\",\"timestamp\":\"2026-08-18T00:00:00.000Z\"},\"usage\":{\"inputTokens\":{\"total\":4,\"noCache\":4},\"outputTokens\":{\"total\":2,\"text\":2}},\"warnings\":[]}}\n"
        );
        var provider = GatewayProvider.Create(
            new GatewayOptions { BaseUrl = "https://api.example.com", ApiKey = "test-api-key" },
            handler
        );

        var batch = provider.ExperimentalBatch();

        Assert.NotNull(batch);
        Assert.Equal("gateway.batch", batch.Provider);
        await batch.DoGetResultsAsync(
            new BatchOperationCall(
                "job_123",
                null,
                new Dictionary<string, string>(),
                CancellationToken.None
            ),
            CancellationToken.None
        );
        Assert.Equal("https://api.example.com/batch/results", handler.Uri);
        Assert.Equal("Bearer test-api-key", handler.Headers["authorization"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "::should use OIDC token when no API key is provided",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream reads config.headers of the GatewayLanguageModel constructor call. The port reads the provider's headers (getHeaders)."
    )]
    public async Task Should_use_OIDC_token_when_no_API_key_is_provided()
    {
        using var scope = new GatewayProviderTestScope();
        var provider = GatewayProvider.Create(
            new GatewayOptions
            {
                BaseUrl = "https://api.example.com",
                Headers = new Dictionary<string, string> { ["Custom-Header"] = "value" },
            }
        );
        provider.LanguageModel("test-model");

        AssertGatewayHeaders(
            await provider.GetHeadersAsync(CancellationToken.None),
            "Bearer mock-oidc-token",
            "oidc",
            custom: "value"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "::should use apiKey and teamIdOrSlug for language model requests",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream reads config.headers of the GatewayLanguageModel constructor call. The port reads the provider's headers (getHeaders)."
    )]
    public async Task Should_use_apiKey_and_teamIdOrSlug_for_language_model_requests()
    {
        using var scope = new GatewayProviderTestScope();
        var provider = GatewayProvider.Create(
            new GatewayOptions
            {
                ApiKey = "vca_test-token",
                TeamIdOrSlug = "vercel",
                Headers = new Dictionary<string, string> { ["Custom-Header"] = "value" },
            }
        );
        provider.LanguageModel("test-model");

        AssertGatewayHeaders(
            await provider.GetHeadersAsync(CancellationToken.None),
            "Bearer vca_test-token",
            "api-key",
            custom: "value",
            team: "vercel"
        );
        Assert.Equal(0, scope.OidcCalls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "::should use apiKey and teamIdOrSlug for non-language model requests",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream reads config.headers of the GatewayImageModel. The port asserts the same headers on the request the image model sends."
    )]
    public async Task Should_use_apiKey_and_teamIdOrSlug_for_non_language_model_requests()
    {
        using var scope = new GatewayProviderTestScope();
        var handler = new UpstreamRecordingHandler("{\"images\":[\"base64-image-1\"]}");
        var provider = GatewayProvider.Create(
            new GatewayOptions { ApiKey = "vca_test-token", TeamIdOrSlug = "vercel" },
            handler
        );

        var model = Assert.IsType<Vercel.AI.Gateway.GatewayImageModel>(
            provider.ImageModel("openai/gpt-image-1")
        );
        await SendImageRequestAsync(model);

        Assert.Equal("Bearer vca_test-token", handler.Headers["authorization"]);
        Assert.Equal("vercel", handler.Headers["x-vercel-ai-gateway-team"]);
        Assert.Equal("api-key", handler.Headers["ai-gateway-auth-method"]);
        Assert.Equal(0, scope.OidcCalls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "::should pass teamIdOrSlug with URL-special characters as a header value",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_teamIdOrSlug_with_URL_special_characters_as_a_header_value()
    {
        using var scope = new GatewayProviderTestScope();
        var provider = GatewayProvider.Create(
            new GatewayOptions { ApiKey = "vca_test-token", TeamIdOrSlug = "team-o'brien" }
        );
        provider.LanguageModel("test-model");

        var headers = await provider.GetHeadersAsync(CancellationToken.None);

        Assert.Equal("team-o'brien", headers["x-vercel-ai-gateway-team"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "::should omit team header for apiKey requests without teamIdOrSlug",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_team_header_for_apiKey_requests_without_teamIdOrSlug()
    {
        using var scope = new GatewayProviderTestScope();
        var provider = GatewayProvider.Create(new GatewayOptions { ApiKey = "vca_test-token" });
        provider.LanguageModel("test-model");

        var headers = await provider.GetHeadersAsync(CancellationToken.None);

        Assert.Equal("Bearer vca_test-token", headers["authorization"]);
        Assert.DoesNotContain("x-vercel-ai-gateway-team", headers.Keys);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "::should pass teamIdOrSlug with OIDC requests",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream reads config.headers of the GatewayLanguageModel constructor call. The port reads the provider's headers (getHeaders)."
    )]
    public async Task Should_pass_teamIdOrSlug_with_OIDC_requests()
    {
        using var scope = new GatewayProviderTestScope();
        var provider = GatewayProvider.Create(new GatewayOptions { TeamIdOrSlug = "vercel" });
        provider.LanguageModel("test-model");

        var headers = await provider.GetHeadersAsync(CancellationToken.None);

        Assert.Equal("Bearer mock-oidc-token", headers["authorization"]);
        Assert.Equal("vercel", headers["x-vercel-ai-gateway-team"]);
        Assert.Equal("oidc", headers["ai-gateway-auth-method"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "::should create GatewayEmbeddingModel for embeddingModel",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_create_GatewayEmbeddingModel_for_embeddingModel()
    {
        var provider = GatewayProvider.Create(
            new GatewayOptions { BaseUrl = "https://api.example.com" }
        );

        var model = provider.EmbeddingModel("openai/text-embedding-3-small");

        Assert.IsType<Vercel.AI.Gateway.GatewayEmbeddingModel>(model);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "::should create GatewayImageModel for imageModel",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream reads provider and baseURL from the internal model config. The port asserts the provider id and the URL of the request the model sends."
    )]
    public async Task Should_create_GatewayImageModel_for_imageModel()
    {
        var handler = new UpstreamRecordingHandler("{\"images\":[\"base64-image-1\"]}");
        var provider = GatewayProvider.Create(
            new GatewayOptions { BaseUrl = "https://api.example.com", ApiKey = "test-api-key" },
            handler
        );

        var model = Assert.IsType<Vercel.AI.Gateway.GatewayImageModel>(
            provider.ImageModel("openai/gpt-image-1")
        );
        await SendImageRequestAsync(model);

        Assert.Equal("gateway", model.Provider);
        Assert.Equal("https://api.example.com/image-model", handler.Uri);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "::should create GatewayVideoModel for videoModel",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream reads provider and baseURL from the internal model config. The port asserts the provider id and the URL of the request the model sends."
    )]
    public async Task Should_create_GatewayVideoModel_for_videoModel()
    {
        var handler = new UpstreamRecordingHandler(
            "data: {\"type\":\"result\",\"videos\":[{\"type\":\"base64\",\"data\":\"base64-video-1\",\"mediaType\":\"video/mp4\"}]}\n\n"
        );
        var provider = GatewayProvider.Create(
            new GatewayOptions { BaseUrl = "https://api.example.com", ApiKey = "test-api-key" },
            handler
        );

        var model = Assert.IsType<Vercel.AI.Gateway.GatewayVideoModel>(
            provider.VideoModel("google/veo-2.0-generate-001")
        );
        await SendVideoRequestAsync(model);

        Assert.Equal("gateway", model.Provider);
        Assert.Equal("https://api.example.com/video-model", handler.Uri);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "::should create GatewayTranscriptionModel for transcriptionModel",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream reads provider and baseURL from the internal model config. The port asserts the provider id and the provider's base URL."
    )]
    public void Should_create_GatewayTranscriptionModel_for_transcriptionModel()
    {
        var provider = GatewayProvider.Create(
            new GatewayOptions { BaseUrl = "https://api.example.com", ApiKey = "test-api-key" }
        );

        var model = Assert.IsType<Vercel.AI.Gateway.GatewayTranscriptionModel>(
            provider.TranscriptionModel("openai/gpt-4o-transcribe")
        );

        Assert.Equal("gateway", model.Provider);
        Assert.Equal("https://api.example.com", provider.CreateConfig().BaseUrl);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "::should create GatewayTranscriptionModel for transcription alias",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_create_GatewayTranscriptionModel_for_transcription_alias()
    {
        var provider = GatewayProvider.Create(
            new GatewayOptions { BaseUrl = "https://api.example.com", ApiKey = "test-api-key" }
        );

        var model = provider.Transcription("openai/gpt-4o-transcribe");

        Assert.IsType<Vercel.AI.Gateway.GatewayTranscriptionModel>(model);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "::should reuse gateway headers and fetch for transcriptionModel",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream reads config.headers, config.fetch and config.o11yHeaders. The port reads the provider's headers (getHeaders), its HTTP client, which stands in for fetch, and its o11y headers."
    )]
    public async Task Should_reuse_gateway_headers_and_fetch_for_transcriptionModel()
    {
        using var scope = new GatewayProviderTestScope();
        var provider = GatewayProvider.Create(
            new GatewayOptions
            {
                BaseUrl = "https://api.example.com",
                ApiKey = "test-api-key",
                Headers = new Dictionary<string, string> { ["Custom-Header"] = "value" },
            }
        );

        var model = Assert.IsType<Vercel.AI.Gateway.GatewayTranscriptionModel>(
            provider.TranscriptionModel("openai/gpt-4o-transcribe")
        );

        AssertGatewayHeaders(
            await provider.GetHeadersAsync(CancellationToken.None),
            "Bearer test-api-key",
            "api-key",
            custom: "value"
        );
        Assert.Same(provider.Http, provider.CreateConfig().Http);
        Assert.Equal("gateway", model.Provider);
        Assert.Equal(
            [new KeyValuePair<string, string>("ai-o11y-request-id", "mock-request-id")],
            provider.O11yHeaders()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "::should fetch available models",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream checks the GatewayFetchMetadata constructor call and the mocked getAvailableModels. The port checks the provider's base URL and the metadata request that reaches the HTTP handler."
    )]
    public async Task Should_fetch_available_models()
    {
        using var scope = new GatewayProviderTestScope();
        var server = new GatewayTestServer("{\"models\":[]}");
        var provider = GatewayProvider.Create(
            new GatewayOptions { BaseUrl = "https://api.example.com", ApiKey = "test-api-key" },
            server
        );

        var models = await provider.GetAvailableModelsAsync();

        Assert.NotNull(models);
        Assert.Equal("https://api.example.com", provider.CreateConfig().BaseUrl);
        Assert.Equal("https://api.example.com/config", server.Calls[0].RequestUri!.ToString());
    }

    [Fact]
    [UpstreamTest(
        Prefix + " > metadata caching::should cache metadata for the specified refresh interval",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_cache_metadata_for_the_specified_refresh_interval()
    {
        var server = new GatewayTestServer(OneModel);
        var currentTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var provider = GatewayProvider.Create(
            new GatewayOptions
            {
                BaseUrl = "https://api.example.com",
                ApiKey = "test-api-key",
                MetadataCacheRefreshMillis = 10000, // 10 seconds
                CurrentDate = () => currentTime,
            },
            server
        );

        // First call should fetch metadata
        await provider.GetAvailableModelsAsync();
        Assert.Single(server.Calls);

        // Second immediate call should use cache
        await provider.GetAvailableModelsAsync();
        Assert.Single(server.Calls);

        // Advance time by 9 seconds (should still use cache)
        currentTime = currentTime.AddMilliseconds(9000);
        await provider.GetAvailableModelsAsync();
        Assert.Single(server.Calls);

        // Advance time past 10 seconds (should refresh)
        currentTime = currentTime.AddMilliseconds(2000);
        await provider.GetAvailableModelsAsync();
        Assert.Equal(2, server.Calls.Count);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + " > metadata caching::should use default 5 minute refresh interval when not specified",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_default_5_minute_refresh_interval_when_not_specified()
    {
        var server = new GatewayTestServer(OneModel);
        var currentTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var provider = GatewayProvider.Create(
            new GatewayOptions
            {
                BaseUrl = "https://api.example.com",
                ApiKey = "test-api-key",
                CurrentDate = () => currentTime,
            },
            server
        );

        // First call should fetch metadata
        await provider.GetAvailableModelsAsync();
        Assert.Single(server.Calls);

        // Advance time by 4 minutes (should still use cache)
        currentTime = currentTime.AddMinutes(4);
        await provider.GetAvailableModelsAsync();
        Assert.Single(server.Calls);

        // Advance time past 5 minutes (should refresh)
        currentTime = currentTime.AddMinutes(2);
        await provider.GetAvailableModelsAsync();
        Assert.Equal(2, server.Calls.Count);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "::should pass o11y headers to GatewayLanguageModel when environment variables are set",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream reads config.o11yHeaders of the GatewayLanguageModel constructor call. The port reads the provider's o11y headers."
    )]
    public void Should_pass_o11y_headers_to_GatewayLanguageModel_when_environment_variables_are_set()
    {
        using var scope = new GatewayProviderTestScope();
        scope.SetEnvironment("VERCEL_DEPLOYMENT_ID", "test-deployment");
        scope.SetEnvironment("VERCEL_ENV", "test");
        scope.SetEnvironment("VERCEL_REGION", "iad1");
        scope.SetEnvironment("VERCEL_PROJECT_ID", "prj_test123");
        using var context = VercelRequestContext.Use(
            new Dictionary<string, string?> { ["x-vercel-id"] = "test-request-id" }
        );
        var provider = GatewayProvider.Create(
            new GatewayOptions { BaseUrl = "https://api.example.com", ApiKey = "test-api-key" }
        );

        var model = provider.LanguageModel("test-model");

        Assert.Equal("gateway", model.Provider);
        Assert.Equal("https://api.example.com", provider.CreateConfig().BaseUrl);
        Assert.Equal(
            [
                new KeyValuePair<string, string>("ai-o11y-deployment-id", "test-deployment"),
                new KeyValuePair<string, string>("ai-o11y-environment", "test"),
                new KeyValuePair<string, string>("ai-o11y-region", "iad1"),
                new KeyValuePair<string, string>("ai-o11y-request-id", "test-request-id"),
                new KeyValuePair<string, string>("ai-o11y-project-id", "prj_test123"),
            ],
            provider.O11yHeaders()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "::should not include undefined o11y headers",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream reads config.o11yHeaders of the GatewayLanguageModel constructor call. The port reads the provider's o11y headers."
    )]
    public void Should_not_include_undefined_o11y_headers()
    {
        using var scope = new GatewayProviderTestScope();
        scope.SetEnvironment("VERCEL_DEPLOYMENT_ID", null);
        scope.SetEnvironment("VERCEL_ENV", null);
        scope.SetEnvironment("VERCEL_REGION", null);
        scope.SetEnvironment("VERCEL_PROJECT_ID", null);
        using var context = VercelRequestContext.Use(headers: null);
        var provider = GatewayProvider.Create(
            new GatewayOptions { BaseUrl = "https://api.example.com", ApiKey = "test-api-key" }
        );

        var model = provider.LanguageModel("test-model");

        Assert.Equal("gateway", model.Provider);
        Assert.Equal("https://api.example.com", provider.CreateConfig().BaseUrl);
        Assert.Empty(provider.O11yHeaders());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "::should reuse gateway headers and fetch for imageModel",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream reads the headers, fetch and o11yHeaders from the internal model config. The port asserts the same values on the request the image model sends through the custom HTTP handler."
    )]
    public async Task Should_reuse_gateway_headers_and_fetch_for_imageModel()
    {
        using var context = VercelRequestContext.Use(
            new Dictionary<string, string?> { ["x-vercel-id"] = "mock-request-id" }
        );
        var handler = new UpstreamRecordingHandler("{\"images\":[\"base64-image-1\"]}");
        var provider = GatewayProvider.Create(
            new GatewayOptions
            {
                BaseUrl = "https://api.example.com",
                ApiKey = "test-api-key",
                Headers = new Dictionary<string, string> { ["Custom-Header"] = "value" },
            },
            handler
        );

        var model = Assert.IsType<Vercel.AI.Gateway.GatewayImageModel>(
            provider.ImageModel("openai/gpt-image-1")
        );
        await model.DoGenerateAsync(
            new ImageModelCall(
                "Test prompt",
                null,
                null,
                1,
                null,
                null,
                null,
                default,
                new Dictionary<string, string>(),
                CancellationToken.None
            ),
            CancellationToken.None
        );

        Assert.Equal(1, handler.Calls);
        AssertReusedGatewayHeaders(handler);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "::should reuse gateway headers and fetch for videoModel",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream reads the headers, fetch and o11yHeaders from the internal model config. The port asserts the same values on the request the video model sends through the custom HTTP handler."
    )]
    public async Task Should_reuse_gateway_headers_and_fetch_for_videoModel()
    {
        using var context = VercelRequestContext.Use(
            new Dictionary<string, string?> { ["x-vercel-id"] = "mock-request-id" }
        );
        var handler = new UpstreamRecordingHandler(
            "data: {\"type\":\"result\",\"videos\":[{\"type\":\"base64\",\"data\":\"base64-video-1\",\"mediaType\":\"video/mp4\"}]}\n\n"
        );
        var provider = GatewayProvider.Create(
            new GatewayOptions
            {
                BaseUrl = "https://api.example.com",
                ApiKey = "test-api-key",
                Headers = new Dictionary<string, string> { ["Custom-Header"] = "value" },
            },
            handler
        );

        var model = Assert.IsType<Vercel.AI.Gateway.GatewayVideoModel>(
            provider.VideoModel("google/veo-2.0-generate-001")
        );
        await model.DoGenerateAsync(
            new VideoModelCall(
                "Test prompt",
                1,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                default,
                new Dictionary<string, string>(),
                CancellationToken.None,
                null,
                null
            ),
            CancellationToken.None
        );

        Assert.Equal(1, handler.Calls);
        AssertReusedGatewayHeaders(handler);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "::should create GatewayRerankingModel for rerankingModel",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream reads provider and baseURL from the internal model config. The port asserts the provider id and the URL of the request the model sends."
    )]
    public async Task Should_create_GatewayRerankingModel_for_rerankingModel()
    {
        var handler = new UpstreamRecordingHandler("{\"ranking\":[]}");
        var provider = GatewayProvider.Create(
            new GatewayOptions { BaseUrl = "https://api.example.com", ApiKey = "test-api-key" },
            handler
        );

        var model = Assert.IsType<Vercel.AI.Gateway.GatewayRerankingModel>(
            provider.RerankingModel("cohere/rerank-v3.5")
        );
        await model.DoRerankAsync("query", ["a"], null, CancellationToken.None);

        Assert.Equal("gateway", model.Provider);
        Assert.Equal("https://api.example.com/reranking-model", handler.Uri);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "::should create GatewayRerankingModel for reranking alias",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_create_GatewayRerankingModel_for_reranking_alias()
    {
        var provider = GatewayProvider.Create(
            new GatewayOptions { BaseUrl = "https://api.example.com", ApiKey = "test-api-key" },
            new UpstreamRecordingHandler("{}")
        );

        var model = Assert.IsType<Vercel.AI.Gateway.GatewayRerankingModel>(
            provider.Reranking("cohere/rerank-v3.5")
        );
        var request = new RerankRequest { Model = model };
        Assert.Same(model, request.Model);
    }

    internal static Task SendImageRequestAsync(Vercel.AI.Gateway.GatewayImageModel model)
    {
        return model.DoGenerateAsync(
            new ImageModelCall(
                "Test prompt",
                null,
                null,
                1,
                null,
                null,
                null,
                default,
                new Dictionary<string, string>(),
                CancellationToken.None
            ),
            CancellationToken.None
        );
    }

    internal static Task SendVideoRequestAsync(Vercel.AI.Gateway.GatewayVideoModel model)
    {
        return model.DoGenerateAsync(
            new VideoModelCall(
                "Test prompt",
                1,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                default,
                new Dictionary<string, string>(),
                CancellationToken.None,
                null,
                null
            ),
            CancellationToken.None
        );
    }

    /// <summary>Maps to <c>toEqual</c> of the gateway headers: the same keys and values, with any protocol version.</summary>
    internal static void AssertGatewayHeaders(
        IReadOnlyDictionary<string, string?> actual,
        string authorization,
        string authMethod,
        string? custom = null,
        string? team = null
    )
    {
        var lowered = actual.ToDictionary(pair => pair.Key.ToLowerInvariant(), pair => pair.Value);
        Assert.True(lowered.ContainsKey("ai-gateway-protocol-version"));
        var expected = new Dictionary<string, string?>
        {
            ["authorization"] = authorization,
            ["ai-gateway-protocol-version"] = lowered["ai-gateway-protocol-version"],
            ["ai-gateway-auth-method"] = authMethod,
            ["user-agent"] = "ai-sdk-gateway/" + AiSdkVersion.Version,
        };
        if (custom is not null)
        {
            expected["custom-header"] = custom;
        }

        if (team is not null)
        {
            expected["x-vercel-ai-gateway-team"] = team;
        }

        Assert.Equal(
            expected.OrderBy(pair => pair.Key, StringComparer.Ordinal),
            lowered.OrderBy(pair => pair.Key, StringComparer.Ordinal)
        );
    }

    private static void AssertReusedGatewayHeaders(UpstreamRecordingHandler handler)
    {
        Assert.Equal("Bearer test-api-key", handler.Headers["Authorization"]);
        Assert.Equal("value", handler.Headers["custom-header"]);
        Assert.True(handler.Headers.ContainsKey("ai-gateway-protocol-version"));
        Assert.Equal("api-key", handler.Headers["ai-gateway-auth-method"]);
        Assert.StartsWith(
            "ai-sdk-gateway/",
            handler.Headers["user-agent"],
            StringComparison.Ordinal
        );
        Assert.Equal("mock-request-id", handler.Headers["ai-o11y-request-id"]);
    }

    private sealed class VercelEnvironmentVariables : IDisposable
    {
        private readonly (string Name, string? Previous)[] _previous;

        public VercelEnvironmentVariables(params (string Name, string? Value)[] variables)
        {
            _previous = variables
                .Select(variable =>
                    (variable.Name, Environment.GetEnvironmentVariable(variable.Name))
                )
                .ToArray();
            foreach (var (name, value) in variables)
            {
                Environment.SetEnvironmentVariable(name, value);
            }
        }

        public void Dispose()
        {
            foreach (var (name, previous) in _previous)
            {
                Environment.SetEnvironmentVariable(name, previous);
            }
        }
    }
}
