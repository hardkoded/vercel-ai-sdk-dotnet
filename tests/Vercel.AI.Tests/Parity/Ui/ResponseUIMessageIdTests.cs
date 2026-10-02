// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AspNetCore;

namespace Vercel.AI.Tests;

public sealed class ResponseUIMessageIdTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/get-response-ui-message-id.test.ts::getResponseUIMessageId::should return undefined when originalMessages is null",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_null_when_original_messages_are_null()
    {
        Assert.Null(UIMessages.GetResponseMessageId(null, () => "new-id"));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/get-response-ui-message-id.test.ts::getResponseUIMessageId::should return the last assistant message id when present",
        Coverage = UpstreamCoverage.Covered)]
    public void Reuses_the_last_assistant_message_id()
    {
        var messages = new[]
        {
            new UIMessage("msg-1", "user"),
            new UIMessage("msg-2", "assistant"),
        };

        Assert.Equal("msg-2", UIMessages.GetResponseMessageId(messages, () => "new-id"));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/get-response-ui-message-id.test.ts::getResponseUIMessageId::should generate new id when last message is not from assistant",
        Coverage = UpstreamCoverage.Covered)]
    public void Generates_an_id_when_the_last_message_is_not_from_the_assistant()
    {
        var messages = new[]
        {
            new UIMessage("msg-1", "assistant"),
            new UIMessage("msg-2", "user"),
        };

        Assert.Equal("new-id", UIMessages.GetResponseMessageId(messages, () => "new-id"));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/get-response-ui-message-id.test.ts::getResponseUIMessageId::should generate new id when messages array is empty",
        Coverage = UpstreamCoverage.Covered)]
    public void Generates_an_id_when_the_message_list_is_empty()
    {
        Assert.Equal("new-id", UIMessages.GetResponseMessageId(Array.Empty<UIMessage>(), () => "new-id"));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/get-response-ui-message-id.test.ts::getResponseUIMessageId::should use the responseMessageId when it is a string",
        Coverage = UpstreamCoverage.Covered)]
    public void Uses_a_string_response_message_id()
    {
        Assert.Equal("response-id", UIMessages.GetResponseMessageId(Array.Empty<UIMessage>(), "response-id"));
    }
}
