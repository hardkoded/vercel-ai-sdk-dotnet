// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAICompatible;

/// <summary>Chat Completions language model.</summary>
public sealed class OpenAICompatibleLanguageModel : ILanguageModel
{
    private static readonly string[] KnownProviderOptionNames =
    {
        "user",
        "reasoningEffort",
        "textVerbosity",
        "strictJsonSchema",
        "responseFormat",
        "responseFormatDescription",
        "structuredOutputs",
    };

    private readonly OpenAICompatibleProvider _provider;
    private readonly string _providerId;

    /// <summary>Creates a language model.</summary>
    public OpenAICompatibleLanguageModel(OpenAICompatibleProvider provider, string modelId)
        : this(provider, modelId, null)
    {
    }

    /// <summary>Creates a language model with an explicit provider id such as <c>anthropic.beta</c>.</summary>
    public OpenAICompatibleLanguageModel(OpenAICompatibleProvider provider, string modelId, string? providerId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _providerId = providerId ?? OpenAICompatibleChat.Qualify(provider.Options.ProviderName, "chat");
        SupportsStructuredOutputs = provider.Options.SupportsStructuredOutputs;
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => _providerId;

    /// <summary>Provider-options key. The segment before the first dot of <see cref="Provider"/>.</summary>
    public string ProviderOptionsName => OpenAICompatibleChat.BaseProviderName(Provider);

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>When true, JSON schema responses use <c>json_schema</c>.</summary>
    public bool SupportsStructuredOutputs { get; set; }

    /// <summary>When set, chat calls use this URL instead of the provider chat route.</summary>
    public Uri? Endpoint { get; set; }

    /// <summary>HTTP response headers from the most recent call.</summary>
    public IReadOnlyDictionary<string, string> LastResponseHeaders { get; private set; } = new Dictionary<string, string>();

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        options = options ?? new LanguageModelCallOptions();
        var prepared = Prepare(options, stream: false);
        var response = await _provider.PostJsonAsync(RequestUri(), prepared.Json, prepared.Headers, cancellationToken).ConfigureAwait(false);
        LastResponseHeaders = response.Headers;
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body);
        var root = document.RootElement;
        if (!root.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array || choices.GetArrayLength() == 0)
        {
            throw new AiSdkException("Response did not contain any choices.");
        }

        var choice = choices[0];
        if (!choice.TryGetProperty("message", out var message) || message.ValueKind != JsonValueKind.Object)
        {
            throw new AiSdkException("Response did not contain any choices.");
        }

        var content = new List<GeneratedContent>();
        if (message.TryGetProperty("content", out var contentElement))
        {
            foreach (var part in OpenAICompatibleChat.ConvertContent(contentElement))
            {
                content.Add(part.Reasoning ? (GeneratedContent)new GeneratedReasoning(part.Text) : new GeneratedText(part.Text));
            }
        }

        var reasoning = PreferredString(message, "reasoning_content", "reasoning");
        if (!string.IsNullOrEmpty(reasoning))
        {
            content.Add(new GeneratedReasoning(reasoning!));
        }

        if (message.TryGetProperty("tool_calls", out var toolCalls) && toolCalls.ValueKind == JsonValueKind.Array)
        {
            foreach (var call in toolCalls.EnumerateArray())
            {
                if (call.ValueKind != JsonValueKind.Object || !call.TryGetProperty("function", out var function))
                {
                    continue;
                }

                var id = call.TryGetProperty("id", out var idElement) && idElement.ValueKind == JsonValueKind.String
                    ? idElement.GetString()
                    : null;
                if (string.IsNullOrEmpty(id))
                {
                    id = JsonValues.GenerateId("call_");
                }

                var signature = ReadWireSignature(call);
                JsonElement? metadata = string.IsNullOrEmpty(signature)
                    ? null
                    : OpenAICompatibleChat.ThoughtSignatureMetadata(prepared.MetadataKey, signature!);
                content.Add(new GeneratedToolCall(
                    id!,
                    function.TryGetProperty("name", out var name) ? name.GetString() ?? string.Empty : string.Empty,
                    function.TryGetProperty("arguments", out var arguments) && arguments.ValueKind == JsonValueKind.String
                        ? arguments.GetString() ?? "{}"
                        : "{}",
                    metadata));
            }
        }

        var raw = choice.TryGetProperty("finish_reason", out var finish) && finish.ValueKind == JsonValueKind.String
            ? finish.GetString()
            : null;
        var usage = root.TryGetProperty("usage", out var usageElement)
            ? OpenAICompatibleChat.ConvertUsage(usageElement)
            : OpenAICompatibleUsage.Missing;
        var metadataElement = OpenAICompatibleChat.ProviderMetadata(prepared.MetadataKey, usage.AcceptedPredictionTokens, usage.RejectedPredictionTokens);
        var finishReason = MapFinish(raw);
        if (prepared.JsonTextOverridesToolCalls && raw == "tool_calls" && content.Any(part => part is GeneratedText { Text.Length: > 0 }))
        {
            // The model can repeat a tool call after valid JSON text. The text is the final answer.
            content.RemoveAll(part => part is GeneratedToolCall);
            finishReason = FinishReason.Stop;
        }

        return new LanguageModelGenerateResult(
            content,
            finishReason,
            ModelUsage(usageElement, usage),
            raw,
            prepared.Warnings,
            ResponseString(root, "id"),
            metadataElement,
            response.Body,
            ResponseString(root, "model"),
            ResponseTimestamp(root),
            response.Headers);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
        LanguageModelCallOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        options = options ?? new LanguageModelCallOptions();
        var prepared = Prepare(options, stream: true);
        var sse = await _provider.PostSseAsync(RequestUri(), prepared.Json, prepared.Headers, cancellationToken).ConfigureAwait(false);
        LastResponseHeaders = sse.Headers;
        yield return new StreamStartStreamPart(prepared.Warnings);

        var toolCalls = new StreamingToolCallTracker();
        var pending = new Dictionary<int, PendingTool>();
        var forwarded = new HashSet<int>();
        var signatures = new Dictionary<string, string>(StringComparer.Ordinal);
        string? finishRaw = null;
        var sawFinish = false;
        JsonElement? usageElement = null;
        var metadataSent = false;
        var textOpen = false;
        var sawText = false;
        var reasoningOpen = false;
        var failed = false;

        await foreach (var data in sse.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            if (options.IncludeRawChunks)
            {
                yield return new RawStreamPart(data);
            }

            JsonNode? node = null;
            var invalidJson = false;
            try
            {
                node = JsonNode.Parse(data);
            }
            catch (JsonException)
            {
                invalidJson = true;
            }

            if (invalidJson)
            {
                failed = true;
                sawFinish = true;
                finishRaw = null;
                yield return new ErrorStreamPart("The provider stream chunk could not be parsed.");
                continue;
            }

            if (node is not JsonObject root)
            {
                failed = true;
                sawFinish = true;
                finishRaw = null;
                yield return new ErrorStreamPart("The provider stream chunk could not be parsed.");
                continue;
            }

            if (root["error"] != null)
            {
                failed = true;
                sawFinish = true;
                finishRaw = null;
                yield return new ErrorStreamPart(ErrorText(root["error"]));
                continue;
            }

            if (!metadataSent)
            {
                var id = AsString(root["id"]);
                var model = AsString(root["model"]);
                var created = ReadInt(root, "created");
                if (created == 0)
                {
                    created = null;
                }

                if (!string.IsNullOrEmpty(id) || !string.IsNullOrEmpty(model) || created != null)
                {
                    metadataSent = true;
                    DateTimeOffset? timestamp = created == null ? null : DateTimeOffset.FromUnixTimeSeconds(created.Value);
                    yield return new ResponseMetadataStreamPart(id, model, timestamp);
                }
            }

            if (root["usage"] is JsonObject usageObject)
            {
                using var usageDocument = JsonDocument.Parse(usageObject.ToJsonString());
                usageElement = usageDocument.RootElement.Clone();
            }

            var choice = root["choices"] is JsonArray { Count: > 0 } choices ? choices[0] as JsonObject : null;
            if (choice == null)
            {
                continue;
            }

            var finishText = AsString(choice["finish_reason"]);
            if (!string.IsNullOrEmpty(finishText))
            {
                finishRaw = finishText;
                sawFinish = true;
            }

            if (choice["delta"] is not JsonObject delta)
            {
                continue;
            }

            var reasoning = PreferredString(delta, "reasoning_content", "reasoning");
            if (!string.IsNullOrEmpty(reasoning))
            {
                foreach (var part in OpenReasoning(ref textOpen, ref reasoningOpen, reasoning!))
                {
                    yield return part;
                }
            }

            if (delta["content"] != null)
            {
                JsonElement contentElement;
                using (var contentDocument = JsonDocument.Parse(delta["content"]!.ToJsonString()))
                {
                    contentElement = contentDocument.RootElement.Clone();
                }

                foreach (var fragment in OpenAICompatibleChat.ConvertContent(contentElement))
                {
                    if (fragment.Reasoning)
                    {
                        foreach (var part in OpenReasoning(ref textOpen, ref reasoningOpen, fragment.Text))
                        {
                            yield return part;
                        }
                    }
                    else
                    {
                        sawText |= fragment.Text.Length > 0;
                        foreach (var part in OpenText(ref textOpen, ref reasoningOpen, fragment.Text))
                        {
                            yield return part;
                        }
                    }
                }
            }

            if (delta["tool_calls"] is JsonArray toolDeltas && toolDeltas.Count > 0)
            {
                if (reasoningOpen)
                {
                    reasoningOpen = false;
                    yield return new ReasoningEndStreamPart("reasoning-0");
                }

                foreach (var item in toolDeltas)
                {
                    if (item is JsonObject tool)
                    {
                        HandleToolDelta(tool, toolCalls, pending, forwarded, signatures);
                    }
                }
            }
        }

        if (reasoningOpen)
        {
            yield return new ReasoningEndStreamPart("reasoning-0");
        }

        if (textOpen)
        {
            yield return new TextEndStreamPart("txt-0");
        }

        foreach (var pair in pending)
        {
            toolCalls.ProcessDelta(new StreamingToolCallDelta(pair.Key, pair.Value.Id, null, pair.Value.Arguments.ToString()));
        }

        pending.Clear();
        var dropToolCalls = prepared.JsonTextOverridesToolCalls && sawText;
        foreach (var call in toolCalls.Flush())
        {
            if (dropToolCalls)
            {
                continue;
            }

            if (signatures.TryGetValue(call.ToolCallId, out var signature))
            {
                yield return new ToolCallStreamPart(
                    call.ToolCallId,
                    call.ToolName,
                    call.ArgumentsJson,
                    OpenAICompatibleChat.ThoughtSignatureMetadata(prepared.MetadataKey, signature));
            }
            else
            {
                yield return call;
            }
        }

        if (!sawFinish)
        {
            failed = true;
            yield return new ErrorStreamPart("Response stream ended without a finish reason.");
        }

        var converted = OpenAICompatibleChat.ConvertUsage(usageElement);
        var finishReason = !sawFinish || (failed && finishRaw == null) ? FinishReason.Error : MapFinish(finishRaw);
        if (dropToolCalls && finishRaw == "tool_calls")
        {
            finishReason = FinishReason.Stop;
        }

        yield return new FinishStreamPart(
            finishReason,
            ModelUsage(usageElement, converted),
            sawFinish ? finishRaw : null,
            OpenAICompatibleChat.ProviderMetadata(prepared.MetadataKey, converted.AcceptedPredictionTokens, converted.RejectedPredictionTokens));
    }

    private PreparedRequest Prepare(LanguageModelCallOptions options, bool stream)
    {
        var warnings = new List<CallWarning>();
        var rawName = ProviderOptionsName;
        if (OpenAICompatibleChat.HasOptions(options.ProviderOptions, "openai-compatible"))
        {
            warnings.Add(new CallWarning(
                "deprecated",
                "providerOptions key 'openai-compatible'. Use 'openaiCompatible' instead."));
        }

        var camel = OpenAICompatibleChat.ToCamelCase(rawName);
        if (!string.Equals(camel, rawName, StringComparison.Ordinal) && OpenAICompatibleChat.HasOptions(options.ProviderOptions, rawName))
        {
            warnings.Add(new CallWarning(
                "deprecated",
                "providerOptions key '" + rawName + "'. Use '" + camel + "' instead."));
        }

        var settings = new ChatSettings();
        ApplyOptions(settings, options.ProviderOptions, "openai-compatible");
        ApplyOptions(settings, options.ProviderOptions, "openaiCompatible");
        ApplyOptions(settings, options.ProviderOptions, rawName);
        if (!string.Equals(camel, rawName, StringComparison.Ordinal))
        {
            ApplyOptions(settings, options.ProviderOptions, camel);
        }

        if (options.TopK != null)
        {
            warnings.Add(new CallWarning("unsupported", "topK"));
        }

        var body = new JsonObject
        {
            ["model"] = ModelId,
        };
        if (!string.IsNullOrEmpty(settings.User))
        {
            body["user"] = settings.User;
        }

        AddSampling(body, options);
        AddResponseFormat(body, options, settings, warnings);
        var jsonResponse = body.ContainsKey("response_format");
        if (options.StopSequences is { Count: > 0 })
        {
            var stop = new JsonArray();
            foreach (var sequence in options.StopSequences)
            {
                stop.Add(sequence);
            }

            body["stop"] = stop;
        }

        if (options.Seed is { } seed)
        {
            body["seed"] = seed;
        }

        foreach (var pair in settings.Extras)
        {
            body[pair.Key] = pair.Value?.DeepClone();
        }

        var reasoningEffort = settings.ReasoningEffort;
        if (reasoningEffort == null && OpenAICompatibleChat.IsCustomReasoning(options.Reasoning))
        {
            reasoningEffort = options.Reasoning;
        }

        if (reasoningEffort != null)
        {
            body["reasoning_effort"] = reasoningEffort;
        }

        if (settings.TextVerbosity != null)
        {
            body["verbosity"] = settings.TextVerbosity;
        }

        var metadataKey = OpenAICompatibleChat.ResolveProviderOptionsKey(rawName, options.ProviderOptions);
        body["messages"] = OpenAICompatibleChat.ConvertMessages(options.Prompt, metadataKey, rawName);
        AddTools(body, options);

        if (stream)
        {
            body["stream"] = true;
            if (_provider.Options.IncludeUsage)
            {
                body["stream_options"] = new JsonObject { ["include_usage"] = true };
            }
        }

        if (_provider.Options.TransformRequestBody != null)
        {
            body = _provider.Options.TransformRequestBody(body, warnings) ?? body;
        }

        return new PreparedRequest(
            body.ToJsonString(),
            _provider.CreateHeaders(options.Headers),
            warnings,
            metadataKey,
            jsonResponse && _provider.Options.JsonTextOverridesToolCalls);
    }

    private void AddResponseFormat(JsonObject body, LanguageModelCallOptions options, ChatSettings settings, List<CallWarning> warnings)
    {
        var wantsJson = string.Equals(settings.ResponseFormat, "json", StringComparison.OrdinalIgnoreCase);
        var hasSchema = options.JsonSchema is { } schema && schema.ValueKind != JsonValueKind.Undefined && schema.ValueKind != JsonValueKind.Null;
        if (!wantsJson && !hasSchema)
        {
            return;
        }

        var structured = settings.StructuredOutputs ?? SupportsStructuredOutputs;
        if (hasSchema && structured)
        {
            var strict = settings.StrictJsonSchema ?? true;
            var schemaObject = new JsonObject
            {
                ["schema"] = JsonNode.Parse(options.JsonSchema!.Value.GetRawText()),
                ["strict"] = strict,
                ["name"] = options.JsonSchemaName ?? "response",
            };
            if (!string.IsNullOrEmpty(settings.ResponseFormatDescription))
            {
                schemaObject["description"] = settings.ResponseFormatDescription;
            }

            body["response_format"] = new JsonObject
            {
                ["type"] = "json_schema",
                ["json_schema"] = schemaObject,
            };
            return;
        }

        if (hasSchema)
        {
            warnings.Add(new CallWarning(
                "unsupported",
                "responseFormat. JSON response format schema is only supported with structuredOutputs."));
        }

        body["response_format"] = new JsonObject { ["type"] = "json_object" };
    }

    private static void AddTools(JsonObject body, LanguageModelCallOptions options)
    {
        if (options.Tools == null || options.Tools.Count == 0)
        {
            return;
        }

        var tools = new JsonArray();
        foreach (var tool in options.Tools)
        {
            var function = new JsonObject
            {
                ["name"] = tool.Name,
                ["parameters"] = JsonNode.Parse(tool.InputSchema.GetRawText()),
            };
            if (tool.Description != null)
            {
                function["description"] = tool.Description;
            }

            if (tool.Strict is { } strict)
            {
                function["strict"] = strict;
            }

            tools.Add(new JsonObject
            {
                ["type"] = "function",
                ["function"] = function,
            });
        }

        body["tools"] = tools;
        if (options.ToolChoice != null)
        {
            body["tool_choice"] = MapToolChoice(options.ToolChoice);
        }
    }

    private static void AddSampling(JsonObject body, LanguageModelCallOptions options)
    {
        if (options.Temperature is { } temperature)
        {
            body["temperature"] = temperature;
        }

        if (options.TopP is { } topP)
        {
            body["top_p"] = topP;
        }

        if (options.MaxOutputTokens is { } maxTokens)
        {
            body["max_tokens"] = maxTokens;
        }

        if (options.PresencePenalty is { } presence)
        {
            body["presence_penalty"] = presence;
        }

        if (options.FrequencyPenalty is { } frequency)
        {
            body["frequency_penalty"] = frequency;
        }
    }

    private static void ApplyOptions(ChatSettings settings, IReadOnlyDictionary<string, JsonElement>? providerOptions, string key)
    {
        if (!OpenAICompatibleChat.HasOptions(providerOptions, key))
        {
            return;
        }

        var bag = providerOptions![key];
        if (bag.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (var property in bag.EnumerateObject())
        {
            switch (property.Name)
            {
                case "user":
                    if (property.Value.ValueKind == JsonValueKind.String)
                    {
                        settings.User = property.Value.GetString();
                    }

                    break;
                case "reasoningEffort":
                    if (property.Value.ValueKind == JsonValueKind.String)
                    {
                        settings.ReasoningEffort = property.Value.GetString();
                    }

                    break;
                case "textVerbosity":
                    if (property.Value.ValueKind == JsonValueKind.String)
                    {
                        settings.TextVerbosity = property.Value.GetString();
                    }

                    break;
                case "strictJsonSchema":
                    if (property.Value.ValueKind == JsonValueKind.True || property.Value.ValueKind == JsonValueKind.False)
                    {
                        settings.StrictJsonSchema = property.Value.GetBoolean();
                    }

                    break;
                case "responseFormat":
                    if (property.Value.ValueKind == JsonValueKind.String)
                    {
                        settings.ResponseFormat = property.Value.GetString();
                    }

                    break;
                case "responseFormatDescription":
                    if (property.Value.ValueKind == JsonValueKind.String)
                    {
                        settings.ResponseFormatDescription = property.Value.GetString();
                    }

                    break;
                case "structuredOutputs":
                    if (property.Value.ValueKind == JsonValueKind.True || property.Value.ValueKind == JsonValueKind.False)
                    {
                        settings.StructuredOutputs = property.Value.GetBoolean();
                    }

                    break;
                default:
                    if (Array.IndexOf(KnownProviderOptionNames, property.Name) < 0)
                    {
                        settings.Extras[property.Name] = JsonNode.Parse(property.Value.GetRawText());
                    }

                    break;
            }
        }
    }

    private static JsonNode MapToolChoice(ToolChoice toolChoice)
    {
        if (toolChoice.Type == "none")
        {
            return "none";
        }

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

    private Uri RequestUri()
    {
        return Endpoint ?? _provider.ChatUri(ModelId);
    }

    private LanguageModelUsage ModelUsage(JsonElement? usage, OpenAICompatibleUsage converted)
    {
        var convert = _provider.Options.ConvertUsage;
        return convert != null && usage is { ValueKind: JsonValueKind.Object } element ? convert(element) : converted.Usage;
    }

    private FinishReason MapFinish(string? raw)
    {
        var mapped = _provider.Options.MapFinishReason?.Invoke(raw);
        return mapped ?? FinishReasons.Parse(raw);
    }

    private static List<LanguageModelStreamPart> OpenReasoning(ref bool textOpen, ref bool reasoningOpen, string delta)
    {
        var parts = new List<LanguageModelStreamPart>();
        if (textOpen)
        {
            textOpen = false;
            parts.Add(new TextEndStreamPart("txt-0"));
        }

        if (!reasoningOpen)
        {
            reasoningOpen = true;
            parts.Add(new ReasoningStartStreamPart("reasoning-0"));
        }

        parts.Add(new ReasoningDeltaStreamPart("reasoning-0", delta));
        return parts;
    }

    private static List<LanguageModelStreamPart> OpenText(ref bool textOpen, ref bool reasoningOpen, string delta)
    {
        var parts = new List<LanguageModelStreamPart>();
        if (reasoningOpen)
        {
            reasoningOpen = false;
            parts.Add(new ReasoningEndStreamPart("reasoning-0"));
        }

        if (!textOpen)
        {
            textOpen = true;
            parts.Add(new TextStartStreamPart("txt-0"));
        }

        parts.Add(new TextDeltaStreamPart("txt-0", delta));
        return parts;
    }

    private static void HandleToolDelta(
        JsonObject tool,
        StreamingToolCallTracker tracker,
        Dictionary<int, PendingTool> pending,
        HashSet<int> forwarded,
        Dictionary<string, string> signatures)
    {
        var delta = ReadToolDelta(tool);
        var signature = ReadWireSignature(tool);
        if (delta.Index == null || forwarded.Contains(delta.Index.Value))
        {
            tracker.ProcessDelta(delta);
            Remember(signatures, delta.Id, signature);
            return;
        }

        var index = delta.Index.Value;
        if (!pending.TryGetValue(index, out var item))
        {
            item = new PendingTool { Id = delta.Id, Signature = signature };
            pending[index] = item;
        }
        else
        {
            if (item.Id == null && delta.Id != null)
            {
                item.Id = delta.Id;
            }

            if (item.Signature == null && signature != null)
            {
                item.Signature = signature;
            }
        }

        if (delta.HasArguments && delta.Arguments != null)
        {
            item.Arguments.Append(delta.Arguments);
        }

        if (!delta.NameIsString)
        {
            return;
        }

        tracker.ProcessDelta(new StreamingToolCallDelta(index, item.Id, delta.Name, item.Arguments.ToString()));
        Remember(signatures, item.Id, item.Signature);
        pending.Remove(index);
        forwarded.Add(index);
    }

    private static void Remember(Dictionary<string, string> signatures, string? id, string? signature)
    {
        if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(signature))
        {
            signatures[id!] = signature!;
        }
    }

    private static string? ReadWireSignature(JsonObject tool)
    {
        if (tool["extra_content"] is JsonObject extra
            && extra["google"] is JsonObject google)
        {
            return AsString(google["thought_signature"]);
        }

        return null;
    }

    private static string? ReadWireSignature(JsonElement call)
    {
        if (call.TryGetProperty("extra_content", out var extra)
            && extra.ValueKind == JsonValueKind.Object
            && extra.TryGetProperty("google", out var google)
            && google.ValueKind == JsonValueKind.Object
            && google.TryGetProperty("thought_signature", out var signature)
            && signature.ValueKind == JsonValueKind.String)
        {
            return signature.GetString();
        }

        return null;
    }

    private static string? PreferredString(JsonObject obj, string preferred, string fallback)
    {
        if (obj.ContainsKey(preferred) && !IsNullNode(obj[preferred]))
        {
            return AsString(obj[preferred]);
        }

        return AsString(obj[fallback]);
    }

    private static string? PreferredString(JsonElement obj, string preferred, string fallback)
    {
        if (obj.TryGetProperty(preferred, out var preferredValue)
            && preferredValue.ValueKind != JsonValueKind.Null
            && preferredValue.ValueKind != JsonValueKind.Undefined)
        {
            return preferredValue.ValueKind == JsonValueKind.String ? preferredValue.GetString() : null;
        }

        if (obj.TryGetProperty(fallback, out var fallbackValue) && fallbackValue.ValueKind == JsonValueKind.String)
        {
            return fallbackValue.GetString();
        }

        return null;
    }

    private static bool IsNullNode(JsonNode? node)
    {
        return node == null || node.GetValueKind() == JsonValueKind.Null;
    }

    private static string ErrorText(JsonNode? error)
    {
        if (error is JsonValue value && value.TryGetValue<string>(out var text) && !string.IsNullOrEmpty(text))
        {
            return text!;
        }

        if (error is JsonObject obj)
        {
            var message = AsString(obj["message"]);
            if (!string.IsNullOrEmpty(message))
            {
                return message!;
            }
        }

        return "Unknown provider error.";
    }

    private static string? ResponseString(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var text = value.GetString();
        return string.IsNullOrEmpty(text) ? null : text;
    }

    private static DateTimeOffset? ResponseTimestamp(JsonElement root)
    {
        if (!root.TryGetProperty("created", out var created) || created.ValueKind != JsonValueKind.Number || !created.TryGetInt64(out var seconds) || seconds == 0)
        {
            return null;
        }

        return DateTimeOffset.FromUnixTimeSeconds(seconds);
    }

    private static string? AsString(JsonNode? node)
    {
        if (node is JsonValue value && value.TryGetValue<string>(out var text))
        {
            return text;
        }

        return null;
    }

    private static int? ReadInt(JsonObject obj, string name)
    {
        if (obj[name] is JsonValue value && value.TryGetValue<int>(out var number))
        {
            return number;
        }

        return null;
    }

    private static StreamingToolCallDelta ReadToolDelta(JsonObject tool)
    {
        int? index = null;
        if (tool["index"] is JsonValue indexValue && indexValue.TryGetValue<int>(out var parsedIndex))
        {
            index = parsedIndex;
        }

        string? name = null;
        string? arguments = null;
        if (tool["function"] is JsonObject function)
        {
            name = AsString(function["name"]);
            if (function["arguments"] is JsonValue argumentValue && argumentValue.TryGetValue<string>(out var argumentText))
            {
                arguments = argumentText;
            }
        }

        return new StreamingToolCallDelta(index, AsString(tool["id"]), name, arguments);
    }

    private sealed class ChatSettings
    {
        public string? User { get; set; }

        public string? ReasoningEffort { get; set; }

        public string? TextVerbosity { get; set; }

        public bool? StrictJsonSchema { get; set; }

        public string? ResponseFormat { get; set; }

        public string? ResponseFormatDescription { get; set; }

        public bool? StructuredOutputs { get; set; }

        public JsonObject Extras { get; } = new();
    }

    private sealed class PendingTool
    {
        public string? Id { get; set; }

        public string? Signature { get; set; }

        public StringBuilder Arguments { get; } = new();
    }

    private sealed class PreparedRequest
    {
        public PreparedRequest(string json, Dictionary<string, string?> headers, List<CallWarning> warnings, string metadataKey, bool jsonTextOverridesToolCalls)
        {
            Json = json;
            Headers = headers;
            Warnings = warnings;
            MetadataKey = metadataKey;
            JsonTextOverridesToolCalls = jsonTextOverridesToolCalls;
        }

        public string Json { get; }

        public Dictionary<string, string?> Headers { get; }

        public List<CallWarning> Warnings { get; }

        public string MetadataKey { get; }

        public bool JsonTextOverridesToolCalls { get; }
    }
}
