// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AspNetCore;

namespace Vercel.AI.Tests;

public sealed class LastAssistantMessageTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/last-assistant-message-is-complete-with-tool-calls.test.ts::lastAssistantMessageIsCompleteWithToolCalls::should return false if the last step of a multi-step sequence only has text",
        Coverage = UpstreamCoverage.Covered)]
    public void Tool_calls_are_incomplete_when_the_last_step_is_only_text()
    {
        Assert.False(ToolCalls(
            Step(),
            Tool("tool-getLocation", "output-available"),
            Step(),
            new UIMessagePart("text")));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/last-assistant-message-is-complete-with-tool-calls.test.ts::lastAssistantMessageIsCompleteWithToolCalls::should return true when there is a text part after the last tool result in the last step",
        Coverage = UpstreamCoverage.Covered)]
    public void Tool_calls_are_complete_when_text_follows_the_tool_result()
    {
        Assert.True(ToolCalls(
            Step(),
            Tool("tool-getWeatherInformation", "output-available"),
            new UIMessagePart("text")));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/last-assistant-message-is-complete-with-tool-calls.test.ts::lastAssistantMessageIsCompleteWithToolCalls::should return true when the tool has a output-error state",
        Coverage = UpstreamCoverage.Covered)]
    public void Tool_calls_are_complete_when_the_tool_output_is_an_error()
    {
        Assert.True(ToolCalls(Step(), Tool("tool-getWeatherInformation", "output-error"), new UIMessagePart("text")));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/last-assistant-message-is-complete-with-tool-calls.test.ts::lastAssistantMessageIsCompleteWithToolCalls::should return true when dynamic tool call is complete",
        Coverage = UpstreamCoverage.Covered)]
    public void Dynamic_tool_calls_are_complete_when_output_is_available()
    {
        Assert.True(ToolCalls(Step(), Tool("dynamic-tool", "output-available")));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/last-assistant-message-is-complete-with-tool-calls.test.ts::lastAssistantMessageIsCompleteWithToolCalls::should return false when a tool output is preliminary",
        Coverage = UpstreamCoverage.Covered)]
    public void Preliminary_tool_output_is_incomplete()
    {
        Assert.False(ToolCalls(Step(), Tool("tool-getWeatherInformation", "output-available", preliminary: true)));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/last-assistant-message-is-complete-with-tool-calls.test.ts::lastAssistantMessageIsCompleteWithToolCalls::should return false when a dynamic tool output is preliminary",
        Coverage = UpstreamCoverage.Covered)]
    public void Preliminary_dynamic_tool_output_is_incomplete()
    {
        Assert.False(ToolCalls(Step(), Tool("dynamic-tool", "output-available", preliminary: true)));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/last-assistant-message-is-complete-with-tool-calls.test.ts::lastAssistantMessageIsCompleteWithToolCalls::should return false when dynamic tool call is still streaming input",
        Coverage = UpstreamCoverage.Covered)]
    public void Streaming_dynamic_tool_input_is_incomplete()
    {
        Assert.False(ToolCalls(Step(), Tool("dynamic-tool", "input-streaming")));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/last-assistant-message-is-complete-with-tool-calls.test.ts::lastAssistantMessageIsCompleteWithToolCalls::should return false when dynamic tool call has input but no output",
        Coverage = UpstreamCoverage.Covered)]
    public void Dynamic_tool_input_without_output_is_incomplete()
    {
        Assert.False(ToolCalls(Step(), Tool("dynamic-tool", "input-available")));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/last-assistant-message-is-complete-with-tool-calls.test.ts::lastAssistantMessageIsCompleteWithToolCalls::should return true when dynamic tool call has an error",
        Coverage = UpstreamCoverage.Covered)]
    public void Dynamic_tool_output_error_is_complete()
    {
        Assert.True(ToolCalls(Step(), Tool("dynamic-tool", "output-error")));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/last-assistant-message-is-complete-with-tool-calls.test.ts::lastAssistantMessageIsCompleteWithToolCalls::should return true when mixing regular and dynamic tool calls and all are complete",
        Coverage = UpstreamCoverage.Covered)]
    public void Mixed_complete_tool_calls_are_complete()
    {
        Assert.True(ToolCalls(
            Step(),
            Tool("tool-getWeatherInformation", "output-available"),
            Tool("dynamic-tool", "output-available")));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/last-assistant-message-is-complete-with-tool-calls.test.ts::lastAssistantMessageIsCompleteWithToolCalls::should return false when mixing regular and dynamic tool calls and some are incomplete",
        Coverage = UpstreamCoverage.Covered)]
    public void Mixed_tool_calls_are_incomplete_when_one_has_no_output()
    {
        Assert.False(ToolCalls(
            Step(),
            Tool("tool-getWeatherInformation", "output-available"),
            Tool("dynamic-tool", "input-available")));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/last-assistant-message-is-complete-with-tool-calls.test.ts::lastAssistantMessageIsCompleteWithToolCalls::should return true for multi-step sequence where last step has complete dynamic tool calls",
        Coverage = UpstreamCoverage.Covered)]
    public void Last_step_complete_dynamic_tool_calls_are_complete()
    {
        Assert.True(ToolCalls(
            Step(),
            Tool("tool-getLocation", "output-available"),
            Step(),
            Tool("dynamic-tool", "output-available"),
            new UIMessagePart("text")));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/last-assistant-message-is-complete-with-tool-calls.test.ts::lastAssistantMessageIsCompleteWithToolCalls::should return false for multi-step sequence where last step has incomplete dynamic tool calls",
        Coverage = UpstreamCoverage.Covered)]
    public void Last_step_incomplete_dynamic_tool_calls_are_incomplete()
    {
        Assert.False(ToolCalls(
            Step(),
            Tool("tool-getLocation", "output-available"),
            Step(),
            Tool("dynamic-tool", "input-streaming")));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/last-assistant-message-is-complete-with-tool-calls.test.ts::lastAssistantMessageIsCompleteWithToolCalls::should return false for complete provider executed tool calls",
        Coverage = UpstreamCoverage.Covered)]
    public void Provider_executed_tool_calls_do_not_count_as_complete()
    {
        Assert.False(ToolCalls(
            Step(),
            Tool("tool-web_search", "output-available", providerExecuted: true),
            new UIMessagePart("text")));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/last-assistant-message-is-complete-with-approval-responses.test.ts::lastAssistantMessageIsCompleteWithApprovalResponses::should return false if messages is empty",
        Coverage = UpstreamCoverage.Covered)]
    public void Approval_responses_are_incomplete_for_an_empty_list()
    {
        Assert.False(UIMessages.LastAssistantMessageIsCompleteWithApprovalResponses(Array.Empty<UIMessage>()));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/last-assistant-message-is-complete-with-approval-responses.test.ts::lastAssistantMessageIsCompleteWithApprovalResponses::should return false if last message is a user message",
        Coverage = UpstreamCoverage.Covered)]
    public void Approval_responses_are_incomplete_when_the_last_message_is_from_the_user()
    {
        Assert.False(UIMessages.LastAssistantMessageIsCompleteWithApprovalResponses(new[]
        {
            new UIMessage("1", "user"),
        }));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/last-assistant-message-is-complete-with-approval-responses.test.ts::lastAssistantMessageIsCompleteWithApprovalResponses::should return false if there are no tool invocations in the last step",
        Coverage = UpstreamCoverage.Covered)]
    public void Approval_responses_are_incomplete_without_tool_invocations()
    {
        Assert.False(Approvals(Step(), new UIMessagePart("text")));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/last-assistant-message-is-complete-with-approval-responses.test.ts::lastAssistantMessageIsCompleteWithApprovalResponses::should return false if no tool has approval-responded state",
        Coverage = UpstreamCoverage.Covered)]
    public void Approval_responses_are_incomplete_when_approval_is_still_requested()
    {
        Assert.False(Approvals(Step(), Tool("tool-getWeather", "approval-requested")));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/last-assistant-message-is-complete-with-approval-responses.test.ts::lastAssistantMessageIsCompleteWithApprovalResponses::should return false if some tools still have approval-requested state",
        Coverage = UpstreamCoverage.Covered)]
    public void Approval_responses_are_incomplete_when_another_tool_is_still_requested()
    {
        Assert.False(Approvals(
            Step(),
            Tool("tool-getWeather", "approval-responded"),
            Tool("tool-getWeather", "approval-requested")));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/last-assistant-message-is-complete-with-approval-responses.test.ts::lastAssistantMessageIsCompleteWithApprovalResponses::should return true when a non-provider-executed tool has approval-responded",
        Coverage = UpstreamCoverage.Covered)]
    public void Approval_responses_are_complete_for_a_client_tool()
    {
        Assert.True(Approvals(Step(), Tool("tool-getWeather", "approval-responded")));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/last-assistant-message-is-complete-with-approval-responses.test.ts::lastAssistantMessageIsCompleteWithApprovalResponses::should return true when a provider-executed tool has approval-responded",
        Coverage = UpstreamCoverage.Covered)]
    public void Approval_responses_are_complete_for_a_provider_executed_tool()
    {
        Assert.True(Approvals(Step(), Tool("dynamic-tool", "approval-responded", providerExecuted: true)));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/last-assistant-message-is-complete-with-approval-responses.test.ts::lastAssistantMessageIsCompleteWithApprovalResponses::should return true when all tools have a terminal state and at least one is approval-responded",
        Coverage = UpstreamCoverage.Covered)]
    public void Approval_responses_are_complete_when_every_tool_is_terminal()
    {
        Assert.True(Approvals(
            Step(),
            Tool("tool-getWeather", "approval-responded"),
            Tool("tool-getWeather", "output-available")));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/last-assistant-message-is-complete-with-approval-responses.test.ts::lastAssistantMessageIsCompleteWithApprovalResponses::should return false when a tool output is preliminary",
        Coverage = UpstreamCoverage.Covered)]
    public void Approval_responses_are_incomplete_when_output_is_preliminary()
    {
        Assert.False(Approvals(
            Step(),
            Tool("tool-getWeather", "approval-responded"),
            Tool("tool-getWeather", "output-available", preliminary: true)));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/last-assistant-message-is-complete-with-approval-responses.test.ts::lastAssistantMessageIsCompleteWithApprovalResponses::should return false when a dynamic tool output is preliminary",
        Coverage = UpstreamCoverage.Covered)]
    public void Approval_responses_are_incomplete_when_dynamic_output_is_preliminary()
    {
        Assert.False(Approvals(
            Step(),
            Tool("tool-getWeather", "approval-responded"),
            Tool("dynamic-tool", "output-available", preliminary: true)));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/last-assistant-message-is-complete-with-approval-responses.test.ts::lastAssistantMessageIsCompleteWithApprovalResponses::should return true when a tool output is denied and another approval has responded",
        Coverage = UpstreamCoverage.Covered)]
    public void Approval_responses_are_complete_when_another_tool_is_denied()
    {
        Assert.True(Approvals(
            Step(),
            Tool("tool-getWeather", "approval-responded"),
            Tool("tool-deleteCalendarEvent", "output-denied")));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/last-assistant-message-is-complete-with-approval-responses.test.ts::lastAssistantMessageIsCompleteWithApprovalResponses::should return true mixing provider-executed (approval-responded) and regular (output-available)",
        Coverage = UpstreamCoverage.Covered)]
    public void Approval_responses_are_complete_for_mixed_provider_and_client_tools()
    {
        Assert.True(Approvals(
            Step(),
            Tool("dynamic-tool", "approval-responded", providerExecuted: true),
            Tool("tool-getWeather", "output-available")));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/last-assistant-message-is-complete-with-approval-responses.test.ts::lastAssistantMessageIsCompleteWithApprovalResponses::should return false when provider-executed tool is approval-responded but regular tool is still approval-requested",
        Coverage = UpstreamCoverage.Covered)]
    public void Approval_responses_are_incomplete_when_a_client_tool_is_still_requested()
    {
        Assert.False(Approvals(
            Step(),
            Tool("dynamic-tool", "approval-responded", providerExecuted: true),
            Tool("tool-getWeather", "approval-requested")));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui/last-assistant-message-is-complete-with-approval-responses.test.ts::lastAssistantMessageIsCompleteWithApprovalResponses::should only consider the last step in a multi-step message",
        Coverage = UpstreamCoverage.Covered)]
    public void Approval_responses_only_consider_the_last_step()
    {
        Assert.False(Approvals(
            Step(),
            Tool("tool-getWeather", "approval-responded"),
            Step(),
            new UIMessagePart("text")));
    }

    private static bool ToolCalls(params UIMessagePart[] parts)
    {
        return UIMessages.LastAssistantMessageIsCompleteWithToolCalls(new[]
        {
            new UIMessage("1", "assistant", parts),
        });
    }

    private static bool Approvals(params UIMessagePart[] parts)
    {
        return UIMessages.LastAssistantMessageIsCompleteWithApprovalResponses(new[]
        {
            new UIMessage("1", "assistant", parts),
        });
    }

    private static UIMessagePart Step()
    {
        return new UIMessagePart("step-start");
    }

    private static UIMessagePart Tool(string type, string state, bool? preliminary = null, bool? providerExecuted = null)
    {
        return new UIMessagePart(type)
        {
            State = state,
            Preliminary = preliminary,
            ProviderExecuted = providerExecuted,
        };
    }
}
