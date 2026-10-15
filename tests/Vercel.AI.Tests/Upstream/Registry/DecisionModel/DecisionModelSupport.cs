// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

#pragma warning disable CS0618

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.Registry.DecisionModel;

/// <summary>Keeps tests that replace <c>DecisionModels.DefaultProvider</c> from racing each other.</summary>
[CollectionDefinition("DecisionDefaultProvider", DisableParallelization = true)]
public sealed class DecisionDefaultProviderCollection
{
}

/// <summary>Restores the global default provider and the Gateway key after each test.</summary>
public abstract class DecisionDefaultProviderFixture : IDisposable
{
    private readonly string? _gatewayKey = Environment.GetEnvironmentVariable("AI_GATEWAY_API_KEY");

    protected DecisionDefaultProviderFixture()
    {
        Reset();
    }

    public void Dispose()
    {
        Reset();
        Environment.SetEnvironmentVariable("AI_GATEWAY_API_KEY", _gatewayKey);
    }

    private static void Reset()
    {
        DecisionModels.DefaultProvider = null;
        DecisionModels.GatewayFactory = null;
    }
}

/// <summary>Maps to <c>MockProviderV4</c>: a provider with no decision factory.</summary>
internal sealed class MockProvider : ProviderBase
{
    private readonly ILanguageModel _languageModel = new TestLanguageModel();

    public MockProvider()
        : base("mock")
    {
    }

    public override ILanguageModel LanguageModel(string modelId) => _languageModel;
}

/// <summary>Maps to <c>DecisionTestProvider</c>.</summary>
internal sealed class DecisionTestProvider : ProviderBase, IDecisionProvider
{
    private readonly IDecisionCaller _model;
    private readonly ILanguageModel _languageModel = new TestLanguageModel();

    public DecisionTestProvider(IDecisionCaller model)
        : base("decision-test")
    {
        _model = model;
    }

    public override ILanguageModel LanguageModel(string modelId) => _languageModel;

    public IDecisionCaller DecisionModel(string modelId)
    {
        Assert.Equal("model:version", modelId);
        return _model;
    }
}

/// <summary>A provider whose decision factory is <c>factory</c>.</summary>
internal sealed class DecisionFactoryProvider : ProviderBase, IDecisionProvider
{
    private readonly Func<string, IDecisionCaller?> _factory;

    public DecisionFactoryProvider(Func<string, IDecisionCaller?> factory)
        : base("decision-factory")
    {
        _factory = factory;
    }

    public override ILanguageModel LanguageModel(string modelId) => throw new NotSupportedException();

    public IDecisionCaller? DecisionModel(string modelId) => _factory(modelId);
}

/// <summary>A provider that has only the deprecated <c>evaluationModel</c> factory.</summary>
internal sealed class EvaluationFactoryProvider : ProviderBase, IEvaluationProvider
{
    private readonly Func<string, IEvaluationCaller?> _factory;

    public EvaluationFactoryProvider(Func<string, IEvaluationCaller?> factory)
        : base("evaluation-factory")
    {
        _factory = factory;
    }

    public IVideoModel? Video { get; set; }

    public override ILanguageModel LanguageModel(string modelId) => throw new NotSupportedException();

    public override IVideoModel VideoModel(string modelId) => Video ?? base.VideoModel(modelId);

    IEvaluationCaller? IEvaluationProvider.EvaluationModel(string modelId) => _factory(modelId);
}

/// <summary>Maps to <c>DecisionMockModelV4</c>, which either factory may return.</summary>
internal sealed class DecisionMockModel : IEvaluationDecisionCaller
{
    public string Provider => "mock-provider";

    public string ModelId => "mock-model-id";

    public string SpecificationVersion { get; set; } = "v4";

    public IReadOnlyList<string> SupportedQuestionTypes { get; set; } = new[] { "choice", "score", "boolean" };

    public Func<DecisionModelCall, DecisionModelResult>? OnDecide { get; set; }

    public List<DecisionModelCall> Calls { get; } = new List<DecisionModelCall>();

    public Task<DecisionModelResult> DoDecideAsync(DecisionModelCall call, CancellationToken cancellationToken)
    {
        Calls.Add(call);
        return Task.FromResult(OnDecide?.Invoke(call) ?? throw new NotImplementedException());
    }

    public async Task<EvaluationModelResult> DoEvaluateAsync(EvaluationModelCall call, CancellationToken cancellationToken)
    {
        return EvaluationModelResult.From(await DoDecideAsync(call, cancellationToken).ConfigureAwait(false));
    }
}

/// <summary>Maps to <c>MockVideoModelV4</c>.</summary>
internal sealed class MockVideoModel : IVideoModel
{
    public string Provider => "mock-provider";

    public string ModelId => "mock-model-id";

    public Task<VideoResult> DoGenerateAsync(VideoCallOptions options, CancellationToken cancellationToken) => throw new NotImplementedException();
}

internal static class DecisionModelSupport
{
    internal static DecisionModelResult RouteResult()
    {
        return new DecisionModelResult(
            new Dictionary<string, DecisionAnswer> { ["route"] = new DecisionAnswer("choice", choice: "billing") },
            Array.Empty<OperationWarning>(),
            new OperationUsage(10, 2, 12));
    }

    internal static Dictionary<string, DecisionQuestion> RouteQuestions()
    {
        return new Dictionary<string, DecisionQuestion>
        {
            ["route"] = new DecisionQuestion("choice", "Team?", new Dictionary<string, object?> { ["billing"] = "Charges", ["support"] = "Other" }),
        };
    }

    internal static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
