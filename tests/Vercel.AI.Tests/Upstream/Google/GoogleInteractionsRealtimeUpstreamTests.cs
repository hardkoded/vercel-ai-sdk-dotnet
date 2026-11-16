// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Interactions finish reasons and input conversion.</summary>
public sealed class GoogleInteractionsRealtimeUpstreamTests
{
    [Fact]
    [UpstreamTest(
        "packages/google/src/interactions/map-google-interactions-finish-reason.test.ts::mapGoogleInteractionsFinishReason::maps \"completed\" without function_call to \"stop\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_completed_without_a_function_call_to_stop()
    {
        Assert.Equal(FinishReason.Stop, GoogleInteractionsFinish.Map("completed", false));
    }

    [Fact]
    [UpstreamTest(
        "packages/google/src/interactions/map-google-interactions-finish-reason.test.ts::mapGoogleInteractionsFinishReason::maps \"completed\" with function_call to \"tool-calls\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_completed_with_a_function_call_to_tool_calls()
    {
        Assert.Equal(FinishReason.ToolCalls, GoogleInteractionsFinish.Map("completed", true));
    }

    [Fact]
    [UpstreamTest(
        "packages/google/src/interactions/map-google-interactions-finish-reason.test.ts::mapGoogleInteractionsFinishReason::maps \"requires_action\" to \"tool-calls\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_requires_action_to_tool_calls()
    {
        Assert.Equal(
            FinishReason.ToolCalls,
            GoogleInteractionsFinish.Map("requires_action", false)
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/google/src/interactions/map-google-interactions-finish-reason.test.ts::mapGoogleInteractionsFinishReason::maps \"failed\" to \"error\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_failed_to_error()
    {
        Assert.Equal(FinishReason.Error, GoogleInteractionsFinish.Map("failed", false));
    }

    [Fact]
    [UpstreamTest(
        "packages/google/src/interactions/map-google-interactions-finish-reason.test.ts::mapGoogleInteractionsFinishReason::maps \"incomplete\" to \"length\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_incomplete_to_length()
    {
        Assert.Equal(FinishReason.Length, GoogleInteractionsFinish.Map("incomplete", false));
    }

    [Fact]
    [UpstreamTest(
        "packages/google/src/interactions/map-google-interactions-finish-reason.test.ts::mapGoogleInteractionsFinishReason::maps \"cancelled\" to \"other\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_cancelled_to_other()
    {
        Assert.Equal(FinishReason.Other, GoogleInteractionsFinish.Map("cancelled", false));
    }

    [Fact]
    [UpstreamTest(
        "packages/google/src/interactions/map-google-interactions-finish-reason.test.ts::mapGoogleInteractionsFinishReason::maps \"in_progress\" / unknown to \"other\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_in_progress_and_unknown_statuses_to_other()
    {
        Assert.Equal(FinishReason.Other, GoogleInteractionsFinish.Map("in_progress", false));
        Assert.Equal(FinishReason.Other, GoogleInteractionsFinish.Map("no-such-status", false));
        Assert.Equal(FinishReason.Other, GoogleInteractionsFinish.Map(null, false));
    }

    [Fact]
    [UpstreamTest(
        "packages/google/src/interactions/convert-to-google-interactions-input.test.ts::convertToGoogleInteractionsInput > text-only prompts::emits a single-turn array of text content blocks",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Emits_a_user_text_turn()
    {
        var body = GoogleInteractionsInput.Convert(
            new ModelMessage[] { new UserModelMessage("Hello, how are you?") }
        );
        GoogleUpstream.JsonEqual(
            body["input"]!,
            "[{\"type\":\"user_input\",\"content\":[{\"type\":\"text\",\"text\":\"Hello, how are you?\"}]}]"
        );
        Assert.False(body.ContainsKey("systemInstruction"));
    }

    [Fact]
    [UpstreamTest(
        "packages/google/src/interactions/convert-to-google-interactions-input.test.ts::convertToGoogleInteractionsInput > text-only prompts::extracts system messages into systemInstruction",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Extracts_system_messages_into_systemInstruction()
    {
        var body = GoogleInteractionsInput.Convert(
            new ModelMessage[]
            {
                new SystemModelMessage("You are a helpful assistant."),
                new UserModelMessage("Hi"),
            }
        );
        Assert.Equal("You are a helpful assistant.", (string?)body["systemInstruction"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/google/src/interactions/convert-to-google-interactions-input.test.ts::convertToGoogleInteractionsInput > assistant tool-call parts::emits a function_call content block from an assistant tool-call part with object input",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Emits_a_function_call_block()
    {
        var body = GoogleInteractionsInput.Convert(
            new ModelMessage[]
            {
                new UserModelMessage("What's the weather in NYC?"),
                new AssistantModelMessage(
                    null,
                    new[]
                    {
                        new GeneratedToolCall(
                            "call_abc",
                            "getWeather",
                            "{\"location\":\"New York\"}"
                        ),
                    },
                    null
                ),
            }
        );
        GoogleUpstream.JsonEqual(
            body["input"]!,
            "[{\"type\":\"user_input\",\"content\":[{\"type\":\"text\",\"text\":\"What's the weather in NYC?\"}]},{\"type\":\"function_call\",\"name\":\"getWeather\",\"id\":\"call_abc\",\"arguments\":{\"location\":\"New York\"}}]"
        );
    }
}
