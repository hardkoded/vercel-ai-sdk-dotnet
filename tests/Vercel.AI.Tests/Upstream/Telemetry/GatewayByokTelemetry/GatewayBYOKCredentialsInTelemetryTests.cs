// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.Gateway.GatewayByokTelemetry;

namespace Vercel.AI.Tests.Upstream.Telemetry.GatewayByokTelemetry;

/// <summary>Ports <c>describe('Gateway BYOK credentials in telemetry')</c> of <c>packages/ai/src/telemetry/gateway-byok-telemetry.test.ts</c>.</summary>
[Collection("DecisionDefaultProvider")]
public sealed class GatewayBYOKCredentialsInTelemetryTests
{
    private const string Prefix =
        "packages/ai/src/telemetry/gateway-byok-telemetry.test.ts::Gateway BYOK credentials in telemetry::";

    private const string Commit = "83026e8e0f12930e1792aa9928bb4025d2cd8fd8";

    private const string ByokSecret = "sk-byok-telemetry-secret";

    private static readonly JsonElement Byok = JsonDocument
        .Parse("""{"byok":{"openai":[{"apiKey":"sk-byok-telemetry-secret"}]}}""")
        .RootElement;

    private static readonly JsonElement GatewayOptionsJson = JsonDocument
        .Parse("""{"gateway":{"byok":{"openai":[{"apiKey":"sk-byok-telemetry-secret"}]}}}""")
        .RootElement;

    /// <summary>A Gateway whose requests all fail with an HTTP 400 error response.</summary>
    private static GatewayProvider CreateFailingGateway() =>
        GatewayProvider.Create(
            new GatewayOptions { ApiKey = "test-gateway-key" },
            new FailingHandler()
        );

    private static ITelemetry Integration(List<TelemetryErrorEvent> events) =>
        new TestTelemetry
        {
            OnError = (e, _) =>
            {
                events.Add(e);
                return Task.CompletedTask;
            },
        };

    [Fact]
    [UpstreamTest(
        Prefix + "keeps BYOK credentials out of telemetry integration errors",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Keeps_BYOK_credentials_out_of_telemetry_integration_errors()
    {
        var events = new List<TelemetryErrorEvent>();

        await Assert.ThrowsAnyAsync<Exception>(() =>
            new AiClient(CreateFailingGateway()).GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = CreateFailingGateway().LanguageModel("openai/gpt-5"),
                    Prompt = "hello",
                    ProviderOptions = new Dictionary<string, JsonElement> { ["gateway"] = Byok },
                    Telemetry = new TelemetryOptions { Integrations = [Integration(events)] },
                }
            )
        );

        var e = Assert.Single(events);
        Assert.DoesNotContain(ByokSecret, GatewayTelemetryDump.Dump(e));
        Assert.DoesNotContain(ByokSecret, GatewayTelemetryDump.Dump(e.Error));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "keeps BYOK credentials out of telemetry integration errors on extraction",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Keeps_BYOK_credentials_out_of_telemetry_integration_errors_on_extraction()
    {
        var events = new List<TelemetryErrorEvent>();

        await Assert.ThrowsAnyAsync<Exception>(() =>
            Extract.ExtractAsync(
                new ExtractRequest
                {
                    Model = CreateFailingGateway().ExtractionModel("openai/gpt-5"),
                    State = "Invoice INV-2041",
                    Fields = new Dictionary<string, ExtractionField>
                    {
                        ["invoiceNumber"] = new("text"),
                    },
                    ProviderOptions = GatewayOptionsJson,
                    Telemetry = new TelemetryOptions { Integrations = [Integration(events)] },
                }
            )
        );

        var e = Assert.Single(events);
        Assert.DoesNotContain(ByokSecret, GatewayTelemetryDump.Dump(e));
        Assert.DoesNotContain(ByokSecret, GatewayTelemetryDump.Dump(e.Error));
    }

    private sealed class FailingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        ) =>
            Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(
                        """{"error":{"message":"bad request"}}""",
                        Encoding.UTF8,
                        "application/json"
                    ),
                }
            );
    }
}
