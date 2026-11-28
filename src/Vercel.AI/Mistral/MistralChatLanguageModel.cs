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
/// Mistral Chat Completions language model. Maps to <c>MistralChatLanguageModel</c> in <c>mistral-chat-language-model.ts</c>.
/// </summary>
public sealed class MistralChatLanguageModel : ILanguageModel
{
    private static readonly JsonSerializerOptions CompactOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private static readonly IReadOnlyDictionary<string, string> PdfUrls =
        new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(
            new Dictionary<string, string> { ["application/pdf"] = "^https://.*\\z" }
        );

    private readonly OpenAICompatibleProvider _provider;
    private readonly Func<string> _generateId;

    /// <summary>Creates a model.</summary>
    public MistralChatLanguageModel(OpenAICompatibleProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _generateId = provider.Options.GenerateId ?? (() => JsonValues.GenerateId(string.Empty));
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => _provider.Options.ProviderName + ".chat";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> SupportedUrls => PdfUrls;

    private Uri ChatCompletionsUri => _provider.ChatUri(ModelId);

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    )
    {
        options ??= new LanguageModelCallOptions();
        var (body, warnings) = GetArgs(options);
        body = Transform(body, warnings);
        var requestJson = body.ToJsonString(CompactOptions);
        var response = await _provider
            .PostJsonAsync(
                ChatCompletionsUri,
                requestJson,
                await _provider
                    .CreateHeadersAsync(options.Headers, cancellationToken)
                    .ConfigureAwait(false),
                cancellationToken
            )
            .ConfigureAwait(false);

        using var document = ParseResponse(response.Body);
        var root = document.RootElement;
        var choices = Require(root, "choices", JsonValueKind.Array);
        if (choices.GetArrayLength() == 0)
        {
            throw Invalid("The response has no choices.", root);
        }

        var choice = choices[0];
        var message = Require(choice, "message", JsonValueKind.Object);
        var usage = MistralUsageSchema.Parse(Require(root, "usage", JsonValueKind.Object));
        var content = new List<GeneratedContent>();

        // Process content parts in order to preserve the sequence.
        if (
            message.TryGetProperty("content", out var messageContent)
            && messageContent.ValueKind == JsonValueKind.Array
        )
        {
            foreach (var part in messageContent.EnumerateArray())
            {
                switch (ContentType(part))
                {
                    case "thinking":
                        var reasoning = ExtractReasoningContent(part);
                        if (reasoning.Length > 0)
                        {
                            content.Add(new GeneratedReasoning(reasoning));
                        }

                        break;
                    case "text":
                        var partText = RequireString(part, "text");
                        if (partText.Length > 0)
                        {
                            content.Add(new GeneratedText(partText));
                        }

                        break;
                }
            }
        }
        else
        {
            // Handle legacy string content.
            var text = ExtractTextContent(messageContent);
            if (!string.IsNullOrEmpty(text))
            {
                content.Add(new GeneratedText(text!));
            }
        }

        // When there is a trailing assistant message, Mistral sends the content of that message again.
        // The repeated content is skipped upstream, so it is not duplicated here either.
        if (
            message.TryGetProperty("tool_calls", out var toolCalls)
            && toolCalls.ValueKind == JsonValueKind.Array
        )
        {
            foreach (var toolCall in toolCalls.EnumerateArray())
            {
                var function = Require(toolCall, "function", JsonValueKind.Object);
                content.Add(
                    new GeneratedToolCall(
                        RequireString(toolCall, "id"),
                        RequireString(function, "name"),
                        RequireString(function, "arguments")
                    )
                );
            }
        }

        var rawFinishReason = NullishString(choice, "finish_reason");
        return new LanguageModelGenerateResult(
            content,
            MapMistralFinishReason.Map(rawFinishReason),
            ConvertMistralUsage.Convert(usage),
            rawFinishReason,
            warnings,
            NullishString(root, "id"),
            null,
            response.Body,
            NullishString(root, "model"),
            Created(root),
            response.Headers,
            null,
            LanguageModelStreamRequest.FromJson(requestJson)
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
        args = Transform(args, warnings);
        var json = args.ToJsonString(CompactOptions);
        var sse = await _provider
            .PostSseAsync(
                ChatCompletionsUri,
                json,
                await _provider
                    .CreateHeadersAsync(options.Headers, cancellationToken)
                    .ConfigureAwait(false),
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

    private JsonObject Transform(JsonObject body, List<CallWarning> warnings) =>
        _provider.Options.TransformRequestBody?.Invoke(body, warnings) ?? body;

    private static string? ContentType(JsonElement element) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty("type", out var type)
        && type.ValueKind == JsonValueKind.String
            ? type.GetString()
            : throw Invalid("Content part needs a type.", element);

    private static string ExtractReasoningContent(JsonElement part)
    {
        var builder = new StringBuilder();
        foreach (var chunk in Require(part, "thinking", JsonValueKind.Array).EnumerateArray())
        {
            if (ContentType(chunk) == "text")
            {
                builder.Append(RequireString(chunk, "text"));
            }
        }

        return builder.ToString();
    }

    private static string? ExtractTextContent(JsonElement content)
    {
        switch (content.ValueKind)
        {
            case JsonValueKind.String:
                return content.GetString();
            case JsonValueKind.Array:
                var builder = new StringBuilder();
                var any = false;
                foreach (var chunk in content.EnumerateArray())
                {
                    switch (ContentType(chunk))
                    {
                        case "text":
                            builder.Append(RequireString(chunk, "text"));
                            any = true;
                            break;
                        // Thinking, image content, and reference content are currently ignored.
                        case "thinking":
                        case "image_url":
                        case "reference":
                            break;
                        default:
                            throw Invalid("Unsupported type: " + ContentType(chunk), chunk);
                    }
                }

                return any && builder.Length > 0 ? builder.ToString() : null;
            default:
                return null;
        }
    }

    private static DateTimeOffset? Created(JsonElement root) =>
        root.TryGetProperty("created", out var created)
        && created.ValueKind == JsonValueKind.Number
        && created.TryGetInt64(out var seconds)
        && seconds is >= -62135596800 and <= 253402300799
            ? DateTimeOffset.FromUnixTimeSeconds(seconds)
            : null;

    private static TypeValidationException Invalid(string message, JsonElement value) =>
        new("Invalid Mistral chat response: " + message, value.Clone());

    private static JsonElement Require(JsonElement owner, string name, JsonValueKind kind)
    {
        if (
            owner.ValueKind == JsonValueKind.Object
            && owner.TryGetProperty(name, out var value)
            && value.ValueKind == kind
        )
        {
            return value;
        }

        throw Invalid("'" + name + "' is missing or has the wrong type.", owner);
    }

    private static string RequireString(JsonElement owner, string name) =>
        Require(owner, name, JsonValueKind.String).GetString()!;

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
                : throw Invalid("'" + name + "' must be a string.", owner);
        }

        return null;
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
            throw Invalid("The response must be an object.", root);
        }

        return document;
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
            .Language.Reasoning;
        string? resolvedReasoningEffort = null;
        if (_provider.Options.ResolveReasoningEffort is { } resolveReasoningEffort)
        {
            resolvedReasoningEffort = resolveReasoningEffort(
                ModelId,
                options.Reasoning,
                providerOptions.ReasoningEffort,
                warnings
            );
        }
        else if (reasoningCapabilities.Support)
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

        var args = new JsonObject { ["model"] = ModelId };
        Set(args, "safe_prompt", providerOptions.SafePrompt);
        Set(args, "max_tokens", options.MaxOutputTokens);
        Set(args, "temperature", options.Temperature);
        Set(args, "top_p", options.TopP);
        Set(args, "frequency_penalty", options.FrequencyPenalty);
        Set(args, "presence_penalty", options.PresencePenalty);
        if (options.StopSequences != null)
        {
            args["stop"] = new JsonArray(
                options.StopSequences.Select(stop => (JsonNode?)stop).ToArray()
            );
        }

        Set(args, "random_seed", options.Seed);
        if (resolvedReasoningEffort != null)
        {
            args["reasoning_effort"] = resolvedReasoningEffort;
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

                args["response_format"] = new JsonObject
                {
                    ["type"] = "json_schema",
                    ["json_schema"] = jsonSchema,
                };
            }
            else
            {
                args["response_format"] = new JsonObject { ["type"] = "json_object" };
            }
        }

        // Mistral-specific provider options.
        Set(args, "document_image_limit", providerOptions.DocumentImageLimit);
        Set(args, "document_page_limit", providerOptions.DocumentPageLimit);
        if (providerOptions.PromptCacheKey != null)
        {
            args["prompt_cache_key"] = providerOptions.PromptCacheKey;
        }

        args["messages"] = ConvertToMistralChatMessages.Convert(prompt);

        var prepared = MistralPrepareTools.PrepareTools(options.Tools, options.ToolChoice);
        if (prepared.Tools != null)
        {
            args["tools"] = prepared.Tools;
        }

        if (prepared.ToolChoice != null)
        {
            args["tool_choice"] = prepared.ToolChoice;
        }

        if (prepared.Tools != null && providerOptions.ParallelToolCalls != null)
        {
            args["parallel_tool_calls"] = providerOptions.ParallelToolCalls;
        }

        warnings.AddRange(prepared.ToolWarnings);
        return (args, warnings);
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

    private static void Set(JsonObject target, string name, bool? value)
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
        var toolCalls = new StreamingToolCallTracker(_generateId);

        // A clean end without the provider's completion signal is interrupted.
        var finishReason = FinishReason.Interrupted;
        string? rawFinishReason = null;
        JsonElement? usage = null;
        var isFirstChunk = true;
        var activeText = false;
        string? activeReasoningId = null;

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
                finishReason = FinishReason.Error;
                rawFinishReason = null;
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
                finishReason = FinishReason.Error;
                rawFinishReason = null;
                parts.Add(new ErrorStreamPart(error.Message));
            }

            foreach (var part in parts)
            {
                yield return part;
            }

            void Handle(JsonElement value)
            {
                if (value.ValueKind != JsonValueKind.Object)
                {
                    throw Invalid("The chunk must be an object.", value);
                }

                var choices = Require(value, "choices", JsonValueKind.Array);
                if (choices.GetArrayLength() == 0)
                {
                    throw Invalid("The chunk has no choices.", value);
                }

                var choice = choices[0];
                var delta = Require(choice, "delta", JsonValueKind.Object);

                // Validate the whole chunk before it changes any stream state.
                var finish = NullishString(choice, "finish_reason");
                var chunkId = NullishString(value, "id");
                var chunkModel = NullishString(value, "model");
                delta.TryGetProperty("content", out var deltaContent);
                var textContent = ExtractTextContent(deltaContent);
                var reasoningParts = new List<string>();
                if (deltaContent.ValueKind == JsonValueKind.Array)
                {
                    foreach (var part in deltaContent.EnumerateArray())
                    {
                        if (ContentType(part) == "thinking")
                        {
                            reasoningParts.Add(ExtractReasoningContent(part));
                        }
                    }
                }

                var toolDeltas = new List<StreamingToolCallDelta>();
                if (
                    delta.TryGetProperty("tool_calls", out var deltaToolCalls)
                    && deltaToolCalls.ValueKind == JsonValueKind.Array
                )
                {
                    foreach (var toolCall in deltaToolCalls.EnumerateArray())
                    {
                        var function = Require(toolCall, "function", JsonValueKind.Object);
                        int? index =
                            toolCall.TryGetProperty("index", out var indexValue)
                            && indexValue.ValueKind == JsonValueKind.Number
                            && indexValue.TryGetInt32(out var indexNumber)
                                ? indexNumber
                                : null;
                        toolDeltas.Add(
                            new StreamingToolCallDelta(
                                index,
                                NullishString(toolCall, "id"),
                                NullishString(function, "name"),
                                NullishString(function, "arguments")
                            )
                        );
                    }
                }

                JsonElement? chunkUsage = null;
                if (
                    value.TryGetProperty("usage", out var usageValue)
                    && usageValue.ValueKind != JsonValueKind.Null
                )
                {
                    chunkUsage = MistralUsageSchema.Parse(usageValue);
                }

                if (isFirstChunk)
                {
                    isFirstChunk = false;
                    parts.Add(new ResponseMetadataStreamPart(chunkId, chunkModel, Created(value)));
                }

                if (chunkUsage != null)
                {
                    usage = chunkUsage;
                }

                foreach (var reasoningDelta in reasoningParts)
                {
                    if (reasoningDelta.Length == 0)
                    {
                        continue;
                    }

                    if (activeReasoningId == null)
                    {
                        // End any active text before starting reasoning.
                        if (activeText)
                        {
                            parts.Add(new TextEndStreamPart("0"));
                            activeText = false;
                        }

                        activeReasoningId = _generateId();
                        parts.Add(new ReasoningStartStreamPart(activeReasoningId));
                    }

                    parts.Add(new ReasoningDeltaStreamPart(activeReasoningId, reasoningDelta));
                }

                if (!string.IsNullOrEmpty(textContent))
                {
                    if (!activeText)
                    {
                        // If we were in reasoning mode, end it before starting text.
                        if (activeReasoningId != null)
                        {
                            parts.Add(new ReasoningEndStreamPart(activeReasoningId));
                            activeReasoningId = null;
                        }

                        parts.Add(new TextStartStreamPart("0"));
                        activeText = true;
                    }

                    parts.Add(new TextDeltaStreamPart("0", textContent!));
                }

                foreach (var toolDelta in toolDeltas)
                {
                    var input = toolCalls.ProcessDelta(toolDelta);
                    if (input == null)
                    {
                        continue;
                    }

                    if (input.Started)
                    {
                        parts.Add(new ToolInputStartStreamPart(input.Id, input.Name));
                    }

                    if (input.ArgumentsDelta != null)
                    {
                        parts.Add(new ToolInputDeltaStreamPart(input.Id, input.ArgumentsDelta));
                    }
                }

                if (finish != null)
                {
                    finishReason = MapMistralFinishReason.Map(finish);
                    rawFinishReason = finish;
                }
            }
        }

        if (activeReasoningId != null)
        {
            yield return new ReasoningEndStreamPart(activeReasoningId);
        }

        if (activeText)
        {
            yield return new TextEndStreamPart("0");
        }

        foreach (var call in toolCalls.Flush())
        {
            yield return new ToolInputEndStreamPart(call.ToolCallId);
            yield return call;
        }

        yield return new FinishStreamPart(
            finishReason,
            ConvertMistralUsage.Convert(usage),
            rawFinishReason
        );
    }
}
