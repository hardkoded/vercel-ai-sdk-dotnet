// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Cohere;

/// <summary>Cohere v2 chat model.</summary>
public sealed class CohereLanguageModel : ILanguageModel
{
    private const int MaxThinkingBudget = 32768;

    private readonly CohereProvider _provider;

    /// <summary>Creates a model.</summary>
    public CohereLanguageModel(CohereProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => CohereProvider.ProviderName;

    /// <inheritdoc />
    public string ModelId { get; }

    private Uri ChatUri => ApiKeys.Combine(_provider.Options.BaseUrl, "chat");

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        var warnings = new List<CallWarning>();
        var body = Build(options, warnings);
        var response = await _provider.Http.SendJsonStringAsync(HttpMethod.Post, ChatUri, body.ToJsonString(), _provider.Headers(options.Headers), cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body);
        var root = document.RootElement;
        var content = new List<GeneratedContent>();
        if (root.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.Object)
        {
            if (message.TryGetProperty("content", out var parts) && parts.ValueKind == JsonValueKind.Array)
            {
                foreach (var part in parts.EnumerateArray())
                {
                    var type = String(part, "type");
                    if (type == "text" && String(part, "text") is { Length: > 0 } text)
                    {
                        content.Add(new GeneratedText(text));
                    }
                    else if (type == "thinking" && String(part, "thinking") is { Length: > 0 } thinking)
                    {
                        content.Add(new GeneratedReasoning(thinking));
                    }
                }
            }

            if (message.TryGetProperty("tool_calls", out var toolCalls) && toolCalls.ValueKind == JsonValueKind.Array)
            {
                foreach (var call in toolCalls.EnumerateArray())
                {
                    var function = call.GetProperty("function");
                    var arguments = String(function, "arguments") ?? string.Empty;

                    // Cohere returns "null" arguments for tools that take no arguments.
                    content.Add(new GeneratedToolCall(String(call, "id") ?? string.Empty, String(function, "name") ?? string.Empty, arguments == "null" ? "{}" : arguments));
                }
            }
        }

        var finishReason = String(root, "finish_reason");
        var usage = root.TryGetProperty("usage", out var usageElement) ? Usage(usageElement) : new LanguageModelUsage(null, null, null);
        return new LanguageModelGenerateResult(
            content,
            MapFinishReason(finishReason),
            usage,
            finishReason,
            warnings,
            String(root, "generation_id"),
            rawResponse: response.Body,
            responseHeaders: response.Headers);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(LanguageModelCallOptions options, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var warnings = new List<CallWarning>();
        var body = Build(options, warnings);
        body["stream"] = true;
        var events = _provider.Http.SendSseAsync(ChatUri, body.ToJsonString(), _provider.Headers(options.Headers), cancellationToken).GetAsyncEnumerator(cancellationToken);
        try
        {
            // The first read sends the request, so HTTP errors surface before stream-start.
            var hasEvent = await events.MoveNextAsync().ConfigureAwait(false);
            yield return new StreamStartStreamPart(warnings);
            var state = new StreamState(options.IncludeRawChunks);
            while (hasEvent)
            {
                foreach (var part in state.Read(events.Current))
                {
                    yield return part;
                }

                hasEvent = await events.MoveNextAsync().ConfigureAwait(false);
            }

            yield return new FinishStreamPart(state.FinishReason, state.Usage, state.RawFinishReason);
        }
        finally
        {
            await events.DisposeAsync().ConfigureAwait(false);
        }
    }

    private JsonObject Build(LanguageModelCallOptions options, List<CallWarning> warnings)
    {
        var documents = new JsonArray();
        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["frequency_penalty"] = options.FrequencyPenalty,
            ["presence_penalty"] = options.PresencePenalty,
            ["max_tokens"] = options.MaxOutputTokens,
            ["temperature"] = options.Temperature,
            ["p"] = options.TopP,
            ["k"] = options.TopK,
            ["seed"] = options.Seed,
            ["stop_sequences"] = options.StopSequences == null ? null : new JsonArray(options.StopSequences.Select(stop => (JsonNode?)stop).ToArray()),
            ["response_format"] = options.JsonSchema is { } schema
                ? new JsonObject { ["type"] = "json_object", ["json_schema"] = JsonNode.Parse(schema.GetRawText()) }
                : null,
            ["messages"] = Messages(options.Prompt, documents),
        };
        AddTools(body, options);
        if (documents.Count > 0)
        {
            body["documents"] = documents;
        }

        if (Thinking(options, warnings) is { } thinking)
        {
            body["thinking"] = thinking;
        }

        // Unset settings are left out of the request.
        foreach (var name in body.Where(pair => pair.Value is null).Select(pair => pair.Key).ToList())
        {
            body.Remove(name);
        }

        return body;
    }

    private static JsonArray Messages(IReadOnlyList<ModelMessage> prompt, JsonArray documents)
    {
        var messages = new JsonArray();
        foreach (var message in prompt)
        {
            switch (message)
            {
                case SystemModelMessage system:
                    messages.Add(new JsonObject { ["role"] = "system", ["content"] = system.Content });
                    break;
                case UserModelMessage user:
                    messages.Add(new JsonObject { ["role"] = "user", ["content"] = UserContent(user, documents) });
                    break;
                case AssistantModelMessage assistant when assistant.ToolCalls.Count > 0:
                    var toolCalls = new JsonArray();
                    foreach (var call in assistant.ToolCalls)
                    {
                        toolCalls.Add(new JsonObject
                        {
                            ["id"] = call.ToolCallId,
                            ["type"] = "function",
                            ["function"] = new JsonObject { ["name"] = call.ToolName, ["arguments"] = call.ArgumentsJson },
                        });
                    }

                    messages.Add(new JsonObject { ["role"] = "assistant", ["tool_calls"] = toolCalls });
                    break;
                case AssistantModelMessage assistant:
                    messages.Add(new JsonObject { ["role"] = "assistant", ["content"] = assistant.Text ?? string.Empty });
                    break;
                case ToolModelMessage tool:
                    messages.Add(new JsonObject { ["role"] = "tool", ["content"] = tool.OutputJson, ["tool_call_id"] = tool.ToolCallId });
                    break;
            }
        }

        return messages;
    }

    // Images stay inline. Other files are sent as RAG documents, so a message without images is plain text.
    private static JsonNode UserContent(UserModelMessage user, JsonArray documents)
    {
        var parts = new JsonArray();
        var text = new StringBuilder();
        var hasImage = false;
        foreach (var part in user.Content)
        {
            if (part is TextContentPart textPart && textPart.Text.Length > 0)
            {
                parts.Add(new JsonObject { ["type"] = "text", ["text"] = textPart.Text });
                text.Append(textPart.Text);
            }
            else if (part is FileContentPart file && MediaTypes.GetTopLevelMediaType(file.MediaType) == "image")
            {
                hasImage = true;
                var url = file.Url ?? "data:" + MediaTypes.ResolveFullMediaType(file.MediaType, file.Data) + ";base64," + Convert.ToBase64String(file.Data ?? Array.Empty<byte>());
                parts.Add(new JsonObject { ["type"] = "image_url", ["image_url"] = new JsonObject { ["url"] = url } });
            }
            else if (part is FileContentPart document)
            {
                if (document.Url != null)
                {
                    throw new UnsupportedFunctionalityException("File URL data", "URLs should be downloaded by the AI SDK and not reach this point. This indicates a configuration issue.");
                }

                var data = new JsonObject { ["text"] = Encoding.UTF8.GetString(document.Data ?? Array.Empty<byte>()) };
                if (document.FileName != null)
                {
                    data["title"] = document.FileName;
                }

                documents.Add(new JsonObject { ["data"] = data });
            }
        }

        return hasImage ? parts : JsonValue.Create(text.ToString())!;
    }

    private static void AddTools(JsonObject body, LanguageModelCallOptions options)
    {
        if (options.Tools is not { Count: > 0 } tools)
        {
            return;
        }

        var choice = options.ToolChoice;
        var selected = choice is ToolChoice.NamedChoice named ? tools.Where(tool => tool.Name == named.ToolName) : tools;
        var array = new JsonArray();
        foreach (var tool in selected)
        {
            var function = new JsonObject { ["name"] = tool.Name };
            if (tool.Description != null)
            {
                function["description"] = tool.Description;
            }

            function["parameters"] = JsonNode.Parse(tool.InputSchema.GetRawText());
            array.Add(new JsonObject { ["type"] = "function", ["function"] = function });
        }

        body["tools"] = array;
        body["tool_choice"] = choice?.Type switch
        {
            "none" => "NONE",
            "required" or "tool" => "REQUIRED",
            _ => null,
        };
    }

    // Provider options win over the top-level reasoning setting.
    private static JsonObject? Thinking(LanguageModelCallOptions options, List<CallWarning> warnings)
    {
        if (options.ProviderOptions != null
            && options.ProviderOptions.TryGetValue(CohereProvider.ProviderName, out var cohere)
            && cohere.ValueKind == JsonValueKind.Object
            && cohere.TryGetProperty("thinking", out var thinking)
            && thinking.ValueKind == JsonValueKind.Object)
        {
            var result = new JsonObject { ["type"] = String(thinking, "type") ?? "enabled" };
            if (thinking.TryGetProperty("tokenBudget", out var budget) && budget.ValueKind == JsonValueKind.Number)
            {
                result["token_budget"] = budget.GetInt32();
            }

            return result;
        }

        if (!ReasoningMap.IsCustomReasoning(options.Reasoning))
        {
            return null;
        }

        if (options.Reasoning == "none")
        {
            return new JsonObject { ["type"] = "disabled" };
        }

        var modelWarnings = new List<ModelWarning>();
        var tokenBudget = ReasoningMap.MapReasoningToProviderBudget(options.Reasoning!, MaxThinkingBudget, MaxThinkingBudget, modelWarnings);
        foreach (var warning in modelWarnings.OfType<UnsupportedWarning>())
        {
            warnings.Add(new CallWarning(warning.Type, warning.Details ?? warning.Feature));
        }

        return tokenBudget is { } value ? new JsonObject { ["type"] = "enabled", ["token_budget"] = value } : null;
    }

    private static FinishReason MapFinishReason(string? finishReason)
    {
        return finishReason switch
        {
            "COMPLETE" or "STOP_SEQUENCE" => FinishReason.Stop,
            "MAX_TOKENS" => FinishReason.Length,
            "ERROR" => FinishReason.Error,
            "TOOL_CALL" => FinishReason.ToolCalls,
            _ => FinishReason.Other,
        };
    }

    // Input and output come from usage.tokens. The usage object is kept unchanged as raw usage.
    private static LanguageModelUsage Usage(JsonElement usage)
    {
        if (usage.ValueKind != JsonValueKind.Object || !usage.TryGetProperty("tokens", out var tokens))
        {
            throw new TypeValidationException("Cohere usage must contain tokens.", usage.Clone());
        }

        var input = Count(tokens, "input_tokens", usage);
        var output = Count(tokens, "output_tokens", usage);
        if (usage.TryGetProperty("billed_units", out var billed) && billed.ValueKind != JsonValueKind.Null)
        {
            Count(billed, "input_tokens", usage);
            Count(billed, "output_tokens", usage);
        }

        if (usage.TryGetProperty("cached_tokens", out var cached) && cached.ValueKind is not (JsonValueKind.Null or JsonValueKind.Number))
        {
            throw new TypeValidationException("Cohere usage.cached_tokens must be a number.", usage.Clone());
        }

        return new LanguageModelUsage(input, output, null, raw: usage.Clone());
    }

    private static int Count(JsonElement owner, string name, JsonElement usage)
    {
        if (owner.ValueKind != JsonValueKind.Object || !owner.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Number)
        {
            throw new TypeValidationException("Cohere usage " + name + " must be a number.", usage.Clone());
        }

        return value.GetInt32();
    }

    private static string? String(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private sealed class StreamState
    {
        private readonly bool _includeRawChunks;
        private string? _toolCallId;
        private string? _toolName;
        private StringBuilder? _toolArguments;
        private bool _reasoningOpen;

        public StreamState(bool includeRawChunks)
        {
            _includeRawChunks = includeRawChunks;
        }

        public FinishReason FinishReason { get; private set; } = FinishReason.Other;

        public string? RawFinishReason { get; private set; }

        public LanguageModelUsage Usage { get; private set; } = new(null, null, null);

        public List<LanguageModelStreamPart> Read(string data)
        {
            var parts = new List<LanguageModelStreamPart>();
            if (_includeRawChunks)
            {
                parts.Add(new RawStreamPart(data));
            }

            var parsed = JsonParsing.SafeParse(data);
            if (!parsed.Success)
            {
                Fail(parts, parsed.Error!);
                return parts;
            }

            var chunk = parsed.Value!.Value;
            var id = chunk.TryGetProperty("index", out var index) ? index.GetRawText() : string.Empty;
            switch (String(chunk, "type"))
            {
                case "message-start":
                    parts.Add(new ResponseMetadataStreamPart(String(chunk, "id"), null, null));
                    break;
                case "content-start":
                    if (String(Content(chunk), "type") == "thinking")
                    {
                        _reasoningOpen = true;
                        parts.Add(new ReasoningStartStreamPart(id));
                    }
                    else
                    {
                        parts.Add(new TextStartStreamPart(id));
                    }

                    break;
                case "content-delta":
                    var content = Content(chunk);
                    if (String(content, "thinking") is { } thinking)
                    {
                        parts.Add(new ReasoningDeltaStreamPart(id, thinking));
                    }
                    else
                    {
                        parts.Add(new TextDeltaStreamPart(id, String(content, "text") ?? string.Empty));
                    }

                    break;
                case "content-end":
                    parts.Add(_reasoningOpen ? new ReasoningEndStreamPart(id) : new TextEndStreamPart(id));
                    _reasoningOpen = false;
                    break;
                case "tool-call-start":
                    var call = ToolCall(chunk);
                    _toolCallId = String(call, "id");
                    _toolName = String(call.GetProperty("function"), "name");
                    _toolArguments = new StringBuilder(String(call.GetProperty("function"), "arguments"));
                    break;
                case "tool-call-delta":
                    _toolArguments?.Append(String(ToolCall(chunk).GetProperty("function"), "arguments"));
                    break;
                case "tool-call-end":
                    if (_toolArguments != null)
                    {
                        var arguments = _toolArguments.ToString().Trim();
                        var input = SecureJson.Parse(arguments.Length == 0 ? "{}" : arguments);
                        parts.Add(new ToolCallStreamPart(_toolCallId ?? string.Empty, _toolName ?? string.Empty, JsonSerializer.Serialize(input)));
                        _toolArguments = null;
                    }

                    break;
                case "message-end":
                    var delta = chunk.GetProperty("delta");
                    LanguageModelUsage usage;
                    try
                    {
                        usage = CohereLanguageModel.Usage(delta.GetProperty("usage"));
                    }
                    catch (TypeValidationException error)
                    {
                        Fail(parts, error);
                        break;
                    }

                    RawFinishReason = String(delta, "finish_reason");
                    FinishReason = MapFinishReason(RawFinishReason);
                    Usage = usage;
                    break;
            }

            return parts;
        }

        private static JsonElement Content(JsonElement chunk)
        {
            return chunk.GetProperty("delta").GetProperty("message").GetProperty("content");
        }

        private static JsonElement ToolCall(JsonElement chunk)
        {
            return chunk.GetProperty("delta").GetProperty("message").GetProperty("tool_calls");
        }

        private void Fail(List<LanguageModelStreamPart> parts, Exception error)
        {
            FinishReason = FinishReason.Error;
            RawFinishReason = null;
            parts.Add(new ErrorStreamPart(error.Message));
        }
    }
}
