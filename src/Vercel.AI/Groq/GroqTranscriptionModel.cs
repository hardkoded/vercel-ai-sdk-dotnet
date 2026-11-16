// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Groq;

/// <summary>Groq speech-to-text model. Maps to <c>GroqTranscriptionModel</c>.</summary>
public sealed class GroqTranscriptionModel : ITranscriptionModel, ITranscriptionCaller, ITranscriptionUrlCaller
{
    private static readonly Regex RedirectFailure = new Regex(
        @"^failed to retrieve media: received status code: (301|302|303|307|308)\b",
        RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1));

    private readonly GroqProvider _provider;
    private readonly Func<DateTimeOffset>? _clock;

    /// <summary>Creates a transcription model. <paramref name="clock"/> sets the response timestamp and defaults to UTC now.</summary>
    public GroqTranscriptionModel(GroqProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _clock = clock;
    }

    /// <inheritdoc />
    public string Provider => "groq.transcription";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public bool CanStream => false;

    /// <inheritdoc />
    public async Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
    {
        audio = audio ?? throw new ArgumentNullException(nameof(audio));
        var result = await DoGenerateAsync(new TranscriptionModelCall(audio.Data, string.IsNullOrEmpty(audio.MediaType) ? "audio/wav" : audio.MediaType, OperationJson.Parse("{}"), new Dictionary<string, string>(), cancellationToken), cancellationToken).ConfigureAwait(false);
        return new TranscriptionResult(result.Text ?? string.Empty);
    }

    /// <inheritdoc />
    public Task<TranscriptionModelResult> DoGenerateAsync(TranscriptionModelCall call, CancellationToken cancellationToken)
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        return GenerateAsync(call.Audio, call.MediaType, null, call.ProviderOptions, call.Headers, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TranscriptionModelResult?> DoGenerateFromUrlAsync(TranscriptionModelUrlCall call, CancellationToken cancellationToken)
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        try
        {
            return await GenerateAsync(null, null, call.Audio, call.ProviderOptions, call.Headers, cancellationToken).ConfigureAwait(false);
        }
        catch (ApiException error) when (error.StatusCode == 400 && RedirectFailure.IsMatch(error.Message))
        {
            // Groq does not follow redirects for its URL parameter. Let core use
            // the existing secure local downloader for that specific limitation.
            return null;
        }
    }

    /// <inheritdoc />
    public Task<TranscriptionStreamStart?> DoStreamAsync(TranscriptionStreamCall call, CancellationToken cancellationToken)
    {
        return Task.FromResult<TranscriptionStreamStart?>(null);
    }

    private static JsonElement? GroqObject(JsonElement providerOptions)
    {
        if (providerOptions.ValueKind == JsonValueKind.Object && providerOptions.TryGetProperty("groq", out var groq) && groq.ValueKind == JsonValueKind.Object)
        {
            return groq;
        }

        return null;
    }

    private static void AddOptionalString(MultipartFormDataContent form, JsonElement groq, string name, string field)
    {
        if (groq.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null)
        {
            if (value.ValueKind != JsonValueKind.String)
            {
                throw new InvalidArgumentException("providerOptions", value.GetRawText(), "invalid groq provider options: " + name + " must be a string");
            }

            form.Add(new StringContent(value.GetString()!), field);
        }
    }

    private async Task<TranscriptionModelResult> GenerateAsync(byte[]? audio, string? mediaType, Uri? audioUrl, JsonElement providerOptions, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken)
    {
        var timestamp = _clock?.Invoke() ?? DateTimeOffset.UtcNow;
        var warnings = new List<OperationWarning>();
        string? responseFormat = null;
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(ModelId), "model");
        if (audioUrl != null)
        {
            if ((audioUrl.Scheme != Uri.UriSchemeHttps && audioUrl.Scheme != Uri.UriSchemeHttp) || audioUrl.UserInfo.Length != 0)
            {
                throw new InvalidArgumentException("audio", null, "Groq URL transcription requires an HTTP(S) URL without userinfo.");
            }

            // Fetch does not send fragments to the audio host. Preserve that behavior
            // without changing the caller's URL object.
            form.Add(new StringContent(new UriBuilder(audioUrl) { Fragment = string.Empty }.Uri.AbsoluteUri), "url");
        }
        else
        {
            var audioMediaType = string.IsNullOrEmpty(mediaType) ? "audio/wav" : mediaType!;
            var file = new ByteArrayContent(audio ?? Array.Empty<byte>());
            file.Headers.ContentType = MediaTypeHeaderValue.Parse(audioMediaType);
            form.Add(file, "file", "audio." + ProviderValues.MediaTypeToExtension(audioMediaType));
        }

        if (GroqObject(providerOptions) is { } groq)
        {
            AddOptionalString(form, groq, "language", "language");
            AddOptionalString(form, groq, "prompt", "prompt");
            AddOptionalString(form, groq, "responseFormat", "response_format");
            if (groq.TryGetProperty("responseFormat", out var format) && format.ValueKind == JsonValueKind.String)
            {
                responseFormat = format.GetString();
            }

            if (groq.TryGetProperty("temperature", out var temperature) && temperature.ValueKind != JsonValueKind.Null)
            {
                if (temperature.ValueKind != JsonValueKind.Number || temperature.GetDouble() < 0 || temperature.GetDouble() > 1)
                {
                    throw new InvalidArgumentException("providerOptions", temperature.GetRawText(), "invalid groq provider options: temperature must be a number between 0 and 1");
                }

                form.Add(new StringContent(temperature.GetDouble().ToString(CultureInfo.InvariantCulture)), "temperature");
            }

            if (groq.TryGetProperty("timestampGranularities", out var granularities) && granularities.ValueKind != JsonValueKind.Null)
            {
                if (granularities.ValueKind != JsonValueKind.Array)
                {
                    throw new InvalidArgumentException("providerOptions", granularities.GetRawText(), "invalid groq provider options: timestampGranularities must be an array of strings");
                }

                foreach (var item in granularities.EnumerateArray())
                {
                    form.Add(new StringContent(item.ValueKind == JsonValueKind.String ? item.GetString()! : item.GetRawText()), "timestamp_granularities[]");
                }
            }
        }

        var requestHeaders = _provider.CreateHeaders(headers.ToDictionary(pair => pair.Key, pair => (string?)pair.Value));
        var response = await _provider.Http.SendBinaryAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "audio/transcriptions"),
            form,
            requestHeaders,
            cancellationToken).ConfigureAwait(false);
        var body = Encoding.UTF8.GetString(response.Body);
        string text;
        JsonElement rawBody;
        var segments = new List<TranscriptSegment>();
        string? language = null;
        double? duration = null;
        if (responseFormat == "text")
        {
            text = body;
            rawBody = JsonSerializer.SerializeToElement(body);
        }
        else
        {
            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(body);
            }
            catch (JsonException)
            {
                throw new InvalidResponseDataException(body, "Invalid Groq transcription response: the body is not valid JSON.");
            }

            using var parsed = document;
            var root = parsed.RootElement;
            rawBody = root.Clone();
            if (root.ValueKind != JsonValueKind.Object
                || !root.TryGetProperty("text", out var textElement) || textElement.ValueKind != JsonValueKind.String
                || !root.TryGetProperty("x_groq", out var xGroq) || xGroq.ValueKind != JsonValueKind.Object
                || !xGroq.TryGetProperty("id", out var xId) || xId.ValueKind != JsonValueKind.String)
            {
                throw new InvalidResponseDataException(body, "Invalid Groq transcription response: expected text and x_groq.id.");
            }

            text = textElement.GetString()!;
            try
            {
                if (root.TryGetProperty("segments", out var segmentItems) && segmentItems.ValueKind == JsonValueKind.Array)
                {
                    foreach (var segment in segmentItems.EnumerateArray())
                    {
                        segments.Add(new TranscriptSegment(segment.GetProperty("text").GetString() ?? string.Empty, segment.GetProperty("start").GetDouble(), segment.GetProperty("end").GetDouble()));
                    }
                }
                else if (root.TryGetProperty("words", out var wordItems) && wordItems.ValueKind == JsonValueKind.Array)
                {
                    foreach (var word in wordItems.EnumerateArray())
                    {
                        segments.Add(new TranscriptSegment(word.GetProperty("word").GetString() ?? string.Empty, word.GetProperty("start").GetDouble(), word.GetProperty("end").GetDouble()));
                    }
                }
            }
            catch (Exception error) when (error is KeyNotFoundException || error is InvalidOperationException || error is FormatException)
            {
                throw new InvalidResponseDataException(body, "Invalid Groq transcription response: a segment or word is malformed.");
            }

            language = root.TryGetProperty("language", out var languageElement) && languageElement.ValueKind == JsonValueKind.String ? languageElement.GetString() : null;
            duration = root.TryGetProperty("duration", out var durationElement) && durationElement.ValueKind == JsonValueKind.Number ? durationElement.GetDouble() : null;
        }

        return new TranscriptionModelResult(
            text,
            segments,
            language,
            duration,
            warnings,
            null,
            new ProviderResponse(response.Headers, rawBody, timestamp: timestamp.UtcDateTime, modelId: ModelId));
    }
}
