// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Web;
using Vercel.AI.Gateway;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayProviderTests;

/// <summary>Port of <c>gateway-provider.test.ts</c> &gt; <c>GatewayProvider &gt; getSpendReport method</c>.</summary>
[Collection("GatewayAuthEnvironment")]
public sealed class GatewayProviderGetSpendReportMethodTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-provider.test.ts::GatewayProvider > getSpendReport method::";
    private const string Results =
        "{\"results\":[{\"day\":\"2026-03-01\",\"total_cost\":10.5,\"request_count\":25}]}";

    private static GatewaySpendReportParams Range() =>
        new() { StartDate = "2026-03-01", EndDate = "2026-03-25" };

    [Fact]
    [UpstreamTest(
        Prefix + "should fetch spend report successfully",
        Coverage = UpstreamCoverage.Covered,
        Note = "The upstream fetch: undefined is the provider's own Http, which the port always sets."
    )]
    public async Task Should_fetch_spend_report_successfully()
    {
        var server = new GatewayTestServer(Results);
        var provider = GatewayProvider.Create(new GatewayOptions { ApiKey = "test-key" }, server);

        var report = await provider.GetSpendReportAsync(Range());

        Assert.Single(report.Results);
        Assert.Equal("2026-03-01", report.Results[0].Day);
        Assert.Equal(
            "https://ai-gateway.vercel.sh/v1/report",
            server.Calls[0].RequestUri!.GetLeftPart(UriPartial.Path)
        );
        var config = provider.CreateConfig();
        Assert.Equal("https://ai-gateway.vercel.sh/v4/ai", config.BaseUrl);
        Assert.NotNull(config.Headers);
        Assert.Same(provider.Http, config.Http);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass params through to GatewaySpendReport",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_params_through_to_GatewaySpendReport()
    {
        var server = new GatewayTestServer("{\"results\":[]}");
        var provider = GatewayProvider.Create(new GatewayOptions { ApiKey = "test-key" }, server);

        await provider.GetSpendReportAsync(
            new GatewaySpendReportParams
            {
                StartDate = "2026-03-01",
                EndDate = "2026-03-25",
                GroupBy = GatewaySpendReportGroupBy.Model,
                DatePart = GatewaySpendReportDatePart.Day,
                UserId = "user-123",
                Model = "anthropic/claude-sonnet-4.6",
                Tags = ["production", "api"],
            }
        );

        var query = HttpUtility.ParseQueryString(server.Calls[0].RequestUri!.Query);
        Assert.Equal("2026-03-01", query["start_date"]);
        Assert.Equal("2026-03-25", query["end_date"]);
        Assert.Equal("model", query["group_by"]);
        Assert.Equal("day", query["date_part"]);
        Assert.Equal("user-123", query["user_id"]);
        Assert.Equal("anthropic/claude-sonnet-4.6", query["model"]);
        Assert.Equal("production,api", query["tags"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should work with custom baseURL", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_work_with_custom_baseURL()
    {
        var server = new GatewayTestServer("{\"results\":[]}");
        var provider = GatewayProvider.Create(
            new GatewayOptions
            {
                ApiKey = "test-key",
                BaseUrl = "https://custom-gateway.example.com/v4/ai",
            },
            server
        );

        await provider.GetSpendReportAsync(Range());

        Assert.Equal(
            "https://custom-gateway.example.com/v1/report",
            server.Calls[0].RequestUri!.GetLeftPart(UriPartial.Path)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should work with custom fetch function",
        Coverage = UpstreamCoverage.Covered,
        Note = "The HTTP handler passed to GatewayProvider.Create stands in for the custom fetch."
    )]
    public async Task Should_work_with_custom_fetch_function()
    {
        var server = new GatewayTestServer("{\"results\":[]}");
        var provider = GatewayProvider.Create(new GatewayOptions { ApiKey = "test-key" }, server);

        await provider.GetSpendReportAsync(Range());

        Assert.Single(server.Calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle errors from the spend report endpoint",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_errors_from_the_spend_report_endpoint()
    {
        var server = new GatewayTestServer(
            "{\"error\":{\"message\":\"Reporting service unavailable\",\"type\":\"internal_server_error\"}}",
            HttpStatusCode.InternalServerError
        );
        var provider = GatewayProvider.Create(new GatewayOptions { ApiKey = "test-key" }, server);

        var error = await Assert.ThrowsAsync<GatewayInternalServerError>(() =>
            provider.GetSpendReportAsync(Range())
        );

        Assert.Contains("Reporting service unavailable", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should work with OIDC authentication in spend reports",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_work_with_OIDC_authentication_in_spend_reports()
    {
        var oidcCalls = 0;
        VercelEnvironment.OidcTokenOverride = _ =>
        {
            oidcCalls++;
            return Task.FromResult("oidc-token");
        };
        try
        {
            var server = new GatewayTestServer("{\"results\":[]}");
            var provider = GatewayProvider.Create(
                new GatewayOptions { ApiKey = string.Empty },
                server
            );

            var report = await provider.GetSpendReportAsync(Range());

            Assert.NotNull(report);
            Assert.Equal(1, oidcCalls);
            Assert.Equal("Bearer oidc-token", server.RequestHeaders()["authorization"]);
        }
        finally
        {
            VercelEnvironment.OidcTokenOverride = null;
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should be available on the provider interface",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_be_available_on_the_provider_interface()
    {
        var provider = GatewayProvider.Create(new GatewayOptions { ApiKey = "test-key" });

        Func<
            GatewaySpendReportParams,
            CancellationToken,
            Task<GatewaySpendReportResponse>
        > getSpendReport = provider.GetSpendReportAsync;

        Assert.NotNull(getSpendReport);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should be available on the default gateway export",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_be_available_on_the_default_gateway_export()
    {
        var gateway = GatewayProvider.Create();

        Func<
            GatewaySpendReportParams,
            CancellationToken,
            Task<GatewaySpendReportResponse>
        > getSpendReport = gateway.GetSpendReportAsync;

        Assert.NotNull(getSpendReport);
    }
}
