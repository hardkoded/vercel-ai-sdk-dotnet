// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenTelemetry;
using Vercel.AI.Provider;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.AspNetCore;

/// <summary>Options of <see cref="UIMessageStreamConverter.ToUIMessageChunk"/> and <see cref="UIMessageStreamConverter.ToUIMessageStream"/>. Maps to <c>ToUIMessageChunkOptions</c>.</summary>
public sealed class ToUIMessageChunkOptions
{
    /// <summary>The tools of the call. A call, input, result or error of a dynamic tool is marked as dynamic. Maps to <c>tools</c>.</summary>
    public IReadOnlyList<Tool>? Tools { get; init; }

    /// <summary>Whether reasoning parts are sent. Defaults to <c>true</c>. Maps to <c>sendReasoning</c>.</summary>
    public bool SendReasoning { get; init; } = true;

    /// <summary>Whether source parts are sent. Defaults to <c>false</c>. Maps to <c>sendSources</c>.</summary>
    public bool SendSources { get; init; }

    /// <summary>Whether the <c>start</c> chunk is sent. Defaults to <c>true</c>. Maps to <c>sendStart</c>.</summary>
    public bool SendStart { get; init; } = true;

    /// <summary>Whether the <c>finish</c> chunk is sent. Defaults to <c>true</c>. Maps to <c>sendFinish</c>.</summary>
    public bool SendFinish { get; init; } = true;

    /// <summary>
    /// Formats stream errors. Also formats invalid tool calls and application-executed tool errors when <see cref="RedactToolErrors"/> is enabled.
    /// Provider-executed tool execution errors bypass this callback so their original error data survives model-message round trips.
    /// Null writes <c>An error occurred.</c>. The argument is the error. Maps to <c>onError</c>.
    /// </summary>
    public Func<Exception, string>? OnError { get; init; }

    /// <summary>
    /// Replaces the error text of failed tool calls with the result of <see cref="OnError"/>.
    /// Defaults to <c>true</c> when <see cref="OnError"/> is provided and to <c>false</c> otherwise.
    /// </summary>
    public bool? RedactToolErrors { get; init; }

    /// <summary>Metadata of the response message, sent with the <c>start</c> and <c>finish</c> chunks. Maps to <c>messageMetadata</c>.</summary>
    public JsonElement? MessageMetadata { get; init; }

    /// <summary>Id of the response message sent with the <c>start</c> chunk. Maps to <c>responseMessageId</c>.</summary>
    public string? ResponseMessageId { get; init; }

    /// <summary>
    /// The messages of the conversation. When the last one is an assistant message, the response continues it and reuses its id.
    /// Maps to <c>originalMessages</c>.
    /// </summary>
    public IReadOnlyList<UIMessage>? OriginalMessages { get; init; }

    /// <summary>Called when each step ends during multi-step agent runs. Maps to <c>onStepEnd</c>.</summary>
    public Func<UIMessageStreamOnStepEndEvent, Task>? OnStepEnd { get; init; }

    /// <summary>
    /// Called when the stream ends, whether it completed, failed, was aborted, or the consumer cancelled it. Maps to <c>onEnd</c>.
    /// </summary>
    public Func<UIMessageStreamOnEndEvent, Task>? OnEnd { get; init; }
}

/// <summary>Converts <c>streamText</c> parts into AI SDK UI message chunks.</summary>
public static partial class UIMessageStreamConverter
{
    /// <summary>
    /// Converts one <see cref="TextStreamPart"/> into a UI message chunk.
    /// Returns <see langword="null"/> for stream parts that do not produce UI message chunks.
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

        // provider-executed, dynamic tools are not listed in the tools. Maps to isDynamic.
        bool? IsDynamic(string toolName, bool dynamic)
        {
            var tool = options.Tools?.FirstOrDefault(candidate => string.Equals(candidate.Name, toolName, StringComparison.Ordinal));
            if (tool is null)
            {
                return dynamic ? true : null;
            }

            return tool.IsDynamic ? true : null;
        }

        switch (part)
        {
            case TextStartPart textStart:
                var textStartChunk = new JsonObject { ["type"] = "text-start", ["id"] = textStart.Id };
                AddProviderMetadata(textStartChunk, textStart.ProviderMetadata);
                return textStartChunk;
            case TextDeltaPart text:
                var textChunk = new JsonObject { ["type"] = "text-delta", ["id"] = text.Id, ["delta"] = text.Text };
                AddProviderMetadata(textChunk, text.ProviderMetadata);
                return textChunk;
            case TextEndPart textEnd:
                var textEndChunk = new JsonObject { ["type"] = "text-end", ["id"] = textEnd.Id };
                if (textEnd.Citations != null)
                {
                    textEndChunk["citations"] = CitationJson.ToJson(textEnd.Citations);
                }

                AddProviderMetadata(textEndChunk, textEnd.ProviderMetadata);
                return textEndChunk;
            case ReasoningStartPart reasoningStart:
                if (!options.SendReasoning)
                {
                    return null;
                }

                var reasoningStartChunk = new JsonObject { ["type"] = "reasoning-start", ["id"] = reasoningStart.Id };
                AddProviderMetadata(reasoningStartChunk, reasoningStart.ProviderMetadata);
                return reasoningStartChunk;
            case ReasoningDeltaPart reasoning:
                if (!options.SendReasoning)
                {
                    return null;
                }

                var reasoningChunk = new JsonObject { ["type"] = "reasoning-delta", ["id"] = reasoning.Id, ["delta"] = reasoning.Text };
                AddProviderMetadata(reasoningChunk, reasoning.ProviderMetadata);
                return reasoningChunk;
            case ReasoningEndPart reasoningEnd:
                if (!options.SendReasoning)
                {
                    return null;
                }

                var reasoningEndChunk = new JsonObject { ["type"] = "reasoning-end", ["id"] = reasoningEnd.Id };
                AddProviderMetadata(reasoningEndChunk, reasoningEnd.ProviderMetadata);
                return reasoningEndChunk;
            case FilePart file:
                var fileChunk = new JsonObject
                {
                    ["type"] = "file",
                    ["mediaType"] = file.File.MediaType,
                    ["url"] = "data:" + file.File.MediaType + ";base64," + Convert.ToBase64String(file.File.Data),
                };
                AddProviderMetadata(fileChunk, file.ProviderMetadata);
                return fileChunk;
            case ReasoningFilePart reasoningFile:
                if (!options.SendReasoning)
                {
                    return null;
                }

                var reasoningFileChunk = new JsonObject
                {
                    ["type"] = "reasoning-file",
                    ["mediaType"] = reasoningFile.File.MediaType,
                    ["url"] = "data:" + reasoningFile.File.MediaType + ";base64," + Convert.ToBase64String(reasoningFile.File.Data),
                };
                AddProviderMetadata(reasoningFileChunk, reasoningFile.ProviderMetadata);
                return reasoningFileChunk;
            case DocumentSourcePart documentSource:
                if (!options.SendSources)
                {
                    return null;
                }

                var documentSourceChunk = new JsonObject
                {
                    ["type"] = "source-document",
                    ["sourceId"] = documentSource.Source.Id,
                    ["mediaType"] = documentSource.Source.MediaType,
                    ["title"] = documentSource.Source.Title,
                };
                if (documentSource.Source.Filename != null)
                {
                    documentSourceChunk["filename"] = documentSource.Source.Filename;
                }

                AddProviderMetadata(documentSourceChunk, documentSource.Source.ProviderMetadata);
                return documentSourceChunk;
            case CustomPart custom:
                var customChunk = new JsonObject { ["type"] = "custom", ["kind"] = custom.Kind };
                AddProviderMetadata(customChunk, custom.ProviderMetadata);
                return customChunk;
            case SteeringMessagePart steering:
                return new JsonObject
                {
                    ["type"] = "steering-message",
                    ["id"] = steering.Id,
                    ["parts"] = ConvertUserMessageToUIParts(steering.Message),
                };
            case SourcePart source:
                if (!options.SendSources)
                {
                    return null;
                }

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
                if (IsDynamic(inputStart.ToolName, inputStart.Dynamic) is { } inputStartDynamic)
                {
                    inputStartChunk["dynamic"] = inputStartDynamic;
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

                if (IsDynamic(call.ToolCall.ToolName, call.ToolCall.Dynamic) is { } callDynamic)
                {
                    toolInput["dynamic"] = callDynamic;
                }

                if (call.ToolCall.Invalid)
                {
                    // streamText sends the model a tool result for an invalid tool call only when the tool is not provider-executed
                    var invalidError = call.ToolCall.Error!;
                    toolInput["errorText"] = call.ToolCall.ProviderExecuted
                        ? onError(invalidError)
                        : ToolErrorText(ModelToolErrorText(invalidError), invalidError, redactToolErrors, onError);
                }

                if (call.ToolCall.Title != null)
                {
                    toolInput["title"] = call.ToolCall.Title;
                }

                return toolInput;
            case ToolApprovalRequestPart approvalRequest:
                var approvalRequestChunk = new JsonObject
                {
                    ["type"] = "tool-approval-request",
                    ["approvalId"] = approvalRequest.ApprovalId,
                    ["toolCallId"] = approvalRequest.ToolCall.ToolCallId,
                };
                if (approvalRequest.ToolCall.InputSchemaInput is { } inputSchemaInput
                    && !JsonNode.DeepEquals(JsonNode.Parse(inputSchemaInput.GetRawText()), ParseJson(approvalRequest.ToolCall.ArgumentsJson)))
                {
                    approvalRequestChunk["inputSchemaInput"] = JsonNode.Parse(inputSchemaInput.GetRawText());
                }

                if (approvalRequest.Reason != null)
                {
                    approvalRequestChunk["reason"] = approvalRequest.Reason;
                }

                if (approvalRequest.IsAutomatic)
                {
                    approvalRequestChunk["isAutomatic"] = true;
                }

                if (approvalRequest.Signature != null)
                {
                    approvalRequestChunk["signature"] = approvalRequest.Signature;
                }

                return approvalRequestChunk;
            case ToolApprovalResponsePart approvalResponse:
                var approvalResponseChunk = new JsonObject
                {
                    ["type"] = "tool-approval-response",
                    ["approvalId"] = approvalResponse.ApprovalId,
                    ["approved"] = approvalResponse.Approved,
                };
                if (approvalResponse.Reason != null)
                {
                    approvalResponseChunk["reason"] = approvalResponse.Reason;
                }

                if (approvalResponse.ProviderExecuted)
                {
                    approvalResponseChunk["providerExecuted"] = true;
                }

                return approvalResponseChunk;
            case ToolOutputDeniedPart outputDenied:
                return new JsonObject { ["type"] = "tool-output-denied", ["toolCallId"] = outputDenied.ToolCallId };
            case ToolResultPart toolResult:
                var toolOutput = new JsonObject
                {
                    ["type"] = "tool-output-available",
                    ["toolCallId"] = toolResult.Result.ToolCallId,
                    // UI stream chunks are serialized as JSON, which drops undefined properties. Use null so tool outputs always keep the output field.
                    ["output"] = string.IsNullOrWhiteSpace(toolResult.Result.OutputJson) ? null : ParseJson(toolResult.Result.OutputJson),
                };
                if (toolResult.Result.ProviderExecuted)
                {
                    toolOutput["providerExecuted"] = true;
                    AddProviderMetadata(toolOutput, toolResult.Result.ProviderMetadata);
                }

                AddToolMetadata(toolOutput, toolResult.Result.ToolMetadata);
                if (toolResult.Result.Preliminary is { } preliminary)
                {
                    toolOutput["preliminary"] = preliminary;
                }

                if (IsDynamic(toolResult.Result.ToolName, toolResult.Result.Dynamic) is { } resultDynamic)
                {
                    toolOutput["dynamic"] = resultDynamic;
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
                        : ToolErrorText(ModelToolErrorText(toolError.Result), toolError.Result.Error ?? new JsError(toolError.Result.OutputJson), redactToolErrors, onError),
                };
                if (providerError)
                {
                    errorOutput["providerExecuted"] = true;
                    AddProviderMetadata(errorOutput, toolError.Result.ProviderMetadata);
                }

                AddToolMetadata(errorOutput, toolError.Result.ToolMetadata);
                if (IsDynamic(toolError.Result.ToolName, toolError.Result.Dynamic) is { } errorDynamic)
                {
                    errorOutput["dynamic"] = errorDynamic;
                }

                return errorOutput;
            case ErrorPart error:
                return new JsonObject { ["type"] = "error", ["errorText"] = onError(error.Error ?? new JsError(error.Message)) };
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
                AddMessageMetadata(start, options.MessageMetadata);
                if (options.ResponseMessageId != null)
                {
                    start["messageId"] = options.ResponseMessageId;
                }

                return start;
            case FinishPart finish:
                if (!options.SendFinish)
                {
                    return null;
                }

                var finishChunk = new JsonObject { ["type"] = "finish", ["finishReason"] = GenAiConventions.FormatFinishReason(finish.FinishReason) };
                AddMessageMetadata(finishChunk, options.MessageMetadata);
                return finishChunk;
            case AbortPart abort:
                return new JsonObject { ["type"] = "abort", ["reason"] = abort.Reason };
            case ToolInputEndPart:
            case PingPart:
            case RawPart:
                // tool-input-end, ping and raw parts do not produce UI message chunks
                return null;
            default:
                throw new InvalidOperationException("Unknown chunk type: " + part.Type);
        }
    }

    // Maps to convertUserMessageToUIParts.
    private static JsonArray ConvertUserMessageToUIParts(UserModelMessage message)
    {
        var parts = new JsonArray();
        foreach (var part in message.Content)
        {
            JsonObject uiPart;
            switch (part)
            {
                case TextContentPart text:
                    uiPart = new JsonObject { ["type"] = "text", ["text"] = text.Text };
                    break;
                case FileContentPart file:
                    uiPart = new JsonObject { ["type"] = "file", ["mediaType"] = file.MediaType };
                    if (file.FileName != null)
                    {
                        uiPart["filename"] = file.FileName;
                    }

                    if (file.ProviderReference != null)
                    {
                        // the provider reference takes precedence over the url in model messages:
                        uiPart["url"] = string.Empty;
                        uiPart["providerReference"] = JsonSerializer.SerializeToNode(file.ProviderReference);
                    }
                    else if (file.Url != null)
                    {
                        uiPart["url"] = file.Url;
                    }
                    else if (file.Data != null)
                    {
                        uiPart["url"] = "data:" + file.MediaType + ";base64," + Convert.ToBase64String(file.Data);
                    }
                    else
                    {
                        uiPart["url"] = "data:" + file.MediaType + ";base64," + Convert.ToBase64String(Encoding.UTF8.GetBytes(file.Text ?? string.Empty));
                    }

                    break;
                default:
                    continue;
            }

            AddProviderMetadata(uiPart, OptionsToElement(part switch
            {
                TextContentPart t => t.ProviderOptions,
                FileContentPart f => f.ProviderOptions,
                _ => null,
            }));
            parts.Add(uiPart);
        }

        return parts;
    }

    private static JsonElement? OptionsToElement(IReadOnlyDictionary<string, JsonElement>? options)
        => options is null ? null : JsonSerializer.SerializeToElement(options);

    private static void AddMessageMetadata(JsonObject payload, JsonElement? metadata)
    {
        if (metadata is { ValueKind: not (JsonValueKind.Null or JsonValueKind.Undefined) } element)
        {
            payload["messageMetadata"] = JsonNode.Parse(element.GetRawText());
        }
    }

    // The error text that the model receives for a failed tool call, or null when the error cannot be serialized. In that case the model
    // did not receive an error text either, because creating the tool result fails the same way. Maps to getModelToolErrorText.
    internal static string? ModelToolErrorText(ExecutedTool result)
    {
        try
        {
            return result.ModelOutput ?? result.Error?.Message ?? result.OutputJson;
        }
        catch (Exception)
        {
            return null;
        }
    }

    internal static string? ModelToolErrorText(Exception error)
    {
        try
        {
            return ErrorMessage.ToErrorString(error);
        }
        catch (Exception)
        {
            return null;
        }
    }

    // The error text of a failed tool call is not altered by default. The UI message is the record that ConvertToModelMessages sends back
    // to the model on the next request, so a replaced text changes the history after the model has received the original. When the text
    // cannot be serialized, onError formats the error.
    private static string ToolErrorText(string? modelErrorText, Exception onErrorInput, bool redactToolErrors, Func<Exception, string> onError)
        => redactToolErrors ? onError(onErrorInput) : modelErrorText ?? onError(onErrorInput);

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
