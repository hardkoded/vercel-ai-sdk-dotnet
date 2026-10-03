// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Collections;

namespace Vercel.AI.OpenTelemetry;

/// <summary>Which recorded attributes a telemetry call should keep.</summary>
public sealed class TelemetrySelection
{
    /// <summary>When false, no attributes are recorded. Null records them.</summary>
    public bool? IsEnabled { get; set; }

    /// <summary>When false, input resolvers are skipped. Null records them.</summary>
    public bool? RecordInputs { get; set; }

    /// <summary>When false, output resolvers are skipped. Null records them.</summary>
    public bool? RecordOutputs { get; set; }
}

/// <summary>An attribute whose value is computed only when inputs or outputs are recorded.</summary>
public sealed class ResolvableTelemetryAttribute
{
    private readonly bool _input;
    private readonly Func<object?>? _resolve;
    private readonly Func<Task<object?>>? _resolveAsync;

    private ResolvableTelemetryAttribute(bool input, Func<object?>? resolve, Func<Task<object?>>? resolveAsync)
    {
        _input = input;
        _resolve = resolve;
        _resolveAsync = resolveAsync;
    }

    /// <summary>True when this value is an input.</summary>
    public bool IsInput => _input;

    /// <summary>Creates an input resolver.</summary>
    public static ResolvableTelemetryAttribute Input(Func<object?> resolve)
    {
        if (resolve is null)
        {
            throw new ArgumentNullException(nameof(resolve));
        }

        return new ResolvableTelemetryAttribute(true, resolve, null);
    }

    /// <summary>Creates an output resolver.</summary>
    public static ResolvableTelemetryAttribute Output(Func<object?> resolve)
    {
        if (resolve is null)
        {
            throw new ArgumentNullException(nameof(resolve));
        }

        return new ResolvableTelemetryAttribute(false, resolve, null);
    }

    /// <summary>Creates an asynchronous input resolver.</summary>
    public static ResolvableTelemetryAttribute Input(Func<Task<object?>> resolve)
    {
        if (resolve is null)
        {
            throw new ArgumentNullException(nameof(resolve));
        }

        return new ResolvableTelemetryAttribute(true, null, resolve);
    }

    /// <summary>Creates an asynchronous output resolver.</summary>
    public static ResolvableTelemetryAttribute Output(Func<Task<object?>> resolve)
    {
        if (resolve is null)
        {
            throw new ArgumentNullException(nameof(resolve));
        }

        return new ResolvableTelemetryAttribute(false, null, resolve);
    }

    /// <summary>Resolves the value.</summary>
    public object? Resolve()
    {
        if (_resolve != null)
        {
            return _resolve();
        }

        if (_resolveAsync == null)
        {
            return null;
        }

        return _resolveAsync().GetAwaiter().GetResult();
    }

    /// <summary>Resolves the value asynchronously.</summary>
    public Task<object?> ResolveAsync()
    {
        if (_resolveAsync != null)
        {
            return _resolveAsync();
        }

        return Task.FromResult(Resolve());
    }
}

/// <summary>Selects and sanitizes OpenTelemetry attribute values.</summary>
public static class TelemetryAttributes
{
    /// <summary>
    /// Returns a scalar or a homogeneous primitive array.
    /// Non-finite numbers and mixed or empty arrays are dropped.
    /// </summary>
    /// <param name="value">Candidate attribute value.</param>
    public static object? SanitizeValue(object? value)
    {
        if (value == null || value is string)
        {
            return value;
        }

        if (IsNumber(value))
        {
            return IsNonFinite(value) ? null : value;
        }

        if (value is bool)
        {
            return value;
        }

        if (value is not IEnumerable enumerable)
        {
            return null;
        }

        var kinds = PrimitiveKind.None;
        var numbersAreFinite = true;
        var kept = new List<object>();
        foreach (var item in enumerable)
        {
            if (item == null || !TryKind(item, out var kind))
            {
                continue;
            }

            if (kind == PrimitiveKind.Number && IsNonFinite(item))
            {
                numbersAreFinite = false;
            }

            kinds |= kind;
            kept.Add(item);
        }

        if (kept.Count == 0 || !IsSingleKind(kinds))
        {
            return null;
        }

        if (kinds == PrimitiveKind.Number && !numbersAreFinite)
        {
            return null;
        }

        return kept.ToArray();
    }

    /// <summary>Drops entries that <see cref="SanitizeValue"/> rejects. A null map returns an empty map.</summary>
    /// <param name="attributes">Attributes to sanitize.</param>
    public static IReadOnlyDictionary<string, object?> Sanitize(IReadOnlyDictionary<string, object?>? attributes)
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

            var sanitized = SanitizeValue(pair.Value);
            if (sanitized != null)
            {
                result[pair.Key] = sanitized;
            }
        }

        return result;
    }

    /// <summary>
    /// Selects attributes, resolving input and output callbacks and dropping values that cannot be recorded.
    /// </summary>
    /// <param name="telemetry">Recording switches. Null records every supplied value.</param>
    /// <param name="attributes">Literal values and <see cref="ResolvableTelemetryAttribute"/> callbacks.</param>
    public static IReadOnlyDictionary<string, object?> SelectAttributes(
        TelemetrySelection? telemetry,
        IReadOnlyDictionary<string, object?>? attributes)
    {
        if (telemetry?.IsEnabled == false || attributes == null)
        {
            return new Dictionary<string, object?>();
        }

        var result = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var pair in attributes)
        {
            if (!TryResolve(telemetry, pair.Value, out var resolved) || resolved == null)
            {
                continue;
            }

            var sanitized = SanitizeValue(resolved);
            if (sanitized != null)
            {
                result[pair.Key] = sanitized;
            }
        }

        return result;
    }

    /// <summary>
    /// Selects attributes without sanitizing them.
    /// Disabled telemetry returns an empty map. Null resolver results are omitted.
    /// </summary>
    /// <param name="telemetry">Recording switches. Null records every supplied value.</param>
    /// <param name="attributes">Literal values and <see cref="ResolvableTelemetryAttribute"/> callbacks.</param>
    /// <param name="cancellationToken">Cancels resolver work.</param>
    public static async Task<IReadOnlyDictionary<string, object?>> SelectTelemetryAttributesAsync(
        TelemetrySelection? telemetry,
        IReadOnlyDictionary<string, object?>? attributes,
        CancellationToken cancellationToken = default)
    {
        if (telemetry?.IsEnabled == false || attributes == null)
        {
            return new Dictionary<string, object?>();
        }

        var result = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var pair in attributes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (pair.Value == null)
            {
                continue;
            }

            if (pair.Value is ResolvableTelemetryAttribute resolvable)
            {
                if (resolvable.IsInput && telemetry?.RecordInputs == false)
                {
                    continue;
                }

                if (!resolvable.IsInput && telemetry?.RecordOutputs == false)
                {
                    continue;
                }

                var resolved = await resolvable.ResolveAsync().ConfigureAwait(false);
                if (resolved != null)
                {
                    result[pair.Key] = resolved;
                }

                continue;
            }

            result[pair.Key] = pair.Value;
        }

        return result;
    }

    private static bool TryResolve(TelemetrySelection? telemetry, object? value, out object? resolved)
    {
        resolved = null;
        if (value == null)
        {
            return false;
        }

        if (value is ResolvableTelemetryAttribute resolvable)
        {
            if (resolvable.IsInput && telemetry?.RecordInputs == false)
            {
                return false;
            }

            if (!resolvable.IsInput && telemetry?.RecordOutputs == false)
            {
                return false;
            }

            resolved = resolvable.Resolve();
            return resolved != null;
        }

        resolved = value;
        return true;
    }

    private static bool TryKind(object value, out PrimitiveKind kind)
    {
        if (value is string)
        {
            kind = PrimitiveKind.String;
            return true;
        }

        if (value is bool)
        {
            kind = PrimitiveKind.Boolean;
            return true;
        }

        if (IsNumber(value))
        {
            kind = PrimitiveKind.Number;
            return true;
        }

        kind = PrimitiveKind.None;
        return false;
    }

    private static bool IsNumber(object value)
    {
        return value is byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal;
    }

    private static bool IsNonFinite(object value)
    {
        return value switch
        {
            double number => double.IsNaN(number) || double.IsInfinity(number),
            float number => float.IsNaN(number) || float.IsInfinity(number),
            _ => false,
        };
    }

    private static bool IsSingleKind(PrimitiveKind kinds)
    {
        return kinds is PrimitiveKind.String or PrimitiveKind.Number or PrimitiveKind.Boolean;
    }

    [Flags]
    private enum PrimitiveKind
    {
        None = 0,
        String = 1,
        Number = 2,
        Boolean = 4,
    }
}
