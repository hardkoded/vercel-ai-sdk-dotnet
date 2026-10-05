// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Tests.Upstream;

internal static class JsonAssert
{
    public static void Equal(JsonNode? actual, string expected)
    {
        var expectedNode = JsonNode.Parse(expected);
        Assert.True(
            JsonNode.DeepEquals(actual, expectedNode),
            (actual == null ? "null" : actual.ToJsonString()) + " != " + expected);
    }

    public static void Equal(JsonElement actual, string expected)
    {
        using var document = JsonDocument.Parse(expected);
        Assert.True(
            JsonElement.DeepEquals(actual, document.RootElement),
            actual.GetRawText() + " != " + expected);
    }

    public static JsonObject ParseObject(string json)
    {
        return JsonNode.Parse(json)!.AsObject();
    }
}
