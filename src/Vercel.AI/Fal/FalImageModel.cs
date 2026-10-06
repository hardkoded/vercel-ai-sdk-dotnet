// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using ProviderImage = Vercel.AI.Provider.GeneratedImage;

namespace Vercel.AI.Fal;

/// <summary>Fal image model. Posts to <c>{baseURL}/{modelId}</c> and downloads each returned image.</summary>
public sealed class FalImageModel : IImageModel, IImageCaller
{
    // Provider options that have a deprecated snake_case spelling, as (camelCase, snake_case).
    private static readonly (string Camel, string Snake)[] RenamedOptions =
    {
        ("imageUrl", "image_url"),
        ("maskUrl", "mask_url"),
        ("guidanceScale", "guidance_scale"),
        ("numInferenceSteps", "num_inference_steps"),
        ("enableSafetyChecker", "enable_safety_checker"),
        ("outputFormat", "output_format"),
        ("syncMode", "sync_mode"),
        ("safetyTolerance", "safety_tolerance"),
    };

    private static readonly string[] CamelOnlyOptions = { "strength", "acceleration", "useMultipleImages" };

    private static readonly HashSet<string> ResponseFieldsOutsideMetadata = new HashSet<string> { "images", "prompt", "has_nsfw_concepts", "nsfw_content_detected" };

    private readonly FalProvider _provider;
    private readonly Func<DateTime>? _clock;

    /// <summary>Creates an image model. <paramref name="clock"/> sets the response timestamp.</summary>
    public FalImageModel(FalProvider provider, string modelId, Func<DateTime>? clock = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _clock = clock;
    }

    /// <inheritdoc cref="IImageModel.Provider" />
    public string Provider => "fal.image";

    /// <inheritdoc cref="IImageModel.ModelId" />
    public string ModelId { get; }

    /// <summary>Specification version.</summary>
    public string SpecificationVersion => "v4";

    /// <summary>Fal returns one image per call.</summary>
    public int? MaxImagesPerCall => 1;

    /// <inheritdoc />
    public Task<int?> ResolveMaxImagesPerCallAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(MaxImagesPerCall);
    }

    /// <inheritdoc />
    public Task<ImageGenerationResult> DoGenerateAsync(ImageCallOptions options, CancellationToken cancellationToken)
    {
        return GenerateAsync(options, null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ImageModelResult> DoGenerateAsync(ImageModelCall call, CancellationToken cancellationToken)
    {
        if (call is null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        var warnings = new List<OperationWarning>();
        var body = CreateBody(call, warnings);
        var timestamp = _clock?.Invoke() ?? DateTime.UtcNow;
        ProviderExchangeResult response;
        try
        {
            response = await _provider.SendAsync(HttpMethod.Post, ApiKeys.Combine(_provider.Options.BaseUrl, "/" + ModelId), ProviderExchange.Json(body.ToJsonString()), call.Headers, cancellationToken).ConfigureAwait(false);
        }
        catch (ApiException error) when (ErrorMessage(error.ResponseBody) is { } message)
        {
            throw ProviderHttp.MapStatus(error.StatusCode, message, error.ResponseBody);
        }

        using var document = JsonDocument.Parse(response.Body);
        var root = document.RootElement;
        var targetImages = ReadImages(root, response.Body);
        var images = new List<object?>();
        var imageMetadata = new JsonArray();
        for (var index = 0; index < targetImages.Count; index++)
        {
            var image = targetImages[index];
            var url = image.TryGetProperty("url", out var urlElement) && urlElement.ValueKind == JsonValueKind.String
                ? urlElement.GetString()!
                : throw new InvalidResponseDataException(response.Body, "Fal image has no url.");
            images.Add(await _provider.DownloadAsync(url, _provider.Options.BaseUrl, cancellationToken).ConfigureAwait(false));
            imageMetadata.Add(ImageMetadata(root, image, index));
        }

        var fal = new JsonObject { ["images"] = imageMetadata };
        foreach (var property in root.EnumerateObject())
        {
            if (!ResponseFieldsOutsideMetadata.Contains(property.Name))
            {
                fal[property.Name] = JsonNode.Parse(property.Value.GetRawText());
            }
        }

        var providerMetadata = JsonSerializer.SerializeToElement(new JsonObject { [FalProvider.ProviderId] = fal });
        return new ImageModelResult(images, warnings, providerMetadata: providerMetadata, response: new ProviderResponse(response.Headers, timestamp: timestamp, modelId: ModelId));
    }

    internal async Task<ImageGenerationResult> GenerateAsync(ImageCallOptions options, IDictionary<string, string>? headers, CancellationToken cancellationToken)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var call = new ImageModelCall(options.Prompt, null, null, options.Count, options.Size, options.AspectRatio, null, null, new Dictionary<string, string>(headers ?? new Dictionary<string, string>()), cancellationToken);
        var result = await DoGenerateAsync(call, cancellationToken).ConfigureAwait(false);
        var images = new List<ProviderImage>();
        foreach (var image in result.Images)
        {
            var bytes = (byte[])image!;
            images.Add(new ProviderImage(MediaTypes.DetectMediaType(bytes, "image") ?? "image/png", bytes, null));
        }

        return new ImageGenerationResult(images);
    }

    private static JsonObject CreateBody(ImageModelCall call, List<OperationWarning> warnings)
    {
        var body = new JsonObject { ["prompt"] = call.Prompt };
        if (call.Seed is { } seed)
        {
            body["seed"] = seed;
        }

        if (ImageSize(call.Size, call.AspectRatio) is { } imageSize)
        {
            body["image_size"] = imageSize;
        }

        body["num_images"] = call.N;
        var fal = FalProvider.FalOptions(call.ProviderOptions);
        var options = fal is { } value ? NormalizeOptions(value, warnings) : new List<KeyValuePair<string, JsonElement>>();
        if (call.Files is { Count: > 0 } files)
        {
            if (options.Exists(option => option.Key == "useMultipleImages" && option.Value.ValueKind == JsonValueKind.True))
            {
                var urls = new JsonArray();
                foreach (var file in files)
                {
                    urls.Add(FileDataConversions.ConvertImageModelFileToDataUri(file));
                }

                body["image_urls"] = urls;
            }
            else
            {
                body["image_url"] = FileDataConversions.ConvertImageModelFileToDataUri(files[0]);
                if (files.Count > 1)
                {
                    warnings.Add(OperationWarning.Other(
                        "Multiple input images provided but useMultipleImages is not enabled. "
                        + "Only the first image will be used. Set providerOptions.fal.useMultipleImages "
                        + "to true for models that support multiple images (e.g., fal-ai/flux-2/edit)."));
                }
            }
        }

        if (call.Mask != null)
        {
            body["mask_url"] = FileDataConversions.ConvertImageModelFileToDataUri(call.Mask);
        }

        foreach (var option in options)
        {
            if (option.Key != "useMultipleImages")
            {
                body[ApiName(option.Key)] = JsonNode.Parse(option.Value.GetRawText());
            }
        }

        return body;
    }

    // Reads camelCase options, accepts the deprecated snake_case spellings with a warning, and keeps unknown options as given.
    private static List<KeyValuePair<string, JsonElement>> NormalizeOptions(JsonElement fal, List<OperationWarning> warnings)
    {
        var options = new List<KeyValuePair<string, JsonElement>>();
        var deprecated = new List<string>();
        foreach (var (camel, snake) in RenamedOptions)
        {
            if (TryGetValue(fal, snake, out var snakeValue))
            {
                deprecated.Add(snake);
                options.Add(new KeyValuePair<string, JsonElement>(camel, snakeValue));
            }
            else if (TryGetValue(fal, camel, out var camelValue))
            {
                options.Add(new KeyValuePair<string, JsonElement>(camel, camelValue));
            }
        }

        foreach (var name in CamelOnlyOptions)
        {
            if (TryGetValue(fal, name, out var value))
            {
                options.Add(new KeyValuePair<string, JsonElement>(name, value));
            }
        }

        foreach (var property in fal.EnumerateObject())
        {
            if (!IsKnownOption(property.Name))
            {
                options.Add(new KeyValuePair<string, JsonElement>(property.Name, property.Value));
            }
        }

        if (deprecated.Count > 0)
        {
            var replacements = deprecated.Select(snake => "'" + snake + "' (use '" + RenamedOptions[Array.FindIndex(RenamedOptions, option => option.Snake == snake)].Camel + "')");
            warnings.Add(OperationWarning.Other("The following provider options use deprecated snake_case and will be removed in @ai-sdk/fal v2.0. Please use camelCase instead: " + string.Join(", ", replacements)));
        }

        return options;
    }

    private static bool TryGetValue(JsonElement fal, string name, out JsonElement value)
    {
        return fal.TryGetProperty(name, out value) && value.ValueKind != JsonValueKind.Null;
    }

    private static bool IsKnownOption(string name)
    {
        return Array.IndexOf(CamelOnlyOptions, name) >= 0 || Array.Exists(RenamedOptions, option => option.Camel == name || option.Snake == name);
    }

    private static string ApiName(string option)
    {
        var index = Array.FindIndex(RenamedOptions, candidate => candidate.Camel == option);
        return index < 0 ? option : RenamedOptions[index].Snake;
    }

    private static JsonNode? ImageSize(string? size, string? aspectRatio)
    {
        if (!string.IsNullOrEmpty(size))
        {
            var parts = size!.Split('x');
            return Dimensions(int.Parse(parts[0], CultureInfo.InvariantCulture), int.Parse(parts[1], CultureInfo.InvariantCulture));
        }

        switch (aspectRatio)
        {
            case "1:1":
                return "square_hd";
            case "16:9":
                return "landscape_16_9";
            case "9:16":
                return "portrait_16_9";
            case "4:3":
                return "landscape_4_3";
            case "3:4":
                return "portrait_4_3";
            case "16:10":
                return Dimensions(1280, 800);
            case "10:16":
                return Dimensions(800, 1280);
            case "21:9":
                return Dimensions(2560, 1080);
            case "9:21":
                return Dimensions(1080, 2560);
            default:
                return null;
        }
    }

    private static JsonObject Dimensions(int width, int height)
    {
        return new JsonObject { ["width"] = width, ["height"] = height };
    }

    // Most models return an images array; some return a single image.
    private static List<JsonElement> ReadImages(JsonElement root, string body)
    {
        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("images", out var images) && images.ValueKind == JsonValueKind.Array)
        {
            return images.EnumerateArray().ToList();
        }

        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("image", out var image) && image.ValueKind == JsonValueKind.Object)
        {
            return new List<JsonElement> { image };
        }

        throw new InvalidResponseDataException(body, "Fal image response has no images.");
    }

    // Keeps width and height, renames the file fields to camelCase, and adds the NSFW flag for this image.
    private static JsonObject ImageMetadata(JsonElement root, JsonElement image, int index)
    {
        var metadata = new JsonObject();
        CopyIfPresent(image, "width", metadata, "width");
        CopyIfPresent(image, "height", metadata, "height");
        CopyIfPresent(image, "content_type", metadata, "contentType");
        CopyIfPresent(image, "file_name", metadata, "fileName");
        CopyIfPresent(image, "file_data", metadata, "fileData");
        CopyIfPresent(image, "file_size", metadata, "fileSize");
        if ((Nsfw(root, "has_nsfw_concepts", index) ?? Nsfw(root, "nsfw_content_detected", index)) is { } nsfw)
        {
            metadata["nsfw"] = nsfw;
        }

        return metadata;
    }

    private static void CopyIfPresent(JsonElement source, string name, JsonObject target, string targetName)
    {
        if (source.TryGetProperty(name, out var value))
        {
            target[targetName] = JsonNode.Parse(value.GetRawText());
        }
    }

    private static bool? Nsfw(JsonElement root, string name, int index)
    {
        if (root.TryGetProperty(name, out var flags) && flags.ValueKind == JsonValueKind.Array && index < flags.GetArrayLength())
        {
            var flag = flags[index];
            return flag.ValueKind == JsonValueKind.True ? true : flag.ValueKind == JsonValueKind.False ? false : null;
        }

        return null;
    }

    // Fal validation errors list each invalid field as loc and msg; other errors carry a message.
    private static string? ErrorMessage(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(body!);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            if (root.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.Array)
            {
                var lines = new List<string>();
                foreach (var item in detail.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object
                        || !item.TryGetProperty("loc", out var loc) || loc.ValueKind != JsonValueKind.Array
                        || !item.TryGetProperty("msg", out var msg) || msg.ValueKind != JsonValueKind.String)
                    {
                        return null;
                    }

                    lines.Add(string.Join(".", loc.EnumerateArray().Select(part => part.ToString())) + ": " + msg.GetString());
                }

                return string.Join("\n", lines);
            }

            return root.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String ? message.GetString() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
