// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenTelemetry;

namespace Vercel.AI.Tests;

public sealed class SelectAttributeTests
{
    [Fact]
    [UpstreamTest(
        "packages/otel/src/select-attributes.test.ts::drops invalid array attribute entries",
        Coverage = UpstreamCoverage.Covered)]
    public void Drops_invalid_array_attribute_entries()
    {
        var result = TelemetryAttributes.Select(
            new TelemetrySettings { IsEnabled = true },
            new Dictionary<string, object?>
            {
                ["keep"] = new object?[] { "stop", null, new Dictionary<string, string>(), "length" },
                ["drop"] = new object?[] { null, new Dictionary<string, string>() },
                ["dropMixed"] = new object[] { "stop", 1 },
                ["input"] = new TelemetryInput(() => new object?[] { null, new Dictionary<string, string>(), "input" }),
                ["output"] = new TelemetryOutput(() => new object?[] { null }),
            });

        Assert.Equal(new[] { "stop", "length" }, (string[])result["keep"]!);
        Assert.Equal(new[] { "input" }, (string[])result["input"]!);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/select-attributes.test.ts::drops non-finite direct and resolver-produced numeric attributes",
        Coverage = UpstreamCoverage.Covered)]
    public void Drops_non_finite_direct_and_resolver_attributes()
    {
        var result = TelemetryAttributes.Select(
            new TelemetrySettings { IsEnabled = true },
            new Dictionary<string, object?>
            {
                ["finite"] = 1,
                ["directNaN"] = double.NaN,
                ["directInfinity"] = double.PositiveInfinity,
                ["directArray"] = new[] { 1d, double.NegativeInfinity },
                ["inputNaN"] = new TelemetryInput(() => double.NaN),
                ["inputArray"] = new TelemetryInput(() => new[] { 1d, double.PositiveInfinity }),
                ["outputInfinity"] = new TelemetryOutput(() => double.NegativeInfinity),
                ["outputArray"] = new TelemetryOutput(() => new[] { 1d, double.NaN }),
            });

        Assert.Equal(1, (int)result["finite"]!);
        Assert.Single(result);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/select-attributes.test.ts::drops array attributes that serialize to empty OTLP AnyValues",
        Coverage = UpstreamCoverage.Covered)]
    public void Drops_array_attributes_that_serialize_to_empty_otlp_values()
    {
        var attributeValue = new object?[] { null };
        ParityAssert.JsonEqual(
            ToOtlpAnyValue(attributeValue),
            "{\"arrayValue\":{\"values\":[{}]}}");

        var result = TelemetryAttributes.Select(
            new TelemetrySettings { IsEnabled = true },
            new Dictionary<string, object?>
            {
                ["test.attribute"] = attributeValue,
            });
        Assert.Empty(result);
    }

    private static JsonObject ToOtlpAnyValue(object? value)
    {
        if (value is System.Collections.IEnumerable enumerable && value is not string)
        {
            var values = new JsonArray();
            foreach (var item in enumerable)
            {
                values.Add(ToOtlpAnyValue(item));
            }

            return new JsonObject
            {
                ["arrayValue"] = new JsonObject
                {
                    ["values"] = values,
                },
            };
        }

        if (value is string text)
        {
            return new JsonObject { ["stringValue"] = text };
        }

        if (value is int or long or double or float)
        {
            return new JsonObject { ["intValue"] = Convert.ToInt64(value!, System.Globalization.CultureInfo.InvariantCulture) };
        }

        if (value is bool flag)
        {
            return new JsonObject { ["boolValue"] = flag };
        }

        return new JsonObject();
    }
}
