// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenTelemetry;

namespace Vercel.AI.Tests;

public sealed class SelectTelemetryAttributeTests
{
    [Fact]
    [UpstreamTest(
        "packages/otel/src/select-telemetry-attributes.test.ts::should return an empty object when telemetry is disabled",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_an_empty_object_when_telemetry_is_disabled()
    {
        var result = await TelemetryAttributes.SelectAsync(
            new TelemetrySettings { IsEnabled = false },
            new Dictionary<string, object?> { ["key"] = "value" });
        Assert.Empty(result);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/select-telemetry-attributes.test.ts::should return attributes even when telemetry enablement is undefined",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_attributes_when_enablement_is_null()
    {
        var result = await TelemetryAttributes.SelectAsync(null, new Dictionary<string, object?> { ["key"] = "value" });
        Assert.Equal("value", result["key"] as string);
        Assert.Single(result);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/select-telemetry-attributes.test.ts::should return attributes with simple values",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_simple_values()
    {
        var result = await TelemetryAttributes.SelectAsync(
            new TelemetrySettings { IsEnabled = true },
            new Dictionary<string, object?>
            {
                ["string"] = "value",
                ["number"] = 42,
                ["boolean"] = true,
            });
        Assert.Equal("value", result["string"] as string);
        Assert.Equal(42, (int)result["number"]!);
        Assert.True((bool)result["boolean"]!);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/select-telemetry-attributes.test.ts::should handle input functions when recordInputs is true",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_input_resolvers_when_recording_inputs()
    {
        var result = await TelemetryAttributes.SelectAsync(
            new TelemetrySettings { IsEnabled = true, RecordInputs = true },
            new Dictionary<string, object?>
            {
                ["input"] = new TelemetryInput(() => "input value"),
                ["other"] = "other value",
            });
        Assert.Equal("input value", result["input"] as string);
        Assert.Equal("other value", result["other"] as string);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/select-telemetry-attributes.test.ts::should not include input functions when recordInputs is false",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Skips_input_resolvers_when_recording_inputs_is_false()
    {
        var result = await TelemetryAttributes.SelectAsync(
            new TelemetrySettings { IsEnabled = true, RecordInputs = false },
            new Dictionary<string, object?>
            {
                ["input"] = new TelemetryInput(() => "input value"),
                ["other"] = "other value",
            });
        Assert.Equal("other value", result["other"] as string);
        Assert.Single(result);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/select-telemetry-attributes.test.ts::should handle output functions when recordOutputs is true",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_output_resolvers_when_recording_outputs()
    {
        var result = await TelemetryAttributes.SelectAsync(
            new TelemetrySettings { IsEnabled = true, RecordOutputs = true },
            new Dictionary<string, object?>
            {
                ["output"] = new TelemetryOutput(() => "output value"),
                ["other"] = "other value",
            });
        Assert.Equal("output value", result["output"] as string);
        Assert.Equal("other value", result["other"] as string);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/select-telemetry-attributes.test.ts::should not include output functions when recordOutputs is false",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Skips_output_resolvers_when_recording_outputs_is_false()
    {
        var result = await TelemetryAttributes.SelectAsync(
            new TelemetrySettings { IsEnabled = true, RecordOutputs = false },
            new Dictionary<string, object?>
            {
                ["output"] = new TelemetryOutput(() => "output value"),
                ["other"] = "other value",
            });
        Assert.Equal("other value", result["other"] as string);
        Assert.Single(result);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/select-telemetry-attributes.test.ts::should ignore undefined values",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Ignores_null_values()
    {
        var result = await TelemetryAttributes.SelectAsync(
            new TelemetrySettings { IsEnabled = true },
            new Dictionary<string, object?>
            {
                ["defined"] = "value",
                ["undefined"] = null,
            });
        Assert.Equal("value", result["defined"] as string);
        Assert.Single(result);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/select-telemetry-attributes.test.ts::should ignore input and output functions that return undefined",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Ignores_resolvers_that_return_null()
    {
        var result = await TelemetryAttributes.SelectAsync(
            new TelemetrySettings { IsEnabled = true },
            new Dictionary<string, object?>
            {
                ["input"] = new TelemetryInput(() => null),
                ["output"] = new TelemetryOutput(() => null),
                ["other"] = "value",
            });
        Assert.Equal("value", result["other"] as string);
        Assert.Single(result);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/select-telemetry-attributes.test.ts::should handle mixed attribute types correctly",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Handles_mixed_attribute_types()
    {
        var result = await TelemetryAttributes.SelectAsync(
            new TelemetrySettings { IsEnabled = true },
            new Dictionary<string, object?>
            {
                ["simple"] = "value",
                ["input"] = new TelemetryInput(() => "input value"),
                ["output"] = new TelemetryOutput(() => "output value"),
                ["undefined"] = null,
                ["null"] = null,
                ["input_null"] = new TelemetryInput(() => null),
            });
        Assert.Equal("value", result["simple"] as string);
        Assert.Equal("input value", result["input"] as string);
        Assert.Equal("output value", result["output"] as string);
        Assert.Equal(3, result.Count);
    }
}
