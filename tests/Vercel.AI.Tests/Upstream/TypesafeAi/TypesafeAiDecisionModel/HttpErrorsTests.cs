// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.TypesafeAi.TypesafeAiDecisionModel.TypesafeAiDecisionModelSupport;

namespace Vercel.AI.Tests.Upstream.TypesafeAi.TypesafeAiDecisionModel;

/// <summary>Port of <c>typesafe-ai-decision-model.test.ts</c> &gt; <c>HTTP errors</c>.</summary>
public sealed class HttpErrorsTests
{
    private const string Prefix = "packages/typesafe-ai/src/typesafe-ai-decision-model.test.ts::HTTP errors::";

    private static Task<ApiCallError> Fail(TypeSafeAiServer server)
    {
        return Assert.ThrowsAsync<ApiCallError>(() => CreateProvider(server).DecisionModel("jev-latest").DoDecideAsync(Call(), CancellationToken.None));
    }

    [Theory]
    [InlineData(401)]
    [InlineData(422)]
    [InlineData(429)]
    [InlineData(529)]
    [UpstreamTest(Prefix + "preserves status and retryability for %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_status_and_retryability_for_status(int status)
    {
        var server = Server();
        server.Status = (HttpStatusCode)status;
        server.ResponseBody = "{\"detail\":\"Provider error\"}";
        var error = await Fail(server);
        Assert.Equal("AI_APICallError", error.ErrorName);
        Assert.Equal(status, error.StatusCode);
        Assert.Equal("Provider error", error.Message);
        Assert.Equal(status == 429 || status == 529, error.IsRetryable);
        Assert.Single(server.Calls);
    }

    [Theory]
    [InlineData("{\"detail\":[{\"loc\":[\"body\",\"questions\"],\"msg\":\"Invalid question\"}]}", "[{\"loc\":[\"body\",\"questions\"],\"msg\":\"Invalid question\"}]")]
    [InlineData("{\"error\":{\"message\":\"Overloaded\"}}", "Overloaded")]
    [InlineData("{\"error\":\"Overloaded\"}", "Overloaded")]
    [InlineData("{\"message\":\"Overloaded\"}", "Overloaded")]
    [UpstreamTest(Prefix + "handles provider error envelopes %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Handles_provider_error_envelopes(string body, string message)
    {
        var server = Server();
        server.Status = HttpStatusCode.UnprocessableEntity;
        server.ResponseBody = body;
        var error = await Fail(server);
        Assert.Equal("AI_APICallError", error.ErrorName);
        Assert.Equal(422, error.StatusCode);
        Assert.Equal(message, error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "surfaces a bare error_type code, observed in production as max_tokens_exceeded", Coverage = UpstreamCoverage.Covered)]
    public async Task Surfaces_a_bare_error_type_code_observed_in_production_as_max_tokens_exceeded()
    {
        var server = Server();
        server.Status = HttpStatusCode.BadRequest;
        server.ResponseBody = "{\"error_type\":\"max_tokens_exceeded\"}";
        var error = await Fail(server);
        Assert.Equal("AI_APICallError", error.ErrorName);
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("max_tokens_exceeded", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "prefers a prose field over the error_type code", Coverage = UpstreamCoverage.Covered)]
    public async Task Prefers_a_prose_field_over_the_error_type_code()
    {
        var server = Server();
        server.Status = HttpStatusCode.BadRequest;
        server.ResponseBody = "{\"error_type\":\"max_tokens_exceeded\",\"message\":\"Input exceeds the model context window\"}";
        var error = await Fail(server);
        Assert.Equal("Input exceeds the model context window", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "still falls back when the body carries no recognised field", Coverage = UpstreamCoverage.Covered)]
    public async Task Still_falls_back_when_the_body_carries_no_recognised_field()
    {
        var server = Server();
        server.Status = HttpStatusCode.BadRequest;
        server.ResponseBody = "{\"unrecognised\":\"shape\"}";
        var error = await Fail(server);
        Assert.Equal("TypeSafe request failed", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects malformed successful responses", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_malformed_successful_responses()
    {
        var server = Server();
        server.ResponseBody = "{\"answers\":{\"test\":{\"type\":\"noul\"}}}";
        await Fail(server);
    }
}
