// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;

namespace Vercel.AI.ProviderUtils;

/// <summary>Checks one provider option value. Returns an error message, or null when the value is valid.</summary>
internal delegate string? OptionValidator(JsonElement value);

/// <summary>
/// Validators for the option shapes that upstream describes with zod. A shape is a map from option name to
/// <see cref="OptionValidator"/>. Every key is optional and a null value is rejected, like <c>.optional()</c>.
/// </summary>
internal static class ProviderOptionShape
{
    /// <summary>Maps to <c>z.string()</c> with an optional <c>.min(length)</c>.</summary>
    public static OptionValidator String(int minLength = 0)
    {
        return value => value.ValueKind != JsonValueKind.String
            ? "expected string, received " + Describe(value)
            : minLength > 0 && value.GetString()!.Length < minLength ? "expected at least " + minLength.ToString(CultureInfo.InvariantCulture) + " character(s)" : null;
    }

    /// <summary>Maps to <c>z.boolean()</c>.</summary>
    public static OptionValidator Boolean()
    {
        return value => value.ValueKind is JsonValueKind.True or JsonValueKind.False ? null : "expected boolean, received " + Describe(value);
    }

    /// <summary>Maps to <c>z.number()</c> with optional <c>.int()</c>, <c>.min()</c> and <c>.max()</c>.</summary>
    public static OptionValidator Number(double? min = null, double? max = null, bool integer = false)
    {
        return value =>
        {
            if (value.ValueKind != JsonValueKind.Number || !value.TryGetDouble(out var number))
            {
                return "expected number, received " + Describe(value);
            }

            if (integer && number != Math.Floor(number))
            {
                return "expected integer";
            }

            if (min is { } lower && number < lower)
            {
                return "expected a number of at least " + lower.ToString(CultureInfo.InvariantCulture);
            }

            return max is { } upper && number > upper ? "expected a number of at most " + upper.ToString(CultureInfo.InvariantCulture) : null;
        };
    }

    /// <summary>Maps to <c>z.enum([...])</c>.</summary>
    public static OptionValidator Enum(params string[] values)
    {
        return value => value.ValueKind == JsonValueKind.String && Array.IndexOf(values, value.GetString()) >= 0
            ? null
            : "expected one of " + string.Join(" | ", values);
    }

    /// <summary>Maps to <c>z.literal(...)</c> for a string.</summary>
    public static OptionValidator Literal(string expected)
    {
        return value => value.ValueKind == JsonValueKind.String && value.GetString() == expected ? null : "expected " + expected;
    }

    /// <summary>Maps to <c>z.array(item)</c> with an optional <c>.length(n)</c>.</summary>
    public static OptionValidator ArrayOf(OptionValidator item, int? length = null)
    {
        return value =>
        {
            if (value.ValueKind != JsonValueKind.Array)
            {
                return "expected array, received " + Describe(value);
            }

            if (length is { } expected && value.GetArrayLength() != expected)
            {
                return "expected an array of exactly " + expected.ToString(CultureInfo.InvariantCulture) + " item(s)";
            }

            var index = 0;
            foreach (var element in value.EnumerateArray())
            {
                var error = item(element);
                if (error != null)
                {
                    return "[" + index.ToString(CultureInfo.InvariantCulture) + "]: " + error;
                }

                index++;
            }

            return null;
        };
    }

    /// <summary>Maps to <c>.nullish()</c>: the value may also be null.</summary>
    public static OptionValidator Nullish(OptionValidator inner)
    {
        return value => value.ValueKind == JsonValueKind.Null ? null : inner(value);
    }

    /// <summary>Maps to <c>z.union([...])</c>: the value is valid when one validator accepts it.</summary>
    public static OptionValidator Union(params OptionValidator[] options)
    {
        return value =>
        {
            foreach (var option in options)
            {
                if (option(value) == null)
                {
                    return null;
                }
            }

            return "invalid value";
        };
    }

    /// <summary>
    /// Maps to <c>z.object(shape)</c>, or <c>z.strictObject(shape)</c> when <paramref name="strict"/> is true. Names in
    /// <paramref name="required"/> must be present.
    /// </summary>
    public static OptionValidator ObjectOf(IReadOnlyDictionary<string, OptionValidator> shape, bool strict = false, params string[] required)
    {
        return value => Validate(value, shape, strict, required);
    }

    /// <summary>Validates <paramref name="value"/> against <paramref name="shape"/>. Returns the first error, or null.</summary>
    public static string? Validate(JsonElement value, IReadOnlyDictionary<string, OptionValidator> shape, bool strict, params string[] required)
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            return "expected object, received " + Describe(value);
        }

        foreach (var name in required)
        {
            if (!value.TryGetProperty(name, out _))
            {
                return name + ": required";
            }
        }

        foreach (var property in value.EnumerateObject())
        {
            if (!shape.TryGetValue(property.Name, out var validator))
            {
                if (strict)
                {
                    return "unrecognized key \"" + property.Name + "\"";
                }

                continue;
            }

            var error = validator(property.Value);
            if (error != null)
            {
                return property.Name + ": " + error;
            }
        }

        return null;
    }

    /// <summary>Copies the entries of <paramref name="overrides"/> over <paramref name="target"/>, like an object spread.</summary>
    public static Dictionary<string, OptionValidator> Spread(Dictionary<string, OptionValidator> target, IReadOnlyDictionary<string, OptionValidator> overrides)
    {
        foreach (var pair in overrides)
        {
            target[pair.Key] = pair.Value;
        }

        return target;
    }

    private static string Describe(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.Null => "null",
            JsonValueKind.Undefined => "undefined",
            JsonValueKind.True or JsonValueKind.False => "boolean",
            JsonValueKind.Number => "number",
            JsonValueKind.String => "string",
            JsonValueKind.Array => "array",
            _ => "object",
        };
    }
}
