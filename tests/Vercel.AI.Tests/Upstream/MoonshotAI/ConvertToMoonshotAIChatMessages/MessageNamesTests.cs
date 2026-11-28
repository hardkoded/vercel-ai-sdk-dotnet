// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.MoonshotAI.ConvertToMoonshotAIChatMessages.MoonshotMessagesSupport;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.ConvertToMoonshotAIChatMessages;

/// <summary>Port of <c>convert-to-moonshotai-chat-messages.test.ts</c> &gt; <c>message names</c>.</summary>
public sealed class MessageNamesTests
{
    private const string Suite = Prefix + "message names::";

    [Fact]
    [UpstreamTest(
        Suite + "should serialize names for system, user, and assistant messages",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_serialize_names_for_system_user_and_assistant_messages()
    {
        var result = Convert(
            new ModelMessage[]
            {
                new SystemModelMessage(
                    "You are a helpful assistant.",
                    Options("{\"moonshotai\":{\"name\":\"guide\"}}")
                ),
                new UserModelMessage(
                    new UserContentPart[] { new TextContentPart("Describe this image.") }
                )
                {
                    ProviderOptions = Options("{\"moonshotai\":{\"name\":\"alice\"}}"),
                },
                AssistantText(
                    "I will inspect it.",
                    Options("{\"moonshotai\":{\"name\":\"vision_assistant\"}}")
                ),
            }
        );

        Equal(
            result.Messages,
            "[{\"role\":\"system\",\"content\":\"You are a helpful assistant.\",\"name\":\"guide\"},{\"role\":\"user\",\"content\":\"Describe this image.\",\"name\":\"alice\"},{\"role\":\"assistant\",\"content\":\"I will inspect it.\",\"name\":\"vision_assistant\"}]"
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Suite + "should ignore a name on a tool message with an unsupported warning",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_ignore_a_name_on_a_tool_message_with_an_unsupported_warning()
    {
        var result = Convert(
            new ModelMessage[]
            {
                new ToolModelMessage("call-1", "weather", "sunny", false)
                {
                    MessageProviderOptions = Options(
                        "{\"moonshotai\":{\"name\":\"weather_tool\"}}"
                    ),
                },
            }
        );

        Equal(
            result.Messages,
            "[{\"role\":\"tool\",\"tool_call_id\":\"call-1\",\"content\":\"sunny\"}]"
        );
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("message name on tool messages", warning.Feature);
    }

    [Fact]
    [UpstreamTest(Suite + "should reject a non-string name", Coverage = UpstreamCoverage.Covered)]
    public void Should_reject_a_non_string_name()
    {
        var error = Assert.Throws<InvalidArgumentException>(() =>
            Convert(
                new ModelMessage[]
                {
                    new UserModelMessage(new UserContentPart[] { new TextContentPart("Hello") })
                    {
                        ProviderOptions = Options("{\"moonshotai\":{\"name\":123}}"),
                    },
                }
            )
        );

        Assert.Contains("invalid moonshotai provider options", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Suite + "should serialize a name from a custom provider options namespace",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_serialize_a_name_from_a_custom_provider_options_namespace()
    {
        var result = Convert(
            new ModelMessage[]
            {
                new UserModelMessage(new UserContentPart[] { new TextContentPart("Hello") })
                {
                    ProviderOptions = Options("{\"custom\":{\"name\":\"alice\"}}"),
                },
            },
            providerOptionsName: "custom"
        );

        Equal(result.Messages, "[{\"role\":\"user\",\"content\":\"Hello\",\"name\":\"alice\"}]");
    }
}
