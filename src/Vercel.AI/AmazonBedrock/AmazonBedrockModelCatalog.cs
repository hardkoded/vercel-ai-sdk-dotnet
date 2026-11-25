// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.AmazonBedrock;

/// <summary>The capabilities of an Amazon Bedrock image model. Maps to the <c>image</c> capabilities of a <c>ModelEntry</c>.</summary>
internal sealed class AmazonBedrockImageModelEntry
{
    public AmazonBedrockImageModelEntry(
        int? maxImagesPerCall,
        ModelCapability fileInput,
        ModelCapability maskInput
    )
    {
        MaxImagesPerCall = maxImagesPerCall;
        FileInput = fileInput;
        MaskInput = maskInput;
    }

    public int? MaxImagesPerCall { get; }

    public ModelCapability FileInput { get; }

    public ModelCapability MaskInput { get; }
}

/// <summary>Model catalog of the Amazon Bedrock provider. Maps to <c>amazon-bedrock-model-catalog.ts</c>.</summary>
internal static class AmazonBedrockModelCatalog
{
    private static readonly AmazonBedrockImageModelEntry NovaCanvas = new(
        5,
        ModelCapability.Supported,
        ModelCapability.Supported
    );

    private static readonly AmazonBedrockImageModelEntry UnknownImageModel = new(
        null,
        ModelCapability.Unknown,
        ModelCapability.Unknown
    );

    /// <summary>Returns the entry of an image model. Maps to <c>getAmazonBedrockImageModelEntry</c>.</summary>
    public static AmazonBedrockImageModelEntry GetAmazonBedrockImageModelEntry(string modelId)
    {
        return modelId == "amazon.nova-canvas-v1:0" ? NovaCanvas : UnknownImageModel;
    }
}
