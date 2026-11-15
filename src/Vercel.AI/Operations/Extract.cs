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
    public ExtractionResult(IReadOnlyDictionary<string, ExtractionAnswer> answers, OperationUsage usage, IReadOnlyList<OperationWarning> warnings, JsonElement? providerMetadata, ProviderResponse response)
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
    /// <summary>An extraction model instance.</summary>
    public IExtractionCaller? Model { get; set; }

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
    public Func<IReadOnlyList<DownloadRequestItem>, Task<IReadOnlyList<DownloadResult>>>? Download { get; set; }
}

/// <summary>Extracts typed field values from one shared state. Maps to <c>experimental_extract</c>.</summary>
public static class Extract
{
    /// <summary>Extracts every field in one model call.</summary>
    public static async Task<ExtractionResult> ExtractAsync(ExtractRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var model = DecisionModels.ResolveExtractionModel(request.Model);

        LanguageModelCallPreparation.PrepareLanguageModelCallOptions(new LanguageModelCallOptionsInput { MaxOutputTokens = request.MaxOutputTokens });
        ValidateExtraction.ValidateExtractionInput(request.State, request.Fields, request.ReferenceDate);
        var fields = request.Fields!;
        foreach (var pair in fields)
        {
            if (!model.SupportedFieldTypes.Contains(pair.Value.Type))
            {
                throw new ExtractionUnsupportedFieldTypeError(pair.Key, pair.Value.Type, model.Provider, model.ModelId);
            }
        }

        var token = request.CancellationToken.CanBeCanceled ? request.CancellationToken : cancellationToken;
        OperationRetry.ResolveMaxRetries(request.MaxRetries);
        var providerOptions = request.ProviderOptions ?? OperationJson.Parse("{}");
        var preparedState = await DecisionState.PrepareDecisionStateAsync(request.State!, token, request.Download).ConfigureAwait(false);
        var headers = OperationHeaders.WithUserAgent(request.Headers, AiSdkVersion.UserAgent);
        var result = await OperationRetry.ExecuteAsync(request.MaxRetries, token, request.AbortReason, async ct =>
        {
            OperationRetry.ThrowIfAborted(ct, request.AbortReason);
            return await model.DoExtractAsync(new ExtractionModelCall(preparedState, fields, providerOptions, headers, ct, request.ReferenceDate, request.MaxOutputTokens, request.Reasoning), ct).ConfigureAwait(false);
        }, null).ConfigureAwait(false);

        OperationRetry.ThrowIfAborted(token, request.AbortReason);
        ValidateExtraction.ValidateExtractionAnswers(fields, preparedState, result.Answers);
        var refused = fields.Keys.Where(id => result.Answers[id].Type == "refusal").ToArray();
        if (refused.Length > 0)
        {
            throw new ExtractionRefusalError(refused, model.Provider, model.ModelId);
        }

        WarningLog.Write(result.Warnings, model.Provider, model.ModelId);
        var input = result.Usage?.InputTokens;
        var output = result.Usage?.OutputTokens;
        var usage = new OperationUsage(input, output, input != null && output != null ? input + output : null);
        var now = request.Now?.Invoke() ?? DateTime.UtcNow;
        var response = new ProviderResponse(result.Response?.Headers, result.Response?.Body, result.Response?.Id, result.Response?.Timestamp ?? now, result.Response?.ModelId ?? model.ModelId, result.Response?.ProviderMetadata);
        return new ExtractionResult(result.Answers, usage, result.Warnings, result.ProviderMetadata, response);
    }
}
