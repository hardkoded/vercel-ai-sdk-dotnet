// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Provider;

namespace Vercel.AI.OpenTelemetry;

/// <summary><see cref="IAiTelemetry"/> that records spans on <see cref="ActivitySource"/>.</summary>
public sealed class OpenTelemetryAiTelemetry : IAiTelemetry
{
    /// <summary>Activity source name. Add this source to a <c>TracerProvider</c>.</summary>
    public const string SourceName = "Vercel.AI";

    /// <summary>Source used by <see cref="Begin"/>.</summary>
    public static readonly ActivitySource ActivitySource = new(SourceName);

    /// <inheritdoc />
    public IDisposable Begin(string operation, string modelId)
    {
        var activity = ActivitySource.StartActivity(operation, ActivityKind.Client);
        if (activity is null)
        {
            return Empty.Instance;
        }

        activity.SetTag("gen_ai.operation.name", operation == null ? null : GenAiConventions.MapOperationName(operation));
        activity.SetTag("gen_ai.request.model", modelId);
        return activity;
    }

    /// <inheritdoc />
    public void OnFinish(IDisposable span, FinishReason finishReason)
    {
        if (span is not Activity activity)
        {
            return;
        }

        activity.SetTag("gen_ai.response.finish_reasons", new[] { GenAiConventions.FormatFinishReason(finishReason) });
        if (finishReason == FinishReason.Error)
        {
            activity.SetStatus(ActivityStatusCode.Error);
        }
    }

    /// <inheritdoc />
    public void OnUsage(IDisposable span, LanguageModelUsage usage)
    {
        if (span is not Activity activity)
        {
            return;
        }

        SetCount(activity, "gen_ai.usage.input_tokens", usage.InputTokens);
        SetCount(activity, "gen_ai.usage.output_tokens", usage.OutputTokens);
        SetCount(activity, "gen_ai.usage.total_tokens", usage.TotalTokens);
        SetCount(activity, "gen_ai.usage.cache_read.input_tokens", usage.CacheReadTokens);
        SetCount(activity, "gen_ai.usage.cache_creation.input_tokens", usage.CacheWriteTokens);
    }

    /// <inheritdoc />
    public void OnUsage(IDisposable span, AudioUsage? usage)
    {
        if (span is not Activity activity || usage is null)
        {
            return;
        }

        SetCount(activity, "gen_ai.usage.characters", usage.Characters);
        if (usage.Seconds is double seconds && IsFinite(seconds))
        {
            activity.SetTag("gen_ai.usage.seconds", seconds);
        }

        SetCount(activity, "gen_ai.usage.input_tokens", usage.InputTokens);
        SetCount(activity, "gen_ai.usage.output_tokens", usage.OutputTokens);
        SetCount(activity, "gen_ai.usage.total_tokens", usage.TotalTokens);
    }

    private static void SetCount(Activity activity, string name, int? value)
    {
        if (value is int number)
        {
            activity.SetTag(name, number);
        }
    }

    private static bool IsFinite(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value);
    }

    private sealed class Empty : IDisposable
    {
        public static readonly Empty Instance = new();

        public void Dispose()
        {
        }
    }
}

/// <summary>Registers <see cref="OpenTelemetryAiTelemetry"/>.</summary>
public static class OpenTelemetryServiceCollectionExtensions
{
    /// <summary>Registers <see cref="IAiTelemetry"/> so <c>AddAiSdk</c> can attach spans.</summary>
    public static IServiceCollection AddAiSdkOpenTelemetry(this IServiceCollection services)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddSingleton<IAiTelemetry, OpenTelemetryAiTelemetry>();
        return services;
    }
}
