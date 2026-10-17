// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.OpenAICompatible;
using ImageModelFile = Vercel.AI.Operations.ImageModelFile;

namespace Vercel.AI.Provider;

/// <summary>Raw HTTP response of an embedding call. Maps to <c>response</c> of <c>doEmbed</c>.</summary>
public sealed class EmbeddingResponseInfo
{
    /// <summary>Creates response info.</summary>
    public EmbeddingResponseInfo(IReadOnlyDictionary<string, string>? headers, string? body)
    {
        Headers = headers ?? new Dictionary<string, string>();
        Body = body;
    }

    /// <summary>HTTP response headers.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Raw HTTP response body.</summary>
    public string? Body { get; }
}

/// <summary>Embedding vector result.</summary>
public sealed class EmbeddingResult
{
    /// <summary>Creates an embedding result.</summary>
    public EmbeddingResult(IReadOnlyList<float[]> embeddings, int? tokens, IReadOnlyList<CallWarning>? warnings = null, EmbeddingResponseInfo? response = null)
    {
        Embeddings = embeddings ?? throw new ArgumentNullException(nameof(embeddings));
        Tokens = tokens;
        Warnings = warnings ?? Array.Empty<CallWarning>();
        Response = response;
    }

    /// <summary>Warnings of this call.</summary>
    public IReadOnlyList<CallWarning> Warnings { get; }

    /// <summary>Raw HTTP response of this call, when the provider reports it.</summary>
    public EmbeddingResponseInfo? Response { get; }

    /// <summary>One vector per input value.</summary>
    public IReadOnlyList<float[]> Embeddings { get; }

    /// <summary>Token usage, when the provider reports it.</summary>
    public int? Tokens { get; }
}

/// <summary>Per-call embedding settings. Maps to the options of <c>doEmbed</c>.</summary>
public sealed class EmbeddingCallOptions
{
    /// <summary>Creates call options.</summary>
    public EmbeddingCallOptions(IReadOnlyList<string> values)
    {
        Values = values ?? throw new ArgumentNullException(nameof(values));
    }

    /// <summary>Values to embed.</summary>
    public IReadOnlyList<string> Values { get; }

    /// <summary>Provider options, keyed by provider id.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }

    /// <summary>Headers for this call. They override the provider headers.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; set; }

    /// <summary>Requested vector width. A provider option for the same setting wins.</summary>
    public int? Dimensions { get; set; }

    /// <summary>End-user identifier.</summary>
    public string? User { get; set; }
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

    /// <summary>Embeds each value. Maps to <c>doEmbed</c>. <paramref name="providerOptions"/> is keyed by provider id. <paramref name="dimensions"/> is the requested vector width. A provider option for the same setting wins. <paramref name="headers"/> are sent with this call and override the provider headers.</summary>
    Task<EmbeddingResult> DoEmbedAsync(IReadOnlyList<string> values, IReadOnlyDictionary<string, JsonElement>? providerOptions, CancellationToken cancellationToken, int? dimensions = null, IReadOnlyDictionary<string, string?>? headers = null);
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

/// <summary>Response metadata of an image call. Maps to <c>response</c> of <c>doGenerate</c> on an image model.</summary>
public sealed class ImageResponseInfo
{
    /// <summary>Creates response info.</summary>
    public ImageResponseInfo(DateTimeOffset timestamp, string? modelId, IReadOnlyDictionary<string, string>? headers)
    {
        Timestamp = timestamp;
        ModelId = modelId;
        Headers = headers ?? new Dictionary<string, string>();
    }

    /// <summary>Time the call started.</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>Model id the provider reported.</summary>
    public string? ModelId { get; }

    /// <summary>HTTP response headers.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }
}

/// <summary>Image generation result.</summary>
public class ImageGenerationResult
{
    /// <summary>Creates an image result.</summary>
    public ImageGenerationResult(IReadOnlyList<GeneratedImage> images, IReadOnlyList<CallWarning>? warnings = null, ImageResponseInfo? response = null, LanguageModelUsage? usage = null)
    {
        Images = images ?? Array.Empty<GeneratedImage>();
        CallWarnings = warnings ?? Array.Empty<CallWarning>();
        Response = response;
        Usage = usage;
    }

    /// <summary>Generated images.</summary>
    public IReadOnlyList<GeneratedImage> Images { get; }

    /// <summary>Warnings of this call. Maps to <c>warnings</c>.</summary>
    public IReadOnlyList<CallWarning> CallWarnings { get; }

    /// <summary>Response metadata of this call, when the model reports it.</summary>
    public ImageResponseInfo? Response { get; }

    /// <summary>Token usage, when the response included it.</summary>
    public LanguageModelUsage? Usage { get; }
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

    /// <summary>Seed. Models that do not support it add a warning.</summary>
    public int? Seed { get; set; }

    /// <summary>Input images for an edit. Maps to <c>files</c>.</summary>
    public IReadOnlyList<ImageModelFile>? Files { get; set; }

    /// <summary>Edit mask.</summary>
    public ImageModelFile? Mask { get; set; }

    /// <summary>Provider options, keyed by provider id.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }

    /// <summary>Headers for this call. They override the provider headers.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; set; }
}

/// <summary>
/// Numeric usage from a speech or transcription call.
/// A count stays unset when the provider does not report that unit.
/// </summary>
public sealed class AudioUsage
{
    /// <summary>Creates usage. Counts that the provider did not report stay null.</summary>
    public AudioUsage(int? characters = null, double? seconds = null, int? inputTokens = null, int? outputTokens = null, int? totalTokens = null)
    {
        Characters = characters;
        Seconds = seconds;
        InputTokens = inputTokens;
        OutputTokens = outputTokens;
        TotalTokens = totalTokens;
    }

    /// <summary>Character count, when the provider reports one.</summary>
    public int? Characters { get; }

    /// <summary>Duration in seconds, when the provider reports one.</summary>
    public double? Seconds { get; }

    /// <summary>Prompt tokens, when the provider reports them.</summary>
    public int? InputTokens { get; }

    /// <summary>Generated tokens, when the provider reports them.</summary>
    public int? OutputTokens { get; }

    /// <summary>Input plus output tokens, when the provider reports them.</summary>
    public int? TotalTokens { get; }
}

/// <summary>Speech synthesis result.</summary>
public sealed class SpeechResult
{
    /// <summary>Creates a speech result with no usage.</summary>
    public SpeechResult(byte[] audio, string mediaType)
        : this(audio, mediaType, null)
    {
    }

    /// <summary>Creates a speech result.</summary>
    public SpeechResult(byte[] audio, string mediaType, AudioUsage? usage)
    {
        Audio = audio ?? throw new ArgumentNullException(nameof(audio));
        MediaType = mediaType;
        Usage = usage;
    }

    /// <summary>Audio bytes.</summary>
    public byte[] Audio { get; }

    /// <summary>IANA media type.</summary>
    public string MediaType { get; }

    /// <summary>Usage reported by the call. Absent unless the caller supplies it.</summary>
    public AudioUsage? Usage { get; }
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
    /// <summary>Creates a transcription with no usage.</summary>
    public TranscriptionResult(string text)
        : this(text, null)
    {
    }

    /// <summary>Creates a transcription.</summary>
    public TranscriptionResult(string text, AudioUsage? usage)
    {
        Text = text ?? string.Empty;
        Usage = usage;
    }

    /// <summary>Transcript text.</summary>
    public string Text { get; }

    /// <summary>Usage reported by the call. Absent unless the caller supplies it.</summary>
    public AudioUsage? Usage { get; }
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

    /// <summary>Reranks documents for a query. <paramref name="headers"/> are sent with this call and override the provider headers.</summary>
    Task<RerankResult> DoRerankAsync(string query, IReadOnlyList<string> documents, int? topN, CancellationToken cancellationToken, IReadOnlyDictionary<string, string?>? headers = null);
}

/// <summary>Score of a candidate against a rubric. Returned by <see cref="IRubricModel"/>.</summary>
public sealed class RubricResult
{
    /// <summary>Creates a rubric result.</summary>
    public RubricResult(double score, string? reason)
    {
        Score = score;
        Reason = reason;
    }

    /// <summary>Score assigned by the evaluator.</summary>
    public double Score { get; }

    /// <summary>Explanation.</summary>
    public string? Reason { get; }
}

/// <summary>A model that scores a candidate against a rubric. Used by <see cref="IAiClient.EvaluateAsync"/>.</summary>
public interface IRubricModel
{
    /// <summary>Provider id.</summary>
    string Provider { get; }

    /// <summary>Model id.</summary>
    string ModelId { get; }

    /// <summary>Scores a candidate against a rubric.</summary>
    Task<RubricResult> DoEvaluateAsync(string rubric, string candidate, CancellationToken cancellationToken);
}

/// <summary>Deprecated name of <see cref="RubricResult"/>.</summary>
[Obsolete("Use RubricResult instead.")]
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

/// <summary>Deprecated name of <see cref="IRubricModel"/>. Pass it to <see cref="EvaluateOptions.EvaluationModel"/> or <see cref="CustomProvider.AddEvaluationModel(string, IEvaluationModel)"/>.</summary>
[Obsolete("Implement IRubricModel instead.")]
public interface IEvaluationModel
{
    /// <summary>Provider id.</summary>
    string Provider { get; }

    /// <summary>Model id.</summary>
    string ModelId { get; }

    /// <summary>Scores a candidate against a rubric.</summary>
    Task<EvaluationResult> DoEvaluateAsync(string rubric, string candidate, CancellationToken cancellationToken);
}

/// <summary>Helpers for <see cref="IRubricModel"/>.</summary>
public static class RubricModels
{
    /// <summary>
    /// Wraps a model that implements only the deprecated <see cref="IEvaluationModel"/>. A model that already implements
    /// <see cref="IRubricModel"/> is returned unchanged.
    /// </summary>
    [Obsolete("Implement IRubricModel on the model instead.")]
    public static IRubricModel FromLegacy(IEvaluationModel model)
    {
        if (model == null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        return model as IRubricModel ?? new LegacyRubricModel(model);
    }
}

#pragma warning disable CS0618
internal sealed class LegacyRubricModel : IRubricModel
{
    public LegacyRubricModel(IEvaluationModel inner)
    {
        Inner = inner;
    }

    public IEvaluationModel Inner { get; }

    public string Provider => Inner.Provider;

    public string ModelId => Inner.ModelId;

    public async Task<RubricResult> DoEvaluateAsync(string rubric, string candidate, CancellationToken cancellationToken)
    {
        var result = await Inner.DoEvaluateAsync(rubric, candidate, cancellationToken).ConfigureAwait(false);
        return new RubricResult(result.Score, result.Reason);
    }
}
#pragma warning restore CS0618

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

    /// <summary>Returns a model that scores a candidate against a rubric.</summary>
    public virtual IRubricModel RubricModel(string modelId)
    {
        throw Unsupported(modelId, "rubric");
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
