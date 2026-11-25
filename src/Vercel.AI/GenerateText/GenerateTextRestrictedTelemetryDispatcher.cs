// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.GenerateText;

/// <summary>
/// Hands text-generation events to the telemetry integrations. Maps to <c>createRestrictedTelemetryDispatcher</c> in
/// <c>generate-text/restricted-telemetry-dispatcher.ts</c>. User callbacks keep the original contexts, integrations only see the
/// runtime context keys <see cref="TelemetryOptions.IncludeRuntimeContext"/> includes and the tool context keys
/// <see cref="TelemetryOptions.IncludeToolsContext"/> includes. The port keeps it apart from the other restricted dispatchers
/// because they share the upstream function name.
/// </summary>
internal sealed class GenerateTextRestrictedTelemetryDispatcher
{
    private readonly TelemetryDispatcher _dispatcher;
    private readonly TelemetryOptions? _telemetry;

    private GenerateTextRestrictedTelemetryDispatcher(TelemetryOptions? telemetry)
    {
        _telemetry = telemetry;
        _dispatcher = TelemetryDispatcher.Create(telemetry);
    }

    /// <summary>Maps to <c>createRestrictedTelemetryDispatcher</c>.</summary>
    internal static GenerateTextRestrictedTelemetryDispatcher CreateRestrictedTelemetryDispatcher(
        TelemetryOptions? telemetry
    )
    {
        return new GenerateTextRestrictedTelemetryDispatcher(telemetry);
    }

    /// <summary>Maps to <c>onStart</c>.</summary>
    internal Task OnStartAsync(GenerateTextStartEvent source, CancellationToken cancellationToken)
    {
        return _dispatcher.OnStart?.Invoke(
            source.WithContexts(
                FilterIncludedContextHelper.FilterIncludedContext(
                    source.RuntimeContext,
                    _telemetry?.IncludeRuntimeContext
                ),
                FilterIncludedContextHelper.FilterToolsContext(
                    source.ToolsContext,
                    _telemetry?.IncludeToolsContext
                )
            ),
            cancellationToken
        )
            ?? Task.CompletedTask;
    }

    /// <summary>Maps to <c>onStepStart</c>.</summary>
    internal Task OnStepStartAsync(
        GenerateTextStepStartEvent source,
        CancellationToken cancellationToken
    )
    {
        return _dispatcher.OnStepStart?.Invoke(RestrictStepStartEvent(source), cancellationToken)
            ?? Task.CompletedTask;
    }

    /// <summary>Maps to <c>onStepEnd</c>.</summary>
    internal Task OnStepEndAsync(StepResult source, CancellationToken cancellationToken)
    {
        return _dispatcher.OnStepEnd?.Invoke(RestrictStepResult(source), cancellationToken)
            ?? Task.CompletedTask;
    }

    /// <summary>Maps to <c>onStepError</c>.</summary>
    internal Task OnStepErrorAsync(
        GenerateTextStepErrorEvent source,
        CancellationToken cancellationToken
    )
    {
        return _dispatcher.OnStepError?.Invoke(
            new GenerateTextStepErrorEvent(RestrictStepStartEvent(source), source.Error),
            cancellationToken
        )
            ?? Task.CompletedTask;
    }

    /// <summary>Maps to <c>onEnd</c>.</summary>
    internal Task OnEndAsync(GenerateTextResult source, CancellationToken cancellationToken)
    {
        return _dispatcher.OnEnd?.Invoke(
            source.WithSteps(source.Steps.Select(RestrictStepResult).ToList()),
            cancellationToken
        )
            ?? Task.CompletedTask;
    }

    /// <summary>Maps to <c>onAbort</c>.</summary>
    internal Task OnAbortAsync(StreamAbortContext source, CancellationToken cancellationToken)
    {
        return _dispatcher.OnAbort?.Invoke(
            new StreamAbortContext(
                source.Steps.Select(RestrictStepResult).ToList(),
                source.Exception,
                source.CallId
            ),
            cancellationToken
        )
            ?? Task.CompletedTask;
    }

    /// <summary>Maps to <c>onToolExecutionStart</c>.</summary>
    internal Task OnToolExecutionStartAsync(
        ToolExecutionStartEvent source,
        CancellationToken cancellationToken
    )
    {
        return _dispatcher.OnToolExecutionStart?.Invoke(
            new ToolExecutionStartEvent(source.ToolCall, source.Messages)
            {
                CallId = source.CallId,
                ToolContext = FilterToolContext(source),
            },
            cancellationToken
        )
            ?? Task.CompletedTask;
    }

    /// <summary>Maps to <c>onToolExecutionEnd</c>.</summary>
    internal Task OnToolExecutionEndAsync(
        ToolExecutionEndEvent source,
        CancellationToken cancellationToken
    )
    {
        return _dispatcher.OnToolExecutionEnd?.Invoke(
            new ToolExecutionEndEvent(
                source.ToolCall,
                source.Messages,
                source.ToolOutput,
                source.ToolExecutionTime
            )
            {
                CallId = source.CallId,
                ToolContext = FilterToolContext(source),
            },
            cancellationToken
        )
            ?? Task.CompletedTask;
    }

    /// <summary>Maps to <c>executeTool</c>, which the restricted dispatcher spreads from the telemetry dispatcher without filtering.</summary>
    internal Task<T> ExecuteToolAsync<T>(
        ToolExecutionStartEvent @event,
        Func<Task<T>> execute,
        CancellationToken cancellationToken
    )
    {
        return _dispatcher.ExecuteToolAsync(@event, execute, cancellationToken);
    }

    // Maps to the restrictStepResult function: a copy of the step with only the included contexts.
    private StepResult RestrictStepResult(StepResult step)
    {
        return step.WithContexts(
            FilterIncludedContextHelper.FilterIncludedContext(
                step.RuntimeContext,
                _telemetry?.IncludeRuntimeContext
            ),
            FilterIncludedContextHelper.FilterToolsContext(
                step.ToolsContext,
                _telemetry?.IncludeToolsContext
            )
        );
    }

    // Maps to the restrictStepStartEvent function: the contexts of the event and of its previous steps.
    private GenerateTextStepStartEvent RestrictStepStartEvent(GenerateTextStepStartEvent source)
    {
        return source.WithContexts(
            FilterIncludedContextHelper.FilterIncludedContext(
                source.RuntimeContext,
                _telemetry?.IncludeRuntimeContext
            ),
            FilterIncludedContextHelper.FilterToolsContext(
                source.ToolsContext,
                _telemetry?.IncludeToolsContext
            ),
            source.Steps.Select(RestrictStepResult).ToList()
        );
    }

    private System.Text.Json.JsonElement? FilterToolContext(ToolExecutionStartEvent source)
    {
        return source.ToolContext is { } toolContext
            ? FilterIncludedContextHelper.FilterToolContext(
                source.ToolCall.ToolName,
                toolContext,
                _telemetry?.IncludeToolsContext
            )
            : null;
    }
}
