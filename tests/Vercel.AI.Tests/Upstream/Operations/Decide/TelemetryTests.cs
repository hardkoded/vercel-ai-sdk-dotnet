// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.Operations.Decide.DecideSupport;
using DecideOperation = Vercel.AI.Operations.Decide;

namespace Vercel.AI.Tests.Upstream.Operations.Decide;

/// <summary>Port of <c>decide.test.ts</c> &gt; <c>telemetry</c>.</summary>
[Collection("DownloadFetch")]
public sealed class TelemetryTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::telemetry::emits operation and model-call lifecycle events", Coverage = UpstreamCoverage.Covered)]
    public async Task Emits_operation_and_model_call_lifecycle_events()
    {
        var genericStarts = 0;
        var userStarts = new List<DecideEvent>();
        var telemetryStarts = new List<DecideEvent>();
        var modelStarts = new List<DecisionModelEvent>();
        var modelEnds = new List<DecisionModelEvent>();
        var telemetryEnds = new List<DecideEvent>();
        var state = new Dictionary<string, object?> { ["message"] = "refund" };
        await DecideOperation.DecideAsync(new DecideRequest
        {
            Model = new DecideModel { Result = new DecisionModelResult(SampleAnswers(), usage: new OperationUsage(30, 4)) },
            State = state,
            Questions = SampleQuestions(),
            GenerateCallId = () => "test-call-id",
            RuntimeContext = new Dictionary<string, object?> { ["requestId"] = "request-1", ["secret"] = "hidden" },
            OnStart = item => { userStarts.Add(item); return Task.CompletedTask; },
            OnEnd = item => { userStarts.Add(item); return Task.CompletedTask; },
            Telemetry = new DecideTelemetry
            {
                RecordInputs = false,
                RecordOutputs = true,
                FunctionId = "evaluate-test",
                OnDecideStart = item => { telemetryStarts.Add(item); return Task.CompletedTask; },
                OnDecideEnd = item => { telemetryEnds.Add(item); return Task.CompletedTask; },
                OnDecisionModelCallStart = item => { modelStarts.Add(item); return Task.CompletedTask; },
                OnDecisionModelCallEnd = item => { modelEnds.Add(item); return Task.CompletedTask; },
            },
        });
        Assert.Equal(0, genericStarts);
        Assert.Equal("test-call-id", telemetryStarts[0].CallId);
        Assert.Equal("ai.decide", telemetryStarts[0].OperationId);
        Assert.Empty(telemetryStarts[0].RuntimeContext);
        Assert.False(telemetryStarts[0].RecordInputs);
        Assert.True(telemetryStarts[0].RecordOutputs);
        Assert.Equal("evaluate-test", telemetryStarts[0].FunctionId);
        Assert.Equal(2, telemetryStarts[0].MaxRetries);
        Assert.Equal("ai.decide.doDecide", modelStarts[0].OperationId);
        Assert.Equal((int?)30, modelEnds[0].Usage!.InputTokens);
        Assert.Null(modelEnds[0].Usage!.TotalTokens);
        Assert.Equal((int?)34, telemetryEnds[0].Usage!.TotalTokens);
        Assert.Equal("hidden", userStarts[0].RuntimeContext["secret"]);
        Assert.Null(userStarts[0].FunctionId);
        Assert.Equal("hidden", userStarts[1].RuntimeContext["secret"]);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::telemetry::includes only selected runtime context fields", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_only_selected_runtime_context_fields()
    {
        DecideEvent? seen = null;
        await DecideOperation.DecideAsync(new DecideRequest
        {
            Model = new DecideModel(),
            State = "text",
            Questions = SampleQuestions(),
            RuntimeContext = new Dictionary<string, object?> { ["requestId"] = "request-1", ["secret"] = "hidden" },
            Telemetry = new DecideTelemetry
            {
                IncludeRuntimeContext = new Dictionary<string, bool> { ["requestId"] = true },
                OnDecideStart = item => { seen = item; return Task.CompletedTask; },
            },
        });
        Assert.Equal("request-1", seen!.RuntimeContext["requestId"]);
        Assert.False(seen.RuntimeContext.ContainsKey("secret"));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/decide/decide.test.ts::telemetry::emits an error event when decision fails", Coverage = UpstreamCoverage.Covered)]
    public async Task Emits_an_error_event_when_decision_fails()
    {
        string? callId = null;
        Exception? seen = null;
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => DecideOperation.DecideAsync(new DecideRequest
        {
            Model = new DecideModel { FailWith = new InvalidOperationException("evaluation failed") },
            State = "text",
            Questions = SampleQuestions(),
            MaxRetries = 0,
            GenerateCallId = () => "test-call-id",
            Telemetry = new DecideTelemetry { OnError = (id, exception) => { callId = id; seen = exception; return Task.CompletedTask; } },
        }));
        Assert.Equal("evaluation failed", error.Message);
        Assert.Equal("test-call-id", callId);
        Assert.Same(error, seen);
    }
}
