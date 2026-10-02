// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using Vercel.AI.Provider;

namespace Vercel.AI.OpenTelemetry;

/// <summary>Starts an Activity span and records errors on it.</summary>
public static class TelemetrySpan
{
    /// <summary>
    /// Runs <paramref name="action"/> inside a span named <paramref name="name"/>.
    /// An exception is recorded, the span is stopped, and the exception is rethrown.
    /// </summary>
    /// <typeparam name="T">Result of <paramref name="action"/>.</typeparam>
    /// <param name="name">Span name.</param>
    /// <param name="action">Work to run. The current span is null when nothing is listening.</param>
    /// <param name="attributes">Tags applied before the action runs. Null adds none.</param>
    /// <param name="endWhenDone">When false, a successful span stays open. Failures still stop the span.</param>
    /// <param name="activitySource">Source for the span. Null uses <see cref="OpenTelemetryAiTelemetry.ActivitySource"/>.</param>
    public static async Task<T> RecordAsync<T>(
        string name,
        Func<Activity?, Task<T>> action,
        Task<IReadOnlyDictionary<string, object?>>? attributes = null,
        bool endWhenDone = true,
        ActivitySource? activitySource = null)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        IReadOnlyDictionary<string, object?>? tags = null;
        if (attributes != null)
        {
            tags = await attributes.ConfigureAwait(false);
        }

        var activity = (activitySource ?? OpenTelemetryAiTelemetry.ActivitySource).StartActivity(name, ActivityKind.Internal);
        if (activity != null && tags != null)
        {
            foreach (var pair in tags)
            {
                if (pair.Value != null)
                {
                    activity.SetTag(pair.Key, pair.Value);
                }
            }
        }

        try
        {
            var result = await action(activity).ConfigureAwait(false);
            if (endWhenDone)
            {
                activity?.Dispose();
            }

            return result;
        }
        catch (Exception exception)
        {
            if (activity != null)
            {
                RecordError(activity, exception);
                activity.Dispose();
            }

            throw;
        }
    }

    /// <summary>
    /// Sets the span status to error.
    /// Exceptions also record an exception event and, for <see cref="ApiException"/>, the HTTP status code.
    /// </summary>
    /// <param name="activity">Span to update.</param>
    /// <param name="error">Exception or other error value.</param>
    public static void RecordError(Activity activity, object error)
    {
        if (activity is null)
        {
            throw new ArgumentNullException(nameof(activity));
        }

        if (error is null)
        {
            throw new ArgumentNullException(nameof(error));
        }

        if (error is Exception exception)
        {
#if NET
            activity.AddEvent(new ActivityEvent(
                "exception",
                tags: new ActivityTagsCollection
                {
                    { "exception.type", exception.GetType().FullName ?? "Exception" },
                    { "exception.message", exception.Message ?? string.Empty },
                    { "exception.stacktrace", exception.StackTrace ?? string.Empty },
                }));
#endif
            activity.SetStatus(ActivityStatusCode.Error, exception.Message);
            if (exception is ApiException api)
            {
                activity.SetTag("http.response.status_code", api.StatusCode);
            }

            return;
        }

        activity.SetStatus(ActivityStatusCode.Error);
    }
}
