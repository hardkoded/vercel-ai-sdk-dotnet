// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Xai.ConvertToXaiResponsesInput.ConvertToXaiResponsesInputSupport;

namespace Vercel.AI.Tests.Upstream.Xai.ConvertToXaiResponsesInput;

/// <summary>Port of <c>convert-to-xai-responses-input.test.ts</c> &gt; <c>convertToXaiResponsesInput &gt; assistant messages</c>.</summary>
public sealed class AssistantMessagesTests
{
    private const string Prefix = TestFile + " > assistant messages::";

    [Fact]
    [UpstreamTest(Prefix + "should convert text content", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_text_content()
    {
        var input = Convert(new AssistantModelMessage("hi there", null, null));

        JsonAssert.Equal(input, """[{"content":"hi there","role":"assistant"}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle client-side tool-call parts", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_client_side_tool_call_parts()
    {
        var input = Convert(new AssistantModelMessage(null, new[] { new GeneratedToolCall("call_123", "weather", "{\"location\":\"sf\"}") }, null));

        JsonAssert.Equal(input, """
            [{"arguments":"{\"location\":\"sf\"}","call_id":"call_123","id":"call_123","name":"weather","status":"completed","type":"function_call"}]
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle client-side tool-call parts named like server-side tools", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_client_side_tool_call_parts_named_like_server_side_tools()
    {
        var input = Convert(new AssistantModelMessage(null, new[] { new GeneratedToolCall("call_ws", "web_search", "{\"query\":\"latest news\"}") }, null));

        JsonAssert.Equal(input, """
            [{"arguments":"{\"query\":\"latest news\"}","call_id":"call_ws","id":"call_ws","name":"web_search","status":"completed","type":"function_call"}]
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should skip server-side tool-call parts", Coverage = UpstreamCoverage.Covered)]
    public void Should_skip_server_side_tool_call_parts()
    {
        var call = new GeneratedToolCall("ws_123", "web_search", "{}") { ProviderExecuted = true };

        var input = Convert(new AssistantModelMessage(new AssistantContentPart[] { new AssistantToolCallPart(call) }));

        JsonAssert.Equal(input, "[]");
    }
}
