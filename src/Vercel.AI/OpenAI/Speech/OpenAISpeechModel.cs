// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAI;

/// <summary>Speech synthesis settings.</summary>
public sealed class OpenAISpeechCall
{
    /// <summary>Creates a speech call.</summary>
    public OpenAISpeechCall(string text)
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
    }

    /// <summary>Text to speak.</summary>
    public string Text { get; }

    /// <summary>Voice. Defaults to <c>alloy</c>.</summary>
    public string? Voice { get; set; }

    /// <summary>Playback speed.</summary>
    public double? Speed { get; set; }

    /// <summary>Speaking instructions.</summary>
    public string? Instructions { get; set; }

    /// <summary>Audio format. Unsupported values fall back to <c>mp3</c>.</summary>
    public string? OutputFormat { get; set; }

    /// <summary>Language. OpenAI speech models ignore it.</summary>
    public string? Language { get; set; }

    /// <summary>Provider options. <c>openai.speed</c> and <c>openai.instructions</c> override the call.</summary>
    public JsonElement? ProviderOptions { get; set; }

    /// <summary>Extra HTTP headers.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; set; }

    internal OpenAISpeechCall WithProviderOptions(JsonElement? providerOptions)
    {
        return new OpenAISpeechCall(Text)
        {
            Voice = Voice,
            Speed = Speed,
            Instructions = Instructions,
            OutputFormat = OutputFormat,
            Language = Language,
            ProviderOptions = providerOptions,
            Headers = Headers,
        };
    }
}

/// <summary>Speech audio plus the response metadata recorded for the call.</summary>
public sealed class OpenAISpeechResult
{
    internal OpenAISpeechResult(byte[] audio, string mediaType, IReadOnlyList<OpenAICallWarning> warnings, string modelId, DateTimeOffset timestamp, IReadOnlyDictionary<string, string> headers, string? requestBody = null)
    {
        RequestBody = requestBody;
        Audio = audio;
        MediaType = mediaType;
        Warnings = warnings;
        ModelId = modelId;
        Timestamp = timestamp;
        Headers = headers;
    }

    /// <summary>Audio bytes.</summary>
    public byte[] Audio { get; }

    /// <summary>IANA media type derived from the request format.</summary>
    public string MediaType { get; }

    /// <summary>Warnings produced while preparing the request.</summary>
    public IReadOnlyList<OpenAICallWarning> Warnings { get; }

    /// <summary>Model id used for the call.</summary>
    public string ModelId { get; }

    /// <summary>Timestamp recorded when the response was read.</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>HTTP response headers.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Body sent to the provider. Maps to <c>request.body</c>.</summary>
    public string? RequestBody { get; }

    internal OpenAISpeechResult WithWarnings(IReadOnlyList<OpenAICallWarning> warnings)
    {
        return new OpenAISpeechResult(Audio, MediaType, warnings, ModelId, Timestamp, Headers, RequestBody);
    }
}

/// <summary>Speech request body and warnings.</summary>
public sealed class OpenAISpeechPreparation
{
    internal OpenAISpeechPreparation(JsonObject body, IReadOnlyList<OpenAICallWarning> warnings)
    {
        Body = body;
        Warnings = warnings;
    }

    /// <summary>JSON body sent to <c>/audio/speech</c>.</summary>
    public JsonObject Body { get; }

    /// <summary>Warnings produced while preparing the request.</summary>
    public IReadOnlyList<OpenAICallWarning> Warnings { get; }
}

/// <summary>OpenAI speech model.</summary>
public sealed class OpenAISpeechModel : ISpeechModel
{
    private static readonly string[] Formats = { "mp3", "opus", "aac", "flac", "wav", "pcm" };

    private readonly ProviderHttp _http;
    private readonly string _providerName;
    private readonly Func<Uri> _url;
    private readonly Func<IReadOnlyDictionary<string, string?>?, Dictionary<string, string?>> _headers;
    private readonly Func<DateTimeOffset>? _clock;

    /// <summary>Creates a speech model.</summary>
    public OpenAISpeechModel(OpenAIProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
    {
        if (provider == null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _clock = clock;
        _http = provider.Http;
        _providerName = provider.Name;
        _url = () => ApiKeys.Combine(provider.Options.BaseUrl, "audio/speech");
        _headers = provider.CreateOpenAIHeaders;
    }

    /// <summary>
    /// Creates an Azure OpenAI speech model for a deployment. The provider id is <c>azure.speech</c> and requests go to
    /// <c>/openai/v1/audio/speech</c> (or the deployments route) with the Azure headers.
    /// </summary>
    public OpenAISpeechModel(Azure.AzureOpenAIProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
    {
        if (provider == null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _clock = clock;
        _http = provider.Http;
        _providerName = provider.Name;
        _url = () => provider.AzureUri("audio/speech", modelId);
        _headers = provider.CreateHeaders;
    }

    /// <inheritdoc />
    public string Provider => _providerName + ".speech";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Builds the speech request body.</summary>
    public static OpenAISpeechPreparation Prepare(string modelId, OpenAISpeechCall call)
    {
        var warnings = new List<OpenAICallWarning>();
        var openai = OpenAIJson.OpenAIObject(call.ProviderOptions);
        var format = "mp3";
        if (!string.IsNullOrEmpty(call.OutputFormat))
        {
            if (IsFormat(call.OutputFormat!))
            {
                format = call.OutputFormat!;
            }
            else
            {
                warnings.Add(new OpenAICallWarning(
                    "unsupported",
                    "outputFormat",
                    "Unsupported output format: " + call.OutputFormat + ". Using mp3 instead."));
            }
        }

        var body = new JsonObject
        {
            ["model"] = modelId,
            ["input"] = call.Text,
            ["voice"] = string.IsNullOrEmpty(call.Voice) ? "alloy" : call.Voice,
            ["response_format"] = format,
        };
        if (call.Speed is { } speed)
        {
            body["speed"] = speed;
        }

        if (call.Instructions != null)
        {
            body["instructions"] = call.Instructions;
        }

        if (OpenAIJson.Double(openai, "speed") is { } providerSpeed)
        {
            body["speed"] = providerSpeed;
        }

        var providerInstructions = OpenAIJson.String(openai, "instructions");
        if (providerInstructions != null)
        {
            body["instructions"] = providerInstructions;
        }

        if (!string.IsNullOrEmpty(call.Language))
        {
            warnings.Add(new OpenAICallWarning(
                "unsupported",
                "language",
                "OpenAI speech models do not support language selection. Language parameter \"" + call.Language + "\" was ignored."));
        }

        return new OpenAISpeechPreparation(body, warnings);
    }

    /// <inheritdoc />
    public async Task<SpeechResult> DoGenerateAsync(SpeechCallOptions options, CancellationToken cancellationToken)
    {
        var result = await GenerateAsync(new OpenAISpeechCall(options.Text) { Voice = options.Voice }, cancellationToken).ConfigureAwait(false);
        return new SpeechResult(result.Audio, result.MediaType);
    }

    /// <summary>Synthesizes speech and returns the audio bytes, warnings, and response headers.</summary>
    public async Task<OpenAISpeechResult> GenerateAsync(OpenAISpeechCall call, CancellationToken cancellationToken)
    {
        var prepared = Prepare(ModelId, call);
        var requestBody = prepared.Body.ToJsonString();
        var response = await _http.SendBinaryAsync(
            HttpMethod.Post,
            _url(),
            new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json"),
            _headers(call.Headers),
            cancellationToken).ConfigureAwait(false);
        var bytes = response.Body;

        var format = prepared.Body["response_format"]?.GetValue<string>() ?? "mp3";
        return new OpenAISpeechResult(bytes, MediaType(format), prepared.Warnings, ModelId, OpenAIClock.Now(_clock), response.Headers, requestBody);
    }

    /// <summary>Clock used when a caller records a response timestamp.</summary>
    public DateTimeOffset Now()
    {
        return OpenAIClock.Now(_clock);
    }

    private static bool IsFormat(string format)
    {
        foreach (var candidate in Formats)
        {
            if (candidate == format)
            {
                return true;
            }
        }

        return false;
    }

    private static string MediaType(string format)
    {
        switch (format)
        {
            case "wav":
                return "audio/wav";
            case "opus":
                return "audio/opus";
            case "aac":
                return "audio/aac";
            case "flac":
                return "audio/flac";
            case "pcm":
                return "audio/pcm";
            default:
                return "audio/mpeg";
        }
    }
}
