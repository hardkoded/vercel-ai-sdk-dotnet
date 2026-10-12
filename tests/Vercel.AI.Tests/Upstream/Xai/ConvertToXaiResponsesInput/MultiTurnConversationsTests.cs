// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Xai.ConvertToXaiResponsesInput.ConvertToXaiResponsesInputSupport;

namespace Vercel.AI.Tests.Upstream.Xai.ConvertToXaiResponsesInput;

/// <summary>Port of <c>convert-to-xai-responses-input.test.ts</c> &gt; <c>convertToXaiResponsesInput &gt; multi-turn conversations</c>.</summary>
public sealed class MultiTurnConversationsTests
{
    private const string Prefix = TestFile + " > multi-turn conversations::";

    [Fact]
    [UpstreamTest(Prefix + "should handle full conversation with client-side tool calls", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_full_conversation_with_client_side_tool_calls()
    {
        var input = Convert(
            new UserModelMessage("whats the weather"),
            new AssistantModelMessage(null, new[] { new GeneratedToolCall("call_123", "weather", "{\"location\":\"sf\"}") }, null),
            new ToolModelMessage("call_123", "weather", "{\"temp\":72}", isError: false),
            new AssistantModelMessage("its 72 degrees", null, null));

        JsonAssert.Equal(input, """
            [{"content":[{"text":"whats the weather","type":"input_text"}],"role":"user"},
             {"arguments":"{\"location\":\"sf\"}","call_id":"call_123","id":"call_123","name":"weather","status":"completed","type":"function_call"},
             {"call_id":"call_123","output":"{\"temp\":72}","type":"function_call_output"},
             {"content":"its 72 degrees","role":"assistant"}]
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should round-trip reasoning with encrypted content in multi-turn", Coverage = UpstreamCoverage.Covered)]
    public void Should_round_trip_reasoning_with_encrypted_content_in_multi_turn()
    {
        var warnings = new List<CallWarning>();
        var reasoningOptions = UpstreamChat.Bag("xai", """{"itemId":"rs_789","reasoningEncryptedContent":"encrypted_xyz"}""");

        var input = Convert(
            warnings,
            new UserModelMessage("What is the capital of France?"),
            new AssistantModelMessage("The capital of France is Paris.", null, "The user is asking about geography.", reasoningOptions),
            new UserModelMessage("And what about Germany?"));

        JsonAssert.Equal(input, """
            [{"content":[{"text":"What is the capital of France?","type":"input_text"}],"role":"user"},
             {"encrypted_content":"encrypted_xyz","id":"rs_789","status":"completed","summary":[{"text":"The user is asking about geography.","type":"summary_text"}],"type":"reasoning"},
             {"content":"The capital of France is Paris.","role":"assistant"},
             {"content":[{"text":"And what about Germany?","type":"input_text"}],"role":"user"}]
            """);
        Assert.Empty(warnings);
    }
}
