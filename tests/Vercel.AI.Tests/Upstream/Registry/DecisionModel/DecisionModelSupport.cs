// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

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

/// <summary>Port of <c>DecisionMockModelV4</c>.</summary>
internal sealed class DecisionMockModel : IDecisionCaller
{
    internal DecisionMockModel(Func<DecisionModelCall, Task<DecisionModelResult>>? doDecide = null, IReadOnlyList<string>? supportedQuestionTypes = null)
    {
        DoDecide = doDecide;
        SupportedQuestionTypes = supportedQuestionTypes ?? new[] { "choice", "score", "boolean" };
    }

    public string Provider => "mock-provider";

    public string ModelId => "mock-model";

    public string SpecificationVersion { get; set; } = "v4";

    public IReadOnlyList<string> SupportedQuestionTypes { get; }

    internal Func<DecisionModelCall, Task<DecisionModelResult>>? DoDecide { get; }

    internal List<DecisionModelCall> Calls { get; } = new();

    public Task<DecisionModelResult> DoDecideAsync(DecisionModelCall call, CancellationToken cancellationToken)
    {
        Calls.Add(call);
        return (DoDecide ?? throw new NotSupportedException()).Invoke(call);
    }
}

/// <summary>A provider with no models.</summary>
internal abstract class TestProviderBase : ProviderBase
{
    protected TestProviderBase(string name)
        : base(name)
    {
    }

    public override ILanguageModel LanguageModel(string modelId) => throw new NotSupportedException();
}

/// <summary>Port of <c>DecisionTestProvider</c>. Resolves only <c>model:version</c>.</summary>
internal sealed class DecisionTestProvider : TestProviderBase, IDecisionModelProvider
{
    private readonly DecisionMockModel _model;

    internal DecisionTestProvider(DecisionMockModel model)
        : base("test")
    {
        _model = model;
    }

    public IDecisionCaller? DecisionModel(string modelId)
    {
        Assert.Equal("model:version", modelId);
        return _model;
    }
}

/// <summary>A provider with no decision models.</summary>
internal sealed class PlainProvider : TestProviderBase
{
    internal PlainProvider()
        : base("plain")
    {
    }
}

/// <summary>A provider whose decision factory returns a fixed result or throws.</summary>
internal sealed class FactoryProvider : TestProviderBase, IDecisionModelProvider
{
    private readonly Func<string, IDecisionCaller?> _factory;

    internal FactoryProvider(Func<string, IDecisionCaller?> factory)
        : base("factory")
    {
        _factory = factory;
    }

    public IDecisionCaller? DecisionModel(string modelId) => _factory(modelId);
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
