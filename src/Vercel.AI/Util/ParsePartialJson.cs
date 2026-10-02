// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;

namespace Vercel.AI.Util;

/// <summary>The outcome of <see cref="JsonRepair.ParsePartialJson"/>.</summary>
public sealed class PartialJsonResult
{
    /// <summary>Creates a parse outcome.</summary>
    public PartialJsonResult(JsonNode? value, string state)
    {
        Value = value;
        State = state;
    }

    /// <summary>The parsed value, or <c>null</c> when parsing failed or the input was missing.</summary>
    public JsonNode? Value { get; }

    /// <summary>
    /// One of <c>undefined-input</c>, <c>successful-parse</c>, <c>repaired-parse</c>, or <c>failed-parse</c>.
    /// </summary>
    public string State { get; }
}

/// <summary>Partial JSON parsing. Maps to <c>parsePartialJson</c>.</summary>
public static partial class JsonRepair
{
    /// <summary>
    /// Parses <paramref name="jsonText"/>, repairing a truncated payload when the first parse fails.
    /// A null input maps to the upstream <c>undefined</c> input.
    /// </summary>
    public static PartialJsonResult ParsePartialJson(string? jsonText)
    {
        if (jsonText is null)
        {
            return new PartialJsonResult(null, "undefined-input");
        }

        if (TryParse(jsonText, out var value))
        {
            return new PartialJsonResult(value, "successful-parse");
        }

        var repaired = FixJson(jsonText);
        if (TryParse(repaired, out value))
        {
            return new PartialJsonResult(value, "repaired-parse");
        }

        return new PartialJsonResult(null, "failed-parse");
    }

    private static bool TryParse(string text, out JsonNode? value)
    {
        try
        {
            value = JsonNode.Parse(text);
            return true;
        }
        catch (System.Text.Json.JsonException)
        {
            value = null;
            return false;
        }
    }
}
