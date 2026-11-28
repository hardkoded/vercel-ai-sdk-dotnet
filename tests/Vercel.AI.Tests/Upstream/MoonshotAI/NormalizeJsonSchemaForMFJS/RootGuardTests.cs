// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Moonshot;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.NormalizeJsonSchemaForMFJS;

/// <summary>Port of <c>normalize-json-schema-for-mfjs.test.ts</c> &gt; <c>root guard</c>.</summary>
public sealed class RootGuardTests
{
    private const string Prefix =
        "packages/moonshotai/src/normalize-json-schema-for-mfjs.test.ts::root guard::";

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
        Prefix + "throws for a non-object root type",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Throws_for_a_non_object_root_type()
    {
        Throws("{\"type\": \"array\", \"items\": {\"type\": \"number\"}}");
    }

    [Fact]
    [UpstreamTest(Prefix + "throws for a missing root type", Coverage = UpstreamCoverage.Covered)]
    public void Throws_for_a_missing_root_type()
    {
        Throws("{\"properties\": {\"a\": {\"type\": \"string\"}}}");
    }

    [Fact]
    [UpstreamTest(Prefix + "throws for a boolean root schema", Coverage = UpstreamCoverage.Covered)]
    public void Throws_for_a_boolean_root_schema()
    {
        Throws("true");
    }

    [Fact]
    [UpstreamTest(Prefix + "produces a clear error message", Coverage = UpstreamCoverage.Covered)]
    public void Produces_a_clear_error_message()
    {
        var error = Assert.Throws<UnsupportedFunctionalityException>(() =>
            MoonshotJsonSchema.NormalizeJsonSchemaForMFJS(Json("{\"type\":\"array\"}"))
        );
        Assert.Contains(
            "tool and response schemas must be a JSON Schema object with type \"object\" for moonshotai (MFJS)",
            error.Message
        );
    }
}
