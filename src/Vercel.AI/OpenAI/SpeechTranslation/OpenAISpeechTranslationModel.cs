// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.OpenAI;

/// <summary>OpenAI speech translation model. REST calls use <c>/audio/translations</c>.</summary>
public sealed class OpenAISpeechTranslationModel : ISpeechTranslationModel
{
    private readonly OpenAIProvider _provider;
    private readonly OpenAITranscriptionModel _inner;

    /// <summary>Creates a speech translation model.</summary>
    public OpenAISpeechTranslationModel(OpenAIProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _inner = new OpenAITranscriptionModel(provider, modelId, "audio/translations");
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string Provider => _provider.Name + ".speech-translation";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public Task<TranscriptionResult> DoTranslateAsync(AudioInput audio, CancellationToken cancellationToken)
    {
        return _inner.DoTranscribeAsync(audio, cancellationToken);
    }
}
