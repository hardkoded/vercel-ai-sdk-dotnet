// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.Operations;

/// <summary>Arguments for <c>doGenerate</c> on a speech model.</summary>
public sealed class SpeechModelCall
{
    /// <summary>Creates a speech call.</summary>
    public SpeechModelCall(string text, string? voice, string? outputFormat, string? instructions, double? speed, string? language, JsonElement providerOptions, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken)
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
    public SpeechModelResult(byte[]? audio, IReadOnlyList<OperationWarning>? warnings = null, JsonElement? providerMetadata = null, ProviderResponse? response = null)
    {
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
}

/// <summary>Speech model used by <see cref="GenerateSpeech"/>.</summary>
public interface ISpeechCaller
{
    /// <summary>Provider id.</summary>
    string Provider { get; }

    /// <summary>Model id.</summary>
    string ModelId { get; }

    /// <summary>Generates speech audio.</summary>
    Task<SpeechModelResult> DoGenerateAsync(SpeechModelCall call, CancellationToken cancellationToken);
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
                throw new InvalidResponseDataException(MediaType, "Could not determine audio format from media type: " + MediaType);
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
    public GenerateSpeechResult(GeneratedAudio audio, IReadOnlyList<OperationWarning> warnings, IReadOnlyList<ProviderResponse> responses, JsonElement providerMetadata)
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
}

/// <summary>Generates speech. Maps to <c>generateSpeech</c>.</summary>
public static class GenerateSpeech
{
    /// <summary>Generates speech audio.</summary>
    public static async Task<GenerateSpeechResult> GenerateSpeechAsync(GenerateSpeechRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var model = request.Model ?? throw new InvalidArgumentException("model", null, "model is required");
        var token = request.CancellationToken.CanBeCanceled ? request.CancellationToken : cancellationToken;
        var headers = OperationHeaders.WithUserAgent(request.Headers, AiSdkVersion.UserAgent);
        var providerOptions = request.ProviderOptions ?? OperationJson.Parse("{}");
        var result = await OperationRetry.ExecuteAsync(request.MaxRetries, token, request.AbortReason, ct => model.DoGenerateAsync(new SpeechModelCall(request.Text, request.Voice, request.OutputFormat, request.Instructions, request.Speed, request.Language, providerOptions, headers, ct), ct), null).ConfigureAwait(false);
        if (result.Audio == null || result.Audio.Length == 0)
        {
            throw new NoSpeechGeneratedException(new[] { result.Response });
        }

        WarningLog.Write(result.Warnings, model.Provider, model.ModelId);
        var mediaType = MediaTypeDetector.Detect(result.Audio, "audio")
            ?? OperationHeaders.MediaTypeFromContentType(result.Response.Headers, "audio/")
            ?? OutputFormatMediaType(request.OutputFormat)
            ?? "audio/mp3";
        return new GenerateSpeechResult(new GeneratedAudio(result.Audio, mediaType), result.Warnings, new[] { result.Response }, result.ProviderMetadata ?? OperationJson.Parse("{}"));
    }

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
