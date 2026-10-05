// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.AssemblyAI;

/// <summary>AssemblyAI settings.</summary>
public class AssemblyAIOptions : OpenAICompatibleOptions
{
    /// <summary>Clock used for response timestamps. Defaults to UTC now.</summary>
    public Func<DateTimeOffset>? Clock { get; set; }

    /// <summary>Wait between transcript status polls. Defaults to three seconds.</summary>
    public Func<CancellationToken, Task>? PollDelay { get; set; }
}

/// <summary>Parsed AssemblyAI <c>error</c> object.</summary>
public sealed class AssemblyAIErrorData
{
    /// <summary>Creates parsed error data.</summary>
    public AssemblyAIErrorData(string message, int code)
    {
        Message = message ?? string.Empty;
        Code = code;
    }

    /// <summary>Provider message. Nested JSON is left as text.</summary>
    public string Message { get; }

    /// <summary>Provider error code.</summary>
    public int Code { get; }
}

/// <summary>Result of parsing an AssemblyAI error body.</summary>
public sealed class AssemblyAIErrorParseResult
{
    /// <summary>Creates a parse result.</summary>
    public AssemblyAIErrorParseResult(bool success, AssemblyAIErrorData? value)
    {
        Success = success;
        Value = value;
        RawValue = value;
    }

    /// <summary>True when <c>error.message</c> and <c>error.code</c> were present.</summary>
    public bool Success { get; }

    /// <summary>Parsed error.</summary>
    public AssemblyAIErrorData? Value { get; }

    /// <summary>Same payload as <see cref="Value"/>.</summary>
    public AssemblyAIErrorData? RawValue { get; }
}

/// <summary>Parses AssemblyAI error JSON.</summary>
public static class AssemblyAIError
{
    /// <summary>Parses a resource error body.</summary>
    public static AssemblyAIErrorParseResult Parse(string json)
    {
        if (ProviderExchange.TryParseError(json, out var message, out var code))
        {
            return new AssemblyAIErrorParseResult(true, new AssemblyAIErrorData(message, code));
        }

        return new AssemblyAIErrorParseResult(false, null);
    }
}

/// <summary>Provider options and headers for one AssemblyAI transcription.</summary>
public sealed class AssemblyAITranscriptionRequest
{
    /// <summary>Headers merged over the provider headers for this call.</summary>
    public IDictionary<string, string>? Headers { get; set; }

    /// <summary>Provider options. The <c>assemblyai</c> object is read, with camelCase keys.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }
}

/// <summary>One transcribed word.</summary>
public sealed class AssemblyAITranscriptionSegment
{
    /// <summary>Creates a segment.</summary>
    public AssemblyAITranscriptionSegment(string text, double startSecond, double endSecond)
    {
        Text = text ?? string.Empty;
        StartSecond = startSecond;
        EndSecond = endSecond;
    }

    /// <summary>Word text.</summary>
    public string Text { get; }

    /// <summary>Start time in seconds.</summary>
    public double StartSecond { get; }

    /// <summary>End time in seconds.</summary>
    public double EndSecond { get; }
}

/// <summary>Metadata returned with a transcription.</summary>
public sealed class AssemblyAITranscriptionResponse
{
    /// <summary>Creates response metadata.</summary>
    public AssemblyAITranscriptionResponse(DateTimeOffset timestamp, string modelId, IReadOnlyDictionary<string, string> headers, string body)
    {
        Timestamp = timestamp;
        ModelId = modelId ?? string.Empty;
        Headers = headers ?? new Dictionary<string, string>();
        Body = body ?? string.Empty;
    }

    /// <summary>Timestamp captured when the call started.</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>Model id.</summary>
    public string ModelId { get; }

    /// <summary>Headers from the final transcript poll.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Raw transcript JSON from the final poll.</summary>
    public string Body { get; }
}

/// <summary>AssemblyAI transcription, including word segments and audio-intelligence metadata.</summary>
public sealed class AssemblyAITranscriptionResult
{
    /// <summary>Creates a transcription result.</summary>
    public AssemblyAITranscriptionResult(
        string text,
        IReadOnlyList<AssemblyAITranscriptionSegment> segments,
        string? language,
        double? durationInSeconds,
        IReadOnlyList<ModelWarning> warnings,
        JsonElement? providerMetadata,
        AssemblyAITranscriptionResponse response)
    {
        Text = text ?? string.Empty;
        Segments = segments ?? Array.Empty<AssemblyAITranscriptionSegment>();
        Language = language;
        DurationInSeconds = durationInSeconds;
        Warnings = warnings ?? Array.Empty<ModelWarning>();
        ProviderMetadata = providerMetadata;
        Response = response;
    }

    /// <summary>Full transcript.</summary>
    public string Text { get; }

    /// <summary>Words with timings in seconds.</summary>
    public IReadOnlyList<AssemblyAITranscriptionSegment> Segments { get; }

    /// <summary>Language code reported on the transcript.</summary>
    public string? Language { get; }

    /// <summary>Audio duration, or the last word end when the duration is missing.</summary>
    public double? DurationInSeconds { get; }

    /// <summary>Warnings produced while building the request.</summary>
    public IReadOnlyList<ModelWarning> Warnings { get; }

    /// <summary>
    /// Diarization and audio-intelligence results under <c>assemblyai</c>, when the transcript has any.
    /// Timings inside these objects stay in milliseconds, as AssemblyAI returns them.
    /// </summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Response metadata.</summary>
    public AssemblyAITranscriptionResponse Response { get; }
}

/// <summary>AssemblyAI provider.</summary>
public sealed class AssemblyAIProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "assemblyai";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.assemblyai.com";

    private readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public AssemblyAIProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        throw new AiSdkException("AssemblyAI does not provide language models.");
    }

    /// <summary>Creates a provider.</summary>
    public static new AssemblyAIProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new AssemblyAIProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "ASSEMBLYAI_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.RawAuthorization;
        options.UserAgent = ProviderExchange.UserAgent("assemblyai");
        return options;
    }

    /// <inheritdoc />
    public override ITranscriptionModel TranscriptionModel(string modelId) => Transcription(modelId);

    /// <summary>Creates the typed transcription model.</summary>
    public AssemblyAITranscriptionModel Transcription(string modelId) => new AssemblyAITranscriptionModel(this, modelId);

    /// <summary>Uploads audio, submits a transcript, and polls until it completes.</summary>
    public sealed class AssemblyAITranscriptionModel : ITranscriptionModel
    {
        private const string SpeechModelDocs = "https://www.assemblyai.com/docs/pre-recorded-audio/select-the-speech-model";

        // Provider options copied to the request with snake_case names. Nested objects list their own keys.
        private static readonly string[] FlatOptions =
        {
            "audioEndAt", "audioStartFrom", "autoChapters", "autoHighlights", "boostParam", "contentSafety",
            "contentSafetyConfidence", "customSpelling", "disfluencies", "entityDetection", "filterProfanity",
            "formatText", "iabCategories", "languageCode", "languageConfidenceThreshold", "languageDetection",
            "multichannel", "punctuate", "redactPii", "redactPiiAudio", "redactPiiAudioQuality", "redactPiiPolicies",
            "redactPiiSub", "sentimentAnalysis", "speakerLabels", "speakersExpected", "speechThreshold", "summarization",
            "summaryModel", "summaryType", "webhookAuthHeaderName", "webhookAuthHeaderValue", "webhookUrl", "wordBoost",
            "keytermsPrompt", "prompt", "temperature", "removeAudioTags", "domain", "redactPiiReturnUnredacted",
            "redactStaticEntities",
        };

        private static readonly KeyValuePair<string, string[]>[] NestedOptions =
        {
            new("speakerOptions", new[] { "minSpeakersExpected", "maxSpeakersExpected" }),
            new("languageDetectionOptions", new[] { "expectedLanguages", "fallbackLanguage", "codeSwitching", "codeSwitchingConfidenceThreshold" }),
            new("redactPiiAudioOptions", new[] { "returnRedactedNoSpeechAudio", "overrideAudioRedactionMethod" }),
        };

        // Transcript fields that the segment shape cannot carry, surfaced as provider metadata.
        private static readonly KeyValuePair<string, string>[] MetadataFields =
        {
            new("utterances", "utterances"),
            new("sentiment_analysis_results", "sentimentAnalysisResults"),
            new("entities", "entities"),
            new("content_safety_labels", "contentSafetyLabels"),
            new("iab_categories_result", "iabCategoriesResult"),
            new("auto_highlights_result", "autoHighlightsResult"),
        };

        private readonly AssemblyAIProvider _provider;

        /// <summary>Creates a transcription model.</summary>
        public AssemblyAITranscriptionModel(AssemblyAIProvider provider, string modelId)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            ModelId = modelId ?? string.Empty;
        }

        /// <inheritdoc />
        public string Provider => "assemblyai.transcription";

        /// <inheritdoc />
        public string ModelId { get; }

        /// <inheritdoc />
        public async Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
        {
            var result = await TranscribeAsync(audio, null, cancellationToken).ConfigureAwait(false);
            return new TranscriptionResult(result.Text);
        }

        /// <summary>Uploads audio, submits a transcript, and polls until it completes.</summary>
        public async Task<AssemblyAITranscriptionResult> TranscribeAsync(AudioInput audio, AssemblyAITranscriptionRequest? request, CancellationToken cancellationToken)
        {
            if (audio == null)
            {
                throw new ArgumentNullException(nameof(audio));
            }

            var options = _provider.Options as AssemblyAIOptions;
            var started = options?.Clock != null ? options.Clock() : DateTimeOffset.UtcNow;
            var headers = ProviderExchange.Merge(_provider.CreateHeaders(), request?.Headers);
            var uploadContent = new ByteArrayContent(audio.Data);
            uploadContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            var uploaded = await ProviderExchange.SendAsync(_provider._httpClient, HttpMethod.Post, ApiKeys.Combine(_provider.Options.BaseUrl, "/v2/upload"), uploadContent, headers, cancellationToken).ConfigureAwait(false);
            string uploadUrl;
            using (var uploadDocument = JsonDocument.Parse(uploaded.Body))
            {
                uploadUrl = RequiredString(uploadDocument.RootElement, "upload_url");
            }

            var warnings = new List<ModelWarning>();
            var body = Body(AssemblyAIObject(request?.ProviderOptions), warnings);
            body["audio_url"] = uploadUrl;
            var submitted = await ProviderExchange.SendAsync(_provider._httpClient, HttpMethod.Post, ApiKeys.Combine(_provider.Options.BaseUrl, "/v2/transcript"), ProviderExchange.Json(body.ToJsonString()), headers, cancellationToken).ConfigureAwait(false);
            string transcriptId;
            using (var submitDocument = JsonDocument.Parse(submitted.Body))
            {
                transcriptId = RequiredString(submitDocument.RootElement, "id");
            }

            var completed = await WaitForCompletionAsync(transcriptId, headers, options, cancellationToken).ConfigureAwait(false);
            using (var document = JsonDocument.Parse(completed.Body))
            {
                var root = document.RootElement;
                var segments = new List<AssemblyAITranscriptionSegment>();
                if (root.TryGetProperty("words", out var words) && words.ValueKind == JsonValueKind.Array)
                {
                    foreach (var word in words.EnumerateArray())
                    {
                        segments.Add(new AssemblyAITranscriptionSegment(
                            ReadString(word, "text") ?? string.Empty,
                            word.GetProperty("start").GetDouble() / 1000,
                            word.GetProperty("end").GetDouble() / 1000));
                    }
                }

                double? duration = root.TryGetProperty("audio_duration", out var audioDuration) && audioDuration.ValueKind == JsonValueKind.Number
                    ? audioDuration.GetDouble()
                    : segments.Count > 0 ? segments[segments.Count - 1].EndSecond : (double?)null;
                return new AssemblyAITranscriptionResult(
                    ReadString(root, "text") ?? string.Empty,
                    segments,
                    ReadString(root, "language_code"),
                    duration,
                    warnings,
                    Metadata(root),
                    new AssemblyAITranscriptionResponse(started, ModelId, completed.Headers, completed.Body));
            }
        }

        private async Task<ProviderExchangeResult> WaitForCompletionAsync(string transcriptId, IReadOnlyDictionary<string, string?> headers, AssemblyAIOptions? options, CancellationToken cancellationToken)
        {
            var url = ApiKeys.Combine(_provider.Options.BaseUrl, "/v2/transcript/" + transcriptId);
            while (true)
            {
                var response = await ProviderExchange.SendAsync(_provider._httpClient, HttpMethod.Get, url, null, headers, cancellationToken).ConfigureAwait(false);
                using (var document = JsonDocument.Parse(response.Body))
                {
                    var status = ReadString(document.RootElement, "status");
                    switch (status)
                    {
                        case "completed":
                            return response;
                        case "error":
                            throw new AiSdkException("Transcription failed: " + (ReadString(document.RootElement, "error") ?? "Unknown error"));
                        case "queued":
                        case "processing":
                            break;
                        default:
                            throw new AiSdkException("Unexpected AssemblyAI transcript status: " + (status ?? "missing") + ".");
                    }
                }

                if (options?.PollDelay != null)
                {
                    await options.PollDelay(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await Task.Delay(3000, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private JsonObject Body(JsonElement? assemblyai, List<ModelWarning> warnings)
        {
            var body = new JsonObject();

            // The legacy best model uses the singular speech_model field; every other model goes in speech_models.
            if (ModelId == "best")
            {
                body["speech_model"] = ModelId;
                warnings.Add(new DeprecatedWarning(
                    "model 'best'",
                    "The 'best' model is a legacy AssemblyAI model. Use 'universal-3-5-pro' instead. See documentation: " + SpeechModelDocs));
            }
            else
            {
                body["speech_models"] = new JsonArray(ModelId);
                if (ModelId == "universal-3-pro")
                {
                    warnings.Add(new OtherWarning("'universal-3-5-pro' is AssemblyAI's latest flagship model and is set to replace 'universal-3-pro'. See " + SpeechModelDocs));
                }
                else if (ModelId == "universal-2")
                {
                    warnings.Add(new OtherWarning("'universal-3-5-pro' is AssemblyAI's latest flagship model. See " + SpeechModelDocs));
                }
            }

            if (assemblyai == null)
            {
                return body;
            }

            var source = assemblyai.Value;
            foreach (var name in FlatOptions)
            {
                if (Present(source, name, out var value))
                {
                    body[SnakeCase(name)] = JsonNode.Parse(value.GetRawText());
                }
            }

            foreach (var nested in NestedOptions)
            {
                if (!Present(source, nested.Key, out var value))
                {
                    continue;
                }

                var target = new JsonObject();
                foreach (var name in nested.Value)
                {
                    if (Present(value, name, out var inner))
                    {
                        target[SnakeCase(name)] = JsonNode.Parse(inner.GetRawText());
                    }
                }

                body[SnakeCase(nested.Key)] = target;
            }

            var deprecatedBoost = new List<string>();
            if (Present(source, "wordBoost", out _))
            {
                deprecatedBoost.Add("wordBoost");
            }

            if (Present(source, "boostParam", out _))
            {
                deprecatedBoost.Add("boostParam");
            }

            if (deprecatedBoost.Count > 0)
            {
                warnings.Add(new DeprecatedWarning(
                    string.Join(", ", deprecatedBoost),
                    "'wordBoost' and 'boostParam' are deprecated and are rejected by 'universal-3-pro' / 'universal-3-5-pro' and 'slam-1'. Use 'keytermsPrompt' instead."));
            }

            // These options only work alongside a prerequisite option. Warn rather than change the request.
            if ((Present(source, "redactPiiReturnUnredacted", out _) || Present(source, "redactStaticEntities", out _)) && !IsTrue(source, "redactPii"))
            {
                warnings.Add(new OtherWarning("'redactPiiReturnUnredacted' and 'redactStaticEntities' require 'redactPii' to be enabled; AssemblyAI rejects the request otherwise."));
            }

            if (Present(source, "redactPiiAudioOptions", out _) && !IsTrue(source, "redactPiiAudio"))
            {
                warnings.Add(new OtherWarning("'redactPiiAudioOptions' only applies when 'redactPiiAudio' is enabled; it is otherwise ignored."));
            }

            if (Present(source, "languageCode", out _) && IsTrue(source, "languageDetection"))
            {
                warnings.Add(new OtherWarning("'languageDetection' cannot be combined with an explicit 'languageCode'; AssemblyAI rejects requests that set both."));
            }

            return body;
        }

        private static JsonElement? Metadata(JsonElement root)
        {
            var assemblyai = new JsonObject();
            foreach (var field in MetadataFields)
            {
                if (Present(root, field.Key, out var value))
                {
                    assemblyai[field.Value] = JsonNode.Parse(value.GetRawText());
                }
            }

            if (assemblyai.Count == 0)
            {
                return null;
            }

            using (var document = JsonDocument.Parse(new JsonObject { ["assemblyai"] = assemblyai }.ToJsonString()))
            {
                return document.RootElement.Clone();
            }
        }

        private static JsonElement? AssemblyAIObject(IReadOnlyDictionary<string, JsonElement>? providerOptions)
        {
            return providerOptions != null && providerOptions.TryGetValue(ProviderId, out var value) && value.ValueKind == JsonValueKind.Object
                ? value
                : null;
        }

        private static bool Present(JsonElement element, string name, out JsonElement value)
        {
            return element.TryGetProperty(name, out value) && value.ValueKind != JsonValueKind.Null;
        }

        private static bool IsTrue(JsonElement element, string name)
        {
            return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.True;
        }

        private static string SnakeCase(string name)
        {
            var builder = new StringBuilder(name.Length + 8);
            foreach (var character in name)
            {
                if (char.IsUpper(character))
                {
                    builder.Append('_').Append(char.ToLowerInvariant(character));
                }
                else
                {
                    builder.Append(character);
                }
            }

            return builder.ToString();
        }

        private static string RequiredString(JsonElement element, string name)
        {
            return ReadString(element, name) ?? throw new AiSdkException("AssemblyAI response is missing '" + name + "'.");
        }

        private static string? ReadString(JsonElement element, string name)
        {
            if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
            {
                return value.GetString();
            }

            return null;
        }
    }
}

/// <summary>Registers AssemblyAI.</summary>
public static class AssemblyAIServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddAssemblyAI(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(AssemblyAIProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new AssemblyAIProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(AssemblyAIProvider.ProviderId), options);
        });
        return services;
    }
}
