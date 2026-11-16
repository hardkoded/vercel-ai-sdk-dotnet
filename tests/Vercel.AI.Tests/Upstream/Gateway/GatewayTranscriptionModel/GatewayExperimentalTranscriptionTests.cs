// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeModel;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayTranscriptionModel;

/// <summary>
/// Port of <c>gateway-transcription-model.test.ts</c> &gt; <c>gateway.experimental_transcription</c>. The browser test
/// simulates the upstream <c>window</c> global through <c>GatewayProvider.IsBrowserEnvironment</c>.
/// </summary>
public sealed class GatewayExperimentalTranscriptionTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-transcription-model.test.ts::gateway.experimental_transcription::";
    private const string Model = "openai/gpt-realtime-whisper";

    [Fact]
    [UpstreamTest(
        Prefix + "creates a transcription model from a model id",
        Coverage = UpstreamCoverage.Covered,
        Commit = "640569da18351d8a75fd20bcd8ff3d214ff0a617"
    )]
    public void Creates_a_transcription_model_from_a_model_id()
    {
        var gateway = GatewayProvider.Create(new GatewayOptions { ApiKey = "vck_test-token" });

        var model = gateway.ExperimentalTranscription(Model);

        Assert.Equal("v4", model.SpecificationVersion);
        Assert.Equal(Model, model.ModelId);
        Assert.Equal("gateway", model.Provider);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "mints a transcription-bound vcst_ client secret and returns it with the wss url",
        Coverage = UpstreamCoverage.Covered,
        Commit = "640569da18351d8a75fd20bcd8ff3d214ff0a617"
    )]
    public async Task Mints_a_transcription_bound_vcst_client_secret_and_returns_it_with_the_wss_url()
    {
        var capture = new RealtimeMintCapture(
            "{\"token\":\"vcst_minted\",\"expiresAt\":1700000060}"
        );
        var mintGateway = GatewayProvider.Create(
            new GatewayOptions { ApiKey = "vck_test-token" },
            capture
        );

        var result = await mintGateway.ExperimentalTranscriptionGetTokenAsync(Model, 120);

        // Minted token (not the raw key), and the mint hit the v1 route on the gateway origin, not the transcription
        // baseURL path (/v4/ai).
        Assert.Equal("vcst_minted", result.Token);
        Assert.Equal(1_700_000_060, result.ExpiresAt);
        Assert.Equal(
            "wss://ai-gateway.vercel.sh/v4/ai/transcription-model?ai-model-id=openai%2Fgpt-realtime-whisper",
            result.Url
        );
        var request = Assert.Single(capture.Requests);
        Assert.Equal(
            "https://ai-gateway.vercel.sh/v1/realtime/client-secrets",
            request.RequestUri!.AbsoluteUri
        );

        // The body binds the token to the transcription surface.
        var sentBody = JsonNode.Parse(capture.Bodies[0])!.AsObject();
        Assert.Equal(Model, (string?)sentBody["model"]);
        Assert.Equal("transcription", (string?)sentBody["routeKind"]);
        Assert.Equal(120, (int?)sentBody["expiresIn"]);
        // Authenticated with the long-lived key.
        Assert.Equal("Bearer vck_test-token", request.Headers.GetValues("authorization").Single());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "tolerates a null expiresAt from the mint endpoint and omits it from the result",
        Coverage = UpstreamCoverage.Covered,
        Commit = "640569da18351d8a75fd20bcd8ff3d214ff0a617"
    )]
    public async Task Tolerates_a_null_expiresAt_from_the_mint_endpoint_and_omits_it_from_the_result()
    {
        var capture = new RealtimeMintCapture("{\"token\":\"vcst_minted\",\"expiresAt\":null}");
        var mintGateway = GatewayProvider.Create(
            new GatewayOptions { ApiKey = "vck_test-token" },
            capture
        );

        var result = await mintGateway.ExperimentalTranscriptionGetTokenAsync(Model);

        Assert.Equal("vcst_minted", result.Token);
        Assert.Null(result.ExpiresAt);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "rejects minting (getToken) in browsers — the credential must stay server-side",
        Coverage = UpstreamCoverage.Covered,
        Commit = "640569da18351d8a75fd20bcd8ff3d214ff0a617"
    )]
    public async Task Rejects_minting_getToken_in_browsers_the_credential_must_stay_server_side()
    {
        var gateway = GatewayProvider.Create(new GatewayOptions { ApiKey = "vck_test-token" });
        gateway.IsBrowserEnvironment = () => true;

        var exception = await Assert.ThrowsAsync<AiSdkException>(() =>
            gateway.ExperimentalTranscriptionGetTokenAsync(Model)
        );

        Assert.Contains("must be minted server-side", exception.Message);
    }
}
