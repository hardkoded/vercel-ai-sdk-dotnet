// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.ScaleDown;

/// <summary>One answer of a ScaleDown response. Maps to a member of <c>scaledownDecisionResponseSchema</c>.</summary>
internal sealed class ScaleDownAnswer
{
    public string Type { get; init; } = string.Empty;

    public double? Noul { get; init; }

    public string? Choice { get; init; }

    public double? Score { get; init; }

    public IReadOnlyDictionary<string, double>? Probabilities { get; init; }

    public double? Confidence { get; init; }

    public IReadOnlyDictionary<string, string>? Legend { get; init; }
}

/// <summary>A parsed ScaleDown response. Maps to the output of <c>scaledownDecisionResponseSchema</c>.</summary>
internal sealed class ScaleDownDecisionResponse
{
    public string? Model { get; init; }

    public IReadOnlyDictionary<string, ScaleDownAnswer> Answers { get; init; } = new Dictionary<string, ScaleDownAnswer>();

    public bool HasUsage { get; init; }

    public int? InputTokens { get; init; }

    public int? OutputTokens { get; init; }

    public double? Cost { get; init; }
}

/// <summary>Maps to <c>scaledownDecisionResponseSchema</c> (OpenAPI 1.0.0 of the ScaleDown API).</summary>
internal static class ScaleDownDecisionApi
{
    /// <summary>Parses the response. Throws <see cref="FormatException"/> when it does not fit the schema.</summary>
    internal static ScaleDownDecisionResponse Parse(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new FormatException("Expected an object.");
        }

        var answers = new Dictionary<string, ScaleDownAnswer>();
        foreach (var pair in Property(root, "answers", JsonValueKind.Object).EnumerateObject())
        {
            if (pair.Value.ValueKind != JsonValueKind.Object)
            {
                throw new FormatException("Invalid answer.");
            }

            var answer = pair.Value;
            switch (Property(answer, "type", JsonValueKind.String).GetString())
            {
                case "noul":
                    answers[pair.Name] = new ScaleDownAnswer { Type = "noul", Noul = Property(answer, "noul", JsonValueKind.Number).GetDouble() };
                    break;
                case "choice":
                    answers[pair.Name] = new ScaleDownAnswer
                    {
                        Type = "choice",
                        Choice = Property(answer, "choice", JsonValueKind.String).GetString(),
                        Probabilities = Numbers(Property(answer, "probabilities", JsonValueKind.Object)),
                        Confidence = Optional(answer, "confidence"),
                    };
                    break;
                case "score":
                    answers[pair.Name] = new ScaleDownAnswer
                    {
                        Type = "score",
                        Score = Property(answer, "score", JsonValueKind.Number).GetDouble(),
                        Probabilities = Numbers(Property(answer, "probabilities", JsonValueKind.Object)),
                        Confidence = Optional(answer, "confidence"),
                        Legend = Legend(answer),
                    };
                    break;
                default:
                    throw new FormatException("Unknown answer type.");
            }
        }

        string? model = null;
        if (IsPresent(root, "model"))
        {
            model = Property(root, "model", JsonValueKind.String).GetString();
        }

        var hasUsage = false;
        int? inputTokens = null;
        int? outputTokens = null;
        double? cost = null;
        if (IsPresent(root, "usage"))
        {
            var usage = Property(root, "usage", JsonValueKind.Object);
            hasUsage = true;
            inputTokens = (int?)Optional(usage, "input_tokens");
            outputTokens = (int?)Optional(usage, "output_tokens");
            cost = Optional(usage, "cost");
        }

        return new ScaleDownDecisionResponse { Model = model, Answers = answers, HasUsage = hasUsage, InputTokens = inputTokens, OutputTokens = outputTokens, Cost = cost };
    }

    private static bool IsPresent(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null;
    }

    private static JsonElement Property(JsonElement element, string name, JsonValueKind kind)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind != kind)
        {
            throw new FormatException("Invalid " + name + ".");
        }

        return value;
    }

    private static double? Optional(JsonElement element, string name)
    {
        return IsPresent(element, name) ? Property(element, name, JsonValueKind.Number).GetDouble() : null;
    }

    private static Dictionary<string, double> Numbers(JsonElement map)
    {
        var values = new Dictionary<string, double>();
        foreach (var item in map.EnumerateObject())
        {
            if (item.Value.ValueKind != JsonValueKind.Number)
            {
                throw new FormatException("Invalid probability.");
            }

            values[item.Name] = item.Value.GetDouble();
        }

        return values;
    }

    private static Dictionary<string, string>? Legend(JsonElement answer)
    {
        if (!IsPresent(answer, "legend"))
        {
            return null;
        }

        var values = new Dictionary<string, string>();
        foreach (var item in Property(answer, "legend", JsonValueKind.Object).EnumerateObject())
        {
            if (item.Value.ValueKind != JsonValueKind.String)
            {
                throw new FormatException("Invalid legend.");
            }

            values[item.Name] = item.Value.GetString()!;
        }

        return values;
    }
}
