// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAICompatible;

/// <summary>Legacy Completions language model.</summary>
public sealed class OpenAICompatibleCompletionLanguageModel : ILanguageModel
{
    private static readonly string[] KnownOptionNames = { "echo", "logitBias", "suffix", "user" };

    private readonly OpenAICompatibleProvider _provider;
    private readonly string _providerId;

    /// <summary>Creates a completion model.</summary>
    public OpenAICompatibleCompletionLanguageModel(OpenAICompatibleProvider provider, string modelId)
        : this(provider, modelId, null)
    {
    }

    /// <summary>Creates a completion model with an explicit provider id.</summary>
    public OpenAICompatibleCompletionLanguageModel(OpenAICompatibleProvider provider, string modelId, string? providerId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _providerId = providerId ?? OpenAICompatibleChat.Qualify(provider.Options.ProviderName, "completion");
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => _providerId;

    /// <summary>Provider-options key. The segment before the first dot of <see cref="Provider"/>.</summary>
    public string ProviderOptionsName => OpenAICompatibleChat.BaseProviderName(Provider);

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>HTTP response headers from the most recent call.</summary>
    public IReadOnlyDictionary<string, string> LastResponseHeaders { get; private set; } = new Dictionary<string, string>();

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        options = options ?? new LanguageModelCallOptions();
        var prepared = Prepare(options, stream: false);
        var response = await _provider.PostJsonAsync(_provider.CompletionsUri(ModelId), prepared.Json, prepared.Headers, cancellationToken).ConfigureAwait(false);
        LastResponseHeaders = response.Headers;
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body);
        var root = document.RootElement;
        if (!root.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array || choices.GetArrayLength() == 0)
        {
            throw new AiSdkException("Response did not contain any choices.");
        }

        var choice = choices[0];
        var content = new List<GeneratedContent>();
        if (choice.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
        {
            var value = text.GetString();
            if (!string.IsNullOrEmpty(value))
            {
                content.Add(new GeneratedText(value!));
            }
        }

        var raw = choice.TryGetProperty("finish_reason", out var finish) && finish.ValueKind == JsonValueKind.String
            ? finish.GetString()
            : null;
        var usage = root.TryGetProperty("usage", out var usageElement)
            ? OpenAICompatibleChat.ConvertCompletionUsage(usageElement)
            : new LanguageModelUsage(null, null, null);
        return new LanguageModelGenerateResult(
            content,
            MapFinish(raw),
            usage,
            raw,
            prepared.Warnings,
            Text(root, "id"),
            null,
            response.Body,
            Text(root, "model"),
            Timestamp(root),
            response.Headers);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
        LanguageModelCallOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        options = options ?? new LanguageModelCallOptions();
        var prepared = Prepare(options, stream: true);
        var sse = await _provider.PostSseAsync(_provider.CompletionsUri(ModelId), prepared.Json, prepared.Headers, cancellationToken).ConfigureAwait(false);
        LastResponseHeaders = sse.Headers;
        yield return new StreamStartStreamPart(prepared.Warnings);

        string? finishRaw = null;
        var sawFinish = false;
        var failed = false;
        JsonElement? usageElement = null;
        var started = false;

        await foreach (var data in sse.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            if (options.IncludeRawChunks)
            {
                yield return new RawStreamPart(data);
            }

            JsonNode? node;
            try
            {
                node = JsonNode.Parse(data);
            }
            catch (JsonException)
            {
                failed = true;
                yield return new ErrorStreamPart("The provider stream chunk could not be parsed.");
                continue;
            }

            if (node is not JsonObject root)
            {
                failed = true;
                yield return new ErrorStreamPart("The provider stream chunk could not be parsed.");
                continue;
            }

            if (root["error"] != null)
            {
                failed = true;
                yield return new ErrorStreamPart(ErrorText(root["error"]));
                continue;
            }

            if (!started)
            {
                started = true;
                var created = ReadInt(root, "created");
                if (created == 0)
                {
                    created = null;
                }

                DateTimeOffset? timestamp = created == null ? null : DateTimeOffset.FromUnixTimeSeconds(created.Value);
                yield return new ResponseMetadataStreamPart(AsString(root["id"]), AsString(root["model"]), timestamp);
                yield return new TextStartStreamPart("0");
            }

            if (root["usage"] is JsonObject usageObject)
            {
                using var usageDocument = JsonDocument.Parse(usageObject.ToJsonString());
                usageElement = usageDocument.RootElement.Clone();
            }

            var choice = root["choices"]?[0] as JsonObject;
            var finishText = choice == null ? null : AsString(choice["finish_reason"]);
            if (!string.IsNullOrEmpty(finishText))
            {
                finishRaw = finishText;
                sawFinish = true;
            }

            var delta = choice == null ? null : AsString(choice["text"]);
            if (delta != null)
            {
                yield return new TextDeltaStreamPart("0", delta);
            }
        }

        if (started)
        {
            yield return new TextEndStreamPart("0");
        }

        var usage = OpenAICompatibleChat.ConvertCompletionUsage(usageElement);
        var reason = failed && !sawFinish ? FinishReason.Error : sawFinish ? MapFinish(finishRaw) : FinishReason.Other;
        yield return new FinishStreamPart(reason, usage, sawFinish ? finishRaw : null);
    }

    private Prepared Prepare(LanguageModelCallOptions options, bool stream)
    {
        var warnings = new List<CallWarning>();
        var rawName = ProviderOptionsName;
        var camel = OpenAICompatibleChat.ToCamelCase(rawName);
        if (!string.Equals(camel, rawName, StringComparison.Ordinal) && OpenAICompatibleChat.HasOptions(options.ProviderOptions, rawName))
        {
            warnings.Add(new CallWarning("deprecated", "providerOptions key '" + rawName + "'. Use '" + camel + "' instead."));
        }

        if (options.TopK != null)
        {
            warnings.Add(new CallWarning("unsupported", "topK"));
        }

        if (options.Tools != null && options.Tools.Count > 0)
        {
            warnings.Add(new CallWarning("unsupported", "tools"));
        }

        if (options.ToolChoice != null)
        {
            warnings.Add(new CallWarning("unsupported", "toolChoice"));
        }

        if (options.JsonSchema != null && options.JsonSchema.Value.ValueKind != JsonValueKind.Undefined && options.JsonSchema.Value.ValueKind != JsonValueKind.Null)
        {
            warnings.Add(new CallWarning("unsupported", "JSON response format is not supported."));
        }

        var settings = ReadSettings(options.ProviderOptions, rawName, camel);
        var converted = ConvertPrompt(options.Prompt);
        var body = new JsonObject { ["model"] = ModelId };
        if (settings.Echo != null)
        {
            body["echo"] = settings.Echo.Value;
        }

        if (settings.LogitBias != null)
        {
            body["logit_bias"] = settings.LogitBias.DeepClone();
        }

        if (settings.Suffix != null)
        {
            body["suffix"] = settings.Suffix;
        }

        if (settings.User != null)
        {
            body["user"] = settings.User;
        }

        if (options.MaxOutputTokens is { } maxTokens)
        {
            body["max_tokens"] = maxTokens;
        }

        if (options.Temperature is { } temperature)
        {
            body["temperature"] = temperature;
        }

        if (options.TopP is { } topP)
        {
            body["top_p"] = topP;
        }

        if (options.FrequencyPenalty is { } frequency)
        {
            body["frequency_penalty"] = frequency;
        }

        if (options.PresencePenalty is { } presence)
        {
            body["presence_penalty"] = presence;
        }

        if (options.Seed is { } seed)
        {
            body["seed"] = seed;
        }

        foreach (var pair in settings.Extras)
        {
            body[pair.Key] = pair.Value?.DeepClone();
        }

        body["prompt"] = converted.Prompt;
        var stop = new JsonArray();
        if (converted.Stop != null)
        {
            stop.Add(converted.Stop);
        }

        if (options.StopSequences != null)
        {
            foreach (var sequence in options.StopSequences)
            {
                stop.Add(sequence);
            }
        }

        if (stop.Count > 0)
        {
            body["stop"] = stop;
        }

        if (stream)
        {
            body["stream"] = true;
            if (_provider.Options.IncludeUsage)
            {
                body["stream_options"] = new JsonObject { ["include_usage"] = true };
            }
        }

        return new Prepared(body.ToJsonString(), _provider.CreateHeaders(options.Headers), warnings);
    }

    private static CompletionSettings ReadSettings(IReadOnlyDictionary<string, JsonElement>? providerOptions, string rawName, string camel)
    {
        var settings = new CompletionSettings();
        Apply(settings, providerOptions, rawName);
        if (!string.Equals(camel, rawName, StringComparison.Ordinal))
        {
            Apply(settings, providerOptions, camel);
        }

        return settings;
    }

    private static void Apply(CompletionSettings settings, IReadOnlyDictionary<string, JsonElement>? providerOptions, string key)
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
                case "echo":
                    if (property.Value.ValueKind == JsonValueKind.True || property.Value.ValueKind == JsonValueKind.False)
                    {
                        settings.Echo = property.Value.GetBoolean();
                    }

                    break;
                case "logitBias":
                    settings.LogitBias = JsonNode.Parse(property.Value.GetRawText());
                    break;
                case "suffix":
                    if (property.Value.ValueKind == JsonValueKind.String)
                    {
                        settings.Suffix = property.Value.GetString();
                    }

                    break;
                case "user":
                    if (property.Value.ValueKind == JsonValueKind.String)
                    {
                        settings.User = property.Value.GetString();
                    }

                    break;
                default:
                    if (Array.IndexOf(KnownOptionNames, property.Name) < 0)
                    {
                        settings.Extras[property.Name] = JsonNode.Parse(property.Value.GetRawText());
                    }

                    break;
            }
        }
    }

    private static CompletionPrompt ConvertPrompt(IReadOnlyList<ModelMessage> prompt)
    {
        var text = new StringBuilder();
        var messages = prompt ?? Array.Empty<ModelMessage>();
        var start = 0;
        if (messages.Count > 0 && messages[0] is SystemModelMessage first)
        {
            text.Append(first.Content);
            text.Append("\n\n");
            start = 1;
        }

        for (var i = start; i < messages.Count; i++)
        {
            switch (messages[i])
            {
                case SystemModelMessage:
                    throw new AiSdkException("Unexpected system message in prompt.");
                case UserModelMessage user:
                    text.Append("user:\n");
                    text.Append(UserText(user));
                    text.Append("\n\n");
                    break;
                case AssistantModelMessage assistant:
                    if (assistant.ToolCalls.Count > 0)
                    {
                        throw OpenAICompatibleChat.Unsupported("tool-call messages");
                    }

                    text.Append("assistant:\n");
                    text.Append(assistant.Text ?? string.Empty);
                    text.Append("\n\n");
                    break;
                case ToolModelMessage:
                    throw OpenAICompatibleChat.Unsupported("tool messages");
                default:
                    throw new AiSdkException("Unsupported role: " + messages[i].Role + ".");
            }
        }

        text.Append("assistant:\n");
        return new CompletionPrompt(text.ToString(), "\nuser:");
    }

    private static string UserText(UserModelMessage user)
    {
        var builder = new StringBuilder();
        foreach (var part in user.Content)
        {
            if (part is TextContentPart text && !string.IsNullOrEmpty(text.Text))
            {
                builder.Append(text.Text);
            }
        }

        return builder.ToString();
    }

    private FinishReason MapFinish(string? raw)
    {
        var mapped = _provider.Options.MapFinishReason?.Invoke(raw);
        return mapped ?? FinishReasons.Parse(raw);
    }

    private static string? Text(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var text = value.GetString();
        return string.IsNullOrEmpty(text) ? null : text;
    }

    private static DateTimeOffset? Timestamp(JsonElement root)
    {
        if (!root.TryGetProperty("created", out var created) || created.ValueKind != JsonValueKind.Number || !created.TryGetInt64(out var seconds) || seconds == 0)
        {
            return null;
        }

        return DateTimeOffset.FromUnixTimeSeconds(seconds);
    }

    private static int? ReadInt(JsonObject obj, string name)
    {
        if (obj[name] is JsonValue value && value.TryGetValue<int>(out var number))
        {
            return number;
        }

        return null;
    }

    private static string? AsString(JsonNode? node)
    {
        if (node is JsonValue value && value.TryGetValue<string>(out var text))
        {
            return text;
        }

        return null;
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

    private sealed class CompletionSettings
    {
        public bool? Echo { get; set; }

        public JsonNode? LogitBias { get; set; }

        public string? Suffix { get; set; }

        public string? User { get; set; }

        public JsonObject Extras { get; } = new();
    }

    private sealed class CompletionPrompt
    {
        public CompletionPrompt(string prompt, string stop)
        {
            Prompt = prompt;
            Stop = stop;
        }

        public string Prompt { get; }

        public string Stop { get; }
    }

    private sealed class Prepared
    {
        public Prepared(string json, Dictionary<string, string?> headers, List<CallWarning> warnings)
        {
            Json = json;
            Headers = headers;
            Warnings = warnings;
        }

        public string Json { get; }

        public Dictionary<string, string?> Headers { get; }

        public List<CallWarning> Warnings { get; }
    }
}
