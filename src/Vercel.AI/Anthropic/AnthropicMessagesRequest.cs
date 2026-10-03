// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Anthropic;

/// <summary>A Messages request body plus the betas and warnings that go with it.</summary>
public sealed class AnthropicPreparedRequest
{
    /// <summary>Creates a prepared request.</summary>
    public AnthropicPreparedRequest(
        JsonObject body,
        IReadOnlyList<AnthropicWarning> warnings,
        IReadOnlyList<string> betas,
        bool usesJsonResponseTool,
        string providerOptionsName,
        bool usedCustomProviderKey)
    {
        Body = body;
        Warnings = warnings;
        Betas = betas;
        UsesJsonResponseTool = usesJsonResponseTool;
        ProviderOptionsName = providerOptionsName;
        UsedCustomProviderKey = usedCustomProviderKey;
    }

    /// <summary>JSON body.</summary>
    public JsonObject Body { get; }

    /// <summary>Preparation warnings.</summary>
    public IReadOnlyList<AnthropicWarning> Warnings { get; }

    /// <summary>Beta names for the <c>anthropic-beta</c> header.</summary>
    public IReadOnlyList<string> Betas { get; }

    /// <summary>True when structured output is sent as the <c>json</c> tool.</summary>
    public bool UsesJsonResponseTool { get; }

    /// <summary>Provider-options key derived from the provider name.</summary>
    public string ProviderOptionsName { get; }

    /// <summary>True when a custom provider-options key was present.</summary>
    public bool UsedCustomProviderKey { get; }
}

/// <summary>Builds an Anthropic Messages request from SDK call options.</summary>
public static class AnthropicMessagesRequest
{
    /// <summary>Prepares the request. <paramref name="stream"/> adds <c>stream: true</c> and eager tool streaming.</summary>
    public static AnthropicPreparedRequest Prepare(string modelId, LanguageModelCallOptions options, bool stream, string providerName)
    {
        var warnings = new List<AnthropicWarning>();
        var betas = new List<string>();
        var capabilities = AnthropicModelCapabilities.Get(modelId);
        var temperature = options.Temperature;
        var topP = options.TopP;
        int? topK = options.TopK;
        if (options.FrequencyPenalty != null)
        {
            warnings.Add(new AnthropicWarning("unsupported", "frequencyPenalty"));
        }

        if (options.PresencePenalty != null)
        {
            warnings.Add(new AnthropicWarning("unsupported", "presencePenalty"));
        }

        if (options.Seed != null)
        {
            warnings.Add(new AnthropicWarning("unsupported", "seed"));
        }

        if (temperature != null && temperature > 1)
        {
            warnings.Add(new AnthropicWarning("unsupported", "temperature", temperature + " exceeds anthropic maximum of 1.0. clamped to 1.0"));
            temperature = 1;
        }
        else if (temperature != null && temperature < 0)
        {
            warnings.Add(new AnthropicWarning("unsupported", "temperature", temperature + " is below anthropic minimum of 0. clamped to 0"));
            temperature = 0;
        }

        if (options.JsonSchema == null && options.JsonSchemaName != null)
        {
            warnings.Add(new AnthropicWarning("unsupported", "responseFormat", "JSON response format requires a schema. The response format is ignored."));
        }

        var providerOptionsName = ProviderOptionsName(providerName);
        var canonical = ReadProvider(options, "anthropic");
        var custom = providerOptionsName == "anthropic" ? null : ReadProvider(options, providerOptionsName);
        var usedCustom = custom != null;
        var anthropic = Merge(canonical, custom);
        var mode = String(anthropic, "structuredOutputMode") ?? "auto";
        var supportsStructured = capabilities.SupportsStructuredOutput;
        var useStructured = mode == "outputFormat" || (mode == "auto" && supportsStructured);
        if (!useStructured && capabilities.RejectsForcedToolUse && supportsStructured && options.JsonSchema != null)
        {
            warnings.Add(new AnthropicWarning(
                "unsupported",
                "providerOptions.anthropic.structuredOutputMode",
                "structuredOutputMode 'jsonTool' is not supported by " + modelId + " because it rejects forced tool use. Using 'outputFormat' instead."));
            useStructured = true;
        }

        var usesJsonTool = options.JsonSchema != null && !useStructured;
        if (usesJsonTool && Bool(anthropic, "disableParallelToolUse") == false)
        {
            warnings.Add(new AnthropicWarning(
                "unsupported",
                "providerOptions.anthropic.disableParallelToolUse",
                "`disableParallelToolUse: false` is ignored when using the JSON response tool. Parallel tool use is disabled to ensure a single coherent JSON tool call."));
        }

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

        if (!capabilities.IsKnownModel && options.MaxOutputTokens == null)
        {
            warnings.Add(new AnthropicWarning(
                "compatibility",
                "maxOutputTokens",
                "The model \"" + modelId + "\" is unknown. The max output tokens have been limited to " + capabilities.MaxOutputTokens + ". Set maxOutputTokens explicitly to override this limit."));
        }

        var validator = new AnthropicCacheControlValidator();
        var promptJson = ToPrompt(options);
        var sendReasoning = Bool(anthropic, "sendReasoning") ?? true;
        var toolNames = ProviderToolNames(options);
        var toolsets = ToolsetNames(options);
        var converted = AnthropicPrompt.Convert(promptJson, sendReasoning, validator, toolNames, toolsets);
        warnings.AddRange(converted.Warnings);
        betas.AddRange(converted.Betas);

        string? thinkingType = null;
        int? budget = null;
        string? display = null;
        string? effort = String(anthropic, "effort");
        JsonElement? blockBinding = anthropic == null ? null : BlockBinding(anthropic.Value);
        if (AnthropicReasoning.IsCustom(options.Reasoning) && effort == null)
        {
            var mapped = AnthropicReasoning.Resolve(options.Reasoning, modelId, capabilities, warnings);
            if (mapped != null)
            {
                if (thinkingType == null && String(anthropic, "thinkingType") == null && !HasThinking(anthropic))
                {
                    thinkingType = mapped.ThinkingType;
                    budget = mapped.BudgetTokens;
                    display = mapped.Display;
                }

                if (mapped.Effort != null && thinkingType != "disabled" && String(anthropic, "thinkingType") != "disabled")
                {
                    effort = mapped.Effort;
                }
            }
        }

        if (HasThinking(anthropic))
        {
            thinkingType = ThinkingType(anthropic);
            budget = ThinkingBudget(anthropic);
            display = ThinkingDisplay(anthropic);
            blockBinding = BlockBinding(anthropic!.Value);
        }

        if (capabilities.RejectsThinkingDisabled && thinkingType != null)
        {
            if (thinkingType == "disabled")
            {
                warnings.Add(new AnthropicWarning(
                    "unsupported",
                    "providerOptions.anthropic.thinking",
                    "thinking cannot be disabled for " + modelId + "; it always uses adaptive thinking. The thinking setting has been removed. Lower 'effort' to reduce thinking."));
                thinkingType = null;
                budget = null;
            }
            else if (thinkingType == "enabled")
            {
                warnings.Add(new AnthropicWarning(
                    "unsupported",
                    "providerOptions.anthropic.thinking",
                    "budget-based thinking is not supported by " + modelId + "; it always uses adaptive thinking. Using adaptive thinking instead. Use 'effort' to control how much the model thinks."));
                thinkingType = "adaptive";
                budget = null;
            }
        }

        if (capabilities.RejectsThinkingDisabledAboveHighEffort && thinkingType == "disabled" && (effort == "xhigh" || effort == "max"))
        {
            warnings.Add(new AnthropicWarning(
                "unsupported",
                "providerOptions.anthropic.effort",
                "effort '" + effort + "' is not supported by " + modelId + " when thinking is disabled. The effort has been lowered to 'high'."));
            effort = "high";
        }

        var isThinking = thinkingType == "enabled" || thinkingType == "adaptive";
        var sendThinking = isThinking || thinkingType == "disabled" || blockBinding != null;
        var maxTokens = options.MaxOutputTokens ?? capabilities.MaxOutputTokens;
        if (isThinking && thinkingType == "enabled" && budget == null)
        {
            warnings.Add(new AnthropicWarning("compatibility", "extended thinking", "thinking budget is required when thinking is enabled. using default budget of 1024 tokens."));
            budget = 1024;
        }

        if (isThinking)
        {
            if (temperature != null)
            {
                temperature = null;
                warnings.Add(new AnthropicWarning("unsupported", "temperature", "temperature is not supported when thinking is enabled"));
            }

            if (topK != null)
            {
                topK = null;
                warnings.Add(new AnthropicWarning("unsupported", "topK", "topK is not supported when thinking is enabled"));
            }

            if (topP != null)
            {
                topP = null;
                warnings.Add(new AnthropicWarning("unsupported", "topP", "topP is not supported when thinking is enabled"));
            }

            maxTokens += budget ?? 0;
        }
        else if ((capabilities.IsKnownModel || modelId.IndexOf("claude-", StringComparison.Ordinal) >= 0) && topP != null && temperature != null)
        {
            warnings.Add(new AnthropicWarning("unsupported", "topP", "topP is not supported when temperature is set. topP is ignored."));
            topP = null;
        }

        if (capabilities.IsKnownModel && maxTokens > capabilities.MaxOutputTokens)
        {
            if (options.MaxOutputTokens != null)
            {
                warnings.Add(new AnthropicWarning(
                    "unsupported",
                    "maxOutputTokens",
                    maxTokens + " (maxOutputTokens + thinkingBudget) is greater than " + modelId + " " + capabilities.MaxOutputTokens + " max output tokens. The max output tokens have been limited to " + capabilities.MaxOutputTokens + "."));
            }

            maxTokens = capabilities.MaxOutputTokens;
        }

        var tools = ToTools(options);
        if (usesJsonTool)
        {
            tools.Add(new AnthropicToolDefinition
            {
                Name = "json",
                Description = "Respond with a JSON object.",
                InputSchema = options.JsonSchema,
            });
        }

        var choiceType = usesJsonTool ? "required" : options.ToolChoice == null ? null : options.ToolChoice.Type;
        var choiceName = options.ToolChoice is ToolChoice.NamedChoice named ? named.ToolName : null;
        var disableParallel = usesJsonTool ? true : Bool(anthropic, "disableParallelToolUse");
        var eagerDefault = stream && (Bool(anthropic, "toolStreaming") ?? true);
        var preparedTools = AnthropicToolPreparer.Prepare(
            tools,
            choiceType,
            choiceName,
            disableParallel,
            validator,
            usesJsonTool ? false : supportsStructured,
            capabilities.SupportsStructuredOutput,
            eagerDefault,
            capabilities.RejectsForcedToolUse);
        warnings.AddRange(preparedTools.Warnings);
        betas.AddRange(preparedTools.Betas);
        warnings.AddRange(validator.Warnings);

        AddContextBetas(anthropic, betas, warnings);
        AddContainerBetas(anthropic, tools, betas, warnings);
        if (anthropic != null && anthropic.Value.TryGetProperty("mcpServers", out var servers) && servers.ValueKind == JsonValueKind.Array && servers.GetArrayLength() > 0)
        {
            betas.Add("mcp-client-2025-04-04");
        }

        if (anthropic != null && anthropic.Value.TryGetProperty("safeguards", out var safeguards) && safeguards.ValueKind == JsonValueKind.Array && safeguards.GetArrayLength() > 0)
        {
            betas.Add("dangerous-tool-use-2026-09-03");
        }

        if (anthropic != null && anthropic.Value.TryGetProperty("taskBudget", out _))
        {
            betas.Add("task-budgets-2026-03-13");
        }

        if (String(anthropic, "speed") == "fast")
        {
            betas.Add("fast-mode-2026-02-01");
        }

        if (display == "updates")
        {
            betas.Add("thinking-display-updates-2026-08-18");
        }

        if (blockBinding != null)
        {
            betas.Add("thinking-binding-controls-2026-08-01");
        }

        var fallbacks = anthropic == null ? null : AnthropicJson.Property(anthropic.Value, "fallbacks");
        if (fallbacks != null && fallbacks.Value.ValueKind == JsonValueKind.String && fallbacks.Value.GetString() == "default")
        {
            betas.Add("server-side-fallback-2026-07-01");
        }
        else if (fallbacks != null && fallbacks.Value.ValueKind == JsonValueKind.Array && fallbacks.Value.GetArrayLength() > 0)
        {
            betas.Add("server-side-fallback-2026-06-01");
        }

        foreach (var beta in HeaderBetas(options))
        {
            if (!betas.Contains(beta))
            {
                betas.Add(beta);
            }
        }

        if (anthropic != null && anthropic.Value.TryGetProperty("anthropicBeta", out var extra) && extra.ValueKind == JsonValueKind.Array)
        {
            foreach (var beta in extra.EnumerateArray())
            {
                var name = beta.GetString();
                if (!string.IsNullOrEmpty(name) && !betas.Contains(name))
                {
                    betas.Add(name);
                }
            }
        }

        var body = new JsonObject
        {
            ["model"] = modelId,
            ["max_tokens"] = maxTokens,
            ["messages"] = converted.Messages,
        };
        AnthropicJson.Set(body, "temperature", temperature);
        AnthropicJson.Set(body, "top_k", topK);
        AnthropicJson.Set(body, "top_p", topP);
        if (options.StopSequences != null && options.StopSequences.Count > 0)
        {
            var stops = new JsonArray();
            foreach (var stop in options.StopSequences)
            {
                stops.Add(stop);
            }

            body["stop_sequences"] = stops;
        }

        if (sendThinking)
        {
            var thinking = new JsonObject();
            AnthropicJson.Set(thinking, "type", thinkingType);
            if (thinkingType == "enabled" && budget != null)
            {
                thinking["budget_tokens"] = budget.Value;
            }

            if (thinkingType == "adaptive")
            {
                AnthropicJson.Set(thinking, "display", display);
            }

            if (blockBinding != null)
            {
                thinking["block_binding"] = new JsonObject
                {
                    ["prefix_mismatch_behavior"] = AnthropicJson.String(blockBinding.Value, "prefixMismatchBehavior"),
                };
            }

            body["thinking"] = thinking;
        }

        var outputConfig = new JsonObject();
        AnthropicJson.Set(outputConfig, "effort", effort);
        if (anthropic != null && anthropic.Value.TryGetProperty("taskBudget", out var taskBudget) && taskBudget.ValueKind == JsonValueKind.Object)
        {
            var budgetNode = new JsonObject
            {
                ["type"] = AnthropicJson.String(taskBudget, "type") ?? "tokens",
                ["total"] = AnthropicJson.Int(taskBudget, "total") ?? 0,
            };
            AnthropicJson.Set(budgetNode, "remaining", AnthropicJson.Int(taskBudget, "remaining"));
            outputConfig["task_budget"] = budgetNode;
        }

        if (useStructured && options.JsonSchema is { } schema)
        {
            outputConfig["format"] = new JsonObject
            {
                ["type"] = "json_schema",
                ["schema"] = AnthropicJsonSchema.Sanitize(schema),
            };
        }

        if (outputConfig.Count > 0)
        {
            body["output_config"] = outputConfig;
        }

        AnthropicJson.Set(body, "speed", String(anthropic, "speed"));
        AnthropicJson.Set(body, "service_tier", String(anthropic, "serviceTier"));
        AnthropicJson.Set(body, "inference_geo", String(anthropic, "inferenceGeo"));
        if (fallbacks != null && (fallbacks.Value.ValueKind == JsonValueKind.String || (fallbacks.Value.ValueKind == JsonValueKind.Array && fallbacks.Value.GetArrayLength() > 0)))
        {
            body["fallbacks"] = AnthropicJson.Node(fallbacks.Value);
        }

        var cacheControl = AnthropicCacheControlValidator.Read(anthropic == null ? null : Wrap(anthropic.Value));
        AnthropicJson.Set(body, "cache_control", cacheControl);
        var userId = anthropic == null ? null : UserId(anthropic.Value);
        if (userId != null)
        {
            body["metadata"] = new JsonObject { ["user_id"] = userId };
        }

        if (anthropic != null && anthropic.Value.TryGetProperty("mcpServers", out var mcp) && mcp.ValueKind == JsonValueKind.Array && mcp.GetArrayLength() > 0)
        {
            body["mcp_servers"] = MapMcp(mcp);
        }

        if (anthropic != null && anthropic.Value.TryGetProperty("container", out var container) && container.ValueKind == JsonValueKind.Object)
        {
            body["container"] = MapContainer(container);
        }

        if (converted.System != null && converted.System.Count > 0)
        {
            body["system"] = converted.System;
        }

        if (anthropic != null && anthropic.Value.TryGetProperty("safeguards", out var safeguardValue) && safeguardValue.ValueKind == JsonValueKind.Array && safeguardValue.GetArrayLength() > 0)
        {
            body["safeguards"] = MapSafeguards(safeguardValue);
        }

        if (anthropic != null && anthropic.Value.TryGetProperty("contextManagement", out var context) && context.ValueKind == JsonValueKind.Object)
        {
            body["context_management"] = MapContext(context, warnings);
        }

        if (preparedTools.Tools != null)
        {
            body["tools"] = preparedTools.Tools;
        }

        if (preparedTools.ToolChoice != null)
        {
            body["tool_choice"] = preparedTools.ToolChoice;
        }

        if (stream)
        {
            body["stream"] = true;
        }

        return new AnthropicPreparedRequest(body, warnings, betas, usesJsonTool, providerOptionsName, usedCustom);
    }

    private static void AddContextBetas(JsonElement? anthropic, List<string> betas, List<AnthropicWarning> warnings)
    {
        if (anthropic == null || !anthropic.Value.TryGetProperty("contextManagement", out var context))
        {
            return;
        }

        betas.Add("context-management-2025-06-27");
        if (context.TryGetProperty("edits", out var edits) && edits.ValueKind == JsonValueKind.Array)
        {
            foreach (var edit in edits.EnumerateArray())
            {
                if (AnthropicJson.String(edit, "type") == "compact_20260112")
                {
                    betas.Add("compact-2026-01-12");
                    break;
                }
            }
        }
    }

    private static void AddContainerBetas(JsonElement? anthropic, List<AnthropicToolDefinition> tools, List<string> betas, List<AnthropicWarning> warnings)
    {
        if (anthropic == null || !anthropic.Value.TryGetProperty("container", out var container) || container.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        if (!container.TryGetProperty("skills", out var skills) || skills.ValueKind != JsonValueKind.Array || skills.GetArrayLength() == 0)
        {
            return;
        }

        betas.Add("code-execution-2025-08-25");
        betas.Add("skills-2025-10-02");
        betas.Add("files-api-2025-04-14");
        var hasCode = false;
        foreach (var tool in tools)
        {
            if (tool.Id == "anthropic.code_execution_20250825" || tool.Id == "anthropic.code_execution_20260120")
            {
                hasCode = true;
            }
        }

        if (!hasCode)
        {
            warnings.Add(new AnthropicWarning("other", message: "code execution tool is required when using skills"));
        }
    }

    private static JsonNode MapContainer(JsonElement container)
    {
        if (!container.TryGetProperty("skills", out var skills) || skills.ValueKind != JsonValueKind.Array || skills.GetArrayLength() == 0)
        {
            return JsonValue.Create(AnthropicJson.String(container, "id") ?? string.Empty)!;
        }

        var mapped = new JsonArray();
        foreach (var skill in skills.EnumerateArray())
        {
            var type = AnthropicJson.String(skill, "type");
            var node = new JsonObject { ["type"] = type };
            if (type == "custom")
            {
                if (!skill.TryGetProperty("providerReference", out var reference) || reference.ValueKind != JsonValueKind.Object || !reference.TryGetProperty("anthropic", out var id))
                {
                    throw new AiSdkException("Custom skill provider reference does not include anthropic.");
                }

                node["skill_id"] = id.GetString();
            }
            else
            {
                node["skill_id"] = AnthropicJson.String(skill, "skillId");
            }

            AnthropicJson.Set(node, "version", AnthropicJson.String(skill, "version"));
            mapped.Add(node);
        }

        var result = new JsonObject { ["skills"] = mapped };
        AnthropicJson.Set(result, "id", AnthropicJson.String(container, "id"));
        return result;
    }

    private static JsonArray MapMcp(JsonElement servers)
    {
        var result = new JsonArray();
        foreach (var server in servers.EnumerateArray())
        {
            var node = new JsonObject
            {
                ["type"] = AnthropicJson.String(server, "type") ?? "url",
                ["name"] = AnthropicJson.String(server, "name"),
                ["url"] = AnthropicJson.String(server, "url"),
            };
            AnthropicJson.Set(node, "authorization_token", AnthropicJson.String(server, "authorizationToken"));
            if (server.TryGetProperty("toolConfiguration", out var configuration) && configuration.ValueKind == JsonValueKind.Object)
            {
                var mapped = new JsonObject();
                if (configuration.TryGetProperty("allowedTools", out var allowed))
                {
                    mapped["allowed_tools"] = AnthropicJson.Node(allowed);
                }

                var enabled = AnthropicJson.Bool(configuration, "enabled");
                if (enabled != null)
                {
                    mapped["enabled"] = enabled.Value;
                }

                node["tool_configuration"] = mapped;
            }

            result.Add(node);
        }

        return result;
    }

    private static JsonArray MapSafeguards(JsonElement safeguards)
    {
        var result = new JsonArray();
        foreach (var safeguard in safeguards.EnumerateArray())
        {
            var node = new JsonObject { ["type"] = AnthropicJson.String(safeguard, "type") };
            if (safeguard.TryGetProperty("classifierContext", out var context))
            {
                node["classifier_context"] = AnthropicJson.Node(context);
            }

            result.Add(node);
        }

        return result;
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
                    Copy(node, edit, "trigger");
                    Copy(node, edit, "keep");
                    CopyRenamed(node, edit, "clearAtLeast", "clear_at_least");
                    CopyBool(node, edit, "clearToolInputs", "clear_tool_inputs");
                    CopyRenamed(node, edit, "excludeTools", "exclude_tools");
                    edits.Add(node);
                }
                else if (type == "clear_thinking_20251015")
                {
                    var node = new JsonObject { ["type"] = type };
                    Copy(node, edit, "keep");
                    edits.Add(node);
                }
                else if (type == "compact_20260112")
                {
                    var node = new JsonObject { ["type"] = type };
                    Copy(node, edit, "trigger");
                    CopyBool(node, edit, "pauseAfterCompaction", "pause_after_compaction");
                    CopyString(node, edit, "instructions", "instructions");
                    edits.Add(node);
                }
                else
                {
                    warnings.Add(new AnthropicWarning("other", message: "Unknown context management strategy: " + type));
                }
            }
        }

        return new JsonObject { ["edits"] = edits };
    }

    private static void Copy(JsonObject target, JsonElement source, string name)
    {
        if (source.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null)
        {
            target[name] = AnthropicJson.Node(value);
        }
    }

    private static void CopyRenamed(JsonObject target, JsonElement source, string from, string to)
    {
        if (source.TryGetProperty(from, out var value) && value.ValueKind != JsonValueKind.Null)
        {
            target[to] = AnthropicJson.Node(value);
        }
    }

    private static void CopyBool(JsonObject target, JsonElement source, string from, string to)
    {
        var value = AnthropicJson.Bool(source, from);
        if (value != null)
        {
            target[to] = value.Value;
        }
    }

    private static void CopyString(JsonObject target, JsonElement source, string from, string to)
    {
        var value = AnthropicJson.String(source, from);
        if (value != null)
        {
            target[to] = value;
        }
    }

    private static List<AnthropicToolDefinition> ToTools(LanguageModelCallOptions options)
    {
        var tools = new List<AnthropicToolDefinition>();
        if (options.Tools != null)
        {
            foreach (var tool in options.Tools)
            {
                tools.Add(new AnthropicToolDefinition
                {
                    Name = tool.Name,
                    Description = tool.Description,
                    InputSchema = tool.InputSchema,
                    Strict = tool.Strict,
                });
            }
        }

        var provider = ReadProvider(options, "anthropic");
        if (provider != null && provider.Value.TryGetProperty("providerTools", out var extra) && extra.ValueKind == JsonValueKind.Array)
        {
            foreach (var tool in extra.EnumerateArray())
            {
                tools.Add(new AnthropicToolDefinition
                {
                    Type = AnthropicJson.String(tool, "type") ?? "provider",
                    Name = AnthropicJson.String(tool, "name") ?? string.Empty,
                    Description = AnthropicJson.String(tool, "description"),
                    Id = AnthropicJson.String(tool, "id"),
                    InputSchema = tool.TryGetProperty("inputSchema", out var schema) ? schema : null,
                    Args = tool.TryGetProperty("args", out var args) ? args : null,
                    ProviderOptions = tool.TryGetProperty("providerOptions", out var providerOptions) ? providerOptions : null,
                    Strict = AnthropicJson.Bool(tool, "strict"),
                    InputExamples = tool.TryGetProperty("inputExamples", out var examples) ? examples : null,
                });
            }
        }

        return tools;
    }

    private static Dictionary<string, string> ProviderToolNames(LanguageModelCallOptions options)
    {
        var names = new Dictionary<string, string>();
        var provider = ReadProvider(options, "anthropic");
        if (provider == null || !provider.Value.TryGetProperty("providerTools", out var tools) || tools.ValueKind != JsonValueKind.Array)
        {
            return names;
        }

        foreach (var tool in tools.EnumerateArray())
        {
            var id = AnthropicJson.String(tool, "id");
            var name = AnthropicJson.String(tool, "name");
            var api = ApiName(id);
            if (name != null && api != null && !names.ContainsKey(name))
            {
                names[name] = api;
            }
        }

        return names;
    }

    private static Dictionary<string, string> ToolsetNames(LanguageModelCallOptions options)
    {
        var names = new Dictionary<string, string>();
        var provider = ReadProvider(options, "anthropic");
        if (provider == null || !provider.Value.TryGetProperty("providerTools", out var tools))
        {
            return names;
        }

        foreach (var tool in tools.EnumerateArray())
        {
            if (AnthropicJson.String(tool, "id") == "anthropic.computer_toolset_20260801")
            {
                var name = AnthropicJson.String(tool, "name");
                if (name != null)
                {
                    names[name] = "computer";
                }
            }
        }

        return names;
    }

    private static string? ApiName(string? id)
    {
        switch (id)
        {
            case "anthropic.code_execution_20250522":
            case "anthropic.code_execution_20250825":
            case "anthropic.code_execution_20260120":
                return "code_execution";
            case "anthropic.computer_20241022":
            case "anthropic.computer_20250124":
            case "anthropic.computer_20251124":
            case "anthropic.computer_toolset_20260801":
                return "computer";
            case "anthropic.text_editor_20241022":
            case "anthropic.text_editor_20250124":
                return "str_replace_editor";
            case "anthropic.text_editor_20250429":
            case "anthropic.text_editor_20250728":
                return "str_replace_based_edit_tool";
            case "anthropic.bash_20241022":
            case "anthropic.bash_20250124":
                return "bash";
            case "anthropic.memory_20250818":
                return "memory";
            case "anthropic.web_search_20250305":
            case "anthropic.web_search_20260209":
            case "anthropic.web_search_20260318":
                return "web_search";
            case "anthropic.web_fetch_20250910":
            case "anthropic.web_fetch_20260209":
            case "anthropic.web_fetch_20260318":
                return "web_fetch";
            case "anthropic.tool_search_regex_20251119":
                return "tool_search_tool_regex";
            case "anthropic.tool_search_bm25_20251119":
                return "tool_search_tool_bm25";
            case "anthropic.advisor_20260301":
                return "advisor";
            default:
                return null;
        }
    }

    private static JsonElement ToPrompt(LanguageModelCallOptions options)
    {
        var provider = ReadProvider(options, "anthropic");
        if (provider != null && provider.Value.TryGetProperty("prompt", out var prompt) && prompt.ValueKind == JsonValueKind.Array)
        {
            return prompt;
        }

        var messages = new JsonArray();
        foreach (var message in options.Prompt)
        {
            if (message is SystemModelMessage system)
            {
                messages.Add(new JsonObject { ["role"] = "system", ["content"] = system.Content });
            }
            else if (message is UserModelMessage user)
            {
                var parts = new JsonArray();
                foreach (var part in user.Content)
                {
                    if (part is TextContentPart text)
                    {
                        parts.Add(new JsonObject { ["type"] = "text", ["text"] = text.Text });
                    }
                    else if (part is FileContentPart file)
                    {
                        var data = new JsonObject();
                        if (!string.IsNullOrEmpty(file.Url))
                        {
                            data["type"] = "url";
                            data["url"] = file.Url;
                        }
                        else if (file.Data != null)
                        {
                            data["type"] = "data";
                            data["data"] = Convert.ToBase64String(file.Data);
                        }

                        var node = new JsonObject { ["type"] = "file", ["mediaType"] = file.MediaType, ["data"] = data };
                        AnthropicJson.Set(node, "filename", file.FileName);
                        parts.Add(node);
                    }
                }

                messages.Add(new JsonObject { ["role"] = "user", ["content"] = parts });
            }
            else if (message is AssistantModelMessage assistant)
            {
                var parts = new JsonArray();
                if (!string.IsNullOrEmpty(assistant.Reasoning))
                {
                    parts.Add(new JsonObject { ["type"] = "reasoning", ["text"] = assistant.Reasoning });
                }

                if (!string.IsNullOrEmpty(assistant.Text))
                {
                    parts.Add(new JsonObject { ["type"] = "text", ["text"] = assistant.Text });
                }

                foreach (var call in assistant.ToolCalls)
                {
                    JsonNode input;
                    try
                    {
                        input = JsonNode.Parse(string.IsNullOrWhiteSpace(call.ArgumentsJson) ? "{}" : call.ArgumentsJson) ?? new JsonObject();
                    }
                    catch (JsonException)
                    {
                        input = new JsonObject { ["rawInvalidInput"] = call.ArgumentsJson };
                    }

                    parts.Add(new JsonObject
                    {
                        ["type"] = "tool-call",
                        ["toolCallId"] = call.ToolCallId,
                        ["toolName"] = call.ToolName,
                        ["input"] = input,
                    });
                }

                messages.Add(new JsonObject { ["role"] = "assistant", ["content"] = parts });
            }
            else if (message is ToolModelMessage tool)
            {
                JsonNode value;
                try
                {
                    value = JsonNode.Parse(tool.OutputJson) ?? JsonValue.Create(tool.OutputJson)!;
                }
                catch (JsonException)
                {
                    value = JsonValue.Create(tool.OutputJson)!;
                }

                messages.Add(new JsonObject
                {
                    ["role"] = "tool",
                    ["content"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["type"] = "tool-result",
                            ["toolName"] = tool.ToolName,
                            ["toolCallId"] = tool.ToolCallId,
                            ["output"] = new JsonObject
                            {
                                ["type"] = tool.IsError ? "error-json" : "json",
                                ["value"] = value,
                            },
                        },
                    },
                });
            }
        }

        using var document = JsonDocument.Parse(messages.ToJsonString());
        return document.RootElement.Clone();
    }

    private static IEnumerable<string> HeaderBetas(LanguageModelCallOptions options)
    {
        if (options.Headers == null)
        {
            yield break;
        }

        string? header = null;
        foreach (var pair in options.Headers)
        {
            if (pair.Key.Equals("anthropic-beta", StringComparison.OrdinalIgnoreCase) && pair.Value != null)
            {
                header = pair.Value;
            }
        }

        if (header == null)
        {
            yield break;
        }

        foreach (var part in header.Split(','))
        {
            var beta = part.Trim().ToLowerInvariant();
            if (beta.Length > 0)
            {
                yield return beta;
            }
        }
    }

    private static string ProviderOptionsName(string provider)
    {
        var dot = provider.IndexOf('.');
        return dot < 0 ? provider : provider.Substring(0, dot);
    }

    private static JsonElement? ReadProvider(LanguageModelCallOptions options, string name)
    {
        if (options.ProviderOptions == null || !options.ProviderOptions.TryGetValue(name, out var value) || value.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return value;
    }

    private static JsonElement? Merge(JsonElement? canonical, JsonElement? custom)
    {
        if (canonical == null)
        {
            return custom;
        }

        if (custom == null)
        {
            return canonical;
        }

        var merged = JsonNode.Parse(canonical.Value.GetRawText()) as JsonObject ?? new JsonObject();
        var extra = JsonNode.Parse(custom.Value.GetRawText()) as JsonObject;
        if (extra != null)
        {
            foreach (var pair in extra)
            {
                merged[pair.Key] = pair.Value?.DeepClone();
            }
        }

        using var document = JsonDocument.Parse(merged.ToJsonString());
        return document.RootElement.Clone();
    }

    private static bool HasThinking(JsonElement? anthropic)
    {
        return anthropic != null && anthropic.Value.TryGetProperty("thinking", out var thinking) && thinking.ValueKind == JsonValueKind.Object;
    }

    private static string? ThinkingType(JsonElement? anthropic)
    {
        if (!HasThinking(anthropic))
        {
            return null;
        }

        return AnthropicJson.String(anthropic!.Value.GetProperty("thinking"), "type");
    }

    private static int? ThinkingBudget(JsonElement? anthropic)
    {
        if (!HasThinking(anthropic))
        {
            return null;
        }

        return AnthropicJson.Int(anthropic!.Value.GetProperty("thinking"), "budgetTokens");
    }

    private static string? ThinkingDisplay(JsonElement? anthropic)
    {
        if (!HasThinking(anthropic))
        {
            return null;
        }

        return AnthropicJson.String(anthropic!.Value.GetProperty("thinking"), "display");
    }

    private static JsonElement? BlockBinding(JsonElement anthropic)
    {
        if (!anthropic.TryGetProperty("thinking", out var thinking) || thinking.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return thinking.TryGetProperty("blockBinding", out var binding) && binding.ValueKind == JsonValueKind.Object ? binding : (JsonElement?)null;
    }

    private static string? UserId(JsonElement anthropic)
    {
        if (!anthropic.TryGetProperty("metadata", out var metadata) || metadata.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return AnthropicJson.String(metadata, "userId");
    }

    private static JsonElement Wrap(JsonElement anthropic)
    {
        using var document = JsonDocument.Parse("{\"anthropic\":" + anthropic.GetRawText() + "}");
        return document.RootElement.Clone();
    }

    private static string? String(JsonElement? element, string name)
    {
        return element == null ? null : AnthropicJson.String(element.Value, name);
    }

    private static bool? Bool(JsonElement? element, string name)
    {
        return element == null ? null : AnthropicJson.Bool(element.Value, name);
    }
}
