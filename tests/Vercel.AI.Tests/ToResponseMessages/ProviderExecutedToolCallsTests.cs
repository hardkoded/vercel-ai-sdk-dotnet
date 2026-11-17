// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.ToResponseMessages.ResponseMessagesAssert;

namespace Vercel.AI.Tests.ToResponseMessages;

/// <summary>Port of <c>to-response-messages.test.ts</c> &gt; <c>toResponseMessages</c> &gt; <c>provider-executed tool calls</c>.</summary>
public sealed class ProviderExecutedToolCallsTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/to-response-messages.test.ts::toResponseMessages > provider-executed tool calls::";

    [Fact]
    [UpstreamTest(
        Prefix + "should include provider-executed tool calls and results",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_include_provider_executed_tool_calls_and_results()
    {
        const string callId = "srvtoolu_011cNtbtzFARKPcAcp7w4nh9";
        const string input = "{\"query\":\"San Francisco major news events June 22 2025\"}";
        const string finalText =
            "Based on the search results, several significant events took place in San Francisco yesterday (June 22, 2025). Here are the main highlights:\n\n1. Juneteenth Celebration:\n";

        var result = ToResponse(
            [
                new GeneratedText("Let me search for recent news from San Francisco."),
                new GeneratedToolCall(callId, "web_search", input) { ProviderExecuted = true },
                new GeneratedToolResult(
                    callId,
                    "web_search",
                    "[{\"url\":\"https://patch.com/california/san-francisco/calendar\"}]"
                ),
                new GeneratedText(finalText),
            ],
            [new Tool("web_search", null, Json("{\"type\":\"object\"}"), null)]
        );

        Assert.Single(result);
        var assistant = Assistant(result, 0);
        Assert.Equal(4, assistant.Parts.Count);
        Text(assistant, 0, "Let me search for recent news from San Francisco.");
        ToolCall(assistant, 1, callId, "web_search", input, providerExecuted: true);
        var toolResult = Part<AssistantToolResultPart>(assistant, 2);
        Assert.Null(toolResult.ProviderOptions);
        AssertToolResult(
            toolResult.Output,
            callId,
            "web_search",
            "json",
            "[{\"url\":\"https://patch.com/california/san-francisco/calendar\"}]"
        );
        Text(assistant, 3, finalText);
    }
}
