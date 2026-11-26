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

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>full lifecycle</c>.</summary>
public sealed class FullLifecycleTests
{
    private const string Test =
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry > full lifecycle::";

    [Fact]
    [UpstreamTest(
        Test + "creates correct span hierarchy for multi-step tool loop",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Creates_correct_span_hierarchy_for_multi_step_tool_loop()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(MakeOnStartEvent(), default);

        await integration.OnStepStart!(MakeStepStartEvent(), default);
        await integration.OnLanguageModelCallStart!(
            MakeLanguageModelCallStartEvent(messages: [new UserModelMessage("Hello")]),
            default
        );
        await integration.OnLanguageModelCallEnd!(
            MakeLanguageModelCallEndEvent(
                finishReason: FinishReason.ToolCalls,
                content: [new GeneratedToolCall("tool-call-1", "myTool", "{\"query\":\"test\"}")]
            ),
            default
        );
        await integration.OnToolExecutionStart!(MakeToolCallStartEvent(), default);
        await integration.OnToolExecutionEnd!(MakeToolCallFinishEvent(true), default);
        await integration.OnStepEnd!(
            MakeStepFinishEvent(
                text: null,
                toolCalls: [new GeneratedToolCall("tool-call-1", "myTool", "{\"query\":\"test\"}")],
                finishReason: FinishReason.ToolCalls
            ),
            default
        );

        await integration.OnStepStart!(MakeStepStartEvent(stepNumber: 1), default);
        await integration.OnLanguageModelCallStart!(
            MakeLanguageModelCallStartEvent(
                messages: [new AssistantModelMessage("Tool result received", null, null)]
            ),
            default
        );
        await integration.OnLanguageModelCallEnd!(MakeLanguageModelCallEndEvent(), default);
        await integration.OnStepEnd!(MakeStepFinishEvent(stepNumber: 1), default);

        await integration.OnEnd!(MakeFinishEvent(), default);

        Assert.Equal(
            [
                ("invoke_agent gpt-4", true),
                ("step 1", true),
                ("chat gpt-4", true),
                ("execute_tool myTool", true),
                ("step 2", true),
                ("chat gpt-4", true),
            ],
            tracer.Spans.Select(span => (span.Name, span.Ended))
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "full trace snapshot for single-step generateText",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Full_trace_snapshot_for_single_step_generateText()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(MakeOnStartEvent(), default);
        await integration.OnStepStart!(MakeStepStartEvent(), default);
        await integration.OnLanguageModelCallStart!(MakeLanguageModelCallStartEvent(), default);
        await integration.OnLanguageModelCallEnd!(MakeLanguageModelCallEndEvent(), default);
        await integration.OnStepEnd!(MakeStepFinishEvent(), default);
        await integration.OnEnd!(MakeFinishEvent(), default);

        OtelJson.Equal(
            """
            [{"ended":true,"initAttributes":{"gen_ai.input.messages":"[{\"role\":\"user\",\"parts\":[{\"type\":\"text\",\"content\":\"Hello\"}]}]","gen_ai.operation.name":"invoke_agent","gen_ai.provider.name":"openai","gen_ai.request.max_tokens":100,"gen_ai.request.model":"gpt-4","gen_ai.request.temperature":0.7},"name":"invoke_agent gpt-4","runtimeAttributes":{"gen_ai.output.messages":"[{\"role\":\"assistant\",\"parts\":[{\"type\":\"text\",\"content\":\"Hello world\"}],\"finish_reason\":\"stop\"}]","gen_ai.response.finish_reasons":["stop"],"gen_ai.usage.input_tokens":10,"gen_ai.usage.output_tokens":20}},{"ended":true,"initAttributes":{"gen_ai.operation.name":"agent_step"},"name":"step 1","runtimeAttributes":{}},{"ended":true,"initAttributes":{"gen_ai.operation.name":"chat","gen_ai.provider.name":"openai","gen_ai.request.max_tokens":100,"gen_ai.request.model":"gpt-4","gen_ai.request.temperature":0.7},"name":"chat gpt-4","runtimeAttributes":{"gen_ai.client.operation.duration":1,"gen_ai.output.messages":"[{\"role\":\"assistant\",\"parts\":[{\"type\":\"text\",\"content\":\"Hello world\"}],\"finish_reason\":\"stop\"}]","gen_ai.provider.name":"openai","gen_ai.response.finish_reasons":["stop"],"gen_ai.response.id":"test-response-id","gen_ai.response.model":"gpt-4","gen_ai.usage.input_tokens":10,"gen_ai.usage.output_tokens":20}}]
            """,
            SerializeTrace(tracer)
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "full trace snapshot for multi-step tool loop",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Full_trace_snapshot_for_multi_step_tool_loop()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(MakeOnStartEvent(), default);

        await integration.OnStepStart!(MakeStepStartEvent(), default);
        await integration.OnLanguageModelCallStart!(MakeLanguageModelCallStartEvent(), default);
        await integration.OnLanguageModelCallEnd!(
            MakeLanguageModelCallEndEvent(finishReason: FinishReason.ToolCalls),
            default
        );
        await integration.OnToolExecutionStart!(MakeToolCallStartEvent(), default);
        await integration.OnToolExecutionEnd!(MakeToolCallFinishEvent(true), default);
        await integration.OnStepEnd!(
            MakeStepFinishEvent(
                text: null,
                toolCalls: [new GeneratedToolCall("tool-call-1", "myTool", "{\"query\":\"test\"}")],
                finishReason: FinishReason.ToolCalls
            ),
            default
        );

        await integration.OnStepStart!(MakeStepStartEvent(stepNumber: 1), default);
        await integration.OnLanguageModelCallStart!(MakeLanguageModelCallStartEvent(), default);
        await integration.OnLanguageModelCallEnd!(MakeLanguageModelCallEndEvent(), default);
        await integration.OnStepEnd!(MakeStepFinishEvent(stepNumber: 1), default);

        await integration.OnEnd!(MakeFinishEvent(), default);

        OtelJson.Equal(
            """
            [{"ended":true,"initAttributes":{"gen_ai.input.messages":"[{\"role\":\"user\",\"parts\":[{\"type\":\"text\",\"content\":\"Hello\"}]}]","gen_ai.operation.name":"invoke_agent","gen_ai.provider.name":"openai","gen_ai.request.max_tokens":100,"gen_ai.request.model":"gpt-4","gen_ai.request.temperature":0.7},"name":"invoke_agent gpt-4","runtimeAttributes":{"gen_ai.output.messages":"[{\"role\":\"assistant\",\"parts\":[{\"type\":\"text\",\"content\":\"Hello world\"}],\"finish_reason\":\"stop\"}]","gen_ai.response.finish_reasons":["stop"],"gen_ai.usage.input_tokens":10,"gen_ai.usage.output_tokens":20}},{"ended":true,"initAttributes":{"gen_ai.operation.name":"agent_step"},"name":"step 1","runtimeAttributes":{}},{"ended":true,"initAttributes":{"gen_ai.operation.name":"chat","gen_ai.provider.name":"openai","gen_ai.request.max_tokens":100,"gen_ai.request.model":"gpt-4","gen_ai.request.temperature":0.7},"name":"chat gpt-4","runtimeAttributes":{"gen_ai.client.operation.duration":1,"gen_ai.output.messages":"[{\"role\":\"assistant\",\"parts\":[{\"type\":\"text\",\"content\":\"Hello world\"}],\"finish_reason\":\"tool_call\"}]","gen_ai.provider.name":"openai","gen_ai.response.finish_reasons":["tool-calls"],"gen_ai.response.id":"test-response-id","gen_ai.response.model":"gpt-4","gen_ai.usage.input_tokens":10,"gen_ai.usage.output_tokens":20}},{"ended":true,"initAttributes":{"gen_ai.operation.name":"execute_tool","gen_ai.tool.call.arguments":"{\"query\":\"test\"}","gen_ai.tool.call.id":"tool-call-1","gen_ai.tool.name":"myTool","gen_ai.tool.type":"function"},"name":"execute_tool myTool","runtimeAttributes":{"gen_ai.execute_tool.duration":0.042,"gen_ai.tool.call.result":"{\"result\":\"ok\"}"}},{"ended":true,"initAttributes":{"gen_ai.operation.name":"agent_step"},"name":"step 2","runtimeAttributes":{}},{"ended":true,"initAttributes":{"gen_ai.operation.name":"chat","gen_ai.provider.name":"openai","gen_ai.request.max_tokens":100,"gen_ai.request.model":"gpt-4","gen_ai.request.temperature":0.7},"name":"chat gpt-4","runtimeAttributes":{"gen_ai.client.operation.duration":1,"gen_ai.output.messages":"[{\"role\":\"assistant\",\"parts\":[{\"type\":\"text\",\"content\":\"Hello world\"}],\"finish_reason\":\"stop\"}]","gen_ai.provider.name":"openai","gen_ai.response.finish_reasons":["stop"],"gen_ai.response.id":"test-response-id","gen_ai.response.model":"gpt-4","gen_ai.usage.input_tokens":10,"gen_ai.usage.output_tokens":20}}]
            """,
            SerializeTrace(tracer)
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "does not use ai.* attribute prefix anywhere",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Does_not_use_ai_attribute_prefix_anywhere()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(MakeOnStartEvent(), default);
        await integration.OnStepStart!(MakeStepStartEvent(), default);
        await integration.OnLanguageModelCallStart!(MakeLanguageModelCallStartEvent(), default);
        await integration.OnLanguageModelCallEnd!(MakeLanguageModelCallEndEvent(), default);
        await integration.OnToolExecutionStart!(MakeToolCallStartEvent(), default);
        await integration.OnToolExecutionEnd!(MakeToolCallFinishEvent(true), default);
        await integration.OnStepEnd!(MakeStepFinishEvent(), default);
        await integration.OnEnd!(MakeFinishEvent(), default);

        foreach (var span in tracer.Spans)
        {
            foreach (var key in span.Attributes.Keys)
            {
                Assert.DoesNotMatch("^ai\\.", key);
            }
        }
    }
}
