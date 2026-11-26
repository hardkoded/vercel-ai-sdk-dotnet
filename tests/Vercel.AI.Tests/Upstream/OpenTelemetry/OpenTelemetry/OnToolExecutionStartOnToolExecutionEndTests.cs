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

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>onToolExecutionStart / onToolExecutionEnd</c>.</summary>
public sealed class OnToolExecutionStartOnToolExecutionEndTests
{
    private const string Test =
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry > onToolExecutionStart / onToolExecutionEnd::";

    [Fact]
    [UpstreamTest(
        Test + "uses extension type for provider-executed tools",
        Coverage = UpstreamCoverage.Covered,
        Commit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e"
    )]
    public async Task Uses_extension_type_for_provider_executed_tools()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(
            new GenerateTextStartEvent
            {
                CallId = "call-1",
                Provider = "openai",
                ModelId = "gpt-4",
            }.WithTelemetry(new TelemetryOptions()),
            default
        );
        await integration.OnStepStart!(
            new GenerateTextStepStartEvent
            {
                CallId = "call-1",
                Provider = "openai",
                ModelId = "gpt-4",
                StepNumber = 0,
            },
            default
        );
        await integration.OnToolExecutionStart!(
            new ToolExecutionStartEvent(
                new GeneratedToolCall(
                    "tool-call-1",
                    "mcp.ask_question",
                    "{\"question\":\"What is this repository?\"}"
                )
                {
                    ProviderExecuted = true,
                    Dynamic = true,
                },
                []
            )
            {
                CallId = "call-1",
            },
            default
        );

        Assert.Equal("extension", tracer.Spans[2].InitAttributes["gen_ai.tool.type"]);
    }

    [Fact]
    [UpstreamTest(
        Test + "creates an execute_tool span with correct attributes",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Creates_an_execute_tool_span_with_correct_attributes()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(MakeOnStartEvent(), default);
        await integration.OnStepStart!(MakeStepStartEvent(), default);
        await integration.OnLanguageModelCallStart!(MakeLanguageModelCallStartEvent(), default);
        await integration.OnToolExecutionStart!(MakeToolCallStartEvent(), default);

        Assert.Equal(4, tracer.Spans.Count);
        OtelJson.Equal(
            """
            {"ended":false,"initAttributes":{"gen_ai.operation.name":"execute_tool","gen_ai.tool.call.arguments":"{\"query\":\"test\"}","gen_ai.tool.call.id":"tool-call-1","gen_ai.tool.name":"myTool","gen_ai.tool.type":"function"},"name":"execute_tool myTool","runtimeAttributes":{}}
            """,
            SerializeSpan(tracer.Spans[3])
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "parents chat and execute_tool spans under the same step span",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Parents_chat_and_execute_tool_spans_under_the_same_step_span()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(MakeOnStartEvent(), default);
        await integration.OnStepStart!(MakeStepStartEvent(), default);
        await integration.OnLanguageModelCallStart!(MakeLanguageModelCallStartEvent(), default);
        await integration.OnToolExecutionStart!(MakeToolCallStartEvent(), default);

        Assert.Equal(tracer.Spans[1].Activity.SpanId, tracer.Spans[2].Activity.ParentSpanId);
        Assert.Equal(tracer.Spans[2].Activity.ParentSpanId, tracer.Spans[3].Activity.ParentSpanId);
    }

    [Fact]
    [UpstreamTest(
        Test + "parents execute_tool under the root span before a step starts",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Parents_execute_tool_under_the_root_span_before_a_step_starts()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(MakeOnStartEvent(), default);
        await integration.OnToolExecutionStart!(MakeToolCallStartEvent(), default);

        Assert.Equal(tracer.Spans[0].Activity.SpanId, tracer.Spans[1].Activity.ParentSpanId);
    }

    [Fact]
    [UpstreamTest(
        Test + "sets gen_ai.tool.call.result on success",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Sets_gen_ai_tool_call_result_on_success()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(MakeOnStartEvent(), default);
        await integration.OnStepStart!(MakeStepStartEvent(), default);
        await integration.OnLanguageModelCallStart!(MakeLanguageModelCallStartEvent(), default);
        await integration.OnToolExecutionStart!(MakeToolCallStartEvent(), default);
        await integration.OnToolExecutionEnd!(MakeToolCallFinishEvent(true), default);

        OtelJson.Equal(
            """
            {"ended":true,"initAttributes":{"gen_ai.operation.name":"execute_tool","gen_ai.tool.call.arguments":"{\"query\":\"test\"}","gen_ai.tool.call.id":"tool-call-1","gen_ai.tool.name":"myTool","gen_ai.tool.type":"function"},"name":"execute_tool myTool","runtimeAttributes":{"gen_ai.execute_tool.duration":0.042,"gen_ai.tool.call.result":"{\"result\":\"ok\"}"}}
            """,
            SerializeSpan(tracer.Spans[3])
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "sets GenAI execute_tool duration on the tool span",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Sets_GenAI_execute_tool_duration_on_the_tool_span()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(MakeOnStartEvent(), default);
        await integration.OnStepStart!(MakeStepStartEvent(), default);
        await integration.OnLanguageModelCallStart!(MakeLanguageModelCallStartEvent(), default);
        await integration.OnToolExecutionStart!(MakeToolCallStartEvent(), default);
        await integration.OnToolExecutionEnd!(
            MakeToolCallFinishEvent(true, toolExecutionMs: 123),
            default
        );

        Assert.Equal(0.123, tracer.Spans[3].Attributes["gen_ai.execute_tool.duration"]);
    }

    [Fact]
    [UpstreamTest(Test + "records error on tool failure", Coverage = UpstreamCoverage.Covered)]
    public async Task Records_error_on_tool_failure()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(MakeOnStartEvent(), default);
        await integration.OnStepStart!(MakeStepStartEvent(), default);
        await integration.OnLanguageModelCallStart!(MakeLanguageModelCallStartEvent(), default);
        await integration.OnToolExecutionStart!(MakeToolCallStartEvent(), default);
        await integration.OnToolExecutionEnd!(MakeToolCallFinishEvent(false), default);

        var toolSpan = tracer.Spans[3];
        Assert.Equal(ActivityStatusCode.Error, toolSpan.Activity.Status);
        Assert.Equal("tool failed", toolSpan.Activity.StatusDescription);
        Assert.True(toolSpan.Ended);
    }
}
