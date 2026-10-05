// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text;
using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Operations;

/// <summary>Package version used in the <c>ai/</c> user-agent suffix.</summary>
public static class AiSdkVersion
{
    /// <summary>Version sent when a package version is not injected at build time.</summary>
    public const string Version = "0.0.0-test";

    /// <summary>User-agent token appended to provider requests.</summary>
    public const string UserAgent = "ai/" + Version;
}

/// <summary>A warning returned by a model call.</summary>
public sealed class OperationWarning
{
    /// <summary>Creates a warning.</summary>
    public OperationWarning(string type, string? message = null, string? feature = null, string? details = null, string? setting = null)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Message = message;
        Feature = feature;
        Details = details;
        Setting = setting;
    }

    /// <summary>Warning type: <c>unsupported</c>, <c>compatibility</c>, <c>deprecated</c>, or <c>other</c>.</summary>
    public string Type { get; }

    /// <summary>Human-readable message for <c>other</c> warnings.</summary>
    public string? Message { get; }

    /// <summary>Feature name for unsupported or compatibility warnings.</summary>
    public string? Feature { get; }

    /// <summary>Extra detail for unsupported or compatibility warnings.</summary>
    public string? Details { get; }

    /// <summary>Setting name for deprecated warnings.</summary>
    public string? Setting { get; }

    /// <summary>Creates an <c>other</c> warning.</summary>
    public static OperationWarning Other(string message)
    {
        return new OperationWarning("other", message: message);
    }

    /// <summary>Creates an <c>unsupported</c> warning.</summary>
    public static OperationWarning Unsupported(string feature, string? details = null)
    {
        return new OperationWarning("unsupported", feature: feature, details: details);
    }
}

/// <summary>Arguments passed to the warning logger.</summary>
public sealed class WarningLogContext
{
    /// <summary>Creates a log context.</summary>
    public WarningLogContext(IReadOnlyList<OperationWarning> warnings, string? provider, string? model)
    {
        Warnings = warnings ?? Array.Empty<OperationWarning>();
        Provider = provider;
        Model = model;
    }

    /// <summary>Warnings from the call.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }

    /// <summary>Provider id.</summary>
    public string? Provider { get; }

    /// <summary>Model id.</summary>
    public string? Model { get; }
}

/// <summary>
/// Warning logger. <see cref="Observer"/> sees every call, including empty warning lists.
/// <see cref="Logger"/> maps to <c>globalThis.AI_SDK_LOG_WARNINGS</c>.
/// </summary>
public static class WarningLog
{
    /// <summary>Receives every log attempt, including an empty warning list.</summary>
    public static Action<WarningLogContext>? Observer { get; set; }

    /// <summary>
    /// Custom logger. <c>false</c> suppresses logging. A delegate receives nonempty warnings.
    /// </summary>
    public static object? Logger { get; set; }

    /// <summary>Records warnings for one model call.</summary>
    public static void Write(IReadOnlyList<OperationWarning>? warnings, string? provider, string? model)
    {
        var context = new WarningLogContext(warnings ?? Array.Empty<OperationWarning>(), provider, model);
        var observer = Observer;
        if (observer != null)
        {
            observer(context);
        }

        if (context.Warnings.Count == 0)
        {
            return;
        }

        if (Logger is bool suppressed && suppressed == false)
        {
            return;
        }

        if (Logger is Action<WarningLogContext> logger)
        {
            logger(context);
        }
    }
}

/// <summary>Invalid caller argument. Maps to <c>InvalidArgumentError</c>.</summary>
public sealed class InvalidArgumentException : AiSdkException
{
    /// <summary>Creates the exception.</summary>
    public InvalidArgumentException(string parameter, object? value, string message)
        : base("Invalid argument for parameter " + parameter + ": " + message)
    {
        Parameter = parameter;
        Value = value;
    }

    /// <summary>Parameter name.</summary>
    public string Parameter { get; }

    /// <summary>Rejected value.</summary>
    public object? Value { get; }
}

/// <summary>The provider response did not match the expected shape.</summary>
public sealed class InvalidResponseDataException : AiSdkException
{
    /// <summary>Creates the exception.</summary>
    public InvalidResponseDataException(object? data, string message)
        : base(message)
    {
        Data = data;
    }

    /// <summary>Response payload that failed validation.</summary>
    public new object? Data { get; }
}

/// <summary>The provider does not implement the requested operation.</summary>
public sealed class UnsupportedFunctionalityException : AiSdkException
{
    /// <summary>Creates the exception.</summary>
    public UnsupportedFunctionalityException(string functionality, string message)
        : base(message)
    {
        Functionality = functionality;
    }

    /// <summary>Short functionality name.</summary>
    public string Functionality { get; }
}

/// <summary>The model specification version is not supported.</summary>
public sealed class UnsupportedModelVersionException : AiSdkException
{
    /// <summary>Creates the exception.</summary>
    public UnsupportedModelVersionException(string version, string provider, string modelId)
        : base("Unsupported model version " + version + " for provider \"" + provider + "\" and model \"" + modelId + "\". AI SDK 5 only supports models that implement specification version \"v2\".")
    {
        Version = version;
        Provider = provider;
        ModelId = modelId;
    }

    /// <summary>Rejected specification version.</summary>
    public string Version { get; }

    /// <summary>Provider id.</summary>
    public string Provider { get; }

    /// <summary>Model id.</summary>
    public string ModelId { get; }
}

/// <summary>A provider HTTP failure that may be retried.</summary>
public class RetryableCallException : AiSdkException
{
    /// <summary>Creates a retryable provider error.</summary>
    public RetryableCallException(string message, int? statusCode, IReadOnlyDictionary<string, string>? responseHeaders, bool? isRetryable = null)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseHeaders = responseHeaders;
        IsRetryable = isRetryable ?? (statusCode == 408 || statusCode == 409 || statusCode == 429 || (statusCode != null && statusCode >= 500));
    }

    /// <summary>HTTP status, when the failure came from HTTP.</summary>
    public int? StatusCode { get; }

    /// <summary>Response headers, including <c>retry-after-ms</c> when present.</summary>
    public IReadOnlyDictionary<string, string>? ResponseHeaders { get; }

    /// <summary>Whether the operation retry loop should try again.</summary>
    public bool IsRetryable { get; }
}

/// <summary>Retries were exhausted. <see cref="LastError"/> is the final attempt.</summary>
public sealed class OperationRetryException : AiSdkException
{
    /// <summary>Creates a retry failure.</summary>
    public OperationRetryException(string message, string reason, IReadOnlyList<Exception> errors)
        : base(message)
    {
        Reason = reason;
        Errors = errors ?? Array.Empty<Exception>();
        LastError = Errors.Count == 0 ? null : Errors[Errors.Count - 1];
    }

    /// <summary><c>maxRetriesExceeded</c> or <c>errorNotRetryable</c>.</summary>
    public string Reason { get; }

    /// <summary>Errors from each attempt.</summary>
    public IReadOnlyList<Exception> Errors { get; }

    /// <summary>The last attempt's error.</summary>
    public Exception? LastError { get; }
}

/// <summary>No speech audio was produced.</summary>
public sealed class NoSpeechGeneratedException : AiSdkException
{
    /// <summary>Creates the exception.</summary>
    public NoSpeechGeneratedException(IReadOnlyList<ProviderResponse> responses)
        : base("No speech audio generated.")
    {
        Responses = responses ?? Array.Empty<ProviderResponse>();
    }

    /// <summary>Provider responses included with the failure.</summary>
    public IReadOnlyList<ProviderResponse> Responses { get; }
}

/// <summary>No transcript text was produced.</summary>
public sealed class NoTranscriptGeneratedException : AiSdkException
{
    /// <summary>Creates the exception.</summary>
    public NoTranscriptGeneratedException(IReadOnlyList<ProviderResponse> responses)
        : base("No transcript generated.")
    {
        Responses = responses ?? Array.Empty<ProviderResponse>();
    }

    /// <summary>Provider responses included with the failure.</summary>
    public IReadOnlyList<ProviderResponse> Responses { get; }
}

/// <summary>No translation text or audio was produced.</summary>
public sealed class NoTranslationGeneratedException : AiSdkException
{
    /// <summary>Creates the exception.</summary>
    public NoTranslationGeneratedException(ProviderResponse response)
        : base("No translation generated.")
    {
        Response = response;
    }

    /// <summary>Provider response included with the failure.</summary>
    public ProviderResponse Response { get; }
}

/// <summary>No video was produced.</summary>
public sealed class NoVideoGeneratedException : AiSdkException
{
    /// <summary>Creates the exception.</summary>
    public NoVideoGeneratedException(IReadOnlyList<ProviderResponse> responses)
        : base("No video generated.")
    {
        Responses = responses ?? Array.Empty<ProviderResponse>();
    }

    /// <summary>Provider responses included with the failure.</summary>
    public IReadOnlyList<ProviderResponse> Responses { get; }
}

/// <summary>Downloading generated media failed.</summary>
public sealed class DownloadException : AiSdkException
{
    /// <summary>Creates the exception.</summary>
    public DownloadException(string url, int statusCode, string? statusText)
        : base("Failed to download " + url + ": " + statusCode.ToString(CultureInfo.InvariantCulture) + " " + (statusText ?? string.Empty).Trim())
    {
        Url = url;
        StatusCode = statusCode;
    }

    /// <summary>URL that failed.</summary>
    public string Url { get; }

    /// <summary>HTTP status.</summary>
    public int StatusCode { get; }
}

/// <summary>The evaluation model does not support a question type.</summary>
public sealed class EvaluationUnsupportedQuestionTypeException : AiSdkException
{
    /// <summary>Creates the exception.</summary>
    public EvaluationUnsupportedQuestionTypeException(string questionId, string questionType, string provider, string modelId)
        : base("Question \"" + questionId + "\" has unsupported type \"" + questionType + "\".")
    {
        QuestionId = questionId;
        QuestionType = questionType;
        Provider = provider;
        ModelId = modelId;
    }

    /// <summary>Question id.</summary>
    public string QuestionId { get; }

    /// <summary>Rejected question type.</summary>
    public string QuestionType { get; }

    /// <summary>Provider id.</summary>
    public string Provider { get; }

    /// <summary>Model id.</summary>
    public string ModelId { get; }
}

/// <summary>Headers and body returned by one provider call.</summary>
public sealed class ProviderResponse
{
    /// <summary>Creates response metadata.</summary>
    public ProviderResponse(IReadOnlyDictionary<string, string>? headers = null, JsonElement? body = null, string? id = null, DateTime? timestamp = null, string? modelId = null, JsonElement? providerMetadata = null)
    {
        Headers = headers;
        Body = body;
        Id = id;
        Timestamp = timestamp;
        ModelId = modelId;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Response headers.</summary>
    public IReadOnlyDictionary<string, string>? Headers { get; }

    /// <summary>Response body.</summary>
    public JsonElement? Body { get; }

    /// <summary>Provider response id.</summary>
    public string? Id { get; }

    /// <summary>Provider timestamp.</summary>
    public DateTime? Timestamp { get; }

    /// <summary>Model id reported by the provider.</summary>
    public string? ModelId { get; }

    /// <summary>Provider metadata stored on the response, when the call includes it there.</summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>Token counts for an image or evaluation call.</summary>
public sealed class OperationUsage
{
    /// <summary>Creates usage. Unset counts stay null.</summary>
    public OperationUsage(int? inputTokens = null, int? outputTokens = null, int? totalTokens = null, double? tokens = null)
    {
        InputTokens = inputTokens;
        OutputTokens = outputTokens;
        TotalTokens = totalTokens;
        Tokens = tokens;
    }

    /// <summary>Prompt tokens.</summary>
    public int? InputTokens { get; }

    /// <summary>Generated tokens.</summary>
    public int? OutputTokens { get; }

    /// <summary>Input plus output, when both are known.</summary>
    public int? TotalTokens { get; }

    /// <summary>Embedding token count. <see cref="double.NaN"/> when the provider omitted usage.</summary>
    public double? Tokens { get; }

    /// <summary>Adds two image usage values. A missing count stays missing until one side reports it.</summary>
    public static OperationUsage AddImage(OperationUsage left, OperationUsage right)
    {
        return new OperationUsage(
            Add(left.InputTokens, right.InputTokens),
            Add(left.OutputTokens, right.OutputTokens),
            Add(left.TotalTokens, right.TotalTokens));
    }

    private static int? Add(int? left, int? right)
    {
        if (left is null && right is null)
        {
            return null;
        }

        return (left ?? 0) + (right ?? 0);
    }
}

/// <summary>Shared request fields for an operation call.</summary>
public class OperationRequest
{
    /// <summary>Maximum retries after the first attempt. Default is 2.</summary>
    public int? MaxRetries { get; set; }

    /// <summary>Cancels the call.</summary>
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Exception thrown when <see cref="CancellationToken"/> is already cancelled.</summary>
    public Exception? AbortReason { get; set; }

    /// <summary>Extra HTTP headers.</summary>
    public IReadOnlyDictionary<string, string>? Headers { get; set; }

    /// <summary>Provider-specific options.</summary>
    public JsonElement? ProviderOptions { get; set; }

    /// <summary>Values copied onto user callbacks. Telemetry receives the filtered copy.</summary>
    public IReadOnlyDictionary<string, object?>? RuntimeContext { get; set; }

    /// <summary>Supplies call ids. Tests pass a constant generator.</summary>
    public Func<string>? GenerateCallId { get; set; }

    /// <summary>Clock used when a provider omits a timestamp.</summary>
    public Func<DateTime>? Now { get; set; }
}

/// <summary>Header and retry helpers shared by the operation types.</summary>
public static class OperationHeaders
{
    /// <summary>Lowercases header names and appends the AI SDK user-agent.</summary>
    public static Dictionary<string, string> WithUserAgent(IReadOnlyDictionary<string, string>? headers, string suffix)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        if (headers != null)
        {
            foreach (var pair in headers)
            {
                if (pair.Key == null || pair.Value == null)
                {
                    continue;
                }

                result[pair.Key.ToLowerInvariant()] = pair.Value;
            }
        }

        result.TryGetValue("user-agent", out var current);
        var combined = string.IsNullOrEmpty(current) ? suffix : current + " " + suffix;
        if (!string.IsNullOrEmpty(combined))
        {
            result["user-agent"] = combined;
        }

        return result;
    }

    /// <summary>Reads a header, ignoring case and parameters after <c>;</c>.</summary>
    public static string? MediaTypeFromContentType(IReadOnlyDictionary<string, string>? headers, string requiredPrefix)
    {
        if (headers == null)
        {
            return null;
        }

        string? mediaType = null;
        foreach (var pair in headers)
        {
            if (string.Equals(pair.Key, "content-type", StringComparison.OrdinalIgnoreCase))
            {
                mediaType = pair.Value;
            }
        }

        if (string.IsNullOrEmpty(mediaType))
        {
            return null;
        }

        var separator = mediaType!.IndexOf(';');
        var normalized = (separator < 0 ? mediaType : mediaType.Substring(0, separator)).Trim().ToLowerInvariant();
        if (normalized.Length == 0 || !normalized.StartsWith(requiredPrefix, StringComparison.Ordinal))
        {
            return null;
        }

        return normalized;
    }
}

/// <summary>Retries a provider call the way <c>prepareRetries</c> does.</summary>
public static class OperationRetry
{
    /// <summary>Validates <paramref name="maxRetries"/> and runs <paramref name="action"/>.</summary>
    public static Task<T> ExecuteAsync<T>(
        int? maxRetries,
        CancellationToken cancellationToken,
        Exception? abortReason,
        Func<CancellationToken, Task<T>> action,
        Func<Exception, bool>? additionalRetryable)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        var resolved = ResolveMaxRetries(maxRetries);
        return ExecuteResolvedAsync(resolved, cancellationToken, abortReason, action, additionalRetryable, 2000, new List<Exception>());
    }

    /// <summary>Returns the retry count after validation. Null becomes 2.</summary>
    public static int ResolveMaxRetries(int? maxRetries, string parameter = "maxRetries")
    {
        if (maxRetries is int value)
        {
            if (value < 0)
            {
                throw new InvalidArgumentException(parameter, value, parameter + " must be >= 0");
            }
        }

        return maxRetries ?? 2;
    }

    private static async Task<T> ExecuteResolvedAsync<T>(
        int maxRetries,
        CancellationToken cancellationToken,
        Exception? abortReason,
        Func<CancellationToken, Task<T>> action,
        Func<Exception, bool>? additionalRetryable,
        int delayInMs,
        List<Exception> errors)
    {
        ThrowIfAborted(cancellationToken, abortReason);
        try
        {
            return await action(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception error)
        {
            if (error is OperationCanceledException)
            {
                throw;
            }

            if (maxRetries == 0)
            {
                throw;
            }

            errors.Add(error);
            var tryNumber = errors.Count;
            if (tryNumber > maxRetries)
            {
                throw new OperationRetryException(
                    "Failed after " + tryNumber.ToString(CultureInfo.InvariantCulture) + " attempts. Last error: " + error.Message,
                    "maxRetriesExceeded",
                    errors);
            }

            var retryable = error is RetryableCallException call && call.IsRetryable;
            if (!retryable && additionalRetryable != null)
            {
                retryable = additionalRetryable(error);
            }

            if (retryable && tryNumber <= maxRetries)
            {
                var delay = DelayFor(error, delayInMs);
                if (delay > 0)
                {
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                }

                ThrowIfAborted(cancellationToken, abortReason);
                return await ExecuteResolvedAsync(maxRetries, cancellationToken, abortReason, action, additionalRetryable, delayInMs * 2, errors).ConfigureAwait(false);
            }

            if (tryNumber == 1)
            {
                throw;
            }

            throw new OperationRetryException(
                "Failed after " + tryNumber.ToString(CultureInfo.InvariantCulture) + " attempts. Last error: " + error.Message,
                "errorNotRetryable",
                errors);
        }
    }

    /// <summary>Throws <paramref name="reason"/> when the token is cancelled.</summary>
    public static void ThrowIfAborted(CancellationToken cancellationToken, Exception? reason)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (reason != null)
        {
            throw reason;
        }

        throw new OperationCanceledException(cancellationToken);
    }

    private static int DelayFor(Exception error, int exponentialBackoffDelay)
    {
        var headers = (error as RetryableCallException)?.ResponseHeaders;
        if (headers == null)
        {
            return exponentialBackoffDelay;
        }

        if (headers.TryGetValue("retry-after-ms", out var retryAfterMs) && double.TryParse(retryAfterMs, NumberStyles.Float, CultureInfo.InvariantCulture, out var milliseconds))
        {
            if (milliseconds >= 0 && (milliseconds < 60000 || milliseconds < exponentialBackoffDelay))
            {
                return (int)milliseconds;
            }
        }

        return exponentialBackoffDelay;
    }
}

/// <summary>Invokes callbacks and swallows their exceptions.</summary>
public static class OperationCallbacks
{
    /// <summary>Invokes each callback. A thrown callback does not fail the operation.</summary>
    public static async Task NotifyAsync<T>(T payload, params Func<T, Task>?[] callbacks)
    {
        if (callbacks == null)
        {
            return;
        }

        var pending = new List<Task>();
        foreach (var callback in callbacks)
        {
            if (callback == null)
            {
                continue;
            }

            pending.Add(InvokeAsync(callback, payload));
        }

        if (pending.Count > 0)
        {
            await Task.WhenAll(pending).ConfigureAwait(false);
        }
    }

    private static async Task InvokeAsync<T>(Func<T, Task> callback, T payload)
    {
        try
        {
            await callback(payload).ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Callback failures are isolated from the operation.
        }
    }
}

/// <summary>Detects media types from file signatures.</summary>
public static class MediaTypeDetector
{
    /// <summary>Returns a media type for <paramref name="data"/>, or null when no signature matches.</summary>
    public static string? Detect(byte[]? data, string? topLevelType)
    {
        return data == null || data.Length == 0 ? null : MediaTypes.DetectMediaType(data, topLevelType);
    }

    /// <summary>Decodes a base64 payload or returns the bytes unchanged.</summary>
    public static byte[] ToBytes(object? data)
    {
        if (data is byte[] bytes)
        {
            return bytes;
        }

        if (data is string text)
        {
            if (text.StartsWith("data:", StringComparison.Ordinal))
            {
                var comma = text.IndexOf(',');
                text = comma >= 0 ? text.Substring(comma + 1) : string.Empty;
            }

            return Convert.FromBase64String(text);
        }

        return Array.Empty<byte>();
    }
}

/// <summary>JSON helpers used while normalizing provider options.</summary>
public static class OperationJson
{
    /// <summary>Parses a JSON string.</summary>
    public static JsonElement Parse(string json)
    {
        using (var document = JsonDocument.Parse(json))
        {
            return document.RootElement.Clone();
        }
    }

    /// <summary>Serializes a value.</summary>
    public static JsonElement Serialize<T>(T value)
    {
        return Parse(JsonSerializer.Serialize(value));
    }

    /// <summary>Reads a property or returns null.</summary>
    public static JsonElement? Get(JsonElement? element, string name)
    {
        if (element is JsonElement value && value.ValueKind == JsonValueKind.Object && value.TryGetProperty(name, out var property))
        {
            return property;
        }

        return null;
    }

    /// <summary>UTF-8 size of a string.</summary>
    public static int Utf8Length(string value)
    {
        return Encoding.UTF8.GetByteCount(value ?? string.Empty);
    }
}
