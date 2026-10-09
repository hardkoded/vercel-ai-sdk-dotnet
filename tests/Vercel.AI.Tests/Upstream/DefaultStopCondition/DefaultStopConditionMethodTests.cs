// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.DefaultStopCondition.DefaultStopConditionSupport;

namespace Vercel.AI.Tests.DefaultStopCondition;

/// <summary>
/// Port of <c>default-stop-condition.test.ts</c> &gt; <c>default stop condition: $method</c>.
/// The agent default limit is 10 here, not the 20 of upstream's <c>ToolLoopAgent</c>.
/// </summary>
[Collection("LogWarnings")]
public sealed class DefaultStopConditionMethodTests
{
    private const string Prefix = "packages/ai/src/agent/default-stop-condition.test.ts::default stop condition: $method::";

    public static TheoryData<string, int> Methods => new()
    {
        { "generateText", 1 },
        { "streamText", 1 },
        { "agent.generate", 10 },
        { "agent.stream", 10 },
    };

    public static TheoryData<string> MethodNames
    {
        get
        {
            var data = new TheoryData<string>();
            foreach (var row in Methods)
            {
                data.Add((string)row[0]);
            }

            return data;
        }
    }

    public static TheoryData<string, int, bool> NaturalFinishes
    {
        get
        {
            var data = new TheoryData<string, int, bool>();
            foreach (var row in Methods)
            {
                data.Add((string)row[0], (int)row[1], true);
                data.Add((string)row[0], (int)row[1], false);
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(Methods))]
    [UpstreamTest(Prefix + "warns only for the agent when the default limit prevents another tool round", Coverage = UpstreamCoverage.Covered)]
    public async Task Warns_only_for_the_agent_when_the_default_limit_prevents_another_tool_round(string method, int limit)
    {
        using var log = new LogRecorder();
        var steps = await Run(method, new ToolLoopModel(), ExecutableTool(), null);

        Assert.Equal(limit, steps.Count);
        if (!method.StartsWith("agent.", StringComparison.Ordinal))
        {
            Assert.Empty(log.Logged);
            return;
        }

        var logged = Assert.Single(log.Logged);
        Assert.Equal("test", logged.Provider);
        Assert.Equal(ModelId, logged.Model);
        var warning = Assert.IsType<OtherWarning>(Assert.Single(logged.Warnings));
        Assert.Contains("isStepCount(" + limit + ")", warning.Message, StringComparison.Ordinal);
        Assert.Contains("stopWhen", warning.Message, StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(NaturalFinishes))]
    [UpstreamTest(Prefix + "does not warn when the model finishes naturally on step %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_warn_when_the_model_finishes_naturally_on_step(string method, int limit, bool firstStep)
    {
        using var log = new LogRecorder();
        var step = firstStep ? 1 : limit;
        var model = new ToolLoopModel { FinishAtStep = step };

        Assert.Equal(step, (await Run(method, model, ExecutableTool(), null)).Count);
        Assert.Empty(log.Logged);
    }

    [Theory]
    [MemberData(nameof(MethodNames))]
    [UpstreamTest(Prefix + "does not warn when a tool has no execute function", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_warn_when_a_tool_has_no_execute_function(string method)
    {
        using var log = new LogRecorder();

        Assert.Single(await Run(method, new ToolLoopModel(), ToolWithoutExecute(), null));
        Assert.Empty(log.Logged);
    }

    [Theory]
    [MemberData(nameof(Methods))]
    [UpstreamTest(Prefix + "does not warn for an explicit default limit (array: %s)", Coverage = UpstreamCoverage.Partial, Note = "Covers array: false. StopWhen is one condition, not an array.")]
    public async Task Does_not_warn_for_an_explicit_default_limit(string method, int limit)
    {
        using var log = new LogRecorder();

        Assert.Equal(limit, (await Run(method, new ToolLoopModel(), ExecutableTool(), StopWhen.IsStepCount(limit))).Count);
        Assert.Empty(log.Logged);
    }

    [Theory]
    [MemberData(nameof(MethodNames))]
    [UpstreamTest(Prefix + "evaluates a custom condition only once per step", Coverage = UpstreamCoverage.Covered)]
    public async Task Evaluates_a_custom_condition_only_once_per_step(string method)
    {
        using var log = new LogRecorder();
        var condition = new CountingCondition(StopWhen.IsStepCount(2));

        Assert.Equal(2, (await Run(method, new ToolLoopModel(), ExecutableTool(), condition)).Count);
        Assert.Equal(2, condition.Calls);
        Assert.Empty(log.Logged);
    }

    [Theory]
    [MemberData(nameof(Methods))]
    [UpstreamTest(Prefix + "respects disabled warning logging", Coverage = UpstreamCoverage.Covered)]
    public async Task Respects_disabled_warning_logging(string method, int limit)
    {
        using var log = new LogRecorder();
        log.Disable();

        Assert.Equal(limit, (await Run(method, new ToolLoopModel(), ExecutableTool(), null)).Count);
        Assert.Empty(log.Logged);
        Assert.Equal(0, log.Emitted);
    }
}
