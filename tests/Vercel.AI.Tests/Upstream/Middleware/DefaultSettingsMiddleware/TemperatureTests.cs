// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Middleware.DefaultSettingsMiddleware.Support;

namespace Vercel.AI.Tests.Upstream.Middleware.DefaultSettingsMiddleware;

/// <summary>Port of <c>default-settings-middleware.test.ts</c> &gt; <c>defaultSettingsMiddleware &gt; temperature</c>.</summary>
public sealed class TemperatureTests
{
    private const string Id = FileId + "temperature::";

    [Fact]
    [UpstreamTest(
        Id + "should keep 0 if settings.temperature is not set",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_keep_0_if_settings_temperature_is_not_set()
    {
        var parameters = Params();
        parameters.Temperature = 0;

        var result = await Transform(new(), parameters);

        Assert.Equal(0, result.Temperature);
    }

    [Fact]
    [UpstreamTest(
        Id + "should use default temperature if param temperature is undefined",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_default_temperature_if_param_temperature_is_undefined()
    {
        var parameters = Params();
        parameters.Temperature = null;

        var result = await Transform(new() { Temperature = 0.7 }, parameters);

        Assert.Equal(0.7, result.Temperature);
    }

    [Fact]
    [UpstreamTest(
        Id + "should not use default temperature if param temperature is null",
        Coverage = UpstreamCoverage.Partial,
        Note = "C# has no null/undefined distinction: a null Temperature is unset, so the default 0.7 applies. Upstream keeps the explicit null. This test asserts the C# behavior and that an explicit value (0, not null) is not overridden."
    )]
    public async Task Should_not_use_default_temperature_if_param_temperature_is_null()
    {
        var parameters = Params();
        parameters.Temperature = null;

        var unset = await Transform(new() { Temperature = 0.7 }, parameters);
        Assert.Equal(0.7, unset.Temperature);

        parameters.Temperature = 0;
        var explicitValue = await Transform(new() { Temperature = 0.7 }, parameters);
        Assert.Equal(0, explicitValue.Temperature);
    }

    [Fact]
    [UpstreamTest(
        Id + "should use param temperature by default",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_param_temperature_by_default()
    {
        var parameters = Params();
        parameters.Temperature = 0.9;

        var result = await Transform(new() { Temperature = 0.7 }, parameters);

        Assert.Equal(0.9, result.Temperature);
    }
}
