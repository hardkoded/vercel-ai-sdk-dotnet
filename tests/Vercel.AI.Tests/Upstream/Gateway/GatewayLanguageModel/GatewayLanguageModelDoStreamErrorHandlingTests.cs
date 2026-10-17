// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using Vercel.AI.Gateway;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel.GatewayLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel;

/// <summary>Port of <c>gateway-language-model.test.ts</c> &gt; <c>GatewayLanguageModel &gt; doStream &gt; Error handling</c>.</summary>
public sealed class GatewayLanguageModelDoStreamErrorHandlingTests
{
    private const string Prefix = "packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doStream > Error handling::";

    [Fact]
    [UpstreamTest(Prefix + "should not double-wrap existing Gateway errors", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_double_wrap_existing_Gateway_errors()
    {
        var existingGatewayError = new GatewayAuthenticationError("Already a Gateway error", 401);
        var server = new GatewayLanguageModelServer { Failure = existingGatewayError };

        var error = await Assert.ThrowsAsync<GatewayAuthenticationError>(() => CreateTestModel(server).DoGenerateAsync(TestPrompt, CancellationToken.None));

        Assert.Same(existingGatewayError, error);
        Assert.Equal("Already a Gateway error", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle network errors gracefully", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_network_errors_gracefully()
    {
        var networkError = new InvalidOperationException("Network connection failed");
        var server = new GatewayLanguageModelServer { Failure = networkError };

        var error = await Assert.ThrowsAsync<GatewayResponseError>(() => CreateTestModel(server).DoGenerateAsync(TestPrompt, CancellationToken.None));

        Assert.Equal("Invalid error response format: Gateway request failed: Network connection failed", error.Message);
        Assert.Same(networkError, error.Cause);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle network errors gracefully in streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_network_errors_gracefully_in_streaming()
    {
        var networkError = new InvalidOperationException("Network connection failed");
        var server = new GatewayLanguageModelServer { Failure = networkError };

        var error = await Assert.ThrowsAsync<GatewayResponseError>(() => CreateTestModel(server).DoStreamAsync(TestPrompt, CancellationToken.None));

        Assert.Equal("Invalid error response format: Gateway request failed: Network connection failed", error.Message);
        Assert.Same(networkError, error.Cause);
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve error cause chain", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_error_cause_chain()
    {
        var server = new GatewayLanguageModelServer
        {
            Status = HttpStatusCode.Unauthorized,
            ContentType = "application/json",
            ResponseBody = "{\"error\":{\"message\":\"Token expired\",\"type\":\"authentication_error\"}}",
        };

        var error = await Assert.ThrowsAsync<GatewayAuthenticationError>(() => CreateTestModel(server).DoGenerateAsync(TestPrompt, CancellationToken.None));

        Assert.NotNull(error.Cause);
    }
}
