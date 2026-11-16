// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using Vercel.AI.Gateway;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayTranscriptionModel.GatewayTranscriptionModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayTranscriptionModel;

/// <summary>Port of <c>gateway-transcription-model.test.ts</c> &gt; <c>GatewayTranscriptionModel &gt; error handling</c>.</summary>
public sealed class GatewayTranscriptionModelErrorHandlingTests
{
    private const string Prefix = "packages/gateway/src/gateway-transcription-model.test.ts::GatewayTranscriptionModel > error handling::";

    [Fact]
    [UpstreamTest(Prefix + "should throw GatewayInvalidRequestError on 400", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_throw_GatewayInvalidRequestError_on_400()
    {
        var handler = new UpstreamRecordingHandler("""{"error":{"message":"Invalid audio format","type":"invalid_request_error"}}""", HttpStatusCode.BadRequest);

        var error = await Assert.ThrowsAsync<GatewayInvalidRequestError>(() => DoGenerate(CreateTestModel(handler)));

        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    [UpstreamTest(Prefix + "should throw GatewayInternalServerError on 500", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_throw_GatewayInternalServerError_on_500()
    {
        var handler = new UpstreamRecordingHandler("""{"error":{"message":"Internal server error","type":"internal_server_error"}}""", HttpStatusCode.InternalServerError);

        var error = await Assert.ThrowsAsync<GatewayInternalServerError>(() => DoGenerate(CreateTestModel(handler)));

        Assert.Equal(500, error.StatusCode);
    }
}
