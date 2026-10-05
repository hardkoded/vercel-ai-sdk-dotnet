// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.HuggingFace;

/// <summary>
/// Hugging Face Responses model. <see cref="OpenAICompatibleProvider.LanguageModel(string)"/> stays on Chat Completions
/// so existing chat clients keep working. Call <see cref="HuggingFaceProvider.ResponsesModel"/> for <c>/responses</c>.
/// </summary>
public sealed class HuggingFaceResponsesLanguageModel : ILanguageModel
{
    private readonly HuggingFaceProvider _provider;
    private readonly Func<string> _generateId;

    /// <summary>Creates a responses model. <paramref name="generateId"/> names sources and defaults to random ids.</summary>
    public HuggingFaceResponsesLanguageModel(HuggingFaceProvider provider, string modelId, Func<string>? generateId = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _generateId = generateId ?? IdGenerator.Generate;
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => "huggingface.responses";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>HTTP response headers from the most recent call.</summary>
    public IReadOnlyDictionary<string, string> LastResponseHeaders { get; private set; } = new Dictionary<string, string>();

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        options = options ?? new LanguageModelCallOptions();
        var warnings = new List<CallWarning>();
        var body = BuildBody(options, warnings);
        body["stream"] = false;
        var response = await _provider.PostJsonAsync(ResponsesUri(), body.ToJsonString(), _provider.CreateHeaders(options.Headers), cancellationToken).ConfigureAwait(false);
        LastResponseHeaders = response.Headers;
        var parsed = ReadResponse(response.Body);
        return new LanguageModelGenerateResult(
            parsed.Content,
            MapFinishReason(parsed.IncompleteReason ?? "stop"),
            parsed.Usage,
            parsed.IncompleteReason,
            warnings,
            parsed.ResponseId,
            ResponseMetadata(parsed.ResponseId),
            response.Body,
            parsed.ModelId,
            parsed.Timestamp,
            response.Headers);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
        LanguageModelCallOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        options = options ?? new LanguageModelCallOptions();
        var warnings = new List<CallWarning>();
        var body = BuildBody(options, warnings);
        body["stream"] = true;
        using var sse = await _provider.PostSseAsync(ResponsesUri(), body.ToJsonString(), _provider.CreateHeaders(options.Headers), cancellationToken).ConfigureAwait(false);
        LastResponseHeaders = sse.Headers;
        yield return new StreamStartStreamPart(warnings);
        var state = new StreamState();
        await foreach (var data in sse.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            foreach (var part in state.Read(data))
            {
                yield return part;
            }
        }

        yield return state.Finish();
    }

    private JsonObject BuildBody(LanguageModelCallOptions options, List<CallWarning> warnings)
    {
        if (options.TopK != null)
        {
            warnings.Add(new CallWarning("unsupported", "topK"));
        }

        if (options.Seed != null)
        {
            warnings.Add(new CallWarning("unsupported", "seed"));
        }

        if (options.PresencePenalty != null)
        {
            warnings.Add(new CallWarning("unsupported", "presencePenalty"));
        }

        if (options.FrequencyPenalty != null)
        {
            warnings.Add(new CallWarning("unsupported", "frequencyPenalty"));
        }

        if (options.StopSequences != null && options.StopSequences.Count > 0)
        {
            warnings.Add(new CallWarning("unsupported", "stopSequences"));
        }

        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["input"] = ConvertInput(options.Prompt, warnings),
        };
        if (options.MaxOutputTokens is { } maxTokens)
        {
            body["max_output_tokens"] = maxTokens;
        }

        if (options.Temperature is { } temperature)
        {
            body["temperature"] = temperature;
        }

        if (options.TopP is { } topP)
        {
            body["top_p"] = topP;
        }

        var huggingFace = ReadHuggingFaceOptions(options.ProviderOptions);
        var effort = options.Reasoning;
        if (huggingFace.ReasoningEffort != null)
        {
            effort = huggingFace.ReasoningEffort;
        }

        if (!string.IsNullOrEmpty(effort))
        {
            body["reasoning"] = new JsonObject { ["effort"] = effort };
        }

        if (huggingFace.Metadata != null)
        {
            body["metadata"] = huggingFace.Metadata;
        }

        if (huggingFace.Instructions != null)
        {
            body["instructions"] = huggingFace.Instructions;
        }

        if (options.JsonSchema is { } schema && schema.ValueKind != JsonValueKind.Undefined && schema.ValueKind != JsonValueKind.Null)
        {
            var format = new JsonObject
            {
                ["type"] = "json_schema",
                ["strict"] = huggingFace.StrictJsonSchema,
                ["name"] = options.JsonSchemaName ?? "response",
                ["schema"] = JsonNode.Parse(schema.GetRawText()),
            };
            if (!string.IsNullOrEmpty(huggingFace.Description))
            {
                format["description"] = huggingFace.Description;
            }

            body["text"] = new JsonObject { ["format"] = format };
        }

        if (options.Tools != null && options.Tools.Count > 0)
        {
            var tools = new JsonArray();
            foreach (var tool in options.Tools)
            {
                var function = new JsonObject
                {
                    ["type"] = "function",
                    ["name"] = tool.Name,
                    ["parameters"] = JsonNode.Parse(tool.InputSchema.GetRawText()),
                };
                if (tool.Description != null)
                {
                    function["description"] = tool.Description;
                }

                tools.Add(function);
            }

            body["tools"] = tools;
            if (options.ToolChoice != null && options.ToolChoice.Type != "none")
            {
                body["tool_choice"] = MapToolChoice(options.ToolChoice);
            }
        }

        return body;
    }

    private Uri ResponsesUri()
    {
        return new Uri(_provider.Options.BaseUrl.TrimEnd('/') + "/responses");
    }

    private static JsonArray ConvertInput(IReadOnlyList<ModelMessage> prompt, List<CallWarning> warnings)
    {
        var input = new JsonArray();
        if (prompt == null)
        {
            return input;
        }

        foreach (var message in prompt)
        {
            switch (message)
            {
                case SystemModelMessage system:
                    input.Add(new JsonObject { ["role"] = "system", ["content"] = system.Content });
                    break;
                case UserModelMessage user:
                    input.Add(new JsonObject { ["role"] = "user", ["content"] = UserContent(user) });
                    break;
                case AssistantModelMessage assistant:
                    var assistantText = assistant.Text;
                    if (string.IsNullOrEmpty(assistantText) && !string.IsNullOrEmpty(assistant.Reasoning))
                    {
                        assistantText = assistant.Reasoning;
                    }

                    input.Add(new JsonObject
                    {
                        ["role"] = "assistant",
                        ["content"] = new JsonArray
                        {
                            new JsonObject { ["type"] = "output_text", ["text"] = assistantText ?? string.Empty },
                        },
                    });
                    break;
                case ToolModelMessage _:
                    warnings.Add(new CallWarning("unsupported", "tool messages"));
                    break;
            }
        }

        return input;
    }

    private static JsonArray UserContent(UserModelMessage user)
    {
        var parts = new JsonArray();
        foreach (var part in user.Content)
        {
            if (part is TextContentPart text)
            {
                parts.Add(new JsonObject { ["type"] = "input_text", ["text"] = text.Text });
            }
            else if (part is FileContentPart file)
            {
                if (file.Url == null && file.Data == null)
                {
                    throw new AiSdkException("'file parts with provider references' functionality not supported.");
                }

                var media = file.MediaType ?? string.Empty;
                var slash = media.IndexOf('/');
                var top = slash < 0 ? media : media.Substring(0, slash);
                if (!top.Equals("image", StringComparison.OrdinalIgnoreCase))
                {
                    throw new AiSdkException("'file part media type " + media + "' functionality not supported.");
                }

                string url;
                if (file.Url != null && file.Data == null)
                {
                    url = file.Url;
                }
                else
                {
                    url = "data:" + ResolveImageMedia(media, file.Data) + ";base64," + Convert.ToBase64String(file.Data ?? Array.Empty<byte>());
                }

                parts.Add(new JsonObject { ["type"] = "input_image", ["image_url"] = url });
            }
        }

        return parts;
    }

    private static JsonNode MapToolChoice(ToolChoice toolChoice)
    {
        if (toolChoice.Type == "required")
        {
            return "required";
        }

        if (toolChoice is ToolChoice.NamedChoice named)
        {
            return new JsonObject
            {
                ["type"] = "function",
                ["function"] = new JsonObject { ["name"] = named.ToolName },
            };
        }

        return "auto";
    }

    private static HuggingFaceCallOptions ReadHuggingFaceOptions(IReadOnlyDictionary<string, JsonElement>? providerOptions)
    {
        var result = new HuggingFaceCallOptions();
        if (providerOptions == null || !providerOptions.TryGetValue("huggingface", out var bag) || bag.ValueKind != JsonValueKind.Object)
        {
            return result;
        }

        if (bag.TryGetProperty("strictJsonSchema", out var strictElement) && (strictElement.ValueKind == JsonValueKind.True || strictElement.ValueKind == JsonValueKind.False))
        {
            result.StrictJsonSchema = strictElement.GetBoolean();
        }

        if (bag.TryGetProperty("responseFormatDescription", out var descriptionElement) && descriptionElement.ValueKind == JsonValueKind.String)
        {
            result.Description = descriptionElement.GetString();
        }

        if (bag.TryGetProperty("instructions", out var instructions) && instructions.ValueKind == JsonValueKind.String)
        {
            result.Instructions = instructions.GetString();
        }

        if (bag.TryGetProperty("reasoningEffort", out var effort) && effort.ValueKind == JsonValueKind.String)
        {
            result.ReasoningEffort = effort.GetString();
        }

        if (bag.TryGetProperty("metadata", out var metadata) && metadata.ValueKind == JsonValueKind.Object)
        {
            result.Metadata = JsonNode.Parse(metadata.GetRawText());
        }

        return result;
    }

    private ParsedResponse ReadResponse(string? json)
    {
        var content = new List<GeneratedContent>();
        if (string.IsNullOrWhiteSpace(json))
        {
            return new ParsedResponse(content, ConvertUsage(null), null, null, null, null);
        }

        using var document = JsonDocument.Parse(json!);
        var root = document.RootElement;
        if (root.TryGetProperty("output", out var output) && output.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in output.EnumerateArray())
            {
                switch (ReadString(item, "type"))
                {
                    case "message":
                        foreach (var part in ContentParts(item))
                        {
                            content.Add(new HuggingFaceText(ReadString(part, "text") ?? string.Empty, ItemMetadata(ReadString(item, "id"))));
                            if (part.TryGetProperty("annotations", out var annotations) && annotations.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var annotation in annotations.EnumerateArray())
                                {
                                    var url = ReadString(annotation, "url");
                                    if (ReadString(annotation, "type") == "url_citation" && !string.IsNullOrEmpty(url))
                                    {
                                        content.Add(new GeneratedSource(_generateId(), url!, ReadString(annotation, "title")));
                                    }
                                }
                            }
                        }

                        break;
                    case "reasoning":
                        foreach (var part in ContentParts(item))
                        {
                            content.Add(new HuggingFaceReasoning(ReadString(part, "text") ?? string.Empty, ItemMetadata(ReadString(item, "id"))));
                        }

                        break;
                    case "mcp_call":
                        AddToolCall(content, ReadString(item, "id"), ReadString(item, "name"), ReadString(item, "arguments"), true, OutputResult(item));
                        break;
                    case "mcp_list_tools":
                        var listArguments = new JsonObject { ["server_label"] = ReadString(item, "server_label") }.ToJsonString();
                        JsonElement? tools = null;
                        if (item.TryGetProperty("tools", out var toolsElement) && toolsElement.ValueKind == JsonValueKind.Array)
                        {
                            using var toolsDocument = JsonDocument.Parse(new JsonObject { ["tools"] = JsonNode.Parse(toolsElement.GetRawText()) }.ToJsonString());
                            tools = toolsDocument.RootElement.Clone();
                        }

                        AddToolCall(content, ReadString(item, "id"), "list_tools", listArguments, true, tools);
                        break;
                    case "function_call":
                        AddToolCall(content, ReadString(item, "call_id"), ReadString(item, "name"), ReadString(item, "arguments"), false, OutputResult(item));
                        break;
                }
            }
        }

        DateTimeOffset? timestamp = null;
        var created = ReadInt(root, "created_at");
        if (created is > 0)
        {
            timestamp = DateTimeOffset.FromUnixTimeSeconds(created.Value);
        }

        return new ParsedResponse(
            content,
            ConvertUsage(root.TryGetProperty("usage", out var usage) ? usage : null),
            ReadString(root, "id"),
            ReadString(root, "model"),
            timestamp,
            IncompleteReason(root));
    }

    private static void AddToolCall(List<GeneratedContent> content, string? id, string? name, string? arguments, bool providerExecuted, JsonElement? result)
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
        {
            return;
        }

        content.Add(providerExecuted
            ? new HuggingFaceToolCall(id!, name!, arguments ?? "{}", true)
            : new GeneratedToolCall(id!, name!, arguments ?? "{}"));
        if (result != null)
        {
            content.Add(new HuggingFaceToolResult(id!, name!, result.Value));
        }
    }

    private static IEnumerable<JsonElement> ContentParts(JsonElement item)
    {
        if (!item.TryGetProperty("content", out var parts) || parts.ValueKind != JsonValueKind.Array)
        {
            yield break;
        }

        foreach (var part in parts.EnumerateArray())
        {
            if (part.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
            {
                yield return part;
            }
        }
    }

    /// <summary>The item's non-empty <c>output</c> string, as a JSON value.</summary>
    private static JsonElement? OutputResult(JsonElement item)
    {
        var output = ReadString(item, "output");
        return string.IsNullOrEmpty(output) ? null : JsonSerializer.SerializeToElement(output);
    }

    private static string? IncompleteReason(JsonElement response)
    {
        return response.TryGetProperty("incomplete_details", out var details) && details.ValueKind == JsonValueKind.Object
            ? ReadString(details, "reason")
            : null;
    }

    private static LanguageModelUsage ConvertUsage(JsonElement? usage)
    {
        if (usage is not { ValueKind: JsonValueKind.Object } element)
        {
            return new LanguageModelUsage(null, null, null);
        }

        var input = ReadInt(element, "input_tokens");
        var output = ReadInt(element, "output_tokens");
        var cacheRead = ReadNestedInt(element, "input_tokens_details", "cached_tokens") ?? 0;
        var reasoning = ReadNestedInt(element, "output_tokens_details", "reasoning_tokens") ?? 0;
        return new LanguageModelUsage(
            input,
            output,
            ReadInt(element, "total_tokens"),
            cacheRead,
            null,
            reasoning,
            element.Clone(),
            input - cacheRead,
            output - reasoning);
    }

    private static FinishReason MapFinishReason(string reason)
    {
        return reason switch
        {
            "stop" => FinishReason.Stop,
            "length" => FinishReason.Length,
            "content_filter" => FinishReason.ContentFilter,
            "tool_calls" => FinishReason.ToolCalls,
            "error" => FinishReason.Error,
            _ => FinishReason.Other,
        };
    }

    private static JsonElement ResponseMetadata(string? responseId)
    {
        return HuggingFaceMetadata(new JsonObject { ["responseId"] = responseId });
    }

    private static JsonElement? ItemMetadata(string? itemId)
    {
        return itemId == null ? null : HuggingFaceMetadata(new JsonObject { ["itemId"] = itemId });
    }

    private static JsonElement HuggingFaceMetadata(JsonObject values)
    {
        using var document = JsonDocument.Parse(new JsonObject { ["huggingface"] = values }.ToJsonString());
        return document.RootElement.Clone();
    }

    private static string? ReadString(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }

    private static string ResolveImageMedia(string media, byte[]? data)
    {
        var slash = media.IndexOf('/');
        var subtype = slash < 0 || slash == media.Length - 1 ? string.Empty : media.Substring(slash + 1);
        var needsDetection = slash < 0 || subtype == "*";
        if (!needsDetection)
        {
            return media;
        }

        if (data != null && data.Length >= 8 && data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
        {
            return "image/png";
        }

        if (data != null && data.Length >= 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
        {
            return "image/jpeg";
        }

        if (data != null && data.Length >= 6 && data[0] == (byte)'G' && data[1] == (byte)'I' && data[2] == (byte)'F')
        {
            return "image/gif";
        }

        if (data != null
            && data.Length >= 12
            && data[0] == (byte)'R'
            && data[1] == (byte)'I'
            && data[2] == (byte)'F'
            && data[3] == (byte)'F'
            && data[8] == (byte)'W'
            && data[9] == (byte)'E'
            && data[10] == (byte)'B'
            && data[11] == (byte)'P')
        {
            return "image/webp";
        }

        return media;
    }

    private static int? ReadInt(JsonElement element, string name)
    {
        if (element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
        {
            return number;
        }

        return null;
    }

    private static int? ReadNestedInt(JsonElement element, string parent, string name)
    {
        if (!element.TryGetProperty(parent, out var child) || child.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return ReadInt(child, name);
    }

    private sealed class HuggingFaceCallOptions
    {
        public bool StrictJsonSchema { get; set; }

        public string? Description { get; set; }

        public string? Instructions { get; set; }

        public string? ReasoningEffort { get; set; }

        public JsonNode? Metadata { get; set; }
    }

    private sealed class ParsedResponse
    {
        public ParsedResponse(List<GeneratedContent> content, LanguageModelUsage usage, string? responseId, string? modelId, DateTimeOffset? timestamp, string? incompleteReason)
        {
            Content = content;
            Usage = usage;
            ResponseId = responseId;
            ModelId = modelId;
            Timestamp = timestamp;
            IncompleteReason = incompleteReason;
        }

        public List<GeneratedContent> Content { get; }

        public LanguageModelUsage Usage { get; }

        public string? ResponseId { get; }

        public string? ModelId { get; }

        public DateTimeOffset? Timestamp { get; }

        public string? IncompleteReason { get; }
    }

    private sealed class StreamState
    {
        private FinishReason _finish = FinishReason.Other;
        private string? _rawFinish;
        private string? _responseId;
        private JsonElement? _usage;

        public IEnumerable<LanguageModelStreamPart> Read(string data)
        {
            JsonElement chunk = default;
            var parsed = false;
            try
            {
                using var document = JsonDocument.Parse(data);
                chunk = document.RootElement.Clone();
                parsed = chunk.ValueKind == JsonValueKind.Object && ReadString(chunk, "type") != null;
            }
            catch (JsonException)
            {
            }

            if (!parsed)
            {
                Fail(null);
                yield return new ErrorStreamPart("JSON parsing failed: Text: " + data + ".");
                yield break;
            }

            var type = ReadString(chunk, "type");
            if (TryReadError(chunk, type!, out var message, out var code))
            {
                Fail(code ?? type);
                yield return new ErrorStreamPart(message);
                yield break;
            }

            switch (type)
            {
                case "response.created":
                    var created = chunk.GetProperty("response");
                    _responseId = ReadString(created, "id");
                    var seconds = ReadInt(created, "created_at");
                    yield return new ResponseMetadataStreamPart(
                        _responseId,
                        ReadString(created, "model"),
                        seconds == null ? null : DateTimeOffset.FromUnixTimeSeconds(seconds.Value));
                    break;
                case "response.output_item.added":
                    var added = chunk.GetProperty("item");
                    switch (ReadString(added, "type"))
                    {
                        case "message" when ReadString(added, "role") == "assistant":
                            yield return new TextStartStreamPart(ReadString(added, "id")!, ItemMetadata(ReadString(added, "id")));
                            break;
                        case "function_call":
                            yield return new ToolInputStartStreamPart(ReadString(added, "call_id")!, ReadString(added, "name")!);
                            break;
                        case "reasoning":
                            yield return new ReasoningStartStreamPart(ReadString(added, "id")!, ItemMetadata(ReadString(added, "id")));
                            break;
                    }

                    break;
                case "response.output_item.done":
                    var done = chunk.GetProperty("item");
                    switch (ReadString(done, "type"))
                    {
                        case "message" when ReadString(done, "role") == "assistant":
                            yield return new TextEndStreamPart(ReadString(done, "id")!);
                            break;
                        case "function_call":
                            var callId = ReadString(done, "call_id")!;
                            var name = ReadString(done, "name")!;
                            yield return new ToolInputEndStreamPart(callId);
                            yield return new ToolCallStreamPart(callId, name, ReadString(done, "arguments")!);
                            if (OutputResult(done) is { } result)
                            {
                                yield return new ToolResultStreamPart(callId, name, result);
                            }

                            break;
                    }

                    break;
                case "response.completed":
                    var response = chunk.GetProperty("response");
                    _responseId = ReadString(response, "id");
                    _rawFinish = IncompleteReason(response);
                    _finish = MapFinishReason(_rawFinish ?? "stop");
                    if (response.TryGetProperty("usage", out var usage) && usage.ValueKind == JsonValueKind.Object)
                    {
                        _usage = usage;
                    }

                    break;
                case "response.reasoning_text.delta":
                    yield return new ReasoningDeltaStreamPart(ReadString(chunk, "item_id")!, ReadString(chunk, "delta")!);
                    break;
                case "response.reasoning_text.done":
                    yield return new ReasoningEndStreamPart(ReadString(chunk, "item_id")!);
                    break;
                case "response.output_text.delta":
                    yield return new TextDeltaStreamPart(ReadString(chunk, "item_id")!, ReadString(chunk, "delta")!);
                    break;
            }
        }

        public FinishStreamPart Finish()
        {
            return new FinishStreamPart(_finish, ConvertUsage(_usage), _rawFinish, ResponseMetadata(_responseId));
        }

        /// <summary>Reads an <c>error</c> or <c>response.failed</c> event. The raw finish reason is its code, else its type.</summary>
        private static bool TryReadError(JsonElement chunk, string type, out string message, out string? code)
        {
            message = string.Empty;
            code = null;
            JsonElement details;
            if (type == "response.failed")
            {
                if (!chunk.TryGetProperty("response", out var response)
                    || response.ValueKind != JsonValueKind.Object
                    || !response.TryGetProperty("error", out details))
                {
                    return false;
                }
            }
            else if (type == "error")
            {
                details = chunk.TryGetProperty("error", out var nested) && nested.ValueKind == JsonValueKind.Object ? nested : chunk;
            }
            else
            {
                return false;
            }

            if (details.ValueKind != JsonValueKind.Object || ReadString(details, "message") is not { } text)
            {
                return false;
            }

            message = text;
            if (details.TryGetProperty("code", out var codeElement) && codeElement.ValueKind is JsonValueKind.String or JsonValueKind.Number)
            {
                code = codeElement.ValueKind == JsonValueKind.String ? codeElement.GetString() : codeElement.GetRawText();
            }

            return true;
        }

        private void Fail(string? raw)
        {
            _finish = FinishReason.Error;
            _rawFinish = raw;
        }
    }
}
