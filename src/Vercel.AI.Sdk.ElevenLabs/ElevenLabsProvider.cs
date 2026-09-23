// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Sdk.OpenAICompatible;
using Vercel.AI.Sdk.Provider;
using Vercel.AI.Sdk.ProviderUtils;

namespace Vercel.AI.Sdk.ElevenLabs;

/// <summary>ElevenLabs provider.</summary>
public sealed class ElevenLabsProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "elevenlabs";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.elevenlabs.io";

    /// <summary>Creates a provider.</summary>
    public ElevenLabsProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
    }

    /// <summary>Creates a provider.</summary>
    public static new ElevenLabsProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new ElevenLabsProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "ELEVENLABS_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.CustomHeader;
        options.ApiKeyHeaderName = "xi-api-key";
        return options;
    }

    /// <inheritdoc />
    public override ISpeechModel SpeechModel(string modelId) => new Speech(this, modelId);

    private sealed class Speech : ISpeechModel
    {
        private readonly ElevenLabsProvider _provider;
        public Speech(ElevenLabsProvider provider, string modelId) { _provider = provider; ModelId = modelId; }
        public string Provider => "elevenlabs";
        public string ModelId { get; }
        public async Task<SpeechResult> DoGenerateAsync(SpeechCallOptions options, CancellationToken cancellationToken)
        {
            var voice = options.Voice ?? "default";
            var path = "/v1/text-to-speech/{voice}".Replace("{voice}", voice);
            var body = new JsonObject { ["text"] = options.Text, ["model_id"] = ModelId, ["model"] = ModelId };
            var bytes = await _provider.Http.SendBytesAsync(HttpMethod.Post, ApiKeys.Combine(_provider.Options.BaseUrl, path), new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json"), _provider.CreateHeaders(), cancellationToken).ConfigureAwait(false);
            return new SpeechResult(bytes, "audio/mpeg");
        }
    }

}

/// <summary>Registers ElevenLabs.</summary>
public static class ElevenLabsServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddElevenLabs(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(ElevenLabsProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new ElevenLabsProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(ElevenLabsProvider.ProviderId), options);
        });
        return services;
    }
}
