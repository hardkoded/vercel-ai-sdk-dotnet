// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.Telemetry.CreateTelemetryDispatcher;

/// <summary>Port of <c>create-telemetry-dispatcher.test.ts</c> &gt; <c>createTelemetryDispatcher &gt; executeLanguageModelCall</c>.</summary>
[Collection("TelemetryRegistry")]
public sealed class CreateTelemetryDispatcherExecuteLanguageModelCallTests
{
    private const string Id =
        "packages/ai/src/telemetry/create-telemetry-dispatcher.test.ts::createTelemetryDispatcher > executeLanguageModelCall::";

    [Fact]
    [UpstreamTest(Id + "passes through when no integrations implement executeLanguageModelCall")]
    public async Task Passes_through_when_no_integrations_implement_executeLanguageModelCall()
    {
        var telemetry = Vercel.AI.TelemetryDispatcher.Create(
            CreateTelemetryDispatcherTests.Options(
                new TestTelemetry { OnStart = (_, _) => Task.CompletedTask }
            )
        );

        Assert.Equal(
            "result",
            await telemetry.ExecuteLanguageModelCallAsync(
                new LanguageModelCallStartEvent { CallId = "call-1" },
                () => Task.FromResult("result"),
                CancellationToken.None
            )
        );
    }

    [Fact]
    [UpstreamTest(Id + "wraps execute with a single integration")]
    public async Task Wraps_execute_with_a_single_integration()
    {
        var executed = 0;
        var wrapper = new Wrapper();
        var telemetry = Vercel.AI.TelemetryDispatcher.Create(
            CreateTelemetryDispatcherTests.Options(
                new TestTelemetry { ExecuteLanguageModelCall = wrapper }
            )
        );

        var result = await telemetry.ExecuteLanguageModelCallAsync(
            new LanguageModelCallStartEvent { CallId = "call-1" },
            () =>
            {
                executed++;
                return Task.FromResult("result");
            },
            CancellationToken.None
        );

        Assert.Equal("wrapped:result", result);
        Assert.Equal(1, wrapper.Calls);
        Assert.Equal(Vercel.AI.Operations.AiSdkVersion.Version, wrapper.Version);
        Assert.Equal("call-1", wrapper.CallId);
        Assert.Equal(1, executed);
    }

    private sealed class Wrapper : IExecuteLanguageModelCall
    {
        public int Calls { get; private set; }

        public string? Version { get; private set; }

        public string? CallId { get; private set; }

        public async Task<T> ExecuteAsync<T>(
            ExecuteLanguageModelCallOptions<T> options,
            CancellationToken cancellationToken
        )
        {
            Calls++;
            Version = options.Event.AiVersion;
            CallId = options.CallId;
            return (T)(object)("wrapped:" + await options.Execute());
        }
    }
}
