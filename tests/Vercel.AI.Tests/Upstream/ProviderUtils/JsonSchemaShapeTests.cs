// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.Upstream.ProviderUtils;

public sealed class JsonSchemaShapeTests
{
    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/schema.test.ts::asSchema::should create an object schema when no schema is provided",
        Coverage = UpstreamCoverage.Covered)]
    public void Empty_object_schema_rejects_unknown_properties()
    {
        JsonAssert.Equal(JsonSchemas.EmptyObject(), "{\"type\":\"object\",\"properties\":{},\"additionalProperties\":false}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/string.test.ts::string::should be possible to describe minimum length of a string",
        Coverage = UpstreamCoverage.Covered)]
    public void String_schema_sets_min_length()
    {
        JsonAssert.Equal(JsonSchemas.String(minLength: 5), "{\"type\":\"string\",\"minLength\":5}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/string.test.ts::string::should be possible to describe maximum length of a string",
        Coverage = UpstreamCoverage.Covered)]
    public void String_schema_sets_max_length()
    {
        JsonAssert.Equal(JsonSchemas.String(maxLength: 5), "{\"type\":\"string\",\"maxLength\":5}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/string.test.ts::string::should be possible to describe both minimum and maximum length of a string",
        Coverage = UpstreamCoverage.Covered)]
    public void String_schema_sets_both_lengths()
    {
        JsonAssert.Equal(JsonSchemas.String(minLength: 5, maxLength: 5), "{\"type\":\"string\",\"minLength\":5,\"maxLength\":5}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/string.test.ts::string::should be possible to use email constraint",
        Coverage = UpstreamCoverage.Covered)]
    public void String_schema_uses_the_email_format()
    {
        JsonAssert.Equal(JsonSchemas.String(format: "email"), "{\"type\":\"string\",\"format\":\"email\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/string.test.ts::string::should be possible to use uuid constraint",
        Coverage = UpstreamCoverage.Covered)]
    public void String_schema_uses_the_uuid_format()
    {
        JsonAssert.Equal(JsonSchemas.String(format: "uuid"), "{\"type\":\"string\",\"format\":\"uuid\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/string.test.ts::string::should be possible to use url constraint",
        Coverage = UpstreamCoverage.Covered)]
    public void String_schema_uses_the_uri_format()
    {
        JsonAssert.Equal(JsonSchemas.String(format: "uri"), "{\"type\":\"string\",\"format\":\"uri\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/string.test.ts::string::should be possible to use regex constraint",
        Coverage = UpstreamCoverage.Covered)]
    public void String_schema_sets_a_pattern()
    {
        JsonAssert.Equal(JsonSchemas.String(pattern: "[A-C]"), "{\"type\":\"string\",\"pattern\":\"[A-C]\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/string.test.ts::string::should be possible to use CUID constraint",
        Coverage = UpstreamCoverage.Covered)]
    public void String_schema_sets_the_cuid_pattern()
    {
        JsonAssert.Equal(JsonSchemas.String(pattern: "^[cC][^\\s-]{8,}$"), "{\"type\":\"string\",\"pattern\":\"^[cC][^\\\\s-]{8,}$\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/string.test.ts::string::should be possible to use Cuid2 constraint",
        Coverage = UpstreamCoverage.Covered)]
    public void String_schema_sets_the_cuid2_pattern()
    {
        JsonAssert.Equal(JsonSchemas.String(pattern: "^[0-9a-z]+$"), "{\"type\":\"string\",\"pattern\":\"^[0-9a-z]+$\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/string.test.ts::string::should be possible to use datetime constraint",
        Coverage = UpstreamCoverage.Covered)]
    public void String_schema_uses_date_time()
    {
        JsonAssert.Equal(JsonSchemas.String(format: "date-time"), "{\"type\":\"string\",\"format\":\"date-time\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/string.test.ts::string::should be possible to use date constraint",
        Coverage = UpstreamCoverage.Covered)]
    public void String_schema_uses_date()
    {
        JsonAssert.Equal(JsonSchemas.String(format: "date"), "{\"type\":\"string\",\"format\":\"date\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/string.test.ts::string::should be possible to use time constraint",
        Coverage = UpstreamCoverage.Covered)]
    public void String_schema_uses_time()
    {
        JsonAssert.Equal(JsonSchemas.String(format: "time"), "{\"type\":\"string\",\"format\":\"time\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/string.test.ts::string::should be possible to use duration constraint",
        Coverage = UpstreamCoverage.Covered)]
    public void String_schema_uses_duration()
    {
        JsonAssert.Equal(JsonSchemas.String(format: "duration"), "{\"type\":\"string\",\"format\":\"duration\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/string.test.ts::string::should be possible to use length constraint",
        Coverage = UpstreamCoverage.Covered)]
    public void String_schema_sets_an_exact_length()
    {
        JsonAssert.Equal(JsonSchemas.String(minLength: 15, maxLength: 15), "{\"type\":\"string\",\"minLength\":15,\"maxLength\":15}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/string.test.ts::string::should work with the startsWith check",
        Coverage = UpstreamCoverage.Covered)]
    public void String_schema_anchors_a_prefix()
    {
        JsonAssert.Equal(
            JsonSchemas.String(pattern: JsonSchemas.StartsWithPattern("aBcD123{}[]")),
            "{\"type\":\"string\",\"pattern\":\"^aBcD123\\\\{\\\\}\\\\[\\\\]\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/string.test.ts::string::should work with the endsWith check",
        Coverage = UpstreamCoverage.Covered)]
    public void String_schema_anchors_a_suffix()
    {
        JsonAssert.Equal(
            JsonSchemas.String(pattern: JsonSchemas.EndsWithPattern("aBcD123{}[]")),
            "{\"type\":\"string\",\"pattern\":\"aBcD123\\\\{\\\\}\\\\[\\\\]$\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/string.test.ts::string::should work with the includes check",
        Coverage = UpstreamCoverage.Covered)]
    public void String_schema_matches_an_escaped_literal()
    {
        JsonAssert.Equal(
            JsonSchemas.String(pattern: JsonSchemas.IncludesPattern("aBcD123{}[]")),
            "{\"type\":\"string\",\"pattern\":\"aBcD123\\\\{\\\\}\\\\[\\\\]\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/string.test.ts::string::should bundle multiple pattern type checks in an allOf container",
        Coverage = UpstreamCoverage.Covered)]
    public void String_schema_groups_patterns_in_all_of()
    {
        var allOf = new JsonArray
        {
            new JsonObject { ["pattern"] = "^alpha" },
            new JsonObject { ["pattern"] = "omega$" },
        };
        JsonAssert.Equal(JsonSchemas.String(allOf: allOf), "{\"type\":\"string\",\"allOf\":[{\"pattern\":\"^alpha\"},{\"pattern\":\"omega$\"}]}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/string.test.ts::string::should default to contentEncoding for base64, but format and pattern should also work",
        Coverage = UpstreamCoverage.Covered)]
    public void String_schema_describes_base64()
    {
        JsonAssert.Equal(JsonSchemas.String(contentEncoding: "base64"), "{\"type\":\"string\",\"contentEncoding\":\"base64\"}");
        JsonAssert.Equal(JsonSchemas.String(format: "binary"), "{\"type\":\"string\",\"format\":\"binary\"}");
        JsonAssert.Equal(
            JsonSchemas.String(pattern: "^([0-9a-zA-Z+/]{4})*(([0-9a-zA-Z+/]{2}==)|([0-9a-zA-Z+/]{3}=))?$"),
            "{\"type\":\"string\",\"pattern\":\"^([0-9a-zA-Z+/]{4})*(([0-9a-zA-Z+/]{2}==)|([0-9a-zA-Z+/]{3}=))?$\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/string.test.ts::string::should be possible to use nanoid constraint",
        Coverage = UpstreamCoverage.Covered)]
    public void String_schema_sets_the_nanoid_pattern()
    {
        JsonAssert.Equal(JsonSchemas.String(pattern: "^[a-zA-Z0-9_-]{21}$"), "{\"type\":\"string\",\"pattern\":\"^[a-zA-Z0-9_-]{21}$\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/string.test.ts::string::should be possible to use ulid constraint",
        Coverage = UpstreamCoverage.Covered)]
    public void String_schema_sets_the_ulid_pattern()
    {
        JsonAssert.Equal(JsonSchemas.String(pattern: "^[0-9A-HJKMNP-TV-Z]{26}$"), "{\"type\":\"string\",\"pattern\":\"^[0-9A-HJKMNP-TV-Z]{26}$\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/number.test.ts::number::should be possible to describe minimum number",
        Coverage = UpstreamCoverage.Covered)]
    public void Number_schema_sets_a_minimum()
    {
        JsonAssert.Equal(JsonSchemas.Number(minimum: 5), "{\"type\":\"number\",\"minimum\":5}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/number.test.ts::number::should be possible to describe maximum number",
        Coverage = UpstreamCoverage.Covered)]
    public void Number_schema_sets_a_maximum()
    {
        JsonAssert.Equal(JsonSchemas.Number(maximum: 5), "{\"type\":\"number\",\"maximum\":5}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/number.test.ts::number::should be possible to describe both minimum and maximum number",
        Coverage = UpstreamCoverage.Covered)]
    public void Number_schema_sets_both_bounds()
    {
        JsonAssert.Equal(JsonSchemas.Number(minimum: 5, maximum: 5), "{\"type\":\"number\",\"minimum\":5,\"maximum\":5}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/number.test.ts::number::should be possible to describe an integer",
        Coverage = UpstreamCoverage.Covered)]
    public void Number_schema_can_be_an_integer()
    {
        JsonAssert.Equal(JsonSchemas.Integer(), "{\"type\":\"integer\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/number.test.ts::number::should be possible to describe multiples of n",
        Coverage = UpstreamCoverage.Covered)]
    public void Number_schema_sets_multiple_of()
    {
        JsonAssert.Equal(JsonSchemas.Number(multipleOf: 2), "{\"type\":\"number\",\"multipleOf\":2}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/number.test.ts::number::should be possible to describe positive, negative, nonpositive and nonnegative numbers",
        Coverage = UpstreamCoverage.Covered)]
    public void Number_schema_sets_inclusive_and_exclusive_zero()
    {
        JsonAssert.Equal(
            JsonSchemas.Number(minimum: 0, maximum: 0, exclusiveMinimum: 0, exclusiveMaximum: 0),
            "{\"type\":\"number\",\"minimum\":0,\"maximum\":0,\"exclusiveMaximum\":0,\"exclusiveMinimum\":0}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/array.test.ts::array::should be possible to describe a simple array",
        Coverage = UpstreamCoverage.Covered)]
    public void Array_schema_describes_string_items()
    {
        JsonAssert.Equal(JsonSchemas.Array(JsonSchemas.String()), "{\"type\":\"array\",\"items\":{\"type\":\"string\"}}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/array.test.ts::array::should be possible to describe a simple array with any item",
        Coverage = UpstreamCoverage.Covered)]
    public void Array_schema_omits_items_when_any_value_is_allowed()
    {
        JsonAssert.Equal(JsonSchemas.Array(), "{\"type\":\"array\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/array.test.ts::array::should be possible to describe a string array with a minimum and maximum length",
        Coverage = UpstreamCoverage.Covered)]
    public void Array_schema_sets_min_and_max_items()
    {
        JsonAssert.Equal(JsonSchemas.Array(JsonSchemas.String(), 2, 4), "{\"type\":\"array\",\"items\":{\"type\":\"string\"},\"minItems\":2,\"maxItems\":4}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/array.test.ts::array::should be possible to describe a string array with an exact length",
        Coverage = UpstreamCoverage.Covered)]
    public void Array_schema_sets_an_exact_length()
    {
        JsonAssert.Equal(JsonSchemas.Array(JsonSchemas.String(), 5, 5), "{\"type\":\"array\",\"items\":{\"type\":\"string\"},\"minItems\":5,\"maxItems\":5}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/array.test.ts::array::should be possible to describe a string array with a minimum length of 1 by using nonempty",
        Coverage = UpstreamCoverage.Covered)]
    public void Array_schema_requires_one_item()
    {
        JsonAssert.Equal(JsonSchemas.Array(minItems: 1), "{\"type\":\"array\",\"minItems\":1}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/object.test.ts::object::should be possible to describe catchAll schema",
        Coverage = UpstreamCoverage.Covered)]
    public void Object_schema_sets_additional_properties_to_a_schema()
    {
        var schema = JsonSchemas.Object(
            new[] { Pair("normalProperty", JsonSchemas.String()) },
            new[] { "normalProperty" },
            JsonSchemas.Boolean());
        JsonAssert.Equal(schema, "{\"type\":\"object\",\"properties\":{\"normalProperty\":{\"type\":\"string\"}},\"required\":[\"normalProperty\"],\"additionalProperties\":{\"type\":\"boolean\"}}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/object.test.ts::object::should be possible to use selective partial",
        Coverage = UpstreamCoverage.Covered)]
    public void Object_schema_requires_only_the_selected_properties()
    {
        var schema = JsonSchemas.Object(
            new[] { Pair("foo", JsonSchemas.Boolean()), Pair("bar", JsonSchemas.Number()) },
            new[] { "bar" },
            additionalPropertiesFlag: false);
        JsonAssert.Equal(schema, "{\"type\":\"object\",\"properties\":{\"foo\":{\"type\":\"boolean\"},\"bar\":{\"type\":\"number\"}},\"required\":[\"bar\"],\"additionalProperties\":false}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/object.test.ts::object::should allow additional properties unless strict when removeAdditionalStrategy is strict",
        Coverage = UpstreamCoverage.Covered)]
    public void Object_schema_can_allow_or_reject_additional_properties()
    {
        var open = JsonSchemas.Object(
            new[] { Pair("foo", JsonSchemas.Boolean()), Pair("bar", JsonSchemas.Number()) },
            new[] { "foo", "bar" },
            additionalPropertiesFlag: true);
        var closed = JsonSchemas.Object(
            new[] { Pair("foo", JsonSchemas.Boolean()), Pair("bar", JsonSchemas.Number()) },
            new[] { "foo", "bar" },
            additionalPropertiesFlag: false);
        JsonAssert.Equal(open, "{\"type\":\"object\",\"properties\":{\"foo\":{\"type\":\"boolean\"},\"bar\":{\"type\":\"number\"}},\"required\":[\"foo\",\"bar\"],\"additionalProperties\":true}");
        JsonAssert.Equal(closed, "{\"type\":\"object\",\"properties\":{\"foo\":{\"type\":\"boolean\"},\"bar\":{\"type\":\"number\"}},\"required\":[\"foo\",\"bar\"],\"additionalProperties\":false}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/object.test.ts::object::should allow additional properties with catchall when removeAdditionalStrategy is strict",
        Coverage = UpstreamCoverage.Covered)]
    public void Object_schema_keeps_a_catchall_schema()
    {
        var schema = JsonSchemas.Object(
            new[] { Pair("foo", JsonSchemas.Boolean()), Pair("bar", JsonSchemas.Number()) },
            new[] { "foo", "bar" },
            JsonSchemas.Boolean());
        JsonAssert.Equal(schema, "{\"type\":\"object\",\"properties\":{\"foo\":{\"type\":\"boolean\"},\"bar\":{\"type\":\"number\"}},\"required\":[\"foo\",\"bar\"],\"additionalProperties\":{\"type\":\"boolean\"}}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/object.test.ts::object::should be possible to not set additionalProperties at all when allowed",
        Coverage = UpstreamCoverage.Covered)]
    public void Object_schema_can_omit_additional_properties_when_extra_values_are_allowed()
    {
        var schema = JsonSchemas.Object(
            new[] { Pair("foo", JsonSchemas.Boolean()), Pair("bar", JsonSchemas.Number()) },
            new[] { "foo", "bar" });
        JsonAssert.Equal(schema, "{\"type\":\"object\",\"properties\":{\"foo\":{\"type\":\"boolean\"},\"bar\":{\"type\":\"number\"}},\"required\":[\"foo\",\"bar\"]}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/object.test.ts::object::should be possible to not set additionalProperties at all when rejected",
        Coverage = UpstreamCoverage.Covered)]
    public void Object_schema_can_omit_additional_properties_when_the_keyword_is_unset()
    {
        var schema = JsonSchemas.Object(
            new[] { Pair("foo", JsonSchemas.Boolean()), Pair("bar", JsonSchemas.Number()) },
            new[] { "foo", "bar" });
        Assert.False(schema.ContainsKey("additionalProperties"));
        JsonAssert.Equal(schema, "{\"type\":\"object\",\"properties\":{\"foo\":{\"type\":\"boolean\"},\"bar\":{\"type\":\"number\"}},\"required\":[\"foo\",\"bar\"]}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/optional.test.ts::Standalone optionals::should work as unions with undefined",
        Coverage = UpstreamCoverage.Covered)]
    public void Optional_string_is_an_any_of_with_an_empty_not()
    {
        JsonAssert.Equal(JsonSchemas.AnyOf(JsonSchemas.NotEmpty(), JsonSchemas.String()), "{\"anyOf\":[{\"not\":{}},{\"type\":\"string\"}]}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/optional.test.ts::Standalone optionals::should work as unions with void",
        Coverage = UpstreamCoverage.Covered)]
    public void Optional_void_is_an_empty_schema()
    {
        JsonAssert.Equal(JsonSchemas.Anything(), "{}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/optional.test.ts::Standalone optionals::should not affect object properties",
        Coverage = UpstreamCoverage.Covered)]
    public void Optional_object_property_stays_a_string_and_is_not_required()
    {
        var schema = JsonSchemas.Object(new[] { Pair("myProperty", JsonSchemas.String()) }, additionalPropertiesFlag: false);
        JsonAssert.Equal(schema, "{\"type\":\"object\",\"properties\":{\"myProperty\":{\"type\":\"string\"}},\"additionalProperties\":false}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/optional.test.ts::Standalone optionals::should work with nested properties",
        Coverage = UpstreamCoverage.Covered)]
    public void Optional_array_items_use_any_of()
    {
        var items = JsonSchemas.AnyOf(JsonSchemas.NotEmpty(), JsonSchemas.String());
        var schema = JsonSchemas.Object(
            new[] { Pair("myProperty", JsonSchemas.Array(items)) },
            new[] { "myProperty" },
            additionalPropertiesFlag: false);
        JsonAssert.Equal(schema, "{\"type\":\"object\",\"properties\":{\"myProperty\":{\"type\":\"array\",\"items\":{\"anyOf\":[{\"not\":{}},{\"type\":\"string\"}]}}},\"required\":[\"myProperty\"],\"additionalProperties\":false}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/optional.test.ts::Standalone optionals::should work with nested properties as object properties",
        Coverage = UpstreamCoverage.Covered)]
    public void Optional_nested_object_property_is_not_required()
    {
        var inner = JsonSchemas.Object(new[] { Pair("myInnerProperty", JsonSchemas.String()) }, additionalPropertiesFlag: false);
        var schema = JsonSchemas.Object(new[] { Pair("myProperty", inner) }, new[] { "myProperty" }, additionalPropertiesFlag: false);
        JsonAssert.Equal(schema, "{\"type\":\"object\",\"properties\":{\"myProperty\":{\"type\":\"object\",\"properties\":{\"myInnerProperty\":{\"type\":\"string\"}},\"additionalProperties\":false}},\"required\":[\"myProperty\"],\"additionalProperties\":false}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/optional.test.ts::Standalone optionals::should work with nested properties with nested object property parents",
        Coverage = UpstreamCoverage.Covered)]
    public void Optional_nested_array_keeps_the_inner_property_required()
    {
        var items = JsonSchemas.AnyOf(JsonSchemas.NotEmpty(), JsonSchemas.String());
        var inner = JsonSchemas.Object(
            new[] { Pair("myInnerProperty", JsonSchemas.Array(items)) },
            new[] { "myInnerProperty" },
            additionalPropertiesFlag: false);
        var schema = JsonSchemas.Object(new[] { Pair("myProperty", inner) }, new[] { "myProperty" }, additionalPropertiesFlag: false);
        JsonAssert.Equal(schema, "{\"type\":\"object\",\"properties\":{\"myProperty\":{\"type\":\"object\",\"properties\":{\"myInnerProperty\":{\"type\":\"array\",\"items\":{\"anyOf\":[{\"not\":{}},{\"type\":\"string\"}]}}},\"required\":[\"myInnerProperty\"],\"additionalProperties\":false}},\"required\":[\"myProperty\"],\"additionalProperties\":false}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/record.test.ts::record::should be possible to describe a simple record",
        Coverage = UpstreamCoverage.Covered)]
    public void Record_schema_uses_additional_properties()
    {
        JsonAssert.Equal(JsonSchemas.Record(JsonSchemas.Number()), "{\"type\":\"object\",\"additionalProperties\":{\"type\":\"number\"}}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/record.test.ts::record::should be possible to describe a simple record with a branded key",
        Coverage = UpstreamCoverage.Covered)]
    public void Record_schema_ignores_a_brand_on_the_key()
    {
        JsonAssert.Equal(JsonSchemas.Record(JsonSchemas.Number()), "{\"type\":\"object\",\"additionalProperties\":{\"type\":\"number\"}}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/record.test.ts::record::should be possible to describe a complex record with checks",
        Coverage = UpstreamCoverage.Covered)]
    public void Record_schema_nests_an_object_value_schema()
    {
        var value = JsonSchemas.Object(
            new[] { Pair("foo", JsonSchemas.Number(minimum: 2)) },
            new[] { "foo" },
            JsonSchemas.String(pattern: "^[cC][^\\s-]{8,}$"));
        JsonAssert.Equal(
            JsonSchemas.Record(value),
            "{\"type\":\"object\",\"additionalProperties\":{\"type\":\"object\",\"properties\":{\"foo\":{\"type\":\"number\",\"minimum\":2}},\"required\":[\"foo\"],\"additionalProperties\":{\"type\":\"string\",\"pattern\":\"^[cC][^\\\\s-]{8,}$\"}}}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/record.test.ts::record::should be possible to describe a key schema",
        Coverage = UpstreamCoverage.Covered)]
    public void Record_schema_sets_property_names()
    {
        JsonAssert.Equal(
            JsonSchemas.Record(JsonSchemas.Number(), new JsonObject { ["format"] = "uuid" }),
            "{\"type\":\"object\",\"additionalProperties\":{\"type\":\"number\"},\"propertyNames\":{\"format\":\"uuid\"}}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/record.test.ts::record::should be possible to describe a branded key schema",
        Coverage = UpstreamCoverage.Covered)]
    public void Record_schema_sets_a_key_pattern()
    {
        JsonAssert.Equal(
            JsonSchemas.Record(JsonSchemas.Number(), new JsonObject { ["pattern"] = ".+" }),
            "{\"type\":\"object\",\"additionalProperties\":{\"type\":\"number\"},\"propertyNames\":{\"pattern\":\".+\"}}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/record.test.ts::record::should be possible to describe a key with an enum",
        Coverage = UpstreamCoverage.Covered)]
    public void Record_schema_sets_an_enum_of_keys()
    {
        JsonAssert.Equal(
            JsonSchemas.Record(JsonSchemas.Number(), new JsonObject { ["enum"] = new JsonArray { "foo", "bar" } }),
            "{\"type\":\"object\",\"additionalProperties\":{\"type\":\"number\"},\"propertyNames\":{\"enum\":[\"foo\",\"bar\"]}}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/native-enum.test.ts::native enum::should be possible to convert a basic native number enum",
        Coverage = UpstreamCoverage.Covered)]
    public void Enum_schema_lists_numbers()
    {
        JsonAssert.Equal(JsonSchemas.Enum(JsonValue.Create("number")!, new JsonArray { 0, 1, 2 }), "{\"type\":\"number\",\"enum\":[0,1,2]}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/native-enum.test.ts::native enum::should be possible to convert a native string enum",
        Coverage = UpstreamCoverage.Covered)]
    public void Enum_schema_lists_strings()
    {
        JsonAssert.Equal(JsonSchemas.Enum(JsonValue.Create("string")!, new JsonArray { "a", "b", "c" }), "{\"type\":\"string\",\"enum\":[\"a\",\"b\",\"c\"]}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/native-enum.test.ts::native enum::should be possible to convert a mixed value native enum",
        Coverage = UpstreamCoverage.Covered)]
    public void Enum_schema_lists_mixed_values()
    {
        JsonAssert.Equal(
            JsonSchemas.Enum(new JsonArray { "string", "number" }, new JsonArray { "a", 1, "c" }),
            "{\"type\":[\"string\",\"number\"],\"enum\":[\"a\",1,\"c\"]}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/native-enum.test.ts::native enum::should be possible to convert a native const assertion object",
        Coverage = UpstreamCoverage.Covered)]
    public void Enum_schema_lists_const_numbers()
    {
        JsonAssert.Equal(JsonSchemas.Enum(JsonValue.Create("number")!, new JsonArray { 0, 1, 2 }), "{\"type\":\"number\",\"enum\":[0,1,2]}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/native-enum.test.ts::native enum::should be possible to convert a native const assertion string object",
        Coverage = UpstreamCoverage.Covered)]
    public void Enum_schema_lists_const_strings()
    {
        JsonAssert.Equal(JsonSchemas.Enum(JsonValue.Create("string")!, new JsonArray { "a", "b", "c" }), "{\"type\":\"string\",\"enum\":[\"a\",\"b\",\"c\"]}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/native-enum.test.ts::native enum::should be possible to convert a mixed value native const assertion string object",
        Coverage = UpstreamCoverage.Covered)]
    public void Enum_schema_lists_mixed_const_values()
    {
        JsonAssert.Equal(
            JsonSchemas.Enum(new JsonArray { "string", "number" }, new JsonArray { "a", 1, "c" }),
            "{\"type\":[\"string\",\"number\"],\"enum\":[\"a\",1,\"c\"]}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/date.test.ts::Date validations::should be possible to date as a string type",
        Coverage = UpstreamCoverage.Covered)]
    public void Date_schema_is_a_date_time_string()
    {
        JsonAssert.Equal(JsonSchemas.DateTimeString(), "{\"type\":\"string\",\"format\":\"date-time\"}");
        JsonAssert.Equal(JsonSchemas.String(format: "date-time"), "{\"type\":\"string\",\"format\":\"date-time\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/date.test.ts::Date validations::should be possible to describe minimum date",
        Coverage = UpstreamCoverage.Covered)]
    public void Date_schema_sets_a_unix_minimum()
    {
        JsonAssert.Equal(JsonSchemas.Integer(minimum: 86400000, format: "unix-time"), "{\"type\":\"integer\",\"format\":\"unix-time\",\"minimum\":86400000}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/date.test.ts::Date validations::should be possible to describe maximum date",
        Coverage = UpstreamCoverage.Covered)]
    public void Date_schema_sets_a_unix_maximum()
    {
        JsonAssert.Equal(JsonSchemas.Integer(maximum: 86400000, format: "unix-time"), "{\"type\":\"integer\",\"format\":\"unix-time\",\"maximum\":86400000}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/date.test.ts::Date validations::should be possible to describe both maximum and minimum date",
        Coverage = UpstreamCoverage.Covered)]
    public void Date_schema_sets_both_unix_bounds()
    {
        JsonAssert.Equal(
            JsonSchemas.Integer(minimum: 86400000, maximum: 63158400000d, format: "unix-time"),
            "{\"type\":\"integer\",\"format\":\"unix-time\",\"minimum\":86400000,\"maximum\":63158400000}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/date.test.ts::Date validations::multiple choices of strategy should result in anyOf",
        Coverage = UpstreamCoverage.Covered)]
    public void Date_schema_offers_several_representations()
    {
        var schema = JsonSchemas.AnyOf(
            JsonSchemas.String(format: "date-time"),
            JsonSchemas.String(format: "date"),
            JsonSchemas.Integer(format: "unix-time"));
        JsonAssert.Equal(schema, "{\"anyOf\":[{\"type\":\"string\",\"format\":\"date-time\"},{\"type\":\"string\",\"format\":\"date\"},{\"type\":\"integer\",\"format\":\"unix-time\"}]}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/default.test.ts::default::should be possible to use default on objects",
        Coverage = UpstreamCoverage.Covered)]
    public void Object_schema_can_carry_a_default()
    {
        var schema = JsonSchemas.Object(
            new[] { Pair("foo", JsonSchemas.Boolean()) },
            new[] { "foo" },
            additionalPropertiesFlag: false,
            defaultValue: new JsonObject { ["foo"] = true });
        JsonAssert.Equal(schema, "{\"type\":\"object\",\"additionalProperties\":false,\"required\":[\"foo\"],\"properties\":{\"foo\":{\"type\":\"boolean\"}},\"default\":{\"foo\":true}}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/default.test.ts::default::should be possible to use default on primitives",
        Coverage = UpstreamCoverage.Covered)]
    public void String_schema_can_carry_a_default()
    {
        JsonAssert.Equal(JsonSchemas.WithDefault(JsonSchemas.String(), JsonValue.Create("default")!), "{\"type\":\"string\",\"default\":\"default\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/default.test.ts::default::default with transform",
        Coverage = UpstreamCoverage.Covered)]
    public void String_default_stays_the_input_value()
    {
        JsonAssert.Equal(JsonSchemas.WithDefault(JsonSchemas.String(), JsonValue.Create("default")!), "{\"type\":\"string\",\"default\":\"default\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/tuple.test.ts::tuple::should be possible to describe a simple tuple schema",
        Coverage = UpstreamCoverage.Covered)]
    public void Tuple_schema_fixes_the_length()
    {
        var schema = JsonSchemas.Array(new JsonArray { JsonSchemas.String(), JsonSchemas.Number() }, 2, 2);
        JsonAssert.Equal(schema, "{\"type\":\"array\",\"items\":[{\"type\":\"string\"},{\"type\":\"number\"}],\"minItems\":2,\"maxItems\":2}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/tuple.test.ts::tuple::should be possible to describe a tuple schema with rest()",
        Coverage = UpstreamCoverage.Covered)]
    public void Tuple_schema_describes_additional_items()
    {
        var schema = JsonSchemas.Array(new JsonArray { JsonSchemas.String(), JsonSchemas.Number() }, minItems: 2, additionalItems: JsonSchemas.Boolean());
        JsonAssert.Equal(schema, "{\"type\":\"array\",\"items\":[{\"type\":\"string\"},{\"type\":\"number\"}],\"minItems\":2,\"additionalItems\":{\"type\":\"boolean\"}}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/union.test.ts::union::Should be possible to get a simple type array from a union of only unvalidated primitives",
        Coverage = UpstreamCoverage.Covered)]
    public void Union_of_primitives_is_a_type_array()
    {
        JsonAssert.Equal(JsonSchemas.PrimitiveUnion("string", "number", "boolean", "null"), "{\"type\":[\"string\",\"number\",\"boolean\",\"null\"]}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/union.test.ts::union::Should be possible to get an anyOf array with enum values from a union of literals",
        Coverage = UpstreamCoverage.Covered)]
    public void Union_of_non_json_literals_is_three_object_schemas()
    {
        JsonAssert.Equal(
            JsonSchemas.AnyOf(JsonSchemas.Object(), JsonSchemas.Object(), JsonSchemas.Object()),
            "{\"anyOf\":[{\"type\":\"object\"},{\"type\":\"object\"},{\"type\":\"object\"}]}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/union.test.ts::union::Should be possible to create a union with objects, arrays and validated primitives as an anyOf",
        Coverage = UpstreamCoverage.Covered)]
    public void Union_of_objects_arrays_and_checked_primitives_is_any_of()
    {
        var schema = JsonSchemas.AnyOf(
            JsonSchemas.Object(
                new[] { Pair("herp", JsonSchemas.String()), Pair("derp", JsonSchemas.Boolean()) },
                new[] { "herp", "derp" },
                additionalPropertiesFlag: false),
            JsonSchemas.Array(JsonSchemas.Number()),
            JsonSchemas.String(minLength: 3),
            JsonSchemas.Number());
        JsonAssert.Equal(schema, "{\"anyOf\":[{\"type\":\"object\",\"properties\":{\"herp\":{\"type\":\"string\"},\"derp\":{\"type\":\"boolean\"}},\"required\":[\"herp\",\"derp\"],\"additionalProperties\":false},{\"type\":\"array\",\"items\":{\"type\":\"number\"}},{\"type\":\"string\",\"minLength\":3},{\"type\":\"number\"}]}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/union.test.ts::union::nullable primitives should come out fine",
        Coverage = UpstreamCoverage.Covered)]
    public void Nullable_string_is_a_type_array()
    {
        JsonAssert.Equal(JsonSchemas.PrimitiveUnion("string", "null"), "{\"type\":[\"string\",\"null\"]}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/union.test.ts::union::should join a union of Zod enums into a single enum",
        Coverage = UpstreamCoverage.Covered)]
    public void Joined_enums_are_one_string_enum()
    {
        JsonAssert.Equal(
            JsonSchemas.Enum(JsonValue.Create("string")!, new JsonArray { "a", "b", "c", "d", "e" }),
            "{\"type\":\"string\",\"enum\":[\"a\",\"b\",\"c\",\"d\",\"e\"]}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/union.test.ts::union::should work with discriminated union type",
        Coverage = UpstreamCoverage.Covered)]
    public void Discriminated_union_is_any_of_objects()
    {
        var schema = JsonSchemas.AnyOf(
            JsonSchemas.Object(
                new[] { Pair("kek", JsonSchemas.Literal("string", JsonValue.Create("A")!)), Pair("lel", JsonSchemas.Boolean()) },
                new[] { "kek", "lel" },
                additionalPropertiesFlag: false),
            JsonSchemas.Object(
                new[] { Pair("kek", JsonSchemas.Literal("string", JsonValue.Create("B")!)), Pair("lel", JsonSchemas.Number()) },
                new[] { "kek", "lel" },
                additionalPropertiesFlag: false));
        JsonAssert.Equal(schema, "{\"anyOf\":[{\"type\":\"object\",\"properties\":{\"kek\":{\"type\":\"string\",\"const\":\"A\"},\"lel\":{\"type\":\"boolean\"}},\"required\":[\"kek\",\"lel\"],\"additionalProperties\":false},{\"type\":\"object\",\"properties\":{\"kek\":{\"type\":\"string\",\"const\":\"B\"},\"lel\":{\"type\":\"number\"}},\"required\":[\"kek\",\"lel\"],\"additionalProperties\":false}]}");
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/to-json-schema/zod3-to-json-schema/parsers/union.test.ts::union::should not ignore descriptions in literal unions",
        Coverage = UpstreamCoverage.Covered)]
    public void Literal_unions_keep_descriptions_as_any_of()
    {
        JsonAssert.Equal(
            JsonSchemas.Enum(new JsonArray { "boolean", "string", "number" }, new JsonArray { true, "herp", 3 }),
            "{\"type\":[\"boolean\",\"string\",\"number\"],\"enum\":[true,\"herp\",3]}");
        JsonAssert.Equal(
            JsonSchemas.AnyOf(
                JsonSchemas.Literal("boolean", JsonValue.Create(true)!),
                JsonSchemas.Literal("string", JsonValue.Create("herp")!, "derp"),
                JsonSchemas.Literal("number", JsonValue.Create(3)!)),
            "{\"anyOf\":[{\"type\":\"boolean\",\"const\":true},{\"type\":\"string\",\"const\":\"herp\",\"description\":\"derp\"},{\"type\":\"number\",\"const\":3}]}");
    }

    private static KeyValuePair<string, JsonNode> Pair(string name, JsonNode schema)
    {
        return new KeyValuePair<string, JsonNode>(name, schema);
    }
}
