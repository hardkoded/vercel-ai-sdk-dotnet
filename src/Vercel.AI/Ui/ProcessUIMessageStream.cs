// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using Vercel.AI.Util;

namespace Vercel.AI.Ui;

/// <summary>A tool call whose input is still streaming. Maps to an entry of <c>partialToolCalls</c>.</summary>
internal sealed class PartialToolCall
{
    public string Text { get; set; } = string.Empty;

    public string? InputEncoding { get; set; }

    public int Index { get; set; }

    public string ToolName { get; set; } = string.Empty;

    public bool Dynamic { get; set; }

    public string? Title { get; set; }

    public JsonNode? ToolMetadata { get; set; }
}

/// <summary>The state of a UI message while its chunks stream in. Maps to <c>StreamingUIMessageState</c>.</summary>
internal sealed class StreamingUIMessageState
{
    public StreamingUIMessageState(JsonObject message)
    {
        Message = message;
    }

    /// <summary>The message in its JSON form.</summary>
    public JsonObject Message { get; set; }

    public Dictionary<string, JsonObject> ActiveTextParts { get; set; } = new(StringComparer.Ordinal);

    public Dictionary<string, JsonObject> ActiveReasoningParts { get; set; } = new(StringComparer.Ordinal);

    public Dictionary<string, PartialToolCall> PartialToolCalls { get; set; } = new(StringComparer.Ordinal);

    public string? FinishReason { get; set; }
}

/// <summary>Options of <see cref="UpdateMessageJobContext"/>. Maps to <c>UIMessageStreamWriteOptions</c>.</summary>
internal sealed class UIMessageStreamWriteOptions
{
    public bool? UpdateStatus { get; init; }
}

/// <summary>The state and the write callback that an update job receives.</summary>
internal sealed class UpdateMessageJobContext
{
    public UpdateMessageJobContext(StreamingUIMessageState state, Action<UIMessageStreamWriteOptions?> write)
    {
        State = state;
        Write = write;
    }

    public StreamingUIMessageState State { get; }

    public Action<UIMessageStreamWriteOptions?> Write { get; }
}

/// <summary>Options of <see cref="UIMessageStreamProcessing.ProcessUIMessageStream"/>.</summary>
internal sealed class ProcessUIMessageStreamOptions
{
    public ProcessUIMessageStreamOptions(Func<Func<UpdateMessageJobContext, Task>, Task> runUpdateMessageJob, Action<Exception>? onError)
    {
        RunUpdateMessageJob = runUpdateMessageJob;
        OnError = onError;
    }

    public Func<Func<UpdateMessageJobContext, Task>, Task> RunUpdateMessageJob { get; }

    public Action<Exception>? OnError { get; }

    /// <summary>Called for a tool call with an available input, unless the provider executed the tool.</summary>
    public Func<JsonObject, Task>? OnToolCall { get; init; }

    /// <summary>Called for every data chunk, including transient ones.</summary>
    public Action<JsonObject>? OnData { get; init; }

    /// <summary>During resume, a different message id identifies a separate response.</summary>
    public bool ResetStateOnMessageIdChange { get; init; }

    /// <summary>A transport can replay a response from the beginning using the same id.</summary>
    public bool ResetStateOnFirstMessageStart { get; init; }
}

/// <summary>Builds a UI message from a stream of UI message chunks. Maps to <c>process-ui-message-stream.ts</c>.</summary>
internal static class UIMessageStreamProcessing
{
    /// <summary>Maps to <c>createStreamingUIMessageState</c>.</summary>
    public static StreamingUIMessageState CreateStreamingUIMessageState(JsonObject? lastMessage, string messageId)
    {
        var message = (string?)lastMessage?["role"] == "assistant"
            ? lastMessage!
            : new JsonObject { ["id"] = messageId, ["role"] = "assistant", ["parts"] = new JsonArray() };
        var state = new StreamingUIMessageState(message);
        var parts = (JsonArray)message["parts"]!;

        var lastStepStartIndex = -1;
        for (var i = 0; i < parts.Count; i++)
        {
            if (parts[i] is JsonObject part && (string?)part["type"] == "step-start")
            {
                lastStepStartIndex = i;
            }
        }

        var staticToolIndex = 0;
        for (var i = lastStepStartIndex + 1; i < parts.Count; i++)
        {
            if (parts[i] is not JsonObject part || !UIMessageJson.IsToolPart(part))
            {
                continue;
            }

            var index = staticToolIndex;
            if (UIMessageJson.IsStaticToolPart(part))
            {
                staticToolIndex++;
            }

            if ((string?)part["state"] != "input-streaming")
            {
                continue;
            }

            var isDynamic = (string?)part["type"] == "dynamic-tool";
            state.PartialToolCalls[(string)part["toolCallId"]!] = new PartialToolCall
            {
                Text = part["rawInput"] is JsonValue raw && raw.TryGetValue<string>(out var rawText) ? rawText : string.Empty,
                InputEncoding = (string?)part["inputEncoding"],
                Index = index,
                ToolName = isDynamic ? (string?)part["toolName"] ?? string.Empty : ((string?)part["type"] ?? string.Empty).Substring(5),
                Dynamic = isDynamic,
                Title = (string?)part["title"],
                ToolMetadata = part["toolMetadata"]?.DeepClone(),
            };
        }

        return state;
    }

    /// <summary>Applies every chunk of <paramref name="stream"/> to the message state and passes the chunk on. Maps to <c>processUIMessageStream</c>.</summary>
    public static async IAsyncEnumerable<JsonObject> ProcessUIMessageStream(
        IAsyncEnumerable<JsonObject> stream,
        ProcessUIMessageStreamOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var isFirstMessageStart = true;

        await foreach (var chunk in stream.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            await options.RunUpdateMessageJob(async context =>
            {
                var isStart = (string?)chunk["type"] == "start";
                var wasFirstMessageStart = isFirstMessageStart;
                if (isStart)
                {
                    isFirstMessageStart = false;
                }

                await ProcessChunkAsync(chunk, context.State, context.Write, options, wasFirstMessageStart).ConfigureAwait(false);
            }).ConfigureAwait(false);

            yield return chunk;
        }
    }

    private static JsonArray Parts(StreamingUIMessageState state) => (JsonArray)state.Message["parts"]!;

    private static List<JsonObject> GetCurrentStepParts(StreamingUIMessageState state)
    {
        var parts = Parts(state);
        var start = parts.Count - 1;
        while (start >= 0 && (string?)(parts[start] as JsonObject)?["type"] != "step-start")
        {
            start--;
        }

        var result = new List<JsonObject>();
        for (var i = start + 1; i < parts.Count; i++)
        {
            if (parts[i] is JsonObject part)
            {
                result.Add(part);
            }
        }

        return result;
    }

    private static JsonObject GetToolInvocation(StreamingUIMessageState state, string toolCallId)
    {
        var invocation = GetCurrentStepParts(state).Find(p => UIMessageJson.IsToolPart(p) && (string?)p["toolCallId"] == toolCallId);

        if (invocation == null)
        {
            var parts = Parts(state);
            for (var i = parts.Count - 1; i >= 0; i--)
            {
                if (parts[i] is JsonObject part && UIMessageJson.IsToolPart(part) && (string?)part["toolCallId"] == toolCallId)
                {
                    invocation = part;
                    break;
                }
            }
        }

        return invocation ?? throw new UIMessageStreamError(
            "tool-invocation",
            toolCallId,
            $"No tool invocation found for tool call ID \"{toolCallId}\".");
    }

    private static JsonObject GetToolInvocationByApprovalId(StreamingUIMessageState state, string approvalId)
    {
        foreach (var node in Parts(state))
        {
            if (node is JsonObject part && UIMessageJson.IsToolPart(part) && (string?)(part["approval"] as JsonObject)?["id"] == approvalId)
            {
                return part;
            }
        }

        throw new UIMessageStreamError(
            "tool-approval-response",
            approvalId,
            $"No tool invocation found for approval ID \"{approvalId}\".");
    }

    // Copies the listed fields of a chunk that are defined (a JSON null counts as defined).
    private static JsonObject Pick(JsonObject chunk, params string[] keys)
    {
        var picked = new JsonObject();
        foreach (var key in keys)
        {
            if (chunk.ContainsKey(key))
            {
                picked[key] = chunk[key]?.DeepClone();
            }
        }

        return picked;
    }

    private static JsonObject With(JsonObject target, params (string Key, JsonNode? Value)[] values)
    {
        foreach (var (key, value) in values)
        {
            target[key] = value?.DeepClone();
        }

        return target;
    }

    // Maps to updateToolPart and updateDynamicToolPart. The options carry the defined fields only.
    private static void UpdateToolPart(StreamingUIMessageState state, bool dynamic, JsonObject options, JsonObject? existingPart = null)
    {
        var toolCallId = (string)options["toolCallId"]!;
        var toolName = (string)options["toolName"]!;
        var toolState = (string)options["state"]!;
        var part = existingPart ?? GetCurrentStepParts(state).Find(p =>
            (dynamic ? (string?)p["type"] == "dynamic-tool" : UIMessageJson.IsStaticToolPart(p)) && (string?)p["toolCallId"] == toolCallId);

        var isResultState = toolState is "output-available" or "output-error";

        if (part != null)
        {
            part["state"] = toolState;
            if (dynamic)
            {
                part["toolName"] = toolName;
            }
            else
            {
                part["type"] = "tool-" + toolName;
            }

            foreach (var key in new[] { "input", "output", "errorText", "rawInput", "preliminary" })
            {
                part.Remove(key);
                if (options.ContainsKey(key))
                {
                    part[key] = options[key]?.DeepClone();
                }
            }

            part.Remove("inputEncoding");
            if (options["inputEncoding"] != null)
            {
                part["inputEncoding"] = options["inputEncoding"]!.DeepClone();
            }

            if (options.ContainsKey("title"))
            {
                part["title"] = options["title"]?.DeepClone();
            }

            if (options.ContainsKey("toolMetadata"))
            {
                part["toolMetadata"] = options["toolMetadata"]?.DeepClone();
            }

            // once providerExecuted is set, it stays for streaming
            if (options["providerExecuted"] != null)
            {
                part["providerExecuted"] = options["providerExecuted"]!.DeepClone();
            }

            if (options["providerMetadata"] is { } providerMetadata)
            {
                part[isResultState ? "resultProviderMetadata" : "callProviderMetadata"] = providerMetadata.DeepClone();
            }

            return;
        }

        var created = new JsonObject
        {
            ["type"] = dynamic ? "dynamic-tool" : "tool-" + toolName,
            ["toolCallId"] = toolCallId,
            ["state"] = toolState,
        };

        if (dynamic)
        {
            created["toolName"] = toolName;
        }

        foreach (var key in new[] { "title", "toolMetadata", "input", "inputEncoding", "output", "rawInput", "errorText", "providerExecuted", "preliminary" })
        {
            if (options.ContainsKey(key) && (options[key] != null || key is "input" or "output" or "rawInput"))
            {
                created[key] = options[key]?.DeepClone();
            }
        }

        if (options["providerMetadata"] is { } metadata)
        {
            created[isResultState ? "resultProviderMetadata" : "callProviderMetadata"] = metadata.DeepClone();
        }

        Parts(state).Add(created);
    }

    private static JsonNode? MergeObjects(JsonNode? target, JsonNode? source)
    {
        if (target is JsonObject targetObject && source is JsonObject sourceObject)
        {
            var merged = (JsonObject)targetObject.DeepClone();
            foreach (var pair in sourceObject)
            {
                merged[pair.Key] = merged.TryGetPropertyValue(pair.Key, out var existing) && existing is JsonObject && pair.Value is JsonObject
                    ? MergeObjects(existing, pair.Value)
                    : pair.Value?.DeepClone();
            }

            return merged;
        }

        return source?.DeepClone();
    }

    private static void UpdateMessageMetadata(StreamingUIMessageState state, JsonNode? metadata)
    {
        if (metadata == null)
        {
            return;
        }

        state.Message["metadata"] = state.Message["metadata"] != null
            ? MergeObjects(state.Message["metadata"], metadata)
            : metadata.DeepClone();
    }

    private static UIMessageStreamError Missing(string chunkType, string id, string what, string startType)
        => new UIMessageStreamError(
            chunkType,
            id,
            $"Received {chunkType} for missing {what} with ID \"{id}\". Ensure a \"{startType}\" chunk is sent before any \"{chunkType}\" chunks.");

    private static string? String(JsonObject chunk, string key)
        => chunk[key] is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;

    private static bool IsTrue(JsonObject chunk, string key)
        => chunk[key] is JsonValue value && value.TryGetValue<bool>(out var flag) && flag;

    private static async Task ProcessChunkAsync(
        JsonObject chunk,
        StreamingUIMessageState state,
        Action<UIMessageStreamWriteOptions?> write,
        ProcessUIMessageStreamOptions options,
        bool isFirstMessageStart)
    {
        var type = (string?)chunk["type"] ?? string.Empty;
        var parts = Parts(state);
        var id = String(chunk, "id") ?? string.Empty;
        var toolCallId = String(chunk, "toolCallId") ?? string.Empty;

        switch (type)
        {
            case "text-start":
                {
                    var textPart = new JsonObject { ["type"] = "text", ["text"] = string.Empty, ["state"] = "streaming" };
                    if (chunk["providerMetadata"] != null)
                    {
                        textPart["providerMetadata"] = chunk["providerMetadata"]!.DeepClone();
                    }

                    state.ActiveTextParts[id] = textPart;
                    parts.Add(textPart);
                    write(null);
                    break;
                }

            case "text-delta":
                {
                    if (!state.ActiveTextParts.TryGetValue(id, out var textPart))
                    {
                        throw Missing("text-delta", id, "text part", "text-start");
                    }

                    textPart["text"] = ((string?)textPart["text"] ?? string.Empty) + String(chunk, "delta");
                    if (chunk["providerMetadata"] != null)
                    {
                        textPart["providerMetadata"] = chunk["providerMetadata"]!.DeepClone();
                    }

                    write(null);
                    break;
                }

            case "text-end":
                {
                    if (!state.ActiveTextParts.TryGetValue(id, out var textPart))
                    {
                        throw Missing("text-end", id, "text part", "text-start");
                    }

                    textPart["state"] = "done";
                    if (chunk["citations"] != null)
                    {
                        textPart["citations"] = chunk["citations"]!.DeepClone();
                    }

                    if (chunk["providerMetadata"] != null)
                    {
                        textPart["providerMetadata"] = chunk["providerMetadata"]!.DeepClone();
                    }

                    state.ActiveTextParts.Remove(id);
                    write(null);
                    break;
                }

            case "custom":
                {
                    var customPart = new JsonObject { ["type"] = "custom", ["kind"] = String(chunk, "kind") };
                    if (chunk["providerMetadata"] != null)
                    {
                        customPart["providerMetadata"] = chunk["providerMetadata"]!.DeepClone();
                    }

                    parts.Add(customPart);
                    write(null);
                    break;
                }

            case "reasoning-start":
                {
                    var reasoningPart = new JsonObject { ["type"] = "reasoning", ["id"] = id, ["text"] = string.Empty, ["state"] = "streaming" };
                    if (chunk["providerMetadata"] != null)
                    {
                        reasoningPart["providerMetadata"] = chunk["providerMetadata"]!.DeepClone();
                    }

                    state.ActiveReasoningParts[id] = reasoningPart;
                    parts.Add(reasoningPart);
                    write(null);
                    break;
                }

            case "reasoning-delta":
                {
                    if (!state.ActiveReasoningParts.TryGetValue(id, out var reasoningPart))
                    {
                        throw Missing("reasoning-delta", id, "reasoning part", "reasoning-start");
                    }

                    reasoningPart["text"] = ((string?)reasoningPart["text"] ?? string.Empty) + String(chunk, "delta");
                    if (chunk["providerMetadata"] != null)
                    {
                        reasoningPart["providerMetadata"] = chunk["providerMetadata"]!.DeepClone();
                    }

                    write(null);
                    break;
                }

            case "reasoning-end":
                {
                    if (!state.ActiveReasoningParts.TryGetValue(id, out var reasoningPart))
                    {
                        throw Missing("reasoning-end", id, "reasoning part", "reasoning-start");
                    }

                    if (chunk["providerMetadata"] != null)
                    {
                        reasoningPart["providerMetadata"] = chunk["providerMetadata"]!.DeepClone();
                    }

                    reasoningPart["state"] = "done";
                    state.ActiveReasoningParts.Remove(id);
                    write(null);
                    break;
                }

            case "file":
            case "reasoning-file":
                {
                    var filePart = new JsonObject { ["type"] = type, ["mediaType"] = String(chunk, "mediaType"), ["url"] = String(chunk, "url") };
                    if (chunk["providerMetadata"] != null)
                    {
                        filePart["providerMetadata"] = chunk["providerMetadata"]!.DeepClone();
                    }

                    parts.Add(filePart);
                    write(null);
                    break;
                }

            case "source-url":
                {
                    parts.Add(With(
                        new JsonObject { ["type"] = "source-url" },
                        ("sourceId", chunk["sourceId"]),
                        ("url", chunk["url"]),
                        ("title", chunk["title"]),
                        ("providerMetadata", chunk["providerMetadata"])));
                    write(null);
                    break;
                }

            case "source-document":
                {
                    parts.Add(With(
                        new JsonObject { ["type"] = "source-document" },
                        ("sourceId", chunk["sourceId"]),
                        ("mediaType", chunk["mediaType"]),
                        ("title", chunk["title"]),
                        ("filename", chunk["filename"]),
                        ("providerMetadata", chunk["providerMetadata"])));
                    write(null);
                    break;
                }

            case "tool-input-start":
                {
                    var dynamic = IsTrue(chunk, "dynamic");
                    var toolName = String(chunk, "toolName") ?? string.Empty;
                    state.PartialToolCalls[toolCallId] = new PartialToolCall
                    {
                        Text = string.Empty,
                        InputEncoding = String(chunk, "inputEncoding"),
                        ToolName = toolName,
                        Index = GetCurrentStepParts(state).FindAll(UIMessageJson.IsStaticToolPart).Count,
                        Dynamic = dynamic,
                        Title = String(chunk, "title"),
                        ToolMetadata = chunk["toolMetadata"]?.DeepClone(),
                    };

                    var update = Pick(chunk, "toolCallId", "toolName", "inputEncoding", "providerExecuted", "title", "toolMetadata", "providerMetadata");
                    update["state"] = "input-streaming";
                    UpdateToolPart(state, dynamic, update);
                    write(null);
                    break;
                }

            case "tool-input-delta":
                {
                    if (!state.PartialToolCalls.TryGetValue(toolCallId, out var partialToolCall))
                    {
                        throw Missing("tool-input-delta", toolCallId, "tool call", "tool-input-start");
                    }

                    partialToolCall.Text += String(chunk, "inputTextDelta");

                    JsonNode? partialArgs;
                    if (partialToolCall.InputEncoding == "text")
                    {
                        partialArgs = partialToolCall.Text;
                    }
                    else
                    {
                        partialArgs = (await PartialJson.ParseAsync(partialToolCall.Text).ConfigureAwait(false)).Value;
                    }

                    var update = new JsonObject
                    {
                        ["toolCallId"] = toolCallId,
                        ["toolName"] = partialToolCall.ToolName,
                        ["state"] = "input-streaming",
                        ["input"] = partialArgs?.DeepClone(),
                        ["rawInput"] = partialToolCall.Text,
                    };
                    if (partialToolCall.InputEncoding != null)
                    {
                        update["inputEncoding"] = partialToolCall.InputEncoding;
                    }

                    if (partialToolCall.Title != null)
                    {
                        update["title"] = partialToolCall.Title;
                    }

                    if (partialToolCall.ToolMetadata != null)
                    {
                        update["toolMetadata"] = partialToolCall.ToolMetadata.DeepClone();
                    }

                    UpdateToolPart(state, partialToolCall.Dynamic, update);
                    write(null);
                    break;
                }

            case "tool-input-available":
                {
                    var update = Pick(chunk, "toolCallId", "toolName", "input", "providerExecuted", "providerMetadata", "title", "toolMetadata");
                    update["state"] = "input-available";
                    UpdateToolPart(state, IsTrue(chunk, "dynamic"), update);
                    write(null);

                    // The callback is blocking. Tools that the provider executed are skipped because they already ran.
                    if (options.OnToolCall != null && !IsTrue(chunk, "providerExecuted"))
                    {
                        await options.OnToolCall(chunk).ConfigureAwait(false);
                    }

                    break;
                }

            case "tool-input-error":
                {
                    // When a part already exists for this toolCallId (for example from tool-input-start), honour its type
                    // so the part updates in place instead of creating a duplicate with a mismatched type.
                    var existingPart = GetCurrentStepParts(state).Find(p => UIMessageJson.IsToolPart(p) && (string?)p["toolCallId"] == toolCallId);
                    var isDynamic = existingPart != null ? (string?)existingPart["type"] == "dynamic-tool" : IsTrue(chunk, "dynamic");

                    var update = Pick(chunk, "toolCallId", "toolName", "input", "errorText", "providerExecuted", "providerMetadata", "toolMetadata");
                    update["state"] = "output-error";
                    UpdateToolPart(state, isDynamic, update);
                    write(null);
                    break;
                }

            case "tool-approval-request":
                {
                    var toolInvocation = GetToolInvocation(state, toolCallId);
                    toolInvocation["state"] = "approval-requested";
                    var approval = new JsonObject { ["id"] = chunk["approvalId"]?.DeepClone() };
                    if (chunk["approvalDescriptor"] != null)
                    {
                        approval["descriptor"] = chunk["approvalDescriptor"]!.DeepClone();
                    }

                    if (chunk.ContainsKey("inputSchemaInput"))
                    {
                        approval["inputSchemaInput"] = chunk["inputSchemaInput"]?.DeepClone();
                    }

                    if (chunk["reason"] != null)
                    {
                        approval["requestReason"] = chunk["reason"]!.DeepClone();
                    }

                    if (IsTrue(chunk, "isAutomatic"))
                    {
                        approval["isAutomatic"] = true;
                    }

                    if (chunk["signature"] != null)
                    {
                        approval["signature"] = chunk["signature"]!.DeepClone();
                    }

                    toolInvocation["approval"] = approval;
                    write(null);
                    break;
                }

            case "tool-approval-response":
                {
                    var approvalId = String(chunk, "approvalId") ?? string.Empty;
                    var toolInvocation = GetToolInvocationByApprovalId(state, approvalId);
                    var approval = toolInvocation["approval"] is JsonObject existing
                        ? (JsonObject)existing.DeepClone()
                        : new JsonObject { ["id"] = approvalId };

                    toolInvocation["state"] = "approval-responded";
                    approval["id"] = approvalId;
                    approval["approved"] = chunk["approved"]?.DeepClone();
                    if (chunk["reason"] != null)
                    {
                        approval["reason"] = chunk["reason"]!.DeepClone();
                    }

                    toolInvocation["approval"] = approval;
                    if (chunk["providerExecuted"] != null)
                    {
                        toolInvocation["providerExecuted"] = chunk["providerExecuted"]!.DeepClone();
                    }

                    if (chunk["providerMetadata"] != null)
                    {
                        toolInvocation["callProviderMetadata"] = chunk["providerMetadata"]!.DeepClone();
                    }

                    write(null);
                    break;
                }

            case "tool-output-denied":
                {
                    GetToolInvocation(state, toolCallId)["state"] = "output-denied";
                    write(null);
                    break;
                }

            case "tool-output-available":
            case "tool-output-error":
                {
                    var toolInvocation = GetToolInvocation(state, toolCallId);
                    var isDynamic = (string?)toolInvocation["type"] == "dynamic-tool";
                    var isOutput = type == "tool-output-available";

                    var update = new JsonObject
                    {
                        ["toolCallId"] = toolCallId,
                        ["toolName"] = isDynamic ? (string?)toolInvocation["toolName"] : ((string?)toolInvocation["type"] ?? string.Empty).Substring(5),
                        ["state"] = isOutput ? "output-available" : "output-error",
                    };

                    if (toolInvocation.ContainsKey("input"))
                    {
                        update["input"] = toolInvocation["input"]?.DeepClone();
                    }

                    if (isOutput)
                    {
                        if (chunk.ContainsKey("output"))
                        {
                            update["output"] = chunk["output"]?.DeepClone();
                        }

                        if (chunk.ContainsKey("preliminary"))
                        {
                            update["preliminary"] = chunk["preliminary"]?.DeepClone();
                        }
                    }
                    else
                    {
                        update["errorText"] = chunk["errorText"]?.DeepClone();
                        if (!isDynamic && toolInvocation.ContainsKey("rawInput"))
                        {
                            update["rawInput"] = toolInvocation["rawInput"]?.DeepClone();
                        }
                    }

                    if (chunk["providerExecuted"] != null)
                    {
                        update["providerExecuted"] = chunk["providerExecuted"]!.DeepClone();
                    }

                    if (chunk["providerMetadata"] != null)
                    {
                        update["providerMetadata"] = chunk["providerMetadata"]!.DeepClone();
                    }

                    if (toolInvocation["title"] != null)
                    {
                        update["title"] = toolInvocation["title"]!.DeepClone();
                    }

                    var toolMetadata = chunk["toolMetadata"] ?? toolInvocation["toolMetadata"];
                    if (toolMetadata != null)
                    {
                        update["toolMetadata"] = toolMetadata.DeepClone();
                    }

                    UpdateToolPart(state, isDynamic, update, toolInvocation);
                    write(null);
                    break;
                }

            case "steering-message":
                {
                    parts.Add(With(new JsonObject { ["type"] = "steering-message" }, ("id", chunk["id"]), ("parts", chunk["parts"])));
                    write(null);
                    break;
                }

            case "start-step":
                // add a step boundary part to the message
                parts.Add(new JsonObject { ["type"] = "step-start" });
                break;

            case "finish-step":
                // Active parts are closed by their explicit end chunks. A merged stream's step can finish while another stream's part is active.
                break;

            case "reset-step":
                {
                    var currentStepParts = GetCurrentStepParts(state);

                    state.ActiveTextParts = new Dictionary<string, JsonObject>(StringComparer.Ordinal);
                    state.ActiveReasoningParts = new Dictionary<string, JsonObject>(StringComparer.Ordinal);
                    state.PartialToolCalls = new Dictionary<string, PartialToolCall>(StringComparer.Ordinal);

                    if (currentStepParts.Count > 0)
                    {
                        for (var i = 0; i < currentStepParts.Count; i++)
                        {
                            parts.RemoveAt(parts.Count - 1);
                        }

                        write(null);
                    }

                    break;
                }

            case "start":
                {
                    var messageId = String(chunk, "messageId");
                    var shouldReset = (options.ResetStateOnFirstMessageStart && isFirstMessageStart)
                        || (options.ResetStateOnMessageIdChange && messageId != null && messageId != (string?)state.Message["id"]);

                    if (shouldReset)
                    {
                        // Rebuild a replay or separate response from empty state. Only reset a replay once: merged streams can emit more starts.
                        // Keep the state object for its callers and preserve the id when the replay does not include one.
                        var fresh = CreateStreamingUIMessageState(null, messageId ?? (string?)state.Message["id"] ?? string.Empty);
                        state.Message = fresh.Message;
                        state.ActiveTextParts = fresh.ActiveTextParts;
                        state.ActiveReasoningParts = fresh.ActiveReasoningParts;
                        state.PartialToolCalls = fresh.PartialToolCalls;
                        state.FinishReason = null;
                    }

                    if (messageId != null)
                    {
                        state.Message["id"] = messageId;
                    }

                    UpdateMessageMetadata(state, chunk["messageMetadata"]);

                    if (shouldReset || messageId != null || chunk["messageMetadata"] != null)
                    {
                        write(new UIMessageStreamWriteOptions { UpdateStatus = false });
                    }

                    break;
                }

            case "finish":
                {
                    if (String(chunk, "finishReason") is { } finishReason)
                    {
                        state.FinishReason = finishReason;
                    }

                    UpdateMessageMetadata(state, chunk["messageMetadata"]);
                    if (chunk["messageMetadata"] != null)
                    {
                        write(null);
                    }

                    break;
                }

            case "message-metadata":
                {
                    UpdateMessageMetadata(state, chunk["messageMetadata"]);
                    if (chunk["messageMetadata"] != null)
                    {
                        write(null);
                    }

                    break;
                }

            case "error":
                options.OnError?.Invoke(new Exception(String(chunk, "errorText")));
                break;

            default:
                {
                    if (!type.StartsWith("data-", StringComparison.Ordinal))
                    {
                        break;
                    }

                    // transient parts are not added to the message state
                    if (IsTrue(chunk, "transient"))
                    {
                        options.OnData?.Invoke(chunk);
                        break;
                    }

                    var dataId = String(chunk, "id");
                    JsonObject? existingUiPart = null;
                    if (dataId != null)
                    {
                        foreach (var node in parts)
                        {
                            if (node is JsonObject candidate && (string?)candidate["type"] == type && (string?)candidate["id"] == dataId)
                            {
                                existingUiPart = candidate;
                                break;
                            }
                        }
                    }

                    if (existingUiPart != null)
                    {
                        existingUiPart["data"] = chunk["data"]?.DeepClone();
                    }
                    else
                    {
                        parts.Add(chunk.DeepClone());
                    }

                    options.OnData?.Invoke(chunk);
                    write(null);
                    break;
                }
        }
    }
}
