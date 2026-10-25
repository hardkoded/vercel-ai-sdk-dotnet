// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Providers.NormalizeJsonSchemaForMfjs.NormalizeJsonSchemaForMfjsSupport;

namespace Vercel.AI.Tests.Upstream.Providers.NormalizeJsonSchemaForMfjs;

/// <summary>Port of <c>normalize-json-schema-for-mfjs.test.ts</c> &gt; <c>type + anyOf on the same node</c>.</summary>
public sealed class TypeAnyOfOnTheSameNodeTests
{
    [Fact]
    [UpstreamTest(TestFile + "::type + anyOf on the same node::moves type into anyOf branches that lack one", Coverage = UpstreamCoverage.Covered)]
    public void Moves_type_into_anyOf_branches_that_lack_one()
    {
        var result = Normalize(Json("""
            {
              "type": "object",
              "properties": {
                "location": {
                  "type": "object",
                  "anyOf": [
                    { "properties": { "city": { "type": "string" } }, "required": ["city"] },
                    {
                      "properties": { "lat": { "type": "number" }, "lon": { "type": "number" } },
                      "required": ["lat", "lon"]
                    }
                  ]
                }
              }
            }
            """));

        AssertEqual(Json("""
            {
              "type": "object",
              "properties": {
                "location": {
                  "anyOf": [
                    {
                      "type": "object",
                      "properties": { "city": { "type": "string" } },
                      "required": ["city"]
                    },
                    {
                      "type": "object",
                      "properties": { "lat": { "type": "number" }, "lon": { "type": "number" } },
                      "required": ["lat", "lon"]
                    }
                  ]
                }
              }
            }
            """), result);
    }

    [Fact]
    [UpstreamTest(TestFile + "::type + anyOf on the same node::keeps branch types when already present", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_branch_types_when_already_present()
    {
        var result = Normalize(Json("""
            {
              "type": "object",
              "properties": {
                "value": {
                  "type": "object",
                  "anyOf": [
                    { "type": "string" },
                    { "properties": { "lat": { "type": "number" } } }
                  ]
                }
              }
            }
            """));

        AssertEqual(Json("""
            {
              "type": "object",
              "properties": {
                "value": {
                  "anyOf": [
                    { "type": "string" },
                    { "type": "object", "properties": { "lat": { "type": "number" } } }
                  ]
                }
              }
            }
            """), result);
    }

    [Fact]
    [UpstreamTest(TestFile + "::type + anyOf on the same node::keeps type \"object\" at the root beside anyOf", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_type_object_at_the_root_beside_anyOf()
    {
        var schema = Json("""
            {
              "type": "object",
              "anyOf": [
                { "properties": { "city": { "type": "string" } }, "required": ["city"] },
                { "properties": { "lat": { "type": "number" } }, "required": ["lat"] }
              ]
            }
            """);

        AssertEqual(schema, Normalize(schema));
    }

    [Fact]
    [UpstreamTest(TestFile + "::type + anyOf on the same node::splits type at nested levels, not just the root", Coverage = UpstreamCoverage.Covered)]
    public void Splits_type_at_nested_levels_not_just_the_root()
    {
        var result = Normalize(Json("""
            {
              "type": "object",
              "properties": {
                "a": {
                  "type": "object",
                  "anyOf": [
                    { "properties": { "city": { "type": "string" } }, "required": ["city"] },
                    { "properties": { "lat": { "type": "number" } }, "required": ["lat"] }
                  ]
                }
              },
              "required": ["a"]
            }
            """));

        AssertEqual(Json("""
            {
              "type": "object",
              "properties": {
                "a": {
                  "anyOf": [
                    {
                      "type": "object",
                      "properties": { "city": { "type": "string" } },
                      "required": ["city"]
                    },
                    {
                      "type": "object",
                      "properties": { "lat": { "type": "number" } },
                      "required": ["lat"]
                    }
                  ]
                }
              },
              "required": ["a"]
            }
            """), result);
    }
}
