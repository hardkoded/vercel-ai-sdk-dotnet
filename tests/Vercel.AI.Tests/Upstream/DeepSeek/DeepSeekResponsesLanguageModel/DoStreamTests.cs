// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekResponsesLanguageModel.DeepSeekResponsesLanguageModelSupport;
using XaiSupport = Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekResponsesLanguageModel;

/// <summary>Port of <c>deepseek-responses-language-model.test.ts</c> &gt; <c>DeepSeekResponsesLanguageModel &gt; doStream</c>.</summary>
public sealed class DoStreamTests
{
    private const string Prefix = TestFile + " > doStream::";

    [Fact]
    [UpstreamTest(Prefix + "streams a recorded reasoning response", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Streams_a_recorded_reasoning_response()
    {
        var capture = ChunksFixture("deepseek-responses");

        var result = await Model(capture).DoStreamAsync(ReasoningPrompt(), CancellationToken.None);
        var parts = await UpstreamChat.Read(result.Stream);

        var actual = XaiSupport.ToJson(parts);
        Assert.True(JsonNode.DeepEquals(Snapshot("deepseek-responses.stream.snap.json"), actual), actual.ToJsonString());
    }

    [Fact]
    [UpstreamTest(Prefix + "streams reasoning, text, function calls, web search and usage", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Streams_reasoning_text_function_calls_web_search_and_usage()
    {
        var capture = StreamResponse(
            """{"type":"response.created","response":{"id":"resp_1","created_at":1793084908,"model":"deepseek-v4-pro","status":"in_progress","output":[],"usage":null}}""",
            """{"type":"response.output_item.added","output_index":0,"item":{"type":"reasoning","id":"rs_1","content":[]}}""",
            """{"type":"response.reasoning_text.delta","item_id":"rs_1","delta":"Hmm"}""",
            """{"type":"response.output_item.done","output_index":0,"item":{"type":"reasoning","id":"rs_1","content":[{"type":"reasoning_text","text":"Hmm"}]}}""",
            """{"type":"response.web_search_call.completed","item_id":"ws_1"}""",
            """{"type":"response.output_item.done","output_index":1,"item":""" + WebSearchCall + "}",
            """{"type":"response.output_item.added","output_index":2,"item":{"type":"message","id":"msg_1","content":[]}}""",
            """{"type":"response.output_text.delta","item_id":"msg_1","delta":"Hi","logprobs":[]}""",
            """{"type":"response.output_item.done","output_index":2,"item":{"type":"message","id":"msg_1","content":[]}}""",
            """{"type":"response.output_item.added","output_index":3,"item":{"type":"function_call","id":"fc_1","call_id":"call_1","name":"lookup","arguments":""}}""",
            """{"type":"response.function_call_arguments.delta","item_id":"fc_1","delta":"{\"q\":"}""",
            """{"type":"response.function_call_arguments.delta","item_id":"fc_1","delta":"\"x\"}"}""",
            """{"type":"response.output_item.done","output_index":3,"item":{"type":"function_call","id":"fc_1","call_id":"call_1","name":"lookup","arguments":"{\"q\":\"x\"}"}}""",
            """{"type":"response.completed","response":{"id":"resp_1","status":"completed","output":[],"usage":{"input_tokens":10,"output_tokens":5,"total_tokens":15,"server_tool_use":{"tool_search":1,"tool_fetch":0}}}}""");
        var options = TestPrompt();
        options.Tools = new[] { WebSearchLanguageModelTool() };

        var result = await Model(capture, "deepseek-v4-pro").DoStreamAsync(options, CancellationToken.None);
        var parts = await UpstreamChat.Read(result.Stream);

        var actual = XaiSupport.ToJson(parts);
        Assert.True(JsonNode.DeepEquals(Snapshot("deepseek-responses-mixed.stream.snap.json"), actual), actual.ToJsonString());
        Assert.True((bool)RequestBody(capture)["stream"]!);
    }

    [Fact]
    [UpstreamTest(Prefix + "surfaces response.failed as an error", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Surfaces_response_failed_as_an_error()
    {
        var capture = StreamResponse(
            """{"type":"response.failed","response":{"id":"resp_1","status":"failed","error":{"code":"server_error","message":"boom"},"output":[]}}""");

        var result = await Model(capture).DoStreamAsync(TestPrompt(), CancellationToken.None);
        var parts = await UpstreamChat.Read(result.Stream);

        Assert.Contains(parts, part => part.Type == "error");
        var finish = Assert.IsType<FinishStreamPart>(parts[^1]);
        Assert.Equal(FinishReason.Error, finish.FinishReason);
        Assert.Equal("failed", finish.RawFinishReason);
    }
}
