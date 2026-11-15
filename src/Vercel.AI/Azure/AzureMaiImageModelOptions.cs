// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Azure;

/// <summary>
/// MAI image options, read from <c>providerOptions.azure</c>. Maps to <c>AzureImageModelMaiOptions</c> and
/// <c>azureMaiImageModelOptionsShape</c> in <c>azure-mai-image-model-options.ts</c>.
/// </summary>
public class AzureImageModelMaiOptions
{
    /// <summary>
    /// Let the model pick the output aspect ratio from the prompt and any reference images. MAI-Image-2.6 models only.
    /// </summary>
    public bool? AutoAspectRatio { get; set; }

    /// <summary>Ground generation in Bing web search results. MAI-Image-2.6 models only.</summary>
    public bool? WebGrounding { get; set; }
}
