// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using Vercel.AI.Gateway;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayDecisionModel.GatewayDecisionModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayDecisionModel;

/// <summary>Port of <c>gateway-decision-model.test.ts</c> &gt; <c>GatewayDecisionModel &gt; error handling</c>.</summary>
public sealed class GatewayDecisionModelErrorHandlingTests
{
    private const string Prefix = "packages/gateway/src/gateway-decision-model.test.ts::GatewayDecisionModel > error handling::";

    [Fact]
    [UpstreamTest(Prefix + "should throw GatewayInvalidRequestError on 400", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_throw_GatewayInvalidRequestError_on_400()
    {
        var body = "{\"error\":{\"message\":\"Invalid questions format\",\"type\":\"invalid_request_error\"}}";
        var error = await Assert.ThrowsAsync<GatewayInvalidRequestError>(() => Setup(body, HttpStatusCode.BadRequest).Model.DoDecideAsync(Call()));
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    [UpstreamTest(Prefix + "should throw GatewayInternalServerError on 500", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_throw_GatewayInternalServerError_on_500()
    {
        var body = "{\"error\":{\"message\":\"Internal server error\",\"type\":\"internal_server_error\"}}";
        var error = await Assert.ThrowsAsync<GatewayInternalServerError>(() => Setup(body, HttpStatusCode.InternalServerError).Model.DoDecideAsync(Call()));
        Assert.Equal(500, error.StatusCode);
    }
}
