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

    private GenerateTextRestrictedTelemetryDispatcher(
        TelemetryDispatcher dispatcher,
        TelemetryOptions? telemetry
    )
    {
        _telemetry = telemetry;
        _dispatcher = dispatcher;
    }

    /// <summary>Maps to <c>createRestrictedTelemetryDispatcher</c>.</summary>
    internal static GenerateTextRestrictedTelemetryDispatcher CreateRestrictedTelemetryDispatcher(
        TelemetryOptions? telemetry
    )
    {
        return new GenerateTextRestrictedTelemetryDispatcher(
            TelemetryDispatcher.Create(telemetry),
            telemetry
        );
    }

    /// <summary>Maps to <c>createRestrictedTelemetryDispatcher</c> for the telemetry settings of a text generation call.</summary>
    internal static GenerateTextRestrictedTelemetryDispatcher CreateRestrictedTelemetryDispatcher(
        GenerateTextOptions options
    )
    {
        var dispatcher = TelemetryDispatcher.Create(options);
        return new GenerateTextRestrictedTelemetryDispatcher(dispatcher, dispatcher.Options);
    }

    /// <summary>Whether any integration is registered.</summary>
    internal bool HasIntegrations => _dispatcher.HasIntegrations;

    /// <summary>Whether the dispatcher publishes tracing channel spans.</summary>
    internal bool IsTracingEnabled => _dispatcher.IsTracingEnabled;

    /// <summary>The <c>executeTool</c> wrappers, spread from the telemetry dispatcher.</summary>
    internal IReadOnlyList<IExecuteTool> ExecuteToolWrappers => _dispatcher.ExecuteToolWrappers;

    /// <summary>Maps to <c>onStart</c>. Null when no integration has it.</summary>
    internal Func<GenerateTextStartEvent, CancellationToken, Task>? OnStart =>
        _dispatcher.OnStart == null ? null : OnStartAsync;

    /// <summary>Maps to <c>onStepStart</c>. Null when no integration has it.</summary>
    internal Func<GenerateTextStepStartEvent, CancellationToken, Task>? OnStepStart =>
        _dispatcher.OnStepStart == null ? null : OnStepStartAsync;

    /// <summary>Maps to <c>onStepEnd</c>. Null when no integration has it.</summary>
    internal Func<StepResult, CancellationToken, Task>? OnStepEnd =>
        _dispatcher.OnStepEnd == null ? null : OnStepEndAsync;

    /// <summary>Maps to <c>onStepError</c>. Null when no integration has it.</summary>
    internal Func<GenerateTextStepErrorEvent, CancellationToken, Task>? OnStepError =>
        _dispatcher.OnStepError == null ? null : OnStepErrorAsync;

    /// <summary>Maps to <c>onEnd</c>. Null when no integration has it.</summary>
    internal Func<GenerateTextResult, CancellationToken, Task>? OnEnd =>
        _dispatcher.OnEnd == null ? null : OnEndAsync;

    /// <summary>Maps to <c>onAbort</c>. Null when no integration has it.</summary>
    internal Func<StreamAbortContext, CancellationToken, Task>? OnAbort =>
        _dispatcher.OnAbort == null ? null : OnAbortAsync;

    /// <summary>Maps to <c>onToolExecutionStart</c>. Null when no integration has it.</summary>
    internal Func<ToolExecutionStartEvent, CancellationToken, Task>? OnToolExecutionStart =>
        _dispatcher.OnToolExecutionStart == null ? null : OnToolExecutionStartAsync;

    /// <summary>Maps to <c>onToolExecutionEnd</c>. Null when no integration has it.</summary>
    internal Func<ToolExecutionEndEvent, CancellationToken, Task>? OnToolExecutionEnd =>
        _dispatcher.OnToolExecutionEnd == null ? null : OnToolExecutionEndAsync;

    /// <summary>Maps to <c>onLanguageModelCallStart</c>, spread from the telemetry dispatcher.</summary>
    internal Func<LanguageModelCallStartEvent, CancellationToken, Task>? OnLanguageModelCallStart =>
        _dispatcher.OnLanguageModelCallStart;

    /// <summary>Maps to <c>onLanguageModelCallEnd</c>, spread from the telemetry dispatcher.</summary>
    internal Func<LanguageModelCallEndEvent, CancellationToken, Task>? OnLanguageModelCallEnd =>
        _dispatcher.OnLanguageModelCallEnd;

    /// <summary>Maps to <c>onError</c>, spread from the telemetry dispatcher.</summary>
    internal Func<TelemetryErrorEvent, CancellationToken, Task>? OnError => _dispatcher.OnError;

    /// <summary>Maps to <c>startTracingChannelContext</c>, spread from the telemetry dispatcher.</summary>
    internal TracingChannelContext? StartTracingChannelContext(
        string type,
        object @event,
        Task<object?> completion
    )
    {
        return _dispatcher.StartTracingChannelContext(type, @event, completion);
    }

    /// <summary>Maps to <c>runInTracingChannelSpan</c>, spread from the telemetry dispatcher.</summary>
    internal Task<T> RunInTracingChannelSpan<T>(string type, object @event, Func<Task<T>> execute)
    {
        return _dispatcher.RunInTracingChannelSpan(type, @event, execute);
    }

    /// <summary>Maps to <c>executeLanguageModelCall</c>, spread from the telemetry dispatcher.</summary>
    internal Task<T> ExecuteLanguageModelCallAsync<T>(
        LanguageModelCallStartEvent @event,
        Func<Task<T>> execute,
        CancellationToken cancellationToken
    )
    {
        return _dispatcher.ExecuteLanguageModelCallAsync(@event, execute, cancellationToken);
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
