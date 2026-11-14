// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenTelemetry;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.AspNetCore;

/// <summary>Options of <see cref="UIMessageStreamConverter.ToUIMessageChunk"/> and <see cref="UIMessageStreamConverter.ToUIMessageStream"/>. Maps to <c>ToUIMessageChunkOptions</c>.</summary>
public sealed class ToUIMessageChunkOptions
{
    /// <summary>Whether reasoning parts are sent. Defaults to <c>true</c>. Maps to <c>sendReasoning</c>.</summary>
    public bool SendReasoning { get; init; } = true;

    /// <summary>Whether the <c>start</c> chunk is sent. Defaults to <c>true</c>. Maps to <c>sendStart</c>.</summary>
    public bool SendStart { get; init; } = true;

    /// <summary>Whether the <c>finish</c> chunk is sent. Defaults to <c>true</c>. Maps to <c>sendFinish</c>.</summary>
    public bool SendFinish { get; init; } = true;

    /// <summary>
    /// Formats stream errors. Also formats invalid tool calls and application-executed tool errors when <see cref="RedactToolErrors"/> is enabled.
    /// Provider-executed tool execution errors bypass this callback so their original error data survives model-message round trips.
    /// Null writes <c>An error occurred.</c>. The argument is the formatted error message.
    /// </summary>
    public Func<string, string>? OnError { get; init; }

    /// <summary>
    /// Replaces the error text of failed tool calls with the result of <see cref="OnError"/>.
    /// Defaults to <c>true</c> when <see cref="OnError"/> is provided and to <c>false</c> otherwise.
    /// </summary>
    public bool? RedactToolErrors { get; init; }

    /// <summary>Id of the response message sent with the <c>start</c> chunk. Maps to <c>responseMessageId</c>.</summary>
    public string? ResponseMessageId { get; init; }
}

/// <summary>Converts <c>streamText</c> parts into AI SDK UI message chunks.</summary>
public static partial class UIMessageStreamConverter
{
    /// <summary>
    /// Converts one <see cref="TextStreamPart"/> into a UI message chunk.
    /// Returns <see langword="null"/> for stream parts that do not produce UI message chunks.
    /// The .NET stream has no text-start, text-end, reasoning-start, or reasoning-end parts, so <see cref="ToUIMessageStream"/> adds those chunks.
    /// </summary>
    /// <param name="part">The part to convert.</param>
    /// <param name="options">Conversion options.</param>
    public static JsonObject? ToUIMessageChunk(TextStreamPart part, ToUIMessageChunkOptions? options = null)
    {
        if (part is null)
        {
            throw new ArgumentNullException(nameof(part));
        }

        options ??= new ToUIMessageChunkOptions();

        // Stream errors never reach the model, so they are masked by default.
        var onError = options.OnError ?? (_ => "An error occurred.");

        // an existing `onError` keeps handling tool errors, as it did before tool errors stopped being redacted by default
        var redactToolErrors = options.RedactToolErrors ?? options.OnError != null;

        switch (part)
        {
            case TextDeltaPart text:
                return new JsonObject { ["type"] = "text-delta", ["id"] = TextId, ["delta"] = text.Text };
            case ReasoningDeltaPart reasoning:
                return options.SendReasoning
                    ? new JsonObject { ["type"] = "reasoning-delta", ["id"] = ReasoningId, ["delta"] = reasoning.Text }
                    : null;
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
                return sourceChunk;
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
                    inputStartChunk["providerExecuted"] = providerExecuted;
                }

                AddProviderMetadata(inputStartChunk, inputStart.ProviderMetadata);
                AddToolMetadata(inputStartChunk, inputStart.ToolMetadata);
                if (inputStart.Dynamic)
                {
                    inputStartChunk["dynamic"] = true;
                }

                if (inputStart.Title != null)
                {
                    inputStartChunk["title"] = inputStart.Title;
                }

                return inputStartChunk;
            case ToolInputDeltaPart inputDelta:
                return new JsonObject
                {
                    ["type"] = "tool-input-delta",
                    ["toolCallId"] = inputDelta.Id,
                    ["inputTextDelta"] = inputDelta.Delta,
                };
            case ToolCallPart call:
                var toolInput = new JsonObject
                {
                    ["type"] = call.ToolCall.Invalid ? "tool-input-error" : "tool-input-available",
                    ["toolCallId"] = call.ToolCall.ToolCallId,
                    ["toolName"] = call.ToolCall.ToolName,
                    ["input"] = ParseJson(call.ToolCall.ArgumentsJson),
                };
                if (call.ToolCall.ProviderExecuted)
                {
                    toolInput["providerExecuted"] = true;
                }

                AddProviderMetadata(toolInput, call.ToolCall.ProviderMetadata);
                AddToolMetadata(toolInput, call.ToolCall.ToolMetadata);

                // Every invalid call is dynamic, but upstream marks it only when its tool is unknown. This converter has no tool list,
                // so a call whose tool exists but whose input is invalid is not marked.
                if (call.ToolCall.Dynamic && (!call.ToolCall.Invalid || call.ToolCall.Error is NoSuchToolError))
                {
                    toolInput["dynamic"] = true;
                }

                if (call.ToolCall.Invalid)
                {
                    // streamText sends the model a tool result for an invalid tool call only when the tool is not provider-executed
                    var invalidModelText = ErrorMessage.ToErrorString(call.ToolCall.Error!);
                    toolInput["errorText"] = call.ToolCall.ProviderExecuted
                        ? onError(invalidModelText)
                        : ToolErrorText(invalidModelText, invalidModelText, redactToolErrors, onError);
                }

                if (call.ToolCall.Title != null)
                {
                    toolInput["title"] = call.ToolCall.Title;
                }

                return toolInput;
            case ToolApprovalRequestPart approvalRequest:
                return new JsonObject
                {
                    ["type"] = "tool-approval-request",
                    ["approvalId"] = approvalRequest.ApprovalId,
                    ["toolCallId"] = approvalRequest.ToolCall.ToolCallId,
                };
            case ToolResultPart toolResult:
                var toolOutput = new JsonObject
                {
                    ["type"] = "tool-output-available",
                    ["toolCallId"] = toolResult.Result.ToolCallId,
                    ["output"] = ParseJson(toolResult.Result.OutputJson),
                };
                if (toolResult.Result.ProviderExecuted)
                {
                    toolOutput["providerExecuted"] = true;
                    AddProviderMetadata(toolOutput, toolResult.Result.ProviderMetadata);
                }

                AddToolMetadata(toolOutput, toolResult.Result.ToolMetadata);
                if (toolResult.Result.Dynamic)
                {
                    toolOutput["dynamic"] = true;
                }

                return toolOutput;
            case ToolErrorPart toolError:
                // Provider-executed execution errors always bypass onError. Application-executed errors keep the text that the model
                // received unless tool errors are redacted (see ToolErrorText).
                var providerError = toolError.Result.ProviderExecuted;
                var errorOutput = new JsonObject
                {
                    ["type"] = "tool-output-error",
                    ["toolCallId"] = toolError.Result.ToolCallId,
                    ["errorText"] = providerError
                        ? ProviderErrorText(toolError.Result.OutputJson)
                        : ToolErrorText(ModelToolErrorText(toolError.Result), toolError.Result.Error?.Message ?? ModelToolErrorText(toolError.Result), redactToolErrors, onError),
                };
                if (providerError)
                {
                    errorOutput["providerExecuted"] = true;
                    AddProviderMetadata(errorOutput, toolError.Result.ProviderMetadata);
                }

                AddToolMetadata(errorOutput, toolError.Result.ToolMetadata);
                if (toolError.Result.Dynamic)
                {
                    errorOutput["dynamic"] = true;
                }

                return errorOutput;
            case ErrorPart error:
                return new JsonObject { ["type"] = "error", ["errorText"] = onError(error.Message) };
            case StartStepPart:
                return new JsonObject { ["type"] = "start-step" };
            case StepFinishPart:
                return new JsonObject { ["type"] = "finish-step" };
            case StartPart:
                if (!options.SendStart)
                {
                    return null;
                }

                var start = new JsonObject { ["type"] = "start" };
                if (options.ResponseMessageId != null)
                {
                    start["messageId"] = options.ResponseMessageId;
                }

                return start;
            case FinishPart finish:
                return options.SendFinish
                    ? new JsonObject { ["type"] = "finish", ["finishReason"] = GenAiConventions.FormatFinishReason(finish.FinishReason) }
                    : null;
            case AbortPart abort:
                return new JsonObject { ["type"] = "abort", ["reason"] = abort.Reason };
            default:
                // tool-input-end and raw parts do not produce UI message chunks
                return null;
        }
    }

    internal const string TextId = "text";

    internal const string ReasoningId = "reasoning";

    // The text that the model receives for a failed tool call.
    internal static string ModelToolErrorText(ExecutedTool result)
        => result.ModelOutput ?? result.Error?.Message ?? result.OutputJson;

    // The error text of a failed tool call is not altered by default. The UI message is the record that ConvertToModelMessages sends back
    // to the model on the next request, so a replaced text changes the history after the model has received the original.
    private static string ToolErrorText(string modelErrorText, string onErrorInput, bool redactToolErrors, Func<string, string> onError)
        => redactToolErrors ? onError(onErrorInput) : modelErrorText;

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
