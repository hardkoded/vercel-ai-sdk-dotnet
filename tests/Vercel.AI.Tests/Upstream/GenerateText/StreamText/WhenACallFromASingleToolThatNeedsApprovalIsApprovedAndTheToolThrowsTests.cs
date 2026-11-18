// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.GenerateText.GenerateText.ToolExecutionApprovalSupport;
using static Vercel.AI.Tests.Upstream.GenerateText.StreamText.ToolExecutionApprovalStreamSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; tool execution approval &gt; when a call from a single tool that needs approval is approved and the tool throws</c>.</summary>
public sealed class WhenACallFromASingleToolThatNeedsApprovalIsApprovedAndTheToolThrowsTests
{
    private const string Id =
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > tool execution approval > when a call from a single tool that needs approval is approved and the tool throws::";

    [Fact]
    [UpstreamTest(
        Id + "should serialize the tool error as error-text in the continuation prompt",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_serialize_the_tool_error_as_error_text_in_the_continuation_prompt()
    {
        var model = new ScriptedModel(_ => TextStep());
        await Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Tools = new[]
                    {
                        ValueTool((_, _) => throw new JsError("No valid token for plugin")),
                    },
                    ToolApproval = new ToolApprovalConfiguration { { "tool1", "user-approval" } },
                    StopWhen = StopWhen.IsStepCount(3),
                    GenerateId = StreamMockId("id"),
                    NowMs = () => 0,
                    Messages =
                        WhenACallFromASingleToolThatNeedsApprovalIsApprovedTests.ApprovedHistory(),
                }
            )
            .ConsumeStreamAsync();

        var prompt = Assert.Single(model.Prompts);
        Assert.Equal(3, prompt.Count);
        Assert.Equal("test-input", PromptText(prompt));
        AssertToolCallPart(
            Assert.Single(Assert.IsType<AssistantModelMessage>(prompt[1]).Parts),
            "call-1",
            "tool1",
            "{\"value\":\"value\"}"
        );
        var result = Assert.IsType<ToolModelMessage>(prompt[2]);
        Assert.Equal("call-1", result.ToolCallId);
        Assert.Equal("tool1", result.ToolName);
        Assert.Equal("error-text", result.OutputType);
        Assert.Equal("Error: No valid token for plugin", result.OutputJson);
    }
}
