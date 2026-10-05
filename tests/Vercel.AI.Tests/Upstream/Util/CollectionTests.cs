// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class CollectionTests
{
    [UpstreamTest("packages/ai/src/util/split-array.test.ts::should split an array into chunks of the specified size", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Splits_into_chunks()
    {
        var result = Collections.SplitArray(new[] { 1, 2, 3, 4, 5 }, 2);
        Assert.Equal(3, result.Count);
        Assert.Equal(new[] { 1, 2 }, result[0]);
        Assert.Equal(new[] { 3, 4 }, result[1]);
        Assert.Equal(new[] { 5 }, result[2]);
    }

    [UpstreamTest("packages/ai/src/util/split-array.test.ts::should return an empty array when the input array is empty", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Splits_an_empty_array()
    {
        Assert.Empty(Collections.SplitArray(Array.Empty<int>(), 2));
    }

    [UpstreamTest("packages/ai/src/util/split-array.test.ts::should return the original array when the chunk size is greater than the array length", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Keeps_one_chunk_when_size_is_larger()
    {
        var result = Collections.SplitArray(new[] { 1, 2, 3 }, 5);
        var chunk = Assert.Single(result);
        Assert.Equal(new[] { 1, 2, 3 }, chunk);
    }

    [UpstreamTest("packages/ai/src/util/split-array.test.ts::should return the original array when the chunk size is equal to the array length", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Keeps_one_chunk_when_size_matches()
    {
        var result = Collections.SplitArray(new[] { 1, 2, 3 }, 3);
        var chunk = Assert.Single(result);
        Assert.Equal(new[] { 1, 2, 3 }, chunk);
    }

    [UpstreamTest("packages/ai/src/util/split-array.test.ts::should handle chunk size of 1 correctly", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Splits_into_singletons()
    {
        var result = Collections.SplitArray(new[] { 1, 2, 3 }, 1);
        Assert.Equal(3, result.Count);
        Assert.Equal(new[] { 1 }, result[0]);
        Assert.Equal(new[] { 2 }, result[1]);
        Assert.Equal(new[] { 3 }, result[2]);
    }

    [UpstreamTest("packages/ai/src/util/split-array.test.ts::should throw InvalidArgumentError for chunk size %s", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Rejects_non_positive_chunk_sizes()
    {
        AssertChunkSize(0);
        AssertChunkSize(-1);
    }

    [UpstreamTest("packages/ai/src/util/split-array.test.ts::should handle non-integer chunk size by flooring the size", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Floors_a_non_integer_chunk_size_before_splitting()
    {
        var size = (int)Math.Floor(2.5d);
        var result = Collections.SplitArray(new[] { 1, 2, 3, 4, 5 }, size);
        Assert.Equal(3, result.Count);
        Assert.Equal(new[] { 1, 2 }, result[0]);
        Assert.Equal(new[] { 3, 4 }, result[1]);
        Assert.Equal(new[] { 5 }, result[2]);
    }

    [UpstreamTest("packages/ai/src/util/get-own.test.ts::getOwn::returns own properties", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Returns_own_properties()
    {
        var obj = new DataObject();
        obj.Properties["a"] = 1;
        obj.Properties["b"] = 2;
        Assert.Equal(1, Collections.GetOwn(obj, "a")!);
    }

    [UpstreamTest("packages/ai/src/util/get-own.test.ts::getOwn::returns undefined for absent keys", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Returns_null_for_absent_keys()
    {
        var obj = new DataObject();
        obj.Properties["a"] = 1;
        Assert.Null(Collections.GetOwn(obj, "missing"));
    }

    [UpstreamTest("packages/ai/src/util/get-own.test.ts::getOwn::returns undefined for inherited object properties rather than a prototype value", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Ignores_inherited_names()
    {
        var obj = new DataObject();
        obj.Properties["a"] = 1;
        Assert.Null(Collections.GetOwn(obj, "constructor"));
        Assert.Null(Collections.GetOwn(obj, "toString"));
        Assert.Null(Collections.GetOwn(obj, "valueOf"));
        Assert.Null(Collections.GetOwn(obj, "__proto__"));
    }

    [UpstreamTest("packages/ai/src/util/get-own.test.ts::getOwn::still returns an own property that shadows an inherited name", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Returns_a_shadowed_own_property()
    {
        var obj = new DataObject();
        obj.Properties["toString"] = "shadowed";
        Assert.Equal("shadowed", Collections.GetOwn(obj, "toString")!);
    }

    [UpstreamTest("packages/ai/src/util/get-own.test.ts::getOwn::returns undefined for null/undefined objects", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Returns_null_for_null_objects()
    {
        Assert.Null(Collections.GetOwn((DataObject?)null, "a"));
        Assert.Null(Collections.GetOwn((IDictionary<string, object>?)null, "a"));
    }

    [UpstreamTest("packages/ai/src/util/get-potential-start-index.test.ts::getPotentialStartIndex::should return null when searchedText is empty", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Returns_null_for_an_empty_search()
    {
        Assert.Null(Collections.GetPotentialStartIndex("1234567890", string.Empty));
    }

    [UpstreamTest("packages/ai/src/util/get-potential-start-index.test.ts::getPotentialStartIndex::should return null when searchedText is not in text", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Returns_null_when_the_search_is_absent()
    {
        Assert.Null(Collections.GetPotentialStartIndex("1234567890", "a"));
    }

    [UpstreamTest("packages/ai/src/util/get-potential-start-index.test.ts::getPotentialStartIndex::should return index when searchedText is in text", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Returns_the_full_match_index()
    {
        Assert.Equal(0, Collections.GetPotentialStartIndex("1234567890", "1234567890"));
    }

    [UpstreamTest("packages/ai/src/util/get-potential-start-index.test.ts::getPotentialStartIndex::should return index when searchedText might start in text", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Returns_a_suffix_prefix_index()
    {
        Assert.Equal(9, Collections.GetPotentialStartIndex("1234567890", "0123"));
    }

    [UpstreamTest("packages/ai/src/util/get-potential-start-index.test.ts::getPotentialStartIndex::should return index when searchedText might start in text #2", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Returns_a_longer_suffix_prefix_index()
    {
        Assert.Equal(8, Collections.GetPotentialStartIndex("1234567890", "90123"));
    }

    [UpstreamTest("packages/ai/src/util/get-potential-start-index.test.ts::getPotentialStartIndex::should return index when searchedText might start in text #3", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Returns_the_longest_suffix_prefix_index()
    {
        Assert.Equal(7, Collections.GetPotentialStartIndex("1234567890", "890123"));
    }

    [UpstreamTest("packages/ai/src/util/create-id-map.test.ts::createIdMap::should create a null-prototype map", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Id_map_has_no_prototype()
    {
        var map = new IdMap<string>();
        Assert.Null(map.Prototype);
    }

    [UpstreamTest("packages/ai/src/util/create-id-map.test.ts::createIdMap::should store object prototype property names as own keys", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Id_map_stores_prototype_names_as_own_keys()
    {
        var map = new IdMap<string>();
        map.Set("__proto__", "proto value");
        map.Set("constructor", "constructor value");
        map.Set("prototype", "prototype value");
        Assert.Equal("proto value", map.Get("__proto__")!);
        Assert.Equal("constructor value", map.Get("constructor")!);
        Assert.Equal("prototype value", map.Get("prototype")!);
        Assert.Equal(new[] { "__proto__", "constructor", "prototype" }, map.Keys);
    }

    [UpstreamTest("packages/ai/src/util/create-id-map.test.ts::createIdMap::should prevent prototype pollution from missing __proto__ lookups", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Id_map_missing_proto_lookup_does_not_pollute()
    {
        var map = new IdMap<string>();
        Assert.Null(map.Get("__proto__"));
        Assert.False(map.Has("__proto__"));
        Assert.Null(map.Prototype);
        Assert.Empty(map.Keys);
    }

    private static void AssertChunkSize(int size)
    {
        var error = Assert.Throws<InvalidArgumentError>(delegate
        {
            Collections.SplitArray(new[] { 1, 2, 3 }, size);
        });
        Assert.True(InvalidArgumentError.IsInstance(error));
        Assert.Equal("chunkSize", error.Parameter);
        Assert.Equal(size, Assert.IsType<int>(error.Value!));
        Assert.Equal("Invalid argument for parameter chunkSize: chunkSize must be greater than 0", error.Message);
    }
}
