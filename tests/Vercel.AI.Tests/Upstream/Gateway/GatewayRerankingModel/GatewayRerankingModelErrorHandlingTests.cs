// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using Vercel.AI.Gateway;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayRerankingModel.GatewayRerankingModelSupport;
using GatewayInternalServerError = Vercel.AI.Gateway.GatewayInternalServerError;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayRerankingModel;

/// <summary>Port of <c>gateway-reranking-model.test.ts</c> &gt; <c>GatewayRerankingModel &gt; error handling</c>.</summary>
public sealed class GatewayRerankingModelErrorHandlingTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-reranking-model.test.ts::GatewayRerankingModel > error handling::";

    [Fact]
    [UpstreamTest(
        Prefix + "should throw GatewayInvalidRequestError on 400",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_throw_GatewayInvalidRequestError_on_400()
    {
        var (model, _) = Setup(
            "{\"error\":{\"message\":\"Invalid documents format\",\"type\":\"invalid_request_error\"}}",
            HttpStatusCode.BadRequest
        );
        var error = await Assert.ThrowsAsync<GatewayInvalidRequestError>(() =>
            model.DoRerankAsync(Call(), CancellationToken.None)
        );
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw GatewayInternalServerError on 500",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_throw_GatewayInternalServerError_on_500()
    {
        var (model, _) = Setup(
            "{\"error\":{\"message\":\"Internal server error\",\"type\":\"internal_server_error\"}}",
            HttpStatusCode.InternalServerError
        );
        var error = await Assert.ThrowsAsync<GatewayInternalServerError>(() =>
            model.DoRerankAsync(Call(), CancellationToken.None)
        );
        Assert.Equal(500, error.StatusCode);
    }
}
