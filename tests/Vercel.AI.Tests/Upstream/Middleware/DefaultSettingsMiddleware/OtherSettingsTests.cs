// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Middleware.DefaultSettingsMiddleware.Support;

namespace Vercel.AI.Tests.Upstream.Middleware.DefaultSettingsMiddleware;

/// <summary>Port of <c>default-settings-middleware.test.ts</c> &gt; <c>defaultSettingsMiddleware &gt; other settings</c>.</summary>
public sealed class OtherSettingsTests
{
    private const string Id = FileId + "other settings::";

    [Fact]
    [UpstreamTest(Id + "should apply default maxOutputTokens", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_apply_default_maxOutputTokens()
    {
        var result = await Transform(new() { MaxOutputTokens = 100 });

        Assert.Equal(100, result.MaxOutputTokens);
    }

    [Fact]
    [UpstreamTest(
        Id + "should prioritize param maxOutputTokens",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_prioritize_param_maxOutputTokens()
    {
        var parameters = Params();
        parameters.MaxOutputTokens = 50;

        var result = await Transform(new() { MaxOutputTokens = 100 }, parameters);

        Assert.Equal(50, result.MaxOutputTokens);
    }

    [Fact]
    [UpstreamTest(Id + "should apply default stopSequences", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_apply_default_stopSequences()
    {
        var result = await Transform(new() { StopSequences = new[] { "stop" } });

        Assert.Equal(new[] { "stop" }, result.StopSequences);
    }

    [Fact]
    [UpstreamTest(
        Id + "should prioritize param stopSequences",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_prioritize_param_stopSequences()
    {
        var parameters = Params();
        parameters.StopSequences = new[] { "end" };

        var result = await Transform(new() { StopSequences = new[] { "stop" } }, parameters);

        Assert.Equal(new[] { "end" }, result.StopSequences);
    }

    [Fact]
    [UpstreamTest(Id + "should apply default topP", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_apply_default_topP()
    {
        var result = await Transform(new() { TopP = 0.9 });

        Assert.Equal(0.9, result.TopP);
    }

    [Fact]
    [UpstreamTest(Id + "should prioritize param topP", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_prioritize_param_topP()
    {
        var parameters = Params();
        parameters.TopP = 0.5;

        var result = await Transform(new() { TopP = 0.9 }, parameters);

        Assert.Equal(0.5, result.TopP);
    }
}
