// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.AssemblyAI;

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
    public override ITranscriptionModel TranscriptionModel(string modelId) => new Transcription(this, modelId);

    /// <summary>Uploads audio, then creates a transcript.</summary>
    public Task<TranscriptionResult> TranscribeAsync(string modelId, AudioInput audio, IDictionary<string, string>? headers, CancellationToken cancellationToken)
    {
        return new Transcription(this, modelId).TranscribeAsync(audio, headers, cancellationToken);
    }

    private sealed class Transcription : ITranscriptionModel
    {
        private readonly AssemblyAIProvider _provider;
        public Transcription(AssemblyAIProvider provider, string modelId) { _provider = provider; ModelId = modelId; }
        public string Provider => "assemblyai.transcription";
        public string ModelId { get; }
        public Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
        {
            return TranscribeAsync(audio, null, cancellationToken);
        }

        public async Task<TranscriptionResult> TranscribeAsync(AudioInput audio, IDictionary<string, string>? headers, CancellationToken cancellationToken)
        {
            var merged = ProviderExchange.Merge(_provider.CreateHeaders(), headers);
            var uploadContent = new ByteArrayContent(audio.Data);
            uploadContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            var uploaded = await ProviderExchange.SendAsync(_provider._httpClient, HttpMethod.Post, ApiKeys.Combine(_provider.Options.BaseUrl, "/v2/upload"), uploadContent, merged, cancellationToken).ConfigureAwait(false);
            string? audioUrl;
            using (var uploadDocument = JsonDocument.Parse(string.IsNullOrWhiteSpace(uploaded.Body) ? "{}" : uploaded.Body))
            {
                audioUrl = ReadString(uploadDocument.RootElement, "upload_url") ?? ReadString(uploadDocument.RootElement, "url");
            }

            var body = new JsonObject();
            if (!string.IsNullOrEmpty(audioUrl))
            {
                body["audio_url"] = audioUrl;
            }

            if (!string.IsNullOrEmpty(ModelId))
            {
                body["speech_model"] = ModelId;
            }

            var transcript = await ProviderExchange.SendAsync(_provider._httpClient, HttpMethod.Post, ApiKeys.Combine(_provider.Options.BaseUrl, "/v2/transcript"), ProviderExchange.Json(body.ToJsonString()), merged, cancellationToken).ConfigureAwait(false);
            using (var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(transcript.Body) ? "{}" : transcript.Body))
            {
                var text = ReadString(document.RootElement, "text") ?? string.Empty;
                return new TranscriptionResult(text);
            }
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
