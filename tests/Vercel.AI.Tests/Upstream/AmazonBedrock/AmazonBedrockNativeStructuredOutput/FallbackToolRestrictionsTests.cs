// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockNativeStructuredOutput.ConverseHarness;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockNativeStructuredOutput;

public sealed class FallbackToolRestrictionsTests
{
    private const string File =
        "packages/amazon-bedrock/src/amazon-bedrock-native-structured-output.test.ts::";
    private const string RestrictionsPrefix =
        File + "Claude 5 structured outputs through Converse > %s fallback tool restrictions::";

    public static TheoryData<string, string, string?, string> ToolChoices =>
        new()
        {
            { "doGenerate", "none", null, "json" },
            { "doGenerate", "tool", "lookup", "lookup,json" },
            { "doGenerate", "auto", null, "lookup,unused,json" },
            { "doGenerate", "required", null, "lookup,unused,json" },
            { "doStream", "none", null, "json" },
            { "doStream", "tool", "lookup", "lookup,json" },
            { "doStream", "auto", null, "lookup,unused,json" },
            { "doStream", "required", null, "lookup,unused,json" },
        };

    [Theory]
    [MemberData(nameof(ToolChoices))]
    [UpstreamTest(
        RestrictionsPrefix + "preserves caller tool choice %j",
        Coverage = UpstreamCoverage.Covered,
        Commit = "2cd0a97424762b59d9979b1bf2f55060bf443fb2"
    )]
    public async Task Preserves_caller_tool_choice(
        string method,
        string choiceType,
        string? toolName,
        string expectedNames
    )
    {
        var options = Call(OpenObject);
        options.ToolChoice = choiceType switch
        {
            "none" => ToolChoice.None,
            "tool" => ToolChoice.Tool(toolName!),
            "required" => ToolChoice.Required,
            _ => ToolChoice.Auto,
        };
        options.Tools =
        [
            new LanguageModelTool("lookup", null, Json(Schema)),
            new LanguageModelTool("unused", null, Json(Schema)),
        ];

        var body = await Send("anthropic.claude-opus-5", options, method);

        Assert.Null(body["additionalModelRequestFields"]?["output_config"]);
        Assert.Equal(
            expectedNames.Split(','),
            body["toolConfig"]!["tools"]!
                .AsArray()
                .Select(tool => tool!["toolSpec"]!["name"]!.GetValue<string>())
        );
    }
}
