// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;

namespace Vercel.AI.Util;

/// <summary>Outcome of <see cref="PartialJson.Parse"/>.</summary>
public sealed class PartialJsonResult
{
    /// <summary>Creates a parse outcome.</summary>
    public PartialJsonResult(JsonNode? value, string state)
    {
        Value = value;
        State = state;
    }

    /// <summary>Parsed value, or <c>null</c> when parsing failed or the input was undefined.</summary>
    public JsonNode? Value { get; }

    /// <summary>
    /// <c>undefined-input</c>, <c>successful-parse</c>, <c>repaired-parse</c>, or <c>failed-parse</c>.
    /// </summary>
    public string State { get; }
}

/// <summary>Parses complete JSON, or JSON repaired by <see cref="FixJson"/>.</summary>
public static class PartialJson
{
    /// <summary>Input was null, the JavaScript <c>undefined</c> case.</summary>
    public const string UndefinedInput = "undefined-input";

    /// <summary>The original text parsed.</summary>
    public const string SuccessfulParse = "successful-parse";

    /// <summary>The original text failed and the repaired text parsed.</summary>
    public const string RepairedParse = "repaired-parse";

    /// <summary>Neither the original nor the repaired text parsed.</summary>
    public const string FailedParse = "failed-parse";

    /// <summary>
    /// Parses <paramref name="jsonText"/>. A null argument is undefined input and is not parsed.
    /// </summary>
    public static Task<PartialJsonResult> ParseAsync(string? jsonText)
    {
        return Task.FromResult(Parse(jsonText));
    }

    /// <summary>
    /// Parses <paramref name="jsonText"/>. A null argument is undefined input and is not parsed.
    /// </summary>
    public static PartialJsonResult Parse(string? jsonText)
    {
        if (jsonText == null)
        {
            return new PartialJsonResult(null, UndefinedInput);
        }

        JsonNode? parsed;
        if (TryParse(jsonText, out parsed))
        {
            return new PartialJsonResult(parsed, SuccessfulParse);
        }

        var repaired = FixJson.Repair(jsonText);
        if (TryParse(repaired, out parsed))
        {
            return new PartialJsonResult(parsed, RepairedParse);
        }

        return new PartialJsonResult(null, FailedParse);
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
