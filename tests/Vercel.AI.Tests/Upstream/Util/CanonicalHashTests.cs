// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;
using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class CanonicalHashTests
{
    [UpstreamTest("packages/ai/src/util/canonical-hash.test.ts::canonicalJSON::is independent of key insertion order", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Canonical_json_ignores_key_order()
    {
        var first = new Dictionary<string, object> { { "a", 1 }, { "b", 2 } };
        var second = new Dictionary<string, object> { { "b", 2 }, { "a", 1 } };
        Assert.Equal(CanonicalHash.CanonicalJson(first)!, CanonicalHash.CanonicalJson(second)!);
    }

    [UpstreamTest("packages/ai/src/util/canonical-hash.test.ts::canonicalJSON::sorts keys recursively", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Canonical_json_sorts_recursively()
    {
        var value = new Dictionary<string, object>
        {
            { "b", new Dictionary<string, object> { { "y", 1 }, { "x", 2 } } },
            { "a", new object[] { 3, new Dictionary<string, object> { { "d", 1 }, { "c", 2 } } } },
        };
        Assert.Equal("{\"a\":[3,{\"c\":2,\"d\":1}],\"b\":{\"x\":2,\"y\":1}}", CanonicalHash.CanonicalJson(value)!);
    }

    [UpstreamTest("packages/ai/src/util/canonical-hash.test.ts::canonicalJSON::serializes primitives and null/undefined", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Canonical_json_serializes_primitives()
    {
        Assert.Equal("null", CanonicalHash.CanonicalJson(null)!);
        Assert.Null(CanonicalHash.CanonicalJson(JsUndefined.Value));
        Assert.Equal("\"x\"", CanonicalHash.CanonicalJson("x")!);
        Assert.Equal("42", CanonicalHash.CanonicalJson(42)!);
    }

    [UpstreamTest("packages/ai/src/util/canonical-hash.test.ts::canonicalJSON::preserves undefined array element positions", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Canonical_json_keeps_undefined_holes()
    {
        Assert.Equal("[]", CanonicalHash.CanonicalJson(Array.Empty<object>())!);
        Assert.Equal("[null]", CanonicalHash.CanonicalJson(new object[] { JsUndefined.Value })!);
        var wrapped = new Dictionary<string, object> { { "values", new object[] { JsUndefined.Value } } };
        Assert.Equal("{\"values\":[null]}", CanonicalHash.CanonicalJson(wrapped)!);
    }

    [UpstreamTest("packages/ai/src/util/canonical-hash.test.ts::hashCanonical::produces a stable base64url digest", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Hash_is_stable_base64url()
    {
        var value = new Dictionary<string, object> { { "a", 1 }, { "b", 2 } };
        var digest = CanonicalHash.HashCanonical(value);
        Assert.Matches(new Regex("^[A-Za-z0-9_-]+$"), digest);
        Assert.Equal(digest, CanonicalHash.HashCanonical(value));
    }

    [UpstreamTest("packages/ai/src/util/canonical-hash.test.ts::hashCanonical::is independent of key order", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Hash_ignores_key_order()
    {
        var first = new Dictionary<string, object> { { "a", 1 }, { "b", 2 } };
        var second = new Dictionary<string, object> { { "b", 2 }, { "a", 1 } };
        Assert.Equal(CanonicalHash.HashCanonical(first), CanonicalHash.HashCanonical(second));
    }

    [UpstreamTest("packages/ai/src/util/canonical-hash.test.ts::hashCanonical::changes when the value changes", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Hash_changes_with_the_value()
    {
        Assert.NotEqual(
            CanonicalHash.HashCanonical(new Dictionary<string, object> { { "a", 1 } }),
            CanonicalHash.HashCanonical(new Dictionary<string, object> { { "a", 2 } }));
    }

    [UpstreamTest("packages/ai/src/util/canonical-hash.test.ts::hashCanonical::distinguishes empty arrays from arrays containing undefined", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Hash_distinguishes_empty_and_undefined_arrays()
    {
        Assert.NotEqual(CanonicalHash.HashCanonical(Array.Empty<object>()), CanonicalHash.HashCanonical(new object[] { JsUndefined.Value }));
        Assert.NotEqual(
            CanonicalHash.HashCanonical(new Dictionary<string, object> { { "values", Array.Empty<object>() } }),
            CanonicalHash.HashCanonical(new Dictionary<string, object> { { "values", new object[] { JsUndefined.Value } } }));
    }
}
