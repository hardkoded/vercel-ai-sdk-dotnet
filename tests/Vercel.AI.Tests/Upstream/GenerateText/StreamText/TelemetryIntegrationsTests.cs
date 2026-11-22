// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Tests.Upstream.Telemetry;
using static Vercel.AI.Tests.Upstream.GenerateText.StreamText.TelemetryStreamSupport;
using static Vercel.AI.Tests.Upstream.GenerateText.ToolExecutionCallbackSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; telemetry integrations</c>.</summary>
[Collection("TelemetryRegistry")]
public sealed class TelemetryIntegrationsTests : IDisposable
{
    private const string Prefix =
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > telemetry integrations::";

    public TelemetryIntegrationsTests()
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
    public async Task Should_call_per_call_integration_listeners_for_all_lifecycle_events()
    {
        var events = new List<string>();

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = StreamModel(("call-1", "{ \"value\": \"test\" }")),
                    Tools = [Tool1()],
                    Prompt = "test-input",
                    OnError = (_, _) => Task.CompletedTask,
                    Telemetry = new TelemetryOptions
                    {
                        Integrations =
                        [
                            new TestTelemetry
                            {
                                OnStart = (_, _) => Record(events, "onStart"),
                                OnStepStart = (_, _) => Record(events, "onStepStart"),
                                OnToolExecutionStart = (_, _) =>
                                    Record(events, "onToolExecutionStart"),
                                OnToolExecutionEnd = (_, _) => Record(events, "onToolExecutionEnd"),
                                OnStepEnd = (_, _) => Record(events, "onStepEnd"),
                                OnEnd = (_, _) => Record(events, "onEnd"),
                            },
                        ],
                    },
                }
            );

        await ConsumeAsync(result);

        Assert.Equal(
            [
                "onStart",
                "onStepStart",
                "onToolExecutionStart",
                "onToolExecutionEnd",
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
    public async Task Should_call_globally_registered_integration_listeners()
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

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = CreateTestModel(),
                    Prompt = "test-input",
                    OnError = (_, _) => Task.CompletedTask,
                }
            );

        await ConsumeAsync(result);

        Assert.Equal(["global-onStart", "global-onStepEnd", "global-onEnd"], events);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should only call per-call integrations when both global and per-call integrations are provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_only_call_per_call_integrations_when_both_global_and_per_call_integrations_are_provided()
    {
        var events = new List<string>();

        TelemetryRegistry.Integrations =
        [
            new TestTelemetry { OnStart = (_, _) => Record(events, "global") },
        ];

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = CreateTestModel(),
                    Prompt = "test-input",
                    OnError = (_, _) => Task.CompletedTask,
                    Telemetry = new TelemetryOptions
                    {
                        Integrations =
                        [
                            new TestTelemetry { OnStart = (_, _) => Record(events, "per-call") },
                        ],
                    },
                }
            );

        await ConsumeAsync(result);

        Assert.Equal(["per-call"], events);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should call integration listeners alongside user callbacks",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_integration_listeners_alongside_user_callbacks()
    {
        var events = new List<string>();

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = CreateTestModel(),
                    Prompt = "test-input",
                    OnError = (_, _) => Task.CompletedTask,
                    OnStart = (_, _) => Record(events, "user-onStart"),
                    OnStepEnd = (_, _) => Record(events, "user-onStepFinish"),
                    OnEnd = (_, _) => Record(events, "user-onFinish"),
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
                }
            );

        await ConsumeAsync(result);

        Assert.Equal(
            [
                "user-onStart",
                "integration-onStart",
                "user-onStepFinish",
                "integration-onStepEnd",
                "user-onFinish",
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
    public async Task Should_not_break_streaming_when_an_integration_listener_throws()
    {
        static Task Throw() => throw new InvalidOperationException("integration error");

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = CreateTestModel(),
                    Prompt = "test-input",
                    OnError = (_, _) => Task.CompletedTask,
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
                }
            );

        await ConsumeAsync(result);

        Assert.Equal("Hello, world!", await result.Text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should support multiple per-call integrations as an array",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_support_multiple_per_call_integrations_as_an_array()
    {
        var events = new List<string>();

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = CreateTestModel(),
                    Prompt = "test-input",
                    OnError = (_, _) => Task.CompletedTask,
                    Telemetry = new TelemetryOptions
                    {
                        Integrations =
                        [
                            new TestTelemetry { OnStart = (_, _) => Record(events, "first") },
                            new TestTelemetry { OnStart = (_, _) => Record(events, "second") },
                        ],
                    },
                }
            );

        await ConsumeAsync(result);

        Assert.Equal(["first", "second"], events);
    }

    private static Task Record(List<string> events, string name)
    {
        events.Add(name);
        return Task.CompletedTask;
    }
}
