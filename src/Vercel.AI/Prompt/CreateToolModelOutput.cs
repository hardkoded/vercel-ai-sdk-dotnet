// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Prompt;

/// <summary>A value that supplies its own JSON form, matching JavaScript <c>toJSON</c>.</summary>
public interface IToolJsonValue
{
    /// <summary>Returns the JSON-ready value.</summary>
    object? ToJson();
}

/// <summary>Arguments passed to <c>toModelOutput</c>.</summary>
public sealed class ToolModelOutputCall
{
    /// <summary>Creates the call.</summary>
    public ToolModelOutputCall(string toolCallId, JsonElement? input, object? output)
    {
        ToolCallId = toolCallId;
        Input = input;
        Output = output;
    }

    /// <summary>Tool call id.</summary>
    public string ToolCallId { get; }

    /// <summary>Tool input.</summary>
    public JsonElement? Input { get; }

    /// <summary>Tool output.</summary>
    public object? Output { get; }
}

/// <summary>A tool result sent back to the model. Maps to <c>ToolResultOutput</c>.</summary>
public abstract class ToolModelOutput
{
    /// <summary>Creates an output.</summary>
    protected ToolModelOutput(string type)
    {
        Type = type;
    }

    /// <summary>Output type.</summary>
    public string Type { get; }
}

/// <summary>Text tool output.</summary>
public sealed class TextToolModelOutput : ToolModelOutput
{
    /// <summary>Creates text output.</summary>
    public TextToolModelOutput(string value)
        : base("text")
    {
        Value = value ?? string.Empty;
    }

    /// <summary>Text.</summary>
    public string Value { get; }
}

/// <summary>JSON tool output.</summary>
public sealed class JsonToolModelOutput : ToolModelOutput
{
    /// <summary>Creates JSON output.</summary>
    public JsonToolModelOutput(JsonElement value)
        : base("json")
    {
        Value = value;
    }

    /// <summary>JSON value.</summary>
    public JsonElement Value { get; }
}

/// <summary>Error text tool output.</summary>
public sealed class ErrorTextToolModelOutput : ToolModelOutput
{
    /// <summary>Creates error text.</summary>
    public ErrorTextToolModelOutput(string value)
        : base("error-text")
    {
        Value = value ?? string.Empty;
    }

    /// <summary>Error text.</summary>
    public string Value { get; }
}

/// <summary>Error JSON tool output.</summary>
public sealed class ErrorJsonToolModelOutput : ToolModelOutput
{
    /// <summary>Creates error JSON.</summary>
    public ErrorJsonToolModelOutput(JsonElement value)
        : base("error-json")
    {
        Value = value;
    }

    /// <summary>Error value.</summary>
    public JsonElement Value { get; }
}

/// <summary>One content part inside a content tool output.</summary>
public sealed class ToolModelContentPart
{
    /// <summary>Creates a text part.</summary>
    public ToolModelContentPart(string type, string text)
    {
        Type = type;
        Text = text ?? string.Empty;
    }

    /// <summary>Part type.</summary>
    public string Type { get; }

    /// <summary>Part text.</summary>
    public string Text { get; }
}

/// <summary>Multi-part tool output.</summary>
public sealed class ContentToolModelOutput : ToolModelOutput
{
    /// <summary>Creates content output.</summary>
    public ContentToolModelOutput(IReadOnlyList<ToolModelContentPart> value)
        : base("content")
    {
        Value = value ?? Array.Empty<ToolModelContentPart>();
    }

    /// <summary>Content parts.</summary>
    public IReadOnlyList<ToolModelContentPart> Value { get; }
}

/// <summary>Builds the model-facing tool result. Maps to <c>createToolModelOutput</c>.</summary>
public static class ToolOutputs
{
    private static readonly JsonSerializerOptions Serializer = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>
    /// Converts a tool output. <paramref name="errorMode"/> is <c>text</c>, <c>json</c>, or <c>none</c>.
    /// Set <paramref name="hasOutput"/> to false when the output is undefined.
    /// </summary>
    public static ToolModelOutput CreateToolModelOutput(
        string toolCallId,
        JsonElement? input,
        object? output,
        bool hasOutput,
        Func<ToolModelOutputCall, ToolModelOutput>? toModelOutput,
        string errorMode)
    {
        var value = hasOutput ? output : null;
        if (string.Equals(errorMode, "text", StringComparison.Ordinal))
        {
            return new ErrorTextToolModelOutput(ErrorMessage(hasOutput ? output : Undefined.Value));
        }

        if (string.Equals(errorMode, "json", StringComparison.Ordinal))
        {
            return new ErrorJsonToolModelOutput(ToJsonValue(hasOutput ? output : null, hasOutput));
        }

        if (toModelOutput != null)
        {
            return toModelOutput(new ToolModelOutputCall(toolCallId, input, value));
        }

        if (hasOutput && output is string text)
        {
            return new TextToolModelOutput(text);
        }

        return new JsonToolModelOutput(ToJsonValue(output, hasOutput));
    }

    private static string ErrorMessage(object? error)
    {
        if (error is null || ReferenceEquals(error, Undefined.Value))
        {
            return "unknown error";
        }

        if (error is string text)
        {
            return text;
        }

        if (error is Exception exception)
        {
            return exception.ToString();
        }

        return JsonSerializer.Serialize(ToNode(error, true), Serializer);
    }

    private static JsonElement ToJsonValue(object? value, bool hasOutput)
    {
        if (!hasOutput || value is null)
        {
            using var document = JsonDocument.Parse("null");
            return document.RootElement.Clone();
        }

        return JsonSerializer.SerializeToElement(ToNode(value, true), Serializer);
    }

    private static JsonNode? ToNode(object? value, bool hasOutput)
    {
        if (!hasOutput || value is null || ReferenceEquals(value, Undefined.Value))
        {
            return null;
        }

        if (value is IToolJsonValue custom)
        {
            return ToNode(custom.ToJson(), true);
        }

        if (value is JsonElement element)
        {
            return JsonNode.Parse(element.GetRawText());
        }

        if (value is JsonNode node)
        {
            return node;
        }

        if (value is IReadOnlyDictionary<string, object?> map)
        {
            var obj = new JsonObject();
            foreach (var pair in map)
            {
                if (ReferenceEquals(pair.Value, Undefined.Value))
                {
                    continue;
                }

                obj[pair.Key] = ToNode(pair.Value, true);
            }

            return obj;
        }

        return value switch
        {
            string text => JsonValue.Create(text),
            bool flag => JsonValue.Create(flag),
            byte number => JsonValue.Create(number),
            short number => JsonValue.Create(number),
            int number => JsonValue.Create(number),
            long number => JsonValue.Create(number),
            float number => JsonValue.Create(number),
            double number => JsonValue.Create(number),
            decimal number => JsonValue.Create(number),
            _ => JsonNode.Parse(JsonSerializer.Serialize(value, Serializer)),
        };
    }

    /// <summary>Sentinel for an undefined tool output.</summary>
    public static class Undefined
    {
        /// <summary>The undefined output value.</summary>
        public static readonly object Value = new();
    }
}
