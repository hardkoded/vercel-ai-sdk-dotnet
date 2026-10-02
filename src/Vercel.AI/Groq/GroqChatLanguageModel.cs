// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Groq;

/// <summary>
/// Groq chat completions model. Generate requests omit <c>stream</c>. Stream requests send
/// <c>stream: true</c> and do not send <c>stream_options</c>. Reasoning is the <c>reasoning</c> field.
/// </summary>
public sealed class GroqChatLanguageModel : ILanguageModel
{
    private readonly OpenAICompatibleProvider _provider;

    /// <summary>Creates a Groq chat model.</summary>
    public GroqChatLanguageModel(OpenAICompatibleProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <summary>Provider id <c>groq.chat</c>.</summary>
    public string Provider => "groq.chat";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>HTTP response headers from the most recent generate or stream call.</summary>
    public IReadOnlyDictionary<string, string> LastResponseHeaders { get; private set; } = new Dictionary<string, string>();

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        options ??= new LanguageModelCallOptions();
        var built = Build(options, stream: false);
        var http = await _provider.Http.SendJsonStringAsync(
            HttpMethod.Post,
            _provider.ChatUri(ModelId),
            built.Body.ToJsonString(),
            Headers(options),
            cancellationToken).ConfigureAwait(false);
        LastResponseHeaders = http.Headers;
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(http.Body) ? "{}" : http.Body);
        var root = document.RootElement;
        if (!root.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array || choices.GetArrayLength() == 0)
        {
            throw new InvalidResponseDataException(http.Body, "Response did not contain any choices.");
        }

        var choice = choices[0];
        var message = choice.TryGetProperty("message", out var messageElement) && messageElement.ValueKind == JsonValueKind.Object
            ? messageElement
            : default;
        var content = new List<GeneratedContent>();
        var text = message.ValueKind == JsonValueKind.Object ? JsonValues.GetString(message, "content") : null;
        if (!string.IsNullOrEmpty(text))
        {
            content.Add(new GeneratedText(text!));
        }

        var reasoning = message.ValueKind == JsonValueKind.Object ? JsonValues.GetString(message, "reasoning") : null;
        if (!string.IsNullOrEmpty(reasoning))
        {
            content.Add(new GeneratedReasoning(reasoning!));
        }

        if (message.ValueKind == JsonValueKind.Object
            && message.TryGetProperty("tool_calls", out var toolCalls)
            && toolCalls.ValueKind == JsonValueKind.Array)
        {
            foreach (var toolCall in toolCalls.EnumerateArray())
            {
                var toolCallId = JsonValues.GetString(toolCall, "id");
                var function = toolCall.TryGetProperty("function", out var functionElement) ? functionElement : default;
                var name = function.ValueKind == JsonValueKind.Object ? JsonValues.GetString(function, "name") : null;
                var arguments = function.ValueKind == JsonValueKind.Object ? JsonValues.GetString(function, "arguments") : null;
                content.Add(new GeneratedToolCall(
                    string.IsNullOrEmpty(toolCallId) ? JsonValues.GenerateId("call_") : toolCallId,
                    name ?? string.Empty,
                    arguments ?? "{}"));
            }
        }

        var finishRaw = choice.ValueKind == JsonValueKind.Object ? JsonValues.GetString(choice, "finish_reason") : null;
        JsonElement? rawUsage = null;
        if (root.TryGetProperty("usage", out var usageElement) && usageElement.ValueKind != JsonValueKind.Null)
        {
            rawUsage = usageElement.Clone();
        }

        var usage = GroqUsage.Convert(rawUsage);
        string? id = JsonValues.GetString(root, "id");
        string? responseModel = JsonValues.GetString(root, "model");
        DateTimeOffset? timestamp = null;
        var created = JsonValues.GetInt(root, "created");
        if (created != null)
        {
            timestamp = DateTimeOffset.FromUnixTimeSeconds(created.Value);
        }

        return new LanguageModelGenerateResult(
            content,
            FinishReasons.Parse(finishRaw),
            usage.ToLanguageModelUsage(),
            finishRaw,
            built.Warnings,
            id,
            null,
            http.Body,
            responseModel,
            timestamp,
            http.Headers);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
        LanguageModelCallOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        options ??= new LanguageModelCallOptions();
        var built = Build(options, stream: true);
        var responseHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var tracker = new StreamingToolCallTracker();
        var started = new HashSet<int>();
        var indexIds = new Dictionary<int, string>();
        string? finishRaw = null;
        var finishError = false;
        JsonElement? rawUsage = null;
        var sawMetadata = false;
        var activeText = false;
        var activeReasoning = false;
        var pending = new List<LanguageModelStreamPart>();
        yield return new StreamStartStreamPart(built.Warnings);
        await foreach (var data in _provider.Http.SendSseAsync(
            _provider.ChatUri(ModelId),
            built.Body.ToJsonString(),
            Headers(options),
            responseHeaders,
            cancellationToken).ConfigureAwait(false))
        {
            LastResponseHeaders = responseHeaders;
            pending.Clear();
            AcceptChunk(
                data,
                options.IncludeRawChunks,
                tracker,
                started,
                indexIds,
                ref finishRaw,
                ref finishError,
                ref rawUsage,
                ref sawMetadata,
                ref activeText,
                ref activeReasoning,
                pending);
            foreach (var part in pending)
            {
                yield return part;
            }
        }

        LastResponseHeaders = responseHeaders;
        if (activeReasoning)
        {
            yield return new ReasoningEndStreamPart("reasoning-0");
        }

        if (activeText)
        {
            yield return new TextEndStreamPart("txt-0");
        }

        foreach (var call in tracker.Flush())
        {
            yield return new GroqToolInputEndStreamPart(call.ToolCallId);
            yield return call;
        }

        var finishReason = finishError ? FinishReason.Error : FinishReasons.Parse(finishRaw);
        yield return new FinishStreamPart(finishReason, GroqUsage.Convert(rawUsage).ToLanguageModelUsage(), finishError ? null : finishRaw);
    }

    private static void AcceptChunk(
        string data,
        bool includeRaw,
        StreamingToolCallTracker tracker,
        HashSet<int> started,
        Dictionary<int, string> indexIds,
        ref string? finishRaw,
        ref bool finishError,
        ref JsonElement? rawUsage,
        ref bool sawMetadata,
        ref bool activeText,
        ref bool activeReasoning,
        List<LanguageModelStreamPart> parts)
    {
        if (includeRaw)
        {
            parts.Add(new RawStreamPart(data));
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(data);
        }
        catch (JsonException exception)
        {
            finishError = true;
            finishRaw = null;
            parts.Add(new ErrorStreamPart(new JsonParseException(data, exception).Message));
            return;
        }

        using (document)
        {
            var root = document.RootElement;
            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.Object)
            {
                finishError = true;
                finishRaw = null;
                var message = JsonValues.GetString(error, "message") ?? "Unknown provider error.";
                parts.Add(new ErrorStreamPart(message));
                return;
            }

            if (!sawMetadata)
            {
                sawMetadata = true;
                var created = JsonValues.GetInt(root, "created");
                parts.Add(new ResponseMetadataStreamPart(
                    JsonValues.GetString(root, "id"),
                    JsonValues.GetString(root, "model"),
                    created == null ? null : DateTimeOffset.FromUnixTimeSeconds(created.Value)));
            }

            if (root.TryGetProperty("x_groq", out var groq)
                && groq.ValueKind == JsonValueKind.Object
                && groq.TryGetProperty("usage", out var groqUsage)
                && groqUsage.ValueKind == JsonValueKind.Object)
            {
                rawUsage = groqUsage.Clone();
            }

            if (!root.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array || choices.GetArrayLength() == 0)
            {
                return;
            }

            var choice = choices[0];
            var finish = JsonValues.GetString(choice, "finish_reason");
            if (!string.IsNullOrEmpty(finish))
            {
                finishRaw = finish;
            }

            if (!choice.TryGetProperty("delta", out var delta) || delta.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            var reasoning = JsonValues.GetString(delta, "reasoning");
            if (!string.IsNullOrEmpty(reasoning))
            {
                if (!activeReasoning)
                {
                    parts.Add(new ReasoningStartStreamPart("reasoning-0"));
                    activeReasoning = true;
                }

                parts.Add(new ReasoningDeltaStreamPart("reasoning-0", reasoning!));
            }

            var text = JsonValues.GetString(delta, "content");
            if (!string.IsNullOrEmpty(text))
            {
                if (activeReasoning)
                {
                    parts.Add(new ReasoningEndStreamPart("reasoning-0"));
                    activeReasoning = false;
                }

                if (!activeText)
                {
                    parts.Add(new TextStartStreamPart("txt-0"));
                    activeText = true;
                }

                parts.Add(new TextDeltaStreamPart("txt-0", text!));
            }

            if (!delta.TryGetProperty("tool_calls", out var toolCalls) || toolCalls.ValueKind != JsonValueKind.Array || toolCalls.GetArrayLength() == 0)
            {
                return;
            }

            if (activeReasoning)
            {
                parts.Add(new ReasoningEndStreamPart("reasoning-0"));
                activeReasoning = false;
            }

            foreach (var toolCall in toolCalls.EnumerateArray())
            {
                var index = JsonValues.GetInt(toolCall, "index");
                var id = JsonValues.GetString(toolCall, "id");
                string? name = null;
                string? arguments = null;
                var hasArguments = false;
                if (toolCall.TryGetProperty("function", out var function) && function.ValueKind == JsonValueKind.Object)
                {
                    if (function.TryGetProperty("name", out var nameElement) && nameElement.ValueKind == JsonValueKind.String)
                    {
                        name = nameElement.GetString();
                    }

                    if (function.TryGetProperty("arguments", out var argumentsElement) && argumentsElement.ValueKind == JsonValueKind.String)
                    {
                        arguments = argumentsElement.GetString() ?? string.Empty;
                        hasArguments = true;
                    }
                }

                if (index != null && !string.IsNullOrEmpty(id))
                {
                    indexIds[index.Value] = id!;
                }

                var resolvedId = id;
                if (string.IsNullOrEmpty(resolvedId) && index != null && indexIds.TryGetValue(index.Value, out var known))
                {
                    resolvedId = known;
                }

                if (!string.IsNullOrEmpty(name) && index != null && started.Add(index.Value))
                {
                    parts.Add(new GroqToolInputStartStreamPart(resolvedId ?? string.Empty, name!));
                }

                if (hasArguments)
                {
                    parts.Add(new GroqToolInputDeltaStreamPart(resolvedId ?? string.Empty, arguments ?? string.Empty));
                }

                tracker.ProcessDelta(new StreamingToolCallDelta(index, id, name, hasArguments ? arguments : null));
            }
        }
    }

    private (JsonObject Body, List<CallWarning> Warnings) Build(LanguageModelCallOptions options, bool stream)
    {
        var warnings = new List<CallWarning>();
        JsonElement? groqElement = null;
        if (options.ProviderOptions != null && options.ProviderOptions.TryGetValue("groq", out var providerOptions))
        {
            groqElement = providerOptions;
        }

        var parsed = GroqChatOptions.TryParse(groqElement);
        if (!parsed.Success || parsed.Options == null)
        {
            throw new InvalidArgumentException("providerOptions.groq", parsed.Error ?? "Invalid Groq provider options.");
        }

        var groq = parsed.Options;
        var structuredOutputs = groq.StructuredOutputs ?? true;
        var strictJsonSchema = groq.StrictJsonSchema ?? true;
        if (options.TopK != null)
        {
            warnings.Add(GroqWarnings.Unsupported("topK"));
        }

        if (options.JsonSchema != null && !structuredOutputs)
        {
            warnings.Add(GroqWarnings.Unsupported(
                "responseFormat",
                "JSON response format schema is only supported with structuredOutputs"));
        }

        var definitions = new List<GroqToolDefinition>();
        if (options.Tools != null)
        {
            foreach (var tool in options.Tools)
            {
                definitions.Add(GroqToolDefinition.Function(tool.Name, tool.Description, tool.InputSchema, tool.Strict));
            }
        }

        var prepared = GroqTools.Prepare(options.Tools == null ? null : definitions, options.ToolChoice, ModelId);
        warnings.AddRange(prepared.Warnings);

        string? reasoningEffort = groq.ReasoningEffort;
        if (reasoningEffort == null && IsCustomReasoning(options.Reasoning))
        {
            var reasoning = options.Reasoning!;
            if (string.Equals(reasoning, "none", StringComparison.Ordinal))
            {
                if (string.Equals(ModelId, "qwen/qwen3.6-27b", StringComparison.Ordinal))
                {
                    reasoningEffort = "none";
                }
                else
                {
                    warnings.Add(GroqWarnings.Unsupported("reasoning", "reasoning \"" + reasoning + "\" is not supported by this model."));
                }
            }
            else
            {
                reasoningEffort = MapEffort(reasoning, warnings);
            }
        }

        var body = new JsonObject { ["model"] = ModelId };
        AddString(body, "user", groq.User);
        if (groq.ParallelToolCalls != null)
        {
            body["parallel_tool_calls"] = groq.ParallelToolCalls.Value;
        }

        if (options.MaxOutputTokens != null)
        {
            body["max_tokens"] = options.MaxOutputTokens.Value;
        }

        if (options.Temperature != null)
        {
            body["temperature"] = options.Temperature.Value;
        }

        if (options.TopP != null)
        {
            body["top_p"] = options.TopP.Value;
        }

        if (options.FrequencyPenalty != null)
        {
            body["frequency_penalty"] = options.FrequencyPenalty.Value;
        }

        if (options.PresencePenalty != null)
        {
            body["presence_penalty"] = options.PresencePenalty.Value;
        }

        if (options.StopSequences != null)
        {
            var stop = new JsonArray();
            foreach (var sequence in options.StopSequences)
            {
                stop.Add(sequence);
            }

            body["stop"] = stop;
        }

        if (options.Seed != null)
        {
            body["seed"] = options.Seed.Value;
        }

        if (options.JsonSchema != null)
        {
            if (structuredOutputs)
            {
                var schema = new JsonObject
                {
                    ["schema"] = JsonNode.Parse(options.JsonSchema.Value.GetRawText()) ?? new JsonObject(),
                    ["strict"] = strictJsonSchema,
                    ["name"] = string.IsNullOrEmpty(options.JsonSchemaName) ? "response" : options.JsonSchemaName,
                };
                if (!string.IsNullOrEmpty(groq.ResponseFormatDescription))
                {
                    schema["description"] = groq.ResponseFormatDescription;
                }

                body["response_format"] = new JsonObject
                {
                    ["type"] = "json_schema",
                    ["json_schema"] = schema,
                };
            }
            else
            {
                body["response_format"] = new JsonObject { ["type"] = "json_object" };
            }
        }

        AddString(body, "reasoning_format", groq.ReasoningFormat);
        AddString(body, "reasoning_effort", reasoningEffort);
        AddString(body, "service_tier", groq.ServiceTier);
        body["messages"] = GroqChatMessages.Convert(options.Prompt);
        if (prepared.Tools != null)
        {
            body["tools"] = prepared.Tools;
        }

        if (prepared.ToolChoice != null)
        {
            body["tool_choice"] = prepared.ToolChoice;
        }

        if (stream)
        {
            body["stream"] = true;
        }

        return (body, warnings);
    }

    private Dictionary<string, string?> Headers(LanguageModelCallOptions options)
    {
        var headers = _provider.CreateHeaders();
        if (!headers.ContainsKey("User-Agent"))
        {
            headers["User-Agent"] = GroqProvider.UserAgent;
        }

        if (options.Headers != null)
        {
            foreach (var pair in options.Headers)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        return headers;
    }

    private static bool IsCustomReasoning(string? reasoning)
    {
        return reasoning != null && !string.Equals(reasoning, "provider-default", StringComparison.Ordinal);
    }

    private static string? MapEffort(string reasoning, List<CallWarning> warnings)
    {
        string? mapped;
        switch (reasoning)
        {
            case "minimal":
                mapped = "low";
                break;
            case "low":
                mapped = "low";
                break;
            case "medium":
                mapped = "medium";
                break;
            case "high":
                mapped = "high";
                break;
            case "xhigh":
                mapped = "high";
                break;
            default:
                warnings.Add(GroqWarnings.Unsupported("reasoning", "reasoning \"" + reasoning + "\" is not supported by this model."));
                return null;
        }

        if (!string.Equals(mapped, reasoning, StringComparison.Ordinal))
        {
            warnings.Add(GroqWarnings.Compatibility(
                "reasoning",
                "reasoning \"" + reasoning + "\" is not directly supported by this model. mapped to effort \"" + mapped + "\"."));
        }

        return mapped;
    }

    private static void AddString(JsonObject body, string name, string? value)
    {
        if (value != null)
        {
            body[name] = value;
        }
    }
}
