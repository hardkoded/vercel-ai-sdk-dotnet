// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.ProviderUtils;
using Redactor = Vercel.AI.Gateway.RedactGatewayRequestCredentials;

namespace Vercel.AI.Tests.Upstream.Gateway.RedactGatewayRequestCredentials;

/// <summary>Ports <c>describe('withGatewayRequestTelemetryRedaction')</c> of <c>redact-gateway-request-credentials.test.ts</c>.</summary>
public sealed class WithGatewayRequestTelemetryRedactionTests
{
    private static JsonElement Parse(string json) => JsonDocument.Parse(json).RootElement;

    private static string Telemetry(TelemetryRedactedValue value) =>
        TelemetryRedaction.RedactTelemetryValue(value)!.Value.GetRawText();

    private static string Canonical(string json) => Parse(json).GetRawText();

    [Fact]
    [UpstreamTest(
        "packages/gateway/src/redact-gateway-request-credentials.test.ts::withGatewayRequestTelemetryRedaction::redacts every BYOK value in the telemetry view",
        Coverage = UpstreamCoverage.Covered,
        Commit = "83026e8e0f12930e1792aa9928bb4025d2cd8fd8"
    )]
    public void Redacts_every_BYOK_value_in_the_telemetry_view()
    {
        var body = Parse(
            """
            {"stateParts":[{"type":"text","text":"hello"}],"fields":{},"providerOptions":{"openai":{"reasoningEffort":"low"},"gateway":{"tags":["team-a"],"byok":{"openai":[{"apiKey":"sk-byok-openai"}],"bedrock":[{"accessKeyId":"AKIAEXAMPLE","secretAccessKey":"bedrock-secret","region":"us-east-1"}]}}}}
            """
        );

        var telemetryView = Telemetry(Redactor.WithGatewayRequestTelemetryRedaction(body));

        Assert.Equal(
            Canonical(
                """
                {"stateParts":[{"type":"text","text":"hello"}],"fields":{},"providerOptions":{"openai":{"reasoningEffort":"low"},"gateway":{"tags":["team-a"],"byok":{"openai":[{"apiKey":"[redacted]"}],"bedrock":[{"accessKeyId":"[redacted]","secretAccessKey":"[redacted]","region":"[redacted]"}]}}}}
                """
            ),
            telemetryView
        );

        // The request the Gateway receives keeps the credentials.
        var serialized = body.GetRawText();
        Assert.Contains("sk-byok-openai", serialized);
        Assert.Contains("bedrock-secret", serialized);
    }

    [Fact]
    [UpstreamTest(
        "packages/gateway/src/redact-gateway-request-credentials.test.ts::withGatewayRequestTelemetryRedaction::keeps the serialized request body unchanged",
        Coverage = UpstreamCoverage.Covered,
        Commit = "83026e8e0f12930e1792aa9928bb4025d2cd8fd8"
    )]
    public void Keeps_the_serialized_request_body_unchanged()
    {
        var body = Parse(
            """{"prompt":[{"role":"user","content":[{"type":"text","text":"hi"}]}],"providerOptions":{"gateway":{"byok":{"openai":[{"apiKey":"secret"}]}}}}"""
        );

        var annotated = Redactor.WithGatewayRequestTelemetryRedaction(body);

        Assert.Equal(body.GetRawText(), annotated.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(
        "packages/gateway/src/redact-gateway-request-credentials.test.ts::withGatewayRequestTelemetryRedaction::redacts BYOK credentials in batch request options",
        Coverage = UpstreamCoverage.Covered,
        Commit = "83026e8e0f12930e1792aa9928bb4025d2cd8fd8"
    )]
    public void Redacts_BYOK_credentials_in_batch_request_options()
    {
        var body = Parse(
            """{"requests":[{"id":"request-1","type":"text","modelId":"openai/gpt-5","options":{"prompt":[],"providerOptions":{"gateway":{"byok":{"openai":[{"apiKey":"sk-byok-batch"}]}}}}}]}"""
        );

        var telemetryView = Telemetry(Redactor.WithGatewayRequestTelemetryRedaction(body));

        Assert.Equal(
            Canonical(
                """{"requests":[{"id":"request-1","type":"text","modelId":"openai/gpt-5","options":{"prompt":[],"providerOptions":{"gateway":{"byok":{"openai":[{"apiKey":"[redacted]"}]}}}}}]}"""
            ),
            telemetryView
        );
        Assert.Contains("sk-byok-batch", body.GetRawText());
    }

    [Fact]
    [UpstreamTest(
        "packages/gateway/src/redact-gateway-request-credentials.test.ts::withGatewayRequestTelemetryRedaction::preserves bodies without BYOK credentials",
        Coverage = UpstreamCoverage.Partial,
        Commit = "83026e8e0f12930e1792aa9928bb4025d2cd8fd8"
    )]
    public void Preserves_bodies_without_BYOK_credentials()
    {
        var withoutByok = Parse(
            """{"prompt":[],"providerOptions":{"gateway":{"tags":["team-a"]}}}"""
        );
        Assert.Null(Redactor.WithGatewayRequestTelemetryRedaction(withoutByok).Redact);

        var withoutProviderOptions = Parse("""{"prompt":[]}""");
        Assert.Null(Redactor.WithGatewayRequestTelemetryRedaction(withoutProviderOptions).Redact);
    }
}
