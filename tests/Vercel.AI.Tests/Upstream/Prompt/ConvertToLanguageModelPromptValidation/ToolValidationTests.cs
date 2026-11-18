// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Prompt.ConvertToLanguageModelPromptValidation;

/// <summary>Port of <c>convert-to-language-model-prompt.validation.test.ts</c> &gt; <c>tool validation</c>.</summary>
public sealed class ToolValidationTests
{
    private const string Id =
        "packages/ai/src/prompt/convert-to-language-model-prompt.validation.test.ts::tool validation::";

    private static Task<IReadOnlyList<ModelMessage>> Convert(params ModelMessage[] messages) =>
        LanguageModelPrompts.ConvertToLanguageModelPromptAsync(
            messages,
            new Dictionary<string, string>(),
            null
        );

    [Fact]
    [UpstreamTest(
        Id + "should pass validation for provider-executed tools (deferred results)",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3c09e0011c718c33f9287c5487c1f95190aeb772"
    )]
    public async Task Should_pass_validation_for_provider_executed_tools_deferred_results()
    {
        var result = await Convert(
            new AssistantModelMessage(
                new AssistantContentPart[]
                {
                    new AssistantToolCallPart(
                        new GeneratedToolCall(
                            "call_1",
                            "code_interpreter",
                            "{\"code\":\"print(\\\"hello\\\")\"}"
                        )
                        {
                            ProviderExecuted = true,
                        }
                    ),
                }
            )
        );

        var assistant = Assert.IsType<AssistantModelMessage>(Assert.Single(result));
        var call = Assert.IsType<AssistantToolCallPart>(Assert.Single(assistant.Parts));
        Assert.Equal("call_1", call.ToolCall.ToolCallId);
        Assert.Equal("code_interpreter", call.ToolCall.ToolName);
        Assert.True(call.ToolCall.ProviderExecuted);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [UpstreamTest(
        Id + "should reject an unresolved local approval (follow-up: %s)",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3c09e0011c718c33f9287c5487c1f95190aeb772"
    )]
    public async Task Should_reject_an_unresolved_local_approval(bool followUp)
    {
        var messages = new List<ModelMessage>
        {
            new AssistantModelMessage(
                new AssistantContentPart[]
                {
                    new AssistantToolCallPart(
                        new GeneratedToolCall(
                            "call_to_approve",
                            "dangerous_action",
                            "{\"action\":\"delete_db\"}"
                        )
                    ),
                    new AssistantToolApprovalRequestPart("approval_123", "call_to_approve"),
                }
            ),
            new ToolApprovalResponseModelMessage("approval_123", true),
        };
        if (followUp)
        {
            messages.Add(new UserModelMessage("Continue."));
        }

        await Assert.ThrowsAsync<MissingToolResultsError>(() => Convert(messages.ToArray()));
    }

    [Fact]
    [UpstreamTest(
        Id + "should preserve provider-executed tool-approval-response",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3c09e0011c718c33f9287c5487c1f95190aeb772"
    )]
    public async Task Should_preserve_provider_executed_tool_approval_response()
    {
        var result = await Convert(
            new AssistantModelMessage(
                new AssistantContentPart[]
                {
                    new AssistantToolCallPart(
                        new GeneratedToolCall(
                            "call_provider_executed",
                            "mcp_tool",
                            "{\"action\":\"execute\"}"
                        )
                        {
                            ProviderExecuted = true,
                        }
                    ),
                    new AssistantToolApprovalRequestPart(
                        "approval_provider",
                        "call_provider_executed"
                    ),
                }
            ),
            new ToolApprovalResponseModelMessage("approval_provider", true, null, true)
        );

        Assert.Equal(2, result.Count);
        var assistant = Assert.IsType<AssistantModelMessage>(result[0]);
        Assert.IsType<AssistantToolCallPart>(Assert.Single(assistant.Parts));
        var response = Assert.IsType<ToolApprovalResponseModelMessage>(result[1]);
        Assert.Equal("approval_provider", response.ApprovalId);
        Assert.True(response.Approved);
    }

    [Fact]
    [UpstreamTest(
        Id + "should throw error for actual missing results",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3c09e0011c718c33f9287c5487c1f95190aeb772"
    )]
    public async Task Should_throw_error_for_actual_missing_results()
    {
        await Assert.ThrowsAsync<MissingToolResultsError>(() =>
            Convert(
                new AssistantModelMessage(
                    new AssistantContentPart[]
                    {
                        new AssistantToolCallPart(
                            new GeneratedToolCall("call_missing_result", "regular_tool", "{}")
                        ),
                    }
                )
            )
        );
    }
}
