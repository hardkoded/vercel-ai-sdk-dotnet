// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Cohere;

/// <summary>Cohere v2 chat model.</summary>
public sealed class CohereLanguageModel : ILanguageModel
{
    private readonly CohereProvider _provider;
    private int _ids;

    /// <summary>Creates a model.</summary>
    public CohereLanguageModel(CohereProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => CohereProvider.ProviderName;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        options ??= new LanguageModelCallOptions();
        var built = Build(options, stream: false);
        var response = await _provider.PostJsonAsync(ApiKeys.Combine(_provider.Options.BaseUrl, "chat"), built.Body.ToJsonString(), options.Headers, cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body);
        return Parse(document.RootElement, response.Headers, response.Body, built.Warnings);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
        LanguageModelCallOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var opened = await OpenStreamAsync(options, cancellationToken).ConfigureAwait(false);
        await foreach (var part in opened.Parts.ConfigureAwait(false))
        {
            yield return part;
        }
    }

    /// <summary>Opens a chat stream and returns response headers before the first event.</summary>
    public async Task<CohereStreamResponse> OpenStreamAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        options ??= new LanguageModelCallOptions();
        var built = Build(options, stream: true);
        var response = await _provider.SendStreamAsync(ApiKeys.Combine(_provider.Options.BaseUrl, "chat"), built.Body.ToJsonString(), options.Headers, cancellationToken).ConfigureAwait(false);
        return new CohereStreamResponse(CohereProvider.CopyResponseHeaders(response), Read(response, built.Warnings, options.IncludeRawChunks, cancellationToken));
    }

    private BuiltRequest Build(LanguageModelCallOptions options, bool stream)
    {
        var warnings = new List<CohereWarning>();
        var converted = CohereChatMapping.Convert(options.Prompt);
        var tools = new List<CohereToolDefinition>();
        if (options.Tools != null)
        {
            foreach (var tool in options.Tools)
            {
                tools.Add(CohereToolDefinition.Function(tool.Name, tool.Description, tool.InputSchema));
            }
        }

        var prepared = CohereChatMapping.PrepareTools(options.Tools == null ? null : tools, options.ToolChoice);
        foreach (var warning in converted.Warnings)
        {
            warnings.Add(warning);
        }

        foreach (var warning in prepared.Warnings)
        {
            warnings.Add(warning);
        }

        JsonElement? cohere = null;
        if (options.ProviderOptions != null && options.ProviderOptions.TryGetValue("cohere", out var found))
        {
            cohere = found;
        }

        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["messages"] = JsonNode.Parse(converted.MessagesJson),
        };
        if (converted.DocumentsJson != "[]")
        {
            body["documents"] = JsonNode.Parse(converted.DocumentsJson);
        }

        Set(body, "frequency_penalty", options.FrequencyPenalty);
        Set(body, "presence_penalty", options.PresencePenalty);
        Set(body, "max_tokens", options.MaxOutputTokens);
        Set(body, "temperature", options.Temperature);
        Set(body, "p", options.TopP);
        Set(body, "k", options.TopK);
        Set(body, "seed", options.Seed);
        if (options.StopSequences is { Count: > 0 })
        {
            var stop = new JsonArray();
            foreach (var sequence in options.StopSequences)
            {
                stop.Add(sequence);
            }

            body["stop_sequences"] = stop;
        }

        if (options.JsonSchema is { } schema)
        {
            body["response_format"] = new JsonObject
            {
                ["type"] = "json_object",
                ["json_schema"] = JsonNode.Parse(schema.GetRawText()),
            };
        }

        if (prepared.ToolsJson != null)
        {
            body["tools"] = JsonNode.Parse(prepared.ToolsJson);
            if (prepared.ToolChoice != null)
            {
                body["tool_choice"] = prepared.ToolChoice;
            }
        }

        var thinking = CohereChatMapping.ResolveThinking(options.Reasoning, cohere, warnings);
        if (thinking != null)
        {
            body["thinking"] = thinking;
        }

        if (stream)
        {
            body["stream"] = true;
        }

        return new BuiltRequest(body, warnings);
    }

    private LanguageModelGenerateResult Parse(JsonElement root, IReadOnlyDictionary<string, string> headers, string raw, IReadOnlyList<CohereWarning> warnings)
    {
        if (root.TryGetProperty("usage", out var usageElement))
        {
            CohereChatMapping.ValidateUsage(usageElement);
        }

        var content = new List<GeneratedContent>();
        if (root.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.Object)
        {
            if (message.TryGetProperty("content", out var parts) && parts.ValueKind == JsonValueKind.Array)
            {
                foreach (var part in parts.EnumerateArray())
                {
                    var type = String(part, "type");
                    if (type == "text")
                    {
                        var text = String(part, "text");
                        if (!string.IsNullOrEmpty(text))
                        {
                            content.Add(new GeneratedText(text!));
                        }
                    }
                    else if (type == "thinking")
                    {
                        var thinking = String(part, "thinking");
                        if (!string.IsNullOrEmpty(thinking))
                        {
                            content.Add(new GeneratedReasoning(thinking!));
                        }
                    }
                }
            }

            if (message.TryGetProperty("citations", out var citations) && citations.ValueKind == JsonValueKind.Array)
            {
                foreach (var citation in citations.EnumerateArray())
                {
                    content.Add(Citation(citation));
                }
            }

            if (message.TryGetProperty("tool_calls", out var calls) && calls.ValueKind == JsonValueKind.Array)
            {
                foreach (var call in calls.EnumerateArray())
                {
                    var function = call.GetProperty("function");
                    var arguments = String(function, "arguments") ?? "{}";
                    if (arguments == "null")
                    {
                        arguments = "{}";
                    }

                    content.Add(new GeneratedToolCall(String(call, "id") ?? string.Empty, String(function, "name") ?? string.Empty, arguments));
                }
            }
        }

        var finish = String(root, "finish_reason");
        var usage = root.TryGetProperty("usage", out var usageValue)
            ? CohereChatMapping.ConvertUsage(usageValue)
            : LanguageModelUsage.Empty;
        var callWarnings = new List<CallWarning>();
        foreach (var warning in warnings)
        {
            callWarnings.Add(new CallWarning(warning.Type, warning.Feature ?? warning.Details ?? warning.Type));
        }

        return new LanguageModelGenerateResult(
            content,
            CohereChatMapping.MapFinishReason(finish),
            usage,
            finish,
            callWarnings,
            String(root, "generation_id"),
            null,
            raw,
            null,
            null,
            headers);
    }

    private CohereCitation Citation(JsonElement citation)
    {
        var title = "Document";
        if (citation.TryGetProperty("sources", out var sources) && sources.ValueKind == JsonValueKind.Array && sources.GetArrayLength() > 0)
        {
            var document = sources[0];
            if (document.TryGetProperty("document", out var doc) && doc.ValueKind == JsonValueKind.Object)
            {
                title = String(doc, "title") ?? title;
            }
        }

        var metadata = new JsonObject
        {
            ["text"] = String(citation, "text"),
        };
        if (citation.TryGetProperty("start", out var start) && start.ValueKind == JsonValueKind.Number)
        {
            metadata["start"] = start.GetInt32();
        }

        if (citation.TryGetProperty("end", out var end) && end.ValueKind == JsonValueKind.Number)
        {
            metadata["end"] = end.GetInt32();
        }
        if (citation.TryGetProperty("sources", out var sourceArray))
        {
            metadata["sources"] = JsonNode.Parse(sourceArray.GetRawText());
        }

        var citationType = String(citation, "type");
        if (citationType != null)
        {
            metadata["citationType"] = citationType;
        }

        return new CohereCitation(NextId(), title, Element(new JsonObject { ["cohere"] = metadata }));
    }

    private string NextId()
    {
        if (_provider.Options.GenerateId != null)
        {
            return _provider.Options.GenerateId();
        }

        var id = "id-" + _ids.ToString();
        _ids++;
        return id;
    }

    private async IAsyncEnumerable<LanguageModelStreamPart> Read(
        HttpResponseMessage response,
        IReadOnlyList<CohereWarning> warnings,
        bool includeRaw,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using (response)
        {
            var callWarnings = new List<CallWarning>();
            foreach (var warning in warnings)
            {
                callWarnings.Add(new CallWarning(warning.Type, warning.Feature ?? warning.Details ?? warning.Type));
            }

            yield return new StreamStartStreamPart(callWarnings);
            string? finishRaw = null;
            var failed = false;
            JsonElement? usage = null;
            PendingTool? pending = null;
            var reasoning = false;
            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            await foreach (var data in SseParser.ReadDataAsync(stream, cancellationToken).ConfigureAwait(false))
            {
                if (includeRaw)
                {
                    yield return new RawStreamPart(data);
                }

                JsonDocument? document = null;
                string? parseError = null;
                try
                {
                    document = JsonDocument.Parse(data);
                }
                catch (JsonException exception)
                {
                    parseError = exception.Message;
                }

                if (parseError != null || document == null)
                {
                    failed = true;
                    finishRaw = null;
                    yield return new ErrorStreamPart(parseError ?? "Invalid JSON.");
                    continue;
                }

                using (document)
                {
                    var root = document.RootElement;
                    var type = String(root, "type");
                    string? usageError = null;
                    if (type == "message-end" && root.TryGetProperty("delta", out var endDelta) && endDelta.TryGetProperty("usage", out var usageElement))
                    {
                        try
                        {
                            CohereChatMapping.ValidateUsage(usageElement);
                            usage = usageElement.Clone();
                            finishRaw = String(endDelta, "finish_reason");
                        }
                        catch (CohereUsageException exception)
                        {
                            usageError = exception.Message;
                        }
                    }

                    if (usageError != null)
                    {
                        failed = true;
                        finishRaw = null;
                        usage = null;
                        yield return new ErrorStreamPart(usageError);
                        continue;
                    }

                    if (type == "message-start")
                    {
                        yield return new ResponseMetadataStreamPart(String(root, "id"), null, null);
                    }
                    else if (type == "content-start")
                    {
                        var index = Index(root);
                        var content = root.GetProperty("delta").GetProperty("message").GetProperty("content");
                        if (String(content, "type") == "thinking")
                        {
                            reasoning = true;
                            yield return new ReasoningStartStreamPart(index);
                        }
                        else
                        {
                            reasoning = false;
                            yield return new TextStartStreamPart(index);
                        }
                    }
                    else if (type == "content-delta")
                    {
                        var index = Index(root);
                        var content = root.GetProperty("delta").GetProperty("message").GetProperty("content");
                        if (content.TryGetProperty("thinking", out var thinking))
                        {
                            yield return new ReasoningDeltaStreamPart(index, thinking.GetString() ?? string.Empty);
                        }
                        else
                        {
                            yield return new TextDeltaStreamPart(index, String(content, "text") ?? string.Empty);
                        }
                    }
                    else if (type == "content-end")
                    {
                        var index = Index(root);
                        if (reasoning)
                        {
                            reasoning = false;
                            yield return new ReasoningEndStreamPart(index);
                        }
                        else
                        {
                            yield return new TextEndStreamPart(index);
                        }
                    }
                    else if (type == "tool-call-start")
                    {
                        var call = root.GetProperty("delta").GetProperty("message").GetProperty("tool_calls");
                        var function = call.GetProperty("function");
                        var id = String(call, "id") ?? string.Empty;
                        var name = String(function, "name") ?? string.Empty;
                        var arguments = String(function, "arguments") ?? string.Empty;
                        pending = new PendingTool(id, name, arguments);
                        yield return new CohereToolInputStartStreamPart(id, name);
                        if (arguments.Length > 0)
                        {
                            yield return new CohereToolInputDeltaStreamPart(id, arguments);
                        }
                    }
                    else if (type == "tool-call-delta" && pending != null)
                    {
                        var arguments = String(root.GetProperty("delta").GetProperty("message").GetProperty("tool_calls").GetProperty("function"), "arguments") ?? string.Empty;
                        pending.Arguments.Append(arguments);
                        yield return new CohereToolInputDeltaStreamPart(pending.Id, arguments);
                    }
                    else if (type == "tool-call-end" && pending != null)
                    {
                        var id = pending.Id;
                        var name = pending.Name;
                        var input = CohereChatMapping.Reserialize(pending.Arguments.ToString());
                        pending = null;
                        yield return new CohereToolInputEndStreamPart(id);
                        yield return new ToolCallStreamPart(id, name, input);
                    }
                    else if (type == "message-end" && !root.GetProperty("delta").TryGetProperty("usage", out _))
                    {
                        finishRaw = String(root.GetProperty("delta"), "finish_reason");
                    }
                }
            }

            yield return new FinishStreamPart(
                failed ? FinishReason.Error : CohereChatMapping.MapFinishReason(finishRaw),
                !failed && usage is { } value ? CohereChatMapping.ConvertUsage(value) : new LanguageModelUsage(null, null, null),
                failed ? null : finishRaw);
        }
    }

    private static string Index(JsonElement element)
    {
        if (element.TryGetProperty("index", out var index) && index.ValueKind == JsonValueKind.Number)
        {
            return index.GetInt32().ToString();
        }

        return "0";
    }

    private static void Set(JsonObject body, string name, double? value)
    {
        if (value is { } number)
        {
            body[name] = number;
        }
    }

    private static void Set(JsonObject body, string name, int? value)
    {
        if (value is { } number)
        {
            body[name] = number;
        }
    }

    private static string? String(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return value.GetString();
    }

    private static JsonElement Element(JsonNode node)
    {
        using var document = JsonDocument.Parse(node.ToJsonString());
        return document.RootElement.Clone();
    }

    private sealed class BuiltRequest
    {
        public BuiltRequest(JsonObject body, IReadOnlyList<CohereWarning> warnings)
        {
            Body = body;
            Warnings = warnings;
        }

        public JsonObject Body { get; }

        public IReadOnlyList<CohereWarning> Warnings { get; }
    }

    private sealed class PendingTool
    {
        public PendingTool(string id, string name, string arguments)
        {
            Id = id;
            Name = name;
            Arguments = new StringBuilder(arguments);
        }

        public string Id { get; }

        public string Name { get; }

        public StringBuilder Arguments { get; }
    }
}
