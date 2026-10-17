// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; doGenerate &gt; custom tool</c>.</summary>
public sealed class DoGenerateCustomToolTests
{
    private const string Id = "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > custom tool::";

    [Fact]
    [UpstreamTest(Id + "should send request body with custom tool", Coverage = UpstreamCoverage.Covered)]
    public void Should_send_request_body_with_custom_tool()
    {
        using var args = JsonDocument.Parse(
            """{"description":"Write a SQL SELECT query to answer the user question.","format":{"type":"grammar","syntax":"regex","definition":"SELECT .+"}}""");
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
            Tools = new[] { LanguageModelTool.ProviderDefined("openai.custom", "write_sql", args.RootElement.Clone()) },
        };

        var request = OpenAIResponsesLanguageModel.Prepare("gpt-5.2-codex", options, false);

        var expected = JsonNode.Parse(
            """
            {
              "input": [{"role":"user","content":[{"type":"input_text","text":"Hello"}]}],
              "model": "gpt-5.2-codex",
              "tools": [{
                "type": "custom",
                "name": "write_sql",
                "description": "Write a SQL SELECT query to answer the user question.",
                "format": {"type":"grammar","syntax":"regex","definition":"SELECT .+"}
              }]
            }
            """);
        Assert.True(JsonNode.DeepEquals(expected, request.Body), request.Body.ToJsonString());
    }
}
