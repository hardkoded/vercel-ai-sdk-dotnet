// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using static Vercel.AI.Tests.Upstream.Middleware.DefaultSettingsMiddleware.Support;

namespace Vercel.AI.Tests.Upstream.Middleware.DefaultSettingsMiddleware;

/// <summary>Port of <c>default-settings-middleware.test.ts</c> &gt; <c>defaultSettingsMiddleware &gt; providerOptions</c>.</summary>
public sealed class ProviderOptionsTests
{
    private const string Id = FileId + "providerOptions::";

    [Fact]
    [UpstreamTest(
        Id + "should handle empty default providerOptions",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_empty_default_providerOptions()
    {
        var parameters = Params();
        parameters.ProviderOptions = new Dictionary<string, JsonElement>
        {
            ["openai"] = Json("{\"user\":\"param-user\"}"),
        };

        var result = await Transform(
            new() { ProviderOptions = new Dictionary<string, JsonElement>() },
            parameters
        );

        var options = Assert.Single(result.ProviderOptions!);
        Assert.Equal("openai", options.Key);
        AssertJsonEqual("{\"user\":\"param-user\"}", options.Value);
    }

    [Fact]
    [UpstreamTest(
        Id + "should handle empty param providerOptions",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_empty_param_providerOptions()
    {
        var parameters = Params();
        parameters.ProviderOptions = new Dictionary<string, JsonElement>();

        var result = await Transform(
            new()
            {
                ProviderOptions = new Dictionary<string, JsonElement>
                {
                    ["anthropic"] = Json("{\"user\":\"default-user\"}"),
                },
            },
            parameters
        );

        var options = Assert.Single(result.ProviderOptions!);
        Assert.Equal("anthropic", options.Key);
        AssertJsonEqual("{\"user\":\"default-user\"}", options.Value);
    }

    [Fact]
    [UpstreamTest(
        Id + "should handle both providerOptions being undefined",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_both_providerOptions_being_undefined()
    {
        var result = await Transform(new());

        Assert.Null(result.ProviderOptions);
    }
}
