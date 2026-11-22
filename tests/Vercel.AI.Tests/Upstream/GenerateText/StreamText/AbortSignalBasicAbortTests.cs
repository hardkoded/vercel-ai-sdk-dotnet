// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Tests.StreamText;
using Vercel.AI.Tests.Upstream.Telemetry;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; abort signal &gt; basic abort</c>.</summary>
[Collection("TelemetryRegistry")]
public sealed class AbortSignalBasicAbortTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > abort signal > basic abort::should call telemetry onAbort but not onEnd or onError when the abort signal is triggered",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_telemetry_onAbort_but_not_onEnd_or_onError_when_the_abort_signal_is_triggered()
    {
        var abortController = new AbortController();
        var telemetryCalls = new List<(string Name, IReadOnlyList<StepResult>? Steps)>();

        async IAsyncEnumerable<LanguageModelStreamPart> Parts()
        {
            yield return new StreamStartStreamPart(null);
            yield return new TextStartStreamPart("1");
            yield return new TextDeltaStreamPart("1", "Hello");
            abortController.Abort(new JsError("This operation was aborted"));
            await Task.CompletedTask;
            throw new OperationCanceledException("The user aborted a request.");
        }

        var result = StreamTextAbortSupport
            .Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    AbortSignal = abortController.Signal,
                    OnError = (_, _) => Task.CompletedTask,
                    OnAbort = (_, _) => Task.CompletedTask,
                    Telemetry = new TelemetryOptions
                    {
                        Integrations =
                        [
                            new TestTelemetry
                            {
                                OnAbort = (e, _) =>
                                {
                                    telemetryCalls.Add(("onAbort", e.Steps));
                                    return Task.CompletedTask;
                                },
                                OnEnd = (e, _) =>
                                {
                                    telemetryCalls.Add(("onEnd", ((GenerateTextResult)e).Steps));
                                    return Task.CompletedTask;
                                },
                                OnError = (_, _) =>
                                {
                                    telemetryCalls.Add(("onError", null));
                                    return Task.CompletedTask;
                                },
                            },
                        ],
                    },
                    GenerateCallId = () => "test-telemetry-call-id",
                    Model = new StreamTextAbortSupport.ScriptedStreamModel(Parts),
                    Prompt = "test-input",
                }
            );

        await foreach (var _ in result.Stream()) { }

        var call = Assert.Single(telemetryCalls);
        Assert.Equal("onAbort", call.Name);
        Assert.Empty(call.Steps!);
    }
}
