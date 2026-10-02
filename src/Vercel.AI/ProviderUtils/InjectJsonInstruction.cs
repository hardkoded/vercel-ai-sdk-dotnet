// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.ProviderUtils;

/// <summary>Inputs for <c>injectJsonInstruction</c>. Unset flags mean the argument was omitted.</summary>
public sealed class JsonInstruction
{
    /// <summary>Prompt text. Null is an explicit null when <see cref="HasPrompt"/> is true.</summary>
    public string? Prompt { get; set; }

    /// <summary>True when prompt was passed, including an explicit null.</summary>
    public bool HasPrompt { get; set; }

    /// <summary>JSON schema.</summary>
    public JsonNode? Schema { get; set; }

    /// <summary>True when a schema argument was passed.</summary>
    public bool HasSchema { get; set; }

    /// <summary>Schema prefix. Null is explicit when <see cref="HasSchemaPrefix"/> is true.</summary>
    public string? SchemaPrefix { get; set; }

    /// <summary>True when the prefix argument was passed.</summary>
    public bool HasSchemaPrefix { get; set; }

    /// <summary>Schema suffix. Null is explicit when <see cref="HasSchemaSuffix"/> is true.</summary>
    public string? SchemaSuffix { get; set; }

    /// <summary>True when the suffix argument was passed.</summary>
    public bool HasSchemaSuffix { get; set; }
}

/// <summary>A prompt message. Content is a string or a <see cref="JsonNode"/>.</summary>
public sealed class InstructionMessage
{
    /// <summary>Creates a message.</summary>
    public InstructionMessage(string role, object? content)
    {
        Role = role ?? throw new ArgumentNullException(nameof(role));
        Content = content;
    }

    /// <summary>Message role.</summary>
    public string Role { get; }

    /// <summary>Message content.</summary>
    public object? Content { get; set; }
}

/// <summary>Injects JSON instructions into prompts. Maps to <c>injectJsonInstruction</c>.</summary>
public static class JsonInstructions
{
    private const string DefaultSchemaPrefix = "JSON schema:";
    private const string DefaultSchemaSuffix = "You MUST answer with a JSON object that matches the JSON schema above.";
    private const string DefaultGenericSuffix = "You MUST answer with JSON.";

    private static readonly JsonSerializerOptions Stringify = new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>Builds the instruction text.</summary>
    public static string InjectJsonInstruction(JsonInstruction instruction)
    {
        if (instruction is null)
        {
            throw new ArgumentNullException(nameof(instruction));
        }

        var schema = instruction.HasSchema ? instruction.Schema : null;
        var schemaPresent = instruction.HasSchema && schema != null;
        var prefix = instruction.HasSchemaPrefix
            ? instruction.SchemaPrefix
            : schemaPresent ? DefaultSchemaPrefix : null;
        var suffix = instruction.HasSchemaSuffix
            ? instruction.SchemaSuffix
            : schemaPresent ? DefaultSchemaSuffix : DefaultGenericSuffix;
        var prompt = instruction.HasPrompt ? instruction.Prompt : null;
        var lines = new List<string?>();
        if (prompt != null && prompt.Length > 0)
        {
            lines.Add(prompt);
            lines.Add(string.Empty);
        }

        if (prefix != null)
        {
            lines.Add(prefix);
        }

        if (schema != null)
        {
            lines.Add(DecodeUnicodeEscapes(schema.ToJsonString(Stringify)));
        }

        if (suffix != null)
        {
            lines.Add(suffix);
        }

        return string.Join("\n", lines);
    }

    private static string DecodeUnicodeEscapes(string json)
    {
        var builder = new StringBuilder(json.Length);
        for (var i = 0; i < json.Length; i++)
        {
            if (json[i] == '\\' && i + 5 < json.Length && json[i + 1] == 'u'
                && int.TryParse(json.Substring(i + 2, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var code))
            {
                builder.Append((char)code);
                i += 5;
                continue;
            }

            builder.Append(json[i]);
        }

        return builder.ToString();
    }

    /// <summary>Injects the instruction into the system message, copying the message list.</summary>
    public static IReadOnlyList<InstructionMessage> InjectJsonInstructionIntoMessages(
        IReadOnlyList<InstructionMessage> messages,
        JsonInstruction instruction)
    {
        if (messages is null)
        {
            throw new ArgumentNullException(nameof(messages));
        }

        var hasSystem = messages.Count > 0 && messages[0].Role == "system";
        var systemContent = hasSystem && messages[0].Content is string text ? text : string.Empty;
        var promptInstruction = new JsonInstruction
        {
            HasPrompt = true,
            Prompt = systemContent,
            HasSchema = instruction.HasSchema,
            Schema = instruction.Schema,
            HasSchemaPrefix = instruction.HasSchemaPrefix,
            SchemaPrefix = instruction.SchemaPrefix,
            HasSchemaSuffix = instruction.HasSchemaSuffix,
            SchemaSuffix = instruction.SchemaSuffix,
        };
        var system = new InstructionMessage("system", InjectJsonInstruction(promptInstruction));
        var result = new List<InstructionMessage> { system };
        var start = hasSystem ? 1 : 0;
        for (var i = start; i < messages.Count; i++)
        {
            result.Add(messages[i]);
        }

        return result;
    }
}
