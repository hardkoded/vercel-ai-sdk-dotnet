// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Azure;

/// <summary>
/// Azure image model. Routes each call to the MAI image API or the OpenAI image API. Resolves the API per call:
/// <c>providerOptions</c> also reach this model through a gateway. Maps to <c>AzureImageModel</c> in <c>azure-openai-provider.ts</c>.
/// </summary>
public sealed class AzureImageModel : IImageModel, IImageCaller
{
    private readonly OpenAIImageModel _openai;
    private readonly AzureMaiImageModel _mai;

    internal AzureImageModel(AzureOpenAIProvider provider, string modelId)
    {
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _openai = new OpenAIImageModel(provider, modelId);
        _mai = new AzureMaiImageModel(provider, modelId);
    }

    /// <inheritdoc />
    public string Provider => "azure.image";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Specification version.</summary>
    public string SpecificationVersion => "v4";

    /// <summary>Maximum images in one call. Follows the default route; an <c>api</c> override is per request.</summary>
    public int MaxImagesPerCall => IsDefaultMai ? _mai.MaxImagesPerCall : _openai.MaxImagesPerCall;

    /// <summary>
    /// True when the model takes input images, null when unknown. Follows the default route. Azure deployment names are
    /// user-defined, so OpenAI model family capabilities cannot be inferred from them.
    /// </summary>
    public bool? SupportsFileInputs => IsDefaultMai ? _mai.SupportsFileInputs : null;

    /// <summary>True when the model takes a mask, false when it takes input images but no mask, null when unknown.</summary>
    public bool? SupportsMaskInputs => IsDefaultMai ? _mai.SupportsMaskInputs : null;

    int? IImageCaller.MaxImagesPerCall => MaxImagesPerCall;

    private bool IsDefaultMai => AzureImageModelOptions.IsMAIImageModel(ModelId);

    /// <inheritdoc />
    public Task<int?> ResolveMaxImagesPerCallAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<int?>(MaxImagesPerCall);
    }

    /// <inheritdoc />
    public async Task<ImageGenerationResult> DoGenerateAsync(ImageCallOptions options, CancellationToken cancellationToken)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var azureOptions = AzureImageModelOptions.Parse(options.ProviderOptionsElement);
        var maiOptions = new AzureImageModelMaiOptions { AutoAspectRatio = azureOptions.AutoAspectRatio, WebGrounding = azureOptions.WebGrounding };
        if ((azureOptions.Api ?? (IsDefaultMai ? "mai" : "openai")) == "mai")
        {
            return await _mai.DoGenerateAsync(options, maiOptions, cancellationToken).ConfigureAwait(false);
        }

        var shape = options.Files is { Count: > 0 } ? OpenAIImageModelOptions.EditOptionsShape() : OpenAIImageModelOptions.GenerationOptionsShape();
        var providerOptions = AzureProviderOptions.WithAzureOptionsForOpenAI(
            options.ProviderOptionsElement,
            AzureProviderOptions.PickAzureOptions(options.ProviderOptionsElement, shape));
        var result = await _openai.DoGenerateAsync(WithProviderOptions(options, providerOptions), cancellationToken).ConfigureAwait(false);

        var maiOnly = new JsonObject();
        if (maiOptions.AutoAspectRatio is { } autoAspectRatio)
        {
            maiOnly["autoAspectRatio"] = autoAspectRatio;
        }

        if (maiOptions.WebGrounding is { } webGrounding)
        {
            maiOnly["webGrounding"] = webGrounding;
        }

        var warnings = result.CallWarnings
            .Concat(AzureProviderOptions.UnsupportedOptionWarnings(maiOnly, "This option requires the MAI image API.").Select(warning => warning.ToCallWarning()))
            .ToList();
        return new ImageGenerationResult(result.Images, warnings, result.Response, result.Usage, result.ProviderMetadata, result.IsRetryable);
    }

    /// <inheritdoc />
    public async Task<ImageModelResult> DoGenerateAsync(ImageModelCall call, CancellationToken cancellationToken)
    {
        if (call is null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        var options = new ImageCallOptions(call.Prompt ?? string.Empty)
        {
            Count = call.N,
            Size = call.Size,
            AspectRatio = call.AspectRatio,
            Seed = call.Seed,
            Files = call.Files,
            Mask = call.Mask,
            ProviderOptions = ToProviderOptions(call.ProviderOptions),
            Headers = call.Headers.ToDictionary(pair => pair.Key, pair => (string?)pair.Value),
        };
        var result = await DoGenerateAsync(options, cancellationToken).ConfigureAwait(false);
        var images = result.Images.Select(image => (object?)image.Data).ToList();
        var warnings = result.CallWarnings
            .Select(warning => warning.Feature == null
                ? new OperationWarning(warning.Type, warning.Message)
                : new OperationWarning(warning.Type, null, warning.Feature, warning.Message == warning.Feature ? null : warning.Message))
            .ToList();
        var usage = result.Usage == null ? null : new OperationUsage(result.Usage.InputTokens, result.Usage.OutputTokens, result.Usage.TotalTokens);
        var response = result.Response == null
            ? null
            : new ProviderResponse(result.Response.Headers, timestamp: result.Response.Timestamp.UtcDateTime, modelId: result.Response.ModelId);
        return new ImageModelResult(images, warnings, usage, result.ProviderMetadata, response, result.IsRetryable);
    }

    private static ImageCallOptions WithProviderOptions(ImageCallOptions options, JsonElement? providerOptions)
    {
        return new ImageCallOptions(options.Prompt)
        {
            Count = options.Count,
            Size = options.Size,
            AspectRatio = options.AspectRatio,
            Seed = options.Seed,
            Files = options.Files,
            Mask = options.Mask,
            ProviderOptions = ToProviderOptions(providerOptions),
            Headers = options.Headers,
        };
    }

    private static IReadOnlyDictionary<string, JsonElement>? ToProviderOptions(JsonElement? providerOptions)
    {
        if (providerOptions is not { ValueKind: JsonValueKind.Object } root)
        {
            return null;
        }

        var result = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach (var property in root.EnumerateObject())
        {
            result[property.Name] = property.Value.Clone();
        }

        return result;
    }
}
