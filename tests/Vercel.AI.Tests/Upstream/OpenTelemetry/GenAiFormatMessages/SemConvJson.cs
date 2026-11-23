// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenTelemetry;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.GenAiFormatMessages;

/// <summary>Compares formatted SemConv values with expected JSON, ignoring property order like an inline snapshot does.</summary>
internal static class SemConvJson
{
    public static void AssertEqual(string expectedJson, IEnumerable<SemConvInputMessage> actual) =>
        AssertNodes(expectedJson, actual.Select(m => m.ToJsonObject()));

    public static void AssertEqual(string expectedJson, IEnumerable<SemConvOutputMessage> actual) =>
        AssertNodes(expectedJson, actual.Select(m => m.ToJsonObject()));

    public static void AssertEqual(
        string expectedJson,
        IEnumerable<SemConvSystemInstruction> actual
    ) => AssertNodes(expectedJson, actual.Select(m => m.ToJsonObject()));

    public static void AssertParts(string expectedJson, IEnumerable<JsonObject> parts) =>
        AssertNodes(expectedJson, parts);

    private static void AssertNodes(string expectedJson, IEnumerable<JsonObject> actual)
    {
        var actualNode = new JsonArray(actual.Select(n => (JsonNode?)n.DeepClone()).ToArray());
        var expectedNode = JsonNode.Parse(expectedJson);
        Assert.True(
            JsonNode.DeepEquals(expectedNode, actualNode),
            "Expected: " + expectedNode!.ToJsonString() + "\nActual:   " + actualNode.ToJsonString()
        );
    }
}
