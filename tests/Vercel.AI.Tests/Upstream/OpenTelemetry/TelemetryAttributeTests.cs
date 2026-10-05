// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenTelemetry;

namespace Vercel.AI.Tests;

public sealed class TelemetryAttributeTests
{
    [Fact]
    [UpstreamTest(
        "packages/otel/src/sanitize-attribute-value.test.ts::sanitizeAttributeValue::returns scalar values unchanged",
        Coverage = UpstreamCoverage.Covered)]
    public void Sanitize_returns_scalar_values_unchanged()
    {
        Assert.Equal("text", TelemetryAttributes.SanitizeValue("text"));
        Assert.Equal(42, TelemetryAttributes.SanitizeValue(42));
        Assert.Equal(true, TelemetryAttributes.SanitizeValue(true));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/sanitize-attribute-value.test.ts::sanitizeAttributeValue::drops non-finite scalar numbers",
        Coverage = UpstreamCoverage.Covered)]
    public void Sanitize_drops_non_finite_scalar_numbers()
    {
        Assert.Null(TelemetryAttributes.SanitizeValue(double.NaN));
        Assert.Null(TelemetryAttributes.SanitizeValue(double.PositiveInfinity));
        Assert.Null(TelemetryAttributes.SanitizeValue(double.NegativeInfinity));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/sanitize-attribute-value.test.ts::sanitizeAttributeValue::returns homogeneous primitive arrays unchanged",
        Coverage = UpstreamCoverage.Covered)]
    public void Sanitize_returns_homogeneous_primitive_arrays()
    {
        Assert.Equal(new object[] { "a", "b" }, (object[])TelemetryAttributes.SanitizeValue(new object?[] { "a", "b" })!);
        Assert.Equal(new object[] { 1, 2 }, (object[])TelemetryAttributes.SanitizeValue(new object?[] { 1, 2 })!);
        Assert.Equal(new object[] { true, false }, (object[])TelemetryAttributes.SanitizeValue(new object?[] { true, false })!);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/sanitize-attribute-value.test.ts::sanitizeAttributeValue::drops numeric arrays containing non-finite values",
        Coverage = UpstreamCoverage.Covered)]
    public void Sanitize_drops_numeric_arrays_containing_non_finite_values()
    {
        Assert.Null(TelemetryAttributes.SanitizeValue(new object?[] { 1, double.NaN }));
        Assert.Null(TelemetryAttributes.SanitizeValue(new object?[] { 1, double.PositiveInfinity }));
        Assert.Null(TelemetryAttributes.SanitizeValue(new object?[] { 1, double.NegativeInfinity }));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/sanitize-attribute-value.test.ts::sanitizeAttributeValue::drops invalid entries from an otherwise homogeneous array",
        Coverage = UpstreamCoverage.Covered)]
    public void Sanitize_drops_invalid_entries_from_a_homogeneous_array()
    {
        var value = TelemetryAttributes.SanitizeValue(new object?[] { "a", null, new object(), "b" });
        Assert.Equal(new object[] { "a", "b" }, (object[])value!);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/sanitize-attribute-value.test.ts::sanitizeAttributeValue::drops arrays with mixed primitive types",
        Coverage = UpstreamCoverage.Covered)]
    public void Sanitize_drops_arrays_with_mixed_primitive_types()
    {
        Assert.Null(TelemetryAttributes.SanitizeValue(new object?[] { "a", 1 }));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/sanitize-attribute-value.test.ts::sanitizeAttributeValue::drops arrays without any primitive entries",
        Coverage = UpstreamCoverage.Covered)]
    public void Sanitize_drops_arrays_without_primitive_entries()
    {
        Assert.Null(TelemetryAttributes.SanitizeValue(new object?[] { null, new object() }));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/sanitize-attribute-value.test.ts::sanitizeAttributeValue::drops empty arrays",
        Coverage = UpstreamCoverage.Covered)]
    public void Sanitize_drops_empty_arrays()
    {
        Assert.Null(TelemetryAttributes.SanitizeValue(Array.Empty<object>()));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/sanitize-attribute-value.test.ts::sanitizeAttributes::returns an empty object when attributes are undefined",
        Coverage = UpstreamCoverage.Covered)]
    public void Sanitize_returns_an_empty_map_when_attributes_are_missing()
    {
        Assert.Empty(TelemetryAttributes.Sanitize(null));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/sanitize-attribute-value.test.ts::sanitizeAttributes::keeps valid entries and drops ones that sanitize away",
        Coverage = UpstreamCoverage.Covered)]
    public void Sanitize_keeps_valid_entries_and_drops_the_rest()
    {
        var result = TelemetryAttributes.Sanitize(new Dictionary<string, object?>
        {
            ["keep"] = new object?[] { "a", null, "b" },
            ["valid"] = "text",
            ["dropMixed"] = new object?[] { "a", 1 },
            ["dropNonFinite"] = double.NaN,
            ["dropNonFiniteArray"] = new object?[] { 1, double.PositiveInfinity },
            ["dropNull"] = null,
        });

        Assert.Equal(new[] { "keep", "valid" }, result.Keys.OrderBy(key => key, StringComparer.Ordinal).ToArray());
        Assert.Equal(new object[] { "a", "b" }, (object[])result["keep"]!);
        Assert.Equal("text", result["valid"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/select-telemetry-attributes.test.ts::should return an empty object when telemetry is disabled",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Select_returns_nothing_when_telemetry_is_disabled()
    {
        var result = await TelemetryAttributes.SelectTelemetryAttributesAsync(
            new TelemetrySelection { IsEnabled = false },
            Map(("key", "value")));

        Assert.Empty(result);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/select-telemetry-attributes.test.ts::should return attributes even when telemetry enablement is undefined",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Select_returns_attributes_when_enablement_is_unset()
    {
        var result = await TelemetryAttributes.SelectTelemetryAttributesAsync(null, Map(("key", "value")));

        Assert.Equal("value", result["key"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/select-telemetry-attributes.test.ts::should return attributes with simple values",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Select_returns_simple_values()
    {
        var result = await TelemetryAttributes.SelectTelemetryAttributesAsync(
            new TelemetrySelection { IsEnabled = true },
            Map(("string", "value"), ("number", 42), ("boolean", true)));

        Assert.Equal("value", result["string"]);
        Assert.Equal(42, result["number"]);
        Assert.Equal(true, result["boolean"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/select-telemetry-attributes.test.ts::should handle input functions when recordInputs is true",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Select_resolves_inputs_when_recording_is_enabled()
    {
        var result = await TelemetryAttributes.SelectTelemetryAttributesAsync(
            new TelemetrySelection { IsEnabled = true, RecordInputs = true },
            Map(
                ("input", ResolvableTelemetryAttribute.Input(() => "input value")),
                ("other", "other value")));

        Assert.Equal("input value", result["input"]);
        Assert.Equal("other value", result["other"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/select-telemetry-attributes.test.ts::should not include input functions when recordInputs is false",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Select_skips_inputs_when_recording_is_disabled()
    {
        var result = await TelemetryAttributes.SelectTelemetryAttributesAsync(
            new TelemetrySelection { IsEnabled = true, RecordInputs = false },
            Map(
                ("input", ResolvableTelemetryAttribute.Input(() => "input value")),
                ("other", "other value")));

        Assert.False(result.ContainsKey("input"));
        Assert.Equal("other value", result["other"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/select-telemetry-attributes.test.ts::should handle output functions when recordOutputs is true",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Select_resolves_outputs_when_recording_is_enabled()
    {
        var result = await TelemetryAttributes.SelectTelemetryAttributesAsync(
            new TelemetrySelection { IsEnabled = true, RecordOutputs = true },
            Map(
                ("output", ResolvableTelemetryAttribute.Output(() => "output value")),
                ("other", "other value")));

        Assert.Equal("output value", result["output"]);
        Assert.Equal("other value", result["other"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/select-telemetry-attributes.test.ts::should not include output functions when recordOutputs is false",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Select_skips_outputs_when_recording_is_disabled()
    {
        var result = await TelemetryAttributes.SelectTelemetryAttributesAsync(
            new TelemetrySelection { IsEnabled = true, RecordOutputs = false },
            Map(
                ("output", ResolvableTelemetryAttribute.Output(() => "output value")),
                ("other", "other value")));

        Assert.False(result.ContainsKey("output"));
        Assert.Equal("other value", result["other"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/select-telemetry-attributes.test.ts::should ignore undefined values",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Select_ignores_undefined_values()
    {
        var result = await TelemetryAttributes.SelectTelemetryAttributesAsync(
            new TelemetrySelection { IsEnabled = true },
            new Dictionary<string, object?>
            {
                ["defined"] = "value",
                ["undefined"] = null,
            });

        Assert.Equal("value", Assert.Single(result).Value);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/select-telemetry-attributes.test.ts::should ignore input and output functions that return undefined",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Select_ignores_resolvers_that_return_null()
    {
        var result = await TelemetryAttributes.SelectTelemetryAttributesAsync(
            new TelemetrySelection { IsEnabled = true },
            Map(
                ("input", ResolvableTelemetryAttribute.Input(() => (object?)null)),
                ("output", ResolvableTelemetryAttribute.Output(() => (object?)null)),
                ("other", "value")));

        Assert.Equal("value", Assert.Single(result).Value);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/select-telemetry-attributes.test.ts::should handle mixed attribute types correctly",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Select_handles_mixed_attribute_types()
    {
        var result = await TelemetryAttributes.SelectTelemetryAttributesAsync(
            new TelemetrySelection { IsEnabled = true },
            new Dictionary<string, object?>
            {
                ["simple"] = "value",
                ["input"] = ResolvableTelemetryAttribute.Input(() => "input value"),
                ["output"] = ResolvableTelemetryAttribute.Output(() => "output value"),
                ["undefined"] = null,
                ["null"] = null,
                ["input_null"] = ResolvableTelemetryAttribute.Input(() => (object?)null),
            });

        Assert.Equal(3, result.Count);
        Assert.Equal("value", result["simple"]);
        Assert.Equal("input value", result["input"]);
        Assert.Equal("output value", result["output"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/select-attributes.test.ts::drops invalid array attribute entries",
        Coverage = UpstreamCoverage.Covered)]
    public void Select_attributes_drops_invalid_array_entries()
    {
        var result = TelemetryAttributes.SelectAttributes(
            new TelemetrySelection { IsEnabled = true },
            new Dictionary<string, object?>
            {
                ["keep"] = new object?[] { "stop", null, new object(), "length" },
                ["drop"] = new object?[] { null, new object() },
                ["dropMixed"] = new object?[] { "stop", 1 },
                ["input"] = ResolvableTelemetryAttribute.Input(() => new object?[] { null, new object(), "input" }),
                ["output"] = ResolvableTelemetryAttribute.Output(() => new object?[] { null }),
            });

        Assert.Equal(new[] { "input", "keep" }, result.Keys.OrderBy(key => key, StringComparer.Ordinal).ToArray());
        Assert.Equal(new object[] { "stop", "length" }, (object[])result["keep"]!);
        Assert.Equal(new object[] { "input" }, (object[])result["input"]!);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/select-attributes.test.ts::drops non-finite direct and resolver-produced numeric attributes",
        Coverage = UpstreamCoverage.Covered)]
    public void Select_attributes_drops_non_finite_numbers()
    {
        var result = TelemetryAttributes.SelectAttributes(
            new TelemetrySelection { IsEnabled = true },
            new Dictionary<string, object?>
            {
                ["finite"] = 1,
                ["directNaN"] = double.NaN,
                ["directInfinity"] = double.PositiveInfinity,
                ["directArray"] = new object?[] { 1, double.NegativeInfinity },
                ["inputNaN"] = ResolvableTelemetryAttribute.Input(() => double.NaN),
                ["inputArray"] = ResolvableTelemetryAttribute.Input(() => new object?[] { 1, double.PositiveInfinity }),
                ["outputInfinity"] = ResolvableTelemetryAttribute.Output(() => double.NegativeInfinity),
                ["outputArray"] = ResolvableTelemetryAttribute.Output(() => new object?[] { 1, double.NaN }),
            });

        Assert.Equal(1, Assert.Single(result).Value);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/select-attributes.test.ts::drops array attributes that serialize to empty OTLP AnyValues",
        Coverage = UpstreamCoverage.Covered)]
    public void Select_attributes_drops_arrays_that_have_no_primitive_values()
    {
        var result = TelemetryAttributes.SelectAttributes(
            new TelemetrySelection { IsEnabled = true },
            new Dictionary<string, object?>
            {
                ["test.attribute"] = new object?[] { null },
            });

        Assert.Empty(result);
    }

    private static Dictionary<string, object?> Map(params (string Key, object? Value)[] pairs)
    {
        var map = new Dictionary<string, object?>();
        foreach (var (key, value) in pairs)
        {
            map[key] = value;
        }

        return map;
    }
}
