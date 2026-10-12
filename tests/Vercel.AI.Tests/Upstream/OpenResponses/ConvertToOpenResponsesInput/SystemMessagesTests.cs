// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.OpenResponses.ConvertToOpenResponsesInput.ConvertToOpenResponsesInputSupport;

namespace Vercel.AI.Tests.Upstream.OpenResponses.ConvertToOpenResponsesInput;

/// <summary>Port of <c>convert-to-open-responses-input.test.ts</c> &gt; <c>convertToOpenResponsesInput &gt; system messages</c>.</summary>
public sealed class SystemMessagesTests
{
    private const string Prefix = "packages/open-responses/src/responses/convert-to-open-responses-input.test.ts::convertToOpenResponsesInput > system messages::";

    [Fact]
    [UpstreamTest(Prefix + "should convert a single system message to instructions", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_a_single_system_message_to_instructions()
    {
        var result = Convert(new SystemModelMessage("You are a helpful assistant."));

        Assert.Equal("You are a helpful assistant.", result.Instructions);
        JsonAssert.Equal(result.Input, "[]");
    }

    [Fact]
    [UpstreamTest(Prefix + "should join multiple system messages with newlines", Coverage = UpstreamCoverage.Covered)]
    public void Should_join_multiple_system_messages_with_newlines()
    {
        var result = Convert(new SystemModelMessage("You are a helpful assistant."), new SystemModelMessage("Always be concise."));

        Assert.Equal("You are a helpful assistant.\nAlways be concise.", result.Instructions);
        JsonAssert.Equal(result.Input, "[]");
    }

    [Fact]
    [UpstreamTest(Prefix + "should return undefined instructions when no system messages", Coverage = UpstreamCoverage.Covered)]
    public void Should_return_undefined_instructions_when_no_system_messages()
    {
        var result = Convert(new UserModelMessage("Hello"));

        Assert.Null(result.Instructions);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle system message with user and assistant messages", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_system_message_with_user_and_assistant_messages()
    {
        var result = Convert(new SystemModelMessage("You are a helpful assistant."), new UserModelMessage("Hello"), AssistantText("Hi there!"));

        Assert.Equal("You are a helpful assistant.", result.Instructions);
        JsonAssert.Equal(result.Input, """
            [
              {"content":[{"text":"Hello","type":"input_text"}],"role":"user","type":"message"},
              {"content":[{"text":"Hi there!","type":"output_text"}],"role":"assistant","type":"message"}
            ]
            """);
    }
}
