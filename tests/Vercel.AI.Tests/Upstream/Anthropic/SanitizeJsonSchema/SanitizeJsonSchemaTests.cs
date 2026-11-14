// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;

namespace Vercel.AI.Tests.Upstream.Anthropic.SanitizeJsonSchema;

public sealed class SanitizeJsonSchemaTests
{
    [Fact]
    [UpstreamTest("packages/anthropic/src/sanitize-json-schema.test.ts::sanitizeJsonSchema::keeps definitions next to a top-level $ref", Coverage = UpstreamCoverage.Covered, Commit = "5700d1269e763c1f9397de3e60ee16321ed09dc3")]
    public void Keeps_definitions_next_to_a_top_level_ref()
    {
        using var schema = JsonDocument.Parse("{\"$ref\":\"#/definitions/Answer\",\"definitions\":{\"Answer\":{\"type\":\"object\",\"properties\":{\"answer\":{\"type\":\"string\",\"minLength\":1}},\"required\":[\"answer\"]}},\"$defs\":{\"Unused\":{\"type\":\"string\"}}}");

        var sanitized = AnthropicJsonSchema.Sanitize(schema.RootElement);

        var expected = JsonNode.Parse("{\"$defs\":{\"Unused\":{\"type\":\"string\"}},\"$ref\":\"#/definitions/Answer\",\"definitions\":{\"Answer\":{\"additionalProperties\":false,\"properties\":{\"answer\":{\"description\":\"min length: 1.\",\"type\":\"string\"}},\"required\":[\"answer\"],\"type\":\"object\"}}}");
        Assert.True(JsonNode.DeepEquals(expected, sanitized), sanitized.ToJsonString());
    }
}
