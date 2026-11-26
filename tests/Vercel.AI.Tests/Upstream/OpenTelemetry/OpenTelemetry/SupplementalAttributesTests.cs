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

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>supplemental attributes</c>.</summary>
public sealed class SupplementalAttributesTests
{
    private const string Test =
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry > supplemental attributes::";

    [Fact]
    [UpstreamTest(
        Test + "exports flat runtime context attribute keys",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Exports_flat_runtime_context_attribute_keys()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(
            new OtelOptions { Tracer = tracer.Source, RuntimeContext = true }
        );

        await integration.OnStart!(
            MakeOnStartEvent(
                runtimeContext: new Dictionary<string, object?> { ["foo.bar"] = "baz" }
            ),
            default
        );
        await integration.OnEnd!(MakeFinishEvent(), default);

        var rootSpan = tracer.FinishedSpans.Single(span => span.Name == "invoke_agent gpt-4");
        var observed = new Dictionary<string, object?>
        {
            ["ai.settings.context.foo.bar"] = rootSpan.Attributes.GetValueOrDefault(
                "ai.settings.context.foo.bar"
            ),
        };
        OtelJson.Equal(
            """
            {"ai.settings.context.foo.bar":"baz"}
            """,
            observed
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "exports nested runtime context attributes",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Exports_nested_runtime_context_attributes()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(
            new OtelOptions { Tracer = tracer.Source, RuntimeContext = true }
        );

        await integration.OnStart!(
            MakeOnStartEvent(
                runtimeContext: new Dictionary<string, object?>
                {
                    ["foo"] = new Dictionary<string, object?> { ["bar"] = "baz" },
                }
            ),
            default
        );
        await integration.OnEnd!(MakeFinishEvent(), default);

        var rootSpan = tracer.FinishedSpans.Single(span => span.Name == "invoke_agent gpt-4");
        var observed = new Dictionary<string, object?>
        {
            ["ai.settings.context.foo.bar"] = rootSpan.Attributes.GetValueOrDefault(
                "ai.settings.context.foo.bar"
            ),
        };
        OtelJson.Equal(
            """
            {"ai.settings.context.foo.bar":"baz"}
            """,
            observed
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "emits supplemental AI SDK attributes on existing spans when enabled",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Emits_supplemental_AI_SDK_attributes_on_existing_spans_when_enabled()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(
            new OtelOptions
            {
                Tracer = tracer.Source,
                Usage = true,
                ProviderMetadata = true,
                Embedding = true,
                Reranking = true,
                RuntimeContext = true,
                Headers = true,
                ToolChoice = true,
                Schema = true,
            }
        );
        var detailedUsage = new LanguageModelUsage(
            10,
            20,
            30,
            cacheReadTokens: 2,
            cacheWriteTokens: 1,
            reasoningTokens: 5,
            noCacheInputTokens: 7,
            textTokens: 15
        );
        var providerMetadata = JsonDocument
            .Parse("{\"openai\":{\"response\":\"metadata\"}}")
            .RootElement.Clone();

        await integration.OnStart!(
            MakeOnStartEvent(
                headers: new Dictionary<string, string?> { ["x-request-id"] = "request-123" },
                runtimeContext: new Dictionary<string, object?> { ["userId"] = "user-123" }
            ),
            default
        );
        await integration.OnStepStart!(
            MakeStepStartEvent(
                toolChoice: ToolChoice.Auto,
                runtimeContext: new Dictionary<string, object?> { ["userId"] = "user-123" }
            ),
            default
        );
        await integration.OnLanguageModelCallStart!(MakeLanguageModelCallStartEvent(), default);
        await integration.OnLanguageModelCallEnd!(
            MakeLanguageModelCallEndEvent(usage: detailedUsage, providerMetadata: providerMetadata),
            default
        );
        await integration.OnToolExecutionStart!(MakeToolCallStartEvent(), default);
        await integration.OnToolExecutionEnd!(MakeToolCallFinishEvent(true), default);
        await integration.OnStepEnd!(
            MakeStepFinishEvent(usage: detailedUsage, providerMetadata: providerMetadata),
            default
        );
        await integration.OnEnd!(
            MakeFinishEvent(usage: detailedUsage, providerMetadata: providerMetadata),
            default
        );

        OtelJson.Equal(
            """
            [{"ended":true,"initAttributes":{"ai.request.headers.x-request-id":"request-123","ai.settings.context.userId":"user-123","gen_ai.input.messages":"[{\"role\":\"user\",\"parts\":[{\"type\":\"text\",\"content\":\"Hello\"}]}]","gen_ai.operation.name":"invoke_agent","gen_ai.provider.name":"openai","gen_ai.request.max_tokens":100,"gen_ai.request.model":"gpt-4","gen_ai.request.temperature":0.7},"name":"invoke_agent gpt-4","runtimeAttributes":{"ai.response.providerMetadata":"{\"openai\":{\"response\":\"metadata\"}}","ai.usage.inputTokenDetails.noCacheTokens":7,"ai.usage.outputTokenDetails.reasoningTokens":5,"ai.usage.outputTokenDetails.textTokens":15,"gen_ai.output.messages":"[{\"role\":\"assistant\",\"parts\":[{\"type\":\"text\",\"content\":\"Hello world\"}],\"finish_reason\":\"stop\"}]","gen_ai.response.finish_reasons":["stop"],"gen_ai.usage.cache_creation.input_tokens":1,"gen_ai.usage.cache_read.input_tokens":2,"gen_ai.usage.input_tokens":10,"gen_ai.usage.output_tokens":20}},{"ended":true,"initAttributes":{"ai.prompt.toolChoice":"{\"type\":\"auto\"}","ai.request.headers.x-request-id":"request-123","ai.settings.context.userId":"user-123","gen_ai.operation.name":"agent_step"},"name":"step 1","runtimeAttributes":{"ai.response.providerMetadata":"{\"openai\":{\"response\":\"metadata\"}}","ai.usage.inputTokenDetails.noCacheTokens":7,"ai.usage.outputTokenDetails.reasoningTokens":5,"ai.usage.outputTokenDetails.textTokens":15}},{"ended":true,"initAttributes":{"gen_ai.operation.name":"chat","gen_ai.provider.name":"openai","gen_ai.request.max_tokens":100,"gen_ai.request.model":"gpt-4","gen_ai.request.temperature":0.7},"name":"chat gpt-4","runtimeAttributes":{"ai.response.providerMetadata":"{\"openai\":{\"response\":\"metadata\"}}","ai.usage.inputTokenDetails.noCacheTokens":7,"ai.usage.outputTokenDetails.reasoningTokens":5,"ai.usage.outputTokenDetails.textTokens":15,"gen_ai.client.operation.duration":1,"gen_ai.output.messages":"[{\"role\":\"assistant\",\"parts\":[{\"type\":\"text\",\"content\":\"Hello world\"}],\"finish_reason\":\"stop\"}]","gen_ai.provider.name":"openai","gen_ai.response.finish_reasons":["stop"],"gen_ai.response.id":"test-response-id","gen_ai.response.model":"gpt-4","gen_ai.usage.cache_creation.input_tokens":1,"gen_ai.usage.cache_read.input_tokens":2,"gen_ai.usage.input_tokens":10,"gen_ai.usage.output_tokens":20}},{"ended":true,"initAttributes":{"gen_ai.operation.name":"execute_tool","gen_ai.tool.call.arguments":"{\"query\":\"test\"}","gen_ai.tool.call.id":"tool-call-1","gen_ai.tool.name":"myTool","gen_ai.tool.type":"function"},"name":"execute_tool myTool","runtimeAttributes":{"gen_ai.execute_tool.duration":0.042,"gen_ai.tool.call.result":"{\"result\":\"ok\"}"}}]
            """,
            SerializeTrace(tracer)
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "only emits explicitly enabled supplemental attributes",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Only_emits_explicitly_enabled_supplemental_attributes()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(
            new OtelOptions
            {
                Tracer = tracer.Source,
                RuntimeContext = true,
                ProviderMetadata = true,
                Usage = true,
            }
        );

        await integration.OnStart!(
            MakeOnStartEvent(
                headers: new Dictionary<string, string?> { ["authorization"] = "secret" },
                runtimeContext: new Dictionary<string, object?> { ["userId"] = "user-123" }
            ),
            default
        );
        await integration.OnStepStart!(MakeStepStartEvent(), default);
        await integration.OnLanguageModelCallStart!(MakeLanguageModelCallStartEvent(), default);
        await integration.OnLanguageModelCallEnd!(MakeLanguageModelCallEndEvent(), default);
        await integration.OnToolExecutionStart!(MakeToolCallStartEvent(), default);
        await integration.OnToolExecutionEnd!(MakeToolCallFinishEvent(true), default);
        await integration.OnStepEnd!(MakeStepFinishEvent(), default);
        await integration.OnEnd!(MakeFinishEvent(), default);

        OtelJson.Equal(
            """
            [{"ended":true,"initAttributes":{"ai.settings.context.userId":"user-123","gen_ai.input.messages":"[{\"role\":\"user\",\"parts\":[{\"type\":\"text\",\"content\":\"Hello\"}]}]","gen_ai.operation.name":"invoke_agent","gen_ai.provider.name":"openai","gen_ai.request.max_tokens":100,"gen_ai.request.model":"gpt-4","gen_ai.request.temperature":0.7},"name":"invoke_agent gpt-4","runtimeAttributes":{"gen_ai.output.messages":"[{\"role\":\"assistant\",\"parts\":[{\"type\":\"text\",\"content\":\"Hello world\"}],\"finish_reason\":\"stop\"}]","gen_ai.response.finish_reasons":["stop"],"gen_ai.usage.input_tokens":10,"gen_ai.usage.output_tokens":20}},{"ended":true,"initAttributes":{"ai.settings.context.userId":"user-123","gen_ai.operation.name":"agent_step"},"name":"step 1","runtimeAttributes":{}},{"ended":true,"initAttributes":{"gen_ai.operation.name":"chat","gen_ai.provider.name":"openai","gen_ai.request.max_tokens":100,"gen_ai.request.model":"gpt-4","gen_ai.request.temperature":0.7},"name":"chat gpt-4","runtimeAttributes":{"gen_ai.client.operation.duration":1,"gen_ai.output.messages":"[{\"role\":\"assistant\",\"parts\":[{\"type\":\"text\",\"content\":\"Hello world\"}],\"finish_reason\":\"stop\"}]","gen_ai.provider.name":"openai","gen_ai.response.finish_reasons":["stop"],"gen_ai.response.id":"test-response-id","gen_ai.response.model":"gpt-4","gen_ai.usage.input_tokens":10,"gen_ai.usage.output_tokens":20}},{"ended":true,"initAttributes":{"gen_ai.operation.name":"execute_tool","gen_ai.tool.call.arguments":"{\"query\":\"test\"}","gen_ai.tool.call.id":"tool-call-1","gen_ai.tool.name":"myTool","gen_ai.tool.type":"function"},"name":"execute_tool myTool","runtimeAttributes":{"gen_ai.execute_tool.duration":0.042,"gen_ai.tool.call.result":"{\"result\":\"ok\"}"}}]
            """,
            SerializeTrace(tracer)
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "does not emit disabled supplemental attributes",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Does_not_emit_disabled_supplemental_attributes()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(
            new OtelOptions { Tracer = tracer.Source, Usage = true }
        );

        await integration.OnStart!(MakeOnStartEvent(), default);
        await integration.OnStepStart!(MakeStepStartEvent(), default);
        await integration.OnToolExecutionStart!(MakeToolCallStartEvent(), default);
        await integration.OnToolExecutionEnd!(MakeToolCallFinishEvent(true), default);
        await integration.OnStepEnd!(MakeStepFinishEvent(), default);
        await integration.OnEnd!(MakeFinishEvent(), default);

        OtelJson.Equal(
            """
            [{"ended":true,"initAttributes":{"gen_ai.input.messages":"[{\"role\":\"user\",\"parts\":[{\"type\":\"text\",\"content\":\"Hello\"}]}]","gen_ai.operation.name":"invoke_agent","gen_ai.provider.name":"openai","gen_ai.request.max_tokens":100,"gen_ai.request.model":"gpt-4","gen_ai.request.temperature":0.7},"name":"invoke_agent gpt-4","runtimeAttributes":{"gen_ai.output.messages":"[{\"role\":\"assistant\",\"parts\":[{\"type\":\"text\",\"content\":\"Hello world\"}],\"finish_reason\":\"stop\"}]","gen_ai.response.finish_reasons":["stop"],"gen_ai.usage.input_tokens":10,"gen_ai.usage.output_tokens":20}},{"ended":true,"initAttributes":{"gen_ai.operation.name":"agent_step"},"name":"step 1","runtimeAttributes":{}},{"ended":true,"initAttributes":{"gen_ai.operation.name":"execute_tool","gen_ai.tool.call.arguments":"{\"query\":\"test\"}","gen_ai.tool.call.id":"tool-call-1","gen_ai.tool.name":"myTool","gen_ai.tool.type":"function"},"name":"execute_tool myTool","runtimeAttributes":{"gen_ai.execute_tool.duration":0.042,"gen_ai.tool.call.result":"{\"result\":\"ok\"}"}}]
            """,
            SerializeTrace(tracer)
        );
    }
}
