// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Sdk.Provider;

/// <summary>Embedding vector result.</summary>
public sealed class EmbeddingResult
{
    /// <summary>Creates an embedding result.</summary>
    public EmbeddingResult(IReadOnlyList<float[]> embeddings, int? tokens)
    {
        Embeddings = embeddings ?? throw new ArgumentNullException(nameof(embeddings));
        Tokens = tokens;
    }

    /// <summary>One vector per input value.</summary>
    public IReadOnlyList<float[]> Embeddings { get; }

    /// <summary>Token usage, when the provider reports it.</summary>
    public int? Tokens { get; }
}

/// <summary>Embedding model specification.</summary>
public interface IEmbeddingModel
{
    /// <summary>Specification version. Always <c>V4</c>.</summary>
    string SpecificationVersion { get; }

    /// <summary>Provider id.</summary>
    string Provider { get; }

    /// <summary>Model id.</summary>
    string ModelId { get; }

    /// <summary>Embeds each value. Maps to <c>doEmbed</c>.</summary>
    Task<EmbeddingResult> DoEmbedAsync(IReadOnlyList<string> values, CancellationToken cancellationToken);
}

/// <summary>One generated image.</summary>
public sealed class GeneratedImage
{
    /// <summary>Creates an image.</summary>
    public GeneratedImage(string mediaType, byte[]? data, string? url)
    {
        MediaType = mediaType;
        Data = data;
        Url = url;
    }

    /// <summary>IANA media type.</summary>
    public string MediaType { get; }

    /// <summary>Inline image bytes.</summary>
    public byte[]? Data { get; }

    /// <summary>Remote URL.</summary>
    public string? Url { get; }
}

/// <summary>Image generation result.</summary>
public sealed class ImageGenerationResult
{
    /// <summary>Creates an image result.</summary>
    public ImageGenerationResult(IReadOnlyList<GeneratedImage> images)
    {
        Images = images ?? Array.Empty<GeneratedImage>();
    }

    /// <summary>Generated images.</summary>
    public IReadOnlyList<GeneratedImage> Images { get; }
}

/// <summary>Image model specification.</summary>
public interface IImageModel
{
    /// <summary>Provider id.</summary>
    string Provider { get; }

    /// <summary>Model id.</summary>
    string ModelId { get; }

    /// <summary>Generates images. Maps to <c>doGenerate</c> on an image model.</summary>
    Task<ImageGenerationResult> DoGenerateAsync(ImageCallOptions options, CancellationToken cancellationToken);
}

/// <summary>Image call settings.</summary>
public sealed class ImageCallOptions
{
    /// <summary>Creates image call options.</summary>
    public ImageCallOptions(string prompt)
    {
        Prompt = prompt ?? throw new ArgumentNullException(nameof(prompt));
    }

    /// <summary>Image prompt.</summary>
    public string Prompt { get; }

    /// <summary>How many images to generate.</summary>
    public int Count { get; set; } = 1;

    /// <summary>Size string such as <c>1024x1024</c>.</summary>
    public string? Size { get; set; }

    /// <summary>Aspect ratio such as <c>16:9</c>.</summary>
    public string? AspectRatio { get; set; }
}

/// <summary>Speech synthesis result.</summary>
public sealed class SpeechResult
{
    /// <summary>Creates a speech result.</summary>
    public SpeechResult(byte[] audio, string mediaType)
    {
        Audio = audio ?? throw new ArgumentNullException(nameof(audio));
        MediaType = mediaType;
    }

    /// <summary>Audio bytes.</summary>
    public byte[] Audio { get; }

    /// <summary>IANA media type.</summary>
    public string MediaType { get; }
}

/// <summary>Speech call settings.</summary>
public sealed class SpeechCallOptions
{
    /// <summary>Creates speech options.</summary>
    public SpeechCallOptions(string text)
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
    }

    /// <summary>Text to speak.</summary>
    public string Text { get; }

    /// <summary>Voice id, when the provider uses one.</summary>
    public string? Voice { get; set; }
}

/// <summary>Speech model specification.</summary>
public interface ISpeechModel
{
    /// <summary>Provider id.</summary>
    string Provider { get; }

    /// <summary>Model id.</summary>
    string ModelId { get; }

    /// <summary>Synthesizes speech.</summary>
    Task<SpeechResult> DoGenerateAsync(SpeechCallOptions options, CancellationToken cancellationToken);
}

/// <summary>Transcription result.</summary>
public sealed class TranscriptionResult
{
    /// <summary>Creates a transcription.</summary>
    public TranscriptionResult(string text)
    {
        Text = text ?? string.Empty;
    }

    /// <summary>Transcript text.</summary>
    public string Text { get; }
}

/// <summary>Audio sent to a transcription or translation model.</summary>
public sealed class AudioInput
{
    /// <summary>Creates audio input.</summary>
    public AudioInput(byte[] data, string mediaType, string? fileName)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        MediaType = mediaType;
        FileName = fileName;
    }

    /// <summary>Audio bytes.</summary>
    public byte[] Data { get; }

    /// <summary>IANA media type.</summary>
    public string MediaType { get; }

    /// <summary>File name sent in multipart bodies.</summary>
    public string? FileName { get; }
}

/// <summary>Transcription model specification.</summary>
public interface ITranscriptionModel
{
    /// <summary>Provider id.</summary>
    string Provider { get; }

    /// <summary>Model id.</summary>
    string ModelId { get; }

    /// <summary>Transcribes audio.</summary>
    Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken);
}

/// <summary>Speech translation model specification.</summary>
public interface ISpeechTranslationModel
{
    /// <summary>Provider id.</summary>
    string Provider { get; }

    /// <summary>Model id.</summary>
    string ModelId { get; }

    /// <summary>Translates speech into text.</summary>
    Task<TranscriptionResult> DoTranslateAsync(AudioInput audio, CancellationToken cancellationToken);
}

/// <summary>Video generation result.</summary>
public sealed class VideoResult
{
    /// <summary>Creates a video result.</summary>
    public VideoResult(string? url, byte[]? data, string mediaType)
    {
        Url = url;
        Data = data;
        MediaType = mediaType;
    }

    /// <summary>Remote video URL.</summary>
    public string? Url { get; }

    /// <summary>Inline video bytes.</summary>
    public byte[]? Data { get; }

    /// <summary>IANA media type.</summary>
    public string MediaType { get; }
}

/// <summary>Video call settings.</summary>
public sealed class VideoCallOptions
{
    /// <summary>Creates video options.</summary>
    public VideoCallOptions(string prompt)
    {
        Prompt = prompt ?? throw new ArgumentNullException(nameof(prompt));
    }

    /// <summary>Video prompt.</summary>
    public string Prompt { get; }

    /// <summary>Duration hint in seconds.</summary>
    public int? DurationSeconds { get; set; }
}

/// <summary>Video model specification.</summary>
public interface IVideoModel
{
    /// <summary>Provider id.</summary>
    string Provider { get; }

    /// <summary>Model id.</summary>
    string ModelId { get; }

    /// <summary>Generates a video.</summary>
    Task<VideoResult> DoGenerateAsync(VideoCallOptions options, CancellationToken cancellationToken);
}

/// <summary>One reranked document.</summary>
public sealed class RerankItem
{
    /// <summary>Creates a rerank item.</summary>
    public RerankItem(int index, double score)
    {
        Index = index;
        Score = score;
    }

    /// <summary>Index in the original document list.</summary>
    public int Index { get; }

    /// <summary>Relevance score.</summary>
    public double Score { get; }
}

/// <summary>Rerank result.</summary>
public sealed class RerankResult
{
    /// <summary>Creates a rerank result.</summary>
    public RerankResult(IReadOnlyList<RerankItem> items)
    {
        Items = items ?? Array.Empty<RerankItem>();
    }

    /// <summary>Documents ordered by relevance.</summary>
    public IReadOnlyList<RerankItem> Items { get; }
}

/// <summary>Reranking model specification.</summary>
public interface IRerankingModel
{
    /// <summary>Provider id.</summary>
    string Provider { get; }

    /// <summary>Model id.</summary>
    string ModelId { get; }

    /// <summary>Reranks documents for a query.</summary>
    Task<RerankResult> DoRerankAsync(string query, IReadOnlyList<string> documents, int? topN, CancellationToken cancellationToken);
}

/// <summary>Evaluation result.</summary>
public sealed class EvaluationResult
{
    /// <summary>Creates an evaluation result.</summary>
    public EvaluationResult(double score, string? reason)
    {
        Score = score;
        Reason = reason;
    }

    /// <summary>Score assigned by the evaluator.</summary>
    public double Score { get; }

    /// <summary>Explanation.</summary>
    public string? Reason { get; }
}

/// <summary>Evaluation model specification.</summary>
public interface IEvaluationModel
{
    /// <summary>Provider id.</summary>
    string Provider { get; }

    /// <summary>Model id.</summary>
    string ModelId { get; }

    /// <summary>Scores a candidate against a rubric.</summary>
    Task<EvaluationResult> DoEvaluateAsync(string rubric, string candidate, CancellationToken cancellationToken);
}

/// <summary>A submitted batch job.</summary>
public sealed class BatchJob
{
    /// <summary>Creates a batch job.</summary>
    public BatchJob(string id, string status)
    {
        Id = id;
        Status = status;
    }

    /// <summary>Provider job id.</summary>
    public string Id { get; }

    /// <summary>Provider status.</summary>
    public string Status { get; }
}

/// <summary>Batch model specification.</summary>
public interface IBatchModel
{
    /// <summary>Provider id.</summary>
    string Provider { get; }

    /// <summary>Submits a batch and returns the job.</summary>
    Task<BatchJob> SubmitAsync(string inputFileId, string endpoint, CancellationToken cancellationToken);

    /// <summary>Reads a batch job.</summary>
    Task<BatchJob> GetAsync(string id, CancellationToken cancellationToken);
}

/// <summary>An uploaded file.</summary>
public sealed class UploadedFile
{
    /// <summary>Creates an uploaded file.</summary>
    public UploadedFile(string id, string? fileName)
    {
        Id = id;
        FileName = fileName;
    }

    /// <summary>Provider file id.</summary>
    public string Id { get; }

    /// <summary>File name.</summary>
    public string? FileName { get; }
}

/// <summary>File store used by <c>uploadFile</c>.</summary>
public interface IFileStore
{
    /// <summary>Uploads a file.</summary>
    Task<UploadedFile> UploadFileAsync(string fileName, byte[] data, string mediaType, CancellationToken cancellationToken);
}

/// <summary>An uploaded skill.</summary>
public sealed class UploadedSkill
{
    /// <summary>Creates an uploaded skill.</summary>
    public UploadedSkill(string id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>Provider skill id.</summary>
    public string Id { get; }

    /// <summary>Skill name.</summary>
    public string Name { get; }
}

/// <summary>Skill store used by <c>uploadSkill</c>.</summary>
public interface ISkillStore
{
    /// <summary>Uploads a skill document.</summary>
    Task<UploadedSkill> UploadSkillAsync(string name, string instructions, CancellationToken cancellationToken);
}

/// <summary>A realtime session.</summary>
public interface IRealtimeSession : IAsyncDisposable
{
    /// <summary>Events from the provider.</summary>
    IAsyncEnumerable<string> Events { get; }

    /// <summary>Sends a JSON event to the provider.</summary>
    Task SendAsync(string json, CancellationToken cancellationToken);
}

/// <summary>Realtime model specification.</summary>
public interface IRealtimeModel
{
    /// <summary>Provider id.</summary>
    string Provider { get; }

    /// <summary>Model id.</summary>
    string ModelId { get; }

    /// <summary>WebSocket URI for this model.</summary>
    Uri BuildUri();

    /// <summary>Opens a realtime session.</summary>
    Task<IRealtimeSession> ConnectAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Provider factory. Maps to <c>ProviderV4</c>. Modalities a provider does not implement throw
/// <see cref="AiSdkException"/>.
/// </summary>
public abstract class ProviderBase
{
    /// <summary>Creates a provider.</summary>
    protected ProviderBase(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>Provider id.</summary>
    public string Name { get; }

    /// <summary>Returns a language model.</summary>
    public abstract ILanguageModel LanguageModel(string modelId);

    /// <summary>Returns an embedding model.</summary>
    public virtual IEmbeddingModel EmbeddingModel(string modelId)
    {
        throw Unsupported(modelId, "embedding");
    }

    /// <summary>Returns an image model.</summary>
    public virtual IImageModel ImageModel(string modelId)
    {
        throw Unsupported(modelId, "image");
    }

    /// <summary>Returns a speech model.</summary>
    public virtual ISpeechModel SpeechModel(string modelId)
    {
        throw Unsupported(modelId, "speech");
    }

    /// <summary>Returns a transcription model.</summary>
    public virtual ITranscriptionModel TranscriptionModel(string modelId)
    {
        throw Unsupported(modelId, "transcription");
    }

    /// <summary>Returns a speech translation model.</summary>
    public virtual ISpeechTranslationModel SpeechTranslationModel(string modelId)
    {
        throw Unsupported(modelId, "speech translation");
    }

    /// <summary>Returns a video model.</summary>
    public virtual IVideoModel VideoModel(string modelId)
    {
        throw Unsupported(modelId, "video");
    }

    /// <summary>Returns a reranking model.</summary>
    public virtual IRerankingModel RerankingModel(string modelId)
    {
        throw Unsupported(modelId, "reranking");
    }

    /// <summary>Returns an evaluation model.</summary>
    public virtual IEvaluationModel EvaluationModel(string modelId)
    {
        throw Unsupported(modelId, "evaluation");
    }

    /// <summary>Returns a batch model.</summary>
    public virtual IBatchModel BatchModel()
    {
        throw new AiSdkException($"Provider '{Name}' does not support batch jobs.");
    }

    /// <summary>Returns a file store.</summary>
    public virtual IFileStore FileStore()
    {
        throw new AiSdkException($"Provider '{Name}' does not support file uploads.");
    }

    /// <summary>Returns a skill store.</summary>
    public virtual ISkillStore SkillStore()
    {
        throw new AiSdkException($"Provider '{Name}' does not support skill uploads.");
    }

    /// <summary>Returns a realtime model.</summary>
    public virtual IRealtimeModel RealtimeModel(string modelId)
    {
        throw Unsupported(modelId, "realtime");
    }

    private AiSdkException Unsupported(string modelId, string modality)
    {
        return new AiSdkException($"Provider '{Name}' does not support {modality} model '{modelId}'.");
    }
}
