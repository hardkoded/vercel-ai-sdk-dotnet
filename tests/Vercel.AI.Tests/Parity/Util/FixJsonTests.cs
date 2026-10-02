// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class FixJsonTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::should handle empty input", Coverage = UpstreamCoverage.Covered)]
    public void Handles_empty_input()
    {
        Assert.Equal(string.Empty, JsonRepair.FixJson(string.Empty));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::literals::should handle incomplete null", Coverage = UpstreamCoverage.Covered)]
    public void Completes_an_incomplete_null()
    {
        Assert.Equal("null", JsonRepair.FixJson("nul"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::literals::should handle incomplete true", Coverage = UpstreamCoverage.Covered)]
    public void Completes_an_incomplete_true()
    {
        Assert.Equal("true", JsonRepair.FixJson("t"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::literals::should handle incomplete false", Coverage = UpstreamCoverage.Covered)]
    public void Completes_an_incomplete_false()
    {
        Assert.Equal("false", JsonRepair.FixJson("fals"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::number::should handle incomplete numbers", Coverage = UpstreamCoverage.Covered)]
    public void Drops_a_trailing_decimal_point()
    {
        Assert.Equal("12", JsonRepair.FixJson("12."));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::number::should handle numbers with dot", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_a_complete_decimal()
    {
        Assert.Equal("12.2", JsonRepair.FixJson("12.2"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::number::should handle negative numbers", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_a_negative_number()
    {
        Assert.Equal("-12", JsonRepair.FixJson("-12"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::number::should handle incomplete negative numbers", Coverage = UpstreamCoverage.Covered)]
    public void Drops_a_lone_minus()
    {
        Assert.Equal(string.Empty, JsonRepair.FixJson("-"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::number::should handle e-notation numbers", Coverage = UpstreamCoverage.Covered)]
    public void Repairs_e_notation()
    {
        Assert.Equal("2.5", JsonRepair.FixJson("2.5e"));
        Assert.Equal("2.5", JsonRepair.FixJson("2.5e-"));
        Assert.Equal("2.5e3", JsonRepair.FixJson("2.5e3"));
        Assert.Equal("-2.5e3", JsonRepair.FixJson("-2.5e3"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::number::should handle uppercase e-notation numbers", Coverage = UpstreamCoverage.Covered)]
    public void Repairs_uppercase_e_notation()
    {
        Assert.Equal("2.5", JsonRepair.FixJson("2.5E"));
        Assert.Equal("2.5", JsonRepair.FixJson("2.5E-"));
        Assert.Equal("2.5E3", JsonRepair.FixJson("2.5E3"));
        Assert.Equal("-2.5E3", JsonRepair.FixJson("-2.5E3"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::number::should handle incomplete numbers #2", Coverage = UpstreamCoverage.Covered)]
    public void Drops_an_incomplete_exponent()
    {
        Assert.Equal("12", JsonRepair.FixJson("12.e"));
        Assert.Equal("12.34", JsonRepair.FixJson("12.34e"));
        Assert.Equal("5", JsonRepair.FixJson("5e"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::string::should handle incomplete strings", Coverage = UpstreamCoverage.Covered)]
    public void Closes_an_incomplete_string()
    {
        Assert.Equal("\"abc\"", JsonRepair.FixJson("\"abc"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::string::should handle escape sequences", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_complete_escape_sequences()
    {
        var input = "\"value with \\\"quoted\\\" text and \\\\ escape";
        Assert.Equal(input + "\"", JsonRepair.FixJson(input));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::string::should handle incomplete escape sequences", Coverage = UpstreamCoverage.Covered)]
    public void Drops_a_trailing_backslash()
    {
        Assert.Equal("\"value with \"", JsonRepair.FixJson("\"value with \\"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::string::should handle incomplete unicode escape sequences", Coverage = UpstreamCoverage.Covered)]
    public void Truncates_incomplete_unicode_escapes()
    {
        Assert.Equal("\"\"", JsonRepair.FixJson("\"\\u"));
        Assert.Equal("\"\"", JsonRepair.FixJson("\"\\u12"));
        Assert.Equal("\"text \"", JsonRepair.FixJson("\"text \\u00"));
        Assert.Equal("{\"a\":\"\"}", JsonRepair.FixJson("{\"a\":\"\\u12"));
        JsonDocument.Parse(JsonRepair.FixJson("\"\\u"));
        JsonDocument.Parse(JsonRepair.FixJson("\"\\u12"));
        JsonDocument.Parse(JsonRepair.FixJson("\"text \\u00"));
        JsonDocument.Parse(JsonRepair.FixJson("{\"a\":\"\\u12"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::string::should handle unicode characters", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_a_unicode_character()
    {
        Assert.Equal("\"value with unicode <\"", JsonRepair.FixJson("\"value with unicode <\""));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::array::should handle incomplete array", Coverage = UpstreamCoverage.Covered)]
    public void Closes_an_empty_array()
    {
        Assert.Equal("[]", JsonRepair.FixJson("["));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::array::should handle closing bracket after number in array", Coverage = UpstreamCoverage.Covered)]
    public void Closes_an_array_after_a_number()
    {
        Assert.Equal("[[1], [2]]", JsonRepair.FixJson("[[1], [2"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::array::should handle closing bracket after string in array", Coverage = UpstreamCoverage.Covered)]
    public void Closes_an_array_after_a_string()
    {
        Assert.Equal("[[\"1\"], [\"2\"]]", JsonRepair.FixJson("[[\"1\"], [\"2"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::array::should handle closing bracket after literal in array", Coverage = UpstreamCoverage.Covered)]
    public void Closes_an_array_after_a_literal()
    {
        Assert.Equal("[[false], [null]]", JsonRepair.FixJson("[[false], [nu"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::array::should handle closing bracket after array in array", Coverage = UpstreamCoverage.Covered)]
    public void Closes_an_array_after_an_array()
    {
        Assert.Equal("[[[]], [[]]]", JsonRepair.FixJson("[[[]], [[]"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::array::should handle closing bracket after object in array", Coverage = UpstreamCoverage.Covered)]
    public void Closes_an_array_after_an_object()
    {
        Assert.Equal("[[{}], [{}]]", JsonRepair.FixJson("[[{}], [{"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::array::should handle trailing comma", Coverage = UpstreamCoverage.Covered)]
    public void Drops_a_trailing_comma()
    {
        Assert.Equal("[1]", JsonRepair.FixJson("[1, "));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::array::should handle closing array", Coverage = UpstreamCoverage.Covered)]
    public void Closes_an_array_after_a_complete_value()
    {
        Assert.Equal("[[], 123]", JsonRepair.FixJson("[[], 123"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::object::should handle keys without values", Coverage = UpstreamCoverage.Covered)]
    public void Drops_a_key_without_a_value()
    {
        Assert.Equal("{}", JsonRepair.FixJson("{\"key\":"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::object::should handle closing brace after number in object", Coverage = UpstreamCoverage.Covered)]
    public void Closes_an_object_after_a_number()
    {
        Assert.Equal("{\"a\": {\"b\": 1}, \"c\": {\"d\": 2}}", JsonRepair.FixJson("{\"a\": {\"b\": 1}, \"c\": {\"d\": 2"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::object::should handle closing brace after string in object", Coverage = UpstreamCoverage.Covered)]
    public void Closes_an_object_after_a_string()
    {
        Assert.Equal("{\"a\": {\"b\": \"1\"}, \"c\": {\"d\": 2}}", JsonRepair.FixJson("{\"a\": {\"b\": \"1\"}, \"c\": {\"d\": 2"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::object::should handle closing brace after literal in object", Coverage = UpstreamCoverage.Covered)]
    public void Closes_an_object_after_a_literal()
    {
        Assert.Equal("{\"a\": {\"b\": false}, \"c\": {\"d\": 2}}", JsonRepair.FixJson("{\"a\": {\"b\": false}, \"c\": {\"d\": 2"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::object::should handle closing brace after array in object", Coverage = UpstreamCoverage.Covered)]
    public void Closes_an_object_after_an_array()
    {
        Assert.Equal("{\"a\": {\"b\": []}, \"c\": {\"d\": 2}}", JsonRepair.FixJson("{\"a\": {\"b\": []}, \"c\": {\"d\": 2"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::object::should handle closing brace after object in object", Coverage = UpstreamCoverage.Covered)]
    public void Closes_an_object_after_an_object()
    {
        Assert.Equal("{\"a\": {\"b\": {}}, \"c\": {\"d\": 2}}", JsonRepair.FixJson("{\"a\": {\"b\": {}}, \"c\": {\"d\": 2"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::object::should handle partial keys (first key)", Coverage = UpstreamCoverage.Covered)]
    public void Drops_a_partial_first_key()
    {
        Assert.Equal("{}", JsonRepair.FixJson("{\"ke"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::object::should handle partial keys (second key)", Coverage = UpstreamCoverage.Covered)]
    public void Drops_a_partial_second_key()
    {
        Assert.Equal("{\"k1\": 1}", JsonRepair.FixJson("{\"k1\": 1, \"k2"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::object::should handle partial keys with colon (second key)", Coverage = UpstreamCoverage.Covered)]
    public void Drops_a_partial_second_key_with_a_colon()
    {
        Assert.Equal("{\"k1\": 1}", JsonRepair.FixJson("{\"k1\": 1, \"k2\":"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::object::should handle trailing whitespace", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_a_complete_object_with_trailing_space()
    {
        Assert.Equal("{\"key\": \"value\"}", JsonRepair.FixJson("{\"key\": \"value\"  "));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::object::should handle closing after empty object", Coverage = UpstreamCoverage.Covered)]
    public void Closes_after_an_empty_nested_object()
    {
        Assert.Equal("{\"a\": {\"b\": {}}}", JsonRepair.FixJson("{\"a\": {\"b\": {}"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::nesting::should handle nested arrays with numbers", Coverage = UpstreamCoverage.Covered)]
    public void Closes_nested_arrays_of_numbers()
    {
        Assert.Equal("[1, [2, 3, []]]", JsonRepair.FixJson("[1, [2, 3, ["));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::nesting::should handle nested arrays with literals", Coverage = UpstreamCoverage.Covered)]
    public void Closes_nested_arrays_of_literals()
    {
        Assert.Equal("[false, [true, []]]", JsonRepair.FixJson("[false, [true, ["));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::nesting::should handle nested objects", Coverage = UpstreamCoverage.Covered)]
    public void Drops_a_nested_key_without_a_value()
    {
        Assert.Equal("{\"key\": {}}", JsonRepair.FixJson("{\"key\": {\"subKey\":"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::nesting::should handle nested objects with numbers", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_a_number_beside_a_partial_nested_object()
    {
        Assert.Equal("{\"key\": 123, \"key2\": {}}", JsonRepair.FixJson("{\"key\": 123, \"key2\": {\"subKey\":"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::nesting::should handle nested objects with literals", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_a_literal_beside_a_partial_nested_object()
    {
        Assert.Equal("{\"key\": null, \"key2\": {}}", JsonRepair.FixJson("{\"key\": null, \"key2\": {\"subKey\":"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::nesting::should handle arrays within objects", Coverage = UpstreamCoverage.Covered)]
    public void Closes_an_object_inside_an_array()
    {
        Assert.Equal("{\"key\": [1, 2, {}]}", JsonRepair.FixJson("{\"key\": [1, 2, {"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::nesting::should handle objects within arrays", Coverage = UpstreamCoverage.Covered)]
    public void Closes_an_object_that_ends_inside_a_string()
    {
        Assert.Equal("[1, 2, {\"key\": \"value\"}]", JsonRepair.FixJson("[1, 2, {\"key\": \"value\","));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::nesting::should handle nested arrays and objects", Coverage = UpstreamCoverage.Covered)]
    public void Closes_mixed_nesting()
    {
        Assert.Equal("{\"a\": {\"b\": [\"c\", {\"d\": \"e\"}]}}", JsonRepair.FixJson("{\"a\": {\"b\": [\"c\", {\"d\": \"e\","));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::nesting::should handle deeply nested objects", Coverage = UpstreamCoverage.Covered)]
    public void Closes_deeply_nested_objects()
    {
        Assert.Equal("{\"a\": {\"b\": {\"c\": {}}}}", JsonRepair.FixJson("{\"a\": {\"b\": {\"c\": {\"d\":"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::nesting::should handle potential nested arrays or objects", Coverage = UpstreamCoverage.Covered)]
    public void Closes_a_partial_array_object_or_string_value()
    {
        Assert.Equal("{\"a\": 1, \"b\": []}", JsonRepair.FixJson("{\"a\": 1, \"b\": ["));
        Assert.Equal("{\"a\": 1, \"b\": {}}", JsonRepair.FixJson("{\"a\": 1, \"b\": {"));
        Assert.Equal("{\"a\": 1, \"b\": \"\"}", JsonRepair.FixJson("{\"a\": 1, \"b\": \""));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::regression::should handle complex nesting 1", Coverage = UpstreamCoverage.Covered)]
    public void Closes_a_partial_pretty_printed_object()
    {
        var input = string.Join(
            "\n",
            "{",
            "  \"a\": [",
            "    {",
            "      \"a1\": \"v1\",",
            "      \"a2\": \"v2\",",
            "      \"a3\": \"v3\"",
            "    }",
            "  ],",
            "  \"b\": [",
            "    {",
            "      \"b1\": \"n");
        var expected = string.Join(
            "\n",
            "{",
            "  \"a\": [",
            "    {",
            "      \"a1\": \"v1\",",
            "      \"a2\": \"v2\",",
            "      \"a3\": \"v3\"",
            "    }",
            "  ],",
            "  \"b\": [",
            "    {",
            "      \"b1\": \"n\"}]}");
        Assert.Equal(expected, JsonRepair.FixJson(input));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::regression::should handle empty objects inside nested objects and arrays", Coverage = UpstreamCoverage.Covered)]
    public void Closes_an_empty_object_inside_nested_arrays()
    {
        Assert.Equal(
            "{\"type\":\"div\",\"children\":[{\"type\":\"Card\",\"props\":{}}]}",
            JsonRepair.FixJson("{\"type\":\"div\",\"children\":[{\"type\":\"Card\",\"props\":{}"));
    }
}
