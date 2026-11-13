// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.Operations;

namespace Vercel.AI.OpenAI;

/// <summary>Text format accepted by <c>textFormat</c> and <c>textFormatUpdate</c>. Mirrors <c>openai-responses-text-format.ts</c>.</summary>
internal static class OpenAIResponsesTextFormat
{
    private static readonly Regex s_name = new("^[a-zA-Z0-9_-]+\\z", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    /// <summary>
    /// Validates a <c>text</c> or <c>json_schema</c> format (<c>openaiResponsesTextFormatSchema</c>) and returns the wire
    /// format with its schema normalized (<c>normalizeOpenAIResponsesTextFormat</c>). The input is not modified.
    /// </summary>
    internal static JsonObject Normalize(JsonElement format, List<OpenAICallWarning> warnings)
    {
        var type = OpenAIJson.String(format, "type");
        if (format.ValueKind != JsonValueKind.Object || type == "text")
        {
            return type == "text" ? new JsonObject { ["type"] = "text" } : throw Invalid(format);
        }

        var name = OpenAIJson.String(format, "name");
        var schema = OpenAIJson.Child(format, "schema");
        var description = OpenAIJson.Child(format, "description");
        var strict = OpenAIJson.Child(format, "strict");
        if (type != "json_schema"
            || name is not { Length: >= 1 and <= 64 }
            || !s_name.IsMatch(name)
            || schema is not { ValueKind: JsonValueKind.Object }
            || description is { } rawDescription && rawDescription.ValueKind != JsonValueKind.String
            || strict is { } rawStrict && rawStrict.ValueKind != JsonValueKind.True && rawStrict.ValueKind != JsonValueKind.False)
        {
            throw Invalid(format);
        }

        var normalized = OpenAIJsonSchema.Normalize(schema.Value);
        warnings.AddRange(normalized.Warnings);
        var result = new JsonObject { ["type"] = "json_schema", ["name"] = name, ["schema"] = normalized.Schema };
        if (description is { } descriptionValue)
        {
            result["description"] = descriptionValue.GetString();
        }

        if (strict is { } strictValue)
        {
            result["strict"] = strictValue.GetBoolean();
        }

        return result;
    }

    private static InvalidArgumentException Invalid(JsonElement format) =>
        new("providerOptions", format.GetRawText(), "invalid openai provider options");
}
