// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.OpenTelemetry;

/// <summary>Which span attributes a call should record.</summary>
public sealed class TelemetrySettings
{
    /// <summary>When false, no attributes are recorded. Null records attributes.</summary>
    public bool? IsEnabled { get; set; }

    /// <summary>When false, input resolvers are skipped. Null records inputs.</summary>
    public bool? RecordInputs { get; set; }

    /// <summary>When false, output resolvers are skipped. Null records outputs.</summary>
    public bool? RecordOutputs { get; set; }

    /// <summary>Function id appended to the operation name.</summary>
    public string? FunctionId { get; set; }
}

/// <summary>An attribute whose value is an input and can be omitted.</summary>
public sealed class TelemetryInput
{
    /// <summary>Creates an input attribute.</summary>
    public TelemetryInput(Func<object?> resolve)
    {
        Resolve = resolve ?? throw new ArgumentNullException(nameof(resolve));
    }

    /// <summary>Produces the attribute value.</summary>
    public Func<object?> Resolve { get; }
}

/// <summary>An attribute whose value is an output and can be omitted.</summary>
public sealed class TelemetryOutput
{
    /// <summary>Creates an output attribute.</summary>
    public TelemetryOutput(Func<object?> resolve)
    {
        Resolve = resolve ?? throw new ArgumentNullException(nameof(resolve));
    }

    /// <summary>Produces the attribute value.</summary>
    public Func<object?> Resolve { get; }
}

/// <summary>Drops attribute values OpenTelemetry cannot export and applies record-input and record-output flags.</summary>
public static class TelemetryAttributes
{
    /// <summary>
    /// Returns <paramref name="value"/> when it is a finite scalar or a homogeneous primitive array.
    /// Non-finite numbers, mixed arrays, and empty arrays return null.
    /// </summary>
    public static object? SanitizeAttributeValue(object? value)
    {
        if (value is not System.Collections.IEnumerable || value is string)
        {
            if (value is double number && (double.IsNaN(number) || double.IsInfinity(number)))
            {
                return null;
            }

            if (value is float single && (float.IsNaN(single) || float.IsInfinity(single)))
            {
                return null;
            }

            return value;
        }

        var items = new List<object?>();
        foreach (var item in (System.Collections.IEnumerable)value)
        {
            items.Add(item);
        }

        string? primitive = null;
        for (var index = 0; index < items.Count; index++)
        {
            if (!IsPrimitive(items[index], out var kind))
            {
                continue;
            }

            if (primitive == null)
            {
                primitive = kind;
            }
            else if (primitive != kind)
            {
                return null;
            }
        }

        if (primitive == null)
        {
            return null;
        }

        if (primitive == "string")
        {
            var strings = new List<string>();
            for (var index = 0; index < items.Count; index++)
            {
                if (items[index] is string text)
                {
                    strings.Add(text);
                }
            }

            return strings.ToArray();
        }

        if (primitive == "number")
        {
            var numbers = new List<object>();
            for (var index = 0; index < items.Count; index++)
            {
                if (!IsNumber(items[index], out var number))
                {
                    continue;
                }

                if (number is double real && (double.IsNaN(real) || double.IsInfinity(real)))
                {
                    return null;
                }

                if (number is float single && (float.IsNaN(single) || float.IsInfinity(single)))
                {
                    return null;
                }

                numbers.Add(number);
            }

            return numbers.ToArray();
        }

        var flags = new List<bool>();
        for (var index = 0; index < items.Count; index++)
        {
            if (items[index] is bool flag)
            {
                flags.Add(flag);
            }
        }

        return flags.ToArray();
    }

    /// <summary>Copies <paramref name="attributes"/>, dropping nulls and values <see cref="SanitizeAttributeValue"/> rejects.</summary>
    public static Dictionary<string, object?> Sanitize(IReadOnlyDictionary<string, object?>? attributes)
    {
        var result = new Dictionary<string, object?>(StringComparer.Ordinal);
        if (attributes == null)
        {
            return result;
        }

        foreach (var pair in attributes)
        {
            if (pair.Value == null)
            {
                continue;
            }

            var sanitized = SanitizeAttributeValue(pair.Value);
            if (sanitized != null)
            {
                result[pair.Key] = sanitized;
            }
        }

        return result;
    }

    /// <summary>
    /// Selects attributes that should be recorded. Disabled telemetry returns an empty map.
    /// Input and output resolvers are included unless the matching record flag is false, then sanitized.
    /// </summary>
    public static Dictionary<string, object?> Select(TelemetrySettings? telemetry, IReadOnlyDictionary<string, object?> attributes)
    {
        if (attributes is null)
        {
            throw new ArgumentNullException(nameof(attributes));
        }

        if (telemetry?.IsEnabled == false)
        {
            return new Dictionary<string, object?>(StringComparer.Ordinal);
        }

        var result = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var pair in attributes)
        {
            if (pair.Value == null)
            {
                continue;
            }

            if (pair.Value is TelemetryInput input)
            {
                if (telemetry?.RecordInputs == false)
                {
                    continue;
                }

                AddSanitized(result, pair.Key, input.Resolve());
                continue;
            }

            if (pair.Value is TelemetryOutput output)
            {
                if (telemetry?.RecordOutputs == false)
                {
                    continue;
                }

                AddSanitized(result, pair.Key, output.Resolve());
                continue;
            }

            AddSanitized(result, pair.Key, pair.Value);
        }

        return result;
    }

    /// <summary>
    /// Selects telemetry attributes without sanitizing them.
    /// A null result from a resolver is omitted. Disabled telemetry returns an empty map.
    /// </summary>
    public static Task<Dictionary<string, object?>> SelectAsync(TelemetrySettings? telemetry, IReadOnlyDictionary<string, object?> attributes)
    {
        if (attributes is null)
        {
            throw new ArgumentNullException(nameof(attributes));
        }

        if (telemetry?.IsEnabled == false)
        {
            return Task.FromResult(new Dictionary<string, object?>(StringComparer.Ordinal));
        }

        var result = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var pair in attributes)
        {
            if (pair.Value == null)
            {
                continue;
            }

            if (pair.Value is TelemetryInput input)
            {
                if (telemetry?.RecordInputs == false)
                {
                    continue;
                }

                var resolved = input.Resolve();
                if (resolved != null)
                {
                    result[pair.Key] = resolved;
                }

                continue;
            }

            if (pair.Value is TelemetryOutput output)
            {
                if (telemetry?.RecordOutputs == false)
                {
                    continue;
                }

                var resolved = output.Resolve();
                if (resolved != null)
                {
                    result[pair.Key] = resolved;
                }

                continue;
            }

            result[pair.Key] = pair.Value;
        }

        return Task.FromResult(result);
    }

    private static void AddSanitized(Dictionary<string, object?> result, string key, object? value)
    {
        if (value == null)
        {
            return;
        }

        var sanitized = SanitizeAttributeValue(value);
        if (sanitized != null)
        {
            result[key] = sanitized;
        }
    }

    private static bool IsPrimitive(object? value, out string kind)
    {
        if (value is string)
        {
            kind = "string";
            return true;
        }

        if (value is bool)
        {
            kind = "boolean";
            return true;
        }

        if (IsNumber(value, out _))
        {
            kind = "number";
            return true;
        }

        kind = string.Empty;
        return false;
    }

    private static bool IsNumber(object? value, out object number)
    {
        switch (value)
        {
            case byte integer:
                number = integer;
                return true;
            case short integer:
                number = integer;
                return true;
            case int integer:
                number = integer;
                return true;
            case long integer:
                number = integer;
                return true;
            case float real:
                number = real;
                return true;
            case double real:
                number = real;
                return true;
            default:
                number = 0;
                return false;
        }
    }
}
