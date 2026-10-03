// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Replicate;

/// <summary>Replicate provider.</summary>
public sealed class ReplicateProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "replicate";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.replicate.com/v1";

    private readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public ReplicateProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        throw new AiSdkException("Replicate does not provide language models.");
    }

    /// <summary>Creates a provider.</summary>
    public static new ReplicateProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new ReplicateProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "REPLICATE_API_TOKEN";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.Bearer;
        options.UserAgent = ProviderExchange.UserAgent("replicate");
        return options;
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId) => new Image(this, modelId);

    /// <summary>Posts a prediction. Slashes in <paramref name="modelId"/> stay in the path.</summary>
    public Task<ImageGenerationResult> GenerateImageAsync(string modelId, ReplicateImageRequest request, CancellationToken cancellationToken)
    {
        return new Image(this, modelId).GenerateAsync(request, cancellationToken);
    }

    private sealed class Image : IImageModel
    {
        private readonly ReplicateProvider _provider;
        public Image(ReplicateProvider provider, string modelId) { _provider = provider; ModelId = modelId; }
        public string Provider => "replicate.image";
        public string ModelId { get; }
        public Task<ImageGenerationResult> DoGenerateAsync(ImageCallOptions options, CancellationToken cancellationToken)
        {
            return GenerateAsync(new ReplicateImageRequest(options.Prompt) { Count = options.Count, Size = options.Size, AspectRatio = options.AspectRatio }, cancellationToken);
        }

        public async Task<ImageGenerationResult> GenerateAsync(ReplicateImageRequest request, CancellationToken cancellationToken)
        {
            var input = new JsonObject { ["prompt"] = request.Prompt, ["num_outputs"] = request.Count };
            if (request.AspectRatio != null)
            {
                input["aspect_ratio"] = request.AspectRatio;
            }

            if (request.Size != null)
            {
                input["size"] = request.Size;
            }

            if (request.Seed is { } seed)
            {
                input["seed"] = seed;
            }

            if (request.ExtraInput != null)
            {
                foreach (var pair in request.ExtraInput)
                {
                    input[pair.Key] = pair.Value == null ? null : JsonNode.Parse(pair.Value.ToJsonString());
                }
            }

            var headers = ProviderExchange.Merge(_provider.CreateHeaders(), request.Headers);
            headers["Prefer"] = string.IsNullOrEmpty(request.Prefer) ? "wait" : request.Prefer;
            var response = await ProviderExchange.SendAsync(
                _provider._httpClient,
                HttpMethod.Post,
                ApiKeys.Combine(_provider.Options.BaseUrl, "/models/" + ModelId + "/predictions"),
                ProviderExchange.Json(new JsonObject { ["input"] = input }.ToJsonString()),
                headers,
                cancellationToken).ConfigureAwait(false);
            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body);
            var url = document.RootElement.TryGetProperty("url", out var value) ? value.GetString() : null;
            return new ImageGenerationResult(new[] { new GeneratedImage("image/png", null, url) });
        }
    }
}

/// <summary>Replicate image prediction.</summary>
public sealed class ReplicateImageRequest
{
    /// <summary>Creates a prediction for <paramref name="prompt"/>.</summary>
    public ReplicateImageRequest(string prompt)
    {
        Prompt = prompt ?? string.Empty;
    }

    /// <summary>Prompt sent as <c>input.prompt</c>.</summary>
    public string Prompt { get; }

    /// <summary><c>input.num_outputs</c>.</summary>
    public int Count { get; set; } = 1;

    /// <summary><c>input.aspect_ratio</c>, when set.</summary>
    public string? AspectRatio { get; set; }

    /// <summary><c>input.size</c>, when set.</summary>
    public string? Size { get; set; }

    /// <summary><c>input.seed</c>, when set.</summary>
    public int? Seed { get; set; }

    /// <summary>Additional <c>input</c> fields, such as <c>style</c>.</summary>
    public JsonObject? ExtraInput { get; set; }

    /// <summary><c>Prefer</c> header. Defaults to <c>wait</c>.</summary>
    public string Prefer { get; set; } = "wait";

    /// <summary>Headers merged over the provider headers.</summary>
    public IDictionary<string, string>? Headers { get; set; }

}

/// <summary>Registers Replicate.</summary>
public static class ReplicateServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddReplicate(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(ReplicateProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new ReplicateProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(ReplicateProvider.ProviderId), options);
        });
        return services;
    }
}
