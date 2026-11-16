// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Gateway;

/// <summary>Pricing of a Gateway model, in USD per token. Maps to the <c>pricing</c> of <c>GatewayLanguageModelEntry</c>.</summary>
public sealed class GatewayModelPricing
{
    /// <summary>Gets or sets the cost per input token in USD.</summary>
    public string Input { get; set; } = string.Empty;

    /// <summary>Gets or sets the cost per output token in USD.</summary>
    public string Output { get; set; } = string.Empty;

    /// <summary>Gets or sets the cost per cached input token in USD. Only present for models that support prompt caching.</summary>
    public string? CachedInputTokens { get; set; }

    /// <summary>Gets or sets the cost per input token to create cache entries in USD. Only present for models that support prompt caching.</summary>
    public string? CacheCreationInputTokens { get; set; }
}

/// <summary>The AI SDK specification of a Gateway model. Maps to <c>GatewayLanguageModelSpecification</c>.</summary>
public sealed class GatewayLanguageModelSpecification
{
    /// <summary>Gets or sets the specification version (<c>v4</c>).</summary>
    public string SpecificationVersion { get; set; } = string.Empty;

    /// <summary>Gets or sets the provider.</summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>Gets or sets the model id.</summary>
    public string ModelId { get; set; } = string.Empty;
}

/// <summary>A model the Gateway offers. Maps to <c>GatewayLanguageModelEntry</c>.</summary>
public sealed class GatewayLanguageModelEntry
{
    /// <summary>Maps to <c>KNOWN_MODEL_TYPES</c>. Entries with another <see cref="ModelType"/> are dropped.</summary>
    public static IReadOnlyList<string> KnownModelTypes { get; } =
    [
        "embedding",
        "decision",

        // Keep "evaluation" until Gateway migrates its model catalog to "decision".
        "evaluation",
        "image",
        "language",
        "realtime",
        "reranking",
        "speech",
        "transcription",
        "video",
    ];

    /// <summary>Gets or sets the model id used by the remote provider in model settings.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name of the model.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the description of the model.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the pricing of the model.</summary>
    public GatewayModelPricing? Pricing { get; set; }

    /// <summary>Gets or sets the AI SDK specification of the model.</summary>
    public GatewayLanguageModelSpecification Specification { get; set; } = new();

    /// <summary>Gets or sets the model type, one of <see cref="KnownModelTypes"/>.</summary>
    public string? ModelType { get; set; }
}
