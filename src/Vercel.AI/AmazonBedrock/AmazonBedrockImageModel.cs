// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using ProviderGeneratedImage = Vercel.AI.Provider.GeneratedImage;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Amazon Bedrock <c>InvokeModel</c> image model, such as Nova Canvas. Maps to <c>AmazonBedrockImageModel</c>.</summary>
public sealed class AmazonBedrockImageModel : IImageModel, IImageCaller
{
    private readonly AmazonBedrockProvider _provider;
    private readonly Func<DateTime>? _currentDate;

    /// <summary>Creates an image model. <paramref name="currentDate"/> sets the response timestamp. Maps to <c>_internal.currentDate</c>.</summary>
    public AmazonBedrockImageModel(
        AmazonBedrockProvider provider,
        string modelId,
        Func<DateTime>? currentDate = null
    )
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _currentDate = currentDate;
    }

    /// <summary>Specification version.</summary>
    public string SpecificationVersion => "v4";

    /// <inheritdoc cref="IImageModel.Provider" />
    public string Provider => AmazonBedrockProvider.ProviderName;

    /// <inheritdoc cref="IImageModel.ModelId" />
    public string ModelId { get; }

    /// <summary>Maximum images in one call. Maps to <c>maxImagesPerCall</c>.</summary>
    public int? MaxImagesPerCall =>
        AmazonBedrockModelCatalog.GetAmazonBedrockImageModelEntry(ModelId).MaxImagesPerCall ?? 1;

    /// <summary>True when the model takes input images, null when unknown. Maps to <c>supportsFileInputs</c>.</summary>
    public bool? SupportsFileInputs =>
        AmazonBedrockModelCatalog.GetAmazonBedrockImageModelEntry(ModelId).FileInput.IsSupported
            ? true
            : null;

    /// <summary>True when the model takes a mask, null when unknown. Maps to <c>supportsMaskInputs</c>.</summary>
    public bool? SupportsMaskInputs =>
        AmazonBedrockModelCatalog.GetAmazonBedrockImageModelEntry(ModelId).MaskInput.IsSupported
            ? true
            : null;

    /// <inheritdoc />
    public Task<int?> ResolveMaxImagesPerCallAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(MaxImagesPerCall);
    }

    /// <inheritdoc />
    public async Task<ImageGenerationResult> DoGenerateAsync(
        ImageCallOptions options,
        CancellationToken cancellationToken
    )
    {
        options = options ?? throw new ArgumentNullException(nameof(options));
        var result = await DoGenerateAsync(
                options.ToModelCall(cancellationToken),
                cancellationToken
            )
            .ConfigureAwait(false);
        var images = new List<ProviderGeneratedImage>();
        foreach (var image in result.Images)
        {
            var data = image is string base64 ? Convert.FromBase64String(base64) : (byte[])image!;
            images.Add(
                new ProviderGeneratedImage(
                    MediaTypes.DetectMediaType(data, "image") ?? "image/png",
                    data,
                    null
                )
            );
        }

        return new ImageGenerationResult(
            images,
            result.Warnings.Select(warning => warning.ToCallWarning()).ToList()
        );
    }

    /// <inheritdoc />
    public async Task<ImageModelResult> DoGenerateAsync(
        ImageModelCall call,
        CancellationToken cancellationToken
    )
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var warnings = new List<OperationWarning>();
        var (width, height) = ParseSize(call.Size);
        var files = call.Files;
        var hasFiles = files != null && files.Count > 0;
        var options = AmazonBedrockImageModelOptions.Parse(call.ProviderOptions);
        var prompt = call.Prompt;
        var hasPrompt = !string.IsNullOrEmpty(prompt);

        var imageGenerationConfig = new JsonObject();
        if (width is { } w and not 0)
        {
            imageGenerationConfig["width"] = w;
        }

        if (height is { } h and not 0)
        {
            imageGenerationConfig["height"] = h;
        }

        if (call.Seed != null)
        {
            imageGenerationConfig["seed"] = call.Seed.Value;
        }

        if (call.N != 0)
        {
            imageGenerationConfig["numberOfImages"] = call.N;
        }

        if (!string.IsNullOrEmpty(options.Quality))
        {
            imageGenerationConfig["quality"] = options.Quality;
        }

        if (options.CfgScale is { } cfgScale and not 0)
        {
            imageGenerationConfig["cfgScale"] = cfgScale;
        }

        JsonObject args;
        if (hasFiles)
        {
            var hasMask = call.Mask != null;
            var hasMaskPrompt = options.MaskPrompt != null;
            var taskType =
                options.TaskType ?? (hasMask || hasMaskPrompt ? "INPAINTING" : "IMAGE_VARIATION");
            var sourceImageBase64 = GetBase64Data(files![0]);
            switch (taskType)
            {
                case "INPAINTING":
                {
                    var parameters = new JsonObject { ["image"] = sourceImageBase64 };
                    SetText(parameters, hasPrompt, prompt);
                    SetNegativeText(parameters, options);
                    SetMask(parameters, call.Mask, options);
                    args = new JsonObject
                    {
                        ["taskType"] = "INPAINTING",
                        ["inPaintingParams"] = parameters,
                        ["imageGenerationConfig"] = imageGenerationConfig,
                    };
                    break;
                }

                case "OUTPAINTING":
                {
                    var parameters = new JsonObject { ["image"] = sourceImageBase64 };
                    SetText(parameters, hasPrompt, prompt);
                    SetNegativeText(parameters, options);
                    if (!string.IsNullOrEmpty(options.OutPaintingMode))
                    {
                        parameters["outPaintingMode"] = options.OutPaintingMode;
                    }

                    SetMask(parameters, call.Mask, options);
                    args = new JsonObject
                    {
                        ["taskType"] = "OUTPAINTING",
                        ["outPaintingParams"] = parameters,
                        ["imageGenerationConfig"] = imageGenerationConfig,
                    };
                    break;
                }

                case "BACKGROUND_REMOVAL":
                    args = new JsonObject
                    {
                        ["taskType"] = "BACKGROUND_REMOVAL",
                        ["backgroundRemovalParams"] = new JsonObject
                        {
                            ["image"] = sourceImageBase64,
                        },
                    };
                    break;

                case "IMAGE_VARIATION":
                {
                    var images = new JsonArray();
                    foreach (var file in files)
                    {
                        images.Add(GetBase64Data(file));
                    }

                    var parameters = new JsonObject { ["images"] = images };
                    SetText(parameters, hasPrompt, prompt);
                    SetNegativeText(parameters, options);
                    if (options.SimilarityStrength != null)
                    {
                        parameters["similarityStrength"] = options.SimilarityStrength.Value;
                    }

                    args = new JsonObject
                    {
                        ["taskType"] = "IMAGE_VARIATION",
                        ["imageVariationParams"] = parameters,
                        ["imageGenerationConfig"] = imageGenerationConfig,
                    };
                    break;
                }

                default:
                    throw new AiSdkException("Unsupported task type: " + taskType);
            }
        }
        else
        {
            var parameters = new JsonObject { ["text"] = prompt };
            SetNegativeText(parameters, options);
            if (!string.IsNullOrEmpty(options.Style))
            {
                parameters["style"] = options.Style;
            }

            args = new JsonObject
            {
                ["taskType"] = "TEXT_IMAGE",
                ["textToImageParams"] = parameters,
                ["imageGenerationConfig"] = imageGenerationConfig,
            };
        }

        if (call.AspectRatio != null)
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "aspectRatio",
                    "This model does not support aspect ratio. Use `size` instead."
                )
            );
        }

        var currentDate = _currentDate?.Invoke() ?? DateTime.UtcNow;
        var json = args.ToJsonString();
        var uri = new Uri(
            AmazonBedrockEndpoints.Resolve(
                _provider.Options.BaseUrl,
                _provider.Options.Region,
                "bedrock-runtime"
            )
                + "/model/"
                + Uri.EscapeDataString(ModelId)
                + "/invoke"
        );
        using var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
        await _provider
            .AddHeadersAsync(
                request,
                call.Headers.Select(header => new KeyValuePair<string, string?>(
                    header.Key,
                    header.Value
                )),
                cancellationToken
            )
            .ConfigureAwait(false);
        await _provider
            .AuthorizeAsync(request, Encoding.UTF8.GetBytes(json), cancellationToken)
            .ConfigureAwait(false);
        using var response = await _provider
            .HttpClient.SendAsync(request, cancellationToken)
            .ConfigureAwait(false);
        var text = await CancellableReads
            .ReadAsStringAsync(response.Content, cancellationToken)
            .ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw AmazonBedrockErrors.Create((int)response.StatusCode, text);
        }

        var responseHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in response.Headers)
        {
            responseHeaders[header.Key] = string.Join(",", header.Value);
        }

        foreach (var header in response.Content.Headers)
        {
            responseHeaders[header.Key] = string.Join(",", header.Value);
        }

        using var document = JsonDocument.Parse(text);
        var root = document.RootElement;
        var status =
            root.ValueKind == JsonValueKind.Object
            && root.TryGetProperty("status", out var statusElement)
            && statusElement.ValueKind == JsonValueKind.String
                ? statusElement.GetString()
                : null;
        if (status == "Request Moderated")
        {
            var reasons = new List<string>();
            if (
                root.TryGetProperty("details", out var details)
                && details.ValueKind == JsonValueKind.Object
                && details.TryGetProperty("Moderation Reasons", out var moderation)
                && moderation.ValueKind == JsonValueKind.Array
            )
            {
                foreach (var reason in moderation.EnumerateArray())
                {
                    reasons.Add(
                        reason.ValueKind == JsonValueKind.String
                            ? reason.GetString()!
                            : reason.GetRawText()
                    );
                }
            }
            else
            {
                reasons.Add("Unknown");
            }

            throw new AiSdkException(
                "Amazon Bedrock request was moderated: " + string.Join(", ", reasons)
            );
        }

        var generated = new List<object?>();
        if (
            root.TryGetProperty("images", out var imagesElement)
            && imagesElement.ValueKind == JsonValueKind.Array
        )
        {
            foreach (var image in imagesElement.EnumerateArray())
            {
                generated.Add(image.GetString());
            }
        }

        if (generated.Count == 0)
        {
            throw new AiSdkException(
                "Amazon Bedrock returned no images. "
                    + (!string.IsNullOrEmpty(status) ? "Status: " + status : string.Empty)
            );
        }

        return new ImageModelResult(
            generated,
            warnings,
            response: new ProviderResponse(
                responseHeaders,
                timestamp: currentDate,
                modelId: ModelId
            )
        );
    }

    private static void SetText(JsonObject parameters, bool hasPrompt, string? prompt)
    {
        if (hasPrompt)
        {
            parameters["text"] = prompt;
        }
    }

    private static void SetNegativeText(JsonObject parameters, AmazonBedrockImageModelOptions options)
    {
        if (!string.IsNullOrEmpty(options.NegativeText))
        {
            parameters["negativeText"] = options.NegativeText;
        }
    }

    private static void SetMask(
        JsonObject parameters,
        ImageModelFile? mask,
        AmazonBedrockImageModelOptions options
    )
    {
        if (mask != null)
        {
            parameters["maskImage"] = GetBase64Data(mask);
        }
        else if (options.MaskPrompt != null)
        {
            parameters["maskPrompt"] = options.MaskPrompt;
        }
    }

    private static (double? Width, double? Height) ParseSize(string? size)
    {
        if (string.IsNullOrEmpty(size))
        {
            return (null, null);
        }

        var parts = size!.Split('x');
        return (Number(parts, 0), Number(parts, 1));
    }

    private static double? Number(string[] parts, int index)
    {
        return
            index < parts.Length
            && double.TryParse(
                parts[index],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var number
            )
            ? number
            : null;
    }

    /// <summary>Maps to <c>getBase64Data</c>.</summary>
    private static string GetBase64Data(ImageModelFile file)
    {
        if (file.Type == "url")
        {
            throw new AiSdkException(
                "URL-based images are not supported for Amazon Bedrock image editing. "
                    + "Please provide the image data directly."
            );
        }

        return file.Base64 ?? Convert.ToBase64String(file.Data ?? Array.Empty<byte>());
    }
}
