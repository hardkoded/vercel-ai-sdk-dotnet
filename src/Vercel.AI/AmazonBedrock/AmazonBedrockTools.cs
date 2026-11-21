// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Builds a Converse <c>toolConfig</c> from V4 function tools.</summary>
public static class AmazonBedrockTools
{
    /// <summary>
    /// Prepares function tools. An empty or missing list returns an empty object.
    /// Tool choice <c>none</c> clears the tools. A named choice keeps only that tool, except for
    /// Anthropic models, which keep every function tool.
    /// </summary>
    public static JsonObject Prepare(
        string modelId,
        IReadOnlyList<LanguageModelTool>? tools,
        ToolChoice? toolChoice,
        out List<CallWarning> warnings
    ) => Prepare(modelId, tools, toolChoice, null, null, out warnings, out _);

    /// <summary>
    /// Prepares function tools and the Anthropic <c>tool_choice</c> that goes into
    /// <c>additionalModelRequestFields</c> (<paramref name="additionalTools"/>).
    /// A named tool choice keeps every function tool for Anthropic models, so parallel tool use is
    /// disabled by default to keep the response to the named tool.
    /// <c>disableParallelToolUse: false</c> opts out.
    /// </summary>
    public static JsonObject Prepare(
        string modelId,
        IReadOnlyList<LanguageModelTool>? tools,
        ToolChoice? toolChoice,
        int? reasoningBudgetTokens,
        bool? disableParallelToolUse,
        out List<CallWarning> warnings,
        out JsonObject? additionalTools
    ) =>
        Prepare(
            modelId,
            tools,
            toolChoice,
            null,
            reasoningBudgetTokens,
            disableParallelToolUse,
            false,
            out warnings,
            out additionalTools,
            out _
        );

    /// <summary>
    /// Prepares function and provider tools. Maps to <c>prepareTools</c>.
    /// <paramref name="rejectsForcedToolUse"/> makes an Anthropic model that rejects forced tool use fall back
    /// from <c>required</c> or a named choice to <c>auto</c>, with a warning. A named choice then sends only the named tool.
    /// <paramref name="betas"/> are the Anthropic beta headers the provider tools need.
    /// </summary>
    public static JsonObject Prepare(
        string modelId,
        IReadOnlyList<LanguageModelTool>? tools,
        ToolChoice? toolChoice,
        AmazonBedrockChatModelFamily? modelFamily,
        int? reasoningBudgetTokens,
        bool? disableParallelToolUse,
        bool rejectsForcedToolUse,
        out List<CallWarning> warnings,
        out JsonObject? additionalTools,
        out IReadOnlyCollection<string> betas
    )
    {
        var toolWarnings = new List<CallWarning>();
        var betaSet = new List<string>();
        warnings = toolWarnings;
        additionalTools = null;
        betas = betaSet;
        if (tools == null || tools.Count == 0)
        {
            return new JsonObject();
        }

        // Filter out Anthropic web tools that Amazon Bedrock does not support.
        var supportedTools = new List<LanguageModelTool>();
        foreach (var tool in tools)
        {
            if (
                tool.ProviderToolId
                is "anthropic.web_search_20250305"
                    or "anthropic.web_search_20260318"
                    or "anthropic.web_fetch_20260318"
            )
            {
                var toolType = tool.ProviderToolId.Substring("anthropic.".Length);
                toolWarnings.Add(
                    new CallWarning(
                        "unsupported",
                        toolType + " tool",
                        "The " + toolType + " tool is not supported on Amazon Bedrock."
                    )
                );
                continue;
            }

            supportedTools.Add(tool);
        }

        if (supportedTools.Count == 0)
        {
            return new JsonObject();
        }

        var isAnthropicModel = AmazonBedrockModelSupport.IsAnthropicModel(
            modelId,
            reasoningBudgetTokens,
            modelFamily
        );
        var providerTools = supportedTools.Where(tool => tool.IsProviderDefined).ToList();
        var functionTools = supportedTools.Where(tool => !tool.IsProviderDefined).ToList();
        var namedChoice = toolChoice as ToolChoice.NamedChoice;
        var rejectsForcedToolChoice =
            isAnthropicModel
            && rejectsForcedToolUse
            && toolChoice is { Type: "required" or "tool" };
        var providerToolsForRequest =
            rejectsForcedToolChoice && namedChoice != null
                ? providerTools
                    .Where(tool =>
                        string.Equals(tool.Name, namedChoice.ToolName, StringComparison.Ordinal)
                    )
                    .ToList()
                : providerTools;
        // A named tool choice keeps every function tool for these models (see below), so parallel tool use
        // is disabled by default to keep the response to the named tool. `disableParallelToolUse: false` opts out.
        var disableParallelToolUseForRequest = disableParallelToolUse
            ?? (isAnthropicModel && namedChoice != null && !rejectsForcedToolChoice ? true : null);

        var prepared = new JsonArray();
        var usingAnthropicTools = isAnthropicModel && providerToolsForRequest.Count > 0;

        // Handle Anthropic provider-defined tools for Anthropic models on Bedrock.
        if (usingAnthropicTools)
        {
            var anthropicPrepared = AnthropicToolPreparer.Prepare(
                providerTools.Select(ToAnthropicTool).ToList(),
                toolChoice?.Type,
                namedChoice?.ToolName,
                disableParallelToolUseForRequest,
                supportsStructuredOutput: false,
                supportsStrictTools: false,
                rejectsForcedToolUse: rejectsForcedToolUse
            );
            foreach (var warning in anthropicPrepared.Warnings)
            {
                toolWarnings.Add(
                    warning.Details != null || warning.Feature != null
                        ? new CallWarning(
                            warning.Type,
                            warning.Feature ?? string.Empty,
                            warning.Details
                        )
                        : new CallWarning(warning.Type, warning.Message ?? string.Empty)
                );
            }

            foreach (var beta in anthropicPrepared.Betas)
            {
                if (!betaSet.Contains(beta))
                {
                    betaSet.Add(beta);
                }
            }

            // For Anthropic tools on Bedrock, only the 'tool_choice' goes into additional fields.
            // The tool definitions themselves are sent in the standard 'toolConfig'.
            if (anthropicPrepared.ToolChoice != null)
            {
                additionalTools = new JsonObject
                {
                    ["tool_choice"] = anthropicPrepared.ToolChoice.DeepClone(),
                };
            }

            // Create a standard Bedrock tool representation for validation purposes.
            foreach (var tool in providerToolsForRequest)
            {
                if (AnthropicTools.InputSchemaFor(tool.ProviderToolId!) is { } inputSchema)
                {
                    prepared.Add(
                        new JsonObject
                        {
                            ["toolSpec"] = new JsonObject
                            {
                                ["name"] = tool.Name,
                                ["inputSchema"] = new JsonObject
                                {
                                    ["json"] = JsonNode.Parse(inputSchema.GetRawText()),
                                },
                            },
                        }
                    );
                }
                else
                {
                    toolWarnings.Add(
                        new CallWarning("unsupported", "tool " + tool.ProviderToolId, null)
                    );
                }
            }
        }
        else
        {
            // Report unsupported provider-defined tools for non-anthropic models.
            foreach (var tool in providerToolsForRequest)
            {
                toolWarnings.Add(
                    new CallWarning("unsupported", "tool " + tool.ProviderToolId, null)
                );
            }
        }

        var preparedToolChoice = toolChoice;
        if (!usingAnthropicTools && rejectsForcedToolChoice)
        {
            toolWarnings.Add(
                new CallWarning(
                    "unsupported",
                    "toolChoice",
                    namedChoice != null
                        ? "toolChoice 'tool' is not supported by this model because it rejects forced tool use. "
                            + "Only the '"
                            + namedChoice.ToolName
                            + "' tool is sent with 'auto' tool choice. "
                            + "Instruct the model to use the tool in the prompt and verify that a tool call was made."
                        : "toolChoice 'required' is not supported by this model because it rejects forced tool use. "
                            + "Using 'auto' instead. Instruct the model to use a tool in the prompt and verify that a tool call was made."
                )
            );
            preparedToolChoice = ToolChoice.Auto;
        }

        // Anthropic models that accept forced tool use guarantee the named tool is called, so
        // they keep every function tool. Other models only receive the named tool so they cannot call a different one.
        var filteredFunctionTools =
            namedChoice != null && (!isAnthropicModel || rejectsForcedToolChoice)
                ? functionTools
                    .Where(tool =>
                        string.Equals(tool.Name, namedChoice.ToolName, StringComparison.Ordinal)
                    )
                    .ToList()
                : functionTools;

        var supportsStrict = AmazonBedrockModelSupport.SupportsStrictTools(modelId);
        foreach (var tool in filteredFunctionTools)
        {
            var includeStrict = false;
            if (tool.Strict != null)
            {
                if (!supportsStrict)
                {
                    if (tool.Strict.Value)
                    {
                        toolWarnings.Add(
                            new CallWarning(
                                "unsupported",
                                "strict",
                                "Tool '"
                                    + tool.Name
                                    + "' has strict: true, but strict mode is not supported by this model on Amazon Bedrock. The strict property will be ignored."
                            )
                        );
                    }
                }
                else if (tool.Strict.Value && !IsStrictSchemaCompatible(tool.InputSchema))
                {
                    toolWarnings.Add(
                        new CallWarning(
                            "unsupported",
                            "strict",
                            "Tool '"
                                + tool.Name
                                + "' has strict: true, but Amazon Bedrock requires every object in a strict tool schema to set additionalProperties: false. The strict property will be ignored."
                        )
                    );
                }
                else
                {
                    includeStrict = true;
                }
            }

            var spec = new JsonObject { ["name"] = tool.Name };
            if (!string.IsNullOrWhiteSpace(tool.Description))
            {
                spec["description"] = tool.Description;
            }

            if (includeStrict)
            {
                spec["strict"] = tool.Strict!.Value;
            }

            spec["inputSchema"] = new JsonObject
            {
                ["json"] = JsonNode.Parse(tool.InputSchema.GetRawText()),
            };
            prepared.Add(new JsonObject { ["toolSpec"] = spec });
        }

        if (
            isAnthropicModel
            && !usingAnthropicTools
            && disableParallelToolUseForRequest == true
            && prepared.Count > 0
            && preparedToolChoice is not { Type: "none" }
        )
        {
            var anthropicChoice = preparedToolChoice switch
            {
                { Type: "required" } => new JsonObject { ["type"] = "any" },
                ToolChoice.NamedChoice named => new JsonObject
                {
                    ["type"] = "tool",
                    ["name"] = named.ToolName,
                },
                _ => new JsonObject { ["type"] = "auto" },
            };
            anthropicChoice["disable_parallel_tool_use"] = true;
            additionalTools = new JsonObject { ["tool_choice"] = anthropicChoice };
        }

        // Handle toolChoice for standard Bedrock tools, but NOT for Anthropic provider-defined tools.
        JsonObject? bedrockToolChoice = null;
        if (
            !usingAnthropicTools
            && additionalTools?["tool_choice"] == null
            && prepared.Count > 0
            && preparedToolChoice != null
        )
        {
            if (preparedToolChoice.Type == "none")
            {
                prepared.Clear();
            }
            else
            {
                bedrockToolChoice = ToolChoiceObject(preparedToolChoice);
            }
        }

        if (prepared.Count == 0)
        {
            return new JsonObject();
        }

        var config = new JsonObject { ["tools"] = prepared };
        if (bedrockToolChoice != null)
        {
            config["toolChoice"] = bedrockToolChoice;
        }

        return config;
    }

    private static AnthropicToolDefinition ToAnthropicTool(LanguageModelTool tool) =>
        new()
        {
            Type = "provider",
            Name = tool.Name,
            Id = tool.ProviderToolId,
            Args = tool.Args,
        };

    private static JsonObject ToolChoiceObject(ToolChoice toolChoice)
    {
        switch (toolChoice.Type)
        {
            case "required":
                return new JsonObject { ["any"] = new JsonObject() };
            case "tool":
                var named = (ToolChoice.NamedChoice)toolChoice;
                return new JsonObject { ["tool"] = new JsonObject { ["name"] = named.ToolName } };
            default:
                return new JsonObject { ["auto"] = new JsonObject() };
        }
    }

    private static bool IsStrictSchemaCompatible(JsonElement schema)
    {
        if (schema.ValueKind != JsonValueKind.Object)
        {
            return true;
        }

        if (
            IsObjectType(schema)
            && (
                !schema.TryGetProperty("additionalProperties", out var additional)
                || additional.ValueKind != JsonValueKind.False
            )
        )
        {
            return false;
        }

        foreach (var name in new[] { "properties", "patternProperties", "definitions", "$defs" })
        {
            if (schema.TryGetProperty(name, out var map) && map.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in map.EnumerateObject())
                {
                    if (!IsStrictSchemaCompatible(property.Value))
                    {
                        return false;
                    }
                }
            }
        }

        if (schema.TryGetProperty("items", out var items))
        {
            if (items.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in items.EnumerateArray())
                {
                    if (!IsStrictSchemaCompatible(item))
                    {
                        return false;
                    }
                }
            }
            else if (!IsStrictSchemaCompatible(items))
            {
                return false;
            }
        }

        foreach (var name in new[] { "anyOf", "allOf", "oneOf" })
        {
            if (
                schema.TryGetProperty(name, out var alternatives)
                && alternatives.ValueKind == JsonValueKind.Array
            )
            {
                foreach (var alternative in alternatives.EnumerateArray())
                {
                    if (!IsStrictSchemaCompatible(alternative))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private static bool IsObjectType(JsonElement schema)
    {
        if (!schema.TryGetProperty("type", out var type))
        {
            return false;
        }

        if (type.ValueKind == JsonValueKind.String)
        {
            return type.GetString() == "object";
        }

        if (type.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in type.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String && item.GetString() == "object")
                {
                    return true;
                }
            }
        }

        return false;
    }
}
