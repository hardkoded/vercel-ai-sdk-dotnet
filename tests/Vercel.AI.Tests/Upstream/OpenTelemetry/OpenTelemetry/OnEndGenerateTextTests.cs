// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry.OpenTelemetrySupport;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>onEnd (generateText)</c>.</summary>
public sealed class OnEndGenerateTextTests
{
    private const string Test =
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry > onEnd (generateText)::";

    [Fact]
    [UpstreamTest(
        Test + "sets total usage and output on root span",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Sets_total_usage_and_output_on_root_span()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(MakeOnStartEvent(), default);
        await integration.OnStepStart!(MakeStepStartEvent(), default);
        await integration.OnStepEnd!(MakeStepFinishEvent(), default);
        await integration.OnEnd!(MakeFinishEvent(), default);

        OtelJson.Equal(
            """
            {"ended":true,"initAttributes":{"gen_ai.input.messages":"[{\"role\":\"user\",\"parts\":[{\"type\":\"text\",\"content\":\"Hello\"}]}]","gen_ai.operation.name":"invoke_agent","gen_ai.provider.name":"openai","gen_ai.request.max_tokens":100,"gen_ai.request.model":"gpt-4","gen_ai.request.temperature":0.7},"name":"invoke_agent gpt-4","runtimeAttributes":{"gen_ai.output.messages":"[{\"role\":\"assistant\",\"parts\":[{\"type\":\"text\",\"content\":\"Hello world\"}],\"finish_reason\":\"stop\"}]","gen_ai.response.finish_reasons":["stop"],"gen_ai.usage.input_tokens":10,"gen_ai.usage.output_tokens":20}}
            """,
            SerializeSpan(tracer.Spans[0])
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "formats output messages on root span",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Formats_output_messages_on_root_span()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(MakeOnStartEvent(), default);
        await integration.OnStepStart!(MakeStepStartEvent(), default);
        await integration.OnStepEnd!(MakeStepFinishEvent(), default);
        await integration.OnEnd!(MakeFinishEvent(), default);

        var rootSpan = tracer.Spans[0];
        OtelJson.Equal(
            """
            [{"finish_reason":"stop","parts":[{"content":"Hello world","type":"text"}],"role":"assistant"}]
            """,
            ParseJsonAttribute(rootSpan.Attributes, "gen_ai.output.messages")
        );
    }
}
