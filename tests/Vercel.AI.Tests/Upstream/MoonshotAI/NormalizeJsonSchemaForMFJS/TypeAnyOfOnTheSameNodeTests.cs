// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Moonshot;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.NormalizeJsonSchemaForMFJS;

/// <summary>Port of <c>normalize-json-schema-for-mfjs.test.ts</c> &gt; <c>type + anyOf on the same node</c>.</summary>
public sealed class TypeAnyOfOnTheSameNodeTests
{
    private const string Prefix =
        "packages/moonshotai/src/normalize-json-schema-for-mfjs.test.ts::type + anyOf on the same node::";

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
        Prefix + "moves type into anyOf branches that lack one",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Moves_type_into_anyOf_branches_that_lack_one()
    {
        Normalizes(
            "{\"type\": \"object\", \"properties\": {\"location\": {\"type\": \"object\", \"anyOf\": [{\"properties\": {\"city\": {\"type\": \"string\"}}, \"required\": [\"city\"]}, {\"properties\": {\"lat\": {\"type\": \"number\"}, \"lon\": {\"type\": \"number\"}}, \"required\": [\"lat\", \"lon\"]}]}}}",
            "{\"type\": \"object\", \"properties\": {\"location\": {\"anyOf\": [{\"type\": \"object\", \"properties\": {\"city\": {\"type\": \"string\"}}, \"required\": [\"city\"]}, {\"type\": \"object\", \"properties\": {\"lat\": {\"type\": \"number\"}, \"lon\": {\"type\": \"number\"}}, \"required\": [\"lat\", \"lon\"]}]}}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "keeps branch types when already present",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Keeps_branch_types_when_already_present()
    {
        Normalizes(
            "{\"type\": \"object\", \"properties\": {\"value\": {\"type\": \"object\", \"anyOf\": [{\"type\": \"string\"}, {\"properties\": {\"lat\": {\"type\": \"number\"}}}]}}}",
            "{\"type\": \"object\", \"properties\": {\"value\": {\"anyOf\": [{\"type\": \"string\"}, {\"type\": \"object\", \"properties\": {\"lat\": {\"type\": \"number\"}}}]}}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "keeps type \"object\" at the root beside anyOf",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Keeps_type_object_at_the_root_beside_anyOf()
    {
        Identity(
            "{\"type\": \"object\", \"anyOf\": [{\"properties\": {\"city\": {\"type\": \"string\"}}, \"required\": [\"city\"]}, {\"properties\": {\"lat\": {\"type\": \"number\"}}, \"required\": [\"lat\"]}]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "splits type at nested levels, not just the root",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Splits_type_at_nested_levels_not_just_the_root()
    {
        Normalizes(
            "{\"type\": \"object\", \"properties\": {\"a\": {\"type\": \"object\", \"anyOf\": [{\"properties\": {\"city\": {\"type\": \"string\"}}, \"required\": [\"city\"]}, {\"properties\": {\"lat\": {\"type\": \"number\"}}, \"required\": [\"lat\"]}]}}, \"required\": [\"a\"]}",
            "{\"type\": \"object\", \"properties\": {\"a\": {\"anyOf\": [{\"type\": \"object\", \"properties\": {\"city\": {\"type\": \"string\"}}, \"required\": [\"city\"]}, {\"type\": \"object\", \"properties\": {\"lat\": {\"type\": \"number\"}}, \"required\": [\"lat\"]}]}}, \"required\": [\"a\"]}"
        );
    }
}
