// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Error;

/// <summary>Error reported by a provider after a model response stream has started.</summary>
public sealed class StreamProviderException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_StreamProviderError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_StreamProviderError";

    /// <summary>Creates the exception.</summary>
    public StreamProviderException(
        string message,
        string? type = null,
        object? code = null,
        int? statusCode = null,
        bool? isRetryable = null,
        object? data = null,
        object? cause = null)
        : base(ErrorName, message, cause)
    {
        SetMarker(TypeMarker);
        Type = type;
        Code = code;
        StatusCode = statusCode;
        IsRetryable = isRetryable ?? IsRetryableStatusCode(statusCode);
        Data = data;
    }

    /// <summary>Provider-defined error type, when the provider supplied one.</summary>
    public string? Type { get; }

    /// <summary>Provider-defined error code. A string or a number, when the provider supplied one.</summary>
    public object? Code { get; }

    /// <summary>HTTP-equivalent status code, when the provider supplied one.</summary>
    public int? StatusCode { get; }

    /// <summary>
    /// Whether retrying the model call may succeed.
    /// Defaults to true for 408, 409, 429, and status codes of 500 or higher.
    /// </summary>
    public bool IsRetryable { get; }

    /// <summary>Original provider error payload.</summary>
    public new object? Data { get; }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }

    private static bool IsRetryableStatusCode(int? statusCode)
    {
        return statusCode != null
            && (statusCode == 408 || statusCode == 409 || statusCode == 429 || statusCode >= 500);
    }
}
