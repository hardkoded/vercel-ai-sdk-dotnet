// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using ProviderGeneratedImage = Vercel.AI.Provider.GeneratedImage;

namespace Vercel.AI.Prodia;

/// <summary>Prodia text-to-image model. It submits one job per image and reads the multipart result.</summary>
public sealed class ProdiaImageModel : IImageModel, IImageCaller
{
    private static readonly string[] StylePresets =
    {
        "3d-model", "analog-film", "anime", "cinematic", "comic-book", "digital-art", "enhance", "fantasy-art", "isometric",
        "line-art", "low-poly", "neon-punk", "origami", "photographic", "pixel-art", "texture", "craft-clay",
    };

    private readonly ProdiaProvider _provider;
    private readonly Func<DateTimeOffset> _clock;

    /// <summary>Creates a model. <paramref name="clock"/> sets the response timestamp.</summary>
    public ProdiaImageModel(ProdiaProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    /// <inheritdoc />
    public string Provider => "prodia.image";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Prodia returns one image per job.</summary>
    public int? MaxImagesPerCall => 1;

    /// <inheritdoc />
    public Task<int?> ResolveMaxImagesPerCallAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(MaxImagesPerCall);
    }

    /// <inheritdoc />
    public async Task<ImageGenerationResult> DoGenerateAsync(ImageCallOptions options, CancellationToken cancellationToken)
    {
        options = options ?? throw new ArgumentNullException(nameof(options));
        var call = new ImageModelCall(options.Prompt, null, null, options.Count, options.Size, options.AspectRatio, null, null, new Dictionary<string, string>(), cancellationToken);
        var result = await DoGenerateAsync(call, cancellationToken).ConfigureAwait(false);
        var data = (byte[])result.Images[0]!;
        return new ImageGenerationResult(new[] { new ProviderGeneratedImage(MediaTypes.DetectMediaType(data, "image") ?? "image/png", data, null) });
    }

    /// <inheritdoc />
    public async Task<ImageModelResult> DoGenerateAsync(ImageModelCall call, CancellationToken cancellationToken)
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var warnings = new List<OperationWarning>();
        var options = Options(call.ProviderOptions);
        int? width = null;
        int? height = null;
        if (!string.IsNullOrEmpty(call.Size))
        {
            var size = call.Size!.Split('x');
            if (size.Length >= 2
                && int.TryParse(size[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedWidth)
                && int.TryParse(size[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedHeight))
            {
                width = parsedWidth;
                height = parsedHeight;
            }
            else
            {
                warnings.Add(new OperationWarning("unsupported", feature: "size", details: "Invalid size format: " + call.Size + ". Expected format: WIDTHxHEIGHT (e.g., 1024x1024)"));
            }
        }

        var config = new JsonObject { ["prompt"] = call.Prompt };
        width = Integer(options, "width", 256, 1920) ?? width;
        height = Integer(options, "height", 256, 1920) ?? height;
        if (width != null)
        {
            config["width"] = width;
        }

        if (height != null)
        {
            config["height"] = height;
        }

        if (call.Seed != null)
        {
            config["seed"] = call.Seed;
        }

        if (Integer(options, "steps", 1, 4) is int steps)
        {
            config["steps"] = steps;
        }

        if (options?.TryGetProperty("stylePreset", out var stylePreset) == true)
        {
            if (stylePreset.ValueKind != JsonValueKind.String || Array.IndexOf(StylePresets, stylePreset.GetString()) < 0)
            {
                throw Invalid("stylePreset");
            }

            config["style_preset"] = stylePreset.GetString();
        }

        if (options?.TryGetProperty("loras", out var loras) == true)
        {
            if (loras.ValueKind != JsonValueKind.Array || loras.GetArrayLength() > 3 || loras.EnumerateArray().Any(lora => lora.ValueKind != JsonValueKind.String))
            {
                throw Invalid("loras");
            }

            if (loras.GetArrayLength() > 0)
            {
                config["loras"] = JsonNode.Parse(loras.GetRawText());
            }
        }

        if (options?.TryGetProperty("progressive", out var progressive) == true)
        {
            if (progressive.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
            {
                throw Invalid("progressive");
            }

            config["progressive"] = progressive.GetBoolean();
        }

        var timestamp = _clock();
        var (parts, headers) = await ProdiaApi.PostJobAsync(
            _provider,
            ProviderExchange.Json(ProdiaApi.Job(ModelId, config)),
            "multipart/form-data; image/png",
            call.Headers,
            cancellationToken).ConfigureAwait(false);
        var job = ProdiaApi.ReadJob(parts);
        byte[]? image = null;
        foreach (var part in parts)
        {
            if (part.ContentDisposition.Contains("name=\"output\"") || part.ContentType.StartsWith("image/", StringComparison.Ordinal))
            {
                image = part.Body;
            }
        }

        if (image == null)
        {
            throw new InvalidResponseDataException(null, "Prodia multipart response missing output image");
        }

        var metadata = new JsonObject { ["prodia"] = new JsonObject { ["images"] = new JsonArray(ProdiaApi.Metadata(job)) } };
        return new ImageModelResult(
            new object?[] { image },
            warnings,
            providerMetadata: OperationJson.Parse(metadata.ToJsonString()),
            response: new ProviderResponse(headers, timestamp: timestamp.UtcDateTime, modelId: ModelId));
    }

    private static JsonElement? Options(JsonElement? providerOptions)
    {
        return providerOptions is { ValueKind: JsonValueKind.Object } bag && bag.TryGetProperty("prodia", out var prodia) && prodia.ValueKind == JsonValueKind.Object
            ? prodia
            : null;
    }

    private static int? Integer(JsonElement? options, string name, int min, int max)
    {
        if (options?.TryGetProperty(name, out var value) != true)
        {
            return null;
        }

        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out var number) || number < min || number > max)
        {
            throw Invalid(name);
        }

        return number;
    }

    private static InvalidArgumentException Invalid(string name)
    {
        return new InvalidArgumentException("providerOptions", null, "Invalid Prodia provider option " + name + ".");
    }
}
