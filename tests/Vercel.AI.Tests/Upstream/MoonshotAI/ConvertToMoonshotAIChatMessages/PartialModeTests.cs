// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.MoonshotAI.ConvertToMoonshotAIChatMessages.MoonshotMessagesSupport;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.ConvertToMoonshotAIChatMessages;

/// <summary>Port of <c>convert-to-moonshotai-chat-messages.test.ts</c> &gt; <c>Partial Mode</c>.</summary>
public sealed class PartialModeTests
{
    private const string Suite = Prefix + "Partial Mode::";

    private static readonly IReadOnlyDictionary<
        string,
        System.Text.Json.JsonElement
    > PartialOptions = Options("{\"moonshotai\":{\"partial\":true}}");

    [Fact]
    [UpstreamTest(
        Suite + "should serialize partial true on the final assistant message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_serialize_partial_true_on_the_final_assistant_message()
    {
        var result = Convert(
            new ModelMessage[]
            {
                new UserModelMessage(
                    new UserContentPart[] { new TextContentPart("Return a JSON object.") }
                ),
                AssistantText(
                    "{",
                    Options("{\"moonshotai\":{\"name\":\"prefix\",\"partial\":true}}")
                ),
            }
        );

        Equal(
            result.Messages[^1],
            "{\"role\":\"assistant\",\"content\":\"{\",\"name\":\"prefix\",\"partial\":true}"
        );
    }

    [Fact]
    [UpstreamTest(
        Suite + "should reject partial true on a non-assistant message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_reject_partial_true_on_a_non_assistant_message()
    {
        Assert.Throws<InvalidPromptException>(() =>
            Convert(
                new ModelMessage[]
                {
                    new UserModelMessage(new UserContentPart[] { new TextContentPart("Continue.") })
                    {
                        ProviderOptions = PartialOptions,
                    },
                }
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Suite + "should reject partial true on a non-final assistant message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_reject_partial_true_on_a_non_final_assistant_message()
    {
        Assert.Throws<InvalidPromptException>(() =>
            Convert(
                new ModelMessage[]
                {
                    AssistantText("{", PartialOptions),
                    new UserModelMessage(
                        new UserContentPart[] { new TextContentPart("Continue.") }
                    ),
                }
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Suite + "should reject Partial Mode with JSON object response format",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_reject_Partial_Mode_with_JSON_object_response_format()
    {
        Assert.Throws<InvalidPromptException>(() =>
            Convert(
                new ModelMessage[] { AssistantText("{", PartialOptions) },
                responseFormatType: "json_object"
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Suite + "should allow Partial Mode with JSON schema response format",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_allow_Partial_Mode_with_JSON_schema_response_format()
    {
        var result = Convert(
            new ModelMessage[] { AssistantText("{", PartialOptions) },
            responseFormatType: "json_schema"
        );

        Assert.True((bool)result.Messages[0]!["partial"]!);
    }
}
