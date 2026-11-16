// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Web;
using Vercel.AI.Gateway;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayFetchMetadata;

/// <summary>Port of <c>gateway-fetch-metadata.test.ts</c> &gt; <c>GatewayFetchMetadata &gt; getCredits</c>.</summary>
public sealed class GatewayFetchMetadataGetCreditsTests
{
    private const string Prefix = "packages/gateway/src/gateway-fetch-metadata.test.ts::GatewayFetchMetadata > getCredits::";

    private static Vercel.AI.Gateway.GatewayFetchMetadata CreateFetcher(GatewayTestServer server, Func<IReadOnlyDictionary<string, string?>>? headers = null) =>
        new(GatewayTestServer.CreateConfig(server, headers));

    private static GatewayTestServer Credits(string balance, string totalUsed) =>
        new("{\"balance\":\"" + balance + "\",\"total_used\":\"" + totalUsed + "\"}");

    private static GatewayTestServer Error(string message, string type, HttpStatusCode status) =>
        new("{\"error\":{\"message\":\"" + message + "\",\"type\":\"" + type + "\"}}", status);

    [Fact]
    [UpstreamTest(Prefix + "should fetch credits from the correct endpoint", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_fetch_credits_from_the_correct_endpoint()
    {
        var result = await CreateFetcher(Credits("150.50", "75.25")).GetCreditsAsync();

        Assert.Equal("150.50", result.Balance);
        Assert.Equal("75.25", result.TotalUsed);
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass headers correctly to credits endpoint", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_headers_correctly_to_credits_endpoint()
    {
        var server = Credits("100.00", "50.00");
        var metadata = CreateFetcher(server, () => new Dictionary<string, string?>
        {
            ["Authorization"] = "Bearer custom-token",
            ["Custom-Header"] = "custom-value",
        });

        var result = await metadata.GetCreditsAsync();

        var headers = server.RequestHeaders();
        Assert.Equal(2, headers.Count);
        Assert.Equal("Bearer custom-token", headers["authorization"]);
        Assert.Equal("custom-value", headers["custom-header"]);
        Assert.Equal("100.00", result.Balance);
        Assert.Equal("50.00", result.TotalUsed);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not add team query parameters without a team header", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_add_team_query_parameters_without_a_team_header()
    {
        var server = Credits("1.00", "0.00");

        await CreateFetcher(server).GetCreditsAsync();

        Assert.Equal("https://api.example.com/v1/credits", server.Calls[0].RequestUri!.ToString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should forward a team ID from the team header as the teamId query parameter", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_forward_a_team_ID_from_the_team_header_as_the_teamId_query_parameter()
    {
        var server = Credits("1.00", "0.00");

        await CreateFetcher(server, () => new Dictionary<string, string?>
        {
            ["Authorization"] = "Bearer vca_test-token",
            ["x-vercel-ai-gateway-team"] = "team_123",
        }).GetCreditsAsync();

        Assert.Equal("https://api.example.com/v1/credits?teamId=team_123", server.Calls[0].RequestUri!.ToString());
        Assert.Equal("team_123", server.RequestHeaders()["x-vercel-ai-gateway-team"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should forward a team slug from the team header as the slug query parameter", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_forward_a_team_slug_from_the_team_header_as_the_slug_query_parameter()
    {
        var server = Credits("1.00", "0.00");

        await CreateFetcher(server, () => new Dictionary<string, string?>
        {
            ["Authorization"] = "Bearer vca_test-token",
            ["X-Vercel-AI-Gateway-Team"] = "team-o'brien",
        }).GetCreditsAsync();

        var url = server.Calls[0].RequestUri!;
        var query = HttpUtility.ParseQueryString(url.Query);
        Assert.Equal("/v1/credits", url.AbsolutePath);
        Assert.Equal("team-o'brien", query["slug"]);
        Assert.Null(query["teamId"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle API errors for credits endpoint", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_API_errors_for_credits_endpoint()
    {
        var server = Error("Invalid API key", "authentication_error", HttpStatusCode.Unauthorized);

        await Assert.ThrowsAsync<GatewayAuthenticationError>(() => CreateFetcher(server).GetCreditsAsync());
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle rate limit errors for credits endpoint", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_rate_limit_errors_for_credits_endpoint()
    {
        var server = Error("Rate limit exceeded", "rate_limit_exceeded", (HttpStatusCode)429);

        await Assert.ThrowsAsync<GatewayRateLimitError>(() => CreateFetcher(server).GetCreditsAsync());
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle internal server errors for credits endpoint", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_internal_server_errors_for_credits_endpoint()
    {
        var server = Error("Database unavailable", "internal_server_error", HttpStatusCode.InternalServerError);

        await Assert.ThrowsAsync<GatewayInternalServerError>(() => CreateFetcher(server).GetCreditsAsync());
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle malformed credits response", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_malformed_credits_response()
    {
        var result = await CreateFetcher(Credits("not-a-number", "75.25")).GetCreditsAsync();

        Assert.Equal("not-a-number", result.Balance);
        Assert.Equal("75.25", result.TotalUsed);
    }

    [Fact]
    [UpstreamTest(Prefix + "should use custom fetch function when provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_custom_fetch_function_when_provided()
    {
        var customFetch = Credits("200.00", "100.50");

        var result = await CreateFetcher(customFetch).GetCreditsAsync();

        Assert.Equal("200.00", result.Balance);
        Assert.Equal("100.50", result.TotalUsed);
        Assert.Equal("https://api.example.com/v1/credits", customFetch.Calls[0].RequestUri!.ToString());
        Assert.Equal(HttpMethod.Get, customFetch.Calls[0].Method);
        Assert.Equal("Bearer test-token", customFetch.RequestHeaders()["authorization"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert API call errors to Gateway errors", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_convert_API_call_errors_to_Gateway_errors()
    {
        var server = Error("Forbidden access", "authentication_error", HttpStatusCode.Forbidden);

        var error = await Assert.ThrowsAsync<GatewayAuthenticationError>(() => CreateFetcher(server).GetCreditsAsync());

        Assert.True(GatewayAuthenticationError.IsInstance(error));
        Assert.Contains("No authentication provided", error.Message);
        Assert.Equal("authentication_error", error.Type);
        Assert.Equal(403, error.StatusCode);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle malformed JSON error responses", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_malformed_JSON_error_responses()
    {
        var server = new GatewayTestServer("{ invalid json", HttpStatusCode.InternalServerError);

        var error = await Assert.ThrowsAsync<GatewayResponseError>(() => CreateFetcher(server).GetCreditsAsync());

        Assert.True(GatewayResponseError.IsInstance(error));
        Assert.Equal(500, error.StatusCode);
        Assert.Equal("response_error", error.Type);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not double-wrap existing Gateway errors", Coverage = UpstreamCoverage.Covered)]
    public void Should_not_double_wrap_existing_Gateway_errors()
    {
        var existingError = new GatewayAuthenticationError("Already wrapped", 401);

        try
        {
            throw existingError;
        }
        catch (Exception error) when (GatewayError.IsInstance(error))
        {
            Assert.Same(existingError, GatewayErrors.AsGatewayError(error));
            Assert.Equal("Already wrapped", error.Message);
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve error cause chain", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_error_cause_chain()
    {
        var server = Error("Token expired", "authentication_error", HttpStatusCode.Unauthorized);

        var error = await Assert.ThrowsAsync<GatewayAuthenticationError>(() => CreateFetcher(server).GetCreditsAsync());

        Assert.True(GatewayAuthenticationError.IsInstance(error));
        Assert.NotNull(error.Cause);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle empty response", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_empty_response()
    {
        var result = await CreateFetcher(Credits("0.00", "0.00")).GetCreditsAsync();

        Assert.Equal("0.00", result.Balance);
        Assert.Equal("0.00", result.TotalUsed);
    }
}
