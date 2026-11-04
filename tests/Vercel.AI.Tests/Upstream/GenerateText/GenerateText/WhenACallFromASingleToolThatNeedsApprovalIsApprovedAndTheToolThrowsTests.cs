// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.GenerateText.GenerateText.ToolExecutionApprovalSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; tool execution approval &gt; when a call from a single tool that needs approval is approved and the tool throws</c>.</summary>
public sealed class WhenACallFromASingleToolThatNeedsApprovalIsApprovedAndTheToolThrowsTests
{
    private const string Id = "packages/ai/src/generate-text/generate-text.test.ts::generateText > tool execution approval > when a call from a single tool that needs approval is approved and the tool throws::";

    [Fact]
    [UpstreamTest(Id + "should serialize the tool error as error-text in the continuation prompt", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_serialize_the_tool_error_as_error_text_in_the_continuation_prompt()
    {
        var model = new TestLanguageModel { OnGenerate = _ => Text("Hello, world!") };

        await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Tools = new[] { ValueTool((_, _) => throw new JsError("No valid token for plugin")) },
            ToolApproval = new ToolApprovalConfiguration { { "tool1", "user-approval" } },
            StopWhen = StopWhen.IsStepCount(3),
            GenerateId = MockId("id"),
            Messages = WhenACallFromASingleToolThatNeedsApprovalIsApprovedTests.ApprovedHistory(),
        });

        var prompt = Assert.Single(model.Calls).Prompt;
        Assert.Equal(3, prompt.Count);
        Assert.Equal("test-input", PromptText(prompt));
        var assistant = Assert.IsType<AssistantModelMessage>(prompt[1]);
        AssertToolCallPart(Assert.Single(assistant.Parts), "call-1", "tool1", "{\"value\":\"value\"}");
        AssertToolResult(Assert.IsType<ToolModelMessage>(prompt[2]), "call-1", "tool1", "error-text", "Error: No valid token for plugin");
    }
}
