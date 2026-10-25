// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using static Vercel.AI.Tests.Upstream.Providers.NormalizeJsonSchemaForMfjs.NormalizeJsonSchemaForMfjsSupport;

namespace Vercel.AI.Tests.Upstream.Providers.NormalizeJsonSchemaForMfjs;

/// <summary>Port of <c>normalize-json-schema-for-mfjs.test.ts</c> &gt; <c>root unions of objects</c>.</summary>
public sealed class RootUnionsOfObjectsTests
{
    private const string City = """{ "type": "object", "properties": { "city": { "type": "string" } }, "required": ["city"] }""";

    private const string Coords = """{ "properties": { "lat": { "type": "number" } }, "required": ["lat"] }""";

    [Fact]
    [UpstreamTest(TestFile + "::root unions of objects::types a root anyOf of objects as an object", Coverage = UpstreamCoverage.Covered)]
    public void Types_a_root_anyOf_of_objects_as_an_object()
    {
        AssertEqual(
            Json("{ \"type\": \"object\", \"anyOf\": [" + City + ", " + Coords + "] }"),
            Normalize(Json("{ \"anyOf\": [" + City + ", " + Coords + "] }")));
    }

    [Fact]
    [UpstreamTest(TestFile + "::root unions of objects::types a root oneOf of objects as an object", Coverage = UpstreamCoverage.Covered)]
    public void Types_a_root_oneOf_of_objects_as_an_object()
    {
        AssertEqual(
            Json("{ \"type\": \"object\", \"oneOf\": [" + City + ", " + Coords + "] }"),
            Normalize(Json("{ \"oneOf\": [" + City + ", " + Coords + "] }")));
    }

    [Fact]
    [UpstreamTest(TestFile + "::root unions of objects::resolves local $ref branches", Coverage = UpstreamCoverage.Covered)]
    public void Resolves_local_ref_branches()
    {
        var schema = (JsonObject)Json(
            "{ \"$defs\": { \"City\": " + City + ", \"Coords\": " + Coords + " },"
            + " \"anyOf\": [{ \"$ref\": \"#/$defs/City\" }, { \"$ref\": \"#/definitions/Coords\" }],"
            + " \"definitions\": { \"Coords\": " + Coords + " } }");

        var expected = new JsonObject { ["type"] = "object" };
        foreach (var pair in schema)
        {
            expected[pair.Key] = pair.Value?.DeepClone();
        }

        AssertEqual(expected, Normalize(schema));
    }

    [Fact]
    [UpstreamTest(TestFile + "::root unions of objects::throws for a non-local $ref branch", Coverage = UpstreamCoverage.Covered)]
    public void Throws_for_a_non_local_ref_branch()
    {
        Assert.Throws<UnsupportedFunctionalityException>(() => Normalize(Json(
            "{ \"anyOf\": [" + City + ", { \"$ref\": \"https://example.com/coords.json\" }] }")));
    }

    [Fact]
    [UpstreamTest(TestFile + "::root unions of objects::requires every root union to be objects", Coverage = UpstreamCoverage.Covered)]
    public void Requires_every_root_union_to_be_objects()
    {
        Assert.Throws<UnsupportedFunctionalityException>(() => Normalize(Json(
            "{ \"anyOf\": [" + City + ", " + Coords + "], \"oneOf\": [" + City + ", { \"type\": \"string\" }] }")));
    }

    [Fact]
    [UpstreamTest(TestFile + "::root unions of objects::throws for a nullable object branch", Coverage = UpstreamCoverage.Covered)]
    public void Throws_for_a_nullable_object_branch()
    {
        Assert.Throws<UnsupportedFunctionalityException>(() => Normalize(Json(
            "{ \"anyOf\": [" + City + ", { \"type\": [\"object\", \"null\"], \"properties\": {} }] }")));
    }

    [Fact]
    [UpstreamTest(TestFile + "::root unions of objects::passes an object root with anyOf nested in allOf through unchanged", Coverage = UpstreamCoverage.Covered)]
    public void Passes_an_object_root_with_anyOf_nested_in_allOf_through_unchanged()
    {
        var schema = Json("{ \"type\": \"object\", \"allOf\": [{ \"anyOf\": [" + City + ", " + Coords + "] }] }");

        AssertEqual(schema, Normalize(schema));
    }

    [Fact]
    [UpstreamTest(TestFile + "::root unions of objects::still throws for a root union with a non-object branch", Coverage = UpstreamCoverage.Covered)]
    public void Still_throws_for_a_root_union_with_a_non_object_branch()
    {
        Assert.Throws<UnsupportedFunctionalityException>(() => Normalize(Json(
            "{ \"anyOf\": [" + City + ", { \"type\": \"string\" }] }")));
    }
}
