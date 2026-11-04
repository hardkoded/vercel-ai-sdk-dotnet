// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.GenerateText.GenerateText.ToolExecutionApprovalSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; tool execution approval</c>.</summary>
public sealed class ToolExecutionApprovalTests
{
    private const string Id = "packages/ai/src/generate-text/generate-text.test.ts::generateText > tool execution approval::";

    [Fact]
    [UpstreamTest(Id + "should surface the reason for user approval requests", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_surface_the_reason_for_user_approval_requests()
    {
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = new TestLanguageModel { OnGenerate = _ => ToolCalls("tool1", ("call-1", "{ \"value\": \"value\" }")) },
            Tools = new[] { ValueTool(null) },
            ToolApproval = new ToolApprovalConfiguration
            {
                { "tool1", new ToolApprovalStatus("user-approval", "requires operator review") },
            },
            Prompt = "test-input",
            GenerateId = MockId("id"),
        });

        var request = Assert.Single(result.Content.OfType<GeneratedToolApprovalRequest>());
        Assert.Equal("tool-approval-request", request.Type);
        Assert.Equal("id-1", request.ApprovalId);
        Assert.Equal("requires operator review", request.Reason);

        var assistant = Assert.IsType<AssistantModelMessage>(result.ResponseMessages[0]);
        var part = Assert.Single(assistant.Parts.OfType<AssistantToolApprovalRequestPart>());
        Assert.Equal("id-1", part.ApprovalId);
        Assert.Equal("requires operator review", part.Reason);
    }

    [Fact]
    [UpstreamTest(Id + "should not duplicate an existing execution-denied result", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_duplicate_an_existing_execution_denied_result()
    {
        var model = new TestLanguageModel { OnGenerate = _ => Text("Hello, world!") };

        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Tools = new[] { ValueTool((_, _) => Task.FromResult("result")) },
            ToolApproval = new ToolApprovalConfiguration { { "tool1", "user-approval" } },
            StopWhen = StopWhen.IsStepCount(3),
            GenerateId = MockId("id"),
            Messages = History(
                "tool1",
                new[] { ("call-1", "{\"value\":\"value\"}", "id-1") },
                new ToolApprovalResponseModelMessage("id-1", false, "User denied the request"),
                new ToolModelMessage("call-1", "tool1", "User denied the request", false, "execution-denied")),
        });

        var promptOccurrences = model.Calls.SelectMany(call => call.Prompt).OfType<ToolModelMessage>().SelectMany(message => message.Results).Count(part => part.OutputType == "execution-denied");
        var responseOccurrences = result.ResponseMessages.OfType<ToolModelMessage>().SelectMany(message => message.Results).Count(part => part.OutputType == "execution-denied");

        Assert.Equal(1, promptOccurrences);
        Assert.Equal(0, responseOccurrences);
    }
}
