// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;

namespace Vercel.AI.OpenTelemetry;

/// <summary>The kind of span an <see cref="EnrichSpan"/> callback is asked about. Maps to <c>OpenTelemetrySpanType</c>.</summary>
public enum OpenTelemetrySpanType
{
    /// <summary>The root span of an operation. Maps to <c>'operation'</c>.</summary>
    Operation,

    /// <summary>A step span. Maps to <c>'step'</c>.</summary>
    Step,

    /// <summary>A model call span. Maps to <c>'languageModel'</c>.</summary>
    LanguageModel,

    /// <summary>A tool execution span. Maps to <c>'tool'</c>.</summary>
    Tool,

    /// <summary>A decision model call span. Maps to <c>'experimental_decision'</c>.</summary>
    ExperimentalDecision,

    /// <summary>An extraction model call span. Maps to <c>'experimental_extraction'</c>.</summary>
    ExperimentalExtraction,

    /// <summary>An embedding model call span. Maps to <c>'embedding'</c>.</summary>
    Embedding,

    /// <summary>A reranking model call span. Maps to <c>'reranking'</c>.</summary>
    Reranking,
}

/// <summary>The call an <see cref="EnrichSpan"/> callback is asked about. Maps to the callback options in <c>EnrichSpan</c>.</summary>
public sealed class EnrichSpanContext
{
    /// <summary>The kind of span being created.</summary>
    public OpenTelemetrySpanType SpanType { get; init; }

    /// <summary>The operation id, such as <c>ai.generateText</c>.</summary>
    public string OperationId { get; init; } = string.Empty;

    /// <summary>The call id.</summary>
    public string CallId { get; init; } = string.Empty;

    /// <summary>The runtime context of the call, or null.</summary>
    public IReadOnlyDictionary<string, object?>? RuntimeContext { get; init; }
}

/// <summary>
/// Adds custom attributes to a span when it is created. Return null to add none. An exception is ignored.
/// Maps to <c>EnrichSpan</c>.
/// </summary>
public delegate IReadOnlyDictionary<string, object?>? EnrichSpan(EnrichSpanContext context);

/// <summary>Maps to the <c>agentSpanName</c> callback of <c>OpenTelemetryOptions</c>. Return null for the default name.</summary>
public delegate string? AgentSpanNameCallback(string modelId, string? functionId);

/// <summary>Controls the root agent span names. Maps to the <c>agentSpanName</c> option of <c>OpenTelemetryOptions</c>.</summary>
public sealed class AgentSpanName
{
    private readonly string? _mode;
    private readonly AgentSpanNameCallback? _callback;

    private AgentSpanName(string? mode, AgentSpanNameCallback? callback)
    {
        _mode = mode;
        _callback = callback;
    }

    /// <summary>Names are <c>invoke_agent {modelId}</c>. The default. Maps to <c>'modelId'</c>.</summary>
    public static AgentSpanName ModelId { get; } = new("modelId", null);

    /// <summary>Names are <c>invoke_agent {functionId}</c>, falling back to the model id. Maps to <c>'functionId'</c>.</summary>
    public static AgentSpanName FunctionId { get; } = new("functionId", null);

    /// <summary>Returns the complete span name. A null result or an exception falls back to the default name.</summary>
    public static AgentSpanName From(AgentSpanNameCallback callback)
    {
        return new AgentSpanName(
            null,
            callback ?? throw new ArgumentNullException(nameof(callback))
        );
    }

    internal bool IsFunctionId => _mode == "functionId";

    internal AgentSpanNameCallback? Callback => _callback;
}

/// <summary>Options of <see cref="OpenTelemetry"/>. Maps to <c>OpenTelemetryOptions</c>.</summary>
public sealed class OpenTelemetryOptions
{
    /// <summary>
    /// The source that creates the spans. Maps to <c>tracer</c>. The default is a source named <c>gen_ai</c>.
    /// Add that name to your <c>TracerProvider</c>.
    /// </summary>
    public ActivitySource? Tracer { get; set; }

    /// <summary>Controls the root agent span names for text generation. Null is <see cref="AgentSpanName.ModelId"/>.</summary>
    public AgentSpanName? AgentSpanName { get; set; }

    /// <summary>Adds custom attributes to spans when they are created.</summary>
    public EnrichSpan? EnrichSpan { get; set; }

    /// <summary>Emit AI SDK usage details that GenAI SemConv does not represent.</summary>
    public bool? Usage { get; set; }

    /// <summary>Emit provider metadata on response spans.</summary>
    public bool? ProviderMetadata { get; set; }

    /// <summary>Emit embedding input and output values.</summary>
    public bool? Embedding { get; set; }

    /// <summary>Emit reranking input documents and output ranking.</summary>
    public bool? Reranking { get; set; }

    /// <summary>Emit decision state, questions, and answers. Maps to <c>experimental_decision</c>.</summary>
    public bool? ExperimentalDecision { get; set; }

    /// <summary>Deprecated alias of <see cref="ExperimentalDecision"/>. Maps to <c>experimental_evaluation</c>.</summary>
    [Obsolete("Use ExperimentalDecision instead.")]
    public bool? ExperimentalEvaluation { get; set; }

    /// <summary>Emit extraction state, fields, reference date, and answers. Maps to <c>experimental_extraction</c>.</summary>
    public bool? ExperimentalExtraction { get; set; }

    /// <summary>Emit runtime context values.</summary>
    public bool? RuntimeContext { get; set; }

    /// <summary>Emit request headers.</summary>
    public bool? Headers { get; set; }

    /// <summary>Emit the tool choice of a step as <c>ai.prompt.toolChoice</c>. Maps to <c>toolChoice</c>.</summary>
    public bool? ToolChoice { get; set; }
}

/// <summary>The enabled supplemental attribute groups. Maps to <c>SupplementalAttributeOptions</c>.</summary>
internal sealed class SupplementalAttributeOptions
{
    public bool Usage { get; init; }

    public bool ProviderMetadata { get; init; }

    public bool Embedding { get; init; }

    public bool Reranking { get; init; }

    public bool ExperimentalDecision { get; init; }

    public bool ExperimentalExtraction { get; init; }

    public bool RuntimeContext { get; init; }

    public bool Headers { get; init; }

    public bool ToolChoice { get; init; }

    /// <summary>Maps to <c>normalizeSupplementalAttributes</c>.</summary>
    public static SupplementalAttributeOptions Normalize(OpenTelemetryOptions options)
    {
#pragma warning disable CS0618 // experimental_evaluation is a supported deprecated alias
        var decision = options.ExperimentalDecision ?? options.ExperimentalEvaluation ?? false;
#pragma warning restore CS0618
        return new SupplementalAttributeOptions
        {
            Usage = options.Usage ?? false,
            ProviderMetadata = options.ProviderMetadata ?? false,
            Embedding = options.Embedding ?? false,
            Reranking = options.Reranking ?? false,
            ExperimentalDecision = decision,
            ExperimentalExtraction = options.ExperimentalExtraction ?? false,
            RuntimeContext = options.RuntimeContext ?? false,
            Headers = options.Headers ?? false,
            ToolChoice = options.ToolChoice ?? false,
        };
    }

    /// <summary>Maps to <c>selectSupplementalAttributes</c> for one group.</summary>
    public IReadOnlyDictionary<string, object?> Select(
        TelemetrySelection telemetry,
        SupplementalGroup group,
        IReadOnlyDictionary<string, object?>? attributes
    )
    {
        var enabled = group switch
        {
            SupplementalGroup.Usage => Usage,
            SupplementalGroup.ProviderMetadata => ProviderMetadata,
            SupplementalGroup.Embedding => Embedding,
            SupplementalGroup.Reranking => Reranking,
            SupplementalGroup.ExperimentalDecision => ExperimentalDecision,
            SupplementalGroup.ExperimentalExtraction => ExperimentalExtraction,
            SupplementalGroup.RuntimeContext => RuntimeContext,
            SupplementalGroup.Headers => Headers,
            SupplementalGroup.ToolChoice => ToolChoice,
            _ => false,
        };
        if (!enabled || attributes is null)
        {
            return new Dictionary<string, object?>();
        }

        return TelemetryAttributes.SelectAttributes(telemetry, attributes);
    }
}

internal enum SupplementalGroup
{
    Usage,
    ProviderMetadata,
    Embedding,
    Reranking,
    ExperimentalDecision,
    ExperimentalExtraction,
    RuntimeContext,
    Headers,
    ToolChoice,
}

/// <summary>Helpers of <c>supplemental-attributes.ts</c>.</summary>
internal static class SupplementalAttributeHelpers
{
    /// <summary>Maps to <c>getRuntimeContextAttributes</c>. Nested objects flatten into dotted keys.</summary>
    public static Dictionary<string, object?> GetRuntimeContextAttributes(
        IReadOnlyDictionary<string, object?>? context
    )
    {
        var attributes = new Dictionary<string, object?>(StringComparer.Ordinal);
        if (context is null)
        {
            return attributes;
        }

        foreach (var pair in context)
        {
            Add(attributes, "ai.settings.context." + pair.Key, pair.Value);
        }

        return attributes;
    }

    /// <summary>Maps to <c>getHeaderAttributes</c>.</summary>
    public static Dictionary<string, object?> GetHeaderAttributes(
        IEnumerable<KeyValuePair<string, string?>>? headers
    )
    {
        var attributes = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var pair in headers ?? [])
        {
            if (pair.Value is not null)
            {
                attributes["ai.request.headers." + pair.Key] = pair.Value;
            }
        }

        return attributes;
    }

    private static void Add(Dictionary<string, object?> attributes, string key, object? value)
    {
        switch (value)
        {
            case null:
                return;
            case System.Text.Json.JsonElement element:
                AddJson(attributes, key, element);
                return;
            case IReadOnlyDictionary<string, object?> nested:
                foreach (var pair in nested)
                {
                    Add(attributes, key + "." + pair.Key, pair.Value);
                }

                return;
            default:
                attributes[key] = value;
                return;
        }
    }

    private static void AddJson(
        Dictionary<string, object?> attributes,
        string key,
        System.Text.Json.JsonElement element
    )
    {
        switch (element.ValueKind)
        {
            case System.Text.Json.JsonValueKind.Null:
            case System.Text.Json.JsonValueKind.Undefined:
                return;
            case System.Text.Json.JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    AddJson(attributes, key + "." + property.Name, property.Value);
                }

                return;
            case System.Text.Json.JsonValueKind.String:
                attributes[key] = element.GetString();
                return;
            case System.Text.Json.JsonValueKind.True:
                attributes[key] = true;
                return;
            case System.Text.Json.JsonValueKind.False:
                attributes[key] = false;
                return;
            case System.Text.Json.JsonValueKind.Number:
                attributes[key] = element.TryGetInt64(out var integer)
                    ? integer
                    : element.GetDouble();
                return;
            default:
                attributes[key] = element.GetRawText();
                return;
        }
    }
}
