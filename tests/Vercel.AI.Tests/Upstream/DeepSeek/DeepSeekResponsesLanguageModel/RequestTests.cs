// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.DeepSeek;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekResponsesLanguageModel.DeepSeekResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekResponsesLanguageModel;

/// <summary>Port of <c>deepseek-responses-language-model.test.ts</c> &gt; <c>DeepSeekResponsesLanguageModel &gt; request</c>.</summary>
public sealed class RequestTests
{
    private const string Prefix = TestFile + " > request::";

    [Fact]
    [UpstreamTest(Prefix + "sends a Responses request with mapped options", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Sends_a_Responses_request_with_mapped_options()
    {
        var capture = JsonResponse();
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new SystemModelMessage("Be brief."), new UserModelMessage("Hello") },
            MaxOutputTokens = 100,
            Temperature = 0.5,
            StopSequences = new[] { "END" },
            JsonResponse = true,
            JsonSchemaName = "answer",
            JsonSchema = UpstreamChat.Json("""{"type":"object","properties":{"a":{"type":"string"}}}"""),
            Tools = new[]
            {
                WebSearchLanguageModelTool(),
                new LanguageModelTool("lookup", "Look something up", UpstreamChat.Json("""{"type":"object","properties":{}}""")),
            },
            ToolChoice = ToolChoice.Required,
            ProviderOptions = UpstreamChat.Bag("deepseek", """{"reasoningEffort":"max","topLogprobs":2,"userId":"u_1"}"""),
        };

        var result = await Model(capture, "deepseek-v4-pro").DoGenerateAsync(options, CancellationToken.None);

        JsonAssert.Equal(RequestBody(capture), """
            {"model":"deepseek-v4-pro",
             "input":[{"role":"system","content":"Be brief."},{"role":"user","content":[{"type":"input_text","text":"Hello"}]}],
             "max_output_tokens":100,"temperature":0.5,"top_logprobs":2,"reasoning":{"effort":"max"},
             "text":{"format":{"type":"json_schema","name":"answer","schema":{"type":"object","properties":{"a":{"type":"string"}}}}},
             "tools":[{"type":"web_search"},{"type":"function","name":"lookup","description":"Look something up","parameters":{"type":"object","properties":{}}}],
             "tool_choice":"required","user":"u_1"}
            """);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("stopSequences", warning.Feature);
    }

    [Fact]
    [UpstreamTest(Prefix + "disables thinking with reasoning effort none", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Disables_thinking_with_reasoning_effort_none()
    {
        var capture = JsonResponse();
        var options = TestPrompt();
        options.ProviderOptions = UpstreamChat.Bag("deepseek", """{"thinking":{"type":"disabled"}}""");

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        JsonAssert.Equal(RequestBody(capture)["reasoning"], """{"effort":"none"}""");
    }

    [Fact]
    [UpstreamTest(Prefix + "maps top-level reasoning to DeepSeek effort", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Maps_top_level_reasoning_to_DeepSeek_effort()
    {
        var capture = JsonResponse();
        var options = TestPrompt();
        options.Reasoning = "medium";

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        JsonAssert.Equal(RequestBody(capture)["reasoning"], """{"effort":"high"}""");
    }

    [Fact]
    [UpstreamTest(Prefix + "replays reasoning, function calls, web search calls and tool outputs", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Replays_reasoning_function_calls_web_search_calls_and_tool_outputs()
    {
        var capture = JsonResponse();
        var deepseek = UpstreamChat.Json("{\"itemId\":\"rs_1\"}");
        var options = TestPrompt();
        options.Prompt = new ModelMessage[]
        {
            new UserModelMessage("Hello"),
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantReasoningPart("thinking", new Dictionary<string, JsonElement> { ["deepseek"] = deepseek }),
                new AssistantToolCallPart(
                    new GeneratedToolCall("ws_1", "web_search", "{}") { ProviderExecuted = true },
                    UpstreamChat.Bag("deepseek", "{\"webSearchCall\":" + WebSearchCall + "}")),
                new AssistantToolResultPart("ws_1", "web_search", new ToolModelMessage("ws_1", "web_search", "{}", false, "json")),
                new AssistantToolCallPart(new GeneratedToolCall("call_1", "lookup", """{"q":"x"}""")),
            }),
            new ToolModelMessage("call_1", "lookup", """{"ok":true}""", false, "json"),
        };

        await Model(capture, "deepseek-v4-pro").DoGenerateAsync(options, CancellationToken.None);

        JsonAssert.Equal(RequestBody(capture)["input"], """
            [{"role":"user","content":[{"type":"input_text","text":"Hello"}]},
             {"type":"reasoning","id":"rs_1","content":[{"type":"reasoning_text","text":"thinking"}]},
             __WS__,
             {"type":"function_call","call_id":"call_1","name":"lookup","arguments":"{\"q\":\"x\"}"},
             {"type":"function_call_output","call_id":"call_1","output":"{\"ok\":true}"}]
            """.Replace("__WS__", WebSearchCall));
    }

    [Fact]
    [UpstreamTest(Prefix + "sends images as input_image parts", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Sends_images_as_input_image_parts()
    {
        var capture = JsonResponse();
        var options = TestPrompt();
        options.Prompt = new ModelMessage[]
        {
            new UserModelMessage(new UserContentPart[]
            {
                new FileContentPart("image/png", "https://example.com/a.png", null, null)
                {
                    ProviderOptions = UpstreamChat.Bag("deepseek", """{"imageDetail":"low"}"""),
                },
                FileContentPart.FromProviderReference("image/png", new Dictionary<string, string> { ["deepseek"] = "file-api-1" }),
            }),
        };

        await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        JsonAssert.Equal(RequestBody(capture)["input"], """
            [{"role":"user","content":[
              {"type":"input_image","image_url":"https://example.com/a.png","detail":"low"},
              {"type":"input_image","file_id":"file-api-1"}]}]
            """);
    }
}
