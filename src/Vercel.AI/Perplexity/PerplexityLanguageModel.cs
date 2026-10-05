// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Perplexity;

/// <summary>
/// Perplexity Agent API language model. Generate and stream requests post to <c>/v1/agent</c>.
/// Sonar PDF input, video input, and image or video results have no Agent API equivalent and are not mapped.
/// </summary>
public sealed class PerplexityLanguageModel : ILanguageModel
{
    private readonly PerplexityProvider _provider;

    /// <summary>Creates a language model for an Agent API preset or direct model id.</summary>
    public PerplexityLanguageModel(PerplexityProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => _provider.Name;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        var built = Build(options, stream: false);
        var http = await _provider.Http.SendJsonStringAsync(
            HttpMethod.Post,
            _provider.AgentUri(),
            built.Body.ToJsonString(),
            Headers(options),
            cancellationToken).ConfigureAwait(false);
        var raw = http.Body;
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(string.IsNullOrWhiteSpace(raw) ? "null" : raw);
        }
        catch (JsonException)
        {
            throw new ApiException("Invalid JSON response", 200, raw);
        }

        using (document)
        {
            if (!PerplexityAgent.TryReadResponse(document.RootElement, out var response))
            {
                throw new ApiException("Invalid JSON response", 200, raw);
            }

            if (response.HasError || string.Equals(response.Status, "failed", StringComparison.Ordinal))
            {
                var message = string.IsNullOrEmpty(response.ErrorMessage) ? "Perplexity response failed" : response.ErrorMessage!;
                throw new BadRequestException(message, raw);
            }

            var content = new List<GeneratedContent>();
            var indexes = new Dictionary<string, int>(StringComparer.Ordinal);
            var hasFunctionCall = false;
            foreach (var item in response.Output)
            {
                CollectItem(item, content, indexes, ref hasFunctionCall);
            }

            var finish = PerplexityAgent.MapPerplexityFinishReason(response.Status, response.IncompleteReason, hasFunctionCall, out var rawFinish);
            return new LanguageModelGenerateResult(
                content,
                finish,
                PerplexityAgent.ConvertPerplexityUsage(response.Usage),
                rawFinish,
                built.Warnings,
                response.Id,
                PerplexityAgent.ProviderMetadata(response.Usage),
                raw,
                response.Model,
                DateTimeOffset.FromUnixTimeSeconds(response.CreatedAt),
                http.Headers);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
        LanguageModelCallOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var built = Build(options, stream: true);
        var session = new StreamSession();
        var enumerator = _provider.Http.SendSseAsync(_provider.AgentUri(), built.Body.ToJsonString(), Headers(options), cancellationToken)
            .GetAsyncEnumerator(cancellationToken);
        try
        {
            var more = await enumerator.MoveNextAsync().ConfigureAwait(false);
            yield return new StreamStartStreamPart(built.Warnings);
            while (more)
            {
                foreach (var part in session.Accept(enumerator.Current, options.IncludeRawChunks))
                {
                    yield return part;
                }

                more = await enumerator.MoveNextAsync().ConfigureAwait(false);
            }
        }
        finally
        {
            await enumerator.DisposeAsync().ConfigureAwait(false);
        }

        foreach (var part in session.Flush())
        {
            yield return part;
        }
    }

    private Dictionary<string, string?> Headers(LanguageModelCallOptions options)
    {
        var headers = _provider.CreateHeaders();
        if (options.Headers != null)
        {
            foreach (var pair in options.Headers)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        return headers;
    }

    private PreparedRequest Build(LanguageModelCallOptions options, bool stream)
    {
        var warnings = new List<CallWarning>();
        WarnIfSet(warnings, options.TopK.HasValue, "topK");
        WarnIfSet(warnings, options.FrequencyPenalty.HasValue, "frequencyPenalty");
        WarnIfSet(warnings, options.PresencePenalty.HasValue, "presencePenalty");
        WarnIfSet(warnings, options.StopSequences != null, "stopSequences");
        WarnIfSet(warnings, options.Seed.HasValue, "seed");

        JsonElement? providerOptions = null;
        if (options.ProviderOptions != null && options.ProviderOptions.TryGetValue("perplexity", out var configured))
        {
            providerOptions = configured;
            PerplexityAgent.ValidateProviderOptions(configured);
        }

        var body = providerOptions is { } provided && provided.ValueKind == JsonValueKind.Object
            ? JsonNode.Parse(provided.GetRawText())!.AsObject()
            : new JsonObject();
        JsonNode? providerReasoning = null;
        if (body["reasoning"] is { } reasoningNode)
        {
            providerReasoning = reasoningNode.DeepClone();
        }

        JsonArray? nativeTools = null;
        if (body["tools"] is JsonArray toolsNode)
        {
            nativeTools = toolsNode.DeepClone().AsArray();
        }

        body.Remove("tools");
        body.Remove("reasoning");
        body.Remove("max_output_tokens");
        body.Remove("temperature");
        body.Remove("top_p");
        body.Remove("response_format");
        body.Remove("input");
        body.Remove("stream");

        var selection = PerplexityAgent.GetModelSelection(ModelId);
        if (selection.Preset != null)
        {
            body["preset"] = selection.Preset;
        }

        if (selection.Model != null)
        {
            body["model"] = selection.Model;
        }

        body["input"] = ConvertToPerplexityInput(options.Prompt, warnings);
        if (options.MaxOutputTokens is { } maxOutputTokens)
        {
            body["max_output_tokens"] = maxOutputTokens;
        }

        if (options.Temperature is { } temperature)
        {
            body["temperature"] = temperature;
        }

        if (options.TopP is { } topP)
        {
            body["top_p"] = topP;
        }

        if (providerReasoning != null)
        {
            body["reasoning"] = providerReasoning;
        }
        else
        {
            var effort = PerplexityAgent.MapReasoningEffort(options.Reasoning, warnings);
            if (effort != null)
            {
                body["reasoning"] = new JsonObject { ["effort"] = effort };
            }
        }

        if (options.JsonSchema is { } schema && schema.ValueKind != JsonValueKind.Undefined && schema.ValueKind != JsonValueKind.Null)
        {
            body["response_format"] = new JsonObject
            {
                ["type"] = "json_schema",
                ["json_schema"] = new JsonObject
                {
                    ["name"] = string.IsNullOrEmpty(options.JsonSchemaName) ? "response" : options.JsonSchemaName,
                    ["schema"] = JsonNode.Parse(schema.GetRawText()),
                    ["strict"] = true,
                },
            };
        }

        var tools = PreparePerplexityTools(options.Tools, options.ToolChoice, nativeTools, warnings);
        if (tools.Count > 0)
        {
            body["tools"] = tools;
        }

        if (stream)
        {
            body["stream"] = true;
        }

        return new PreparedRequest(body, warnings);
    }

    private static JsonArray PreparePerplexityTools(
        IReadOnlyList<LanguageModelTool>? tools,
        ToolChoice? toolChoice,
        JsonArray? nativeTools,
        List<CallWarning> warnings)
    {
        var prepared = new JsonArray();
        if (nativeTools != null)
        {
            foreach (var tool in nativeTools)
            {
                if (tool != null)
                {
                    prepared.Add(tool.DeepClone());
                }
            }
        }

        if (tools != null)
        {
            foreach (var tool in tools)
            {
                var function = new JsonObject
                {
                    ["type"] = "function",
                    ["name"] = tool.Name,
                    ["parameters"] = tool.InputSchema.ValueKind == JsonValueKind.Undefined
                        ? JsonNode.Parse("{}")
                        : JsonNode.Parse(tool.InputSchema.GetRawText()),
                };
                if (!string.IsNullOrEmpty(tool.Description))
                {
                    function["description"] = tool.Description;
                }

                if (tool.Strict is { } strict)
                {
                    function["strict"] = strict;
                }

                prepared.Add(function);
            }
        }

        if (toolChoice != null && !string.Equals(toolChoice.Type, "auto", StringComparison.Ordinal))
        {
            warnings.Add(new CallWarning("unsupported", "toolChoice: The Perplexity Agent API currently selects tools automatically."));
        }

        return prepared;
    }

    private static JsonNode ConvertToPerplexityInput(IReadOnlyList<ModelMessage> prompt, List<CallWarning> warnings)
    {
        var input = new JsonArray();
        foreach (var message in prompt)
        {
            switch (message)
            {
                case SystemModelMessage system:
                    input.Add(new JsonObject
                    {
                        ["type"] = "message",
                        ["role"] = "system",
                        ["content"] = system.Content,
                    });
                    break;
                case UserModelMessage user:
                    input.Add(ConvertUser(user));
                    break;
                case AssistantModelMessage assistant:
                    ConvertAssistant(assistant, input, warnings);
                    break;
                case ToolModelMessage tool:
                    input.Add(FunctionOutput(tool.ToolCallId, tool.ToolName, tool.OutputJson, tool.ProviderMetadata));
                    break;
                default:
                    throw new AiSdkException("Unsupported prompt role '" + message.Role + "'.");
            }
        }

        return input;
    }

    private static JsonObject ConvertUser(UserModelMessage user)
    {
        var parts = new JsonArray();
        var textOnly = true;
        foreach (var part in user.Content)
        {
            switch (part)
            {
                case TextContentPart textPart:
                    parts.Add(new JsonObject { ["type"] = "input_text", ["text"] = textPart.Text });
                    break;
                case FileContentPart file:
                    textOnly = false;
                    parts.Add(new JsonObject { ["type"] = "input_image", ["image_url"] = ImageUrl(file) });
                    break;
                default:
                    throw new AiSdkException("Unsupported user content part.");
            }
        }

        if (!textOnly)
        {
            return new JsonObject
            {
                ["type"] = "message",
                ["role"] = "user",
                ["content"] = parts,
            };
        }

        var text = string.Empty;
        foreach (var part in user.Content)
        {
            if (part is TextContentPart textPart)
            {
                text += textPart.Text;
            }
        }

        return new JsonObject
        {
            ["type"] = "message",
            ["role"] = "user",
            ["content"] = text,
        };
    }

    private static void ConvertAssistant(AssistantModelMessage assistant, JsonArray input, List<CallWarning> warnings)
    {
        if (!string.IsNullOrEmpty(assistant.Text))
        {
            input.Add(new JsonObject
            {
                ["type"] = "message",
                ["role"] = "assistant",
                ["content"] = assistant.Text,
            });
        }

        if (!string.IsNullOrEmpty(assistant.Reasoning))
        {
            warnings.Add(new CallWarning("unsupported", "reasoning content in prompt"));
        }

        foreach (var call in assistant.ToolCalls)
        {
            var item = new JsonObject
            {
                ["type"] = "function_call",
                ["call_id"] = call.ToolCallId,
                ["name"] = call.ToolName,
                ["arguments"] = string.IsNullOrEmpty(call.ArgumentsJson) ? "{}" : call.ArgumentsJson,
            };
            var signature = PerplexityAgent.ThoughtSignature(call.ProviderMetadata);
            if (!string.IsNullOrEmpty(signature))
            {
                item["thought_signature"] = signature;
            }

            input.Add(item);
        }
    }

    private static JsonObject FunctionOutput(string callId, string name, string output, JsonElement? metadata)
    {
        var item = new JsonObject
        {
            ["type"] = "function_call_output",
            ["call_id"] = callId,
            ["name"] = name,
            ["output"] = output ?? "null",
        };
        var signature = PerplexityAgent.ThoughtSignature(metadata);
        if (!string.IsNullOrEmpty(signature))
        {
            item["thought_signature"] = signature;
        }

        return item;
    }

    private static string ImageUrl(FileContentPart file)
    {
        var top = TopLevelMediaType(file.MediaType);
        if (!string.Equals(top, "image", StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(top, "video", StringComparison.OrdinalIgnoreCase) || IsPdf(file.MediaType))
            {
                throw new AiSdkException(
                    "The Perplexity Agent API has no equivalent for Sonar PDF or video input (file part media type " + file.MediaType + ").");
            }

            throw new AiSdkException("Unsupported file part media type " + file.MediaType + ".");
        }

        if (file.Data != null)
        {
            return "data:" + DataMediaType(file.MediaType) + ";base64," + Convert.ToBase64String(file.Data);
        }

        if (!string.IsNullOrEmpty(file.Url))
        {
            return file.Url!;
        }

        throw new AiSdkException("Image input requires a URL or inline bytes.");
    }

    private static string TopLevelMediaType(string? mediaType)
    {
        var value = mediaType ?? string.Empty;
        var semi = value.IndexOf(';');
        if (semi >= 0)
        {
            value = value.Substring(0, semi);
        }

        value = value.Trim();
        var slash = value.IndexOf('/');
        return (slash < 0 ? value : value.Substring(0, slash)).Trim();
    }

    private static string DataMediaType(string? mediaType)
    {
        var value = (mediaType ?? string.Empty).Trim();
        var semi = value.IndexOf(';');
        if (semi >= 0)
        {
            value = value.Substring(0, semi).Trim();
        }

        return value.IndexOf('/') < 0 ? "image/png" : value;
    }

    private static bool IsPdf(string? mediaType)
    {
        var value = mediaType ?? string.Empty;
        return value.IndexOf("pdf", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static void CollectItem(AgentItem item, List<GeneratedContent> content, Dictionary<string, int> indexes, ref bool hasFunctionCall)
    {
        if (string.Equals(item.Type, "message", StringComparison.Ordinal))
        {
            foreach (var part in item.Content)
            {
                if (string.Equals(part.Type, "output_text", StringComparison.Ordinal) && part.Text != null)
                {
                    content.Add(new GeneratedText(part.Text));
                }

                foreach (var annotation in part.Annotations)
                {
                    if (!string.IsNullOrEmpty(annotation.Url))
                    {
                        AddSource(content, indexes, PerplexityAgent.CreateAnnotationSource(annotation.Url!, annotation.Title, NewId));
                    }
                }
            }
        }
        else if (string.Equals(item.Type, "search_results", StringComparison.Ordinal))
        {
            foreach (var result in item.Results)
            {
                AddSource(content, indexes, PerplexityAgent.CreateSource(result, NewId));
            }
        }
        else if (string.Equals(item.Type, "fetch_url_results", StringComparison.Ordinal))
        {
            foreach (var result in item.Contents)
            {
                AddSource(content, indexes, PerplexityAgent.CreateFetchSource(result, NewId));
            }
        }
        else if (string.Equals(item.Type, "function_call", StringComparison.Ordinal)
            && item.CallId != null
            && item.Name != null
            && item.Arguments != null)
        {
            hasFunctionCall = true;
            content.Add(new GeneratedToolCall(item.CallId, item.Name, item.Arguments, PerplexityAgent.ToolMetadata(item.Id, item.ThoughtSignature)));
        }
    }

    private static void AddSource(List<GeneratedContent> content, Dictionary<string, int> indexes, GeneratedSource source)
    {
        if (!indexes.TryGetValue(source.Url, out var existing))
        {
            indexes[source.Url] = content.Count;
            content.Add(source);
            return;
        }

        if (content[existing] is GeneratedSource current
            && PerplexityAgent.HasSearchResultId(source)
            && !PerplexityAgent.HasSearchResultId(current))
        {
            content[existing] = source;
        }
    }

    private static void WarnIfSet(List<CallWarning> warnings, bool set, string feature)
    {
        if (set)
        {
            warnings.Add(new CallWarning("unsupported", feature));
        }
    }

    private static string NewId()
    {
        return "src_" + Guid.NewGuid().ToString("N");
    }

    private sealed class PreparedRequest
    {
        public PreparedRequest(JsonObject body, List<CallWarning> warnings)
        {
            Body = body;
            Warnings = warnings;
        }

        public JsonObject Body { get; }

        public List<CallWarning> Warnings { get; }
    }

    private sealed class StreamSession
    {
        private readonly Dictionary<string, TextState> _text = new(StringComparer.Ordinal);
        private readonly List<string> _textOrder = new();
        private readonly HashSet<string> _emittedUrls = new(StringComparer.Ordinal);
        private readonly Dictionary<string, GeneratedSource> _pending = new(StringComparer.Ordinal);
        private readonly List<string> _pendingOrder = new();
        private readonly HashSet<string> _seenCalls = new(StringComparer.Ordinal);
        private string? _reasoningId;
        private bool _metadataSent;
        private bool _hasFunctionCall;
        private FinishReason _finish = FinishReason.Other;
        private string? _rawFinish;
        private JsonElement? _usage;

        public List<LanguageModelStreamPart> Accept(string data, bool includeRaw)
        {
            var parts = new List<LanguageModelStreamPart>();
            if (includeRaw)
            {
                parts.Add(new RawStreamPart(data));
            }

            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(data);
            }
            catch (JsonException)
            {
                Fail(parts, "Invalid JSON response");
                return parts;
            }

            using (document)
            {
                if (!PerplexityAgent.TryReadChunk(document.RootElement, out var chunk))
                {
                    Fail(parts, "Invalid JSON response");
                    return parts;
                }

                switch (chunk.Type)
                {
                    case "response.created":
                    case "response.in_progress":
                        AddMetadata(chunk.Response, parts);
                        break;
                    case "response.output_text.delta":
                        if (chunk.Delta != null)
                        {
                            EmitTextDelta(TextKey(chunk.ItemId, chunk.OutputIndex, chunk.ContentIndex), chunk.Delta, parts);
                        }

                        break;
                    case "response.output_text.done":
                        FinishText(TextKey(chunk.ItemId, chunk.OutputIndex, chunk.ContentIndex), chunk.Text, parts);
                        break;
                    case "response.reasoning.started":
                        if (_reasoningId != null)
                        {
                            parts.Add(new ReasoningEndStreamPart(_reasoningId));
                        }

                        _reasoningId = "reasoning-" + (chunk.SequenceNumber.HasValue
                            ? chunk.SequenceNumber.Value.ToString(CultureInfo.InvariantCulture)
                            : Guid.NewGuid().ToString("N"));
                        parts.Add(new ReasoningStartStreamPart(_reasoningId));
                        EmitThought(chunk.Thought, parts);
                        break;
                    case "response.reasoning.search_queries":
                    case "response.reasoning.fetch_url_queries":
                        EmitThought(chunk.Thought, parts);
                        break;
                    case "response.reasoning.search_results":
                        EmitThought(chunk.Thought, parts);
                        if (chunk.Results != null)
                        {
                            foreach (var result in chunk.Results)
                            {
                                StageSource(PerplexityAgent.CreateSource(result, NewId), parts);
                            }
                        }

                        break;
                    case "response.reasoning.fetch_url_results":
                        EmitThought(chunk.Thought, parts);
                        if (chunk.Contents != null)
                        {
                            foreach (var result in chunk.Contents)
                            {
                                StageSource(PerplexityAgent.CreateFetchSource(result, NewId), parts);
                            }
                        }

                        break;
                    case "response.reasoning.stopped":
                        EmitThought(chunk.Thought, parts);
                        if (_reasoningId != null)
                        {
                            parts.Add(new ReasoningEndStreamPart(_reasoningId));
                            _reasoningId = null;
                        }

                        break;
                    case "response.output_item.done":
                        if (chunk.Item != null)
                        {
                            FinishOutputText(chunk.Item, chunk.OutputIndex, parts);
                            EmitOutputSources(chunk.Item, parts);
                            EmitFunctionCall(chunk.Item, parts);
                        }

                        break;
                    case "response.completed":
                    case "response.incomplete":
                        if (chunk.Response != null)
                        {
                            AddMetadata(chunk.Response, parts);
                            for (var index = 0; index < chunk.Response.Output.Count; index++)
                            {
                                var item = chunk.Response.Output[index];
                                FinishOutputText(item, index, parts);
                                EmitOutputSources(item, parts);
                                EmitFunctionCall(item, parts);
                            }

                            _usage = chunk.Response.Usage;
                            _finish = PerplexityAgent.MapPerplexityFinishReason(chunk.Response.Status, chunk.Response.IncompleteReason, _hasFunctionCall, out _rawFinish);
                        }

                        break;
                    case "response.failed":
                        _finish = FinishReason.Error;
                        _rawFinish = "failed";
                        parts.Add(new ErrorStreamPart(string.IsNullOrEmpty(chunk.ErrorMessage) ? "Perplexity response failed" : chunk.ErrorMessage!));
                        break;
                }
            }

            return parts;
        }

        public List<LanguageModelStreamPart> Flush()
        {
            var parts = new List<LanguageModelStreamPart>();
            foreach (var url in _pendingOrder)
            {
                if (_pending.TryGetValue(url, out var source))
                {
                    parts.Add(ToSourcePart(source));
                }
            }

            if (_reasoningId != null)
            {
                parts.Add(new ReasoningEndStreamPart(_reasoningId));
                _reasoningId = null;
            }

            foreach (var id in _textOrder)
            {
                var state = _text[id];
                if (!state.Ended)
                {
                    state.Ended = true;
                    parts.Add(new TextEndStreamPart(id));
                }
            }

            parts.Add(new FinishStreamPart(_finish, PerplexityAgent.ConvertPerplexityUsage(_usage), _rawFinish, PerplexityAgent.ProviderMetadata(_usage)));
            return parts;
        }

        private void Fail(List<LanguageModelStreamPart> parts, string message)
        {
            _finish = FinishReason.Error;
            _rawFinish = null;
            parts.Add(new ErrorStreamPart(message));
        }

        private void AddMetadata(AgentResponse? response, List<LanguageModelStreamPart> parts)
        {
            if (_metadataSent || response is null)
            {
                return;
            }

            _metadataSent = true;
            parts.Add(new ResponseMetadataStreamPart(response.Id, response.Model, DateTimeOffset.FromUnixTimeSeconds(response.CreatedAt)));
        }

        private void EmitTextDelta(string id, string delta, List<LanguageModelStreamPart> parts)
        {
            if (!_text.TryGetValue(id, out var state))
            {
                state = new TextState();
                _text[id] = state;
                _textOrder.Add(id);
                parts.Add(new TextStartStreamPart(id));
            }

            if (state.Ended)
            {
                return;
            }

            state.Text += delta;
            parts.Add(new TextDeltaStreamPart(id, delta));
        }

        private void FinishText(string id, string? text, List<LanguageModelStreamPart> parts)
        {
            _text.TryGetValue(id, out var state);
            if (state is { Ended: true })
            {
                return;
            }

            var emitted = state?.Text ?? string.Empty;
            if (text != null && PerplexityAgent.StartsWithOrdinal(text, emitted) && text.Length > emitted.Length)
            {
                EmitTextDelta(id, text.Substring(emitted.Length), parts);
            }

            if (_text.TryGetValue(id, out var finalState) && !finalState.Ended)
            {
                finalState.Ended = true;
                parts.Add(new TextEndStreamPart(id));
            }
        }

        private void FinishOutputText(AgentItem item, double? outputIndex, List<LanguageModelStreamPart> parts)
        {
            if (!string.Equals(item.Type, "message", StringComparison.Ordinal))
            {
                return;
            }

            var index = outputIndex.HasValue ? (int)outputIndex.Value : (int?)null;
            for (var contentIndex = 0; contentIndex < item.Content.Count; contentIndex++)
            {
                var part = item.Content[contentIndex];
                if (string.Equals(part.Type, "output_text", StringComparison.Ordinal))
                {
                    FinishText(PerplexityAgent.TextId(item.Id, index, contentIndex), part.Text, parts);
                }
            }
        }

        private static string TextKey(string? itemId, double? outputIndex, double? contentIndex)
        {
            var index = outputIndex.HasValue ? (int)outputIndex.Value : (int?)null;
            int? partIndex = contentIndex.HasValue ? (int)contentIndex.Value : null;
            return PerplexityAgent.TextId(itemId, index, partIndex);
        }

        private void EmitThought(string? thought, List<LanguageModelStreamPart> parts)
        {
            if (_reasoningId != null && thought != null)
            {
                parts.Add(new ReasoningDeltaStreamPart(_reasoningId, thought));
            }
        }

        private void EmitFunctionCall(AgentItem item, List<LanguageModelStreamPart> parts)
        {
            if (!string.Equals(item.Type, "function_call", StringComparison.Ordinal)
                || item.CallId == null
                || item.Name == null
                || item.Arguments == null
                || !_seenCalls.Add(item.CallId))
            {
                return;
            }

            _hasFunctionCall = true;
            parts.Add(new ToolCallStreamPart(item.CallId, item.Name, item.Arguments, PerplexityAgent.ToolMetadata(item.Id, item.ThoughtSignature)));
        }

        private void EmitOutputSources(AgentItem item, List<LanguageModelStreamPart> parts)
        {
            if (string.Equals(item.Type, "message", StringComparison.Ordinal))
            {
                foreach (var part in item.Content)
                {
                    foreach (var annotation in part.Annotations)
                    {
                        if (!string.IsNullOrEmpty(annotation.Url))
                        {
                            StageSource(PerplexityAgent.CreateAnnotationSource(annotation.Url!, annotation.Title, NewId), parts);
                        }
                    }
                }
            }

            foreach (var result in item.Results)
            {
                StageSource(PerplexityAgent.CreateSource(result, NewId), parts);
            }

            foreach (var result in item.Contents)
            {
                StageSource(PerplexityAgent.CreateFetchSource(result, NewId), parts);
            }
        }

        private void StageSource(GeneratedSource source, List<LanguageModelStreamPart> parts)
        {
            if (_emittedUrls.Contains(source.Url))
            {
                return;
            }

            if (PerplexityAgent.HasSearchResultId(source))
            {
                if (_pending.Remove(source.Url))
                {
                    _pendingOrder.Remove(source.Url);
                }

                _emittedUrls.Add(source.Url);
                parts.Add(ToSourcePart(source));
                return;
            }

            if (_pending.ContainsKey(source.Url))
            {
                return;
            }

            _pending[source.Url] = source;
            _pendingOrder.Add(source.Url);
        }

        private static SourceStreamPart ToSourcePart(GeneratedSource source)
        {
            return new SourceStreamPart(source.Id, source.Url, source.Title, source.ProviderMetadata);
        }

        private sealed class TextState
        {
            public string Text { get; set; } = string.Empty;

            public bool Ended { get; set; }
        }
    }
}
