// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Azure;

/// <summary>Azure Responses model. Requests still use the deployments URL and the configured API version.</summary>
public sealed class AzureResponsesLanguageModel : ILanguageModel
{
    private readonly AzureOpenAIProvider _provider;

    /// <summary>Creates a Responses model.</summary>
    public AzureResponsesLanguageModel(AzureOpenAIProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => AzureOpenAIProvider.ProviderId;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        return GenerateAsync(options, null, cancellationToken);
    }

    /// <summary>Generates a response, including provider-defined tools.</summary>
    public async Task<LanguageModelGenerateResult> GenerateAsync(
        LanguageModelCallOptions options,
        IReadOnlyList<AzureProviderTool>? providerTools,
        CancellationToken cancellationToken)
    {
        options ??= new LanguageModelCallOptions();
        var warnings = new List<CallWarning>();
        var body = BuildBody(options, providerTools, warnings);
        var response = await _provider.PostJsonAsync(
            _provider.DeploymentUri(ModelId, "responses"),
            body.ToJsonString(),
            options.Headers,
            cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body);
        return Parse(document.RootElement, response.Headers, response.Body, warnings);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
        LanguageModelCallOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var opened = await OpenStreamAsync(options, null, cancellationToken).ConfigureAwait(false);
        await foreach (var part in opened.Parts.ConfigureAwait(false))
        {
            yield return part;
        }
    }

    /// <summary>Opens a Responses stream and returns headers before the first part.</summary>
    public async Task<AzureStreamResponse> OpenStreamAsync(
        LanguageModelCallOptions options,
        IReadOnlyList<AzureProviderTool>? providerTools,
        CancellationToken cancellationToken)
    {
        options ??= new LanguageModelCallOptions();
        var warnings = new List<CallWarning>();
        var body = BuildBody(options, providerTools, warnings);
        body["stream"] = true;
        var response = await _provider.SendStreamAsync(
            _provider.DeploymentUri(ModelId, "responses"),
            body.ToJsonString(),
            options.Headers,
            cancellationToken).ConfigureAwait(false);
        var headers = AzureOpenAIProvider.CopyResponseHeaders(response);
        return new AzureStreamResponse(headers, Read(response, warnings, options.IncludeRawChunks, cancellationToken));
    }

    private JsonObject BuildBody(LanguageModelCallOptions options, IReadOnlyList<AzureProviderTool>? providerTools, List<CallWarning> warnings)
    {
        var azure = default(JsonElement);
        var hasAzure = AzureJson.ProviderOptions(options.ProviderOptions, "azure", out azure);
        var input = new JsonArray();
        foreach (var message in options.Prompt)
        {
            var mapped = MapMessage(message);
            if (mapped != null)
            {
                input.Add(mapped);
            }
        }

        var include = new List<string>();
        if (hasAzure && azure.TryGetProperty("include", out var includeElement) && includeElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in includeElement.EnumerateArray())
            {
                var value = item.GetString();
                if (!string.IsNullOrEmpty(value))
                {
                    include.Add(value!);
                }
            }
        }

        var tools = new JsonArray();
        if (options.Tools != null)
        {
            foreach (var tool in options.Tools)
            {
                tools.Add(new JsonObject
                {
                    ["type"] = "function",
                    ["name"] = tool.Name,
                    ["parameters"] = JsonNode.Parse(tool.InputSchema.GetRawText()),
                    ["strict"] = tool.Strict ?? false,
                });
            }
        }

        if (providerTools != null)
        {
            foreach (var tool in providerTools)
            {
                var mapped = MapProviderTool(tool);
                if (mapped != null)
                {
                    tools.Add(mapped);
                }

                if (tool.Id == "openai.code_interpreter")
                {
                    AddInclude(include, "code_interpreter_call.outputs");
                }
            }
        }

        var store = hasAzure && azure.TryGetProperty("store", out var storeElement) && (storeElement.ValueKind == JsonValueKind.True || storeElement.ValueKind == JsonValueKind.False)
            ? (bool?)(storeElement.ValueKind == JsonValueKind.True)
            : null;
        var forceReasoning = hasAzure && azure.TryGetProperty("forceReasoning", out var forceElement) && forceElement.ValueKind == JsonValueKind.True;
        if (store == false && forceReasoning)
        {
            AddInclude(include, "reasoning.encrypted_content");
        }

        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["input"] = input,
        };
        AzureJson.Set(body, "temperature", options.Temperature);
        AzureJson.Set(body, "top_p", options.TopP);
        if (hasAzure && AzureJson.Int(azure, "maxCompletionTokens") is { } maxCompletion)
        {
            body["max_output_tokens"] = maxCompletion;
        }
        else
        {
            AzureJson.Set(body, "max_output_tokens", options.MaxOutputTokens);
        }

        if (store != null)
        {
            body["store"] = store;
        }

        if (include.Count > 0)
        {
            var includeArray = new JsonArray();
            foreach (var item in include)
            {
                includeArray.Add(item);
            }

            body["include"] = includeArray;
        }

        if (tools.Count > 0)
        {
            body["tools"] = tools;
        }

        var effort = hasAzure ? AzureJson.String(azure, "reasoningEffort") : null;
        if (string.IsNullOrEmpty(effort) && !string.IsNullOrEmpty(options.Reasoning) && options.Reasoning != "provider-default" && options.Reasoning != "none")
        {
            effort = options.Reasoning;
        }

        var summary = hasAzure ? AzureJson.String(azure, "reasoningSummary") : null;
        if (!string.IsNullOrEmpty(effort) || !string.IsNullOrEmpty(summary))
        {
            var reasoning = new JsonObject();
            AzureJson.Set(reasoning, "effort", effort);
            AzureJson.Set(reasoning, "summary", summary);
            body["reasoning"] = reasoning;
        }

        if (warnings.Count == 0 && options.TopK != null)
        {
            warnings.Add(new CallWarning("unsupported", "topK"));
        }

        return body;
    }

    private JsonObject? MapMessage(ModelMessage message)
    {
        var explicitType = _provider.UseExplicitMessageTypes;
        if (message is SystemModelMessage system)
        {
            var item = new JsonObject { ["role"] = "system", ["content"] = system.Content };
            if (explicitType)
            {
                item["type"] = "message";
            }

            return item;
        }

        if (message is UserModelMessage user)
        {
            var content = new JsonArray();
            foreach (var part in user.Content)
            {
                if (part is TextContentPart text && text.Text.Length > 0)
                {
                    content.Add(new JsonObject { ["type"] = "input_text", ["text"] = text.Text });
                }
                else if (part is AzureInputFile file)
                {
                    content.Add(MapFile(file));
                }
            }

            var item = new JsonObject { ["role"] = "user", ["content"] = content };
            if (explicitType)
            {
                item["type"] = "message";
            }

            return item;
        }

        if (message is AssistantModelMessage assistant)
        {
            if (explicitType && assistant.ToolCalls.Count == 0)
            {
                return new JsonObject
                {
                    ["type"] = "message",
                    ["role"] = "assistant",
                    ["content"] = assistant.Text ?? string.Empty,
                };
            }

            var content = new JsonArray();
            if (!string.IsNullOrEmpty(assistant.Text))
            {
                content.Add(new JsonObject { ["type"] = "output_text", ["text"] = assistant.Text });
            }

            return new JsonObject { ["role"] = "assistant", ["content"] = content };
        }

        return null;
    }

    private static JsonObject MapFile(AzureInputFile file)
    {
        var assistant = file.Data.StartsWith("assistant-", StringComparison.Ordinal);
        var media = file.MediaType ?? string.Empty;
        if (assistant && media.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return new JsonObject { ["type"] = "input_image", ["file_id"] = file.Data };
        }

        if (assistant && media.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
        {
            return new JsonObject { ["type"] = "input_file", ["file_id"] = file.Data };
        }

        if (media.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return new JsonObject
            {
                ["type"] = "input_image",
                ["image_url"] = "data:" + media + ";base64," + file.Data,
            };
        }

        return new JsonObject
        {
            ["type"] = "input_file",
            ["filename"] = "file",
            ["file_data"] = "data:" + media + ";base64," + file.Data,
        };
    }

    private static JsonObject? MapProviderTool(AzureProviderTool tool)
    {
        var args = tool.Arguments.ValueKind == JsonValueKind.Object ? tool.Arguments : default;
        if (tool.Id == "openai.file_search" || tool.Name == "file_search")
        {
            var item = new JsonObject { ["type"] = "file_search" };
            if (args.ValueKind == JsonValueKind.Object && args.TryGetProperty("vectorStoreIds", out var stores) && stores.ValueKind == JsonValueKind.Array)
            {
                item["vector_store_ids"] = JsonNode.Parse(stores.GetRawText());
            }

            if (args.ValueKind == JsonValueKind.Object && AzureJson.Int(args, "maxNumResults") is { } max)
            {
                item["max_num_results"] = max;
            }

            if (args.ValueKind == JsonValueKind.Object && args.TryGetProperty("ranking", out var ranking) && ranking.ValueKind == JsonValueKind.Object)
            {
                var options = new JsonObject();
                AzureJson.Set(options, "ranker", AzureJson.String(ranking, "ranker"));
                if (ranking.TryGetProperty("scoreThreshold", out var threshold) && threshold.ValueKind == JsonValueKind.Number)
                {
                    options["score_threshold"] = threshold.GetDouble();
                }

                if (options.Count > 0)
                {
                    item["ranking_options"] = options;
                }
            }

            if (args.ValueKind == JsonValueKind.Object && args.TryGetProperty("filters", out var filters) && filters.ValueKind == JsonValueKind.Object)
            {
                item["filters"] = JsonNode.Parse(filters.GetRawText());
            }

            return item;
        }

        if (tool.Id == "openai.code_interpreter" || tool.Name == "code_interpreter")
        {
            return new JsonObject
            {
                ["type"] = "code_interpreter",
                ["container"] = new JsonObject { ["type"] = "auto" },
            };
        }

        if (tool.Id == "openai.image_generation" || tool.Name == "image_generation")
        {
            var item = new JsonObject { ["type"] = "image_generation" };
            if (args.ValueKind == JsonValueKind.Object)
            {
                AzureJson.Set(item, "output_format", AzureJson.String(args, "outputFormat"));
                AzureJson.Set(item, "quality", AzureJson.String(args, "quality"));
                AzureJson.Set(item, "size", AzureJson.String(args, "size"));
                AzureJson.Set(item, "partial_images", AzureJson.Int(args, "partialImages"));
            }

            return item;
        }

        if (tool.Id == "openai.web_search_preview" || tool.Name == "web_search_preview")
        {
            return new JsonObject { ["type"] = "web_search_preview" };
        }

        return new JsonObject { ["type"] = tool.Name };
    }

    private static void AddInclude(List<string> include, string value)
    {
        if (!include.Contains(value))
        {
            include.Add(value);
        }
    }

    private static LanguageModelGenerateResult Parse(
        JsonElement root,
        IReadOnlyDictionary<string, string> headers,
        string raw,
        IReadOnlyList<CallWarning> warnings)
    {
        var content = new List<GeneratedContent>();
        var sawFunction = false;
        if (root.TryGetProperty("output", out var output) && output.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in output.EnumerateArray())
            {
                var type = AzureJson.String(item, "type");
                if (type == "reasoning")
                {
                    content.Add(ParseReasoning(item));
                }
                else if (type == "message")
                {
                    content.Add(ParseMessage(item));
                }
                else if (type == "function_call")
                {
                    sawFunction = true;
                    content.Add(ParseFunction(item));
                }
                else if (type == "code_interpreter_call")
                {
                    AddProviderCall(content, item, "code_interpreter", CodeInterpreterInput(item), CodeInterpreterResult(item));
                }
                else if (type == "file_search_call")
                {
                    AddProviderCall(content, item, "file_search", "{}", FileSearchResult(item));
                }
                else if (type == "image_generation_call")
                {
                    AddProviderCall(content, item, "image_generation", "{}", ImageResult(item));
                }
                else if (type == "web_search_call" || type == "web_search")
                {
                    var name = type == "web_search" ? "web_search" : "web_search_preview";
                    AddProviderCall(content, item, name, "{}", WebSearchResult(item));
                }
            }
        }

        var usage = AzureJson.Object(root, "usage", out var usageElement) ? ConvertUsage(usageElement) : LanguageModelUsage.Empty;
        var metadata = new JsonObject
        {
            ["responseId"] = JsonValue.Create(AzureJson.String(root, "id")),
        };
        var tier = AzureJson.String(root, "service_tier");
        if (tier != null)
        {
            metadata["serviceTier"] = tier;
        }

        var created = AzureJson.Int(root, "created_at");
        return new LanguageModelGenerateResult(
            content,
            sawFunction ? FinishReason.ToolCalls : FinishReason.Stop,
            usage,
            null,
            warnings,
            AzureJson.String(root, "id"),
            AzureJson.Element(new JsonObject { ["azure"] = metadata }),
            raw,
            AzureJson.String(root, "model"),
            created is null ? null : DateTimeOffset.FromUnixTimeSeconds(created.Value),
            headers);
    }

    private static AzureGeneratedReasoning ParseReasoning(JsonElement item)
    {
        var text = new System.Text.StringBuilder();
        if (item.TryGetProperty("summary", out var summary) && summary.ValueKind == JsonValueKind.Array)
        {
            foreach (var part in summary.EnumerateArray())
            {
                var piece = AzureJson.String(part, "text");
                if (!string.IsNullOrEmpty(piece))
                {
                    text.Append(piece);
                }
            }
        }

        var azure = new JsonObject
        {
            ["itemId"] = AzureJson.String(item, "id"),
            ["reasoningEncryptedContent"] = item.TryGetProperty("encrypted_content", out var encrypted) && encrypted.ValueKind == JsonValueKind.String
                ? JsonValue.Create(encrypted.GetString())
                : AzureJson.Null(),
        };
        return new AzureGeneratedReasoning(text.ToString(), AzureJson.Element(new JsonObject { ["azure"] = azure }));
    }

    private static AzureGeneratedText ParseMessage(JsonElement item)
    {
        var text = new System.Text.StringBuilder();
        JsonNode? annotations = null;
        if (item.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.Array)
        {
            foreach (var part in content.EnumerateArray())
            {
                if (AzureJson.String(part, "type") == "output_text")
                {
                    text.Append(AzureJson.String(part, "text"));
                    if (part.TryGetProperty("annotations", out var found) && found.ValueKind == JsonValueKind.Array && found.GetArrayLength() > 0)
                    {
                        annotations = JsonNode.Parse(found.GetRawText());
                    }
                }
            }
        }

        var azure = new JsonObject { ["itemId"] = AzureJson.String(item, "id") };
        if (annotations != null)
        {
            azure["annotations"] = annotations;
        }

        return new AzureGeneratedText(text.ToString(), AzureJson.Element(new JsonObject { ["azure"] = azure }));
    }

    private static GeneratedToolCall ParseFunction(JsonElement item)
    {
        var callId = AzureJson.String(item, "call_id") ?? AzureJson.String(item, "id") ?? string.Empty;
        var arguments = AzureJson.String(item, "arguments") ?? "{}";
        var metadata = AzureJson.Element(new JsonObject
        {
            ["azure"] = new JsonObject { ["itemId"] = AzureJson.String(item, "id") },
        });
        return new GeneratedToolCall(callId, AzureJson.String(item, "name") ?? string.Empty, arguments, metadata);
    }

    private static void AddProviderCall(List<GeneratedContent> content, JsonElement item, string name, string input, string result)
    {
        var id = AzureJson.String(item, "id") ?? string.Empty;
        var metadata = AzureJson.Element(new JsonObject
        {
            ["azure"] = new JsonObject { ["itemId"] = id },
        });
        content.Add(new GeneratedToolCall(id, name, input, metadata));
        content.Add(new AzureToolResult(id, name, result));
    }

    private static string CodeInterpreterInput(JsonElement item)
    {
        var input = new JsonObject();
        AzureJson.Set(input, "code", AzureJson.String(item, "code"));
        AzureJson.Set(input, "containerId", AzureJson.String(item, "container_id"));
        return input.ToJsonString();
    }

    private static string CodeInterpreterResult(JsonElement item)
    {
        var outputs = item.TryGetProperty("outputs", out var value) ? JsonNode.Parse(value.GetRawText()) : new JsonArray();
        return new JsonObject { ["outputs"] = outputs }.ToJsonString();
    }

    private static string FileSearchResult(JsonElement item)
    {
        var result = new JsonObject();
        result["queries"] = item.TryGetProperty("queries", out var queries) ? JsonNode.Parse(queries.GetRawText()) : new JsonArray();
        result["results"] = item.TryGetProperty("results", out var results) && results.ValueKind != JsonValueKind.Null
            ? JsonNode.Parse(results.GetRawText())
            : AzureJson.Null();
        return result.ToJsonString();
    }

    private static string ImageResult(JsonElement item)
    {
        return new JsonObject { ["result"] = AzureJson.String(item, "result") }.ToJsonString();
    }

    private static string WebSearchResult(JsonElement item)
    {
        var action = item.TryGetProperty("action", out var value) && value.ValueKind == JsonValueKind.Object
            ? JsonNode.Parse(value.GetRawText())
            : new JsonObject();
        return new JsonObject { ["action"] = action }.ToJsonString();
    }

    private static LanguageModelUsage ConvertUsage(JsonElement usage)
    {
        var input = AzureJson.Int(usage, "input_tokens");
        var output = AzureJson.Int(usage, "output_tokens");
        var total = AzureJson.Int(usage, "total_tokens");
        var cacheRead = 0;
        if (AzureJson.Object(usage, "input_tokens_details", out var inputDetails))
        {
            cacheRead = AzureJson.Int(inputDetails, "cached_tokens") ?? 0;
        }

        var reasoning = 0;
        if (AzureJson.Object(usage, "output_tokens_details", out var outputDetails))
        {
            reasoning = AzureJson.Int(outputDetails, "reasoning_tokens") ?? 0;
        }

        return new LanguageModelUsage(input, output, total, cacheRead, null, reasoning, AzureJson.Clone(usage));
    }

    private async IAsyncEnumerable<LanguageModelStreamPart> Read(
        HttpResponseMessage response,
        IReadOnlyList<CallWarning> warnings,
        bool includeRaw,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using (response)
        {
            yield return new StreamStartStreamPart(warnings);
            string? responseId = null;
            var sawCreated = false;
            var sourceIndex = 0;
            var annotations = new JsonArray();
            string? textId = null;
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
                    var type = AzureJson.String(root, "type");
                    if (type == "response.created" && root.TryGetProperty("response", out var created) && created.ValueKind == JsonValueKind.Object)
                    {
                        sawCreated = true;
                        responseId = AzureJson.String(created, "id");
                        var createdAt = AzureJson.Int(created, "created_at");
                        DateTimeOffset? createdTimestamp = createdAt is { } seconds ? DateTimeOffset.FromUnixTimeSeconds(seconds) : null;
                        yield return new ResponseMetadataStreamPart(
                            responseId,
                            AzureJson.String(created, "model"),
                            createdTimestamp);
                    }
                    else if (type == "response.output_text.annotation.added" && root.TryGetProperty("annotation", out var annotation))
                    {
                        if (AzureJson.String(annotation, "type") == "file_citation")
                        {
                            var filename = AzureJson.String(annotation, "filename") ?? string.Empty;
                            var fileId = AzureJson.String(annotation, "file_id");
                            var index = AzureJson.Int(annotation, "index") ?? 0;
                            var metadata = new JsonObject
                            {
                                ["fileId"] = fileId,
                                ["index"] = index,
                                ["type"] = "file_citation",
                            };
                            annotations.Add(JsonNode.Parse(annotation.GetRawText()));
                            textId = AzureJson.String(root, "item_id") ?? textId;
                            yield return new AzureDocumentSourceStreamPart(
                                "id-" + sourceIndex.ToString(),
                                filename,
                                filename,
                                AzureJson.Element(new JsonObject { ["azure"] = metadata }));
                            sourceIndex++;
                        }
                    }
                    else if (type == "response.output_item.done" && root.TryGetProperty("item", out var item) && AzureJson.String(item, "type") == "message")
                    {
                        var id = AzureJson.String(item, "id") ?? textId ?? string.Empty;
                        JsonArray itemAnnotations = annotations;
                        if (item.TryGetProperty("content", out var parts) && parts.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var part in parts.EnumerateArray())
                            {
                                if (part.TryGetProperty("annotations", out var found) && found.ValueKind == JsonValueKind.Array)
                                {
                                    itemAnnotations = (JsonArray)JsonNode.Parse(found.GetRawText())!;
                                }
                            }
                        }

                        var metadata = new JsonObject
                        {
                            ["annotations"] = itemAnnotations,
                            ["itemId"] = id,
                        };
                        yield return new AzureAnnotatedTextEndStreamPart(id, AzureJson.Element(new JsonObject { ["azure"] = metadata }));
                    }
                    else if (type == "response.completed" && root.TryGetProperty("response", out var completed))
                    {
                        if (AzureJson.Object(completed, "usage", out var usageElement))
                        {
                            usage = AzureJson.Clone(usageElement);
                        }
                    }
                }
            }

            var finishMetadata = new JsonObject
            {
                ["responseId"] = sawCreated ? JsonValue.Create(responseId) : AzureJson.Null(),
            };
            yield return new FinishStreamPart(
                FinishReason.Stop,
                usage is { } value ? ConvertUsage(value) : LanguageModelUsage.Empty,
                null,
                AzureJson.Element(new JsonObject { ["azure"] = finishMetadata }));
        }
    }
}
