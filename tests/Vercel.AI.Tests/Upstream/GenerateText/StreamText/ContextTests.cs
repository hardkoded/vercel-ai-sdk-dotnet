// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.StreamText;
using Vercel.AI.Tests.Upstream.ToolLoopAgent;
using static Vercel.AI.Tests.Upstream.GenerateText.ToolExecutionCallbackSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; context</c>, the sandbox tests of <c>prepareStep</c>.</summary>
public sealed class ContextTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > context::should pass sandbox to prepareStep",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_sandbox_to_prepareStep()
    {
        var sandbox = new TestSandboxSession();
        ISandboxSession? capturedSandbox = null;

        await ConsumeAsync(
            Client()
                .StreamTextAsync(
                    new StreamTextOptions
                    {
                        Model = new StreamTextAbortSupport.ScriptedStreamModel(() => TextStream()),
                        ExperimentalSandbox = sandbox,
                        PrepareStep = context =>
                        {
                            capturedSandbox = context.ExperimentalSandbox;
                            return null;
                        },
                        Prompt = "test",
                    }
                )
        );

        Assert.Same(sandbox, capturedSandbox);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > context::should use sandbox returned from prepareStep for that step only",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_sandbox_returned_from_prepareStep_for_that_step_only()
    {
        var sandbox = new TestSandboxSession();
        var stepSandbox = new TestSandboxSession();
        var recordedSandboxes = new List<ISandboxSession?>();
        var responseCount = 0;

        var tool = Tool.Function("tool1", null, ValueSchema, null);
        tool.ExecuteWithOptions = (input, options) =>
        {
            recordedSandboxes.Add(options.ExperimentalSandbox);
            return Task.FromResult(
                JsonSerializer.Serialize(new { value = input.GetProperty("value").GetString() })
            );
        };

        await ConsumeAsync(
            Client()
                .StreamTextAsync(
                    new StreamTextOptions
                    {
                        Model = new StreamTextAbortSupport.ScriptedStreamModel(() =>
                            responseCount++ switch
                            {
                                0 => ToolCallStream(("call-1", "{ \"value\": \"first\" }")),
                                1 => ToolCallStream(("call-2", "{ \"value\": \"second\" }")),
                                _ => TextStream(),
                            }
                        ),
                        Tools = [tool],
                        ExperimentalSandbox = sandbox,
                        PrepareStep = context =>
                            context.StepNumber == 0
                                ? new PrepareStepUpdate { ExperimentalSandbox = stepSandbox }
                                : new PrepareStepUpdate(),
                        Prompt = "test-input",
                        StopWhen = StopWhen.IsStepCount(3),
                    }
                )
        );

        Assert.Equal(new[] { stepSandbox, sandbox }, recordedSandboxes);
    }
}
