// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Perplexity;

/// <summary>
/// Maps Sonar chat-completions usage. Reasoning tokens are reported separately and added
/// to completion tokens for the output total.
/// </summary>
public static class PerplexityChatUsage
{
    /// <summary>Converts a Sonar <c>usage</c> object. A missing object becomes empty usage.</summary>
    public static LanguageModelUsage Convert(JsonElement? usage)
    {
        if (usage is not { } element || element.ValueKind != JsonValueKind.Object)
        {
            return new LanguageModelUsage(null, null, null);
        }

        var prompt = Read(element, "prompt_tokens");
        var completion = Read(element, "completion_tokens");
        var reasoning = 0;
        if (element.TryGetProperty("reasoning_tokens", out var reasoningElement) && reasoningElement.ValueKind == JsonValueKind.Number)
        {
            reasoning = ReadValue(reasoningElement);
        }

        return new LanguageModelUsage(prompt, completion + reasoning, null, null, null, reasoning, element.Clone());
    }

    private static int Read(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number
            ? ReadValue(value)
            : 0;
    }

    private static int ReadValue(JsonElement value)
    {
        if (value.TryGetInt32(out var number))
        {
            return number;
        }

        return (int)value.GetDouble();
    }
}

/// <summary>
/// Result of a Sonar chat stream. Headers are available before <see cref="Parts"/> is read.
/// </summary>
public sealed class PerplexityChatStream
{
    /// <summary>Creates a stream result.</summary>
    public PerplexityChatStream(IReadOnlyDictionary<string, string> headers, IAsyncEnumerable<LanguageModelStreamPart> parts)
    {
        Headers = headers ?? new Dictionary<string, string>();
        Parts = parts ?? throw new ArgumentNullException(nameof(parts));
    }

    /// <summary>HTTP response headers from the chat-completions call.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Model stream parts. Reading them consumes the response body.</summary>
    public IAsyncEnumerable<LanguageModelStreamPart> Parts { get; }
}

/// <summary>
/// Sonar chat-completions language model at <c>{base}/chat/completions</c>.
/// <see cref="PerplexityProvider.LanguageModel"/> uses the Agent API instead.
/// </summary>
public sealed class PerplexityChatLanguageModel : ILanguageModel
{
    private static readonly string[] Recency = { "hour", "day", "week", "month", "year" };
    private static readonly string[] SearchModes = { "web", "academic", "sec" };
    private static readonly string[] StreamModes = { "full", "concise" };
    private static readonly string[] Efforts = { "minimal", "low", "medium", "high" };
    private static readonly string[] ContextSizes = { "low", "medium", "high" };
    private static readonly string[] SearchTypes = { "fast", "pro", "auto" };
    private static readonly string[] CostFields =
    {
        "input_tokens_cost",
        "output_tokens_cost",
        "reasoning_tokens_cost",
        "request_cost",
        "citation_tokens_cost",
        "search_queries_cost",
        "total_cost",
    };

    private readonly PerplexityProvider _provider;
    private readonly Func<string> _generateId;

    /// <summary>Creates a Sonar chat model.</summary>
    public PerplexityChatLanguageModel(PerplexityProvider provider, string modelId, Func<string>? generateId = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _generateId = generateId ?? (() => "src_" + Guid.NewGuid().ToString("N"));
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
            _provider.ChatUri(ModelId),
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
            throw new AiSdkException("Invalid Perplexity response.");
        }

        using (document)
        {
            ValidateGenerate(document.RootElement);
            var root = document.RootElement;
            var choice = root.GetProperty("choices")[0];
            var text = choice.GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
            var content = new List<GeneratedContent>();
            if (text.Length > 0)
            {
                content.Add(new GeneratedText(text));
            }

            AddCitations(root, content);
            var usage = ReadUsage(root);
            string? rawFinish = null;
            if (choice.TryGetProperty("finish_reason", out var finish) && finish.ValueKind == JsonValueKind.String)
            {
                rawFinish = finish.GetString();
            }

            return new LanguageModelGenerateResult(
                content,
                MapFinish(rawFinish),
                PerplexityChatUsage.Convert(usage),
                rawFinish,
                built.Warnings,
                root.GetProperty("id").GetString(),
                Metadata(usage, ReadImages(root)),
                raw,
                root.GetProperty("model").GetString(),
                DateTimeOffset.FromUnixTimeSeconds(root.GetProperty("created").GetInt64()),
                http.Headers);
        }
    }

    /// <summary>
    /// Opens a chat stream and returns response headers before the body is read.
    /// </summary>
    public async Task<PerplexityChatStream> OpenStreamAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        var built = Build(options, stream: true);
        var response = await PostAsync(built.Body.ToJsonString(), Headers(options), cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            response.Dispose();
            throw ProviderHttp.MapStatus((int)response.StatusCode, body);
        }

        var headers = CopyHeaders(response);
        return new PerplexityChatStream(headers, Read(response, built.Warnings, options, cancellationToken));
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
        LanguageModelCallOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var opened = await OpenStreamAsync(options, cancellationToken).ConfigureAwait(false);
        await foreach (var part in opened.Parts)
        {
            yield return part;
        }
    }

    private async IAsyncEnumerable<LanguageModelStreamPart> Read(
        HttpResponseMessage response,
        List<CallWarning> warnings,
        LanguageModelCallOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var session = new Session(_generateId);
        try
        {
            yield return new StreamStartStreamPart(warnings);
            var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            await foreach (var data in SseParser.ReadDataAsync(stream, cancellationToken))
            {
                foreach (var part in session.Accept(data, options.IncludeRawChunks))
                {
                    yield return part;
                }
            }
        }
        finally
        {
            response.Dispose();
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

    private Prepared Build(LanguageModelCallOptions options, bool stream)
    {
        var warnings = new List<CallWarning>();
        if (options.TopK.HasValue)
        {
            warnings.Add(new CallWarning("unsupported", "topK"));
        }

        if (options.StopSequences != null)
        {
            warnings.Add(new CallWarning("unsupported", "stopSequences"));
        }

        if (options.Seed.HasValue)
        {
            warnings.Add(new CallWarning("unsupported", "seed"));
        }

        if (options.Reasoning != null && !string.Equals(options.Reasoning, "provider-default", StringComparison.Ordinal))
        {
            warnings.Add(new CallWarning("unsupported", "reasoning"));
        }

        JsonElement? providerOptions = null;
        if (options.ProviderOptions != null && options.ProviderOptions.TryGetValue("perplexity", out var configured))
        {
            ValidateProviderOptions(configured);
            providerOptions = configured;
        }

        var body = new JsonObject
        {
            ["model"] = ModelId,
        };
        if (options.FrequencyPenalty is { } frequency)
        {
            body["frequency_penalty"] = frequency;
        }

        if (options.MaxOutputTokens is { } maxTokens)
        {
            body["max_tokens"] = maxTokens;
        }

        if (options.PresencePenalty is { } presence)
        {
            body["presence_penalty"] = presence;
        }

        if (options.Temperature is { } temperature)
        {
            body["temperature"] = temperature;
        }

        if (options.TopK is { } topK)
        {
            body["top_k"] = topK;
        }

        if (options.TopP is { } topP)
        {
            body["top_p"] = topP;
        }

        if (options.JsonSchema is { } schema && schema.ValueKind != JsonValueKind.Undefined && schema.ValueKind != JsonValueKind.Null)
        {
            body["response_format"] = new JsonObject
            {
                ["type"] = "json_schema",
                ["json_schema"] = new JsonObject
                {
                    ["schema"] = JsonNode.Parse(schema.GetRawText()),
                },
            };
        }

        if (providerOptions is { } provided)
        {
            foreach (var property in provided.EnumerateObject())
            {
                body[property.Name] = JsonNode.Parse(property.Value.GetRawText());
            }
        }

        body["messages"] = PerplexityMessages.Convert(options.Prompt);
        if (stream)
        {
            body["stream"] = true;
        }

        return new Prepared(body, warnings);
    }

    private async Task<HttpResponseMessage> PostAsync(string json, IReadOnlyDictionary<string, string?> headers, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, _provider.ChatUri(ModelId));
        ApplyHeaders(request, headers);
        request.Content = new StringContent(json ?? string.Empty, Encoding.UTF8, "application/json");
        return await _provider.HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
    }

    private void AddCitations(JsonElement root, List<GeneratedContent> content)
    {
        if (!root.TryGetProperty("citations", out var citations) || citations.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var citation in citations.EnumerateArray())
        {
            var url = citation.GetString();
            if (!string.IsNullOrEmpty(url))
            {
                content.Add(new GeneratedSource(_generateId(), url!, null));
            }
        }
    }

    private sealed class Prepared
    {
        public Prepared(JsonObject body, List<CallWarning> warnings)
        {
            Body = body;
            Warnings = warnings;
        }

        public JsonObject Body { get; }

        public List<CallWarning> Warnings { get; }
    }

    private sealed class ChatImage
    {
        public string ImageUrl { get; set; } = string.Empty;

        public string OriginUrl { get; set; } = string.Empty;

        public double Height { get; set; }

        public double Width { get; set; }
    }

    private sealed class Session
    {
        private readonly Func<string> _generateId;
        private bool _first = true;
        private bool _active;
        private FinishReason _finish = FinishReason.Other;
        private string? _rawFinish;
        private JsonElement? _usage;
        private List<ChatImage>? _images;

        public Session(Func<string> generateId)
        {
            _generateId = generateId;
        }

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
                parts.Add(new ErrorStreamPart("Invalid Perplexity response."));
                return parts;
            }

            using (document)
            {
                try
                {
                    ValidateChunk(document.RootElement);
                }
                catch (AiSdkException exception)
                {
                    parts.Add(new ErrorStreamPart(exception.Message));
                    return parts;
                }

                var root = document.RootElement;
                if (_first)
                {
                    parts.Add(new ResponseMetadataStreamPart(
                        root.GetProperty("id").GetString(),
                        root.GetProperty("model").GetString(),
                        DateTimeOffset.FromUnixTimeSeconds(root.GetProperty("created").GetInt64())));
                    if (root.TryGetProperty("citations", out var citations) && citations.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var citation in citations.EnumerateArray())
                        {
                            var url = citation.GetString();
                            if (!string.IsNullOrEmpty(url))
                            {
                                parts.Add(new SourceStreamPart(_generateId(), url!, null));
                            }
                        }
                    }

                    _first = false;
                }

                if (root.TryGetProperty("usage", out var usage) && usage.ValueKind == JsonValueKind.Object)
                {
                    _usage = usage.Clone();
                }

                var images = ReadImages(root);
                if (images != null)
                {
                    _images = images;
                }

                if (root.GetProperty("choices").GetArrayLength() == 0)
                {
                    return parts;
                }

                var choice = root.GetProperty("choices")[0];
                if (choice.TryGetProperty("finish_reason", out var finish) && finish.ValueKind == JsonValueKind.String)
                {
                    _rawFinish = finish.GetString();
                    _finish = MapFinish(_rawFinish);
                }

                if (!choice.TryGetProperty("delta", out var delta) || delta.ValueKind != JsonValueKind.Object)
                {
                    return parts;
                }

                if (delta.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.String)
                {
                    var text = content.GetString() ?? string.Empty;
                    if (!_active)
                    {
                        _active = true;
                        parts.Add(new TextStartStreamPart("0"));
                    }

                    parts.Add(new TextDeltaStreamPart("0", text));
                }
            }

            return parts;
        }

        public List<LanguageModelStreamPart> Flush()
        {
            var parts = new List<LanguageModelStreamPart>();
            if (_active)
            {
                parts.Add(new TextEndStreamPart("0"));
            }

            parts.Add(new FinishStreamPart(_finish, PerplexityChatUsage.Convert(_usage), _rawFinish, Metadata(_usage, _images)));
            return parts;
        }
    }

    private static JsonElement? ReadUsage(JsonElement root)
    {
        if (!root.TryGetProperty("usage", out var usage) || usage.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return usage.Clone();
    }

    private static List<ChatImage>? ReadImages(JsonElement root)
    {
        if (!root.TryGetProperty("images", out var images) || images.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        var list = new List<ChatImage>();
        foreach (var image in images.EnumerateArray())
        {
            list.Add(new ChatImage
            {
                ImageUrl = image.GetProperty("image_url").GetString() ?? string.Empty,
                OriginUrl = image.GetProperty("origin_url").GetString() ?? string.Empty,
                Height = image.GetProperty("height").GetDouble(),
                Width = image.GetProperty("width").GetDouble(),
            });
        }

        return list;
    }

    private static JsonElement Metadata(JsonElement? usage, List<ChatImage>? images)
    {
        var imageNode = images is null ? JsonNull() : ImageArray(images);
        var perplexity = new JsonObject
        {
            ["images"] = imageNode,
            ["usage"] = new JsonObject
            {
                ["citationTokens"] = NumberOrNull(usage, "citation_tokens"),
                ["numSearchQueries"] = NumberOrNull(usage, "num_search_queries"),
            },
            ["cost"] = CostOrNull(usage),
        };
        return JsonDocument.Parse(new JsonObject { ["perplexity"] = perplexity }.ToJsonString()).RootElement.Clone();
    }

    private static JsonArray ImageArray(List<ChatImage> images)
    {
        var array = new JsonArray();
        foreach (var image in images)
        {
            array.Add(new JsonObject
            {
                ["imageUrl"] = image.ImageUrl,
                ["originUrl"] = image.OriginUrl,
                ["height"] = image.Height,
                ["width"] = image.Width,
            });
        }

        return array;
    }

    private static JsonNode NumberOrNull(JsonElement? usage, string name)
    {
        if (usage is not { } element
            || !element.TryGetProperty(name, out var value)
            || value.ValueKind == JsonValueKind.Null
            || value.ValueKind != JsonValueKind.Number)
        {
            return JsonNull();
        }

        return JsonNode.Parse(value.GetRawText())!;
    }

    private static JsonNode CostOrNull(JsonElement? usage)
    {
        if (usage is not { } element
            || !element.TryGetProperty("cost", out var cost)
            || cost.ValueKind != JsonValueKind.Object)
        {
            return JsonNull();
        }

        return new JsonObject
        {
            ["inputTokensCost"] = NumberOrNull(cost, "input_tokens_cost"),
            ["outputTokensCost"] = NumberOrNull(cost, "output_tokens_cost"),
            ["requestCost"] = NumberOrNull(cost, "request_cost"),
            ["totalCost"] = NumberOrNull(cost, "total_cost"),
        };
    }

    private static FinishReason MapFinish(string? raw)
    {
        if (raw == "stop" || raw == "length")
        {
            return raw == "stop" ? FinishReason.Stop : FinishReason.Length;
        }

        return FinishReason.Other;
    }

    private static void ValidateGenerate(JsonElement root)
    {
        RequireString(root, "id");
        RequireNumber(root, "created");
        RequireString(root, "model");
        if (!root.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array || choices.GetArrayLength() == 0)
        {
            throw new AiSdkException("Invalid Perplexity response.");
        }

        var choice = choices[0];
        if (choice.ValueKind != JsonValueKind.Object
            || !choice.TryGetProperty("message", out var message)
            || message.ValueKind != JsonValueKind.Object)
        {
            throw new AiSdkException("Invalid Perplexity response.");
        }

        if (!message.TryGetProperty("role", out var role) || role.GetString() != "assistant")
        {
            throw new AiSdkException("Invalid Perplexity response.");
        }

        RequireString(message, "content");
        ValidateOptionalString(choice, "finish_reason");
        ValidateCitations(root);
        ValidateImages(root);
        ValidateUsage(root);
    }

    private static void ValidateChunk(JsonElement root)
    {
        RequireString(root, "id");
        RequireNumber(root, "created");
        RequireString(root, "model");
        if (!root.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array)
        {
            throw new AiSdkException("Invalid Perplexity response.");
        }

        foreach (var choice in choices.EnumerateArray())
        {
            if (choice.ValueKind != JsonValueKind.Object || !choice.TryGetProperty("delta", out var delta) || delta.ValueKind != JsonValueKind.Object)
            {
                throw new AiSdkException("Invalid Perplexity response.");
            }

            if (delta.TryGetProperty("role", out var role) && role.ValueKind != JsonValueKind.Null && role.GetString() != "assistant")
            {
                throw new AiSdkException("Invalid Perplexity response.");
            }

            ValidateOptionalString(delta, "content");
            ValidateOptionalString(choice, "finish_reason");
        }

        ValidateCitations(root);
        ValidateImages(root);
        ValidateUsage(root);
    }

    private static void ValidateCitations(JsonElement root)
    {
        if (!root.TryGetProperty("citations", out var citations) || citations.ValueKind == JsonValueKind.Null)
        {
            return;
        }

        if (citations.ValueKind != JsonValueKind.Array)
        {
            throw new AiSdkException("Invalid Perplexity response.");
        }

        foreach (var citation in citations.EnumerateArray())
        {
            if (citation.ValueKind != JsonValueKind.String)
            {
                throw new AiSdkException("Invalid Perplexity response.");
            }
        }
    }

    private static void ValidateImages(JsonElement root)
    {
        if (!root.TryGetProperty("images", out var images) || images.ValueKind == JsonValueKind.Null)
        {
            return;
        }

        if (images.ValueKind != JsonValueKind.Array)
        {
            throw new AiSdkException("Invalid Perplexity response.");
        }

        foreach (var image in images.EnumerateArray())
        {
            if (image.ValueKind != JsonValueKind.Object)
            {
                throw new AiSdkException("Invalid Perplexity response.");
            }

            RequireString(image, "image_url");
            RequireString(image, "origin_url");
            RequireNumber(image, "height");
            RequireNumber(image, "width");
        }
    }

    private static void ValidateUsage(JsonElement root)
    {
        if (!root.TryGetProperty("usage", out var usage) || usage.ValueKind == JsonValueKind.Null)
        {
            return;
        }

        if (usage.ValueKind != JsonValueKind.Object)
        {
            throw new AiSdkException("Invalid Perplexity response.");
        }

        RequireNumber(usage, "prompt_tokens");
        RequireNumber(usage, "completion_tokens");
        ValidateOptionalNumber(usage, "total_tokens");
        ValidateOptionalNumber(usage, "citation_tokens");
        ValidateOptionalNumber(usage, "num_search_queries");
        ValidateOptionalNumber(usage, "reasoning_tokens");
        if (usage.TryGetProperty("search_context_size", out var size) && size.ValueKind != JsonValueKind.Null)
        {
            RequireEnum(size, ContextSizes, "search_context_size");
        }

        if (!usage.TryGetProperty("cost", out var cost) || cost.ValueKind == JsonValueKind.Null)
        {
            return;
        }

        if (cost.ValueKind != JsonValueKind.Object)
        {
            throw new AiSdkException("Invalid Perplexity response.");
        }

        for (var index = 0; index < CostFields.Length; index++)
        {
            ValidateOptionalNumber(cost, CostFields[index]);
        }
    }

    private static void ValidateProviderOptions(JsonElement options)
    {
        if (options.ValueKind != JsonValueKind.Object)
        {
            throw new AiSdkException("Invalid perplexity provider option: expected an object.");
        }

        foreach (var property in options.EnumerateObject())
        {
            switch (property.Name)
            {
                case "search_recency_filter":
                    RequireEnum(property.Value, Recency, property.Name);
                    break;
                case "search_mode":
                    RequireEnum(property.Value, SearchModes, property.Name);
                    break;
                case "stream_mode":
                    RequireEnum(property.Value, StreamModes, property.Name);
                    break;
                case "reasoning_effort":
                    RequireEnum(property.Value, Efforts, property.Name);
                    break;
                case "search_domain_filter":
                case "search_language_filter":
                case "image_domain_filter":
                case "image_format_filter":
                    RequireStringArray(property.Value, property.Name);
                    break;
                case "search_after_date_filter":
                case "search_before_date_filter":
                case "last_updated_after_filter":
                case "last_updated_before_filter":
                case "language_preference":
                    RequireText(property.Value, property.Name);
                    break;
                case "enable_search_classifier":
                case "disable_search":
                case "return_related_questions":
                case "return_images":
                    RequireBool(property.Value, property.Name);
                    break;
                case "media_response":
                    ValidateMedia(property.Value);
                    break;
                case "web_search_options":
                    ValidateWeb(property.Value);
                    break;
            }
        }
    }

    private static void ValidateMedia(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            throw new AiSdkException("Invalid perplexity provider option: media_response.");
        }

        if (!value.TryGetProperty("overrides", out var overrides))
        {
            return;
        }

        if (overrides.ValueKind != JsonValueKind.Object)
        {
            throw new AiSdkException("Invalid perplexity provider option: media_response.overrides.");
        }

        if (overrides.TryGetProperty("return_videos", out var videos))
        {
            RequireBool(videos, "return_videos");
        }
    }

    private static void ValidateWeb(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            throw new AiSdkException("Invalid perplexity provider option: web_search_options.");
        }

        if (value.TryGetProperty("search_context_size", out var size))
        {
            RequireEnum(size, ContextSizes, "search_context_size");
        }

        if (value.TryGetProperty("search_type", out var type))
        {
            RequireEnum(type, SearchTypes, "search_type");
        }

        if (value.TryGetProperty("image_results_enhanced_relevance", out var relevance))
        {
            RequireBool(relevance, "image_results_enhanced_relevance");
        }

        if (!value.TryGetProperty("user_location", out var location))
        {
            return;
        }

        if (location.ValueKind != JsonValueKind.Object)
        {
            throw new AiSdkException("Invalid perplexity provider option: user_location.");
        }

        ValidateOptionalNumber(location, "latitude");
        ValidateOptionalNumber(location, "longitude");
        foreach (var name in new[] { "country", "city", "region" })
        {
            if (location.TryGetProperty(name, out var item))
            {
                RequireText(item, name);
            }
        }
    }

    private static void RequireString(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String)
        {
            throw new AiSdkException("Invalid Perplexity response.");
        }
    }

    private static void RequireText(JsonElement value, string name)
    {
        if (value.ValueKind != JsonValueKind.String)
        {
            throw new AiSdkException("Invalid perplexity provider option: " + name + ".");
        }
    }

    private static void RequireNumber(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Number)
        {
            throw new AiSdkException("Invalid Perplexity response.");
        }
    }

    private static void RequireBool(JsonElement value, string name)
    {
        if (value.ValueKind != JsonValueKind.True && value.ValueKind != JsonValueKind.False)
        {
            throw new AiSdkException("Invalid perplexity provider option: " + name + ".");
        }
    }

    private static void RequireStringArray(JsonElement value, string name)
    {
        if (value.ValueKind != JsonValueKind.Array)
        {
            throw new AiSdkException("Invalid perplexity provider option: " + name + ".");
        }

        foreach (var item in value.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String)
            {
                throw new AiSdkException("Invalid perplexity provider option: " + name + ".");
            }
        }
    }

    private static void RequireEnum(JsonElement value, string[] allowed, string name)
    {
        if (value.ValueKind != JsonValueKind.String)
        {
            throw new AiSdkException("Invalid perplexity provider option: " + name + ".");
        }

        var text = value.GetString();
        for (var index = 0; index < allowed.Length; index++)
        {
            if (string.Equals(allowed[index], text, StringComparison.Ordinal))
            {
                return;
            }
        }

        throw new AiSdkException("Invalid perplexity provider option: " + name + ".");
    }

    private static void ValidateOptionalString(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return;
        }

        if (value.ValueKind != JsonValueKind.String)
        {
            throw new AiSdkException("Invalid Perplexity response.");
        }
    }

    private static void ValidateOptionalNumber(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return;
        }

        if (value.ValueKind != JsonValueKind.Number)
        {
            throw new AiSdkException("Invalid Perplexity response.");
        }
    }

    private static void ApplyHeaders(HttpRequestMessage request, IReadOnlyDictionary<string, string?>? headers)
    {
        if (headers is null)
        {
            return;
        }

        foreach (var pair in headers)
        {
            if (string.IsNullOrEmpty(pair.Value))
            {
                continue;
            }

            if (pair.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (pair.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
            {
                var value = pair.Value!;
                var space = value.IndexOf(' ');
                if (space > 0)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue(value.Substring(0, space), value.Substring(space + 1));
                }
                else
                {
                    request.Headers.TryAddWithoutValidation("Authorization", value);
                }

                continue;
            }

            request.Headers.TryAddWithoutValidation(pair.Key, pair.Value);
        }
    }

    private static Dictionary<string, string> CopyHeaders(HttpResponseMessage response)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in response.Headers)
        {
            headers[header.Key] = string.Join(",", header.Value);
        }

        if (response.Content != null)
        {
            foreach (var header in response.Content.Headers)
            {
                headers[header.Key] = string.Join(",", header.Value);
            }
        }

        return headers;
    }

    private static JsonNode JsonNull()
    {
        return JsonNode.Parse("null")!;
    }
}
