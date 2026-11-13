// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Replicate;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.CancelPrediction;

/// <summary>Port of <c>cancel-prediction.test.ts</c> &gt; <c>cancelReplicatePrediction</c>.</summary>
public sealed class CancelReplicatePredictionTests
{
    private const string Id = "packages/replicate/src/cancel-prediction.test.ts::cancelReplicatePrediction::";

    [Fact]
    [UpstreamTest(Id + "posts to the prediction cancel endpoint with the configured headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Posts_to_the_prediction_cancel_endpoint_with_the_configured_headers()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json("{\"status\":\"canceled\"}"));
        var provider = ReplicateProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-token" }, handler);

        await provider.CancelPredictionAsync("s7x1e3dcmhrmc0cm8rbatcneec");

        var call = Assert.Single(handler.Calls);
        Assert.Equal("https://api.replicate.com/v1/predictions/s7x1e3dcmhrmc0cm8rbatcneec/cancel", call.Uri.AbsoluteUri);
        Assert.Equal(HttpMethod.Post, call.Method);
        Assert.Equal("Bearer test-api-token", call.Header("authorization"));
        // no idempotency key on the cancel request (and no abort signal: the method takes no cancellation token)
        Assert.Null(call.Header("idempotency-key"));
    }

    [Fact]
    [UpstreamTest(Id + "encodes the prediction id in the URL path", Coverage = UpstreamCoverage.Covered)]
    public async Task Encodes_the_prediction_id_in_the_URL_path()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json("{\"status\":\"canceled\"}"));
        var provider = ReplicateProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-token" }, handler);

        await provider.CancelPredictionAsync("owner/prediction id");

        Assert.Equal("https://api.replicate.com/v1/predictions/owner%2Fprediction%20id/cancel", Assert.Single(handler.Calls).Uri.AbsoluteUri);
    }

    [Theory]
    [InlineData("the network request fails")]
    [InlineData("the API responds with an error status")]
    [UpstreamTest(Id + "swallows the error when $name", Coverage = UpstreamCoverage.Covered)]
    public async Task Swallows_the_error_when_name(string name)
    {
        var handler = new ParityHandler(_ => name == "the network request fails"
            ? throw new HttpRequestException("fetch failed")
            : new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent("{\"detail\":\"Something went wrong\"}") });
        var provider = ReplicateProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-token" }, handler);

        await provider.CancelPredictionAsync("s7x1e3dcmhrmc0cm8rbatcneec");

        Assert.Single(handler.Calls);
    }

    [Fact]
    [UpstreamTest(Id + "swallows errors while resolving the headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Swallows_errors_while_resolving_the_headers()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json("{\"status\":\"canceled\"}"));
        var provider = ReplicateProvider.Create(new OpenAICompatibleOptions { RequireApiKey = true }, handler);

        await provider.CancelPredictionAsync("s7x1e3dcmhrmc0cm8rbatcneec");

        Assert.Empty(handler.Calls);
    }
}
