// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Providers.NormalizeJsonSchemaForMfjs.NormalizeJsonSchemaForMfjsSupport;

namespace Vercel.AI.Tests.Upstream.Providers.NormalizeJsonSchemaForMfjs;

/// <summary>Port of <c>normalize-json-schema-for-mfjs.test.ts</c> &gt; <c>tuple items -&gt; prefixItems</c>.</summary>
public sealed class TupleItemsPrefixItemsTests
{
    [Fact]
    [UpstreamTest(TestFile + "::tuple items -> prefixItems::rewrites a tuple items array to prefixItems and drops items", Coverage = UpstreamCoverage.Covered)]
    public void Rewrites_a_tuple_items_array_to_prefixItems_and_drops_items()
    {
        var result = Normalize(Json("""
            {
              "type": "object",
              "properties": {
                "a": { "type": "array", "items": [{ "type": "number" }, { "type": "number" }] }
              },
              "required": ["a"]
            }
            """));

        AssertEqual(Json("""
            {
              "type": "object",
              "properties": {
                "a": { "type": "array", "prefixItems": [{ "type": "number" }, { "type": "number" }] }
              },
              "required": ["a"]
            }
            """), result);
    }

    [Fact]
    [UpstreamTest(TestFile + "::tuple items -> prefixItems::preserves minItems and maxItems when rewriting tuples", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_minItems_and_maxItems_when_rewriting_tuples()
    {
        var result = Normalize(Json("""
            {
              "type": "object",
              "properties": {
                "a": {
                  "type": "array",
                  "items": [{ "type": "string" }, { "type": "number" }],
                  "minItems": 2,
                  "maxItems": 2
                }
              }
            }
            """));

        AssertEqual(Json("""
            {
              "type": "object",
              "properties": {
                "a": {
                  "type": "array",
                  "prefixItems": [{ "type": "string" }, { "type": "number" }],
                  "minItems": 2,
                  "maxItems": 2
                }
              }
            }
            """), result);
    }

    [Fact]
    [UpstreamTest(TestFile + "::tuple items -> prefixItems::appends tuple items after existing prefixItems", Coverage = UpstreamCoverage.Covered)]
    public void Appends_tuple_items_after_existing_prefixItems()
    {
        var result = Normalize(Json("""
            {
              "type": "object",
              "properties": {
                "a": {
                  "type": "array",
                  "prefixItems": [{ "type": "string" }],
                  "items": [{ "type": "number" }]
                }
              }
            }
            """));

        AssertEqual(Json("""
            {
              "type": "object",
              "properties": {
                "a": {
                  "type": "array",
                  "prefixItems": [{ "type": "string" }, { "type": "number" }]
                }
              }
            }
            """), result);
    }

    [Fact]
    [UpstreamTest(TestFile + "::tuple items -> prefixItems::rewrites tuples nested in anyOf branches", Coverage = UpstreamCoverage.Covered)]
    public void Rewrites_tuples_nested_in_anyOf_branches()
    {
        var result = Normalize(Json("""
            {
              "type": "object",
              "properties": {
                "a": {
                  "anyOf": [
                    { "type": "array", "items": [{ "type": "string" }, { "type": "number" }] },
                    { "type": "string" }
                  ]
                }
              }
            }
            """));

        AssertEqual(Json("""
            {
              "type": "object",
              "properties": {
                "a": {
                  "anyOf": [
                    { "type": "array", "prefixItems": [{ "type": "string" }, { "type": "number" }] },
                    { "type": "string" }
                  ]
                }
              }
            }
            """), result);
    }
}
