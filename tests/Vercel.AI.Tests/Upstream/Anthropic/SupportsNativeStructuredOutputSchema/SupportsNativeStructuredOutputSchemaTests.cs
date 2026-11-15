// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Anthropic;

namespace Vercel.AI.Tests.Upstream.Anthropic.SupportsNativeStructuredOutputSchema;

public sealed class SupportsNativeStructuredOutputSchemaTests
{
    [Fact]
    [UpstreamTest("packages/anthropic/src/supports-native-structured-output-schema.test.ts::supportsNativeStructuredOutputSchema::accepts a closed object", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public void Accepts_a_closed_object()
    {
        Check("{\"type\":\"object\",\"properties\":{\"answer\":{\"type\":\"string\"}},\"required\":[\"answer\"],\"additionalProperties\":false}", true);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/supports-native-structured-output-schema.test.ts::supportsNativeStructuredOutputSchema::accepts an object without additionalProperties that has properties", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public void Accepts_an_object_without_additionalproperties_that_has_properties()
    {
        Check("{\"type\":\"object\",\"properties\":{\"answer\":{\"type\":\"string\"}}}", true);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/supports-native-structured-output-schema.test.ts::supportsNativeStructuredOutputSchema::accepts many required properties", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public void Accepts_many_required_properties()
    {
        Check("{\"type\":\"object\",\"properties\":{\"f0\":{\"type\":\"string\"},\"f1\":{\"type\":\"string\"},\"f2\":{\"type\":\"string\"},\"f3\":{\"type\":\"string\"},\"f4\":{\"type\":\"string\"},\"f5\":{\"type\":\"string\"},\"f6\":{\"type\":\"string\"},\"f7\":{\"type\":\"string\"},\"f8\":{\"type\":\"string\"},\"f9\":{\"type\":\"string\"},\"f10\":{\"type\":\"string\"},\"f11\":{\"type\":\"string\"},\"f12\":{\"type\":\"string\"},\"f13\":{\"type\":\"string\"},\"f14\":{\"type\":\"string\"},\"f15\":{\"type\":\"string\"},\"f16\":{\"type\":\"string\"},\"f17\":{\"type\":\"string\"},\"f18\":{\"type\":\"string\"},\"f19\":{\"type\":\"string\"},\"f20\":{\"type\":\"string\"},\"f21\":{\"type\":\"string\"},\"f22\":{\"type\":\"string\"},\"f23\":{\"type\":\"string\"},\"f24\":{\"type\":\"string\"},\"f25\":{\"type\":\"string\"},\"f26\":{\"type\":\"string\"},\"f27\":{\"type\":\"string\"},\"f28\":{\"type\":\"string\"},\"f29\":{\"type\":\"string\"},\"f30\":{\"type\":\"string\"},\"f31\":{\"type\":\"string\"},\"f32\":{\"type\":\"string\"},\"f33\":{\"type\":\"string\"},\"f34\":{\"type\":\"string\"},\"f35\":{\"type\":\"string\"},\"f36\":{\"type\":\"string\"},\"f37\":{\"type\":\"string\"},\"f38\":{\"type\":\"string\"},\"f39\":{\"type\":\"string\"},\"f40\":{\"type\":\"string\"},\"f41\":{\"type\":\"string\"},\"f42\":{\"type\":\"string\"},\"f43\":{\"type\":\"string\"},\"f44\":{\"type\":\"string\"},\"f45\":{\"type\":\"string\"},\"f46\":{\"type\":\"string\"},\"f47\":{\"type\":\"string\"},\"f48\":{\"type\":\"string\"},\"f49\":{\"type\":\"string\"},\"f50\":{\"type\":\"string\"},\"f51\":{\"type\":\"string\"},\"f52\":{\"type\":\"string\"},\"f53\":{\"type\":\"string\"},\"f54\":{\"type\":\"string\"},\"f55\":{\"type\":\"string\"},\"f56\":{\"type\":\"string\"},\"f57\":{\"type\":\"string\"},\"f58\":{\"type\":\"string\"},\"f59\":{\"type\":\"string\"}},\"required\":[\"f0\",\"f1\",\"f2\",\"f3\",\"f4\",\"f5\",\"f6\",\"f7\",\"f8\",\"f9\",\"f10\",\"f11\",\"f12\",\"f13\",\"f14\",\"f15\",\"f16\",\"f17\",\"f18\",\"f19\",\"f20\",\"f21\",\"f22\",\"f23\",\"f24\",\"f25\",\"f26\",\"f27\",\"f28\",\"f29\",\"f30\",\"f31\",\"f32\",\"f33\",\"f34\",\"f35\",\"f36\",\"f37\",\"f38\",\"f39\",\"f40\",\"f41\",\"f42\",\"f43\",\"f44\",\"f45\",\"f46\",\"f47\",\"f48\",\"f49\",\"f50\",\"f51\",\"f52\",\"f53\",\"f54\",\"f55\",\"f56\",\"f57\",\"f58\",\"f59\"],\"additionalProperties\":false}", true);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/supports-native-structured-output-schema.test.ts::supportsNativeStructuredOutputSchema::rejects self-referencing schemas", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public void Rejects_self_referencing_schemas()
    {
        Check("{\"type\":\"object\",\"properties\":{\"root\":{\"$ref\":\"#/$defs/node\"}},\"$defs\":{\"node\":{\"type\":\"object\",\"properties\":{\"children\":{\"items\":{\"$ref\":\"#/$defs/node\"}}}}}}", false);
    }

    [Theory]
    [InlineData("{\"type\":\"object\",\"properties\":{\"field\":{\"type\":\"object\",\"additionalProperties\":{\"type\":\"number\"}}},\"required\":[\"field\"],\"additionalProperties\":false}")]
    [InlineData("{\"type\":\"object\",\"properties\":{\"field\":{\"type\":\"object\",\"additionalProperties\":true}},\"required\":[\"field\"],\"additionalProperties\":false}")]
    [InlineData("{\"type\":\"object\",\"properties\":{\"field\":{\"type\":\"object\"}},\"required\":[\"field\"],\"additionalProperties\":false}")]
    [InlineData("{\"type\":\"object\",\"properties\":{\"field\":{\"type\":\"object\",\"patternProperties\":{\"^x-\":{\"type\":\"string\"}}}},\"required\":[\"field\"],\"additionalProperties\":false}")]
    [UpstreamTest("packages/anthropic/src/supports-native-structured-output-schema.test.ts::supportsNativeStructuredOutputSchema::open objects::rejects %s", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public void Rejects(string schema)
    {
        Check(schema, false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/supports-native-structured-output-schema.test.ts::supportsNativeStructuredOutputSchema::open objects::rejects an open object reached through a $ref", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public void Rejects_an_open_object_reached_through_a_ref()
    {
        Check("{\"type\":\"object\",\"properties\":{\"scores\":{\"$ref\":\"#/$defs/Scores\"}},\"required\":[\"scores\"],\"additionalProperties\":false,\"$defs\":{\"Scores\":{\"type\":\"object\",\"additionalProperties\":{\"type\":\"number\"}}}}", false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/supports-native-structured-output-schema.test.ts::supportsNativeStructuredOutputSchema::open objects::accepts an empty closed object", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public void Accepts_an_empty_closed_object()
    {
        Check("{\"type\":\"object\",\"additionalProperties\":false}", true);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/supports-native-structured-output-schema.test.ts::supportsNativeStructuredOutputSchema::optional properties::accepts 10 optional properties", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public void Accepts_10_optional_properties()
    {
        Check("{\"type\":\"object\",\"properties\":{\"f0\":{\"type\":\"string\"},\"f1\":{\"type\":\"string\"},\"f2\":{\"type\":\"string\"},\"f3\":{\"type\":\"string\"},\"f4\":{\"type\":\"string\"},\"f5\":{\"type\":\"string\"},\"f6\":{\"type\":\"string\"},\"f7\":{\"type\":\"string\"},\"f8\":{\"type\":\"string\"},\"f9\":{\"type\":\"string\"}},\"additionalProperties\":false}", true);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/supports-native-structured-output-schema.test.ts::supportsNativeStructuredOutputSchema::optional properties::rejects 11 optional properties", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public void Rejects_11_optional_properties()
    {
        Check("{\"type\":\"object\",\"properties\":{\"f0\":{\"type\":\"string\"},\"f1\":{\"type\":\"string\"},\"f2\":{\"type\":\"string\"},\"f3\":{\"type\":\"string\"},\"f4\":{\"type\":\"string\"},\"f5\":{\"type\":\"string\"},\"f6\":{\"type\":\"string\"},\"f7\":{\"type\":\"string\"},\"f8\":{\"type\":\"string\"},\"f9\":{\"type\":\"string\"},\"f10\":{\"type\":\"string\"}},\"additionalProperties\":false}", false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/supports-native-structured-output-schema.test.ts::supportsNativeStructuredOutputSchema::optional properties::counts optional properties across nested objects", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public void Counts_optional_properties_across_nested_objects()
    {
        Check("{\"type\":\"object\",\"properties\":{\"a\":{\"type\":\"string\"},\"nested\":{\"type\":\"object\",\"properties\":{\"f0\":{\"type\":\"string\"},\"f1\":{\"type\":\"string\"},\"f2\":{\"type\":\"string\"},\"f3\":{\"type\":\"string\"},\"f4\":{\"type\":\"string\"},\"f5\":{\"type\":\"string\"},\"f6\":{\"type\":\"string\"},\"f7\":{\"type\":\"string\"},\"f8\":{\"type\":\"string\"},\"f9\":{\"type\":\"string\"}},\"additionalProperties\":false}},\"required\":[\"nested\"],\"additionalProperties\":false}", false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/supports-native-structured-output-schema.test.ts::supportsNativeStructuredOutputSchema::optional properties::counts optional properties in array items and union branches", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public void Counts_optional_properties_in_array_items_and_union_branches()
    {
        Check("{\"type\":\"object\",\"properties\":{\"items\":{\"type\":\"array\",\"items\":{\"type\":\"object\",\"properties\":{\"i0\":{\"type\":\"string\"},\"i1\":{\"type\":\"string\"},\"i2\":{\"type\":\"string\"},\"i3\":{\"type\":\"string\"},\"i4\":{\"type\":\"string\"}},\"additionalProperties\":false}},\"union\":{\"anyOf\":[{\"type\":\"object\",\"properties\":{\"a0\":{\"type\":\"string\"},\"a1\":{\"type\":\"string\"},\"a2\":{\"type\":\"string\"}},\"additionalProperties\":false},{\"type\":\"object\",\"properties\":{\"b0\":{\"type\":\"string\"},\"b1\":{\"type\":\"string\"},\"b2\":{\"type\":\"string\"}},\"additionalProperties\":false}]}},\"required\":[\"items\",\"union\"],\"additionalProperties\":false}", false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/supports-native-structured-output-schema.test.ts::supportsNativeStructuredOutputSchema::optional properties::counts a referenced definition at every use", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public void Counts_a_referenced_definition_at_every_use()
    {
        Check("{\"type\":\"object\",\"properties\":{\"x\":{\"$ref\":\"#/$defs/D\"},\"y\":{\"$ref\":\"#/$defs/D\"}},\"required\":[\"x\",\"y\"],\"additionalProperties\":false,\"$defs\":{\"D\":{\"type\":\"object\",\"properties\":{\"d0\":{\"type\":\"string\"},\"d1\":{\"type\":\"string\"},\"d2\":{\"type\":\"string\"},\"d3\":{\"type\":\"string\"},\"d4\":{\"type\":\"string\"}},\"additionalProperties\":false}}}", true);
        Check("{\"type\":\"object\",\"properties\":{\"x\":{\"$ref\":\"#/$defs/D\"},\"y\":{\"$ref\":\"#/$defs/D\"}},\"required\":[\"x\",\"y\"],\"additionalProperties\":false,\"$defs\":{\"D\":{\"type\":\"object\",\"properties\":{\"d0\":{\"type\":\"string\"},\"d1\":{\"type\":\"string\"},\"d2\":{\"type\":\"string\"},\"d3\":{\"type\":\"string\"},\"d4\":{\"type\":\"string\"},\"d5\":{\"type\":\"string\"}},\"additionalProperties\":false}}}", false);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/supports-native-structured-output-schema.test.ts::supportsNativeStructuredOutputSchema::optional properties::ignores definitions that are never referenced", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public void Ignores_definitions_that_are_never_referenced()
    {
        Check("{\"type\":\"object\",\"properties\":{\"answer\":{\"type\":\"string\"}},\"required\":[\"answer\"],\"additionalProperties\":false,\"$defs\":{\"Unused\":{\"type\":\"object\",\"properties\":{\"u0\":{\"type\":\"string\"},\"u1\":{\"type\":\"string\"},\"u2\":{\"type\":\"string\"},\"u3\":{\"type\":\"string\"},\"u4\":{\"type\":\"string\"},\"u5\":{\"type\":\"string\"},\"u6\":{\"type\":\"string\"},\"u7\":{\"type\":\"string\"},\"u8\":{\"type\":\"string\"},\"u9\":{\"type\":\"string\"},\"u10\":{\"type\":\"string\"},\"u11\":{\"type\":\"string\"},\"u12\":{\"type\":\"string\"},\"u13\":{\"type\":\"string\"},\"u14\":{\"type\":\"string\"},\"u15\":{\"type\":\"string\"},\"u16\":{\"type\":\"string\"},\"u17\":{\"type\":\"string\"},\"u18\":{\"type\":\"string\"},\"u19\":{\"type\":\"string\"},\"u20\":{\"type\":\"string\"},\"u21\":{\"type\":\"string\"},\"u22\":{\"type\":\"string\"},\"u23\":{\"type\":\"string\"},\"u24\":{\"type\":\"string\"},\"u25\":{\"type\":\"string\"},\"u26\":{\"type\":\"string\"},\"u27\":{\"type\":\"string\"},\"u28\":{\"type\":\"string\"},\"u29\":{\"type\":\"string\"}},\"additionalProperties\":false}}}", true);
    }

    private static void Check(string schema, bool expected)
    {
        using var document = JsonDocument.Parse(schema);
        Assert.Equal(expected, SupportsNativeStructuredOutputSchemaChecker.SupportsNativeStructuredOutputSchema(document.RootElement));
    }
}
