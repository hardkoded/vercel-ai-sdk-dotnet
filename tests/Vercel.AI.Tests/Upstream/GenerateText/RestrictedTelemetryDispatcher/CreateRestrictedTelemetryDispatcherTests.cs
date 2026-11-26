// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.Telemetry;

namespace Vercel.AI.Tests.Upstream.GenerateText.RestrictedTelemetryDispatcher;

/// <summary>Port of <c>restricted-telemetry-dispatcher.test.ts</c> &gt; <c>createRestrictedTelemetryDispatcher</c>.</summary>
public sealed class CreateRestrictedTelemetryDispatcherTests
{
    private const string Id =
        "packages/ai/src/generate-text/restricted-telemetry-dispatcher.test.ts::createRestrictedTelemetryDispatcher::";

    private static readonly IReadOnlyDictionary<string, object?> RuntimeContext = new Dictionary<
        string,
        object?
    >
    {
        ["userId"] = "user-123",
        ["requestId"] = "request-123",
    };

    private static readonly IReadOnlyDictionary<string, bool> IncludeRuntimeContext =
        new Dictionary<string, bool> { ["requestId"] = true };

    private static readonly IReadOnlyDictionary<string, JsonElement> ToolsContext = new Dictionary<
        string,
        JsonElement
    >
    {
        ["weather"] = Json("""{"apiKey":"secret-api-key","city":"Berlin"}"""),
        ["stocks"] = Json("""{"symbol":"AI"}"""),
    };

    private static readonly IReadOnlyDictionary<
        string,
        IReadOnlyDictionary<string, bool>
    > IncludeToolsContext = new Dictionary<string, IReadOnlyDictionary<string, bool>>
    {
        ["weather"] = new Dictionary<string, bool> { ["city"] = true },
        ["stocks"] = new Dictionary<string, bool> { ["symbol"] = true },
    };

    private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();

    private static void AssertFilteredToolsContext(IReadOnlyDictionary<string, JsonElement>? actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(2, actual.Count);
        Assert.Equal("""{"city":"Berlin"}""", actual["weather"].GetRawText());
        Assert.Equal("""{"symbol":"AI"}""", actual["stocks"].GetRawText());
    }

    private static void AssertRequestIdOnly(IReadOnlyDictionary<string, object?>? actual)
    {
        Assert.NotNull(actual);
        var entry = Assert.Single(actual);
        Assert.Equal("requestId", entry.Key);
        Assert.Equal("request-123", entry.Value);
    }

    private static StepResult CreateStepResult(
        IReadOnlyDictionary<string, JsonElement>? toolContexts = null
    ) =>
        new(
            "Hello",
            null,
            [],
            [],
            FinishReason.Stop,
            LanguageModelUsage.Null,
            [],
            provider: "test-provider",
            modelId: "test-model"
        )
        {
            CallId = "call-1",
            RuntimeContext = RuntimeContext,
            ToolsContext = toolContexts ?? new Dictionary<string, JsonElement>(),
        };

    private static GeneratedToolCall WeatherCall() =>
        new("tool-call-1", "weather", """{"value":"input"}""");

    [Fact]
    [UpstreamTest(Id + "excludes runtimeContext when no include context is configured")]
    public async Task Excludes_runtimeContext_when_no_include_context_is_configured()
    {
        GenerateTextStartEvent? received = null;
        var telemetryDispatcher =
            GenerateTextRestrictedTelemetryDispatcher.CreateRestrictedTelemetryDispatcher(
                new TelemetryOptions
                {
                    Integrations =
                    [
                        new TestTelemetry
                        {
                            OnStart = (e, _) =>
                            {
                                received = (GenerateTextStartEvent)e;
                                return Task.CompletedTask;
                            },
                        },
                    ],
                }
            );
        var source = new GenerateTextStartEvent { RuntimeContext = RuntimeContext };

        await telemetryDispatcher.OnStartAsync(source, CancellationToken.None);

        Assert.NotNull(received);
        Assert.NotSame(RuntimeContext, received.RuntimeContext);
        Assert.Empty(received.RuntimeContext!);
    }

    [Fact]
    [UpstreamTest(Id + "only includes runtimeContext properties marked as true")]
    public async Task Only_includes_runtimeContext_properties_marked_as_true()
    {
        GenerateTextStartEvent? received = null;
        var telemetryDispatcher =
            GenerateTextRestrictedTelemetryDispatcher.CreateRestrictedTelemetryDispatcher(
                new TelemetryOptions
                {
                    Integrations = [OnStart(e => received = e)],
                    IncludeRuntimeContext = new Dictionary<string, bool>
                    {
                        ["userId"] = false,
                        ["requestId"] = true,
                    },
                }
            );

        await telemetryDispatcher.OnStartAsync(
            new GenerateTextStartEvent { RuntimeContext = RuntimeContext },
            CancellationToken.None
        );

        AssertRequestIdOnly(received!.RuntimeContext);
    }

    [Fact]
    [UpstreamTest(
        Id + "includes configured runtimeContext for start events without mutating the source event"
    )]
    public async Task Includes_configured_runtimeContext_for_start_events_without_mutating_the_source_event()
    {
        GenerateTextStartEvent? received = null;
        var telemetryDispatcher =
            GenerateTextRestrictedTelemetryDispatcher.CreateRestrictedTelemetryDispatcher(
                new TelemetryOptions
                {
                    Integrations = [OnStart(e => received = e)],
                    IncludeRuntimeContext = IncludeRuntimeContext,
                }
            );
        var source = new GenerateTextStartEvent { RuntimeContext = RuntimeContext };

        await telemetryDispatcher.OnStartAsync(source, CancellationToken.None);

        AssertRequestIdOnly(received!.RuntimeContext);
        Assert.Equal(2, source.RuntimeContext!.Count);
        Assert.Equal("user-123", source.RuntimeContext["userId"]);
    }

    [Fact]
    [UpstreamTest(
        Id + "filters toolsContext per tool for start events without mutating the source event"
    )]
    public async Task Filters_toolsContext_per_tool_for_start_events_without_mutating_the_source_event()
    {
        GenerateTextStartEvent? received = null;
        var telemetryDispatcher =
            GenerateTextRestrictedTelemetryDispatcher.CreateRestrictedTelemetryDispatcher(
                new TelemetryOptions
                {
                    Integrations = [OnStart(e => received = e)],
                    IncludeToolsContext = IncludeToolsContext,
                }
            );
        var source = new GenerateTextStartEvent
        {
            RuntimeContext = RuntimeContext,
            ToolsContext = ToolsContext,
        };

        await telemetryDispatcher.OnStartAsync(source, CancellationToken.None);

        AssertFilteredToolsContext(received!.ToolsContext);
        Assert.Equal(
            """{"apiKey":"secret-api-key","city":"Berlin"}""",
            source.ToolsContext!["weather"].GetRawText()
        );
    }

    [Fact]
    [UpstreamTest(Id + "excludes toolsContext properties when no include context is configured")]
    public async Task Excludes_toolsContext_properties_when_no_include_context_is_configured()
    {
        GenerateTextStartEvent? received = null;
        var telemetryDispatcher =
            GenerateTextRestrictedTelemetryDispatcher.CreateRestrictedTelemetryDispatcher(
                new TelemetryOptions { Integrations = [OnStart(e => received = e)] }
            );

        await telemetryDispatcher.OnStartAsync(
            new GenerateTextStartEvent
            {
                RuntimeContext = RuntimeContext,
                ToolsContext = ToolsContext,
            },
            CancellationToken.None
        );

        Assert.Empty(received!.ToolsContext!);
    }

    [Fact]
    [UpstreamTest(
        Id + "includes configured runtimeContext for step start events and previous steps"
    )]
    public async Task Includes_configured_runtimeContext_for_step_start_events_and_previous_steps()
    {
        GenerateTextStepStartEvent? received = null;
        var telemetryDispatcher =
            GenerateTextRestrictedTelemetryDispatcher.CreateRestrictedTelemetryDispatcher(
                new TelemetryOptions
                {
                    Integrations =
                    [
                        new TestTelemetry
                        {
                            OnStepStart = Capture<GenerateTextStepStartEvent>(e => received = e),
                        },
                    ],
                    IncludeRuntimeContext = IncludeRuntimeContext,
                }
            );
        var previousStep = CreateStepResult();

        await telemetryDispatcher.OnStepStartAsync(
            new GenerateTextStepStartEvent
            {
                RuntimeContext = RuntimeContext,
                Steps = [previousStep],
            },
            CancellationToken.None
        );

        AssertRequestIdOnly(received!.RuntimeContext);
        AssertRequestIdOnly(received.Steps[0].RuntimeContext);
        Assert.Equal("Hello", received.Steps[0].Text);
        Assert.Equal(2, previousStep.RuntimeContext.Count);
    }

    [Fact]
    [UpstreamTest(Id + "filters toolsContext for step start events and previous steps")]
    public async Task Filters_toolsContext_for_step_start_events_and_previous_steps()
    {
        GenerateTextStepStartEvent? received = null;
        var telemetryDispatcher =
            GenerateTextRestrictedTelemetryDispatcher.CreateRestrictedTelemetryDispatcher(
                new TelemetryOptions
                {
                    Integrations =
                    [
                        new TestTelemetry
                        {
                            OnStepStart = Capture<GenerateTextStepStartEvent>(e => received = e),
                        },
                    ],
                    IncludeToolsContext = IncludeToolsContext,
                }
            );
        var previousStep = CreateStepResult(ToolsContext);

        await telemetryDispatcher.OnStepStartAsync(
            new GenerateTextStepStartEvent
            {
                RuntimeContext = RuntimeContext,
                ToolsContext = ToolsContext,
                Steps = [previousStep],
            },
            CancellationToken.None
        );

        AssertFilteredToolsContext(received!.ToolsContext);
        AssertFilteredToolsContext(received.Steps[0].ToolsContext);
        Assert.Equal(
            """{"apiKey":"secret-api-key","city":"Berlin"}""",
            previousStep.ToolsContext["weather"].GetRawText()
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "filters runtimeContext and toolsContext for step error events and keeps the error",
        Commit = "cd1c98ed42c3eb3463f6f69eab45a269f6dff1ac"
    )]
    public async Task Filters_runtimeContext_and_toolsContext_for_step_error_events_and_keeps_the_error()
    {
        GenerateTextStepErrorEvent? received = null;
        var telemetryDispatcher =
            GenerateTextRestrictedTelemetryDispatcher.CreateRestrictedTelemetryDispatcher(
                new TelemetryOptions
                {
                    Integrations =
                    [
                        new TestTelemetry
                        {
                            OnStepError = (e, _) =>
                            {
                                received = e;
                                return Task.CompletedTask;
                            },
                        },
                    ],
                    IncludeRuntimeContext = IncludeRuntimeContext,
                    IncludeToolsContext = IncludeToolsContext,
                }
            );
        var previousStep = CreateStepResult(ToolsContext);
        var error = new Exception("step failure");

        await telemetryDispatcher.OnStepErrorAsync(
            new GenerateTextStepErrorEvent(
                new GenerateTextStepStartEvent
                {
                    RuntimeContext = RuntimeContext,
                    ToolsContext = ToolsContext,
                    Steps = [previousStep],
                },
                error
            ),
            CancellationToken.None
        );

        AssertRequestIdOnly(received!.RuntimeContext);
        AssertFilteredToolsContext(received.ToolsContext);
        AssertRequestIdOnly(received.Steps[0].RuntimeContext);
        Assert.Same(error, received.Error);
        Assert.Equal(2, previousStep.RuntimeContext.Count);
    }

    [Fact]
    [UpstreamTest(
        Id
            + "includes configured runtimeContext for step end events without mutating the source step"
    )]
    public async Task Includes_configured_runtimeContext_for_step_end_events_without_mutating_the_source_step()
    {
        StepResult? received = null;
        var telemetryDispatcher =
            GenerateTextRestrictedTelemetryDispatcher.CreateRestrictedTelemetryDispatcher(
                new TelemetryOptions
                {
                    Integrations =
                    [
                        new TestTelemetry { OnStepEnd = Capture<StepResult>(e => received = e) },
                    ],
                    IncludeRuntimeContext = IncludeRuntimeContext,
                }
            );
        var step = CreateStepResult();

        await telemetryDispatcher.OnStepEndAsync(step, CancellationToken.None);

        AssertRequestIdOnly(received!.RuntimeContext);
        Assert.Equal("Hello", received.Text);
        Assert.Equal(2, step.RuntimeContext.Count);
    }

    [Fact]
    [UpstreamTest(Id + "filters toolsContext for step end events without mutating the source step")]
    public async Task Filters_toolsContext_for_step_end_events_without_mutating_the_source_step()
    {
        StepResult? received = null;
        var telemetryDispatcher =
            GenerateTextRestrictedTelemetryDispatcher.CreateRestrictedTelemetryDispatcher(
                new TelemetryOptions
                {
                    Integrations =
                    [
                        new TestTelemetry { OnStepEnd = Capture<StepResult>(e => received = e) },
                    ],
                    IncludeToolsContext = IncludeToolsContext,
                }
            );
        var step = CreateStepResult(ToolsContext);

        await telemetryDispatcher.OnStepEndAsync(step, CancellationToken.None);

        AssertFilteredToolsContext(received!.ToolsContext);
        Assert.Equal("Hello", received.Text);
        Assert.Equal(
            """{"apiKey":"secret-api-key","city":"Berlin"}""",
            step.ToolsContext["weather"].GetRawText()
        );
    }

    [Fact]
    [UpstreamTest(
        Id
            + "includes configured runtimeContext for end events and all steps without mutating source steps"
    )]
    public async Task Includes_configured_runtimeContext_for_end_events_and_all_steps_without_mutating_source_steps()
    {
        GenerateTextResult? received = null;
        var telemetryDispatcher =
            GenerateTextRestrictedTelemetryDispatcher.CreateRestrictedTelemetryDispatcher(
                new TelemetryOptions
                {
                    Integrations = [OnEnd(e => received = (GenerateTextResult)e)],
                    IncludeRuntimeContext = IncludeRuntimeContext,
                }
            );
        var step = CreateStepResult();

        await telemetryDispatcher.OnEndAsync(
            new GenerateTextResult(
                "Hello",
                null,
                [step],
                FinishReason.Stop,
                LanguageModelUsage.Null,
                null,
                []
            ),
            CancellationToken.None
        );

        AssertRequestIdOnly(received!.RuntimeContext);
        AssertRequestIdOnly(received.Steps[0].RuntimeContext);
        Assert.Same(received.Steps[0], received.FinalStep);
        Assert.Equal("Hello", received.Text);
        Assert.Equal(2, step.RuntimeContext.Count);
    }

    [Fact]
    [UpstreamTest(
        Id + "filters toolsContext for end events and all steps without mutating source steps"
    )]
    public async Task Filters_toolsContext_for_end_events_and_all_steps_without_mutating_source_steps()
    {
        GenerateTextResult? received = null;
        var telemetryDispatcher =
            GenerateTextRestrictedTelemetryDispatcher.CreateRestrictedTelemetryDispatcher(
                new TelemetryOptions
                {
                    Integrations = [OnEnd(e => received = (GenerateTextResult)e)],
                    IncludeToolsContext = IncludeToolsContext,
                }
            );
        var step = CreateStepResult(ToolsContext);

        await telemetryDispatcher.OnEndAsync(
            new GenerateTextResult(
                "Hello",
                null,
                [step],
                FinishReason.Stop,
                LanguageModelUsage.Null,
                null,
                []
            ),
            CancellationToken.None
        );

        AssertFilteredToolsContext(received!.ToolsContext);
        AssertFilteredToolsContext(received.Steps[0].ToolsContext);
        Assert.Same(received.Steps[0], received.FinalStep);
        Assert.Equal("Hello", received.Text);
        Assert.Equal(
            """{"apiKey":"secret-api-key","city":"Berlin"}""",
            step.ToolsContext["weather"].GetRawText()
        );
    }

    [Fact]
    [UpstreamTest(
        Id
            + "includes configured runtimeContext for abort events and all steps without mutating source steps"
    )]
    public async Task Includes_configured_runtimeContext_for_abort_events_and_all_steps_without_mutating_source_steps()
    {
        StreamAbortContext? received = null;
        var telemetryDispatcher =
            GenerateTextRestrictedTelemetryDispatcher.CreateRestrictedTelemetryDispatcher(
                new TelemetryOptions
                {
                    Integrations =
                    [
                        new TestTelemetry
                        {
                            OnAbort = (e, _) =>
                            {
                                received = e;
                                return Task.CompletedTask;
                            },
                        },
                    ],
                    IncludeRuntimeContext = IncludeRuntimeContext,
                }
            );
        var step = CreateStepResult();
        var reason = new OperationCanceledException("manual abort");

        await telemetryDispatcher.OnAbortAsync(
            new StreamAbortContext([step], reason, "call-1"),
            CancellationToken.None
        );

        Assert.Same(reason, received!.Exception);
        AssertRequestIdOnly(received.Steps[0].RuntimeContext);
        Assert.Equal("Hello", received.Steps[0].Text);
        Assert.Equal(2, step.RuntimeContext.Count);
    }

    [Fact]
    [UpstreamTest(Id + "filters tool execution start events without mutating the source event")]
    public async Task Filters_tool_execution_start_events_without_mutating_the_source_event()
    {
        ToolExecutionStartEvent? received = null;
        var telemetryDispatcher =
            GenerateTextRestrictedTelemetryDispatcher.CreateRestrictedTelemetryDispatcher(
                new TelemetryOptions
                {
                    Integrations =
                    [
                        new TestTelemetry
                        {
                            OnToolExecutionStart = (e, _) =>
                            {
                                received = e;
                                return Task.CompletedTask;
                            },
                        },
                    ],
                    IncludeRuntimeContext = IncludeRuntimeContext,
                    IncludeToolsContext = IncludeToolsContext,
                }
            );
        var source = new ToolExecutionStartEvent(WeatherCall(), [])
        {
            CallId = "call-1",
            ToolContext = ToolsContext["weather"],
        };

        await telemetryDispatcher.OnToolExecutionStartAsync(source, CancellationToken.None);

        Assert.Equal("call-1", received!.CallId);
        Assert.Same(source.ToolCall, received.ToolCall);
        Assert.Equal("""{"city":"Berlin"}""", received.ToolContext!.Value.GetRawText());
        Assert.Equal(
            """{"apiKey":"secret-api-key","city":"Berlin"}""",
            source.ToolContext!.Value.GetRawText()
        );
    }

    [Fact]
    [UpstreamTest(Id + "filters tool execution end events without mutating the source event")]
    public async Task Filters_tool_execution_end_events_without_mutating_the_source_event()
    {
        ToolExecutionEndEvent? received = null;
        var telemetryDispatcher =
            GenerateTextRestrictedTelemetryDispatcher.CreateRestrictedTelemetryDispatcher(
                new TelemetryOptions
                {
                    Integrations =
                    [
                        new TestTelemetry
                        {
                            OnToolExecutionEnd = (e, _) =>
                            {
                                received = e;
                                return Task.CompletedTask;
                            },
                        },
                    ],
                    IncludeRuntimeContext = IncludeRuntimeContext,
                    IncludeToolsContext = IncludeToolsContext,
                }
            );
        var source = new ToolExecutionEndEvent(
            WeatherCall(),
            [],
            new ExecutedTool("tool-call-1", "weather", """{"value":"output"}""", false),
            TimeSpan.FromMilliseconds(10)
        )
        {
            CallId = "call-1",
            ToolContext = ToolsContext["weather"],
        };

        await telemetryDispatcher.OnToolExecutionEndAsync(source, CancellationToken.None);

        Assert.Equal("call-1", received!.CallId);
        Assert.Same(source.ToolCall, received.ToolCall);
        Assert.Same(source.ToolOutput, received.ToolOutput);
        Assert.Equal(TimeSpan.FromMilliseconds(10), received.ToolExecutionTime);
        Assert.Equal("""{"city":"Berlin"}""", received.ToolContext!.Value.GetRawText());
        Assert.Equal(
            """{"apiKey":"secret-api-key","city":"Berlin"}""",
            source.ToolContext!.Value.GetRawText()
        );
    }

    [Fact]
    [UpstreamTest(Id + "passes through executeTool without filtering")]
    public async Task Passes_through_executeTool_without_filtering()
    {
        var wrapper = new PassThroughExecuteTool();
        var telemetryDispatcher =
            GenerateTextRestrictedTelemetryDispatcher.CreateRestrictedTelemetryDispatcher(
                new TelemetryOptions
                {
                    Integrations = [new TestTelemetry { ExecuteTool = wrapper }],
                    IncludeRuntimeContext = IncludeRuntimeContext,
                }
            );

        var result = await telemetryDispatcher.ExecuteToolAsync(
            new ToolExecutionStartEvent(WeatherCall(), []) { CallId = "call-1" },
            () => Task.FromResult("result"),
            CancellationToken.None
        );

        Assert.Equal("result", result);
        Assert.Equal(1, wrapper.Calls);
    }

    private static TestTelemetry OnStart(Action<GenerateTextStartEvent> capture) =>
        new()
        {
            OnStart = (e, _) =>
            {
                capture((GenerateTextStartEvent)e);
                return Task.CompletedTask;
            },
        };

    private static TestTelemetry OnEnd(Action<IOperationEndEvent> capture) =>
        new()
        {
            OnEnd = (e, _) =>
            {
                capture(e);
                return Task.CompletedTask;
            },
        };

    private static Func<T, CancellationToken, Task> Capture<T>(Action<T> capture) =>
        (e, _) =>
        {
            capture(e);
            return Task.CompletedTask;
        };

    private sealed class PassThroughExecuteTool : IExecuteTool
    {
        public int Calls { get; private set; }

        public Task<T> ExecuteAsync<T>(
            ExecuteToolOptions<T> options,
            CancellationToken cancellationToken
        )
        {
            Calls++;
            return options.Execute();
        }
    }
}
