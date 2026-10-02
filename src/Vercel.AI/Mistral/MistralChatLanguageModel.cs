// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Mistral;

/// <summary>Tools prepared for a Mistral chat request.</summary>
public sealed class MistralPreparedTools
{
    internal MistralPreparedTools(JsonArray? tools, string? toolChoice, IReadOnlyList<CallWarning> warnings)
    {
        Tools = tools;
        ToolChoice = toolChoice;
        Warnings = warnings ?? Array.Empty<CallWarning>();
    }

    /// <summary>Wire tools. Null when the caller passed no tools.</summary>
    public JsonArray? Tools { get; }

    /// <summary>Wire tool choice. <c>required</c> and a named tool become <c>any</c>.</summary>
    public string? ToolChoice { get; }

    /// <summary>Warnings produced while preparing tools.</summary>
    public IReadOnlyList<CallWarning> Warnings { get; }
}

/// <summary>Maps tools onto Mistral chat tool definitions.</summary>
public static class MistralTools
{
    /// <summary>Prepares function tools. Named tool choice keeps only that function and sends <c>any</c>.</summary>
    public static MistralPreparedTools Prepare(IReadOnlyList<LanguageModelTool>? tools, ToolChoice? toolChoice)
    {
        if (tools == null || tools.Count == 0)
        {
            return new MistralPreparedTools(null, null, Array.Empty<CallWarning>());
        }

        var prepared = new JsonArray();
        foreach (var tool in tools)
        {
            if (toolChoice is ToolChoice.NamedChoice named && !string.Equals(tool.Name, named.ToolName, StringComparison.Ordinal))
            {
                continue;
            }

            var function = new JsonObject
            {
                ["name"] = tool.Name,
                ["parameters"] = tool.InputSchema.ValueKind == JsonValueKind.Undefined
                    ? new JsonObject()
                    : JsonNode.Parse(tool.InputSchema.GetRawText()) ?? new JsonObject(),
            };
            if (tool.Description != null)
            {
                function["description"] = tool.Description;
            }

            if (tool.Strict != null)
            {
                function["strict"] = tool.Strict.Value;
            }

            prepared.Add(new JsonObject
            {
                ["type"] = "function",
                ["function"] = function,
            });
        }

        string? choice = null;
        if (toolChoice != null)
        {
            choice = toolChoice is ToolChoice.NamedChoice || toolChoice.Type == "required" ? "any" : toolChoice.Type;
        }

        return new MistralPreparedTools(prepared, choice, Array.Empty<CallWarning>());
    }
}

/// <summary>
/// Mistral chat model. Supporting models send <c>reasoning_effort</c>.
/// <c>required</c> tool choice is sent as <c>any</c>, and <c>seed</c> is sent as <c>random_seed</c>.
/// </summary>
public sealed class MistralChatLanguageModel : ILanguageModel
{
    private static readonly HashSet<string> ReasoningModels = new HashSet<string>(StringComparer.Ordinal)
    {
        "glm-5-2",
        "labs-leanstral-1-5",
        "labs-leanstral-1-5-1",
        "magistral-medium-latest",
        "magistral-small-latest",
        "mistral-medium",
        "mistral-medium-2604",
        "mistral-medium-3",
        "mistral-medium-3-5",
        "mistral-medium-3.5",
        "mistral-medium-latest",
        "mistral-small-2603",
        "mistral-small-latest",
        "mistral-vibe-cli-fast",
        "mistral-vibe-cli-latest",
        "mistral-vibe-cli-with-tools",
        "zai-glm-5-2",
    };

    private readonly OpenAICompatibleProvider _provider;

    /// <summary>Creates a Mistral chat model.</summary>
    public MistralChatLanguageModel(OpenAICompatibleProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <summary>Provider id <c>mistral.chat</c>.</summary>
    public string Provider => "mistral.chat";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>True when <paramref name="modelId"/> accepts <c>reasoning_effort</c>.</summary>
    public static bool SupportsReasoningEffort(string modelId)
    {
        return modelId != null && ReasoningModels.Contains(modelId);
    }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        options ??= new LanguageModelCallOptions();
        var built = Build(options);
        var http = await _provider.Http.SendJsonStringAsync(
            HttpMethod.Post,
            _provider.ChatUri(ModelId),
            built.Body.ToJsonString(),
            _provider.CreateHeaders(),
            cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(http.Body) ? "{}" : http.Body);
        var content = new List<GeneratedContent>();
        string? finish = null;
        if (document.RootElement.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0)
        {
            var choice = choices[0];
            finish = JsonValues.GetString(choice, "finish_reason");
            if (choice.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.Object)
            {
                ReadMessage(message, content);
            }
        }

        return new LanguageModelGenerateResult(content, FinishReasons.Parse(finish), LanguageModelUsage.Empty, finish, built.Warnings, null, null, http.Body);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
        LanguageModelCallOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var result = await DoGenerateAsync(options, cancellationToken).ConfigureAwait(false);
        yield return new StreamStartStreamPart(result.Warnings);
        yield return new FinishStreamPart(result.FinishReason, result.Usage, result.RawFinishReason);
    }

    private static void ReadMessage(JsonElement message, List<GeneratedContent> content)
    {
        if (message.TryGetProperty("content", out var body))
        {
            if (body.ValueKind == JsonValueKind.String)
            {
                var text = body.GetString();
                if (!string.IsNullOrEmpty(text))
                {
                    content.Add(new GeneratedText(text!));
                }
            }
            else if (body.ValueKind == JsonValueKind.Array)
            {
                foreach (var part in body.EnumerateArray())
                {
                    var type = JsonValues.GetString(part, "type");
                    var text = JsonValues.GetString(part, "text");
                    if (string.IsNullOrEmpty(text))
                    {
                        continue;
                    }

                    if (string.Equals(type, "thinking", StringComparison.Ordinal))
                    {
                        content.Add(new GeneratedReasoning(text!));
                    }
                    else
                    {
                        content.Add(new GeneratedText(text!));
                    }
                }
            }
        }
    }

    private (JsonObject Body, List<CallWarning> Warnings) Build(LanguageModelCallOptions options)
    {
        var warnings = new List<CallWarning>();
        if (options.TopK != null)
        {
            warnings.Add(new CallWarning("unsupported", "topK"));
        }

        string? providerEffort = null;
        if (options.ProviderOptions != null
            && options.ProviderOptions.TryGetValue("mistral", out var mistral)
            && mistral.ValueKind == JsonValueKind.Object
            && mistral.TryGetProperty("reasoningEffort", out var effortElement)
            && effortElement.ValueKind == JsonValueKind.String)
        {
            providerEffort = effortElement.GetString();
        }

        string? reasoningEffort = null;
        if (SupportsReasoningEffort(ModelId))
        {
            if (providerEffort != null)
            {
                reasoningEffort = providerEffort;
            }
            else if (IsCustom(options.Reasoning))
            {
                if (string.Equals(options.Reasoning, "none", StringComparison.Ordinal))
                {
                    reasoningEffort = "none";
                }
                else
                {
                    reasoningEffort = "high";
                    if (!string.Equals(options.Reasoning, "high", StringComparison.Ordinal))
                    {
                        warnings.Add(new CallWarning(
                            "compatibility",
                            "reasoning | reasoning \"" + options.Reasoning + "\" is not directly supported by this model. mapped to effort \"high\"."));
                    }
                }
            }
        }
        else if (IsCustom(options.Reasoning))
        {
            warnings.Add(new CallWarning("unsupported", "reasoning | This model does not support reasoning configuration."));
        }

        var prepared = MistralTools.Prepare(options.Tools, options.ToolChoice);
        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["messages"] = Messages(options.Prompt),
        };
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

        if (options.Seed != null)
        {
            body["random_seed"] = options.Seed.Value;
        }

        if (reasoningEffort != null)
        {
            body["reasoning_effort"] = reasoningEffort;
        }

        if (prepared.Tools != null)
        {
            body["tools"] = prepared.Tools;
        }

        if (prepared.ToolChoice != null)
        {
            body["tool_choice"] = prepared.ToolChoice;
        }

        return (body, warnings);
    }

    private static JsonArray Messages(IReadOnlyList<ModelMessage> prompt)
    {
        var messages = new JsonArray();
        if (prompt == null)
        {
            return messages;
        }

        foreach (var message in prompt)
        {
            if (message is UserModelMessage user)
            {
                var text = string.Empty;
                foreach (var part in user.Content)
                {
                    if (part is TextContentPart textPart)
                    {
                        text += textPart.Text;
                    }
                }

                messages.Add(new JsonObject { ["role"] = "user", ["content"] = text });
            }
        }

        return messages;
    }

    private static bool IsCustom(string? reasoning)
    {
        return reasoning != null && !string.Equals(reasoning, "provider-default", StringComparison.Ordinal);
    }
}
