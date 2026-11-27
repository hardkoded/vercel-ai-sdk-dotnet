// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; function tool</c>.</summary>
public sealed class DoGenerateFunctionToolTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > function tool::";

    [Fact]
    [UpstreamTest(Prefix + "should extract tool calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_tool_calls()
    {
        var handler = AnthropicParity.Ok(
            "{\"id\":\"msg_017TfcQ4AgGxKyBduUpqYPZn\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"Some text\\n\\n\"},{\"type\":\"tool_use\",\"id\":\"toolu_1\",\"name\":\"test-tool\",\"input\":{\"value\":\"example value\"}}],\"model\":\"claude-3-haiku-20240307\",\"stop_reason\":\"tool_use\",\"stop_sequence\":null,\"usage\":{\"input_tokens\":4,\"output_tokens\":30}}"
        );
        var options = AnthropicParity.Hello();
        options.Tools = new[]
        {
            new LanguageModelTool(
                "test-tool",
                null,
                AnthropicParity.Json(
                    """
                    {
                      "type": "object",
                      "properties": { "value": { "type": "string" } },
                      "required": ["value"],
                      "additionalProperties": false,
                      "$schema": "http://json-schema.org/draft-07/schema#"
                    }
                    """
                )
            ),
        };

        var result = await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-3-haiku-20240307",
            options
        );

        LanguageModelTestSupport.AssertJson(
            """
            [
              { "type": "text", "text": "Some text\n\n" },
              {
                "type": "tool-call",
                "input": "{\"value\":\"example value\"}",
                "toolCallId": "toolu_1",
                "toolName": "test-tool"
              }
            ]
            """,
            LanguageModelTestSupport.Snapshot(result.Content)
        );
        Assert.Equal("tool_use", result.RawFinishReason);
        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should support tools with empty parameters",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_support_tools_with_empty_parameters()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-tool-no-args");
        var options = AnthropicParity.Hello();
        options.Tools = new[]
        {
            new LanguageModelTool(
                "test-tool",
                null,
                AnthropicParity.Json(
                    """
                    {
                      "type": "object",
                      "properties": {},
                      "required": [],
                      "additionalProperties": false,
                      "$schema": "http://json-schema.org/draft-07/schema#"
                    }
                    """
                )
            ),
        };

        var result = await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-3-haiku-20240307",
            options
        );

        LanguageModelTestSupport.AssertJson(
            """
            [
              {
                "type": "text",
                "text": "<thinking>\nThe updateIssueList tool was provided in the list of available functions. The tool has no required parameters, so it can be called without any additional information needed from the user.\n</thinking>\n\nOkay, I will update the current issue list:"
              },
              {
                "type": "tool-call",
                "input": "{}",
                "toolCallId": "toolu_01LRmxn9vGM1d2DZSDBowdZ1",
                "toolName": "updateIssueList"
              }
            ]
            """,
            LanguageModelTestSupport.Snapshot(result.Content)
        );
    }
}
