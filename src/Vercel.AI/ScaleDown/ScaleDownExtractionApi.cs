// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.ScaleDown;

/// <summary>A parsed ScaleDown chat-completions response. Maps to the output of <c>scaledownExtractionResponseSchema</c>.</summary>
internal sealed class ScaleDownExtractionResponse
{
    public string? Id { get; init; }

    public double? Created { get; init; }

    public string? Model { get; init; }

    public string Content { get; init; } = string.Empty;

    public int? PromptTokens { get; init; }

    public int? CompletionTokens { get; init; }
}

/// <summary>One entity of the extraction content.</summary>
internal sealed class ScaleDownEntity
{
    public string Text { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public double Confidence { get; init; }

    public int? Start { get; init; }

    public int? End { get; init; }
}

/// <summary>The parsed message content. Maps to the output of <c>scaledownExtractionContentSchema</c>.</summary>
internal sealed class ScaleDownExtractionContent
{
    public IReadOnlyList<ScaleDownEntity> Entities { get; init; } = [];

    public int? InputTokens { get; init; }

    public string? OcrText { get; init; }
}

/// <summary>Maps to <c>scaledownExtractionResponseSchema</c> and <c>scaledownExtractionContentSchema</c>. See https://docs.scaledown.ai/api-reference/extract.</summary>
internal static class ScaleDownExtractionApi
{
    /// <summary>Parses the response. Throws <see cref="FormatException"/> when it does not fit the schema.</summary>
    internal static ScaleDownExtractionResponse ParseResponse(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new FormatException("Expected an object.");
        }

        var choices = Property(root, "choices", JsonValueKind.Array);
        if (choices.GetArrayLength() < 1)
        {
            throw new FormatException("Expected at least one choice.");
        }

        foreach (var choice in choices.EnumerateArray())
        {
            if (
                choice.ValueKind != JsonValueKind.Object
                || Property(choice, "message", JsonValueKind.Object).ValueKind
                    != JsonValueKind.Object
            )
            {
                throw new FormatException("Invalid choice.");
            }

            Property(
                Property(choice, "message", JsonValueKind.Object),
                "content",
                JsonValueKind.String
            );
        }

        int? promptTokens = null;
        int? completionTokens = null;
        if (root.TryGetProperty("usage", out var usage) && usage.ValueKind != JsonValueKind.Null)
        {
            if (usage.ValueKind != JsonValueKind.Object)
            {
                throw new FormatException("Invalid usage.");
            }

            promptTokens = OptionalInt(usage, "prompt_tokens");
            completionTokens = OptionalInt(usage, "completion_tokens");
        }

        return new ScaleDownExtractionResponse
        {
            Id = OptionalString(root, "id"),
            Created = OptionalNumber(root, "created"),
            Model = OptionalString(root, "model"),
            Content = choices[0].GetProperty("message").GetProperty("content").GetString()!,
            PromptTokens = promptTokens,
            CompletionTokens = completionTokens,
        };
    }

    /// <summary>Parses the JSON text of the message content. Returns null when it does not fit the schema.</summary>
    internal static ScaleDownExtractionContent? ParseContent(string text)
    {
        try
        {
            using var document = JsonDocument.Parse(text);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var entities = new List<ScaleDownEntity>();
            foreach (var entity in Property(root, "entities", JsonValueKind.Array).EnumerateArray())
            {
                if (entity.ValueKind != JsonValueKind.Object)
                {
                    return null;
                }

                entities.Add(
                    new ScaleDownEntity
                    {
                        Text = Property(entity, "text", JsonValueKind.String).GetString()!,
                        Type = Property(entity, "type", JsonValueKind.String).GetString()!,
                        Confidence = Property(entity, "confidence", JsonValueKind.Number)
                            .GetDouble(),
                        Start = OptionalOffset(entity, "start"),
                        End = OptionalOffset(entity, "end"),
                    }
                );
            }

            return new ScaleDownExtractionContent
            {
                Entities = entities,
                InputTokens = OptionalInt(root, "input_tokens"),
                OcrText = OptionalString(root, "ocr_text"),
            };
        }
        catch (Exception exception)
            when (exception is JsonException
                || exception is FormatException
                || exception is InvalidOperationException
            )
        {
            return null;
        }
    }

    private static JsonElement Property(JsonElement element, string name, JsonValueKind kind)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind != kind)
        {
            throw new FormatException("Invalid " + name + ".");
        }

        return value;
    }

    private static string? OptionalString(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : throw new FormatException("Invalid " + name + ".");
    }

    private static double? OptionalNumber(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.Number
            ? value.GetDouble()
            : throw new FormatException("Invalid " + name + ".");
    }

    private static int? OptionalInt(JsonElement element, string name)
    {
        var number = OptionalNumber(element, name);
        return number == null ? null : (int)Math.Round(number.Value);
    }

    // z.number().int().nonnegative().nullish()
    private static int? OptionalOffset(JsonElement element, string name)
    {
        var number = OptionalNumber(element, name);
        if (number == null)
        {
            return null;
        }

        if (number < 0 || number != Math.Truncate(number.Value) || number > int.MaxValue)
        {
            throw new FormatException("Invalid " + name + ".");
        }

        return (int)number.Value;
    }
}
