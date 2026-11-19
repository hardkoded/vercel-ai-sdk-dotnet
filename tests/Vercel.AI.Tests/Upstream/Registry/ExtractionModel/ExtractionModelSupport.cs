// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Tests.Upstream.Operations.Extract;
using Vercel.AI.Tests.Upstream.Registry.DecisionModel;

namespace Vercel.AI.Tests.Upstream.Registry.ExtractionModel;

/// <summary>Maps to <c>ExtractionTestProvider</c>: its factory expects the id <c>model:version</c>.</summary>
internal sealed class ExtractionTestProvider : ProviderBase, IExtractionProvider
{
    private readonly IExtractionCaller _model;
    private readonly ILanguageModel _languageModel = new TestLanguageModel();

    public ExtractionTestProvider(IExtractionCaller model)
        : base("extraction-test")
    {
        _model = model;
    }

    public override ILanguageModel LanguageModel(string modelId) => _languageModel;

    public IExtractionCaller ExtractionModel(string modelId)
    {
        Assert.Equal("model:version", modelId);
        return _model;
    }
}

/// <summary>A provider whose extraction factory is <c>factory</c>. Optionally has a video model.</summary>
internal sealed class ExtractionFactoryProvider : ProviderBase, IExtractionProvider
{
    private readonly Func<string, IExtractionCaller?> _factory;

    public ExtractionFactoryProvider(Func<string, IExtractionCaller?> factory)
        : base("extraction-factory")
    {
        _factory = factory;
    }

    public IVideoModel? Video { get; set; }

    public override ILanguageModel LanguageModel(string modelId) =>
        throw new NotSupportedException();

    public override IVideoModel VideoModel(string modelId) => Video ?? base.VideoModel(modelId);

    public IExtractionCaller? ExtractionModel(string modelId) => _factory(modelId);
}
