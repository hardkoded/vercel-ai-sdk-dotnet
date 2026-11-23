// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

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

    /// <summary>Filters the tool call and the tool output of a tool execution event. Maps to the <c>toolCall</c> and <c>toolOutput</c> branches of <c>filterTelemetryEvent</c>.</summary>
    internal static T FilterToolEvent<T>(T source, TelemetryOptions? telemetry)
        where T : ToolExecutionStartEvent
    {
        var hideInputs = telemetry?.RecordInputs == false;
        var hideOutputs = telemetry?.RecordOutputs == false;
        if (!hideInputs && !hideOutputs)
        {
            return source;
        }

        // Raw provider payloads and errors can contain either inputs or outputs.
        var toolCall = new GeneratedToolCall(
            source.ToolCall.ToolCallId,
            source.ToolCall.ToolName,
            hideInputs ? "{}" : source.ToolCall.ArgumentsJson
        )
        {
            ProviderExecuted = source.ToolCall.ProviderExecuted,
            Dynamic = source.ToolCall.Dynamic,
            Invalid = source.ToolCall.Invalid,
            Error =
                source.ToolCall.Error == null
                    ? null
                    : new InvalidOperationException("AI SDK operation failed"),
            Title = source.ToolCall.Title,
            ToolMetadata = source.ToolCall.ToolMetadata,
            InputSchemaInput = hideInputs ? null : source.ToolCall.InputSchemaInput,
        };
        if (source is not ToolExecutionEndEvent end)
        {
            return (T)
                (object)
                    new ToolExecutionStartEvent(toolCall, source.Messages)
                    {
                        CallId = source.CallId,
                        ToolContext = source.ToolContext,
                    };
        }

        var output = new ExecutedTool(
            end.ToolOutput.ToolCallId,
            end.ToolOutput.ToolName,
            hideOutputs ? "null" : end.ToolOutput.OutputJson,
            end.ToolOutput.IsError
        )
        {
            Error =
                hideOutputs || end.ToolOutput.Error == null
                    ? null
                    : new InvalidOperationException("AI SDK operation failed"),
            ProviderExecuted = end.ToolOutput.ProviderExecuted,
            InputJson = hideInputs ? null : end.ToolOutput.InputJson,
            Dynamic = end.ToolOutput.Dynamic,
            Title = end.ToolOutput.Title,
            ToolMetadata = end.ToolOutput.ToolMetadata,
            Preliminary = end.ToolOutput.Preliminary,
        };
        return (T)
            (object)
                new ToolExecutionEndEvent(toolCall, end.Messages, output, end.ToolExecutionTime)
                {
                    CallId = end.CallId,
                    ToolContext = end.ToolContext,
                };
    }

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
            telemetry?.FunctionId,
            source.Warnings,
            source.Rounding,
            source.ProviderMetadata,
            source.Response
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

    /// <summary>Filters the start event of <c>embed</c> and <c>embedMany</c>.</summary>
    internal static EmbedStartEvent FilterTelemetryEvent(
        EmbedStartEvent source,
        TelemetryOptions? telemetry
    )
    {
        var hideInputs = telemetry?.RecordInputs == false;
        return new EmbedStartEvent(
            source.CallId,
            source.OperationId,
            source.RuntimeContext,
            source.Provider,
            source.ModelId,
            hideInputs ? EmptyValue(source.Value) : source.Value,
            source.MaxRetries,
            source.Headers,
            hideInputs ? OperationJson.Parse("{}") : source.ProviderOptions,
            source.Dimensions
        )
        {
            AiVersion = AiSdkVersion.Version,
            RecordInputs = telemetry?.RecordInputs,
            RecordOutputs = telemetry?.RecordOutputs,
            FunctionId = telemetry?.FunctionId,
        };
    }

    /// <summary>Filters the end event of <c>embed</c> and <c>embedMany</c>.</summary>
    internal static EmbedEndEvent FilterTelemetryEvent(
        EmbedEndEvent source,
        TelemetryOptions? telemetry
    )
    {
        var hideInputs = telemetry?.RecordInputs == false;
        var hideOutputs = telemetry?.RecordOutputs == false;
        var hidden = hideInputs || hideOutputs;
        return new EmbedEndEvent(
            source.CallId,
            source.OperationId,
            source.RuntimeContext,
            source.Provider,
            source.ModelId,
            hideInputs ? EmptyValue(source.Value) : source.Value,
            hideOutputs
                ? source.Embedding.IsMany
                    ? EmbedEventValue<double[], IReadOnlyList<double[]>>.FromMany([])
                    : EmbedEventValue<double[], IReadOnlyList<double[]>>.FromOne([])
                : source.Embedding,
            source.Usage,
            source.Warnings,
            hidden ? null : source.ProviderMetadata,
            hidden ? WithoutBody(source.Response) : source.Response
        )
        {
            AiVersion = AiSdkVersion.Version,
            RecordInputs = telemetry?.RecordInputs,
            RecordOutputs = telemetry?.RecordOutputs,
            FunctionId = telemetry?.FunctionId,
        };
    }

    /// <summary>Filters the start event of a <c>doEmbed</c> call.</summary>
    internal static EmbeddingModelCallStartEvent FilterTelemetryEvent(
        EmbeddingModelCallStartEvent source,
        TelemetryOptions? telemetry
    )
    {
        return new EmbeddingModelCallStartEvent(
            source.CallId,
            source.EmbedCallId,
            source.OperationId,
            source.Provider,
            source.ModelId,
            telemetry?.RecordInputs == false ? [] : source.Values,
            source.Dimensions
        )
        {
            AiVersion = AiSdkVersion.Version,
            RecordInputs = telemetry?.RecordInputs,
            RecordOutputs = telemetry?.RecordOutputs,
            FunctionId = telemetry?.FunctionId,
        };
    }

    /// <summary>Filters the end event of a <c>doEmbed</c> call.</summary>
    internal static EmbeddingModelCallEndEvent FilterTelemetryEvent(
        EmbeddingModelCallEndEvent source,
        TelemetryOptions? telemetry
    )
    {
        return new EmbeddingModelCallEndEvent(
            source.CallId,
            source.EmbedCallId,
            source.OperationId,
            source.Provider,
            source.ModelId,
            telemetry?.RecordInputs == false ? [] : source.Values,
            telemetry?.RecordOutputs == false ? [] : source.Embeddings,
            source.Usage,
            source.Dimensions
        )
        {
            AiVersion = AiSdkVersion.Version,
            RecordInputs = telemetry?.RecordInputs,
            RecordOutputs = telemetry?.RecordOutputs,
            FunctionId = telemetry?.FunctionId,
        };
    }

    /// <summary>Filters the start event of <c>rerank</c>.</summary>
    internal static RerankStartEvent FilterTelemetryEvent(
        RerankStartEvent source,
        TelemetryOptions? telemetry
    )
    {
        var hideInputs = telemetry?.RecordInputs == false;
        return new RerankStartEvent(
            source.CallId,
            source.OperationId,
            source.RuntimeContext,
            source.Provider,
            source.ModelId,
            hideInputs ? [] : source.Documents,
            hideInputs ? string.Empty : source.Query,
            source.TopN,
            source.MaxRetries,
            source.Headers,
            hideInputs ? OperationJson.Parse("{}") : source.ProviderOptions
        )
        {
            AiVersion = AiSdkVersion.Version,
            RecordInputs = telemetry?.RecordInputs,
            RecordOutputs = telemetry?.RecordOutputs,
            FunctionId = telemetry?.FunctionId,
        };
    }

    /// <summary>Filters the end event of <c>rerank</c>.</summary>
    internal static RerankEndEvent FilterTelemetryEvent(
        RerankEndEvent source,
        TelemetryOptions? telemetry
    )
    {
        var hideInputs = telemetry?.RecordInputs == false;
        var hideOutputs = telemetry?.RecordOutputs == false;
        var hidden = hideInputs || hideOutputs;
        return new RerankEndEvent(
            source.CallId,
            source.OperationId,
            source.RuntimeContext,
            source.Provider,
            source.ModelId,
            hideInputs ? [] : source.Documents,
            hideInputs ? string.Empty : source.Query,
            hideOutputs ? []
                : hideInputs
                    ? source
                        .Ranking.Select(entry => new RankedDocument(
                            entry.OriginalIndex,
                            entry.Score,
                            entry.Document is string
                                ? string.Empty
                                : new Dictionary<string, object?>()
                        ))
                        .ToList()
                : source.Ranking,
            source.Warnings,
            hidden ? null : source.ProviderMetadata,
            hidden
                ? new RerankResponseInfo(
                    source.Response.Id,
                    source.Response.Timestamp,
                    source.Response.ModelId,
                    source.Response.Headers,
                    null
                )
                : source.Response
        )
        {
            AiVersion = AiSdkVersion.Version,
            RecordInputs = telemetry?.RecordInputs,
            RecordOutputs = telemetry?.RecordOutputs,
            FunctionId = telemetry?.FunctionId,
        };
    }

    /// <summary>Filters the start event of a <c>doRerank</c> call.</summary>
    internal static RerankingModelCallStartEvent FilterTelemetryEvent(
        RerankingModelCallStartEvent source,
        TelemetryOptions? telemetry
    )
    {
        var hideInputs = telemetry?.RecordInputs == false;
        return new RerankingModelCallStartEvent(
            source.CallId,
            source.OperationId,
            source.Provider,
            source.ModelId,
            hideInputs ? [] : source.Documents,
            source.DocumentsType,
            hideInputs ? string.Empty : source.Query,
            source.TopN
        )
        {
            AiVersion = AiSdkVersion.Version,
            RecordInputs = telemetry?.RecordInputs,
            RecordOutputs = telemetry?.RecordOutputs,
            FunctionId = telemetry?.FunctionId,
        };
    }

    /// <summary>Filters the end event of a <c>doRerank</c> call.</summary>
    internal static RerankingModelCallEndEvent FilterTelemetryEvent(
        RerankingModelCallEndEvent source,
        TelemetryOptions? telemetry
    )
    {
        return new RerankingModelCallEndEvent(
            source.CallId,
            source.OperationId,
            source.Provider,
            source.ModelId,
            source.DocumentsType,
            telemetry?.RecordOutputs == false ? [] : source.Ranking
        )
        {
            AiVersion = AiSdkVersion.Version,
            RecordInputs = telemetry?.RecordInputs,
            RecordOutputs = telemetry?.RecordOutputs,
            FunctionId = telemetry?.FunctionId,
        };
    }

    /// <summary>Filters the start event of <c>transcribe</c>.</summary>
    internal static TranscriptionStartEvent FilterTelemetryEvent(
        TranscriptionStartEvent source,
        TelemetryOptions? telemetry
    )
    {
        return new TranscriptionStartEvent(
            source.CallId,
            source.Provider,
            source.ModelId,
            source.Audio,
            source.MaxRetries,
            source.Headers,
            telemetry?.RecordInputs == false ? OperationJson.Parse("{}") : source.ProviderOptions,
            source.TimestampGranularities,
            source.InputAudioFormat,
            telemetry?.RecordInputs,
            telemetry?.RecordOutputs,
            telemetry?.FunctionId
        )
        {
            AiVersion = AiSdkVersion.Version,
        };
    }

    /// <summary>Filters the end event of <c>transcribe</c>.</summary>
    internal static TranscriptionEndEvent FilterTelemetryEvent(
        TranscriptionEndEvent source,
        TelemetryOptions? telemetry
    )
    {
        var hideOutputs = telemetry?.RecordOutputs == false;
        var hidden = hideOutputs || telemetry?.RecordInputs == false;
        return new TranscriptionEndEvent(
            source.CallId,
            source.Provider,
            source.ModelId,
            source.Audio,
            hideOutputs ? string.Empty : source.Text,
            hideOutputs ? [] : source.Segments,
            hideOutputs ? [] : source.Words,
            source.Language,
            source.DurationInSeconds,
            source.Usage,
            source.Warnings,
            // Raw provider metadata can contain inputs or outputs, so any hidden side hides it.
            hidden ? null : source.ProviderMetadata,
            hidden ? WithoutBody(source.Response)! : source.Response,
            telemetry?.RecordInputs,
            telemetry?.RecordOutputs,
            telemetry?.FunctionId
        )
        {
            AiVersion = AiSdkVersion.Version,
        };
    }

    /// <summary>Filters the start event of <c>streamTranscribe</c>.</summary>
    internal static StreamTranscriptionStartEvent FilterTelemetryEvent(
        StreamTranscriptionStartEvent source,
        TelemetryOptions? telemetry
    )
    {
        return new StreamTranscriptionStartEvent(
            source.CallId,
            source.Provider,
            source.ModelId,
            source.Audio,
            source.InputAudioFormat,
            source.MaxRetries,
            source.Headers,
            telemetry?.RecordInputs == false ? OperationJson.Parse("{}") : source.ProviderOptions
        )
        {
            AiVersion = AiSdkVersion.Version,
            RecordInputs = telemetry?.RecordInputs,
            RecordOutputs = telemetry?.RecordOutputs,
            FunctionId = telemetry?.FunctionId,
        };
    }

    /// <summary>Filters the end event of <c>streamTranscribe</c>.</summary>
    internal static StreamTranscriptionEndEvent FilterTelemetryEvent(
        StreamTranscriptionEndEvent source,
        TelemetryOptions? telemetry
    )
    {
        var hideOutputs = telemetry?.RecordOutputs == false;
        var hidden = hideOutputs || telemetry?.RecordInputs == false;
        return new StreamTranscriptionEndEvent(
            source.CallId,
            source.Provider,
            source.ModelId,
            source.Audio,
            hideOutputs ? string.Empty : source.Text,
            hideOutputs ? [] : source.Segments,
            source.Language,
            source.DurationInSeconds,
            source.Usage,
            source.Warnings,
            hidden ? null : source.ProviderMetadata,
            hidden ? WithoutBody(source.Response)! : source.Response
        )
        {
            AiVersion = AiSdkVersion.Version,
            RecordInputs = telemetry?.RecordInputs,
            RecordOutputs = telemetry?.RecordOutputs,
            FunctionId = telemetry?.FunctionId,
        };
    }

    private static EmbedEventValue<string, IReadOnlyList<string>> EmptyValue(
        EmbedEventValue<string, IReadOnlyList<string>> value
    )
    {
        return value.IsMany
            ? EmbedEventValue<string, IReadOnlyList<string>>.FromMany([])
            : EmbedEventValue<string, IReadOnlyList<string>>.FromOne(string.Empty);
    }

    private static EmbedEventValue<ProviderResponse?, IReadOnlyList<ProviderResponse?>> WithoutBody(
        EmbedEventValue<ProviderResponse?, IReadOnlyList<ProviderResponse?>> value
    )
    {
        return value.IsMany
            ? EmbedEventValue<ProviderResponse?, IReadOnlyList<ProviderResponse?>>.FromMany(
                value.Many!.Select(WithoutBody).ToList()
            )
            : EmbedEventValue<ProviderResponse?, IReadOnlyList<ProviderResponse?>>.FromOne(
                WithoutBody(value.One)
            );
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
