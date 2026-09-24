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
    /// Returns an <c>text/event-stream</c> result a JavaScript <c>useChat</c> client can consume.
    /// Emitted chunks are start, text, reasoning, tool input/output, source-url, step boundaries, finish, and error.
    /// </summary>
    public static IResult ToUIMessageStreamResult(this StreamTextResult result)
    {
        if (result is null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        return new UIMessageStreamResult(result);
    }
}

/// <summary>SSE result for the AI SDK UI message protocol.</summary>
public sealed class UIMessageStreamResult : IResult
{
    private readonly StreamTextResult _result;

    /// <summary>Creates a result that writes <paramref name="result"/>.</summary>
    public UIMessageStreamResult(StreamTextResult result)
    {
        _result = result ?? throw new ArgumentNullException(nameof(result));
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
        await WriteAsync(_result, httpContext.Response.Body, httpContext.RequestAborted).ConfigureAwait(false);
    }

    /// <summary>Writes the UI message stream to <paramref name="destination"/>.</summary>
    public static async Task WriteAsync(StreamTextResult result, Stream destination, CancellationToken cancellationToken = default)
    {
        if (result is null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        if (destination is null)
        {
            throw new ArgumentNullException(nameof(destination));
        }

        using var writer = new StreamWriter(destination, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), bufferSize: 1024, leaveOpen: true);
        var messageId = JsonValues.GenerateId("msg_");
        var stepOpen = false;
        string? textId = null;
        string? reasoningId = null;

        await SendAsync(writer, new JsonObject { ["type"] = "start", ["messageId"] = messageId }, cancellationToken).ConfigureAwait(false);

        await foreach (var part in result.Stream(cancellationToken).ConfigureAwait(false))
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
