// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Moonshot;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.NormalizeJsonSchemaForMFJS;

/// <summary>Port of <c>normalize-json-schema-for-mfjs.test.ts</c> &gt; <c>identity (no normalization needed)</c>.</summary>
public sealed class IdentityNoNormalizationNeededTests
{
    private const string Prefix =
        "packages/moonshotai/src/normalize-json-schema-for-mfjs.test.ts::identity (no normalization needed)::";

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
        Prefix + "passes a plain object schema through unchanged",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Passes_a_plain_object_schema_through_unchanged()
    {
        Identity(
            "{\"type\": \"object\", \"properties\": {\"city\": {\"type\": \"string\", \"description\": \"The city\"}, \"zip\": {\"type\": \"string\", \"pattern\": \"^[0-9]{5}$\"}}, \"required\": [\"city\"], \"additionalProperties\": false}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves keywords Moonshot accepts verbatim",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Preserves_keywords_Moonshot_accepts_verbatim()
    {
        Identity(
            "{\"$schema\": \"http://json-schema.org/draft-07/schema#\", \"type\": \"object\", \"properties\": {\"a\": {\"type\": \"string\", \"enum\": [\"x\", \"y\"], \"format\": \"uri\"}, \"b\": {\"type\": [\"string\", \"null\"]}, \"c\": {\"type\": \"array\", \"contains\": {\"type\": \"string\"}}, \"d\": {\"$ref\": \"#/$defs/thing\"}}, \"$defs\": {\"thing\": {\"type\": \"number\"}}, \"not\": {\"type\": \"string\"}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "passes anyOf without a sibling type through unchanged",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Passes_anyOf_without_a_sibling_type_through_unchanged()
    {
        Identity(
            "{\"type\": \"object\", \"properties\": {\"location\": {\"anyOf\": [{\"type\": \"string\"}, {\"type\": \"number\"}]}}, \"required\": [\"location\"]}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "leaves oneOf next to type alone (accepted by MFJS)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Leaves_oneOf_next_to_type_alone_accepted_by_MFJS()
    {
        Identity(
            "{\"type\": \"object\", \"oneOf\": [{\"properties\": {\"city\": {\"type\": \"string\"}}, \"required\": [\"city\"]}, {\"properties\": {\"lat\": {\"type\": \"number\"}}, \"required\": [\"lat\"]}]}"
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "leaves single-schema items alone", Coverage = UpstreamCoverage.Covered)]
    public void Leaves_single_schema_items_alone()
    {
        Identity(
            "{\"type\": \"object\", \"properties\": {\"a\": {\"type\": \"array\", \"items\": {\"type\": \"number\"}, \"minItems\": 2, \"maxItems\": 2}}}"
        );
    }
}
