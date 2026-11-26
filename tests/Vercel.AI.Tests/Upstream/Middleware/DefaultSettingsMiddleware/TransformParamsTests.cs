// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Middleware.DefaultSettingsMiddleware.Support;

namespace Vercel.AI.Tests.Upstream.Middleware.DefaultSettingsMiddleware;

/// <summary>Port of <c>default-settings-middleware.test.ts</c> &gt; <c>defaultSettingsMiddleware &gt; transformParams</c>.</summary>
public sealed class TransformParamsTests
{
    private const string Id = FileId + "transformParams::";

    [Fact]
    [UpstreamTest(Id + "should apply default settings", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_apply_default_settings()
    {
        var result = await Transform(new() { Temperature = 0.7 });

        Assert.Equal(0.7, result.Temperature);
    }

    [Fact]
    [UpstreamTest(
        Id + "should give precedence to user-provided settings",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_give_precedence_to_user_provided_settings()
    {
        var parameters = Params();
        parameters.Temperature = 0.5;

        var result = await Transform(new() { Temperature = 0.7 }, parameters);

        Assert.Equal(0.5, result.Temperature);
    }

    [Fact]
    [UpstreamTest(
        Id + "should merge provider metadata with default settings",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_merge_provider_metadata_with_default_settings()
    {
        var result = await Transform(
            new()
            {
                Temperature = 0.7,
                ProviderOptions = new Dictionary<string, System.Text.Json.JsonElement>
                {
                    ["anthropic"] = Json("{\"cacheControl\":{\"type\":\"ephemeral\"}}"),
                },
            }
        );

        Assert.Equal(0.7, result.Temperature);
        var options = Assert.Single(result.ProviderOptions!);
        Assert.Equal("anthropic", options.Key);
        AssertJsonEqual("{\"cacheControl\":{\"type\":\"ephemeral\"}}", options.Value);
    }

    [Fact]
    [UpstreamTest(
        Id + "should merge complex provider metadata objects",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_merge_complex_provider_metadata_objects()
    {
        var parameters = Params();
        parameters.ProviderOptions = new Dictionary<string, System.Text.Json.JsonElement>
        {
            ["anthropic"] = Json("{\"feature\":{\"enabled\":false},\"otherSetting\":\"value\"}"),
        };

        var result = await Transform(
            new()
            {
                ProviderOptions = new Dictionary<string, System.Text.Json.JsonElement>
                {
                    ["anthropic"] = Json(
                        "{\"cacheControl\":{\"type\":\"ephemeral\"},\"feature\":{\"enabled\":true}}"
                    ),
                    ["openai"] = Json("{\"logit_bias\":{\"50256\":-100}}"),
                },
            },
            parameters
        );

        Assert.Equal(2, result.ProviderOptions!.Count);
        AssertJsonEqual(
            "{\"cacheControl\":{\"type\":\"ephemeral\"},\"feature\":{\"enabled\":false},\"otherSetting\":\"value\"}",
            result.ProviderOptions["anthropic"]
        );
        AssertJsonEqual("{\"logit_bias\":{\"50256\":-100}}", result.ProviderOptions["openai"]);
    }

    [Fact]
    [UpstreamTest(
        Id + "should handle nested provider metadata objects correctly",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_nested_provider_metadata_objects_correctly()
    {
        var parameters = Params();
        parameters.ProviderOptions = new Dictionary<string, System.Text.Json.JsonElement>
        {
            ["anthropic"] = Json(
                "{\"tools\":{\"retrieval\":{\"enabled\":false},\"code\":{\"enabled\":true}}}"
            ),
        };

        var result = await Transform(
            new()
            {
                ProviderOptions = new Dictionary<string, System.Text.Json.JsonElement>
                {
                    ["anthropic"] = Json(
                        "{\"tools\":{\"retrieval\":{\"enabled\":true},\"math\":{\"enabled\":true}}}"
                    ),
                },
            },
            parameters
        );

        var options = Assert.Single(result.ProviderOptions!);
        Assert.Equal("anthropic", options.Key);
        AssertJsonEqual(
            "{\"tools\":{\"retrieval\":{\"enabled\":false},\"math\":{\"enabled\":true},\"code\":{\"enabled\":true}}}",
            options.Value
        );
    }
}
