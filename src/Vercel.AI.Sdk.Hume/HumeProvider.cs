// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Sdk.OpenAICompatible;
using Vercel.AI.Sdk.Provider;
using Vercel.AI.Sdk.ProviderUtils;

namespace Vercel.AI.Sdk.Hume;

/// <summary>Hume provider.</summary>
public sealed class HumeProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "hume";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.hume.ai";

    /// <summary>Creates a provider.</summary>
    public HumeProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
    }

    /// <summary>Creates a provider.</summary>
    public static new HumeProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new HumeProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "HUME_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.CustomHeader;
        options.ApiKeyHeaderName = "X-Hume-Api-Key";
        return options;
    }

    /// <inheritdoc />
    public override ISpeechModel SpeechModel(string modelId) => new Speech(this, modelId);

    private sealed class Speech : ISpeechModel
    {
        private readonly HumeProvider _provider;
        public Speech(HumeProvider provider, string modelId) { _provider = provider; ModelId = modelId; }
        public string Provider => "hume";
        public string ModelId { get; }
        public async Task<SpeechResult> DoGenerateAsync(SpeechCallOptions options, CancellationToken cancellationToken)
        {
            var voice = options.Voice ?? "default";
            var path = "/v0/tts".Replace("{voice}", voice);
            var body = new JsonObject { ["text"] = options.Text, ["model_id"] = ModelId, ["model"] = ModelId };
            var bytes = await _provider.Http.SendBytesAsync(HttpMethod.Post, ApiKeys.Combine(_provider.Options.BaseUrl, path), new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json"), _provider.CreateHeaders(), cancellationToken).ConfigureAwait(false);
            return new SpeechResult(bytes, "audio/mpeg");
        }
    }

}

/// <summary>Registers Hume.</summary>
public static class HumeServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddHume(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(HumeProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new HumeProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HumeProvider.ProviderId), options);
        });
        return services;
    }
}
