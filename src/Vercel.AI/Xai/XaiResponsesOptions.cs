// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using Vercel.AI.Util;

namespace Vercel.AI.Xai;

/// <summary>The <c>xai</c> provider options of a Responses call. Maps to <c>xaiLanguageModelResponsesOptions</c>.</summary>
internal sealed class XaiResponsesOptions
{
    private static readonly string[] ReasoningEfforts = { "none", "low", "medium", "high", "xhigh" };
    private static readonly string[] ReasoningSummaries = { "auto", "concise", "detailed" };
    private static readonly string[] ServiceTiers = { "default", "priority" };
    private static readonly string[] IncludeValues =
    {
        "file_search_call.results",
        "web_search_call.action.sources",
        "code_interpreter_call.outputs",
        "reasoning.encrypted_content",
        "no_inline_citations",
    };

    public string? ReasoningEffort { get; private set; }

    public bool? Logprobs { get; private set; }

    public int? TopLogprobs { get; private set; }

    public double? MinP { get; private set; }

    public int? MaxTurns { get; private set; }

    public bool? ParallelToolCalls { get; private set; }

    public string? PromptCacheKey { get; private set; }

    public string? SafetyIdentifier { get; private set; }

    public string? ServiceTier { get; private set; }

    public bool? Store { get; private set; }

    public string? PreviousResponseId { get; private set; }

    public string? User { get; private set; }

    public List<string>? Include { get; private set; }

    /// <summary>Reads and validates the options under <paramref name="providerOptions"/>["xai"].</summary>
    /// <exception cref="InvalidArgumentError">An option has the wrong type or value.</exception>
    public static XaiResponsesOptions Parse(IReadOnlyDictionary<string, JsonElement>? providerOptions)
    {
        var options = new XaiResponsesOptions();
        if (providerOptions == null || !providerOptions.TryGetValue(XaiProvider.ProviderId, out var source) || source.ValueKind == JsonValueKind.Null)
        {
            return options;
        }

        if (source.ValueKind != JsonValueKind.Object)
        {
            throw Invalid("xai", "expected an object.");
        }

        foreach (var property in source.EnumerateObject())
        {
            var value = property.Value;
            if (value.ValueKind == JsonValueKind.Null)
            {
                continue;
            }

            switch (property.Name)
            {
                case "reasoningEffort":
                    options.ReasoningEffort = OneOf(property, ReasoningEfforts);
                    break;
                case "reasoningSummary":
                    // xAI ignores the requested summary level, so the value is checked and then dropped.
                    OneOf(property, ReasoningSummaries);
                    break;
                case "logprobs":
                    options.Logprobs = Bool(property);
                    break;
                case "topLogprobs":
                    options.TopLogprobs = Int(property, 0, 8);
                    break;
                case "minP":
                    options.MinP = Number(property, 0, 1);
                    break;
                case "maxTurns":
                    options.MaxTurns = Int(property, int.MinValue, int.MaxValue);
                    break;
                case "parallelToolCalls":
                    options.ParallelToolCalls = Bool(property);
                    break;
                case "promptCacheKey":
                    options.PromptCacheKey = Text(property);
                    break;
                case "safetyIdentifier":
                    options.SafetyIdentifier = Text(property);
                    break;
                case "serviceTier":
                    options.ServiceTier = OneOf(property, ServiceTiers);
                    break;
                case "store":
                    options.Store = Bool(property);
                    break;
                case "previousResponseId":
                    options.PreviousResponseId = Text(property);
                    break;
                case "user":
                    options.User = Text(property);
                    break;
                case "include":
                    options.Include = IncludeList(property);
                    break;
            }
        }

        return options;
    }

    private static string Text(JsonProperty property)
    {
        if (property.Value.ValueKind != JsonValueKind.String)
        {
            throw Invalid(property.Name, "expected a string.");
        }

        return property.Value.GetString()!;
    }

    private static string OneOf(JsonProperty property, string[] allowed)
    {
        var value = Text(property);
        if (Array.IndexOf(allowed, value) < 0)
        {
            throw Invalid(property.Name, "expected one of " + string.Join(", ", allowed) + ".");
        }

        return value;
    }

    private static bool Bool(JsonProperty property)
    {
        if (property.Value.ValueKind != JsonValueKind.True && property.Value.ValueKind != JsonValueKind.False)
        {
            throw Invalid(property.Name, "expected a boolean.");
        }

        return property.Value.GetBoolean();
    }

    private static int Int(JsonProperty property, int minimum, int maximum)
    {
        if (property.Value.ValueKind != JsonValueKind.Number || !property.Value.TryGetInt32(out var number) || number < minimum || number > maximum)
        {
            throw Invalid(property.Name, "expected an integer" + Range(minimum, maximum) + ".");
        }

        return number;
    }

    private static double Number(JsonProperty property, double minimum, double maximum)
    {
        if (property.Value.ValueKind != JsonValueKind.Number || !property.Value.TryGetDouble(out var number) || number < minimum || number > maximum)
        {
            throw Invalid(property.Name, "expected a number between " + minimum.ToString(CultureInfo.InvariantCulture) + " and " + maximum.ToString(CultureInfo.InvariantCulture) + ".");
        }

        return number;
    }

    private static List<string> IncludeList(JsonProperty property)
    {
        if (property.Value.ValueKind != JsonValueKind.Array)
        {
            throw Invalid(property.Name, "expected an array.");
        }

        var values = new List<string>();
        foreach (var item in property.Value.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String || Array.IndexOf(IncludeValues, item.GetString()) < 0)
            {
                throw Invalid(property.Name, "expected values from " + string.Join(", ", IncludeValues) + ".");
            }

            values.Add(item.GetString()!);
        }

        return values;
    }

    private static string Range(int minimum, int maximum)
    {
        return minimum == int.MinValue && maximum == int.MaxValue
            ? string.Empty
            : " between " + minimum.ToString(CultureInfo.InvariantCulture) + " and " + maximum.ToString(CultureInfo.InvariantCulture);
    }

    private static InvalidArgumentError Invalid(string name, string message)
    {
        return new InvalidArgumentError("providerOptions.xai." + name, null, message);
    }
}
