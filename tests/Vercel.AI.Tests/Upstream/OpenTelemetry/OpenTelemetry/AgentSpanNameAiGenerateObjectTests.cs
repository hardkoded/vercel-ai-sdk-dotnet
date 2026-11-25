// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateObject;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>agentSpanName</c> &gt; <c>ai.generateObject</c>.</summary>
public sealed class AgentSpanNameAiGenerateObjectTests
{
    [Theory]
    [MemberData(nameof(AgentSpanNameSupport.Cases), MemberType = typeof(AgentSpanNameSupport))]
    [UpstreamTest(
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry > agentSpanName > ai.generateObject::uses $expectedName with agentSpanName=$agentSpanName and functionId=$functionId",
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
            new GenerateObjectStartEvent("call-1", "openai", "gpt-4", null, null)
            {
                OperationId = "ai.generateObject",
            }.AugmentEvent(new TelemetryOptions { FunctionId = functionId })
        );
}
