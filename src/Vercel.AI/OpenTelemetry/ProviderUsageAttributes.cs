// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Vercel.AI.OpenTelemetry;

/// <summary>Helpers of <c>provider-usage-attributes.ts</c>.</summary>
internal static class ProviderUsageAttributes
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);

    private static readonly Dictionary<string, string> UsageKeyAliases = new(StringComparer.Ordinal)
    {
        ["inputtokens"] = "input_tokens",
        ["prompttokens"] = "input_tokens",
        ["prompttokencount"] = "input_tokens",
        ["totalinputtokens"] = "input_tokens",
        ["outputtokens"] = "output_tokens",
        ["completiontokens"] = "output_tokens",
        ["completiontokencount"] = "output_tokens",
        ["candidatestokencount"] = "output_tokens",
        ["totaltokens"] = "total_tokens",
        ["totaltokencount"] = "total_tokens",
    };

    /// <summary>
    /// Converts provider-native usage objects into numeric span attributes. Usage units such as tokens, characters, and seconds
    /// remain distinct so observability backends can apply the provider and model specific price unit. Maps to
    /// <c>getProviderUsageAttributes</c>.
    /// </summary>
    public static Dictionary<string, object?> GetProviderUsageAttributes(
        JsonElement? usage,
        string prefix
    )
    {
        var attributes = new Dictionary<string, object?>(StringComparer.Ordinal);
        if (usage is { } value)
        {
            AddValue(attributes, prefix, value, []);
        }

        return attributes;
    }

    private static void AddValue(
        Dictionary<string, object?> attributes,
        string prefix,
        JsonElement value,
        List<string> path
    )
    {
        if (value.ValueKind == JsonValueKind.Number)
        {
            if (!value.TryGetDouble(out var number))
            {
                return;
            }

            if (!double.IsNaN(number) && !double.IsInfinity(number) && path.Count > 0)
            {
                // Whole numbers stay integers, like the token counts of the other spans.
                attributes[prefix + "." + string.Join(".", path)] = value.TryGetInt64(out var whole)
                    ? (object)whole
                    : number;
            }

            return;
        }

        if (value.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (var property in value.EnumerateObject())
        {
            var normalizedKey = NormalizeUsageKey(property.Name);
            if (normalizedKey.Length > 0)
            {
                AddValue(attributes, prefix, property.Value, [.. path, normalizedKey]);
            }
        }
    }

    private static string NormalizeUsageKey(string key)
    {
        var aliasKey = Regex
            .Replace(key, "[^a-zA-Z0-9]", string.Empty, RegexOptions.None, RegexTimeout)
            .ToLowerInvariant();
        if (UsageKeyAliases.TryGetValue(aliasKey, out var alias))
        {
            return alias;
        }

        var snake = Regex.Replace(
            key,
            "([a-z0-9])([A-Z])",
            "$1_$2",
            RegexOptions.None,
            RegexTimeout
        );
        snake = Regex.Replace(snake, "[^a-zA-Z0-9]+", "_", RegexOptions.None, RegexTimeout);
        snake = Regex.Replace(snake, "^_+|_+$", string.Empty, RegexOptions.None, RegexTimeout);
        return snake.ToLower(CultureInfo.InvariantCulture);
    }
}
