// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Moonshot;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.NormalizeJsonSchemaForMFJS;

/// <summary>Port of <c>normalize-json-schema-for-mfjs.test.ts</c> &gt; <c>root unions of objects</c>.</summary>
public sealed class RootUnionsOfObjectsTests
{
    private const string Prefix =
        "packages/moonshotai/src/normalize-json-schema-for-mfjs.test.ts::root unions of objects::";

    private static JsonNode Json(string json) => JsonNode.Parse(json)!;

    private static void Identity(string json)
    {
        JsonAssert.Equal(MoonshotJsonSchema.NormalizeJsonSchemaForMFJS(Json(json)), json);
    }

    private static void Normalizes(string input, string expected)
    {
        JsonAssert.Equal(MoonshotJsonSchema.NormalizeJsonSchemaForMFJS(Json(input)), expected);
    }

    private static void Throws(string input)
    {
        Assert.Throws<UnsupportedFunctionalityException>(() =>
            MoonshotJsonSchema.NormalizeJsonSchemaForMFJS(Json(input))
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "types a root anyOf of objects as an object",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Types_a_root_anyOf_of_objects_as_an_object()
    {
        Normalizes(
            "{\"anyOf\": [{\"type\": \"object\", \"properties\": {\"city\": {\"type\": \"string\"}}, \"required\": [\"city\"]}, {\"properties\": {\"lat\": {\"type\": \"number\"}}, \"required\": [\"lat\"]}]}",
            "{\"type\": \"object\", \"anyOf\": [{\"type\": \"object\", \"properties\": {\"city\": {\"type\": \"string\"}}, \"required\": [\"city\"]}, {\"properties\": {\"lat\": {\"type\": \"number\"}}, \"required\": [\"lat\"]}]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "types a root oneOf of objects as an object",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Types_a_root_oneOf_of_objects_as_an_object()
    {
        Normalizes(
            "{\"oneOf\": [{\"type\": \"object\", \"properties\": {\"city\": {\"type\": \"string\"}}, \"required\": [\"city\"]}, {\"properties\": {\"lat\": {\"type\": \"number\"}}, \"required\": [\"lat\"]}]}",
            "{\"type\": \"object\", \"oneOf\": [{\"type\": \"object\", \"properties\": {\"city\": {\"type\": \"string\"}}, \"required\": [\"city\"]}, {\"properties\": {\"lat\": {\"type\": \"number\"}}, \"required\": [\"lat\"]}]}"
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "resolves local $ref branches", Coverage = UpstreamCoverage.Covered)]
    public void Resolves_local_ref_branches()
    {
        Normalizes(
            "{\"$defs\": {\"City\": {\"type\": \"object\", \"properties\": {\"city\": {\"type\": \"string\"}}, \"required\": [\"city\"]}, \"Coords\": {\"properties\": {\"lat\": {\"type\": \"number\"}}, \"required\": [\"lat\"]}}, \"anyOf\": [{\"$ref\": \"#/$defs/City\"}, {\"$ref\": \"#/definitions/Coords\"}], \"definitions\": {\"Coords\": {\"properties\": {\"lat\": {\"type\": \"number\"}}, \"required\": [\"lat\"]}}}",
            "{\"type\": \"object\", \"$defs\": {\"City\": {\"type\": \"object\", \"properties\": {\"city\": {\"type\": \"string\"}}, \"required\": [\"city\"]}, \"Coords\": {\"properties\": {\"lat\": {\"type\": \"number\"}}, \"required\": [\"lat\"]}}, \"anyOf\": [{\"$ref\": \"#/$defs/City\"}, {\"$ref\": \"#/definitions/Coords\"}], \"definitions\": {\"Coords\": {\"properties\": {\"lat\": {\"type\": \"number\"}}, \"required\": [\"lat\"]}}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "throws for a non-local $ref branch",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Throws_for_a_non_local_ref_branch()
    {
        Throws(
            "{\"anyOf\": [{\"type\": \"object\", \"properties\": {\"city\": {\"type\": \"string\"}}, \"required\": [\"city\"]}, {\"$ref\": \"https://example.com/coords.json\"}]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "requires every root union to be objects",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Requires_every_root_union_to_be_objects()
    {
        Throws(
            "{\"anyOf\": [{\"type\": \"object\", \"properties\": {\"city\": {\"type\": \"string\"}}, \"required\": [\"city\"]}, {\"properties\": {\"lat\": {\"type\": \"number\"}}, \"required\": [\"lat\"]}], \"oneOf\": [{\"type\": \"object\", \"properties\": {\"city\": {\"type\": \"string\"}}, \"required\": [\"city\"]}, {\"type\": \"string\"}]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "throws for a nullable object branch",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Throws_for_a_nullable_object_branch()
    {
        Throws(
            "{\"anyOf\": [{\"type\": \"object\", \"properties\": {\"city\": {\"type\": \"string\"}}, \"required\": [\"city\"]}, {\"type\": [\"object\", \"null\"], \"properties\": {}}]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "passes an object root with anyOf nested in allOf through unchanged",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Passes_an_object_root_with_anyOf_nested_in_allOf_through_unchanged()
    {
        Identity(
            "{\"type\": \"object\", \"allOf\": [{\"anyOf\": [{\"type\": \"object\", \"properties\": {\"city\": {\"type\": \"string\"}}, \"required\": [\"city\"]}, {\"properties\": {\"lat\": {\"type\": \"number\"}}, \"required\": [\"lat\"]}]}]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "still throws for a root union with a non-object branch",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Still_throws_for_a_root_union_with_a_non_object_branch()
    {
        Throws(
            "{\"anyOf\": [{\"type\": \"object\", \"properties\": {\"city\": {\"type\": \"string\"}}, \"required\": [\"city\"]}, {\"type\": \"string\"}]}"
        );
    }
}
