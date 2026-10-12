// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

#pragma warning disable CS0618

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using DecideModel = Vercel.AI.Tests.Upstream.Operations.Decide.DecideModel;
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
    [UpstreamTest(Prefix + "keeps the function alias and accepts existing model implementations", Coverage = UpstreamCoverage.Partial, Note = "C# cannot make two methods the same object, so the test checks that EvaluateAsync and DecideAsync (through DecisionCaller.FromLegacy) return the same answers from a legacy model.")]
    public async Task Keeps_the_function_alias_and_accepts_existing_model_implementations()
    {
        var legacy = new LegacyModel();
        var output = await Evaluate.EvaluateAsync(new EvaluateRequest { Model = legacy, State = "message", Questions = Questions });
        Assert.Equal("billing", output.Answers["route"].Choice);
        Assert.Equal(1, legacy.Calls);

        var wrapped = new LegacyModel();
        var current = await DecideOperation.DecideAsync(new DecideRequest { Model = DecisionCaller.FromLegacy(wrapped), State = "message", Questions = new Dictionary<string, DecisionQuestion>(Questions.ToDictionary(pair => pair.Key, pair => (DecisionQuestion)pair.Value)) });
        Assert.Equal(output.Answers["route"].Choice, current.Answers["route"].Choice);
        Assert.Equal(1, wrapped.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "resolves deprecated provider factories through default, custom, and registry access", Coverage = UpstreamCoverage.Partial, Note = "Checks DecisionModel through the default provider, the registry, a custom fallback, and the deprecated alias map. The deprecated EvaluationModel accessors are not ported (issue #390).")]
    public async Task Resolves_deprecated_provider_factories_through_default_custom_and_registry_access()
    {
        var legacy = new LegacyProvider();
        var registry = new ProviderRegistry("|");
        registry.Register("legacy", legacy);
        var custom = new CustomProvider { FallbackProvider = legacy };
        var aliases = new CustomProvider().AddEvaluationModel("alias", new LegacyModel());
        DecisionModels.DefaultProvider = legacy;
        try
        {
            var requests = new[]
            {
                new DecideRequest { ModelId = "model" },
                new DecideRequest { Model = registry.DecisionModel("legacy|model") },
                new DecideRequest { Model = custom.DecisionModel("model") },
                new DecideRequest { Model = aliases.DecisionModel("alias") },
            };
            foreach (var request in requests)
            {
                request.State = "message";
                request.Questions = DecisionQuestions();
                var output = await DecideOperation.DecideAsync(request);
                Assert.Equal("billing", Assert.Single(output.Answers).Value.Choice);
            }
        }
        finally
        {
            DecisionModels.DefaultProvider = null;
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "prefers new model maps and factories and supports direct deprecated model calls", Coverage = UpstreamCoverage.Partial, Note = "Checks that the decision map wins over the deprecated map, and that a fallback provider's decision factory wins over its deprecated factory. The default provider (issue #389) and direct DoEvaluateAsync calls through CustomProvider.EvaluationModel (issue #390) are not ported.")]
    public async Task Prefers_new_model_maps_and_factories_and_supports_direct_deprecated_model_calls()
    {
        var model = new DecideModel { Result = Result() };
        var provider = new CustomProvider().AddDecisionModel("alias", model).AddEvaluationModel("alias", new LegacyModel());
        Assert.Same(model, provider.DecisionModel("alias"));

        var both = new BothFactoriesProvider(model);
        await DecideOperation.DecideAsync(new DecideRequest { Model = new CustomProvider { FallbackProvider = both }.DecisionModel("model"), State = "message", Questions = DecisionQuestions() });
        Assert.Single(model.Calls);
        Assert.Equal(0, both.DeprecatedCalls);
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

    private static Dictionary<string, DecisionQuestion> DecisionQuestions()
    {
        return Questions.ToDictionary(pair => pair.Key, pair => (DecisionQuestion)pair.Value);
    }

    private sealed class LegacyProvider : ProviderBase, IEvaluationProvider
    {
        private readonly LegacyModel _model = new LegacyModel();

        public LegacyProvider()
            : base("legacy")
        {
        }

        public override ILanguageModel LanguageModel(string modelId) => new TestLanguageModel(modelId);

        IEvaluationCaller IEvaluationProvider.EvaluationModel(string modelId)
        {
            Assert.Equal("model", modelId);
            return _model;
        }
    }

    private sealed class BothFactoriesProvider : ProviderBase, IDecisionProvider, IEvaluationProvider
    {
        private readonly IDecisionCaller _model;

        public BothFactoriesProvider(IDecisionCaller model)
            : base("both")
        {
            _model = model;
        }

        public int DeprecatedCalls { get; private set; }

        public override ILanguageModel LanguageModel(string modelId) => new TestLanguageModel(modelId);

        public IDecisionCaller DecisionModel(string modelId) => _model;

        IEvaluationCaller IEvaluationProvider.EvaluationModel(string modelId)
        {
            DeprecatedCalls++;
            return new LegacyModel();
        }
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
}
