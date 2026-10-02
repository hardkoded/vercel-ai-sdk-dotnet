// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenTelemetry;

namespace Vercel.AI.Tests;

public sealed class SanitizeAttributeTests
{
    [Fact]
    [UpstreamTest(
        "packages/otel/src/sanitize-attribute-value.test.ts::sanitizeAttributeValue::returns scalar values unchanged",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_scalar_values_unchanged()
    {
        Assert.Equal("text", TelemetryAttributes.SanitizeAttributeValue("text") as string);
        Assert.Equal(42, (int)TelemetryAttributes.SanitizeAttributeValue(42)!);
        Assert.True((bool)TelemetryAttributes.SanitizeAttributeValue(true)!);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/sanitize-attribute-value.test.ts::sanitizeAttributeValue::drops non-finite scalar numbers",
        Coverage = UpstreamCoverage.Covered)]
    public void Drops_non_finite_scalar_numbers()
    {
        Assert.Null(TelemetryAttributes.SanitizeAttributeValue(double.NaN));
        Assert.Null(TelemetryAttributes.SanitizeAttributeValue(double.PositiveInfinity));
        Assert.Null(TelemetryAttributes.SanitizeAttributeValue(double.NegativeInfinity));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/sanitize-attribute-value.test.ts::sanitizeAttributeValue::returns homogeneous primitive arrays unchanged",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_homogeneous_primitive_arrays()
    {
        Assert.Equal(new[] { "a", "b" }, (string[])TelemetryAttributes.SanitizeAttributeValue(new[] { "a", "b" })!);
        Assert.Equal(new object[] { 1, 2 }, (object[])TelemetryAttributes.SanitizeAttributeValue(new[] { 1, 2 })!);
        Assert.Equal(new[] { true, false }, (bool[])TelemetryAttributes.SanitizeAttributeValue(new[] { true, false })!);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/sanitize-attribute-value.test.ts::sanitizeAttributeValue::drops numeric arrays containing non-finite values",
        Coverage = UpstreamCoverage.Covered)]
    public void Drops_numeric_arrays_containing_non_finite_values()
    {
        Assert.Null(TelemetryAttributes.SanitizeAttributeValue(new[] { 1d, double.NaN }));
        Assert.Null(TelemetryAttributes.SanitizeAttributeValue(new[] { 1d, double.PositiveInfinity }));
        Assert.Null(TelemetryAttributes.SanitizeAttributeValue(new[] { 1d, double.NegativeInfinity }));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/sanitize-attribute-value.test.ts::sanitizeAttributeValue::drops invalid entries from an otherwise homogeneous array",
        Coverage = UpstreamCoverage.Covered)]
    public void Drops_invalid_entries_from_a_homogeneous_array()
    {
        var value = TelemetryAttributes.SanitizeAttributeValue(new object?[] { "a", null, new Dictionary<string, string>(), "b" });
        Assert.Equal(new[] { "a", "b" }, (string[])value!);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/sanitize-attribute-value.test.ts::sanitizeAttributeValue::drops arrays with mixed primitive types",
        Coverage = UpstreamCoverage.Covered)]
    public void Drops_arrays_with_mixed_primitive_types()
    {
        Assert.Null(TelemetryAttributes.SanitizeAttributeValue(new object[] { "a", 1 }));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/sanitize-attribute-value.test.ts::sanitizeAttributeValue::drops arrays without any primitive entries",
        Coverage = UpstreamCoverage.Covered)]
    public void Drops_arrays_without_primitive_entries()
    {
        Assert.Null(TelemetryAttributes.SanitizeAttributeValue(new object?[] { null, new Dictionary<string, string>() }));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/sanitize-attribute-value.test.ts::sanitizeAttributeValue::drops empty arrays",
        Coverage = UpstreamCoverage.Covered)]
    public void Drops_empty_arrays()
    {
        Assert.Null(TelemetryAttributes.SanitizeAttributeValue(Array.Empty<string>()));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/sanitize-attribute-value.test.ts::sanitizeAttributes::returns an empty object when attributes are undefined",
        Coverage = UpstreamCoverage.Covered)]
    public void Sanitize_returns_an_empty_object_when_attributes_are_null()
    {
        Assert.Empty(TelemetryAttributes.Sanitize(null));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/sanitize-attribute-value.test.ts::sanitizeAttributes::keeps valid entries and drops ones that sanitize away",
        Coverage = UpstreamCoverage.Covered)]
    public void Sanitize_keeps_valid_entries()
    {
        var result = TelemetryAttributes.Sanitize(new Dictionary<string, object?>
        {
            ["keep"] = new object?[] { "a", null, "b" },
            ["valid"] = "text",
            ["dropMixed"] = new object[] { "a", 1 },
            ["dropNonFinite"] = double.NaN,
            ["dropNonFiniteArray"] = new[] { 1d, double.PositiveInfinity },
            ["dropNull"] = null,
        });

        Assert.Equal(new[] { "a", "b" }, (string[])result["keep"]!);
        Assert.Equal("text", result["valid"] as string);
        Assert.Equal(2, result.Count);
    }
}
