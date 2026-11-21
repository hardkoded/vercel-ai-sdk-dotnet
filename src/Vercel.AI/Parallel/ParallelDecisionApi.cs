// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.Parallel;

/// <summary>One answer of a Parallel response. Maps to a member of <c>parallelDecisionResponseSchema</c>.</summary>
internal sealed class ParallelAnswer
{
    public string Type { get; init; } = string.Empty;

    public double? Noul { get; init; }

    public string? Choice { get; init; }

    public double? Score { get; init; }

    public IReadOnlyDictionary<string, double>? Probabilities { get; init; }

    public double? Confidence { get; init; }

    public IReadOnlyDictionary<string, string>? Legend { get; init; }
}

/// <summary>A parsed Parallel response. Maps to the output of <c>parallelDecisionResponseSchema</c>.</summary>
internal sealed class ParallelDecisionResponse
{
    public IReadOnlyDictionary<string, ParallelAnswer> Answers { get; init; } =
        new Dictionary<string, ParallelAnswer>();

    public JsonElement Citations { get; init; }

    public string DecisionId { get; init; } = string.Empty;

    public string Mode { get; init; } = string.Empty;

    public int InputTokens { get; init; }

    public int OutputTokens { get; init; }
}

/// <summary>Maps to <c>parallelDecisionResponseSchema</c> (https://docs.parallel.ai/api-reference/decisions/make-web-grounded-typed-decisions).</summary>
internal static class ParallelDecisionApi
{
    /// <summary>Parses the response. Throws <see cref="FormatException"/> when it does not fit the schema.</summary>
    internal static ParallelDecisionResponse Parse(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new FormatException("Expected an object.");
        }

        var answers = new Dictionary<string, ParallelAnswer>();
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
                    answers[pair.Name] = new ParallelAnswer
                    {
                        Type = "noul",
                        Noul = Probability(Property(answer, "noul", JsonValueKind.Number)),
                    };
                    break;
                case "choice":
                    answers[pair.Name] = new ParallelAnswer
                    {
                        Type = "choice",
                        Choice = Property(answer, "choice", JsonValueKind.String).GetString(),
                        Probabilities = Probabilities(
                            Property(answer, "probabilities", JsonValueKind.Object)
                        ),
                        Confidence = OptionalProbability(answer, "confidence"),
                    };
                    break;
                case "score":
                    answers[pair.Name] = new ParallelAnswer
                    {
                        Type = "score",
                        Score = Property(answer, "score", JsonValueKind.Number).GetDouble(),
                        Probabilities = Probabilities(
                            Property(answer, "probabilities", JsonValueKind.Object)
                        ),
                        Legend = Legend(Property(answer, "legend", JsonValueKind.Object)),
                        Confidence = OptionalProbability(answer, "confidence"),
                    };
                    break;
                case "refusal":
                    answers[pair.Name] = new ParallelAnswer { Type = "refusal" };
                    break;
                default:
                    throw new FormatException("Unknown answer type.");
            }
        }

        var citations = Property(root, "citations", JsonValueKind.Array);
        foreach (var citation in citations.EnumerateArray())
        {
            if (citation.ValueKind != JsonValueKind.Object)
            {
                throw new FormatException("Invalid citation.");
            }

            Property(citation, "url", JsonValueKind.String);
            if (
                citation.TryGetProperty("title", out var title)
                && title.ValueKind is not (JsonValueKind.String or JsonValueKind.Null)
            )
            {
                throw new FormatException("Invalid title.");
            }

            foreach (
                var excerpt in Property(citation, "excerpts", JsonValueKind.Array).EnumerateArray()
            )
            {
                if (excerpt.ValueKind != JsonValueKind.String)
                {
                    throw new FormatException("Invalid excerpt.");
                }
            }
        }

        var usage = Property(root, "usage", JsonValueKind.Object);
        // z.number().int().nonnegative(): judge the numeric value, not its JSON spelling (4212.0, 4.212e3).
        var input = Integer(Property(usage, "input_tokens", JsonValueKind.Number));
        if (input < 0)
        {
            throw new FormatException("Invalid input_tokens.");
        }

        // z.literal(0)
        var output = Integer(Property(usage, "output_tokens", JsonValueKind.Number));
        if (output != 0)
        {
            throw new FormatException("Invalid output_tokens.");
        }

        return new ParallelDecisionResponse
        {
            Answers = answers,
            Citations = citations.Clone(),
            DecisionId = Property(root, "decision_id", JsonValueKind.String).GetString()!,
            Mode = Property(root, "mode", JsonValueKind.String).GetString()!,
            InputTokens = input,
            OutputTokens = output,
        };
    }

    private static JsonElement Property(JsonElement element, string name, JsonValueKind kind)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind != kind)
        {
            throw new FormatException("Invalid " + name + ".");
        }

        return value;
    }

    private static int Integer(JsonElement value)
    {
        var number = value.GetDouble();
        if (number != Math.Floor(number) || number < int.MinValue || number > int.MaxValue)
        {
            throw new FormatException("Expected an integer.");
        }

        return (int)number;
    }

    // z.number().min(0).max(1)
    private static double Probability(JsonElement value)
    {
        var number = value.GetDouble();
        if (number < 0 || number > 1)
        {
            throw new FormatException("Probability out of range.");
        }

        return number;
    }

    // probability.nullish()
    private static double? OptionalProbability(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null
            ? Probability(Property(element, name, JsonValueKind.Number))
            : null;
    }

    private static Dictionary<string, double> Probabilities(JsonElement map)
    {
        var values = new Dictionary<string, double>();
        foreach (var item in map.EnumerateObject())
        {
            if (item.Value.ValueKind != JsonValueKind.Number)
            {
                throw new FormatException("Invalid probability.");
            }

            values[item.Name] = Probability(item.Value);
        }

        return values;
    }

    private static Dictionary<string, string> Legend(JsonElement map)
    {
        var values = new Dictionary<string, string>();
        foreach (var item in map.EnumerateObject())
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
