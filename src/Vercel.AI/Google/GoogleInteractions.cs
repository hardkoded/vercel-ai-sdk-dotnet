// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

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

/// <summary>Converts a prompt into an Interactions <c>input</c> array.</summary>
public static class GoogleInteractionsInput
{
    /// <summary>Converts shared messages. System text is joined into <c>systemInstruction</c>.</summary>
    public static JsonObject Convert(IReadOnlyList<ModelMessage> prompt)
    {
        var system = new List<string>();
        var input = new JsonArray();
        if (prompt != null)
        {
            foreach (var message in ToolModelMessage.Flatten(prompt))
            {
                if (message is SystemModelMessage systemMessage)
                {
                    system.Add(systemMessage.Content);
                }
                else if (message is UserModelMessage user)
                {
                    var content = new JsonArray();
                    foreach (var part in user.Content)
                    {
                        if (part is TextContentPart text)
                        {
                            content.Add(new JsonObject { ["type"] = "text", ["text"] = text.Text });
                        }
                    }

                    input.Add(new JsonObject { ["type"] = "user_input", ["content"] = content });
                }
                else if (message is AssistantModelMessage assistant)
                {
                    if (!string.IsNullOrEmpty(assistant.Text))
                    {
                        input.Add(new JsonObject
                        {
                            ["type"] = "model_output",
                            ["content"] = new JsonArray { new JsonObject { ["type"] = "text", ["text"] = assistant.Text } },
                        });
                    }

                    foreach (var call in assistant.ToolCalls)
                    {
                        input.Add(new JsonObject
                        {
                            ["type"] = "function_call",
                            ["name"] = call.ToolName,
                            ["id"] = call.ToolCallId,
                            ["arguments"] = GoogleJson.ParseObject(call.ArgumentsJson),
                        });
                    }
                }
                else if (message is ToolModelMessage tool)
                {
                    input.Add(new JsonObject
                    {
                        ["type"] = "user_input",
                        ["content"] = new JsonArray
                        {
                            new JsonObject
                            {
                                ["type"] = "function_result",
                                ["name"] = tool.ToolName,
                                ["call_id"] = tool.ToolCallId,
                                ["result"] = GoogleJson.ParseObject(tool.OutputJson),
                            },
                        },
                    });
                }
            }
        }

        var body = new JsonObject { ["input"] = input };
        if (system.Count > 0)
        {
            body["systemInstruction"] = string.Join("\n\n", system);
        }

        return body;
    }
}

/// <summary>Reads text and function calls from an Interactions response.</summary>
public static class GoogleInteractionsOutput
{
    /// <summary>Parses <c>steps</c> into generated content and reports whether a client function call is present.</summary>
    public static (IReadOnlyList<GeneratedContent> Content, bool HasFunctionCall) Parse(JsonObject response)
    {
        var content = new List<GeneratedContent>();
        var function = false;
        if (response["steps"] is JsonArray steps)
        {
            foreach (var stepNode in steps)
            {
                if (stepNode is not JsonObject step)
                {
                    continue;
                }

                var type = (string?)step["type"];
                if (type == "function_call")
                {
                    function = true;
                    content.Add(new GeneratedToolCall((string?)step["id"] ?? "call", (string?)step["name"] ?? string.Empty, step["arguments"]?.ToJsonString() ?? "{}"));
                }

                if (step["content"] is JsonArray parts)
                {
                    foreach (var partNode in parts)
                    {
                        if (partNode is not JsonObject part)
                        {
                            continue;
                        }

                        if ((string?)part["type"] == "text")
                        {
                            content.Add(new GeneratedText((string?)part["text"] ?? string.Empty));
                        }
                        else if ((string?)part["type"] == "function_call")
                        {
                            function = true;
                            content.Add(new GeneratedToolCall((string?)part["id"] ?? "call", (string?)part["name"] ?? string.Empty, part["arguments"]?.ToJsonString() ?? "{}"));
                        }
                    }
                }
            }
        }

        return (content, function);
    }
}

/// <summary>An agent to call through the Interactions API: <c>{ agent: name }</c> or <c>{ managedAgent: name }</c> upstream.</summary>
public sealed class GoogleInteractionsAgent
{
    /// <summary>Creates an agent input for a Gemini agent preset such as <c>deep-research-pro-preview-12-2025</c>.</summary>
    public GoogleInteractionsAgent(string agent)
    {
        Agent = !string.IsNullOrEmpty(agent) ? agent : throw new ArgumentException("The agent name is required.", nameof(agent));
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
            return _provider.ModelProvider.IndexOf("vertex", StringComparison.OrdinalIgnoreCase) >= 0
                ? "google.vertex.interactions"
                : _provider.ModelProvider + ".interactions";
        }
    }

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        var (input, warnings) = Prepare(options);
        var background = Background(options);
        var pollingTimeout = PollingTimeout(options);
        var headers = await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false);
        var baseUrl = InteractionsBase(_provider.Options.BaseUrl);
        var post = await _provider.Http.SendJsonStringAsync(
            HttpMethod.Post,
            ProviderUtils.ApiKeys.Combine(baseUrl, "interactions"),
            GoogleJson.Write(input),
            headers,
            cancellationToken).ConfigureAwait(false);
        var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(post.Body) ? "{}" : post.Body);

        // A background or agent call (such as Gemini 4 long decode, where the server runs every hop) may return a
        // non-terminal status. Poll GET /interactions/{id} until it is terminal so the result matches a synchronous call.
        if (background || _isAgent)
        {
            document = await PollIfNonTerminalAsync(document, baseUrl, headers, pollingTimeout, cancellationToken).ConfigureAwait(false);
        }

        using (document)
        {
            var response = JsonNode.Parse(document.RootElement.GetRawText()) as JsonObject ?? new JsonObject();
            var parsed = GoogleInteractionsOutput.Parse(response);
            var status = (string?)response["status"];

            // The API omits `id` when `store: false`. Surface an interaction id only when the API returned one.
            var interactionId = GoogleInteractionsStreaming.String(response, "id") is { Length: > 0 } id ? id : null;

            // The Interactions API returns the applied tier in the body. The header is a defensive fallback.
            post.Headers.TryGetValue("x-gemini-service-tier", out var headerServiceTier);
            var serviceTier = GoogleInteractionsStreaming.String(response, "service_tier") ?? headerServiceTier;
            var usage = response["usage"] as JsonObject;
            return new LanguageModelGenerateResult(
                parsed.Content,
                GoogleInteractionsFinish.Map(status, parsed.HasFunctionCall),
                Usage(usage),
                status,
                warnings,
                interactionId,
                ProviderMetadata(interactionId, serviceTier, OutputTokensByModality(usage)));
        }
    }

    // The `google` provider metadata of a result: the interaction id, the applied service tier, and the output token breakdown.
    internal static JsonElement ProviderMetadata(string? interactionId, string? serviceTier, JsonObject? outputTokensByModality)
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
            if (entry is JsonObject item && GoogleInteractionsStreaming.String(item, "modality") is { } modality && item["tokens"] is JsonValue tokens)
            {
                result[modality] = tokens.DeepClone();
            }
        }

        return result.Count > 0 ? result : null;
    }

    // Builds the request body. An agent call sends `agent` and no generation_config, because the API rejects it.
    private (JsonObject Body, IReadOnlyList<CallWarning> Warnings) Prepare(LanguageModelCallOptions options)
    {
        var warnings = new List<CallWarning>();
        var thinkingLevel = EnumOption(options, "thinkingLevel", "minimal", "low", "medium", "high");
        var thinkingSummaries = EnumOption(options, "thinkingSummaries", "auto", "none");
        var serviceTier = EnumOption(options, "serviceTier", "flex", "standard", "priority");
        var previousInteractionId = StringOption(options, "previousInteractionId");
        var store = Store(options);

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

        // `response_format` is an array of entries. A JSON call-level response format adds one text entry.
        JsonArray? responseFormat = null;
        if (options.JsonResponse)
        {
            if (_isAgent)
            {
                warnings.Add(new CallWarning("other", "google.interactions: structured output (responseFormat) is not supported when an agent is set; responseFormat will be ignored."));
            }
            else
            {
                var entry = new JsonObject { ["type"] = "text", ["mime_type"] = "application/json" };
                if (options.JsonSchema is { } schema)
                {
                    entry["schema"] = JsonNode.Parse(schema.GetRawText());
                }

                responseFormat = new JsonArray { entry };
            }
        }

        var input = GoogleInteractionsInput.Convert(options.Prompt);
        if (previousInteractionId != null && store == false)
        {
            warnings.Add(new CallWarning("other", "google.interactions: providerOptions.google.previousInteractionId was set together with store: false. These are incoherent (the prior interaction cannot be referenced when nothing was stored on the server); the full history will be sent and previous_interaction_id will still be emitted."));
        }

        input[_isAgent ? "agent" : "model"] = ModelId;
        GoogleJson.Set(input, "response_format", responseFormat);
        GoogleJson.Set(input, "previous_interaction_id", previousInteractionId);
        GoogleJson.Set(input, "service_tier", serviceTier);
        GoogleJson.Set(input, "store", store);
        if (Background(options))
        {
            input["background"] = true;
        }

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
            AddIf(dropped, thinkingLevel != null, "thinkingLevel");
            AddIf(dropped, thinkingSummaries != null, "thinkingSummaries");
            if (dropped.Count > 0)
            {
                warnings.Add(new CallWarning("other", "google.interactions: " + string.Join(", ", dropped) + (dropped.Count == 1 ? " is" : " are") + " not supported when an agent is set; use providerOptions.google.agentConfig instead. Dropped from the request body."));
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
                generationConfig["stop_sequences"] = new JsonArray(stopSequences.Select(sequence => (JsonNode?)JsonValue.Create(sequence)).ToArray());
            }

            GoogleJson.Set(generationConfig, "max_output_tokens", options.MaxOutputTokens);
            GoogleJson.Set(generationConfig, "thinking_level", thinkingLevel);
            GoogleJson.Set(generationConfig, "thinking_summaries", thinkingSummaries);
            if (generationConfig.Count > 0)
            {
                input["generation_config"] = generationConfig;
            }
        }

        return (input, warnings);
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
        return options.ProviderOptions != null
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
            ? value.ValueKind == JsonValueKind.String ? value.GetString() : throw new AiSdkException("providerOptions.google." + name + " must be a string.")
            : null;
    }

    private static string? EnumOption(LanguageModelCallOptions options, string name, params string[] allowed)
    {
        var text = StringOption(options, name);
        if (text != null && Array.IndexOf(allowed, text) < 0)
        {
            throw new AiSdkException("providerOptions.google." + name + " must be one of: " + string.Join(", ", allowed) + ".");
        }

        return text;
    }

    private static bool? Store(LanguageModelCallOptions options)
    {
        return options.ProviderOptions != null
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
        if (options.ProviderOptions != null
            && options.ProviderOptions.TryGetValue("google", out var google)
            && google.ValueKind == JsonValueKind.Object
            && google.TryGetProperty("pollingTimeoutMs", out var value)
            && value.ValueKind != JsonValueKind.Null)
        {
            if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt64(out var milliseconds) || milliseconds <= 0 || milliseconds > int.MaxValue)
            {
                throw new AiSdkException("providerOptions.google.pollingTimeoutMs must be a positive integer of at most 2147483647.");
            }

            return TimeSpan.FromMilliseconds(milliseconds);
        }

        return null;
    }

    private async Task<JsonDocument> PollIfNonTerminalAsync(JsonDocument first, string baseUrl, IReadOnlyDictionary<string, string?> headers, TimeSpan? timeout, CancellationToken cancellationToken)
    {
        var status = first.RootElement.TryGetProperty("status", out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
        if (GoogleInteractionsPolling.IsTerminal(status))
        {
            return first;
        }

        var id = first.RootElement.TryGetProperty("id", out var idValue) && idValue.ValueKind == JsonValueKind.String ? idValue.GetString() : null;
        first.Dispose();
        return await GoogleInteractionsPolling.PollUntilTerminalAsync(_provider.Http, baseUrl, id, headers, null, null, timeout, cancellationToken).ConfigureAwait(false);
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
        int? outputTotal = output == null && thought == null ? null : (output ?? 0) + (thought ?? 0);
        int? noCache = input == null ? null : input - (cached ?? 0);
        var raw = JsonSerializer.SerializeToElement(usage);
        return new LanguageModelUsage(input, outputTotal, input == null && outputTotal == null ? null : (input ?? 0) + (outputTotal ?? 0), cached, null, thought, raw, noCache, output);
    }

    private static int? Count(JsonObject usage, string name)
    {
        return usage[name] is JsonValue value && value.TryGetValue<int>(out var count) ? count : null;
    }

    /// <inheritdoc />
    public async Task<LanguageModelStreamResult> DoStreamAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        if (Background(options))
        {
            return await StreamBackgroundAsync(options, cancellationToken).ConfigureAwait(false);
        }

        return await StreamLiveAsync(options, cancellationToken).ConfigureAwait(false);
    }

    // POSTs with `stream: true` and pipes the SSE feed through the transform.
    private async Task<LanguageModelStreamResult> StreamLiveAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        var (body, warnings) = Prepare(options);
        body["stream"] = true;
        var headers = await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false);
        var requestJson = GoogleJson.Write(body);
        var sse = await _provider.Http.OpenSseResponseAsync(
            ProviderUtils.ApiKeys.Combine(InteractionsBase(_provider.Options.BaseUrl), "interactions"),
            requestJson,
            headers,
            cancellationToken).ConfigureAwait(false);

        // The tier is in the interaction.completed body. The header is the fallback, and the body wins.
        sse.Headers.TryGetValue("x-gemini-service-tier", out var headerServiceTier);
        var transform = new GoogleInteractionsStreamTransform(warnings, options.IncludeRawChunks, _provider.Options.GenerateId, headerServiceTier);
        return sse.ToStreamResult(transform.Pipe(ParseEvents(sse.Events, cancellationToken), cancellationToken), requestJson);
    }

    private static async IAsyncEnumerable<GoogleInteractionsEvent> ParseEvents(IAsyncEnumerable<string> data, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var raw in data.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return new GoogleInteractionsEvent(raw);
        }
    }

    // `background: true` is incompatible with `stream: true` on POST. POST the background call, then stream
    // GET /interactions/{id}?stream=true. A terminal POST response has nothing left to stream, so replay it.
    private async Task<LanguageModelStreamResult> StreamBackgroundAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        var (body, warnings) = Prepare(options);
        var headers = await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false);
        var baseUrl = InteractionsBase(_provider.Options.BaseUrl);
        var requestJson = GoogleJson.Write(body);
        using var post = await _provider.Http.SendJsonAsync(
            HttpMethod.Post,
            ProviderUtils.ApiKeys.Combine(baseUrl, "interactions"),
            requestJson,
            headers,
            cancellationToken).ConfigureAwait(false);
        var response = JsonNode.Parse(post.RootElement.GetRawText()) as JsonObject ?? new JsonObject();
        var id = (string?)response["id"];
        if (string.IsNullOrEmpty(id))
        {
            throw new InvalidOperationException("google.interactions: background POST response did not include an interaction id; cannot stream the result.");
        }

        var transform = new GoogleInteractionsStreamTransform(warnings, options.IncludeRawChunks, _provider.Options.GenerateId);
        var request = LanguageModelStreamRequest.FromJson(requestJson);
        if (GoogleInteractionsPolling.IsTerminal((string?)response["status"]))
        {
            return new LanguageModelStreamResult(ToAsync(transform.Replay(response)), request);
        }

        var events = GoogleInteractionsStreaming.StreamEvents(
            _provider.Http,
            baseUrl,
            id,
            headers,
            GoogleInteractionsStreaming.DefaultMaxRetries,
            GoogleInteractionsStreaming.DefaultRetryDelay,
            cancellationToken);
        return new LanguageModelStreamResult(transform.Pipe(events, cancellationToken), request);
    }

    private static async IAsyncEnumerable<LanguageModelStreamPart> ToAsync(IEnumerable<LanguageModelStreamPart> parts)
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
