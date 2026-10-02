// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.GmiCloud;

/// <summary>
/// GMI Cloud chat completions. The request is OpenAI-compatible. Error bodies
/// unwrap the engine diagnostic stored in <c>error.details</c>.
/// </summary>
public sealed class GmiCloudChatLanguageModel : ILanguageModel
{
    private readonly OpenAICompatibleLanguageModel _inner;

    /// <summary>Creates a chat model.</summary>
    public GmiCloudChatLanguageModel(GmiCloudProvider provider, string modelId)
    {
        if (provider == null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        _inner = new OpenAICompatibleLanguageModel(provider, modelId);
        ModelId = modelId ?? string.Empty;
    }

    /// <inheritdoc />
    public string SpecificationVersion
    {
        get { return "V4"; }
    }

    /// <inheritdoc />
    public string Provider
    {
        get { return "gmicloud.chat"; }
    }

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>GMI Cloud requests usage on streamed chunks.</summary>
    public bool IncludeUsage
    {
        get { return GmiCloudErrors.IncludeUsage; }
    }

    /// <inheritdoc />
    public Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        return _inner.DoGenerateAsync(options, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        return _inner.DoStreamAsync(options, cancellationToken);
    }
}
