// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Middleware.DefaultSettingsMiddleware.Support;

namespace Vercel.AI.Tests.Upstream.Middleware.DefaultSettingsMiddleware;

/// <summary>Port of <c>default-settings-middleware.test.ts</c> &gt; <c>defaultSettingsMiddleware &gt; headers</c>.</summary>
public sealed class HeadersTests
{
    private const string Id = FileId + "headers::";

    [Fact]
    [UpstreamTest(Id + "should merge headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_merge_headers()
    {
        var parameters = Params();
        parameters.Headers = new Dictionary<string, string?> { ["X-Custom-Header"] = "test2" };

        var result = await Transform(
            new()
            {
                Headers = new Dictionary<string, string?>
                {
                    ["X-Custom-Header"] = "test",
                    ["X-Another-Header"] = "test2",
                },
            },
            parameters
        );

        Assert.Equal(
            new Dictionary<string, string?>
            {
                ["X-Custom-Header"] = "test2",
                ["X-Another-Header"] = "test2",
            },
            result.Headers
        );
    }

    [Fact]
    [UpstreamTest(Id + "should handle empty default headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_empty_default_headers()
    {
        var parameters = Params();
        parameters.Headers = new Dictionary<string, string?> { ["X-Param-Header"] = "param" };

        var result = await Transform(
            new() { Headers = new Dictionary<string, string?>() },
            parameters
        );

        Assert.Equal(
            new Dictionary<string, string?> { ["X-Param-Header"] = "param" },
            result.Headers
        );
    }

    [Fact]
    [UpstreamTest(Id + "should handle empty param headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_empty_param_headers()
    {
        var parameters = Params();
        parameters.Headers = new Dictionary<string, string?>();

        var result = await Transform(
            new()
            {
                Headers = new Dictionary<string, string?> { ["X-Default-Header"] = "default" },
            },
            parameters
        );

        Assert.Equal(
            new Dictionary<string, string?> { ["X-Default-Header"] = "default" },
            result.Headers
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should handle both headers being undefined",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_both_headers_being_undefined()
    {
        var result = await Transform(new());

        Assert.Null(result.Headers);
    }
}
