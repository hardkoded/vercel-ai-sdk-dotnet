// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.OpenAI.NormalizeOpenAIJsonSchema;

/// <summary>Port of <c>normalize-openai-json-schema.test.ts</c> &gt; <c>normalizeOpenAIJsonSchema</c>.</summary>
public sealed class NormalizeOpenAIJsonSchemaTests
{
    [Fact]
    [UpstreamTest(
        "packages/openai/src/normalize-openai-json-schema.test.ts::normalizeOpenAIJsonSchema::removes string propertyNames recursively and warns",
        Coverage = UpstreamCoverage.Covered
    )]
    public void RemovesStringPropertyNames()
    {
        var original = JsonNode.Parse(
            "{\"type\":\"object\",\"properties\":{\"variables\":{\"type\":\"object\",\"propertyNames\":{\"type\":\"string\",\"format\":\"uuid\"},\"additionalProperties\":{\"type\":\"object\",\"propertyNames\":{\"type\":\"string\",\"pattern\":\"^[A-Z_]+$\"}}}},\"definitions\":{\"variable\":{\"type\":\"object\",\"propertyNames\":{\"type\":\"string\"}}},\"$defs\":{\"conditional\":{\"if\":{\"type\":\"object\",\"propertyNames\":{\"type\":\"string\"}}}}}"
        )!;
        var result = OpenAIJsonSchema.Normalize(original);
        OpenAIUpstream.Equal(
            result.Schema,
            "{\"type\":\"object\",\"properties\":{\"variables\":{\"type\":\"object\",\"additionalProperties\":{\"type\":\"object\"}}},\"definitions\":{\"variable\":{\"type\":\"object\"}},\"$defs\":{\"conditional\":{\"if\":{\"type\":\"object\"}}}}"
        );
        Assert.Equal("compatibility", result.Warnings[0].Type);
        Assert.Equal("JSON Schema propertyNames", result.Warnings[0].Feature);
        Assert.Equal(OpenAIJsonSchema.PropertyNamesDetails, result.Warnings[0].Details);
        Assert.Single(result.Warnings);
        Assert.NotNull(original["properties"]!["variables"]!["propertyNames"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/normalize-openai-json-schema.test.ts::normalizeOpenAIJsonSchema::rejects non-string propertyNames schemas",
        Coverage = UpstreamCoverage.Covered
    )]
    public void RejectsNonStringPropertyNames()
    {
        var exception = Assert.Throws<AiSdkException>(() =>
            OpenAIJsonSchema.Normalize(
                JsonNode.Parse("{\"type\":\"object\",\"propertyNames\":{\"type\":\"number\"}}")!
            )
        );
        Assert.Equal(
            "JSON Schema propertyNames that does not use a string schema",
            exception.Message
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/normalize-openai-json-schema.test.ts::normalizeOpenAIJsonSchema::removes regex lookaround patterns recursively and warns",
        Coverage = UpstreamCoverage.Covered
    )]
    public void RemovesRegexLookaround()
    {
        var original = JsonNode.Parse(
            "{\"type\":\"object\",\"properties\":{\"email\":{\"type\":\"string\",\"format\":\"email\",\"pattern\":\"^(?!\\\\.)(?!.*\\\\.\\\\.).+@.+$\"},\"username\":{\"type\":\"string\",\"pattern\":\"^@[a-zA-Z0-9_]+$\"},\"contacts\":{\"type\":\"array\",\"items\":{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\",\"pattern\":\"(?<=prefix)value\"}}}}},\"$defs\":{\"value\":{\"type\":\"string\",\"pattern\":\"value(?=suffix)\"}}}"
        )!;
        var result = OpenAIJsonSchema.Normalize(original);
        OpenAIUpstream.Equal(
            result.Schema,
            "{\"type\":\"object\",\"properties\":{\"email\":{\"type\":\"string\",\"format\":\"email\"},\"username\":{\"type\":\"string\",\"pattern\":\"^@[a-zA-Z0-9_]+$\"},\"contacts\":{\"type\":\"array\",\"items\":{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}}}}},\"$defs\":{\"value\":{\"type\":\"string\"}}}"
        );
        Assert.Equal("JSON Schema pattern with regex lookaround", result.Warnings[0].Feature);
        Assert.Equal(OpenAIJsonSchema.LookaroundDetails, result.Warnings[0].Details);
        Assert.NotNull(original["properties"]!["email"]!["pattern"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/normalize-openai-json-schema.test.ts::normalizeOpenAIJsonSchema::preserves escaped and character-class lookaround-like text",
        Coverage = UpstreamCoverage.Covered
    )]
    public void PreservesEscapedLookaroundText()
    {
        const string schema =
            "{\"type\":\"object\",\"properties\":{\"escaped\":{\"type\":\"string\",\"pattern\":\"\\\\(\\\\?=literal\\\\)\"},\"characterClass\":{\"type\":\"string\",\"pattern\":\"[(?=!)]\"}}}";
        var result = OpenAIJsonSchema.Normalize(JsonNode.Parse(schema)!);
        OpenAIUpstream.Equal(result.Schema, schema);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/normalize-openai-json-schema.test.ts::normalizeOpenAIJsonSchema::unwraps singleton reference allOf schemas from recursive Zod schemas",
        Coverage = UpstreamCoverage.Covered
    )]
    public void UnwrapsSingletonReferenceAllOfSchemas()
    {
        const string definitions =
            "{\"person\":{\"type\":\"object\",\"properties\":{\"firstName\":{\"type\":\"string\"}},\"required\":[\"firstName\"],\"additionalProperties\":false}}";
        var result = OpenAIJsonSchema.Normalize(
            JsonNode.Parse(
                "{\"type\":\"object\",\"properties\":{\"relatives\":{\"type\":\"array\",\"items\":{\"allOf\":[{\"$ref\":\"#/definitions/person\"}]}}},\"definitions\":"
                    + definitions
                    + ",\"required\":[\"relatives\"],\"additionalProperties\":false}"
            )!
        );
        OpenAIUpstream.Equal(
            result.Schema,
            "{\"type\":\"object\",\"properties\":{\"relatives\":{\"type\":\"array\",\"items\":{\"$ref\":\"#/definitions/person\"}}},\"definitions\":"
                + definitions
                + ",\"required\":[\"relatives\"],\"additionalProperties\":false}"
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/normalize-openai-json-schema.test.ts::normalizeOpenAIJsonSchema::expands a singleton local reference allOf at the root",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ExpandsSingletonLocalReferenceAllOfAtTheRoot()
    {
        var result = OpenAIJsonSchema.Normalize(
            JsonNode.Parse(
                "{\"default\":{\"firstName\":\"John\"},\"allOf\":[{\"$ref\":\"#/definitions/person\"}],\"definitions\":{\"person\":{\"type\":\"object\",\"properties\":{\"firstName\":{\"type\":\"string\"},\"relatives\":{\"type\":\"array\",\"items\":{\"allOf\":[{\"$ref\":\"#/definitions/person\"}]}}},\"required\":[\"firstName\",\"relatives\"],\"additionalProperties\":false}}}"
            )!
        );
        const string person =
            "{\"type\":\"object\",\"properties\":{\"firstName\":{\"type\":\"string\"},\"relatives\":{\"type\":\"array\",\"items\":{\"$ref\":\"#/definitions/person\"}}},\"required\":[\"firstName\",\"relatives\"],\"additionalProperties\":false}";
        OpenAIUpstream.Equal(
            result.Schema,
            "{\"type\":\"object\",\"properties\":{\"firstName\":{\"type\":\"string\"},\"relatives\":{\"type\":\"array\",\"items\":{\"$ref\":\"#/definitions/person\"}}},\"required\":[\"firstName\",\"relatives\"],\"additionalProperties\":false,\"default\":{\"firstName\":\"John\"},\"definitions\":{\"person\":"
                + person
                + "}}"
        );
        Assert.Empty(result.Warnings);
    }
}
