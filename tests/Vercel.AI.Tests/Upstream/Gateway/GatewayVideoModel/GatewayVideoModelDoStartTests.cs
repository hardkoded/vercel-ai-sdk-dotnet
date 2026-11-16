// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json.Nodes;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayVideoModel.GatewayVideoModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayVideoModel;

/// <summary>Port of <c>gateway-video-model.test.ts</c> &gt; <c>GatewayVideoModel &gt; doStart</c>.</summary>
public sealed class GatewayVideoModelDoStartTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-video-model.test.ts::GatewayVideoModel > doStart::";

    private static Vercel.AI.Operations.VideoModelCall BaseCallOptions(
        string? webhookUrl = null,
        IReadOnlyDictionary<string, string>? headers = null
    ) =>
        Call(prompt: "A beautiful sunset over mountains", webhookUrl: webhookUrl, headers: headers);

    [Fact]
    [UpstreamTest(
        Prefix + "should call the Gateway start endpoint and return the operation",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_the_Gateway_start_endpoint_and_return_the_operation()
    {
        var handler = new UpstreamRecordingHandler(
            "{\"operation\":{\"gatewayJobId\":\"job_123\"},\"warnings\":[{\"type\":\"other\",\"message\":\"queued\"}],\"providerMetadata\":{\"gateway\":{\"asyncJob\":{\"jobId\":\"job_123\",\"status\":\"queued\"}}}}"
        );
        var call = Call(
            prompt: "A beautiful sunset over mountains",
            aspectRatio: "16:9",
            resolution: "1280x720",
            duration: 5,
            fps: 24,
            seed: 123,
            generateAudio: true,
            providerOptions: "{\"gateway\":{\"tags\":[\"async\"]}}",
            webhookUrl: "https://example.com/webhook"
        );

        var result = await CreateTestModel(handler).DoStartAsync(call, CancellationToken.None);

        Assert.True(
            Equal(
                "{\"gatewayJobId\":\"job_123\"}",
                JsonNode.Parse(((System.Text.Json.JsonElement)result.Operation!).GetRawText())
            )
        );
        Assert.Equal(
            new[] { ("other", (string?)null, (string?)null, (string?)"queued") },
            result.Warnings.Select(Warning).ToArray()
        );
        Assert.True(
            Equal(
                "{\"gateway\":{\"asyncJob\":{\"jobId\":\"job_123\",\"status\":\"queued\"}}}",
                Node(result.ProviderMetadata)
            )
        );
        Assert.Equal(ModelId, result.Response.ModelId);
        Assert.NotNull(result.Response.Timestamp);
        Assert.NotNull(result.Response.Headers);

        Assert.Equal("https://api.test.com/video-model/start", handler.Uri);
        Assert.Equal("Bearer test-token", handler.Headers["Authorization"]);
        Assert.Equal("4", handler.Headers["ai-video-model-specification-version"]);
        Assert.Equal(ModelId, handler.Headers["ai-model-id"]);

        // The spec-side webhookUrl option maps onto the Gateway's callbackUrl wire field (its completion-webhook contract).
        Assert.True(
            Equal(
                "{\"prompt\":\"A beautiful sunset over mountains\",\"n\":1,\"aspectRatio\":\"16:9\",\"resolution\":\"1280x720\",\"duration\":5,\"fps\":24,\"seed\":123,\"generateAudio\":true,\"providerOptions\":{\"gateway\":{\"tags\":[\"async\"]}},\"callbackUrl\":\"https://example.com/webhook\"}",
                Body(handler)
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit optional fields and callbackUrl when webhookUrl is not provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_optional_fields_and_callbackUrl_when_webhookUrl_is_not_provided()
    {
        var handler = new UpstreamRecordingHandler(
            "{\"operation\":{\"gatewayJobId\":\"job_123\"}}"
        );

        var result = await CreateTestModel(handler)
            .DoStartAsync(BaseCallOptions(), CancellationToken.None);

        // warnings defaults to an empty array when omitted by the gateway
        Assert.Empty(result.Warnings);
        Assert.True(
            Equal(
                "{\"prompt\":\"A beautiful sunset over mountains\",\"n\":1,\"providerOptions\":{}}",
                Body(handler)
            )
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should map gateway errors", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_gateway_errors()
    {
        var handler = new UpstreamRecordingHandler(
            "{\"error\":{\"message\":\"Invalid request\",\"code\":\"invalid_request\"}}",
            HttpStatusCode.BadRequest
        );

        await Assert.ThrowsAnyAsync<Exception>(() =>
            CreateTestModel(handler).DoStartAsync(BaseCallOptions(), CancellationToken.None)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should forward the caller-supplied idempotency key",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_forward_the_caller_supplied_idempotency_key()
    {
        var handler = new UpstreamRecordingHandler(
            "{\"operation\":{\"gatewayJobId\":\"job_123\"}}"
        );

        // The key is minted by GenerateVideo once per logical start and passed down, so a retry of a lost response
        // dedupes at the gateway rather than starting a second billable generation. The model must not invent one.
        await CreateTestModel(handler)
            .DoStartAsync(
                BaseCallOptions(
                    headers: new Dictionary<string, string>
                    {
                        ["idempotency-key"] = "aisdk_vid_abc123",
                    }
                ),
                CancellationToken.None
            );

        Assert.Equal("aisdk_vid_abc123", handler.Headers["idempotency-key"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send no idempotency key when the caller supplies none",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_no_idempotency_key_when_the_caller_supplies_none()
    {
        var handler = new UpstreamRecordingHandler(
            "{\"operation\":{\"gatewayJobId\":\"job_123\"}}"
        );

        await CreateTestModel(handler).DoStartAsync(BaseCallOptions(), CancellationToken.None);

        Assert.False(handler.Headers.ContainsKey("idempotency-key"));
    }
}
