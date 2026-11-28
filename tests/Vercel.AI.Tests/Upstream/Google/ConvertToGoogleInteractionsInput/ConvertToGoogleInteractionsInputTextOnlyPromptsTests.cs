// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.ConvertToGoogleInteractionsInput;

/// <summary>Port of <c>convert-to-google-interactions-input.test.ts</c> &gt; <c>convertToGoogleInteractionsInput</c> &gt; <c>text-only prompts</c>.</summary>
public sealed class ConvertToGoogleInteractionsInputTextOnlyPromptsTests
{
    private const string Prefix =
        ConvertToGoogleInteractionsInputSupport.Id + "text-only prompts::";

    [Fact]
    [UpstreamTest(
        Prefix + "emits a single-turn array of text content blocks",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Emits_a_single_turn_array_of_text_content_blocks()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage("Hello, how are you?")
        );

        GoogleUpstream.JsonEqual(
            ConvertToGoogleInteractionsInputSupport.Snapshot(result),
            "{\"input\":[{\"type\":\"user_input\",\"content\":[{\"type\":\"text\",\"text\":\"Hello, how are you?\"}]}],\"warnings\":[]}"
        );
        Assert.Null(result.SystemInstruction);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "extracts system messages into systemInstruction",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Extracts_system_messages_into_systemInstruction()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new SystemModelMessage("You are a helpful assistant."),
            new UserModelMessage("Hi")
        );

        Assert.Equal("You are a helpful assistant.", result.SystemInstruction);
    }
}
