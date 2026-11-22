// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Operations;

/// <summary>
/// Hands decision events to the telemetry integrations. Maps to <c>createRestrictedTelemetryDispatcher</c> in
/// <c>decide/restricted-telemetry-dispatcher.ts</c>. The port keeps it apart from the extract dispatcher because both share the
/// upstream function name. Recording flags, the SDK version and the content hiding come from <see cref="TelemetryDispatcher"/>.
/// </summary>
internal sealed class DecideRestrictedTelemetryDispatcher
{
    private readonly TelemetryDispatcher _dispatcher;
    private readonly TelemetryOptions? _telemetry;

    private DecideRestrictedTelemetryDispatcher(TelemetryOptions? telemetry)
    {
        _telemetry = telemetry;
        _dispatcher = TelemetryDispatcher.Create(telemetry);
    }

    /// <summary>Maps to <c>createRestrictedTelemetryDispatcher</c>.</summary>
    internal static DecideRestrictedTelemetryDispatcher CreateRestrictedTelemetryDispatcher(
        TelemetryOptions? telemetry
    )
    {
        return new DecideRestrictedTelemetryDispatcher(telemetry);
    }

    /// <summary>Maps to <c>runInTracingChannelSpan</c>, which the restricted dispatcher spreads from the telemetry dispatcher.</summary>
    internal Task<T> RunInTracingChannelSpan<T>(string type, object @event, Func<Task<T>> execute)
    {
        return _dispatcher.RunInTracingChannelSpan(type, @event, execute);
    }

    /// <summary>Maps to <c>onStart</c>, which calls <c>experimental_onDecideStart</c>.</summary>
    internal Task OnStartAsync(DecideEvent source, CancellationToken cancellationToken)
    {
        return _dispatcher.OnDecideStart?.Invoke(WithFilteredContext(source), cancellationToken)
            ?? Task.CompletedTask;
    }

    /// <summary>Maps to <c>onEnd</c>, which calls <c>experimental_onDecideEnd</c>.</summary>
    internal Task OnEndAsync(DecideEvent source, CancellationToken cancellationToken)
    {
        return _dispatcher.OnDecideEnd?.Invoke(WithFilteredContext(source), cancellationToken)
            ?? Task.CompletedTask;
    }

    /// <summary>Maps to <c>experimental_onDecisionModelCallStart</c>.</summary>
    internal Task ExperimentalOnDecisionModelCallStartAsync(
        DecisionModelEvent source,
        CancellationToken cancellationToken
    )
    {
        return _dispatcher.OnDecisionModelCallStart?.Invoke(source, cancellationToken)
            ?? Task.CompletedTask;
    }

    /// <summary>Maps to <c>experimental_onDecisionModelCallEnd</c>.</summary>
    internal Task ExperimentalOnDecisionModelCallEndAsync(
        DecisionModelEvent source,
        CancellationToken cancellationToken
    )
    {
        return _dispatcher.OnDecisionModelCallEnd?.Invoke(source, cancellationToken)
            ?? Task.CompletedTask;
    }

    /// <summary>Maps to <c>onError</c>. A download error reaches integrations without its URL.</summary>
    internal Task OnErrorAsync(string callId, Exception error, CancellationToken cancellationToken)
    {
        return _dispatcher.OnError?.Invoke(
            new TelemetryErrorEvent(callId, error),
            cancellationToken
        ) ?? Task.CompletedTask;
    }

    private DecideEvent WithFilteredContext(DecideEvent source)
    {
        return new DecideEvent(
            source.CallId,
            source.OperationId,
            FilterIncludedContextHelper.FilterIncludedContext(
                source.RuntimeContext,
                _telemetry?.IncludeRuntimeContext
            ),
            source.Provider,
            source.ModelId,
            source.State,
            source.Questions,
            source.MaxRetries,
            source.Headers,
            source.ProviderOptions,
            source.Answers,
            source.Usage,
            source.Warnings,
            source.RecordInputs,
            source.RecordOutputs,
            source.FunctionId,
            source.MaxOutputTokens,
            source.Reasoning
        );
    }
}
