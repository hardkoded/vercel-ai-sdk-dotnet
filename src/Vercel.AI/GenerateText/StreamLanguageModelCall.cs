// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.GenerateText;

/// <summary>A model call started. Maps to the <c>model-call-start</c> part of <c>LanguageModelStreamPart</c>.</summary>
internal sealed class ModelCallStartPart : TextStreamPart
{
    /// <summary>Creates a part.</summary>
    public ModelCallStartPart(IReadOnlyList<CallWarning> warnings)
        : base("model-call-start")
    {
        Warnings = warnings;
    }

    /// <summary>Warnings of the provider for the call.</summary>
    public IReadOnlyList<CallWarning> Warnings { get; }
}

/// <summary>Response metadata of a model call. Maps to the <c>model-call-response-metadata</c> part.</summary>
internal sealed class ModelCallResponseMetadataPart : TextStreamPart
{
    /// <summary>Creates a part.</summary>
    public ModelCallResponseMetadataPart(string? id, DateTimeOffset? timestamp, string? modelId)
        : base("model-call-response-metadata")
    {
        Id = id;
        Timestamp = timestamp;
        ModelId = modelId;
    }

    /// <summary>ID for the generated response, if the provider sends one.</summary>
    public string? Id { get; }

    /// <summary>Timestamp for the start of the generated response, if the provider sends one.</summary>
    public DateTimeOffset? Timestamp { get; }

    /// <summary>The ID of the response model that was used to generate the response, if the provider sends one.</summary>
    public string? ModelId { get; }
}

/// <summary>Performance of one model call. Maps to <c>performance</c> of the <c>model-call-end</c> part.</summary>
internal sealed class ModelCallPerformance
{
    /// <summary>Time the model call took, in milliseconds.</summary>
    public double ResponseTimeMs { get; init; }

    /// <summary>Effective output tokens per second over the full model response.</summary>
    public double EffectiveOutputTokensPerSecond { get; init; }

    /// <summary>Output tokens per second after the first output chunk.</summary>
    public double? OutputTokensPerSecond { get; init; }

    /// <summary>Input tokens per second before the first output chunk.</summary>
    public double? InputTokensPerSecond { get; init; }

    /// <summary>Effective input and output tokens per second over the full model response.</summary>
    public double EffectiveTotalTokensPerSecond { get; init; }

    /// <summary>Time until the first generated output chunk, in milliseconds.</summary>
    public double? TimeToFirstOutputMs { get; init; }

    /// <summary>Gaps between output chunks. Only set with at least two output chunks.</summary>
    public OutputChunkTimingStats? TimeBetweenOutputChunksMs { get; init; }
}

/// <summary>A model call finished. Maps to the <c>model-call-end</c> part.</summary>
internal sealed class ModelCallEndPart : TextStreamPart
{
    /// <summary>Creates a part.</summary>
    public ModelCallEndPart(
        FinishReason finishReason,
        string? rawFinishReason,
        LanguageModelUsage usage,
        JsonElement? providerMetadata,
        ModelCallPerformance performance
    )
        : base("model-call-end")
    {
        FinishReason = finishReason;
        RawFinishReason = rawFinishReason;
        Usage = usage;
        ProviderMetadata = providerMetadata;
        Performance = performance;
    }

    /// <summary>Finish reason. It is an error when the call violated the tool choice.</summary>
    public FinishReason FinishReason { get; }

    /// <summary>Finish reason the provider reported.</summary>
    public string? RawFinishReason { get; }

    /// <summary>Usage of the call.</summary>
    public LanguageModelUsage Usage { get; }

    /// <summary>Provider metadata of the call.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Performance of the call.</summary>
    public ModelCallPerformance Performance { get; }
}

/// <summary>
/// The parameters of the <c>executeLanguageModelCallInTelemetryContext</c> wrapper.
/// Maps to <c>Partial&lt;LanguageModelCallStartEvent&gt; &amp; { callId, execute }</c>.
/// </summary>
internal sealed record ExecuteLanguageModelCallInTelemetryContextParams(
    LanguageModelCallStartEvent Event,
    Func<Task<LanguageModelStreamResult>> Execute
)
{
    /// <summary>Id of the model call.</summary>
    public string CallId => Event.CallId;
}

/// <summary>Passed to <see cref="StreamLanguageModelCallOptions.OnStart"/> with the converted prompt. Maps to the <c>{ promptMessages }</c> argument of <c>onStart</c>.</summary>
internal sealed class StreamLanguageModelCallStartEvent
{
    /// <summary>The fully converted prompt, before the model call starts.</summary>
    public IReadOnlyList<ModelMessage> PromptMessages { get; init; } = [];
}

/// <summary>Options of <see cref="LanguageModelCallStreaming.StreamLanguageModelCall"/>.</summary>
internal sealed class StreamLanguageModelCallOptions : GenerateTextOptions
{
    /// <summary>Id of the call. A generated one is used when it is not set. Maps to <c>callId</c>.</summary>
    public string? CallId { get; set; }

    /// <summary>Whether to include raw provider chunks in the stream. Maps to <c>includeRawChunks</c>.</summary>
    public bool IncludeRawChunks { get; set; }

    /// <summary>
    /// Runs the model call in a telemetry-integration-specific context. Receives the start event of the call and an
    /// <c>Execute</c> function that performs it. Maps to <c>executeLanguageModelCallInTelemetryContext</c>.
    /// </summary>
    public Func<
        ExecuteLanguageModelCallInTelemetryContextParams,
        Task<LanguageModelStreamResult>
    >? ExecuteLanguageModelCallInTelemetryContext { get; set; }

    /// <summary>
    /// Receives the fully converted prompt before the model call starts. Prompt conversion downloads URLs, so it runs once per step
    /// and telemetry callbacks reuse its result. Maps to <c>onStart</c>.
    /// </summary>
    public new Func<
        StreamLanguageModelCallStartEvent,
        CancellationToken,
        Task
    >? OnStart { get; set; }

    /// <summary>Whether to collect the content of the call for <see cref="GenerateTextOptions.OnLanguageModelCallEnd"/>. Maps to <c>_internal.collectContent</c>.</summary>
    public bool CollectContent { get; set; } = true;

    /// <summary>The prompt of the step, with its leading instructions. A step loop sets it, after <c>prepareStep</c>.</summary>
    internal IReadOnlyList<ModelMessage>? PromptMessages { get; set; }

    /// <summary>The instructions of <see cref="PromptMessages"/>.</summary>
    internal Instructions? PromptInstructions { get; set; }

    /// <summary>Signal that aborts the provider request.</summary>
    internal AbortSignal? CallAbortSignal { get; set; }

    /// <summary>Copies the settings of a call to options for one model call.</summary>
    internal static StreamLanguageModelCallOptions From(GenerateTextOptions source)
    {
        var target = new StreamLanguageModelCallOptions();
        foreach (
            var property in typeof(GenerateTextOptions).GetProperties(
                global::System.Reflection.BindingFlags.Instance
                    | global::System.Reflection.BindingFlags.Public
                    | global::System.Reflection.BindingFlags.NonPublic
            )
        )
        {
            if (property.GetIndexParameters().Length == 0 && property.SetMethod != null)
            {
                property.SetValue(target, property.GetValue(source));
            }
        }

        return target;
    }
}

/// <summary>The result of <see cref="LanguageModelCallStreaming.StreamLanguageModelCall"/>.</summary>
internal sealed class StreamLanguageModelCallResult
{
    /// <summary>The parts of the model call.</summary>
    public IAsyncEnumerable<TextStreamPart> Stream { get; init; } = null!;

    /// <summary>Request metadata.</summary>
    public LanguageModelStreamRequest? Request { get; init; }

    /// <summary>Response metadata.</summary>
    public LanguageModelStreamResponse? Response { get; init; }

    /// <summary>Closes the provider stream. It is safe to call while the stream is being read.</summary>
    internal EnumeratorDisposal Disposal { get; init; } = null!;
}

/// <summary>Streams a single language model call. Maps to <c>stream-language-model-call.ts</c>.</summary>
internal static class LanguageModelCallStreaming
{
    /// <summary>
    /// Streams a single language model call after standardizing the prompt and tools. The stream carries the model call parts
    /// together with request and response metadata when available. Maps to <c>streamLanguageModelCall</c>.
    /// </summary>
    internal static async Task<StreamLanguageModelCallResult> StreamLanguageModelCall(
        StreamLanguageModelCallOptions options,
        CancellationToken cancellationToken = default
    )
    {
        var model = options.Model
            ?? throw new AiSdkException("A model is required to stream a language model call.");
        var effectiveCallId = options.CallId
            ?? (
                options.GenerateCallId
                ?? (() => Vercel.AI.ProviderUtils.JsonValues.GenerateId("call-"))
            )();

        var messages = options.PromptMessages?.ToList() ?? Generation.BuildPrompt(options);
        var instructions =
            options.PromptMessages != null
                ? options.PromptInstructions
                : Generation.ResolveInstructions(options);
        var instructionCount = Generation.InstructionMessages(instructions).Count;

        var callOptions = Generation.CallOptions(options, messages.ToArray());
        callOptions.AbortSignal = options.CallAbortSignal;
        callOptions.IncludeRawChunks = options.IncludeRawChunks;
        callOptions.Prompt = await LanguageModelPrompts
            .ConvertToLanguageModelPromptAsync(
                callOptions.Prompt,
                model.SupportedUrls,
                options.ExperimentalDownload,
                cancellationToken,
                model.Provider.Split('.')[0]
            )
            .ConfigureAwait(false);

        if (options.OnStart != null)
        {
            await Generation
                .NotifyAsync(
                    () =>
                        options.OnStart(
                            new StreamLanguageModelCallStartEvent
                            {
                                PromptMessages = callOptions.Prompt,
                            },
                            cancellationToken
                        ),
                    cancellationToken
                )
                .ConfigureAwait(false);
        }

        var startEvent = new LanguageModelCallStartEvent
        {
            CallId = effectiveCallId,
            Provider = model.Provider,
            ModelId = model.ModelId,
            Instructions = instructions,
            Messages = messages.Skip(instructionCount).ToArray(),
            Tools = callOptions.Tools,
            CallOptions = callOptions,
        };
        if (options.OnLanguageModelCallStart != null)
        {
            await Generation
                .NotifyAsync(
                    () => options.OnLanguageModelCallStart(startEvent, cancellationToken),
                    cancellationToken
                )
                .ConfigureAwait(false);
        }

        var now = options.NowMs ?? Generation.DefaultNowMs;
        var callStartTimestampMs = now();
        Task<LanguageModelStreamResult> Execute() =>
            model.DoStreamAsync(callOptions, cancellationToken);
        var streamResult = await (
            options.ExecuteLanguageModelCallInTelemetryContext is { } wrap
                ? wrap(new ExecuteLanguageModelCallInTelemetryContextParams(startEvent, Execute))
                : Execute()
        ).ConfigureAwait(false);

        IAsyncEnumerator<LanguageModelStreamPart> enumerator;
        try
        {
            enumerator = streamResult.Stream.GetAsyncEnumerator(cancellationToken);
        }
        catch
        {
            await streamResult.DisposeAsync().ConfigureAwait(false);
            throw;
        }

        var disposal = new EnumeratorDisposal(new StreamResultDisposal(enumerator, streamResult));
        Transform transform;
        try
        {
            transform = new Transform(
                options,
                model,
                messages,
                instructions,
                instructionCount,
                effectiveCallId,
                callOptions.ToolChoice,
                now,
                callStartTimestampMs,
                streamResult.Request
            );
        }
        catch
        {
            await disposal.DisposeAsync().ConfigureAwait(false);
            throw;
        }

        return new StreamLanguageModelCallResult
        {
            Stream = transform.RunAsync(enumerator, disposal, cancellationToken),
            Request = streamResult.Request,
            Response = streamResult.Response,
            Disposal = disposal,
        };
    }

    // Returns true for chunks that contain generated model output. Used to measure time-to-first-output for text,
    // reasoning, tool input, generated files, and tool calls.
    private static bool IsOutputChunk(LanguageModelStreamPart chunk)
    {
        return chunk switch
        {
            TextDeltaStreamPart { Delta.Length: > 0 } => true,
            ReasoningDeltaStreamPart { Delta.Length: > 0 } => true,
            ToolInputDeltaStreamPart { Delta.Length: > 0 } => true,
            FileStreamPart => true,
            ReasoningFileStreamPart => true,
            ToolCallStreamPart => true,
            _ => false,
        };
    }

    private static OutputChunkTimingStats CalculateOutputChunkTimingStats(List<double> timingsMs)
    {
        var sorted = timingsMs.OrderBy(timing => timing).ToArray();
        return new OutputChunkTimingStats(
            sorted[0],
            sorted[sorted.Length - 1],
            timingsMs.Sum() / timingsMs.Count,
            CalculateNearestRankPercentile(sorted, 0.5),
            CalculateNearestRankPercentile(sorted, 0.1),
            CalculateNearestRankPercentile(sorted, 0.9)
        );
    }

    private static double CalculateNearestRankPercentile(double[] sortedValues, double percentile)
    {
        return sortedValues[(int)Math.Ceiling(percentile * sortedValues.Length) - 1];
    }

    // Maps to createLanguageModelV4StreamPartToLanguageModelStreamPartTransform.
    private sealed class Transform(
        StreamLanguageModelCallOptions options,
        ILanguageModel model,
        IReadOnlyList<ModelMessage> messages,
        Instructions? instructions,
        int instructionCount,
        string callId,
        ToolChoice? toolChoice,
        Func<double> now,
        double callStartTimestampMs,
        LanguageModelStreamRequest? request
    )
    {
        // keep track of parsed tool calls so provider-emitted approval requests can reference them
        // keep track of tool inputs for provider-side tool results
        private readonly Dictionary<string, GeneratedToolCall> _toolCallsByToolCallId = new(
            StringComparer.Ordinal
        );
        private readonly List<object> _modelCallContent = [];
        private readonly Dictionary<string, ContentBlock> _textParts = new(StringComparer.Ordinal);
        private readonly Dictionary<string, ContentBlock> _reasoningParts = new(
            StringComparer.Ordinal
        );
        private string _responseId = (options.GenerateId ?? DefaultGenerateId)();
        private string _responseModelId = model.ModelId;
        private double? _timeToFirstOutputMs;
        private double? _previousOutputChunkTimestampMs;
        private readonly List<double> _timeBetweenOutputChunksMs = [];

        // Tool choice none rejects tool calls when the model call finishes. Withhold tool parts so that consumers
        // (e.g. client-side tools) cannot act on them before the rejection, and skip user callbacks that would
        // repair or refine them.
        private readonly bool _withholdToolParts = toolChoice?.Type == "none";

        public async IAsyncEnumerable<TextStreamPart> RunAsync(
            IAsyncEnumerator<LanguageModelStreamPart> enumerator,
            EnumeratorDisposal disposal,
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            try
            {
                while (await enumerator.MoveNextAsync().ConfigureAwait(false))
                {
                    var chunk = enumerator.Current;
                    var parts = new List<TextStreamPart>();
                    await TransformAsync(chunk, parts, cancellationToken).ConfigureAwait(false);
                    foreach (var part in parts)
                    {
                        yield return part;
                    }
                }
            }
            finally
            {
                await disposal.DisposeAsync().ConfigureAwait(false);
            }
        }

        private static string DefaultGenerateId() =>
            Vercel.AI.ProviderUtils.JsonValues.GenerateId("aitxt-");

        private async Task TransformAsync(
            LanguageModelStreamPart chunk,
            List<TextStreamPart> controller,
            CancellationToken cancellationToken
        )
        {
            if (IsOutputChunk(chunk))
            {
                var outputChunkTimestampMs = now();

                if (_timeToFirstOutputMs == null)
                {
                    _timeToFirstOutputMs = outputChunkTimestampMs - callStartTimestampMs;
                }
                else if (_previousOutputChunkTimestampMs != null)
                {
                    _timeBetweenOutputChunksMs.Add(
                        outputChunkTimestampMs - _previousOutputChunkTimestampMs.Value
                    );
                }

                _previousOutputChunkTimestampMs = outputChunkTimestampMs;
            }

            if (
                _withholdToolParts
                && chunk
                    is ToolInputStartStreamPart
                        or ToolInputDeltaStreamPart
                        or ToolInputEndStreamPart
            )
            {
                return;
            }

            switch (chunk)
            {
                case ErrorStreamPart error:
                    var normalized =
                        error.Error != null
                            ? StreamErrors.NormalizeStreamProviderError(error.Error) as Exception
                            : null;
                    controller.Add(
                        new ErrorPart((normalized ?? error.Exception)?.Message ?? error.Message)
                        {
                            Error = normalized ?? error.Exception,
                        }
                    );
                    break;

                case TextStartStreamPart textStart:
                    UpsertTextContentPart(
                        _textParts,
                        textStart.Id,
                        reasoning: false,
                        null,
                        textStart.ProviderMetadata
                    );
                    controller.Add(new TextStartPart(textStart.Id, textStart.ProviderMetadata));
                    break;

                case TextDeltaStreamPart textDelta:
                    UpsertTextContentPart(
                        _textParts,
                        textDelta.Id,
                        reasoning: false,
                        textDelta.Delta,
                        textDelta.ProviderMetadata
                    );
                    controller.Add(
                        new TextDeltaPart(textDelta.Id, textDelta.Delta, textDelta.ProviderMetadata)
                    );
                    break;

                case TextEndStreamPart textEnd:
                    var endedText = UpsertTextContentPart(
                        _textParts,
                        textEnd.Id,
                        reasoning: false,
                        null,
                        textEnd.ProviderMetadata
                    );
                    if (options.CollectContent && endedText != null)
                    {
                        endedText.Citations = textEnd.Citations;
                    }

                    _textParts.Remove(textEnd.Id);
                    controller.Add(
                        new TextEndPart(textEnd.Id, textEnd.Citations, textEnd.ProviderMetadata)
                    );
                    break;

                case ReasoningStartStreamPart reasoningStart:
                    UpsertTextContentPart(
                        _reasoningParts,
                        reasoningStart.Id,
                        reasoning: true,
                        null,
                        reasoningStart.ProviderMetadata
                    );
                    controller.Add(
                        new ReasoningStartPart(reasoningStart.Id, reasoningStart.ProviderMetadata)
                    );
                    break;

                case ReasoningDeltaStreamPart reasoningDelta:
                    UpsertTextContentPart(
                        _reasoningParts,
                        reasoningDelta.Id,
                        reasoning: true,
                        reasoningDelta.Delta,
                        reasoningDelta.ProviderMetadata
                    );
                    controller.Add(
                        new ReasoningDeltaPart(
                            reasoningDelta.Id,
                            reasoningDelta.Delta,
                            reasoningDelta.ProviderMetadata
                        )
                    );
                    break;

                case ReasoningEndStreamPart reasoningEnd:
                    UpsertTextContentPart(
                        _reasoningParts,
                        reasoningEnd.Id,
                        reasoning: true,
                        null,
                        reasoningEnd.ProviderMetadata
                    );
                    _reasoningParts.Remove(reasoningEnd.Id);
                    controller.Add(
                        new ReasoningEndPart(reasoningEnd.Id, reasoningEnd.ProviderMetadata)
                    );
                    break;

                case FileStreamPart filePart:
                    var file = filePart.Url is null
                        ? new GeneratedFile(filePart.Data!, filePart.MediaType)
                        : await Generation
                            .ResolveGeneratedFileDataAsync(
                                new GeneratedFileUrl(filePart.Url, filePart.MediaType),
                                options.ExperimentalDownload,
                                cancellationToken
                            )
                            .ConfigureAwait(false);
                    if (options.CollectContent)
                    {
                        _modelCallContent.Add(file);
                    }

                    controller.Add(new FilePart(file, filePart.ProviderMetadata));
                    break;

                case ReasoningFileStreamPart reasoningFilePart:
                    var reasoningFile = reasoningFilePart.Url is null
                        ? new GeneratedFile(reasoningFilePart.Data!, reasoningFilePart.MediaType)
                        : await Generation
                            .ResolveGeneratedFileDataAsync(
                                new GeneratedFileUrl(
                                    reasoningFilePart.Url,
                                    reasoningFilePart.MediaType
                                ),
                                options.ExperimentalDownload,
                                cancellationToken
                            )
                            .ConfigureAwait(false);
                    if (options.CollectContent)
                    {
                        _modelCallContent.Add(
                            new GeneratedReasoningFile(
                                reasoningFile.Data,
                                reasoningFile.MediaType,
                                reasoningFilePart.ProviderMetadata
                            )
                        );
                    }

                    controller.Add(
                        new ReasoningFilePart(reasoningFile, reasoningFilePart.ProviderMetadata)
                    );
                    break;

                case FinishStreamPart finish:
                    await FinishAsync(finish, controller, cancellationToken).ConfigureAwait(false);
                    break;

                case ToolCallStreamPart toolCallChunk:
                    await ToolCallAsync(toolCallChunk, controller, cancellationToken)
                        .ConfigureAwait(false);
                    break;

                case ToolApprovalRequestStreamPart approvalRequest:
                    if (_withholdToolParts)
                    {
                        break;
                    }

                    if (
                        !_toolCallsByToolCallId.TryGetValue(
                            approvalRequest.ToolCallId,
                            out var approvedCall
                        )
                    )
                    {
                        controller.Add(
                            new ErrorPart(
                                new ToolCallNotFoundForApprovalException(
                                    approvalRequest.ToolCallId,
                                    approvalRequest.ApprovalId
                                ).Message
                            )
                            {
                                Error = new ToolCallNotFoundForApprovalException(
                                    approvalRequest.ToolCallId,
                                    approvalRequest.ApprovalId
                                ),
                            }
                        );
                        break;
                    }

                    var toolApprovalRequest = new GeneratedToolApprovalRequest(
                        approvalRequest.ApprovalId,
                        approvalRequest.ToolCallId
                    )
                    {
                        ToolCall = approvedCall,
                    };
                    controller.Add(new ToolApprovalRequestPart(toolApprovalRequest));
                    if (options.CollectContent)
                    {
                        _modelCallContent.Add(toolApprovalRequest);
                    }

                    break;

                case ToolResultStreamPart toolResult:
                    var providerResult = new GeneratedToolResult(
                        toolResult.ToolCallId,
                        toolResult.ToolName,
                        toolResult.Result.GetRawText(),
                        toolResult.IsError
                    )
                    {
                        ProviderMetadata = toolResult.ProviderMetadata,
                    };
                    var providerExecuted = Generation.ProviderExecutedResult(
                        providerResult,
                        _toolCallsByToolCallId.Values.ToArray(),
                        toolResult.Preliminary
                    );
                    if (!_withholdToolParts)
                    {
                        controller.Add(
                            providerResult.IsError
                                ? new ToolErrorPart(providerExecuted)
                                : new ToolResultPart(providerExecuted)
                        );
                    }

                    if (options.CollectContent && toolResult.Preliminary != true)
                    {
                        _modelCallContent.Add(providerResult);
                    }

                    break;

                case ToolInputStartStreamPart inputStart:
                    var inputStartTool = Generation.FindTool(options, inputStart.ToolName);
                    controller.Add(
                        new ToolInputStartPart(
                            inputStart.Id,
                            inputStart.ToolName,
                            inputStart.ProviderMetadata,
                            inputStart.ProviderExecuted,
                            inputStartTool?.IsDynamic ?? inputStart.Dynamic ?? false,
                            inputStart.InputEncoding,
                            inputStartTool?.Title
                        )
                        {
                            ToolMetadata = inputStartTool?.Metadata,
                        }
                    );
                    break;

                case ToolInputDeltaStreamPart inputDelta:
                    controller.Add(
                        new ToolInputDeltaPart(
                            inputDelta.Id,
                            inputDelta.Delta,
                            inputDelta.ProviderMetadata
                        )
                    );
                    break;

                case ToolInputEndStreamPart inputEnd:
                    controller.Add(new ToolInputEndPart(inputEnd.Id, inputEnd.ProviderMetadata));
                    break;

                case StreamStartStreamPart start:
                    controller.Add(new ModelCallStartPart(start.Warnings));
                    break;

                case ResponseMetadataStreamPart responseMetadata:
                    _responseId = responseMetadata.Id ?? _responseId;
                    _responseModelId = responseMetadata.ModelId ?? _responseModelId;
                    controller.Add(
                        new ModelCallResponseMetadataPart(
                            responseMetadata.Id,
                            responseMetadata.Timestamp,
                            responseMetadata.ModelId
                        )
                    );
                    break;

                case SourceStreamPart source:
                    var generatedSource = new GeneratedSource(
                        source.Id,
                        source.Url,
                        source.Title,
                        source.ProviderMetadata
                    );
                    if (options.CollectContent)
                    {
                        _modelCallContent.Add(generatedSource);
                    }

                    controller.Add(new SourcePart(generatedSource));
                    break;

                case DocumentSourceStreamPart documentSource:
                    var generatedDocumentSource = new GeneratedDocumentSource(
                        documentSource.Id,
                        documentSource.MediaType,
                        documentSource.Title,
                        documentSource.Filename,
                        documentSource.ProviderMetadata
                    );
                    if (options.CollectContent)
                    {
                        _modelCallContent.Add(generatedDocumentSource);
                    }

                    controller.Add(new DocumentSourcePart(generatedDocumentSource));
                    break;

                case CustomStreamPart custom:
                    if (options.CollectContent)
                    {
                        _modelCallContent.Add(
                            new GeneratedCustom(custom.Kind, custom.ProviderMetadata)
                        );
                    }

                    controller.Add(new CustomPart(custom.Kind, custom.ProviderMetadata));
                    break;

                case RawStreamPart raw:
                    if (options.IncludeRawChunks)
                    {
                        using (
                            var rawDocument = JsonDocument.Parse(
                                raw.RawJson.Length == 0 ? "null" : raw.RawJson
                            )
                        )
                        {
                            controller.Add(new RawPart(rawDocument.RootElement.Clone()));
                        }
                    }

                    break;

                case PingStreamPart:
                    controller.Add(new PingPart());
                    break;
            }
        }

        private async Task FinishAsync(
            FinishStreamPart chunk,
            List<TextStreamPart> controller,
            CancellationToken cancellationToken
        )
        {
            var usage = chunk.Usage;
            var responseTimeMs = now() - callStartTimestampMs;
            var performance = new ModelCallPerformance
            {
                ResponseTimeMs = responseTimeMs,
                EffectiveOutputTokensPerSecond = TokenRates.CalculateTokensPerSecond(
                    usage.OutputTokens,
                    responseTimeMs
                ),
                OutputTokensPerSecond = _timeToFirstOutputMs is double outputStart
                    ? TokenRates.CalculateTokensPerSecond(
                        usage.OutputTokens,
                        responseTimeMs - outputStart
                    )
                    : null,
                InputTokensPerSecond = _timeToFirstOutputMs is double inputEnd
                    ? TokenRates.CalculateTokensPerSecond(usage.InputTokens, inputEnd)
                    : null,
                EffectiveTotalTokensPerSecond = TokenRates.CalculateTokensPerSecond(
                    TokenCounts.SumTokenCounts(usage.InputTokens, usage.OutputTokens),
                    responseTimeMs
                ),
                TimeToFirstOutputMs = _timeToFirstOutputMs,
                TimeBetweenOutputChunksMs =
                    _timeBetweenOutputChunksMs.Count > 0
                        ? CalculateOutputChunkTimingStats(_timeBetweenOutputChunksMs)
                        : null,
            };

            var content = BuildContent();
            var telemetryDispatcher = TelemetryDispatcher.Create(options);
            if (
                options.OnLanguageModelCallEnd != null
                || telemetryDispatcher.OnLanguageModelCallEnd != null
            )
            {
                var callEndEvent = new LanguageModelCallEndEvent(
                    model.Provider,
                    _responseModelId,
                    chunk.FinishReason,
                    usage,
                    content,
                    _responseId,
                    chunk.ProviderMetadata,
                    TimeSpan.FromMilliseconds(responseTimeMs)
                )
                {
                    CallId = callId,
                    Request = RequestOrigin.GetRequestOrigin(request?.Url) is { } origin
                        ? new LanguageModelStreamRequest { Url = origin }
                        : null,
                    Performance = performance,
                };
                if (options.OnLanguageModelCallEnd != null)
                {
                    await Generation
                        .NotifyAsync(
                            () => options.OnLanguageModelCallEnd(callEndEvent, cancellationToken),
                            cancellationToken
                        )
                        .ConfigureAwait(false);
                }

                if (telemetryDispatcher.OnLanguageModelCallEnd != null)
                {
                    await telemetryDispatcher
                        .OnLanguageModelCallEnd(callEndEvent, cancellationToken)
                        .ConfigureAwait(false);
                }
            }

            var toolChoiceViolation = Generation.ToolChoiceViolation(
                toolChoice,
                chunk.FinishReason,
                content,
                model.Provider,
                model.ModelId
            );

            // Preserve the completed model call's usage, metadata, and performance even when response validation below
            // surfaces a semantic error. Prevent invalid tool calls from being executed when the model-call-end event
            // reaches the tool executor.
            controller.Add(
                new ModelCallEndPart(
                    toolChoiceViolation == null ? chunk.FinishReason : FinishReason.Error,
                    chunk.RawFinishReason,
                    usage,
                    chunk.ProviderMetadata,
                    performance
                )
            );

            if (toolChoiceViolation != null)
            {
                controller.Add(
                    new ErrorPart(toolChoiceViolation.Message) { Error = toolChoiceViolation }
                );
            }
        }

        private async Task ToolCallAsync(
            ToolCallStreamPart chunk,
            List<TextStreamPart> controller,
            CancellationToken cancellationToken
        )
        {
            try
            {
                var toolCall = Generation.ParseProviderExecutedInput(
                    options,
                    await Generation
                        .ParseToolCallAsync(
                            new GeneratedToolCall(
                                chunk.ToolCallId,
                                chunk.ToolName,
                                chunk.ArgumentsJson,
                                chunk.ProviderMetadata
                            )
                            {
                                ProviderExecuted = chunk.ProviderExecuted,
                                Dynamic = chunk.Dynamic,
                            },
                            options,
                            messages,
                            instructions,
                            instructionCount,
                            _withholdToolParts,
                            cancellationToken
                        )
                        .ConfigureAwait(false)
                );

                _toolCallsByToolCallId[toolCall.ToolCallId] = toolCall;
                if (options.CollectContent)
                {
                    _modelCallContent.Add(toolCall);
                }

                if (_withholdToolParts)
                {
                    return;
                }

                controller.Add(new ToolCallPart(toolCall));

                if (toolCall.Invalid && !toolCall.ProviderExecuted)
                {
                    controller.Add(new ToolErrorPart(Generation.InvalidToolResult(toolCall)));
                }
            }
            catch (Exception error) when (!cancellationToken.IsCancellationRequested)
            {
                controller.Add(new ErrorPart(error.Message) { Error = error });
            }
        }

        // Appends a text or reasoning content part into the content and updates the part indexes.
        private ContentBlock? UpsertTextContentPart(
            Dictionary<string, ContentBlock> partIndexes,
            string id,
            bool reasoning,
            string? textDelta,
            JsonElement? providerMetadata
        )
        {
            if (!options.CollectContent)
            {
                return null;
            }

            if (!partIndexes.TryGetValue(id, out var part))
            {
                part = new ContentBlock(reasoning) { ProviderMetadata = providerMetadata };
                partIndexes[id] = part;
                _modelCallContent.Add(part);
            }

            if (textDelta != null)
            {
                part.Text.Append(textDelta);
            }

            if (providerMetadata != null)
            {
                part.ProviderMetadata = providerMetadata;
            }

            return part;
        }

        private List<GeneratedContent> BuildContent()
        {
            var content = new List<GeneratedContent>();
            foreach (var entry in _modelCallContent)
            {
                switch (entry)
                {
                    case ContentBlock { Reasoning: true } block:
                        content.Add(
                            new GeneratedReasoning(block.Text.ToString(), block.ProviderMetadata)
                        );
                        break;
                    case ContentBlock block:
                        content.Add(
                            new GeneratedText(
                                block.Text.ToString(),
                                block.Citations,
                                block.ProviderMetadata
                            )
                        );
                        break;
                    case GeneratedContent generated:
                        content.Add(generated);
                        break;
                }
            }

            return content;
        }
    }

    private sealed class ContentBlock(bool reasoning)
    {
        public bool Reasoning { get; } = reasoning;

        public StringBuilder Text { get; } = new();

        public IReadOnlyList<Citation>? Citations { get; set; }

        public JsonElement? ProviderMetadata { get; set; }
    }
}
