// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

#pragma warning disable CS0618

using Vercel.AI.Operations;
using DecideOperation = Vercel.AI.Operations.Decide;

namespace Vercel.AI.Tests.Upstream.Operations.DeprecatedAliases;

/// <summary>Port of <c>deprecated-aliases.test.ts</c>.</summary>
[Collection("DownloadFetch")]
public sealed class DeprecatedAliasesTests
{
    private const string Prefix = "packages/ai/src/decide/deprecated-aliases.test.ts::";

    private static readonly Dictionary<string, EvaluationQuestion> Questions = new Dictionary<string, EvaluationQuestion>
    {
        ["route"] = new EvaluationQuestion("choice", "Team?", new Dictionary<string, object?> { ["billing"] = null, ["support"] = null }),
    };

    private static DecisionModelResult Result()
    {
        return new DecisionModelResult(new Dictionary<string, DecisionAnswer> { ["route"] = new DecisionAnswer("choice", choice: "billing") });
    }

    [Fact]
    [UpstreamTest(Prefix + "keeps the function alias and accepts existing model implementations", Coverage = UpstreamCoverage.Partial, Note = "C# cannot make two methods the same object, so the test checks that EvaluateAsync and DecideAsync return the same answers.")]
    public async Task Keeps_the_function_alias_and_accepts_existing_model_implementations()
    {
        var legacy = new LegacyModel();
        var output = await Evaluate.EvaluateAsync(new EvaluateRequest { Model = legacy, State = "message", Questions = Questions });
        Assert.Equal("billing", output.Answers["route"].Choice);
        Assert.Equal(1, legacy.Calls);

        var current = await DecideOperation.DecideAsync(new DecideRequest { Model = new LegacyAdapterModel(), State = "message", Questions = new Dictionary<string, DecisionQuestion>(Questions.ToDictionary(pair => pair.Key, pair => (DecisionQuestion)pair.Value)) });
        Assert.Equal(output.Answers["route"].Choice, current.Answers["route"].Choice);
    }

    [Fact]
    [UpstreamTest(Prefix + "dispatches deprecated telemetry callbacks once with decision operation names", Coverage = UpstreamCoverage.Covered)]
    public async Task Dispatches_deprecated_telemetry_callbacks_once_with_decision_operation_names()
    {
        var calls = new Dictionary<string, List<string>>
        {
            ["OnEvaluateStart"] = new List<string>(),
            ["OnModelStart"] = new List<string>(),
            ["OnModelEnd"] = new List<string>(),
            ["OnEvaluateEnd"] = new List<string>(),
        };
        await Evaluate.EvaluateAsync(new EvaluateRequest
        {
            Model = new LegacyModel(),
            State = "message",
            Questions = Questions,
            Telemetry = new EvaluateTelemetry
            {
                OnEvaluateStart = e => { calls["OnEvaluateStart"].Add(e.OperationId); return Task.CompletedTask; },
                OnModelStart = e => { calls["OnModelStart"].Add(e.OperationId); return Task.CompletedTask; },
                OnModelEnd = e => { calls["OnModelEnd"].Add(e.OperationId); return Task.CompletedTask; },
                OnEvaluateEnd = e => { calls["OnEvaluateEnd"].Add(e.OperationId); return Task.CompletedTask; },
            },
        });
        foreach (var pair in calls)
        {
            Assert.Equal(new[] { pair.Key.Contains("Model") ? "ai.decide.doDecide" : "ai.decide" }, pair.Value);
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "prefers each decision telemetry callback over its deprecated alias", Coverage = UpstreamCoverage.Covered)]
    public async Task Prefers_each_decision_telemetry_callback_over_its_deprecated_alias()
    {
        var deprecated = 0;
        var current = 0;
        Task Current<T>(T _)
        {
            current++;
            return Task.CompletedTask;
        }

        Task Deprecated<T>(T _)
        {
            deprecated++;
            return Task.CompletedTask;
        }

        await Evaluate.EvaluateAsync(new EvaluateRequest
        {
            Model = new LegacyModel(),
            State = "message",
            Questions = Questions,
            Telemetry = new EvaluateTelemetry
            {
                OnDecideStart = Current,
                OnEvaluateStart = Deprecated,
                OnDecisionModelCallStart = Current,
                OnModelStart = Deprecated,
                OnDecisionModelCallEnd = Current,
                OnModelEnd = Deprecated,
                OnDecideEnd = Current,
                OnEvaluateEnd = Deprecated,
            },
        });
        Assert.Equal(4, current);
        Assert.Equal(0, deprecated);
    }

    private sealed class LegacyModel : IEvaluationCaller
    {
        public string Provider => "legacy";

        public string ModelId => "legacy-model";

        public string SpecificationVersion => "v4";

        public IReadOnlyList<string> SupportedQuestionTypes { get; } = new[] { "choice" };

        public int Calls { get; private set; }

        public Task<EvaluationModelResult> DoEvaluateAsync(EvaluationModelCall call, CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult(EvaluationModelResult.From(Result()));
        }
    }

    private sealed class LegacyAdapterModel : IDecisionCaller
    {
        public string Provider => "legacy";

        public string ModelId => "legacy-model";

        public string SpecificationVersion => "v4";

        public IReadOnlyList<string> SupportedQuestionTypes { get; } = new[] { "choice" };

        public Task<DecisionModelResult> DoDecideAsync(DecisionModelCall call, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result());
        }
    }
}
