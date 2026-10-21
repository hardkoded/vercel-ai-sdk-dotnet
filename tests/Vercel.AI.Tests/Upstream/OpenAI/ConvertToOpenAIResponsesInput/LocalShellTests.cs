// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.ConvertToOpenAIResponsesInput;

/// <summary>Port of <c>convert-to-openai-responses-input.test.ts</c> &gt; <c>convertToOpenAIResponsesInput &gt; provider-defined tools &gt; local shell</c>.</summary>
public sealed class LocalShellTests
{
    private const string Id = "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > provider-defined tools > local shell::";

    private static readonly ToolNameMapping Mapping = ToolNames.CreateToolNameMapping(
        new[] { new MappedTool("provider", "local_shell", "openai.local_shell") },
        new Dictionary<string, string> { ["openai.local_shell"] = "local_shell" });

    private static OpenAIResponsesLanguageModel.ResponsesInput Convert(bool store)
    {
        var messages = new List<ModelMessage>
        {
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantToolCallPart(new GeneratedToolCall(
                    "call_XWgeTylovOiS8xLNz2TONOgO",
                    "local_shell",
                    """{"action":{"type":"exec","command":["ls"]}}""",
                    OpenAIUpstream.Json("""{"openai":{"itemId":"lsh_68c2e2cf522c81908f3e2c1bccd1493b0b24aae9c6c01e4f"}}"""))),
            }),
            new ToolModelMessage("call_XWgeTylovOiS8xLNz2TONOgO", "local_shell", """{"output":"example output"}""", false, "json"),
        };

        return OpenAIResponsesLanguageModel.ConvertInput(messages, "system", false, null, "openai", false, store, toolNameMapping: Mapping, hasLocalShellTool: true);
    }

    private static void AssertInput(string expected, OpenAIResponsesLanguageModel.ResponsesInput result) =>
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), result.Input), result.Input.ToJsonString());

    [Fact]
    [UpstreamTest(Id + "should convert local shell tool call and result into item reference with store: true", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_local_shell_tool_call_and_result_into_item_reference_with_store_true()
    {
        AssertInput(
            """
            [
              {"id":"lsh_68c2e2cf522c81908f3e2c1bccd1493b0b24aae9c6c01e4f","type":"item_reference"},
              {"call_id":"call_XWgeTylovOiS8xLNz2TONOgO","output":"example output","type":"local_shell_call_output"}
            ]
            """,
            Convert(true));
    }

    [Fact]
    [UpstreamTest(Id + "should convert local shell tool call and result into item reference with store: false", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_local_shell_tool_call_and_result_into_item_reference_with_store_false()
    {
        AssertInput(
            """
            [
              {
                "action":{"command":["ls"],"type":"exec"},
                "call_id":"call_XWgeTylovOiS8xLNz2TONOgO",
                "id":"lsh_68c2e2cf522c81908f3e2c1bccd1493b0b24aae9c6c01e4f",
                "type":"local_shell_call"
              },
              {"call_id":"call_XWgeTylovOiS8xLNz2TONOgO","output":"example output","type":"local_shell_call_output"}
            ]
            """,
            Convert(false));
    }
}
