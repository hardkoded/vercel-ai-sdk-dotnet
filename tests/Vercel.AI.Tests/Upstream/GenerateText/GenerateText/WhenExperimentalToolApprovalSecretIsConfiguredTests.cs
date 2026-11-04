// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.GenerateText.GenerateText.ToolExecutionApprovalSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; tool execution approval &gt; when experimental_toolApprovalSecret is configured</c>.</summary>
public sealed class WhenExperimentalToolApprovalSecretIsConfiguredTests
{
    private const string Id = "packages/ai/src/generate-text/generate-text.test.ts::generateText > tool execution approval > when experimental_toolApprovalSecret is configured::";

    private const string TestSecret = "test-hmac-secret-do-not-use-in-production";

    [Fact]
    [UpstreamTest(Id + "should execute the tool when the signature is valid", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_execute_the_tool_when_the_signature_is_valid()
    {
        var executeCalls = 0;
        using var input = JsonDocument.Parse("{\"value\":\"test\"}");
        var signature = ToolApprovalSignatures.SignToolApproval(TestSecret, "approval-1", "call-1", "tool1", input.RootElement);

        await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = new TestLanguageModel { OnGenerate = _ => Text("done") },
            Tools = new[] { Counting(() => executeCalls++) },
            ToolApproval = new ToolApprovalConfiguration { { "tool1", "user-approval" } },
            ExperimentalToolApprovalSecret = TestSecret,
            StopWhen = StopWhen.IsStepCount(3),
            Messages = Messages("{\"value\":\"test\"}", signature),
        });

        Assert.Equal(1, executeCalls);
    }

    [Fact]
    [UpstreamTest(Id + "should reject when the signature is missing", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_reject_when_the_signature_is_missing()
    {
        var executeCalls = 0;

        var error = await Assert.ThrowsAsync<InvalidToolApprovalSignatureError>(() => Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = new TestLanguageModel { OnGenerate = _ => throw new InvalidOperationException("model should not be called") },
            Tools = new[] { Counting(() => executeCalls++) },
            ToolApproval = new ToolApprovalConfiguration { { "tool1", "user-approval" } },
            ExperimentalToolApprovalSecret = TestSecret,
            StopWhen = StopWhen.IsStepCount(3),
            Messages = Messages("{\"value\":\"test\"}", null),
        }));

        Assert.Matches("missing signature", error.Message);
        Assert.Equal(0, executeCalls);
    }

    [Fact]
    [UpstreamTest(Id + "should reject when the input was tampered after signing", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_reject_when_the_input_was_tampered_after_signing()
    {
        var executeCalls = 0;
        using var original = JsonDocument.Parse("{\"value\":\"original\"}");
        var signature = ToolApprovalSignatures.SignToolApproval(TestSecret, "approval-1", "call-1", "tool1", original.RootElement);

        var error = await Assert.ThrowsAsync<InvalidToolApprovalSignatureError>(() => Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = new TestLanguageModel { OnGenerate = _ => throw new InvalidOperationException("model should not be called") },
            Tools = new[] { Counting(() => executeCalls++) },
            ToolApproval = new ToolApprovalConfiguration { { "tool1", "user-approval" } },
            ExperimentalToolApprovalSecret = TestSecret,
            StopWhen = StopWhen.IsStepCount(3),
            Messages = Messages("{\"value\":\"tampered\"}", signature),
        }));

        Assert.Matches("invalid signature", error.Message);
        Assert.Equal(0, executeCalls);
    }

    [Fact]
    [UpstreamTest(Id + "should work without a secret (backward compatible)", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_work_without_a_secret_backward_compatible()
    {
        var executeCalls = 0;

        await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = new TestLanguageModel { OnGenerate = _ => Text("done") },
            Tools = new[] { Counting(() => executeCalls++) },
            ToolApproval = new ToolApprovalConfiguration { { "tool1", "user-approval" } },
            // no ExperimentalToolApprovalSecret
            StopWhen = StopWhen.IsStepCount(3),
            Messages = Messages("{\"value\":\"test\"}", "unverified-signature"),
        });

        Assert.Equal(1, executeCalls);
    }

    private static Tool Counting(Action onExecute) =>
        ValueTool((_, _) =>
        {
            onExecute();
            return Task.FromResult("result1");
        });

    private static List<ModelMessage> Messages(string input, string? signature) =>
        new()
        {
            new UserModelMessage("test"),
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantToolCallPart(new GeneratedToolCall("call-1", "tool1", input)),
                new AssistantToolApprovalRequestPart("approval-1", "call-1") { Signature = signature },
            }),
            new ToolApprovalResponseModelMessage("approval-1", true),
        };
}
