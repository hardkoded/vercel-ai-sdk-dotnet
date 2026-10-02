// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

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
        if (keepAliveMs is int)
        {
            await WriteCommentAsync(writer, StreamOpenComment, cancellationToken).ConfigureAwait(false);
        }

        var messageId = JsonValues.GenerateId("msg_");
        var stepOpen = false;
        string? textId = null;
        string? reasoningId = null;

        await SendAsync(writer, new JsonObject { ["type"] = "start", ["messageId"] = messageId }, cancellationToken).ConfigureAwait(false);

        IAsyncEnumerable<TextStreamPart> parts = keepAliveMs is int interval
            ? ReadWithKeepAlive(result.Stream(cancellationToken), writer, interval, cancellationToken)
            : result.Stream(cancellationToken);

        await foreach (var part in parts.ConfigureAwait(false))
        {
            if (part is not StepFinishPart && part is not FinishPart && part is not ErrorPart)
            {
                stepOpen = await EnsureStepAsync(writer, stepOpen, cancellationToken).ConfigureAwait(false);
            }

            switch (part)
            {
                case TextDeltaPart text:
                    textId = await OpenAsync(writer, textId, "text-start", "text", cancellationToken).ConfigureAwait(false);
                    await SendAsync(writer, new JsonObject { ["type"] = "text-delta", ["id"] = textId, ["delta"] = text.Text }, cancellationToken).ConfigureAwait(false);
                    break;
                case ReasoningDeltaPart reasoning:
                    reasoningId = await OpenAsync(writer, reasoningId, "reasoning-start", "reasoning", cancellationToken).ConfigureAwait(false);
                    await SendAsync(writer, new JsonObject { ["type"] = "reasoning-delta", ["id"] = reasoningId, ["delta"] = reasoning.Text }, cancellationToken).ConfigureAwait(false);
                    break;
                case ToolCallPart call:
                    await SendAsync(
                        writer,
                        new JsonObject
                        {
                            ["type"] = "tool-input-available",
                            ["toolCallId"] = call.ToolCall.ToolCallId,
                            ["toolName"] = call.ToolCall.ToolName,
                            ["input"] = ParseJson(call.ToolCall.ArgumentsJson),
                        },
                        cancellationToken).ConfigureAwait(false);
                    break;
                case ToolResultPart toolResult:
                    if (toolResult.Result.IsError)
                    {
                        await SendAsync(
                            writer,
                            new JsonObject
                            {
                                ["type"] = "tool-output-error",
                                ["toolCallId"] = toolResult.Result.ToolCallId,
                                ["errorText"] = toolResult.Result.OutputJson,
                            },
                            cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        await SendAsync(
                            writer,
                            new JsonObject
                            {
                                ["type"] = "tool-output-available",
                                ["toolCallId"] = toolResult.Result.ToolCallId,
                                ["output"] = ParseJson(toolResult.Result.OutputJson),
                            },
                            cancellationToken).ConfigureAwait(false);
                    }

                    break;
                case SourcePart source:
                    await SendAsync(
                        writer,
                        new JsonObject
                        {
                            ["type"] = "source-url",
                            ["sourceId"] = source.Source.Id,
                            ["url"] = source.Source.Url,
                            ["title"] = source.Source.Title,
                        },
                        cancellationToken).ConfigureAwait(false);
                    break;
                case StepFinishPart:
                    textId = await CloseAsync(writer, textId, "text-end", cancellationToken).ConfigureAwait(false);
                    reasoningId = await CloseAsync(writer, reasoningId, "reasoning-end", cancellationToken).ConfigureAwait(false);
                    if (stepOpen)
                    {
                        await SendAsync(writer, new JsonObject { ["type"] = "finish-step" }, cancellationToken).ConfigureAwait(false);
                        stepOpen = false;
                    }

                    break;
                case ErrorPart error:
                    await SendAsync(writer, new JsonObject { ["type"] = "error", ["errorText"] = error.Message }, cancellationToken).ConfigureAwait(false);
                    break;
                case FinishPart finish:
                    textId = await CloseAsync(writer, textId, "text-end", cancellationToken).ConfigureAwait(false);
                    reasoningId = await CloseAsync(writer, reasoningId, "reasoning-end", cancellationToken).ConfigureAwait(false);
                    if (stepOpen)
                    {
                        await SendAsync(writer, new JsonObject { ["type"] = "finish-step" }, cancellationToken).ConfigureAwait(false);
                        stepOpen = false;
                    }

                    await SendAsync(
                        writer,
                        new JsonObject
                        {
                            ["type"] = "finish",
                            ["finishReason"] = finish.FinishReason.ToString().ToLowerInvariant(),
                        },
                        cancellationToken).ConfigureAwait(false);
                    break;
            }
        }

        await writer.WriteAsync("data: [DONE]\n\n").ConfigureAwait(false);
        await writer.FlushAsync().ConfigureAwait(false);
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

    private static async IAsyncEnumerable<TextStreamPart> ReadWithKeepAlive(
        IAsyncEnumerable<TextStreamPart> source,
        StreamWriter writer,
        int keepAliveMs,
        CancellationToken cancellationToken)
    {
        var enumerator = source.GetAsyncEnumerator(cancellationToken);
        try
        {
            while (await WaitForPartAsync(enumerator, writer, keepAliveMs, cancellationToken).ConfigureAwait(false))
            {
                yield return enumerator.Current;
            }
        }
        finally
        {
            await enumerator.DisposeAsync().ConfigureAwait(false);
        }
    }

    private static async Task<bool> WaitForPartAsync(
        IAsyncEnumerator<TextStreamPart> enumerator,
        StreamWriter writer,
        int keepAliveMs,
        CancellationToken cancellationToken)
    {
        var pending = enumerator.MoveNextAsync().AsTask();
        using (var timer = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
        {
            try
            {
                while (!pending.IsCompleted)
                {
                    var delay = Task.Delay(keepAliveMs, timer.Token);
                    if (await Task.WhenAny(pending, delay).ConfigureAwait(false) == pending)
                    {
                        break;
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                    await WriteCommentAsync(writer, KeepAliveComment, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                try
                {
                    await pending.ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // The read is stopped; the caller still sees cancellation.
                }

                throw;
            }
            catch
            {
                Observe(pending);
                throw;
            }
        }

        return await pending.ConfigureAwait(false);
    }

    private static void Observe(Task pending)
    {
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

    private static async Task WriteCommentAsync(StreamWriter writer, string comment, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await writer.WriteAsync(comment.AsMemory(), cancellationToken).ConfigureAwait(false);
        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<bool> EnsureStepAsync(StreamWriter writer, bool stepOpen, CancellationToken cancellationToken)
    {
        if (stepOpen)
        {
            return true;
        }

        await SendAsync(writer, new JsonObject { ["type"] = "start-step" }, cancellationToken).ConfigureAwait(false);
        return true;
    }

    private static async Task<string> OpenAsync(StreamWriter writer, string? current, string type, string prefix, CancellationToken cancellationToken)
    {
        if (current != null)
        {
            return current;
        }

        var id = prefix;
        await SendAsync(writer, new JsonObject { ["type"] = type, ["id"] = id }, cancellationToken).ConfigureAwait(false);
        return id;
    }

    private static async Task<string?> CloseAsync(StreamWriter writer, string? current, string type, CancellationToken cancellationToken)
    {
        if (current is null)
        {
            return null;
        }

        await SendAsync(writer, new JsonObject { ["type"] = type, ["id"] = current }, cancellationToken).ConfigureAwait(false);
        return null;
    }

    private static async Task SendAsync(StreamWriter writer, JsonObject payload, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await writer.WriteAsync("data: ").ConfigureAwait(false);
        await writer.WriteAsync(payload.ToJsonString()).ConfigureAwait(false);
        await writer.WriteAsync("\n\n").ConfigureAwait(false);
        await writer.FlushAsync().ConfigureAwait(false);
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
