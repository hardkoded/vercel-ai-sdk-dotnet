// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Google;

/// <summary>Maps an Interactions API status onto a finish reason.</summary>
public static class GoogleInteractionsFinish
{
    /// <summary>
    /// <c>completed</c> is a tool call when the response contains a client function call.
    /// <c>requires_action</c> is always a tool call.
    /// </summary>
    public static FinishReason Map(string? status, bool hasFunctionCall)
    {
        switch (status)
        {
            case "completed":
                return hasFunctionCall ? FinishReason.ToolCalls : FinishReason.Stop;
            case "requires_action":
                return FinishReason.ToolCalls;
            case "failed":
                return FinishReason.Error;
            case "incomplete":
                return FinishReason.Length;
            default:
                return FinishReason.Other;
        }
    }
}

/// <summary>The result of <see cref="GoogleInteractionsInput.Convert(IReadOnlyList{ModelMessage}, string, bool?, string)"/>. Maps to <c>ConvertToGoogleInteractionsInputResult</c>.</summary>
public sealed class ConvertToGoogleInteractionsInputResult
{
    /// <summary>Creates a result.</summary>
    public ConvertToGoogleInteractionsInputResult(
        JsonArray input,
        string? systemInstruction,
        IReadOnlyList<CallWarning> warnings
    )
    {
        Input = input;
        SystemInstruction = systemInstruction;
        Warnings = warnings;
    }

    /// <summary>The <c>input</c> steps.</summary>
    public JsonArray Input { get; }

    /// <summary>System messages joined with a blank line, or null.</summary>
    public string? SystemInstruction { get; }

    /// <summary>Warnings produced while converting.</summary>
    public IReadOnlyList<CallWarning> Warnings { get; }
}

/// <summary>Converts a prompt into an Interactions <c>input</c> array. Maps to <c>convertToGoogleInteractionsInput</c>.</summary>
public static class GoogleInteractionsInput
{
    /// <summary>
    /// Converts shared messages into Interactions steps. User turns become <c>user_input</c> steps. Assistant content becomes
    /// <c>model_output</c>, <c>thought</c>, <c>function_call</c>, and processing steps. Tool results become <c>function_result</c> steps.
    /// </summary>
    /// <param name="prompt">The prompt.</param>
    /// <param name="previousInteractionId">Drops assistant turns whose parts carry this interaction id, unless <paramref name="store"/> is false.</param>
    /// <param name="store">The <c>store</c> option.</param>
    /// <param name="mediaResolution">Default <c>resolution</c> of image and video blocks: <c>low</c>, <c>medium</c>, <c>high</c>, or <c>ultra_high</c>.</param>
    public static ConvertToGoogleInteractionsInputResult Convert(
        IReadOnlyList<ModelMessage> prompt,
        string? previousInteractionId = null,
        bool? store = null,
        string? mediaResolution = null
    )
    {
        var warnings = new List<CallWarning>();

        // previousInteractionId + store=false is incoherent: warn, and send the full history.
        if (previousInteractionId != null && store == false)
        {
            warnings.Add(
                new CallWarning(
                    "other",
                    "google.interactions: providerOptions.google.previousInteractionId was set together with store: false. These are incoherent (the prior interaction cannot be referenced when nothing was stored on the server); the full history will be sent and previous_interaction_id will still be emitted."
                )
            );
        }

        var messages = ToolModelMessage.Flatten(prompt ?? []);
        if (previousInteractionId != null && store != false)
        {
            messages = CompactPromptForPreviousInteraction(messages, previousInteractionId);
        }

        var systemTexts = new List<string>();
        var steps = new JsonArray();
        foreach (var message in messages)
        {
            switch (message)
            {
                case SystemModelMessage system:
                    systemTexts.Add(system.Content);
                    break;
                case UserModelMessage user:
                    var content = new List<JsonObject>();
                    foreach (var part in user.Content)
                    {
                        var block = ConvertUserPart(part, warnings, mediaResolution);
                        if (block != null)
                        {
                            content.Add(block);
                        }
                    }

                    var merged = MergeAdjacentTextContent(content);
                    if (merged.Count > 0)
                    {
                        steps.Add(
                            new JsonObject
                            {
                                ["type"] = "user_input",
                                ["content"] = new JsonArray(
                                    merged.Select(block => (JsonNode?)block).ToArray()
                                ),
                            }
                        );
                    }

                    break;
                case AssistantModelMessage assistant:
                    ConvertAssistant(assistant, steps, warnings, mediaResolution);
                    break;
                case ToolModelMessage tool:
                    steps.Add(ConvertToolResultPart(tool, warnings, mediaResolution));
                    break;
            }
        }

        return new ConvertToGoogleInteractionsInputResult(
            steps,
            systemTexts.Count > 0 ? string.Join("\n\n", systemTexts) : null,
            warnings
        );
    }

    private static void ConvertAssistant(
        AssistantModelMessage assistant,
        JsonArray steps,
        List<CallWarning> warnings,
        string? mediaResolution
    )
    {
        // Adjacent text and file content coalesces into one model_output step. Reasoning and tool calls are their own steps.
        var pending = new JsonArray();
        void Flush()
        {
            if (pending.Count > 0)
            {
                steps.Add(new JsonObject { ["type"] = "model_output", ["content"] = pending });
                pending = [];
            }
        }

        foreach (var part in assistant.Parts)
        {
            var signature = GoogleOption(part.ProviderOptions, "signature");
            switch (part)
            {
                case AssistantTextPart text:
                    pending.Add(new JsonObject { ["type"] = "text", ["text"] = text.Text });
                    break;
                case AssistantReasoningPart reasoning:
                    Flush();
                    var thought = new JsonObject { ["type"] = "thought" };
                    GoogleJson.Set(thought, "signature", signature);
                    if (reasoning.Text.Length > 0)
                    {
                        thought["summary"] = new JsonArray
                        {
                            new JsonObject { ["type"] = "text", ["text"] = reasoning.Text },
                        };
                    }

                    steps.Add(thought);
                    break;
                case AssistantReasoningFilePart:
                    warnings.Add(UnsupportedAssistantPart(part.Type));
                    break;
                case AssistantFilePart file:
                    var fileBlock = ConvertFile(
                        file.MediaType,
                        file.Data,
                        null,
                        file.Url,
                        file.ProviderReference,
                        null,
                        file.ProviderOptions,
                        warnings,
                        mediaResolution
                    );
                    if (fileBlock != null)
                    {
                        pending.Add(fileBlock);
                    }

                    break;
                case AssistantCustomPart custom:
                    Flush();
                    var processingId = GoogleOption(part.ProviderOptions, "processingId");
                    var processingCallId = GoogleOption(part.ProviderOptions, "processingCallId");
                    if (custom.Kind == "google.processing_call" && processingId != null)
                    {
                        var call = new JsonObject
                        {
                            ["type"] = "processing_call",
                            ["id"] = processingId,
                        };
                        GoogleJson.Set(call, "signature", signature);
                        steps.Add(call);
                    }
                    else if (custom.Kind == "google.processing_result" && processingCallId != null)
                    {
                        var result = new JsonObject
                        {
                            ["type"] = "processing_result",
                            ["call_id"] = processingCallId,
                        };
                        GoogleJson.Set(result, "signature", signature);
                        steps.Add(result);
                    }
                    else
                    {
                        warnings.Add(
                            new CallWarning(
                                "other",
                                $"google.interactions: unsupported or invalid custom assistant content part \"{custom.Kind}\"; part dropped."
                            )
                        );
                    }

                    break;
                case AssistantToolCallPart toolCall:
                    Flush();
                    var functionCall = new JsonObject
                    {
                        ["type"] = "function_call",
                        ["id"] = toolCall.ToolCall.ToolCallId,
                        ["name"] = toolCall.ToolCall.ToolName,
                        ["arguments"] = SafeParseToolArgs(toolCall.ToolCall.ArgumentsJson),
                    };
                    GoogleJson.Set(functionCall, "signature", signature);
                    steps.Add(functionCall);
                    break;
                default:
                    warnings.Add(UnsupportedAssistantPart(part.Type));
                    break;
            }
        }

        Flush();
    }

    private static CallWarning UnsupportedAssistantPart(string type)
    {
        return new CallWarning(
            "other",
            $"google.interactions: unsupported assistant content part type \"{type}\"; part dropped."
        );
    }

    private static JsonObject? ConvertUserPart(
        UserContentPart part,
        List<CallWarning> warnings,
        string? mediaResolution
    )
    {
        switch (part)
        {
            case TextContentPart text:
                return new JsonObject { ["type"] = "text", ["text"] = text.Text };
            case FileContentPart file:
                return ConvertFile(
                    file.MediaType,
                    file.Data,
                    null,
                    file.Url,
                    file.ProviderReference,
                    file.Text,
                    file.ProviderOptions,
                    warnings,
                    mediaResolution
                );
            case FileUrlContentPart fileUrl:
                return ConvertFile(
                    fileUrl.MediaType ?? "application",
                    null,
                    null,
                    fileUrl.Url,
                    null,
                    null,
                    fileUrl.ProviderOptions,
                    warnings,
                    mediaResolution
                );
            case ImageUrlContentPart imageUrl:
                return ConvertFile(
                    "image",
                    null,
                    null,
                    imageUrl.Url,
                    null,
                    null,
                    imageUrl.ProviderOptions,
                    warnings,
                    mediaResolution
                );
            case FileDataContentPart fileData:
                return ConvertFile(
                    fileData.MediaType,
                    null,
                    fileData.Data,
                    null,
                    null,
                    null,
                    fileData.ProviderOptions,
                    warnings,
                    mediaResolution
                );
            case ImageDataContentPart imageData:
                return ConvertFile(
                    imageData.MediaType,
                    null,
                    imageData.Data,
                    null,
                    null,
                    null,
                    imageData.ProviderOptions,
                    warnings,
                    mediaResolution
                );
            case ImageFileReferenceContentPart imageReference:
                return ConvertFile(
                    "image",
                    null,
                    null,
                    null,
                    imageReference.ProviderReference,
                    null,
                    imageReference.ProviderOptions,
                    warnings,
                    mediaResolution
                );
            case FileReferenceContentPart reference:
                return ConvertFile(
                    "application",
                    null,
                    null,
                    null,
                    reference.ProviderReference,
                    null,
                    reference.ProviderOptions,
                    warnings,
                    mediaResolution
                );
            default:
                return null;
        }
    }

    // Maps a file to an `image`, `audio`, `video`, or `document` content block.
    private static JsonObject? ConvertFile(
        string mediaType,
        byte[]? bytes,
        string? base64,
        string? url,
        IReadOnlyDictionary<string, string>? reference,
        string? text,
        IReadOnlyDictionary<string, JsonElement>? providerOptions,
        List<CallWarning> warnings,
        string? mediaResolution
    )
    {
        if (text != null)
        {
            return new JsonObject { ["type"] = "text", ["text"] = text };
        }

        var kind = MediaTypes.GetTopLevelMediaType(mediaType) switch
        {
            "image" => "image",
            "audio" => "audio",
            "video" => "video",
            "application" or "text" => "document",
            _ => null,
        };
        if (kind == null)
        {
            warnings.Add(
                new CallWarning(
                    "other",
                    $"google.interactions: unsupported file media type \"{mediaType}\"; part dropped."
                )
            );
            return null;
        }

        var block = new JsonObject { ["type"] = kind };
        if (bytes != null || base64 != null)
        {
            block["data"] = base64 ?? System.Convert.ToBase64String(bytes!);
            block["mime_type"] = MediaTypes.ResolveFullMediaType(
                mediaType,
                bytes ?? (base64 != null ? System.Convert.FromBase64String(base64) : null)
            );
        }
        else
        {
            block["uri"] = url ?? ProviderReferences.ResolveProviderReference(reference!, "google");
            if (MediaTypes.IsFullMediaType(mediaType))
            {
                block["mime_type"] = mediaType;
            }
        }

        var options = FileOptions(providerOptions);
        var resolution = options.Resolution ?? mediaResolution;
        if (resolution != null && kind != "audio")
        {
            block["resolution"] = resolution;
        }

        if (kind != "image" && options.Processing != null)
        {
            block["processing"] = options.Processing;
        }

        return block;
    }

    private static (string? Resolution, JsonNode? Processing) FileOptions(
        IReadOnlyDictionary<string, JsonElement>? providerOptions
    )
    {
        if (
            providerOptions == null
            || !providerOptions.TryGetValue("google", out var google)
            || google.ValueKind != JsonValueKind.Object
        )
        {
            return (null, null);
        }

        string? resolution = null;
        if (
            google.TryGetProperty("resolution", out var value)
            && value.ValueKind == JsonValueKind.String
        )
        {
            resolution = value.GetString();
            if (resolution is not ("low" or "medium" or "high"))
            {
                throw new AiSdkException(
                    "providerOptions.google.resolution must be one of: low, medium, high."
                );
            }
        }

        if (
            !google.TryGetProperty("processing", out var processing)
            || processing.ValueKind == JsonValueKind.Null
        )
        {
            return (resolution, null);
        }

        if (processing.ValueKind == JsonValueKind.String)
        {
            var mode = processing.GetString();
            return mode is "agentic" or "static"
                ? (resolution, JsonValue.Create(mode))
                : throw new AiSdkException(
                    "providerOptions.google.processing must be one of: agentic, static."
                );
        }

        if (
            processing.ValueKind != JsonValueKind.Object
            || !processing.TryGetProperty("type", out var type)
            || type.GetString() != "static"
        )
        {
            return (resolution, null);
        }

        var result = new JsonObject { ["type"] = "static" };
        CopyOffset(result, processing, "startOffset", "start_offset");
        CopyOffset(result, processing, "endOffset", "end_offset");
        CopyRaw(result, processing, "fps", "fps");
        CopyRaw(result, processing, "contentLayers", "content_layers");
        CopyNested(
            result,
            processing,
            "pageOptions",
            "page_options",
            ("startPage", "start_page"),
            ("endPage", "end_page"),
            ("contentLayers", "content_layers")
        );
        CopyNested(
            result,
            processing,
            "spreadsheetOptions",
            "spreadsheet_options",
            ("sheetIndex", "sheet_index"),
            ("visibleCells", "visible_cells"),
            ("contentLayers", "content_layers")
        );
        return (resolution, result);
    }

    private static void CopyOffset(JsonObject target, JsonElement source, string name, string key)
    {
        if (source.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number)
        {
            target[key] = value.GetDouble().ToString("R", CultureInfo.InvariantCulture) + "s";
        }
    }

    private static void CopyRaw(JsonObject target, JsonElement source, string name, string key)
    {
        if (source.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null)
        {
            target[key] = JsonNode.Parse(value.GetRawText());
        }
    }

    private static void CopyNested(
        JsonObject target,
        JsonElement source,
        string name,
        string key,
        params (string Name, string Key)[] fields
    )
    {
        if (
            !source.TryGetProperty(name, out var nested)
            || nested.ValueKind != JsonValueKind.Object
        )
        {
            return;
        }

        var result = new JsonObject();
        foreach (var (fieldName, fieldKey) in fields)
        {
            CopyRaw(result, nested, fieldName, fieldKey);
        }

        target[key] = result;
    }

    private static string? GoogleOption(
        IReadOnlyDictionary<string, JsonElement>? providerOptions,
        string name
    )
    {
        return
            providerOptions != null
            && providerOptions.TryGetValue("google", out var google)
            && google.ValueKind == JsonValueKind.Object
            && google.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    // Drops assistant messages of the linked interaction, and the tool results of their tool calls.
    private static IReadOnlyList<ModelMessage> CompactPromptForPreviousInteraction(
        IReadOnlyList<ModelMessage> prompt,
        string previousInteractionId
    )
    {
        var result = new List<ModelMessage>();
        var droppedToolCallIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var message in prompt)
        {
            if (
                message is AssistantModelMessage assistant
                && assistant.Parts.Any(part =>
                    GoogleOption(part.ProviderOptions, "interactionId") == previousInteractionId
                )
            )
            {
                foreach (var part in assistant.Parts.OfType<AssistantToolCallPart>())
                {
                    droppedToolCallIds.Add(part.ToolCall.ToolCallId);
                }

                continue;
            }

            if (message is ToolModelMessage tool && droppedToolCallIds.Contains(tool.ToolCallId))
            {
                continue;
            }

            result.Add(message);
        }

        return result;
    }

    private static JsonNode SafeParseToolArgs(string input)
    {
        try
        {
            // A call without arguments has a blank ArgumentsJson in this port. Upstream has no such value, so it sends {}.
            var text = string.IsNullOrWhiteSpace(input) ? "{}" : input;

            // secureJsonParse: rejects __proto__ and constructor.prototype keys.
            SecureJson.Parse(text);
            var parsed = JsonNode.Parse(text);
            if (parsed is JsonObject)
            {
                return parsed;
            }

            return new JsonObject { ["value"] = parsed };
        }
        catch (JsonException)
        {
            return new JsonObject { ["value"] = input };
        }
    }

    private static JsonObject ConvertToolResultPart(
        ToolModelMessage tool,
        List<CallWarning> warnings,
        string? mediaResolution
    )
    {
        var step = new JsonObject
        {
            ["type"] = "function_result",
            ["call_id"] = tool.ToolCallId,
            ["name"] = tool.ToolName,
        };
        GoogleJson.Set(step, "signature", GoogleOption(tool.ProviderOptions, "signature"));

        if (tool.OutputContent != null)
        {
            var blocks = new JsonArray();
            foreach (var item in tool.OutputContent)
            {
                if (item is TextContentPart text)
                {
                    blocks.Add(new JsonObject { ["type"] = "text", ["text"] = text.Text });
                    continue;
                }

                var block = ConvertUserPart(item, warnings, mediaResolution);
                if (
                    item.Type
                    is "file"
                        or "file-url"
                        or "file-data"
                        or "file-reference"
                        or "image-url"
                        or "image-data"
                        or "image-file-reference"
                )
                {
                    if (block != null)
                    {
                        blocks.Add(block);
                    }
                }
                else
                {
                    warnings.Add(
                        new CallWarning(
                            "other",
                            $"google.interactions: tool-result content part type \"{item.Type}\" is not supported; part dropped."
                        )
                    );
                }
            }

            step["result"] = blocks;
            return step;
        }

        switch (tool.OutputType)
        {
            case "text":
                step["result"] = tool.OutputJson;
                break;
            case "json":
                step["result"] = CompactJson(tool.OutputJson);
                break;
            case "error-text":
                step["is_error"] = true;
                step["result"] = tool.OutputJson;
                break;
            case "error-json":
                step["is_error"] = true;
                step["result"] = CompactJson(tool.OutputJson);
                break;
            default:
                step["is_error"] = true;
                step["result"] = tool.ExecutionDeniedReason ?? "Tool execution denied by user.";
                break;
        }

        return step;
    }

    private static string CompactJson(string json)
    {
        try
        {
            return JsonNode.Parse(json)?.ToJsonString() ?? "null";
        }
        catch (JsonException)
        {
            return json;
        }
    }

    // Collapses adjacent text blocks into one, separated by a blank line.
    private static List<JsonObject> MergeAdjacentTextContent(List<JsonObject> content)
    {
        if (content.Count < 2)
        {
            return content;
        }

        var result = new List<JsonObject>();
        foreach (var block in content)
        {
            var last = result.Count > 0 ? result[result.Count - 1] : null;
            if (
                last != null
                && (string?)block["type"] == "text"
                && (string?)last["type"] == "text"
                && !last.ContainsKey("annotations")
                && !block.ContainsKey("annotations")
            )
            {
                result[result.Count - 1] = new JsonObject
                {
                    ["type"] = "text",
                    ["text"] = (string?)last["text"] + "\n\n" + (string?)block["text"],
                };
                continue;
            }

            result.Add(block);
        }

        return result;
    }
}

/// <summary>Reads the <c>steps</c> of an Interactions response into generated content. Maps to <c>parseGoogleInteractionsOutputs</c>.</summary>
public static class GoogleInteractionsOutput
{
    private static readonly HashSet<string> BuiltinToolCallTypes = new(StringComparer.Ordinal)
    {
        "google_search_call",
        "code_execution_call",
        "url_context_call",
        "file_search_call",
        "google_maps_call",
        "mcp_server_tool_call",
    };

    private static readonly HashSet<string> BuiltinToolResultTypes = new(StringComparer.Ordinal)
    {
        "google_search_result",
        "code_execution_result",
        "url_context_result",
        "file_search_result",
        "google_maps_result",
        "mcp_server_tool_result",
    };

    /// <summary>
    /// Walks <c>steps</c> and emits content: text with its annotation sources, images and videos as files, thoughts as reasoning,
    /// processing steps as custom parts, function calls, and built-in tool calls and results. <c>user_input</c> steps are skipped.
    /// </summary>
    /// <param name="steps">The <c>steps</c> of the interaction.</param>
    /// <param name="generateId">Creates ids for steps that have none.</param>
    /// <param name="interactionId">The interaction id, stamped on each part's <c>providerMetadata.google.interactionId</c> so a later turn can compact history.</param>
    public static (IReadOnlyList<GeneratedContent> Content, bool HasFunctionCall) Parse(
        JsonArray? steps,
        Func<string> generateId,
        string? interactionId = null
    )
    {
        var content = new List<GeneratedContent>();
        var hasFunctionCall = false;
        if (steps == null)
        {
            return (content, hasFunctionCall);
        }

        foreach (var stepNode in steps)
        {
            if (stepNode is not JsonObject step || step["type"] is not JsonValue typeNode)
            {
                continue;
            }

            var type = (string?)typeNode;
            switch (type)
            {
                case "user_input":
                    break;
                case "model_output":
                    if (step["content"] is JsonArray blocks)
                    {
                        foreach (var blockNode in blocks)
                        {
                            if (blockNode is JsonObject block)
                            {
                                ParseModelOutputBlock(block, content, generateId, interactionId);
                            }
                        }
                    }

                    break;
                case "thought":
                    var summary = step["summary"] is JsonArray items
                        ? items
                            .OfType<JsonObject>()
                            .Where(item =>
                                (string?)item["type"] == "text" && item["text"] is JsonValue
                            )
                            .Select(item => (string?)item["text"] ?? string.Empty)
                        : [];
                    content.Add(
                        new GeneratedReasoning(
                            string.Join("\n", summary),
                            Metadata(Signature(step), interactionId)
                        )
                    );
                    break;
                case "processing_call":
                    var processingId = GoogleInteractionsStreaming.String(step, "id");
                    content.Add(
                        new GeneratedCustom(
                            "google.processing_call",
                            Metadata(
                                Signature(step),
                                interactionId,
                                processingId: string.IsNullOrEmpty(processingId)
                                    ? generateId()
                                    : processingId
                            )
                        )
                    );
                    break;
                case "processing_result":
                    var processingCallId = GoogleInteractionsStreaming.String(step, "call_id");
                    content.Add(
                        new GeneratedCustom(
                            "google.processing_result",
                            Metadata(
                                Signature(step),
                                interactionId,
                                processingCallId: string.IsNullOrEmpty(processingCallId)
                                    ? generateId()
                                    : processingCallId
                            )
                        )
                    );
                    break;
                case "function_call":
                    hasFunctionCall = true;
                    content.Add(
                        new GeneratedToolCall(
                            GoogleInteractionsStreaming.String(step, "id") ?? string.Empty,
                            GoogleInteractionsStreaming.String(step, "name") ?? string.Empty,
                            ArgumentsJson(step),
                            Metadata(Signature(step), interactionId)
                        )
                    );
                    break;
                default:
                    if (type != null && BuiltinToolCallTypes.Contains(type))
                    {
                        var id = GoogleInteractionsStreaming.String(step, "id");
                        content.Add(
                            new GeneratedToolCall(
                                string.IsNullOrEmpty(id) ? generateId() : id,
                                type == "mcp_server_tool_call"
                                    ? GoogleInteractionsStreaming.String(step, "name")
                                        ?? "mcp_server_tool"
                                    : type.Substring(0, type.Length - "_call".Length),
                                ArgumentsJson(step)
                            )
                            {
                                ProviderExecuted = true,
                            }
                        );
                    }
                    else if (type != null && BuiltinToolResultTypes.Contains(type))
                    {
                        var callId = GoogleInteractionsStreaming.String(step, "call_id");
                        content.Add(
                            new GeneratedToolResult(
                                string.IsNullOrEmpty(callId) ? generateId() : callId,
                                type == "mcp_server_tool_result"
                                    ? GoogleInteractionsStreaming.String(step, "name")
                                        ?? "mcp_server_tool"
                                    : type.Substring(0, type.Length - "_result".Length),
                                step["result"]?.ToJsonString() ?? "null"
                            )
                        );
                        foreach (
                            var source in GoogleInteractionsSources.BuiltinToolResultToSources(
                                step,
                                generateId
                            )
                        )
                        {
                            content.Add(ToGenerated(source));
                        }
                    }

                    break;
            }
        }

        return (content, hasFunctionCall);
    }

    private static void ParseModelOutputBlock(
        JsonObject block,
        List<GeneratedContent> content,
        Func<string> generateId,
        string? interactionId
    )
    {
        var blockType = GoogleInteractionsStreaming.String(block, "type");
        switch (blockType)
        {
            case "text":
                content.Add(
                    new GeneratedText(
                        GoogleInteractionsStreaming.String(block, "text") ?? string.Empty,
                        null,
                        Metadata(null, interactionId)
                    )
                );
                foreach (
                    var source in GoogleInteractionsSources.AnnotationsToSources(
                        block["annotations"] as JsonArray,
                        generateId
                    )
                )
                {
                    content.Add(ToGenerated(source));
                }

                break;
            case "image":
            case "video":
                var mediaType = GoogleInteractionsStreaming.String(block, "mime_type")
                    ?? (blockType == "image" ? "image/png" : "video/mp4");
                var data = GoogleInteractionsStreaming.String(block, "data");
                var uri = GoogleInteractionsStreaming.String(block, "uri");
                if (!string.IsNullOrEmpty(data))
                {
                    content.Add(
                        new GoogleGeneratedFile(
                            System.Convert.FromBase64String(data),
                            mediaType,
                            Metadata(null, interactionId)
                        )
                    );
                }
                else if (!string.IsNullOrEmpty(uri))
                {
                    content.Add(
                        new GoogleGeneratedFile(
                            new Uri(uri),
                            mediaType,
                            Metadata(null, interactionId)
                        )
                    );
                }

                break;
        }
    }

    private static GeneratedContent ToGenerated(LanguageModelStreamPart source)
    {
        return source switch
        {
            SourceStreamPart url => new GeneratedSource(
                url.Id,
                url.Url,
                url.Title,
                url.ProviderMetadata
            ),
            DocumentSourceStreamPart document => new GeneratedDocumentSource(
                document.Id,
                document.MediaType,
                document.Title,
                document.Filename,
                document.ProviderMetadata
            ),
            _ => throw new InvalidOperationException("Unexpected source part " + source.Type),
        };
    }

    private static string? Signature(JsonObject step)
    {
        return GoogleInteractionsStreaming.String(step, "signature");
    }

    private static string ArgumentsJson(JsonObject step)
    {
        return step["arguments"] is { } arguments && arguments is not null
            ? arguments.ToJsonString()
            : "{}";
    }

    // `providerMetadata.google` of an output part: the signature, the interaction id, and the processing ids a later turn round-trips.
    private static JsonElement? Metadata(
        string? signature,
        string? interactionId,
        string? processingId = null,
        string? processingCallId = null
    )
    {
        var google = new JsonObject();
        GoogleJson.Set(google, "signature", signature);
        GoogleJson.Set(google, "interactionId", interactionId);
        GoogleJson.Set(google, "processingId", processingId);
        GoogleJson.Set(google, "processingCallId", processingCallId);
        return google.Count == 0
            ? null
            : JsonSerializer.SerializeToElement(new JsonObject { ["google"] = google });
    }
}

/// <summary>An agent to call through the Interactions API: <c>{ agent: name }</c> or <c>{ managedAgent: name }</c> upstream.</summary>
public sealed class GoogleInteractionsAgent
{
    /// <summary>Creates an agent input for a Gemini agent preset such as <c>deep-research-pro-preview-12-2025</c>.</summary>
    public GoogleInteractionsAgent(string agent)
    {
        Agent = !string.IsNullOrEmpty(agent)
            ? agent
            : throw new ArgumentException("The agent name is required.", nameof(agent));
    }

    /// <summary>The agent name, sent as <c>agent</c> in the request body.</summary>
    public string Agent { get; }

    /// <summary>Creates an agent input for a managed agent you created on Google's side.</summary>
    public static GoogleInteractionsAgent Managed(string managedAgent)
    {
        return new GoogleInteractionsAgent(managedAgent);
    }
}

/// <summary>Language model that posts to the Gemini Interactions API.</summary>
public sealed class GoogleInteractionsModel : ILanguageModel
{
    private readonly GoogleProvider _provider;
    private readonly bool _isAgent;

    /// <summary>Creates an interactions model.</summary>
    public GoogleInteractionsModel(GoogleProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <summary>Creates an interactions model that calls an agent. The request sends <c>agent</c> instead of <c>model</c>.</summary>
    public GoogleInteractionsModel(GoogleProvider provider, GoogleInteractionsAgent agent)
        : this(provider, (agent ?? throw new ArgumentNullException(nameof(agent))).Agent)
    {
        _isAgent = true;
    }

    /// <inheritdoc />
    public string SpecificationVersion
    {
        get { return "V4"; }
    }

    /// <inheritdoc />
    public string Provider
    {
        get
        {
            return
                _provider.ModelProvider.IndexOf("vertex", StringComparison.OrdinalIgnoreCase) >= 0
                ? "google.vertex.interactions"
                : _provider.ModelProvider + ".interactions";
        }
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> SupportedUrls { get; } =
        new Dictionary<string, string>
        {
            ["image/*"] = "^https?://.+\\z",
            ["application/pdf"] = "^https?://.+\\z",
            ["audio/*"] = "^https?://.+\\z",
            ["video/*"] =
                "^https?://(www\\.)?youtube\\.com/watch\\?v=.+\\z|^https?://youtu\\.be/.+\\z|^gs://.+\\z",
        };

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    )
    {
        // An aborted call fails before the request is dispatched.
        cancellationToken.ThrowIfCancellationRequested();
        var (input, warnings) = Prepare(options);
        var background = Background(options);
        var pollingTimeout = PollingTimeout(options);
        var headers = await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false);
        var baseUrl = InteractionsBase(_provider.Options.BaseUrl);
        var requestJson = GoogleJson.Write(input);
        var post = await _provider
            .Http.SendJsonStringAsync(
                HttpMethod.Post,
                ProviderUtils.ApiKeys.Combine(baseUrl, "interactions"),
                requestJson,
                headers,
                cancellationToken
            )
            .ConfigureAwait(false);
        var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(post.Body) ? "{}" : post.Body);

        // A background or agent call (such as Gemini 4 long decode, where the server runs every hop) may return a
        // non-terminal status. Poll GET /interactions/{id} until it is terminal so the result matches a synchronous call.
        if (background || _isAgent)
        {
            document = await PollIfNonTerminalAsync(
                    document,
                    baseUrl,
                    headers,
                    pollingTimeout,
                    cancellationToken
                )
                .ConfigureAwait(false);
        }

        using (document)
        {
            var response =
                JsonNode.Parse(document.RootElement.GetRawText()) as JsonObject ?? new JsonObject();
            var status = (string?)response["status"];

            // The API omits `id` when `store: false`. Surface an interaction id only when the API returned one.
            var interactionId = GoogleInteractionsStreaming.String(response, "id")
                is { Length: > 0 } id
                ? id
                : null;
            var parsed = GoogleInteractionsOutput.Parse(
                response["steps"] as JsonArray,
                _provider.Options.GenerateId ?? DefaultId,
                interactionId
            );

            // The Interactions API returns the applied tier in the body. The header is a defensive fallback.
            post.Headers.TryGetValue("x-gemini-service-tier", out var headerServiceTier);
            var serviceTier = GoogleInteractionsStreaming.String(response, "service_tier")
                ?? headerServiceTier;
            var usage = response["usage"] as JsonObject;
            return new LanguageModelGenerateResult(
                parsed.Content,
                GoogleInteractionsFinish.Map(status, parsed.HasFunctionCall),
                Usage(usage),
                status,
                warnings,
                interactionId,
                ProviderMetadata(interactionId, serviceTier, OutputTokensByModality(usage)),
                post.Body,
                responseHeaders: post.Headers,
                request: LanguageModelStreamRequest.FromJson(requestJson)
            );
        }
    }

    private static string DefaultId()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 16);
    }

    // The `google` provider metadata of a result: the interaction id, the applied service tier, and the output token breakdown.
    internal static JsonElement ProviderMetadata(
        string? interactionId,
        string? serviceTier,
        JsonObject? outputTokensByModality
    )
    {
        var google = new JsonObject();
        if (interactionId != null)
        {
            google["interactionId"] = interactionId;
        }

        if (serviceTier != null)
        {
            google["serviceTier"] = serviceTier;
        }

        if (outputTokensByModality != null)
        {
            google["outputTokensByModality"] = outputTokensByModality;
        }

        return JsonSerializer.SerializeToElement(new JsonObject { ["google"] = google });
    }

    // Maps usage.output_tokens_by_modality (a list of { modality, tokens }) to { modality: tokens }, or null when it has no entry.
    internal static JsonObject? OutputTokensByModality(JsonObject? usage)
    {
        if (usage?["output_tokens_by_modality"] is not JsonArray entries)
        {
            return null;
        }

        var result = new JsonObject();
        foreach (var entry in entries)
        {
            if (
                entry is JsonObject item
                && GoogleInteractionsStreaming.String(item, "modality") is { } modality
                && item["tokens"] is JsonValue tokens
            )
            {
                result[modality] = tokens.DeepClone();
            }
        }

        return result.Count > 0 ? result : null;
    }

    // Builds the request body. An agent call sends `agent` and no generation_config, because the API rejects it.
    private (JsonObject Body, IReadOnlyList<CallWarning> Warnings) Prepare(
        LanguageModelCallOptions options
    )
    {
        var warnings = new List<CallWarning>();
        var thinkingLevel = EnumOption(
            options,
            "thinkingLevel",
            "minimal",
            "low",
            "medium",
            "high",
            "xhigh"
        );
        var reasoningWarnings = new List<ModelWarning>();
        var thinkingSummaries = EnumOption(options, "thinkingSummaries", "auto", "none");
        var serviceTier = EnumOption(options, "serviceTier", "flex", "standard", "priority");
        var previousInteractionId = StringOption(options, "previousInteractionId");
        var store = Store(options);
        var labels = LabelsOption(options);

        if (!_isAgent)
        {
            if (options.FrequencyPenalty != null)
            {
                warnings.Add(new CallWarning("unsupported", "frequencyPenalty", null));
            }

            if (options.PresencePenalty != null)
            {
                warnings.Add(new CallWarning("unsupported", "presencePenalty", null));
            }
        }

        PrepareGoogleInteractionsToolsResult? preparedTools = null;
        if (options.Tools is { Count: > 0 })
        {
            preparedTools = GoogleInteractionsTools.Prepare(options.Tools, options.ToolChoice);
            warnings.AddRange(preparedTools.ToolWarnings);
        }

        // `response_format` is an array of entries. A JSON call-level response format adds one text entry.
        JsonArray? responseFormat = null;
        if (options.JsonResponse)
        {
            if (_isAgent)
            {
                warnings.Add(
                    new CallWarning(
                        "other",
                        "google.interactions: structured output (responseFormat) is not supported when an agent is set; responseFormat will be ignored."
                    )
                );
            }
            else
            {
                var entry = new JsonObject
                {
                    ["type"] = "text",
                    ["mime_type"] = "application/json",
                };
                if (options.JsonSchema is { } schema)
                {
                    entry["schema"] = JsonNode.Parse(schema.GetRawText());
                }

                responseFormat = new JsonArray { entry };
            }
        }

        // `providerOptions.google.responseFormat` entries are appended in order, with camelCase renamed to snake_case.
        if (GoogleOption(options, "responseFormat") is { ValueKind: JsonValueKind.Array } entries)
        {
            responseFormat ??= new JsonArray();
            foreach (var entry in entries.EnumerateArray())
            {
                if (ResponseFormatEntry(entry) is { } mapped)
                {
                    responseFormat.Add(mapped);
                }
            }

            if (responseFormat.Count == 0)
            {
                responseFormat = null;
            }
        }

        // The deprecated `imageConfig` adds an image entry only when none was supplied, and always warns.
        if (
            !_isAgent
            && GoogleOption(options, "imageConfig")
                is { ValueKind: JsonValueKind.Object } imageConfig
        )
        {
            var alreadyHasImageEntry =
                responseFormat != null
                && responseFormat.Any(entry => (string?)entry?["type"] == "image");
            warnings.Add(
                new CallWarning(
                    "other",
                    alreadyHasImageEntry
                        ? "google.interactions: providerOptions.google.imageConfig is deprecated and was ignored because providerOptions.google.responseFormat already supplies an image entry. Use responseFormat exclusively."
                        : "google.interactions: providerOptions.google.imageConfig is deprecated. Use providerOptions.google.responseFormat with a { type: \"image\", ... } entry instead."
                )
            );
            if (!alreadyHasImageEntry)
            {
                var imageEntry = new JsonObject { ["type"] = "image", ["mime_type"] = "image/png" };
                GoogleJson.Set(imageEntry, "aspect_ratio", Str(imageConfig, "aspectRatio"));
                GoogleJson.Set(imageEntry, "image_size", Str(imageConfig, "imageSize"));
                responseFormat ??= new JsonArray();
                responseFormat.Add(imageEntry);
            }
        }

        var converted = GoogleInteractionsInput.Convert(
            options.Prompt,
            previousInteractionId,
            store,
            StringOption(options, "mediaResolution")
        );
        warnings.AddRange(converted.Warnings);
        var input = new JsonObject { ["input"] = converted.Input };
        var systemInstruction = converted.SystemInstruction;
        var optionSystemInstruction = StringOption(options, "systemInstruction");
        if (systemInstruction != null && optionSystemInstruction != null)
        {
            warnings.Add(
                new CallWarning(
                    "other",
                    "google.interactions: both AI SDK system message and providerOptions.google.systemInstruction were set; using the AI SDK system message."
                )
            );
        }
        else if (systemInstruction == null && optionSystemInstruction != null)
        {
            systemInstruction = optionSystemInstruction;
        }

        GoogleJson.Set(input, "system_instruction", systemInstruction);
        input[_isAgent ? "agent" : "model"] = ModelId;
        GoogleJson.Set(input, "labels", labels);
        GoogleJson.Set(input, "tools", preparedTools?.Tools);
        GoogleJson.Set(input, "response_format", responseFormat);
        if (
            GoogleOption(options, "responseModalities") is
            { ValueKind: JsonValueKind.Array } modalities
        )
        {
            input["response_modalities"] = JsonNode.Parse(modalities.GetRawText());
        }

        GoogleJson.Set(input, "previous_interaction_id", previousInteractionId);
        GoogleJson.Set(input, "service_tier", serviceTier);
        GoogleJson.Set(input, "store", store);
        if (_isAgent)
        {
            var dropped = new List<string>();
            AddIf(dropped, options.Temperature != null, "temperature");
            AddIf(dropped, options.TopP != null, "topP");
            AddIf(dropped, options.TopK != null, "topK");
            AddIf(dropped, options.FrequencyPenalty != null, "frequencyPenalty");
            AddIf(dropped, options.PresencePenalty != null, "presencePenalty");
            AddIf(dropped, options.Seed != null, "seed");
            AddIf(dropped, options.StopSequences is { Count: > 0 }, "stopSequences");
            AddIf(dropped, options.MaxOutputTokens != null, "maxOutputTokens");
            AddIf(dropped, ReasoningMap.IsCustomReasoning(options.Reasoning), "reasoning");
            AddIf(dropped, thinkingLevel != null, "thinkingLevel");
            AddIf(dropped, thinkingSummaries != null, "thinkingSummaries");
            AddIf(dropped, GoogleOption(options, "imageConfig") != null, "imageConfig");
            if (dropped.Count > 0)
            {
                warnings.Add(
                    new CallWarning(
                        "other",
                        "google.interactions: "
                            + string.Join(", ", dropped)
                            + (dropped.Count == 1 ? " is" : " are")
                            + " not supported when an agent is set; use providerOptions.google.agentConfig instead. Dropped from the request body."
                    )
                );
            }
        }
        else
        {
            var generationConfig = new JsonObject();
            GoogleJson.Set(generationConfig, "temperature", options.Temperature);
            GoogleJson.Set(generationConfig, "top_p", options.TopP);
            GoogleJson.Set(generationConfig, "top_k", options.TopK);
            GoogleJson.Set(generationConfig, "seed", options.Seed);
            if (options.StopSequences is { Count: > 0 } stopSequences)
            {
                generationConfig["stop_sequences"] = new JsonArray(
                    stopSequences
                        .Select(sequence => (JsonNode?)JsonValue.Create(sequence))
                        .ToArray()
                );
            }

            GoogleJson.Set(generationConfig, "max_output_tokens", options.MaxOutputTokens);
            string? mappedLevel = thinkingLevel;
            if (thinkingLevel == null && ReasoningMap.IsCustomReasoning(options.Reasoning))
            {
                var effortMap = GoogleThinking
                    .ReasoningEfforts(ModelId)
                    .ToDictionary(level => level, level => level);
                mappedLevel = ReasoningMap.MapReasoningToProviderEffort(
                    options.Reasoning!,
                    effortMap,
                    reasoningWarnings
                );
                warnings.AddRange(
                    reasoningWarnings.Select(ReasoningMap.ToCallWarning).OfType<CallWarning>()
                );
            }

            GoogleJson.Set(generationConfig, "thinking_level", mappedLevel);
            GoogleJson.Set(generationConfig, "thinking_summaries", thinkingSummaries);
            GoogleJson.Set(generationConfig, "tool_choice", preparedTools?.ToolChoice);
            if (generationConfig.Count > 0)
            {
                input["generation_config"] = generationConfig;
            }
        }

        if (_isAgent && GoogleOption(options, "agentConfig") is { } agentConfig)
        {
            GoogleJson.Set(input, "agent_config", AgentConfig(agentConfig));
        }

        if (GoogleOption(options, "environment") is { } environment)
        {
            if (!_isAgent)
            {
                warnings.Add(
                    new CallWarning(
                        "other",
                        "google.interactions: environment is only supported when an agent is set; environment will be omitted from the request body."
                    )
                );
            }
            else
            {
                GoogleJson.Set(input, "environment", Environment(environment));
            }
        }

        if (Background(options))
        {
            input["background"] = true;
        }

        return (input, warnings);
    }

    // One `providerOptions.google.responseFormat` entry as a wire entry. A null or missing field is left out.
    private static JsonObject? ResponseFormatEntry(JsonElement entry)
    {
        if (
            entry.ValueKind != JsonValueKind.Object
            || !entry.TryGetProperty("type", out var typeValue)
            || typeValue.ValueKind != JsonValueKind.String
        )
        {
            return null;
        }

        var type = typeValue.GetString();
        string[] fields;
        switch (type)
        {
            case "text":
                fields = new[] { "mimeType", "schema" };
                break;
            case "image":
                fields = new[] { "mimeType", "aspectRatio", "imageSize" };
                break;
            case "audio":
                fields = new[] { "mimeType" };
                break;
            case "video":
                fields = new[] { "aspectRatio", "resolution", "duration", "delivery", "gcsUri" };
                break;
            default:
                return null;
        }

        var result = new JsonObject { ["type"] = type };
        foreach (var field in fields)
        {
            if (entry.TryGetProperty(field, out var value) && value.ValueKind != JsonValueKind.Null)
            {
                result[ToSnakeCase(field)] = JsonNode.Parse(value.GetRawText());
            }
        }

        return result;
    }

    private static string ToSnakeCase(string name)
    {
        var builder = new System.Text.StringBuilder();
        foreach (var character in name)
        {
            if (char.IsUpper(character))
            {
                builder.Append('_');
                builder.Append(char.ToLowerInvariant(character));
            }
            else
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }

    // `providerOptions.google.agentConfig`: a `deep-research` config renames its fields to snake_case, a `dynamic` config is `{ type: 'dynamic' }`.
    private static JsonObject? AgentConfig(JsonElement config)
    {
        if (config.ValueKind != JsonValueKind.Object)
        {
            throw new AiSdkException("providerOptions.google.agentConfig must be an object.");
        }

        var type =
            config.TryGetProperty("type", out var typeValue)
            && typeValue.ValueKind == JsonValueKind.String
                ? typeValue.GetString()
                : null;
        switch (type)
        {
            case "deep-research":
                var result = new JsonObject { ["type"] = "deep-research" };
                CopyOption(
                    result,
                    "thinking_summaries",
                    config,
                    "thinkingSummaries",
                    "auto",
                    "none"
                );
                CopyOption(result, "visualization", config, "visualization", "off", "auto");
                if (
                    config.TryGetProperty("collaborativePlanning", out var planning)
                    && planning.ValueKind is JsonValueKind.True or JsonValueKind.False
                )
                {
                    result["collaborative_planning"] = planning.GetBoolean();
                }

                return result;
            case "dynamic":
                return new JsonObject { ["type"] = "dynamic" };
            default:
                throw new AiSdkException(
                    "providerOptions.google.agentConfig.type must be one of: dynamic, deep-research."
                );
        }
    }

    private static void CopyOption(
        JsonObject target,
        string key,
        JsonElement source,
        string name,
        params string[] allowed
    )
    {
        if (source.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null)
        {
            var text = value.ValueKind == JsonValueKind.String ? value.GetString() : null;
            if (text == null || Array.IndexOf(allowed, text) < 0)
            {
                throw new AiSdkException(
                    "providerOptions.google.agentConfig."
                        + name
                        + " must be one of: "
                        + string.Join(", ", allowed)
                        + "."
                );
            }

            target[key] = text;
        }
    }

    // `providerOptions.google.environment`: a string is `remote` or an existing environment id; an object provisions a remote sandbox.
    private static JsonNode? Environment(JsonElement environment)
    {
        if (environment.ValueKind == JsonValueKind.String)
        {
            return environment.GetString();
        }

        if (
            environment.ValueKind != JsonValueKind.Object
            || !environment.TryGetProperty("type", out var type)
            || type.ValueKind != JsonValueKind.String
            || type.GetString() != "remote"
        )
        {
            throw new AiSdkException(
                "providerOptions.google.environment must be a string or an object with type \"remote\"."
            );
        }

        var result = new JsonObject { ["type"] = "remote" };
        if (
            environment.TryGetProperty("sources", out var sources)
            && sources.ValueKind == JsonValueKind.Array
        )
        {
            var list = new JsonArray();
            foreach (var source in sources.EnumerateArray())
            {
                var kind = source.ValueKind == JsonValueKind.Object ? Str(source, "type") : null;
                var entry = new JsonObject { ["type"] = kind };
                if (kind == "inline")
                {
                    entry["content"] = Str(source, "content");
                    entry["target"] = Str(source, "target");
                }
                else if (kind is "gcs" or "repository")
                {
                    entry["source"] = Str(source, "source");
                    GoogleJson.Set(entry, "target", Str(source, "target"));
                }
                else
                {
                    throw new AiSdkException(
                        "providerOptions.google.environment.sources[].type must be one of: gcs, repository, inline."
                    );
                }

                list.Add(entry);
            }

            if (list.Count > 0)
            {
                result["sources"] = list;
            }
        }

        if (
            environment.TryGetProperty("network", out var network)
            && network.ValueKind != JsonValueKind.Null
        )
        {
            if (network.ValueKind == JsonValueKind.String && network.GetString() == "disabled")
            {
                result["network"] = "disabled";
            }
            else if (
                network.ValueKind == JsonValueKind.Object
                && network.TryGetProperty("allowlist", out var allowlist)
                && allowlist.ValueKind == JsonValueKind.Array
            )
            {
                var entries = new JsonArray();
                foreach (var item in allowlist.EnumerateArray())
                {
                    var entry = new JsonObject
                    {
                        ["domain"] =
                            item.ValueKind == JsonValueKind.Object ? Str(item, "domain") : null,
                    };
                    if (
                        item.TryGetProperty("transform", out var transform)
                        && transform.ValueKind != JsonValueKind.Null
                    )
                    {
                        entry["transform"] = JsonNode.Parse(transform.GetRawText());
                    }

                    entries.Add(entry);
                }

                result["network"] = new JsonObject { ["allowlist"] = entries };
            }
            else
            {
                throw new AiSdkException(
                    "providerOptions.google.environment.network must be \"disabled\" or an object with an allowlist."
                );
            }
        }

        return result;
    }

    private static string? Str(JsonElement node, string name)
    {
        return
            node.ValueKind == JsonValueKind.Object
            && node.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static void AddIf(List<string> list, bool condition, string name)
    {
        if (condition)
        {
            list.Add(name);
        }
    }

    private static JsonElement? GoogleOption(LanguageModelCallOptions options, string name)
    {
        return
            options.ProviderOptions != null
            && options.ProviderOptions.TryGetValue("google", out var google)
            && google.ValueKind == JsonValueKind.Object
            && google.TryGetProperty(name, out var value)
            && value.ValueKind != JsonValueKind.Null
            ? value
            : null;
    }

    private static string? StringOption(LanguageModelCallOptions options, string name)
    {
        return GoogleOption(options, name) is { } value
            ? value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : throw new AiSdkException("providerOptions.google." + name + " must be a string.")
            : null;
    }

    // `labels` is a record of string values. A null or missing value omits it, and a non-string value fails before any request.
    private static JsonObject? LabelsOption(LanguageModelCallOptions options)
    {
        if (GoogleOption(options, "labels") is not { } value)
        {
            return null;
        }

        if (value.ValueKind != JsonValueKind.Object)
        {
            throw new AiSdkException(
                "providerOptions.google.labels must be an object of string values."
            );
        }

        var labels = new JsonObject();
        foreach (var property in value.EnumerateObject())
        {
            if (property.Value.ValueKind != JsonValueKind.String)
            {
                throw new AiSdkException(
                    "providerOptions.google.labels." + property.Name + " must be a string."
                );
            }

            labels[property.Name] = property.Value.GetString();
        }

        return labels;
    }

    private static string? EnumOption(
        LanguageModelCallOptions options,
        string name,
        params string[] allowed
    )
    {
        var text = StringOption(options, name);
        if (text != null && Array.IndexOf(allowed, text) < 0)
        {
            throw new AiSdkException(
                "providerOptions.google."
                    + name
                    + " must be one of: "
                    + string.Join(", ", allowed)
                    + "."
            );
        }

        return text;
    }

    private static bool? Store(LanguageModelCallOptions options)
    {
        return
            options.ProviderOptions != null
            && options.ProviderOptions.TryGetValue("google", out var google)
            && google.ValueKind == JsonValueKind.Object
            && google.TryGetProperty("store", out var flag)
            && (flag.ValueKind == JsonValueKind.True || flag.ValueKind == JsonValueKind.False)
            ? flag.GetBoolean()
            : null;
    }

    private static bool Background(LanguageModelCallOptions options)
    {
        return options.ProviderOptions != null
            && options.ProviderOptions.TryGetValue("google", out var google)
            && google.ValueKind == JsonValueKind.Object
            && google.TryGetProperty("background", out var flag)
            && flag.ValueKind == JsonValueKind.True;
    }

    private static TimeSpan? PollingTimeout(LanguageModelCallOptions options)
    {
        if (
            options.ProviderOptions != null
            && options.ProviderOptions.TryGetValue("google", out var google)
            && google.ValueKind == JsonValueKind.Object
            && google.TryGetProperty("pollingTimeoutMs", out var value)
            && value.ValueKind != JsonValueKind.Null
        )
        {
            if (
                value.ValueKind != JsonValueKind.Number
                || !value.TryGetInt64(out var milliseconds)
                || milliseconds <= 0
                || milliseconds > int.MaxValue
            )
            {
                throw new AiSdkException(
                    "providerOptions.google.pollingTimeoutMs must be a positive integer of at most 2147483647."
                );
            }

            return TimeSpan.FromMilliseconds(milliseconds);
        }

        return null;
    }

    private async Task<JsonDocument> PollIfNonTerminalAsync(
        JsonDocument first,
        string baseUrl,
        IReadOnlyDictionary<string, string?> headers,
        TimeSpan? timeout,
        CancellationToken cancellationToken
    )
    {
        var status =
            first.RootElement.TryGetProperty("status", out var value)
            && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        if (GoogleInteractionsPolling.IsTerminal(status))
        {
            return first;
        }

        var id =
            first.RootElement.TryGetProperty("id", out var idValue)
            && idValue.ValueKind == JsonValueKind.String
                ? idValue.GetString()
                : null;
        first.Dispose();
        return await GoogleInteractionsPolling
            .PollUntilTerminalAsync(
                _provider.Http,
                baseUrl,
                id,
                headers,
                null,
                null,
                timeout,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    // total_output_tokens is the text count. Thought tokens add to the output total.
    internal static LanguageModelUsage Usage(JsonObject? usage)
    {
        if (usage == null)
        {
            return LanguageModelUsage.Empty;
        }

        var input = Count(usage, "total_input_tokens");
        var output = Count(usage, "total_output_tokens");
        var thought = Count(usage, "total_thought_tokens");
        var cached = Count(usage, "total_cached_tokens");
        int? outputTotal =
            output == null && thought == null ? null : (output ?? 0) + (thought ?? 0);
        int? noCache = input == null ? null : input - (cached ?? 0);
        var raw = JsonSerializer.SerializeToElement(usage);
        return new LanguageModelUsage(
            input,
            outputTotal,
            input == null && outputTotal == null ? null : (input ?? 0) + (outputTotal ?? 0),
            cached,
            null,
            thought,
            raw,
            noCache,
            output
        );
    }

    private static int? Count(JsonObject usage, string name)
    {
        return usage[name] is JsonValue value && value.TryGetValue<int>(out var count)
            ? count
            : null;
    }

    /// <inheritdoc />
    public async Task<LanguageModelStreamResult> DoStreamAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    )
    {
        if (Background(options))
        {
            return await StreamBackgroundAsync(options, cancellationToken).ConfigureAwait(false);
        }

        return await StreamLiveAsync(options, cancellationToken).ConfigureAwait(false);
    }

    // POSTs with `stream: true` and pipes the SSE feed through the transform.
    private async Task<LanguageModelStreamResult> StreamLiveAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    )
    {
        var (body, warnings) = Prepare(options);
        body["stream"] = true;
        var headers = await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false);
        var requestJson = GoogleJson.Write(body);
        var sse = await _provider
            .Http.OpenSseResponseAsync(
                ProviderUtils.ApiKeys.Combine(
                    InteractionsBase(_provider.Options.BaseUrl),
                    "interactions"
                ),
                requestJson,
                headers,
                cancellationToken
            )
            .ConfigureAwait(false);

        // The tier is in the interaction.completed body. The header is the fallback, and the body wins.
        sse.Headers.TryGetValue("x-gemini-service-tier", out var headerServiceTier);
        var transform = new GoogleInteractionsStreamTransform(
            warnings,
            options.IncludeRawChunks,
            _provider.Options.GenerateId,
            headerServiceTier
        );
        return sse.ToStreamResult(
            transform.Pipe(ParseEvents(sse.Events, cancellationToken), cancellationToken),
            requestJson
        );
    }

    private static async IAsyncEnumerable<GoogleInteractionsEvent> ParseEvents(
        IAsyncEnumerable<string> data,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        await foreach (var raw in data.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return new GoogleInteractionsEvent(raw);
        }
    }

    // `background: true` is incompatible with `stream: true` on POST. POST the background call, then stream
    // GET /interactions/{id}?stream=true. A terminal POST response has nothing left to stream, so replay it.
    private async Task<LanguageModelStreamResult> StreamBackgroundAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    )
    {
        var (body, warnings) = Prepare(options);
        var headers = await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false);
        var baseUrl = InteractionsBase(_provider.Options.BaseUrl);
        var requestJson = GoogleJson.Write(body);
        var post = await _provider
            .Http.SendJsonStringAsync(
                HttpMethod.Post,
                ProviderUtils.ApiKeys.Combine(baseUrl, "interactions"),
                requestJson,
                headers,
                cancellationToken
            )
            .ConfigureAwait(false);
        var response =
            (string.IsNullOrWhiteSpace(post.Body) ? null : JsonNode.Parse(post.Body)) as JsonObject
            ?? new JsonObject();
        var id = (string?)response["id"];
        if (string.IsNullOrEmpty(id))
        {
            throw new InvalidOperationException(
                "google.interactions: background POST response did not include an interaction id; cannot stream the result."
            );
        }

        var transform = new GoogleInteractionsStreamTransform(
            warnings,
            options.IncludeRawChunks,
            _provider.Options.GenerateId
        );
        var request = LanguageModelStreamRequest.FromJson(requestJson);
        var postResponse = new LanguageModelStreamResponse(post.Headers);
        if (GoogleInteractionsPolling.IsTerminal((string?)response["status"]))
        {
            return new LanguageModelStreamResult(
                ToAsync(transform.Replay(response)),
                request,
                postResponse
            );
        }

        var events = GoogleInteractionsStreaming.StreamEvents(
            _provider.Http,
            baseUrl,
            id,
            headers,
            GoogleInteractionsStreaming.DefaultMaxRetries,
            GoogleInteractionsStreaming.DefaultRetryDelay,
            cancellationToken
        );
        return new LanguageModelStreamResult(
            transform.Pipe(events, cancellationToken),
            request,
            postResponse
        );
    }

    private static async IAsyncEnumerable<LanguageModelStreamPart> ToAsync(
        IEnumerable<LanguageModelStreamPart> parts
    )
    {
        foreach (var part in parts)
        {
            yield return part;
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }

    private static string InteractionsBase(string? baseUrl)
    {
        var value = baseUrl ?? string.Empty;
        const string suffix = "/publishers/google";
        if (value.EndsWith(suffix, StringComparison.Ordinal))
        {
            return value.Substring(0, value.Length - suffix.Length);
        }

        return value;
    }
}
