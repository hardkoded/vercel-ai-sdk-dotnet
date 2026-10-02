// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>Error reported by a provider after a model response stream has started.</summary>
public sealed class StreamProviderError : AiSdkError
{
    /// <summary>Marker for <see cref="IsInstance(object)"/>.</summary>
    public const string ErrorMarker = "vercel.ai.error.AI_StreamProviderError";

    /// <summary>Creates the error from provider metadata.</summary>
    public StreamProviderError(
        string message,
        string? type = null,
        object? code = null,
        int? statusCode = null,
        bool? isRetryable = null,
        object? data = null,
        Exception? cause = null)
        : base("AI_StreamProviderError", message, cause)
    {
        Type = type;
        Code = code;
        StatusCode = statusCode;
        IsRetryable = isRetryable ?? IsRetryableStatusCode(statusCode);
        Data = data;
        SetMarker(ErrorMarker, true);
    }

    /// <summary>Provider-defined error type.</summary>
    public string? Type { get; }

    /// <summary>Provider-defined error code.</summary>
    public object? Code { get; }

    /// <summary>HTTP-equivalent status code.</summary>
    public int? StatusCode { get; }

    /// <summary>Whether retrying the model call may succeed.</summary>
    public bool IsRetryable { get; }

    /// <summary>Original provider error payload.</summary>
    public object? Data { get; }

    /// <summary>Returns whether <paramref name="error"/> carries this error's marker.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, ErrorMarker);
    }

    /// <summary>Retryable statuses are 408, 409, 429, and 500 and above.</summary>
    public static bool IsRetryableStatusCode(int? statusCode)
    {
        return statusCode != null && (statusCode == 408 || statusCode == 409 || statusCode == 429 || statusCode >= 500);
    }
}
