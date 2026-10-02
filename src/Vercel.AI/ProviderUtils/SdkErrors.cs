// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.ProviderUtils;

/// <summary>Reads a message from an arbitrary thrown value. Maps to <c>getErrorMessage</c>.</summary>
public static class ErrorMessages
{
    /// <summary>Returns a display message for <paramref name="error"/>.</summary>
    public static string GetErrorMessage(object? error)
    {
        if (error is null || error is JsUndefined)
        {
            return "unknown error";
        }

        if (error is string text)
        {
            return text;
        }

        if (error is Exception exception)
        {
            var name = exception is SdkError sdk ? sdk.Name : exception.GetType().Name;
            return name + ": " + exception.Message;
        }

        return JsonSerializer.Serialize(error);
    }
}

/// <summary>Base error with the upstream <c>AI_*</c> name. Maps to <c>AISDKError</c>.</summary>
public abstract class SdkError : Exception
{
    /// <summary>Creates an SDK error.</summary>
    protected SdkError(string name, string message, object? cause)
        : base(message, cause as Exception)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Cause = cause;
    }

    /// <summary>Upstream error name, such as <c>AI_JSONParseError</c>.</summary>
    public string Name { get; }

    /// <summary>Underlying cause. May be a value that is not an <see cref="Exception"/>.</summary>
    public object? Cause { get; }
}

/// <summary>A function argument is invalid. Maps to <c>InvalidArgumentError</c>.</summary>
public sealed class InvalidArgumentError : SdkError
{
    /// <summary>Creates the error.</summary>
    public InvalidArgumentError(string argument, string message, object? cause = null)
        : base("AI_InvalidArgumentError", message, cause)
    {
        Argument = argument ?? throw new ArgumentNullException(nameof(argument));
    }

    /// <summary>Argument name.</summary>
    public string Argument { get; }

    /// <summary>True when <paramref name="error"/> is an <see cref="InvalidArgumentError"/>.</summary>
    public static bool IsInstance(object? error)
    {
        return error is InvalidArgumentError;
    }
}

/// <summary>JSON text could not be parsed. Maps to <c>JSONParseError</c>.</summary>
public sealed class JSONParseError : SdkError
{
    /// <summary>Creates the error.</summary>
    public JSONParseError(string text, object? cause)
        : base(
            "AI_JSONParseError",
            "JSON parsing failed: Text: " + text + ".\nError message: " + ErrorMessages.GetErrorMessage(cause),
            cause)
    {
        Text = text ?? string.Empty;
    }

    /// <summary>Original text.</summary>
    public string Text { get; }

    /// <summary>True when <paramref name="error"/> is a <see cref="JSONParseError"/>.</summary>
    public static bool IsInstance(object? error)
    {
        return error is JSONParseError;
    }
}

/// <summary>Optional context for a type-validation failure.</summary>
public sealed class TypeValidationContext
{
    /// <summary>Field path in dot notation.</summary>
    public string? Field { get; set; }

    /// <summary>Entity name.</summary>
    public string? EntityName { get; set; }

    /// <summary>Entity identifier.</summary>
    public string? EntityId { get; set; }
}

/// <summary>A value failed schema validation. Maps to <c>TypeValidationError</c>.</summary>
public sealed class TypeValidationError : SdkError
{
    /// <summary>Creates the error.</summary>
    public TypeValidationError(object? value, object? cause, TypeValidationContext? context = null)
        : base("AI_TypeValidationError", BuildMessage(value, cause, context), cause)
    {
        Value = value;
        Context = context;
    }

    /// <summary>Value that failed validation.</summary>
    public object? Value { get; }

    /// <summary>Optional validation context.</summary>
    public TypeValidationContext? Context { get; }

    /// <summary>True when <paramref name="error"/> is a <see cref="TypeValidationError"/>.</summary>
    public static bool IsInstance(object? error)
    {
        return error is TypeValidationError;
    }

    /// <summary>
    /// Returns <paramref name="cause"/> when it is already a <see cref="TypeValidationError"/> for the same value and context.
    /// </summary>
    public static TypeValidationError Wrap(object? value, object? cause, TypeValidationContext? context = null)
    {
        if (cause is TypeValidationError existing
            && Equals(existing.Value, value)
            && SameContext(existing.Context, context))
        {
            return existing;
        }

        return new TypeValidationError(value, cause, context);
    }

    private static bool SameContext(TypeValidationContext? left, TypeValidationContext? right)
    {
        return left?.Field == right?.Field
            && left?.EntityName == right?.EntityName
            && left?.EntityId == right?.EntityId;
    }

    private static string BuildMessage(object? value, object? cause, TypeValidationContext? context)
    {
        var prefix = "Type validation failed";
        if (!string.IsNullOrEmpty(context?.Field))
        {
            prefix += " for " + context!.Field;
        }

        if (!string.IsNullOrEmpty(context?.EntityName) || !string.IsNullOrEmpty(context?.EntityId))
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(context?.EntityName))
            {
                parts.Add(context!.EntityName!);
            }

            if (!string.IsNullOrEmpty(context?.EntityId))
            {
                parts.Add("id: \"" + context!.EntityId + "\"");
            }

            prefix += " (" + string.Join(", ", parts) + ")";
        }

        return prefix + ": Value: " + JsonSerializer.Serialize(value) + ".\nError message: " + ErrorMessages.GetErrorMessage(cause);
    }
}

/// <summary>A provider feature is not supported. Maps to <c>UnsupportedFunctionalityError</c>.</summary>
public sealed class UnsupportedFunctionalityError : SdkError
{
    /// <summary>Creates the error.</summary>
    public UnsupportedFunctionalityError(string functionality, string? message = null)
        : base("AI_UnsupportedFunctionalityError", message ?? ("'" + functionality + "' functionality not supported."), null)
    {
        Functionality = functionality ?? throw new ArgumentNullException(nameof(functionality));
    }

    /// <summary>Feature that is not supported.</summary>
    public string Functionality { get; }

    /// <summary>True when <paramref name="error"/> is an <see cref="UnsupportedFunctionalityError"/>.</summary>
    public static bool IsInstance(object? error)
    {
        return error is UnsupportedFunctionalityError;
    }
}

/// <summary>A provider id is missing from a provider reference. Maps to <c>NoSuchProviderReferenceError</c>.</summary>
public sealed class NoSuchProviderReferenceError : SdkError
{
    /// <summary>Creates the error.</summary>
    public NoSuchProviderReferenceError(string provider, IReadOnlyDictionary<string, string> reference, string? message = null)
        : base(
            "AI_NoSuchProviderReferenceError",
            message ?? ("No provider reference found for provider '" + provider + "'. Available providers: " + string.Join(", ", reference.Keys)),
            null)
    {
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        Reference = reference ?? new Dictionary<string, string>();
    }

    /// <summary>Provider that was requested.</summary>
    public string Provider { get; }

    /// <summary>Reference map that was searched.</summary>
    public IReadOnlyDictionary<string, string> Reference { get; }

    /// <summary>True when <paramref name="error"/> is a <see cref="NoSuchProviderReferenceError"/>.</summary>
    public static bool IsInstance(object? error)
    {
        return error is NoSuchProviderReferenceError;
    }
}

/// <summary>A model option could not be serialized. Maps to <c>SerializationError</c>.</summary>
public sealed class SerializationError : SdkError
{
    /// <summary>Creates the error.</summary>
    public SerializationError(string? message = null, object? cause = null)
        : base("AI_SerializationError", message ?? "Failed to serialize value.", cause)
    {
    }

    /// <summary>True when <paramref name="error"/> is a <see cref="SerializationError"/>.</summary>
    public static bool IsInstance(object? error)
    {
        return error is SerializationError;
    }
}

/// <summary>The response body was empty. Maps to <c>EmptyResponseBodyError</c>.</summary>
public sealed class EmptyResponseBodyError : SdkError
{
    /// <summary>Creates the error.</summary>
    public EmptyResponseBodyError(string? message = null)
        : base("AI_EmptyResponseBodyError", message ?? "Empty response body", null)
    {
    }

    /// <summary>True when <paramref name="error"/> is an <see cref="EmptyResponseBodyError"/>.</summary>
    public static bool IsInstance(object? error)
    {
        return error is EmptyResponseBodyError;
    }
}

/// <summary>A download exceeded its limit or failed. Maps to <c>DownloadError</c>.</summary>
public sealed class DownloadError : SdkError
{
    /// <summary>Creates the error.</summary>
    public DownloadError(string url, string? message = null, int? statusCode = null, string? statusText = null, object? cause = null)
        : base(
            "AI_DownloadError",
            message ?? (cause is null
                ? "Failed to download " + url + ": " + statusCode + " " + statusText
                : "Failed to download " + url + ": " + cause),
            cause)
    {
        Url = url ?? string.Empty;
        StatusCode = statusCode;
        StatusText = statusText;
    }

    /// <summary>URL that was downloaded.</summary>
    public string Url { get; }

    /// <summary>HTTP status, when the failure came from a response.</summary>
    public int? StatusCode { get; }

    /// <summary>HTTP status text, when the failure came from a response.</summary>
    public string? StatusText { get; }

    /// <summary>True when <paramref name="error"/> is a <see cref="DownloadError"/>.</summary>
    public static bool IsInstance(object? error)
    {
        return error is DownloadError;
    }
}

/// <summary>A failed provider HTTP call. Maps to <c>APICallError</c>.</summary>
public sealed class APICallError : SdkError
{
    /// <summary>Creates the error.</summary>
    public APICallError(
        string message,
        string url,
        object? requestBodyValues,
        int? statusCode = null,
        IReadOnlyDictionary<string, string>? responseHeaders = null,
        string? responseBody = null,
        object? cause = null,
        bool? isRetryable = null,
        object? data = null)
        : base("AI_APICallError", message, cause)
    {
        Url = url ?? string.Empty;
        RequestBodyValues = requestBodyValues;
        StatusCode = statusCode;
        ResponseHeaders = responseHeaders;
        ResponseBody = responseBody;
        Data = data;
        IsRetryable = isRetryable ?? (statusCode == 408 || statusCode == 409 || statusCode == 429 || (statusCode != null && statusCode >= 500));
    }

    /// <summary>Request URL.</summary>
    public string Url { get; }

    /// <summary>Request body that was sent.</summary>
    public object? RequestBodyValues { get; }

    /// <summary>HTTP status, when a response was received.</summary>
    public int? StatusCode { get; }

    /// <summary>Response headers.</summary>
    public IReadOnlyDictionary<string, string>? ResponseHeaders { get; }

    /// <summary>Response body text.</summary>
    public string? ResponseBody { get; }

    /// <summary>Parsed provider error payload, when one was read.</summary>
    public new object? Data { get; }

    /// <summary>Whether the call should be retried.</summary>
    public bool IsRetryable { get; }

    /// <summary>True when <paramref name="error"/> is an <see cref="APICallError"/>.</summary>
    public static bool IsInstance(object? error)
    {
        return error is APICallError;
    }
}

/// <summary>DOM <c>DOMException</c> equivalent used by abortable delays.</summary>
public sealed class DomException : Exception
{
    /// <summary>Creates a DOM exception.</summary>
    public DomException(string message, string name)
        : base(message)
    {
        ErrorName = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>DOM exception name, such as <c>AbortError</c>.</summary>
    public string ErrorName { get; }
}

/// <summary>JavaScript <c>Error</c> with an assignable name.</summary>
public sealed class JsError : Exception
{
    /// <summary>Creates an error.</summary>
    public JsError(string message, string? name = null)
        : base(message)
    {
        ErrorName = name ?? "Error";
    }

    /// <summary>Error name.</summary>
    public string ErrorName { get; set; }
}

/// <summary>Sentinel for JavaScript <c>undefined</c> where null is a distinct value.</summary>
public sealed class JsUndefined
{
    private JsUndefined()
    {
    }

    /// <summary>The undefined value.</summary>
    public static JsUndefined Value { get; } = new JsUndefined();
}

/// <summary>Sentinel for a JavaScript symbol.</summary>
public sealed class JsSymbol
{
    /// <summary>Creates a symbol.</summary>
    public JsSymbol(string description)
    {
        Description = description ?? string.Empty;
    }

    /// <summary>Symbol description.</summary>
    public string Description { get; }
}
