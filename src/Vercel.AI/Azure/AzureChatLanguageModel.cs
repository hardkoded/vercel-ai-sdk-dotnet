// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Azure;

/// <summary>Azure Chat Completions model. DeepSeek adds reasoning effort and strict JSON schema.</summary>
public sealed class AzureChatLanguageModel : ILanguageModel
{
    private readonly AzureOpenAIProvider _provider;
    private readonly bool _deepseek;

    /// <summary>Creates a chat model.</summary>
    public AzureChatLanguageModel(AzureOpenAIProvider provider, string modelId, bool deepseek)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _deepseek = deepseek;
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => AzureOpenAIProvider.ProviderId;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        options ??= new LanguageModelCallOptions();
        var body = BuildBody(options, stream: false);
        var response = await _provider.PostJsonAsync(
            _provider.DeploymentUri(ModelId, "chat/completions"),
            body.ToJsonString(),
            options.Headers,
            cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body);
        return ParseGenerate(document.RootElement, response.Headers, response.Body);
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

    /// <summary>Opens a chat stream and returns response headers before the first part.</summary>
    public async Task<AzureStreamResponse> OpenStreamAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        options ??= new LanguageModelCallOptions();
        var body = BuildBody(options, stream: true);
        var response = await _provider.SendStreamAsync(
            _provider.DeploymentUri(ModelId, "chat/completions"),
            body.ToJsonString(),
            options.Headers,
            cancellationToken).ConfigureAwait(false);
        var headers = AzureOpenAIProvider.CopyResponseHeaders(response);
        return new AzureStreamResponse(headers, Read(response, options.IncludeRawChunks, cancellationToken));
    }

    private async IAsyncEnumerable<LanguageModelStreamPart> Read(
        HttpResponseMessage response,
        bool includeRaw,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using (response)
        {
            yield return new StreamStartStreamPart(Array.Empty<CallWarning>());
            string? responseId = null;
            string? responseModel = null;
            long? created = null;
            var metadataSent = false;
            var reasoningOpen = false;
            var textOpen = false;
            string? finishRaw = null;
            int? choiceIndex = null;
            string? messageRole = null;
            string? responseObject = null;
            JsonElement? usage = null;
            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            await foreach (var data in SseParser.ReadDataAsync(stream, cancellationToken).ConfigureAwait(false))
            {
                if (includeRaw)
                {
                    yield return new RawStreamPart(data);
                }

                JsonDocument? document = null;
                try
                {
                    document = JsonDocument.Parse(data);
                }
                catch (JsonException)
                {
                    continue;
                }

                if (document == null)
                {
                    continue;
                }

                using (document)
                {
                    var root = document.RootElement;
                    var objectName = AzureJson.String(root, "object");
                    if (!string.IsNullOrEmpty(objectName))
                    {
                        responseObject = objectName;
                    }

                    if (!metadataSent && root.TryGetProperty("id", out _))
                    {
                        responseId = AzureJson.String(root, "id");
                        responseModel = AzureJson.String(root, "model");
                        created = AzureJson.Int(root, "created");
                        metadataSent = true;
                        DateTimeOffset? timestamp = created is { } seconds ? DateTimeOffset.FromUnixTimeSeconds(seconds) : null;
                        yield return new ResponseMetadataStreamPart(responseId, responseModel, timestamp);
                    }

                    if (AzureJson.Object(root, "usage", out var usageElement))
                    {
                        usage = AzureJson.Clone(usageElement);
                    }

                    if (!root.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array || choices.GetArrayLength() == 0)
                    {
                        continue;
                    }

                    var choice = choices[0];
                    choiceIndex = AzureJson.Int(choice, "index") ?? choiceIndex ?? 0;
                    var finish = AzureJson.String(choice, "finish_reason");
                    if (!string.IsNullOrEmpty(finish))
                    {
                        finishRaw = finish;
                    }

                    if (!choice.TryGetProperty("delta", out var delta) || delta.ValueKind != JsonValueKind.Object)
                    {
                        continue;
                    }

                    var role = AzureJson.String(delta, "role");
                    if (!string.IsNullOrEmpty(role))
                    {
                        messageRole = role;
                    }

                    var reasoning = AzureJson.String(delta, "reasoning_content");
                    if (!string.IsNullOrEmpty(reasoning))
                    {
                        if (!reasoningOpen)
                        {
                            reasoningOpen = true;
                            yield return new ReasoningStartStreamPart("reasoning-0");
                        }

                        yield return new ReasoningDeltaStreamPart("reasoning-0", reasoning!);
                    }

                    var text = AzureJson.String(delta, "content");
                    if (!string.IsNullOrEmpty(text))
                    {
                        if (!textOpen)
                        {
                            textOpen = true;
                            yield return new TextStartStreamPart("txt-0");
                            if (reasoningOpen)
                            {
                                reasoningOpen = false;
                                yield return new ReasoningEndStreamPart("reasoning-0");
                            }
                        }

                        yield return new TextDeltaStreamPart("txt-0", text!);
                    }
                }
            }

            if (textOpen)
            {
                yield return new TextEndStreamPart("txt-0");
            }

            if (reasoningOpen)
            {
                yield return new ReasoningEndStreamPart("reasoning-0");
            }

            yield return new FinishStreamPart(
                FinishReasons.Parse(finishRaw),
                ConvertUsage(usage),
                finishRaw,
                FinishMetadata(choiceIndex, messageRole, responseObject));
        }
    }

    private JsonObject BuildBody(LanguageModelCallOptions options, bool stream)
    {
        var messages = new JsonArray();
        foreach (var message in options.Prompt)
        {
            messages.Add(MapMessage(message));
        }

        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["messages"] = messages,
        };
        if (stream)
        {
            body["stream"] = true;
            body["stream_options"] = new JsonObject { ["include_usage"] = true };
        }

        AzureJson.Set(body, "temperature", options.Temperature);
        AzureJson.Set(body, "top_p", options.TopP);
        AzureJson.Set(body, "frequency_penalty", options.FrequencyPenalty);
        AzureJson.Set(body, "presence_penalty", options.PresencePenalty);
        AzureJson.Set(body, "max_tokens", options.MaxOutputTokens);
        AzureJson.Set(body, "seed", options.Seed);
        if (options.StopSequences is { Count: > 0 })
        {
            var stop = new JsonArray();
            foreach (var sequence in options.StopSequences)
            {
                stop.Add(sequence);
            }

            body["stop"] = stop;
        }

        if (_deepseek)
        {
            var effort = ReasoningEffort(options);
            AzureJson.Set(body, "reasoning_effort", effort);
            if (options.JsonSchema is { } schema)
            {
                body["response_format"] = new JsonObject
                {
                    ["type"] = "json_schema",
                    ["json_schema"] = new JsonObject
                    {
                        ["name"] = options.JsonSchemaName ?? "response",
                        ["strict"] = true,
                        ["schema"] = JsonNode.Parse(schema.GetRawText()),
                    },
                };
            }
        }

        return body;
    }

    private static string? ReasoningEffort(LanguageModelCallOptions options)
    {
        if (AzureJson.ProviderOptions(options.ProviderOptions, "azure", out var azure))
        {
            var effort = AzureJson.String(azure, "reasoningEffort");
            if (!string.IsNullOrEmpty(effort))
            {
                return effort;
            }
        }

        if (string.IsNullOrEmpty(options.Reasoning) || options.Reasoning == "provider-default")
        {
            return null;
        }

        return options.Reasoning;
    }

    private static JsonObject MapMessage(ModelMessage message)
    {
        if (message is SystemModelMessage system)
        {
            return new JsonObject { ["role"] = "system", ["content"] = system.Content };
        }

        if (message is UserModelMessage user)
        {
            var text = new System.Text.StringBuilder();
            var onlyText = true;
            foreach (var part in user.Content)
            {
                if (part is TextContentPart textPart)
                {
                    text.Append(textPart.Text);
                }
                else
                {
                    onlyText = false;
                }
            }

            if (onlyText)
            {
                return new JsonObject { ["role"] = "user", ["content"] = text.ToString() };
            }

            var content = new JsonArray();
            foreach (var part in user.Content)
            {
                if (part is TextContentPart textPart && textPart.Text.Length > 0)
                {
                    content.Add(new JsonObject { ["type"] = "text", ["text"] = textPart.Text });
                }
            }

            return new JsonObject { ["role"] = "user", ["content"] = content };
        }

        if (message is AssistantModelMessage assistant)
        {
            return new JsonObject { ["role"] = "assistant", ["content"] = assistant.Text ?? string.Empty };
        }

        if (message is ToolModelMessage tool)
        {
            return new JsonObject
            {
                ["role"] = "tool",
                ["tool_call_id"] = tool.ToolCallId,
                ["content"] = tool.OutputJson,
            };
        }

        return new JsonObject { ["role"] = "user", ["content"] = string.Empty };
    }

    private static LanguageModelGenerateResult ParseGenerate(JsonElement root, IReadOnlyDictionary<string, string> headers, string raw)
    {
        var content = new List<GeneratedContent>();
        string? finish = null;
        if (root.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0)
        {
            var choice = choices[0];
            finish = AzureJson.String(choice, "finish_reason");
            if (choice.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.Object)
            {
                var reasoning = AzureJson.String(message, "reasoning_content");
                if (!string.IsNullOrEmpty(reasoning))
                {
                    content.Add(new GeneratedReasoning(reasoning!));
                }

                var text = AzureJson.String(message, "content");
                if (!string.IsNullOrEmpty(text))
                {
                    content.Add(new GeneratedText(text!));
                }
            }
        }

        JsonElement? usage = null;
        if (AzureJson.Object(root, "usage", out var usageElement))
        {
            usage = AzureJson.Clone(usageElement);
        }

        return new LanguageModelGenerateResult(
            content,
            FinishReasons.Parse(finish),
            ConvertUsage(usage),
            finish,
            responseHeaders: headers,
            rawResponse: raw,
            responseId: AzureJson.String(root, "id"),
            responseModelId: AzureJson.String(root, "model"));
    }

    private static LanguageModelUsage ConvertUsage(JsonElement? usage)
    {
        if (usage is null || usage.Value.ValueKind != JsonValueKind.Object)
        {
            return LanguageModelUsage.Empty;
        }

        var element = usage.Value;
        var input = AzureJson.Int(element, "prompt_tokens");
        var output = AzureJson.Int(element, "completion_tokens");
        var total = AzureJson.Int(element, "total_tokens");
        var cacheRead = 0;
        if (element.TryGetProperty("prompt_cache_hit_tokens", out var hit) && hit.ValueKind == JsonValueKind.Number && hit.TryGetInt32(out var hitTokens))
        {
            cacheRead = hitTokens;
        }

        var reasoning = 0;
        if (AzureJson.Object(element, "completion_tokens_details", out var details))
        {
            reasoning = AzureJson.Int(details, "reasoning_tokens") ?? 0;
        }

        return new LanguageModelUsage(input, output, total, cacheRead, null, reasoning, AzureJson.Clone(element));
    }

    private static JsonElement? FinishMetadata(int? choiceIndex, string? messageRole, string? responseObject)
    {
        var azure = new JsonObject();
        if (choiceIndex is { } index)
        {
            azure["choiceIndex"] = index;
        }

        if (!string.IsNullOrEmpty(messageRole))
        {
            azure["messageRole"] = messageRole;
        }
        else if (choiceIndex != null)
        {
            azure["messageRole"] = "assistant";
        }

        if (!string.IsNullOrEmpty(responseObject))
        {
            azure["responseObject"] = responseObject;
        }

        if (azure.Count == 0)
        {
            return null;
        }

        return AzureJson.Element(new JsonObject { ["azure"] = azure });
    }
}
