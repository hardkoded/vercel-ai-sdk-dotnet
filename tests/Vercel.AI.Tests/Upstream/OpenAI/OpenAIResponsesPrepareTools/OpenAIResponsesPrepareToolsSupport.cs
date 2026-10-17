// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools;

internal static class OpenAIResponsesPrepareToolsSupport
{
    public const string Suite = "packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools";

    public const string FunctionToolJson = "{\"type\":\"function\",\"name\":\"testFunction\",\"description\":\"A test function\",\"parameters\":{\"type\":\"object\",\"properties\":{\"input\":{\"type\":\"string\"}}},\"strict\":false}";

    public static LanguageModelTool FunctionTool()
    {
        using var document = JsonDocument.Parse("{\"type\":\"object\",\"properties\":{\"input\":{\"type\":\"string\"}}}");
        return new LanguageModelTool("testFunction", "A test function", document.RootElement.Clone());
    }

    public static LanguageModelTool ProviderTool(string id, string name, string argsJson)
    {
        using var document = JsonDocument.Parse(argsJson);
        return LanguageModelTool.ProviderDefined(id, name, document.RootElement.Clone());
    }

    public static PreparedResponsesTools Prepare(ToolChoice? toolChoice, params LanguageModelTool[] tools)
    {
        return OpenAIResponsesLanguageModel.PrepareResponsesTools(tools, toolChoice);
    }

    public static void AssertTools(PreparedResponsesTools result, string expectedJson, bool expectedWarnings = false)
    {
        if (!expectedWarnings)
        {
            Assert.Empty(result.ToolWarnings);
        }

        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(expectedJson), result.Tools),
            "Expected " + expectedJson + " but was " + result.Tools?.ToJsonString());
    }

    public static void AssertToolChoice(PreparedResponsesTools result, string expectedJson)
    {
        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(expectedJson), result.ToolChoice),
            "Expected " + expectedJson + " but was " + result.ToolChoice?.ToJsonString());
    }

    public static LanguageModelTool Function(string name, string? description, string schemaJson = "{\"type\":\"object\",\"properties\":{}}", string? openaiOptionsJson = null, bool? strict = null)
    {
        using var schema = JsonDocument.Parse(schemaJson);
        var tool = new LanguageModelTool(name, description, schema.RootElement.Clone(), strict);
        if (openaiOptionsJson == null)
        {
            return tool;
        }

        using var options = JsonDocument.Parse("{\"openai\":" + openaiOptionsJson + "}");
        return new LanguageModelTool(name, description, schema.RootElement.Clone(), strict) { ProviderOptions = options.RootElement.Clone() };
    }

    public static PreparedResponsesTools Prepare(PrepareResponsesToolsOptions options, ToolChoice? toolChoice, params LanguageModelTool[] tools)
    {
        return OpenAIResponsesLanguageModel.PrepareResponsesTools(tools, toolChoice, options);
    }

    public static PrepareResponsesToolsOptions Allowed(string? mode, string[] names, ToolNameMapping? mapping = null)
    {
        return new PrepareResponsesToolsOptions { AllowedTools = new OpenAIResponsesAllowedTools(names, mode), ToolNameMapping = mapping };
    }

    /// <summary>Builds the name mapping the language model builds from its provider tool names for <paramref name="tools"/>.</summary>
    public static ToolNameMapping Mapping(LanguageModelTool[] tools, params (string Id, string Name)[] providerToolNames)
    {
        return ToolNames.CreateToolNameMapping(
            tools.Select(tool => tool.IsProviderDefined ? new MappedTool("provider", tool.Name, tool.ProviderToolId) : new MappedTool("function", tool.Name)).ToArray(),
            providerToolNames.ToDictionary(entry => entry.Id, entry => entry.Name));
    }

    public static void AssertWarnings(PreparedResponsesTools result, params (string Type, string Feature, string Details)[] expected)
    {
        Assert.Equal(
            expected,
            result.ToolWarnings.Select(warning => (warning.Type, warning.Feature ?? string.Empty, warning.Message)).ToArray());
    }

    public static void AssertNoToolChoice(PreparedResponsesTools result)
    {
        Assert.Null(result.ToolChoice);
    }
}
