// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Moonshot;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.NormalizeJsonSchemaForMFJS;

/// <summary>Port of <c>normalize-json-schema-for-mfjs.test.ts</c> &gt; <c>tuple items -&gt; prefixItems</c>.</summary>
public sealed class TupleItemsPrefixItemsTests
{
    private const string Prefix =
        "packages/moonshotai/src/normalize-json-schema-for-mfjs.test.ts::tuple items -> prefixItems::";

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
        Prefix + "rewrites a tuple items array to prefixItems and drops items",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Rewrites_a_tuple_items_array_to_prefixItems_and_drops_items()
    {
        Normalizes(
            "{\"type\": \"object\", \"properties\": {\"a\": {\"type\": \"array\", \"items\": [{\"type\": \"number\"}, {\"type\": \"number\"}]}}, \"required\": [\"a\"]}",
            "{\"type\": \"object\", \"properties\": {\"a\": {\"type\": \"array\", \"prefixItems\": [{\"type\": \"number\"}, {\"type\": \"number\"}]}}, \"required\": [\"a\"]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves minItems and maxItems when rewriting tuples",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Preserves_minItems_and_maxItems_when_rewriting_tuples()
    {
        Normalizes(
            "{\"type\": \"object\", \"properties\": {\"a\": {\"type\": \"array\", \"items\": [{\"type\": \"string\"}, {\"type\": \"number\"}], \"minItems\": 2, \"maxItems\": 2}}}",
            "{\"type\": \"object\", \"properties\": {\"a\": {\"type\": \"array\", \"prefixItems\": [{\"type\": \"string\"}, {\"type\": \"number\"}], \"minItems\": 2, \"maxItems\": 2}}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "appends tuple items after existing prefixItems",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Appends_tuple_items_after_existing_prefixItems()
    {
        Normalizes(
            "{\"type\": \"object\", \"properties\": {\"a\": {\"type\": \"array\", \"prefixItems\": [{\"type\": \"string\"}], \"items\": [{\"type\": \"number\"}]}}}",
            "{\"type\": \"object\", \"properties\": {\"a\": {\"type\": \"array\", \"prefixItems\": [{\"type\": \"string\"}, {\"type\": \"number\"}]}}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "rewrites tuples nested in anyOf branches",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Rewrites_tuples_nested_in_anyOf_branches()
    {
        Normalizes(
            "{\"type\": \"object\", \"properties\": {\"a\": {\"anyOf\": [{\"type\": \"array\", \"items\": [{\"type\": \"string\"}, {\"type\": \"number\"}]}, {\"type\": \"string\"}]}}}",
            "{\"type\": \"object\", \"properties\": {\"a\": {\"anyOf\": [{\"type\": \"array\", \"prefixItems\": [{\"type\": \"string\"}, {\"type\": \"number\"}]}, {\"type\": \"string\"}]}}}"
        );
    }
}
