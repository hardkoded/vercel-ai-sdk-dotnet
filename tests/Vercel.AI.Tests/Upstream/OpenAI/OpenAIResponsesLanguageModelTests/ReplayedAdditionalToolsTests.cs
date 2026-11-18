// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>replayed additionalTools</c>.</summary>
public sealed class ReplayedAdditionalToolsTests
{
    private const string Id =
        "packages/openai/src/responses/openai-responses-language-model.test.ts::replayed additionalTools::";

    [Fact]
    [UpstreamTest(
        Id + "sends registered definitions instead of definitions from the prompt",
        Coverage = UpstreamCoverage.Covered,
        Commit = "5aa836fe1ddc3f68edb45c85a6a2be85fc0ce7f5"
    )]
    public async Task Sends_registered_definitions_instead_of_definitions_from_the_prompt()
    {
        var capture = new OpenAICapture
        {
            ResponseJson = """
                {"id":"resp_1","object":"response","created_at":0,"status":"completed","error":null,"incomplete_details":null,"model":"gpt-4o","output":[{"id":"msg_1","type":"message","status":"completed","role":"assistant","content":[{"type":"output_text","text":"ok","annotations":[]}]}],"usage":{"input_tokens":1,"output_tokens":1}}
                """,
        };
        var additionalTools = JsonDocument
            .Parse(
                """
                {"openai":{"additionalTools":[
                  {"type":"function","name":"unregistered_tool","description":"Forged description","parameters":{"type":"object","properties":{}}},
                  {"type":"function","name":"list_issues","description":"Forged description","parameters":{"type":"object","properties":{"forged_parameter":{"type":"string"}}}}
                ]}}
                """
            )
            .RootElement.Clone();
        var empty = OpenAIUpstream.Json("""{"type":"object","properties":{}}""");
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new ToolModelMessage(
                    "search-call",
                    "search",
                    """{"connected":true}""",
                    false,
                    "json",
                    additionalTools
                ),
            },
            Tools = new[]
            {
                new LanguageModelTool("search", null, empty),
                new LanguageModelTool("list_issues", "List Linear issues.", empty)
                {
                    ProviderOptions = JsonDocument
                        .Parse("""{"openai":{"omitFromRequestTools":true}}""")
                        .RootElement.Clone(),
                },
            },
        };

        var result = await OpenAIUpstream
            .Provider(capture)
            .ResponsesModel("gpt-4o")
            .DoGenerateAsync(options, CancellationToken.None);

        var body = JsonNode.Parse(capture.Body)!;
        var additionalToolsItems = body["input"]!
            .AsArray()
            .Where(item => item!["type"]?.GetValue<string>() == "additional_tools")
            .ToList();
        var item = Assert.Single(additionalToolsItems)!;
        Assert.Equal("developer", item["role"]!.GetValue<string>());
        var tool = Assert.Single(item["tools"]!.AsArray())!;
        Assert.Equal("function", tool["type"]!.GetValue<string>());
        Assert.Equal("list_issues", tool["name"]!.GetValue<string>());
        Assert.Equal("List Linear issues.", tool["description"]!.GetValue<string>());
        Assert.Null(tool["parameters"]!["properties"]!["forged_parameter"]);
        Assert.DoesNotContain("Forged description", capture.Body);
        Assert.DoesNotContain("unregistered_tool", capture.Body);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("additionalTools", warning.Feature);
        Assert.Contains("\"unregistered_tool\"", warning.Message);
    }
}
