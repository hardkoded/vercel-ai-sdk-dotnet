// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.OpenTelemetry;

/// <summary>Status recorded on a span. Code <c>2</c> is error.</summary>
public sealed class AiSpanStatus
{
    /// <summary>Creates a status.</summary>
    public AiSpanStatus(int code, string? message = null)
    {
        Code = code;
        Message = message;
    }

    /// <summary>OpenTelemetry status code.</summary>
    public int Code { get; }

    /// <summary>Status description. Null when the failure was not an exception.</summary>
    public string? Message { get; }
}

/// <summary>One span event.</summary>
public sealed class AiSpanEvent
{
    /// <summary>Creates an event.</summary>
    public AiSpanEvent(string name)
    {
        Name = name ?? string.Empty;
    }

    /// <summary>Event name. Exceptions use <c>exception</c>.</summary>
    public string Name { get; }
}

/// <summary>Span used by <see cref="AiSpanRecorder"/>.</summary>
public interface IAiSpan
{
    /// <summary>Span name.</summary>
    string Name { get; }

    /// <summary>Attributes set on the span.</summary>
    IDictionary<string, object?> Attributes { get; }

    /// <summary>Events recorded on the span.</summary>
    IReadOnlyList<AiSpanEvent> Events { get; }

    /// <summary>Status. Null until one is set.</summary>
    AiSpanStatus? Status { get; }

    /// <summary>True after <see cref="End"/>.</summary>
    bool Ended { get; }

    /// <summary>Sets one attribute.</summary>
    void SetAttribute(string name, object? value);

    /// <summary>Sets the span status.</summary>
    void SetStatus(int code, string? message = null);

    /// <summary>Records an exception event.</summary>
    void RecordException(Exception exception);

    /// <summary>Ends the span.</summary>
    void End();
}

/// <summary>Creates spans for <see cref="AiSpanRecorder"/>.</summary>
public interface IAiTracer
{
    /// <summary>Starts a span that is not made active.</summary>
    IAiSpan StartSpan(string name);

    /// <summary>Starts a span and passes <paramref name="attributes"/> as its initial attributes.</summary>
    IAiSpan StartActiveSpan(string name, IReadOnlyDictionary<string, object?>? attributes);
}

/// <summary>
/// A thrown value that is not an <see cref="Exception"/> in the upstream recorder.
/// <see cref="AiSpanRecorder.RecordError"/> stores status code 2 and does not add an exception event.
/// </summary>
public sealed class AiNonExceptionFailure : Exception
{
    /// <summary>Creates a failure whose <see cref="Value"/> is recorded as a non-exception.</summary>
    public AiNonExceptionFailure(object? value)
        : base(value as string ?? "Non-exception failure.")
    {
        Value = value;
    }

    /// <summary>Original thrown value.</summary>
    public object? Value { get; }
}

/// <summary>Retries exhausted. <see cref="LastError"/> supplies the HTTP status recorded on the span.</summary>
public sealed class AiRetryException : Exception
{
    /// <summary>Creates a retry failure.</summary>
    public AiRetryException(string message, Exception lastError)
        : base(message)
    {
        LastError = lastError ?? throw new ArgumentNullException(nameof(lastError));
    }

    /// <summary>Last provider error.</summary>
    public Exception LastError { get; }
}

/// <summary>Runs work inside a span and records failures on it.</summary>
public static class AiSpanRecorder
{
    /// <summary>Error status code.</summary>
    public const int ErrorStatus = 2;

    /// <summary>Runs <paramref name="action"/> on a span named <paramref name="name"/>.</summary>
    public static Task<T> RecordAsync<T>(
        string name,
        IAiTracer tracer,
        IReadOnlyDictionary<string, object?> attributes,
        Func<IAiSpan, Task<T>> action,
        bool endWhenDone = true)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (tracer is null)
        {
            throw new ArgumentNullException(nameof(tracer));
        }

        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        return RecordCore(name, tracer, Task.FromResult(attributes), action, endWhenDone);
    }

    /// <summary>Runs <paramref name="action"/> after <paramref name="attributes"/> resolves.</summary>
    public static Task<T> RecordAsync<T>(
        string name,
        IAiTracer tracer,
        Task<IReadOnlyDictionary<string, object?>> attributes,
        Func<IAiSpan, Task<T>> action,
        bool endWhenDone = true)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (tracer is null)
        {
            throw new ArgumentNullException(nameof(tracer));
        }

        if (attributes is null)
        {
            throw new ArgumentNullException(nameof(attributes));
        }

        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        return RecordCore(name, tracer, attributes, action, endWhenDone);
    }

    /// <summary>
    /// Sets an error status. Exceptions also record an <c>exception</c> event.
    /// <see cref="ApiException"/> and the last error of <see cref="AiRetryException"/> set <c>http.response.status_code</c>.
    /// </summary>
    public static void RecordError(IAiSpan span, object? error)
    {
        if (span is null)
        {
            throw new ArgumentNullException(nameof(span));
        }

        if (error is AiNonExceptionFailure nonException)
        {
            error = nonException.Value;
        }

        var apiError = error is AiRetryException retry ? retry.LastError : error;
        if (apiError is ApiException api)
        {
            span.SetAttribute("http.response.status_code", api.StatusCode);
        }

        if (error is Exception exception && error is not AiNonExceptionFailure)
        {
            span.RecordException(exception);
            span.SetStatus(ErrorStatus, exception.Message);
            return;
        }

        span.SetStatus(ErrorStatus);
    }

    private static async Task<T> RecordCore<T>(
        string name,
        IAiTracer tracer,
        Task<IReadOnlyDictionary<string, object?>> attributes,
        Func<IAiSpan, Task<T>> action,
        bool endWhenDone)
    {
        var resolved = await attributes.ConfigureAwait(false);
        var span = tracer.StartActiveSpan(name, resolved);
        try
        {
            var result = await action(span).ConfigureAwait(false);
            if (endWhenDone)
            {
                span.End();
            }

            return result;
        }
        catch (Exception error)
        {
            try
            {
                RecordError(span, error);
            }
            finally
            {
                span.End();
            }

            throw;
        }
    }
}
