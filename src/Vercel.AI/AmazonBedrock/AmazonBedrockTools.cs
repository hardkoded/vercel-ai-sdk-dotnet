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
    /// Prepares tools. An empty or missing list returns an empty object.
    /// Tool choice <c>none</c> clears the tools. A named choice keeps only that tool, except for
    /// Anthropic models, which keep every function tool.
    /// </summary>
    public static JsonObject Prepare(
        string modelId,
        IReadOnlyList<LanguageModelTool>? tools,
        ToolChoice? toolChoice,
        out List<CallWarning> warnings
    ) => Prepare(modelId, tools, toolChoice, null, null, null, false, out warnings, out _, out _);

    /// <summary>
    /// Prepares tools and the Anthropic <c>tool_choice</c> that goes into
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
    /// Prepares function tools and Anthropic provider tools.
    /// <paramref name="betas"/> are the Anthropic beta headers the provider tools need.
    /// <paramref name="rejectsForcedToolUse"/> makes a model that rejects forced tool use fall back to
    /// <c>auto</c> for tool choice <c>required</c> and <c>tool</c>.
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
        out HashSet<string> betas
    )
    {
        warnings = new List<CallWarning>();
        additionalTools = null;
        betas = new HashSet<string>(StringComparer.Ordinal);
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
                var name = tool.ProviderToolId.Substring("anthropic.".Length);
                warnings.Add(
                    new CallWarning(
                        "unsupported",
                        name + " tool",
                        "The " + name + " tool is not supported on Amazon Bedrock."
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
            modelFamily,
            reasoningBudgetTokens
        );
        var providerTools = supportedTools.Where(tool => tool.IsProviderDefined).ToList();
        var functionTools = supportedTools.Where(tool => !tool.IsProviderDefined).ToList();
        var rejectsForcedToolChoice =
            isAnthropicModel
            && rejectsForcedToolUse
            && toolChoice is { Type: "required" } or ToolChoice.NamedChoice;
        var providerToolsForRequest =
            rejectsForcedToolChoice && toolChoice is ToolChoice.NamedChoice namedProvider
                ? providerTools
                    .Where(tool =>
                        string.Equals(tool.Name, namedProvider.ToolName, StringComparison.Ordinal)
                    )
                    .ToList()
                : providerTools;

        // A named tool choice keeps every function tool for these models (see below), so parallel
        // tool use is disabled by default to keep the response to the named tool.
        // `disableParallelToolUse: false` opts out.
        var disableParallelToolUseForRequest = disableParallelToolUse
            ?? (
                isAnthropicModel && toolChoice is ToolChoice.NamedChoice && !rejectsForcedToolChoice
                    ? true
                    : null
            );

        var bedrockTools = new JsonArray();
        var usingAnthropicTools = isAnthropicModel && providerToolsForRequest.Count > 0;

        // Handle Anthropic provider-defined tools for Anthropic models on Bedrock.
        if (usingAnthropicTools)
        {
            var prepared = AnthropicToolPreparer.Prepare(
                providerTools.Select(ToAnthropicTool).ToList(),
                toolChoice?.Type,
                (toolChoice as ToolChoice.NamedChoice)?.ToolName,
                disableParallelToolUseForRequest,
                supportsStructuredOutput: false,
                supportsStrictTools: false,
                rejectsForcedToolUse: rejectsForcedToolUse
            );
            foreach (var warning in prepared.Warnings)
            {
                warnings.Add(
                    new CallWarning(warning.Type, warning.Feature ?? string.Empty, warning.Details)
                );
            }

            foreach (var beta in prepared.Betas)
            {
                betas.Add(beta);
            }

            // For Anthropic tools on Bedrock, only the 'tool_choice' goes into additional fields.
            // The tool definitions themselves are sent in the standard 'toolConfig'.
            if (prepared.ToolChoice != null)
            {
                additionalTools = new JsonObject { ["tool_choice"] = prepared.ToolChoice };
            }

            // Create a standard Bedrock tool representation for validation purposes.
            foreach (var tool in providerToolsForRequest)
            {
                var schema = AnthropicProviderToolSchemas.Get(tool.ProviderToolId!, tool.Args);
                if (schema != null)
                {
                    bedrockTools.Add(
                        new JsonObject
                        {
                            ["toolSpec"] = new JsonObject
                            {
                                ["name"] = tool.Name,
                                ["inputSchema"] = new JsonObject { ["json"] = schema },
                            },
                        }
                    );
                }
                else
                {
                    warnings.Add(
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
                warnings.Add(new CallWarning("unsupported", "tool " + tool.ProviderToolId, null));
            }
        }

        var preparedToolChoice = toolChoice;
        if (!usingAnthropicTools && rejectsForcedToolChoice)
        {
            warnings.Add(
                new CallWarning(
                    "unsupported",
                    "toolChoice",
                    toolChoice is ToolChoice.NamedChoice namedChoice
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
        // they keep every function tool. Other models only receive the named tool.
        var filteredFunctionTools = functionTools;
        if (
            toolChoice is ToolChoice.NamedChoice named
            && (!isAnthropicModel || rejectsForcedToolChoice)
        )
        {
            filteredFunctionTools = functionTools
                .Where(tool => string.Equals(tool.Name, named.ToolName, StringComparison.Ordinal))
                .ToList();
        }

        var selected = filteredFunctionTools;
        var supportsStrict = AmazonBedrockModelSupport.SupportsStrictTools(modelId);
        foreach (var tool in selected)
        {
            var includeStrict = false;
            if (tool.Strict != null)
            {
                if (!supportsStrict)
                {
                    if (tool.Strict.Value)
                    {
                        warnings.Add(
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
                    warnings.Add(
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
            bedrockTools.Add(new JsonObject { ["toolSpec"] = spec });
        }

        if (
            isAnthropicModel
            && !usingAnthropicTools
            && disableParallelToolUseForRequest == true
            && bedrockTools.Count > 0
            && preparedToolChoice is not { Type: "none" }
        )
        {
            var anthropicChoice = preparedToolChoice switch
            {
                { Type: "required" } => new JsonObject { ["type"] = "any" },
                ToolChoice.NamedChoice namedAnthropic => new JsonObject
                {
                    ["type"] = "tool",
                    ["name"] = namedAnthropic.ToolName,
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
            && bedrockTools.Count > 0
            && preparedToolChoice != null
        )
        {
            if (preparedToolChoice.Type == "none")
            {
                bedrockTools.Clear();
            }
            else
            {
                bedrockToolChoice = ToolChoiceObject(preparedToolChoice);
            }
        }

        if (bedrockTools.Count == 0)
        {
            return new JsonObject();
        }

        var config = new JsonObject { ["tools"] = bedrockTools };
        if (bedrockToolChoice != null)
        {
            config["toolChoice"] = bedrockToolChoice;
        }

        return config;
    }

    private static AnthropicToolDefinition ToAnthropicTool(LanguageModelTool tool) =>
        new AnthropicToolDefinition
        {
            Type = "provider",
            Name = tool.Name,
            Id = tool.ProviderToolId,
            Args = tool.Args.ValueKind == JsonValueKind.Undefined ? null : tool.Args,
            ProviderOptions = tool.ProviderOptions,
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
