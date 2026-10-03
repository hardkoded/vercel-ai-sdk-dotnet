// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.HuggingFace;

/// <summary>
/// Hugging Face Responses model. <see cref="OpenAICompatibleProvider.LanguageModel(string)"/> stays on Chat Completions
/// so existing chat clients keep working. Call <see cref="HuggingFaceProvider.ResponsesModel"/> for <c>/responses</c>.
/// </summary>
public sealed class HuggingFaceResponsesLanguageModel : ILanguageModel
{
    private readonly HuggingFaceProvider _provider;

    /// <summary>Creates a responses model.</summary>
    public HuggingFaceResponsesLanguageModel(HuggingFaceProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
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

        body["stream"] = false;
        var uri = new Uri(_provider.Options.BaseUrl.TrimEnd('/') + "/responses");
        var response = await _provider.PostJsonAsync(uri, body.ToJsonString(), _provider.CreateHeaders(options.Headers), cancellationToken).ConfigureAwait(false);
        LastResponseHeaders = response.Headers;
        var parsed = ReadResponse(response.Body);
        return new LanguageModelGenerateResult(
            parsed.Content,
            FinishReason.Stop,
            parsed.Usage,
            "stop",
            warnings,
            parsed.ResponseId,
            null,
            response.Body,
            parsed.ModelId,
            parsed.Timestamp,
            response.Headers);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        throw new AiSdkException("Hugging Face responses streaming is not implemented.");
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

    private static ParsedResponse ReadResponse(string? json)
    {
        var content = new List<GeneratedContent>();
        var usage = new LanguageModelUsage(null, null, null);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new ParsedResponse(content, usage, null, null, null);
        }

        using var document = JsonDocument.Parse(json!);
        var root = document.RootElement;
        if (root.TryGetProperty("usage", out var usageElement) && usageElement.ValueKind == JsonValueKind.Object)
        {
            int? input = ReadInt(usageElement, "input_tokens");
            int? outputTokens = ReadInt(usageElement, "output_tokens");
            int? total = ReadInt(usageElement, "total_tokens");
            var cacheRead = ReadNestedInt(usageElement, "input_tokens_details", "cached_tokens") ?? 0;
            var reasoning = ReadNestedInt(usageElement, "output_tokens_details", "reasoning_tokens") ?? 0;
            usage = new LanguageModelUsage(input, outputTokens, total, cacheRead, null, reasoning, usageElement.Clone());
        }

        var sourceIndex = 0;
        if (root.TryGetProperty("output", out var output) && output.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in output.EnumerateArray())
            {
                var type = item.TryGetProperty("type", out var typeElement) && typeElement.ValueKind == JsonValueKind.String
                    ? typeElement.GetString()
                    : null;
                if (type == "message" || type == "reasoning")
                {
                    if (!item.TryGetProperty("content", out var parts) || parts.ValueKind != JsonValueKind.Array)
                    {
                        continue;
                    }

                    foreach (var part in parts.EnumerateArray())
                    {
                        if (!part.TryGetProperty("text", out var textElement) || textElement.ValueKind != JsonValueKind.String)
                        {
                            continue;
                        }

                        var text = textElement.GetString() ?? string.Empty;
                        content.Add(type == "reasoning" ? (GeneratedContent)new GeneratedReasoning(text) : new GeneratedText(text));
                        if (part.TryGetProperty("annotations", out var annotations) && annotations.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var annotation in annotations.EnumerateArray())
                            {
                                var annotationType = annotation.TryGetProperty("type", out var annotationTypeElement) ? annotationTypeElement.GetString() : null;
                                var url = annotation.TryGetProperty("url", out var urlElement) ? urlElement.GetString() : null;
                                if (annotationType != "url_citation" || string.IsNullOrEmpty(url))
                                {
                                    continue;
                                }

                                var title = annotation.TryGetProperty("title", out var titleElement) && titleElement.ValueKind == JsonValueKind.String
                                    ? titleElement.GetString()
                                    : null;
                                content.Add(new GeneratedSource("source-" + sourceIndex.ToString(System.Globalization.CultureInfo.InvariantCulture), url!, title));
                                sourceIndex++;
                            }
                        }
                    }
                }
                else if (type == "function_call")
                {
                    var id = item.TryGetProperty("call_id", out var callId) && callId.ValueKind == JsonValueKind.String
                        ? callId.GetString()
                        : null;
                    var name = item.TryGetProperty("name", out var nameElement) && nameElement.ValueKind == JsonValueKind.String
                        ? nameElement.GetString()
                        : null;
                    var arguments = item.TryGetProperty("arguments", out var argumentsElement) && argumentsElement.ValueKind == JsonValueKind.String
                        ? argumentsElement.GetString()
                        : "{}";
                    if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(name))
                    {
                        content.Add(new GeneratedToolCall(id!, name!, arguments ?? "{}"));
                    }
                }
            }
        }

        string? responseId = root.TryGetProperty("id", out var idElement) && idElement.ValueKind == JsonValueKind.String ? idElement.GetString() : null;
        string? modelId = root.TryGetProperty("model", out var modelElement) && modelElement.ValueKind == JsonValueKind.String ? modelElement.GetString() : null;
        DateTimeOffset? timestamp = null;
        var created = ReadInt(root, "created_at");
        if (created is > 0)
        {
            timestamp = DateTimeOffset.FromUnixTimeSeconds(created.Value);
        }

        return new ParsedResponse(content, usage, responseId, modelId, timestamp);
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
        public ParsedResponse(List<GeneratedContent> content, LanguageModelUsage usage, string? responseId, string? modelId, DateTimeOffset? timestamp)
        {
            Content = content;
            Usage = usage;
            ResponseId = responseId;
            ModelId = modelId;
            Timestamp = timestamp;
        }

        public List<GeneratedContent> Content { get; }

        public LanguageModelUsage Usage { get; }

        public string? ResponseId { get; }

        public string? ModelId { get; }

        public DateTimeOffset? Timestamp { get; }
    }
}
