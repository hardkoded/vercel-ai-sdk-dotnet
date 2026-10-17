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

namespace Vercel.AI.Xai;

/// <summary>Reasoning text that carries the xAI Responses item id and encrypted content.</summary>
public sealed class XaiReasoning : GeneratedReasoning
{
    /// <summary>Creates reasoning.</summary>
    public XaiReasoning(string text, JsonElement? providerMetadata)
        : base(text, providerMetadata)
    {
    }
}

/// <summary>
/// xAI Responses API language model. Generate and stream requests post to <c>/responses</c>.
/// Pass xAI provider-defined tools with <see cref="LanguageModelTool.ProviderDefined"/> and the ids <c>xai.web_search</c>,
/// <c>xai.x_search</c>, <c>xai.code_execution</c>, <c>xai.view_image</c>, <c>xai.view_x_video</c>, <c>xai.image_generation</c>,
/// <c>xai.file_search</c>, and <c>xai.mcp</c>.
/// </summary>
public sealed class XaiResponsesLanguageModel : ILanguageModel
{
    private static readonly Dictionary<string, string> BaseEffortMap = new Dictionary<string, string>
    {
        { "minimal", "low" },
        { "low", "low" },
        { "medium", "medium" },
        { "high", "high" },
    };

    private readonly XaiProvider _provider;
    private readonly Func<string> _generateId;

    /// <summary>Creates a Responses model for <paramref name="modelId"/>.</summary>
    public XaiResponsesLanguageModel(XaiProvider provider, string modelId)
        : this(provider, modelId, IdGenerator.Generate)
    {
    }

    internal XaiResponsesLanguageModel(XaiProvider provider, string modelId, Func<string> generateId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _generateId = generateId;
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => XaiProvider.ProviderId + ".responses";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        var prepared = Prepare(options, stream: false);
        var http = await _provider.Http.SendJsonStringAsync(
            HttpMethod.Post,
            RequestUri(),
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
                return Parse(document.RootElement, prepared, http.Body, http.Headers);
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
        var session = new XaiResponsesStream(_generateId, prepared.ToolNames);
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
        var xai = XaiResponsesOptions.Parse(options.ProviderOptions);

        if (options.FrequencyPenalty.HasValue)
        {
            warnings.Add(new CallWarning("unsupported", "frequencyPenalty"));
        }

        if (options.PresencePenalty.HasValue)
        {
            warnings.Add(new CallWarning("unsupported", "presencePenalty"));
        }

        if (options.StopSequences != null)
        {
            warnings.Add(new CallWarning("unsupported", "stopSequences"));
        }

        var input = XaiResponsesInput.Convert(options.Prompt, warnings);
        var tools = XaiResponsesTools.Prepare(options.Tools, options.ToolChoice);
        warnings.AddRange(tools.Warnings);
        var include = xai.Include == null ? null : new List<string>(xai.Include);
        if (xai.Store == false)
        {
            // Without a stored response, the encrypted reasoning is the only way to carry reasoning to the next turn.
            include ??= new List<string>();
            include.Add("reasoning.encrypted_content");
        }

        var body = new JsonObject { ["model"] = ModelId, ["input"] = input };
        if (xai.Logprobs == true || xai.TopLogprobs.HasValue)
        {
            body["logprobs"] = true;
        }

        OpenAIJson.Set(body, "top_logprobs", xai.TopLogprobs.HasValue ? JsonValue.Create(xai.TopLogprobs.Value) : null);
        OpenAIJson.Set(body, "max_output_tokens", options.MaxOutputTokens.HasValue ? JsonValue.Create(options.MaxOutputTokens.Value) : null);
        OpenAIJson.Set(body, "temperature", options.Temperature.HasValue ? JsonValue.Create(options.Temperature.Value) : null);
        OpenAIJson.Set(body, "top_p", options.TopP.HasValue ? JsonValue.Create(options.TopP.Value) : null);
        OpenAIJson.Set(body, "top_k", options.TopK.HasValue ? JsonValue.Create(options.TopK.Value) : null);
        OpenAIJson.Set(body, "min_p", xai.MinP.HasValue ? JsonValue.Create(xai.MinP.Value) : null);
        OpenAIJson.Set(body, "seed", options.Seed.HasValue ? JsonValue.Create(options.Seed.Value) : null);

        if (options.JsonSchema is { } schema && schema.ValueKind != JsonValueKind.Undefined && schema.ValueKind != JsonValueKind.Null)
        {
            var format = new JsonObject
            {
                ["type"] = "json_schema",
                ["strict"] = true,
                ["name"] = string.IsNullOrEmpty(options.JsonSchemaName) ? "response" : options.JsonSchemaName,
            };
            OpenAIJson.Set(format, "description", options.JsonSchemaDescription == null ? null : JsonValue.Create(options.JsonSchemaDescription));
            format["schema"] = JsonNode.Parse(schema.GetRawText());
            body["text"] = new JsonObject { ["format"] = format };
        }
        else if (options.JsonResponse)
        {
            body["text"] = new JsonObject { ["format"] = new JsonObject { ["type"] = "json_object" } };
        }

        var effort = ResolveReasoningEffort(xai.ReasoningEffort, options.Reasoning, warnings);
        if (effort != null)
        {
            body["reasoning"] = new JsonObject { ["effort"] = effort };
        }

        if (xai.Store == false)
        {
            body["store"] = false;
        }

        if (include != null)
        {
            var includeArray = new JsonArray();
            foreach (var value in include)
            {
                includeArray.Add(value);
            }

            body["include"] = includeArray;
        }

        OpenAIJson.Set(body, "previous_response_id", xai.PreviousResponseId == null ? null : JsonValue.Create(xai.PreviousResponseId));
        OpenAIJson.Set(body, "max_turns", xai.MaxTurns.HasValue ? JsonValue.Create(xai.MaxTurns.Value) : null);
        OpenAIJson.Set(body, "parallel_tool_calls", xai.ParallelToolCalls.HasValue ? JsonValue.Create(xai.ParallelToolCalls.Value) : null);
        OpenAIJson.Set(body, "prompt_cache_key", xai.PromptCacheKey == null ? null : JsonValue.Create(xai.PromptCacheKey));
        OpenAIJson.Set(body, "safety_identifier", xai.SafetyIdentifier == null ? null : JsonValue.Create(xai.SafetyIdentifier));
        OpenAIJson.Set(body, "service_tier", xai.ServiceTier == null ? null : JsonValue.Create(xai.ServiceTier));
        OpenAIJson.Set(body, "user", xai.User == null ? null : JsonValue.Create(xai.User));

        OpenAIJson.Set(body, "tools", tools.Tools is { Count: > 0 } ? tools.Tools : null);
        OpenAIJson.Set(body, "tool_choice", tools.ToolChoice);

        if (stream)
        {
            body["stream"] = true;
        }

        return new PreparedRequest(body, warnings, new XaiToolNames(options.Tools));
    }

    private string? ResolveReasoningEffort(string? providerEffort, string? reasoning, List<CallWarning> warnings)
    {
        if (providerEffort != null || !ReasoningMap.IsCustomReasoning(reasoning))
        {
            return providerEffort;
        }

        if (!XaiReasoningEffort.Supports(ModelId))
        {
            warnings.Add(new CallWarning("unsupported", "reasoning \"" + reasoning + "\" is not supported by this model."));
            return null;
        }

        if (reasoning == "none")
        {
            return "none";
        }

        // Only grok-4.6 accepts xhigh. Other models take high for both xhigh and max.
        var top = ModelId == "grok-4.6" ? "xhigh" : "high";
        var effortMap = new Dictionary<string, string>(BaseEffortMap) { { "xhigh", top }, { "max", top } };
        var mappingWarnings = new List<ModelWarning>();
        var effort = ReasoningMap.MapReasoningToProviderEffort(reasoning!, effortMap, mappingWarnings);
        foreach (var warning in mappingWarnings)
        {
            warnings.Add(new CallWarning(warning.Type, (warning as CompatibilityWarning)?.Details ?? (warning as UnsupportedWarning)?.Details ?? string.Empty));
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
        IReadOnlyDictionary<string, string> headers)
    {
        if (root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty("output", out var output)
            || output.ValueKind != JsonValueKind.Array
            || OpenAIJson.String(root, "status") is not { } status)
        {
            throw new InvalidOperationException("The response has no output or status.");
        }

        var content = new List<GeneratedContent>();
        var hasFunctionCall = false;
        foreach (var item in output.EnumerateArray())
        {
            var type = OpenAIJson.String(item, "type");
            if (XaiResponsesOutput.IsProviderToolItem(type))
            {
                AddProviderToolItem(type!, item, prepared.ToolNames, content);
                continue;
            }

            switch (type)
            {
                case "message":
                    AddMessage(item, content);
                    break;
                case "function_call":
                    hasFunctionCall = true;
                    content.Add(new GeneratedToolCall(
                        OpenAIJson.String(item, "call_id") ?? string.Empty,
                        OpenAIJson.String(item, "name") ?? string.Empty,
                        OpenAIJson.String(item, "arguments") ?? "{}"));
                    break;
                case "reasoning":
                    AddReasoning(item, content);
                    break;
            }
        }

        LanguageModelUsage usage;
        JsonNode? cost = null;
        if (OpenAIJson.Child(root, "usage") is { } usageElement)
        {
            usage = XaiResponsesResults.ConvertUsage(usageElement);
            cost = OpenAIJson.Node(OpenAIJson.Child(usageElement, "cost_in_usd_ticks"));
        }
        else
        {
            usage = XaiResponsesResults.MissingUsage();
        }

        return new LanguageModelGenerateResult(
            content,
            hasFunctionCall ? FinishReason.ToolCalls : XaiResponsesResults.MapFinishReason(status),
            usage,
            status,
            prepared.Warnings,
            OpenAIJson.String(root, "id"),
            XaiResponsesStream.ProviderMetadata(
                cost,
                OpenAIJson.String(root, "service_tier"),
                OpenAIJson.String(root, "prompt_cache_key"),
                OpenAIJson.String(root, "safety_identifier")),
            raw,
            OpenAIJson.String(root, "model"),
            XaiResponsesStream.CreatedAt(root),
            headers);
    }

    private static void AddProviderToolItem(string type, JsonElement item, XaiToolNames toolNames, List<GeneratedContent> content)
    {
        var id = OpenAIJson.String(item, "id") ?? throw new InvalidOperationException("missing id");
        var toolName = toolNames.Resolve(type, OpenAIJson.String(item, "name"));
        content.Add(new GeneratedToolCall(id, toolName, XaiResponsesOutput.ToolInput(type, item)) { ProviderExecuted = true });
        switch (type)
        {
            case "file_search_call":
                content.Add(new GeneratedToolResult(id, toolName, XaiResponsesOutput.FileSearchResult(item).ToJsonString()));
                break;
            case "image_generation_call":
                var (result, isError) = XaiResponsesOutput.ImageGenerationResult(item);
                content.Add(new GeneratedToolResult(id, toolName, result.ToJsonString(), isError));
                break;
            case "web_search_call":
                content.Add(new GeneratedToolResult(id, toolName, XaiResponsesOutput.WebSearchResult(OpenAIJson.Child(item, "action")).ToJsonString()));
                break;
        }
    }

    private void AddMessage(JsonElement item, List<GeneratedContent> content)
    {
        if (OpenAIJson.Child(item, "content") is not { ValueKind: JsonValueKind.Array } parts)
        {
            return;
        }

        foreach (var part in parts.EnumerateArray())
        {
            if (OpenAIJson.String(part, "text") is { Length: > 0 } text)
            {
                content.Add(new GeneratedText(text));
            }

            if (OpenAIJson.Child(part, "annotations") is not { ValueKind: JsonValueKind.Array } annotations)
            {
                continue;
            }

            foreach (var annotation in annotations.EnumerateArray())
            {
                if (XaiResponsesStream.Citation(annotation, _generateId) is { } source)
                {
                    content.Add(source);
                }
            }
        }
    }

    private static void AddReasoning(JsonElement item, List<GeneratedContent> content)
    {
        var texts = new List<string>();
        var summary = OpenAIJson.Child(item, "summary");
        if (summary is { ValueKind: JsonValueKind.Array } summaryParts && summaryParts.GetArrayLength() > 0)
        {
            texts.AddRange(Texts(summaryParts));
        }
        else if (OpenAIJson.Child(item, "content") is { ValueKind: JsonValueKind.Array } contentParts)
        {
            texts.AddRange(Texts(contentParts));
        }

        var text = string.Concat(texts.Where(value => value.Length > 0));
        var encrypted = OpenAIJson.String(item, "encrypted_content");
        if (text.Length == 0 && string.IsNullOrEmpty(encrypted))
        {
            return;
        }

        var itemId = OpenAIJson.String(item, "id");
        var metadata = XaiResponsesStream.ReasoningMetadata(itemId, encrypted);
        content.Add(new XaiReasoning(text, metadata));
    }

    private static IEnumerable<string> Texts(JsonElement parts)
    {
        foreach (var part in parts.EnumerateArray())
        {
            yield return OpenAIJson.String(part, "text") ?? string.Empty;
        }
    }

    internal sealed class PreparedRequest
    {
        public PreparedRequest(JsonObject body, List<CallWarning> warnings, XaiToolNames toolNames)
        {
            Body = body;
            Warnings = warnings;
            ToolNames = toolNames;
        }

        public JsonObject Body { get; }

        public List<CallWarning> Warnings { get; }

        /// <summary>Names of the provider-defined tools in the request, used to name provider-executed calls.</summary>
        public XaiToolNames ToolNames { get; }
    }
}
