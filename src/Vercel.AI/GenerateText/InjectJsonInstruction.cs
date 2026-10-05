// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.GenerateText;

/// <summary>An optional string that can be omitted, null, or set. Distinguishes a missing argument from null.</summary>
public readonly struct JsonInstructionText
{
    private JsonInstructionText(bool isSet, string? value)
    {
        IsSet = isSet;
        Value = value;
    }

    /// <summary>True when the caller passed a value, including null.</summary>
    public bool IsSet { get; }

    /// <summary>The text, when <see cref="IsSet"/> is true.</summary>
    public string? Value { get; }

    /// <summary>The argument was omitted.</summary>
    public static JsonInstructionText Unset => default;

    /// <summary>The argument was passed as null.</summary>
    public static JsonInstructionText Null { get; } = new(true, null);

    /// <summary>The argument was passed as <paramref name="value"/>.</summary>
    public static JsonInstructionText From(string? value)
    {
        return new JsonInstructionText(true, value);
    }
}

/// <summary>JSON output instructions. Maps to <c>injectJsonInstruction</c>.</summary>
public static class JsonInstructions
{
    private const string SchemaPrefix = "JSON schema:";
    private const string SchemaSuffix = "You MUST answer with a JSON object that matches the JSON schema above.";
    private const string GenericSuffix = "You MUST answer with JSON.";

    /// <summary>Builds the instruction text appended to a prompt.</summary>
    public static string InjectJsonInstruction(
        string? prompt = null,
        JsonElement? schema = null,
        JsonInstructionText schemaPrefix = default,
        JsonInstructionText schemaSuffix = default)
    {
        var prefix = schemaPrefix.IsSet ? schemaPrefix.Value : schema.HasValue ? SchemaPrefix : null;
        var suffix = schemaSuffix.IsSet ? schemaSuffix.Value : schema.HasValue ? SchemaSuffix : GenericSuffix;
        var lines = new List<string>();
        if (prompt != null && prompt.Length > 0)
        {
            lines.Add(prompt);
            lines.Add(string.Empty);
        }

        if (prefix != null)
        {
            lines.Add(prefix);
        }

        if (schema.HasValue)
        {
            lines.Add(schema.Value.GetRawText());
        }

        if (suffix != null)
        {
            lines.Add(suffix);
        }

        return string.Join("\n", lines);
    }
}
