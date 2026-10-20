// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.ConvertToOpenAIResponsesInput;

/// <summary>Port of <c>convert-to-openai-responses-input.test.ts</c> &gt; <c>convertToOpenAIResponsesInput &gt; provider tool outputs</c>.</summary>
public sealed class ProviderToolOutputsTests
{
    private const string Id = "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > provider tool outputs::";

    private static readonly ToolNameMapping Mapping = ToolNames.CreateToolNameMapping(
        new[] { new MappedTool("provider", "shell", "openai.shell") },
        new Dictionary<string, string> { ["openai.shell"] = "shell" });

    private static OpenAIResponsesLanguageModel.ResponsesInput Convert(string output) =>
        OpenAIResponsesLanguageModel.ConvertInput(
            new ModelMessage[] { new ToolModelMessage("call-shell", "shell", output, false, "json") },
            "system",
            false,
            null,
            "openai",
            false,
            true,
            toolNameMapping: Mapping);

    [Fact]
    public void Should_reject_a_non_numeric_maxOutputLength_in_a_shell_tool_result()
    {
        Assert.Throws<AiSdkException>(() => Convert("""{"maxOutputLength":"10000","output":[{"stdout":"hi\n","stderr":"","outcome":{"type":"exit","exitCode":0}}]}"""));
    }

    [Fact]
    [UpstreamTest(Id + "should include max_output_length in shell_call_output when the tool result has maxOutputLength", Coverage = UpstreamCoverage.Covered)]
    public void Should_include_max_output_length_in_shell_call_output_when_the_tool_result_has_maxOutputLength()
    {
        var result = Convert("""{"maxOutputLength":10000,"output":[{"stdout":"hi\n","stderr":"","outcome":{"type":"exit","exitCode":0}}]}""");

        var expected = JsonNode.Parse(
            """
            [{
              "type": "shell_call_output",
              "call_id": "call-shell",
              "max_output_length": 10000,
              "output": [{"stdout":"hi\n","stderr":"","outcome":{"type":"exit","exit_code":0}}]
            }]
            """);
        Assert.True(JsonNode.DeepEquals(expected, result.Input), result.Input.ToJsonString());
    }

    [Fact]
    [UpstreamTest(Id + "should omit max_output_length from shell_call_output when the tool result has no maxOutputLength", Coverage = UpstreamCoverage.Covered)]
    public void Should_omit_max_output_length_from_shell_call_output_when_the_tool_result_has_no_maxOutputLength()
    {
        var result = Convert("""{"output":[{"stdout":"hi\n","stderr":"","outcome":{"type":"exit","exitCode":0}}]}""");

        var expected = JsonNode.Parse(
            """
            [{
              "type": "shell_call_output",
              "call_id": "call-shell",
              "output": [{"stdout":"hi\n","stderr":"","outcome":{"type":"exit","exit_code":0}}]
            }]
            """);
        Assert.True(JsonNode.DeepEquals(expected, result.Input), result.Input.ToJsonString());
        Assert.False(((JsonObject)result.Input[0]!).ContainsKey("max_output_length"));
    }
}
