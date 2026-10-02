// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.DeepInfra;

/// <summary>DeepInfra image model. Generation posts to <c>/inference/{model}</c>.</summary>
public sealed class DeepInfraImageModel : IImageModel
{
    private readonly DeepInfraProvider _provider;

    /// <summary>Creates an image model.</summary>
    public DeepInfraImageModel(DeepInfraProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string Provider => "deepinfra.image";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Random seed sent as <c>seed</c>.</summary>
    public int? Seed { get; set; }

    /// <summary>Provider-specific fields merged into the body.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }

    /// <summary>Headers for this call.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; set; }

    /// <summary>Warnings from the most recent call.</summary>
    public IReadOnlyList<CallWarning> LastWarnings { get; private set; } = Array.Empty<CallWarning>();

    /// <summary>HTTP response headers from the most recent call.</summary>
    public IReadOnlyDictionary<string, string> LastResponseHeaders { get; private set; } = new Dictionary<string, string>();

    /// <inheritdoc />
    public async Task<ImageGenerationResult> DoGenerateAsync(ImageCallOptions options, CancellationToken cancellationToken)
    {
        options = options ?? throw new ArgumentNullException(nameof(options));
        var warnings = new List<CallWarning>();
        var body = new JsonObject
        {
            ["prompt"] = options.Prompt,
            ["num_images"] = options.Count,
        };
        if (!string.IsNullOrEmpty(options.AspectRatio))
        {
            body["aspect_ratio"] = options.AspectRatio;
        }

        var size = SplitSize(options.Size);
        if (size != null)
        {
            body["width"] = size.Value.Width;
            body["height"] = size.Value.Height;
        }

        if (Seed != null)
        {
            body["seed"] = Seed.Value;
        }

        OpenAICompatibleImages.MergeOptions(body, Provider, ProviderOptions, warnings);
        var origin = string.IsNullOrEmpty(_provider.Options.ImageBaseUrl)
            ? "https://api.deepinfra.com/v1/inference"
            : _provider.Options.ImageBaseUrl!.TrimEnd('/');
        var response = await _provider.PostJsonAsync(new Uri(origin + "/" + ModelId), body.ToJsonString(), _provider.CreateHeaders(Headers), cancellationToken).ConfigureAwait(false);
        LastWarnings = warnings;
        LastResponseHeaders = response.Headers;
        return ReadImages(response.Body);
    }

    /// <summary>Reads <c>detail.error</c>, then the shared OpenAI-compatible error message.</summary>
    public static string? ReadError(string? body)
    {
        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                var root = JsonNode.Parse(body!) as JsonObject;
                if (root?["detail"] is JsonObject detail && detail["error"] is JsonValue value && value.TryGetValue<string>(out var text) && !string.IsNullOrEmpty(text))
                {
                    return text;
                }
            }
            catch (JsonException)
            {
            }
        }

        return OpenAICompatibleChat.ReadErrorMessage(body);
    }

    private static (string Width, string Height)? SplitSize(string? size)
    {
        if (string.IsNullOrEmpty(size))
        {
            return null;
        }

        var parts = size!.Split('x');
        if (parts.Length != 2)
        {
            return null;
        }

        return (parts[0], parts[1]);
    }

    private static ImageGenerationResult ReadImages(string? json)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json!);
        var images = new List<GeneratedImage>();
        if (!document.RootElement.TryGetProperty("images", out var items) || items.ValueKind != JsonValueKind.Array)
        {
            return new ImageGenerationResult(images);
        }

        foreach (var item in items.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            var value = item.GetString() ?? string.Empty;
            var comma = value.IndexOf(",", StringComparison.Ordinal);
            if (value.StartsWith("data:", StringComparison.Ordinal) && comma >= 0)
            {
                value = value.Substring(comma + 1);
            }

            images.Add(new GeneratedImage("image/png", Convert.FromBase64String(value), null));
        }

        return new ImageGenerationResult(images);
    }
}
