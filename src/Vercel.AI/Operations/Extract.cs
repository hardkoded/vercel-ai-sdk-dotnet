// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Util;

namespace Vercel.AI.Operations;

/// <summary>Extraction result. Maps to <c>ExtractionResult</c>.</summary>
public sealed class ExtractionResult
{
    /// <summary>Creates a result.</summary>
    public ExtractionResult(
        IReadOnlyDictionary<string, ExtractionAnswer> answers,
        OperationUsage usage,
        IReadOnlyList<OperationWarning> warnings,
        JsonElement? providerMetadata,
        ProviderResponse response
    )
    {
        Answers = answers;
        Usage = usage;
        Warnings = warnings ?? Array.Empty<OperationWarning>();
        ProviderMetadata = providerMetadata;
        Response = response;
    }

    /// <summary>One answer per field, under the field ids. A value is null when the state has nothing for the field.</summary>
    public IReadOnlyDictionary<string, ExtractionAnswer> Answers { get; }

    /// <summary>Usage. <see cref="OperationUsage.TotalTokens"/> is input plus output when both are set.</summary>
    public OperationUsage Usage { get; }

    /// <summary>Warnings.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Response. Timestamp and model id are filled when the provider omitted them.</summary>
    public ProviderResponse Response { get; }
}

/// <summary>Options for <see cref="Extract.ExtractAsync"/>.</summary>
public sealed class ExtractRequest : OperationRequest
{
    /// <summary>An extraction model instance. Wins over <see cref="ModelId"/> when both are set.</summary>
    public IExtractionCaller? Model { get; set; }

    /// <summary>A model id resolved through <see cref="DecisionModels.DefaultProvider"/>, or Gateway when none is set.</summary>
    public string? ModelId { get; set; }

    /// <summary>
    /// Shared state: a string, a JSON object, or a list of parts. A JSON array is not valid state.
    /// Wrap it in an object or in a <see cref="DecisionJsonPart"/>. See <see cref="DecisionState"/>.
    /// </summary>
    public DecisionState? State { get; set; }

    /// <summary>Nonempty field map.</summary>
    public IReadOnlyDictionary<string, ExtractionField>? Fields { get; set; }

    /// <summary><c>YYYY-MM-DD</c> date that anchors relative dates and dates without a year.</summary>
    public string? ReferenceDate { get; set; }

    /// <summary>Maximum output tokens for supported models. Must be an integer of at least 1 when set.</summary>
    public double? MaxOutputTokens { get; set; }

    /// <summary>
    /// Reasoning effort for supported models: <c>provider-default</c>, <c>none</c>, <c>minimal</c>, <c>low</c>, <c>medium</c>, <c>high</c>,
    /// <c>xhigh</c>, or <c>max</c>. Language-model adapters default to <c>none</c>.
    /// </summary>
    public string? Reasoning { get; set; }

    /// <summary>
    /// Download function required for URL-backed file state. Maps to <c>experimental_download</c>. Use
    /// <see cref="Download.CreateDownload"/> with a DNS-protected transport for untrusted URLs. The model's fetch is not used.
    /// Without it a URL file throws <see cref="DownloadError"/>.
    /// </summary>
    public Func<
        IReadOnlyList<DownloadRequestItem>,
        Task<IReadOnlyList<DownloadResult>>
    >? Download { get; set; }

    /// <summary>Called when the extraction begins. Receives the full runtime context, headers, and provider options.</summary>
    public Func<ExtractStartEvent, Task>? OnStart { get; set; }

    /// <summary>Called when the extraction completes. Receives the full runtime context, headers, and provider options.</summary>
    public Func<ExtractEndEvent, Task>? OnEnd { get; set; }

    /// <summary>Telemetry callbacks. Request headers and provider options never reach them.</summary>
    public ExtractTelemetry? Telemetry { get; set; }
}

/// <summary>
/// Telemetry callbacks for <see cref="Extract.ExtractAsync"/>. Generic operation callbacks on an integration are not called.
/// </summary>
public sealed class ExtractTelemetry
{
    /// <summary><c>experimental_onExtractStart</c>.</summary>
    public Func<ExtractStartEvent, Task>? OnExtractStart { get; set; }

    /// <summary><c>experimental_onExtractionModelCallStart</c>.</summary>
    public Func<ExtractionModelCallStartEvent, Task>? OnExtractionModelCallStart { get; set; }

    /// <summary><c>experimental_onExtractionModelCallEnd</c>.</summary>
    public Func<ExtractionModelCallEndEvent, Task>? OnExtractionModelCallEnd { get; set; }

    /// <summary><c>experimental_onExtractEnd</c>.</summary>
    public Func<ExtractEndEvent, Task>? OnExtractEnd { get; set; }

    /// <summary><c>onError</c>. A download error arrives without its URL.</summary>
    public Func<string, Exception, Task>? OnError { get; set; }

    /// <summary>Keys copied into telemetry runtime context. Null yields an empty context.</summary>
    public IReadOnlyDictionary<string, bool>? IncludeRuntimeContext { get; set; }

    /// <summary>When false, telemetry events carry no state, fields, or reference date.</summary>
    public bool? RecordInputs { get; set; }

    /// <summary>When false, telemetry end events carry no answers.</summary>
    public bool? RecordOutputs { get; set; }

    /// <summary>Copied onto telemetry events.</summary>
    public string? FunctionId { get; set; }

    /// <summary>When false, telemetry callbacks are skipped.</summary>
    public bool IsEnabled { get; set; } = true;
}

/// <summary>Extracts typed field values from one shared state. Maps to <c>experimental_extract</c>.</summary>
public static class Extract
{
    /// <summary>Extracts every field in one model call.</summary>
    public static async Task<ExtractionResult> ExtractAsync(
        ExtractRequest request,
        CancellationToken cancellationToken = default
    )
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var model = DecisionModels.ResolveExtractionModel(request.Model, request.ModelId);

        LanguageModelCallPreparation.PrepareLanguageModelCallOptions(
            new LanguageModelCallOptionsInput { MaxOutputTokens = request.MaxOutputTokens }
        );
        ValidateExtraction.ValidateExtractionInput(
            request.State,
            request.Fields,
            request.ReferenceDate
        );
        var fields = request.Fields!;
        foreach (var pair in fields)
        {
            if (!model.SupportedFieldTypes.Contains(pair.Value.Type))
            {
                throw new ExtractionUnsupportedFieldTypeError(
                    pair.Key,
                    pair.Value.Type,
                    model.Provider,
                    model.ModelId
                );
            }
        }

        var token = request.CancellationToken.CanBeCanceled
            ? request.CancellationToken
            : cancellationToken;
        var callId = (request.GenerateCallId ?? (() => "call-" + Guid.NewGuid().ToString("N")))();
        var maxRetries = OperationRetry.ResolveMaxRetries(request.MaxRetries);
        var providerOptions = request.ProviderOptions ?? OperationJson.Parse("{}");
        var telemetryDispatcher = RestrictedTelemetryDispatcher.CreateRestrictedTelemetryDispatcher(
            request.Telemetry
        );
        var startEvent = new ExtractStartEvent(
            callId,
            request.RuntimeContext ?? new Dictionary<string, object?>(),
            model.Provider,
            model.ModelId,
            request.State,
            fields,
            request.ReferenceDate,
            request.MaxOutputTokens,
            request.Reasoning,
            maxRetries,
            request.Headers,
            providerOptions
        );
        await OperationCallbacks.NotifyAsync(startEvent, request.OnStart).ConfigureAwait(false);
        await telemetryDispatcher.OnStartAsync(startEvent).ConfigureAwait(false);

        try
        {
            var preparedState = await DecisionState
                .PrepareDecisionStateAsync(request.State!, token, request.Download)
                .ConfigureAwait(false);
            var modelCallEvent = new ExtractionModelCallStartEvent(
                callId,
                model.Provider,
                model.ModelId,
                preparedState,
                fields,
                request.ReferenceDate
            );
            await telemetryDispatcher
                .ExperimentalOnExtractionModelCallStartAsync(modelCallEvent)
                .ConfigureAwait(false);
            var headers = OperationHeaders.WithUserAgent(request.Headers, AiSdkVersion.UserAgent);
            var result = await OperationRetry
                .ExecuteAsync(
                    request.MaxRetries,
                    token,
                    request.AbortReason,
                    async ct =>
                    {
                        OperationRetry.ThrowIfAborted(ct, request.AbortReason);
                        return await model
                            .DoExtractAsync(
                                new ExtractionModelCall(
                                    preparedState,
                                    fields,
                                    providerOptions,
                                    headers,
                                    ct,
                                    request.ReferenceDate,
                                    request.MaxOutputTokens,
                                    request.Reasoning
                                ),
                                ct
                            )
                            .ConfigureAwait(false);
                    },
                    null
                )
                .ConfigureAwait(false);

            OperationRetry.ThrowIfAborted(token, request.AbortReason);
            var answers = NormalizeAnswers(result.Answers);
            ValidateExtraction.ValidateExtractionAnswers(fields, preparedState, answers);
            var refused = fields.Keys.Where(id => answers[id].Type == "refusal").ToArray();
            if (refused.Length > 0)
            {
                throw new ExtractionRefusalError(refused, model.Provider, model.ModelId);
            }

            await telemetryDispatcher
                .ExperimentalOnExtractionModelCallEndAsync(
                    new ExtractionModelCallEndEvent(
                        modelCallEvent,
                        answers,
                        result.Usage,
                        result.Warnings,
                        result.ProviderMetadata,
                        result.Response
                    )
                )
                .ConfigureAwait(false);

            WarningLog.Write(result.Warnings, model.Provider, model.ModelId);
            var input = result.Usage?.InputTokens;
            var output = result.Usage?.OutputTokens;
            var usage = new OperationUsage(
                input,
                output,
                input != null && output != null ? input + output : null
            );
            var now = request.Now?.Invoke() ?? DateTime.UtcNow;
            var response = new ProviderResponse(
                result.Response?.Headers,
                result.Response?.Body,
                result.Response?.Id,
                result.Response?.Timestamp ?? now,
                result.Response?.ModelId ?? model.ModelId,
                result.Response?.ProviderMetadata
            );
            var extraction = new ExtractionResult(
                answers,
                usage,
                result.Warnings,
                result.ProviderMetadata,
                response
            );
            var endEvent = new ExtractEndEvent(
                startEvent,
                extraction.Answers,
                extraction.Usage,
                extraction.Warnings,
                extraction.ProviderMetadata,
                extraction.Response
            );
            await OperationCallbacks.NotifyAsync(endEvent, request.OnEnd).ConfigureAwait(false);
            await telemetryDispatcher.OnEndAsync(endEvent).ConfigureAwait(false);
            return extraction;
        }
        catch (Exception error)
        {
            await telemetryDispatcher.OnErrorAsync(callId, error).ConfigureAwait(false);
            throw;
        }
    }

    // An empty text or an empty list means the state has no value. Refusals pass through unchanged.
    private static IReadOnlyDictionary<string, ExtractionAnswer> NormalizeAnswers(
        IReadOnlyDictionary<string, ExtractionAnswer> answers
    )
    {
        var normalized = new Dictionary<string, ExtractionAnswer>(answers.Count);
        foreach (var pair in answers)
        {
            var answer = pair.Value;
            var isEmpty =
                answer.Type != "refusal"
                && (
                    answer.Text?.Length == 0
                    || answer.TextItems?.Count == 0
                    || answer.NumberItems?.Count == 0
                );
            normalized[pair.Key] = isEmpty
                ? new ExtractionAnswer(
                    answer.Type,
                    guessText: answer.GuessText,
                    guessNumber: answer.GuessNumber,
                    pNull: answer.PNull,
                    confidence: answer.Confidence,
                    source: answer.Source,
                    items: answer.Type == "list" ? null : answer.Items
                )
                : answer;
        }

        return normalized;
    }
}
