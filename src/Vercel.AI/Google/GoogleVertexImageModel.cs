// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using ProviderGeneratedImage = Vercel.AI.Provider.GeneratedImage;

namespace Vercel.AI.Google;

/// <summary>
/// Gemini image generation on Vertex AI. The call runs through <see cref="GoogleLanguageModel"/> with the <c>IMAGE</c> response modality.
/// Provider options are read from <c>googleVertex</c>, then <c>vertex</c>. Maps to <c>GoogleVertexImageModel</c>.
/// </summary>
public sealed class GoogleVertexImageModel : IImageModel, IImageCaller
{
    private readonly GoogleVertexProvider _provider;

    /// <summary>Creates a Vertex image model.</summary>
    public GoogleVertexImageModel(GoogleVertexProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string Provider
    {
        get { return "google.vertex.image"; }
    }

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Images accepted in one call.</summary>
    public int MaxImagesPerCall
    {
        get { return 1; }
    }

    int? IImageCaller.MaxImagesPerCall => MaxImagesPerCall;

    /// <summary><see langword="true"/> for Gemini image models that take input images, otherwise <see langword="null"/>. Maps to <c>supportsFileInputs</c>.</summary>
    public bool? SupportsFileInputs =>
        ModelId
            is "gemini-2.5-flash-image"
                or "gemini-3-pro-image-preview"
                or "gemini-3.1-flash-image-preview"
                or "gemini-nano-banana-2.1"
            ? true
            : null;

    /// <summary><see langword="false"/> for Gemini image models that take input images but no mask, otherwise <see langword="null"/>. Maps to <c>supportsMaskInputs</c>.</summary>
    public bool? SupportsMaskInputs => SupportsFileInputs == true ? false : null;

    /// <inheritdoc />
    public Task<int?> ResolveMaxImagesPerCallAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<int?>(MaxImagesPerCall);
    }

    /// <inheritdoc />
    public async Task<ImageGenerationResult> DoGenerateAsync(
        ImageCallOptions options,
        CancellationToken cancellationToken
    )
    {
        options ??= new ImageCallOptions(string.Empty);
        var result = await DoGenerateAsync(
                options.ToModelCall(cancellationToken),
                cancellationToken
            )
            .ConfigureAwait(false);
        var images = new List<ProviderGeneratedImage>(result.Images.Count);
        foreach (var image in result.Images)
        {
            var bytes = (byte[])image!;
            images.Add(
                new ProviderGeneratedImage(
                    MediaTypes.DetectMediaType(bytes, "image") ?? "image/png",
                    bytes,
                    null
                )
            );
        }

        var response = result.Response;
        var usage =
            result.Usage == null
                ? null
                : new LanguageModelUsage(
                    result.Usage.InputTokens,
                    result.Usage.OutputTokens,
                    result.Usage.TotalTokens
                );
        return new ImageGenerationResult(
            images,
            result.Warnings.Select(warning => warning.ToCallWarning()).ToList(),
            new ImageResponseInfo(
                new DateTimeOffset(
                    DateTime.SpecifyKind(response.Timestamp ?? DateTime.UtcNow, DateTimeKind.Utc)
                ),
                response.ModelId,
                response.Headers
            ),
            usage,
            result.ProviderMetadata,
            result.IsRetryable
        );
    }

    /// <inheritdoc />
    public async Task<ImageModelResult> DoGenerateAsync(
        ImageModelCall call,
        CancellationToken cancellationToken
    )
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        if (!ModelId.StartsWith("gemini-", StringComparison.Ordinal))
        {
            throw new AiSdkException(
                "Google image models other than Gemini are no longer supported. Use a model ID that starts with `gemini-`."
            );
        }

        var warnings = new List<OperationWarning>();

        // Gemini does not support mask-based inpainting
        if (call.Mask != null)
        {
            throw new AiSdkException(
                "Gemini image models do not support mask-based image editing."
            );
        }

        if (!string.IsNullOrEmpty(call.Size))
        {
            warnings.Add(
                new OperationWarning(
                    "unsupported",
                    feature: "size",
                    details: "This model does not support the `size` option. Use `aspectRatio` instead."
                )
            );
        }

        var content = new List<UserContentPart>();
        if (call.Prompt != null)
        {
            content.Add(new TextContentPart(call.Prompt));
        }

        if (call.Files != null)
        {
            foreach (var file in call.Files)
            {
                content.Add(
                    file.Type == "url"
                        ? new FileContentPart("image/*", new Uri(file.Url!).ToString(), null, null)
                        : new FileContentPart(
                            file.MediaType!,
                            null,
                            file.Data ?? Array.Empty<byte>(),
                            file.FileName
                        )
                );
            }
        }

        // `googleSearch` is the dedicated escape hatch for grounding (generateImage has no `tools` parameter).
        var google = ReadGoogleOptions(call.ProviderOptions);
        var languageModelOptions = new JsonObject();
        JsonNode? userImageConfig = null;
        if (google is { ValueKind: JsonValueKind.Object } bag)
        {
            foreach (var property in bag.EnumerateObject())
            {
                switch (property.Name)
                {
                    case "responseModalities":
                        break;
                    case "imageConfig":
                        userImageConfig = JsonNode.Parse(property.Value.GetRawText());
                        break;
                    default:
                        languageModelOptions[property.Name] = JsonNode.Parse(
                            property.Value.GetRawText()
                        );
                        break;
                }
            }
        }

        languageModelOptions["responseModalities"] = new JsonArray { JsonValue.Create("IMAGE") };
        if (!string.IsNullOrEmpty(call.AspectRatio) || userImageConfig != null)
        {
            var imageConfig = userImageConfig as JsonObject ?? new JsonObject();
            if (!string.IsNullOrEmpty(call.AspectRatio))
            {
                imageConfig["aspectRatio"] = call.AspectRatio;
            }

            languageModelOptions["imageConfig"] = imageConfig;
        }

        var languageModelCall = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage(content) },
            Seed = call.Seed,
            Headers = call.Headers.ToDictionary(pair => pair.Key, pair => (string?)pair.Value),
            ProviderOptions = new Dictionary<string, JsonElement>
            {
                ["googleVertex"] = JsonSerializer.SerializeToElement(languageModelOptions),
                ["vertex"] = JsonSerializer.SerializeToElement(languageModelOptions),
            },
        };

        var result = await new GoogleLanguageModel(_provider, ModelId)
            .DoGenerateAsync(languageModelCall, cancellationToken)
            .ConfigureAwait(false);
        var timestamp = _provider.Clock();
        var images = new List<object?>();
        foreach (var part in result.Content)
        {
            if (
                part is GeneratedFile file
                && file.MediaType.StartsWith("image/", StringComparison.Ordinal)
            )
            {
                images.Add(file.Data);
            }
        }

        var usage = new OperationUsage(
            result.Usage.InputTokens,
            result.Usage.OutputTokens,
            (result.Usage.InputTokens ?? 0) + (result.Usage.OutputTokens ?? 0)
        );
        return new ImageModelResult(
            images,
            warnings,
            usage,
            ProviderMetadata(images.Count),
            new ProviderResponse(
                result.ResponseHeaders,
                timestamp: timestamp.UtcDateTime,
                modelId: ModelId
            ),
            result.FinishReason == FinishReason.ContentFilter ? false : null
        );
    }

    private static JsonElement? ReadGoogleOptions(JsonElement? providerOptions)
    {
        if (providerOptions is not { ValueKind: JsonValueKind.Object } options)
        {
            return null;
        }

        if (options.TryGetProperty("googleVertex", out var vertex))
        {
            return vertex;
        }

        return options.TryGetProperty("vertex", out var legacy) ? legacy : null;
    }

    private static JsonElement? ProviderMetadata(int imageCount)
    {
        var images = new JsonArray();
        for (var i = 0; i < imageCount; i++)
        {
            images.Add(new JsonObject());
        }

        return JsonSerializer.SerializeToElement(
            new JsonObject
            {
                ["googleVertex"] = new JsonObject { ["images"] = images },
                ["vertex"] = new JsonObject { ["images"] = images.DeepClone() },
            }
        );
    }
}
