// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel;

/// <summary>Port of <c>xai-responses-language-model.test.ts</c> &gt; <c>XaiResponsesLanguageModel &gt; doStream &gt; tool call streaming</c>.</summary>
public sealed class DoStreamToolCallStreamingTests
{
    private const string Prefix = TestFile + " > doStream > tool call streaming::";

    [Fact]
    [UpstreamTest(Prefix + "should stream web_search tool calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_web_search_tool_calls()
    {
        var capture = StreamResponse(
            Created,
            """{"type":"response.output_item.added","item":{"type":"web_search_call","id":"ws_123","name":"web_search","arguments":"{\"query\":\"test\"}","call_id":"","status":"completed"},"output_index":0}""",
            Done);

        var parts = await Collect(Model(capture), WithTools(ProviderTool("xai.web_search", "web_search")));

        ContainsEqual(ToJson(parts), """{"type":"tool-call","toolCallId":"ws_123","toolName":"web_search","input":"{\"query\":\"test\"}","providerExecuted":true}""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should emit a tool result when provider-executed tool calls finish", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_emit_a_tool_result_when_provider_executed_tool_calls_finish()
    {
        var capture = StreamResponse(
            Created,
            """{"type":"response.output_item.added","item":{"type":"web_search_call","id":"ws_123","name":"web_search","arguments":"{\"query\":\"test\"}","call_id":"","status":"in_progress"},"output_index":0}""",
            """{"type":"response.output_item.done","item":{"type":"web_search_call","id":"ws_123","name":"web_search","arguments":"{\"query\":\"test\"}","call_id":"","status":"completed"},"output_index":0}""",
            Done);

        var parts = ToJson(await Collect(Model(capture), WithTools(ProviderTool("xai.web_search", "web_search"))));

        ContainsEqual(parts, """{"type":"tool-call","toolCallId":"ws_123","toolName":"web_search","input":"{\"query\":\"test\"}","providerExecuted":true}""");
        ContainsEqual(parts, """{"type":"tool-result","toolCallId":"ws_123","toolName":"web_search","result":{}}""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should forward web_search_call action and sources to tool-result", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_forward_web_search_call_action_and_sources_to_tool_result()
    {
        var capture = StreamResponse(
            Created,
            """{"type":"response.output_item.added","item":{"type":"web_search_call","id":"ws_action","name":"web_search","call_id":"","status":"in_progress"},"output_index":0}""",
            """
            {"type":"response.output_item.done","item":{"type":"web_search_call","id":"ws_action","name":"web_search","call_id":"","status":"completed",
              "action":{"type":"search","query":"latest AI news","sources":[{"type":"url","url":"https://example.com/a"}]}},"output_index":0}
            """.Replace("\n", string.Empty),
            Done);

        var parts = await Collect(Model(capture), WithTools(ProviderTool("xai.web_search", "web_search")));

        ContainsEqual(ToJson(parts), """
            {"type":"tool-result","toolCallId":"ws_action","toolName":"web_search",
             "result":{"action":{"type":"search","query":"latest AI news"},"sources":[{"type":"url","url":"https://example.com/a"}]}}
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should map open_page and find_in_page actions in stream tool-results", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_open_page_and_find_in_page_actions_in_stream_tool_results()
    {
        var capture = StreamResponse(
            Created,
            """{"type":"response.output_item.done","item":{"type":"web_search_call","id":"ws_open","name":"web_search","call_id":"","status":"completed","action":{"type":"open_page","url":"https://example.com/a"}},"output_index":0}""",
            """{"type":"response.output_item.done","item":{"type":"web_search_call","id":"ws_find","name":"web_search","call_id":"","status":"completed","action":{"type":"find_in_page","url":"https://example.com/a","pattern":"climate"}},"output_index":1}""",
            Done);

        var parts = ToJson(await Collect(Model(capture), WithTools(ProviderTool("xai.web_search", "web_search"))));

        ContainsEqual(parts, """{"type":"tool-result","toolCallId":"ws_open","toolName":"web_search","result":{"action":{"type":"openPage","url":"https://example.com/a"}}}""");
        ContainsEqual(parts, """
            {"type":"tool-result","toolCallId":"ws_find","toolName":"web_search","result":{"action":{"type":"findInPage","url":"https://example.com/a","pattern":"climate"}}}
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream image_generation tool calls from chunks fixture", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_image_generation_tool_calls_from_chunks_fixture()
    {
        var parts = await Collect(Model(ChunksFixture("xai-image-generation-tool.1.chunks.txt")), WithTools(ProviderTool("xai.image_generation", "image_generation")));

        JsonAssert.Equal(ToJson(parts), Expected("xai-image-generation-tool.1.stream.json").ToJsonString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream image_generation tool calls with progress events", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_image_generation_tool_calls_with_progress_events()
    {
        var capture = StreamResponse(
            Created,
            """{"type":"response.image_generation_call.in_progress","item_id":"ig_123","output_index":0}""",
            """{"type":"response.image_generation_call.generating","item_id":"ig_123","output_index":0}""",
            """{"type":"response.image_generation_call.completed","item_id":"ig_123","output_index":0}""",
            """{"type":"response.output_item.done","item":{"type":"image_generation_call","id":"ig_123","status":"completed","prompt":"An origami fox in a paper forest","result":"base64imagedata"},"output_index":0}""",
            """{"type":"response.output_text.delta","item_id":"msg_123","output_index":1,"content_index":0,"delta":"Here is your origami fox."}""",
            Done);

        var parts = await Collect(Model(capture), WithTools(ProviderTool("xai.image_generation", "image_generation")));

        JsonAssert.Equal(ToJson(parts), """
            [{"type":"stream-start","warnings":[]},{"id":"resp_123","modelId":"grok-4-fast-non-reasoning","type":"response-metadata"},{"id":"ig_123","toolName":"image_generation","type":"tool-input-start"},{"delta":"{}","id":"ig_123","type":"tool-input-delta"},{"id":"ig_123","type":"tool-input-end"},{"input":"{}","providerExecuted":true,"toolCallId":"ig_123","toolName":"image_generation","type":"tool-call"},{"result":{"prompt":"An origami fox in a paper forest","result":"base64imagedata"},"toolCallId":"ig_123","toolName":"image_generation","type":"tool-result"},{"id":"text-msg_123","type":"text-start"},{"delta":"Here is your origami fox.","id":"text-msg_123","type":"text-delta"},{"id":"text-msg_123","type":"text-end"},{"finishReason":{"raw":"completed","unified":"stop"},"type":"finish","usage":{"inputTokens":{"cacheRead":0,"noCache":10,"total":10},"outputTokens":{"reasoning":0,"text":5,"total":5},"raw":{"input_tokens":10,"output_tokens":5}}}]
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream image_generation tool calls when only output items are emitted", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_image_generation_tool_calls_when_only_output_items_are_emitted()
    {
        var capture = StreamResponse(
            Created,
            """{"type":"response.output_item.added","item":{"type":"image_generation_call","id":"ig_123","status":"in_progress"},"output_index":0}""",
            """{"type":"response.output_item.done","item":{"type":"image_generation_call","id":"ig_123","status":"completed","prompt":"An origami fox in a paper forest","result":"base64imagedata"},"output_index":0}""",
            Done);

        var parts = ToJson(await Collect(Model(capture), WithTools(ProviderTool("xai.image_generation", "image_generation"))));

        ContainsEqual(parts, """{"type":"tool-input-start","id":"ig_123","toolName":"image_generation"}""");
        ContainsEqual(parts, """{"type":"tool-call","toolCallId":"ig_123","toolName":"image_generation","input":"{}","providerExecuted":true}""");
        ContainsEqual(parts, """
            {"type":"tool-result","toolCallId":"ig_123","toolName":"image_generation","result":{"result":"base64imagedata","prompt":"An origami fox in a paper forest"}}
            """);
        Assert.Single(parts, part => (string?)part!["type"] == "tool-input-start");
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream an error tool result for failed image_generation calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_an_error_tool_result_for_failed_image_generation_calls()
    {
        var capture = StreamResponse(
            Created,
            """{"type":"response.image_generation_call.in_progress","item_id":"ig_123","output_index":0}""",
            """{"type":"response.output_item.done","item":{"type":"image_generation_call","id":"ig_123","status":"failed"},"output_index":0}""",
            Done);

        var parts = ToJson(await Collect(Model(capture), WithTools(ProviderTool("xai.image_generation", "image_generation"))));

        ContainsEqual(parts, """{"type":"tool-call","toolCallId":"ig_123","toolName":"image_generation","input":"{}","providerExecuted":true}""");
        ContainsEqual(parts, """
            {"type":"tool-result","toolCallId":"ig_123","toolName":"image_generation","isError":true,"result":"Image generation failed (status: failed)."}
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream function tool call arguments", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_function_tool_call_arguments()
    {
        var options = HelloPrompt();
        options.Tools = new[] { WeatherTool() };

        var parts = await Collect(Model(FunctionCallStream(withDeltas: true)), options);

        var start = Assert.Single(parts.OfType<ToolInputStartStreamPart>());
        Assert.Equal(("call_123", "weather"), (start.Id, start.ToolName));
        Assert.Equal(new[] { "{\"location\"", ":\"sf\"}" }, parts.OfType<ToolInputDeltaStreamPart>().Select(delta => delta.Delta));
        Assert.All(parts.OfType<ToolInputDeltaStreamPart>(), delta => Assert.Equal("call_123", delta.Id));
        Assert.Equal("call_123", Assert.Single(parts.OfType<ToolInputEndStreamPart>()).Id);
        var call = Assert.Single(parts.OfType<ToolCallStreamPart>());
        Assert.Equal(("call_123", "weather", "{\"location\":\"sf\"}"), (call.ToolCallId, call.ToolName, call.ArgumentsJson));
    }

    [Fact]
    [UpstreamTest(Prefix + "should return tool-calls finish reason when function_call is streamed", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_tool_calls_finish_reason_when_function_call_is_streamed()
    {
        var options = HelloPrompt();
        options.Tools = new[] { WeatherTool() };

        var parts = await Collect(Model(FunctionCallStream(withDeltas: false)), options);

        var finish = First<FinishStreamPart>(parts);
        Assert.Equal(FinishReason.ToolCalls, finish.FinishReason);
        Assert.Equal("completed", finish.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream file_search tool call and result", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_file_search_tool_call_and_result()
    {
        var capture = StreamResponse(
            Created,
            """{"type":"response.output_item.added","item":{"type":"file_search_call","id":"fs_stream_123","status":"in_progress","queries":["search query"],"results":null},"output_index":0}""",
            """
            {"type":"response.output_item.done","item":{"type":"file_search_call","id":"fs_stream_123","status":"completed","queries":["search query"],
              "results":[{"file_id":"file_abc","filename":"doc.txt","score":0.9,"text":"Found text content"}]},"output_index":0}
            """.Replace("\n", string.Empty),
            Done);

        var parts = ToJson(await Collect(Model(capture), WithTools(ProviderTool("xai.file_search", "file_search", """{"vectorStoreIds":["collection_123"]}"""))));

        ContainsEqual(parts, """{"type":"tool-input-start","id":"fs_stream_123","toolName":"file_search"}""");
        ContainsEqual(parts, """{"type":"tool-input-delta","id":"fs_stream_123","delta":""}""");
        ContainsEqual(parts, """{"type":"tool-input-end","id":"fs_stream_123"}""");
        ContainsEqual(parts, """{"type":"tool-call","toolCallId":"fs_stream_123","toolName":"file_search","input":"","providerExecuted":true}""");
        ContainsEqual(parts, """
            {"type":"tool-result","toolCallId":"fs_stream_123","toolName":"file_search",
             "result":{"queries":["search query"],"results":[{"fileId":"file_abc","filename":"doc.txt","score":0.9,"text":"Found text content"}]}}
            """);
    }

    private static UpstreamCapture FunctionCallStream(bool withDeltas)
    {
        var events = new List<string>
        {
            """{"type":"response.created","response":{"id":"resp_123","object":"response","model":"grok-4-fast-non-reasoning","status":"in_progress","output":[]}}""",
            """{"type":"response.output_item.added","output_index":0,"item":{"type":"function_call","id":"fc_123","call_id":"call_123","name":"weather","arguments":"","status":"in_progress"}}""",
        };
        if (withDeltas)
        {
            events.Add("""{"type":"response.function_call_arguments.delta","item_id":"fc_123","output_index":0,"delta":"{\"location\""}""");
            events.Add("""{"type":"response.function_call_arguments.delta","item_id":"fc_123","output_index":0,"delta":":\"sf\"}"}""");
        }

        events.Add("""{"type":"response.function_call_arguments.done","item_id":"fc_123","output_index":0,"arguments":"{\"location\":\"sf\"}"}""");
        events.Add("""{"type":"response.output_item.done","output_index":0,"item":{"type":"function_call","id":"fc_123","call_id":"call_123","name":"weather","arguments":"{\"location\":\"sf\"}","status":"completed"}}""");
        events.Add("""{"type":"response.done","response":{"id":"resp_123","object":"response","status":"completed","output":[],"usage":{"input_tokens":10,"output_tokens":5}}}""");
        return StreamResponse(events.ToArray());
    }
}
