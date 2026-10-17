// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel.GatewayLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel;

/// <summary>Port of <c>gateway-language-model.test.ts</c> &gt; <c>GatewayLanguageModel &gt; doStream</c>.</summary>
public sealed class GatewayLanguageModelDoStreamTests
{
    private const string Prefix = "packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doStream::";

    [Fact]
    [UpstreamTest(
        Prefix + "should stream text deltas",
        Coverage = UpstreamCoverage.Partial,
        Note = "Checks the text deltas and the stop finish part. The port maps usage to its own usage type and does not keep the raw prompt_tokens and completion_tokens keys. See #581.")]
    public async Task Should_stream_text_deltas()
    {
        var server = PrepareStreamResponse("Hello", ", ", "World!");

        var result = await CreateTestModel(server).DoStreamAsync(TestPrompt, CancellationToken.None);
        var parts = await ReadAll(result);

        Assert.Collection(
            parts,
            part => Assert.Equal("Hello", Assert.IsType<TextDeltaStreamPart>(part).Delta),
            part => Assert.Equal(", ", Assert.IsType<TextDeltaStreamPart>(part).Delta),
            part => Assert.Equal("World!", Assert.IsType<TextDeltaStreamPart>(part).Delta),
            part => Assert.Equal(FinishReason.Stop, Assert.IsType<FinishStreamPart>(part).FinishReason));
    }

    [Fact]
    [UpstreamTest(Prefix + "should forward server stream-start warnings exactly once", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_forward_server_stream_start_warnings_exactly_once()
    {
        var server = StreamChunks(
            "{\"type\":\"stream-start\",\"warnings\":[{\"type\":\"other\",\"message\":\"from provider\"}]}",
            "{\"type\":\"text-delta\",\"textDelta\":\"Hello\"}",
            "{\"type\":\"finish\",\"finishReason\":\"stop\",\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":20}}");

        var result = await CreateTestModel(server).DoStreamAsync(TestPrompt, CancellationToken.None);
        var parts = await ReadAll(result);

        var start = Assert.Single(parts.OfType<StreamStartStreamPart>());
        var warning = Assert.Single(start.Warnings);
        Assert.Equal("other", warning.Type);
        Assert.Equal("from provider", warning.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve explicit mid-stream provider error metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_explicit_mid_stream_provider_error_metadata()
    {
        const string error = "{\"message\":\"Upstream provider overloaded\",\"type\":\"provider_overloaded\",\"statusCode\":503,\"isRetryable\":true}";
        var server = StreamChunks(
            "{\"type\":\"text-delta\",\"textDelta\":\"Partial output\"}",
            "{\"type\":\"error\",\"error\":" + error + "}");

        var result = await CreateTestModel(server).DoStreamAsync(TestPrompt, CancellationToken.None);
        var parts = await ReadAll(result);

        Assert.Collection(
            parts,
            part => Assert.Equal("Partial output", Assert.IsType<TextDeltaStreamPart>(part).Delta),
            part =>
            {
                var providerError = Assert.IsType<ErrorStreamPart>(part).Error!;
                Assert.Equal("Upstream provider overloaded", providerError.Message);
                Assert.Equal("provider_overloaded", providerError.Type);
                Assert.Equal(503, providerError.StatusCode);
                Assert.True(providerError.IsRetryable);
                JsonAssert.Equal(Assert.IsType<JsonElement>(providerError.Data), error);
            });
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass streaming headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_streaming_headers()
    {
        var server = PrepareStreamResponse("Test");

        await CreateTestModel(server).DoStreamAsync(TestPrompt, CancellationToken.None);

        Assert.Equal("4", server.RequestHeaders["ai-language-model-specification-version"]);
        Assert.Equal("test-model", server.RequestHeaders["ai-language-model-id"]);
        Assert.Equal("true", server.RequestHeaders["ai-language-model-streaming"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should remove abortSignal from the streaming request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_remove_abortSignal_from_the_streaming_request_body()
    {
        var server = PrepareStreamResponse("Test content");
        using var source = new CancellationTokenSource();

        await CreateTestModel(server).DoStreamAsync(TestPrompt, source.Token);

        Assert.Null(server.RequestJson["abortSignal"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass abortSignal to fetch when provided for streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_abortSignal_to_fetch_when_provided_for_streaming()
    {
        var server = PrepareStreamResponse("Test content");
        using var source = new CancellationTokenSource();
        server.BeforeRespond = source.Cancel;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => CreateTestModel(server).DoStreamAsync(TestPrompt, source.Token));

        Assert.Equal(1, server.Calls);
        Assert.True(server.RequestToken.CanBeCanceled);
        Assert.True(server.RequestToken.IsCancellationRequested);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not pass abortSignal to fetch when not provided for streaming",
        Coverage = UpstreamCoverage.Covered,
        Note = "A .NET call always has a token and HttpClient adds its own, so the test checks that the request token is not cancelled.")]
    public async Task Should_not_pass_abortSignal_to_fetch_when_not_provided_for_streaming()
    {
        var server = PrepareStreamResponse("Test content");

        await CreateTestModel(server).DoStreamAsync(TestPrompt, CancellationToken.None);

        Assert.Equal(1, server.Calls);
        Assert.False(server.RequestToken.IsCancellationRequested);
    }

    [Fact]
    [UpstreamTest(Prefix + "should include o11y headers in the streaming request", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_o11y_headers_in_the_streaming_request()
    {
        var server = PrepareStreamResponse("Test content");
        var o11yHeaders = new Dictionary<string, string>
        {
            ["ai-o11y-deployment-id"] = "test-deployment",
            ["ai-o11y-environment"] = "production",
            ["ai-o11y-region"] = "iad1",
        };

        await CreateTestModel(server, o11yHeaders).DoStreamAsync(TestPrompt, CancellationToken.None);

        foreach (var header in o11yHeaders)
        {
            Assert.Equal(header.Value, server.RequestHeaders[header.Key]);
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert API call errors to Gateway errors in streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_convert_API_call_errors_to_Gateway_errors_in_streaming()
    {
        var server = new GatewayLanguageModelServer
        {
            Status = HttpStatusCode.TooManyRequests,
            ContentType = "application/json",
            ResponseBody = "{\"error\":{\"message\":\"Rate limit exceeded\",\"type\":\"rate_limit_exceeded\"}}",
        };

        var error = await Assert.ThrowsAsync<GatewayRateLimitError>(() => CreateTestModel(server).DoStreamAsync(TestPrompt, CancellationToken.None));

        Assert.Equal("Rate limit exceeded", error.Message);
        Assert.Equal(429, error.StatusCode);
        Assert.Equal("rate_limit_exceeded", error.Type);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle authentication errors in streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_authentication_errors_in_streaming()
    {
        var server = new GatewayLanguageModelServer
        {
            Status = HttpStatusCode.Unauthorized,
            ContentType = "application/json",
            ResponseBody = "{\"error\":{\"message\":\"Authentication failed for streaming\",\"type\":\"authentication_error\"}}",
        };

        var error = await Assert.ThrowsAsync<GatewayAuthenticationError>(() => CreateTestModel(server).DoStreamAsync(TestPrompt, CancellationToken.None));

        Assert.Contains("Invalid API key", error.Message);
        Assert.Contains("vercel.com/d?to=", error.Message);
        Assert.Equal(401, error.StatusCode);
        Assert.Equal("authentication_error", error.Type);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle invalid request errors in streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_invalid_request_errors_in_streaming()
    {
        var server = new GatewayLanguageModelServer
        {
            Status = HttpStatusCode.BadRequest,
            ContentType = "application/json",
            ResponseBody = "{\"error\":{\"message\":\"Invalid streaming request\",\"type\":\"invalid_request_error\"}}",
        };

        var error = await Assert.ThrowsAsync<GatewayInvalidRequestError>(() => CreateTestModel(server).DoStreamAsync(TestPrompt, CancellationToken.None));

        Assert.Equal("Invalid streaming request", error.Message);
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("invalid_request_error", error.Type);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle malformed error responses in streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_malformed_error_responses_in_streaming()
    {
        var server = new GatewayLanguageModelServer
        {
            Status = HttpStatusCode.InternalServerError,
            ContentType = "application/json",
            ResponseBody = "Invalid JSON for streaming",
        };

        var error = await Assert.ThrowsAsync<GatewayResponseError>(() => CreateTestModel(server).DoStreamAsync(TestPrompt, CancellationToken.None));

        Assert.Equal(500, error.StatusCode);
        Assert.Equal("response_error", error.Type);
    }
}
