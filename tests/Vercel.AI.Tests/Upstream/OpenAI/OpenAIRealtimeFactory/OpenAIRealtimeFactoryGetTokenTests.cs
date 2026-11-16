// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIRealtimeFactory;

/// <summary>Port of <c>openai-realtime-factory.test.ts</c> &gt; <c>OpenAI realtime factory getToken</c>.</summary>
public sealed class OpenAIRealtimeFactoryGetTokenTests
{
    private const string Prefix =
        "packages/openai/src/realtime/openai-realtime-factory.test.ts::OpenAI realtime factory getToken::";

    [Theory]
    [InlineData("gpt-live-1", null)]
    [InlineData("not-yet-released", "live")]
    [InlineData("gpt-realtime", "live")]
    [UpstreamTest(
        Prefix + "rejects Live credentials without fetching (%j)",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_Live_credentials_without_fetching(string model, string? api)
    {
        var capture = new OpenAICapture();
        var provider = OpenAIUpstream.Provider(capture, options =>
        {
            options.ApiKey = null;
            options.ApiKeyEnvironmentVariable = "OPENAI_API_KEY_UNSET_FOR_REALTIME_FACTORY_TESTS";
        });
        var options = api == null ? null : new OpenAIRealtimeOptions { Api = api };

        await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() =>
            provider.ExperimentalRealtimeGetTokenAsync(model, options)
        );
        var exception = await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() =>
            provider.ExperimentalRealtimeGetTokenAsync(model, options)
        );
        Assert.Contains(
            "Use server WebSocket setup via GetServerWebSocketConfigAsync()",
            exception.Message,
            StringComparison.Ordinal
        );
        Assert.Equal(0, capture.Calls);
    }

    [Theory]
    [InlineData("gpt-realtime", null)]
    [InlineData("unknown", null)]
    [InlineData("gpt-live-1-preview", null)]
    [InlineData("gpt-live-1", "realtime")]
    [UpstreamTest(
        Prefix + "preserves the Realtime mint request and result (%j)",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_the_Realtime_mint_request_and_result(string model, string? api)
    {
        var capture = new OpenAICapture
        {
            ResponseJson = "{\"value\":\"secret\",\"expires_at\":123}",
        };
        var provider = OpenAIUpstream.Provider(capture, options =>
        {
            options.ApiKey = "test-key";
            options.BaseUrl = "https://example.com/proxy/v1/";
            options.Organization = "org-test";
            options.Project = "proj-test";
            options.Headers["X-Custom"] = "value";
        });

        var secret = await provider.ExperimentalRealtimeGetTokenAsync(
            model,
            api == null ? null : new OpenAIRealtimeOptions { Api = api },
            expiresAfterSeconds: 60
        );

        Assert.Equal("secret", secret.Token);
        Assert.Equal("wss://example.com/v1/realtime?model=" + model, secret.Url);
        Assert.Equal(123, secret.ExpiresAt);
        Assert.Equal(1, capture.Calls);
        Assert.Equal("POST", capture.Method);
        Assert.Equal("https://example.com/proxy/v1/realtime/client_secrets", capture.Uri);
        Assert.Equal("Bearer test-key", capture.Headers["Authorization"]);
        Assert.Equal("org-test", capture.Headers["OpenAI-Organization"]);
        Assert.Equal("proj-test", capture.Headers["OpenAI-Project"]);
        Assert.Equal("value", capture.Headers["X-Custom"]);
        OpenAIUpstream.Equal(
            JsonNode.Parse(capture.Body),
            "{\"session\":{\"type\":\"realtime\",\"model\":\""
                + model
                + "\"},\"expires_after\":{\"anchor\":\"created_at\",\"seconds\":60}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "forwards session configuration and omits unspecified expiry",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Forwards_session_configuration_and_omits_unspecified_expiry()
    {
        var capture = new OpenAICapture { ResponseJson = "{\"value\":\"secret\"}" };

        await OpenAIUpstream
            .Provider(capture)
            .ExperimentalRealtimeGetTokenAsync(
                "gpt-realtime",
                sessionConfig: new RealtimeModelV4SessionConfig { Instructions = "Be concise." }
            );

        var body = JsonNode.Parse(capture.Body)!;
        Assert.Equal("realtime", body["session"]!["type"]!.GetValue<string>());
        Assert.Equal("gpt-realtime", body["session"]!["model"]!.GetValue<string>());
        Assert.Equal("Be concise.", body["session"]!["instructions"]!.GetValue<string>());
        Assert.Null(body["expires_after"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "does not fall back to Live after a Realtime HTTP error",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Does_not_fall_back_to_Live_after_a_Realtime_HTTP_error()
    {
        var capture = new OpenAICapture
        {
            Status = HttpStatusCode.NotFound,
            ResponseJson = "Unknown model",
            ResponseMediaType = "text/plain",
        };

        var exception = await Assert.ThrowsAnyAsync<ApiException>(() =>
            OpenAIUpstream.Provider(capture).ExperimentalRealtimeGetTokenAsync("not-yet-released")
        );

        Assert.Contains("Unknown model", exception.Message, StringComparison.Ordinal);
        Assert.Equal(404, exception.StatusCode);
        Assert.Equal(1, capture.Calls);
    }
}
