// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;

namespace Vercel.AI;

/// <summary>
/// Hides the content of telemetry events on a copy, as <see cref="TelemetryOptions.RecordInputs"/> and
/// <see cref="TelemetryOptions.RecordOutputs"/> ask. Maps to <c>filterTelemetryEvent</c> in
/// <c>telemetry/filter-telemetry-event.ts</c> and the <c>augmentEvent</c> step of <c>createTelemetryDispatcher</c>. Only the
/// extract, decide and error events are ported so far.
/// </summary>
internal static class FilterTelemetryEventHelper
{
    /// <summary>Filters an extract start event.</summary>
    internal static ExtractStartEvent FilterTelemetryEvent(
        ExtractStartEvent source,
        TelemetryOptions? telemetry
    )
    {
        var hideInputs = telemetry?.RecordInputs == false;
        return new ExtractStartEvent(
            source.CallId,
            source.RuntimeContext,
            source.Provider,
            source.ModelId,
            hideInputs ? EmptyLike(source.State) : source.State,
            hideInputs ? new Dictionary<string, ExtractionField>() : source.Fields,
            hideInputs ? null : source.ReferenceDate,
            source.MaxOutputTokens,
            source.Reasoning,
            source.MaxRetries,
            source.Headers,
            hideInputs ? OperationJson.Parse("{}") : source.ProviderOptions,
            telemetry?.RecordInputs,
            telemetry?.RecordOutputs,
            telemetry?.FunctionId
        )
        {
            AiVersion = AiSdkVersion.Version,
        };
    }

    /// <summary>Filters an extract end event.</summary>
    internal static ExtractEndEvent FilterTelemetryEvent(
        ExtractEndEvent source,
        TelemetryOptions? telemetry
    )
    {
        var hidden = telemetry?.RecordInputs == false || telemetry?.RecordOutputs == false;
        return new ExtractEndEvent(
            FilterTelemetryEvent((ExtractStartEvent)source, telemetry),
            telemetry?.RecordOutputs == false
                ? new Dictionary<string, ExtractionAnswer>()
                : source.Answers,
            source.Usage,
            source.Warnings,
            hidden ? null : source.ProviderMetadata,
            hidden ? WithoutBody(source.Response)! : source.Response
        );
    }

    /// <summary>Filters an extraction model call start event.</summary>
    internal static ExtractionModelCallStartEvent FilterTelemetryEvent(
        ExtractionModelCallStartEvent source,
        TelemetryOptions? telemetry
    )
    {
        var hideInputs = telemetry?.RecordInputs == false;
        return new ExtractionModelCallStartEvent(
            source.CallId,
            source.Provider,
            source.ModelId,
            hideInputs ? [] : source.State,
            hideInputs ? new Dictionary<string, ExtractionField>() : source.Fields,
            hideInputs ? null : source.ReferenceDate,
            telemetry?.RecordInputs,
            telemetry?.RecordOutputs,
            telemetry?.FunctionId
        )
        {
            AiVersion = AiSdkVersion.Version,
        };
    }

    /// <summary>Filters an extraction model call end event.</summary>
    internal static ExtractionModelCallEndEvent FilterTelemetryEvent(
        ExtractionModelCallEndEvent source,
        TelemetryOptions? telemetry
    )
    {
        var hidden = telemetry?.RecordInputs == false || telemetry?.RecordOutputs == false;
        return new ExtractionModelCallEndEvent(
            FilterTelemetryEvent((ExtractionModelCallStartEvent)source, telemetry),
            telemetry?.RecordOutputs == false
                ? new Dictionary<string, ExtractionAnswer>()
                : source.Answers,
            source.Usage,
            source.Warnings,
            hidden ? null : source.ProviderMetadata,
            hidden ? WithoutBody(source.Response) : source.Response
        );
    }

    /// <summary>Filters a decide lifecycle event.</summary>
    internal static DecideEvent FilterTelemetryEvent(
        DecideEvent source,
        TelemetryOptions? telemetry
    )
    {
        var hideInputs = telemetry?.RecordInputs == false;
        return new DecideEvent(
            source.CallId,
            source.OperationId,
            source.RuntimeContext,
            source.Provider,
            source.ModelId,
            hideInputs ? EmptyLike(source.State) : source.State,
            hideInputs ? new Dictionary<string, DecisionQuestion>() : source.Questions,
            source.MaxRetries,
            source.Headers,
            hideInputs ? OperationJson.Parse("{}") : source.ProviderOptions,
            telemetry?.RecordOutputs == false && source.Answers != null
                ? new Dictionary<string, DecisionAnswer>()
                : source.Answers,
            source.Usage,
            source.Warnings,
            telemetry?.RecordInputs,
            telemetry?.RecordOutputs,
            telemetry?.FunctionId,
            source.MaxOutputTokens,
            source.Reasoning
        )
        {
            AiVersion = AiSdkVersion.Version,
        };
    }

    /// <summary>Filters a decision model call event.</summary>
    internal static DecisionModelEvent FilterTelemetryEvent(
        DecisionModelEvent source,
        TelemetryOptions? telemetry
    )
    {
        var hideInputs = telemetry?.RecordInputs == false;
        return new DecisionModelEvent(
            source.CallId,
            source.OperationId,
            source.Provider,
            source.ModelId,
            hideInputs && source.State != null ? [] : source.State,
            hideInputs ? new Dictionary<string, DecisionQuestion>() : source.Questions,
            telemetry?.RecordOutputs == false && source.Answers != null
                ? new Dictionary<string, DecisionAnswer>()
                : source.Answers,
            source.Usage,
            telemetry?.RecordInputs,
            telemetry?.RecordOutputs,
            telemetry?.FunctionId
        )
        {
            AiVersion = AiSdkVersion.Version,
        };
    }

    /// <summary>Filters an error event and redacts credentials from its error.</summary>
    internal static TelemetryErrorEvent FilterTelemetryEvent(
        TelemetryErrorEvent source,
        TelemetryOptions? telemetry
    )
    {
        // Raw provider errors can contain either inputs or outputs.
        var hidden = telemetry?.RecordInputs == false || telemetry?.RecordOutputs == false;
        return new TelemetryErrorEvent(
            source.CallId,
            hidden
                ? new InvalidOperationException("AI SDK operation failed")
                : RedactTelemetryCredentialsHelper.RedactTelemetryCredentials(source.Error)
        )
        {
            AiVersion = AiSdkVersion.Version,
            RecordInputs = telemetry?.RecordInputs,
            RecordOutputs = telemetry?.RecordOutputs,
            FunctionId = telemetry?.FunctionId,
        };
    }

    // The state keeps its shape: text stays text, parts stay parts and a JSON object stays an object.
    private static DecisionState? EmptyLike(DecisionState? state)
    {
        if (state == null)
        {
            return null;
        }

        if (state.Parts != null)
        {
            return DecisionState.From(Array.Empty<DecisionStatePart>());
        }

        return state.Json != null
            ? DecisionState.FromJson(OperationJson.Parse("{}"))
            : DecisionState.From(string.Empty);
    }

    // The raw body can contain either inputs or outputs.
    private static ProviderResponse? WithoutBody(ProviderResponse? response)
    {
        return response == null
            ? null
            : new ProviderResponse(
                response.Headers,
                null,
                response.Id,
                response.Timestamp,
                response.ModelId,
                null
            );
    }
}
