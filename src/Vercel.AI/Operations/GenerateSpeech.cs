// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.Operations;

/// <summary>Arguments for <c>doGenerate</c> on a speech model.</summary>
public sealed class SpeechModelCall
{
    /// <summary>Creates a speech call.</summary>
    public SpeechModelCall(
        string text,
        string? voice,
        string? outputFormat,
        string? instructions,
        double? speed,
        string? language,
        JsonElement providerOptions,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken cancellationToken
    )
    {
        Text = text ?? string.Empty;
        Voice = voice;
        OutputFormat = outputFormat;
        Instructions = instructions;
        Speed = speed;
        Language = language;
        ProviderOptions = providerOptions;
        Headers = headers;
        CancellationToken = cancellationToken;
    }

    /// <summary>Text to speak.</summary>
    public string Text { get; }

    /// <summary>Voice id.</summary>
    public string? Voice { get; }

    /// <summary>Requested output format.</summary>
    public string? OutputFormat { get; }

    /// <summary>Speaking instructions.</summary>
    public string? Instructions { get; }

    /// <summary>Playback speed.</summary>
    public double? Speed { get; }

    /// <summary>Language or <c>auto</c>.</summary>
    public string? Language { get; }

    /// <summary>Provider options. Empty object when omitted.</summary>
    public JsonElement ProviderOptions { get; }

    /// <summary>Headers including the user-agent suffix.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Cancellation token.</summary>
    public CancellationToken CancellationToken { get; }
}

/// <summary>Speech model response.</summary>
public sealed class SpeechModelResult
{
    /// <summary>Creates a speech result.</summary>
    public SpeechModelResult(
        byte[]? audio,
        IReadOnlyList<OperationWarning>? warnings = null,
        JsonElement? providerMetadata = null,
        ProviderResponse? response = null,
        ProviderRequest? request = null,
        JsonElement? usage = null
    )
    {
        Request = request;
        Usage = usage;
        Audio = audio ?? Array.Empty<byte>();
        Warnings = warnings ?? Array.Empty<OperationWarning>();
        ProviderMetadata = providerMetadata;
        Response = response ?? new ProviderResponse();
    }

    /// <summary>Audio bytes. Empty when the provider returned none.</summary>
    public byte[] Audio { get; }

    /// <summary>Warnings.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Response metadata.</summary>
    public ProviderResponse Response { get; }

    /// <summary>Request metadata, such as the body sent to the provider. Maps to <c>request</c>.</summary>
    public ProviderRequest? Request { get; }

    /// <summary>Usage reported by the provider, in the provider's native shape. Maps to <c>usage</c>.</summary>
    public JsonElement? Usage { get; }
}

/// <summary>Speech model used by <see cref="GenerateSpeech"/>.</summary>
public interface ISpeechCaller
{
    /// <summary>Provider id.</summary>
    string Provider { get; }

    /// <summary>Model id.</summary>
    string ModelId { get; }

    /// <summary>Generates speech audio.</summary>
    Task<SpeechModelResult> DoGenerateAsync(
        SpeechModelCall call,
        CancellationToken cancellationToken
    );
}

/// <summary>Generated speech audio.</summary>
public sealed class GeneratedAudio
{
    /// <summary>Creates generated audio.</summary>
    /// <exception cref="InvalidResponseDataException">The media type has an empty subtype, such as <c>audio/</c>.</exception>
    public GeneratedAudio(byte[] data, string mediaType)
    {
        Data = data ?? Array.Empty<byte>();
        MediaType = mediaType ?? "audio/mp3";
        Format = "mp3";
        var parts = MediaType.Split('/');
        if (parts.Length == 2 && MediaType != "audio/mpeg")
        {
            if (parts[1].Length == 0)
            {
                throw new InvalidResponseDataException(
                    MediaType,
                    "Could not determine audio format from media type: " + MediaType
                );
            }

            Format = parts[1];
        }
    }

    /// <summary>Audio bytes.</summary>
    public byte[] Data { get; }

    /// <summary>Media type.</summary>
    public string MediaType { get; }

    /// <summary>Format subtype. <c>audio/mpeg</c> stays <c>mp3</c>.</summary>
    public string Format { get; }
}

/// <summary>Result of <see cref="GenerateSpeech.GenerateSpeechAsync"/>.</summary>
public sealed class GenerateSpeechResult
{
    /// <summary>Creates a speech result.</summary>
    public GenerateSpeechResult(
        GeneratedAudio audio,
        IReadOnlyList<OperationWarning> warnings,
        IReadOnlyList<ProviderResponse> responses,
        JsonElement providerMetadata
    )
    {
        Audio = audio;
        Warnings = warnings ?? Array.Empty<OperationWarning>();
        Responses = responses ?? Array.Empty<ProviderResponse>();
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Generated audio.</summary>
    public GeneratedAudio Audio { get; }

    /// <summary>Warnings.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }

    /// <summary>Provider responses.</summary>
    public IReadOnlyList<ProviderResponse> Responses { get; }

    /// <summary>Provider metadata. Empty object when omitted.</summary>
    public JsonElement ProviderMetadata { get; }
}

/// <summary>Metadata about the generated audio of a <see cref="GenerateSpeechEndEvent"/>. Raw audio is intentionally not included.</summary>
public sealed class GenerateSpeechAudioInfo
{
    /// <summary>Creates the metadata.</summary>
    public GenerateSpeechAudioInfo(int byteLength, string mediaType, string format)
    {
        ByteLength = byteLength;
        MediaType = mediaType;
        Format = format;
    }

    /// <summary>Size of the audio in bytes.</summary>
    public int ByteLength { get; }

    /// <summary>Media type of the audio.</summary>
    public string MediaType { get; }

    /// <summary>Format of the audio.</summary>
    public string Format { get; }
}

/// <summary>Event passed to telemetry integrations when speech generation begins. Maps to <c>GenerateSpeechStartEvent</c>.</summary>
public sealed class GenerateSpeechStartEvent : IOperationStartEvent
{
    /// <summary>Creates the event.</summary>
    public GenerateSpeechStartEvent(
        string callId,
        string provider,
        string modelId,
        string text,
        string? voice,
        string? outputFormat,
        string? instructions,
        double? speed,
        string? language,
        int maxRetries,
        IReadOnlyDictionary<string, string>? headers,
        JsonElement providerOptions
    )
    {
        CallId = callId;
        Provider = provider;
        ModelId = modelId;
        Text = text;
        Voice = voice;
        OutputFormat = outputFormat;
        Instructions = instructions;
        Speed = speed;
        Language = language;
        MaxRetries = maxRetries;
        Headers = headers;
        ProviderOptions = providerOptions;
    }

    /// <summary>Unique identifier for this speech generation call.</summary>
    public string CallId { get; }

    /// <summary><c>ai.generateSpeech</c>.</summary>
    public string OperationId => "ai.generateSpeech";

    /// <summary>The provider identifier.</summary>
    public string Provider { get; }

    /// <summary>The specific model identifier.</summary>
    public string ModelId { get; }

    /// <summary>The text to convert to speech.</summary>
    public string Text { get; }

    /// <summary>The requested voice.</summary>
    public string? Voice { get; }

    /// <summary>The requested output format.</summary>
    public string? OutputFormat { get; }

    /// <summary>Additional speech instructions.</summary>
    public string? Instructions { get; }

    /// <summary>The requested speech speed.</summary>
    public double? Speed { get; }

    /// <summary>The requested language.</summary>
    public string? Language { get; }

    /// <summary>Maximum number of retries for failed requests.</summary>
    public int MaxRetries { get; }

    /// <summary>Additional HTTP headers sent with the request.</summary>
    public IReadOnlyDictionary<string, string>? Headers { get; }

    /// <summary>Additional provider-specific options.</summary>
    public JsonElement ProviderOptions { get; }

    /// <summary>The version of the SDK that emitted the event. Set only on events sent to telemetry integrations. Maps to <c>aiVersion</c>.</summary>
    public string? AiVersion { get; init; }

    /// <summary>Copied from <see cref="TelemetryOptions.RecordInputs"/>. Set only on events sent to telemetry integrations.</summary>
    public bool? RecordInputs { get; init; }

    /// <summary>Copied from <see cref="TelemetryOptions.RecordOutputs"/>. Set only on events sent to telemetry integrations.</summary>
    public bool? RecordOutputs { get; init; }

    /// <summary>Copied from <see cref="TelemetryOptions.FunctionId"/>. Set only on events sent to telemetry integrations.</summary>
    public string? FunctionId { get; init; }
}

/// <summary>
/// Event passed to telemetry integrations when speech generation completes. Raw audio is intentionally not included. Integrations
/// receive only its size, media type, and format. Maps to <c>GenerateSpeechEndEvent</c>.
/// </summary>
public sealed class GenerateSpeechEndEvent : IOperationEndEvent
{
    /// <summary>Creates the event.</summary>
    public GenerateSpeechEndEvent(
        string callId,
        string provider,
        string modelId,
        string text,
        GenerateSpeechAudioInfo audio,
        JsonElement? usage,
        IReadOnlyList<OperationWarning> warnings,
        JsonElement? providerMetadata,
        ProviderResponse response
    )
    {
        CallId = callId;
        Provider = provider;
        ModelId = modelId;
        Text = text;
        Audio = audio;
        Usage = usage;
        Warnings = warnings;
        ProviderMetadata = providerMetadata;
        Response = response;
    }

    /// <summary>Unique identifier for this speech generation call.</summary>
    public string CallId { get; }

    /// <summary><c>ai.generateSpeech</c>.</summary>
    public string OperationId => "ai.generateSpeech";

    /// <summary>The provider identifier.</summary>
    public string Provider { get; }

    /// <summary>The specific model identifier.</summary>
    public string ModelId { get; }

    /// <summary>The text that was converted to speech.</summary>
    public string Text { get; }

    /// <summary>Metadata about the generated audio.</summary>
    public GenerateSpeechAudioInfo Audio { get; }

    /// <summary>Usage reported by the provider, when available.</summary>
    public JsonElement? Usage { get; }

    /// <summary>Warnings from the speech model.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }

    /// <summary>Optional provider-specific metadata.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Response metadata from the provider: timestamp, model id and headers.</summary>
    public ProviderResponse Response { get; }

    /// <summary>The version of the SDK that emitted the event. Set only on events sent to telemetry integrations. Maps to <c>aiVersion</c>.</summary>
    public string? AiVersion { get; init; }

    /// <summary>Copied from <see cref="TelemetryOptions.RecordInputs"/>. Set only on events sent to telemetry integrations.</summary>
    public bool? RecordInputs { get; init; }

    /// <summary>Copied from <see cref="TelemetryOptions.RecordOutputs"/>. Set only on events sent to telemetry integrations.</summary>
    public bool? RecordOutputs { get; init; }

    /// <summary>Copied from <see cref="TelemetryOptions.FunctionId"/>. Set only on events sent to telemetry integrations.</summary>
    public string? FunctionId { get; init; }
}

/// <summary>Options for <see cref="GenerateSpeech.GenerateSpeechAsync"/>.</summary>
public sealed class GenerateSpeechRequest : OperationRequest
{
    /// <summary>Speech model.</summary>
    public ISpeechCaller? Model { get; set; }

    /// <summary>Text to speak.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Voice id.</summary>
    public string? Voice { get; set; }

    /// <summary>Output format such as <c>mp3</c> or <c>pcm</c>.</summary>
    public string? OutputFormat { get; set; }

    /// <summary>Speaking instructions.</summary>
    public string? Instructions { get; set; }

    /// <summary>Playback speed.</summary>
    public double? Speed { get; set; }

    /// <summary>Language code or <c>auto</c>.</summary>
    public string? Language { get; set; }

    /// <summary>Telemetry callbacks.</summary>
    public TelemetryOptions? Telemetry { get; set; }
}

/// <summary>Generates speech. Maps to <c>generateSpeech</c>.</summary>
public static class GenerateSpeech
{
    /// <summary>Generates speech audio.</summary>
    public static async Task<GenerateSpeechResult> GenerateSpeechAsync(
        GenerateSpeechRequest request,
        CancellationToken cancellationToken = default
    )
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var model = request.Model
            ?? throw new InvalidArgumentException("model", null, "model is required");
        var token = request.CancellationToken.CanBeCanceled
            ? request.CancellationToken
            : cancellationToken;
        var headers = OperationHeaders.WithUserAgent(request.Headers, AiSdkVersion.UserAgent);
        var providerOptions = request.ProviderOptions ?? OperationJson.Parse("{}");
        var maxRetries = OperationRetry.ResolveMaxRetries(request.MaxRetries);
        var callId = (request.GenerateCallId ?? Embed.DefaultCallId)();
        var telemetry = TelemetryDispatcher.Create(request.Telemetry);
        if (telemetry.OnStart is { } onStart)
        {
            await onStart(
                    new GenerateSpeechStartEvent(
                        callId,
                        model.Provider,
                        model.ModelId,
                        request.Text,
                        request.Voice,
                        request.OutputFormat,
                        request.Instructions,
                        request.Speed,
                        request.Language,
                        maxRetries,
                        request.Headers,
                        providerOptions
                    ),
                    token
                )
                .ConfigureAwait(false);
        }

        try
        {
            var result = await OperationRetry
                .ExecuteAsync(
                    request.MaxRetries,
                    token,
                    request.AbortReason,
                    ct =>
                        model.DoGenerateAsync(
                            new SpeechModelCall(
                                request.Text,
                                request.Voice,
                                request.OutputFormat,
                                request.Instructions,
                                request.Speed,
                                request.Language,
                                providerOptions,
                                headers,
                                ct
                            ),
                            ct
                        ),
                    null
                )
                .ConfigureAwait(false);
            if (result.Audio == null || result.Audio.Length == 0)
            {
                throw new NoSpeechGeneratedException(new[] { result.Response });
            }

            WarningLog.Write(result.Warnings, model.Provider, model.ModelId);
            var mediaType = MediaTypeDetector.Detect(result.Audio, "audio")
                ?? OperationHeaders.MediaTypeFromContentType(result.Response.Headers, "audio/")
                ?? OutputFormatMediaType(request.OutputFormat)
                ?? "audio/mp3";
            var audio = new GeneratedAudio(result.Audio, mediaType);
            if (telemetry.OnEnd is { } onEnd)
            {
                await onEnd(
                        new GenerateSpeechEndEvent(
                            callId,
                            model.Provider,
                            model.ModelId,
                            request.Text,
                            new GenerateSpeechAudioInfo(
                                result.Audio.Length,
                                audio.MediaType,
                                audio.Format
                            ),
                            result.Usage,
                            result.Warnings,
                            result.ProviderMetadata,
                            GetTelemetryResponseMetadata(result.Response)
                        ),
                        token
                    )
                    .ConfigureAwait(false);
            }

            return new GenerateSpeechResult(
                audio,
                result.Warnings,
                new[] { result.Response },
                result.ProviderMetadata ?? OperationJson.Parse("{}")
            );
        }
        catch (Exception error)
        {
            if (telemetry.OnError is { } onError)
            {
                await onError(new TelemetryErrorEvent(callId, error), token).ConfigureAwait(false);
            }

            throw;
        }
    }

    /// <summary>Maps to <c>getTelemetryResponseMetadata</c>. The raw body stays out of telemetry.</summary>
    private static ProviderResponse GetTelemetryResponseMetadata(ProviderResponse response) =>
        new(response.Headers, null, null, response.Timestamp, response.ModelId, null);

    private static string? OutputFormatMediaType(string? outputFormat)
    {
        if (outputFormat == null)
        {
            return null;
        }

        switch (outputFormat.Trim().ToLowerInvariant())
        {
            case "pcm":
            case "audio/pcm":
                return "audio/pcm";
            case "audio/l16":
                return "audio/l16";
            case "mulaw":
            case "audio/mulaw":
                return "audio/mulaw";
            case "alaw":
            case "audio/alaw":
                return "audio/alaw";
            default:
                return null;
        }
    }
}
