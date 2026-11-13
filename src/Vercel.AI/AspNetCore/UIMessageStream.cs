// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Vercel.AI.OpenTelemetry;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.AspNetCore;

/// <summary>Writes a <see cref="StreamTextResult"/> as an AI SDK UI message stream.</summary>
public static class UIMessageStreamExtensions
{
    /// <summary>
    /// Returns a <c>text/event-stream</c> result a JavaScript <c>useChat</c> client can consume.
    /// Emitted chunks are start, text, reasoning, tool input/output, tool approval request, source-url, step boundaries, finish, abort, and error.
    /// Finish reasons use the protocol values <c>stop</c>, <c>length</c>, <c>content-filter</c>, <c>tool-calls</c>, <c>error</c>, and <c>other</c>.
    /// </summary>
    /// <param name="result">The stream to write.</param>
    /// <param name="keepAliveMs">
    /// Optional idle interval, in milliseconds. Null sends no SSE comments.
    /// A positive value up to 2147483647 writes <c>: stream-open</c> before the start event and <c>: keep-alive</c> while the next part is still pending.
    /// </param>
    /// <param name="onError">
    /// Optional mapper for <c>error</c> chunk text. Null writes <c>An error occurred.</c>. It also formats failed tool calls when
    /// <c>redactToolErrors</c> is enabled, which is the default when this callback is provided. Errors of provider-executed tools are not masked.
    /// </param>
    /// <param name="redactToolErrors">
    /// Replaces the error text of failed tool calls (tool execution errors and invalid tool calls) with the result of <c>onError</c>.
    /// By default, the client receives the same error text as the model. The UI messages are sent back to the model on the next request,
    /// so a different text changes the message history that the model's reasoning and answers are based on. Some models reject such a history,
    /// for example Anthropic models with preserved thinking. To keep error details away from the model and the client, throw a safe error from the tool.
    /// Defaults to <c>true</c> when <c>onError</c> is provided, so that an existing <c>onError</c> keeps handling tool errors, and to <c>false</c> otherwise.
    /// </param>
    public static IResult ToUIMessageStreamResult(
        this StreamTextResult result,
        int? keepAliveMs = null,
        Func<string, string>? onError = null,
        bool? redactToolErrors = null)
    {
        if (result is null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        return new UIMessageStreamResult(result, keepAliveMs, onError, redactToolErrors);
    }
}

/// <summary>SSE result for the AI SDK UI message protocol.</summary>
public sealed class UIMessageStreamResult : IResult
{
    private const int MaxKeepAliveMilliseconds = 2147483647;
    private const string StreamOpenComment = ": stream-open\n\n";
    private const string KeepAliveComment = ": keep-alive\n\n";

    private readonly StreamTextResult _result;
    private readonly int? _keepAliveMs;
    private readonly Func<string, string>? _onError;
    private readonly bool? _redactToolErrors;

    /// <summary>Creates a result that writes <paramref name="result"/>.</summary>
    /// <param name="result">The stream to write.</param>
    /// <param name="keepAliveMs">Optional idle interval, in milliseconds. Null sends no SSE comments.</param>
    /// <param name="onError">Optional mapper for <c>error</c> chunk text. Null writes <c>An error occurred.</c>. It also formats failed tool calls when <c>redactToolErrors</c> is enabled.</param>
    /// <param name="redactToolErrors">Replaces the error text of failed tool calls with the result of <c>onError</c>. Defaults to <c>true</c> when <c>onError</c> is provided and to <c>false</c> otherwise.</param>
    public UIMessageStreamResult(StreamTextResult result, int? keepAliveMs = null, Func<string, string>? onError = null, bool? redactToolErrors = null)
    {
        _result = result ?? throw new ArgumentNullException(nameof(result));
        ValidateKeepAlive(keepAliveMs);
        _keepAliveMs = keepAliveMs;
        _onError = onError;
        _redactToolErrors = redactToolErrors;
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        if (httpContext is null)
        {
            throw new ArgumentNullException(nameof(httpContext));
        }

        ApplyProtocolHeaders(httpContext.Response);
        await WriteAsync(_result, httpContext.Response.Body, _keepAliveMs, httpContext.RequestAborted, _onError, _redactToolErrors).ConfigureAwait(false);
    }

    /// <summary>Writes the UI message stream to <paramref name="destination"/>.</summary>
    /// <param name="result">The stream to write.</param>
    /// <param name="destination">The response body or another writable stream.</param>
    /// <param name="keepAliveMs">
    /// Optional idle interval, in milliseconds. Null sends no SSE comments.
    /// A positive value up to 2147483647 writes <c>: stream-open</c> before the start event and <c>: keep-alive</c> while the next part is still pending.
    /// </param>
    /// <param name="cancellationToken">Cancels the write and stops keep-alive comments. ASP.NET Core passes <c>HttpContext.RequestAborted</c>.</param>
    /// <param name="onError">Optional mapper for <c>error</c> chunk text. Null writes <c>An error occurred.</c>. It also formats failed tool calls when <c>redactToolErrors</c> is enabled.</param>
    /// <param name="redactToolErrors">Replaces the error text of failed tool calls with the result of <c>onError</c>. Defaults to <c>true</c> when <c>onError</c> is provided and to <c>false</c> otherwise.</param>
    public static async Task WriteAsync(
        StreamTextResult result,
        Stream destination,
        int? keepAliveMs = null,
        CancellationToken cancellationToken = default,
        Func<string, string>? onError = null,
        bool? redactToolErrors = null)
    {
        if (result is null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        if (destination is null)
        {
            throw new ArgumentNullException(nameof(destination));
        }

        ValidateKeepAlive(keepAliveMs);

        // Not disposed on purpose: every chunk is flushed asynchronously, and StreamWriter.Dispose flushes the
        // destination synchronously, which Kestrel rejects. The writer owns no other resources (leaveOpen).
        var writer = new StreamWriter(destination, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), bufferSize: 1024, leaveOpen: true);
        var chunks = CreateSseStreamWithKeepAlive(ReadSseChunks(result, onError, redactToolErrors, cancellationToken), keepAliveMs, cancellationToken);
        await foreach (var chunk in chunks.ConfigureAwait(false))
        {
            await writer.WriteAsync(chunk.AsMemory(), cancellationToken).ConfigureAwait(false);
            await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Wraps an SSE chunk stream with optional idle comments.
    /// A null interval returns <paramref name="source"/> unchanged.
    /// </summary>
    internal static IAsyncEnumerable<string> CreateSseStreamWithKeepAlive(
        IAsyncEnumerable<string> source,
        int? keepAliveMs,
        CancellationToken cancellationToken = default)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (keepAliveMs is null)
        {
            return source;
        }

        ValidateKeepAlive(keepAliveMs);
        return KeepAlive(source, keepAliveMs.Value, cancellationToken);
    }

    /// <summary>Writes protocol headers for an AI SDK UI message response.</summary>
    /// <param name="response">The HTTP response.</param>
    internal static void ApplyProtocolHeaders(HttpResponse response)
    {
        response.ContentType = "text/event-stream";
        response.Headers["Cache-Control"] = "no-cache";
        response.Headers["x-vercel-ai-ui-message-stream"] = "v1";
        response.Headers["x-accel-buffering"] = "no";
        try
        {
            response.Headers["Connection"] = "keep-alive";
        }
        catch (InvalidOperationException)
        {
            // Kestrel owns the hop-by-hop Connection header.
        }
    }

    /// <summary>Writes already-built UI message chunks as SSE, then <c>data: [DONE]</c>.</summary>
    /// <param name="chunks">Chunk objects. Each one is one <c>data:</c> frame.</param>
    /// <param name="destination">The response body or another writable stream.</param>
    /// <param name="cancellationToken">Cancels the write.</param>
    public static async Task WriteChunksAsync(
        IReadOnlyList<JsonObject> chunks,
        Stream destination,
        CancellationToken cancellationToken = default)
    {
        if (chunks is null)
        {
            throw new ArgumentNullException(nameof(chunks));
        }

        if (destination is null)
        {
            throw new ArgumentNullException(nameof(destination));
        }

        using var writer = new StreamWriter(destination, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), bufferSize: 1024, leaveOpen: true);
        foreach (var chunk in chunks)
        {
            if (chunk is null)
            {
                throw new ArgumentException("Chunks cannot contain null.", nameof(chunks));
            }

            await writer.WriteAsync(Frame(chunk).AsMemory(), cancellationToken).ConfigureAwait(false);
            await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        await writer.WriteAsync("data: [DONE]\n\n".AsMemory(), cancellationToken).ConfigureAwait(false);
        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async IAsyncEnumerable<string> ReadSseChunks(
        StreamTextResult result,
        Func<string, string>? onError,
        bool? redactToolErrorsOption,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // an existing `onError` keeps handling tool errors, as it did before tool errors stopped being redacted by default
        var redactToolErrors = redactToolErrorsOption ?? onError != null;
        var messageId = JsonValues.GenerateId("msg_");
        var stepOpen = false;
        string? textId = null;
        string? reasoningId = null;
        var providerExecutedIds = new HashSet<string>(StringComparer.Ordinal);
        var knownToolInvalidIds = new HashSet<string>(StringComparer.Ordinal);

        // streamText follows each invalid tool call with a tool-error part so the model receives a tool result. The
        // tool-input-error chunk already reports the failure, so that tool-error is skipped to avoid a second onError call.
        // TODO AI SDK 8: remove once invalid tool calls no longer inject tool-error parts.
        var invalidToolCallIds = new HashSet<string>(StringComparer.Ordinal);
        onError ??= _ => "An error occurred.";

        // When tool errors are redacted, a failed tool call has two error texts: the model receives the error text with the next step,
        // and the UI receives the onError result. Reasoning from a later step was produced after the model read its text, so UI messages
        // that contain both no longer reproduce the history that this reasoning belongs to.
        string? mismatchedToolCallIdInCurrentStep = null;
        string? mismatchedToolCallIdInEarlierStep = null;
        var hasWarnedAboutToolErrorMismatch = false;

        yield return Frame(new JsonObject { ["type"] = "start", ["messageId"] = messageId });

        await foreach (var part in result.Stream(cancellationToken).ConfigureAwait(false))
        {
            if (part is not StartPart && part is not StepFinishPart && part is not FinishPart && part is not ErrorPart && part is not AbortPart && !stepOpen)
            {
                yield return Frame(new JsonObject { ["type"] = "start-step" });
                stepOpen = true;

                // the model receives the tool errors of a step with the next step
                mismatchedToolCallIdInEarlierStep ??= mismatchedToolCallIdInCurrentStep;
                mismatchedToolCallIdInCurrentStep = null;
            }

            switch (part)
            {
                case TextDeltaPart text:
                    if (textId == null)
                    {
                        textId = "text";
                        yield return Frame(new JsonObject { ["type"] = "text-start", ["id"] = textId });
                    }

                    yield return Frame(new JsonObject { ["type"] = "text-delta", ["id"] = textId, ["delta"] = text.Text });
                    break;
                case ReasoningDeltaPart reasoning:
                    if (reasoningId == null)
                    {
                        reasoningId = "reasoning";
                        if (mismatchedToolCallIdInEarlierStep != null && !hasWarnedAboutToolErrorMismatch)
                        {
                            hasWarnedAboutToolErrorMismatch = true;
                            LogToolErrorMismatchWarning(mismatchedToolCallIdInEarlierStep);
                        }

                        yield return Frame(new JsonObject { ["type"] = "reasoning-start", ["id"] = reasoningId });
                    }

                    yield return Frame(new JsonObject { ["type"] = "reasoning-delta", ["id"] = reasoningId, ["delta"] = reasoning.Text });
                    break;
                case ToolInputStartPart inputStart:
                    var inputStartChunk = new JsonObject
                    {
                        ["type"] = "tool-input-start",
                        ["toolCallId"] = inputStart.Id,
                        ["toolName"] = inputStart.ToolName,
                    };
                    if (inputStart.InputEncoding is ToolInputEncoding inputEncoding)
                    {
                        inputStartChunk["inputEncoding"] = inputEncoding == ToolInputEncoding.Text ? "text" : "json";
                    }

                    if (inputStart.ProviderExecuted is bool providerExecuted)
                    {
                        if (providerExecuted)
                        {
                            providerExecutedIds.Add(inputStart.Id);
                        }

                        inputStartChunk["providerExecuted"] = providerExecuted;
                    }

                    AddProviderMetadata(inputStartChunk, inputStart.ProviderMetadata);
                    if (inputStart.ToolMetadata is { } inputStartMetadata)
                    {
                        inputStartChunk["toolMetadata"] = JsonSerializer.SerializeToNode(inputStartMetadata);
                    }

                    if (inputStart.Dynamic)
                    {
                        inputStartChunk["dynamic"] = true;
                    }

                    if (inputStart.Title != null)
                    {
                        inputStartChunk["title"] = inputStart.Title;
                    }

                    yield return Frame(inputStartChunk);
                    break;
                case ToolInputDeltaPart inputDelta:
                    yield return Frame(new JsonObject
                    {
                        ["type"] = "tool-input-delta",
                        ["toolCallId"] = inputDelta.Id,
                        ["inputTextDelta"] = inputDelta.Delta,
                    });
                    break;
                case ToolCallPart call:
                    if (call.ToolCall.Invalid)
                    {
                        invalidToolCallIds.Add(call.ToolCall.ToolCallId);
                    }

                    var toolInput = new JsonObject
                    {
                        ["type"] = call.ToolCall.Invalid ? "tool-input-error" : "tool-input-available",
                        ["toolCallId"] = call.ToolCall.ToolCallId,
                        ["toolName"] = call.ToolCall.ToolName,
                        ["input"] = ParseJson(call.ToolCall.ArgumentsJson),
                    };
                    if (call.ToolCall.ProviderExecuted)
                    {
                        providerExecutedIds.Add(call.ToolCall.ToolCallId);
                        toolInput["providerExecuted"] = true;
                    }

                    AddProviderMetadata(toolInput, call.ToolCall.ProviderMetadata);
                    if (call.ToolCall.ToolMetadata is { } toolMetadata)
                    {
                        toolInput["toolMetadata"] = JsonSerializer.SerializeToNode(toolMetadata);
                    }

                    // Every invalid call is dynamic, but upstream marks it only when its tool is unknown. This writer has no tool list,
                    // so a call whose tool exists but whose input is invalid is not marked.
                    if (call.ToolCall.Dynamic && (!call.ToolCall.Invalid || call.ToolCall.Error is NoSuchToolError))
                    {
                        toolInput["dynamic"] = true;
                    }

                    if (call.ToolCall.Invalid && call.ToolCall.Error is not NoSuchToolError)
                    {
                        knownToolInvalidIds.Add(call.ToolCall.ToolCallId);
                    }

                    if (call.ToolCall.Invalid)
                    {
                        // streamText sends the model a tool result for an invalid tool call only when the tool is not provider-executed
                        var invalidModelText = ErrorMessage.ToErrorString(call.ToolCall.Error!);
                        var invalidErrorText = call.ToolCall.ProviderExecuted ? onError(invalidModelText) : ToolErrorText(invalidModelText, invalidModelText, redactToolErrors, onError);
                        toolInput["errorText"] = invalidErrorText;
                        if (!call.ToolCall.ProviderExecuted && invalidErrorText != invalidModelText)
                        {
                            mismatchedToolCallIdInCurrentStep ??= call.ToolCall.ToolCallId;
                        }
                    }

                    if (call.ToolCall.Title != null)
                    {
                        toolInput["title"] = call.ToolCall.Title;
                    }

                    yield return Frame(toolInput);
                    break;
                case ToolApprovalRequestPart approvalRequest:
                    yield return Frame(new JsonObject
                    {
                        ["type"] = "tool-approval-request",
                        ["approvalId"] = approvalRequest.ApprovalId,
                        ["toolCallId"] = approvalRequest.ToolCall.ToolCallId,
                    });
                    break;
                case ToolErrorPart toolError when !providerExecutedIds.Contains(toolError.Result.ToolCallId)
                    && invalidToolCallIds.Remove(toolError.Result.ToolCallId):
                    break;
                case ToolErrorPart toolError:
                    var providerError = providerExecutedIds.Contains(toolError.Result.ToolCallId);
                    var modelErrorText = toolError.Result.ModelOutput ?? toolError.Result.Error?.Message ?? toolError.Result.OutputJson;
                    var toolErrorText = providerError
                        ? null
                        : ToolErrorText(modelErrorText, toolError.Result.Error?.Message ?? modelErrorText, redactToolErrors, onError);
                    if (!providerError && toolErrorText != modelErrorText)
                    {
                        mismatchedToolCallIdInCurrentStep ??= toolError.Result.ToolCallId;
                    }

                    var errorOutput = new JsonObject
                    {
                        ["type"] = "tool-output-error",
                        ["toolCallId"] = toolError.Result.ToolCallId,
                        ["errorText"] = providerError
                            ? ProviderErrorText(toolError.Result.OutputJson)
                            : toolErrorText,
                    };
                    if (providerError)
                    {
                        errorOutput["providerExecuted"] = true;
                        AddProviderMetadata(errorOutput, toolError.Result.ProviderMetadata);
                    }

                    AddToolMetadata(errorOutput, toolError.Result.ToolMetadata);
                    if (toolError.Result.Dynamic && !knownToolInvalidIds.Contains(toolError.Result.ToolCallId))
                    {
                        errorOutput["dynamic"] = true;
                    }

                    yield return Frame(errorOutput);
                    break;
                case ToolResultPart toolResult:
                    var toolOutput = new JsonObject
                    {
                        ["type"] = "tool-output-available",
                        ["toolCallId"] = toolResult.Result.ToolCallId,
                        ["output"] = ParseJson(toolResult.Result.OutputJson),
                    };
                    if (providerExecutedIds.Contains(toolResult.Result.ToolCallId))
                    {
                        toolOutput["providerExecuted"] = true;
                        AddProviderMetadata(toolOutput, toolResult.Result.ProviderMetadata);
                    }

                    AddToolMetadata(toolOutput, toolResult.Result.ToolMetadata);
                    if (toolResult.Result.Dynamic)
                    {
                        toolOutput["dynamic"] = true;
                    }

                    yield return Frame(toolOutput);
                    break;
                case SourcePart source:
                    var sourceChunk = new JsonObject
                    {
                        ["type"] = "source-url",
                        ["sourceId"] = source.Source.Id,
                        ["url"] = source.Source.Url,
                    };
                    if (source.Source.Title != null)
                    {
                        sourceChunk["title"] = source.Source.Title;
                    }

                    AddProviderMetadata(sourceChunk, source.Source.ProviderMetadata);
                    yield return Frame(sourceChunk);
                    break;
                case StepFinishPart stepFinish:
                    if (textId != null)
                    {
                        var textEnd = new JsonObject { ["type"] = "text-end", ["id"] = textId };
                        var citations = new List<Citation>();
                        foreach (var content in stepFinish.Step.Content)
                        {
                            if (content is GeneratedText { Citations: { } textCitations })
                            {
                                citations.AddRange(textCitations);
                            }
                        }

                        if (citations.Count > 0)
                        {
                            textEnd["citations"] = CitationJson.ToJson(citations);
                        }

                        yield return Frame(textEnd);
                        textId = null;
                    }

                    if (reasoningId != null)
                    {
                        yield return Frame(new JsonObject { ["type"] = "reasoning-end", ["id"] = reasoningId });
                        reasoningId = null;
                    }

                    if (!stepOpen)
                    {
                        yield return Frame(new JsonObject { ["type"] = "start-step" });
                    }

                    yield return Frame(new JsonObject { ["type"] = "finish-step" });
                    stepOpen = false;
                    break;
                case AbortPart abortPart:
                    yield return Frame(new JsonObject { ["type"] = "abort", ["reason"] = abortPart.Reason });
                    break;
                case ErrorPart error:
                    var errorText = onError(error.Message);
                    yield return Frame(new JsonObject { ["type"] = "error", ["errorText"] = errorText });
                    break;
                case FinishPart finish:
                    if (textId != null)
                    {
                        yield return Frame(new JsonObject { ["type"] = "text-end", ["id"] = textId });
                        textId = null;
                    }

                    if (reasoningId != null)
                    {
                        yield return Frame(new JsonObject { ["type"] = "reasoning-end", ["id"] = reasoningId });
                        reasoningId = null;
                    }

                    if (stepOpen)
                    {
                        yield return Frame(new JsonObject { ["type"] = "finish-step" });
                        stepOpen = false;
                    }

                    yield return Frame(new JsonObject
                    {
                        ["type"] = "finish",
                        ["finishReason"] = GenAiConventions.FormatFinishReason(finish.FinishReason),
                    });
                    break;
            }
        }

        yield return "data: [DONE]\n\n";
    }

    private static readonly TimeSpan SourceStopGrace = TimeSpan.FromSeconds(1);

    private static async IAsyncEnumerable<string> KeepAlive(
        IAsyncEnumerable<string> source,
        int keepAliveMs,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var readCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var enumerator = source.GetAsyncEnumerator(readCancellation.Token);
        Task<bool>? pending = null;
        CancellationTokenSource? timer = null;
        try
        {
            yield return StreamOpenComment;
            pending = enumerator.MoveNextAsync().AsTask();
            timer = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            while (true)
            {
                while (!pending.IsCompleted)
                {
                    var delay = Task.Delay(keepAliveMs, timer!.Token);
                    if (await Task.WhenAny(pending, delay).ConfigureAwait(false) == pending)
                    {
                        break;
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                    yield return KeepAliveComment;
                }

                var finished = timer;
                timer = null;
                finished!.Cancel();
                finished.Dispose();

                if (!await pending.ConfigureAwait(false))
                {
                    yield break;
                }

                var chunk = enumerator.Current;
                yield return chunk;
                pending = enumerator.MoveNextAsync().AsTask();
                timer = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            }
        }
        finally
        {
            if (timer != null)
            {
                timer.Cancel();
                timer.Dispose();
            }

            if (pending != null && !pending.IsCompleted)
            {
                // The consumer stopped while a read is in flight. Stop the read and wait for it before the enumerator is disposed.
                // A source that ignores cancellation gets a short grace period, so a stalled source cannot hold up the response.
                readCancellation.Cancel();
                try
                {
                    await Task.WhenAny(pending, Task.Delay(SourceStopGrace)).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // The in-flight read is observed before the source enumerator is disposed.
                }
            }

            await enumerator.DisposeAsync().ConfigureAwait(false);
            Observe(pending);
        }
    }

    private static void ValidateKeepAlive(int? keepAliveMs)
    {
        if (keepAliveMs is null)
        {
            return;
        }

        var milliseconds = keepAliveMs.Value;
        if (milliseconds <= 0 || (long)milliseconds > MaxKeepAliveMilliseconds)
        {
            throw new ArgumentOutOfRangeException(
                nameof(keepAliveMs),
                milliseconds,
                "keepAliveMs must be a positive duration no greater than 2147483647 ms.");
        }
    }

    private static void Observe(Task? pending)
    {
        if (pending is null)
        {
            return;
        }

        if (pending.IsCompleted)
        {
            _ = pending.Exception;
            return;
        }

        _ = pending.ContinueWith(
            static completed => _ = completed.Exception,
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    private static string Frame(JsonObject payload)
    {
        return "data: " + payload.ToJsonString() + "\n\n";
    }

    private static void AddToolMetadata(JsonObject payload, IReadOnlyDictionary<string, JsonElement>? metadata)
    {
        if (metadata != null)
        {
            payload["toolMetadata"] = JsonSerializer.SerializeToNode(metadata);
        }
    }

    private static void AddProviderMetadata(JsonObject payload, JsonElement? metadata)
    {
        if (metadata is not JsonElement element || element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return;
        }

        var node = JsonNode.Parse(element.GetRawText());
        if (node != null)
        {
            payload["providerMetadata"] = node;
        }
    }

    // The error text of a failed tool call is not altered by default. The UI message is the record that ConvertToModelMessages sends back
    // to the model on the next request, so a replaced text changes the history after the model has received the original.
    private static string ToolErrorText(string modelErrorText, string onErrorInput, bool redactToolErrors, Func<string, string> onError)
        => redactToolErrors ? onError(onErrorInput) : modelErrorText;

    private static void LogToolErrorMismatchWarning(string toolCallId)
        => LogWarnings.Log(new LogWarningsOptions(new ModelWarning[]
        {
            new OtherWarning(
                $"The error text sent to the UI for tool call \"{toolCallId}\" differs from the error text the model received, " +
                "and reasoning from a later step was sent to the UI. " +
                "Converting these UI messages back to model messages changes the tool result. " +
                "Models that bind reasoning to the preceding messages (for example Anthropic models with preserved thinking) reject that history. " +
                "See https://ai-sdk.dev/docs/ai-sdk-ui/chatbot-tool-usage#tool-error-text-and-message-history"),
        }));

    // A provider-executed error keeps its structure: a string stays text and any other value is its JSON.
    private static string ProviderErrorText(string outputJson)
    {
        try
        {
            using var document = JsonDocument.Parse(outputJson);
            return document.RootElement.ValueKind == JsonValueKind.String ? document.RootElement.GetString()! : outputJson;
        }
        catch (JsonException)
        {
            return outputJson;
        }
    }

    private static JsonNode ParseJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new JsonObject();
        }

        try
        {
            var node = JsonNode.Parse(json);
            if (node != null)
            {
                return node;
            }

            using var document = JsonDocument.Parse(json);
            return JsonSerializer.SerializeToNode(document.RootElement)!;
        }
        catch (JsonException)
        {
            return JsonValue.Create(json)!;
        }
    }
}
