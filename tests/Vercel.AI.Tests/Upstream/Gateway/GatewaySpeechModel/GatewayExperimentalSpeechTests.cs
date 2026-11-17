// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeModel;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewaySpeechModel;

/// <summary>
/// Port of <c>gateway-speech-model.test.ts</c> &gt; <c>gateway.experimental_speech</c>. The browser test simulates the
/// upstream <c>window</c> global through <c>GatewayProvider.IsBrowserEnvironment</c>.
/// </summary>
public sealed class GatewayExperimentalSpeechTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-speech-model.test.ts::gateway.experimental_speech::";
    private const string Model = "elevenlabs/eleven-flash-v2.5";

    [Fact]
    [UpstreamTest(
        Prefix + "mints a speech-bound vcst_ client secret and returns it with the wss url",
        Coverage = UpstreamCoverage.Covered,
        Commit = "640569da18351d8a75fd20bcd8ff3d214ff0a617"
    )]
    public async Task Mints_a_speech_bound_vcst_client_secret_and_returns_it_with_the_wss_url()
    {
        var capture = new RealtimeMintCapture(
            "{\"token\":\"vcst_minted\",\"expiresAt\":1700000060}"
        );
        var gateway = GatewayProvider.Create(
            new GatewayOptions { ApiKey = "vck_test-token" },
            capture
        );

        var result = await gateway.ExperimentalSpeechGetTokenAsync(Model, 120);

        Assert.Equal("vcst_minted", result.Token);
        Assert.Equal(1_700_000_060, result.ExpiresAt);
        Assert.Equal(
            "wss://ai-gateway.vercel.sh/v4/ai/speech-model?ai-model-id=elevenlabs%2Feleven-flash-v2.5",
            result.Url
        );
        var request = Assert.Single(capture.Requests);
        Assert.Equal(
            "https://ai-gateway.vercel.sh/v1/realtime/client-secrets",
            request.RequestUri!.AbsoluteUri
        );
        var sentBody = JsonNode.Parse(capture.Bodies[0])!.AsObject();
        Assert.Equal(Model, (string?)sentBody["model"]);
        Assert.Equal("speech", (string?)sentBody["routeKind"]);
        Assert.Equal(120, (int?)sentBody["expiresIn"]);
        Assert.Equal("Bearer vck_test-token", request.Headers.GetValues("authorization").Single());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "rejects minting (getToken) in browsers",
        Coverage = UpstreamCoverage.Covered,
        Commit = "640569da18351d8a75fd20bcd8ff3d214ff0a617"
    )]
    public async Task Rejects_minting_getToken_in_browsers()
    {
        var gateway = GatewayProvider.Create(new GatewayOptions { ApiKey = "vck_test-token" });
        gateway.IsBrowserEnvironment = () => true;

        var exception = await Assert.ThrowsAsync<AiSdkException>(() =>
            gateway.ExperimentalSpeechGetTokenAsync(Model)
        );

        Assert.Contains("must be minted server-side", exception.Message);
    }
}
