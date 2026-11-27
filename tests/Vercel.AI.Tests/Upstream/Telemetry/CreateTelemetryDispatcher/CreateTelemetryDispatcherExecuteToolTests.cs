// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.Telemetry.CreateTelemetryDispatcher;

/// <summary>Port of <c>create-telemetry-dispatcher.test.ts</c> &gt; <c>createTelemetryDispatcher &gt; executeTool</c>.</summary>
[Collection("TelemetryRegistry")]
public sealed class CreateTelemetryDispatcherExecuteToolTests : IDisposable
{
    private const string Id =
        "packages/ai/src/telemetry/create-telemetry-dispatcher.test.ts::createTelemetryDispatcher > executeTool::";

    public CreateTelemetryDispatcherExecuteToolTests()
    {
        Vercel.AI.TelemetryRegistry.Integrations = null;
    }

    public void Dispose()
    {
        Vercel.AI.TelemetryRegistry.Integrations = null;
    }

    private static ToolExecutionStartEvent Event() =>
        new(DispatcherEvents.ToolCall(), []) { CallId = "call-1" };

    [Fact]
    [UpstreamTest(
        Id + "passes through when no integrations implement executeTool",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Passes_through_when_no_integrations_implement_executeTool()
    {
        var telemetry = Vercel.AI.TelemetryDispatcher.Create(
            CreateTelemetryDispatcherTests.Options(
                new TestTelemetry { OnStart = (_, _) => Task.CompletedTask }
            )
        );

        Assert.Equal(
            "result",
            await telemetry.ExecuteToolAsync(
                Event(),
                () => Task.FromResult("result"),
                CancellationToken.None
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "wraps execute with a single integration",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Wraps_execute_with_a_single_integration()
    {
        var executed = 0;
        var wrapper = new Wrapper("wrapper", []);
        var telemetry = Vercel.AI.TelemetryDispatcher.Create(
            CreateTelemetryDispatcherTests.Options(new TestTelemetry { ExecuteTool = wrapper })
        );

        var result = await telemetry.ExecuteToolAsync(
            Event(),
            () =>
            {
                executed++;
                return Task.FromResult("result");
            },
            CancellationToken.None
        );

        Assert.Equal("wrapper:result", result);
        Assert.Equal(1, wrapper.Calls);
        Assert.Equal("call-1", wrapper.CallId);
        Assert.Equal("tool-1", wrapper.ToolCallId);
        Assert.Equal(Vercel.AI.Operations.AiSdkVersion.Version, wrapper.Version);
        Assert.Equal(1, executed);
    }

    [Fact]
    [UpstreamTest(
        Id + "uses only local executeTool when provided, ignoring global",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Uses_only_local_executeTool_when_provided_ignoring_global()
    {
        var order = new List<string>();
        Vercel.AI.TelemetryRegistry.RegisterTelemetry(
            new TestTelemetry { ExecuteTool = new Wrapper("global", order) }
        );
        var telemetry = Vercel.AI.TelemetryDispatcher.Create(
            CreateTelemetryDispatcherTests.Options(
                new TestTelemetry { ExecuteTool = new Wrapper("local", order) }
            )
        );

        var result = await telemetry.ExecuteToolAsync(
            Event(),
            () =>
            {
                order.Add("execute");
                return Task.FromResult("done");
            },
            CancellationToken.None
        );

        Assert.Equal("local:done", result);
        Assert.Equal(["local-before", "execute", "local-after"], order);
    }

    [Fact]
    [UpstreamTest(
        Id + "uses global executeTool when no local integrations are provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Uses_global_executeTool_when_no_local_integrations_are_provided()
    {
        var order = new List<string>();
        Vercel.AI.TelemetryRegistry.RegisterTelemetry(
            new TestTelemetry { ExecuteTool = new Wrapper("global", order) }
        );
        var telemetry = Vercel.AI.TelemetryDispatcher.Create(new TelemetryOptions());

        var result = await telemetry.ExecuteToolAsync(
            Event(),
            () =>
            {
                order.Add("execute");
                return Task.FromResult("done");
            },
            CancellationToken.None
        );

        Assert.Equal("global:done", result);
        Assert.Equal(["global-before", "execute", "global-after"], order);
    }

    [Fact]
    [UpstreamTest(
        Id + "auto-binds class-based executeTool integrations",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Auto_binds_class_based_executeTool_integrations()
    {
        var integration = new CountingWrapper();
        var telemetry = Vercel.AI.TelemetryDispatcher.Create(
            CreateTelemetryDispatcherTests.Options(new TestTelemetry { ExecuteTool = integration })
        );

        await telemetry.ExecuteToolAsync(
            Event(),
            () => Task.FromResult("done"),
            CancellationToken.None
        );

        Assert.Equal(1, integration.Calls);
    }

    // Stands in for upstream's class whose executeTool method uses this.
    private sealed class CountingWrapper : IExecuteTool
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

    private sealed class Wrapper(string name, List<string> order) : IExecuteTool
    {
        public int Calls { get; private set; }

        public string? CallId { get; private set; }

        public string? ToolCallId { get; private set; }

        public string? Version { get; private set; }

        public async Task<T> ExecuteAsync<T>(
            ExecuteToolOptions<T> options,
            CancellationToken cancellationToken
        )
        {
            Calls++;
            CallId = options.CallId;
            ToolCallId = options.ToolCallId;
            Version = options.Event.AiVersion;
            order.Add(name + "-before");
            var result = await options.Execute();
            order.Add(name + "-after");
            return (T)(object)(name + ":" + result);
        }
    }
}
