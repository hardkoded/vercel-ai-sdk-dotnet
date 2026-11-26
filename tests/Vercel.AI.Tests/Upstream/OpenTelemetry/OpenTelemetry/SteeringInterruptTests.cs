// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.GenerateText;
using Vercel.AI.Tests.Upstream.OpenTelemetry;
using static Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry.OpenTelemetrySupport;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>steering interrupt</c>.</summary>
public sealed class SteeringInterruptTests
{
    private const string Test =
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry > steering interrupt::";

    [Theory]
    [InlineData("generateText")]
    [InlineData("streamText")]
    [UpstreamTest(
        Test + "ends the span of the model call that an interrupt cancels in %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = "7dc49985c58ae25456aa1f23c28f2f891266629f"
    )]
    public async Task Ends_the_span_of_the_model_call_that_an_interrupt_cancels(string mode)
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });
        var steering = Vercel.AI.GenerateText.Steering.ExperimentalCreateSteering();
        var generateCalls = 0;
        var streamCalls = 0;
        var usage = new LanguageModelUsage(2, 1, 3, 0, 0, 0);

        async Task<T> InterruptAndWaitForAbort<T>(CancellationToken token)
        {
            _ = steering.SteerAsync(
                "Use Paris, Texas instead.",
                new Vercel.AI.GenerateText.SteerOptions { Interrupt = true }
            );
            return await StepErrorModel.HangUntilAborted<T>(token);
        }

        async IAsyncEnumerable<LanguageModelStreamPart> Recovered()
        {
            await Task.Yield();
            yield return new StreamStartStreamPart([]);
            yield return new TextStartStreamPart("1");
            yield return new TextDeltaStreamPart("1", "It is sunny.");
            yield return new TextEndStreamPart("1");
            yield return new FinishStreamPart(FinishReason.Stop, usage);
        }

        var model = new StepErrorModel(
            doGenerate: (_, token) =>
                Interlocked.Increment(ref generateCalls) == 1
                    ? InterruptAndWaitForAbort<LanguageModelGenerateResult>(token)
                    : Task.FromResult(
                        new LanguageModelGenerateResult(
                            [new GeneratedText("It is sunny.")],
                            FinishReason.Stop,
                            usage
                        )
                    ),
            doStream: (_, token) =>
                Interlocked.Increment(ref streamCalls) == 1
                    ? InterruptAndWaitForAbort<LanguageModelStreamResult>(token)
                    : StepErrorModel.StreamOf(Recovered())
        );
        var options = new StreamTextOptions
        {
            Model = model,
            Prompt = "What is the weather in Paris?",
            ExperimentalSteering = steering,
            Telemetry = new TelemetryOptions { Integrations = [integration] },
        };
        var client = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));

        if (mode == "generateText")
        {
            await client.GenerateTextAsync(options);
        }
        else
        {
            await client.StreamTextAsync(options).ConsumeStreamAsync();
        }

        Assert.Equal(
            [
                ("invoke_agent mock-model-id", true, ActivityStatusCode.Unset),
                ("step 1", true, ActivityStatusCode.Unset),
                ("chat mock-model-id", true, ActivityStatusCode.Error),
                ("chat mock-model-id", true, ActivityStatusCode.Unset),
            ],
            tracer.Spans.Select(span => (span.Name, span.Ended, span.Activity.Status))
        );
    }
}
