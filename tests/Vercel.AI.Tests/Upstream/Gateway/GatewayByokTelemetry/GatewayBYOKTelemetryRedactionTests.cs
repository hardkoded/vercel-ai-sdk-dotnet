// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayByokTelemetry;

/// <summary>
/// Ports <c>describe('Gateway BYOK telemetry redaction')</c> of <c>gateway-byok-telemetry.test.ts</c> for the request body metadata. The port's
/// Gateway errors carry no request body, so the error-body its are not ported.
/// </summary>
[Collection("DecisionDefaultProvider")]
public sealed class GatewayBYOKTelemetryRedactionTests
{
    private const string ByokSecret = "sk-byok-telemetry-secret";

    private static readonly JsonElement Byok = JsonDocument
        .Parse("""{"openai":[{"apiKey":"sk-byok-telemetry-secret"}]}""")
        .RootElement;

    private static LanguageModelCallOptions Options() =>
        new()
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
            ProviderOptions = new Dictionary<string, JsonElement>
            {
                ["gateway"] = JsonDocument
                    .Parse("""{"byok":{"openai":[{"apiKey":"sk-byok-telemetry-secret"}]}}""")
                    .RootElement,
            },
        };

    /// <summary>The application-facing body keeps the credentials; only the telemetry view hides them.</summary>
    private static void ExpectTelemetryViewRedacted(
        JsonElement? body,
        Func<JsonElement, JsonElement>? redact
    )
    {
        Assert.Contains(ByokSecret, body!.Value.GetRawText());

        var telemetryView = TelemetryRedaction
            .RedactTelemetryValue(body.Value, redact)!
            .Value.GetRawText();
        Assert.DoesNotContain(ByokSecret, telemetryView);
        Assert.Contains("[redacted]", telemetryView);
    }

    [Fact]
    [UpstreamTest(
        "packages/gateway/src/gateway-byok-telemetry.test.ts::Gateway BYOK telemetry redaction::redacts the request body metadata of a generate call",
        Coverage = UpstreamCoverage.Covered,
        Commit = "83026e8e0f12930e1792aa9928bb4025d2cd8fd8"
    )]
    public async Task Redacts_the_request_body_metadata_of_a_generate_call()
    {
        var server = new GatewayLanguageModelServer
        {
            ContentType = "application/json",
            ResponseBody =
                """{"content":[{"type":"text","text":"Hello"}],"finishReason":"stop","usage":{"inputTokens":{"total":1},"outputTokens":{"total":1}}}""",
        };

        var result = await GatewayLanguageModelSupport
            .CreateTestModel(server)
            .DoGenerateAsync(Options(), CancellationToken.None);

        Assert.Contains(ByokSecret, server.RequestBody);
        ExpectTelemetryViewRedacted(result.Request?.Body, result.Request?.RedactBody);
    }

    [Fact]
    [UpstreamTest(
        "packages/gateway/src/gateway-byok-telemetry.test.ts::Gateway BYOK telemetry redaction::redacts the request body metadata of a stream call",
        Coverage = UpstreamCoverage.Covered,
        Commit = "83026e8e0f12930e1792aa9928bb4025d2cd8fd8"
    )]
    public async Task Redacts_the_request_body_metadata_of_a_stream_call()
    {
        var server = GatewayLanguageModelSupport.PrepareStreamResponse();

        var result = await GatewayLanguageModelSupport
            .CreateTestModel(server)
            .DoStreamAsync(Options(), CancellationToken.None);

        Assert.Contains(ByokSecret, server.RequestBody);
        ExpectTelemetryViewRedacted(result.Request!.Body, result.Request.RedactBody);
    }
}
