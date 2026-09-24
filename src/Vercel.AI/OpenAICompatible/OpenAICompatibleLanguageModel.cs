// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAICompatible;

/// <summary>Chat Completions language model.</summary>
public sealed class OpenAICompatibleLanguageModel : ILanguageModel
{
    private readonly OpenAICompatibleProvider _provider;

    /// <summary>Creates a language model.</summary>
    public OpenAICompatibleLanguageModel(OpenAICompatibleProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => _provider.Name;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        var body = BuildBody(options, stream: false);
        using var document = await _provider.Http.SendJsonAsync(
            HttpMethod.Post,
            _provider.ChatUri(ModelId),
            body.ToJsonString(),
            _provider.CreateHeaders(),
            cancellationToken).ConfigureAwait(false);
        return ParseGenerate(document.RootElement);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
        LanguageModelCallOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var body = BuildBody(options, stream: true);
        var toolCalls = new SortedDictionary<int, ToolAccumulator>();
        string? finishRaw = null;
        LanguageModelUsage? usage = null;
        await foreach (var data in _provider.Http.SendSseAsync(
            _provider.ChatUri(ModelId),
            body.ToJsonString(),
            _provider.CreateHeaders(),
            cancellationToken).ConfigureAwait(false))
        {
            JsonNode? node;
            try
            {
                node = JsonNode.Parse(data);
            }
            catch (System.Text.Json.JsonException)
            {
                continue;
            }

            if (node is not JsonObject root)
            {
                continue;
            }

            var usageNode = root["usage"];
            if (usageNode is JsonObject usageObject)
            {
                usage = ReadUsage(usageObject);
            }

            var choice = root["choices"]?[0] as JsonObject;
            if (choice is null)
            {
                continue;
            }

            var finishText = AsString(choice["finish_reason"]);
            if (!string.IsNullOrEmpty(finishText))
            {
                finishRaw = finishText;
            }

            if (choice["delta"] is not JsonObject delta)
            {
                continue;
            }

            var text = AsString(delta["content"]);
            if (!string.IsNullOrEmpty(text))
            {
                yield return new TextDeltaStreamPart("text", text!);
            }

            var reasoning = AsString(delta["reasoning_content"]);
            if (!string.IsNullOrEmpty(reasoning))
            {
                yield return new ReasoningDeltaStreamPart("reasoning", reasoning!);
            }

            if (delta["tool_calls"] is JsonArray toolDeltas)
            {
                foreach (var item in toolDeltas)
                {
                    if (item is not JsonObject tool)
                    {
                        continue;
                    }

                    var index = 0;
                    if (tool["index"] is JsonValue indexValue && indexValue.TryGetValue<int>(out var parsedIndex))
                    {
                        index = parsedIndex;
                    }
                    if (!toolCalls.TryGetValue(index, out var accumulator))
                    {
                        accumulator = new ToolAccumulator();
                        toolCalls[index] = accumulator;
                    }

                    var id = AsString(tool["id"]);
                    if (!string.IsNullOrEmpty(id))
                    {
                        accumulator.Id = id;
                    }

                    if (tool["function"] is JsonObject function)
                    {
                        var name = AsString(function["name"]);
                        if (!string.IsNullOrEmpty(name))
                        {
                            accumulator.Name = (accumulator.Name ?? string.Empty) + name;
                        }

                        var arguments = AsString(function["arguments"]);
                        if (!string.IsNullOrEmpty(arguments))
                        {
                            accumulator.Arguments.Append(arguments);
                        }
                    }
                }
            }
        }

        foreach (var pair in toolCalls)
        {
            yield return new ToolCallStreamPart(
                pair.Value.Id ?? ("call_" + pair.Key),
                pair.Value.Name ?? string.Empty,
                pair.Value.Arguments.ToString());
        }

        yield return new FinishStreamPart(FinishReasons.Parse(finishRaw), usage ?? LanguageModelUsage.Empty, finishRaw);
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
            ["stream"] = stream,
        };

        if (stream)
        {
            body["stream_options"] = new JsonObject { ["include_usage"] = true };
        }

        AddSampling(body, options);
        if (options.Tools is { Count: > 0 })
        {
            var tools = new JsonArray();
            foreach (var tool in options.Tools)
            {
                tools.Add(new JsonObject
                {
                    ["type"] = "function",
                    ["function"] = new JsonObject
                    {
                        ["name"] = tool.Name,
                        ["description"] = tool.Description,
                        ["parameters"] = JsonNode.Parse(tool.InputSchema.GetRawText()),
                    },
                });
            }

            body["tools"] = tools;
            body["tool_choice"] = MapToolChoice(options.ToolChoice);
        }

        if (options.JsonSchema is { } schema)
        {
            body["response_format"] = new JsonObject
            {
                ["type"] = "json_schema",
                ["json_schema"] = new JsonObject
                {
                    ["name"] = options.JsonSchemaName ?? "response",
                    ["schema"] = JsonNode.Parse(schema.GetRawText()),
                },
            };
        }

        return body;
    }

    private static void AddSampling(JsonObject body, LanguageModelCallOptions options)
    {
        if (options.Temperature is { } temperature)
        {
            body["temperature"] = temperature;
        }

        if (options.TopP is { } topP)
        {
            body["top_p"] = topP;
        }

        if (options.MaxOutputTokens is { } maxTokens)
        {
            body["max_tokens"] = maxTokens;
        }

        if (options.PresencePenalty is { } presence)
        {
            body["presence_penalty"] = presence;
        }

        if (options.FrequencyPenalty is { } frequency)
        {
            body["frequency_penalty"] = frequency;
        }

        if (options.Seed is { } seed)
        {
            body["seed"] = seed;
        }

        if (options.StopSequences is { Count: > 0 })
        {
            var stop = new JsonArray();
            foreach (var sequence in options.StopSequences)
            {
                stop.Add(sequence);
            }

            body["stop"] = stop;
        }
    }

    private static JsonNode MapToolChoice(ToolChoice? toolChoice)
    {
        if (toolChoice is null || toolChoice.Type == "auto")
        {
            return "auto";
        }

        if (toolChoice.Type == "none")
        {
            return "none";
        }

        if (toolChoice.Type == "required")
        {
            return "required";
        }

        if (toolChoice is ToolChoice.NamedChoice named)
        {
            return new JsonObject
            {
                ["type"] = "function",
                ["function"] = new JsonObject { ["name"] = named.ToolName },
            };
        }

        return "auto";
    }

    private static JsonObject MapMessage(ModelMessage message)
    {
        switch (message)
        {
            case SystemModelMessage system:
                return new JsonObject { ["role"] = "system", ["content"] = system.Content };
            case UserModelMessage user:
                return new JsonObject { ["role"] = "user", ["content"] = MapUserContent(user) };
            case AssistantModelMessage assistant:
                var assistantJson = new JsonObject
                {
                    ["role"] = "assistant",
                    ["content"] = assistant.Text,
                };
                if (assistant.ToolCalls.Count > 0)
                {
                    var calls = new JsonArray();
                    foreach (var call in assistant.ToolCalls)
                    {
                        calls.Add(new JsonObject
                        {
                            ["id"] = call.ToolCallId,
                            ["type"] = "function",
                            ["function"] = new JsonObject
                            {
                                ["name"] = call.ToolName,
                                ["arguments"] = call.ArgumentsJson,
                            },
                        });
                    }

                    assistantJson["tool_calls"] = calls;
                }

                return assistantJson;
            case ToolModelMessage tool:
                return new JsonObject
                {
                    ["role"] = "tool",
                    ["tool_call_id"] = tool.ToolCallId,
                    ["content"] = tool.OutputJson,
                };
            default:
                throw new AiSdkException("Unsupported message role '" + message.Role + "'.");
        }
    }

    private static JsonNode MapUserContent(UserModelMessage user)
    {
        var onlyText = true;
        foreach (var part in user.Content)
        {
            if (part is not TextContentPart)
            {
                onlyText = false;
                break;
            }
        }

        if (onlyText)
        {
            var builder = new StringBuilder();
            foreach (var part in user.Content)
            {
                if (part is TextContentPart text)
                {
                    builder.Append(text.Text);
                }
            }

            return builder.ToString();
        }

        var parts = new JsonArray();
        foreach (var part in user.Content)
        {
            if (part is TextContentPart textPart)
            {
                parts.Add(new JsonObject { ["type"] = "text", ["text"] = textPart.Text });
            }
            else if (part is FileContentPart file)
            {
                var url = file.Url;
                if (url is null && file.Data != null)
                {
                    url = "data:" + file.MediaType + ";base64," + Convert.ToBase64String(file.Data);
                }

                parts.Add(new JsonObject
                {
                    ["type"] = "image_url",
                    ["image_url"] = new JsonObject { ["url"] = url },
                });
            }
        }

        return parts;
    }

    private static LanguageModelGenerateResult ParseGenerate(System.Text.Json.JsonElement root)
    {
        var choice = root.GetProperty("choices")[0];
        var message = choice.GetProperty("message");
        var content = new List<GeneratedContent>();
        if (message.TryGetProperty("content", out var text) && text.ValueKind == System.Text.Json.JsonValueKind.String)
        {
            var value = text.GetString();
            if (!string.IsNullOrEmpty(value))
            {
                content.Add(new GeneratedText(value!));
            }
        }

        if (message.TryGetProperty("reasoning_content", out var reasoning) && reasoning.ValueKind == System.Text.Json.JsonValueKind.String)
        {
            content.Add(new GeneratedReasoning(reasoning.GetString() ?? string.Empty));
        }

        if (message.TryGetProperty("tool_calls", out var toolCalls) && toolCalls.ValueKind == System.Text.Json.JsonValueKind.Array)
        {
            foreach (var call in toolCalls.EnumerateArray())
            {
                var function = call.GetProperty("function");
                content.Add(new GeneratedToolCall(
                    call.GetProperty("id").GetString() ?? JsonValues.GenerateId("call_"),
                    function.GetProperty("name").GetString() ?? string.Empty,
                    function.GetProperty("arguments").GetString() ?? "{}"));
            }
        }

        var raw = choice.TryGetProperty("finish_reason", out var finish) ? finish.GetString() : null;
        var usage = LanguageModelUsage.Empty;
        if (root.TryGetProperty("usage", out var usageElement) && usageElement.ValueKind == System.Text.Json.JsonValueKind.Object)
        {
            usage = new LanguageModelUsage(
                usageElement.TryGetProperty("prompt_tokens", out var input) ? input.GetInt32() : null,
                usageElement.TryGetProperty("completion_tokens", out var output) ? output.GetInt32() : null,
                usageElement.TryGetProperty("total_tokens", out var total) ? total.GetInt32() : null);
        }

        var id = root.TryGetProperty("id", out var responseId) ? responseId.GetString() : null;
        return new LanguageModelGenerateResult(content, FinishReasons.Parse(raw), usage, raw, responseId: id);
    }

    private static LanguageModelUsage ReadUsage(JsonObject usage)
    {
        return new LanguageModelUsage(
            ReadInt(usage, "prompt_tokens"),
            ReadInt(usage, "completion_tokens"),
            ReadInt(usage, "total_tokens"));
    }

    private static string? AsString(JsonNode? node)
    {
        if (node is JsonValue value && value.TryGetValue<string>(out var text))
        {
            return text;
        }

        return null;
    }

    private static int? ReadInt(JsonObject obj, string name)
    {
        if (obj[name] is JsonValue value && value.TryGetValue<int>(out var number))
        {
            return number;
        }

        return null;
    }

    private sealed class ToolAccumulator
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public StringBuilder Arguments { get; } = new();
    }
}
