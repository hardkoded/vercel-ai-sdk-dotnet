// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Operations;
using Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayProviderTests;

/// <summary>Port of the <c>getAvailableModels</c> tests of <c>gateway-provider.test.ts</c> &gt; <c>GatewayProvider &gt; createGateway</c>.</summary>
[Collection("GatewayAuthEnvironment")]
public sealed class GatewayProviderTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-provider.test.ts::GatewayProvider > createGateway";
    private const string OneModel =
        "{\"models\":[{\"id\":\"test-model\",\"name\":\"Test\",\"specification\":{\"specificationVersion\":\"v4\",\"provider\":\"p\",\"modelId\":\"test-model\"}}]}";

    [Fact]
    [UpstreamTest(
        Prefix + "::should use OIDC token when no API key is provided",
        Coverage = UpstreamCoverage.Partial,
        Note = "Checks the headers of a language model request. The upstream test mocks the GatewayLanguageModel constructor and resolves config.headers."
    )]
    public async Task Should_use_OIDC_token_when_no_API_key_is_provided()
    {
        var previousKey = Environment.GetEnvironmentVariable(
            GatewayProvider.ApiKeyEnvironmentVariable
        );
        Environment.SetEnvironmentVariable(GatewayProvider.ApiKeyEnvironmentVariable, null);
        VercelEnvironment.OidcTokenOverride = _ => Task.FromResult("mock-oidc-token");
        try
        {
            var server = new GatewayLanguageModelServer { Failure = new HttpRequestException() };
            var provider = GatewayProvider.Create(
                new GatewayOptions
                {
                    BaseUrl = "https://api.example.com",
                    Headers = new Dictionary<string, string> { ["Custom-Header"] = "value" },
                },
                server
            );
            var model = provider.LanguageModel("test-model");

            await Assert.ThrowsAnyAsync<Exception>(() =>
                model.DoGenerateAsync(
                    GatewayLanguageModelSupport.TestPrompt,
                    CancellationToken.None
                )
            );

            Assert.Equal("Bearer mock-oidc-token", server.RequestHeaders["authorization"]);
            Assert.Equal("oidc", server.RequestHeaders["ai-gateway-auth-method"]);
        }
        finally
        {
            VercelEnvironment.OidcTokenOverride = null;
            Environment.SetEnvironmentVariable(
                GatewayProvider.ApiKeyEnvironmentVariable,
                previousKey
            );
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "::should pass teamIdOrSlug with OIDC requests",
        Coverage = UpstreamCoverage.Partial,
        Note = "Checks the headers of a language model request. The upstream test mocks the GatewayLanguageModel constructor and resolves config.headers."
    )]
    public async Task Should_pass_teamIdOrSlug_with_OIDC_requests()
    {
        var previousKey = Environment.GetEnvironmentVariable(
            GatewayProvider.ApiKeyEnvironmentVariable
        );
        Environment.SetEnvironmentVariable(GatewayProvider.ApiKeyEnvironmentVariable, null);
        VercelEnvironment.OidcTokenOverride = _ => Task.FromResult("mock-oidc-token");
        try
        {
            var server = new GatewayLanguageModelServer { Failure = new HttpRequestException() };
            var provider = GatewayProvider.Create(
                new GatewayOptions { TeamIdOrSlug = "vercel" },
                server
            );
            var model = provider.LanguageModel("test-model");

            await Assert.ThrowsAnyAsync<Exception>(() =>
                model.DoGenerateAsync(
                    GatewayLanguageModelSupport.TestPrompt,
                    CancellationToken.None
                )
            );

            Assert.Equal("Bearer mock-oidc-token", server.RequestHeaders["authorization"]);
            Assert.Equal("vercel", server.RequestHeaders["x-vercel-ai-gateway-team"]);
            Assert.Equal("oidc", server.RequestHeaders["ai-gateway-auth-method"]);
        }
        finally
        {
            VercelEnvironment.OidcTokenOverride = null;
            Environment.SetEnvironmentVariable(
                GatewayProvider.ApiKeyEnvironmentVariable,
                previousKey
            );
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "::should fetch available models",
        Coverage = UpstreamCoverage.Partial,
        Note = "Checks the base URL and the call through the HTTP handler. The upstream test mocks the GatewayFetchMetadata constructor."
    )]
    public async Task Should_fetch_available_models()
    {
        var server = new GatewayTestServer("{\"models\":[]}");
        var provider = GatewayProvider.Create(
            new GatewayOptions { BaseUrl = "https://api.example.com", ApiKey = "test-api-key" },
            server
        );

        await provider.GetAvailableModelsAsync();

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
        Coverage = UpstreamCoverage.Partial,
        Note = "Asserts the headers of the provider's O11yHeaders. The upstream test reads them from the GatewayLanguageModel constructor config, which the port does not mock."
    )]
    public void Should_pass_o11y_headers_to_GatewayLanguageModel_when_environment_variables_are_set()
    {
        using var environment = new VercelEnvironmentVariables(
            ("VERCEL_DEPLOYMENT_ID", "test-deployment"),
            ("VERCEL_ENV", "test"),
            ("VERCEL_REGION", "iad1"),
            ("VERCEL_PROJECT_ID", "prj_test123")
        );
        using var context = VercelRequestContext.Use(
            new Dictionary<string, string?> { ["x-vercel-id"] = "test-request-id" }
        );
        var provider = GatewayProvider.Create(
            new GatewayOptions { BaseUrl = "https://api.example.com", ApiKey = "test-api-key" },
            new GatewayTestServer("{}")
        );

        var o11yHeaders = provider.O11yHeaders().ToList();

        Assert.Equal(
            [
                new KeyValuePair<string, string>("ai-o11y-deployment-id", "test-deployment"),
                new KeyValuePair<string, string>("ai-o11y-environment", "test"),
                new KeyValuePair<string, string>("ai-o11y-region", "iad1"),
                new KeyValuePair<string, string>("ai-o11y-request-id", "test-request-id"),
                new KeyValuePair<string, string>("ai-o11y-project-id", "prj_test123"),
            ],
            o11yHeaders
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "::should not include undefined o11y headers",
        Coverage = UpstreamCoverage.Partial,
        Note = "Asserts the headers of the provider's O11yHeaders. The upstream test reads them from the GatewayLanguageModel constructor config, which the port does not mock."
    )]
    public void Should_not_include_undefined_o11y_headers()
    {
        using var environment = new VercelEnvironmentVariables(
            ("VERCEL_DEPLOYMENT_ID", null),
            ("VERCEL_ENV", null),
            ("VERCEL_REGION", null),
            ("VERCEL_PROJECT_ID", null)
        );
        using var context = VercelRequestContext.Use(headers: null);
        var provider = GatewayProvider.Create(
            new GatewayOptions { BaseUrl = "https://api.example.com", ApiKey = "test-api-key" },
            new GatewayTestServer("{}")
        );

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
