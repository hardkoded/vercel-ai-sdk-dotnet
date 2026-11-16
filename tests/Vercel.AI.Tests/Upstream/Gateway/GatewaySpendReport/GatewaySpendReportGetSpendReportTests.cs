// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Specialized;
using System.Net;
using System.Web;
using Vercel.AI.Gateway;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewaySpendReport;

/// <summary>Port of <c>gateway-spend-report.test.ts</c> &gt; <c>GatewaySpendReport &gt; getSpendReport</c>.</summary>
public sealed class GatewaySpendReportGetSpendReportTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-spend-report.test.ts::GatewaySpendReport > getSpendReport::";

    private static Vercel.AI.Gateway.GatewaySpendReport CreateSpendReport(
        GatewayTestServer server,
        Func<IReadOnlyDictionary<string, string?>>? headers = null
    ) => new(GatewayTestServer.CreateConfig(server, headers));

    private static GatewayTestServer EmptyResults() => new("{\"results\":[]}");

    private static GatewaySpendReportParams Range(
        string start = "2026-03-01",
        string end = "2026-03-25"
    ) => new() { StartDate = start, EndDate = end };

    private static NameValueCollection Query(GatewayTestServer server) =>
        HttpUtility.ParseQueryString(server.Calls[0].RequestUri!.Query);

    [Fact]
    [UpstreamTest(
        Prefix + "should fetch from the correct endpoint with required params",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_fetch_from_the_correct_endpoint_with_required_params()
    {
        var server = EmptyResults();

        await CreateSpendReport(server).GetSpendReportAsync(Range());

        Assert.Equal(HttpMethod.Get, server.Calls[0].Method);
        Assert.Equal("/v1/report", server.Calls[0].RequestUri!.AbsolutePath);
        Assert.Equal("2026-03-01", Query(server)["start_date"]);
        Assert.Equal("2026-03-25", Query(server)["end_date"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should serialize all optional query params",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_serialize_all_optional_query_params()
    {
        var server = EmptyResults();

        await CreateSpendReport(server)
            .GetSpendReportAsync(
                new GatewaySpendReportParams
                {
                    StartDate = "2026-03-01",
                    EndDate = "2026-03-25",
                    GroupBy = GatewaySpendReportGroupBy.Model,
                    DatePart = GatewaySpendReportDatePart.Hour,
                    UserId = "user_123",
                    Model = "anthropic/claude-sonnet-4.5",
                    Provider = "anthropic",
                    CredentialType = GatewayCredentialType.Byok,
                    Tags = ["production", "api"],
                }
            );

        var query = Query(server);
        Assert.Equal("model", query["group_by"]);
        Assert.Equal("hour", query["date_part"]);
        Assert.Equal("user_123", query["user_id"]);
        Assert.Equal("anthropic/claude-sonnet-4.5", query["model"]);
        Assert.Equal("anthropic", query["provider"]);
        Assert.Equal("byok", query["credential_type"]);
        Assert.Equal("production,api", query["tags"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not include optional params when not provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_include_optional_params_when_not_provided()
    {
        var server = EmptyResults();

        await CreateSpendReport(server).GetSpendReportAsync(Range());

        var keys = Query(server).AllKeys;
        foreach (
            var key in new[]
            {
                "group_by",
                "date_part",
                "user_id",
                "model",
                "provider",
                "credential_type",
                "tags",
            }
        )
        {
            Assert.DoesNotContain(key, keys);
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not include tags when array is empty",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_include_tags_when_array_is_empty()
    {
        var server = EmptyResults();

        await CreateSpendReport(server)
            .GetSpendReportAsync(
                new GatewaySpendReportParams
                {
                    StartDate = "2026-03-01",
                    EndDate = "2026-03-25",
                    Tags = [],
                }
            );

        Assert.DoesNotContain("tags", Query(server).AllKeys);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should transform snake_case response fields to camelCase",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_transform_snake_case_response_fields_to_camelCase()
    {
        var server = new GatewayTestServer(
            "{\"results\":[{\"day\":\"2026-03-01\",\"total_cost\":12.5,\"market_cost\":11.0,\"input_tokens\":50000,\"output_tokens\":10000,\"cached_input_tokens\":5000,\"cache_creation_input_tokens\":2000,\"reasoning_tokens\":1000,\"request_count\":42}]}"
        );

        var result = await CreateSpendReport(server).GetSpendReportAsync(Range(end: "2026-03-01"));

        var row = result.Results[0];
        Assert.Equal("2026-03-01", row.Day);
        Assert.Equal(12.5, row.TotalCost);
        Assert.Equal(11.0, row.MarketCost);
        Assert.Equal(50000, row.InputTokens);
        Assert.Equal(10000, row.OutputTokens);
        Assert.Equal(5000, row.CachedInputTokens);
        Assert.Equal(2000, row.CacheCreationInputTokens);
        Assert.Equal(1000, row.ReasoningTokens);
        Assert.Equal(42, row.RequestCount);
        Assert.Null(row.Hour);
        Assert.Null(row.User);
        Assert.Null(row.Model);
        Assert.Null(row.Tag);
        Assert.Null(row.Provider);
        Assert.Null(row.CredentialType);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should transform credential_type to credentialType in response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_transform_credential_type_to_credentialType_in_response()
    {
        var server = new GatewayTestServer(
            "{\"results\":[{\"credential_type\":\"byok\",\"total_cost\":5.0}]}"
        );

        var result = await CreateSpendReport(server)
            .GetSpendReportAsync(
                new GatewaySpendReportParams
                {
                    StartDate = "2026-03-01",
                    EndDate = "2026-03-25",
                    GroupBy = GatewaySpendReportGroupBy.CredentialType,
                }
            );

        var row = result.Results[0];
        Assert.Equal(GatewayCredentialType.Byok, row.CredentialType);
        Assert.Equal(5.0, row.TotalCost);
        Assert.Null(row.MarketCost);
        Assert.DoesNotContain(typeof(GatewaySpendReportRow).GetProperties(), p =>
            p.Name == "credential_type"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle groupBy model response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_groupBy_model_response()
    {
        var server = new GatewayTestServer(
            "{\"results\":[{\"model\":\"anthropic/claude-sonnet-4.5\",\"total_cost\":10.0,\"request_count\":100},{\"model\":\"openai/gpt-4o\",\"total_cost\":8.0,\"request_count\":50}]}"
        );

        var result = await CreateSpendReport(server)
            .GetSpendReportAsync(
                new GatewaySpendReportParams
                {
                    StartDate = "2026-03-01",
                    EndDate = "2026-03-25",
                    GroupBy = GatewaySpendReportGroupBy.Model,
                }
            );

        Assert.Equal(2, result.Results.Count);
        Assert.Equal("anthropic/claude-sonnet-4.5", result.Results[0].Model);
        Assert.Equal("openai/gpt-4o", result.Results[1].Model);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle empty results", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_empty_results()
    {
        var result = await CreateSpendReport(EmptyResults()).GetSpendReportAsync(Range());

        Assert.Empty(result.Results);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit optional metric fields when not present",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_optional_metric_fields_when_not_present()
    {
        var server = new GatewayTestServer(
            "{\"results\":[{\"day\":\"2026-03-01\",\"total_cost\":1.5}]}"
        );

        var result = await CreateSpendReport(server).GetSpendReportAsync(Range(end: "2026-03-01"));

        var row = result.Results[0];
        Assert.Equal("2026-03-01", row.Day);
        Assert.Equal(1.5, row.TotalCost);
        Assert.Null(row.MarketCost);
        Assert.Null(row.InputTokens);
        Assert.Null(row.OutputTokens);
        Assert.Null(row.CachedInputTokens);
        Assert.Null(row.CacheCreationInputTokens);
        Assert.Null(row.ReasoningTokens);
        Assert.Null(row.RequestCount);
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass headers correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_headers_correctly()
    {
        var server = EmptyResults();
        var report = CreateSpendReport(server, () =>
            new Dictionary<string, string?>
            {
                ["Authorization"] = "Bearer custom-token",
                ["Custom-Header"] = "custom-value",
            }
        );

        await report.GetSpendReportAsync(Range());

        var headers = server.RequestHeaders();
        Assert.Equal(2, headers.Count);
        Assert.Equal("Bearer custom-token", headers["authorization"]);
        Assert.Equal("custom-value", headers["custom-header"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle 401 authentication errors",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_401_authentication_errors()
    {
        var server = new GatewayTestServer(
            "{\"error\":{\"message\":\"Unauthorized\",\"type\":\"authentication_error\"}}",
            HttpStatusCode.Unauthorized
        );

        var error = await Assert.ThrowsAsync<GatewayAuthenticationError>(() =>
            CreateSpendReport(server).GetSpendReportAsync(Range())
        );

        Assert.True(GatewayAuthenticationError.IsInstance(error));
        Assert.Equal(401, error.StatusCode);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle 429 rate limit errors",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_429_rate_limit_errors()
    {
        var server = new GatewayTestServer(
            "{\"error\":{\"message\":\"Rate limit exceeded\",\"type\":\"rate_limit_exceeded\"}}",
            (HttpStatusCode)429
        );

        await Assert.ThrowsAsync<GatewayRateLimitError>(() =>
            CreateSpendReport(server).GetSpendReportAsync(Range())
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle 500 internal server errors",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_500_internal_server_errors()
    {
        var server = new GatewayTestServer(
            "{\"error\":{\"message\":\"Internal server error\",\"type\":\"internal_server_error\"}}",
            HttpStatusCode.InternalServerError
        );

        await Assert.ThrowsAsync<GatewayInternalServerError>(() =>
            CreateSpendReport(server).GetSpendReportAsync(Range())
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle malformed JSON error responses",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_malformed_JSON_error_responses()
    {
        var server = new GatewayTestServer("{ invalid json", HttpStatusCode.InternalServerError);

        var error = await Assert.ThrowsAsync<GatewayResponseError>(() =>
            CreateSpendReport(server).GetSpendReportAsync(Range())
        );

        Assert.True(GatewayResponseError.IsInstance(error));
        Assert.Equal(500, error.StatusCode);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use custom fetch function when provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_custom_fetch_function_when_provided()
    {
        var mockFetch = new GatewayTestServer(
            "{\"results\":[{\"day\":\"2026-03-01\",\"total_cost\":5.0}]}"
        );

        var result = await CreateSpendReport(mockFetch)
            .GetSpendReportAsync(Range(end: "2026-03-01"));

        Assert.NotEmpty(mockFetch.Calls);
        Assert.Equal(5.0, result.Results[0].TotalCost);
    }
}
