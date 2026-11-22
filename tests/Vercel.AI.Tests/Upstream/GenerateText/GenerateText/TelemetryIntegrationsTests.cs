// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Testing;
using Vercel.AI.Tests.Upstream.Telemetry;
using static Vercel.AI.Tests.Upstream.GenerateText.ToolExecutionCallbackSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; telemetry integrations</c>.</summary>
[Collection("TelemetryRegistry")]
public sealed class TelemetryIntegrationsTests : IDisposable
{
    private const string Prefix =
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > telemetry integrations::";

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

        await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = GenerateModel(("call-1", "{ \"value\": \"test\" }")),
                    Tools = [Tool1()],
                    Prompt = "test-input",
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

        await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = _ => TestLanguageModel.Text("Hello!"),
                    },
                    Prompt = "test-input",
                }
            );

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

        await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = _ => TestLanguageModel.Text("Hello!"),
                    },
                    Prompt = "test-input",
                    Telemetry = new TelemetryOptions
                    {
                        Integrations =
                        [
                            new TestTelemetry { OnStart = (_, _) => Record(events, "per-call") },
                        ],
                    },
                }
            );

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

        await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = _ => TestLanguageModel.Text("Hello!"),
                    },
                    Prompt = "test-input",
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
        Prefix + "should not break generation when an integration listener throws",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_break_generation_when_an_integration_listener_throws()
    {
        static Task Throw() => throw new InvalidOperationException("integration error");

        var result = await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = _ => TestLanguageModel.Text("Hello!"),
                    },
                    Prompt = "test-input",
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

        Assert.Equal("Hello!", result.Text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should support multiple per-call integrations as an array",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_support_multiple_per_call_integrations_as_an_array()
    {
        var events = new List<string>();

        await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = _ => TestLanguageModel.Text("Hello!"),
                    },
                    Prompt = "test-input",
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

        Assert.Equal(["first", "second"], events);
    }

    private static Task Record(List<string> events, string name)
    {
        events.Add(name);
        return Task.CompletedTask;
    }
}
