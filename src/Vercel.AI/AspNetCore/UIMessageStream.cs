// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Vercel.AI.OpenTelemetry;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.AspNetCore;

/// <summary>Writes a <see cref="StreamTextResult"/> as an AI SDK UI message stream.</summary>
public static class UIMessageStreamExtensions
{
    /// <summary>
    /// Returns a <c>text/event-stream</c> result a JavaScript <c>useChat</c> client can consume.
    /// Emitted chunks are start, text, reasoning, tool input/output, source-url, step boundaries, finish, and error.
    /// Finish reasons use the protocol values <c>stop</c>, <c>length</c>, <c>content-filter</c>, <c>tool-calls</c>, <c>error</c>, and <c>other</c>.
    /// </summary>
    /// <param name="result">The stream to write.</param>
    /// <param name="keepAliveMs">
    /// Optional idle interval, in milliseconds. Null sends no SSE comments.
    /// A positive value up to 2147483647 writes <c>: stream-open</c> before the start event and <c>: keep-alive</c> while the next part is still pending.
    /// </param>
    /// <param name="onError">
    /// Optional mapper for <c>error</c> chunk text. Null writes <see cref="ErrorPart.Message"/>.
    /// </param>
    public static IResult ToUIMessageStreamResult(
        this StreamTextResult result,
        int? keepAliveMs = null,
        Func<string, string>? onError = null)
    {
        if (result is null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        return new UIMessageStreamResult(result, keepAliveMs, onError);
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

    /// <summary>Creates a result that writes <paramref name="result"/>.</summary>
    /// <param name="result">The stream to write.</param>
    /// <param name="keepAliveMs">Optional idle interval, in milliseconds. Null sends no SSE comments.</param>
    /// <param name="onError">Optional mapper for <c>error</c> chunk text. Null writes the part message.</param>
    public UIMessageStreamResult(StreamTextResult result, int? keepAliveMs = null, Func<string, string>? onError = null)
    {
        _result = result ?? throw new ArgumentNullException(nameof(result));
        ValidateKeepAlive(keepAliveMs);
        _keepAliveMs = keepAliveMs;
        _onError = onError;
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        if (httpContext is null)
        {
            throw new ArgumentNullException(nameof(httpContext));
        }

        ApplyProtocolHeaders(httpContext.Response);
        await WriteAsync(_result, httpContext.Response.Body, _keepAliveMs, httpContext.RequestAborted, _onError).ConfigureAwait(false);
    }

    /// <summary>Writes the UI message stream to <paramref name="destination"/>.</summary>
    /// <param name="result">The stream to write.</param>
    /// <param name="destination">The response body or another writable stream.</param>
    /// <param name="keepAliveMs">
    /// Optional idle interval, in milliseconds. Null sends no SSE comments.
    /// A positive value up to 2147483647 writes <c>: stream-open</c> before the start event and <c>: keep-alive</c> while the next part is still pending.
    /// </param>
    /// <param name="cancellationToken">Cancels the write and stops keep-alive comments. ASP.NET Core passes <c>HttpContext.RequestAborted</c>.</param>
    /// <param name="onError">Optional mapper for <c>error</c> chunk text. Null writes the part message.</param>
    public static async Task WriteAsync(
        StreamTextResult result,
        Stream destination,
        int? keepAliveMs = null,
        CancellationToken cancellationToken = default,
        Func<string, string>? onError = null)
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

        using var writer = new StreamWriter(destination, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), bufferSize: 1024, leaveOpen: true);
        var chunks = CreateSseStreamWithKeepAlive(ReadSseChunks(result, onError, cancellationToken), keepAliveMs, cancellationToken);
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
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var messageId = JsonValues.GenerateId("msg_");
        var stepOpen = false;
        string? textId = null;
        string? reasoningId = null;

        yield return Frame(new JsonObject { ["type"] = "start", ["messageId"] = messageId });

        await foreach (var part in result.Stream(cancellationToken).ConfigureAwait(false))
        {
            if (part is not StepFinishPart && part is not FinishPart && part is not ErrorPart && !stepOpen)
            {
                yield return Frame(new JsonObject { ["type"] = "start-step" });
                stepOpen = true;
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
                        yield return Frame(new JsonObject { ["type"] = "reasoning-start", ["id"] = reasoningId });
                    }

                    yield return Frame(new JsonObject { ["type"] = "reasoning-delta", ["id"] = reasoningId, ["delta"] = reasoning.Text });
                    break;
                case ToolCallPart call:
                    var toolInput = new JsonObject
                    {
                        ["type"] = "tool-input-available",
                        ["toolCallId"] = call.ToolCall.ToolCallId,
                        ["toolName"] = call.ToolCall.ToolName,
                        ["input"] = ParseJson(call.ToolCall.ArgumentsJson),
                    };
                    AddProviderMetadata(toolInput, call.ToolCall.ProviderMetadata);
                    yield return Frame(toolInput);
                    break;
                case ToolResultPart toolResult:
                    if (toolResult.Result.IsError)
                    {
                        yield return Frame(new JsonObject
                        {
                            ["type"] = "tool-output-error",
                            ["toolCallId"] = toolResult.Result.ToolCallId,
                            ["errorText"] = toolResult.Result.OutputJson,
                        });
                    }
                    else
                    {
                        yield return Frame(new JsonObject
                        {
                            ["type"] = "tool-output-available",
                            ["toolCallId"] = toolResult.Result.ToolCallId,
                            ["output"] = ParseJson(toolResult.Result.OutputJson),
                        });
                    }

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
                case StepFinishPart:
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

                    if (!stepOpen)
                    {
                        yield return Frame(new JsonObject { ["type"] = "start-step" });
                    }

                    yield return Frame(new JsonObject { ["type"] = "finish-step" });
                    stepOpen = false;
                    break;
                case ErrorPart error:
                    var errorText = onError != null ? onError(error.Message) : error.Message;
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

    private static async IAsyncEnumerable<string> KeepAlive(
        IAsyncEnumerable<string> source,
        int keepAliveMs,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var enumerator = source.GetAsyncEnumerator(cancellationToken);
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

            if (pending != null && !pending.IsCompleted && cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await pending.ConfigureAwait(false);
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
