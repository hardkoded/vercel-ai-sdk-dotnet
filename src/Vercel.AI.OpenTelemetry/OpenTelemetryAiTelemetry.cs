// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

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

        activity.SetTag("gen_ai.operation.name", operation);
        activity.SetTag("gen_ai.request.model", modelId);
        return activity;
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
