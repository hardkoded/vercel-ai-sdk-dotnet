// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Prompt;

/// <summary>A provider stream payload that already carries status metadata.</summary>
public sealed class ProviderStreamError
{
    /// <summary>Creates a provider stream error.</summary>
    public ProviderStreamError(string message, object? data, string? type = null, object? code = null, int? statusCode = null, bool? isRetryable = null)
    {
        Message = message ?? string.Empty;
        Data = data;
        Type = type;
        Code = code;
        StatusCode = statusCode;
        IsRetryable = isRetryable;
    }

    /// <summary>Error message.</summary>
    public string Message { get; }

    /// <summary>Provider type.</summary>
    public string? Type { get; }

    /// <summary>Provider code.</summary>
    public object? Code { get; }

    /// <summary>HTTP status.</summary>
    public int? StatusCode { get; }

    /// <summary>Whether the caller should retry.</summary>
    public bool? IsRetryable { get; }

    /// <summary>Original provider data.</summary>
    public object? Data { get; }
}

/// <summary>A normalized stream provider error. Maps to <c>StreamProviderError</c>.</summary>
public sealed class StreamProviderError : Exception
{
    /// <summary>Creates the error.</summary>
    public StreamProviderError(string message, string? type, object? code, int? statusCode, bool isRetryable, object? data)
        : base(message)
    {
        Type = type;
        Code = code;
        StatusCode = statusCode;
        IsRetryable = isRetryable;
        Data = data;
    }

    /// <summary>Provider type.</summary>
    public string? Type { get; }

    /// <summary>Provider code.</summary>
    public object? Code { get; }

    /// <summary>HTTP status.</summary>
    public int? StatusCode { get; }

    /// <summary>Whether the caller should retry.</summary>
    public bool IsRetryable { get; }

    /// <summary>Original payload.</summary>
    public object? Data { get; }
}

/// <summary>An error-shaped value that must be returned unchanged.</summary>
public sealed class PreservedStreamError
{
    /// <summary>Creates a preserved value.</summary>
    public PreservedStreamError(string message, bool isAiSdkError)
    {
        Message = message;
        IsAiSdkError = isAiSdkError;
    }

    /// <summary>Error message.</summary>
    public string Message { get; }

    /// <summary>True when the value stands in for a cross-realm AI SDK error.</summary>
    public bool IsAiSdkError { get; }
}

/// <summary>Normalizes provider stream failures. Maps to <c>normalizeStreamProviderError</c>.</summary>
public static class StreamErrors
{
    /// <summary>
    /// Turns a provider payload into <see cref="StreamProviderError"/>.
    /// Existing exceptions, preserved errors, and values without a string message are returned unchanged.
    /// </summary>
    public static object? NormalizeStreamProviderError(object? error)
    {
        if (error is Exception || error is PreservedStreamError || error is StreamProviderError)
        {
            return error;
        }

        if (error is ProviderStreamError provider)
        {
            return Build(provider.Message, provider.Type, provider.Code, provider.StatusCode, provider.IsRetryable, provider.Data, provider);
        }

        if (error is not IReadOnlyDictionary<string, object?> outer)
        {
            return error;
        }

        var details = Nested(outer, "response", "error") ?? Nested(outer, "error") ?? outer;
        if (!TryString(details, "message", out var message))
        {
            return error;
        }

        var type = TryString(details, "type", out var detailType) ? detailType : TryString(outer, "type", out var outerType) ? outerType : null;
        var code = FirstCode(details, outer);
        var status = FirstStatus(details, outer);
        var retry = FirstBool(details, "isRetryable") ?? FirstBool(outer, "isRetryable") ?? FirstBool(details, "is_retryable") ?? FirstBool(outer, "is_retryable");
        return Build(message, type, code, status, retry, error, outer);
    }

    private static StreamProviderError Build(string message, string? type, object? code, int? explicitStatus, bool? explicitRetry, object? data, object source)
    {
        var status = explicitStatus ?? StatusFromCode(code) ?? MessageStatus(message);
        var retry = explicitRetry ?? MessageRetry(message) ?? IsRetryableStatus(status);
        return new StreamProviderError(message, type, code, status, retry, data ?? source);
    }

    private static int? MessageStatus(string message)
    {
        return message.Trim().ToLowerInvariant() switch
        {
            "overloaded" or "overloaded error" or "model overloaded" or "service unavailable" => 503,
            "internal server error" => 500,
            _ => null,
        };
    }

    private static bool? MessageRetry(string message)
    {
        return MessageStatus(message).HasValue ? true : null;
    }

    private static bool IsRetryableStatus(int? statusCode)
    {
        return statusCode is 408 or 409 or 429 || (statusCode.HasValue && statusCode.Value >= 500);
    }

    private static int? StatusFromCode(object? code)
    {
        return HttpStatus(code);
    }

    private static int? FirstStatus(IReadOnlyDictionary<string, object?> details, IReadOnlyDictionary<string, object?> outer)
    {
        return HttpStatus(Get(details, "statusCode"))
            ?? HttpStatus(Get(outer, "statusCode"))
            ?? HttpStatus(Get(details, "status_code"))
            ?? HttpStatus(Get(outer, "status_code"))
            ?? HttpStatus(Get(details, "status"))
            ?? HttpStatus(Get(outer, "status"))
            ?? HttpStatus(Get(details, "code"))
            ?? HttpStatus(Get(outer, "code"));
    }

    private static object? FirstCode(IReadOnlyDictionary<string, object?> details, IReadOnlyDictionary<string, object?> outer)
    {
        var code = Get(details, "code") ?? Get(outer, "code");
        return code is string or int or long ? code : null;
    }

    private static bool? FirstBool(IReadOnlyDictionary<string, object?> source, string name)
    {
        var value = Get(source, name);
        return value is bool flag ? flag : null;
    }

    private static int? HttpStatus(object? value)
    {
        if (value is string text && text.Length == 3 && int.TryParse(text, out var parsed) && parsed is >= 400 and <= 599)
        {
            return parsed;
        }

        if (value is int number && number is >= 400 and <= 599)
        {
            return number;
        }

        if (value is long wide && wide is >= 400 and <= 599)
        {
            return (int)wide;
        }

        return null;
    }

    private static bool TryString(IReadOnlyDictionary<string, object?> source, string name, out string value)
    {
        if (Get(source, name) is string text)
        {
            value = text;
            return true;
        }

        value = string.Empty;
        return false;
    }

    private static IReadOnlyDictionary<string, object?>? Nested(IReadOnlyDictionary<string, object?> source, params string[] path)
    {
        object? current = source;
        foreach (var name in path)
        {
            if (current is not IReadOnlyDictionary<string, object?> map || !map.TryGetValue(name, out current))
            {
                return null;
            }
        }

        return current as IReadOnlyDictionary<string, object?>;
    }

    private static object? Get(IReadOnlyDictionary<string, object?> source, string name)
    {
        return source.TryGetValue(name, out var value) ? value : null;
    }
}
