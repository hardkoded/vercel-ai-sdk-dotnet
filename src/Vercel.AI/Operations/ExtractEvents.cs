// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.Operations;

/// <summary>
/// Event passed to <see cref="ExtractRequest.OnStart"/> for extraction operations. Maps to <c>ExtractStartEvent</c> in
/// <c>extract/extract-events.ts</c>. Called when the operation begins, before the extraction model is called.
/// </summary>
public class ExtractStartEvent
{
    /// <summary>Creates an event.</summary>
    public ExtractStartEvent(
        string callId,
        IReadOnlyDictionary<string, object?> runtimeContext,
        string provider,
        string modelId,
        DecisionState? state,
        IReadOnlyDictionary<string, ExtractionField> fields,
        string? referenceDate,
        double? maxOutputTokens,
        string? reasoning,
        int maxRetries,
        IReadOnlyDictionary<string, string>? headers,
        JsonElement providerOptions,
        bool? recordInputs = null,
        bool? recordOutputs = null,
        string? functionId = null
    )
    {
        CallId = callId;
        RuntimeContext = runtimeContext;
        Provider = provider;
        ModelId = modelId;
        State = state;
        Fields = fields;
        ReferenceDate = referenceDate;
        MaxOutputTokens = maxOutputTokens;
        Reasoning = reasoning;
        MaxRetries = maxRetries;
        Headers = headers;
        ProviderOptions = providerOptions;
        RecordInputs = recordInputs;
        RecordOutputs = recordOutputs;
        FunctionId = functionId;
    }

    /// <summary>Unique identifier for this extraction call, used to correlate events.</summary>
    public string CallId { get; }

    /// <summary>Identifies the operation type: <c>ai.extract</c>.</summary>
    public string OperationId => "ai.extract";

    /// <summary>User-defined runtime context. Telemetry receives the filtered copy.</summary>
    public IReadOnlyDictionary<string, object?> RuntimeContext { get; }

    /// <summary>The provider identifier.</summary>
    public string Provider { get; }

    /// <summary>The extraction model identifier.</summary>
    public string ModelId { get; }

    /// <summary>The shared state to extract field values from.</summary>
    public DecisionState? State { get; }

    /// <summary>The fields to extract from the shared state.</summary>
    public IReadOnlyDictionary<string, ExtractionField> Fields { get; }

    /// <summary><c>YYYY-MM-DD</c> date that anchors relative dates and dates without a year.</summary>
    public string? ReferenceDate { get; }

    /// <summary>Maximum output tokens requested for the extraction.</summary>
    public double? MaxOutputTokens { get; }

    /// <summary>Reasoning effort requested for the extraction.</summary>
    public string? Reasoning { get; }

    /// <summary>Maximum number of retries for the extraction model call.</summary>
    public int MaxRetries { get; }

    /// <summary>Additional HTTP headers sent with the request.</summary>
    public IReadOnlyDictionary<string, string>? Headers { get; }

    /// <summary>Additional provider-specific options.</summary>
    public JsonElement ProviderOptions { get; }

    /// <summary>Present only on telemetry events.</summary>
    public bool? RecordInputs { get; }

    /// <summary>Present only on telemetry events.</summary>
    public bool? RecordOutputs { get; }

    /// <summary>Present only on telemetry events.</summary>
    public string? FunctionId { get; }
}

/// <summary>
/// Event passed to <see cref="ExtractRequest.OnEnd"/> for extraction operations. Maps to <c>ExtractEndEvent</c>.
/// Called when the operation completes successfully.
/// </summary>
public sealed class ExtractEndEvent : ExtractStartEvent
{
    /// <summary>Creates an event.</summary>
    public ExtractEndEvent(
        ExtractStartEvent start,
        IReadOnlyDictionary<string, ExtractionAnswer> answers,
        OperationUsage usage,
        IReadOnlyList<OperationWarning> warnings,
        JsonElement? providerMetadata,
        ProviderResponse response
    )
        : base(
            (start ?? throw new ArgumentNullException(nameof(start))).CallId,
            start.RuntimeContext,
            start.Provider,
            start.ModelId,
            start.State,
            start.Fields,
            start.ReferenceDate,
            start.MaxOutputTokens,
            start.Reasoning,
            start.MaxRetries,
            start.Headers,
            start.ProviderOptions,
            start.RecordInputs,
            start.RecordOutputs,
            start.FunctionId
        )
    {
        Answers = answers;
        Usage = usage;
        Warnings = warnings;
        ProviderMetadata = providerMetadata;
        Response = response;
    }

    /// <summary>Exactly one typed answer per field id. Telemetry receives an empty map when outputs are not recorded.</summary>
    public IReadOnlyDictionary<string, ExtractionAnswer> Answers { get; }

    /// <summary>Token usage for the extraction operation.</summary>
    public OperationUsage Usage { get; }

    /// <summary>Warnings from the extraction model.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }

    /// <summary>Optional provider-specific metadata.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Response metadata, including the resolved model id and timestamp.</summary>
    public ProviderResponse Response { get; }
}

/// <summary>
/// Event fired when the extraction model call begins. Maps to <c>ExtractionModelCallStartEvent</c>.
/// The logical model call includes any provider retries.
/// </summary>
public class ExtractionModelCallStartEvent
{
    /// <summary>Creates an event.</summary>
    public ExtractionModelCallStartEvent(
        string callId,
        string provider,
        string modelId,
        IReadOnlyList<DecisionModelStatePart> state,
        IReadOnlyDictionary<string, ExtractionField> fields,
        string? referenceDate,
        bool? recordInputs = null,
        bool? recordOutputs = null,
        string? functionId = null
    )
    {
        CallId = callId;
        Provider = provider;
        ModelId = modelId;
        State = state;
        Fields = fields;
        ReferenceDate = referenceDate;
        RecordInputs = recordInputs;
        RecordOutputs = recordOutputs;
        FunctionId = functionId;
    }

    /// <summary>Unique identifier for the outer extraction call.</summary>
    public string CallId { get; }

    /// <summary>Identifies the inner operation: <c>ai.extract.doExtract</c>.</summary>
    public string OperationId => "ai.extract.doExtract";

    /// <summary>The provider identifier.</summary>
    public string Provider { get; }

    /// <summary>The extraction model identifier.</summary>
    public string ModelId { get; }

    /// <summary>The normalized list of state parts the model receives.</summary>
    public IReadOnlyList<DecisionModelStatePart> State { get; }

    /// <summary>The fields to extract from the shared state.</summary>
    public IReadOnlyDictionary<string, ExtractionField> Fields { get; }

    /// <summary><c>YYYY-MM-DD</c> date that anchors relative dates and dates without a year.</summary>
    public string? ReferenceDate { get; }

    /// <summary>Present only on telemetry events.</summary>
    public bool? RecordInputs { get; }

    /// <summary>Present only on telemetry events.</summary>
    public bool? RecordOutputs { get; }

    /// <summary>Present only on telemetry events.</summary>
    public string? FunctionId { get; }
}

/// <summary>
/// Event fired after the extraction model response has been validated. Maps to <c>ExtractionModelCallEndEvent</c>.
/// Contains the result of the logical model call, including any retries.
/// </summary>
public sealed class ExtractionModelCallEndEvent : ExtractionModelCallStartEvent
{
    /// <summary>Creates an event.</summary>
    public ExtractionModelCallEndEvent(
        ExtractionModelCallStartEvent start,
        IReadOnlyDictionary<string, ExtractionAnswer> answers,
        OperationUsage? usage,
        IReadOnlyList<OperationWarning> warnings,
        JsonElement? providerMetadata,
        ProviderResponse? response
    )
        : base(
            (start ?? throw new ArgumentNullException(nameof(start))).CallId,
            start.Provider,
            start.ModelId,
            start.State,
            start.Fields,
            start.ReferenceDate,
            start.RecordInputs,
            start.RecordOutputs,
            start.FunctionId
        )
    {
        Answers = answers;
        Usage = usage;
        Warnings = warnings;
        ProviderMetadata = providerMetadata;
        Response = response;
    }

    /// <summary>Exactly one answer per field id. Telemetry receives an empty map when outputs are not recorded.</summary>
    public IReadOnlyDictionary<string, ExtractionAnswer> Answers { get; }

    /// <summary>Token usage reported by the extraction model, before total tokens are derived.</summary>
    public OperationUsage? Usage { get; }

    /// <summary>Warnings from the extraction model.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }

    /// <summary>Optional provider-specific metadata.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Optional raw response metadata from the provider.</summary>
    public ProviderResponse? Response { get; }
}
