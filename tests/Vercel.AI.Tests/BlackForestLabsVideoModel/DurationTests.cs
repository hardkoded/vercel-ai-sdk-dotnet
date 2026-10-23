// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.BlackForestLabsVideoModel.BlackForestLabsVideoModelFixture;

namespace Vercel.AI.Tests.BlackForestLabsVideoModel;

/// <summary>Port of <c>black-forest-labs-video-model.test.ts</c> &gt; <c>BlackForestLabsVideoModel</c> &gt; <c>duration</c>.</summary>
public sealed class DurationTests
{
    private const string Id = "packages/black-forest-labs/src/black-forest-labs-video-model.test.ts::BlackForestLabsVideoModel > duration::";

    [Fact]
    [UpstreamTest(Id + "should send an in-range duration", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSendAnInRangeDuration()
    {
        var server = CreateServer();

        await CreateModel(server).DoGenerateAsync(Call(duration: 12), CancellationToken.None);

        AssertMatches("{\"duration\":12}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should omit duration when unset, leaving the API default of auto", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldOmitDurationWhenUnsetLeavingTheApiDefaultOfAuto()
    {
        var server = CreateServer();

        await CreateModel(server).DoGenerateAsync(Call(), CancellationToken.None);

        AssertNoProperty("duration", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should clamp a duration above the maximum and warn", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldClampADurationAboveTheMaximumAndWarn()
    {
        var server = CreateServer();

        var result = await CreateModel(server).DoGenerateAsync(Call(duration: 30), CancellationToken.None);

        AssertMatches("{\"duration\":20}", RequestBody(server));
        Assert.Contains(
            ("unsupported", "duration", "FLUX 3 video supports at most 20 seconds. The requested duration of 30 was clamped to 20."),
            result.Warnings.Select(Warning));
    }

    [Fact]
    [UpstreamTest(Id + "should clamp a duration below the minimum and warn", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldClampADurationBelowTheMinimumAndWarn()
    {
        var server = CreateServer();

        var result = await CreateModel(server).DoGenerateAsync(Call(duration: 2), CancellationToken.None);

        AssertMatches("{\"duration\":5}", RequestBody(server));
        Assert.Contains(
            ("unsupported", "duration", "FLUX 3 video requires at least 5 seconds. The requested duration of 2 was clamped to 5."),
            result.Warnings.Select(Warning));
    }

    [Fact]
    [UpstreamTest(Id + "should round a fractional duration and warn", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldRoundAFractionalDurationAndWarn()
    {
        var server = CreateServer();

        var result = await CreateModel(server).DoGenerateAsync(Call(duration: 7.4), CancellationToken.None);

        AssertMatches("{\"duration\":7}", RequestBody(server));
        Assert.Contains(
            ("unsupported", "duration", "FLUX 3 video requires a whole number of seconds. The requested duration of 7.4 was rounded to 7."),
            result.Warnings.Select(Warning));
    }

    [Fact]
    [UpstreamTest(Id + "should round and then clamp a fractional out-of-range duration", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldRoundAndThenClampAFractionalOutOfRangeDuration()
    {
        var server = CreateServer();

        var result = await CreateModel(server).DoGenerateAsync(Call(duration: 24.6), CancellationToken.None);

        AssertMatches("{\"duration\":20}", RequestBody(server));
        Assert.Equal(2, result.Warnings.Count);
    }
}
