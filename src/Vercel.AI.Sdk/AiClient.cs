// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Sdk.Gateway;
using Vercel.AI.Sdk.Provider;

namespace Vercel.AI.Sdk;

/// <summary>Telemetry hook invoked around generate and stream calls.</summary>
public interface IAiTelemetry
{
    /// <summary>Starts a span. Dispose it when the call finishes.</summary>
    IDisposable Begin(string operation, string modelId);
}

/// <summary>Core AI SDK client. Maps to the <c>ai</c> package functions.</summary>
public interface IAiClient
{
    /// <summary>Generates text and runs tools. Maps to <c>generateText</c>.</summary>
    Task<GenerateTextResult> GenerateTextAsync(GenerateTextOptions options, CancellationToken cancellationToken = default);

    /// <summary>Streams text and runs tools. Maps to <c>streamText</c>.</summary>
    StreamTextResult StreamTextAsync(StreamTextOptions options, CancellationToken cancellationToken = default);

    /// <summary>Embeds one value. Maps to <c>embed</c>.</summary>
    Task<float[]> EmbedAsync(EmbedOptions options, CancellationToken cancellationToken = default);

    /// <summary>Embeds many values. Maps to <c>embedMany</c>.</summary>
    Task<EmbeddingResult> EmbedManyAsync(EmbedOptions options, CancellationToken cancellationToken = default);

    /// <summary>Reranks documents. Maps to <c>rerank</c>.</summary>
    Task<RerankResult> RerankAsync(RerankOptions options, CancellationToken cancellationToken = default);

    /// <summary>Generates images. Maps to <c>generateImage</c>.</summary>
    Task<ImageGenerationResult> GenerateImageAsync(GenerateImageOptions options, CancellationToken cancellationToken = default);

    /// <summary>Generates speech. Maps to <c>generateSpeech</c>.</summary>
    Task<SpeechResult> GenerateSpeechAsync(GenerateSpeechOptions options, CancellationToken cancellationToken = default);

    /// <summary>Transcribes audio. Maps to <c>transcribe</c>.</summary>
    Task<TranscriptionResult> TranscribeAsync(TranscribeOptions options, CancellationToken cancellationToken = default);

    /// <summary>Translates speech to text. Maps to <c>translate</c>.</summary>
    Task<TranscriptionResult> TranslateAsync(TranslateOptions options, CancellationToken cancellationToken = default);

    /// <summary>Generates video. Maps to <c>generateVideo</c>.</summary>
    Task<VideoResult> GenerateVideoAsync(GenerateVideoOptions options, CancellationToken cancellationToken = default);

    /// <summary>Scores a candidate. Uses <see cref="EvaluateOptions.Model"/> or a language model.</summary>
    Task<EvaluationResult> EvaluateAsync(EvaluateOptions options, CancellationToken cancellationToken = default);

    /// <summary>Uploads a file through <paramref name="store"/>.</summary>
    Task<UploadedFile> UploadFileAsync(IFileStore store, string fileName, byte[] data, string mediaType, CancellationToken cancellationToken = default);

    /// <summary>Uploads a skill through <paramref name="store"/>.</summary>
    Task<UploadedSkill> UploadSkillAsync(ISkillStore store, string name, string instructions, CancellationToken cancellationToken = default);
}

/// <summary>Default <see cref="IAiClient"/>. String model ids are resolved by the AI Gateway.</summary>
public sealed class AiClient : IAiClient
{
    private readonly GatewayProvider _gateway;
    private readonly IAiTelemetry? _telemetry;

    /// <summary>Creates a client.</summary>
    public AiClient(GatewayProvider gateway, IAiTelemetry? telemetry = null)
    {
        _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
        _telemetry = telemetry;
    }

    /// <inheritdoc />
    public Task<GenerateTextResult> GenerateTextAsync(GenerateTextOptions options, CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return Generation.GenerateAsync(ResolveLanguage(options.Model, options.ModelId), options, _telemetry, cancellationToken);
    }

    /// <inheritdoc />
    public StreamTextResult StreamTextAsync(StreamTextOptions options, CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return Generation.Stream(ResolveLanguage(options.Model, options.ModelId), options, _telemetry, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<float[]> EmbedAsync(EmbedOptions options, CancellationToken cancellationToken = default)
    {
        var result = await EmbedManyAsync(options, cancellationToken).ConfigureAwait(false);
        if (result.Embeddings.Count == 0)
        {
            throw new AiSdkException("The embedding model returned no vectors.");
        }

        return result.Embeddings[0];
    }

    /// <inheritdoc />
    public Task<EmbeddingResult> EmbedManyAsync(EmbedOptions options, CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var model = options.Model ?? _gateway.EmbeddingModel(Require(options.ModelId, "embedding"));
        return model.DoEmbedAsync(options.Values, cancellationToken);
    }

    /// <inheritdoc />
    public Task<RerankResult> RerankAsync(RerankOptions options, CancellationToken cancellationToken = default)
    {
        if (options?.Model is null)
        {
            throw new AiSdkException("Rerank requires a reranking model.");
        }

        return options.Model.DoRerankAsync(options.Query, options.Documents, options.TopN, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ImageGenerationResult> GenerateImageAsync(GenerateImageOptions options, CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var model = options.Model ?? _gateway.ImageModel(Require(options.ModelId, "image"));
        return model.DoGenerateAsync(new ImageCallOptions(options.Prompt) { Count = options.Count, Size = options.Size, AspectRatio = options.AspectRatio }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<SpeechResult> GenerateSpeechAsync(GenerateSpeechOptions options, CancellationToken cancellationToken = default)
    {
        if (options?.Model is null)
        {
            throw new AiSdkException("GenerateSpeech requires a speech model.");
        }

        return options.Model.DoGenerateAsync(new SpeechCallOptions(options.Text) { Voice = options.Voice }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<TranscriptionResult> TranscribeAsync(TranscribeOptions options, CancellationToken cancellationToken = default)
    {
        if (options?.Model is null)
        {
            throw new AiSdkException("Transcribe requires a transcription model.");
        }

        return options.Model.DoTranscribeAsync(options.Audio, cancellationToken);
    }

    /// <inheritdoc />
    public Task<TranscriptionResult> TranslateAsync(TranslateOptions options, CancellationToken cancellationToken = default)
    {
        if (options?.Model is null)
        {
            throw new AiSdkException("Translate requires a speech translation model.");
        }

        return options.Model.DoTranslateAsync(options.Audio, cancellationToken);
    }

    /// <inheritdoc />
    public Task<VideoResult> GenerateVideoAsync(GenerateVideoOptions options, CancellationToken cancellationToken = default)
    {
        if (options?.Model is null)
        {
            throw new AiSdkException("GenerateVideo requires a video model.");
        }

        return options.Model.DoGenerateAsync(new VideoCallOptions(options.Prompt) { DurationSeconds = options.DurationSeconds }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<EvaluationResult> EvaluateAsync(EvaluateOptions options, CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (options.EvaluationModel != null)
        {
            return await options.EvaluationModel.DoEvaluateAsync(options.Rubric, options.Candidate, cancellationToken).ConfigureAwait(false);
        }

        var generated = await GenerateTextAsync(new GenerateTextOptions
        {
            Model = options.Model,
            ModelId = options.ModelId,
            System = "Score the candidate against the rubric. Reply with JSON {\"score\": number, \"reason\": string}.",
            Prompt = "Rubric:\n" + options.Rubric + "\n\nCandidate:\n" + options.Candidate,
            Output = OutputSpec.Object("{\"type\":\"object\",\"properties\":{\"score\":{\"type\":\"number\"},\"reason\":{\"type\":\"string\"}},\"required\":[\"score\"]}"),
        }, cancellationToken).ConfigureAwait(false);
        var score = 0d;
        string? reason = null;
        if (generated.Output is { } output && output.TryGetProperty("score", out var scoreElement) && scoreElement.TryGetDouble(out var parsed))
        {
            score = parsed;
        }

        if (generated.Output is { } outputWithReason && outputWithReason.TryGetProperty("reason", out var reasonElement))
        {
            reason = reasonElement.GetString();
        }

        return new EvaluationResult(score, reason);
    }

    /// <inheritdoc />
    public Task<UploadedFile> UploadFileAsync(IFileStore store, string fileName, byte[] data, string mediaType, CancellationToken cancellationToken = default)
    {
        if (store is null)
        {
            throw new ArgumentNullException(nameof(store));
        }

        return store.UploadFileAsync(fileName, data, mediaType, cancellationToken);
    }

    /// <inheritdoc />
    public Task<UploadedSkill> UploadSkillAsync(ISkillStore store, string name, string instructions, CancellationToken cancellationToken = default)
    {
        if (store is null)
        {
            throw new ArgumentNullException(nameof(store));
        }

        return store.UploadSkillAsync(name, instructions, cancellationToken);
    }

    private ILanguageModel ResolveLanguage(ILanguageModel? model, string? modelId)
    {
        if (model != null)
        {
            return model;
        }

        if (string.IsNullOrWhiteSpace(modelId))
        {
            throw new AiSdkException("Model or ModelId is required.");
        }

        return _gateway.LanguageModel(modelId!);
    }

    private static string Require(string? modelId, string modality)
    {
        if (string.IsNullOrWhiteSpace(modelId))
        {
            throw new AiSdkException("A " + modality + " model id is required.");
        }

        return modelId!;
    }
}

/// <summary>Embedding options.</summary>
public sealed class EmbedOptions
{
    /// <summary>Explicit embedding model.</summary>
    public IEmbeddingModel? Model { get; set; }

    /// <summary>Gateway embedding model id.</summary>
    public string? ModelId { get; set; }

    /// <summary>Values to embed.</summary>
    public IReadOnlyList<string> Values { get; set; } = Array.Empty<string>();
}

/// <summary>Rerank options.</summary>
public sealed class RerankOptions
{
    /// <summary>Reranking model.</summary>
    public IRerankingModel? Model { get; set; }

    /// <summary>Query.</summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>Documents.</summary>
    public IReadOnlyList<string> Documents { get; set; } = Array.Empty<string>();

    /// <summary>Maximum results.</summary>
    public int? TopN { get; set; }
}

/// <summary>Image generation options.</summary>
public sealed class GenerateImageOptions
{
    /// <summary>Explicit image model.</summary>
    public IImageModel? Model { get; set; }

    /// <summary>Gateway image model id.</summary>
    public string? ModelId { get; set; }

    /// <summary>Prompt.</summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>Image count.</summary>
    public int Count { get; set; } = 1;

    /// <summary>Size.</summary>
    public string? Size { get; set; }

    /// <summary>Aspect ratio.</summary>
    public string? AspectRatio { get; set; }
}

/// <summary>Speech options.</summary>
public sealed class GenerateSpeechOptions
{
    /// <summary>Speech model.</summary>
    public ISpeechModel? Model { get; set; }

    /// <summary>Text to speak.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Voice id.</summary>
    public string? Voice { get; set; }
}

/// <summary>Transcription options.</summary>
public sealed class TranscribeOptions
{
    /// <summary>Transcription model.</summary>
    public ITranscriptionModel? Model { get; set; }

    /// <summary>Audio.</summary>
    public AudioInput Audio { get; set; } = new(Array.Empty<byte>(), "audio/mpeg", null);
}

/// <summary>Speech translation options.</summary>
public sealed class TranslateOptions
{
    /// <summary>Speech translation model.</summary>
    public ISpeechTranslationModel? Model { get; set; }

    /// <summary>Audio.</summary>
    public AudioInput Audio { get; set; } = new(Array.Empty<byte>(), "audio/mpeg", null);
}

/// <summary>Video options.</summary>
public sealed class GenerateVideoOptions
{
    /// <summary>Video model.</summary>
    public IVideoModel? Model { get; set; }

    /// <summary>Prompt.</summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>Duration hint.</summary>
    public int? DurationSeconds { get; set; }
}

/// <summary>Evaluation options.</summary>
public sealed class EvaluateOptions
{
    /// <summary>Dedicated evaluation model.</summary>
    public IEvaluationModel? EvaluationModel { get; set; }

    /// <summary>Language model used when <see cref="EvaluationModel"/> is null.</summary>
    public ILanguageModel? Model { get; set; }

    /// <summary>Gateway model id used when no model instance is set.</summary>
    public string? ModelId { get; set; }

    /// <summary>Rubric.</summary>
    public string Rubric { get; set; } = string.Empty;

    /// <summary>Candidate text.</summary>
    public string Candidate { get; set; } = string.Empty;
}

/// <summary>Static entry points. String model ids use the AI Gateway and <c>AI_GATEWAY_API_KEY</c>.</summary>
public static class Ai
{
    private static readonly Lazy<AiClient> DefaultClient = new(() => new AiClient(GatewayProvider.Create()));

    /// <summary>Generates text.</summary>
    public static Task<GenerateTextResult> GenerateTextAsync(GenerateTextOptions options, CancellationToken cancellationToken = default)
    {
        return DefaultClient.Value.GenerateTextAsync(options, cancellationToken);
    }

    /// <summary>Streams text.</summary>
    public static StreamTextResult StreamTextAsync(StreamTextOptions options, CancellationToken cancellationToken = default)
    {
        return DefaultClient.Value.StreamTextAsync(options, cancellationToken);
    }

    /// <summary>Embeds one value.</summary>
    public static Task<float[]> EmbedAsync(EmbedOptions options, CancellationToken cancellationToken = default)
    {
        return DefaultClient.Value.EmbedAsync(options, cancellationToken);
    }

    /// <summary>Embeds many values.</summary>
    public static Task<EmbeddingResult> EmbedManyAsync(EmbedOptions options, CancellationToken cancellationToken = default)
    {
        return DefaultClient.Value.EmbedManyAsync(options, cancellationToken);
    }

    /// <summary>Cosine similarity of two vectors.</summary>
    public static double CosineSimilarity(IReadOnlyList<float> left, IReadOnlyList<float> right)
    {
        if (left is null)
        {
            throw new ArgumentNullException(nameof(left));
        }

        if (right is null)
        {
            throw new ArgumentNullException(nameof(right));
        }

        if (left.Count != right.Count || left.Count == 0)
        {
            throw new ArgumentException("Vectors must be non-empty and the same length.");
        }

        double dot = 0;
        double leftNorm = 0;
        double rightNorm = 0;
        for (var i = 0; i < left.Count; i++)
        {
            dot += left[i] * right[i];
            leftNorm += left[i] * left[i];
            rightNorm += right[i] * right[i];
        }

        if (leftNorm == 0 || rightNorm == 0)
        {
            return 0;
        }

        return dot / (Math.Sqrt(leftNorm) * Math.Sqrt(rightNorm));
    }
}

/// <summary>Options for <see cref="AiSdkServiceCollectionExtensions.AddAiSdk"/>.</summary>
public sealed class AiSdkOptions
{
    /// <summary>Gateway API key. Falls back to <c>AI_GATEWAY_API_KEY</c>.</summary>
    public string? GatewayApiKey { get; set; }

    /// <summary>Gateway base URL.</summary>
    public string? GatewayBaseUrl { get; set; }
}

/// <summary>Dependency injection for <see cref="IAiClient"/>.</summary>
public static class AiSdkServiceCollectionExtensions
{
    /// <summary>Named HTTP client used by <see cref="AddAiSdk"/>.</summary>
    public const string HttpClientName = "Vercel.AI.Sdk";

    /// <summary>Registers <see cref="IAiClient"/> and the Gateway HTTP client.</summary>
    public static IServiceCollection AddAiSdk(this IServiceCollection services, Action<AiSdkOptions>? configure = null)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddHttpClient(HttpClientName);
        services.AddSingleton<IAiClient>(sp =>
        {
            var options = new AiSdkOptions();
            configure?.Invoke(options);
            var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName);
            var gatewayOptions = new GatewayOptions();
            if (!string.IsNullOrEmpty(options.GatewayApiKey))
            {
                gatewayOptions.ApiKey = options.GatewayApiKey;
            }

            if (!string.IsNullOrEmpty(options.GatewayBaseUrl))
            {
                gatewayOptions.BaseUrl = options.GatewayBaseUrl!;
            }

            return new AiClient(new GatewayProvider(gatewayOptions, http), sp.GetService<IAiTelemetry>());
        });
        return services;
    }
}
