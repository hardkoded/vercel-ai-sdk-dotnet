// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenTelemetry;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry.OpenTelemetrySupport;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>enrichSpan</c>.</summary>
public sealed class EnrichSpanTests
{
    private const string Test =
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry > enrichSpan::";

    [Fact]
    [UpstreamTest(
        Test + "adds custom attributes to created spans",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Adds_custom_attributes_to_created_spans()
    {
        using var tracer = new MockTracer();
        var calls = new List<EnrichSpanContext>();
        var integration = new OtelIntegration(
            new OtelOptions
            {
                Tracer = tracer.Source,
                EnrichSpan = context =>
                {
                    calls.Add(context);
                    var userId = context.RuntimeContext?.GetValueOrDefault("userId");
                    var attributes = new Dictionary<string, object?>
                    {
                        ["custom.span_type"] =
                            char.ToLowerInvariant(context.SpanType.ToString()[0])
                            + context.SpanType.ToString()[1..],
                        ["gen_ai.operation.name"] = "custom_operation",
                    };
                    if (userId is string)
                    {
                        attributes["custom.user_id"] = userId;
                    }

                    return attributes;
                },
            }
        );

        await integration.OnStart!(
            MakeOnStartEvent(
                runtimeContext: new Dictionary<string, object?> { ["userId"] = "root-user" }
            ),
            default
        );
        await integration.OnStepStart!(
            MakeStepStartEvent(
                runtimeContext: new Dictionary<string, object?> { ["userId"] = "step-user" }
            ),
            default
        );
        await integration.OnLanguageModelCallStart!(MakeLanguageModelCallStartEvent(), default);
        await integration.OnToolExecutionStart!(MakeToolCallStartEvent(), default);

        var spans = new JsonArray();
        foreach (var span in tracer.Spans)
        {
            spans.Add(JsonSerializer.SerializeToNode(span.InitAttributes));
        }

        OtelJson.Equal(
            """
            [{"custom.span_type":"operation","custom.user_id":"root-user","gen_ai.input.messages":"[{\"role\":\"user\",\"parts\":[{\"type\":\"text\",\"content\":\"Hello\"}]}]","gen_ai.operation.name":"invoke_agent","gen_ai.provider.name":"openai","gen_ai.request.max_tokens":100,"gen_ai.request.model":"gpt-4","gen_ai.request.temperature":0.7},{"custom.span_type":"step","custom.user_id":"step-user","gen_ai.operation.name":"agent_step"},{"custom.span_type":"languageModel","custom.user_id":"step-user","gen_ai.operation.name":"chat","gen_ai.provider.name":"openai","gen_ai.request.max_tokens":100,"gen_ai.request.model":"gpt-4","gen_ai.request.temperature":0.7},{"custom.span_type":"tool","custom.user_id":"step-user","gen_ai.operation.name":"execute_tool","gen_ai.tool.call.arguments":"{\"query\":\"test\"}","gen_ai.tool.call.id":"tool-call-1","gen_ai.tool.name":"myTool","gen_ai.tool.type":"function"}]
            """,
            spans
        );

        Assert.Equal(
            [
                (CallId, "ai.generateText", "root-user", OpenTelemetrySpanType.Operation),
                (CallId, "ai.generateText", "step-user", OpenTelemetrySpanType.Step),
                (CallId, "ai.generateText", "step-user", OpenTelemetrySpanType.LanguageModel),
                (CallId, "ai.generateText", "step-user", OpenTelemetrySpanType.Tool),
            ],
            calls.Select(c =>
                (c.CallId, c.OperationId, (string?)c.RuntimeContext?["userId"], c.SpanType)
            )
        );
    }

    [Fact]
    [UpstreamTest(Test + "ignores enrichment callback errors", Coverage = UpstreamCoverage.Covered)]
    public async Task Ignores_enrichment_callback_errors()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(
            new OtelOptions
            {
                Tracer = tracer.Source,
                EnrichSpan = _ => throw new InvalidOperationException("custom attribute failure"),
            }
        );

        await integration.OnStart!(MakeOnStartEvent(), default);

        OtelJson.Equal(
            """
            {"gen_ai.input.messages":"[{\"role\":\"user\",\"parts\":[{\"type\":\"text\",\"content\":\"Hello\"}]}]","gen_ai.operation.name":"invoke_agent","gen_ai.provider.name":"openai","gen_ai.request.max_tokens":100,"gen_ai.request.model":"gpt-4","gen_ai.request.temperature":0.7}
            """,
            tracer.Spans[0].InitAttributes
        );
    }
}
