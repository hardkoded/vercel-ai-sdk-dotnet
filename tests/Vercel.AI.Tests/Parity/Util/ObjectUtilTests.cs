// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;
using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class ObjectUtilTests
{
    private sealed class OtherMap : Dictionary<string, object?>
    {
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/is-deep-equal-data.test.ts::should check if two primitives are equal", Coverage = UpstreamCoverage.Covered)]
    public void Compares_primitives()
    {
        Assert.True(DataEquality.IsDeepEqualData(1, 1));
        Assert.False(DataEquality.IsDeepEqualData(1, 2));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/is-deep-equal-data.test.ts::should return false for different types", Coverage = UpstreamCoverage.Covered)]
    public void Returns_false_for_different_types()
    {
        Assert.False(DataEquality.IsDeepEqualData(new Dictionary<string, object?> { ["a"] = 1 }, 1));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/is-deep-equal-data.test.ts::should return false for null values compared with objects", Coverage = UpstreamCoverage.Covered)]
    public void Returns_false_when_an_object_is_compared_with_null()
    {
        Assert.False(DataEquality.IsDeepEqualData(new Dictionary<string, object?> { ["a"] = 1 }, null));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/is-deep-equal-data.test.ts::should identify two equal objects", Coverage = UpstreamCoverage.Covered)]
    public void Identifies_equal_objects()
    {
        var left = new Dictionary<string, object?> { ["a"] = 1, ["b"] = 2 };
        var right = new Dictionary<string, object?> { ["a"] = 1, ["b"] = 2 };
        Assert.True(DataEquality.IsDeepEqualData(left, right));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/is-deep-equal-data.test.ts::should identify two objects with different values", Coverage = UpstreamCoverage.Covered)]
    public void Identifies_objects_with_different_values()
    {
        var left = new Dictionary<string, object?> { ["a"] = 1, ["b"] = 2 };
        var right = new Dictionary<string, object?> { ["a"] = 1, ["b"] = 3 };
        Assert.False(DataEquality.IsDeepEqualData(left, right));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/is-deep-equal-data.test.ts::should identify two objects with different number of keys", Coverage = UpstreamCoverage.Covered)]
    public void Identifies_objects_with_different_key_counts()
    {
        var left = new Dictionary<string, object?> { ["a"] = 1, ["b"] = 2 };
        var right = new Dictionary<string, object?> { ["a"] = 1, ["b"] = 2, ["c"] = 3 };
        Assert.False(DataEquality.IsDeepEqualData(left, right));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/is-deep-equal-data.test.ts::should handle nested objects", Coverage = UpstreamCoverage.Covered)]
    public void Compares_nested_objects()
    {
        var left = new Dictionary<string, object?>
        {
            ["a"] = new Dictionary<string, object?> { ["c"] = 1 },
            ["b"] = 2,
        };
        var right = new Dictionary<string, object?>
        {
            ["a"] = new Dictionary<string, object?> { ["c"] = 1 },
            ["b"] = 2,
        };
        Assert.True(DataEquality.IsDeepEqualData(left, right));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/is-deep-equal-data.test.ts::should detect inequality in nested objects", Coverage = UpstreamCoverage.Covered)]
    public void Detects_inequality_in_nested_objects()
    {
        var left = new Dictionary<string, object?>
        {
            ["a"] = new Dictionary<string, object?> { ["c"] = 1 },
            ["b"] = 2,
        };
        var right = new Dictionary<string, object?>
        {
            ["a"] = new Dictionary<string, object?> { ["c"] = 2 },
            ["b"] = 2,
        };
        Assert.False(DataEquality.IsDeepEqualData(left, right));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/is-deep-equal-data.test.ts::should compare arrays correctly", Coverage = UpstreamCoverage.Covered)]
    public void Compares_arrays()
    {
        Assert.True(DataEquality.IsDeepEqualData(new object?[] { 1, 2, 3 }, new object?[] { 1, 2, 3 }));
        Assert.False(DataEquality.IsDeepEqualData(new object?[] { 1, 2, 3 }, new object?[] { 1, 2, 4 }));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/is-deep-equal-data.test.ts::should return false for null comparison with object", Coverage = UpstreamCoverage.Covered)]
    public void Returns_false_for_a_second_null_comparison()
    {
        Assert.False(DataEquality.IsDeepEqualData(new Dictionary<string, object?> { ["a"] = 1 }, null));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/is-deep-equal-data.test.ts::should distinguish between array and object with same enumerable properties",
        Coverage = UpstreamCoverage.Covered)]
    public void Distinguishes_an_array_from_an_object()
    {
        var obj = new Dictionary<string, object?> { ["0"] = "one", ["1"] = "two", ["length"] = 2 };
        var array = new List<object?> { "one", "two" };
        Assert.False(DataEquality.IsDeepEqualData(obj, array));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/is-deep-equal-data.test.ts::should return false when comparing objects with different prototypes",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_false_for_different_dictionary_types()
    {
        var left = new OtherMap { ["b"] = 2 };
        var right = new Dictionary<string, object?> { ["b"] = 2 };
        Assert.False(DataEquality.IsDeepEqualData(left, right));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/is-deep-equal-data.test.ts::should handle date object comparisons correctly", Coverage = UpstreamCoverage.Covered)]
    public void Compares_dates_by_instant()
    {
        var date1 = new DateTime(2000, 1, 1);
        var date2 = new DateTime(2000, 1, 1);
        var date3 = new DateTime(2000, 1, 2);
        Assert.True(DataEquality.IsDeepEqualData(date1, date2));
        Assert.False(DataEquality.IsDeepEqualData(date1, date3));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/is-deep-equal-data.test.ts::should handle function comparisons", Coverage = UpstreamCoverage.Covered)]
    public void Compares_functions_by_reference()
    {
        Action hello = () => { };
        Action helloAgain = () => { };
        Action world = () => { };
        Assert.False(DataEquality.IsDeepEqualData(hello, helloAgain));
        Assert.False(DataEquality.IsDeepEqualData(hello, world));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/merge-objects.test.ts::mergeObjects::should merge two flat objects", Coverage = UpstreamCoverage.Covered)]
    public void Merges_flat_objects_without_mutating_inputs()
    {
        var target = new Dictionary<string, object?> { ["a"] = 1, ["b"] = 2 };
        var source = new Dictionary<string, object?> { ["b"] = 3, ["c"] = 4 };
        var result = ObjectMerge.MergeObjects(target, source);
        Assert.Equal(1, result!["a"]);
        Assert.Equal(3, result["b"]);
        Assert.Equal(4, result["c"]);
        Assert.Equal(1, target["a"]);
        Assert.Equal(2, target["b"]);
        Assert.False(target.ContainsKey("c"));
        Assert.Equal(3, source["b"]);
        Assert.Equal(4, source["c"]);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/merge-objects.test.ts::mergeObjects::should deeply merge nested objects", Coverage = UpstreamCoverage.Covered)]
    public void Deeply_merges_nested_objects()
    {
        var target = new Dictionary<string, object?>
        {
            ["a"] = 1,
            ["b"] = new Dictionary<string, object?> { ["c"] = 2, ["d"] = 3 },
        };
        var source = new Dictionary<string, object?>
        {
            ["b"] = new Dictionary<string, object?> { ["c"] = 4, ["e"] = 5 },
        };
        var result = ObjectMerge.MergeObjects(target, source);
        var nested = (IDictionary<string, object?>)result!["b"]!;
        Assert.Equal(1, result["a"]);
        Assert.Equal(4, nested["c"]);
        Assert.Equal(3, nested["d"]);
        Assert.Equal(5, nested["e"]);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/merge-objects.test.ts::mergeObjects::should replace arrays instead of merging them", Coverage = UpstreamCoverage.Covered)]
    public void Replaces_arrays()
    {
        var replacement = new object?[] { 4, 5 };
        var target = new Dictionary<string, object?> { ["a"] = new object?[] { 1, 2, 3 }, ["b"] = 2 };
        var source = new Dictionary<string, object?> { ["a"] = replacement };
        var result = ObjectMerge.MergeObjects(target, source);
        Assert.Same(replacement, result!["a"]);
        Assert.Equal(2, result["b"]);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/merge-objects.test.ts::mergeObjects::should handle null and undefined values", Coverage = UpstreamCoverage.Covered)]
    public void Copies_null_and_skips_undefined_overrides()
    {
        var target = new Dictionary<string, object?> { ["a"] = 1, ["b"] = null, ["c"] = JsonUndefined.Value };
        var source = new Dictionary<string, object?> { ["a"] = null, ["b"] = 2, ["d"] = JsonUndefined.Value };
        var result = ObjectMerge.MergeObjects(target, source);
        Assert.Null(result!["a"]);
        Assert.Equal(2, result["b"]);
        Assert.Same(JsonUndefined.Value, result["c"]);
        Assert.False(result.ContainsKey("d"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/merge-objects.test.ts::mergeObjects::should handle complex nested structures", Coverage = UpstreamCoverage.Covered)]
    public void Merges_complex_nested_structures()
    {
        var replacement = new object?[] { 4, 5 };
        var target = new Dictionary<string, object?>
        {
            ["a"] = 1,
            ["b"] = new Dictionary<string, object?>
            {
                ["c"] = new object?[] { 1, 2, 3 },
                ["d"] = new Dictionary<string, object?> { ["e"] = 4, ["f"] = 5 },
            },
        };
        var source = new Dictionary<string, object?>
        {
            ["b"] = new Dictionary<string, object?>
            {
                ["c"] = replacement,
                ["d"] = new Dictionary<string, object?> { ["f"] = 6, ["g"] = 7 },
            },
            ["h"] = 8,
        };
        var result = ObjectMerge.MergeObjects(target, source);
        var nested = (IDictionary<string, object?>)result!["b"]!;
        var inner = (IDictionary<string, object?>)nested["d"]!;
        Assert.Equal(1, result["a"]);
        Assert.Same(replacement, nested["c"]);
        Assert.Equal(4, inner["e"]);
        Assert.Equal(6, inner["f"]);
        Assert.Equal(7, inner["g"]);
        Assert.Equal(8, result["h"]);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/merge-objects.test.ts::mergeObjects::should handle Date objects", Coverage = UpstreamCoverage.Covered)]
    public void Replaces_dates_by_reference()
    {
        object date1 = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        object date2 = new DateTime(2023, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        var result = ObjectMerge.MergeObjects(
            new Dictionary<string, object?> { ["a"] = date1 },
            new Dictionary<string, object?> { ["a"] = date2 });
        Assert.Same(date2, result!["a"]);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/merge-objects.test.ts::mergeObjects::should handle RegExp objects", Coverage = UpstreamCoverage.Covered)]
    public void Replaces_regular_expressions_by_reference()
    {
        var regex1 = new Regex("abc");
        var regex2 = new Regex("def");
        var result = ObjectMerge.MergeObjects(
            new Dictionary<string, object?> { ["a"] = regex1 },
            new Dictionary<string, object?> { ["a"] = regex2 });
        Assert.Same(regex2, result!["a"]);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/merge-objects.test.ts::mergeObjects::should handle empty objects", Coverage = UpstreamCoverage.Covered)]
    public void Merges_empty_objects()
    {
        var filled = ObjectMerge.MergeObjects(new Dictionary<string, object?>(), new Dictionary<string, object?> { ["a"] = 1 });
        Assert.Equal(1, filled!["a"]);
        var unchanged = ObjectMerge.MergeObjects(new Dictionary<string, object?> { ["a"] = 1 }, new Dictionary<string, object?>());
        Assert.Equal(1, unchanged!["a"]);
        Assert.Single(unchanged);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/merge-objects.test.ts::mergeObjects::should handle undefined inputs", Coverage = UpstreamCoverage.Covered)]
    public void Handles_undefined_inputs()
    {
        Assert.Null(ObjectMerge.MergeObjects(null, null));
        var left = new Dictionary<string, object?> { ["a"] = 1 };
        var right = new Dictionary<string, object?> { ["b"] = 2 };
        Assert.Same(left, ObjectMerge.MergeObjects(left, null));
        Assert.Same(right, ObjectMerge.MergeObjects(null, right));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/merge-objects.test.ts::mergeObjects::should not pollute Object.prototype via __proto__",
        Coverage = UpstreamCoverage.Covered)]
    public void Ignores_a_proto_key()
    {
        var malicious = new Dictionary<string, object?>
        {
            ["__proto__"] = new Dictionary<string, object?> { ["polluted"] = true },
        };
        var result = ObjectMerge.MergeObjects(new Dictionary<string, object?>(), malicious)!;
        Assert.Empty(result);
        Assert.False(result.ContainsKey("polluted"));
        Assert.False(result.ContainsKey("__proto__"));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/merge-objects.test.ts::mergeObjects::should ignore __proto__, constructor, and prototype keys",
        Coverage = UpstreamCoverage.Covered)]
    public void Ignores_dangerous_keys()
    {
        var malicious = new Dictionary<string, object?>
        {
            ["__proto__"] = new Dictionary<string, object?> { ["a"] = 1 },
            ["constructor"] = new Dictionary<string, object?>
            {
                ["prototype"] = new Dictionary<string, object?> { ["b"] = 2 },
            },
            ["prototype"] = new Dictionary<string, object?> { ["c"] = 3 },
            ["safe"] = "value",
        };
        var result = ObjectMerge.MergeObjects(new Dictionary<string, object?> { ["existing"] = "ok" }, malicious);
        Assert.Equal("ok", result!["existing"]);
        Assert.Equal("value", result["safe"]);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/merge-objects.test.ts::mergeObjects::should ignore dangerous keys nested in mergeable objects",
        Coverage = UpstreamCoverage.Covered)]
    public void Ignores_nested_dangerous_keys()
    {
        var result = ObjectMerge.MergeObjects(
            new Dictionary<string, object?>
            {
                ["metadata"] = new Dictionary<string, object?> { ["user"] = "alice" },
            },
            new Dictionary<string, object?>
            {
                ["metadata"] = new Dictionary<string, object?>
                {
                    ["__proto__"] = new Dictionary<string, object?> { ["polluted"] = true },
                    ["role"] = "admin",
                },
            });
        var metadata = (IDictionary<string, object?>)result!["metadata"]!;
        Assert.Equal("alice", metadata["user"]);
        Assert.Equal("admin", metadata["role"]);
        Assert.False(metadata.ContainsKey("__proto__"));
        Assert.False(metadata.ContainsKey("polluted"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/get-own.test.ts::getOwn::returns own properties", Coverage = UpstreamCoverage.Covered)]
    public void Returns_own_properties()
    {
        var map = new Dictionary<string, object?> { ["a"] = 1, ["b"] = 2 };
        Assert.Equal(1, OwnProperties.GetOwn(map, "a"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/get-own.test.ts::getOwn::returns undefined for absent keys", Coverage = UpstreamCoverage.Covered)]
    public void Returns_null_for_an_absent_key()
    {
        Assert.Null(OwnProperties.GetOwn(new Dictionary<string, object?> { ["a"] = 1 }, "missing"));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/get-own.test.ts::getOwn::returns undefined for inherited object properties rather than a prototype value",
        Coverage = UpstreamCoverage.Covered)]
    public void Does_not_return_prototype_names()
    {
        var map = new Dictionary<string, object?> { ["a"] = 1 };
        Assert.Null(OwnProperties.GetOwn(map, "constructor"));
        Assert.Null(OwnProperties.GetOwn(map, "toString"));
        Assert.Null(OwnProperties.GetOwn(map, "valueOf"));
        Assert.Null(OwnProperties.GetOwn(map, "__proto__"));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/get-own.test.ts::getOwn::still returns an own property that shadows an inherited name",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_an_own_shadowing_key()
    {
        var map = new Dictionary<string, object?> { ["toString"] = "shadowed" };
        Assert.Equal("shadowed", OwnProperties.GetOwn(map, "toString"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/get-own.test.ts::getOwn::returns undefined for null/undefined objects", Coverage = UpstreamCoverage.Covered)]
    public void Returns_null_for_a_null_map()
    {
        Assert.Null(OwnProperties.GetOwn(null, "a"));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/create-id-map.test.ts::createIdMap::should store object prototype property names as own keys",
        Coverage = UpstreamCoverage.Covered)]
    public void Stores_prototype_property_names()
    {
        var map = new IdMap<string>();
        map.Set("__proto__", "proto value");
        map.Set("constructor", "constructor value");
        map.Set("prototype", "prototype value");
        Assert.Equal("proto value", map.Get("__proto__"));
        Assert.Equal("constructor value", map.Get("constructor"));
        Assert.Equal("prototype value", map.Get("prototype"));
        Assert.Equal(new[] { "__proto__", "constructor", "prototype" }, map.Keys);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/create-id-map.test.ts::createIdMap::should prevent prototype pollution from missing __proto__ lookups",
        Coverage = UpstreamCoverage.Covered)]
    public void Missing_proto_lookup_returns_null()
    {
        var map = new IdMap<TextPart>();
        Assert.Null(map.Get("__proto__"));
        map.Set("kept", new TextPart("safe"));
        Assert.Equal("safe", map.Get("kept")!.Text);
        Assert.Null(map.Get("__proto__"));
    }

    private sealed class TextPart
    {
        public TextPart(string text)
        {
            Text = text;
        }

        public string Text { get; set; }
    }
}
