// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.AmazonBedrock;

/// <summary>A prepared Converse request.</summary>
public sealed class AmazonBedrockPreparedRequest
{
    /// <summary>Creates a prepared request.</summary>
    public AmazonBedrockPreparedRequest(JsonObject body, IReadOnlyList<AmazonBedrockWarning> warnings, bool usesJsonResponseTool, bool usesJsonInstruction)
    {
        Body = body ?? new JsonObject();
        Warnings = warnings ?? Array.Empty<AmazonBedrockWarning>();
        UsesJsonResponseTool = usesJsonResponseTool;
        UsesJsonInstruction = usesJsonInstruction;
    }

    /// <summary>Upstream Converse command, including service tier and additional model fields.</summary>
    public JsonObject Body { get; }

    /// <summary>Warnings produced while building the command.</summary>
    public IReadOnlyList<AmazonBedrockWarning> Warnings { get; }

    /// <summary>Whether a JSON response tool was added.</summary>
    public bool UsesJsonResponseTool { get; }

    /// <summary>Whether a JSON instruction was injected into the system prompt.</summary>
    public bool UsesJsonInstruction { get; }

    /// <summary>
    /// HTTP body. <c>serviceTier</c> and <c>additionalModelRequestFields</c> stay on <see cref="Body"/>
    /// and are omitted here so existing request-metadata calls do not leak those provider-option names.
    /// </summary>
    public JsonObject CreateTransportBody()
    {
        var copy = (JsonObject)Body.DeepClone();
        copy.Remove("serviceTier");
        copy.Remove("additionalModelRequestFields");
        copy.Remove("reasoningConfig");
        return copy;
    }
}

/// <summary>Builds an Amazon Bedrock Converse request body.</summary>
public static class AmazonBedrockConverseRequest
{
    private const string JsonInstructionSuffix = "You MUST answer with only a JSON object that matches the JSON schema above. Do not wrap it in markdown fences or include any other text.";

    /// <summary>Builds the Converse command for <paramref name="modelId"/>.</summary>
    public static AmazonBedrockPreparedRequest Prepare(string modelId, LanguageModelCallOptions options, string? modelFamily = null)
    {
        options = options ?? new LanguageModelCallOptions();
        var warnings = new List<AmazonBedrockWarning>();
        var settings = AmazonBedrockChatOptions.Parse(options.ProviderOptions);
        var anthropic = ReadAnthropicOptions(options.ProviderOptions);
        var capabilities = AmazonBedrockModelCapabilitiesResolver.Resolve(modelId);
        WarnIfSet(warnings, options.FrequencyPenalty != null, "frequencyPenalty", null);
        WarnIfSet(warnings, options.PresencePenalty != null, "presencePenalty", null);
        WarnIfSet(warnings, options.Seed != null, "seed", null);

        double? temperature = options.Temperature;
        double? topP = options.TopP;
        int? topK = options.TopK;
        if (capabilities.RejectsSamplingParameters)
        {
            if (temperature != null)
            {
                warnings.Add(new AmazonBedrockWarning("unsupported", "temperature", "temperature is not supported by " + modelId + " and will be ignored"));
                temperature = null;
            }

            if (topK != null)
            {
                warnings.Add(new AmazonBedrockWarning("unsupported", "topK", "topK is not supported by " + modelId + " and will be ignored"));
                topK = null;
            }

            if (topP != null)
            {
                warnings.Add(new AmazonBedrockWarning("unsupported", "topP", "topP is not supported by " + modelId + " and will be ignored"));
                topP = null;
            }
        }

        var openAi = TryOpenAiModel(modelId);
        var shouldNormalizeTemperature = !openAi.IsOpenAi || openAi.IsGptOss;
        if (shouldNormalizeTemperature && temperature != null && temperature.Value > 1)
        {
            warnings.Add(new AmazonBedrockWarning("unsupported", "temperature", FormatNumber(temperature.Value) + " exceeds bedrock maximum of 1.0. clamped to 1.0"));
            temperature = 1;
        }
        else if (shouldNormalizeTemperature && temperature != null && temperature.Value < 0)
        {
            warnings.Add(new AmazonBedrockWarning("unsupported", "temperature", FormatNumber(temperature.Value) + " is below bedrock minimum of 0. clamped to 0"));
            temperature = 0;
        }

        var isAnthropic = AmazonBedrockModelSupport.IsAnthropicModel(modelId, modelFamily, settings.Reasoning?.BudgetTokens);
        ResolveReasoning(options.Reasoning, settings, warnings, isAnthropic, modelId, capabilities);
        var additional = CloneObject(settings.AdditionalModelRequestFields);
        var mode = settings.StructuredOutputMode ?? anthropic.StructuredOutputMode ?? "auto";
        if (mode == "jsonTool")
        {
            RemoveOutputFormat(additional);
        }

        var thinkingEnabled = settings.Reasoning?.Type == "enabled" || settings.Reasoning?.Type == "adaptive";
        var nativeSupported = AmazonBedrockModelSupport.SupportsNativeStructuredOutput(modelId)
            && (capabilities.SupportsStructuredOutput || thinkingEnabled || string.Equals(modelFamily, "anthropic", StringComparison.Ordinal));
        var hasSchema = options.JsonSchema != null && options.JsonSchema.Value.ValueKind != JsonValueKind.Undefined && options.JsonSchema.Value.ValueKind != JsonValueKind.Null;
        var useNative = isAnthropic && hasSchema && (mode == "outputFormat" || (mode == "auto" && nativeSupported));
        var useInstruction = !useNative && isAnthropic && hasSchema && (capabilities.RejectsForcedToolUse || (mode != "jsonTool" && !AmazonBedrockModelSupport.SupportsStrictTools(modelId) && options.Tools != null && options.Tools.Count > 0));
        var useJsonTool = hasSchema && !useNative && !useInstruction;

        var toolInputs = new List<AmazonBedrockToolInput>();
        if (options.Tools != null)
        {
            foreach (var tool in options.Tools)
            {
                toolInputs.Add(AmazonBedrockToolInput.Function(tool.Name, tool.Description, JsonNode.Parse(tool.InputSchema.GetRawText()), tool.Strict));
            }
        }

        if (useJsonTool)
        {
            toolInputs.Add(AmazonBedrockToolInput.Function("json", "Respond with a JSON object.", JsonNode.Parse(options.JsonSchema!.Value.GetRawText()), null));
        }

        var preparedTools = AmazonBedrockPrepareTools.Prepare(
            toolInputs,
            useJsonTool ? ToolChoice.Required : options.ToolChoice,
            modelId,
            modelFamily,
            settings.Reasoning?.BudgetTokens,
            anthropic.DisableParallelToolUse);
        warnings.AddRange(preparedTools.Warnings);
        if (preparedTools.AdditionalTools != null)
        {
            MergeInto(additional, preparedTools.AdditionalTools);
        }

        if ((settings.AnthropicBeta != null && settings.AnthropicBeta.Count > 0) || preparedTools.Betas.Count > 0)
        {
            var betas = new JsonArray();
            if (settings.AnthropicBeta != null)
            {
                foreach (var beta in settings.AnthropicBeta)
                {
                    betas.Add(beta);
                }
            }

            foreach (var beta in preparedTools.Betas)
            {
                betas.Add(beta);
            }

            additional["anthropic_beta"] = betas;
        }

        var thinkingType = settings.Reasoning?.Type;
        int? thinkingBudget = thinkingType == "enabled" ? settings.Reasoning?.BudgetTokens : null;
        var thinkingDisplay = thinkingType == "adaptive" ? settings.Reasoning?.Display : null;
        var anthropicThinking = isAnthropic && thinkingEnabled;
        int? maxTokens = options.MaxOutputTokens;
        if (anthropicThinking)
        {
            if (thinkingBudget != null)
            {
                maxTokens = maxTokens != null ? maxTokens.Value + thinkingBudget.Value : thinkingBudget.Value + 4096;
                var thinking = new JsonObject
                {
                    ["type"] = "enabled",
                    ["budget_tokens"] = thinkingBudget.Value,
                };
                additional["thinking"] = thinking;
            }
            else if (thinkingType == "adaptive")
            {
                var thinking = new JsonObject { ["type"] = "adaptive" };
                if (thinkingDisplay != null)
                {
                    thinking["display"] = thinkingDisplay;
                }

                additional["thinking"] = thinking;
            }
        }
        else if (!isAnthropic)
        {
            if (settings.Reasoning?.BudgetTokens != null)
            {
                warnings.Add(new AmazonBedrockWarning("unsupported", "budgetTokens", "budgetTokens applies only to Anthropic models on Bedrock and will be ignored for this model."));
            }

            if (thinkingType == "adaptive")
            {
                warnings.Add(new AmazonBedrockWarning("unsupported", "adaptive thinking", "adaptive thinking type applies only to Anthropic models on Bedrock."));
            }
        }

        var effort = settings.Reasoning?.MaxReasoningEffort;
        if (effort != null)
        {
            if (isAnthropic)
            {
                var outputConfig = additional["output_config"] as JsonObject ?? new JsonObject();
                outputConfig["effort"] = effort;
                additional["output_config"] = outputConfig;
            }
            else if (openAi.IsOpenAi && openAi.IsGptOss)
            {
                additional["reasoning_effort"] = effort;
            }
            else if (openAi.IsOpenAi)
            {
                var reasoning = additional["reasoning"] as JsonObject ?? new JsonObject();
                reasoning["effort"] = effort;
                additional["reasoning"] = reasoning;
            }
            else
            {
                var reasoningConfig = new JsonObject();
                if (thinkingType != null && thinkingType != "adaptive")
                {
                    reasoningConfig["type"] = thinkingType;
                }

                if (thinkingBudget != null)
                {
                    reasoningConfig["budgetTokens"] = thinkingBudget.Value;
                }

                reasoningConfig["maxReasoningEffort"] = effort;
                additional["reasoningConfig"] = reasoningConfig;
            }
        }

        if (useNative)
        {
            var outputConfig = additional["output_config"] as JsonObject ?? new JsonObject();
            outputConfig["format"] = new JsonObject
            {
                ["type"] = "json_schema",
                ["schema"] = JsonNode.Parse(options.JsonSchema!.Value.GetRawText()),
            };
            additional["output_config"] = outputConfig;
        }

        if (anthropicThinking && temperature != null)
        {
            temperature = null;
            warnings.Add(new AmazonBedrockWarning("unsupported", "temperature", "temperature is not supported when thinking is enabled"));
        }

        if (anthropicThinking && topP != null)
        {
            topP = null;
            warnings.Add(new AmazonBedrockWarning("unsupported", "topP", "topP is not supported when thinking is enabled"));
        }

        if (anthropicThinking && topK != null)
        {
            topK = null;
            warnings.Add(new AmazonBedrockWarning("unsupported", "topK", "topK is not supported when thinking is enabled"));
        }

        IReadOnlyList<string>? stopSequences = options.StopSequences;
        if (openAi.IsOpenAi)
        {
            if (!openAi.IsGptOss)
            {
                if (temperature != null)
                {
                    temperature = null;
                    warnings.Add(new AmazonBedrockWarning("unsupported", "temperature", "temperature is not supported by this OpenAI model on the Converse API"));
                }

                if (topP != null)
                {
                    topP = null;
                    warnings.Add(new AmazonBedrockWarning("unsupported", "topP", "topP is not supported by this OpenAI model on the Converse API"));
                }
            }

            if (stopSequences != null)
            {
                stopSequences = null;
                warnings.Add(new AmazonBedrockWarning("unsupported", "stopSequences", "stopSequences is not supported by this OpenAI model on the Converse API"));
            }
        }

        var prompt = new List<AmazonBedrockPromptMessage>(AmazonBedrockMessages.FromModelMessages(options.Prompt));
        var hasTools = preparedTools.ToolConfig["tools"] is JsonArray toolArray && toolArray.Count > 0 || preparedTools.AdditionalTools != null;
        if (!hasTools)
        {
            var removed = FilterToolContent(prompt);
            if (removed)
            {
                warnings.Add(new AmazonBedrockWarning("unsupported", "toolContent", "Tool calls and results removed from conversation because Bedrock does not support tool content without active tools."));
            }
        }

        if (useInstruction)
        {
            InjectJsonInstruction(prompt, options.JsonSchema!.Value);
        }

        var converted = AmazonBedrockMessages.Convert(prompt, AmazonBedrockToolCallId.IsMistralModel(modelId));
        var body = new JsonObject
        {
            ["system"] = converted.System,
            ["messages"] = converted.Messages,
        };
        if (additional.Count > 0)
        {
            body["additionalModelRequestFields"] = additional;
        }

        if (isAnthropic)
        {
            body["additionalModelResponseFieldPaths"] = new JsonArray { "/delta/stop_sequence" };
        }

        var inference = new JsonObject();
        if (maxTokens != null)
        {
            inference["maxTokens"] = maxTokens.Value;
        }

        if (temperature != null)
        {
            inference["temperature"] = JsonNode.Parse(FormatNumber(temperature.Value));
        }

        if (topP != null)
        {
            inference["topP"] = JsonNode.Parse(FormatNumber(topP.Value));
        }

        if (topK != null)
        {
            inference["topK"] = topK.Value;
        }

        if (stopSequences != null)
        {
            var stops = new JsonArray();
            foreach (var stop in stopSequences)
            {
                stops.Add(stop);
            }

            inference["stopSequences"] = stops;
        }

        if (inference.Count > 0)
        {
            body["inferenceConfig"] = inference;
        }

        if (settings.ServiceTier != null)
        {
            body["serviceTier"] = new JsonObject { ["type"] = settings.ServiceTier };
        }

        if (settings.Passthrough != null)
        {
            foreach (var pair in settings.Passthrough)
            {
                body[pair.Key] = pair.Value == null ? null : pair.Value.DeepClone();
            }
        }

        if (settings.RequestMetadata != null)
        {
            body["requestMetadata"] = settings.RequestMetadata.DeepClone();
        }

        if (preparedTools.ToolConfig["tools"] is JsonArray sent && sent.Count > 0)
        {
            body["toolConfig"] = preparedTools.ToolConfig.DeepClone();
        }

        return new AmazonBedrockPreparedRequest(body, warnings, useJsonTool, useInstruction);
    }

    private static void ResolveReasoning(string? reasoning, AmazonBedrockChatSettings settings, List<AmazonBedrockWarning> warnings, bool isAnthropic, string modelId, AmazonBedrockModelCapabilities capabilities)
    {
        if (reasoning == null || reasoning == "provider-default")
        {
            return;
        }

        var user = settings.Reasoning;
        if (isAnthropic)
        {
            if (reasoning == "none")
            {
                settings.Reasoning = MergeReasoning(new AmazonBedrockReasoningSettings { Type = "disabled" }, user);
            }
            else if (capabilities.SupportsAdaptiveThinking)
            {
                var effort = MapEffort(reasoning, false, warnings);
                settings.Reasoning = MergeReasoning(new AmazonBedrockReasoningSettings { Type = "adaptive", MaxReasoningEffort = effort }, user);
            }
            else
            {
                var budget = MapBudget(reasoning, capabilities.MaxOutputTokens, warnings);
                if (budget != null)
                {
                    settings.Reasoning = MergeReasoning(new AmazonBedrockReasoningSettings { Type = "enabled", BudgetTokens = budget }, user);
                }
            }
        }
        else if (reasoning != "none")
        {
            var effort = MapEffort(reasoning, false, warnings);
            settings.Reasoning = MergeReasoning(new AmazonBedrockReasoningSettings { MaxReasoningEffort = effort }, user);
        }

        if (settings.Reasoning?.Type == "disabled")
        {
            settings.Reasoning.MaxReasoningEffort = null;
            settings.Reasoning.BudgetTokens = null;
        }

        _ = modelId;
    }

    private static AmazonBedrockReasoningSettings MergeReasoning(AmazonBedrockReasoningSettings derived, AmazonBedrockReasoningSettings? user)
    {
        if (user == null)
        {
            return derived;
        }

        if (user.Type != null)
        {
            derived.Type = user.Type;
        }

        if (user.BudgetTokens != null)
        {
            derived.BudgetTokens = user.BudgetTokens;
        }

        if (user.MaxReasoningEffort != null)
        {
            derived.MaxReasoningEffort = user.MaxReasoningEffort;
        }

        if (user.Display != null)
        {
            derived.Display = user.Display;
        }

        return derived;
    }

    private static string? MapEffort(string reasoning, bool supportsXhigh, List<AmazonBedrockWarning> warnings)
    {
        string? mapped;
        switch (reasoning)
        {
            case "minimal":
                mapped = "low";
                break;
            case "low":
                mapped = "low";
                break;
            case "medium":
                mapped = "medium";
                break;
            case "high":
                mapped = "high";
                break;
            case "xhigh":
                mapped = supportsXhigh ? "xhigh" : "max";
                break;
            default:
                warnings.Add(new AmazonBedrockWarning("unsupported", "reasoning", "reasoning \"" + reasoning + "\" is not supported by this model."));
                return null;
        }

        if (mapped != reasoning)
        {
            warnings.Add(new AmazonBedrockWarning("compatibility", "reasoning", "reasoning \"" + reasoning + "\" is not directly supported by this model. mapped to effort \"" + mapped + "\"."));
        }

        return mapped;
    }

    private static int? MapBudget(string reasoning, int maxOutputTokens, List<AmazonBedrockWarning> warnings)
    {
        double? percent;
        switch (reasoning)
        {
            case "minimal":
                percent = 0.02;
                break;
            case "low":
                percent = 0.1;
                break;
            case "medium":
                percent = 0.3;
                break;
            case "high":
                percent = 0.6;
                break;
            case "xhigh":
                percent = 0.9;
                break;
            default:
                warnings.Add(new AmazonBedrockWarning("unsupported", "reasoning", "reasoning \"" + reasoning + "\" is not supported by this model."));
                return null;
        }

        var budget = (int)Math.Round(maxOutputTokens * percent.Value, MidpointRounding.AwayFromZero);
        if (budget < 1024)
        {
            budget = 1024;
        }

        if (budget > maxOutputTokens)
        {
            budget = maxOutputTokens;
        }

        return budget;
    }

    private static void InjectJsonInstruction(List<AmazonBedrockPromptMessage> prompt, JsonElement schema)
    {
        var schemaJson = schema.GetRawText();
        var instruction = "JSON schema:\n" + schemaJson + "\n" + JsonInstructionSuffix;
        if (prompt.Count > 0 && prompt[0].Role == "system")
        {
            var existing = prompt[0].SystemText ?? string.Empty;
            prompt[0].SystemText = existing.Length > 0 ? existing + "\n\n" + instruction : instruction;
            return;
        }

        prompt.Insert(0, AmazonBedrockPromptMessage.System(instruction));
    }

    private static bool FilterToolContent(List<AmazonBedrockPromptMessage> prompt)
    {
        var removed = false;
        for (var i = prompt.Count - 1; i >= 0; i--)
        {
            var message = prompt[i];
            if (message.Role == "system")
            {
                continue;
            }

            var kept = new List<AmazonBedrockPromptPart>();
            foreach (var part in message.Parts)
            {
                if (part.Type == "tool-call" || part.Type == "tool-result")
                {
                    removed = true;
                    continue;
                }

                kept.Add(part);
            }

            if (kept.Count == 0)
            {
                prompt.RemoveAt(i);
            }
            else if (kept.Count != message.Parts.Count)
            {
                message.Parts = kept;
            }
        }

        return removed;
    }

    private static void RemoveOutputFormat(JsonObject additional)
    {
        if (additional["output_config"] is not JsonObject outputConfig)
        {
            return;
        }

        outputConfig.Remove("format");
        if (outputConfig.Count == 0)
        {
            additional.Remove("output_config");
        }
    }

    private static void MergeInto(JsonObject target, JsonObject extra)
    {
        foreach (var pair in extra)
        {
            target[pair.Key] = pair.Value == null ? null : pair.Value.DeepClone();
        }
    }

    private static JsonObject CloneObject(JsonElement? element)
    {
        if (element == null || element.Value.ValueKind != JsonValueKind.Object)
        {
            return new JsonObject();
        }

        return (JsonObject)JsonNode.Parse(element.Value.GetRawText())!;
    }

    private static AnthropicFlags ReadAnthropicOptions(IReadOnlyDictionary<string, JsonElement>? providerOptions)
    {
        var flags = new AnthropicFlags();
        if (providerOptions == null || !providerOptions.TryGetValue("anthropic", out var anthropic) || anthropic.ValueKind != JsonValueKind.Object)
        {
            return flags;
        }

        if (anthropic.TryGetProperty("disableParallelToolUse", out var disable) && (disable.ValueKind == JsonValueKind.True || disable.ValueKind == JsonValueKind.False))
        {
            flags.DisableParallelToolUse = disable.GetBoolean();
        }

        if (anthropic.TryGetProperty("structuredOutputMode", out var mode) && mode.ValueKind == JsonValueKind.String && AmazonBedrockChatOptions.IsStructuredOutputMode(mode.GetString()))
        {
            flags.StructuredOutputMode = mode.GetString();
        }

        return flags;
    }

    private static OpenAiModel TryOpenAiModel(string modelId)
    {
        var result = new OpenAiModel();
        if (string.IsNullOrEmpty(modelId))
        {
            return result;
        }

        var rest = modelId;
        var dot = modelId.IndexOf('.');
        if (dot >= 0)
        {
            var after = modelId.Substring(dot + 1);
            if (after.IndexOf("openai.", StringComparison.Ordinal) == 0)
            {
                rest = after;
            }
        }

        if (rest.IndexOf("openai.", StringComparison.Ordinal) != 0)
        {
            return result;
        }

        result.IsOpenAi = true;
        result.IsGptOss = rest.IndexOf("openai.gpt-oss-", StringComparison.Ordinal) == 0;
        return result;
    }

    private static void WarnIfSet(List<AmazonBedrockWarning> warnings, bool set, string feature, string? details)
    {
        if (set)
        {
            warnings.Add(new AmazonBedrockWarning("unsupported", feature, details));
        }
    }

    private static string FormatNumber(double value)
    {
        return value.ToString("0.################", CultureInfo.InvariantCulture);
    }

    private struct AnthropicFlags
    {
        public bool DisableParallelToolUse;

        public string? StructuredOutputMode;
    }

    private struct OpenAiModel
    {
        public bool IsOpenAi;

        public bool IsGptOss;
    }
}
