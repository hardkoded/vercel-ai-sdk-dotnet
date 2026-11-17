// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Mistral;

/// <summary>
/// Mistral Conversations API language model, with support for web search. Maps to <c>MistralConversationLanguageModel</c>
/// in <c>mistral-conversation-language-model.ts</c>.
/// </summary>
public sealed class MistralConversationLanguageModel : ILanguageModel
{
    private static readonly JsonSerializerOptions CompactOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private static readonly IReadOnlyDictionary<string, string> PdfUrls = new Dictionary<
        string,
        string
    >
    {
        ["application/pdf"] = "^https://.*\\z",
    };

    private readonly OpenAICompatibleProvider _provider;
    private readonly Func<string> _generateId;

    /// <summary>Creates a model.</summary>
    public MistralConversationLanguageModel(OpenAICompatibleProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _generateId = provider.Options.GenerateId ?? (() => JsonValues.GenerateId(string.Empty));
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => MistralProvider.ProviderId + ".conversation";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> SupportedUrls => PdfUrls;

    private Uri ConversationsUri => ApiKeys.Combine(_provider.Options.BaseUrl, "conversations");

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    )
    {
        options ??= new LanguageModelCallOptions();
        var (body, warnings) = GetArgs(options);
        var requestJson = body.ToJsonString(CompactOptions);
        var response = await _provider
            .PostJsonAsync(
                ConversationsUri,
                requestJson,
                _provider.CreateHeaders(options.Headers),
                cancellationToken
            )
            .ConfigureAwait(false);

        using var document = ParseResponse(response.Body);
        var root = document.RootElement;
        var outputs = Require(root, "outputs", JsonValueKind.Array);
        var converter = new ContentConverter(options.Tools, _generateId);
        var content = new List<GeneratedContent>();
        var hasFunctionCalls = false;
        string? modelId = null;

        foreach (var output in outputs.EnumerateArray())
        {
            switch (Type(output))
            {
                case "tool.execution":
                    var (call, result) = converter.ConvertTool(ParseToolExecution(output));
                    content.Add(call);
                    content.Add(result);
                    break;
                case "function.call":
                    hasFunctionCalls = true;
                    var arguments = Require(
                        output,
                        "arguments",
                        JsonValueKind.String,
                        JsonValueKind.Object
                    );
                    content.Add(
                        new GeneratedToolCall(
                            RequireString(output, "tool_call_id"),
                            RequireString(output, "name"),
                            arguments.ValueKind == JsonValueKind.String
                                ? arguments.GetString()!
                                : Stringify(arguments)
                        )
                    );
                    break;
                case "message.output":
                    modelId = String(output, "model") ?? modelId;
                    var messageContent = Require(
                        output,
                        "content",
                        JsonValueKind.String,
                        JsonValueKind.Array
                    );
                    if (messageContent.ValueKind == JsonValueKind.String)
                    {
                        content.AddRange(
                            converter.ConvertChunk(new Chunk("text", messageContent.GetString()))
                        );
                    }
                    else
                    {
                        foreach (var chunk in messageContent.EnumerateArray())
                        {
                            content.AddRange(converter.ConvertChunk(ParseChunk(chunk)));
                        }
                    }

                    break;
                default:
                    throw Invalid("Unknown conversation output type.", output);
            }
        }

        return new LanguageModelGenerateResult(
            content,
            hasFunctionCalls ? FinishReason.ToolCalls : FinishReason.Stop,
            ConvertMistralUsage.Convert(Usage(root)),
            null,
            warnings,
            RequireString(root, "conversation_id"),
            null,
            response.Body,
            modelId,
            null,
            response.Headers,
            request: LanguageModelStreamRequest.FromJson(requestJson)
        );
    }

    /// <inheritdoc />
    public async Task<LanguageModelStreamResult> DoStreamAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    )
    {
        options ??= new LanguageModelCallOptions();
        var (args, warnings) = GetArgs(options);
        args["stream"] = true;
        var json = args.ToJsonString(CompactOptions);
        var sse = await _provider
            .PostSseAsync(
                ConversationsUri,
                json,
                _provider.CreateHeaders(options.Headers),
                cancellationToken
            )
            .ConfigureAwait(false);
        try
        {
            return new LanguageModelStreamResult(
                new ReleasingAsyncEnumerable<LanguageModelStreamPart>(
                    StreamAsync(options, warnings, sse, cancellationToken),
                    sse
                ),
                LanguageModelStreamRequest.FromJson(json),
                new LanguageModelStreamResponse(sse.Headers),
                sse
            );
        }
        catch
        {
            sse.Dispose();
            throw;
        }
    }

    private static string? String(JsonElement element, string name)
    {
        return
            element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static string Type(JsonElement element)
    {
        return String(element, "type")
            ?? throw Invalid("Conversation entry needs a type.", element);
    }

    private static TypeValidationException Invalid(string message, JsonElement value)
    {
        return new TypeValidationException(message, value.Clone());
    }

    private static JsonElement Require(JsonElement owner, string name, params JsonValueKind[] kinds)
    {
        if (
            owner.ValueKind == JsonValueKind.Object
            && owner.TryGetProperty(name, out var value)
            && Array.IndexOf(kinds, value.ValueKind) >= 0
        )
        {
            return value;
        }

        throw Invalid(
            "Invalid Mistral conversation response: '"
                + name
                + "' is missing or has the wrong type.",
            owner
        );
    }

    private static string RequireString(JsonElement owner, string name)
    {
        return Require(owner, name, JsonValueKind.String).GetString()!;
    }

    private static string? NullishString(JsonElement owner, string name)
    {
        if (
            owner.ValueKind == JsonValueKind.Object
            && owner.TryGetProperty(name, out var value)
            && value.ValueKind != JsonValueKind.Null
        )
        {
            return value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : throw Invalid(
                    "Invalid Mistral conversation response: '" + name + "' must be a string.",
                    owner
                );
        }

        return null;
    }

    private static JsonElement? NullishObject(JsonElement owner, string name)
    {
        if (
            owner.ValueKind == JsonValueKind.Object
            && owner.TryGetProperty(name, out var value)
            && value.ValueKind != JsonValueKind.Null
        )
        {
            return value.ValueKind == JsonValueKind.Object
                ? value
                : throw Invalid(
                    "Invalid Mistral conversation response: '" + name + "' must be an object.",
                    owner
                );
        }

        return null;
    }

    private static JsonElement? Usage(JsonElement owner)
    {
        return NullishObject(owner, "usage");
    }

    private static string Stringify(JsonElement value)
    {
        return JsonNode.Parse(value.GetRawText())?.ToJsonString(CompactOptions) ?? "null";
    }

    private static JsonDocument ParseResponse(string body)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(body);
        }
        catch (JsonException exception)
        {
            throw new TypeValidationException("Invalid JSON response: " + exception.Message, null);
        }

        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            var root = document.RootElement.Clone();
            document.Dispose();
            throw Invalid("Invalid Mistral conversation response.", root);
        }

        return document;
    }

    private static ToolExecution ParseToolExecution(JsonElement element)
    {
        return new ToolExecution(
            RequireString(element, "id"),
            RequireString(element, "name"),
            RequireString(element, "arguments"),
            NullishString(element, "function"),
            NullishObject(element, "info")
        );
    }

    private static Chunk ParseChunk(JsonElement element)
    {
        switch (Type(element))
        {
            case "text":
                return new Chunk("text", RequireString(element, "text"));
            case "thinking":
                var parts = Require(element, "thinking", JsonValueKind.Array);
                var text = new StringBuilder();
                foreach (var part in parts.EnumerateArray())
                {
                    if (Type(part) != "text")
                    {
                        throw Invalid("Invalid Mistral conversation thinking chunk.", part);
                    }

                    text.Append(RequireString(part, "text"));
                }

                return new Chunk("thinking", text.ToString());
            case "tool_reference":
                NullishString(element, "tool");
                return new Chunk(
                    "tool_reference",
                    null,
                    NullishString(element, "title"),
                    NullishString(element, "url")
                );
            default:
                throw Invalid("Unknown Mistral conversation content chunk.", element);
        }
    }

    // Reads the content of a message.output.delta: a string, or a single chunk.
    private static Chunk ParseDeltaContent(JsonElement element)
    {
        var content = element.TryGetProperty("content", out var value) ? value : default;
        return content.ValueKind switch
        {
            JsonValueKind.String => new Chunk("text", content.GetString()),
            JsonValueKind.Object => ParseChunk(content),
            _ => throw Invalid("Invalid Mistral conversation delta content.", element),
        };
    }

    private static IReadOnlyList<ModelMessage> InjectJsonInstructionIntoMessages(
        IReadOnlyList<ModelMessage> messages,
        JsonElement? schema
    )
    {
        var first = messages.Count > 0 ? messages[0] as SystemModelMessage : null;
        var instruction = Vercel.AI.GenerateText.JsonInstructions.InjectJsonInstruction(
            first?.Content,
            schema
        );
        var result = new List<ModelMessage>
        {
            new SystemModelMessage(instruction, first?.ProviderOptions),
        };
        for (var i = first != null ? 1 : 0; i < messages.Count; i++)
        {
            result.Add(messages[i]);
        }

        return result;
    }

    private (JsonObject Args, List<CallWarning> Warnings) GetArgs(LanguageModelCallOptions options)
    {
        var warnings = new List<CallWarning>();
        var providerOptions = MistralLanguageModelConversationOptions.Parse(options.ProviderOptions)
            ?? new MistralLanguageModelChatOptions();

        if (options.TopK != null)
        {
            warnings.Add(new CallWarning("unsupported", "topK", null));
        }

        var reasoningCapabilities = MistralModelCatalog
            .GetMistralLanguageModelEntry(ModelId)
            .Language.Endpoints.Conversation.Reasoning;
        string? resolvedReasoningEffort = null;
        if (reasoningCapabilities.Support)
        {
            if (providerOptions.ReasoningEffort != null)
            {
                resolvedReasoningEffort = providerOptions.ReasoningEffort;
            }
            else if (ReasoningMap.IsCustomReasoning(options.Reasoning))
            {
                var mappingWarnings = new List<ModelWarning>();
                resolvedReasoningEffort = ReasoningMap.MapReasoningToProviderEffort(
                    options.Reasoning!,
                    reasoningCapabilities.Efforts.ToDictionary(effort => effort, effort => effort),
                    mappingWarnings
                );
                foreach (var warning in mappingWarnings)
                {
                    if (ReasoningMap.ToCallWarning(warning) is { } callWarning)
                    {
                        warnings.Add(callWarning);
                    }
                }
            }
        }
        else if (ReasoningMap.IsCustomReasoning(options.Reasoning))
        {
            warnings.Add(
                new CallWarning(
                    "unsupported",
                    "reasoning",
                    "This model does not support reasoning configuration."
                )
            );
        }

        var structuredOutputs = providerOptions.StructuredOutputs ?? true;
        var strictJsonSchema = providerOptions.StrictJsonSchema ?? false;
        var prompt = options.Prompt;

        // For Mistral we need to instruct the model when using JSON Object mode.
        // https://docs.mistral.ai/capabilities/structured-output/structured_output_overview/
        var hasJson = options.JsonResponse || options.JsonSchema != null;
        if (hasJson && (!structuredOutputs || options.JsonSchema == null))
        {
            prompt = InjectJsonInstructionIntoMessages(prompt, options.JsonSchema);
        }

        var completionArgs = new JsonObject();
        Set(completionArgs, "max_tokens", options.MaxOutputTokens);
        Set(completionArgs, "temperature", options.Temperature);
        Set(completionArgs, "top_p", options.TopP);
        Set(completionArgs, "frequency_penalty", options.FrequencyPenalty);
        Set(completionArgs, "presence_penalty", options.PresencePenalty);
        if (options.StopSequences != null)
        {
            completionArgs["stop"] = new JsonArray(
                options.StopSequences.Select(stop => (JsonNode?)stop).ToArray()
            );
        }

        Set(completionArgs, "random_seed", options.Seed);
        if (resolvedReasoningEffort != null)
        {
            completionArgs["reasoning_effort"] = resolvedReasoningEffort;
        }

        if (hasJson)
        {
            if (structuredOutputs && options.JsonSchema != null)
            {
                var jsonSchema = new JsonObject
                {
                    ["schema"] = JsonNode.Parse(options.JsonSchema.Value.GetRawText()),
                    ["strict"] = strictJsonSchema,
                    ["name"] = options.JsonSchemaName ?? "response",
                };
                if (options.JsonSchemaDescription != null)
                {
                    jsonSchema["description"] = options.JsonSchemaDescription;
                }

                completionArgs["response_format"] = new JsonObject
                {
                    ["type"] = "json_schema",
                    ["json_schema"] = jsonSchema,
                };
            }
            else
            {
                completionArgs["response_format"] = new JsonObject { ["type"] = "json_object" };
            }
        }

        var prepared = MistralConversationPrepareTools.PrepareConversationTools(
            options.Tools,
            options.ToolChoice
        );
        if (prepared.ToolChoice != null)
        {
            completionArgs["tool_choice"] = prepared.ToolChoice;
        }

        UnsupportedOption(warnings, "safePrompt", providerOptions.SafePrompt != null);
        UnsupportedOption(
            warnings,
            "documentImageLimit",
            providerOptions.DocumentImageLimit != null
        );
        UnsupportedOption(warnings, "documentPageLimit", providerOptions.DocumentPageLimit != null);
        UnsupportedOption(warnings, "promptCacheKey", providerOptions.PromptCacheKey != null);
        UnsupportedOption(warnings, "parallelToolCalls", providerOptions.ParallelToolCalls != null);

        var (inputs, instructions) = ConvertToMistralConversationInputs.Convert(
            prompt,
            MistralConversationPrepareTools.CreateToolNameMapping(options.Tools)
        );
        var args = new JsonObject
        {
            ["model"] = ModelId,
            ["store"] = false,
            ["inputs"] = inputs,
        };
        if (instructions != null)
        {
            args["instructions"] = instructions;
        }

        if (prepared.Tools != null)
        {
            args["tools"] = prepared.Tools;
        }

        args["completion_args"] = completionArgs;
        warnings.AddRange(prepared.ToolWarnings);
        return (args, warnings);
    }

    private static void UnsupportedOption(List<CallWarning> warnings, string feature, bool present)
    {
        if (present)
        {
            warnings.Add(
                new CallWarning(
                    "unsupported",
                    feature,
                    "Not supported by the Mistral Conversations API."
                )
            );
        }
    }

    private static void Set(JsonObject target, string name, int? value)
    {
        if (value != null)
        {
            target[name] = value;
        }
    }

    private static void Set(JsonObject target, string name, double? value)
    {
        if (value != null)
        {
            target[name] = value;
        }
    }

    private async IAsyncEnumerable<LanguageModelStreamPart> StreamAsync(
        LanguageModelCallOptions options,
        List<CallWarning> warnings,
        OpenAICompatibleSseResponse sse,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        using var disposable = sse;
        var converter = new ContentConverter(options.Tools, _generateId);
        var toolCalls = new StreamingToolCallTracker(_generateId);
        var executions = new Dictionary<string, ToolExecution>(StringComparer.Ordinal);
        (string Id, string Type)? active = null;
        JsonElement? usage = null;
        var hasFunctionCalls = false;
        var hasError = false;
        var isDone = false;
        var hasModelId = false;

        yield return new StreamStartStreamPart(warnings);

        await foreach (var data in sse.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            if (options.IncludeRawChunks)
            {
                yield return new RawStreamPart(data);
            }

            var parsed = JsonParsing.SafeParse(data);
            if (!parsed.Success)
            {
                hasError = true;
                yield return new ErrorStreamPart(parsed.Error!);
                continue;
            }

            var parts = new List<LanguageModelStreamPart>();
            try
            {
                Handle(parsed.Value!.Value);
            }
            catch (TypeValidationException error)
            {
                hasError = true;
                parts.Add(new ErrorStreamPart(error));
            }

            foreach (var part in parts)
            {
                yield return part;
            }

            void Handle(JsonElement chunk)
            {
                var type = Type(chunk);
                switch (type)
                {
                    case "conversation.response.started":
                        var conversationId = RequireString(chunk, "conversation_id");
                        var createdAt = NullishString(chunk, "created_at");
                        parts.Add(
                            new ResponseMetadataStreamPart(
                                conversationId,
                                null,
                                createdAt != null
                                && DateTimeOffset.TryParse(
                                    createdAt,
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    System.Globalization.DateTimeStyles.AssumeUniversal,
                                    out var timestamp
                                )
                                    ? timestamp
                                    : null
                            )
                        );
                        break;
                    case "conversation.response.done":
                        usage = Usage(chunk);
                        isDone = true;
                        break;
                    case "conversation.response.error":
                        RequireString(chunk, "message");
                        Require(chunk, "code", JsonValueKind.Number);
                        hasError = true;
                        parts.Add(
                            new ErrorStreamPart(new ConversationStreamException(chunk.GetRawText()))
                        );
                        break;
                    case "message.output.delta":
                        var id = RequireString(chunk, "id");
                        var model = NullishString(chunk, "model");
                        var content = ParseDeltaContent(chunk);
                        if (!hasModelId && model != null)
                        {
                            hasModelId = true;
                            parts.Add(new ResponseMetadataStreamPart(null, model, null));
                        }

                        var contentIndex =
                            chunk.TryGetProperty("content_index", out var index)
                            && index.ValueKind == JsonValueKind.Number
                                ? index.GetInt32()
                                : 0;
                        foreach (var part in converter.ConvertChunk(content))
                        {
                            if (part is GeneratedText or GeneratedReasoning)
                            {
                                var partType = part is GeneratedText ? "text" : "reasoning";
                                var blockId =
                                    id
                                    + "-"
                                    + contentIndex.ToString(
                                        System.Globalization.CultureInfo.InvariantCulture
                                    );
                                if (
                                    active is null
                                    || active.Value.Id != blockId
                                    || active.Value.Type != partType
                                )
                                {
                                    CloseContent(parts, ref active);
                                    active = (blockId, partType);
                                    parts.Add(
                                        partType == "text"
                                            ? new TextStartStreamPart(blockId)
                                            : new ReasoningStartStreamPart(blockId)
                                    );
                                }

                                parts.Add(
                                    part is GeneratedText text
                                        ? new TextDeltaStreamPart(blockId, text.Text)
                                        : new ReasoningDeltaStreamPart(
                                            blockId,
                                            ((GeneratedReasoning)part).Text
                                        )
                                );
                            }
                            else if (part is GeneratedSource source)
                            {
                                parts.Add(
                                    new SourceStreamPart(
                                        source.Id,
                                        source.Url,
                                        source.Title,
                                        source.ProviderMetadata
                                    )
                                );
                            }
                        }

                        break;
                    case "function.call.delta":
                        var callId = RequireString(chunk, "tool_call_id");
                        var callName = RequireString(chunk, "name");
                        var callArguments = RequireString(chunk, "arguments");
                        int? outputIndex =
                            chunk.TryGetProperty("output_index", out var outputIndexElement)
                            && outputIndexElement.ValueKind == JsonValueKind.Number
                                ? outputIndexElement.GetInt32()
                                : null;
                        CloseContent(parts, ref active);
                        hasFunctionCalls = true;
                        var input = toolCalls.ProcessDelta(
                            new StreamingToolCallDelta(outputIndex, callId, callName, callArguments)
                        );
                        if (input != null)
                        {
                            if (input.Started)
                            {
                                parts.Add(new ToolInputStartStreamPart(input.Id, input.Name));
                            }

                            if (input.ArgumentsDelta != null)
                            {
                                parts.Add(
                                    new ToolInputDeltaStreamPart(input.Id, input.ArgumentsDelta)
                                );
                            }
                        }

                        break;
                    case "tool.execution.started":
                    case "tool.execution.delta":
                        var executionId = RequireString(chunk, "id");
                        var executionName = RequireString(chunk, "name");
                        var executionArguments = RequireString(chunk, "arguments");
                        var function = NullishString(chunk, "function");
                        CloseContent(parts, ref active);
                        if (!executions.TryGetValue(executionId, out var execution))
                        {
                            executions[executionId] = new ToolExecution(
                                executionId,
                                executionName,
                                executionArguments,
                                function,
                                null
                            );
                            parts.Add(
                                new ToolInputStartStreamPart(
                                    executionId,
                                    converter.ToolNameMapping.ToCustomToolName(executionName),
                                    null,
                                    true
                                )
                            );
                        }
                        else
                        {
                            executions[executionId] = execution with
                            {
                                Arguments = execution.Arguments + executionArguments,
                            };
                        }

                        break;
                    case "tool.execution.done":
                        var doneId = RequireString(chunk, "id");
                        RequireString(chunk, "name");
                        var doneFunction = NullishString(chunk, "function");
                        var info = NullishObject(chunk, "info");
                        if (!executions.TryGetValue(doneId, out var started))
                        {
                            hasError = true;
                            parts.Add(
                                new ErrorStreamPart(
                                    new InvalidOperationException(
                                        "Missing tool execution start for " + doneId
                                    )
                                )
                            );
                            break;
                        }

                        parts.Add(
                            new ToolInputDeltaStreamPart(
                                doneId,
                                new JsonObject { ["arguments"] = started.Arguments }.ToJsonString(
                                    CompactOptions
                                )
                            )
                        );
                        parts.Add(new ToolInputEndStreamPart(doneId));
                        var (doneCall, doneResult) = converter.ConvertTool(
                            new ToolExecution(
                                doneId,
                                started.Name,
                                started.Arguments,
                                doneFunction ?? started.Function,
                                info
                            )
                        );
                        parts.Add(
                            new ToolCallStreamPart(
                                doneCall.ToolCallId,
                                doneCall.ToolName,
                                doneCall.ArgumentsJson,
                                doneCall.ProviderMetadata
                            )
                            {
                                ProviderExecuted = true,
                            }
                        );
                        parts.Add(
                            new ToolResultStreamPart(
                                doneResult.ToolCallId,
                                doneResult.ToolName,
                                JsonValues.ParseOrEmpty(doneResult.ResultJson)
                            )
                            {
                                ProviderMetadata = doneResult.ProviderMetadata,
                            }
                        );
                        executions.Remove(doneId);
                        break;
                    default:
                        throw Invalid("Unknown Mistral conversation event type: " + type, chunk);
                }
            }
        }

        var flush = new List<LanguageModelStreamPart>();
        CloseContent(flush, ref active);
        foreach (var part in flush)
        {
            yield return part;
        }

        foreach (var call in toolCalls.Flush())
        {
            yield return new ToolInputEndStreamPart(call.ToolCallId);
            yield return call;
        }

        yield return new FinishStreamPart(
            hasError || !isDone || executions.Count > 0 ? FinishReason.Error
                : hasFunctionCalls ? FinishReason.ToolCalls
                : FinishReason.Stop,
            ConvertMistralUsage.Convert(usage),
            null
        );
    }

    private static void CloseContent(
        List<LanguageModelStreamPart> parts,
        ref (string Id, string Type)? active
    )
    {
        if (active is { } current)
        {
            parts.Add(
                current.Type == "text"
                    ? new TextEndStreamPart(current.Id)
                    : new ReasoningEndStreamPart(current.Id)
            );
            active = null;
        }
    }

    private sealed class ConversationStreamException : Exception
    {
        public ConversationStreamException(string message)
            : base(message) { }
    }

    private sealed record Chunk(
        string Type,
        string? Text,
        string? Title = null,
        string? Url = null
    );

    private sealed record ToolExecution(
        string Id,
        string Name,
        string Arguments,
        string? Function,
        JsonElement? Info
    );

    // Maps to createContentConverter.
    private sealed class ContentConverter
    {
        private readonly Func<string> _generateId;
        private readonly HashSet<string> _sourceUrls = new(StringComparer.Ordinal);

        public ContentConverter(IReadOnlyList<LanguageModelTool>? tools, Func<string> generateId)
        {
            _generateId = generateId;
            ToolNameMapping = MistralConversationPrepareTools.CreateToolNameMapping(tools);
        }

        public ToolNameMapping ToolNameMapping { get; }

        public IReadOnlyList<GeneratedContent> ConvertChunk(Chunk chunk)
        {
            switch (chunk.Type)
            {
                case "text":
                    return chunk.Text is { Length: > 0 } text
                        ? new GeneratedContent[] { new GeneratedText(text) }
                        : Array.Empty<GeneratedContent>();
                case "thinking":
                    return chunk.Text is { Length: > 0 } thinking
                        ? new GeneratedContent[] { new GeneratedReasoning(thinking) }
                        : Array.Empty<GeneratedContent>();
                default:
                    if (chunk.Url == null || !_sourceUrls.Add(chunk.Url))
                    {
                        return Array.Empty<GeneratedContent>();
                    }

                    return new GeneratedContent[]
                    {
                        new GeneratedSource(_generateId(), chunk.Url, chunk.Title ?? chunk.Url),
                    };
            }
        }

        public (GeneratedToolCall Call, GeneratedToolResult Result) ConvertTool(
            ToolExecution output
        )
        {
            var toolName = ToolNameMapping.ToCustomToolName(output.Name);
            var metadata = new JsonObject { ["type"] = "tool.execution", ["name"] = output.Name };
            if (output.Function != null)
            {
                metadata["function"] = output.Function;
            }

            var providerMetadata = JsonSerializer.SerializeToElement(
                new JsonObject { ["mistral"] = metadata }
            );
            var result = new JsonObject();
            if (output.Info is { } info)
            {
                result["info"] = JsonNode.Parse(info.GetRawText());
            }

            return (
                new GeneratedToolCall(
                    output.Id,
                    toolName,
                    new JsonObject { ["arguments"] = output.Arguments }.ToJsonString(
                        CompactOptions
                    ),
                    providerMetadata
                )
                {
                    ProviderExecuted = true,
                },
                new GeneratedToolResult(output.Id, toolName, result.ToJsonString(CompactOptions))
            );
        }
    }
}
