// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAI;

/// <summary>
/// Turns Responses stream events into stream parts. Retrieved web and file sources are sources. Annotations become
/// the citations on the <c>text-end</c> of the message that carries them. Citation-derived sources are held back
/// until the stream ends, because the retrieval event can arrive after the annotation.
/// </summary>
internal sealed class OpenAIResponsesStream
{
    // JSON.stringify leaves characters such as '>' and non-ASCII text unescaped.
    private static readonly JsonSerializerOptions TextInputOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private readonly Func<string> _generateId;
    private readonly string _providerOptionsName;
    private readonly List<OpenAIResponsesApi.Annotation> _annotations = new();
    private readonly List<OpenAIResponsesApi.Annotation> _fallbackAnnotations = new();
    private readonly HashSet<string> _retrievedFileIds = new(StringComparer.Ordinal);
    private string? _openTextId;
    private string? _openTextPhase;
    private bool _hasWebSearchActionSources;
    private bool _hasFileSearchResults;
    private string? _incompleteReason;
    private bool _hasFunctionCall;
    private bool _hasRefusal;
    private bool _finished;
    private bool _streamError;
    private FinishReason? _failedFinish;
    private string? _failedRaw;
    private LanguageModelUsage _usage = LanguageModelUsage.Empty;
    private string? _responseId;
    private string? _serviceTier;
    private string? _reasoningContext;
    private JsonObject? _promptCacheDiagnostics;
    private JsonArray? _inputTransformations;
    private readonly IReadOnlyCollection<string> _functionToolNames;
    private readonly Dictionary<int, OngoingFunctionCall> _ongoingFunctionCalls = new();
    private readonly Dictionary<int, OngoingCustomToolCall> _ongoingCustomToolCalls = new();
    private readonly Dictionary<int, OngoingApplyPatchCall> _ongoingApplyPatchCalls = new();
    private readonly bool _isShellProviderExecuted;

    private readonly ToolNameMapping? _toolNameMapping;
    private readonly bool _store;
    private readonly string? _webSearchToolName;
    private readonly IReadOnlyDictionary<
        string,
        string
    > _approvalRequestIdToDummyToolCallIdFromPrompt;
    private readonly Dictionary<string, string> _approvalRequestIdToDummyToolCallIdFromStream = new(
        StringComparer.Ordinal
    );
    private readonly Dictionary<string, ActiveReasoning> _activeReasoning = new(
        StringComparer.Ordinal
    );

    // Compatible providers can rotate item ids between events. The output index stays the same for the life of an item.
    private readonly Dictionary<int, string> _activeOutputItemIds = new();

    public OpenAIResponsesStream(
        Func<string>? generateId = null,
        IReadOnlyCollection<string>? functionToolNames = null,
        ToolNameMapping? toolNameMapping = null,
        bool store = false,
        string providerOptionsName = "openai",
        string? webSearchToolName = null,
        bool isShellProviderExecuted = false,
        IReadOnlyDictionary<string, string>? approvalRequestIdToDummyToolCallIdFromPrompt = null
    )
    {
        _approvalRequestIdToDummyToolCallIdFromPrompt = approvalRequestIdToDummyToolCallIdFromPrompt
            ?? new Dictionary<string, string>();
        _isShellProviderExecuted = isShellProviderExecuted;
        _store = store;
        _webSearchToolName = webSearchToolName;
        _providerOptionsName = providerOptionsName;
        _toolNameMapping = toolNameMapping;
        _functionToolNames = functionToolNames ?? Array.Empty<string>();
        _generateId = generateId
            ?? (() => Vercel.AI.ProviderUtils.JsonValues.GenerateId("source_"));
    }

    public IEnumerable<LanguageModelStreamPart> Push(OpenAIResponsesApi.Chunk chunk)
    {
        var parts = new List<LanguageModelStreamPart>();
        switch (chunk)
        {
            case OpenAIResponsesApi.ResponseCreated created:
                _responseId = created.Response.Id;
                parts.Add(
                    new ResponseMetadataStreamPart(
                        _responseId,
                        created.Response.Model,
                        OpenAIJson.UnixSeconds((long)created.Response.CreatedAt)
                    )
                );
                break;
            case OpenAIResponsesApi.OutputItemAdded added:
                OutputItemAdded(parts, added);
                break;
            case OpenAIResponsesApi.OutputTextDelta textDelta:
                var itemId = ResolveOutputItemId(
                    textDelta.ItemId,
                    OutputIndex(textDelta.OutputIndex)
                );
                OpenText(parts, itemId);
                if (!string.IsNullOrEmpty(textDelta.Delta))
                {
                    parts.Add(new TextDeltaStreamPart(itemId, textDelta.Delta));
                }

                break;
            case OpenAIResponsesApi.RefusalDelta refusalDelta:
                _hasRefusal = true;
                var refusalItemId = ResolveOutputItemId(
                    refusalDelta.ItemId,
                    OutputIndex(refusalDelta.OutputIndex)
                );
                OpenText(parts, refusalItemId);
                parts.Add(new TextDeltaStreamPart(refusalItemId, refusalDelta.Delta));
                break;
            case OpenAIResponsesApi.RefusalDone:
                _hasRefusal = true;
                break;
            case OpenAIResponsesApi.ReasoningSummaryPartAdded partAdded:
                var partItemId = ResolveOutputItemId(
                    partAdded.ItemId,
                    OutputIndex(partAdded.OutputIndex)
                );
                var summaryIndex = (int)partAdded.SummaryIndex;
                // The first reasoning start comes from output_item.added.
                if (
                    summaryIndex > 0
                    && _activeReasoning.TryGetValue(partItemId, out var activeReasoning)
                )
                {
                    activeReasoning.SummaryParts[summaryIndex] = ReasoningPartState.Active;

                    // A new summary part lets every part that could conclude conclude.
                    foreach (
                        var concludable in activeReasoning
                            .SummaryParts.Where(entry =>
                                entry.Value == ReasoningPartState.CanConclude
                            )
                            .Select(entry => entry.Key)
                            .ToList()
                    )
                    {
                        parts.Add(
                            new ReasoningEndStreamPart(
                                ReasoningPartId(partItemId, concludable),
                                ReasoningMetadata(partItemId, null)
                            )
                        );
                        activeReasoning.SummaryParts[concludable] = ReasoningPartState.Concluded;
                    }

                    parts.Add(
                        new ReasoningStartStreamPart(
                            ReasoningPartId(partItemId, summaryIndex),
                            ReasoningMetadata(
                                partItemId,
                                activeReasoning.EncryptedContent,
                                includeEncrypted: true
                            )
                        )
                    );
                }

                break;
            case OpenAIResponsesApi.ReasoningSummaryTextDelta reasoningDelta:
                var reasoningDeltaItemId = ResolveOutputItemId(
                    reasoningDelta.ItemId,
                    OutputIndex(reasoningDelta.OutputIndex)
                );
                parts.Add(
                    new ReasoningDeltaStreamPart(
                        ReasoningPartId(reasoningDeltaItemId, (int)reasoningDelta.SummaryIndex),
                        reasoningDelta.Delta,
                        ReasoningMetadata(reasoningDeltaItemId, null)
                    )
                );
                break;
            case OpenAIResponsesApi.ReasoningSummaryPartDone partDone:
                var reasoningDoneItemId = ResolveOutputItemId(
                    partDone.ItemId,
                    OutputIndex(partDone.OutputIndex)
                );
                if (_activeReasoning.TryGetValue(reasoningDoneItemId, out var doneReasoning))
                {
                    var doneSummaryIndex = (int)partDone.SummaryIndex;
                    if (_store)
                    {
                        // Stored reasoning needs no encrypted content, so the part can end now.
                        parts.Add(
                            new ReasoningEndStreamPart(
                                ReasoningPartId(reasoningDoneItemId, doneSummaryIndex),
                                ReasoningMetadata(reasoningDoneItemId, null)
                            )
                        );
                        doneReasoning.SummaryParts[doneSummaryIndex] = ReasoningPartState.Concluded;
                    }
                    else
                    {
                        // Wait for the final item, which carries the encrypted content.
                        doneReasoning.SummaryParts[doneSummaryIndex] =
                            ReasoningPartState.CanConclude;
                    }
                }

                break;
            case OpenAIResponsesApi.OutputTextAnnotationAdded annotationAdded:
                var annotation = annotationAdded.Annotation;
                _annotations.Add(annotation);
                if (annotation is OpenAIResponsesApi.UrlCitation or OpenAIResponsesApi.FileCitation)
                {
                    // Retrieval events may arrive after the annotation. Decide the fallback once the stream ends.
                    _fallbackAnnotations.Add(annotation);
                }
                else
                {
                    parts.Add(
                        ToSourcePart(
                            OpenAIResponsesOutput.AnnotationSource(
                                annotation,
                                _generateId(),
                                _providerOptionsName
                            )
                        )
                    );
                }

                break;
            case OpenAIResponsesApi.FunctionCallArgumentsDelta argumentsDelta:
                if (
                    _ongoingFunctionCalls.TryGetValue(
                        (int)argumentsDelta.OutputIndex,
                        out var ongoing
                    )
                )
                {
                    if (ongoing.SuppressInputStreaming)
                    {
                        ongoing.BufferedInputDeltas.Add(argumentsDelta.Delta);
                    }
                    else
                    {
                        parts.Add(
                            new ToolInputDeltaStreamPart(ongoing.ToolCallId, argumentsDelta.Delta)
                        );
                    }
                }

                break;
            case OpenAIResponsesApi.CustomToolCallInputDelta customDelta:
                if (
                    _ongoingCustomToolCalls.TryGetValue(
                        (int)customDelta.OutputIndex,
                        out var ongoingCustom
                    )
                )
                {
                    parts.Add(
                        new ToolInputDeltaStreamPart(ongoingCustom.ToolCallId, customDelta.Delta)
                    );
                }

                break;
            case OpenAIResponsesApi.ApplyPatchCallOperationDiffDelta patchDelta:
                if (
                    _ongoingApplyPatchCalls.TryGetValue(
                        (int)patchDelta.OutputIndex,
                        out var patchDeltaCall
                    )
                )
                {
                    parts.Add(
                        new ToolInputDeltaStreamPart(
                            patchDeltaCall.ToolCallId,
                            EscapeJsonDelta(patchDelta.Delta)
                        )
                    );
                    patchDeltaCall.HasDiff = true;
                }

                break;
            case OpenAIResponsesApi.ApplyPatchCallOperationDiffDone patchDone:
                if (
                    _ongoingApplyPatchCalls.TryGetValue(
                        (int)patchDone.OutputIndex,
                        out var patchDoneCall
                    ) && !patchDoneCall.EndEmitted
                )
                {
                    if (!patchDoneCall.HasDiff)
                    {
                        parts.Add(
                            new ToolInputDeltaStreamPart(
                                patchDoneCall.ToolCallId,
                                EscapeJsonDelta(patchDone.Diff)
                            )
                        );
                        patchDoneCall.HasDiff = true;
                    }

                    parts.Add(new ToolInputDeltaStreamPart(patchDoneCall.ToolCallId, "\"}}"));
                    parts.Add(new ToolInputEndStreamPart(patchDoneCall.ToolCallId));
                    patchDoneCall.EndEmitted = true;
                }

                break;
            case OpenAIResponsesApi.OutputItemDone done:
                OutputItemDone(parts, done);
                break;
            case OpenAIResponsesApi.ResponseCompleted completed:
                FinishResponse(completed.Response);
                break;
            case OpenAIResponsesApi.ResponseIncomplete incomplete:
                FinishResponse(incomplete.Response);
                break;
            case OpenAIResponsesApi.ResponseFailed failedChunk:
                var failed = failedChunk.Response;
                var failedReason = failed.IncompleteReason;
                _reasoningContext = failed.ReasoningContext ?? _reasoningContext;
                _promptCacheDiagnostics = OpenAIResponsesLanguageModel.PromptCacheDiagnostics(
                    failed.PromptCacheDiagnostics
                );
                _inputTransformations = OpenAIResponsesLanguageModel.InputTransformations(
                    failed.InputTransformations
                );
                _failedFinish =
                    failedReason != null
                        ? OpenAIResponsesLanguageModel.MapFinishReason(
                            failedReason,
                            _hasFunctionCall,
                            _hasRefusal
                        )
                        : FinishReason.Error;
                _failedRaw = failedReason ?? "error";
                if (failed.Error is { } failedError && !_streamError)
                {
                    _streamError = true;
                    parts.Add(new ErrorStreamPart(failedError.Message));
                }

                if (failed.Usage != null)
                {
                    _usage = OpenAIJson.ResponsesUsage(failed.Usage);
                }

                break;
            case OpenAIResponsesApi.ErrorChunk error:
                _streamError = true;
                _failedFinish = FinishReason.Error;
                _failedRaw = "error";
                parts.Add(new ErrorStreamPart(error.Message));
                break;
        }

        return parts;
    }

    private void FinishResponse(OpenAIResponsesApi.FinishedResponse response)
    {
        _finished = true;
        _incompleteReason = response.IncompleteReason;
        if (response.ServiceTier is { } serviceTier)
        {
            _serviceTier = serviceTier;
        }

        _reasoningContext = response.ReasoningContext ?? _reasoningContext;
        _promptCacheDiagnostics = OpenAIResponsesLanguageModel.PromptCacheDiagnostics(
            response.PromptCacheDiagnostics
        );
        _inputTransformations = OpenAIResponsesLanguageModel.InputTransformations(
            response.InputTransformations
        );
        if (response.Usage != null)
        {
            _usage = OpenAIJson.ResponsesUsage(response.Usage);
        }
    }

    private void OutputItemAdded(
        List<LanguageModelStreamPart> parts,
        OpenAIResponsesApi.OutputItemAdded added
    )
    {
        var index = (int)added.OutputIndex;
        switch (added.Item)
        {
            case OpenAIResponsesApi.AddedWebSearchCall webSearch:
                _activeOutputItemIds[index] = webSearch.Id;
                parts.Add(
                    new ToolInputStartStreamPart(
                        webSearch.Id,
                        WebSearchName(),
                        providerExecuted: true
                    )
                );
                parts.Add(new ToolInputEndStreamPart(webSearch.Id));
                parts.Add(
                    new ToolCallStreamPart(webSearch.Id, WebSearchName(), "{}")
                    {
                        ProviderExecuted = true,
                    }
                );
                break;
            case OpenAIResponsesApi.AddedFunctionCall functionCall:
                var suppress = OpenAIParallelToolCall.IsUndeclared(
                    functionCall.Name,
                    _functionToolNames
                );
                _ongoingFunctionCalls[index] = new OngoingFunctionCall(
                    functionCall.Name,
                    functionCall.CallId,
                    suppress,
                    functionCall.Async
                );
                if (!suppress)
                {
                    parts.Add(new ToolInputStartStreamPart(functionCall.CallId, functionCall.Name));
                }

                break;
            case OpenAIResponsesApi.CustomToolCall customCall:
                _ongoingCustomToolCalls[index] = new OngoingCustomToolCall(
                    customCall.CallId,
                    customCall.Async
                );
                parts.Add(
                    new ToolInputStartStreamPart(
                        customCall.CallId,
                        customCall.Name,
                        inputEncoding: ToolInputEncoding.Text
                    )
                );
                break;
            case OpenAIResponsesApi.ApplyPatchCall applyPatch:
                var operationType = OpenAIResponsesOutput.ApplyPatchOperationType(
                    applyPatch.Operation
                );
                var isDelete = applyPatch.Operation is OpenAIResponsesApi.DeleteFileOperation;
                var applyPatchName = ToCustomToolName("apply_patch");

                // delete_file has no diff
                _ongoingApplyPatchCalls[index] = new OngoingApplyPatchCall(applyPatch.CallId)
                {
                    HasDiff = isDelete,
                    EndEmitted = isDelete,
                };
                parts.Add(new ToolInputStartStreamPart(applyPatch.CallId, applyPatchName));
                if (isDelete)
                {
                    parts.Add(
                        new ToolInputDeltaStreamPart(
                            applyPatch.CallId,
                            OpenAIResponsesOutput.ApplyPatchInput(applyPatch)
                        )
                    );
                    parts.Add(new ToolInputEndStreamPart(applyPatch.CallId));
                }
                else
                {
                    var path = applyPatch.Operation switch
                    {
                        OpenAIResponsesApi.CreateFileOperation create => create.Path,
                        OpenAIResponsesApi.UpdateFileOperation update => update.Path,
                        _ => string.Empty,
                    };
                    parts.Add(
                        new ToolInputDeltaStreamPart(
                            applyPatch.CallId,
                            "{\"callId\":\""
                                + EscapeJsonDelta(applyPatch.CallId)
                                + "\",\"operation\":{\"type\":\""
                                + EscapeJsonDelta(operationType)
                                + "\",\"path\":\""
                                + EscapeJsonDelta(path)
                                + "\",\"diff\":\""
                        )
                    );
                }

                break;
            case OpenAIResponsesApi.ReasoningItem reasoning:
                _activeOutputItemIds[index] = reasoning.Id;
                _activeReasoning[reasoning.Id] = new ActiveReasoning(reasoning.EncryptedContent);
                parts.Add(
                    new ReasoningStartStreamPart(
                        ReasoningPartId(reasoning.Id, 0),
                        ReasoningMetadata(
                            reasoning.Id,
                            reasoning.EncryptedContent,
                            includeEncrypted: true
                        )
                    )
                );
                break;
            case OpenAIResponsesApi.AddedMessage message:
                _annotations.Clear();
                _activeOutputItemIds[index] = message.Id;
                OpenText(parts, message.Id, OpenAIResponsesOutput.PhaseName(message.Phase));
                break;
        }
    }

    private void OutputItemDone(
        List<LanguageModelStreamPart> parts,
        OpenAIResponsesApi.OutputItemDone chunk
    )
    {
        var index = (int)chunk.OutputIndex;
        switch (chunk.Item)
        {
            case OpenAIResponsesApi.DoneFunctionCall functionCall:
                _ongoingFunctionCalls.TryGetValue(index, out var finished);
                _ongoingFunctionCalls.Remove(index);
                FunctionCallDone(parts, functionCall, finished);
                break;
            case OpenAIResponsesApi.DoneCustomToolCall customCall:
                _ongoingCustomToolCalls.TryGetValue(index, out var finishedCustom);
                _ongoingCustomToolCalls.Remove(index);
                CustomToolCallDone(parts, customCall, finishedCustom);
                break;
            case OpenAIResponsesApi.ReasoningItem reasoning:
                var itemIdForDone = ResolveOutputItemId(reasoning.Id, index);
                if (_activeReasoning.TryGetValue(itemIdForDone, out var finishedReasoning))
                {
                    _activeReasoning.Remove(itemIdForDone);
                    foreach (
                        var summaryIndex in finishedReasoning
                            .SummaryParts.Where(entry =>
                                entry.Value
                                    is ReasoningPartState.Active
                                        or ReasoningPartState.CanConclude
                            )
                            .Select(entry => entry.Key)
                    )
                    {
                        parts.Add(
                            new ReasoningEndStreamPart(
                                ReasoningPartId(itemIdForDone, summaryIndex),
                                ReasoningMetadata(
                                    itemIdForDone,
                                    reasoning.EncryptedContent,
                                    includeEncrypted: true
                                )
                            )
                        );
                    }
                }

                _activeOutputItemIds.Remove(index);
                break;
            case OpenAIResponsesApi.ApplyPatchCall applyPatch:
                _ongoingApplyPatchCalls.TryGetValue(index, out var patchCall);
                _ongoingApplyPatchCalls.Remove(index);
                if (
                    patchCall is { EndEmitted: false }
                    && applyPatch.Operation is not OpenAIResponsesApi.DeleteFileOperation
                )
                {
                    if (!patchCall.HasDiff)
                    {
                        parts.Add(
                            new ToolInputDeltaStreamPart(
                                patchCall.ToolCallId,
                                EscapeJsonDelta(
                                    applyPatch.Operation switch
                                    {
                                        OpenAIResponsesApi.CreateFileOperation create =>
                                            create.Diff,
                                        OpenAIResponsesApi.UpdateFileOperation update =>
                                            update.Diff,
                                        _ => string.Empty,
                                    }
                                )
                            )
                        );
                    }

                    parts.Add(new ToolInputDeltaStreamPart(patchCall.ToolCallId, "\"}}"));
                    parts.Add(new ToolInputEndStreamPart(patchCall.ToolCallId));
                    patchCall.EndEmitted = true;
                }

                // Emit the final tool-call with the complete diff once the item is completed.
                if (patchCall != null && applyPatch.Status == "completed")
                {
                    _hasFunctionCall = true;
                    parts.Add(
                        new ToolCallStreamPart(
                            patchCall.ToolCallId,
                            ToCustomToolName("apply_patch"),
                            OpenAIResponsesOutput.ApplyPatchInput(applyPatch),
                            OpenAIResponsesOutput.ItemMetadata(applyPatch.Id, _providerOptionsName)
                        )
                    );
                }

                break;
            case OpenAIResponsesApi.DoneLocalShellCall localShell:
                parts.Add(
                    new ToolCallStreamPart(
                        localShell.Call.CallId,
                        ToCustomToolName("local_shell"),
                        OpenAIResponsesOutput.LocalShellStreamCallInput(localShell.Call),
                        OpenAIResponsesOutput.ItemMetadata(localShell.Call.Id, _providerOptionsName)
                    )
                );
                break;
            case OpenAIResponsesApi.ShellCall shellCall:
                parts.Add(
                    new ToolCallStreamPart(
                        shellCall.CallId,
                        ToCustomToolName("shell"),
                        OpenAIResponsesOutput.ShellCallInput(shellCall),
                        OpenAIResponsesOutput.ItemMetadata(shellCall.Id, _providerOptionsName)
                    )
                    {
                        ProviderExecuted = _isShellProviderExecuted,
                    }
                );
                break;
            case OpenAIResponsesApi.ShellCallOutput shellOutput:
                parts.Add(
                    new ToolResultStreamPart(
                        shellOutput.CallId,
                        ToCustomToolName("shell"),
                        JsonSerializer.Deserialize<JsonElement>(
                            OpenAIResponsesOutput.ShellCallOutputResult(shellOutput)
                        )
                    )
                );
                break;
            case OpenAIResponsesApi.IDoneItem done:
                ItemDone(parts, done, index);
                break;
        }
    }

    /// <summary>Records an unparsable chunk: the stream finishes with the error reason.</summary>
    public void MarkParseError()
    {
        _streamError = true;
        _failedFinish = FinishReason.Error;
        _failedRaw = null;
    }

    /// <summary>Ends the stream: sources that only citations supplied, then the finish part.</summary>
    public IEnumerable<LanguageModelStreamPart> Complete()
    {
        var parts = new List<LanguageModelStreamPart>();
        if (_openTextId != null)
        {
            parts.Add(
                new TextEndStreamPart(
                    _openTextId,
                    providerMetadata: OpenAIResponsesOutput.TextMetadata(
                        _openTextId,
                        [],
                        _openTextPhase,
                        _providerOptionsName
                    )
                )
            );
            _openTextId = null;
            _openTextPhase = null;
        }

        foreach (var annotation in _fallbackAnnotations)
        {
            if (
                (annotation is OpenAIResponsesApi.UrlCitation && _hasWebSearchActionSources)
                || (annotation is OpenAIResponsesApi.FileCitation && _hasFileSearchResults)
            )
            {
                continue;
            }

            parts.Add(
                ToSourcePart(
                    OpenAIResponsesOutput.AnnotationSource(
                        annotation,
                        _generateId(),
                        _providerOptionsName
                    )
                )
            );
        }

        _fallbackAnnotations.Clear();
        // A clean end without the provider's completion signal is interrupted.
        var failedFinish = _failedFinish != null && (_streamError || !_finished);
        var finish =
            failedFinish ? _failedFinish!.Value
            : _finished
                ? OpenAIResponsesLanguageModel.MapFinishReason(
                    _incompleteReason,
                    _hasFunctionCall,
                    _hasRefusal
                )
            : FinishReason.Interrupted;
        parts.Add(
            new FinishStreamPart(
                finish,
                _usage,
                failedFinish ? _failedRaw : _incompleteReason,
                OpenAIJson.ProviderMetadata(FinishMetadata(), _providerOptionsName)
            )
        );
        return parts;
    }

    private JsonObject FinishMetadata()
    {
        var metadata = new JsonObject { ["responseId"] = _responseId };
        if (_serviceTier != null)
        {
            metadata["serviceTier"] = _serviceTier;
        }

        if (_reasoningContext != null)
        {
            metadata["reasoningContext"] = _reasoningContext;
        }

        if (_promptCacheDiagnostics != null)
        {
            metadata["promptCacheDiagnostics"] = _promptCacheDiagnostics;
        }

        if (_inputTransformations != null)
        {
            metadata["inputTransformations"] = _inputTransformations;
        }

        return metadata;
    }

    private void OpenText(List<LanguageModelStreamPart> parts, string id, string? phase = null)
    {
        if (_openTextId == id)
        {
            _openTextPhase ??= phase;
            return;
        }

        if (_openTextId != null)
        {
            parts.Add(
                new TextEndStreamPart(
                    _openTextId,
                    providerMetadata: OpenAIResponsesOutput.TextMetadata(
                        _openTextId,
                        [],
                        _openTextPhase,
                        _providerOptionsName
                    )
                )
            );
        }

        _openTextId = id;
        _openTextPhase = phase;
        parts.Add(
            new TextStartStreamPart(
                id,
                OpenAIResponsesOutput.TextMetadata(id, [], phase, _providerOptionsName)
            )
        );
    }

    private static int? OutputIndex(double? value) => value is { } index ? (int)index : null;

    private string ResolveOutputItemId(string itemId, int? outputIndex) =>
        outputIndex is { } index && _activeOutputItemIds.TryGetValue(index, out var active)
            ? active
            : itemId;

    private static string ReasoningPartId(string itemId, int summaryIndex) =>
        itemId + ":" + summaryIndex.ToString(System.Globalization.CultureInfo.InvariantCulture);

    // The start and end parts carry the encrypted content, null when there is none. Deltas carry only the item id.
    private JsonElement ReasoningMetadata(
        string itemId,
        string? encryptedContent,
        bool includeEncrypted = false
    )
    {
        var values = new JsonObject { ["itemId"] = itemId };
        if (includeEncrypted)
        {
            values["reasoningEncryptedContent"] = encryptedContent;
        }

        return OpenAIJson.ProviderMetadata(values, _providerOptionsName);
    }

    private enum ReasoningPartState
    {
        Active,
        CanConclude,
        Concluded,
    }

    private sealed class ActiveReasoning(string? encryptedContent)
    {
        public string? EncryptedContent { get; } = encryptedContent;

        public Dictionary<int, ReasoningPartState> SummaryParts { get; } =
            new() { [0] = ReasoningPartState.Active };
    }

    private sealed class OngoingFunctionCall(
        string toolName,
        string toolCallId,
        bool suppressInputStreaming,
        bool? async
    )
    {
        public bool? Async { get; } = async;

        public string ToolName { get; } = toolName;

        public string ToolCallId { get; } = toolCallId;

        public bool SuppressInputStreaming { get; } = suppressInputStreaming;

        public List<string> BufferedInputDeltas { get; } = new();
    }

    private sealed class OngoingApplyPatchCall(string toolCallId)
    {
        public string ToolCallId { get; } = toolCallId;

        public bool HasDiff { get; set; }

        public bool EndEmitted { get; set; }
    }

    private sealed class OngoingCustomToolCall(string toolCallId, bool? async)
    {
        public string ToolCallId { get; } = toolCallId;

        public bool? Async { get; } = async;
    }

    // The input deltas are raw text. The final tool call input is the JSON encoding of that text.
    private void CustomToolCallDone(
        List<LanguageModelStreamPart> parts,
        OpenAIResponsesApi.DoneCustomToolCall item,
        OngoingCustomToolCall? ongoing
    )
    {
        _hasFunctionCall = true;
        var metadata = new JsonObject();
        if (item.Id is { Length: > 0 } itemId)
        {
            metadata["itemId"] = itemId;
        }

        if (item.Async is { } asyncMode)
        {
            metadata["async"] = asyncMode;
        }
        else if (ongoing?.Async is { } ongoingAsync)
        {
            metadata["async"] = ongoingAsync;
        }

        parts.Add(new ToolInputEndStreamPart(item.CallId));
        parts.Add(
            new ToolCallStreamPart(
                item.CallId,
                item.Name,
                JsonSerializer.Serialize(item.Input, TextInputOptions),
                OpenAIJson.ProviderMetadata(metadata, _providerOptionsName)
            )
        );
    }

    private void FunctionCallDone(
        List<LanguageModelStreamPart> parts,
        OpenAIResponsesApi.DoneFunctionCall item,
        OngoingFunctionCall? ongoing
    )
    {
        _hasFunctionCall = true;
        var callId = item.CallId;
        var name = item.Name;
        var input = item.Arguments;
        var suppress = ongoing?.SuppressInputStreaming
            ?? OpenAIParallelToolCall.IsUndeclared(name, _functionToolNames);

        if (
            suppress
            && OpenAIParallelToolCall.Expand(
                callId,
                name,
                input,
                _functionToolNames,
                item.Id,
                _providerOptionsName
            )
                is { } expanded
        )
        {
            foreach (var call in expanded)
            {
                parts.Add(new ToolInputStartStreamPart(call.ToolCallId, call.ToolName));
                parts.Add(new ToolInputDeltaStreamPart(call.ToolCallId, call.Input));
                parts.Add(new ToolInputEndStreamPart(call.ToolCallId));
                parts.Add(
                    new ToolCallStreamPart(
                        call.ToolCallId,
                        call.ToolName,
                        call.Input,
                        call.ProviderMetadata
                    )
                );
            }

            return;
        }

        if (suppress)
        {
            parts.Add(new ToolInputStartStreamPart(callId, name));
            if (ongoing is { BufferedInputDeltas.Count: > 0 })
            {
                foreach (var delta in ongoing.BufferedInputDeltas)
                {
                    parts.Add(new ToolInputDeltaStreamPart(callId, delta));
                }
            }
            else if (input.Length > 0)
            {
                parts.Add(new ToolInputDeltaStreamPart(callId, input));
            }
        }

        JsonElement? endMetadata = item.Namespace is { } ns
            ? OpenAIJson.ProviderMetadata(
                new JsonObject { ["namespace"] = ns },
                _providerOptionsName
            )
            : null;
        parts.Add(new ToolInputEndStreamPart(callId, endMetadata));
        parts.Add(
            new ToolCallStreamPart(
                callId,
                name,
                input,
                OpenAIParallelToolCall.CallMetadata(
                    item.Id,
                    item.Async,
                    item.Namespace,
                    item.Caller,
                    ongoing?.Async,
                    _providerOptionsName
                )
            )
        );
    }

    private string ToCustomToolName(string providerToolName) =>
        _toolNameMapping?.ToCustomToolName(providerToolName) ?? providerToolName;

    // JSON.stringify(delta).slice(1, -1): the delta escaped for the inside of a JSON string.
    private static string EscapeJsonDelta(string delta)
    {
        var json = JsonSerializer.Serialize(delta, TextInputOptions);
        return json.Substring(1, json.Length - 2);
    }

    private void ItemDone(
        List<LanguageModelStreamPart> parts,
        OpenAIResponsesApi.IDoneItem item,
        int outputIndex
    )
    {
        switch (item)
        {
            case OpenAIResponsesApi.DoneMessage message:
                var id = ResolveOutputItemId(message.Id, outputIndex);
                var phase = OpenAIResponsesOutput.PhaseName(message.Phase) ?? _openTextPhase;
                _openTextId = null;
                _openTextPhase = null;
                parts.Add(
                    new TextEndStreamPart(
                        id,
                        OpenAIResponsesOutput.Citations(_annotations),
                        OpenAIResponsesOutput.TextMetadata(
                            id,
                            _annotations,
                            phase,
                            _providerOptionsName
                        )
                    )
                );
                _annotations.Clear();
                _activeOutputItemIds.Remove(outputIndex);
                break;
            case OpenAIResponsesApi.Program program:
                parts.Add(
                    new ToolCallStreamPart(
                        program.CallId,
                        OpenAIResponsesLanguageModel.ProgrammaticToolName(_toolNameMapping),
                        OpenAIResponsesOutput.ProgramInput(program),
                        OpenAIResponsesOutput.ItemMetadata(program.Id, _providerOptionsName)
                    )
                    {
                        ProviderExecuted = true,
                    }
                );
                break;
            case OpenAIResponsesApi.ProgramOutput programOutput:
                parts.Add(
                    new ToolResultStreamPart(
                        programOutput.CallId,
                        OpenAIResponsesLanguageModel.ProgrammaticToolName(_toolNameMapping),
                        JsonSerializer.Deserialize<JsonElement>(
                            OpenAIResponsesOutput.ProgramOutputResult(programOutput)
                        )
                    )
                    {
                        ProviderMetadata = OpenAIResponsesOutput.ItemMetadata(
                            programOutput.Id,
                            _providerOptionsName
                        ),
                    }
                );
                break;
            case OpenAIResponsesApi.McpCall mcpCall:
                var mcpToolName = OpenAIResponsesOutput.McpToolName(mcpCall.Name);

                // When MCP tools require approval, they are tracked with our own tool call ids. The approval_request_id
                // maps back to that id so the results match.
                var mcpToolCallId =
                    mcpCall.ApprovalRequestId != null
                        ? _approvalRequestIdToDummyToolCallIdFromStream.TryGetValue(
                            mcpCall.ApprovalRequestId,
                            out var streamToolCallId
                        )
                            ? streamToolCallId
                            : _approvalRequestIdToDummyToolCallIdFromPrompt.TryGetValue(
                                mcpCall.ApprovalRequestId,
                                out var promptToolCallId
                            )
                                ? promptToolCallId
                                : mcpCall.Id
                        : mcpCall.Id;
                parts.Add(
                    new ToolCallStreamPart(mcpToolCallId, mcpToolName, mcpCall.Arguments)
                    {
                        ProviderExecuted = true,
                        Dynamic = true,
                    }
                );
                parts.Add(
                    new ToolResultStreamPart(
                        mcpToolCallId,
                        mcpToolName,
                        JsonSerializer.Deserialize<JsonElement>(
                            OpenAIResponsesOutput.McpCallResult(mcpCall)
                        )
                    )
                    {
                        ProviderMetadata = OpenAIResponsesOutput.ItemMetadata(
                            mcpCall.Id,
                            _providerOptionsName
                        ),
                    }
                );
                break;
            case OpenAIResponsesApi.McpListTools:
                // Skip listTools: it is not exposed and not sent back.
                break;
            case OpenAIResponsesApi.McpApprovalRequest approvalRequest:
                var dummyToolCallId = _generateId();
                var approvalRequestId = approvalRequest.ApprovalRequestId ?? approvalRequest.Id;
                _approvalRequestIdToDummyToolCallIdFromStream[approvalRequestId] = dummyToolCallId;
                parts.Add(
                    new ToolCallStreamPart(
                        dummyToolCallId,
                        OpenAIResponsesOutput.McpToolName(approvalRequest.Name),
                        approvalRequest.Arguments
                    )
                    {
                        ProviderExecuted = true,
                        Dynamic = true,
                    }
                );
                parts.Add(new ToolApprovalRequestStreamPart(approvalRequestId, dummyToolCallId));
                break;
            case OpenAIResponsesApi.DoneWebSearchCall webSearch:
                parts.Add(
                    new ToolResultStreamPart(
                        ResolveOutputItemId(webSearch.Id, outputIndex),
                        WebSearchName(),
                        JsonSerializer.Deserialize<JsonElement>(
                            OpenAIResponsesOutput.WebSearchResult(webSearch.Action)
                        )
                    )
                );
                foreach (var url in OpenAIResponsesOutput.WebSearchUrls(webSearch.Action))
                {
                    _hasWebSearchActionSources = true;
                    parts.Add(new SourceStreamPart(_generateId(), url, null));
                }

                _activeOutputItemIds.Remove(outputIndex);
                break;
            case OpenAIResponsesApi.DoneFileSearchCall fileSearch:
                parts.Add(
                    new ToolResultStreamPart(
                        fileSearch.Id,
                        "file_search",
                        JsonSerializer.Deserialize<JsonElement>(
                            OpenAIResponsesOutput.FileSearchResult(
                                fileSearch.Queries,
                                fileSearch.Results
                            )
                        )
                    )
                );
                foreach (
                    var (fileId, filename) in OpenAIResponsesOutput.RetrievedFiles(
                        fileSearch.Results
                    )
                )
                {
                    _hasFileSearchResults = true;
                    if (_retrievedFileIds.Add(fileId))
                    {
                        parts.Add(
                            new DocumentSourceStreamPart(
                                _generateId(),
                                "text/plain",
                                filename,
                                filename,
                                OpenAIResponsesOutput.FileSearchMetadata(
                                    fileId,
                                    _providerOptionsName
                                )
                            )
                        );
                    }
                }

                break;
        }
    }

    private string WebSearchName() =>
        _toolNameMapping?.ToCustomToolName(_webSearchToolName ?? "web_search")
        ?? _webSearchToolName
        ?? "web_search";

    private static LanguageModelStreamPart ToSourcePart(GeneratedContent source) =>
        source switch
        {
            GeneratedSource url => new SourceStreamPart(
                url.Id,
                url.Url,
                url.Title,
                url.ProviderMetadata
            ),
            GeneratedDocumentSource document => new DocumentSourceStreamPart(
                document.Id,
                document.MediaType,
                document.Title,
                document.Filename,
                document.ProviderMetadata
            ),
            _ => throw new InvalidOperationException(
                "A source is a URL source or a document source."
            ),
        };
}
