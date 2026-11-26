// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Moonshot;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.ConvertToMoonshotAIChatMessages.Helpers;

namespace Vercel.AI.Tests.ConvertToMoonshotAIChatMessages;

/// <summary>Port of <c>convert-to-moonshotai-chat-messages.test.ts</c> &gt; <c>Partial Mode</c>.</summary>
public sealed class PartialModeTests
{
    private const string Prefix = Source + "Partial Mode::";

    private static AssistantModelMessage Partial(string options)
    {
        return new AssistantModelMessage("{", null, null) { ProviderOptions = Options(options) };
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should serialize partial true on the final assistant message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_serialize_partial_true_on_the_final_assistant_message()
    {
        var result = MoonshotChatMessages.ConvertToMoonshotAIChatMessages([
            new UserModelMessage("Return a JSON object."),
            Partial("{\"moonshotai\":{\"name\":\"prefix\",\"partial\":true}}"),
        ]);

        AssertJson(
            "{\"role\":\"assistant\",\"content\":\"{\",\"name\":\"prefix\",\"partial\":true}",
            result.Messages[result.Messages.Count - 1]
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject partial true on a non-assistant message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_reject_partial_true_on_a_non_assistant_message()
    {
        Assert.Throws<InvalidPromptException>(() =>
            MoonshotChatMessages.ConvertToMoonshotAIChatMessages([
                new UserModelMessage("Continue.")
                {
                    ProviderOptions = Options("{\"moonshotai\":{\"partial\":true}}"),
                },
            ])
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject partial true on a non-final assistant message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_reject_partial_true_on_a_non_final_assistant_message()
    {
        Assert.Throws<InvalidPromptException>(() =>
            MoonshotChatMessages.ConvertToMoonshotAIChatMessages([
                Partial("{\"moonshotai\":{\"partial\":true}}"),
                new UserModelMessage("Continue."),
            ])
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject Partial Mode with JSON object response format",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_reject_Partial_Mode_with_JSON_object_response_format()
    {
        Assert.Throws<InvalidPromptException>(() =>
            MoonshotChatMessages.ConvertToMoonshotAIChatMessages(
                [Partial("{\"moonshotai\":{\"partial\":true}}")],
                responseFormatType: "json_object"
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should allow Partial Mode with JSON schema response format",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_allow_Partial_Mode_with_JSON_schema_response_format()
    {
        var result = MoonshotChatMessages.ConvertToMoonshotAIChatMessages(
            [Partial("{\"moonshotai\":{\"partial\":true}}")],
            responseFormatType: "json_schema"
        );

        Assert.True(result.Messages[0]!["partial"]!.GetValue<bool>());
    }
}
