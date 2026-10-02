// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>
/// Reads error markers used to recognize AI SDK errors across assembly copies.
/// Mirrors <c>Symbol.for</c> markers in the TypeScript SDK.
/// </summary>
public interface IErrorMarkers
{
    /// <summary>Returns whether <paramref name="marker"/> is set to <c>true</c>.</summary>
    bool HasMarker(string marker);
}

/// <summary>Plain marker bag used to recognize an error shape copied across packages.</summary>
public sealed class ErrorMarkerBag : IErrorMarkers
{
    private readonly Dictionary<string, bool> _markers;

    /// <summary>Creates a bag with the given marker values.</summary>
    public ErrorMarkerBag(params KeyValuePair<string, bool>[] markers)
    {
        _markers = new Dictionary<string, bool>(StringComparer.Ordinal);
        if (markers != null)
        {
            for (var i = 0; i < markers.Length; i++)
            {
                _markers[markers[i].Key] = markers[i].Value;
            }
        }
    }

    /// <summary>Creates a bag with one marker.</summary>
    public ErrorMarkerBag(string marker, bool value)
        : this(new KeyValuePair<string, bool>(marker, value))
    {
    }

    /// <inheritdoc />
    public bool HasMarker(string marker)
    {
        bool value;
        return marker != null && _markers.TryGetValue(marker, out value) && value;
    }
}

/// <summary>
/// Base error for upstream AI SDK error types. Marker checks work without a shared assembly.
/// </summary>
public class AiSdkError : Exception, IErrorMarkers
{
    /// <summary>Marker present on every AI SDK error.</summary>
    public const string Marker = "vercel.ai.error";

    private readonly Dictionary<string, bool> _markers = new Dictionary<string, bool>(StringComparer.Ordinal);

    /// <summary>Creates an error with the upstream <paramref name="name"/> and <paramref name="message"/>.</summary>
    public AiSdkError(string? name, string message, Exception? cause = null)
        : base(message, cause)
    {
        ErrorName = name ?? string.Empty;
        Cause = cause;
        SetMarker(Marker, true);
    }

    /// <summary>Upstream error name, such as <c>AI_APICallError</c>.</summary>
    public string ErrorName { get; }

    /// <summary>Underlying cause, when one was supplied.</summary>
    public Exception? Cause { get; }

    /// <summary>Records a marker used by <see cref="HasMarker(object, string)"/>.</summary>
    protected void SetMarker(string marker, bool value)
    {
        if (marker != null)
        {
            _markers[marker] = value;
        }
    }

    /// <inheritdoc />
    public bool HasMarker(string marker)
    {
        bool value;
        return marker != null && _markers.TryGetValue(marker, out value) && value;
    }

    /// <summary>Returns whether <paramref name="error"/> carries the base AI SDK marker.</summary>
    public static bool IsInstance(object? error)
    {
        return HasMarker(error, Marker);
    }

    /// <summary>Returns whether <paramref name="error"/> has <paramref name="marker"/> set to <c>true</c>.</summary>
    public static bool HasMarker(object? error, string? marker)
    {
        var markers = error as IErrorMarkers;
        return markers != null && markers.HasMarker(marker);
    }
}

/// <summary>An argument failed validation. Message format matches the TypeScript SDK.</summary>
public sealed class InvalidArgumentError : AiSdkError
{
    /// <summary>Marker for <see cref="IsInstance(object)"/>.</summary>
    public const string ErrorMarker = "vercel.ai.error.AI_InvalidArgumentError";

    /// <summary>Creates the error for <paramref name="parameter"/>.</summary>
    public InvalidArgumentError(string parameter, object? value, string message)
        : base("AI_InvalidArgumentError", "Invalid argument for parameter " + parameter + ": " + message)
    {
        Parameter = parameter;
        Value = value;
        SetMarker(ErrorMarker, true);
    }

    /// <summary>Parameter that failed validation.</summary>
    public string Parameter { get; }

    /// <summary>Rejected value.</summary>
    public object? Value { get; }

    /// <summary>Returns whether <paramref name="error"/> carries this error's marker.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, ErrorMarker);
    }
}
