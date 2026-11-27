// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.Gateway;

/// <summary>Validates <c>providerOptions.gateway</c> for decision requests.</summary>
internal static class GatewayDecisionProviderOptions
{
    /// <summary>Maps to <c>DECISION_FALLBACK_MAX_CONDITION_DEPTH</c>.</summary>
    internal const int DecisionFallbackMaxConditionDepth = 5;

    /// <summary>Maps to <c>DECISION_FALLBACK_MAX_CONDITIONS_PER_LIST</c>.</summary>
    internal const int DecisionFallbackMaxConditionsPerList = 20;

    /// <summary>Maps to <c>DECISION_FALLBACK_MAX_QUESTION_LENGTH</c>.</summary>
    internal const int DecisionFallbackMaxQuestionLength = 256;

    private const int MaxDepth = DecisionFallbackMaxConditionDepth;
    private const int MaxConditionsPerList = DecisionFallbackMaxConditionsPerList;
    private const int MaxQuestionLength = DecisionFallbackMaxQuestionLength;

    /// <summary>
    /// Validates the <c>gateway</c> provider options object against <c>gatewayDecisionProviderOptionsSchema</c>, as
    /// <c>safeValidateTypes</c> does. Service-owned keys are kept as they are.
    /// </summary>
    public static GatewayDecisionProviderOptionsResult SafeValidateTypes(JsonElement value)
    {
        var problem =
            value.ValueKind != JsonValueKind.Object ? "gateway options must be an object" : null;
        if (problem == null && value.TryGetProperty("models", out var models))
        {
            problem = Models(models);
        }

        return new GatewayDecisionProviderOptionsResult(problem == null, value, problem);
    }

    public static void Validate(JsonElement providerOptions)
    {
        if (
            providerOptions.ValueKind != JsonValueKind.Object
            || !providerOptions.TryGetProperty("gateway", out var gateway)
        )
        {
            return;
        }

        var problem = SafeValidateTypes(gateway).Error;

        if (problem != null)
        {
            throw new InvalidArgumentException(
                "providerOptions",
                gateway.ToString(),
                "invalid gateway provider options: " + problem
            );
        }
    }

    private static string? Models(JsonElement models)
    {
        if (models.ValueKind != JsonValueKind.Array)
        {
            return "models must be an array";
        }

        var index = 0;
        var conditional = 0;
        foreach (var entry in models.EnumerateArray())
        {
            if (entry.ValueKind == JsonValueKind.String)
            {
                index++;
                continue;
            }

            if (entry.ValueKind != JsonValueKind.Object)
            {
                return "models entries must be strings or conditional fallbacks";
            }

            conditional++;
            if (conditional > 1)
            {
                return "models supports at most one conditional decision fallback";
            }

            if (index != 0)
            {
                return "a conditional decision fallback must be the first models entry";
            }

            var problem = Fallback(entry);
            if (problem != null)
            {
                return problem;
            }

            index++;
        }

        return null;
    }

    private static string? Fallback(JsonElement entry)
    {
        foreach (var property in entry.EnumerateObject())
        {
            if (property.Name != "model" && property.Name != "when")
            {
                return "unrecognized key \"" + property.Name + "\" in a conditional fallback";
            }
        }

        if (
            !entry.TryGetProperty("model", out var model)
            || model.ValueKind != JsonValueKind.String
            || model.GetString()!.Length == 0
        )
        {
            return "a conditional fallback requires a model";
        }

        return entry.TryGetProperty("when", out var when)
            ? Condition(when, 1)
            : "a conditional fallback requires a when condition";
    }

    private static string? Condition(JsonElement condition, int depth)
    {
        if (condition.ValueKind != JsonValueKind.Object)
        {
            return "a condition must be an object";
        }

        var keys = condition.EnumerateObject().Select(property => property.Name).ToArray();
        if (keys.Contains("confidenceBelow") || keys.Contains("probabilityBetween"))
        {
            return Direct(condition, keys);
        }

        if (keys.Length != 1 || (keys[0] != "any" && keys[0] != "all" && keys[0] != "atLeast"))
        {
            return "a condition must be confidenceBelow, probabilityBetween, any, all, or atLeast";
        }

        if (depth == MaxDepth)
        {
            return "conditions can be nested at most "
                + MaxDepth.ToString(CultureInfo.InvariantCulture)
                + " levels deep";
        }

        var group = condition.GetProperty(keys[0]);
        if (keys[0] != "atLeast")
        {
            return List(group, depth);
        }

        if (group.ValueKind != JsonValueKind.Object)
        {
            return "atLeast must be an object";
        }

        foreach (var property in group.EnumerateObject())
        {
            if (property.Name != "count" && property.Name != "conditions")
            {
                return "unrecognized key \"" + property.Name + "\" in atLeast";
            }
        }

        if (
            !group.TryGetProperty("count", out var count)
            || count.ValueKind != JsonValueKind.Number
            || !count.TryGetInt32(out var number)
            || number < 1
        )
        {
            return "atLeast count must be an integer of at least 1";
        }

        if (!group.TryGetProperty("conditions", out var conditions))
        {
            return "atLeast requires conditions";
        }

        var problem = List(conditions, depth);
        if (problem == null && number > conditions.GetArrayLength())
        {
            return "atLeast count cannot exceed the number of conditions";
        }

        return problem;
    }

    private static string? List(JsonElement list, int depth)
    {
        if (
            list.ValueKind != JsonValueKind.Array
            || list.GetArrayLength() < 1
            || list.GetArrayLength() > MaxConditionsPerList
        )
        {
            return "a condition list must contain between 1 and "
                + MaxConditionsPerList.ToString(CultureInfo.InvariantCulture)
                + " conditions";
        }

        foreach (var child in list.EnumerateArray())
        {
            var problem = Condition(child, depth + 1);
            if (problem != null)
            {
                return problem;
            }
        }

        return null;
    }

    private static string? Direct(JsonElement condition, string[] keys)
    {
        var allowed = keys.Contains("confidenceBelow") ? "confidenceBelow" : "probabilityBetween";
        foreach (var key in keys)
        {
            if (key != "question" && key != allowed)
            {
                return "unrecognized key \"" + key + "\" in a condition";
            }
        }

        if (
            condition.TryGetProperty("question", out var question)
            && (
                question.ValueKind != JsonValueKind.String
                || question.GetString()!.Length < 1
                || question.GetString()!.Length > MaxQuestionLength
            )
        )
        {
            return "question must be a string of 1 to "
                + MaxQuestionLength.ToString(CultureInfo.InvariantCulture)
                + " characters";
        }

        var value = condition.GetProperty(allowed);
        if (allowed == "confidenceBelow")
        {
            return Probability(value) ? null : "confidenceBelow must be a number between 0 and 1";
        }

        if (
            value.ValueKind != JsonValueKind.Array
            || value.GetArrayLength() != 2
            || !value.EnumerateArray().All(Probability)
        )
        {
            return "probabilityBetween must be two numbers between 0 and 1";
        }

        return value[0].GetDouble() <= value[1].GetDouble()
            ? null
            : "probabilityBetween minimum must be less than or equal to maximum";
    }

    private static bool Probability(JsonElement value)
    {
        return value.ValueKind == JsonValueKind.Number
            && value.GetDouble() >= 0
            && value.GetDouble() <= 1;
    }
}

/// <summary>Result of <see cref="GatewayDecisionProviderOptions.SafeValidateTypes"/>. Maps to the <c>safeValidateTypes</c> result.</summary>
internal sealed record GatewayDecisionProviderOptionsResult(
    bool Success,
    JsonElement Value,
    string? Error
);
