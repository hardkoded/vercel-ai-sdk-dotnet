// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.Upstream.ProviderUtils;

public sealed class JsonParseTests
{
    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/parse-json.test.ts::parseJSON::should parse basic JSON without schema",
        Coverage = UpstreamCoverage.Covered)]
    public void Parse_reads_json_without_a_schema()
    {
        JsonAssert.Equal(JsonParsing.Parse("{\"foo\": \"bar\"}"), "{\"foo\":\"bar\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/parse-json.test.ts::parseJSON::should parse JSON with schema validation",
        Coverage = UpstreamCoverage.Covered)]
    public void Parse_checks_a_string_property()
    {
        var schema = JsonSchemas.Object(new[] { Pair("foo", JsonSchemas.String()) }, new[] { "foo" });
        JsonAssert.Equal(JsonParsing.Parse("{\"foo\": \"bar\"}", schema), "{\"foo\":\"bar\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/parse-json.test.ts::parseJSON::should throw JSONParseError for invalid JSON",
        Coverage = UpstreamCoverage.Covered)]
    public void Parse_throws_when_the_text_is_not_json()
    {
        var error = Assert.Throws<JsonParseException>(() => JsonParsing.Parse("invalid json"));
        Assert.Equal("invalid json", error.Text);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/parse-json.test.ts::parseJSON::should throw TypeValidationError for schema validation failures",
        Coverage = UpstreamCoverage.Covered)]
    public void Parse_throws_when_the_schema_rejects_the_value()
    {
        var schema = JsonSchemas.Object(new[] { Pair("foo", JsonSchemas.Number()) }, new[] { "foo" });
        Assert.Throws<TypeValidationException>(() => JsonParsing.Parse("{\"foo\": \"bar\"}", schema));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/parse-json.test.ts::safeParseJSON::should safely parse basic JSON without schema and include rawValue",
        Coverage = UpstreamCoverage.Covered)]
    public void Safe_parse_returns_the_value_and_the_raw_value()
    {
        var result = JsonParsing.SafeParse("{\"foo\": \"bar\"}");
        Assert.True(result.Success);
        JsonAssert.Equal(result.Value!.Value, "{\"foo\":\"bar\"}");
        JsonAssert.Equal(result.RawValue!.Value, "{\"foo\":\"bar\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/parse-json.test.ts::safeParseJSON::should handle failed parsing with error details",
        Coverage = UpstreamCoverage.Covered)]
    public void Safe_parse_reports_invalid_json()
    {
        var result = JsonParsing.SafeParse("invalid json");
        Assert.False(result.Success);
        Assert.IsType<JsonParseException>(result.Error);
        Assert.Null(result.RawValue);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/parse-json.test.ts::safeParseJSON::should handle schema validation failures",
        Coverage = UpstreamCoverage.Covered)]
    public void Safe_parse_keeps_the_raw_value_when_validation_fails()
    {
        var schema = JsonSchemas.Object(new[] { Pair("age", JsonSchemas.Number()) }, new[] { "age" });
        var result = JsonParsing.SafeParse("{\"age\": \"twenty\"}", schema);
        Assert.False(result.Success);
        Assert.IsType<TypeValidationException>(result.Error);
        JsonAssert.Equal(result.RawValue!.Value, "{\"age\":\"twenty\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/parse-json.test.ts::safeParseJSON::should handle discriminated unions in schema",
        Coverage = UpstreamCoverage.Covered)]
    public void Safe_parse_accepts_a_matching_object_branch()
    {
        var schema = JsonSchemas.AnyOf(
            JsonSchemas.Object(
                new[] { Pair("type", JsonSchemas.Literal("string", JsonValue.Create("text")!)), Pair("content", JsonSchemas.String()) },
                new[] { "type", "content" },
                additionalPropertiesFlag: false),
            JsonSchemas.Object(
                new[] { Pair("type", JsonSchemas.Literal("string", JsonValue.Create("number")!)), Pair("value", JsonSchemas.Number()) },
                new[] { "type", "value" },
                additionalPropertiesFlag: false));
        var result = JsonParsing.SafeParse("{\"type\": \"text\", \"content\": \"hello\"}", schema);
        Assert.True(result.Success);
        JsonAssert.Equal(result.Value!.Value, "{\"type\":\"text\",\"content\":\"hello\"}");
        JsonAssert.Equal(result.RawValue!.Value, "{\"type\":\"text\",\"content\":\"hello\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/parse-json.test.ts::safeParseJSON::should handle nullable fields in schema",
        Coverage = UpstreamCoverage.Covered)]
    public void Safe_parse_accepts_a_null_string_field()
    {
        var schema = JsonSchemas.Object(
            new[] { Pair("id", JsonSchemas.PrimitiveUnion("string", "null")), Pair("data", JsonSchemas.String()) },
            new[] { "data" });
        var result = JsonParsing.SafeParse("{\"id\": null, \"data\": \"test\"}", schema);
        Assert.True(result.Success);
        JsonAssert.Equal(result.Value!.Value, "{\"id\":null,\"data\":\"test\"}");
        JsonAssert.Equal(result.RawValue!.Value, "{\"id\":null,\"data\":\"test\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/parse-json.test.ts::safeParseJSON::should handle union types in schema",
        Coverage = UpstreamCoverage.Covered)]
    public void Safe_parse_accepts_a_string_or_number()
    {
        var schema = JsonSchemas.Object(new[] { Pair("value", JsonSchemas.PrimitiveUnion("string", "number")) }, new[] { "value" });
        var text = JsonParsing.SafeParse("{\"value\": \"test\"}", schema);
        var number = JsonParsing.SafeParse("{\"value\": 123}", schema);
        Assert.True(text.Success);
        Assert.True(number.Success);
        JsonAssert.Equal(text.Value!.Value, "{\"value\":\"test\"}");
        JsonAssert.Equal(number.Value!.Value, "{\"value\":123}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/parse-json.test.ts::isParsableJson::should return true for valid JSON",
        Coverage = UpstreamCoverage.Covered)]
    public void Parsable_json_accepts_objects_arrays_and_strings()
    {
        Assert.True(JsonParsing.IsParsable("{\"foo\": \"bar\"}"));
        Assert.True(JsonParsing.IsParsable("[1, 2, 3]"));
        Assert.True(JsonParsing.IsParsable("\"hello\""));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/parse-json.test.ts::isParsableJson::should return false for invalid JSON",
        Coverage = UpstreamCoverage.Covered)]
    public void Parsable_json_rejects_broken_text()
    {
        Assert.False(JsonParsing.IsParsable("invalid"));
        Assert.False(JsonParsing.IsParsable("{foo: \"bar\"}"));
        Assert.False(JsonParsing.IsParsable("{\"foo\": }"));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/secure-json-parse.test.ts::secureJsonParse::parses object string",
        Coverage = UpstreamCoverage.Covered)]
    public void Secure_parse_reads_an_object()
    {
        JsonAssert.Equal(SecureJson.Parse("{\"a\": 5, \"b\": 6}"), "{\"a\":5,\"b\":6}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/secure-json-parse.test.ts::secureJsonParse::parses null string",
        Coverage = UpstreamCoverage.Covered)]
    public void Secure_parse_reads_null()
    {
        Assert.Equal(JsonValueKind.Null, SecureJson.Parse("null").ValueKind);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/secure-json-parse.test.ts::secureJsonParse::parses 0 string",
        Coverage = UpstreamCoverage.Covered)]
    public void Secure_parse_reads_zero()
    {
        Assert.Equal(0, SecureJson.Parse("0").GetInt32());
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/secure-json-parse.test.ts::secureJsonParse::parses string string",
        Coverage = UpstreamCoverage.Covered)]
    public void Secure_parse_reads_a_string()
    {
        Assert.Equal("X", SecureJson.Parse("\"X\"").GetString());
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/secure-json-parse.test.ts::secureJsonParse::allows constructor property with non-object value",
        Coverage = UpstreamCoverage.Covered)]
    public void Secure_parse_allows_a_string_named_constructor()
    {
        JsonAssert.Equal(SecureJson.Parse("{ \"constructor\": \"string value\" }"), "{\"constructor\":\"string value\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/secure-json-parse.test.ts::secureJsonParse::allows constructor property with null value",
        Coverage = UpstreamCoverage.Covered)]
    public void Secure_parse_allows_a_null_constructor()
    {
        JsonAssert.Equal(SecureJson.Parse("{ \"constructor\": null }"), "{\"constructor\":null}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/secure-json-parse.test.ts::secureJsonParse::errors on constructor property",
        Coverage = UpstreamCoverage.Covered)]
    public void Secure_parse_rejects_a_constructor_payload_that_carries_proto()
    {
        var text = "{ \"a\": 5, \"b\": 6, \"constructor\": { \"x\": 7 }, \"c\": { \"d\": 0, \"e\": \"text\", \"__proto__\": { \"y\": 8 }, \"f\": { \"g\": 2 } } }";
        var error = Assert.Throws<JsonException>(() => SecureJson.Parse(text));
        Assert.Contains("Object contains forbidden prototype property", error.Message);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/secure-json-parse.test.ts::secureJsonParse::errors on proto property",
        Coverage = UpstreamCoverage.Covered)]
    public void Secure_parse_rejects_proto()
    {
        var text = "{ \"a\": 5, \"b\": 6, \"__proto__\": { \"x\": 7 }, \"c\": { \"d\": 0, \"e\": \"text\", \"__proto__\": { \"y\": 8 }, \"f\": { \"g\": 2 } } }";
        var error = Assert.Throws<JsonException>(() => SecureJson.Parse(text));
        Assert.Contains("Object contains forbidden prototype property", error.Message);
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/secure-json-parse.test.ts::secureJsonParse::errors on unicode-escaped __proto__ property",
        Coverage = UpstreamCoverage.Covered)]
    public void Secure_parse_rejects_a_partially_escaped_proto_key()
    {
        var text = "{ \"\\u005f\\u005fproto__\": { \"isAdmin\": true } }";
        Assert.Throws<JsonException>(() => SecureJson.Parse(text));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/secure-json-parse.test.ts::secureJsonParse::errors on fully unicode-escaped __proto__ property",
        Coverage = UpstreamCoverage.Covered)]
    public void Secure_parse_rejects_a_fully_escaped_proto_key()
    {
        var text = "{ \"\\u005f\\u005f\\u0070\\u0072\\u006f\\u0074\\u006f\\u005f\\u005f\": { \"isAdmin\": true } }";
        Assert.Throws<JsonException>(() => SecureJson.Parse(text));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/secure-json-parse.test.ts::secureJsonParse::errors on unicode-escaped constructor property",
        Coverage = UpstreamCoverage.Covered)]
    public void Secure_parse_rejects_a_partially_escaped_constructor_key()
    {
        var text = "{ \"\\u0063\\u006fnstructor\": { \"prototype\": { \"isAdmin\": true } } }";
        Assert.Throws<JsonException>(() => SecureJson.Parse(text));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/secure-json-parse.test.ts::secureJsonParse::errors on fully unicode-escaped constructor property",
        Coverage = UpstreamCoverage.Covered)]
    public void Secure_parse_rejects_a_fully_escaped_constructor_key()
    {
        var text = "{ \"\\u0063\\u006f\\u006e\\u0073\\u0074\\u0072\\u0075\\u0063\\u0074\\u006f\\u0072\": { \"prototype\": { \"isAdmin\": true } } }";
        Assert.Throws<JsonException>(() => SecureJson.Parse(text));
    }

    private static KeyValuePair<string, JsonNode> Pair(string name, JsonNode schema)
    {
        return new KeyValuePair<string, JsonNode>(name, schema);
    }
}
