// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class DeepEqualTests
{
    [UpstreamTest("packages/ai/src/util/is-deep-equal-data.test.ts::should check if two primitives are equal", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Compares_primitives()
    {
        Assert.True(DeepEqual.IsDeepEqualData(1, 1));
        Assert.False(DeepEqual.IsDeepEqualData(1, 2));
    }

    [UpstreamTest("packages/ai/src/util/is-deep-equal-data.test.ts::should return false for different types", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Rejects_different_types()
    {
        Assert.False(DeepEqual.IsDeepEqualData(Map(("a", 1)), 1));
    }

    [UpstreamTest("packages/ai/src/util/is-deep-equal-data.test.ts::should return false for null values compared with objects", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Rejects_null_compared_with_an_object()
    {
        Assert.False(DeepEqual.IsDeepEqualData(Map(("a", 1)), null));
    }

    [UpstreamTest("packages/ai/src/util/is-deep-equal-data.test.ts::should identify two equal objects", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Accepts_equal_objects()
    {
        Assert.True(DeepEqual.IsDeepEqualData(Map(("a", 1), ("b", 2)), Map(("a", 1), ("b", 2))));
    }

    [UpstreamTest("packages/ai/src/util/is-deep-equal-data.test.ts::should identify two objects with different values", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Rejects_different_values()
    {
        Assert.False(DeepEqual.IsDeepEqualData(Map(("a", 1), ("b", 2)), Map(("a", 1), ("b", 3))));
    }

    [UpstreamTest("packages/ai/src/util/is-deep-equal-data.test.ts::should identify two objects with different number of keys", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Rejects_different_key_counts()
    {
        Assert.False(DeepEqual.IsDeepEqualData(Map(("a", 1), ("b", 2)), Map(("a", 1), ("b", 2), ("c", 3))));
    }

    [UpstreamTest("packages/ai/src/util/is-deep-equal-data.test.ts::should handle nested objects", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Accepts_equal_nested_objects()
    {
        Assert.True(DeepEqual.IsDeepEqualData(
            Map(("a", Map(("c", 1))), ("b", 2)),
            Map(("a", Map(("c", 1))), ("b", 2))));
    }

    [UpstreamTest("packages/ai/src/util/is-deep-equal-data.test.ts::should detect inequality in nested objects", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Rejects_unequal_nested_objects()
    {
        Assert.False(DeepEqual.IsDeepEqualData(
            Map(("a", Map(("c", 1))), ("b", 2)),
            Map(("a", Map(("c", 2))), ("b", 2))));
    }

    [UpstreamTest("packages/ai/src/util/is-deep-equal-data.test.ts::should compare arrays correctly", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Compares_arrays()
    {
        Assert.True(DeepEqual.IsDeepEqualData(new[] { 1, 2, 3 }, new[] { 1, 2, 3 }));
        Assert.False(DeepEqual.IsDeepEqualData(new[] { 1, 2, 3 }, new[] { 1, 2, 4 }));
    }

    [UpstreamTest("packages/ai/src/util/is-deep-equal-data.test.ts::should return false for null comparison with object", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Rejects_another_null_object_comparison()
    {
        Assert.False(DeepEqual.IsDeepEqualData(Map(("a", 1)), null));
    }

    [UpstreamTest("packages/ai/src/util/is-deep-equal-data.test.ts::should distinguish between array and object with same enumerable properties", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Distinguishes_arrays_from_objects()
    {
        var obj = Map(("0", "one"), ("1", "two"), ("length", 2));
        Assert.False(DeepEqual.IsDeepEqualData(obj, new[] { "one", "two" }));
    }

    [UpstreamTest("packages/ai/src/util/is-deep-equal-data.test.ts::should return false when comparing objects with different prototypes", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Distinguishes_constructors()
    {
        var left = new DataObject { Constructor = new object() };
        left.Properties["b"] = 2;
        var right = new DataObject { Constructor = new object() };
        right.Properties["b"] = 2;
        Assert.False(DeepEqual.IsDeepEqualData(left, right));
    }

    [UpstreamTest("packages/ai/src/util/is-deep-equal-data.test.ts::should handle date object comparisons correctly", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Compares_dates_by_instant()
    {
        var first = new DateTime(2000, 1, 1);
        var second = new DateTime(2000, 1, 1);
        var third = new DateTime(2000, 1, 2);
        Assert.True(DeepEqual.IsDeepEqualData(first, second));
        Assert.False(DeepEqual.IsDeepEqualData(first, third));
    }

    [UpstreamTest("packages/ai/src/util/is-deep-equal-data.test.ts::should handle function comparisons", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Treats_distinct_functions_as_unequal()
    {
        Action hello = delegate { };
        Action alsoHello = delegate { };
        Action world = delegate { };
        Assert.False(DeepEqual.IsDeepEqualData(hello, alsoHello));
        Assert.False(DeepEqual.IsDeepEqualData(hello, world));
    }

    private static Dictionary<string, object> Map(params (string Key, object Value)[] entries)
    {
        var map = new Dictionary<string, object>();
        for (var i = 0; i < entries.Length; i++)
        {
            map[entries[i].Key] = entries[i].Value;
        }

        return map;
    }
}
