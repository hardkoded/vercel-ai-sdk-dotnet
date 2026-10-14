// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel;

/// <summary>Port of <c>xai-responses-language-model.test.ts</c> &gt; <c>XaiResponsesLanguageModel &gt; doStream &gt; tool name mapping by type in streaming</c>.</summary>
public sealed class DoStreamToolNameMappingByTypeInStreamingTests
{
    private const string Prefix = TestFile + " > doStream > tool name mapping by type in streaming::";

    [Fact]
    [UpstreamTest(Prefix + "should map web_search_call type to web_search tool name when name is empty", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_web_search_call_type_to_web_search_tool_name_when_name_is_empty()
    {
        var capture = StreamResponse(
            Created,
            """{"type":"response.output_item.added","item":{"type":"web_search_call","id":"ws_123","name":"","arguments":"{\"query\":\"test\"}","call_id":"","status":"completed"},"output_index":0}""",
            Done);

        var parts = await Collect(Model(capture), WithTools(ProviderTool("xai.web_search", "web_search")));

        ContainsEqual(ToJson(parts), """
            {"type":"tool-call","toolCallId":"ws_123","toolName":"web_search","input":"{\"query\":\"test\"}","providerExecuted":true}
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should map x_search_call type to x_search tool name when name is empty", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_x_search_call_type_to_x_search_tool_name_when_name_is_empty()
    {
        var capture = StreamResponse(
            Created,
            """{"type":"response.output_item.added","item":{"type":"x_search_call","id":"xs_123","name":"","arguments":"{\"query\":\"test\"}","call_id":"","status":"completed"},"output_index":0}""",
            Done);

        var parts = await Collect(Model(capture), WithTools(ProviderTool("xai.x_search", "x_search")));

        ContainsEqual(ToJson(parts), """
            {"type":"tool-call","toolCallId":"xs_123","toolName":"x_search","input":"{\"query\":\"test\"}","providerExecuted":true}
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should map code_interpreter_call type to code_execution tool name when name is empty", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_code_interpreter_call_type_to_code_execution_tool_name_when_name_is_empty()
    {
        var capture = StreamResponse(
            Created,
            """{"type":"response.output_item.added","item":{"type":"code_interpreter_call","id":"ci_123","name":"","arguments":"{}","call_id":"","status":"completed"},"output_index":0}""",
            Done);

        var parts = await Collect(Model(capture), WithTools(ProviderTool("xai.code_execution", "code_execution")));

        ContainsEqual(ToJson(parts), """
            {"type":"tool-call","toolCallId":"ci_123","toolName":"code_execution","input":"{}","providerExecuted":true}
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should map code_execution_call type to code_execution tool name when name is empty", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_code_execution_call_type_to_code_execution_tool_name_when_name_is_empty()
    {
        var capture = StreamResponse(
            Created,
            """{"type":"response.output_item.added","item":{"type":"code_execution_call","id":"ce_123","name":"","arguments":"{}","call_id":"","status":"completed"},"output_index":0}""",
            Done);

        var parts = await Collect(Model(capture), WithTools(ProviderTool("xai.code_execution", "code_execution")));

        ContainsEqual(ToJson(parts), """
            {"type":"tool-call","toolCallId":"ce_123","toolName":"code_execution","input":"{}","providerExecuted":true}
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should use custom tool name from provider tool when type matches", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_custom_tool_name_from_provider_tool_when_type_matches()
    {
        var capture = StreamResponse(
            Created,
            """{"type":"response.output_item.added","item":{"type":"web_search_call","id":"ws_123","name":"","arguments":"{}","call_id":"","status":"completed"},"output_index":0}""",
            Done);

        var parts = await Collect(Model(capture), WithTools(ProviderTool("xai.web_search", "my_custom_search")));

        ContainsEqual(ToJson(parts), """
            {"type":"tool-call","toolCallId":"ws_123","toolName":"my_custom_search","input":"{}","providerExecuted":true}
            """);
    }
}
