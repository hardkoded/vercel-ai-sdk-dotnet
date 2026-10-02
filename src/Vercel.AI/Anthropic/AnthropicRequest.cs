// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.Provider;

namespace Vercel.AI.Anthropic;

/// <summary>Capabilities used to choose Anthropic defaults.</summary>
public sealed class AnthropicModelCapabilities
{
    /// <summary>Creates model capabilities.</summary>
    public AnthropicModelCapabilities(
        int maxOutputTokens,
        bool supportsStructuredOutput,
        bool supportsAdaptiveThinking,
        bool rejectsSamplingParameters,
        bool supportsXhighEffort,
        bool rejectsThinkingDisabledAboveHighEffort,
        bool rejectsThinkingDisabled,
        bool rejectsForcedToolUse,
        bool isKnownModel)
    {
        MaxOutputTokens = maxOutputTokens;
        SupportsStructuredOutput = supportsStructuredOutput;
        SupportsAdaptiveThinking = supportsAdaptiveThinking;
        RejectsSamplingParameters = rejectsSamplingParameters;
        SupportsXhighEffort = supportsXhighEffort;
        RejectsThinkingDisabledAboveHighEffort = rejectsThinkingDisabledAboveHighEffort;
        RejectsThinkingDisabled = rejectsThinkingDisabled;
        RejectsForcedToolUse = rejectsForcedToolUse;
        IsKnownModel = isKnownModel;
    }

    /// <summary>Model output token ceiling.</summary>
    public int MaxOutputTokens { get; }

    /// <summary>Native <c>output_config.format</c> is supported.</summary>
    public bool SupportsStructuredOutput { get; }

    /// <summary>Adaptive thinking is supported.</summary>
    public bool SupportsAdaptiveThinking { get; }

    /// <summary>Temperature, top-k, and top-p are rejected.</summary>
    public bool RejectsSamplingParameters { get; }

    /// <summary><c>xhigh</c> effort is supported.</summary>
    public bool SupportsXhighEffort { get; }

    /// <summary>Thinking cannot be disabled above high effort.</summary>
    public bool RejectsThinkingDisabledAboveHighEffort { get; }

    /// <summary>Thinking cannot be disabled.</summary>
    public bool RejectsThinkingDisabled { get; }

    /// <summary>Forced tool choice is rejected.</summary>
    public bool RejectsForcedToolUse { get; }

    /// <summary>The model id is a known Claude family.</summary>
    public bool IsKnownModel { get; }

    /// <summary>Resolves capabilities from a model id.</summary>
    public static AnthropicModelCapabilities For(string modelId)
    {
        if (modelId.IndexOf("claude-opus-5-5", StringComparison.Ordinal) >= 0)
        {
            return new AnthropicModelCapabilities(128000, true, true, true, true, true, true, true, true);
        }

        if (modelId.IndexOf("claude-opus-5", StringComparison.Ordinal) >= 0)
        {
            return new AnthropicModelCapabilities(128000, true, true, true, true, true, false, false, true);
        }

        if (modelId.IndexOf("claude-fable-5-1", StringComparison.Ordinal) >= 0)
        {
            return new AnthropicModelCapabilities(128000, true, true, true, true, false, true, true, true);
        }

        if (modelId.IndexOf("claude-fable-5", StringComparison.Ordinal) >= 0)
        {
            return new AnthropicModelCapabilities(128000, true, true, true, true, false, true, false, true);
        }

        if (modelId.IndexOf("claude-opus-4-8", StringComparison.Ordinal) >= 0
            || modelId.IndexOf("claude-opus-4-7", StringComparison.Ordinal) >= 0
            || modelId.IndexOf("claude-sonnet-5", StringComparison.Ordinal) >= 0)
        {
            return new AnthropicModelCapabilities(128000, true, true, true, true, false, false, false, true);
        }

        if (modelId.IndexOf("claude-sonnet-4-6", StringComparison.Ordinal) >= 0
            || modelId.IndexOf("claude-opus-4-6", StringComparison.Ordinal) >= 0)
        {
            return new AnthropicModelCapabilities(128000, true, true, false, false, false, false, false, true);
        }

        if (modelId.IndexOf("claude-sonnet-4-5", StringComparison.Ordinal) >= 0
            || modelId.IndexOf("claude-opus-4-5", StringComparison.Ordinal) >= 0
            || modelId.IndexOf("claude-haiku-4-5", StringComparison.Ordinal) >= 0)
        {
            return new AnthropicModelCapabilities(64000, true, false, false, false, false, false, false, true);
        }

        if (modelId.IndexOf("claude-opus-4-1", StringComparison.Ordinal) >= 0)
        {
            return new AnthropicModelCapabilities(32000, true, false, false, false, false, false, false, true);
        }

        if (Regex.IsMatch(modelId, "claude-sonnet-4(?:-|@)"))
        {
            return new AnthropicModelCapabilities(64000, false, false, false, false, false, false, false, true);
        }

        if (Regex.IsMatch(modelId, "claude-opus-4(?:-|@)"))
        {
            return new AnthropicModelCapabilities(32000, false, false, false, false, false, false, false, true);
        }

        if (modelId.IndexOf("claude-3-haiku", StringComparison.Ordinal) >= 0)
        {
            return new AnthropicModelCapabilities(4096, false, false, false, false, false, false, false, true);
        }

        if (Regex.IsMatch(modelId, "claude-(?:instant(?:-|$)|v?2(?=$|[-.:])|3(?=$|[-.]))"))
        {
            return new AnthropicModelCapabilities(4096, false, false, false, false, false, false, false, false);
        }

        if (modelId.IndexOf("claude-", StringComparison.Ordinal) >= 0)
        {
            return new AnthropicModelCapabilities(128000, true, true, true, true, true, false, false, false);
        }

        return new AnthropicModelCapabilities(4096, false, false, false, false, false, false, false, false);
    }
}

/// <summary>A prepared Messages request.</summary>
public sealed class AnthropicPreparedRequest
{
    /// <summary>Creates a prepared request.</summary>
    public AnthropicPreparedRequest(JsonObject body, IReadOnlyList<AnthropicWarning> warnings, IReadOnlyList<string> betas, bool usesJsonResponseTool)
    {
        Body = body ?? new JsonObject();
        Warnings = warnings ?? Array.Empty<AnthropicWarning>();
        Betas = betas ?? Array.Empty<string>();
        UsesJsonResponseTool = usesJsonResponseTool;
    }

    /// <summary>JSON request body.</summary>
    public JsonObject Body { get; }

    /// <summary>Warnings discovered while preparing the call.</summary>
    public IReadOnlyList<AnthropicWarning> Warnings { get; }

    /// <summary>Beta header values.</summary>
    public IReadOnlyList<string> Betas { get; }

    /// <summary>True when JSON output is forced through the <c>json</c> tool.</summary>
    public bool UsesJsonResponseTool { get; }
}

/// <summary>Builds an Anthropic Messages request body.</summary>
public static class AnthropicRequest
{
    private static readonly JsonSerializerOptions BodyJson = new JsonSerializerOptions
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>Prepares the Messages body for one generate or stream call.</summary>
    public static AnthropicPreparedRequest Prepare(string modelId, LanguageModelCallOptions options, bool stream)
    {
        var warnings = new List<AnthropicWarning>();
        var betas = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        if (options.FrequencyPenalty != null)
        {
            warnings.Add(new AnthropicWarning("unsupported", "frequencyPenalty", null));
        }

        if (options.PresencePenalty != null)
        {
            warnings.Add(new AnthropicWarning("unsupported", "presencePenalty", null));
        }

        if (options.Seed != null)
        {
            warnings.Add(new AnthropicWarning("unsupported", "seed", null));
        }

        double? temperature = options.Temperature;
        if (temperature != null && temperature > 1)
        {
            warnings.Add(new AnthropicWarning("unsupported", "temperature", temperature.Value.ToString(CultureInfo.InvariantCulture) + " exceeds anthropic maximum of 1.0. clamped to 1.0"));
            temperature = 1;
        }
        else if (temperature != null && temperature < 0)
        {
            warnings.Add(new AnthropicWarning("unsupported", "temperature", temperature.Value.ToString(CultureInfo.InvariantCulture) + " is below anthropic minimum of 0. clamped to 0"));
            temperature = 0;
        }

        var capabilities = AnthropicModelCapabilities.For(modelId);
        if (!capabilities.IsKnownModel && options.MaxOutputTokens == null)
        {
            warnings.Add(new AnthropicWarning(
                "compatibility",
                "maxOutputTokens",
                "The model \"" + modelId + "\" is unknown. The max output tokens have been limited to " + capabilities.MaxOutputTokens + ". Set maxOutputTokens explicitly to override this limit."));
        }

        var topP = options.TopP;
        var topK = options.TopK;
        if (capabilities.RejectsSamplingParameters)
        {
            if (temperature != null)
            {
                warnings.Add(new AnthropicWarning("unsupported", "temperature", "temperature is not supported by " + modelId + " and will be ignored"));
                temperature = null;
            }

            if (topK != null)
            {
                warnings.Add(new AnthropicWarning("unsupported", "topK", "topK is not supported by " + modelId + " and will be ignored"));
                topK = null;
            }

            if (topP != null)
            {
                warnings.Add(new AnthropicWarning("unsupported", "topP", "topP is not supported by " + modelId + " and will be ignored"));
                topP = null;
            }
        }

        var anthropic = ReadAnthropicOptions(options);
        var isAnthropicModel = capabilities.IsKnownModel || modelId.IndexOf("claude-", StringComparison.Ordinal) >= 0;
        var mode = AnthropicJson.String(anthropic, "structuredOutputMode") ?? "auto";
        var useStructuredOutput = mode == "outputFormat" || (mode == "auto" && capabilities.SupportsStructuredOutput);
        if (!useStructuredOutput && capabilities.RejectsForcedToolUse && capabilities.SupportsStructuredOutput && options.JsonSchema != null)
        {
            warnings.Add(new AnthropicWarning(
                "unsupported",
                "providerOptions.anthropic.structuredOutputMode",
                "structuredOutputMode 'jsonTool' is not supported by " + modelId + " because it rejects forced tool use. Using 'outputFormat' instead."));
            useStructuredOutput = true;
        }

        var usesJsonTool = options.JsonSchema != null && !useStructuredOutput;
        var validator = new AnthropicCacheControlValidator();
        var promptJson = AnthropicMessages.FromCall(options);
        using (var promptDocument = JsonDocument.Parse(promptJson.ToJsonString()))
        {
            var converted = AnthropicMessages.Convert(promptDocument.RootElement, new AnthropicConvertOptions
            {
                SendReasoning = anthropic.ValueKind != JsonValueKind.Object || !anthropic.TryGetProperty("sendReasoning", out var send) || send.ValueKind != JsonValueKind.False,
                Warnings = warnings,
                CacheControl = validator,
            });
            foreach (var beta in converted.Betas)
            {
                AddBeta(betas, seen, beta);
            }

            ApplyReasoning(modelId, options.Reasoning, capabilities, ref anthropic, warnings);
            var thinkingType = ThinkingType(anthropic);
            var isThinking = thinkingType == "enabled" || thinkingType == "adaptive";
            int? thinkingBudget = thinkingType == "enabled" && TryInt(anthropic, "thinking", "budgetTokens", out var budget) ? budget : (int?)null;
            var maxTokens = options.MaxOutputTokens ?? capabilities.MaxOutputTokens;
            var body = new JsonObject
            {
                ["model"] = modelId,
                ["max_tokens"] = maxTokens,
                ["messages"] = converted.Prompt["messages"] == null ? new JsonArray() : converted.Prompt["messages"]!.DeepClone(),
            };
            if (converted.Prompt["system"] != null)
            {
                body["system"] = converted.Prompt["system"]!.DeepClone();
            }

            if (temperature != null)
            {
                body["temperature"] = temperature.Value;
            }

            if (topK != null)
            {
                body["top_k"] = topK.Value;
            }

            if (topP != null)
            {
                body["top_p"] = topP.Value;
            }

            if (options.StopSequences != null)
            {
                var stops = new JsonArray();
                foreach (var stop in options.StopSequences)
                {
                    stops.Add(stop);
                }

                body["stop_sequences"] = stops;
            }

            if (thinkingType == "enabled" || thinkingType == "adaptive" || thinkingType == "disabled")
            {
                var thinking = new JsonObject();
                if (thinkingType != null)
                {
                    thinking["type"] = thinkingType;
                }

                if (thinkingBudget != null)
                {
                    thinking["budget_tokens"] = thinkingBudget.Value;
                }

                var display = ThinkingString(anthropic, "display");
                if (thinkingType == "adaptive" && display != null)
                {
                    thinking["display"] = display;
                }

                body["thinking"] = thinking;
            }

            if (isThinking && thinkingType == "enabled" && thinkingBudget == null)
            {
                warnings.Add(new AnthropicWarning("compatibility", "extended thinking", "thinking budget is required when thinking is enabled. using default budget of 1024 tokens."));
                body["thinking"] = new JsonObject { ["type"] = "enabled", ["budget_tokens"] = 1024 };
                thinkingBudget = 1024;
            }

            if (isThinking)
            {
                if (body.ContainsKey("temperature"))
                {
                    body.Remove("temperature");
                    warnings.Add(new AnthropicWarning("unsupported", "temperature", "temperature is not supported when thinking is enabled"));
                }

                if (topK != null)
                {
                    body.Remove("top_k");
                    warnings.Add(new AnthropicWarning("unsupported", "topK", "topK is not supported when thinking is enabled"));
                }

                if (topP != null)
                {
                    body.Remove("top_p");
                    warnings.Add(new AnthropicWarning("unsupported", "topP", "topP is not supported when thinking is enabled"));
                }

                body["max_tokens"] = maxTokens + (thinkingBudget ?? 0);
            }
            else if (isAnthropicModel && topP != null && temperature != null)
            {
                warnings.Add(new AnthropicWarning("unsupported", "topP", "topP is not supported when temperature is set. topP is ignored."));
                body.Remove("top_p");
            }

            var limited = body["max_tokens"]!.GetValue<int>();
            if (capabilities.IsKnownModel && limited > capabilities.MaxOutputTokens)
            {
                if (options.MaxOutputTokens != null)
                {
                    warnings.Add(new AnthropicWarning(
                        "unsupported",
                        "maxOutputTokens",
                        limited + " (maxOutputTokens + thinkingBudget) is greater than " + modelId + " " + capabilities.MaxOutputTokens + " max output tokens. The max output tokens have been limited to " + capabilities.MaxOutputTokens + "."));
                }

                body["max_tokens"] = capabilities.MaxOutputTokens;
            }

            AppendProviderFields(body, anthropic, betas, seen, warnings);
            if (useStructuredOutput && options.JsonSchema != null)
            {
                var format = new JsonObject
                {
                    ["type"] = "json_schema",
                    ["schema"] = AnthropicJsonSchema.Sanitize(options.JsonSchema.Value),
                };
                if (body["output_config"] is JsonObject outputConfig)
                {
                    outputConfig["format"] = format;
                }
                else
                {
                    body["output_config"] = new JsonObject { ["format"] = format };
                }
            }

            var tools = BuildTools(options, usesJsonTool);
            var toolOptions = new AnthropicToolPrepareOptions
            {
                CacheControl = validator,
                SupportsStructuredOutput = usesJsonTool ? false : capabilities.SupportsStructuredOutput,
                SupportsStrictTools = capabilities.SupportsStructuredOutput,
                DefaultEagerInputStreaming = stream && (!anthropic.TryGetProperty("toolStreaming", out var streaming) || streaming.ValueKind != JsonValueKind.False),
                RejectsForcedToolUse = capabilities.RejectsForcedToolUse,
                DisableParallelToolUse = usesJsonTool ? true : ReadBool(anthropic, "disableParallelToolUse"),
                ToolChoice = usesJsonTool ? Parse("{\"type\":\"required\"}") : ToolChoiceElement(options.ToolChoice),
            };
            var preparedTools = AnthropicTools.Prepare(tools, toolOptions);
            if (preparedTools.Tools != null)
            {
                body["tools"] = preparedTools.Tools;
            }

            if (preparedTools.ToolChoice != null)
            {
                body["tool_choice"] = preparedTools.ToolChoice;
            }

            warnings.AddRange(preparedTools.Warnings);
            foreach (var beta in preparedTools.Betas)
            {
                AddBeta(betas, seen, beta);
            }

            warnings.AddRange(validator.Warnings);
            if (stream)
            {
                body["stream"] = true;
            }

            AddHeaderBetas(options, betas, seen);
            return new AnthropicPreparedRequest(JsonNode.Parse(body.ToJsonString(BodyJson))!.AsObject(), warnings, betas, usesJsonTool);
        }
    }

    /// <summary>Serializes a prepared body.</summary>
    public static string Serialize(JsonObject body)
    {
        return body.ToJsonString(BodyJson);
    }

    private static void ApplyReasoning(string modelId, string? reasoning, AnthropicModelCapabilities capabilities, ref JsonElement anthropic, List<AnthropicWarning> warnings)
    {
        if (string.IsNullOrEmpty(reasoning) || reasoning == "provider-default")
        {
            return;
        }

        if (anthropic.ValueKind == JsonValueKind.Object && anthropic.TryGetProperty("effort", out _))
        {
            return;
        }

        if (anthropic.ValueKind == JsonValueKind.Object && anthropic.TryGetProperty("thinking", out _))
        {
            return;
        }

        JsonObject thinking;
        string? effort = null;
        if (reasoning == "none")
        {
            if (capabilities.RejectsThinkingDisabled)
            {
                warnings.Add(new AnthropicWarning(
                    "compatibility",
                    "reasoning",
                    "reasoning 'none' is not supported by " + modelId + "; it always uses adaptive thinking. Using effort 'low' to minimize thinking instead."));
                effort = "low";
                thinking = new JsonObject();
            }
            else
            {
                thinking = new JsonObject { ["type"] = "disabled" };
            }
        }
        else if (capabilities.SupportsAdaptiveThinking)
        {
            effort = MapEffort(reasoning!, capabilities.SupportsXhighEffort, warnings);
            thinking = new JsonObject { ["type"] = "adaptive", ["display"] = "summarized" };
        }
        else
        {
            var budget = MapBudget(reasoning!, capabilities.MaxOutputTokens);
            thinking = new JsonObject { ["type"] = "enabled", ["budgetTokens"] = budget };
        }

        var merged = anthropic.ValueKind == JsonValueKind.Object ? AnthropicJson.ObjectFrom(anthropic) : new JsonObject();
        if (thinking.Count > 0)
        {
            merged["thinking"] = thinking;
        }

        if (effort != null && ThinkingType(ParseObject(merged)) != "disabled")
        {
            merged["effort"] = effort;
        }

        using (var mergedDocument = JsonDocument.Parse(merged.ToJsonString()))
        {
            anthropic = mergedDocument.RootElement.Clone();
        }
    }

    private static string? MapEffort(string reasoning, bool xhigh, List<AnthropicWarning> warnings)
    {
        string mapped;
        if (reasoning == "minimal")
        {
            mapped = "low";
        }
        else if (reasoning == "low" || reasoning == "medium" || reasoning == "high")
        {
            mapped = reasoning;
        }
        else if (reasoning == "xhigh")
        {
            mapped = xhigh ? "xhigh" : "max";
        }
        else
        {
            warnings.Add(new AnthropicWarning("unsupported", "reasoning", "reasoning \"" + reasoning + "\" is not supported by this model."));
            return null;
        }

        if (mapped != reasoning)
        {
            warnings.Add(new AnthropicWarning("compatibility", "reasoning", "reasoning \"" + reasoning + "\" is not directly supported by this model. mapped to effort \"" + mapped + "\"."));
        }

        return mapped;
    }

    private static int MapBudget(string reasoning, int maxOutputTokens)
    {
        double percent = reasoning == "minimal" ? 0.02 : reasoning == "low" ? 0.1 : reasoning == "medium" ? 0.3 : reasoning == "high" ? 0.6 : 0.9;
        var budget = (int)Math.Round(maxOutputTokens * percent, MidpointRounding.AwayFromZero);
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

    private static void AppendProviderFields(JsonObject body, JsonElement anthropic, List<string> betas, HashSet<string> seen, List<AnthropicWarning> warnings)
    {
        var effort = AnthropicJson.String(anthropic, "effort");
        if (effort != null)
        {
            var output = new JsonObject { ["effort"] = effort };
            body["output_config"] = output;
        }

        if (anthropic.ValueKind == JsonValueKind.Object && anthropic.TryGetProperty("cacheControl", out var cache))
        {
            body["cache_control"] = AnthropicJson.Node(cache);
        }

        if (anthropic.ValueKind == JsonValueKind.Object && anthropic.TryGetProperty("metadata", out var metadata) && metadata.ValueKind == JsonValueKind.Object)
        {
            var userId = AnthropicJson.String(metadata, "userId");
            if (userId != null)
            {
                body["metadata"] = new JsonObject { ["user_id"] = userId };
            }
        }

        var speed = AnthropicJson.String(anthropic, "speed");
        if (speed != null)
        {
            body["speed"] = speed;
            if (speed == "fast")
            {
                AddBeta(betas, seen, "fast-mode-2026-02-01");
            }
        }

        var geo = AnthropicJson.String(anthropic, "inferenceGeo");
        if (geo != null)
        {
            body["inference_geo"] = geo;
        }

        if (anthropic.ValueKind == JsonValueKind.Object && anthropic.TryGetProperty("fallbacks", out var fallbacks))
        {
            if (fallbacks.ValueKind == JsonValueKind.String && fallbacks.GetString() == "default")
            {
                body["fallbacks"] = "default";
                AddBeta(betas, seen, "server-side-fallback-2026-07-01");
            }
            else if (fallbacks.ValueKind == JsonValueKind.Array && fallbacks.GetArrayLength() > 0)
            {
                body["fallbacks"] = AnthropicJson.Node(fallbacks);
                AddBeta(betas, seen, "server-side-fallback-2026-06-01");
            }
        }

        if (anthropic.ValueKind == JsonValueKind.Object && anthropic.TryGetProperty("contextManagement", out var context) && context.ValueKind == JsonValueKind.Object)
        {
            body["context_management"] = MapContext(context, warnings);
            AddBeta(betas, seen, "context-management-2025-06-27");
            if (context.TryGetProperty("edits", out var edits) && edits.ValueKind == JsonValueKind.Array)
            {
                foreach (var edit in edits.EnumerateArray())
                {
                    if (AnthropicJson.String(edit, "type") == "compact_20260112")
                    {
                        AddBeta(betas, seen, "compact-2026-01-12");
                    }
                }
            }
        }

        if (anthropic.ValueKind == JsonValueKind.Object && anthropic.TryGetProperty("safeguards", out var safeguards) && safeguards.ValueKind == JsonValueKind.Array && safeguards.GetArrayLength() > 0)
        {
            var mapped = new JsonArray();
            foreach (var safeguard in safeguards.EnumerateArray())
            {
                var node = new JsonObject { ["type"] = AnthropicJson.String(safeguard, "type") };
                AnthropicJson.CopyIfPresent(node, "classifier_context", safeguard, "classifierContext");
                mapped.Add(node);
            }

            body["safeguards"] = mapped;
            AddBeta(betas, seen, "dangerous-tool-use-2026-09-03");
        }
    }

    private static JsonObject MapContext(JsonElement context, List<AnthropicWarning> warnings)
    {
        var edits = new JsonArray();
        if (context.TryGetProperty("edits", out var source) && source.ValueKind == JsonValueKind.Array)
        {
            foreach (var edit in source.EnumerateArray())
            {
                var type = AnthropicJson.String(edit, "type");
                if (type == "clear_tool_uses_20250919")
                {
                    var node = new JsonObject { ["type"] = type };
                    AnthropicJson.CopyIfPresent(node, "trigger", edit, "trigger");
                    AnthropicJson.CopyIfPresent(node, "keep", edit, "keep");
                    AnthropicJson.CopyIfPresent(node, "clear_at_least", edit, "clearAtLeast");
                    AnthropicJson.CopyIfPresent(node, "clear_tool_inputs", edit, "clearToolInputs");
                    AnthropicJson.CopyIfPresent(node, "exclude_tools", edit, "excludeTools");
                    edits.Add(node);
                }
                else if (type == "clear_thinking_20251015")
                {
                    var node = new JsonObject { ["type"] = type };
                    AnthropicJson.CopyIfPresent(node, "keep", edit, "keep");
                    edits.Add(node);
                }
                else if (type == "compact_20260112")
                {
                    var node = new JsonObject { ["type"] = type };
                    AnthropicJson.CopyIfPresent(node, "trigger", edit, "trigger");
                    AnthropicJson.CopyIfPresent(node, "pause_after_compaction", edit, "pauseAfterCompaction");
                    AnthropicJson.CopyIfPresent(node, "instructions", edit, "instructions");
                    edits.Add(node);
                }
                else
                {
                    warnings.Add(new AnthropicWarning("other", null, "Unknown context management strategy: " + type));
                }
            }
        }

        return new JsonObject { ["edits"] = edits };
    }

    private static JsonElement? BuildTools(LanguageModelCallOptions options, bool usesJsonTool)
    {
        var tools = new JsonArray();
        if (options.Tools != null)
        {
            foreach (var tool in options.Tools)
            {
                var node = new JsonObject
                {
                    ["type"] = "function",
                    ["name"] = tool.Name,
                    ["inputSchema"] = AnthropicJson.Node(tool.InputSchema),
                };
                AnthropicJson.Set(node, "description", tool.Description == null ? null : JsonValue.Create(tool.Description));
                if (tool.Strict != null)
                {
                    node["strict"] = tool.Strict.Value;
                }

                tools.Add(node);
            }
        }

        var anthropic = ReadAnthropicOptions(options);
        if (anthropic.ValueKind == JsonValueKind.Object && anthropic.TryGetProperty("providerTools", out var extra) && extra.ValueKind == JsonValueKind.Array)
        {
            foreach (var tool in extra.EnumerateArray())
            {
                tools.Add(AnthropicJson.Node(tool));
            }
        }

        if (usesJsonTool && options.JsonSchema != null)
        {
            tools.Add(new JsonObject
            {
                ["type"] = "function",
                ["name"] = "json",
                ["description"] = "Respond with a JSON object.",
                ["inputSchema"] = AnthropicJson.Node(options.JsonSchema.Value),
            });
        }

        if (tools.Count == 0)
        {
            return null;
        }

        return CloneJson(tools.ToJsonString());
    }

    private static JsonElement ReadAnthropicOptions(LanguageModelCallOptions options)
    {
        if (options.ProviderOptions != null && options.ProviderOptions.TryGetValue("anthropic", out var configured) && configured.ValueKind == JsonValueKind.Object)
        {
            return configured;
        }

        return default;
    }

    private static void AddHeaderBetas(LanguageModelCallOptions options, List<string> betas, HashSet<string> seen)
    {
        if (options.Headers != null && options.Headers.TryGetValue("anthropic-beta", out var header) && !string.IsNullOrEmpty(header))
        {
            foreach (var beta in header!.Split(','))
            {
                var trimmed = beta.Trim();
                if (trimmed.Length > 0)
                {
                    AddBeta(betas, seen, trimmed);
                }
            }
        }

        var anthropic = ReadAnthropicOptions(options);
        if (anthropic.ValueKind == JsonValueKind.Object && anthropic.TryGetProperty("anthropicBeta", out var extra) && extra.ValueKind == JsonValueKind.Array)
        {
            foreach (var beta in extra.EnumerateArray())
            {
                if (beta.ValueKind == JsonValueKind.String)
                {
                    AddBeta(betas, seen, beta.GetString() ?? string.Empty);
                }
            }
        }
    }

    private static string? ThinkingType(JsonElement anthropic)
    {
        if (anthropic.ValueKind != JsonValueKind.Object || !anthropic.TryGetProperty("thinking", out var thinking) || thinking.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return AnthropicJson.String(thinking, "type");
    }

    private static string? ThinkingString(JsonElement anthropic, string name)
    {
        if (anthropic.ValueKind != JsonValueKind.Object || !anthropic.TryGetProperty("thinking", out var thinking))
        {
            return null;
        }

        return AnthropicJson.String(thinking, name);
    }

    private static bool TryInt(JsonElement anthropic, string objectName, string name, out int value)
    {
        value = 0;
        if (anthropic.ValueKind != JsonValueKind.Object || !anthropic.TryGetProperty(objectName, out var nested) || nested.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        return nested.TryGetProperty(name, out var number) && number.TryGetInt32(out value);
    }

    private static bool? ReadBool(JsonElement anthropic, string name)
    {
        if (anthropic.ValueKind != JsonValueKind.Object || !anthropic.TryGetProperty(name, out var value))
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.True)
        {
            return true;
        }

        if (value.ValueKind == JsonValueKind.False)
        {
            return false;
        }

        return null;
    }

    private static JsonElement? ToolChoiceElement(ToolChoice? choice)
    {
        if (choice == null)
        {
            return null;
        }

        if (choice.Type == "tool" && choice is ToolChoice.NamedChoice named)
        {
            return Parse("{\"type\":\"tool\",\"toolName\":" + JsonSerializer.Serialize(named.ToolName) + "}");
        }

        return Parse("{\"type\":" + JsonSerializer.Serialize(choice.Type) + "}");
    }

    private static JsonElement Parse(string json)
    {
        return CloneJson(json);
    }

    private static JsonElement CloneJson(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static JsonElement ParseObject(JsonObject obj)
    {
        return CloneJson(obj.ToJsonString());
    }

    private static void AddBeta(List<string> betas, HashSet<string> seen, string beta)
    {
        if (beta.Length > 0 && seen.Add(beta))
        {
            betas.Add(beta);
        }
    }
}

/// <summary>Parses an Anthropic Messages JSON response.</summary>
public static class AnthropicResponse
{
    /// <summary>Parses a generate response into the V4 result.</summary>
    public static LanguageModelGenerateResult Parse(string json, IReadOnlyDictionary<string, string> headers, AnthropicPreparedRequest request)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
        var root = document.RootElement;
        var usesJson = request.UsesJsonResponseTool;
        var jsonTool = false;
        var content = new List<GeneratedContent>();
        if (root.TryGetProperty("content", out var parts) && parts.ValueKind == JsonValueKind.Array)
        {
            foreach (var part in parts.EnumerateArray())
            {
                var type = AnthropicJson.String(part, "type");
                if (type == "text")
                {
                    if (!usesJson)
                    {
                        content.Add(new GeneratedText(AnthropicJson.String(part, "text") ?? string.Empty));
                    }
                }
                else if (type == "thinking")
                {
                    content.Add(new GeneratedReasoning(AnthropicJson.String(part, "thinking") ?? string.Empty));
                }
                else if (type == "redacted_thinking")
                {
                    content.Add(new GeneratedReasoning(string.Empty));
                }
                else if (type == "tool_use")
                {
                    var name = AnthropicJson.String(part, "name") ?? string.Empty;
                    var input = part.TryGetProperty("input", out var inputElement) ? JsonSerializer.Serialize(inputElement) : "{}";
                    if (usesJson && name == "json")
                    {
                        jsonTool = true;
                        content.Add(new GeneratedText(input));
                    }
                    else
                    {
                        content.Add(new GeneratedToolCall(
                            AnthropicJson.String(part, "id") ?? "tool",
                            name,
                            input,
                            CallerMetadata(part)));
                    }
                }
            }
        }

        var raw = root.TryGetProperty("stop_reason", out var stop) && stop.ValueKind == JsonValueKind.String ? stop.GetString() : null;
        var usageElement = root.TryGetProperty("usage", out var usageNode) ? usageNode : default;
        var usage = ConvertUsage(usageElement);
        var metadata = Metadata(root, usageElement);
        var warnings = new List<CallWarning>();
        foreach (var warning in request.Warnings)
        {
            warnings.Add(new CallWarning(warning.Type, warning.CallMessage));
        }

        return new LanguageModelGenerateResult(
            content,
            MapStop(raw, jsonTool),
            usage,
            raw,
            warnings,
            root.TryGetProperty("id", out var id) ? id.GetString() : null,
            metadata,
            json,
            root.TryGetProperty("model", out var model) ? model.GetString() : null,
            null,
            headers);
    }

    /// <summary>Maps an Anthropic stop reason.</summary>
    public static FinishReason MapStop(string? finishReason, bool isJsonResponseFromTool)
    {
        switch (finishReason)
        {
            case "pause_turn":
            case "end_turn":
            case "stop_sequence":
                return FinishReason.Stop;
            case "refusal":
                return FinishReason.ContentFilter;
            case "tool_use":
                return isJsonResponseFromTool ? FinishReason.Stop : FinishReason.ToolCalls;
            case "max_tokens":
            case "model_context_window_exceeded":
                return FinishReason.Length;
            case "compaction":
                return FinishReason.Other;
            default:
                return FinishReason.Other;
        }
    }

    private static LanguageModelUsage ConvertUsage(JsonElement usage)
    {
        if (usage.ValueKind != JsonValueKind.Object)
        {
            return LanguageModelUsage.Empty;
        }

        var input = ReadInt(usage, "input_tokens") ?? 0;
        var output = ReadInt(usage, "output_tokens") ?? 0;
        var cacheWrite = ReadInt(usage, "cache_creation_input_tokens") ?? 0;
        var cacheRead = ReadInt(usage, "cache_read_input_tokens") ?? 0;
        int? reasoning = null;
        if (usage.TryGetProperty("output_tokens_details", out var details) && details.ValueKind == JsonValueKind.Object && details.TryGetProperty("thinking_tokens", out var thinking) && thinking.ValueKind == JsonValueKind.Number)
        {
            reasoning = thinking.GetInt32();
        }

        var servedByFallback = false;
        if (usage.TryGetProperty("iterations", out var iterations) && iterations.ValueKind == JsonValueKind.Array && iterations.GetArrayLength() > 0)
        {
            var summedInput = 0;
            var summedOutput = 0;
            var executor = 0;
            foreach (var iteration in iterations.EnumerateArray())
            {
                var type = AnthropicJson.String(iteration, "type");
                if (type == "fallback_message")
                {
                    servedByFallback = true;
                }

                if (type == "compaction" || type == "message")
                {
                    executor++;
                    summedInput += ReadInt(iteration, "input_tokens") ?? 0;
                    summedOutput += ReadInt(iteration, "output_tokens") ?? 0;
                }
            }

            if (!servedByFallback && executor > 0)
            {
                input = summedInput;
                output = summedOutput;
            }
        }

        return new LanguageModelUsage(input + cacheWrite + cacheRead, output, null, cacheRead, cacheWrite, reasoning, usage.Clone());
    }

    private static JsonElement Metadata(JsonElement root, JsonElement usage)
    {
        var anthropic = new JsonObject();
        anthropic["usage"] = usage.ValueKind == JsonValueKind.Object ? AnthropicJson.Node(usage) : new JsonObject();
        if (root.TryGetProperty("stop_sequence", out var sequence) && sequence.ValueKind == JsonValueKind.String)
        {
            anthropic["stopSequence"] = sequence.GetString();
        }
        else
        {
            AnthropicJson.SetNull(anthropic, "stopSequence");
        }

        if (root.TryGetProperty("stop_details", out var details) && details.ValueKind == JsonValueKind.Object)
        {
            var mapped = new JsonObject { ["type"] = AnthropicJson.String(details, "type") };
            AnthropicJson.CopyIfPresent(mapped, "category", details, "category");
            AnthropicJson.CopyIfPresent(mapped, "explanation", details, "explanation");
            AnthropicJson.CopyIfPresent(mapped, "recommendedModel", details, "recommended_model");
            anthropic["stopDetails"] = mapped;
        }

        if (usage.ValueKind == JsonValueKind.Object && usage.TryGetProperty("iterations", out var iterations) && iterations.ValueKind == JsonValueKind.Array)
        {
            var mapped = new JsonArray();
            foreach (var iteration in iterations.EnumerateArray())
            {
                var node = new JsonObject
                {
                    ["type"] = AnthropicJson.String(iteration, "type"),
                    ["inputTokens"] = ReadInt(iteration, "input_tokens") ?? 0,
                    ["outputTokens"] = ReadInt(iteration, "output_tokens") ?? 0,
                };
                AnthropicJson.CopyIfPresent(node, "model", iteration, "model");
                mapped.Add(node);
            }

            anthropic["iterations"] = mapped;
        }
        else
        {
            AnthropicJson.SetNull(anthropic, "iterations");
        }

        if (root.TryGetProperty("container", out var container) && container.ValueKind == JsonValueKind.Object)
        {
            anthropic["container"] = AnthropicJson.Node(container);
        }
        else
        {
            AnthropicJson.SetNull(anthropic, "container");
        }

        if (root.TryGetProperty("context_management", out var context) && context.ValueKind == JsonValueKind.Object)
        {
            anthropic["contextManagement"] = AnthropicJson.Node(context);
        }
        else
        {
            AnthropicJson.SetNull(anthropic, "contextManagement");
        }

        var metadata = new JsonObject { ["anthropic"] = anthropic };
        return CloneJson(metadata.ToJsonString());
    }

    private static JsonElement? CallerMetadata(JsonElement part)
    {
        if (!part.TryGetProperty("caller", out var caller) || caller.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var type = AnthropicJson.String(caller, "type");
        var node = new JsonObject { ["type"] = type };
        if (caller.TryGetProperty("tool_id", out var toolId) && toolId.ValueKind == JsonValueKind.String)
        {
            node["toolId"] = toolId.GetString();
        }

        var metadata = new JsonObject { ["anthropic"] = new JsonObject { ["caller"] = node } };
        return CloneJson(metadata.ToJsonString());
    }

    private static int? ReadInt(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Number)
        {
            return null;
        }

        return value.TryGetInt32(out var number) ? number : (int?)value.GetDouble();
    }

    private static JsonElement CloneJson(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
