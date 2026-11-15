// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Azure;

/// <summary>
/// Azure Speech text to speech with SSML. The model is appended to the voice name, for example
/// <c>en-US-Harper:MAI-Voice-2</c>. Maps to <c>AzureSpeechSpeechModel</c> in <c>azure-speech-speech-model.ts</c>.
/// </summary>
public sealed class AzureSpeechSpeechModel : ISpeechModel
{
    private const string DefaultVoice = "en-US-Harper";
    private const string DefaultOutputFormat = "audio-24khz-160kbitrate-mono-mp3";
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);

    // Default voice per ISO 639-1 language; available on MAI-Voice-2, MAI-Voice-2.1, and their Flash variants.
    private static readonly Dictionary<string, string> DefaultVoices = new(StringComparer.Ordinal)
    {
        ["de"] = "de-DE-Mia",
        ["en"] = DefaultVoice,
        ["es"] = "es-MX-Valeria",
        ["fr"] = "fr-FR-Soleil",
        ["hi"] = "hi-IN-Kavya",
        ["hu"] = "hu-HU-Lilla",
        ["it"] = "it-IT-Rosa",
        ["ko"] = "ko-KR-Haena",
        ["nl"] = "nl-NL-Fleur",
        ["pt"] = "pt-BR-Luana",
        ["ro"] = "ro-RO-Elena",
        ["ru"] = "ru-RU-Masha",
        ["th"] = "th-TH-Krit",
        ["tr"] = "tr-TR-Elif",
        ["zh"] = "zh-CN-Mei",
    };

    // Shorthand formats for `outputFormat`; other X-Microsoft-OutputFormat values
    // (for example `audio-48khz-192kbitrate-mono-mp3`) are passed through unchanged.
    private static readonly Dictionary<string, string> OutputFormats = new(StringComparer.Ordinal)
    {
        ["mp3"] = DefaultOutputFormat,
        ["opus"] = "ogg-24khz-16bit-mono-opus",
        ["pcm"] = "raw-24khz-16bit-mono-pcm",
        ["wav"] = "riff-24khz-16bit-mono-pcm",
    };

    private static readonly Regex NativeOutputFormat = new(
        @"^(?:amr|audio|g722|ogg|raw|riff|webm)-[a-z0-9-]+\z",
        RegexOptions.CultureInvariant,
        RegexTimeout);

    private static readonly Regex VoiceLocale = new("^([a-zA-Z]{2,3}-[a-zA-Z]{2,4})-", RegexOptions.CultureInvariant, RegexTimeout);
    private static readonly Regex VoiceLanguage = new("^([a-zA-Z]{2,3})-[a-zA-Z]{2,4}-", RegexOptions.CultureInvariant, RegexTimeout);

    private readonly ProviderHttp _http;
    private readonly Func<Uri> _url;
    private readonly Func<Dictionary<string, string?>> _headers;
    private readonly Func<DateTimeOffset>? _currentDate;

    /// <summary>
    /// Creates an Azure Speech model. <paramref name="url"/> and <paramref name="headers"/> are read on every call.
    /// <paramref name="currentDate"/> sets the response timestamp. Maps to the config of <c>AzureSpeechSpeechModel</c>.
    /// </summary>
    public AzureSpeechSpeechModel(
        string modelId,
        ProviderHttp http,
        Func<Uri> url,
        Func<Dictionary<string, string?>> headers,
        Func<DateTimeOffset>? currentDate = null)
    {
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _url = url ?? throw new ArgumentNullException(nameof(url));
        _headers = headers ?? throw new ArgumentNullException(nameof(headers));
        _currentDate = currentDate;
    }

    /// <inheritdoc />
    public string Provider => "azure.speech";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<SpeechResult> DoGenerateAsync(SpeechCallOptions options, CancellationToken cancellationToken)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var result = await GenerateAsync(new OpenAISpeechCall(options.Text) { Voice = options.Voice }, new AzureSpeechSpeechModelOptions(), cancellationToken).ConfigureAwait(false);
        return new SpeechResult(result.Audio, result.MediaType);
    }

    /// <summary>Synthesizes speech with SSML. Maps to <c>doGenerate(options, azureOptions)</c>.</summary>
    public async Task<OpenAISpeechResult> GenerateAsync(OpenAISpeechCall call, AzureSpeechSpeechModelOptions? azureOptions, CancellationToken cancellationToken)
    {
        if (call is null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        azureOptions ??= new AzureSpeechSpeechModelOptions();
        var currentDate = _currentDate?.Invoke() ?? DateTimeOffset.UtcNow;
        var warnings = new List<OpenAICallWarning>();
        var style = azureOptions.Style;
        var styleDegree = azureOptions.StyleDegree;

        if (call.Instructions != null)
        {
            warnings.Add(new OpenAICallWarning("unsupported", "instructions", "Use providerOptions.azure.style to control speaking style."));
        }

        var (voice, languageWarning) = ResolveVoice(call.Voice, call.Language);
        if (languageWarning != null)
        {
            warnings.Add(new OpenAICallWarning("unsupported", "language", languageWarning));
        }

        if (styleDegree != null && style == null)
        {
            warnings.Add(new OpenAICallWarning("unsupported", "providerOptions.azure.styleDegree", "styleDegree requires style."));
        }

        var outputFormat = DefaultOutputFormat;
        if (call.OutputFormat != null)
        {
            var format = call.OutputFormat.ToLowerInvariant();
            if (OutputFormats.TryGetValue(format, out var shorthand))
            {
                outputFormat = shorthand;
            }
            else if (NativeOutputFormat.IsMatch(format))
            {
                outputFormat = format;
            }
            else
            {
                warnings.Add(new OpenAICallWarning("unsupported", "outputFormat", "Unsupported output format: " + call.OutputFormat + ". Using mp3 instead."));
            }
        }

        var ssml = BuildSsml(
            call.Text,
            voice.IndexOf(':') >= 0 ? voice : voice + ":" + (AzureSpeechModelOptions.GetMAIVoiceModel(ModelId) ?? ModelId),
            VoiceLocale.Match(voice) is { Success: true } locale ? locale.Groups[1].Value : "en-US",
            call.Speed,
            style,
            style != null ? styleDegree : null);

        var headers = _headers();
        headers["X-Microsoft-OutputFormat"] = outputFormat;
        if (call.Headers != null)
        {
            foreach (var pair in call.Headers)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        var content = new StringContent(ssml, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/ssml+xml");
        ProviderBinaryResponse response;
        try
        {
            response = await _http.SendBinaryAsync(HttpMethod.Post, _url(), content, headers, cancellationToken).ConfigureAwait(false);
        }
        catch (ApiException exception)
        {
            throw FailedResponse(exception);
        }

        return new OpenAISpeechResult(response.Body, response.MediaType ?? "audio/mpeg", warnings, ModelId, currentDate, response.Headers, ssml);
    }

    private static string BuildSsml(string text, string voiceName, string locale, double? speed, string? style, double? styleDegree)
    {
        var content = EscapeXml(text);
        if (speed != null)
        {
            content = "<prosody rate=\"" + Number(speed.Value) + "\">" + content + "</prosody>";
        }

        if (style != null)
        {
            var degree = styleDegree != null ? " styledegree=\"" + Number(styleDegree.Value) + "\"" : string.Empty;
            content = "<mstts:express-as style=\"" + EscapeXml(style) + "\"" + degree + ">" + content + "</mstts:express-as>";
        }

        return "<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" "
            + "xmlns:mstts=\"http://www.w3.org/2001/mstts\" xml:lang=\"" + EscapeXml(locale) + "\">"
            + "<voice name=\"" + EscapeXml(voiceName) + "\">" + content + "</voice></speak>";
    }

    private static string Number(double value)
    {
        return value.ToString("R", CultureInfo.InvariantCulture);
    }

    private static string EscapeXml(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            switch (character)
            {
                case '&':
                    builder.Append("&amp;");
                    break;
                case '<':
                    builder.Append("&lt;");
                    break;
                case '>':
                    builder.Append("&gt;");
                    break;
                case '"':
                    builder.Append("&quot;");
                    break;
                case '\'':
                    builder.Append("&apos;");
                    break;
                default:
                    builder.Append(character);
                    break;
            }
        }

        return builder.ToString();
    }

    // Uses the language's default voice when no voice is set. An explicit voice wins, and its locale selects the language.
    private static (string Voice, string? LanguageWarning) ResolveVoice(string? voice, string? language)
    {
        var code = string.IsNullOrEmpty(language) ? null : language!.Split('-')[0].ToLowerInvariant();
        if (voice == null)
        {
            if (code == null)
            {
                return (DefaultVoice, null);
            }

            if (code == "auto")
            {
                return (DefaultVoice, "Automatic language detection is not supported. " + DefaultVoice + " was used.");
            }

            return DefaultVoices.TryGetValue(code, out var defaultVoice)
                ? (defaultVoice, null)
                : (DefaultVoice, "No default MAI voice for language \"" + language + "\". " + DefaultVoice + " was used.");
        }

        var voiceLanguage = VoiceLanguage.Match(voice) is { Success: true } match ? match.Groups[1].Value.ToLowerInvariant() : null;
        return code != null && code != "auto" && voiceLanguage != null && code != voiceLanguage
            ? (voice, "The voice " + voice + " selects the language. Language \"" + language + "\" was ignored.")
            : (voice, null);
    }

    // Azure Speech answers invalid requests (unknown voice, unsupported style, or output format) with an empty 400 body.
    private static ApiException FailedResponse(ApiException exception)
    {
        var message = ParseErrorMessage(exception.ResponseBody)
            ?? (exception.StatusCode == 400
                ? "Azure Speech request failed with status 400. Check the voice name, style, and output format."
                : "Azure Speech request failed with status " + exception.StatusCode.ToString(CultureInfo.InvariantCulture) + ".");
        return ProviderHttp.MapStatusWithHeaders(exception.StatusCode, message, exception.ResponseBody, exception.ResponseHeaders);
    }

    private static string? ParseErrorMessage(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            return document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty("error", out var error)
                && error.ValueKind == JsonValueKind.Object
                && error.TryGetProperty("message", out var message)
                && message.ValueKind == JsonValueKind.String
                    ? message.GetString()
                    : null;
        }
        catch (JsonException)
        {
            // Not JSON: the status message applies.
            return null;
        }
    }
}
