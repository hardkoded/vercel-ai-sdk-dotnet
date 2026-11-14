// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.DeepSeek;

/// <summary>
/// DeepSeek Responses API language model. Generate and stream requests post to <c>/responses</c>.
/// It supports the server-side <see cref="DeepSeekTools.WebSearch"/> tool. Maps to <c>DeepSeekResponsesLanguageModel</c>.
/// </summary>
public sealed class DeepSeekResponsesLanguageModel : ILanguageModel
{
    private static readonly Dictionary<string, string> EffortMap = new Dictionary<string, string>
    {
        { "minimal", "low" },
        { "low", "low" },
        { "medium", "high" },
        { "high", "high" },
        { "xhigh", "max" },
        { "max", "max" },
    };

    private readonly DeepSeekProvider _provider;

    /// <summary>Creates a Responses model for <paramref name="modelId"/>.</summary>
    public DeepSeekResponsesLanguageModel(DeepSeekProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => ProviderOptionsName + ".responses";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>URL patterns the model accepts without downloading: <c>image/*</c> over http or https. Maps to <c>supportedUrls</c>.</summary>
    public IReadOnlyDictionary<string, string> SupportedUrls { get; } = new Dictionary<string, string>
    {
        ["image/*"] = "^https?://.*\\z",
    };

    private string ProviderOptionsName => _provider.Options.ProviderName;

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        var prepared = Prepare(options, stream: false);
        var uri = RequestUri();
        var http = await _provider.Http.SendJsonStringAsync(
            HttpMethod.Post,
            uri,
            prepared.Body.ToJsonString(),
            _provider.CreateHeaders(options?.Headers),
            cancellationToken).ConfigureAwait(false);
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(string.IsNullOrWhiteSpace(http.Body) ? "null" : http.Body);
        }
        catch (JsonException)
        {
            throw new ApiException("Invalid JSON response", 200, http.Body);
        }

        using (document)
        {
            try
            {
                return Parse(document.RootElement, prepared, http.Body, http.Headers, uri);
            }
            catch (InvalidOperationException exception)
            {
                throw new ApiException("Invalid JSON response: " + exception.Message, 200, http.Body, exception);
            }
        }
    }

    /// <inheritdoc />
    public async Task<LanguageModelStreamResult> DoStreamAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        var prepared = Prepare(options, stream: true);
        var json = prepared.Body.ToJsonString();
        var sse = await _provider.Http.OpenSseResponseAsync(RequestUri(), json, _provider.CreateHeaders(options?.Headers), cancellationToken).ConfigureAwait(false);
        return sse.ToStreamResult(StreamAsync(options, prepared, sse.Events, cancellationToken), json);
    }

    private async IAsyncEnumerable<LanguageModelStreamPart> StreamAsync(
        LanguageModelCallOptions? options,
        PreparedRequest prepared,
        IAsyncEnumerable<string> events,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var session = new DeepSeekResponsesStream(ProviderOptionsName, prepared.WebSearchToolName ?? "web_search");
        var enumerator = events.GetAsyncEnumerator(cancellationToken);
        try
        {
            var more = await enumerator.MoveNextAsync().ConfigureAwait(false);
            yield return new StreamStartStreamPart(prepared.Warnings);
            while (more)
            {
                foreach (var part in session.Accept(enumerator.Current, options?.IncludeRawChunks == true))
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

    internal PreparedRequest Prepare(LanguageModelCallOptions? options, bool stream)
    {
        options ??= new LanguageModelCallOptions();
        var warnings = new List<CallWarning>();
        var deepseek = DeepSeekResponsesOptions.Parse(options.ProviderOptions, ProviderOptionsName);

        if (options.TopK.HasValue)
        {
            warnings.Add(new CallWarning("unsupported", "topK", null));
        }

        if (options.Seed.HasValue)
        {
            warnings.Add(new CallWarning("unsupported", "seed", null));
        }

        if (options.FrequencyPenalty.HasValue)
        {
            warnings.Add(new CallWarning("unsupported", "frequencyPenalty", null));
        }

        if (options.PresencePenalty.HasValue)
        {
            warnings.Add(new CallWarning("unsupported", "presencePenalty", null));
        }

        if (options.StopSequences is { Count: > 0 })
        {
            warnings.Add(new CallWarning("unsupported", "stopSequences", null));
        }

        var input = DeepSeekResponsesInput.Convert(options.Prompt, ProviderOptionsName, warnings);
        var tools = DeepSeekResponsesTools.Prepare(options.Tools, options.ToolChoice);
        warnings.AddRange(tools.Warnings);
        var webSearchToolName = WebSearchToolNameOf(options.Tools);

        var effort = deepseek.ThinkingType == "disabled" ? "none" : deepseek.ReasoningEffort ?? MapReasoning(options.Reasoning, warnings);

        var body = new JsonObject { ["model"] = ModelId, ["input"] = input };
        OpenAIJson.Set(body, "max_output_tokens", options.MaxOutputTokens.HasValue ? JsonValue.Create(options.MaxOutputTokens.Value) : null);
        OpenAIJson.Set(body, "temperature", options.Temperature.HasValue ? JsonValue.Create(options.Temperature.Value) : null);
        OpenAIJson.Set(body, "top_p", options.TopP.HasValue ? JsonValue.Create(options.TopP.Value) : null);
        OpenAIJson.Set(body, "top_logprobs", deepseek.TopLogprobs.HasValue ? JsonValue.Create(deepseek.TopLogprobs.Value) : null);
        if (effort != null)
        {
            body["reasoning"] = new JsonObject { ["effort"] = effort };
        }

        if (options.JsonSchema is { } schema && schema.ValueKind != JsonValueKind.Undefined && schema.ValueKind != JsonValueKind.Null)
        {
            body["text"] = new JsonObject
            {
                ["format"] = new JsonObject
                {
                    ["type"] = "json_schema",
                    ["name"] = options.JsonSchemaName ?? "response",
                    ["schema"] = JsonNode.Parse(schema.GetRawText()),
                },
            };
        }
        else if (options.JsonResponse)
        {
            body["text"] = new JsonObject { ["format"] = new JsonObject { ["type"] = "json_object" } };
        }

        OpenAIJson.Set(body, "tools", tools.Tools is { Count: > 0 } ? tools.Tools : null);
        OpenAIJson.Set(body, "tool_choice", tools.ToolChoice);
        OpenAIJson.Set(body, "user", deepseek.UserId == null ? null : JsonValue.Create(deepseek.UserId));
        if (stream)
        {
            body["stream"] = true;
        }

        return new PreparedRequest(body, warnings, webSearchToolName);
    }

    private static string? WebSearchToolNameOf(IReadOnlyList<LanguageModelTool>? tools)
    {
        return tools?.FirstOrDefault(tool => tool.ProviderToolId == DeepSeekTools.WebSearchToolId)?.Name;
    }

    private static string? MapReasoning(string? reasoning, List<CallWarning> warnings)
    {
        if (!ReasoningMap.IsCustomReasoning(reasoning))
        {
            return null;
        }

        if (reasoning == "none")
        {
            return "none";
        }

        var mappingWarnings = new List<ModelWarning>();
        var effort = ReasoningMap.MapReasoningToProviderEffort(reasoning!, EffortMap, mappingWarnings);
        foreach (var warning in mappingWarnings)
        {
            if (ReasoningMap.ToCallWarning(warning) is { } callWarning)
            {
                warnings.Add(callWarning);
            }
        }

        return effort;
    }

    private Uri RequestUri()
    {
        return ApiKeys.Combine(_provider.Options.BaseUrl, "responses");
    }

    private LanguageModelGenerateResult Parse(
        JsonElement root,
        PreparedRequest prepared,
        string raw,
        IReadOnlyDictionary<string, string> headers,
        Uri uri)
    {
        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("The response must be an object.");
        }

        var status = OpenAIJson.String(root, "status");
        var error = OpenAIJson.Child(root, "error");
        if (status == "failed" || error is { ValueKind: JsonValueKind.Object })
        {
            var code = OpenAIJson.String(error, "code");
            var (statusCode, isRetryable) = FailedResponseStatus(code);
            throw new ApiException(OpenAIJson.String(error, "message") ?? "DeepSeek response failed", statusCode, raw)
            {
                Url = uri.ToString(),
                RequestBodyValues = prepared.Body,
                ResponseHeaders = headers,
                IsRetryable = isRetryable,
            };
        }

        var content = new List<GeneratedContent>();
        var logprobs = new List<JsonElement>();
        var hasFunctionCall = false;
        if (OpenAIJson.Child(root, "output") is { ValueKind: JsonValueKind.Array } output)
        {
            foreach (var item in output.EnumerateArray())
            {
                switch (OpenAIJson.String(item, "type"))
                {
                    case "reasoning":
                        var itemId = OpenAIJson.String(item, "id");
                        content.Add(new GeneratedReasoning(
                            JoinText(OpenAIJson.Child(item, "content")),
                            itemId == null ? null : ItemIdMetadata(ProviderOptionsName, itemId)));
                        break;
                    case "message":
                        if (OpenAIJson.Child(item, "content") is { ValueKind: JsonValueKind.Array } parts)
                        {
                            foreach (var part in parts.EnumerateArray())
                            {
                                if (OpenAIJson.Child(part, "logprobs") is { ValueKind: JsonValueKind.Array } partLogprobs)
                                {
                                    logprobs.AddRange(partLogprobs.EnumerateArray().Select(entry => entry.Clone()));
                                }

                                if (OpenAIJson.String(part, "text") is { Length: > 0 } text)
                                {
                                    content.Add(new GeneratedText(text));
                                }
                            }
                        }

                        break;
                    case "function_call":
                        hasFunctionCall = true;
                        content.Add(new GeneratedToolCall(
                            Required(item, "call_id"),
                            Required(item, "name"),
                            OpenAIJson.String(item, "arguments") ?? string.Empty));
                        break;
                    case "web_search_call":
                        var toolName = prepared.WebSearchToolName ?? "web_search";
                        var id = Required(item, "id");
                        content.Add(new GeneratedToolCall(id, toolName, "{}", WebSearchCallMetadata(ProviderOptionsName, item)) { ProviderExecuted = true });
                        content.Add(new GeneratedToolResult(id, toolName, WebSearchResult(item).ToJsonString()));
                        break;
                }
            }
        }

        var usageElement = OpenAIJson.Child(root, "usage");
        var incompleteReason = OpenAIJson.String(OpenAIJson.Child(root, "incomplete_details"), "reason");
        return new LanguageModelGenerateResult(
            content,
            DeepSeekResponsesResults.MapFinishReason(status, incompleteReason, hasFunctionCall),
            DeepSeekResponsesResults.ConvertUsage(usageElement),
            incompleteReason ?? status,
            prepared.Warnings,
            OpenAIJson.String(root, "id"),
            DeepSeekResponsesResults.ProviderMetadata(ProviderOptionsName, OpenAIJson.String(root, "id"), usageElement, logprobs),
            raw,
            OpenAIJson.String(root, "model"),
            CreatedAt(root),
            headers);
    }

    internal static DateTimeOffset? CreatedAt(JsonElement response)
    {
        return OpenAIJson.Child(response, "created_at") is { ValueKind: JsonValueKind.Number } created && created.TryGetInt64(out var seconds)
            ? DateTimeOffset.FromUnixTimeSeconds(seconds)
            : null;
    }

    internal static (int StatusCode, bool IsRetryable) FailedResponseStatus(string? code)
    {
        switch (code)
        {
            case "rate_limit_exceeded":
            case "rate_limit_error":
                return (429, true);
            case "insufficient_quota":
                return (429, false);
            case "authentication_error":
            case "invalid_api_key":
                return (401, false);
            case "permission_error":
                return (403, false);
            case "bad_request":
            case "context_length_exceeded":
            case "invalid_request_error":
                return (400, false);
            case "timeout":
            case "timeout_error":
                return (504, true);
            default:
                return (500, true);
        }
    }

    internal static string JoinText(JsonElement? parts)
    {
        if (parts is not { ValueKind: JsonValueKind.Array } array)
        {
            return string.Empty;
        }

        return string.Concat(array.EnumerateArray().Select(part => OpenAIJson.String(part, "text") ?? string.Empty));
    }

    internal static JsonElement ItemIdMetadata(string providerOptionsName, string itemId)
    {
        return OpenAIJson.Element(new JsonObject { [providerOptionsName] = new JsonObject { ["itemId"] = itemId } });
    }

    internal static JsonElement WebSearchCallMetadata(string providerOptionsName, JsonElement item)
    {
        return OpenAIJson.Element(new JsonObject
        {
            [providerOptionsName] = new JsonObject { ["webSearchCall"] = JsonNode.Parse(item.GetRawText()) },
        });
    }

    internal static JsonObject WebSearchResult(JsonElement item)
    {
        var result = new JsonObject();
        if (OpenAIJson.String(item, "status") is { } status)
        {
            result["status"] = status;
        }

        if (OpenAIJson.Child(item, "action") is { ValueKind: JsonValueKind.Object } action)
        {
            result["action"] = JsonNode.Parse(action.GetRawText());
        }

        return result;
    }

    internal static string Required(JsonElement element, string name)
    {
        return OpenAIJson.String(element, name) ?? throw new InvalidOperationException("missing " + name);
    }

    internal sealed class PreparedRequest
    {
        public PreparedRequest(JsonObject body, List<CallWarning> warnings, string? webSearchToolName)
        {
            Body = body;
            Warnings = warnings;
            WebSearchToolName = webSearchToolName;
        }

        public JsonObject Body { get; }

        public List<CallWarning> Warnings { get; }

        /// <summary>Name of the <c>deepseek.web_search</c> tool in the request, used to name provider-executed calls.</summary>
        public string? WebSearchToolName { get; }
    }
}

/// <summary>Usage, finish reason, and provider metadata of a DeepSeek Responses call.</summary>
internal static class DeepSeekResponsesResults
{
    /// <summary>Converts a Responses <c>usage</c> object. Maps to <c>convertDeepSeekResponsesUsage</c>.</summary>
    public static LanguageModelUsage ConvertUsage(JsonElement? usage)
    {
        if (usage is not { ValueKind: JsonValueKind.Object } value)
        {
            return LanguageModelUsage.Empty;
        }

        var input = OpenAIJson.Int(value, "input_tokens") ?? 0;
        var output = OpenAIJson.Int(value, "output_tokens") ?? 0;
        var cacheRead = OpenAIJson.NestedInt(value, "input_tokens_details", "cached_tokens") ?? 0;
        var reasoning = OpenAIJson.NestedInt(value, "output_tokens_details", "reasoning_tokens") ?? 0;
        return new LanguageModelUsage(
            input,
            output,
            null,
            cacheReadTokens: cacheRead,
            reasoningTokens: reasoning,
            raw: value.Clone(),
            noCacheInputTokens: input - cacheRead,
            textTokens: Math.Max(0, output - reasoning));
    }

    /// <summary>The successful search and fetch counts of <c>usage.server_tool_use</c>, or null. Maps to <c>getDeepSeekServerToolUse</c>.</summary>
    public static JsonObject? ServerToolUse(JsonElement? usage)
    {
        if (usage is not { ValueKind: JsonValueKind.Object } value
            || OpenAIJson.Child(value, "server_tool_use") is not { ValueKind: JsonValueKind.Object } serverToolUse)
        {
            return null;
        }

        return new JsonObject
        {
            ["toolSearch"] = OpenAIJson.Int(serverToolUse, "tool_search") ?? 0,
            ["toolFetch"] = OpenAIJson.Int(serverToolUse, "tool_fetch") ?? 0,
        };
    }

    /// <summary>Maps a Responses status. Maps to <c>mapDeepSeekResponsesFinishReason</c>.</summary>
    public static FinishReason MapFinishReason(string? status, string? incompleteReason, bool hasFunctionCall)
    {
        switch (status)
        {
            case "completed":
                return hasFunctionCall ? FinishReason.ToolCalls : FinishReason.Stop;
            case "incomplete":
                return incompleteReason == "max_output_tokens" ? FinishReason.Length
                    : incompleteReason == "content_filter" ? FinishReason.ContentFilter
                    : FinishReason.Other;
            case "failed":
                return FinishReason.Error;
            default:
                return FinishReason.Other;
        }
    }

    public static JsonElement ProviderMetadata(string providerOptionsName, string? responseId, JsonElement? usage, IReadOnlyList<JsonElement> logprobs)
    {
        var metadata = new JsonObject();
        if (responseId != null)
        {
            metadata["responseId"] = responseId;
        }

        if (ServerToolUse(usage) is { } serverToolUse)
        {
            metadata["serverToolUse"] = serverToolUse;
        }

        if (logprobs.Count > 0)
        {
            var array = new JsonArray();
            foreach (var entry in logprobs)
            {
                array.Add(JsonNode.Parse(entry.GetRawText()));
            }

            metadata["logprobs"] = array;
        }

        return OpenAIJson.Element(new JsonObject { [providerOptionsName] = metadata });
    }
}
