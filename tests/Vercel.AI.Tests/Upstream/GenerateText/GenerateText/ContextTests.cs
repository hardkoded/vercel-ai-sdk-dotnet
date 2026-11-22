// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Testing;
using Vercel.AI.Tests.Upstream.Telemetry;
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

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > context::should include configured runtimeContext properties in telemetry",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_configured_runtimeContext_properties_in_telemetry()
    {
        var contexts = new List<string>();

        await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = _ => TestLanguageModel.Text("Hello, world!"),
                    },
                    RuntimeContext = TelemetryContextSupport.RuntimeContext(),
                    Prompt = "test-input",
                    Telemetry = new TelemetryOptions
                    {
                        IncludeRuntimeContext = new Dictionary<string, bool>
                        {
                            ["requestId"] = true,
                        },
                        Integrations =
                        [
                            TelemetryContextSupport.RuntimeContextIntegration(contexts),
                        ],
                    },
                }
            );

        Assert.Equal(Enumerable.Repeat("{\"requestId\":\"request-123\"}", 4), contexts);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > context::should include configured toolsContext properties in telemetry",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_configured_toolsContext_properties_in_telemetry()
    {
        var contexts = new List<string>();

        await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = _ => TestLanguageModel.Text("Hello, world!"),
                    },
                    Prompt = "test-input",
                    Tools = [TelemetryContextSupport.WeatherTool()],
                    ToolsContext = TelemetryContextSupport.WeatherToolsContext(),
                    Telemetry = new TelemetryOptions
                    {
                        IncludeToolsContext = new Dictionary<
                            string,
                            IReadOnlyDictionary<string, bool>
                        >
                        {
                            ["weather"] = new Dictionary<string, bool> { ["region"] = true },
                        },
                        Integrations = [TelemetryContextSupport.ToolsContextIntegration(contexts)],
                    },
                }
            );

        Assert.Equal(Enumerable.Repeat("{\"weather\":{\"region\":\"eu\"}}", 4), contexts);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > context::should exclude toolsContext properties from telemetry by default",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_exclude_toolsContext_properties_from_telemetry_by_default()
    {
        var contexts = new List<string>();

        await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = _ => TestLanguageModel.Text("Hello, world!"),
                    },
                    Prompt = "test-input",
                    Tools = [TelemetryContextSupport.WeatherTool()],
                    ToolsContext = TelemetryContextSupport.WeatherToolsContext(),
                    Telemetry = new TelemetryOptions
                    {
                        Integrations = [TelemetryContextSupport.ToolsContextIntegration(contexts)],
                    },
                }
            );

        Assert.Equal(Enumerable.Repeat("{}", 4), contexts);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > context::should exclude runtimeContext from telemetry by default",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_exclude_runtimeContext_from_telemetry_by_default()
    {
        var contexts = new List<string>();

        await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = _ => TestLanguageModel.Text("Hello, world!"),
                    },
                    RuntimeContext = TelemetryContextSupport.RuntimeContext(),
                    Prompt = "test-input",
                    Telemetry = new TelemetryOptions
                    {
                        Integrations =
                        [
                            TelemetryContextSupport.RuntimeContextIntegration(contexts),
                        ],
                    },
                }
            );

        Assert.Equal(Enumerable.Repeat("{}", 4), contexts);
    }

    private static LanguageModelGenerateResult ToolCall(string id, string input) =>
        new(
            new GeneratedContent[] { new GeneratedToolCall(id, "t1", input) },
            FinishReason.ToolCalls,
            new LanguageModelUsage(3, 10, 13)
        );
}
