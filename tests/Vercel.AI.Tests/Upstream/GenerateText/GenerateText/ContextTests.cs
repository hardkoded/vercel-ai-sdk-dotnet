// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Testing;
using Vercel.AI.Tests.Upstream.ToolLoopAgent;
using static Vercel.AI.Tests.Upstream.GenerateText.ToolExecutionCallbackSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; context</c>, the sandbox tests of <c>prepareStep</c>.</summary>
public sealed class ContextTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > context::should pass sandbox to prepareStep",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_sandbox_to_prepareStep()
    {
        var sandbox = new TestSandboxSession();
        ISandboxSession? capturedSandbox = null;

        await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = _ => TestLanguageModel.Text("Hello, world!"),
                    },
                    ExperimentalSandbox = sandbox,
                    PrepareStep = context =>
                    {
                        capturedSandbox = context.ExperimentalSandbox;
                        return null;
                    },
                    Prompt = "test",
                }
            );

        Assert.Same(sandbox, capturedSandbox);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > context::should use sandbox returned from prepareStep for that step only",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_sandbox_returned_from_prepareStep_for_that_step_only()
    {
        var sandbox = new TestSandboxSession();
        var stepSandbox = new TestSandboxSession();
        var recordedSandboxes = new List<ISandboxSession?>();
        var responseCount = 0;

        var tool = Tool.Function("t1", null, ValueSchema, null);
        tool.ExecuteWithOptions = (input, options) =>
        {
            recordedSandboxes.Add(options.ExperimentalSandbox);
            return Task.FromResult(
                JsonSerializer.Serialize(new { value = input.GetProperty("value").GetString() })
            );
        };

        await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = _ =>
                            responseCount++ switch
                            {
                                0 => ToolCall("call-1", "{ \"value\": \"first\" }"),
                                1 => ToolCall("call-2", "{ \"value\": \"second\" }"),
                                _ => TestLanguageModel.Text("done"),
                            },
                    },
                    Tools = [tool],
                    ExperimentalSandbox = sandbox,
                    PrepareStep = context =>
                        context.StepNumber == 0
                            ? new PrepareStepUpdate { ExperimentalSandbox = stepSandbox }
                            : new PrepareStepUpdate(),
                    Prompt = "test-input",
                    StopWhen = StopWhen.IsStepCount(3),
                }
            );

        Assert.Equal(new[] { stepSandbox, sandbox }, recordedSandboxes);
    }

    private static LanguageModelGenerateResult ToolCall(string id, string input) =>
        new(
            new GeneratedContent[] { new GeneratedToolCall(id, "t1", input) },
            FinishReason.ToolCalls,
            new LanguageModelUsage(3, 10, 13)
        );
}
