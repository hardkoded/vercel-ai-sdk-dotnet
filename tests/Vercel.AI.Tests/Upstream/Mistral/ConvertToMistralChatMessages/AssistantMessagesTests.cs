// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Converter = Vercel.AI.Mistral.ConvertToMistralChatMessages;

namespace Vercel.AI.Tests.Upstream.Mistral.ConvertToMistralChatMessages;

/// <summary>Port of <c>convert-to-mistral-chat-messages.test.ts</c> &gt; <c>assistant messages</c>.</summary>
public sealed class AssistantMessagesTests
{
    private const string Prefix =
        "packages/mistral/src/convert-to-mistral-chat-messages.test.ts::assistant messages::";

    private static void AssertJson(JsonNode? actual, string expected) =>
        Assert.True(JsonNode.DeepEquals(actual, JsonNode.Parse(expected)), actual?.ToJsonString());

    [Fact]
    [UpstreamTest(
        Prefix + "should add prefix true to trailing assistant messages",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_add_prefix_true_to_trailing_assistant_messages()
    {
        var result = Converter.Convert(
            new ModelMessage[]
            {
                new UserModelMessage("Hello"),
                new AssistantModelMessage("Hello!", null, null),
            }
        );

        AssertJson(
            result,
            "[{\"content\":[{\"text\":\"Hello\",\"type\":\"text\"}],\"role\":\"user\"},{\"content\":\"Hello!\",\"prefix\":true,\"role\":\"assistant\"}]"
        );
    }
}
