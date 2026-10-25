// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Providers.NormalizeJsonSchemaForMfjs.NormalizeJsonSchemaForMfjsSupport;

namespace Vercel.AI.Tests.Upstream.Providers.NormalizeJsonSchemaForMfjs;

/// <summary>Port of <c>normalize-json-schema-for-mfjs.test.ts</c> &gt; <c>identity (no normalization needed)</c>.</summary>
public sealed class IdentityNoNormalizationNeededTests
{
    [Fact]
    [UpstreamTest(TestFile + "::identity (no normalization needed)::passes a plain object schema through unchanged", Coverage = UpstreamCoverage.Covered)]
    public void Passes_a_plain_object_schema_through_unchanged()
    {
        var schema = Json("""
            {
              "type": "object",
              "properties": {
                "city": { "type": "string", "description": "The city" },
                "zip": { "type": "string", "pattern": "^[0-9]{5}$" }
              },
              "required": ["city"],
              "additionalProperties": false
            }
            """);

        AssertEqual(schema, Normalize(schema));
    }

    [Fact]
    [UpstreamTest(TestFile + "::identity (no normalization needed)::preserves keywords Moonshot accepts verbatim", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_keywords_Moonshot_accepts_verbatim()
    {
        var schema = Json("""
            {
              "$schema": "http://json-schema.org/draft-07/schema#",
              "type": "object",
              "properties": {
                "a": { "type": "string", "enum": ["x", "y"], "format": "uri" },
                "b": { "type": ["string", "null"] },
                "c": { "type": "array", "contains": { "type": "string" } },
                "d": { "$ref": "#/$defs/thing" }
              },
              "$defs": { "thing": { "type": "number" } },
              "not": { "type": "string" }
            }
            """);

        AssertEqual(schema, Normalize(schema));
    }

    [Fact]
    [UpstreamTest(TestFile + "::identity (no normalization needed)::passes anyOf without a sibling type through unchanged", Coverage = UpstreamCoverage.Covered)]
    public void Passes_anyOf_without_a_sibling_type_through_unchanged()
    {
        var schema = Json("""
            {
              "type": "object",
              "properties": {
                "location": {
                  "anyOf": [{ "type": "string" }, { "type": "number" }]
                }
              },
              "required": ["location"]
            }
            """);

        AssertEqual(schema, Normalize(schema));
    }

    [Fact]
    [UpstreamTest(TestFile + "::identity (no normalization needed)::leaves oneOf next to type alone (accepted by MFJS)", Coverage = UpstreamCoverage.Covered)]
    public void Leaves_oneOf_next_to_type_alone_accepted_by_MFJS()
    {
        var schema = Json("""
            {
              "type": "object",
              "oneOf": [
                { "properties": { "city": { "type": "string" } }, "required": ["city"] },
                { "properties": { "lat": { "type": "number" } }, "required": ["lat"] }
              ]
            }
            """);

        AssertEqual(schema, Normalize(schema));
    }

    [Fact]
    [UpstreamTest(TestFile + "::identity (no normalization needed)::leaves single-schema items alone", Coverage = UpstreamCoverage.Covered)]
    public void Leaves_single_schema_items_alone()
    {
        var schema = Json("""
            {
              "type": "object",
              "properties": {
                "a": {
                  "type": "array",
                  "items": { "type": "number" },
                  "minItems": 2,
                  "maxItems": 2
                }
              }
            }
            """);

        AssertEqual(schema, Normalize(schema));
    }
}
