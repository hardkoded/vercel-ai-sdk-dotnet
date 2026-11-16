// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Mistral;

/// <summary>Tools and tool choice ready for a Conversations request.</summary>
internal sealed class MistralPreparedConversationTools
{
    /// <summary>Creates a result.</summary>
    public MistralPreparedConversationTools(
        JsonArray? tools,
        string? toolChoice,
        IReadOnlyList<CallWarning> toolWarnings
    )
    {
        Tools = tools;
        ToolChoice = toolChoice;
        ToolWarnings = toolWarnings;
    }

    /// <summary>The <c>tools</c> array. Null when there are no tools.</summary>
    public JsonArray? Tools { get; }

    /// <summary>The <c>tool_choice</c> value: <c>auto</c>, <c>none</c>, or <c>any</c>.</summary>
    public string? ToolChoice { get; }

    /// <summary>Warnings for tools Mistral does not support.</summary>
    public IReadOnlyList<CallWarning> ToolWarnings { get; }
}

/// <summary>Prepares Conversations tools. Maps to <c>mistral-conversation-prepare-tools.ts</c>.</summary>
internal static class MistralConversationPrepareTools
{
    /// <summary>Maps provider tool ids to Mistral tool names. Maps to <c>mistralProviderToolNames</c>.</summary>
    public static readonly IReadOnlyDictionary<string, string> MistralProviderToolNames =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["mistral.web_search"] = "web_search",
            ["mistral.web_search_premium"] = "web_search_premium",
        };

    /// <summary>Builds the name mapping for the tools of a call. Maps to <c>createToolNameMapping({ tools, providerToolNames })</c>.</summary>
    public static ToolNameMapping CreateToolNameMapping(IReadOnlyList<LanguageModelTool>? tools)
    {
        return ToolNames.CreateToolNameMapping(
            tools
                ?.Select(tool => new MappedTool(
                    tool.IsProviderDefined ? "provider" : "function",
                    tool.Name,
                    tool.ProviderToolId
                ))
                .ToList(),
            MistralProviderToolNames
        );
    }

    /// <summary>Prepares the tools and tool choice. Maps to <c>prepareConversationTools</c>.</summary>
    public static MistralPreparedConversationTools PrepareConversationTools(
        IReadOnlyList<LanguageModelTool>? tools,
        ToolChoice? toolChoice = null
    )
    {
        // When the tools array is empty, treat it as undefined to prevent errors.
        if (tools is not { Count: > 0 })
        {
            return new MistralPreparedConversationTools(null, null, Array.Empty<CallWarning>());
        }

        var toolWarnings = new List<CallWarning>();
        var mistralTools = new List<JsonObject>();
        foreach (var tool in tools)
        {
            if (tool.IsProviderDefined)
            {
                if (
                    tool.ProviderToolId is { } id
                    && MistralProviderToolNames.TryGetValue(id, out var name)
                )
                {
                    mistralTools.Add(new JsonObject { ["type"] = name });
                    continue;
                }

                toolWarnings.Add(
                    new CallWarning(
                        "unsupported",
                        "provider-defined tool " + tool.ProviderToolId,
                        null
                    )
                );
            }
            else
            {
                var function = new JsonObject { ["name"] = tool.Name };
                if (tool.Description != null)
                {
                    function["description"] = tool.Description;
                }

                function["parameters"] = JsonNode.Parse(tool.InputSchema.GetRawText());
                if (tool.Strict != null)
                {
                    function["strict"] = tool.Strict;
                }

                mistralTools.Add(new JsonObject { ["type"] = "function", ["function"] = function });
            }
        }

        if (toolChoice == null)
        {
            return Result(mistralTools, null, toolWarnings);
        }

        switch (toolChoice.Type)
        {
            case "auto":
            case "none":
                return Result(mistralTools, toolChoice.Type, toolWarnings);
            case "required":
                return Result(mistralTools, "any", toolWarnings);

            // Mistral does not support tool mode directly, so the tools are filtered and the tool choice is forced through 'any'.
            case "tool":
                var toolName = CreateToolNameMapping(tools)
                    .ToProviderToolName(((ToolChoice.NamedChoice)toolChoice).ToolName);
                return Result(
                    mistralTools
                        .Where(tool =>
                            tool["type"]!.GetValue<string>() == "function"
                                ? tool["function"]!["name"]!.GetValue<string>() == toolName
                                : tool["type"]!.GetValue<string>() == toolName
                        )
                        .ToList(),
                    "any",
                    toolWarnings
                );
            default:
                throw new UnsupportedFunctionalityException(
                    "tool choice type: " + toolChoice.Type,
                    "'tool choice type: " + toolChoice.Type + "' functionality not supported."
                );
        }
    }

    private static MistralPreparedConversationTools Result(
        List<JsonObject> tools,
        string? toolChoice,
        List<CallWarning> toolWarnings
    )
    {
        return new MistralPreparedConversationTools(
            new JsonArray(tools.Cast<JsonNode?>().ToArray()),
            toolChoice,
            toolWarnings
        );
    }
}
