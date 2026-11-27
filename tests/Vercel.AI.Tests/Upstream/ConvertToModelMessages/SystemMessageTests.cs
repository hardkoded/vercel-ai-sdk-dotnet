// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.ConvertToModelMessages.ConvertToModelMessagesSupport;

namespace Vercel.AI.Tests.ConvertToModelMessages;

/// <summary>Port of <c>convert-to-model-messages.test.ts</c> &gt; <c>convertToModelMessages &gt; system message</c>.</summary>
[Collection("LogWarnings")]
public sealed class SystemMessageTests : WarningsOff
{
    private const string Suite = Prefix + "convertToModelMessages > system message::";

    [UpstreamTest(
        Suite + "should convert a simple system message",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_convert_a_simple_system_message()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [{"role":"system","parts":[{"text":"System message","type":"text"}]}]
                """
            )
        );

        AssertEqual(
            """
            [{"role":"system","content":"System message"}]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should convert a system message with provider metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_convert_a_system_message_with_provider_metadata()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [{"role":"system","parts":[{"text":"System message with metadata","type":"text","providerMetadata":{"testProvider":{"systemSignature":"abc123"}}}]}]
                """
            )
        );

        AssertEqual(
            """
            [{"role":"system","content":"System message with metadata","providerOptions":{"testProvider":{"systemSignature":"abc123"}}}]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should merge provider metadata from multiple text parts in system message",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_merge_provider_metadata_from_multiple_text_parts_in_system_message()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [{"role":"system","parts":[{"text":"Part 1","type":"text","providerMetadata":{"provider1":{"key1":"value1"}}},{"text":" Part 2","type":"text","providerMetadata":{"provider2":{"key2":"value2"}}}]}]
                """
            )
        );

        AssertEqual(
            """
            [{"role":"system","content":"Part 1 Part 2","providerOptions":{"provider1":{"key1":"value1"},"provider2":{"key2":"value2"}}}]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should convert a system message with Anthropic cache control metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_convert_a_system_message_with_Anthropic_cache_control_metadata()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [{"id":"system","role":"system","parts":[{"type":"text","text":"You are a helpful assistant.","providerMetadata":{"anthropic":{"cacheControl":{"type":"ephemeral"}}}}]}]
                """
            )
        );

        AssertEqual(
            """
            [{"role":"system","content":"You are a helpful assistant.","providerOptions":{"anthropic":{"cacheControl":{"type":"ephemeral"}}}}]
            """,
            result
        );
    }
}
