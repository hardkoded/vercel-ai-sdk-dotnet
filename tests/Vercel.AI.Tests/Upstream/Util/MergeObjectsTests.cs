// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;
using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class MergeObjectsTests
{
    [UpstreamTest("packages/ai/src/util/merge-objects.test.ts::mergeObjects::should merge two flat objects", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Merges_flat_objects_without_mutating_inputs()
    {
        var target = Map(("a", 1), ("b", 2));
        var source = Map(("b", 3), ("c", 4));
        var result = ObjectMerge.MergeObjects(target, source)!;
        Assert.Equal(1, result["a"]);
        Assert.Equal(3, result["b"]);
        Assert.Equal(4, result["c"]);
        Assert.Equal(1, target["a"]);
        Assert.Equal(2, target["b"]);
        Assert.False(target.ContainsKey("c"));
        Assert.Equal(3, source["b"]);
        Assert.Equal(4, source["c"]);
    }

    [UpstreamTest("packages/ai/src/util/merge-objects.test.ts::mergeObjects::should deeply merge nested objects", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Deep_merges_nested_objects()
    {
        var target = Map(("a", 1), ("b", Map(("c", 2), ("d", 3))));
        var source = Map(("b", Map(("c", 4), ("e", 5))));
        var result = ObjectMerge.MergeObjects(target, source)!;
        var nested = (IDictionary<string, object>)result["b"];
        Assert.Equal(1, result["a"]);
        Assert.Equal(4, nested["c"]);
        Assert.Equal(3, nested["d"]);
        Assert.Equal(5, nested["e"]);
    }

    [UpstreamTest("packages/ai/src/util/merge-objects.test.ts::mergeObjects::should replace arrays instead of merging them", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Replaces_arrays()
    {
        var target = Map(("a", new[] { 1, 2, 3 }), ("b", 2));
        var source = Map(("a", new[] { 4, 5 }));
        var result = ObjectMerge.MergeObjects(target, source)!;
        Assert.Equal(new[] { 4, 5 }, (int[])result["a"]);
        Assert.Equal(2, result["b"]);
    }

    [UpstreamTest("packages/ai/src/util/merge-objects.test.ts::mergeObjects::should handle null and undefined values", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Replaces_with_null_and_skips_undefined_overrides()
    {
        var target = Map(("a", 1), ("b", null!), ("c", JsUndefined.Value));
        var source = Map(("a", null!), ("b", 2), ("d", JsUndefined.Value));
        var result = ObjectMerge.MergeObjects(target, source)!;
        Assert.Null(result["a"]);
        Assert.Equal(2, result["b"]);
        Assert.Same(JsUndefined.Value, result["c"]);
        Assert.False(result.ContainsKey("d"));
    }

    [UpstreamTest("packages/ai/src/util/merge-objects.test.ts::mergeObjects::should handle complex nested structures", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Merges_complex_structures()
    {
        var target = Map(
            ("a", 1),
            ("b", Map(("c", new[] { 1, 2, 3 }), ("d", Map(("e", 4), ("f", 5))))));
        var source = Map(
            ("b", Map(("c", new[] { 4, 5 }), ("d", Map(("f", 6), ("g", 7))))),
            ("h", 8));
        var result = ObjectMerge.MergeObjects(target, source)!;
        var nested = (IDictionary<string, object>)result["b"];
        var inner = (IDictionary<string, object>)nested["d"];
        Assert.Equal(1, result["a"]);
        Assert.Equal(new[] { 4, 5 }, (int[])nested["c"]);
        Assert.Equal(4, inner["e"]);
        Assert.Equal(6, inner["f"]);
        Assert.Equal(7, inner["g"]);
        Assert.Equal(8, result["h"]);
    }

    [UpstreamTest("packages/ai/src/util/merge-objects.test.ts::mergeObjects::should handle Date objects", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Replaces_dates_by_reference()
    {
        var first = new DateTime(2023, 1, 1);
        var second = new DateTime(2023, 2, 1);
        var result = ObjectMerge.MergeObjects(Map(("a", first)), Map(("a", second)))!;
        Assert.Equal(second, result["a"]);
    }

    [UpstreamTest("packages/ai/src/util/merge-objects.test.ts::mergeObjects::should handle RegExp objects", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Replaces_regular_expressions_by_reference()
    {
        var first = new Regex("abc");
        var second = new Regex("def");
        var result = ObjectMerge.MergeObjects(Map(("a", first)), Map(("a", second)))!;
        Assert.Same(second, result["a"]);
    }

    [UpstreamTest("packages/ai/src/util/merge-objects.test.ts::mergeObjects::should handle empty objects", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Merges_empty_objects()
    {
        Assert.Equal(1, ObjectMerge.MergeObjects(new Dictionary<string, object>(), Map(("a", 1)))!["a"]);
        var kept = ObjectMerge.MergeObjects(Map(("a", 1)), new Dictionary<string, object>())!;
        Assert.Equal(1, kept["a"]);
        Assert.Single(kept);
    }

    [UpstreamTest("packages/ai/src/util/merge-objects.test.ts::mergeObjects::should handle undefined inputs", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Handles_null_inputs()
    {
        Assert.Null(ObjectMerge.MergeObjects(null, null));
        var left = Map(("a", 1));
        var right = Map(("b", 2));
        Assert.Same(left, ObjectMerge.MergeObjects(left, null));
        Assert.Same(right, ObjectMerge.MergeObjects(null, right));
    }

    [UpstreamTest("packages/ai/src/util/merge-objects.test.ts::mergeObjects::should not pollute Object.prototype via __proto__", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Drops_a_proto_override()
    {
        var result = ObjectMerge.MergeObjects(new Dictionary<string, object>(), Map(("__proto__", Map(("polluted", true)))))!;
        Assert.False(result.ContainsKey("__proto__"));
        Assert.False(result.ContainsKey("polluted"));
        Assert.Empty(result);
    }

    [UpstreamTest("packages/ai/src/util/merge-objects.test.ts::mergeObjects::should ignore __proto__, constructor, and prototype keys", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Ignores_dangerous_keys()
    {
        var source = Map(
            ("__proto__", Map(("a", 1))),
            ("constructor", Map(("prototype", Map(("b", 2))))),
            ("prototype", Map(("c", 3))),
            ("safe", "value"));
        var result = ObjectMerge.MergeObjects(Map(("existing", "ok")), source)!;
        Assert.Equal("ok", result["existing"]);
        Assert.Equal("value", result["safe"]);
        Assert.Equal(2, result.Count);
    }

    [UpstreamTest("packages/ai/src/util/merge-objects.test.ts::mergeObjects::should ignore dangerous keys nested in mergeable objects", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Ignores_nested_dangerous_keys()
    {
        var result = ObjectMerge.MergeObjects(
            Map(("metadata", Map(("user", "alice")))),
            Map(("metadata", Map(("__proto__", Map(("polluted", true))), ("role", "admin")))))!;
        var metadata = (IDictionary<string, object>)result["metadata"];
        Assert.Equal("alice", metadata["user"]);
        Assert.Equal("admin", metadata["role"]);
        Assert.False(metadata.ContainsKey("__proto__"));
        Assert.False(metadata.ContainsKey("polluted"));
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
