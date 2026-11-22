// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Tests.Upstream.GenerateText;
using Vercel.AI.Tests.Upstream.GenerateTextSteering;
using Vercel.AI.Tests.Upstream.Telemetry;

namespace Vercel.AI.Tests.Upstream.ToolLoopAgent;

/// <summary>Port of <c>tool-loop-agent.test.ts</c> &gt; <c>ToolLoopAgent</c> &gt; <c>telemetry integrations</c> &gt; <c>stream</c>.</summary>
[Collection("TelemetryRegistry")]
public sealed class TelemetryIntegrationsStreamTests : IDisposable
{
    private const string Prefix =
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > telemetry integrations > stream::";

    public TelemetryIntegrationsStreamTests()
    {
        TelemetryRegistry.Integrations = null;
    }

    public void Dispose()
    {
        TelemetryRegistry.Integrations = null;
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should call per-call integration listeners for all lifecycle events",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldCallPerCallIntegrationListenersForAllLifecycleEvents()
    {
        var events = new List<string>();

        var agent = new Agent(
            new AgentOptions
            {
                Model = ToolLoopAgentToolCalls.StreamModel(out _),
                Tools = [ToolLoopAgentToolCalls.TestTool((value, _) => value + "-result")],
                Telemetry = new TelemetryOptions
                {
                    Integrations =
                    [
                        new TestTelemetry
                        {
                            OnStart = (_, _) => Record(events, "onStart"),
                            OnStepStart = (_, _) => Record(events, "onStepStart"),
                            OnToolExecutionStart = (_, _) => Record(events, "onToolExecutionStart"),
                            OnToolExecutionEnd = (_, _) => Record(events, "onToolExecutionEnd"),
                            OnStepEnd = (_, _) => Record(events, "onStepEnd"),
                            OnEnd = (_, _) => Record(events, "onEnd"),
                        },
                    ],
                },
            },
            ToolLoopAgentInstructions.Client()
        );

        await ToolExecutionCallbackSupport.ConsumeAsync(agent.Stream("test"));

        Assert.Equal(
            [
                "onStart",
                "onStepStart",
                "onToolExecutionStart",
                "onToolExecutionEnd",
                "onStepEnd",
                "onStepStart",
                "onStepEnd",
                "onEnd",
            ],
            events
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should call globally registered integration listeners",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldCallGloballyRegisteredIntegrationListeners()
    {
        var events = new List<string>();

        TelemetryRegistry.Integrations =
        [
            new TestTelemetry
            {
                OnStart = (_, _) => Record(events, "global-onStart"),
                OnStepEnd = (_, _) => Record(events, "global-onStepEnd"),
                OnEnd = (_, _) => Record(events, "global-onEnd"),
            },
        ];

        var agent = new Agent(
            new AgentOptions
            {
                Model = SteeringTestModel.Stream(() => SteeringTestSupport.TextStream("Hello!")),
            },
            ToolLoopAgentInstructions.Client()
        );

        await ToolExecutionCallbackSupport.ConsumeAsync(agent.Stream("test"));

        Assert.Equal(["global-onStart", "global-onStepEnd", "global-onEnd"], events);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include configured runtimeContext properties in telemetry",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldIncludeConfiguredRuntimeContextPropertiesInTelemetry()
    {
        var callbackContexts = new List<string>();
        var telemetryContexts = new List<string>();

        var agent = new Agent(
            new AgentOptions
            {
                Model = SteeringTestModel.Stream(() => SteeringTestSupport.TextStream("Hello!")),
                RuntimeContext = TelemetryContextSupport.RuntimeContext(),
                OnStart = (e, _) =>
                    Record(callbackContexts, ((GenerateTextStartEvent)e).RuntimeContext),
                OnStepEnd = (e, _) => Record(callbackContexts, e.RuntimeContext),
                OnEnd = (e, _) => Record(callbackContexts, e.RuntimeContext),
                Telemetry = new TelemetryOptions
                {
                    IncludeRuntimeContext = new Dictionary<string, bool> { ["requestId"] = true },
                    Integrations =
                    [
                        new TestTelemetry
                        {
                            OnStart = (e, _) =>
                                Record(
                                    telemetryContexts,
                                    ((GenerateTextStartEvent)e).RuntimeContext
                                ),
                            OnStepEnd = (e, _) => Record(telemetryContexts, e.RuntimeContext),
                            OnEnd = (e, _) => Record(telemetryContexts, ((GenerateTextResult)e).RuntimeContext),
                        },
                    ],
                },
            },
            ToolLoopAgentInstructions.Client()
        );

        await ToolExecutionCallbackSupport.ConsumeAsync(agent.Stream("test"));

        Assert.Equal(
            Enumerable.Repeat("{\"userId\":\"user-123\",\"requestId\":\"request-123\"}", 3),
            callbackContexts
        );
        Assert.Equal(Enumerable.Repeat("{\"requestId\":\"request-123\"}", 3), telemetryContexts);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should call integration listeners alongside agent callbacks",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldCallIntegrationListenersAlongsideAgentCallbacks()
    {
        var events = new List<string>();

        var agent = new Agent(
            new AgentOptions
            {
                Model = SteeringTestModel.Stream(() => SteeringTestSupport.TextStream("Hello!")),
                OnStart = (_, _) => Record(events, "agent-onStart"),
                OnStepEnd = (_, _) => Record(events, "agent-onStepFinish"),
                OnEnd = (_, _) => Record(events, "agent-onFinish"),
                Telemetry = new TelemetryOptions
                {
                    Integrations =
                    [
                        new TestTelemetry
                        {
                            OnStart = (_, _) => Record(events, "integration-onStart"),
                            OnStepEnd = (_, _) => Record(events, "integration-onStepEnd"),
                            OnEnd = (_, _) => Record(events, "integration-onEnd"),
                        },
                    ],
                },
            },
            ToolLoopAgentInstructions.Client()
        );

        await ToolExecutionCallbackSupport.ConsumeAsync(agent.Stream("test"));

        Assert.Equal(
            [
                "agent-onStart",
                "integration-onStart",
                "agent-onStepFinish",
                "integration-onStepEnd",
                "agent-onFinish",
                "integration-onEnd",
            ],
            events
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not break streaming when an integration listener throws",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldNotBreakStreamingWhenAnIntegrationListenerThrows()
    {
        static Task Throw() => throw new InvalidOperationException("integration error");

        var agent = new Agent(
            new AgentOptions
            {
                Model = SteeringTestModel.Stream(() => SteeringTestSupport.TextStream("Hello!")),
                Telemetry = new TelemetryOptions
                {
                    Integrations =
                    [
                        new TestTelemetry
                        {
                            OnStart = (_, _) => Throw(),
                            OnStepEnd = (_, _) => Throw(),
                            OnEnd = (_, _) => Throw(),
                        },
                    ],
                },
            },
            ToolLoopAgentInstructions.Client()
        );

        var result = agent.Stream("test");
        await ToolExecutionCallbackSupport.ConsumeAsync(result);

        Assert.Equal("Hello!", await result.Text);
    }

    private static Task Record(List<string> events, string name)
    {
        events.Add(name);
        return Task.CompletedTask;
    }

    private static Task Record(List<string> contexts, object? context)
    {
        contexts.Add(System.Text.Json.JsonSerializer.Serialize(context));
        return Task.CompletedTask;
    }
}
