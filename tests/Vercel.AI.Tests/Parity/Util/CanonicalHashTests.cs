// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class CanonicalHashTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/canonical-hash.test.ts::canonicalJSON::is independent of key insertion order",
        Coverage = UpstreamCoverage.Covered)]
    public void Canonical_json_ignores_key_insertion_order()
    {
        var left = new Dictionary<string, object?> { ["a"] = 1, ["b"] = 2 };
        var right = new Dictionary<string, object?> { ["b"] = 2, ["a"] = 1 };
        Assert.Equal(CanonicalHash.CanonicalJson(left), CanonicalHash.CanonicalJson(right));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/canonical-hash.test.ts::canonicalJSON::sorts keys recursively",
        Coverage = UpstreamCoverage.Covered)]
    public void Canonical_json_sorts_keys_recursively()
    {
        var value = new Dictionary<string, object?>
        {
            ["b"] = new Dictionary<string, object?> { ["y"] = 1, ["x"] = 2 },
            ["a"] = new object?[] { 3, new Dictionary<string, object?> { ["d"] = 1, ["c"] = 2 } },
        };
        Assert.Equal("{\"a\":[3,{\"c\":2,\"d\":1}],\"b\":{\"x\":2,\"y\":1}}", CanonicalHash.CanonicalJson(value));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/canonical-hash.test.ts::canonicalJSON::serializes primitives and null/undefined",
        Coverage = UpstreamCoverage.Covered)]
    public void Canonical_json_serializes_primitives()
    {
        Assert.Equal("null", CanonicalHash.CanonicalJson(null));
        Assert.Null(CanonicalHash.CanonicalJson(JsonUndefined.Value));
        Assert.Equal("\"x\"", CanonicalHash.CanonicalJson("x"));
        Assert.Equal("42", CanonicalHash.CanonicalJson(42));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/canonical-hash.test.ts::canonicalJSON::preserves undefined array element positions",
        Coverage = UpstreamCoverage.Covered)]
    public void Canonical_json_preserves_undefined_array_slots()
    {
        Assert.Equal("[]", CanonicalHash.CanonicalJson(new object?[0]));
        Assert.Equal("[null]", CanonicalHash.CanonicalJson(new object?[] { JsonUndefined.Value }));
        var wrapped = new Dictionary<string, object?>
        {
            ["values"] = new object?[] { JsonUndefined.Value },
        };
        Assert.Equal("{\"values\":[null]}", CanonicalHash.CanonicalJson(wrapped));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/canonical-hash.test.ts::hashCanonical::produces a stable base64url digest",
        Coverage = UpstreamCoverage.Covered)]
    public void Hash_is_stable_base64url()
    {
        var value = new Dictionary<string, object?> { ["a"] = 1, ["b"] = 2 };
        var digest = CanonicalHash.HashCanonical(value);
        Assert.Matches("^[A-Za-z0-9_-]+$", digest);
        Assert.Equal(digest, CanonicalHash.HashCanonical(value));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/canonical-hash.test.ts::hashCanonical::is independent of key order",
        Coverage = UpstreamCoverage.Covered)]
    public void Hash_ignores_key_order()
    {
        var left = new Dictionary<string, object?> { ["a"] = 1, ["b"] = 2 };
        var right = new Dictionary<string, object?> { ["b"] = 2, ["a"] = 1 };
        Assert.Equal(CanonicalHash.HashCanonical(left), CanonicalHash.HashCanonical(right));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/canonical-hash.test.ts::hashCanonical::changes when the value changes",
        Coverage = UpstreamCoverage.Covered)]
    public void Hash_changes_when_the_value_changes()
    {
        Assert.NotEqual(
            CanonicalHash.HashCanonical(new Dictionary<string, object?> { ["a"] = 1 }),
            CanonicalHash.HashCanonical(new Dictionary<string, object?> { ["a"] = 2 }));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/canonical-hash.test.ts::hashCanonical::distinguishes empty arrays from arrays containing undefined",
        Coverage = UpstreamCoverage.Covered)]
    public void Hash_distinguishes_empty_and_undefined_arrays()
    {
        Assert.NotEqual(CanonicalHash.HashCanonical(new object?[0]), CanonicalHash.HashCanonical(new object?[] { JsonUndefined.Value }));
        Assert.NotEqual(
            CanonicalHash.HashCanonical(new Dictionary<string, object?> { ["values"] = new object?[0] }),
            CanonicalHash.HashCanonical(new Dictionary<string, object?> { ["values"] = new object?[] { JsonUndefined.Value } }));
    }
}
