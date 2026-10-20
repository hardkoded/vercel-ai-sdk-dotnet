// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; doGenerate &gt; shell tool with container multiturn</c>.</summary>
public sealed class DoGenerateShellToolWithContainerMultiturnTests
{
    private const string Id = "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > shell tool with container multiturn::";

    private static LanguageModelCallOptions Options()
    {
        var call = new GeneratedToolCall(
            "call_abc123def456ghi789jkl012",
            "shell",
            """{"action":{"commands":["uname -a"]}}""",
            OpenAIUpstream.Json("""{"openai":{"itemId":"sh_0f1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b1c2d3e50"}}"""))
        {
            ProviderExecuted = true,
        };
        var output = new ToolModelMessage(
            "call_abc123def456ghi789jkl012",
            "shell",
            """{"output":[{"stdout":"Linux container-host 6.1.0 #1 SMP x86_64 GNU/Linux\n","stderr":"","outcome":{"type":"exit","exitCode":0}}]}""",
            false,
            "json");
        var text = new AssistantTextPart(
            "Linux container-host 6.1.0 #1 SMP x86_64 GNU/Linux",
            OpenAIUpstream.OpenAIOptionsJson("""{"itemId":"msg_0f1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b1c2d3e52"}"""));

        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage("Run uname -a"),
                new AssistantModelMessage(new AssistantContentPart[]
                {
                    new AssistantToolCallPart(call),
                    new AssistantToolResultPart("call_abc123def456ghi789jkl012", "shell", output),
                    text,
                }),
                new UserModelMessage("What architecture do you run in?"),
            },
            Tools = new[]
            {
                LanguageModelTool.ProviderDefined("openai.shell", "shell", OpenAIUpstream.Json("""{"environment":{"type":"containerAuto"}}""")),
            },
        };
    }

    [Fact]
    [UpstreamTest(Id + "should send request body with shell history converted to shell_call and shell_call_output", Coverage = UpstreamCoverage.Covered)]
    public void Should_send_request_body_with_shell_history_converted_to_shell_call_and_shell_call_output()
    {
        var request = OpenAIResponsesLanguageModel.Prepare("gpt-5.2", Options(), false);

        var expected = JsonNode.Parse(
            """
            {
              "input": [
                {"content":[{"text":"Run uname -a","type":"input_text"}],"role":"user"},
                {"id":"sh_0f1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b1c2d3e50","type":"item_reference"},
                {
                  "call_id": "call_abc123def456ghi789jkl012",
                  "output": [{
                    "outcome": {"exit_code":0,"type":"exit"},
                    "stderr": "",
                    "stdout": "Linux container-host 6.1.0 #1 SMP x86_64 GNU/Linux\n"
                  }],
                  "type": "shell_call_output"
                },
                {"content":"Linux container-host 6.1.0 #1 SMP x86_64 GNU/Linux","role":"assistant"},
                {"content":[{"text":"What architecture do you run in?","type":"input_text"}],"role":"user"}
              ],
              "model": "gpt-5.2",
              "tools": [{"environment":{"type":"container_auto"},"type":"shell"}]
            }
            """);
        Assert.True(JsonNode.DeepEquals(expected, request.Body), request.Body.ToJsonString());
    }

    [Fact]
    [UpstreamTest(Id + "should return text content from the follow-up response", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_text_content_from_the_follow_up_response()
    {
        var capture = new OpenAICapture { ResponseJson = OpenAIResponsesSupport.Fixture("openai-shell-container-multiturn.1.json") };

        var result = await OpenAIUpstream.Provider(capture).ResponsesModel("gpt-5.2").DoGenerateAsync(Options(), CancellationToken.None);

        var text = Assert.IsType<OpenAIText>(Assert.Single(result.Content));
        Assert.Equal("`x86_64` (64-bit x86 / AMD64).", text.Text);
        JsonAssert.Equal(text.ProviderMetadata, """{"openai":{"itemId":"msg_0fc28e14d2bb7565006994e621f78481919e9fa42a95ce6b6c"}}""");
    }
}
