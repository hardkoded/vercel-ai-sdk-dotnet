// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI;

/// <summary>
/// Sends telemetry events to the resolved integrations. Per-call integrations take precedence over the global ones. Maps to
/// <c>TelemetryDispatcher</c> in <c>telemetry/telemetry.ts</c>.
/// </summary>
internal sealed class TelemetryDispatcher
{
    /// <summary>Whether the call has an enabled integration, which needs the content of the generation.</summary>
    public bool HasIntegrations { get; init; }

    /// <summary>Maps to <c>onStart</c>. Null when no integration has it.</summary>
    public Func<GenerateTextStartEvent, CancellationToken, Task>? OnStart { get; init; }

    /// <summary>Maps to <c>onStepStart</c>. Null when no integration has it.</summary>
    public Func<GenerateTextStepStartEvent, CancellationToken, Task>? OnStepStart { get; init; }

    /// <summary>Maps to <c>onToolExecutionStart</c>. Null when no integration has it.</summary>
    public Func<
        ToolExecutionStartEvent,
        CancellationToken,
        Task
    >? OnToolExecutionStart { get; init; }

    /// <summary>Maps to <c>onToolExecutionEnd</c>. Null when no integration has it.</summary>
    public Func<ToolExecutionEndEvent, CancellationToken, Task>? OnToolExecutionEnd { get; init; }

    /// <summary>Maps to <c>onStepEnd</c>, fanned out to the deprecated <c>onStepFinish</c> too. Null when no integration has either.</summary>
    public Func<StepResult, CancellationToken, Task>? OnStepEnd { get; init; }

    /// <summary>Maps to <c>onAbort</c>. Null when no integration has it.</summary>
    public Func<StreamAbortContext, CancellationToken, Task>? OnAbort { get; init; }

    /// <summary>Maps to <c>onError</c>. Null when no integration has it.</summary>
    public Func<Exception, CancellationToken, Task>? OnError { get; init; }

    /// <summary>Maps to <c>onEnd</c>. Null when no integration has it.</summary>
    public Func<GenerateTextResult, CancellationToken, Task>? OnEnd { get; init; }

    /// <summary>Maps to <c>onLanguageModelCallStart</c>. Null when no integration has it.</summary>
    public Func<
        LanguageModelCallStartEvent,
        CancellationToken,
        Task
    >? OnLanguageModelCallStart { get; init; }

    /// <summary>Maps to <c>onLanguageModelCallEnd</c>. Null when no integration has it.</summary>
    public Func<
        LanguageModelCallEndEvent,
        CancellationToken,
        Task
    >? OnLanguageModelCallEnd { get; init; }

    /// <summary>The integrations a call resolves: the per-call ones, or the global ones. Maps to the lookup in <c>createTelemetryDispatcher</c>.</summary>
    internal static IReadOnlyList<ITelemetry> ResolveIntegrations(TelemetryOptions? telemetry)
    {
        return telemetry?.Integrations ?? TelemetryRegistry.GetGlobalTelemetryIntegrations();
    }

    /// <summary>Creates the dispatcher of a text generation call.</summary>
    internal static TelemetryDispatcher Create(GenerateTextOptions options)
    {
#pragma warning disable CS0618 // the deprecated alias is read like its replacement
        return options.TelemetrySnapshot
            ?? Create(options.Telemetry ?? options.ExperimentalTelemetry);
#pragma warning restore CS0618
    }

    /// <summary>Creates the dispatcher of a call. Maps to <c>createTelemetryDispatcher</c>.</summary>
    internal static TelemetryDispatcher Create(TelemetryOptions? telemetry)
    {
        // When telemetry is explicitly disabled, the dispatcher performs no work.
        if (telemetry?.IsEnabled == false)
        {
            return new TelemetryDispatcher();
        }

        var integrations = ResolveIntegrations(telemetry);
        return new TelemetryDispatcher
        {
            HasIntegrations = integrations.Count > 0,
            OnEnd = Filter<GenerateTextResult>(Merge(integrations.Select(i => i.OnEnd)), result =>
                FilterResult(result, telemetry)
            ),
            OnStart = Filter<GenerateTextStartEvent>(
                Merge(integrations.Select(i => i.OnStart)),
                e =>
                    e.WithContexts(
                        FilterIncludedContextHelper.FilterIncludedContext(
                            e.RuntimeContext,
                            telemetry?.IncludeRuntimeContext
                        ),
                        FilterIncludedContextHelper.FilterToolsContext(
                            e.ToolsContext,
                            telemetry?.IncludeToolsContext
                        )
                    )
            ),
            OnStepStart = Filter<GenerateTextStepStartEvent>(
                Merge(integrations.Select(i => i.OnStepStart)),
                e =>
                    e.WithContexts(
                        FilterIncludedContextHelper.FilterIncludedContext(
                            e.RuntimeContext,
                            telemetry?.IncludeRuntimeContext
                        ),
                        FilterIncludedContextHelper.FilterToolsContext(
                            e.ToolsContext,
                            telemetry?.IncludeToolsContext
                        )
                    )
            ),
            OnToolExecutionStart = Filter<ToolExecutionStartEvent>(
                Merge(integrations.Select(i => i.OnToolExecutionStart)),
                e => new ToolExecutionStartEvent(e.ToolCall, e.Messages)
                {
                    CallId = e.CallId,
                    ToolContext = FilterToolContext(e, telemetry),
                }
            ),
            OnToolExecutionEnd = Filter<ToolExecutionEndEvent>(
                Merge(integrations.Select(i => i.OnToolExecutionEnd)),
                e => new ToolExecutionEndEvent(
                    e.ToolCall,
                    e.Messages,
                    e.ToolOutput,
                    e.ToolExecutionTime
                )
                {
                    CallId = e.CallId,
                    ToolContext = FilterToolContext(e, telemetry),
                }
            ),
            // Fans out to the deprecated onStepFinish too, so integrations that only implement it keep working.
            OnStepEnd = Filter<StepResult>(
                Merge(
                    integrations
                        .Select(i => i.OnStepEnd)
                        .Concat(integrations.Select(i => i.OnStepFinish))
                ),
                step => FilterStep(step, telemetry)
            ),
            OnAbort = Filter<StreamAbortContext>(
                Merge(integrations.Select(i => i.OnAbort)),
                context => new StreamAbortContext(
                    context.Steps.Select(step => FilterStep(step, telemetry)).ToList(),
                    context.Exception
                )
            ),
            OnError = Merge(integrations.Select(i => i.OnError)),
            OnLanguageModelCallStart = Merge(integrations.Select(i => i.OnLanguageModelCallStart)),
            OnLanguageModelCallEnd = Merge(integrations.Select(i => i.OnLanguageModelCallEnd)),
        };
    }

    // Integrations see only the runtime and tool context properties the call includes. Maps to the filterContext step of augmentEvent.
    private static GenerateTextResult FilterResult(
        GenerateTextResult result,
        TelemetryOptions? telemetry
    )
    {
        var steps = result.Steps.Select(step => FilterStep(step, telemetry)).ToList();
        return result.WithSteps(steps);
    }

    private static StepResult FilterStep(StepResult step, TelemetryOptions? telemetry)
    {
        return step.WithContexts(
            FilterIncludedContextHelper.FilterIncludedContext(
                step.RuntimeContext,
                telemetry?.IncludeRuntimeContext
            ),
            FilterIncludedContextHelper.FilterToolsContext(
                step.ToolsContext,
                telemetry?.IncludeToolsContext
            )
        );
    }

    // Maps to the toolContext branch of augmentEvent: the context of the called tool is filtered by name.
    private static JsonElement? FilterToolContext(
        ToolExecutionStartEvent e,
        TelemetryOptions? telemetry
    )
    {
        return e.ToolContext is { } toolContext
            ? FilterIncludedContextHelper.FilterToolContext(
                e.ToolCall.ToolName,
                toolContext,
                telemetry?.IncludeToolsContext
            )
            : null;
    }

    private static Func<TEvent, CancellationToken, Task>? Filter<TEvent>(
        Func<TEvent, CancellationToken, Task>? callback,
        Func<TEvent, TEvent> filter
    )
    {
        return callback == null
            ? null
            : (e, cancellationToken) => callback(filter(e), cancellationToken);
    }

    private static Func<TEvent, CancellationToken, Task>? Merge<TEvent>(
        IEnumerable<Func<TEvent, CancellationToken, Task>?> callbacks
    )
    {
        var list = callbacks.Where(c => c != null).ToList();
        if (list.Count == 0)
        {
            return null;
        }

        return async (e, cancellationToken) =>
        {
            foreach (var callback in list)
            {
                // Like upstream notify, an integration failure does not break the generation.
                await Generation
                    .NotifyAsync(() => callback!(e, cancellationToken), cancellationToken)
                    .ConfigureAwait(false);
            }
        };
    }
}
