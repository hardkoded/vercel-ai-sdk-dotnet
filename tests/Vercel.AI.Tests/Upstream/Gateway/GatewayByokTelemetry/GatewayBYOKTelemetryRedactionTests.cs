// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.Upstream.Gateway.GatewayExtractionModel;
using Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayByokTelemetry;

/// <summary>Ports <c>describe('Gateway BYOK telemetry redaction')</c> of <c>gateway-byok-telemetry.test.ts</c>.</summary>
[Collection("DecisionDefaultProvider")]
public sealed class GatewayBYOKTelemetryRedactionTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-byok-telemetry.test.ts::Gateway BYOK telemetry redaction::";

    private const string Commit = "83026e8e0f12930e1792aa9928bb4025d2cd8fd8";

    private const string ByokSecret = "sk-byok-telemetry-secret";

    private const string ProviderOptionsJson =
        """{"gateway":{"byok":{"openai":[{"apiKey":"sk-byok-telemetry-secret"}]}}}""";

    private static LanguageModelCallOptions Options(bool includeRawChunks = false) =>
        new()
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
            IncludeRawChunks = includeRawChunks,
            ProviderOptions = new Dictionary<string, JsonElement>
            {
                ["gateway"] = JsonDocument
                    .Parse("""{"byok":{"openai":[{"apiKey":"sk-byok-telemetry-secret"}]}}""")
                    .RootElement,
            },
        };

    private static GatewayLanguageModelServer ErrorServer() =>
        new()
        {
            Status = HttpStatusCode.InternalServerError,
            ContentType = "application/json",
            ResponseBody = """{"error":{"message":"bad request"}}""",
        };

    private static JsonElement ProviderOptions() =>
        JsonDocument.Parse(ProviderOptionsJson).RootElement;

    /// <summary>The application-facing body keeps the credentials; only the telemetry view hides them.</summary>
    private static void ExpectTelemetryViewRedacted(
        string body,
        Func<JsonElement, JsonElement>? redact
    )
    {
        Assert.Contains(ByokSecret, body);

        var telemetryView = TelemetryRedaction
            .RedactTelemetryValue(JsonDocument.Parse(body).RootElement, redact)!
            .Value.GetRawText();
        Assert.DoesNotContain(ByokSecret, telemetryView);
        Assert.Contains("[redacted]", telemetryView);
    }

    private static void ExpectErrorBodyRedacted(Exception? error)
    {
        var body = GetRequestBodyValues(error);
        ExpectTelemetryViewRedacted(body.Value.GetRawText(), body.Redact);
        Assert.Contains(ByokSecret, GatewayTelemetryDump.Dump(error!));
    }

    private static TelemetryRedactedValue GetRequestBodyValues(Exception? error)
    {
        var cause = (error as GatewayError)?.Cause;
        return Assert.IsAssignableFrom<ApiException>(cause).RequestBodyValues
                as TelemetryRedactedValue
            ?? throw new InvalidOperationException("expected the request body to be annotated");
    }

    private static async Task<Exception> CaptureError(Func<Task> call)
    {
        try
        {
            await call();
        }
        catch (Exception error)
        {
            return error;
        }

        throw new InvalidOperationException("expected the call to throw");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "redacts the error body of a failed generate call",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Redacts_the_error_body_of_a_failed_generate_call()
    {
        var server = ErrorServer();

        var error = await CaptureError(() =>
            GatewayLanguageModelSupport
                .CreateTestModel(server)
                .DoGenerateAsync(Options(), CancellationToken.None)
        );

        // The Gateway receives the credentials unchanged.
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    """{"prompt":[{"role":"user","content":[{"type":"text","text":"Hello"}]}],"providerOptions":{"gateway":{"byok":{"openai":[{"apiKey":"sk-byok-telemetry-secret"}]}}}}"""
                ),
                server.RequestJson
            ),
            server.RequestBody
        );
        ExpectErrorBodyRedacted(error);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "redacts the error body of a failed stream call",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Redacts_the_error_body_of_a_failed_stream_call()
    {
        var server = ErrorServer();

        var error = await CaptureError(() =>
            GatewayLanguageModelSupport
                .CreateTestModel(server)
                .DoStreamAsync(Options(), CancellationToken.None)
        );

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    """{"prompt":[{"role":"user","content":[{"type":"text","text":"Hello"}]}],"includeRawChunks":false,"providerOptions":{"gateway":{"byok":{"openai":[{"apiKey":"sk-byok-telemetry-secret"}]}}}}"""
                ),
                server.RequestJson
            ),
            server.RequestBody
        );
        ExpectErrorBodyRedacted(error);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "redacts the request body metadata of a generate call",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
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

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    """{"prompt":[{"role":"user","content":[{"type":"text","text":"Hello"}]}],"providerOptions":{"gateway":{"byok":{"openai":[{"apiKey":"sk-byok-telemetry-secret"}]}}}}"""
                ),
                JsonNode.Parse(result.Request!.Body!.Value.GetRawText())
            ),
            result.Request.Body.Value.GetRawText()
        );
        ExpectTelemetryViewRedacted(
            result.Request.Body.Value.GetRawText(),
            result.Request.RedactBody
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "redacts the request body metadata of a stream call",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Redacts_the_request_body_metadata_of_a_stream_call()
    {
        var server = GatewayLanguageModelSupport.PrepareStreamResponse();

        var result = await GatewayLanguageModelSupport
            .CreateTestModel(server)
            .DoStreamAsync(Options(), CancellationToken.None);

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    """{"prompt":[{"role":"user","content":[{"type":"text","text":"Hello"}]}],"includeRawChunks":false,"providerOptions":{"gateway":{"byok":{"openai":[{"apiKey":"sk-byok-telemetry-secret"}]}}}}"""
                ),
                JsonNode.Parse(result.Request!.Body!.Value.GetRawText())
            ),
            result.Request.Body.Value.GetRawText()
        );
        ExpectTelemetryViewRedacted(
            result.Request.Body.Value.GetRawText(),
            result.Request.RedactBody
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "redacts the error body of a failed extraction call",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Redacts_the_error_body_of_a_failed_extraction_call()
    {
        var server = ErrorServer();
        var model = GatewayProvider
            .Create(
                new GatewayOptions { ApiKey = "test-token", BaseUrl = "https://api.test.com" },
                server
            )
            .ExtractionModel("openai/gpt-5");

        var error = await CaptureError(() =>
            model.DoExtractAsync(
                GatewayExtractionModelSupport.Call(providerOptions: ProviderOptionsJson)
            )
        );

        Assert.Contains(ByokSecret, server.RequestBody);
        Assert.Equal(
            ByokSecret,
            server.RequestJson["providerOptions"]!["gateway"]!["byok"]!["openai"]![0]![
                "apiKey"
            ]!.GetValue<string>()
        );
        ExpectErrorBodyRedacted(error);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "redacts the error body of a failed embedding call",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Redacts_the_error_body_of_a_failed_embedding_call()
    {
        var server = ErrorServer();
        var model = (Vercel.AI.Gateway.GatewayEmbeddingModel)
            GatewayProvider
                .Create(
                    new GatewayOptions { ApiKey = "test-token", BaseUrl = "https://api.test.com" },
                    server
                )
                .EmbeddingModel("openai/text-embedding-3-small");

        var error = await CaptureError(() =>
            model.DoEmbedAsync(
                new[] { "sunny day" },
                ProviderOptions(),
                null,
                CancellationToken.None,
                null
            )
        );

        Assert.Equal(
            ByokSecret,
            server.RequestJson["providerOptions"]!["gateway"]!["byok"]!["openai"]![0]![
                "apiKey"
            ]!.GetValue<string>()
        );
        ExpectErrorBodyRedacted(error);
    }
}
