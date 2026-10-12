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
