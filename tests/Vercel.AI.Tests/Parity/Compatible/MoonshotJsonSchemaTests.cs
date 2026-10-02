// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Moonshot;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class MoonshotJsonSchemaTests
{
    [Fact]
    [UpstreamTest("packages/moonshotai/src/normalize-json-schema-for-mfjs.test.ts::identity (no normalization needed)::passes a plain object schema through unchanged", Coverage = UpstreamCoverage.Covered)]
    public void Passes_a_plain_object_schema_through()
    {
        Equal("{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\",\"description\":\"The city\"},\"zip\":{\"type\":\"string\",\"pattern\":\"^[0-9]{5}$\"}},\"required\":[\"city\"],\"additionalProperties\":false}");
    }

    [Fact]
    [UpstreamTest("packages/moonshotai/src/normalize-json-schema-for-mfjs.test.ts::identity (no normalization needed)::preserves keywords Moonshot accepts verbatim", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_accepted_keywords()
    {
        Equal("{\"$schema\":\"http://json-schema.org/draft-07/schema#\",\"type\":\"object\",\"properties\":{\"a\":{\"type\":\"string\",\"enum\":[\"x\",\"y\"],\"format\":\"uri\"},\"b\":{\"type\":[\"string\",\"null\"]},\"c\":{\"type\":\"array\",\"contains\":{\"type\":\"string\"}},\"d\":{\"$ref\":\"#/$defs/thing\"}},\"$defs\":{\"thing\":{\"type\":\"number\"}},\"not\":{\"type\":\"string\"}}");
    }

    [Fact]
    [UpstreamTest("packages/moonshotai/src/normalize-json-schema-for-mfjs.test.ts::identity (no normalization needed)::passes anyOf without a sibling type through unchanged", Coverage = UpstreamCoverage.Covered)]
    public void Passes_any_of_without_a_sibling_type()
    {
        Equal("{\"type\":\"object\",\"properties\":{\"location\":{\"anyOf\":[{\"type\":\"string\"},{\"type\":\"number\"}]}},\"required\":[\"location\"]}");
    }

    [Fact]
    [UpstreamTest("packages/moonshotai/src/normalize-json-schema-for-mfjs.test.ts::identity (no normalization needed)::leaves oneOf next to type alone (accepted by MFJS)", Coverage = UpstreamCoverage.Covered)]
    public void Leaves_one_of_next_to_type()
    {
        Equal("{\"type\":\"object\",\"oneOf\":[{\"properties\":{\"city\":{\"type\":\"string\"}},\"required\":[\"city\"]},{\"properties\":{\"lat\":{\"type\":\"number\"}},\"required\":[\"lat\"]}]}");
    }

    [Fact]
    [UpstreamTest("packages/moonshotai/src/normalize-json-schema-for-mfjs.test.ts::identity (no normalization needed)::leaves single-schema items alone", Coverage = UpstreamCoverage.Covered)]
    public void Leaves_single_schema_items()
    {
        Equal("{\"type\":\"object\",\"properties\":{\"a\":{\"type\":\"array\",\"items\":{\"type\":\"number\"},\"minItems\":2,\"maxItems\":2}}}");
    }

    [Fact]
    [UpstreamTest("packages/moonshotai/src/normalize-json-schema-for-mfjs.test.ts::tuple items -> prefixItems::rewrites a tuple items array to prefixItems and drops items", Coverage = UpstreamCoverage.Covered)]
    public void Rewrites_tuple_items()
    {
        var result = MoonshotJsonSchema.Normalize(Parse("{\"type\":\"object\",\"properties\":{\"a\":{\"type\":\"array\",\"items\":[{\"type\":\"number\"},{\"type\":\"number\"}]}},\"required\":[\"a\"]}"));
        AssertSame("{\"type\":\"object\",\"properties\":{\"a\":{\"type\":\"array\",\"prefixItems\":[{\"type\":\"number\"},{\"type\":\"number\"}]}},\"required\":[\"a\"]}", result);
        Assert.Null(result["properties"]!["a"]!["items"]);
    }

    [Fact]
    [UpstreamTest("packages/moonshotai/src/normalize-json-schema-for-mfjs.test.ts::tuple items -> prefixItems::preserves minItems and maxItems when rewriting tuples", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_tuple_bounds()
    {
        var result = MoonshotJsonSchema.Normalize(Parse("{\"type\":\"object\",\"properties\":{\"a\":{\"type\":\"array\",\"items\":[{\"type\":\"string\"},{\"type\":\"number\"}],\"minItems\":2,\"maxItems\":2}}}"));
        AssertSame("{\"type\":\"object\",\"properties\":{\"a\":{\"type\":\"array\",\"prefixItems\":[{\"type\":\"string\"},{\"type\":\"number\"}],\"minItems\":2,\"maxItems\":2}}}", result);
    }

    [Fact]
    [UpstreamTest("packages/moonshotai/src/normalize-json-schema-for-mfjs.test.ts::tuple items -> prefixItems::appends tuple items after existing prefixItems", Coverage = UpstreamCoverage.Covered)]
    public void Appends_tuple_items_after_prefix_items()
    {
        var result = MoonshotJsonSchema.Normalize(Parse("{\"type\":\"object\",\"properties\":{\"a\":{\"type\":\"array\",\"prefixItems\":[{\"type\":\"string\"}],\"items\":[{\"type\":\"number\"}]}}}"));
        AssertSame("{\"type\":\"object\",\"properties\":{\"a\":{\"type\":\"array\",\"prefixItems\":[{\"type\":\"string\"},{\"type\":\"number\"}]}}}", result);
    }

    [Fact]
    [UpstreamTest("packages/moonshotai/src/normalize-json-schema-for-mfjs.test.ts::tuple items -> prefixItems::rewrites tuples nested in anyOf branches", Coverage = UpstreamCoverage.Covered)]
    public void Rewrites_tuples_inside_any_of()
    {
        var result = MoonshotJsonSchema.Normalize(Parse("{\"type\":\"object\",\"properties\":{\"a\":{\"anyOf\":[{\"type\":\"array\",\"items\":[{\"type\":\"string\"},{\"type\":\"number\"}]},{\"type\":\"string\"}]}}}"));
        AssertSame("{\"type\":\"object\",\"properties\":{\"a\":{\"anyOf\":[{\"type\":\"array\",\"prefixItems\":[{\"type\":\"string\"},{\"type\":\"number\"}]},{\"type\":\"string\"}]}}}", result);
    }

    [Fact]
    [UpstreamTest("packages/moonshotai/src/normalize-json-schema-for-mfjs.test.ts::type + anyOf on the same node::moves type into anyOf branches that lack one", Coverage = UpstreamCoverage.Covered)]
    public void Moves_type_into_any_of_branches()
    {
        var result = MoonshotJsonSchema.Normalize(Parse("{\"type\":\"object\",\"anyOf\":[{\"properties\":{\"city\":{\"type\":\"string\"}},\"required\":[\"city\"]},{\"properties\":{\"lat\":{\"type\":\"number\"},\"lon\":{\"type\":\"number\"}},\"required\":[\"lat\",\"lon\"]}]}"));
        Assert.Null(result["type"]);
        Assert.Equal("object", result["anyOf"]![0]!["type"]!.GetValue<string>());
        Assert.Equal("object", result["anyOf"]![1]!["type"]!.GetValue<string>());
        Assert.Equal("string", result["anyOf"]![0]!["properties"]!["city"]!["type"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/moonshotai/src/normalize-json-schema-for-mfjs.test.ts::type + anyOf on the same node::keeps branch types when already present", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_existing_branch_types()
    {
        var result = MoonshotJsonSchema.Normalize(Parse("{\"type\":\"object\",\"anyOf\":[{\"type\":\"string\"},{\"properties\":{\"lat\":{\"type\":\"number\"}}}]}"));
        Assert.Null(result["type"]);
        Assert.Equal("string", result["anyOf"]![0]!["type"]!.GetValue<string>());
        Assert.Equal("object", result["anyOf"]![1]!["type"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/moonshotai/src/normalize-json-schema-for-mfjs.test.ts::type + anyOf on the same node::splits type at nested levels, not just the root", Coverage = UpstreamCoverage.Covered)]
    public void Splits_nested_type_and_any_of()
    {
        var result = MoonshotJsonSchema.Normalize(Parse("{\"type\":\"object\",\"properties\":{\"a\":{\"type\":\"object\",\"anyOf\":[{\"properties\":{\"city\":{\"type\":\"string\"}},\"required\":[\"city\"]},{\"properties\":{\"lat\":{\"type\":\"number\"}},\"required\":[\"lat\"]}]}},\"required\":[\"a\"]}"));
        Assert.Equal("object", result["type"]!.GetValue<string>());
        Assert.Null(result["properties"]!["a"]!["type"]);
        Assert.Equal("object", result["properties"]!["a"]!["anyOf"]![0]!["type"]!.GetValue<string>());
        Assert.Equal("object", result["properties"]!["a"]!["anyOf"]![1]!["type"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/moonshotai/src/normalize-json-schema-for-mfjs.test.ts::root guard::throws for a non-object root type", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_a_non_object_root()
    {
        Assert.Throws<UnsupportedFunctionalityException>(() => MoonshotJsonSchema.Normalize(Parse("{\"type\":\"array\",\"items\":{\"type\":\"number\"}}")));
    }

    [Fact]
    [UpstreamTest("packages/moonshotai/src/normalize-json-schema-for-mfjs.test.ts::root guard::throws for a missing root type", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_a_missing_root_type()
    {
        Assert.Throws<UnsupportedFunctionalityException>(() => MoonshotJsonSchema.Normalize(Parse("{\"properties\":{\"a\":{\"type\":\"string\"}}}")));
    }

    [Fact]
    [UpstreamTest("packages/moonshotai/src/normalize-json-schema-for-mfjs.test.ts::root guard::throws for a boolean root schema", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_a_boolean_root()
    {
        Assert.Throws<UnsupportedFunctionalityException>(() => MoonshotJsonSchema.Normalize(JsonValue.Create(true)!));
    }

    [Fact]
    [UpstreamTest("packages/moonshotai/src/normalize-json-schema-for-mfjs.test.ts::root guard::produces a clear error message", Coverage = UpstreamCoverage.Covered)]
    public void Explains_an_invalid_root()
    {
        var error = Assert.Throws<UnsupportedFunctionalityException>(() => MoonshotJsonSchema.Normalize(Parse("{\"type\":\"array\"}")));
        Assert.Equal(MoonshotJsonSchema.RootFunctionality, error.Functionality);
        Assert.Contains(MoonshotJsonSchema.RootFunctionality, error.Message, StringComparison.Ordinal);
    }

    private static void Equal(string json)
    {
        var input = Parse(json);
        AssertSame(json, MoonshotJsonSchema.Normalize(input));
    }

    private static void AssertSame(string expected, JsonNode actual)
    {
        Assert.True(JsonNode.DeepEquals(Parse(expected), actual));
    }

    private static JsonNode Parse(string json)
    {
        return JsonNode.Parse(json)!;
    }
}
