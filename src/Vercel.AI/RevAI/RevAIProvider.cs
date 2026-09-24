// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.RevAI;

/// <summary>RevAI provider.</summary>
public sealed class RevAIProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "revai";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.rev.ai";

    /// <summary>Creates a provider.</summary>
    public RevAIProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
    }

    /// <summary>Creates a provider.</summary>
    public static new RevAIProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new RevAIProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "REVAI_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.Bearer;
        return options;
    }

    /// <inheritdoc />
    public override ITranscriptionModel TranscriptionModel(string modelId) => new Transcription(this, modelId);

    private sealed class Transcription : ITranscriptionModel
    {
        private readonly RevAIProvider _provider;
        public Transcription(RevAIProvider provider, string modelId) { _provider = provider; ModelId = modelId; }
        public string Provider => "revai";
        public string ModelId { get; }
        public async Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
        {
            var body = new JsonObject { ["model"] = ModelId, ["audio"] = Convert.ToBase64String(audio.Data) };
            using var document = await _provider.Http.SendJsonAsync(HttpMethod.Post, ApiKeys.Combine(_provider.Options.BaseUrl, "/speechtotext/v1/jobs"), body.ToJsonString(), _provider.CreateHeaders(), cancellationToken).ConfigureAwait(false);
            var text = document.RootElement.TryGetProperty("text", out var value) ? value.GetString() ?? string.Empty : string.Empty;
            return new TranscriptionResult(text);
        }
    }

}

/// <summary>Registers RevAI.</summary>
public static class RevAIServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddRevAI(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(RevAIProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new RevAIProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(RevAIProvider.ProviderId), options);
        });
        return services;
    }
}
