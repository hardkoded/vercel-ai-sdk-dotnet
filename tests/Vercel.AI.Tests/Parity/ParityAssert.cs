// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;

namespace Vercel.AI.Tests;

internal static class ParityAssert
{
    public static void JsonEqual(JsonNode? actual, string expected)
    {
        var parsed = JsonNode.Parse(expected);
        var normalized = actual == null ? null : JsonNode.Parse(actual.ToJsonString());
        Assert.True(
            JsonNode.DeepEquals(parsed, normalized),
            "expected " + expected + " but was " + (actual == null ? "null" : actual.ToJsonString()));
    }
}
