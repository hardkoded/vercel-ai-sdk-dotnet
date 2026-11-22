// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI;

/// <summary>
/// Sends telemetry events to the resolved integrations. Per-call integrations take precedence over the global ones. Maps to
/// <c>TelemetryDispatcher</c> in <c>telemetry/telemetry.ts</c>.
/// </summary>
internal sealed class TelemetryDispatcher
{
    /// <summary>Whether the call has an enabled integration, which needs the content of the generation.</summary>
    public bool HasIntegrations { get; init; }

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
                AugmentResult(result, telemetry)
            ),
            OnLanguageModelCallStart = Filter<LanguageModelCallStartEvent>(
                Merge(integrations.Select(i => i.OnLanguageModelCallStart)),
                e => AugmentEvent(e, telemetry)
            ),
            OnLanguageModelCallEnd = Filter<LanguageModelCallEndEvent>(
                Merge(integrations.Select(i => i.OnLanguageModelCallEnd)),
                e => AugmentEvent(e, telemetry)
            ),
        };
    }

    // Adds the telemetry settings and the SDK version to an event, then strips the inputs and outputs the call does not record. Maps to augmentEvent.
    private static LanguageModelCallStartEvent AugmentEvent(
        LanguageModelCallStartEvent value,
        TelemetryOptions? telemetry
    )
    {
        var augmented = new LanguageModelCallStartEvent
        {
            CallId = value.CallId,
            Provider = value.Provider,
            ModelId = value.ModelId,
            Instructions = value.Instructions,
            Messages = value.Messages,
            Tools = value.Tools,
            CallOptions = value.CallOptions,
            RecordInputs = telemetry?.RecordInputs,
            RecordOutputs = telemetry?.RecordOutputs,
            FunctionId = telemetry?.FunctionId,
            AiVersion = Operations.AiSdkVersion.Version,
        };
        return FilterTelemetryEventHelper.FilterTelemetryEvent(augmented, telemetry);
    }

    // Maps to augmentEvent.
    private static LanguageModelCallEndEvent AugmentEvent(
        LanguageModelCallEndEvent value,
        TelemetryOptions? telemetry
    )
    {
        var augmented = new LanguageModelCallEndEvent(
            value.Provider,
            value.ModelId,
            value.FinishReason,
            value.Usage,
            value.Content,
            value.ResponseId,
            value.ProviderMetadata,
            value.ResponseTime
        )
        {
            CallId = value.CallId,
            Request = value.Request,
            Performance = value.Performance,
            RecordInputs = telemetry?.RecordInputs,
            RecordOutputs = telemetry?.RecordOutputs,
            FunctionId = telemetry?.FunctionId,
            AiVersion = Operations.AiSdkVersion.Version,
        };
        return FilterTelemetryEventHelper.FilterTelemetryEvent(augmented, telemetry);
    }

    // Maps to augmentEvent: the settings and version, the context filter, the content filter, and the credential redaction.
    private static GenerateTextResult AugmentResult(
        GenerateTextResult result,
        TelemetryOptions? telemetry
    )
    {
        var augmented = FilterResult(result, telemetry);
        augmented = new GenerateTextResult(
            augmented.Text,
            augmented.ReasoningText,
            augmented.Steps,
            augmented.FinishReason,
            augmented.Usage,
            augmented.Output,
            augmented.Sources,
            augmented.ProviderMetadata
        )
        {
            ResponseMessages = augmented.ResponseMessages,
            RecordInputs = telemetry?.RecordInputs,
            RecordOutputs = telemetry?.RecordOutputs,
            FunctionId = telemetry?.FunctionId,
            AiVersion = Operations.AiSdkVersion.Version,
        };
        return FilterTelemetryEventHelper.FilterTelemetryEvent(
            RedactTelemetryCredentialsHelper.RedactTelemetryCredentials(augmented),
            telemetry
        );
    }

    // Integrations see only the runtime and tool context properties the call includes. Maps to the filterContext step of augmentEvent.
    private static GenerateTextResult FilterResult(
        GenerateTextResult result,
        TelemetryOptions? telemetry
    )
    {
        var steps = result
            .Steps.Select(step =>
                step.WithContexts(
                    FilterIncludedContextHelper.FilterIncludedContext(
                        step.RuntimeContext,
                        telemetry?.IncludeRuntimeContext
                    ),
                    FilterIncludedContextHelper.FilterToolsContext(
                        step.ToolsContext,
                        telemetry?.IncludeToolsContext
                    )
                )
            )
            .ToList();
        return result.WithSteps(steps);
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
