// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests;

public sealed class JsonParityTests
{
    [Fact]
    [UpstreamTest("packages/provider-utils/src/secure-json-parse.test.ts::secureJsonParse::parses object string", Coverage = UpstreamCoverage.Covered)]
    public void Secure_json_parses_an_object()
    {
        AssertSame("{\"a\": 5, \"b\": 6}");
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/secure-json-parse.test.ts::secureJsonParse::parses null string", Coverage = UpstreamCoverage.Covered)]
    public void Secure_json_parses_null()
    {
        Assert.Null(SecureJson.SecureJsonParse("null"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/secure-json-parse.test.ts::secureJsonParse::parses 0 string", Coverage = UpstreamCoverage.Covered)]
    public void Secure_json_parses_zero()
    {
        Assert.Equal(0, SecureJson.SecureJsonParse("0")!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/secure-json-parse.test.ts::secureJsonParse::parses string string", Coverage = UpstreamCoverage.Covered)]
    public void Secure_json_parses_a_string()
    {
        Assert.Equal("X", SecureJson.SecureJsonParse("\"X\"")!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/secure-json-parse.test.ts::secureJsonParse::allows constructor property with non-object value", Coverage = UpstreamCoverage.Covered)]
    public void Secure_json_allows_a_string_constructor()
    {
        AssertSame("{ \"constructor\": \"string value\" }");
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/secure-json-parse.test.ts::secureJsonParse::allows constructor property with null value", Coverage = UpstreamCoverage.Covered)]
    public void Secure_json_allows_a_null_constructor()
    {
        AssertSame("{ \"constructor\": null }");
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/secure-json-parse.test.ts::secureJsonParse::errors on constructor property", Coverage = UpstreamCoverage.Covered)]
    public void Secure_json_rejects_a_constructor_object()
    {
        Assert.Throws<JsonSyntaxException>(() => SecureJson.SecureJsonParse("{ \"a\": 5, \"b\": 6, \"constructor\": { \"x\": 7 }, \"c\": { \"d\": 0, \"e\": \"text\", \"__proto__\": { \"y\": 8 }, \"f\": { \"g\": 2 } } }"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/secure-json-parse.test.ts::secureJsonParse::errors on proto property", Coverage = UpstreamCoverage.Covered)]
    public void Secure_json_rejects_proto()
    {
        Assert.Throws<JsonSyntaxException>(() => SecureJson.SecureJsonParse("{ \"a\": 5, \"b\": 6, \"__proto__\": { \"x\": 7 }, \"c\": { \"d\": 0, \"e\": \"text\", \"__proto__\": { \"y\": 8 }, \"f\": { \"g\": 2 } } }"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/secure-json-parse.test.ts::secureJsonParse::errors on unicode-escaped __proto__ property", Coverage = UpstreamCoverage.Covered)]
    public void Secure_json_rejects_escaped_proto()
    {
        Assert.Throws<JsonSyntaxException>(() => SecureJson.SecureJsonParse("{ \"\\u005f\\u005fproto__\": { \"isAdmin\": true } }"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/secure-json-parse.test.ts::secureJsonParse::errors on fully unicode-escaped __proto__ property", Coverage = UpstreamCoverage.Covered)]
    public void Secure_json_rejects_fully_escaped_proto()
    {
        Assert.Throws<JsonSyntaxException>(() => SecureJson.SecureJsonParse("{ \"\\u005f\\u005f\\u0070\\u0072\\u006f\\u0074\\u006f\\u005f\\u005f\": { \"isAdmin\": true } }"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/secure-json-parse.test.ts::secureJsonParse::errors on unicode-escaped constructor property", Coverage = UpstreamCoverage.Covered)]
    public void Secure_json_rejects_escaped_constructor()
    {
        Assert.Throws<JsonSyntaxException>(() => SecureJson.SecureJsonParse("{ \"\\u0063\\u006fnstructor\": { \"prototype\": { \"isAdmin\": true } } }"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/secure-json-parse.test.ts::secureJsonParse::errors on fully unicode-escaped constructor property", Coverage = UpstreamCoverage.Covered)]
    public void Secure_json_rejects_fully_escaped_constructor()
    {
        Assert.Throws<JsonSyntaxException>(() => SecureJson.SecureJsonParse("{ \"\\u0063\\u006f\\u006e\\u0073\\u0074\\u0072\\u0075\\u0063\\u0074\\u006f\\u0072\": { \"prototype\": { \"isAdmin\": true } } }"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/parse-json.test.ts::parseJSON::should parse basic JSON without schema", Coverage = UpstreamCoverage.Covered)]
    public void Parse_json_reads_an_object()
    {
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"foo\":\"bar\"}"), JsonParsing.ParseJson("{\"foo\": \"bar\"}")));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/parse-json.test.ts::parseJSON::should parse JSON with schema validation", Coverage = UpstreamCoverage.Covered)]
    public void Parse_json_validates_a_string_field()
    {
        var value = JsonParsing.ParseJson("{\"foo\": \"bar\"}", StringField("foo"));
        Assert.Equal("bar", value!["foo"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/parse-json.test.ts::parseJSON::should throw JSONParseError for invalid JSON", Coverage = UpstreamCoverage.Covered)]
    public void Parse_json_wraps_invalid_text()
    {
        Assert.True(JSONParseError.IsInstance(Assert.Throws<JSONParseError>(() => JsonParsing.ParseJson("invalid json"))));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/parse-json.test.ts::parseJSON::should throw TypeValidationError for schema validation failures", Coverage = UpstreamCoverage.Covered)]
    public void Parse_json_throws_when_the_schema_rejects_the_value()
    {
        Assert.True(TypeValidationError.IsInstance(Assert.Throws<TypeValidationError>(() => JsonParsing.ParseJson("{\"foo\": \"bar\"}", NumberField("foo")))));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/parse-json.test.ts::safeParseJSON::should safely parse basic JSON without schema and include rawValue", Coverage = UpstreamCoverage.Covered)]
    public void Safe_parse_includes_the_raw_value()
    {
        var result = JsonParsing.SafeParseJson("{\"foo\": \"bar\"}");
        Assert.True(result.Success);
        Assert.True(result.HasRawValue);
        Assert.Equal("bar", result.Value!["foo"]!.GetValue<string>());
        Assert.Equal("bar", result.RawValue!["foo"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/parse-json.test.ts::safeParseJSON::should preserve rawValue even after schema transformation", Coverage = UpstreamCoverage.Covered)]
    public void Safe_parse_keeps_the_raw_value_after_coercion()
    {
        var schema = new FlexibleSchema(node =>
        {
            var count = node!["count"]!.GetValue<string>();
            return SchemaValidationResult.Ok(new JsonObject { ["count"] = int.Parse(count) });
        });
        var result = JsonParsing.SafeParseJson("{\"count\": \"42\"}", schema);
        Assert.True(result.Success);
        Assert.Equal(42, result.Value!["count"]!.GetValue<int>());
        Assert.Equal("42", result.RawValue!["count"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/parse-json.test.ts::safeParseJSON::should handle failed parsing with error details", Coverage = UpstreamCoverage.Covered)]
    public void Safe_parse_reports_invalid_json()
    {
        var result = JsonParsing.SafeParseJson("invalid json");
        Assert.False(result.Success);
        Assert.False(result.HasRawValue);
        Assert.True(JSONParseError.IsInstance(result.Error));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/parse-json.test.ts::safeParseJSON::should handle schema validation failures", Coverage = UpstreamCoverage.Covered)]
    public void Safe_parse_reports_schema_failures()
    {
        var result = JsonParsing.SafeParseJson("{\"age\": \"twenty\"}", NumberField("age"));
        Assert.False(result.Success);
        Assert.True(TypeValidationError.IsInstance(result.Error));
        Assert.Equal("twenty", result.RawValue!["age"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/parse-json.test.ts::safeParseJSON::should handle nested objects and preserve raw values", Coverage = UpstreamCoverage.Covered)]
    public void Safe_parse_transforms_a_nested_id()
    {
        var schema = new FlexibleSchema(node =>
        {
            var user = node!["user"]!.AsObject();
            return SchemaValidationResult.Ok(new JsonObject
            {
                ["user"] = new JsonObject
                {
                    ["id"] = int.Parse(user["id"]!.GetValue<string>()),
                    ["name"] = user["name"]!.GetValue<string>(),
                },
            });
        });
        var result = JsonParsing.SafeParseJson("{\"user\": {\"id\": \"123\", \"name\": \"John\"}}", schema);
        Assert.Equal(123, result.Value!["user"]!["id"]!.GetValue<int>());
        Assert.Equal("John", result.Value["user"]!["name"]!.GetValue<string>());
        Assert.Equal("123", result.RawValue!["user"]!["id"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/parse-json.test.ts::safeParseJSON::should handle arrays and preserve raw values", Coverage = UpstreamCoverage.Covered)]
    public void Safe_parse_transforms_an_array()
    {
        var schema = new FlexibleSchema(node =>
        {
            var upper = new JsonArray();
            foreach (var item in node!.AsArray())
            {
                upper.Add(item!.GetValue<string>().ToUpperInvariant());
            }

            return SchemaValidationResult.Ok(upper);
        });
        var result = JsonParsing.SafeParseJson("[\"hello\", \"world\"]", schema);
        Assert.Equal("HELLO", result.Value![0]!.GetValue<string>());
        Assert.Equal("WORLD", result.Value[1]!.GetValue<string>());
        Assert.Equal("hello", result.RawValue![0]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/parse-json.test.ts::safeParseJSON::should handle discriminated unions in schema", Coverage = UpstreamCoverage.Covered)]
    public void Safe_parse_accepts_a_text_union_member()
    {
        var schema = new FlexibleSchema(node =>
        {
            var type = node!["type"]!.GetValue<string>();
            if (type == "text" && node["content"] is JsonValue)
            {
                return SchemaValidationResult.Ok(node);
            }

            if (type == "number" && node["value"] is JsonValue value && value.TryGetValue<int>(out _))
            {
                return SchemaValidationResult.Ok(node);
            }

            return SchemaValidationResult.Fail(new Exception("Invalid input"));
        });
        var result = JsonParsing.SafeParseJson("{\"type\": \"text\", \"content\": \"hello\"}", schema);
        Assert.True(result.Success);
        Assert.Equal("hello", result.Value!["content"]!.GetValue<string>());
        Assert.Equal("hello", result.RawValue!["content"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/parse-json.test.ts::safeParseJSON::should handle nullable fields in schema", Coverage = UpstreamCoverage.Covered)]
    public void Safe_parse_accepts_a_null_id()
    {
        var schema = new FlexibleSchema(node =>
        {
            var data = node!["data"];
            var id = node["id"];
            if (data is JsonValue && (id is null || id is JsonValue))
            {
                return SchemaValidationResult.Ok(node);
            }

            return SchemaValidationResult.Fail(new Exception("Invalid input"));
        });
        var result = JsonParsing.SafeParseJson("{\"id\": null, \"data\": \"test\"}", schema);
        Assert.True(result.Success);
        Assert.Null(result.Value!["id"]);
        Assert.Equal("test", result.Value["data"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/parse-json.test.ts::safeParseJSON::should handle union types in schema", Coverage = UpstreamCoverage.Covered)]
    public void Safe_parse_accepts_string_or_number()
    {
        var schema = new FlexibleSchema(node =>
        {
            var value = node!["value"];
            if (value is JsonValue json && (json.TryGetValue<string>(out _) || json.TryGetValue<int>(out _)))
            {
                return SchemaValidationResult.Ok(node);
            }

            return SchemaValidationResult.Fail(new Exception("Invalid input"));
        });
        var text = JsonParsing.SafeParseJson("{\"value\": \"test\"}", schema);
        var number = JsonParsing.SafeParseJson("{\"value\": 123}", schema);
        Assert.Equal("test", text.Value!["value"]!.GetValue<string>());
        Assert.Equal(123, number.Value!["value"]!.GetValue<int>());
        Assert.Equal("test", text.RawValue!["value"]!.GetValue<string>());
        Assert.Equal(123, number.RawValue!["value"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/parse-json.test.ts::isParsableJson::should return true for valid JSON", Coverage = UpstreamCoverage.Covered)]
    public void Parsable_json_accepts_objects_arrays_and_strings()
    {
        Assert.True(JsonParsing.IsParsableJson("{\"foo\": \"bar\"}"));
        Assert.True(JsonParsing.IsParsableJson("[1, 2, 3]"));
        Assert.True(JsonParsing.IsParsableJson("\"hello\""));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/parse-json.test.ts::isParsableJson::should return false for invalid JSON", Coverage = UpstreamCoverage.Covered)]
    public void Parsable_json_rejects_invalid_text()
    {
        Assert.False(JsonParsing.IsParsableJson("invalid"));
        Assert.False(JsonParsing.IsParsableJson("{foo: \"bar\"}"));
        Assert.False(JsonParsing.IsParsableJson("{\"foo\": }"));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/validate-types.test.ts::validateTypes::should return validated object for valid input", Coverage = UpstreamCoverage.Covered)]
    public void Validate_types_returns_a_valid_object()
    {
        var input = JsonNode.Parse("{\"name\":\"John\",\"age\":30}");
        var value = TypeValidation.ValidateTypes(input, PersonSchema());
        Assert.Equal("John", value!["name"]!.GetValue<string>());
        Assert.Equal(30, value["age"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/validate-types.test.ts::validateTypes::should throw TypeValidationError for invalid input", Coverage = UpstreamCoverage.Covered)]
    public void Validate_types_throws_for_a_string_age()
    {
        var input = JsonNode.Parse("{\"name\":\"John\",\"age\":\"30\"}");
        var error = Assert.Throws<TypeValidationError>(() => TypeValidation.ValidateTypes(input, PersonSchema()));
        Assert.Equal("AI_TypeValidationError", error.Name);
        Assert.Same(input, error.Value);
        var cause = Assert.IsAssignableFrom<IReadOnlyList<object>>(error.Cause);
        Assert.IsAssignableFrom<Exception>(cause[0]);
        Assert.Contains("Type validation failed", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/validate-types.test.ts::safeValidateTypes::should return validated object for valid input", Coverage = UpstreamCoverage.Covered)]
    public void Safe_validate_returns_the_value()
    {
        var input = JsonNode.Parse("{\"name\":\"John\",\"age\":30}");
        var result = TypeValidation.SafeValidateTypes(input, PersonSchema());
        Assert.True(result.Success);
        Assert.Same(input, result.Value);
        Assert.Same(input, result.RawValue);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/validate-types.test.ts::safeValidateTypes::should return error object for invalid input", Coverage = UpstreamCoverage.Covered)]
    public void Safe_validate_returns_the_error()
    {
        var input = JsonNode.Parse("{\"name\":\"John\",\"age\":\"30\"}");
        var result = TypeValidation.SafeValidateTypes(input, PersonSchema());
        Assert.False(result.Success);
        Assert.True(TypeValidationError.IsInstance(result.Error));
        Assert.Same(input, result.RawValue);
        Assert.Same(input, ((TypeValidationError)result.Error!).Value);
        Assert.Contains("Type validation failed", result.Error.Message);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/add-additional-properties-to-json-schema.test.ts::addAdditionalPropertiesToJsonSchema::adds additionalProperties: false to objects recursively", Coverage = UpstreamCoverage.Covered)]
    public void Additional_properties_are_added_recursively()
    {
        AssertSchema(
            "{\"type\":\"object\",\"properties\":{\"user\":{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}}},\"age\":{\"type\":\"number\"}}}",
            "{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"user\":{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"name\":{\"type\":\"string\"}}},\"age\":{\"type\":\"number\"}}}");
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/add-additional-properties-to-json-schema.test.ts::addAdditionalPropertiesToJsonSchema::adds additionalProperties: false to objects inside arrays", Coverage = UpstreamCoverage.Covered)]
    public void Additional_properties_are_added_inside_arrays()
    {
        AssertSchema(
            "{\"type\":\"object\",\"properties\":{\"ingredients\":{\"type\":\"array\",\"items\":{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"},\"amount\":{\"type\":\"string\"}},\"required\":[\"name\",\"amount\"]}}},\"required\":[\"ingredients\"]}",
            "{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"ingredients\":{\"type\":\"array\",\"items\":{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"name\":{\"type\":\"string\"},\"amount\":{\"type\":\"string\"}},\"required\":[\"name\",\"amount\"]}}},\"required\":[\"ingredients\"]}");
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/add-additional-properties-to-json-schema.test.ts::addAdditionalPropertiesToJsonSchema::adds additionalProperties: false when type is a union that includes \"object\"", Coverage = UpstreamCoverage.Covered)]
    public void Additional_properties_are_added_for_a_type_union()
    {
        AssertSchema(
            "{\"type\":\"object\",\"properties\":{\"response\":{\"type\":[\"object\",\"null\"],\"properties\":{\"name\":{\"type\":\"string\"}}}}}",
            "{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"response\":{\"type\":[\"object\",\"null\"],\"additionalProperties\":false,\"properties\":{\"name\":{\"type\":\"string\"}}}}}");
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/add-additional-properties-to-json-schema.test.ts::addAdditionalPropertiesToJsonSchema::adds additionalProperties: false to objects inside anyOf", Coverage = UpstreamCoverage.Covered)]
    public void Additional_properties_are_added_inside_any_of()
    {
        AssertSchema(
            "{\"type\":\"object\",\"properties\":{\"response\":{\"anyOf\":[{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}}},{\"type\":\"object\",\"properties\":{\"amount\":{\"type\":\"string\"}}}]}}}",
            "{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"response\":{\"anyOf\":[{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"name\":{\"type\":\"string\"}}},{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"amount\":{\"type\":\"string\"}}}]}}}");
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/add-additional-properties-to-json-schema.test.ts::addAdditionalPropertiesToJsonSchema::adds additionalProperties: false to objects inside allOf", Coverage = UpstreamCoverage.Covered)]
    public void Additional_properties_are_added_inside_all_of()
    {
        AssertSchema(
            "{\"type\":\"object\",\"properties\":{\"response\":{\"allOf\":[{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}}},{\"type\":\"object\",\"properties\":{\"age\":{\"type\":\"number\"}}}]}}}",
            "{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"response\":{\"allOf\":[{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"name\":{\"type\":\"string\"}}},{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"age\":{\"type\":\"number\"}}}]}}}");
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/add-additional-properties-to-json-schema.test.ts::addAdditionalPropertiesToJsonSchema::adds additionalProperties: false to objects inside oneOf", Coverage = UpstreamCoverage.Covered)]
    public void Additional_properties_are_added_inside_one_of()
    {
        AssertSchema(
            "{\"type\":\"object\",\"properties\":{\"response\":{\"oneOf\":[{\"type\":\"object\",\"properties\":{\"success\":{\"type\":\"boolean\"}}},{\"type\":\"object\",\"properties\":{\"error\":{\"type\":\"string\"}}}]}}}",
            "{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"response\":{\"oneOf\":[{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"success\":{\"type\":\"boolean\"}}},{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"error\":{\"type\":\"string\"}}}]}}}");
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/add-additional-properties-to-json-schema.test.ts::addAdditionalPropertiesToJsonSchema::adds additionalProperties: false to object schemas inside definitions (refs)", Coverage = UpstreamCoverage.Covered)]
    public void Additional_properties_are_added_inside_definitions()
    {
        AssertSchema(
            "{\"type\":\"object\",\"properties\":{\"node\":{\"$ref\":\"#/definitions/Node\"}},\"definitions\":{\"Node\":{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"},\"next\":{\"$ref\":\"#/definitions/Node\"}}}}}",
            "{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"node\":{\"$ref\":\"#/definitions/Node\"}},\"definitions\":{\"Node\":{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"value\":{\"type\":\"string\"},\"next\":{\"$ref\":\"#/definitions/Node\"}}}}}");
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/add-additional-properties-to-json-schema.test.ts::addAdditionalPropertiesToJsonSchema::overwrites existing additionalProperties flags", Coverage = UpstreamCoverage.Covered)]
    public void Additional_properties_overwrite_true()
    {
        AssertSchema(
            "{\"type\":\"object\",\"additionalProperties\":true,\"properties\":{\"meta\":{\"type\":\"object\",\"additionalProperties\":true,\"properties\":{\"id\":{\"type\":\"string\"}}}}}",
            "{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"meta\":{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"id\":{\"type\":\"string\"}}}}}");
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/add-additional-properties-to-json-schema.test.ts::addAdditionalPropertiesToJsonSchema::preserves schema-valued additionalProperties recursively", Coverage = UpstreamCoverage.Covered)]
    public void Additional_properties_visit_schema_values()
    {
        AssertSchema(
            "{\"type\":\"object\",\"additionalProperties\":{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}}}}",
            "{\"type\":\"object\",\"additionalProperties\":{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"value\":{\"type\":\"string\"}}}}");
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/add-additional-properties-to-json-schema.test.ts::addAdditionalPropertiesToJsonSchema::leaves non-object schemas unchanged", Coverage = UpstreamCoverage.Covered)]
    public void Additional_properties_leave_strings_unchanged()
    {
        var schema = JsonNode.Parse("{\"type\":\"string\"}")!;
        var result = JsonSchemas.AddAdditionalPropertiesToJsonSchema(schema);
        Assert.Same(schema, result);
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"type\":\"string\"}"), result));
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/number.test.ts::number::should be possible to describe minimum number", Coverage = UpstreamCoverage.Covered)]
    public void Number_schema_describes_a_minimum()
    {
        AssertNumber(new[] { new NumberCheck("min", 5) }, "{\"type\":\"number\",\"minimum\":5}");
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/number.test.ts::number::should be possible to describe maximum number", Coverage = UpstreamCoverage.Covered)]
    public void Number_schema_describes_a_maximum()
    {
        AssertNumber(new[] { new NumberCheck("max", 5) }, "{\"type\":\"number\",\"maximum\":5}");
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/number.test.ts::number::should be possible to describe both minimum and maximum number", Coverage = UpstreamCoverage.Covered)]
    public void Number_schema_describes_both_bounds()
    {
        AssertNumber(new[] { new NumberCheck("min", 5), new NumberCheck("max", 5) }, "{\"type\":\"number\",\"minimum\":5,\"maximum\":5}");
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/number.test.ts::number::should be possible to describe an integer", Coverage = UpstreamCoverage.Covered)]
    public void Number_schema_describes_an_integer()
    {
        AssertNumber(new[] { new NumberCheck("int") }, "{\"type\":\"integer\"}");
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/number.test.ts::number::should be possible to describe multiples of n", Coverage = UpstreamCoverage.Covered)]
    public void Number_schema_describes_a_multiple()
    {
        AssertNumber(new[] { new NumberCheck("multipleOf", 2) }, "{\"type\":\"number\",\"multipleOf\":2}");
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/number.test.ts::number::should be possible to describe positive, negative, nonpositive and nonnegative numbers", Coverage = UpstreamCoverage.Covered)]
    public void Number_schema_describes_sign_checks()
    {
        AssertNumber(
            new[]
            {
                new NumberCheck("min", 0, false),
                new NumberCheck("max", 0, false),
                new NumberCheck("max", 0, true),
                new NumberCheck("min", 0, true),
            },
            "{\"type\":\"number\",\"exclusiveMinimum\":0,\"exclusiveMaximum\":0,\"maximum\":0,\"minimum\":0}");
    }

    private static void AssertSame(string text)
    {
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(text), SecureJson.SecureJsonParse(text)));
    }

    private static void AssertSchema(string input, string expected)
    {
        var result = JsonSchemas.AddAdditionalPropertiesToJsonSchema(JsonNode.Parse(input)!);
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), result));
    }

    private static void AssertNumber(IReadOnlyList<NumberCheck> checks, string expected)
    {
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), JsonSchemas.ParseNumberDef(checks)));
    }

    private static FlexibleSchema StringField(string name)
    {
        return new FlexibleSchema(node =>
        {
            if (node is JsonObject obj && obj[name] is JsonValue value && value.TryGetValue<string>(out _))
            {
                return SchemaValidationResult.Ok(obj);
            }

            return SchemaValidationResult.Fail(new Exception("Invalid input"));
        });
    }

    private static FlexibleSchema NumberField(string name)
    {
        return new FlexibleSchema(node =>
        {
            if (node is JsonObject obj && obj[name] is JsonValue value && value.TryGetValue<int>(out _))
            {
                return SchemaValidationResult.Ok(obj);
            }

            return SchemaValidationResult.Fail(new Exception("Invalid input"));
        });
    }

    private static StandardSchema PersonSchema()
    {
        return new StandardSchema("custom", value =>
        {
            if (value is JsonObject obj
                && obj["name"] is JsonValue name
                && name.TryGetValue<string>(out _)
                && obj["age"] is JsonValue age
                && age.TryGetValue<int>(out _))
            {
                return new StandardSchemaResult { HasValue = true, Value = obj };
            }

            return new StandardSchemaResult { Issues = new object[] { new Exception("Invalid input") } };
        });
    }
}
