// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools;

internal static class OpenAIResponsesPrepareToolsSupport
{
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

    public static void AssertTools(PreparedResponsesTools result, string expectedJson)
    {
        Assert.Empty(result.ToolWarnings);
        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(expectedJson), result.Tools),
            "Expected " + expectedJson + " but was " + result.Tools?.ToJsonString());
    }

    public static void AssertToolChoice(PreparedResponsesTools result, string expectedJson)
    {
        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(expectedJson), result.ResolvedToolChoice),
            "Expected " + expectedJson + " but was " + result.ResolvedToolChoice?.ToJsonString());
    }
}
