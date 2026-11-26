// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.ResolveModel;

internal sealed class FakeEmbeddingModel(string provider, string modelId, string version = "v4")
    : IEmbeddingModel
{
    public string SpecificationVersion => version;

    public string Provider => provider;

    public string ModelId => modelId;

    public Task<EmbeddingResult> DoEmbedAsync(
        IReadOnlyList<string> values,
        IReadOnlyDictionary<string, System.Text.Json.JsonElement>? providerOptions,
        CancellationToken cancellationToken,
        int? dimensions = null,
        IReadOnlyDictionary<string, string?>? headers = null
    ) => throw new NotSupportedException();
}

internal sealed class FakeImageModel(string provider, string modelId) : IImageModel
{
    public string Provider => provider;

    public string ModelId => modelId;

    public Task<ImageGenerationResult> DoGenerateAsync(
        ImageCallOptions options,
        CancellationToken cancellationToken
    ) => throw new NotSupportedException();
}

internal sealed class FakeSpeechModel(string provider, string modelId) : ISpeechModel
{
    public string Provider => provider;

    public string ModelId => modelId;

    public Task<SpeechResult> DoGenerateAsync(
        SpeechCallOptions options,
        CancellationToken cancellationToken
    ) => throw new NotSupportedException();
}

internal sealed class FakeTranscriptionModel(string provider, string modelId) : ITranscriptionModel
{
    public string Provider => provider;

    public string ModelId => modelId;

    public Task<TranscriptionResult> DoTranscribeAsync(
        AudioInput audio,
        CancellationToken cancellationToken
    ) => throw new NotSupportedException();
}

internal sealed class FakeVideoModel(string provider, string modelId, string version = "v4")
    : IVideoModel
{
    public string SpecificationVersion => version;

    public string Provider => provider;

    public string ModelId => modelId;

    public Task<VideoResult> DoGenerateAsync(
        VideoCallOptions options,
        CancellationToken cancellationToken
    ) => throw new NotSupportedException();
}

internal sealed class FakeRerankingModel(string provider, string modelId) : IRerankingModel
{
    public string Provider => provider;

    public string ModelId => modelId;

    public Task<Vercel.AI.Provider.RerankResult> DoRerankAsync(
        string query,
        IReadOnlyList<string> documents,
        int? topN,
        CancellationToken cancellationToken,
        IReadOnlyDictionary<string, string?>? headers = null
    ) => throw new NotSupportedException();
}

/// <summary>Sets <see cref="DecisionModels.DefaultProvider"/> (<c>AI_SDK_DEFAULT_PROVIDER</c>) and restores it.</summary>
internal sealed class GlobalProviderScope : IDisposable
{
    private readonly ProviderBase? _previous = DecisionModels.DefaultProvider;

    public GlobalProviderScope(ProviderBase? provider)
    {
        DecisionModels.DefaultProvider = provider;
    }

    public void Dispose() => DecisionModels.DefaultProvider = _previous;
}

/// <summary>Tests that touch the global default provider cannot run in parallel.</summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class GlobalProviderCollection
{
    public const string Name = "GlobalDefaultProvider";
}
