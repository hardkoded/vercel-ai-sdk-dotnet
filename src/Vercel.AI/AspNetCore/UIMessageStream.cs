// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.AspNetCore;

/// <summary>Writes a <see cref="StreamTextResult"/> as an AI SDK UI message stream.</summary>
public static class UIMessageStreamExtensions
{
    /// <summary>
    /// Returns a <c>text/event-stream</c> result a JavaScript <c>useChat</c> client can consume.
    /// Emitted chunks are start, text, reasoning, tool input/output, source-url, step boundaries, finish, and error.
    /// </summary>
    /// <param name="result">The stream to write.</param>
    /// <param name="keepAliveMs">
    /// Optional idle interval, in milliseconds. Null sends no SSE comments.
    /// A positive value up to 2147483647 writes <c>: stream-open</c> before the start event and <c>: keep-alive</c> while the next part is still pending.
    /// </param>
    public static IResult ToUIMessageStreamResult(this StreamTextResult result, int? keepAliveMs = null)
    {
        if (result is null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        return new UIMessageStreamResult(result, keepAliveMs);
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

    /// <summary>Creates a result that writes <paramref name="result"/>.</summary>
    /// <param name="result">The stream to write.</param>
    /// <param name="keepAliveMs">Optional idle interval, in milliseconds. Null sends no SSE comments.</param>
    public UIMessageStreamResult(StreamTextResult result, int? keepAliveMs = null)
    {
        _result = result ?? throw new ArgumentNullException(nameof(result));
        ValidateKeepAlive(keepAliveMs);
        _keepAliveMs = keepAliveMs;
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        if (httpContext is null)
        {
            throw new ArgumentNullException(nameof(httpContext));
        }

        httpContext.Response.ContentType = "text/event-stream";
        httpContext.Response.Headers["Cache-Control"] = "no-cache";
        httpContext.Response.Headers["x-vercel-ai-ui-message-stream"] = "v1";
        await WriteAsync(_result, httpContext.Response.Body, _keepAliveMs, httpContext.RequestAborted).ConfigureAwait(false);
    }

    /// <summary>Writes the UI message stream to <paramref name="destination"/>.</summary>
    /// <param name="result">The stream to write.</param>
    /// <param name="destination">The response body or another writable stream.</param>
    /// <param name="keepAliveMs">
    /// Optional idle interval, in milliseconds. Null sends no SSE comments.
    /// A positive value up to 2147483647 writes <c>: stream-open</c> before the start event and <c>: keep-alive</c> while the next part is still pending.
    /// </param>
    /// <param name="cancellationToken">Cancels the write and stops keep-alive comments. ASP.NET Core passes <c>HttpContext.RequestAborted</c>.</param>
    public static async Task WriteAsync(
        StreamTextResult result,
        Stream destination,
        int? keepAliveMs = null,
        CancellationToken cancellationToken = default)
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
        var chunks = CreateSseStreamWithKeepAlive(ReadSseChunks(result, cancellationToken), keepAliveMs, cancellationToken);
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

    private static async IAsyncEnumerable<string> ReadSseChunks(
        StreamTextResult result,
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
                    yield return Frame(new JsonObject
                    {
                        ["type"] = "tool-input-available",
                        ["toolCallId"] = call.ToolCall.ToolCallId,
                        ["toolName"] = call.ToolCall.ToolName,
                        ["input"] = ParseJson(call.ToolCall.ArgumentsJson),
                    });
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
                    yield return Frame(new JsonObject
                    {
                        ["type"] = "source-url",
                        ["sourceId"] = source.Source.Id,
                        ["url"] = source.Source.Url,
                        ["title"] = source.Source.Title,
                    });
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

                    if (stepOpen)
                    {
                        yield return Frame(new JsonObject { ["type"] = "finish-step" });
                        stepOpen = false;
                    }

                    break;
                case ErrorPart error:
                    yield return Frame(new JsonObject { ["type"] = "error", ["errorText"] = error.Message });
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
                        ["finishReason"] = finish.FinishReason.ToString().ToLowerInvariant(),
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

    private static JsonNode ParseJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new JsonObject();
        }

        try
        {
            return JsonNode.Parse(json) ?? JsonValue.Create(json)!;
        }
        catch (JsonException)
        {
            return JsonValue.Create(json)!;
        }
    }
}
