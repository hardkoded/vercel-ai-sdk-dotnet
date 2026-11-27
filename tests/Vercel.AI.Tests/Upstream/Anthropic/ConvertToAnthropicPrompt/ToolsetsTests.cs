// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using static Vercel.AI.Tests.ConvertToAnthropicPrompt.PromptRunner;

namespace Vercel.AI.Tests.ConvertToAnthropicPrompt;

/// <summary>Port of <c>convert-to-anthropic-prompt.test.ts</c> &gt; <c>toolsets</c>.</summary>
public sealed class ToolsetsTests
{
    private const string Prefix =
        "packages/anthropic/src/convert-to-anthropic-prompt.test.ts::toolsets::";

    // The tool name mapping of the computer toolset: the custom name `computer` maps to the provider tool `computer`.
    private static readonly Dictionary<string, string> ToolsetToolNameMapping = new()
    {
        ["computer"] = "computer",
    };

    [Fact]
    [UpstreamTest(
        Prefix + "should serialize toolset tool calls and results with toolset_name",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_serialize_toolset_tool_calls_and_results_with_toolset_name()
    {
        var result = Run(
            "[{\"role\":\"assistant\",\"content\":[{\"type\":\"tool-call\",\"toolCallId\":\"toolu_click\",\"toolName\":\"computer\",\"input\":{\"action\":\"left_click\",\"coordinate\":[640,60]}}]},"
                + "{\"role\":\"tool\",\"content\":[{\"type\":\"tool-result\",\"toolCallId\":\"toolu_click\",\"toolName\":\"computer\",\"output\":{\"type\":\"text\",\"value\":\"OK\"}}]}]",
            providerToolNames: ToolsetToolNameMapping,
            toolsetNames: new Dictionary<string, string> { ["computer"] = "computer" }
        );

        AssertJson(
            "[{\"content\":[{\"id\":\"toolu_click\",\"input\":{\"action\":\"left_click\",\"coordinate\":[640,60]},\"name\":\"left_click\",\"toolset_name\":\"computer\",\"type\":\"tool_use\"}],\"role\":\"assistant\"},"
                + "{\"content\":[{\"content\":\"OK\",\"tool_use_id\":\"toolu_click\",\"toolset_name\":\"computer\",\"type\":\"tool_result\"}],\"role\":\"user\"}]",
            result.Messages
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should detect toolset tool calls through provider metadata when the tool is not passed",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_detect_toolset_tool_calls_through_provider_metadata_when_the_tool_is_not_passed()
    {
        var result = Run(
            "[{\"role\":\"assistant\",\"content\":[{\"type\":\"tool-call\",\"toolCallId\":\"toolu_screenshot\",\"toolName\":\"desktop\",\"input\":{\"action\":\"screenshot\"},\"providerOptions\":{\"anthropic\":{\"toolsetName\":\"computer\"}}}]},"
                + "{\"role\":\"tool\",\"content\":[{\"type\":\"tool-result\",\"toolCallId\":\"toolu_screenshot\",\"toolName\":\"desktop\",\"output\":{\"type\":\"text\",\"value\":\"OK\"},\"providerOptions\":{\"anthropic\":{\"toolsetName\":\"computer\"}}}]}]"
        );

        AssertJson(
            "[{\"type\":\"tool_use\",\"id\":\"toolu_screenshot\",\"name\":\"screenshot\",\"toolset_name\":\"computer\",\"input\":{\"action\":\"screenshot\"}}]",
            result.Messages[0]!["content"]
        );
        AssertJson(
            "[{\"type\":\"tool_result\",\"tool_use_id\":\"toolu_screenshot\",\"toolset_name\":\"computer\",\"content\":\"OK\"}]",
            result.Messages[1]!["content"]
        );
    }

    [Theory]
    [InlineData("{\"coordinate\":[1,2]}", "computer")]
    [InlineData("{\"action\":null,\"coordinate\":[1,2]}", "computer")]
    [InlineData("\"invalid JSON\"", "computer")]
    [InlineData("{\"action\":null}", "desktop")]
    [UpstreamTest(
        Prefix + "should retain malformed toolset calls and their existing error results: %j",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_retain_malformed_toolset_calls_and_their_existing_error_results(
        string input,
        string toolName
    )
    {
        var result = Run(
            "[{\"role\":\"assistant\",\"content\":[{\"type\":\"tool-call\",\"toolCallId\":\"toolu_bad\",\"toolName\":\""
                + toolName
                + "\",\"input\":"
                + input
                + "}]},"
                + "{\"role\":\"tool\",\"content\":[{\"type\":\"tool-result\",\"toolCallId\":\"toolu_bad\",\"toolName\":\""
                + toolName
                + "\",\"output\":{\"type\":\"error-text\",\"value\":\"Invalid input for tool computer\"}}]}]",
            providerToolNames: ToolsetToolNameMapping,
            toolsetNames: new Dictionary<string, string> { [toolName] = "computer" }
        );

        var expectedInput = input.StartsWith('"') ? "{\"rawInvalidInput\":" + input + "}" : input;
        AssertJson(
            "[{\"role\":\"assistant\",\"content\":[{\"type\":\"tool_use\",\"id\":\"toolu_bad\",\"name\":\"computer\",\"toolset_name\":\"computer\",\"input\":"
                + expectedInput
                + "}]},{\"role\":\"user\",\"content\":[{\"type\":\"tool_result\",\"tool_use_id\":\"toolu_bad\",\"toolset_name\":\"computer\",\"content\":\"Invalid input for tool computer\",\"is_error\":true}]}]",
            result.Messages
        );
        AssertWarnings(result, "toolset tool call for tool " + toolName + " is missing the action");
    }
}
