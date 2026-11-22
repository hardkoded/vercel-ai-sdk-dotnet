// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Operations;

/// <summary>
/// Hands <c>embed</c> and <c>embedMany</c> events to the telemetry integrations. Maps to <c>createRestrictedTelemetryDispatcher</c> in
/// <c>embed/restricted-telemetry-dispatcher.ts</c>. User callbacks keep the original runtime context, integrations only see the
/// keys <see cref="TelemetryOptions.IncludeRuntimeContext"/> includes. The port keeps it apart from the other restricted dispatchers
/// because they share the upstream function name.
/// </summary>
internal sealed class EmbedRestrictedTelemetryDispatcher
{
    private readonly TelemetryDispatcher _dispatcher;
    private readonly TelemetryOptions? _telemetry;

    private EmbedRestrictedTelemetryDispatcher(TelemetryOptions? telemetry)
    {
        _telemetry = telemetry;
        _dispatcher = TelemetryDispatcher.Create(telemetry);
    }

    /// <summary>Maps to <c>createRestrictedTelemetryDispatcher</c>.</summary>
    internal static EmbedRestrictedTelemetryDispatcher CreateRestrictedTelemetryDispatcher(
        TelemetryOptions? telemetry
    )
    {
        return new EmbedRestrictedTelemetryDispatcher(telemetry);
    }

    /// <summary>Maps to <c>onStart</c>.</summary>
    internal Task OnStartAsync(EmbedStartEvent source, CancellationToken cancellationToken)
    {
        return _dispatcher.OnStart?.Invoke(
            new EmbedStartEvent(
                source.CallId,
                source.OperationId,
                Embed.FilterContext(source.RuntimeContext, _telemetry?.IncludeRuntimeContext),
                source.Provider,
                source.ModelId,
                source.Value,
                source.MaxRetries,
                source.Headers,
                source.ProviderOptions,
                source.Dimensions
            ),
            cancellationToken
        )
            ?? Task.CompletedTask;
    }

    /// <summary>Maps to <c>onEnd</c>.</summary>
    internal Task OnEndAsync(EmbedEndEvent source, CancellationToken cancellationToken)
    {
        return _dispatcher.OnEnd?.Invoke(
            new EmbedEndEvent(
                source.CallId,
                source.OperationId,
                Embed.FilterContext(source.RuntimeContext, _telemetry?.IncludeRuntimeContext),
                source.Provider,
                source.ModelId,
                source.Value,
                source.Embedding,
                source.Usage,
                source.Warnings,
                source.ProviderMetadata,
                source.Response
            ),
            cancellationToken
        )
            ?? Task.CompletedTask;
    }

    /// <summary>Maps to <c>onEmbedStart</c>.</summary>
    internal Task OnEmbedStartAsync(
        EmbeddingModelCallStartEvent source,
        CancellationToken cancellationToken
    )
    {
        return _dispatcher.OnEmbedStart?.Invoke(source, cancellationToken) ?? Task.CompletedTask;
    }

    /// <summary>Maps to <c>onEmbedEnd</c>.</summary>
    internal Task OnEmbedEndAsync(
        EmbeddingModelCallEndEvent source,
        CancellationToken cancellationToken
    )
    {
        return _dispatcher.OnEmbedEnd?.Invoke(source, cancellationToken) ?? Task.CompletedTask;
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
