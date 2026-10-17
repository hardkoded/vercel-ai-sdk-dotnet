// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.GenerateText;

/// <summary>A tool call after its input was parsed and validated. Maps to a <c>TypedToolCall</c>.</summary>
public sealed class ParsedToolCall
{
    /// <summary>Creates a parsed tool call.</summary>
    public ParsedToolCall(string toolCallId, string toolName, JsonElement input)
    {
        ToolCallId = toolCallId;
        ToolName = toolName;
        Input = input;
    }

    /// <summary>Provider id for this call.</summary>
    public string ToolCallId { get; }

    /// <summary>Tool name.</summary>
    public string ToolName { get; }

    /// <summary>The parsed input. When <see cref="Invalid"/> is true this is the raw input, parsed when it is valid JSON.</summary>
    public JsonElement Input { get; }

    /// <summary>True when the input did not parse or did not match the tool's input schema.</summary>
    public bool Invalid { get; init; }

    /// <summary>Why the call is invalid, such as an <see cref="InvalidToolInputError"/>.</summary>
    public Exception? Error { get; init; }

    /// <summary>True when the provider executed the tool.</summary>
    public bool ProviderExecuted { get; init; }

    /// <summary>Provider-specific metadata of the call.</summary>
    public JsonElement? ProviderMetadata { get; init; }
}

/// <summary>Parses the input of a tool call and validates it against the tool's input schema. Maps to <c>parseToolCall</c>.</summary>
public static class ToolCallParser
{
    /// <summary>
    /// Parses <paramref name="toolCall"/>. Empty input is validated as an empty object. A call whose input is not valid JSON
    /// or does not match the input schema of its tool comes back with <see cref="ParsedToolCall.Invalid"/> set and an
    /// <see cref="InvalidToolInputError"/> in <see cref="ParsedToolCall.Error"/>.
    /// </summary>
    public static ParsedToolCall ParseToolCall(GeneratedToolCall toolCall, IReadOnlyList<Tool>? tools)
    {
        if (toolCall == null)
        {
            throw new ArgumentNullException(nameof(toolCall));
        }

        Tool? tool = null;
        if (tools != null)
        {
            foreach (var candidate in tools)
            {
                if (string.Equals(candidate.Name, toolCall.ToolName, StringComparison.Ordinal))
                {
                    tool = candidate;
                    break;
                }
            }
        }

        var text = toolCall.ArgumentsJson;
        var empty = string.IsNullOrWhiteSpace(text);
        JsonElement input;
        Exception? failure = null;

        try
        {
            using var document = JsonDocument.Parse(empty ? "{}" : text);
            input = document.RootElement.Clone();
        }
        catch (JsonException exception)
        {
            return Invalid(toolCall, JsonDocumentOrString(text), new InvalidToolInputError(toolCall.ToolName, text, exception));
        }

        if (tool != null && !tool.IsProviderDefined)
        {
            var errors = ToolInputSchemaValidator.Validate(tool.InputSchema, input);
            if (errors.Count > 0)
            {
                failure = new InvalidToolInputError(
                    toolCall.ToolName,
                    text,
                    new FormatException("Type validation failed: Value: " + input.GetRawText() + ".\nError message: " + string.Join("; ", errors)));
            }
        }

        if (failure != null)
        {
            return Invalid(toolCall, input, failure);
        }

        return new ParsedToolCall(toolCall.ToolCallId, toolCall.ToolName, input)
        {
            ProviderExecuted = toolCall.ProviderExecuted,
            ProviderMetadata = toolCall.ProviderMetadata,
        };
    }

    private static ParsedToolCall Invalid(GeneratedToolCall toolCall, JsonElement input, Exception error) =>
        new ParsedToolCall(toolCall.ToolCallId, toolCall.ToolName, input)
        {
            Invalid = true,
            Error = error,
            ProviderExecuted = toolCall.ProviderExecuted,
            ProviderMetadata = toolCall.ProviderMetadata,
        };

    private static JsonElement JsonDocumentOrString(string text)
    {
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(text));
        return document.RootElement.Clone();
    }
}
