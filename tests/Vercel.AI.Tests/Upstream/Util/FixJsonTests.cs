// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class FixJsonTests
{
    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::should handle empty input", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Repairs_empty_input()
    {
        Assert.Equal(string.Empty, FixJson.Repair(string.Empty));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::literals::should handle incomplete null", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Completes_null()
    {
        Assert.Equal("null", FixJson.Repair("nul"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::literals::should handle incomplete true", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Completes_true()
    {
        Assert.Equal("true", FixJson.Repair("t"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::literals::should handle incomplete false", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Completes_false()
    {
        Assert.Equal("false", FixJson.Repair("fals"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::number::should handle incomplete numbers", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Drops_a_trailing_decimal_point()
    {
        Assert.Equal("12", FixJson.Repair("12."));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::number::should handle numbers with dot", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Keeps_a_decimal_number()
    {
        Assert.Equal("12.2", FixJson.Repair("12.2"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::number::should handle negative numbers", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Keeps_a_negative_number()
    {
        Assert.Equal("-12", FixJson.Repair("-12"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::number::should handle incomplete negative numbers", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Drops_a_lone_minus()
    {
        Assert.Equal(string.Empty, FixJson.Repair("-"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::number::should handle e-notation numbers", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Repairs_exponent_notation()
    {
        Assert.Equal("2.5", FixJson.Repair("2.5e"));
        Assert.Equal("2.5", FixJson.Repair("2.5e-"));
        Assert.Equal("2.5e3", FixJson.Repair("2.5e3"));
        Assert.Equal("-2.5e3", FixJson.Repair("-2.5e3"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::number::should handle uppercase e-notation numbers", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Repairs_uppercase_exponent_notation()
    {
        Assert.Equal("2.5", FixJson.Repair("2.5E"));
        Assert.Equal("2.5", FixJson.Repair("2.5E-"));
        Assert.Equal("2.5E3", FixJson.Repair("2.5E3"));
        Assert.Equal("-2.5E3", FixJson.Repair("-2.5E3"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::number::should handle incomplete numbers #2", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Drops_an_incomplete_exponent()
    {
        Assert.Equal("12", FixJson.Repair("12.e"));
        Assert.Equal("12.34", FixJson.Repair("12.34e"));
        Assert.Equal("5", FixJson.Repair("5e"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::string::should handle incomplete strings", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Closes_a_string()
    {
        Assert.Equal("\"abc\"", FixJson.Repair("\"abc"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::string::should handle escape sequences", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Closes_a_string_that_contains_escapes()
    {
        var input = "\"value with \\\"quoted\\\" text and \\\\ escape";
        Assert.Equal(input + "\"", FixJson.Repair(input));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::string::should handle incomplete escape sequences", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Drops_a_dangling_escape()
    {
        Assert.Equal("\"value with \"", FixJson.Repair("\"value with \\"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::string::should handle incomplete unicode escape sequences", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Drops_incomplete_unicode_escapes()
    {
        var repaired = new[]
        {
            FixJson.Repair("\"\\u"),
            FixJson.Repair("\"\\u12"),
            FixJson.Repair("\"text \\u00"),
            FixJson.Repair("{\"a\":\"\\u12"),
        };
        Assert.Equal("\"\"", repaired[0]);
        Assert.Equal("\"\"", repaired[1]);
        Assert.Equal("\"text \"", repaired[2]);
        Assert.Equal("{\"a\":\"\"}", repaired[3]);
        for (var i = 0; i < repaired.Length; i++)
        {
            using (JsonDocument.Parse(repaired[i]))
            {
            }
        }
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::string::should handle unicode characters", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Keeps_a_unicode_character()
    {
        Assert.Equal("\"value with unicode <\"", FixJson.Repair("\"value with unicode <\""));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::array::should handle incomplete array", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Closes_an_empty_array()
    {
        Assert.Equal("[]", FixJson.Repair("["));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::array::should handle closing bracket after number in array", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Closes_an_array_after_a_number()
    {
        Assert.Equal("[[1], [2]]", FixJson.Repair("[[1], [2"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::array::should handle closing bracket after string in array", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Closes_an_array_after_a_string()
    {
        Assert.Equal("[[\"1\"], [\"2\"]]", FixJson.Repair("[[\"1\"], [\"2"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::array::should handle closing bracket after literal in array", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Closes_an_array_after_a_literal()
    {
        Assert.Equal("[[false], [null]]", FixJson.Repair("[[false], [nu"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::array::should handle closing bracket after array in array", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Closes_an_array_after_an_array()
    {
        Assert.Equal("[[[]], [[]]]", FixJson.Repair("[[[]], [[]"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::array::should handle closing bracket after object in array", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Closes_an_array_after_an_object()
    {
        Assert.Equal("[[{}], [{}]]", FixJson.Repair("[[{}], [{"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::array::should handle trailing comma", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Drops_a_trailing_comma()
    {
        Assert.Equal("[1]", FixJson.Repair("[1, "));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::array::should handle closing array", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Closes_an_array_after_a_complete_value()
    {
        Assert.Equal("[[], 123]", FixJson.Repair("[[], 123"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::object::should handle keys without values", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Drops_a_key_without_a_value()
    {
        Assert.Equal("{}", FixJson.Repair("{\"key\":"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::object::should handle closing brace after number in object", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Closes_an_object_after_a_number()
    {
        Assert.Equal("{\"a\": {\"b\": 1}, \"c\": {\"d\": 2}}", FixJson.Repair("{\"a\": {\"b\": 1}, \"c\": {\"d\": 2"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::object::should handle closing brace after string in object", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Closes_an_object_after_a_string()
    {
        Assert.Equal("{\"a\": {\"b\": \"1\"}, \"c\": {\"d\": 2}}", FixJson.Repair("{\"a\": {\"b\": \"1\"}, \"c\": {\"d\": 2"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::object::should handle closing brace after literal in object", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Closes_an_object_after_a_literal()
    {
        Assert.Equal("{\"a\": {\"b\": false}, \"c\": {\"d\": 2}}", FixJson.Repair("{\"a\": {\"b\": false}, \"c\": {\"d\": 2"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::object::should handle closing brace after array in object", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Closes_an_object_after_an_array()
    {
        Assert.Equal("{\"a\": {\"b\": []}, \"c\": {\"d\": 2}}", FixJson.Repair("{\"a\": {\"b\": []}, \"c\": {\"d\": 2"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::object::should handle closing brace after object in object", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Closes_an_object_after_an_object()
    {
        Assert.Equal("{\"a\": {\"b\": {}}, \"c\": {\"d\": 2}}", FixJson.Repair("{\"a\": {\"b\": {}}, \"c\": {\"d\": 2"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::object::should handle partial keys (first key)", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Drops_a_partial_first_key()
    {
        Assert.Equal("{}", FixJson.Repair("{\"ke"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::object::should handle partial keys (second key)", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Drops_a_partial_second_key()
    {
        Assert.Equal("{\"k1\": 1}", FixJson.Repair("{\"k1\": 1, \"k2"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::object::should handle partial keys with colon (second key)", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Drops_a_partial_second_key_with_a_colon()
    {
        Assert.Equal("{\"k1\": 1}", FixJson.Repair("{\"k1\": 1, \"k2\":"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::object::should handle trailing whitespace", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Keeps_a_complete_object_with_trailing_space()
    {
        Assert.Equal("{\"key\": \"value\"}", FixJson.Repair("{\"key\": \"value\"  "));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::object::should handle closing after empty object", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Closes_after_an_empty_object()
    {
        Assert.Equal("{\"a\": {\"b\": {}}}", FixJson.Repair("{\"a\": {\"b\": {}"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::nesting::should handle nested arrays with numbers", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Closes_nested_number_arrays()
    {
        Assert.Equal("[1, [2, 3, []]]", FixJson.Repair("[1, [2, 3, ["));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::nesting::should handle nested arrays with literals", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Closes_nested_literal_arrays()
    {
        Assert.Equal("[false, [true, []]]", FixJson.Repair("[false, [true, ["));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::nesting::should handle nested objects", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Drops_an_incomplete_nested_key()
    {
        Assert.Equal("{\"key\": {}}", FixJson.Repair("{\"key\": {\"subKey\":"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::nesting::should handle nested objects with numbers", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Drops_an_incomplete_nested_key_after_a_number()
    {
        Assert.Equal("{\"key\": 123, \"key2\": {}}", FixJson.Repair("{\"key\": 123, \"key2\": {\"subKey\":"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::nesting::should handle nested objects with literals", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Drops_an_incomplete_nested_key_after_null()
    {
        Assert.Equal("{\"key\": null, \"key2\": {}}", FixJson.Repair("{\"key\": null, \"key2\": {\"subKey\":"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::nesting::should handle arrays within objects", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Closes_an_array_inside_an_object()
    {
        Assert.Equal("{\"key\": [1, 2, {}]}", FixJson.Repair("{\"key\": [1, 2, {"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::nesting::should handle objects within arrays", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Closes_an_object_inside_an_array()
    {
        Assert.Equal("[1, 2, {\"key\": \"value\"}]", FixJson.Repair("[1, 2, {\"key\": \"value\","));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::nesting::should handle nested arrays and objects", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Closes_mixed_nesting()
    {
        Assert.Equal("{\"a\": {\"b\": [\"c\", {\"d\": \"e\"}]}}", FixJson.Repair("{\"a\": {\"b\": [\"c\", {\"d\": \"e\","));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::nesting::should handle deeply nested objects", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Closes_deep_objects()
    {
        Assert.Equal("{\"a\": {\"b\": {\"c\": {}}}}", FixJson.Repair("{\"a\": {\"b\": {\"c\": {\"d\":"));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::nesting::should handle potential nested arrays or objects", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Closes_started_nested_values()
    {
        Assert.Equal("{\"a\": 1, \"b\": []}", FixJson.Repair("{\"a\": 1, \"b\": ["));
        Assert.Equal("{\"a\": 1, \"b\": {}}", FixJson.Repair("{\"a\": 1, \"b\": {"));
        Assert.Equal("{\"a\": 1, \"b\": \"\"}", FixJson.Repair("{\"a\": 1, \"b\": \""));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::regression::should handle complex nesting 1", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Repairs_a_multiline_object()
    {
        var input = string.Join("\n", new[]
        {
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
            "      \"b1\": \"n",
        });
        var expected = string.Join("\n", new[]
        {
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
            "      \"b1\": \"n\"}]}",
        });
        Assert.Equal(expected, FixJson.Repair(input));
    }

    [UpstreamTest("packages/ai/src/util/fix-json.test.ts::regression::should handle empty objects inside nested objects and arrays", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Closes_an_empty_object_inside_an_array()
    {
        Assert.Equal(
            "{\"type\":\"div\",\"children\":[{\"type\":\"Card\",\"props\":{}}]}",
            FixJson.Repair("{\"type\":\"div\",\"children\":[{\"type\":\"Card\",\"props\":{}"));
    }
}
