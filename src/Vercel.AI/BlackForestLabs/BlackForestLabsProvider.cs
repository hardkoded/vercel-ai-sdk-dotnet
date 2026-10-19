// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.BlackForestLabs;

/// <summary>BlackForestLabs provider.</summary>
public sealed class BlackForestLabsProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "black-forest-labs";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.bfl.ai/v1";

    /// <summary>Creates a provider.</summary>
    public BlackForestLabsProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        throw new AiSdkException("Black Forest Labs does not provide language models.");
    }

    /// <summary>Creates a provider.</summary>
    public static new BlackForestLabsProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new BlackForestLabsProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options = options?.Copy() ?? new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "BFL_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.CustomHeader;
        options.ApiKeyHeaderName = "x-key";
        return options;
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId) => new Image(this, modelId);

    private sealed class Image : IImageModel
    {
        private readonly BlackForestLabsProvider _provider;
        public Image(BlackForestLabsProvider provider, string modelId) { _provider = provider; ModelId = modelId; }
        public string Provider => "black-forest-labs";
        public string ModelId { get; }
        private static readonly (string Option, string Field)[] ProviderOptionFields =
        {
            ("width", "width"),
            ("height", "height"),
            ("steps", "steps"),
            ("guidance", "guidance"),
            ("imagePromptStrength", "image_prompt_strength"),
            ("imagePrompt", "image_prompt"),
            ("outputFormat", "output_format"),
            ("promptUpsampling", "prompt_upsampling"),
            ("raw", "raw"),
            ("safetyTolerance", "safety_tolerance"),
            ("webhookSecret", "webhook_secret"),
            ("webhookUrl", "webhook_url"),
        };

        private static string? ImageValue(Operations.ImageModelFile file)
        {
            return file.Type == "url" ? file.Url : Convert.ToBase64String(file.Data ?? Array.Empty<byte>());
        }

        public async Task<ImageGenerationResult> DoGenerateAsync(ImageCallOptions options, CancellationToken cancellationToken)
        {
            var path = "/" + ModelId;
            var body = new JsonObject { ["prompt"] = options.Prompt };
            if (options.Seed != null)
            {
                body["seed"] = options.Seed;
            }

            if (!string.IsNullOrEmpty(options.AspectRatio))
            {
                body["aspect_ratio"] = options.AspectRatio;
            }

            var inputImageField = ModelId == "flux-pro-1.0-fill" ? "image" : "input_image";
            for (var index = 0; options.Files != null && index < options.Files.Count; index++)
            {
                body[inputImageField + (index == 0 ? string.Empty : "_" + (index + 1))] = ImageValue(options.Files[index]);
            }

            if (options.Mask != null)
            {
                body["mask"] = ImageValue(options.Mask);
            }

            if (options.ProviderOptions != null && options.ProviderOptions.TryGetValue("blackForestLabs", out var bfl) && bfl.ValueKind == JsonValueKind.Object)
            {
                foreach (var (option, field) in ProviderOptionFields)
                {
                    if (bfl.TryGetProperty(option, out var optionValue) && optionValue.ValueKind != JsonValueKind.Null)
                    {
                        body[field] = JsonNode.Parse(optionValue.GetRawText());
                    }
                }
            }

            using var document = await _provider.Http.SendJsonAsync(HttpMethod.Post, ApiKeys.Combine(_provider.Options.BaseUrl, path), body.ToJsonString(), _provider.CreateHeaders(options.Headers), cancellationToken).ConfigureAwait(false);
            var url = document.RootElement.TryGetProperty("url", out var value) ? value.GetString() : null;
            return new ImageGenerationResult(new[] { new GeneratedImage("image/png", null, url) });
        }
    }

}

/// <summary>Registers BlackForestLabs.</summary>
public static class BlackForestLabsServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddBlackForestLabs(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(BlackForestLabsProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new BlackForestLabsProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(BlackForestLabsProvider.ProviderId), options);
        });
        return services;
    }
}
