// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Operations;

/// <summary>
/// Hands <c>rerank</c> events to the telemetry integrations. Maps to <c>createRestrictedTelemetryDispatcher</c> in
/// <c>rerank/restricted-telemetry-dispatcher.ts</c>. User callbacks keep the original runtime context, integrations only see the
/// keys <see cref="TelemetryOptions.IncludeRuntimeContext"/> includes. The port keeps it apart from the other restricted dispatchers
/// because they share the upstream function name.
/// </summary>
internal sealed class RerankRestrictedTelemetryDispatcher
{
    private readonly TelemetryDispatcher _dispatcher;
    private readonly TelemetryOptions? _telemetry;

    private RerankRestrictedTelemetryDispatcher(TelemetryOptions? telemetry)
    {
        _telemetry = telemetry;
        _dispatcher = TelemetryDispatcher.Create(telemetry);
    }

    /// <summary>Maps to <c>createRestrictedTelemetryDispatcher</c>.</summary>
    internal static RerankRestrictedTelemetryDispatcher CreateRestrictedTelemetryDispatcher(
        TelemetryOptions? telemetry
    )
    {
        return new RerankRestrictedTelemetryDispatcher(telemetry);
    }

    /// <summary>Maps to <c>runInTracingChannelSpan</c>, which the restricted dispatcher spreads from the telemetry dispatcher.</summary>
    internal Task<T> RunInTracingChannelSpan<T>(string type, object @event, Func<Task<T>> execute)
    {
        return _dispatcher.RunInTracingChannelSpan(type, @event, execute);
    }

    /// <summary>Maps to <c>onStart</c>.</summary>
    internal Task OnStartAsync(RerankStartEvent source, CancellationToken cancellationToken)
    {
        return _dispatcher.OnStart?.Invoke(
            new RerankStartEvent(
                source.CallId,
                source.OperationId,
                Embed.FilterContext(source.RuntimeContext, _telemetry?.IncludeRuntimeContext),
                source.Provider,
                source.ModelId,
                source.Documents,
                source.Query,
                source.TopN,
                source.MaxRetries,
                source.Headers,
                source.ProviderOptions
            ),
            cancellationToken
        )
            ?? Task.CompletedTask;
    }

    /// <summary>Maps to <c>onEnd</c>.</summary>
    internal Task OnEndAsync(RerankEndEvent source, CancellationToken cancellationToken)
    {
        return _dispatcher.OnEnd?.Invoke(
            new RerankEndEvent(
                source.CallId,
                source.OperationId,
                Embed.FilterContext(source.RuntimeContext, _telemetry?.IncludeRuntimeContext),
                source.Provider,
                source.ModelId,
                source.Documents,
                source.Query,
                source.Ranking,
                source.Warnings,
                source.ProviderMetadata,
                source.Response
            ),
            cancellationToken
        )
            ?? Task.CompletedTask;
    }

    /// <summary>Maps to <c>onRerankStart</c>.</summary>
    internal Task OnRerankStartAsync(
        RerankingModelCallStartEvent source,
        CancellationToken cancellationToken
    )
    {
        return _dispatcher.OnRerankStart?.Invoke(source, cancellationToken) ?? Task.CompletedTask;
    }

    /// <summary>Maps to <c>onRerankEnd</c>.</summary>
    internal Task OnRerankEndAsync(
        RerankingModelCallEndEvent source,
        CancellationToken cancellationToken
    )
    {
        return _dispatcher.OnRerankEnd?.Invoke(source, cancellationToken) ?? Task.CompletedTask;
    }

    /// <summary>Maps to <c>onError</c>.</summary>
    internal Task OnErrorAsync(string callId, Exception error, CancellationToken cancellationToken)
    {
        return _dispatcher.OnError?.Invoke(
            new TelemetryErrorEvent(callId, error),
            cancellationToken
        ) ?? Task.CompletedTask;
    }
}
