// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI;

/// <summary>
/// Strips the inputs and outputs of an event when <see cref="TelemetryOptions.RecordInputs"/> or <see cref="TelemetryOptions.RecordOutputs"/> is
/// <c>false</c>. Every method returns a copy and leaves the original event alone. Maps to <c>filterTelemetryEvent</c> in <c>telemetry/filter-telemetry-event.ts</c>.
/// </summary>
internal static class FilterTelemetryEventHelper
{
    /// <summary>Filters the start event of a model call. Maps to <c>filterTelemetryEvent</c>.</summary>
    internal static LanguageModelCallStartEvent FilterTelemetryEvent(
        LanguageModelCallStartEvent value,
        TelemetryOptions? telemetry
    )
    {
        if (telemetry?.RecordInputs != false && telemetry?.RecordOutputs != false)
        {
            return value;
        }

        if (telemetry?.RecordInputs != false)
        {
            // Outputs only hide output fields, and a start event has none.
            return value;
        }

        var callOptions = value.CallOptions.Copy();
        callOptions.Prompt = [];
        callOptions.Tools = null;
        callOptions.ProviderOptions = new Dictionary<string, JsonElement>();
        callOptions.JsonSchema = null;
        callOptions.JsonSchemaName = null;
        callOptions.JsonSchemaDescription = null;
        return new LanguageModelCallStartEvent
        {
            CallId = value.CallId,
            Provider = value.Provider,
            ModelId = value.ModelId,
            Instructions = null,
            Messages = [],
            Tools = null,
            CallOptions = callOptions,
            RecordInputs = value.RecordInputs,
            RecordOutputs = value.RecordOutputs,
            FunctionId = value.FunctionId,
            AiVersion = value.AiVersion,
        };
    }

    /// <summary>Filters the end event of a model call. Maps to <c>filterTelemetryEvent</c>.</summary>
    internal static LanguageModelCallEndEvent FilterTelemetryEvent(
        LanguageModelCallEndEvent value,
        TelemetryOptions? telemetry
    )
    {
        var hideInputs = telemetry?.RecordInputs == false;
        var hideOutputs = telemetry?.RecordOutputs == false;
        if (!hideInputs && !hideOutputs)
        {
            return value;
        }

        return new LanguageModelCallEndEvent(
            value.Provider,
            value.ModelId,
            value.FinishReason,
            value.Usage,
            hideOutputs ? [] : value.Content,
            value.ResponseId,
            // Raw provider payloads can contain either inputs or outputs.
            null,
            value.ResponseTime
        )
        {
            CallId = value.CallId,
            Request = value.Request,
            Performance = value.Performance,
            RecordInputs = value.RecordInputs,
            RecordOutputs = value.RecordOutputs,
            FunctionId = value.FunctionId,
            AiVersion = value.AiVersion,
        };
    }

    /// <summary>Filters the result a call ends with, and each of its steps. Maps to <c>filterTelemetryEvent</c>.</summary>
    internal static GenerateTextResult FilterTelemetryEvent(
        GenerateTextResult value,
        TelemetryOptions? telemetry
    )
    {
        var hideInputs = telemetry?.RecordInputs == false;
        var hideOutputs = telemetry?.RecordOutputs == false;
        if (!hideInputs && !hideOutputs)
        {
            return value;
        }

        var steps = value.Steps.Select(step => FilterStep(step, hideInputs, hideOutputs)).ToList();
        return new GenerateTextResult(
            hideOutputs ? string.Empty : value.Text,
            hideOutputs ? null : value.ReasoningText,
            steps,
            value.FinishReason,
            value.Usage,
            hideOutputs ? null : value.Output,
            hideOutputs ? [] : value.Sources,
            null
        )
        {
            ResponseMessages = hideOutputs ? [] : value.ResponseMessages,
            RecordInputs = value.RecordInputs,
            RecordOutputs = value.RecordOutputs,
            FunctionId = value.FunctionId,
            AiVersion = value.AiVersion,
        };
    }

    private static StepResult FilterStep(StepResult step, bool hideInputs, bool hideOutputs)
    {
        // The request and response body can hold either inputs or outputs, so telemetry never gets it.
        var request = new LanguageModelRequestMetadata
        {
            Url = step.Request.Url,
            Messages = hideInputs ? [] : step.Request.Messages,
        };
        var response = new StepResponse(
            step.Response.Id,
            step.Response.Timestamp,
            step.Response.ModelId,
            step.Response.Headers,
            hideOutputs ? [] : step.Response.Messages
        );
        return new StepResult(
            hideOutputs ? string.Empty : step.Text,
            hideOutputs ? null : step.ReasoningText,
            hideOutputs ? [] : step.ToolCalls,
            hideOutputs ? [] : step.ToolResults,
            step.FinishReason,
            step.Usage,
            hideOutputs ? [] : step.Sources,
            null,
            step.Warnings,
            hideOutputs ? [] : step.Files,
            step.Provider,
            step.ModelId,
            hideOutputs ? [] : step.Content,
            step.Performance,
            request,
            response,
            step.RawFinishReason
        )
        {
            CallId = step.CallId,
            StepNumber = step.StepNumber,
            ToolsContext = step.ToolsContext,
            RuntimeContext = step.RuntimeContext,
            ExperimentalSteeringMessages = hideInputs ? [] : step.ExperimentalSteeringMessages,
            StructuredOutputText = hideOutputs ? string.Empty : step.StructuredOutputText,
        };
    }
}
