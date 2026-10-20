// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.ConvertToOpenAIResponsesInput;

/// <summary>Port of <c>convert-to-openai-responses-input.test.ts</c> &gt; <c>convertToOpenAIResponsesInput &gt; provider-defined tools &gt; provider-executed shell</c>.</summary>
public sealed class ProviderExecutedShellTests
{
    private const string Id = "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > provider-defined tools > provider-executed shell::";

    private static readonly ToolNameMapping Mapping = ToolNames.CreateToolNameMapping(
        new[] { new MappedTool("provider", "shell", "openai.shell") },
        new Dictionary<string, string> { ["openai.shell"] = "shell" });

    private static OpenAIResponsesLanguageModel.ResponsesInput Convert(string input, string output, string? firstUserText)
    {
        var call = new GeneratedToolCall(
            "call_shell",
            "shell",
            input,
            OpenAIUpstream.Json("""{"openai":{"itemId":"shell_item"}}"""))
        {
            ProviderExecuted = true,
        };
        var result = new ToolModelMessage("call_shell", "shell", output, false, "json");
        var messages = new List<ModelMessage>();
        if (firstUserText != null)
        {
            messages.Add(new UserModelMessage(firstUserText));
        }

        messages.Add(new AssistantModelMessage(new AssistantContentPart[]
        {
            new AssistantToolCallPart(call),
            new AssistantToolResultPart("call_shell", "shell", result),
            new AssistantTextPart("hello"),
        }));
        messages.Add(new UserModelMessage("What did the command print?"));

        return OpenAIResponsesLanguageModel.ConvertInput(messages, "system", false, null, "openai", false, false, toolNameMapping: Mapping);
    }

    [Fact]
    [UpstreamTest(Id + "should reconstruct the shell call and output with store: false", Coverage = UpstreamCoverage.Covered)]
    public void Should_reconstruct_the_shell_call_and_output_with_store_false()
    {
        var result = Convert(
            """{"action":{"commands":["printf hello"]}}""",
            """{"output":[{"stdout":"hello","stderr":"","outcome":{"type":"exit","exitCode":0}}]}""",
            "Run `printf hello` using the shell tool.");

        Assert.Contains(result.Input, item => JsonNode.DeepEquals(item, JsonNode.Parse(
            """{"type":"shell_call","call_id":"call_shell","id":"shell_item","status":"completed","action":{"commands":["printf hello"]}}""")));
        Assert.Contains(result.Input, item => JsonNode.DeepEquals(item, JsonNode.Parse(
            """{"type":"shell_call_output","call_id":"call_shell","output":[{"stdout":"hello","stderr":"","outcome":{"type":"exit","exit_code":0}}]}""")));
    }

    [Fact]
    [UpstreamTest(Id + "should round-trip maxOutputLength as max_output_length with store: false", Coverage = UpstreamCoverage.Covered)]
    public void Should_round_trip_maxOutputLength_as_max_output_length_with_store_false()
    {
        var call = new GeneratedToolCall(
            "call_shell",
            "shell",
            """{"action":{"commands":["printf hello"],"timeoutMs":1000,"maxOutputLength":10000}}""",
            OpenAIUpstream.Json("""{"openai":{"itemId":"shell_item"}}"""))
        {
            ProviderExecuted = true,
        };
        var output = new ToolModelMessage(
            "call_shell",
            "shell",
            """{"maxOutputLength":10000,"output":[{"stdout":"hello","stderr":"","outcome":{"type":"exit","exitCode":0}}]}""",
            false,
            "json");

        var result = OpenAIResponsesLanguageModel.ConvertInput(
            new ModelMessage[]
            {
                new AssistantModelMessage(new AssistantContentPart[]
                {
                    new AssistantToolCallPart(call),
                    new AssistantToolResultPart("call_shell", "shell", output),
                }),
            },
            "system",
            false,
            null,
            "openai",
            false,
            false,
            toolNameMapping: Mapping);

        var expected = JsonNode.Parse(
            """
            [
              {
                "action": {"commands":["printf hello"],"max_output_length":10000,"timeout_ms":1000},
                "call_id": "call_shell",
                "id": "shell_item",
                "status": "completed",
                "type": "shell_call"
              },
              {
                "call_id": "call_shell",
                "max_output_length": 10000,
                "output": [{"outcome":{"exit_code":0,"type":"exit"},"stderr":"","stdout":"hello"}],
                "type": "shell_call_output"
              }
            ]
            """);
        Assert.True(JsonNode.DeepEquals(expected, result.Input), result.Input.ToJsonString());
    }
}
