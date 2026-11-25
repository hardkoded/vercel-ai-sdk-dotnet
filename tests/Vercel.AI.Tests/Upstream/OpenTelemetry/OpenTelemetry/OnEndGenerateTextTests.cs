// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using static Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry.OpenTelemetryEvents;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>onEnd (generateText)</c>.</summary>
public sealed class OnEndGenerateTextTests
{
    private const string Test =
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry::onEnd (generateText)::";

    private static async Task<MockTracer> Run()
    {
        var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });
        await integration.OnStart!(MakeOnStartEvent(), default);
        await integration.OnStepStart!(MakeStepStartEvent(), default);
        await integration.OnEnd!(MakeFinishEvent(), default);
        return tracer;
    }

    [Fact]
    [UpstreamTest(
        Test + "sets total usage and output on root span",
        Coverage = UpstreamCoverage.Covered,
        Commit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e"
    )]
    public async Task Sets_total_usage_and_output_on_root_span()
    {
        using var tracer = await Run();

        var span = tracer.Spans[0];
        Assert.True(span.Ended);
        Assert.Equal(
            """[{"role":"user","parts":[{"type":"text","content":"Hello"}]}]""",
            span.InitAttributes["gen_ai.input.messages"]
        );
        Assert.Equal(
            """[{"role":"assistant","parts":[{"type":"text","content":"Hello world"}],"finish_reason":"stop"}]""",
            span.RuntimeAttributes["gen_ai.output.messages"]
        );
        Assert.Equal(
            new object?[] { "stop" },
            (object?[])span.RuntimeAttributes["gen_ai.response.finish_reasons"]!
        );
        Assert.Equal(10, span.RuntimeAttributes["gen_ai.usage.input_tokens"]);
        Assert.Equal(20, span.RuntimeAttributes["gen_ai.usage.output_tokens"]);
    }

    [Fact]
    [UpstreamTest(
        Test + "formats output messages on root span",
        Coverage = UpstreamCoverage.Covered,
        Commit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e"
    )]
    public async Task Formats_output_messages_on_root_span()
    {
        using var tracer = await Run();

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    """[{"finish_reason":"stop","parts":[{"content":"Hello world","type":"text"}],"role":"assistant"}]"""
                ),
                ParseJsonAttribute(tracer.Spans[0].Attributes, "gen_ai.output.messages")
            )
        );
    }
}
