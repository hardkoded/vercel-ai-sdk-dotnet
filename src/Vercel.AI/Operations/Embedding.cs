// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;

namespace Vercel.AI.Operations;

/// <summary>One embedding returned by a model call.</summary>
public sealed class EmbeddingModelResponse
{
    /// <summary>Creates a model response.</summary>
    public EmbeddingModelResponse(
        IReadOnlyList<double[]> embeddings,
        double? tokens = null,
        IReadOnlyList<OperationWarning>? warnings = null,
        JsonElement? providerMetadata = null,
        ProviderResponse? response = null,
        bool usageOmitted = false
    )
    {
        Embeddings = embeddings ?? Array.Empty<double[]>();
        Tokens = usageOmitted ? double.NaN : tokens ?? double.NaN;
        Warnings = warnings ?? Array.Empty<OperationWarning>();
        ProviderMetadata = providerMetadata;
        Response = response;
    }

    /// <summary>Vectors in request order.</summary>
    public IReadOnlyList<double[]> Embeddings { get; }

    /// <summary>Token count. <see cref="double.NaN"/> when the provider omitted usage.</summary>
    public double Tokens { get; }

    /// <summary>Provider warnings. Missing warnings become an empty list.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Raw response.</summary>
    public ProviderResponse? Response { get; }
}

/// <summary>Arguments for one <c>doEmbed</c> call.</summary>
public sealed class EmbeddingModelCall
{
    /// <summary>Creates a model call.</summary>
    public EmbeddingModelCall(
        IReadOnlyList<string> values,
        IReadOnlyDictionary<string, string> headers,
        JsonElement? providerOptions,
        CancellationToken cancellationToken,
        int? dimensions = null
    )
    {
        Values = values ?? Array.Empty<string>();
        Headers = headers ?? new Dictionary<string, string>();
        ProviderOptions = providerOptions;
        CancellationToken = cancellationToken;
        Dimensions = dimensions;
    }

    /// <summary>Requested output dimensions. Null when the caller did not ask for any.</summary>
    public int? Dimensions { get; }

    /// <summary>Values in this call.</summary>
    public IReadOnlyList<string> Values { get; }

    /// <summary>Headers, including the user-agent suffix.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Provider options for this chunk.</summary>
    public JsonElement? ProviderOptions { get; }

    /// <summary>Cancellation token.</summary>
    public CancellationToken CancellationToken { get; }
}

/// <summary>Embedding model used by <see cref="Embed"/> and <see cref="EmbedMany"/>.</summary>
public interface IEmbeddingCaller
{
    /// <summary>Provider id.</summary>
    string Provider { get; }

    /// <summary>Model id.</summary>
    string ModelId { get; }

    /// <summary>Specification version. <c>v4</c> is current. <c>v2</c> omits warnings.</summary>
    string SpecificationVersion { get; }

    /// <summary>Maximum values per call. Null means unlimited.</summary>
    int? MaxEmbeddingsPerCall { get; }

    /// <summary>Maximum UTF-8 input bytes per call. Null and infinity mean unlimited.</summary>
    double? MaxInputBytesPerCall { get; }

    /// <summary>Whether chunks may run at the same time.</summary>
    bool SupportsParallelCalls { get; }

    /// <summary>Slices provider options for one chunk. Null leaves the options unchanged.</summary>
    Task<JsonElement?> TransformProviderOptionsAsync(
        JsonElement? providerOptions,
        IReadOnlyList<string> values,
        int startIndex,
        int endIndex,
        CancellationToken cancellationToken
    );

    /// <summary>Embeds one chunk.</summary>
    Task<EmbeddingModelResponse> DoEmbedAsync(
        EmbeddingModelCall call,
        CancellationToken cancellationToken
    );
}

/// <summary>Start event for <c>embed</c> and <c>embedMany</c>.</summary>
public sealed class EmbedStartEvent : IOperationStartEvent
{
    /// <summary>Creates a start event.</summary>
    public EmbedStartEvent(
        string callId,
        string operationId,
        IReadOnlyDictionary<string, object?> runtimeContext,
        string provider,
        string modelId,
        EmbedEventValue<string, IReadOnlyList<string>> value,
        int maxRetries,
        IReadOnlyDictionary<string, string> headers,
        JsonElement? providerOptions,
        int? dimensions = null
    )
    {
        CallId = callId;
        OperationId = operationId;
        RuntimeContext = runtimeContext;
        Provider = provider;
        ModelId = modelId;
        Value = value;
        Dimensions = dimensions;
        MaxRetries = maxRetries;
        Headers = headers;
        ProviderOptions = providerOptions;
    }

    /// <summary>Correlates start and end.</summary>
    public string CallId { get; }

    /// <summary><c>ai.embed</c> or <c>ai.embedMany</c>.</summary>
    public string OperationId { get; }

    /// <summary>Caller runtime context.</summary>
    public IReadOnlyDictionary<string, object?> RuntimeContext { get; }

    /// <summary>Provider id.</summary>
    public string Provider { get; }

    /// <summary>Model id.</summary>
    public string ModelId { get; }

    /// <summary>A string for embed, or the value list for embedMany.</summary>
    public EmbedEventValue<string, IReadOnlyList<string>> Value { get; }

    /// <summary>Requested output dimensions, when specified.</summary>
    public int? Dimensions { get; }

    /// <summary>Resolved retry limit.</summary>
    public int MaxRetries { get; }

    /// <summary>Headers sent to the model.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Provider options.</summary>
    public JsonElement? ProviderOptions { get; }

    /// <summary>The version of the SDK that emitted the event. Set only on events sent to telemetry integrations. Maps to <c>aiVersion</c>.</summary>
    public string? AiVersion { get; init; }

    /// <summary>Copied from <see cref="TelemetryOptions.RecordInputs"/>. Set only on events sent to telemetry integrations.</summary>
    public bool? RecordInputs { get; init; }

    /// <summary>Copied from <see cref="TelemetryOptions.RecordOutputs"/>. Set only on events sent to telemetry integrations.</summary>
    public bool? RecordOutputs { get; init; }

    /// <summary>Copied from <see cref="TelemetryOptions.FunctionId"/>. Set only on events sent to telemetry integrations.</summary>
    public string? FunctionId { get; init; }
}

/// <summary>End event for <c>embed</c> and <c>embedMany</c>.</summary>
public sealed class EmbedEndEvent : IOperationEndEvent
{
    /// <summary>Creates an end event.</summary>
    public EmbedEndEvent(
        string callId,
        string operationId,
        IReadOnlyDictionary<string, object?> runtimeContext,
        string provider,
        string modelId,
        EmbedEventValue<string, IReadOnlyList<string>> value,
        EmbedEventValue<double[], IReadOnlyList<double[]>> embedding,
        OperationUsage usage,
        IReadOnlyList<OperationWarning> warnings,
        JsonElement? providerMetadata,
        EmbedEventValue<ProviderResponse?, IReadOnlyList<ProviderResponse?>> response
    )
    {
        CallId = callId;
        OperationId = operationId;
        RuntimeContext = runtimeContext;
        Provider = provider;
        ModelId = modelId;
        Value = value;
        Embedding = embedding;
        Usage = usage;
        Warnings = warnings;
        ProviderMetadata = providerMetadata;
        Response = response;
    }

    /// <summary>Correlates start and end.</summary>
    public string CallId { get; }

    /// <summary><c>ai.embed</c> or <c>ai.embedMany</c>.</summary>
    public string OperationId { get; }

    /// <summary>Caller runtime context.</summary>
    public IReadOnlyDictionary<string, object?> RuntimeContext { get; }

    /// <summary>Provider id.</summary>
    public string Provider { get; }

    /// <summary>Model id.</summary>
    public string ModelId { get; }

    /// <summary>Embedded value or values.</summary>
    public EmbedEventValue<string, IReadOnlyList<string>> Value { get; }

    /// <summary>One vector, or the vector list for embedMany.</summary>
    public EmbedEventValue<double[], IReadOnlyList<double[]>> Embedding { get; }

    /// <summary>Token usage.</summary>
    public OperationUsage Usage { get; }

    /// <summary>Aggregated warnings.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }

    /// <summary>Merged provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>One response for embed, or the response list for embedMany.</summary>
    public EmbedEventValue<ProviderResponse?, IReadOnlyList<ProviderResponse?>> Response { get; }

    /// <summary>The version of the SDK that emitted the event. Set only on events sent to telemetry integrations. Maps to <c>aiVersion</c>.</summary>
    public string? AiVersion { get; init; }

    /// <summary>Copied from <see cref="TelemetryOptions.RecordInputs"/>. Set only on events sent to telemetry integrations.</summary>
    public bool? RecordInputs { get; init; }

    /// <summary>Copied from <see cref="TelemetryOptions.RecordOutputs"/>. Set only on events sent to telemetry integrations.</summary>
    public bool? RecordOutputs { get; init; }

    /// <summary>Copied from <see cref="TelemetryOptions.FunctionId"/>. Set only on events sent to telemetry integrations.</summary>
    public string? FunctionId { get; init; }
}

/// <summary>Event fired before an individual <c>doEmbed</c> call. Maps to <c>EmbeddingModelCallStartEvent</c>.</summary>
public class EmbeddingModelCallStartEvent
{
    /// <summary>Creates the event.</summary>
    public EmbeddingModelCallStartEvent(
        string callId,
        string embedCallId,
        string operationId,
        string provider,
        string modelId,
        IReadOnlyList<string> values,
        int? dimensions = null
    )
    {
        CallId = callId;
        EmbedCallId = embedCallId;
        OperationId = operationId;
        Provider = provider;
        ModelId = modelId;
        Values = values;
        Dimensions = dimensions;
    }

    /// <summary>Operation call id.</summary>
    public string CallId { get; }

    /// <summary>Id of this <c>doEmbed</c> invocation.</summary>
    public string EmbedCallId { get; }

    /// <summary><c>ai.embed.doEmbed</c> or <c>ai.embedMany.doEmbed</c>.</summary>
    public string OperationId { get; }

    /// <summary>Provider id.</summary>
    public string Provider { get; }

    /// <summary>Model id.</summary>
    public string ModelId { get; }

    /// <summary>Values in this call.</summary>
    public IReadOnlyList<string> Values { get; }

    /// <summary>Requested output dimensions, when specified.</summary>
    public int? Dimensions { get; }

    /// <summary>The version of the SDK that emitted the event. Set only on events sent to telemetry integrations. Maps to <c>aiVersion</c>.</summary>
    public string? AiVersion { get; init; }

    /// <summary>Copied from <see cref="TelemetryOptions.RecordInputs"/>. Set only on events sent to telemetry integrations.</summary>
    public bool? RecordInputs { get; init; }

    /// <summary>Copied from <see cref="TelemetryOptions.RecordOutputs"/>. Set only on events sent to telemetry integrations.</summary>
    public bool? RecordOutputs { get; init; }

    /// <summary>Copied from <see cref="TelemetryOptions.FunctionId"/>. Set only on events sent to telemetry integrations.</summary>
    public string? FunctionId { get; init; }
}

/// <summary>Event fired after an individual <c>doEmbed</c> call. Maps to <c>EmbeddingModelCallEndEvent</c>.</summary>
public sealed class EmbeddingModelCallEndEvent : EmbeddingModelCallStartEvent
{
    /// <summary>Creates the event.</summary>
    public EmbeddingModelCallEndEvent(
        string callId,
        string embedCallId,
        string operationId,
        string provider,
        string modelId,
        IReadOnlyList<string> values,
        IReadOnlyList<double[]> embeddings,
        OperationUsage usage,
        int? dimensions = null
    )
        : base(callId, embedCallId, operationId, provider, modelId, values, dimensions)
    {
        Embeddings = embeddings;
        Usage = usage;
    }

    /// <summary>Embeddings of the call.</summary>
    public IReadOnlyList<double[]> Embeddings { get; }

    /// <summary>Usage of the call.</summary>
    public OperationUsage Usage { get; }
}

/// <summary>Result of <see cref="Embed.EmbedAsync"/>.</summary>
public sealed class EmbedResult
{
    /// <summary>Creates an embed result.</summary>
    public EmbedResult(
        string value,
        double[] embedding,
        OperationUsage usage,
        IReadOnlyList<OperationWarning> warnings,
        JsonElement? providerMetadata,
        ProviderResponse? response
    )
    {
        Value = value;
        Embedding = embedding;
        Usage = usage;
        Warnings = warnings ?? Array.Empty<OperationWarning>();
        ProviderMetadata = providerMetadata;
        Response = response;
    }

    /// <summary>Value that was embedded.</summary>
    public string Value { get; }

    /// <summary>Embedding vector.</summary>
    public double[] Embedding { get; }

    /// <summary>Token usage.</summary>
    public OperationUsage Usage { get; }

    /// <summary>Warnings.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Provider response.</summary>
    public ProviderResponse? Response { get; }
}

/// <summary>Result of <see cref="EmbedMany.EmbedManyAsync"/>.</summary>
public sealed class EmbedManyResult
{
    /// <summary>Creates an embedMany result.</summary>
    public EmbedManyResult(
        IReadOnlyList<string> values,
        IReadOnlyList<double[]> embeddings,
        OperationUsage usage,
        IReadOnlyList<OperationWarning> warnings,
        JsonElement? providerMetadata,
        IReadOnlyList<ProviderResponse?> responses
    )
    {
        Values = values ?? Array.Empty<string>();
        Embeddings = embeddings ?? Array.Empty<double[]>();
        Usage = usage;
        Warnings = warnings ?? Array.Empty<OperationWarning>();
        ProviderMetadata = providerMetadata;
        Responses = responses ?? Array.Empty<ProviderResponse?>();
    }

    /// <summary>Values that were embedded.</summary>
    public IReadOnlyList<string> Values { get; }

    /// <summary>Embeddings in value order.</summary>
    public IReadOnlyList<double[]> Embeddings { get; }

    /// <summary>Summed token usage.</summary>
    public OperationUsage Usage { get; }

    /// <summary>Warnings from every chunk.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }

    /// <summary>Merged provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>One response per model call.</summary>
    public IReadOnlyList<ProviderResponse?> Responses { get; }
}

/// <summary>Options for <see cref="Embed.EmbedAsync"/>.</summary>
public sealed class EmbedRequest : OperationRequest
{
    /// <summary>Embedding model.</summary>
    public IEmbeddingCaller? Model { get; set; }

    /// <summary>Value to embed.</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>Requested output dimensions. Must be a positive integer. Support depends on the model.</summary>
    public int? Dimensions { get; set; }

    /// <summary>Called before the model.</summary>
    public Func<EmbedStartEvent, Task>? OnStart { get; set; }

    /// <summary>Deprecated alias of <see cref="OnStart"/>.</summary>
    public Func<EmbedStartEvent, Task>? ExperimentalOnStart { get; set; }

    /// <summary>Called after the model.</summary>
    public Func<EmbedEndEvent, Task>? OnEnd { get; set; }

    /// <summary>Deprecated alias of <see cref="OnEnd"/>.</summary>
    public Func<EmbedEndEvent, Task>? ExperimentalOnEnd { get; set; }

    /// <summary>Telemetry integration callbacks.</summary>
    public TelemetryOptions? Telemetry { get; set; }

    /// <summary>Deprecated alias of <see cref="Telemetry"/>.</summary>
    public TelemetryOptions? ExperimentalTelemetry { get; set; }
}

/// <summary>Options for <see cref="EmbedMany.EmbedManyAsync"/>.</summary>
public sealed class EmbedManyRequest : OperationRequest
{
    /// <summary>Embedding model.</summary>
    public IEmbeddingCaller? Model { get; set; }

    /// <summary>Values to embed.</summary>
    public IReadOnlyList<string> Values { get; set; } = Array.Empty<string>();

    /// <summary>Requested output dimensions. Must be a positive integer. Support depends on the model.</summary>
    public int? Dimensions { get; set; }

    /// <summary>Concurrent chunks when the model supports parallel calls. Null means unlimited.</summary>
    public int? MaxParallelCalls { get; set; }

    /// <summary>Called before the model.</summary>
    public Func<EmbedStartEvent, Task>? OnStart { get; set; }

    /// <summary>Deprecated alias of <see cref="OnStart"/>.</summary>
    public Func<EmbedStartEvent, Task>? ExperimentalOnStart { get; set; }

    /// <summary>Called after every chunk returns.</summary>
    public Func<EmbedEndEvent, Task>? OnEnd { get; set; }

    /// <summary>Deprecated alias of <see cref="OnEnd"/>.</summary>
    public Func<EmbedEndEvent, Task>? ExperimentalOnEnd { get; set; }

    /// <summary>Telemetry integration callbacks.</summary>
    public TelemetryOptions? Telemetry { get; set; }

    /// <summary>Deprecated alias of <see cref="Telemetry"/>.</summary>
    public TelemetryOptions? ExperimentalTelemetry { get; set; }
}

/// <summary>Embeds one value. Maps to <c>embed</c>.</summary>
public static class Embed
{
    /// <summary>Embeds <see cref="EmbedRequest.Value"/>.</summary>
    public static async Task<EmbedResult> EmbedAsync(
        EmbedRequest request,
        CancellationToken cancellationToken = default
    )
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        ValidateEmbeddingDimensions(request.Dimensions);
        var model = request.Model
            ?? throw new InvalidArgumentException("model", null, "model is required");
        var token = request.CancellationToken.CanBeCanceled
            ? request.CancellationToken
            : cancellationToken;
        var maxRetries = OperationRetry.ResolveMaxRetries(request.MaxRetries);
        var headers = OperationHeaders.WithUserAgent(request.Headers, AiSdkVersion.UserAgent);
        var callId = (request.GenerateCallId ?? DefaultCallId)();
        var telemetry = request.Telemetry ?? request.ExperimentalTelemetry;
        var telemetryDispatcher =
            EmbedRestrictedTelemetryDispatcher.CreateRestrictedTelemetryDispatcher(telemetry);
        var context = request.RuntimeContext ?? EmptyContext;
        var onStart = request.OnStart ?? request.ExperimentalOnStart;
        var onEnd = request.OnEnd ?? request.ExperimentalOnEnd;
        var start = new EmbedStartEvent(
            callId,
            "ai.embed",
            context,
            model.Provider,
            model.ModelId,
            EmbedEventValue<string, IReadOnlyList<string>>.FromOne(request.Value),
            maxRetries,
            headers,
            request.ProviderOptions,
            request.Dimensions
        );
        await OperationCallbacks
            .NotifyAsync(start, onStart, e => telemetryDispatcher.OnStartAsync(e, token))
            .ConfigureAwait(false);
        try
        {
            var outcome = await OperationRetry
                .ExecuteAsync(
                    maxRetries,
                    token,
                    request.AbortReason,
                    async ct =>
                    {
                        var embedCallId = (request.GenerateCallId ?? DefaultCallId)();
                        var values = new[] { request.Value };
                        await telemetryDispatcher
                            .OnEmbedStartAsync(
                                new EmbeddingModelCallStartEvent(
                                    callId,
                                    embedCallId,
                                    "ai.embed.doEmbed",
                                    model.Provider,
                                    model.ModelId,
                                    values,
                                    request.Dimensions
                                ),
                                ct
                            )
                            .ConfigureAwait(false);
                        var response = await model
                            .DoEmbedAsync(
                                new EmbeddingModelCall(
                                    values,
                                    headers,
                                    request.ProviderOptions,
                                    ct,
                                    request.Dimensions
                                ),
                                ct
                            )
                            .ConfigureAwait(false);
                        var usage = new OperationUsage(tokens: response.Tokens);
                        await telemetryDispatcher
                            .OnEmbedEndAsync(
                                new EmbeddingModelCallEndEvent(
                                    callId,
                                    embedCallId,
                                    "ai.embed.doEmbed",
                                    model.Provider,
                                    model.ModelId,
                                    values,
                                    response.Embeddings,
                                    usage,
                                    request.Dimensions
                                ),
                                ct
                            )
                            .ConfigureAwait(false);
                        if (response.Embeddings.Count == 0)
                        {
                            throw new InvalidResponseDataException(
                                response.Embeddings,
                                "No embedding generated."
                            );
                        }

                        return response;
                    },
                    null
                )
                .ConfigureAwait(false);
            var warnings = outcome.Warnings ?? Array.Empty<OperationWarning>();
            WarningLog.Write(warnings, model.Provider, model.ModelId);
            var resultUsage = new OperationUsage(tokens: outcome.Tokens);
            var end = new EmbedEndEvent(
                callId,
                "ai.embed",
                context,
                model.Provider,
                model.ModelId,
                EmbedEventValue<string, IReadOnlyList<string>>.FromOne(request.Value),
                EmbedEventValue<double[], IReadOnlyList<double[]>>.FromOne(outcome.Embeddings[0]),
                resultUsage,
                warnings,
                outcome.ProviderMetadata,
                EmbedEventValue<ProviderResponse?, IReadOnlyList<ProviderResponse?>>.FromOne(
                    outcome.Response
                )
            );
            await OperationCallbacks
                .NotifyAsync(end, onEnd, e => telemetryDispatcher.OnEndAsync(e, token))
                .ConfigureAwait(false);
            return new EmbedResult(
                request.Value,
                outcome.Embeddings[0],
                resultUsage,
                warnings,
                outcome.ProviderMetadata,
                outcome.Response
            );
        }
        catch (Exception error)
        {
            await telemetryDispatcher.OnErrorAsync(callId, error, token).ConfigureAwait(false);

            throw;
        }
    }

    /// <summary>Throws when <paramref name="dimensions"/> is set and is not a positive integer.</summary>
    public static void ValidateEmbeddingDimensions(double? dimensions)
    {
        if (
            dimensions is double value
            && (
                double.IsNaN(value)
                || double.IsInfinity(value)
                || value != Math.Floor(value)
                || value <= 0
            )
        )
        {
            throw new InvalidArgumentException(
                "dimensions",
                value,
                "dimensions must be a positive integer"
            );
        }
    }

    internal static string DefaultCallId()
    {
        return "call-"
            + Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture).Substring(0, 24);
    }

    internal static readonly IReadOnlyDictionary<string, object?> EmptyContext =
        new Dictionary<string, object?>();

    internal static IReadOnlyDictionary<string, object?> FilterContext(
        IReadOnlyDictionary<string, object?> source,
        IReadOnlyDictionary<string, bool>? include
    )
    {
        if (include == null)
        {
            return EmptyContext;
        }

        var filtered = new Dictionary<string, object?>();
        foreach (var pair in include)
        {
            if (pair.Value && source.TryGetValue(pair.Key, out var value))
            {
                filtered[pair.Key] = value;
            }
        }

        return filtered;
    }
}

/// <summary>Embeds many values, splitting on count and UTF-8 byte limits. Maps to <c>embedMany</c>.</summary>
public static class EmbedMany
{
    /// <summary>Embeds <see cref="EmbedManyRequest.Values"/>.</summary>
    public static async Task<EmbedManyResult> EmbedManyAsync(
        EmbedManyRequest request,
        CancellationToken cancellationToken = default
    )
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        Embed.ValidateEmbeddingDimensions(request.Dimensions);
        var model = request.Model
            ?? throw new InvalidArgumentException("model", null, "model is required");
        var token = request.CancellationToken.CanBeCanceled
            ? request.CancellationToken
            : cancellationToken;
        var maxRetries = OperationRetry.ResolveMaxRetries(request.MaxRetries);
        var headers = OperationHeaders.WithUserAgent(request.Headers, AiSdkVersion.UserAgent);
        var callId = (request.GenerateCallId ?? Embed.DefaultCallId)();
        var telemetry = request.Telemetry ?? request.ExperimentalTelemetry;
        var telemetryDispatcher =
            EmbedRestrictedTelemetryDispatcher.CreateRestrictedTelemetryDispatcher(telemetry);
        var context = request.RuntimeContext ?? Embed.EmptyContext;
        var onStart = request.OnStart ?? request.ExperimentalOnStart;
        var onEnd = request.OnEnd ?? request.ExperimentalOnEnd;
        var values = request.Values ?? Array.Empty<string>();
        var start = new EmbedStartEvent(
            callId,
            "ai.embedMany",
            context,
            model.Provider,
            model.ModelId,
            EmbedEventValue<string, IReadOnlyList<string>>.FromMany(values),
            maxRetries,
            headers,
            request.ProviderOptions,
            request.Dimensions
        );
        await OperationCallbacks
            .NotifyAsync(start, onStart, e => telemetryDispatcher.OnStartAsync(e, token))
            .ConfigureAwait(false);
        try
        {
            var hasCountLimit = model.MaxEmbeddingsPerCall is int limit && limit != int.MaxValue;
            var byteLimit = model.MaxInputBytesPerCall;
            var hasByteLimit =
                byteLimit is double bytes
                && !double.IsPositiveInfinity(bytes)
                && !double.IsNaN(bytes);
            if (!hasCountLimit && !hasByteLimit)
            {
                var single = await CallAsync(
                        model,
                        request,
                        values,
                        headers,
                        request.ProviderOptions,
                        token,
                        callId,
                        "ai.embedMany.doEmbed",
                        telemetryDispatcher,
                        request.Dimensions
                    )
                    .ConfigureAwait(false);
                ValidateCount(single.Embeddings, values);
                WarningLog.Write(single.Warnings, model.Provider, model.ModelId);
                var usage = new OperationUsage(tokens: single.Tokens);
                var singleResponses = new ProviderResponse?[] { single.Response };
                var end = new EmbedEndEvent(
                    callId,
                    "ai.embedMany",
                    context,
                    model.Provider,
                    model.ModelId,
                    EmbedEventValue<string, IReadOnlyList<string>>.FromMany(values),
                    EmbedEventValue<double[], IReadOnlyList<double[]>>.FromMany(single.Embeddings),
                    usage,
                    single.Warnings,
                    single.ProviderMetadata,
                    EmbedEventValue<ProviderResponse?, IReadOnlyList<ProviderResponse?>>.FromMany(
                        singleResponses
                    )
                );
                await OperationCallbacks
                    .NotifyAsync(end, onEnd, e => telemetryDispatcher.OnEndAsync(e, token))
                    .ConfigureAwait(false);
                return new EmbedManyResult(
                    values,
                    single.Embeddings,
                    usage,
                    single.Warnings,
                    single.ProviderMetadata,
                    singleResponses
                );
            }

            var chunks = SplitByLimits(
                values,
                hasCountLimit ? model.MaxEmbeddingsPerCall!.Value : int.MaxValue,
                hasByteLimit ? byteLimit!.Value : double.PositiveInfinity
            );
            var parallel = model.SupportsParallelCalls
                ? request.MaxParallelCalls ?? int.MaxValue
                : 1;
            var groups = SplitGroups(chunks, parallel);
            var embeddings = new List<double[]>();
            var warnings = new List<OperationWarning>();
            var responses = new List<ProviderResponse?>();
            double tokens = 0;
            JsonElement? metadata = null;
            var nextIndex = 0;
            foreach (var group in groups)
            {
                var tasks = new List<Task<ChunkOutcome>>(group.Count);
                foreach (var chunk in group)
                {
                    var startIndex = nextIndex;
                    nextIndex += chunk.Count;
                    tasks.Add(
                        RunChunkAsync(
                            model,
                            request,
                            values,
                            chunk,
                            startIndex,
                            headers,
                            token,
                            callId,
                            telemetryDispatcher
                        )
                    );
                }

                var results = await Task.WhenAll(tasks).ConfigureAwait(false);
                foreach (var result in results)
                {
                    embeddings.AddRange(result.Response.Embeddings);
                    warnings.AddRange(result.Response.Warnings);
                    responses.Add(result.Response.Response);
                    tokens += result.Response.Tokens;
                    metadata = MergeMetadata(metadata, result.Response.ProviderMetadata);
                }
            }

            WarningLog.Write(warnings, model.Provider, model.ModelId);
            var total = new OperationUsage(tokens: tokens);
            var endEvent = new EmbedEndEvent(
                callId,
                "ai.embedMany",
                context,
                model.Provider,
                model.ModelId,
                EmbedEventValue<string, IReadOnlyList<string>>.FromMany(values),
                EmbedEventValue<double[], IReadOnlyList<double[]>>.FromMany(embeddings),
                total,
                warnings,
                metadata,
                EmbedEventValue<ProviderResponse?, IReadOnlyList<ProviderResponse?>>.FromMany(
                    responses
                )
            );
            await OperationCallbacks
                .NotifyAsync(endEvent, onEnd, e => telemetryDispatcher.OnEndAsync(e, token))
                .ConfigureAwait(false);
            return new EmbedManyResult(values, embeddings, total, warnings, metadata, responses);
        }
        catch (Exception error)
        {
            await telemetryDispatcher.OnErrorAsync(callId, error, token).ConfigureAwait(false);

            throw;
        }
    }

    /// <summary>Wraps a model so middleware can rewrite each <c>doEmbed</c> call.</summary>
    public static IEmbeddingCaller Wrap(
        IEmbeddingCaller model,
        Func<
            EmbeddingModelCall,
            IEmbeddingCaller,
            CancellationToken,
            Task<EmbeddingModelCall>
        >? transformParams
    )
    {
        if (model == null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        return new WrappedEmbeddingCaller(model, transformParams);
    }

    private static async Task<ChunkOutcome> RunChunkAsync(
        IEmbeddingCaller model,
        EmbedManyRequest request,
        IReadOnlyList<string> values,
        IReadOnlyList<string> chunk,
        int startIndex,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken token,
        string callId,
        EmbedRestrictedTelemetryDispatcher telemetryDispatcher
    )
    {
        var options = await model
            .TransformProviderOptionsAsync(
                request.ProviderOptions,
                values,
                startIndex,
                startIndex + chunk.Count,
                token
            )
            .ConfigureAwait(false);
        var response = await CallAsync(
                model,
                request,
                chunk,
                headers,
                options,
                token,
                callId,
                "ai.embedMany.doEmbed",
                telemetryDispatcher,
                request.Dimensions
            )
            .ConfigureAwait(false);
        ValidateCount(response.Embeddings, chunk);
        return new ChunkOutcome(response);
    }

    private static Task<EmbeddingModelResponse> CallAsync(
        IEmbeddingCaller model,
        OperationRequest request,
        IReadOnlyList<string> values,
        IReadOnlyDictionary<string, string> headers,
        JsonElement? providerOptions,
        CancellationToken token,
        string callId,
        string operationId,
        EmbedRestrictedTelemetryDispatcher telemetryDispatcher,
        int? dimensions
    )
    {
        var maxRetries = OperationRetry.ResolveMaxRetries(request.MaxRetries);
        return OperationRetry.ExecuteAsync(
            maxRetries,
            token,
            request.AbortReason,
            async ct =>
            {
                var embedCallId = (request.GenerateCallId ?? Embed.DefaultCallId)();
                await telemetryDispatcher
                    .OnEmbedStartAsync(
                        new EmbeddingModelCallStartEvent(
                            callId,
                            embedCallId,
                            operationId,
                            model.Provider,
                            model.ModelId,
                            values,
                            dimensions
                        ),
                        ct
                    )
                    .ConfigureAwait(false);

                var response = await model
                    .DoEmbedAsync(
                        new EmbeddingModelCall(values, headers, providerOptions, ct, dimensions),
                        ct
                    )
                    .ConfigureAwait(false);
                await telemetryDispatcher
                    .OnEmbedEndAsync(
                        new EmbeddingModelCallEndEvent(
                            callId,
                            embedCallId,
                            operationId,
                            model.Provider,
                            model.ModelId,
                            values,
                            response.Embeddings,
                            new OperationUsage(tokens: response.Tokens),
                            dimensions
                        ),
                        ct
                    )
                    .ConfigureAwait(false);

                return response;
            },
            null
        );
    }

    private static void ValidateCount(
        IReadOnlyList<double[]> embeddings,
        IReadOnlyList<string> values
    )
    {
        if (embeddings.Count != values.Count)
        {
            throw new InvalidResponseDataException(
                embeddings,
                "Expected "
                    + values.Count.ToString(CultureInfo.InvariantCulture)
                    + " embeddings, but received "
                    + embeddings.Count.ToString(CultureInfo.InvariantCulture)
                    + "."
            );
        }
    }

    /// <summary>Splits values by embedding count and UTF-8 byte budget.</summary>
    public static IReadOnlyList<IReadOnlyList<string>> SplitByLimits(
        IReadOnlyList<string> values,
        int maxEmbeddingsPerCall,
        double maxInputBytesPerCall
    )
    {
        if (maxEmbeddingsPerCall <= 0)
        {
            throw new InvalidOperationException("maxEmbeddingsPerCall must be greater than 0");
        }

        if (maxInputBytesPerCall <= 0)
        {
            throw new InvalidOperationException("maxInputBytesPerCall must be greater than 0");
        }

        if (values.Count == 0)
        {
            return Array.Empty<IReadOnlyList<string>>();
        }

        var chunks = new List<IReadOnlyList<string>>();
        var current = new List<string>();
        var currentBytes = 0;
        foreach (var value in values)
        {
            var inputBytes = OperationJson.Utf8Length(value);
            if (
                current.Count > 0
                && (
                    current.Count >= maxEmbeddingsPerCall
                    || currentBytes + inputBytes > maxInputBytesPerCall
                )
            )
            {
                chunks.Add(current);
                current = new List<string>();
                currentBytes = 0;
            }

            current.Add(value);
            currentBytes += inputBytes;
        }

        chunks.Add(current);
        return chunks;
    }

    private static List<List<IReadOnlyList<string>>> SplitGroups(
        IReadOnlyList<IReadOnlyList<string>> chunks,
        int chunkSize
    )
    {
        if (chunkSize <= 0)
        {
            throw new InvalidArgumentException(
                "chunkSize",
                chunkSize,
                "chunkSize must be greater than 0"
            );
        }

        var groups = new List<List<IReadOnlyList<string>>>();
        if (chunkSize >= chunks.Count)
        {
            groups.Add(new List<IReadOnlyList<string>>(chunks));
            return groups;
        }

        for (var i = 0; i < chunks.Count; i += chunkSize)
        {
            var count = Math.Min(chunkSize, chunks.Count - i);
            var group = new List<IReadOnlyList<string>>(count);
            for (var j = 0; j < count; j++)
            {
                group.Add(chunks[i + j]);
            }

            groups.Add(group);
        }

        return groups;
    }

    internal static JsonElement? MergeMetadata(JsonElement? current, JsonElement? incoming)
    {
        if (incoming == null || incoming.Value.ValueKind != JsonValueKind.Object)
        {
            return current;
        }

        if (current == null || current.Value.ValueKind != JsonValueKind.Object)
        {
            return incoming;
        }

        var merged = new Dictionary<string, JsonElement>();
        foreach (var property in current.Value.EnumerateObject())
        {
            merged[property.Name] = property.Value.Clone();
        }

        foreach (var property in incoming.Value.EnumerateObject())
        {
            if (
                merged.TryGetValue(property.Name, out var existing)
                && existing.ValueKind == JsonValueKind.Object
                && property.Value.ValueKind == JsonValueKind.Object
            )
            {
                var inner = new Dictionary<string, JsonElement>();
                foreach (var child in existing.EnumerateObject())
                {
                    inner[child.Name] = child.Value.Clone();
                }

                foreach (var child in property.Value.EnumerateObject())
                {
                    inner[child.Name] = child.Value.Clone();
                }

                merged[property.Name] = OperationJson.Serialize(inner);
            }
            else
            {
                merged[property.Name] = property.Value.Clone();
            }
        }

        return OperationJson.Serialize(merged);
    }

    private sealed class ChunkOutcome
    {
        public ChunkOutcome(EmbeddingModelResponse response)
        {
            Response = response;
        }

        public EmbeddingModelResponse Response { get; }
    }

    private sealed class WrappedEmbeddingCaller : IEmbeddingCaller
    {
        private readonly IEmbeddingCaller _inner;
        private readonly Func<
            EmbeddingModelCall,
            IEmbeddingCaller,
            CancellationToken,
            Task<EmbeddingModelCall>
        >? _transform;

        public WrappedEmbeddingCaller(
            IEmbeddingCaller inner,
            Func<
                EmbeddingModelCall,
                IEmbeddingCaller,
                CancellationToken,
                Task<EmbeddingModelCall>
            >? transform
        )
        {
            _inner = inner;
            _transform = transform;
        }

        public string Provider => _inner.Provider;

        public string ModelId => _inner.ModelId;

        public string SpecificationVersion => _inner.SpecificationVersion;

        public int? MaxEmbeddingsPerCall => _inner.MaxEmbeddingsPerCall;

        public double? MaxInputBytesPerCall => _inner.MaxInputBytesPerCall;

        public bool SupportsParallelCalls => _inner.SupportsParallelCalls;

        public Task<JsonElement?> TransformProviderOptionsAsync(
            JsonElement? providerOptions,
            IReadOnlyList<string> values,
            int startIndex,
            int endIndex,
            CancellationToken cancellationToken
        )
        {
            return _inner.TransformProviderOptionsAsync(
                providerOptions,
                values,
                startIndex,
                endIndex,
                cancellationToken
            );
        }

        public async Task<EmbeddingModelResponse> DoEmbedAsync(
            EmbeddingModelCall call,
            CancellationToken cancellationToken
        )
        {
            var transformed =
                _transform == null
                    ? call
                    : await _transform(call, _inner, cancellationToken).ConfigureAwait(false);
            return await _inner.DoEmbedAsync(transformed, cancellationToken).ConfigureAwait(false);
        }
    }
}
