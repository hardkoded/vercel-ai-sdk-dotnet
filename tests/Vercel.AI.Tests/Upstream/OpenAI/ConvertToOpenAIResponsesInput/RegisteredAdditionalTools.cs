// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.ConvertToOpenAIResponsesInput;

/// <summary>
/// Port of the helpers <c>resolveAdditionalToolFor</c>, <c>registerAdditionalTools</c>, <c>definitionOf</c>,
/// <c>registeredTool</c>, <c>registeredCustomTool</c> and <c>namespaceDefinitionOf</c> from
/// <c>convert-to-openai-responses-input.test.ts</c>. Conversation history is untrusted, so the converter resolves each
/// entry to a definition derived from the tools registered for the request.
/// </summary>
internal static class RegisteredAdditionalTools
{
    public static readonly (string Name, string Description) LinearNamespace = (
        "linear",
        "Linear tools"
    );

    public static readonly LanguageModelTool RegisteredCustomTool =
        LanguageModelTool.ProviderDefined(
            "openai.custom",
            "apply_patch",
            JsonDocument
                .Parse(
                    "{\"description\":\"Apply a patch\",\"format\":{\"type\":\"grammar\",\"syntax\":\"lark\",\"definition\":\"start: \\\"x\\\"\"}}"
                )
                .RootElement.Clone()
        );

    public static Func<string, string, AdditionalToolResolution?> ResolveAdditionalToolFor(
        IReadOnlyList<LanguageModelTool> tools
    ) =>
        (type, name) =>
            OpenAIResponsesPrepareTools.FindAdditionalToolDefinition(tools, (type, name));

    public static LanguageModelTool RegisteredTool(
        string name,
        string? description = null,
        bool? deferLoading = null,
        (string Name, string Description)? @namespace = null
    )
    {
        var openai = new JsonObject { ["omitFromRequestTools"] = true };
        if (deferLoading != null)
        {
            openai["deferLoading"] = deferLoading;
        }

        if (@namespace is { } ns)
        {
            openai["namespace"] = new JsonObject
            {
                ["name"] = ns.Name,
                ["description"] = ns.Description,
            };
        }

        return new LanguageModelTool(
            name,
            description,
            JsonDocument
                .Parse("{\"type\":\"object\",\"properties\":{},\"additionalProperties\":false}")
                .RootElement.Clone()
        )
        {
            ProviderOptions = JsonDocument
                .Parse(new JsonObject { ["openai"] = openai }.ToJsonString())
                .RootElement.Clone(),
        };
    }

    public static JsonNode DefinitionOf(
        IReadOnlyList<LanguageModelTool> tools,
        string type,
        string name
    ) =>
        OpenAIResponsesPrepareTools.FindAdditionalToolDefinition(tools, (type, name))?.Definition
        ?? throw new InvalidOperationException("expected a registered tool definition");

    public static JsonNode NamespaceDefinitionOf(
        IReadOnlyList<LanguageModelTool> tools,
        string name,
        IReadOnlyList<string> toolNames
    )
    {
        var definition = (JsonObject)DefinitionOf(tools, "namespace", name).DeepClone();
        var kept = new JsonArray();
        foreach (var tool in definition["tools"]!.AsArray())
        {
            if (toolNames.Contains(tool!["name"]!.GetValue<string>()))
            {
                kept.Add(tool.DeepClone());
            }
        }

        definition["tools"] = kept;
        return definition;
    }
}
