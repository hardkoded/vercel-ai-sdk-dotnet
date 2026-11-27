// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AspNetCore;
using Vercel.AI.Ui;

namespace Vercel.AI.Tests.Upstream.GetResponseUiMessageId;

/// <summary>Port of <c>get-response-ui-message-id.test.ts</c> &gt; <c>getResponseUIMessageId</c>.</summary>
public sealed class GetResponseUIMessageIdTests
{
    private const string Prefix =
        "packages/ai/src/ui-message-stream/get-response-ui-message-id.test.ts::getResponseUIMessageId::";

    private static string MockGenerateId() => "new-id";

    private static UIMessage Message(string id, UIMessageRole role) => new(role, []) { Id = id };

    [Fact]
    [UpstreamTest(
        Prefix + "should return undefined when originalMessages is null",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_return_undefined_when_originalMessages_is_null()
    {
        var result = UIMessageStreamConverter.GetResponseUIMessageId(null, MockGenerateId);
        Assert.Null(result);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return the last assistant message id when present",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_return_the_last_assistant_message_id_when_present()
    {
        var messages = new[]
        {
            Message("msg-1", UIMessageRole.User),
            Message("msg-2", UIMessageRole.Assistant),
        };
        var result = UIMessageStreamConverter.GetResponseUIMessageId(messages, MockGenerateId);
        Assert.Equal("msg-2", result);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should generate new id when last message is not from assistant",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_generate_new_id_when_last_message_is_not_from_assistant()
    {
        var messages = new[]
        {
            Message("msg-1", UIMessageRole.Assistant),
            Message("msg-2", UIMessageRole.User),
        };
        var result = UIMessageStreamConverter.GetResponseUIMessageId(messages, MockGenerateId);
        Assert.Equal("new-id", result);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should generate new id when messages array is empty",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_generate_new_id_when_messages_array_is_empty()
    {
        var result = UIMessageStreamConverter.GetResponseUIMessageId([], MockGenerateId);
        Assert.Equal("new-id", result);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use the responseMessageId when it is a string",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_use_the_responseMessageId_when_it_is_a_string()
    {
        var result = UIMessageStreamConverter.GetResponseUIMessageId([], "response-id");
        Assert.Equal("response-id", result);
    }
}
