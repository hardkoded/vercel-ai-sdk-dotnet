// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Groq.ConvertToGroqChatMessages.ConvertToGroqChatMessagesSupport;

namespace Vercel.AI.Tests.Upstream.Groq.ConvertToGroqChatMessages;

/// <summary>Port of <c>convert-to-groq-chat-messages.test.ts</c> &gt; <c>tool calls</c>.</summary>
public sealed class ToolCallsTests
{
    private const string Prefix = TestFile + "::tool calls::";

    [Fact]
    [UpstreamTest(
        Prefix + "should stringify arguments to tool calls",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66"
    )]
    public void Should_stringify_arguments_to_tool_calls()
    {
        var result = Run(
            Assistant(Call("quux", "thwomp", """{"foo":"bar123"}""")),
            ToolJson("quux", "thwomp", """{"oof":"321rab"}""")
        );

        JsonAssert.Equal(
            result,
            """
            [{"content":"","role":"assistant",
              "tool_calls":[{"function":{"arguments":"{\"foo\":\"bar123\"}","name":"thwomp"},"id":"quux","type":"function"}]},
             {"content":"{\"oof\":\"321rab\"}","role":"tool","tool_call_id":"quux"}]
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send reasoning if present",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66"
    )]
    public void Should_send_reasoning_if_present()
    {
        var result = Run(
            Assistant(
                new AssistantReasoningPart("I think the tool will return the correct value."),
                Call("quux", "thwomp", """{"foo":"bar123"}""")
            )
        );

        JsonAssert.Equal(
            result,
            """
            [{"content":"","reasoning":"I think the tool will return the correct value.","role":"assistant",
              "tool_calls":[{"function":{"arguments":"{\"foo\":\"bar123\"}","name":"thwomp"},"id":"quux","type":"function"}]}]
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not include reasoning field when no reasoning content is present",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66"
    )]
    public void Should_not_include_reasoning_field_when_no_reasoning_content_is_present()
    {
        var result = Run(Assistant(new AssistantTextPart("Hello, how can I help you?")));

        JsonAssert.Equal(
            result,
            """[{"content":"Hello, how can I help you?","role":"assistant"}]"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw for file parts with provider references",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66"
    )]
    public void Should_throw_for_file_parts_with_provider_references()
    {
        var exception = Assert.Throws<UnsupportedFunctionalityException>(() =>
            Run(
                User(
                    FileContentPart.FromProviderReference(
                        "image/png",
                        new Dictionary<string, string> { ["groq"] = "file-ref-123" }
                    )
                )
            )
        );

        Assert.Contains(
            "'file parts with provider references' functionality not supported",
            exception.Message
        );
    }
}
