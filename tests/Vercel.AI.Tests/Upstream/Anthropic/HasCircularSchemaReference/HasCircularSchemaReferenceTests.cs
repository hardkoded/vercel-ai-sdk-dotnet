// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Anthropic;

namespace Vercel.AI.Tests.Upstream.Anthropic.HasCircularSchemaReference;

public sealed class HasCircularSchemaReferenceTests
{
    private const string Prefix = "packages/anthropic/src/has-circular-schema-reference.test.ts::hasCircularSchemaReference::";

    [Fact]
    [UpstreamTest(Prefix + "returns false for a schema without references", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public void Returns_false_for_a_schema_without_references()
    {
        Check("{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}}}", false);
    }

    [Fact]
    [UpstreamTest(Prefix + "returns false for shared definitions without a cycle", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public void Returns_false_for_shared_definitions_without_a_cycle()
    {
        Check("{\"type\":\"object\",\"properties\":{\"home\":{\"$ref\":\"#/$defs/address\"},\"work\":{\"$ref\":\"#/$defs/address\"}},\"$defs\":{\"address\":{\"type\":\"object\",\"properties\":{\"country\":{\"$ref\":\"#/$defs/country\"}}},\"country\":{\"type\":\"string\"}}}", false);
    }

    [Fact]
    [UpstreamTest(Prefix + "returns false for a top-level $ref to a non-recursive definition", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public void Returns_false_for_a_top_level_ref_to_a_non_recursive_definition()
    {
        Check("{\"$ref\":\"#/definitions/Answer\",\"definitions\":{\"Answer\":{\"type\":\"object\",\"properties\":{\"answer\":{\"type\":\"string\"}}}}}", false);
    }

    [Fact]
    [UpstreamTest(Prefix + "detects a self-referencing definition", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public void Detects_a_self_referencing_definition()
    {
        Check("{\"type\":\"object\",\"properties\":{\"root\":{\"$ref\":\"#/$defs/node\"}},\"$defs\":{\"node\":{\"type\":\"object\",\"properties\":{\"children\":{\"type\":\"array\",\"items\":{\"$ref\":\"#/$defs/node\"}}}}}}", true);
    }

    [Fact]
    [UpstreamTest(Prefix + "detects mutually referencing definitions", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public void Detects_mutually_referencing_definitions()
    {
        Check("{\"$ref\":\"#/definitions/a\",\"definitions\":{\"a\":{\"type\":\"object\",\"properties\":{\"b\":{\"$ref\":\"#/definitions/b\"}}},\"b\":{\"type\":\"object\",\"properties\":{\"a\":{\"$ref\":\"#/definitions/a\"}}}}}", true);
    }

    [Fact]
    [UpstreamTest(Prefix + "detects a reference to the root schema", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public void Detects_a_reference_to_the_root_schema()
    {
        Check("{\"type\":\"object\",\"properties\":{\"parent\":{\"anyOf\":[{\"$ref\":\"#\"},{\"type\":\"null\"}]}}}", true);
    }

    [Fact]
    [UpstreamTest(Prefix + "ignores cycles in definitions that are never referenced", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public void Ignores_cycles_in_definitions_that_are_never_referenced()
    {
        Check("{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}},\"$defs\":{\"node\":{\"items\":{\"$ref\":\"#/$defs/node\"}}}}", false);
    }

    [Fact]
    [UpstreamTest(Prefix + "treats properties named like keywords as subschemas", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public void Treats_properties_named_like_keywords_as_subschemas()
    {
        Check("{\"type\":\"object\",\"properties\":{\"definitions\":{\"$ref\":\"#/$defs/node\"},\"$ref\":{\"type\":\"string\"}},\"$defs\":{\"node\":{\"type\":\"object\",\"properties\":{\"next\":{\"$ref\":\"#/$defs/node\"}}}}}", true);
    }

    [Fact]
    [UpstreamTest(Prefix + "ignores $ref keys inside instance data", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public void Ignores_ref_keys_inside_instance_data()
    {
        Check("{\"type\":\"object\",\"default\":{\"$ref\":\"#\"},\"properties\":{\"kind\":{\"enum\":[{\"$ref\":\"#\"}]}}}", false);
    }

    [Fact]
    [UpstreamTest(Prefix + "returns false for unresolvable or remote references", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public void Returns_false_for_unresolvable_or_remote_references()
    {
        Check("{\"type\":\"object\",\"properties\":{\"a\":{\"$ref\":\"#/$defs/missing\"},\"b\":{\"$ref\":\"https://example.com/schema.json\"},\"c\":{\"$ref\":\"#/%E0%A4%A\"}}}", false);
    }

    private static void Check(string schema, bool expected)
    {
        using var document = JsonDocument.Parse(schema);
        Assert.Equal(expected, HasCircularSchemaReferenceChecker.HasCircularSchemaReference(document.RootElement));
    }
}
