// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Moonshot;

namespace Vercel.AI.Tests.Upstream.Providers.NormalizeJsonSchemaForMfjs;

/// <summary>Helpers shared by the <c>normalize-json-schema-for-mfjs.test.ts</c> ports.</summary>
internal static class NormalizeJsonSchemaForMfjsSupport
{
    internal const string TestFile = "packages/moonshotai/src/normalize-json-schema-for-mfjs.test.ts";

    internal const string RootMessage = "tool and response schemas must be a JSON Schema object with type \"object\" for moonshotai (MFJS)";

    internal static JsonNode Json(string json)
    {
        return JsonNode.Parse(json)!;
    }

    internal static JsonNode? Normalize(JsonNode? schema)
    {
        return MoonshotJsonSchema.NormalizeJsonSchemaForMFJS(schema);
    }

    internal static void AssertEqual(JsonNode expected, JsonNode? actual)
    {
        Assert.True(JsonNode.DeepEquals(expected, actual), "Expected " + expected.ToJsonString() + " but got " + actual?.ToJsonString());
    }
}
