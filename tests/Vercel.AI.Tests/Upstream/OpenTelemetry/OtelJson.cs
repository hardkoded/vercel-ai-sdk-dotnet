// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry;

/// <summary>Compares a value with the JSON of an upstream inline snapshot. Property order does not matter, like in the snapshot.</summary>
internal static class OtelJson
{
    public static void Equal(string expectedJson, object? actual)
    {
        var expected = JsonNode.Parse(expectedJson);
        var actualNode = actual switch
        {
            string text => JsonNode.Parse(text),
            JsonNode node => node.DeepClone(),
            _ => ToNode(actual),
        };
        Assert.True(
            JsonNode.DeepEquals(expected, actualNode),
            "Expected: " + expected?.ToJsonString() + "\nActual:   " + actualNode?.ToJsonString()
        );
    }

    private static JsonNode? ToNode(object? value) =>
        value is null ? null : JsonSerializer.SerializeToNode(value);
}
