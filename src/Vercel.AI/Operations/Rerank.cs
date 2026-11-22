// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.Operations;

/// <summary>Documents sent to a reranking model.</summary>
public sealed class RerankModelDocuments
{
    /// <summary>Creates a document batch.</summary>
    public RerankModelDocuments(string type, IReadOnlyList<object?> values)
    {
        Type = type;
        Values = values ?? Array.Empty<object?>();
    }

    /// <summary><c>text</c> or <c>object</c>.</summary>
    public string Type { get; }

    /// <summary>Document values.</summary>
    public IReadOnlyList<object?> Values { get; }
}

/// <summary>One ranked index from the model.</summary>
public sealed class RerankModelRank
{
    /// <summary>Creates a rank.</summary>
    public RerankModelRank(int index, double relevanceScore)
    {
        Index = index;
        RelevanceScore = relevanceScore;
    }

    /// <summary>Index in the submitted document list.</summary>
    public int Index { get; }

    /// <summary>Relevance score.</summary>
    public double RelevanceScore { get; }
}

/// <summary>Model response for <c>doRerank</c>.</summary>
public sealed class RerankModelResponse
{
    /// <summary>Creates a model response.</summary>
    public RerankModelResponse(
        IReadOnlyList<RerankModelRank> ranking,
        IReadOnlyList<OperationWarning>? warnings = null,
        JsonElement? providerMetadata = null,
        ProviderResponse? response = null
    )
    {
        Ranking = ranking ?? Array.Empty<RerankModelRank>();
        Warnings = warnings ?? Array.Empty<OperationWarning>();
        ProviderMetadata = providerMetadata;
        Response = response;
    }

    /// <summary>Ranked indexes.</summary>
    public IReadOnlyList<RerankModelRank> Ranking { get; }

    /// <summary>Warnings.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Raw response.</summary>
    public ProviderResponse? Response { get; }
}

/// <summary>Arguments for <c>doRerank</c>.</summary>
public sealed class RerankModelCall
{
    /// <summary>Creates a model call.</summary>
    public RerankModelCall(
        RerankModelDocuments documents,
        string query,
        int? topN,
        JsonElement? providerOptions,
        IReadOnlyDictionary<string, string>? headers,
        CancellationToken cancellationToken
    )
    {
        Documents = documents;
        Query = query ?? string.Empty;
        TopN = topN;
        ProviderOptions = providerOptions;
        Headers = headers;
        CancellationToken = cancellationToken;
    }

    /// <summary>Documents.</summary>
    public RerankModelDocuments Documents { get; }

    /// <summary>Query.</summary>
    public string Query { get; }

    /// <summary>Maximum results.</summary>
    public int? TopN { get; }

    /// <summary>Provider options.</summary>
    public JsonElement? ProviderOptions { get; }

    /// <summary>Request headers. Rerank does not append a user-agent.</summary>
    public IReadOnlyDictionary<string, string>? Headers { get; }

    /// <summary>Cancellation token.</summary>
    public CancellationToken CancellationToken { get; }
}

/// <summary>Reranking model used by <see cref="Rerank"/>.</summary>
public interface IRerankCaller
{
    /// <summary>Provider id.</summary>
    string Provider { get; }

    /// <summary>Model id.</summary>
    string ModelId { get; }

    /// <summary>Reranks documents.</summary>
    Task<RerankModelResponse> DoRerankAsync(
        RerankModelCall call,
        CancellationToken cancellationToken
    );
}

/// <summary>One document in the reranked order.</summary>
public sealed class RankedDocument
{
    /// <summary>Creates a ranked document.</summary>
    public RankedDocument(int originalIndex, double score, object? document)
    {
        OriginalIndex = originalIndex;
        Score = score;
        Document = document;
    }

    /// <summary>Index in <see cref="RerankResult.OriginalDocuments"/>.</summary>
    public int OriginalIndex { get; }

    /// <summary>Relevance score.</summary>
    public double Score { get; }

    /// <summary>Original document.</summary>
    public object? Document { get; }
}

/// <summary>Response metadata for a rerank call.</summary>
public sealed class RerankResponseInfo
{
    /// <summary>Creates response metadata.</summary>
    public RerankResponseInfo(
        string? id,
        DateTime timestamp,
        string modelId,
        IReadOnlyDictionary<string, string>? headers,
        JsonElement? body
    )
    {
        Id = id;
        Timestamp = timestamp;
        ModelId = modelId;
        Headers = headers;
        Body = body;
    }

    /// <summary>Provider response id.</summary>
    public string? Id { get; }

    /// <summary>Response timestamp.</summary>
    public DateTime Timestamp { get; }

    /// <summary>Model id.</summary>
    public string ModelId { get; }

    /// <summary>Response headers.</summary>
    public IReadOnlyDictionary<string, string>? Headers { get; }

    /// <summary>Response body.</summary>
    public JsonElement? Body { get; }
}

/// <summary>Result of <see cref="Rerank.RerankAsync"/>.</summary>
public sealed class RerankResult
{
    private readonly IReadOnlyList<RankedDocument> _ranking;

    /// <summary>Creates a rerank result. The ranking list is copied.</summary>
    public RerankResult(
        IReadOnlyList<object?> originalDocuments,
        IReadOnlyList<RankedDocument> ranking,
        JsonElement? providerMetadata,
        RerankResponseInfo response
    )
    {
        OriginalDocuments = originalDocuments ?? Array.Empty<object?>();
        var copy = new List<RankedDocument>();
        if (ranking != null)
        {
            copy.AddRange(ranking);
        }

        _ranking = copy;
        ProviderMetadata = providerMetadata;
        Response = response ?? throw new ArgumentNullException(nameof(response));
    }

    /// <summary>Documents that were submitted.</summary>
    public IReadOnlyList<object?> OriginalDocuments { get; }

    /// <summary>Ranking. Mutations of the list passed to <c>onEnd</c> do not change this copy.</summary>
    public IReadOnlyList<RankedDocument> Ranking => _ranking;

    /// <summary>Documents in ranking order.</summary>
    public IReadOnlyList<object?> RerankedDocuments
    {
        get
        {
            var documents = new List<object?>(_ranking.Count);
            foreach (var rank in _ranking)
            {
                documents.Add(rank.Document);
            }

            return documents;
        }
    }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Response metadata.</summary>
    public RerankResponseInfo Response { get; }
}

/// <summary>Start event for rerank.</summary>
public sealed class RerankStartEvent : IOperationStartEvent
{
    /// <summary>Creates a start event.</summary>
    public RerankStartEvent(
        string callId,
        string operationId,
        IReadOnlyDictionary<string, object?> runtimeContext,
        string provider,
        string modelId,
        IReadOnlyList<object?> documents,
        string query,
        int? topN,
        int maxRetries,
        IReadOnlyDictionary<string, string>? headers,
        JsonElement? providerOptions
    )
    {
        CallId = callId;
        OperationId = operationId;
        RuntimeContext = runtimeContext;
        Provider = provider;
        ModelId = modelId;
        Documents = documents;
        Query = query;
        TopN = topN;
        MaxRetries = maxRetries;
        Headers = headers;
        ProviderOptions = providerOptions;
    }

    /// <summary>Call id.</summary>
    public string CallId { get; }

    /// <summary><c>ai.rerank</c>.</summary>
    public string OperationId { get; }

    /// <summary>Runtime context.</summary>
    public IReadOnlyDictionary<string, object?> RuntimeContext { get; }

    /// <summary>Provider id.</summary>
    public string Provider { get; }

    /// <summary>Model id.</summary>
    public string ModelId { get; }

    /// <summary>Submitted documents.</summary>
    public IReadOnlyList<object?> Documents { get; }

    /// <summary>Query.</summary>
    public string Query { get; }

    /// <summary>Maximum results.</summary>
    public int? TopN { get; }

    /// <summary>Retry limit.</summary>
    public int MaxRetries { get; }

    /// <summary>Request headers.</summary>
    public IReadOnlyDictionary<string, string>? Headers { get; }

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

/// <summary>End event for rerank.</summary>
public sealed class RerankEndEvent : IOperationEndEvent
{
    /// <summary>Creates an end event.</summary>
    public RerankEndEvent(
        string callId,
        string operationId,
        IReadOnlyDictionary<string, object?> runtimeContext,
        string provider,
        string modelId,
        IReadOnlyList<object?> documents,
        string query,
        IReadOnlyList<RankedDocument> ranking,
        IReadOnlyList<OperationWarning> warnings,
        JsonElement? providerMetadata,
        RerankResponseInfo response
    )
    {
        CallId = callId;
        OperationId = operationId;
        RuntimeContext = runtimeContext;
        Provider = provider;
        ModelId = modelId;
        Documents = documents;
        Query = query;
        Ranking = ranking;
        Warnings = warnings;
        ProviderMetadata = providerMetadata;
        Response = response;
    }

    /// <summary>Call id.</summary>
    public string CallId { get; }

    /// <summary><c>ai.rerank</c>.</summary>
    public string OperationId { get; }

    /// <summary>Runtime context.</summary>
    public IReadOnlyDictionary<string, object?> RuntimeContext { get; }

    /// <summary>Provider id.</summary>
    public string Provider { get; }

    /// <summary>Model id.</summary>
    public string ModelId { get; }

    /// <summary>Submitted documents.</summary>
    public IReadOnlyList<object?> Documents { get; }

    /// <summary>Query.</summary>
    public string Query { get; }

    /// <summary>Ranking visible to the callback. The result keeps its own copy.</summary>
    public IReadOnlyList<RankedDocument> Ranking { get; }

    /// <summary>Warnings.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Response metadata.</summary>
    public RerankResponseInfo Response { get; }

    /// <summary>The version of the SDK that emitted the event. Set only on events sent to telemetry integrations. Maps to <c>aiVersion</c>.</summary>
    public string? AiVersion { get; init; }

    /// <summary>Copied from <see cref="TelemetryOptions.RecordInputs"/>. Set only on events sent to telemetry integrations.</summary>
    public bool? RecordInputs { get; init; }

    /// <summary>Copied from <see cref="TelemetryOptions.RecordOutputs"/>. Set only on events sent to telemetry integrations.</summary>
    public bool? RecordOutputs { get; init; }

    /// <summary>Copied from <see cref="TelemetryOptions.FunctionId"/>. Set only on events sent to telemetry integrations.</summary>
    public string? FunctionId { get; init; }
}

/// <summary>Event fired before the <c>doRerank</c> call. Maps to <c>RerankingModelCallStartEvent</c>.</summary>
public class RerankingModelCallStartEvent
{
    /// <summary>Creates the event.</summary>
    public RerankingModelCallStartEvent(
        string callId,
        string operationId,
        string provider,
        string modelId,
        IReadOnlyList<object?> documents,
        string documentsType,
        string query,
        int? topN
    )
    {
        CallId = callId;
        OperationId = operationId;
        Provider = provider;
        ModelId = modelId;
        Documents = documents;
        DocumentsType = documentsType;
        Query = query;
        TopN = topN;
    }

    /// <summary>Call id.</summary>
    public string CallId { get; }

    /// <summary><c>ai.rerank.doRerank</c>.</summary>
    public string OperationId { get; }

    /// <summary>Provider id.</summary>
    public string Provider { get; }

    /// <summary>Model id.</summary>
    public string ModelId { get; }

    /// <summary>Documents sent to the model.</summary>
    public IReadOnlyList<object?> Documents { get; }

    /// <summary><c>text</c> or <c>object</c>.</summary>
    public string DocumentsType { get; }

    /// <summary>Query.</summary>
    public string Query { get; }

    /// <summary>Maximum results.</summary>
    public int? TopN { get; }

    /// <summary>The version of the SDK that emitted the event. Set only on events sent to telemetry integrations. Maps to <c>aiVersion</c>.</summary>
    public string? AiVersion { get; init; }

    /// <summary>Copied from <see cref="TelemetryOptions.RecordInputs"/>. Set only on events sent to telemetry integrations.</summary>
    public bool? RecordInputs { get; init; }

    /// <summary>Copied from <see cref="TelemetryOptions.RecordOutputs"/>. Set only on events sent to telemetry integrations.</summary>
    public bool? RecordOutputs { get; init; }

    /// <summary>Copied from <see cref="TelemetryOptions.FunctionId"/>. Set only on events sent to telemetry integrations.</summary>
    public string? FunctionId { get; init; }
}

/// <summary>Event fired after the <c>doRerank</c> call. Maps to <c>RerankingModelCallEndEvent</c>.</summary>
public sealed class RerankingModelCallEndEvent
{
    /// <summary>Creates the event.</summary>
    public RerankingModelCallEndEvent(
        string callId,
        string operationId,
        string provider,
        string modelId,
        string documentsType,
        IReadOnlyList<RerankModelRank> ranking
    )
    {
        CallId = callId;
        OperationId = operationId;
        Provider = provider;
        ModelId = modelId;
        DocumentsType = documentsType;
        Ranking = ranking;
    }

    /// <summary>Call id.</summary>
    public string CallId { get; }

    /// <summary><c>ai.rerank.doRerank</c>.</summary>
    public string OperationId { get; }

    /// <summary>Provider id.</summary>
    public string Provider { get; }

    /// <summary>Model id.</summary>
    public string ModelId { get; }

    /// <summary><c>text</c> or <c>object</c>.</summary>
    public string DocumentsType { get; }

    /// <summary>Ranking returned by the model.</summary>
    public IReadOnlyList<RerankModelRank> Ranking { get; }

    /// <summary>The version of the SDK that emitted the event. Set only on events sent to telemetry integrations. Maps to <c>aiVersion</c>.</summary>
    public string? AiVersion { get; init; }

    /// <summary>Copied from <see cref="TelemetryOptions.RecordInputs"/>. Set only on events sent to telemetry integrations.</summary>
    public bool? RecordInputs { get; init; }

    /// <summary>Copied from <see cref="TelemetryOptions.RecordOutputs"/>. Set only on events sent to telemetry integrations.</summary>
    public bool? RecordOutputs { get; init; }

    /// <summary>Copied from <see cref="TelemetryOptions.FunctionId"/>. Set only on events sent to telemetry integrations.</summary>
    public string? FunctionId { get; init; }
}

/// <summary>Options for <see cref="Rerank.RerankAsync"/>.</summary>
public sealed class RerankRequest : OperationRequest
{
    /// <summary>Reranking model.</summary>
    public IRerankCaller? Model { get; set; }

    /// <summary>Documents. Strings are sent as text; anything else is sent as objects.</summary>
    public IReadOnlyList<object?> Documents { get; set; } = Array.Empty<object?>();

    /// <summary>Query.</summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>Maximum results.</summary>
    public int? TopN { get; set; }

    /// <summary>Called before the model.</summary>
    public Func<RerankStartEvent, Task>? OnStart { get; set; }

    /// <summary>Deprecated alias of <see cref="OnStart"/>.</summary>
    public Func<RerankStartEvent, Task>? ExperimentalOnStart { get; set; }

    /// <summary>Called after the model.</summary>
    public Func<RerankEndEvent, Task>? OnEnd { get; set; }

    /// <summary>Deprecated alias of <see cref="OnEnd"/>.</summary>
    public Func<RerankEndEvent, Task>? ExperimentalOnEnd { get; set; }

    /// <summary>Telemetry callbacks.</summary>
    public TelemetryOptions? Telemetry { get; set; }

    /// <summary>Deprecated alias of <see cref="Telemetry"/>.</summary>
    public TelemetryOptions? ExperimentalTelemetry { get; set; }
}

/// <summary>Reranks documents. Maps to <c>rerank</c>.</summary>
public static class Rerank
{
    /// <summary>Reranks <see cref="RerankRequest.Documents"/>.</summary>
    public static async Task<RerankResult> RerankAsync(
        RerankRequest request,
        CancellationToken cancellationToken = default
    )
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var model = request.Model
            ?? throw new InvalidArgumentException("model", null, "model is required");
        var documents = request.Documents ?? Array.Empty<object?>();
        var callId = (request.GenerateCallId ?? Embed.DefaultCallId)();
        var telemetry = request.Telemetry ?? request.ExperimentalTelemetry;
        var telemetryDispatcher =
            RerankRestrictedTelemetryDispatcher.CreateRestrictedTelemetryDispatcher(telemetry);
        var context = request.RuntimeContext ?? Embed.EmptyContext;
        var onStart = request.OnStart ?? request.ExperimentalOnStart;
        var onEnd = request.OnEnd ?? request.ExperimentalOnEnd;
        var now = request.Now ?? (() => DateTime.UtcNow);
        var token = request.CancellationToken.CanBeCanceled
            ? request.CancellationToken
            : cancellationToken;
        if (documents.Count == 0)
        {
            var emptyRetries = request.MaxRetries ?? 2;
            var emptyStart = new RerankStartEvent(
                callId,
                "ai.rerank",
                context,
                model.Provider,
                model.ModelId,
                documents,
                request.Query,
                request.TopN,
                emptyRetries,
                request.Headers,
                request.ProviderOptions
            );
            await NotifyStartAsync(emptyStart, onStart, telemetryDispatcher, token)
                .ConfigureAwait(false);
            var emptyResponse = new RerankResponseInfo(null, now(), model.ModelId, null, null);
            var emptyRanking = Array.Empty<RankedDocument>();
            var emptyEnd = new RerankEndEvent(
                callId,
                "ai.rerank",
                context,
                model.Provider,
                model.ModelId,
                documents,
                request.Query,
                emptyRanking,
                Array.Empty<OperationWarning>(),
                null,
                emptyResponse
            );
            await NotifyEndAsync(emptyEnd, onEnd, telemetryDispatcher, token).ConfigureAwait(false);
            return new RerankResult(Array.Empty<object?>(), emptyRanking, null, emptyResponse);
        }

        var maxRetries = OperationRetry.ResolveMaxRetries(request.MaxRetries);
        var text = documents[0] is string;
        var sent = new RerankModelDocuments(text ? "text" : "object", documents);
        var start = new RerankStartEvent(
            callId,
            "ai.rerank",
            context,
            model.Provider,
            model.ModelId,
            documents,
            request.Query,
            request.TopN,
            maxRetries,
            request.Headers,
            request.ProviderOptions
        );
        await NotifyStartAsync(start, onStart, telemetryDispatcher, token).ConfigureAwait(false);
        try
        {
            var outcome = await OperationRetry
                .ExecuteAsync(
                    maxRetries,
                    token,
                    request.AbortReason,
                    async ct =>
                    {
                        await telemetryDispatcher
                            .OnRerankStartAsync(
                                new RerankingModelCallStartEvent(
                                    callId,
                                    "ai.rerank.doRerank",
                                    model.Provider,
                                    model.ModelId,
                                    documents,
                                    sent.Type,
                                    request.Query,
                                    request.TopN
                                ),
                                ct
                            )
                            .ConfigureAwait(false);

                        var response = await model
                            .DoRerankAsync(
                                new RerankModelCall(
                                    sent,
                                    request.Query,
                                    request.TopN,
                                    request.ProviderOptions,
                                    request.Headers,
                                    ct
                                ),
                                ct
                            )
                            .ConfigureAwait(false);
                        await telemetryDispatcher
                            .OnRerankEndAsync(
                                new RerankingModelCallEndEvent(
                                    callId,
                                    "ai.rerank.doRerank",
                                    model.Provider,
                                    model.ModelId,
                                    sent.Type,
                                    response.Ranking
                                ),
                                ct
                            )
                            .ConfigureAwait(false);

                        return response;
                    },
                    null
                )
                .ConfigureAwait(false);
            Validate(outcome.Ranking, documents);
            WarningLog.Write(outcome.Warnings, model.Provider, model.ModelId);
            var ranking = new List<RankedDocument>();
            foreach (var rank in outcome.Ranking)
            {
                ranking.Add(
                    new RankedDocument(rank.Index, rank.RelevanceScore, documents[rank.Index])
                );
            }

            var providerResponse = outcome.Response;
            var responseInfo = new RerankResponseInfo(
                providerResponse?.Id,
                providerResponse?.Timestamp ?? now(),
                providerResponse?.ModelId ?? model.ModelId,
                providerResponse?.Headers,
                providerResponse?.Body
            );
            var callbackRanking = new List<RankedDocument>(ranking);
            var end = new RerankEndEvent(
                callId,
                "ai.rerank",
                context,
                model.Provider,
                model.ModelId,
                documents,
                request.Query,
                callbackRanking,
                outcome.Warnings,
                outcome.ProviderMetadata,
                responseInfo
            );
            await NotifyEndAsync(end, onEnd, telemetryDispatcher, token).ConfigureAwait(false);
            return new RerankResult(documents, ranking, outcome.ProviderMetadata, responseInfo);
        }
        catch (Exception error)
        {
            await telemetryDispatcher.OnErrorAsync(callId, error, token).ConfigureAwait(false);

            throw;
        }
    }

    private static void Validate(
        IReadOnlyList<RerankModelRank> ranking,
        IReadOnlyList<object?> documents
    )
    {
        foreach (var rank in ranking)
        {
            if (rank.Index < 0 || rank.Index >= documents.Count)
            {
                throw new InvalidResponseDataException(
                    ranking,
                    "Invalid ranking index "
                        + rank.Index.ToString(System.Globalization.CultureInfo.InvariantCulture)
                        + ". Expected an integer between 0 and "
                        + (documents.Count - 1).ToString(
                            System.Globalization.CultureInfo.InvariantCulture
                        )
                        + "."
                );
            }
        }
    }

    private static Task NotifyStartAsync(
        RerankStartEvent payload,
        Func<RerankStartEvent, Task>? onStart,
        RerankRestrictedTelemetryDispatcher telemetryDispatcher,
        CancellationToken cancellationToken
    )
    {
        return OperationCallbacks.NotifyAsync(payload, onStart, e =>
            telemetryDispatcher.OnStartAsync(e, cancellationToken)
        );
    }

    private static Task NotifyEndAsync(
        RerankEndEvent payload,
        Func<RerankEndEvent, Task>? onEnd,
        RerankRestrictedTelemetryDispatcher telemetryDispatcher,
        CancellationToken cancellationToken
    )
    {
        return OperationCallbacks.NotifyAsync(payload, onEnd, e =>
            telemetryDispatcher.OnEndAsync(e, cancellationToken)
        );
    }
}
