// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>agentSpanName</c> &gt; <c>ai.generateText</c>.</summary>
public sealed class AgentSpanNameAiGenerateTextTests
{
    [Theory]
    [MemberData(nameof(AgentSpanNameSupport.Cases), MemberType = typeof(AgentSpanNameSupport))]
    [UpstreamTest(
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry > agentSpanName > ai.generateText::uses $expectedName with agentSpanName=$agentSpanName and functionId=$functionId",
        Coverage = UpstreamCoverage.Covered,
        Commit = AgentSpanNameSupport.Commit
    )]
    public Task Uses_expected_name_with_agent_span_name_and_function_id(
        string agentSpanName,
        string? functionId,
        string expectedName
    ) =>
        AgentSpanNameSupport.AssertName(
            agentSpanName,
            functionId,
            expectedName,
            new GenerateTextStartEvent
            {
                CallId = "call-1",
                Provider = "openai",
                ModelId = "gpt-4",
            }.WithTelemetry(new TelemetryOptions { FunctionId = functionId })
        );
}
