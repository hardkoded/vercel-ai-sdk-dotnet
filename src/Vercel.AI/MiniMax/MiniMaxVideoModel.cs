// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.MiniMax;

/// <summary>Configuration of a <see cref="MiniMaxVideoModel"/>.</summary>
public sealed class MiniMaxVideoModelConfig
{
    /// <summary>Creates a configuration.</summary>
    public MiniMaxVideoModelConfig(string provider, string baseUrl, Func<Dictionary<string, string>> headers, HttpClient httpClient)
    {
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        BaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        Headers = headers ?? throw new ArgumentNullException(nameof(headers));
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>Provider id, <c>minimax.video</c>.</summary>
    public string Provider { get; }

    /// <summary>API origin with no version suffix.</summary>
    public string BaseUrl { get; }

    /// <summary>Resolves the request headers on every call (bearer token and user agent).</summary>
    public Func<Dictionary<string, string>> Headers { get; }

    /// <summary>The client every request goes through (upstream <c>fetch</c>).</summary>
    public HttpClient HttpClient { get; }
}

/// <summary>
/// MiniMax video model. This is the shell the provider factories create. Request building, polling, and errors are not ported yet,
/// so every operation throws <see cref="NotSupportedException"/>.
/// </summary>
public sealed class MiniMaxVideoModel : IVideoModel, IVideoCaller
{
    /// <summary>Creates a model.</summary>
    public MiniMaxVideoModel(string modelId, MiniMaxVideoModelConfig config)
    {
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        Config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public string Provider => Config.Provider;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>MiniMax creates one video per task.</summary>
    public int? MaxVideosPerCall => 1;

    /// <inheritdoc />
    public bool CanGenerate => false;

    /// <inheritdoc />
    public bool CanStart => false;

    /// <inheritdoc />
    public bool CanStatus => false;

    /// <inheritdoc />
    public bool CanWebhook => false;

    /// <summary>The configuration the provider passed in.</summary>
    public MiniMaxVideoModelConfig Config { get; }

    /// <inheritdoc />
    public Task<int?> ResolveMaxVideosPerCallAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(MaxVideosPerCall);
    }

    /// <inheritdoc />
    public Task<VideoResult> DoGenerateAsync(VideoCallOptions options, CancellationToken cancellationToken)
    {
        throw NotPorted();
    }

    /// <inheritdoc />
    public Task<VideoModelResult> DoGenerateAsync(VideoModelCall call, CancellationToken cancellationToken)
    {
        throw NotPorted();
    }

    /// <inheritdoc />
    public Task<VideoStartResult> DoStartAsync(VideoModelCall call, CancellationToken cancellationToken)
    {
        throw NotPorted();
    }

    /// <inheritdoc />
    public Task<VideoStatusResult> DoStatusAsync(VideoModelCall call, CancellationToken cancellationToken)
    {
        throw NotPorted();
    }

    /// <inheritdoc />
    public Task<VideoWebhook> HandleWebhookAsync(Func<Task<VideoWebhook>> webhook, CancellationToken cancellationToken)
    {
        throw NotPorted();
    }

    private static NotSupportedException NotPorted()
    {
        return new NotSupportedException("MiniMax video request building, polling, and errors are not ported yet.");
    }
}
