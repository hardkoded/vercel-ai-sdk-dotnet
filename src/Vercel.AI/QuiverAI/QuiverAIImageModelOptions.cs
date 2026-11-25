// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.QuiverAI;

/// <summary>The operation of an image call. Maps to the <c>operation</c> union of <c>QuiverAIImageModelOptions</c>.</summary>
public enum QuiverAIImageOperation
{
    /// <summary><c>generate</c>: text-to-SVG generation.</summary>
    Generate,

    /// <summary><c>vectorize</c>: convert an input raster image into an SVG.</summary>
    Vectorize,

    /// <summary><c>animate</c>: animate an input SVG.</summary>
    Animate,

    /// <summary><c>edit</c>: edit a single SVG using the prompt text as the instruction.</summary>
    Edit,
}

/// <summary>The SVG <c>viewBox</c> of <c>QuiverAIImageModelOptions.attributes</c>.</summary>
public sealed class QuiverAIViewBox
{
    /// <summary>Maps to <c>minX</c>.</summary>
    public double MinX { get; set; }

    /// <summary>Maps to <c>minY</c>.</summary>
    public double MinY { get; set; }

    /// <summary>Maps to <c>width</c>. Must be positive.</summary>
    public double Width { get; set; }

    /// <summary>Maps to <c>height</c>. Must be positive.</summary>
    public double Height { get; set; }
}

/// <summary>SVG root attributes. Maps to <c>attributes</c> of <c>QuiverAIImageModelOptions</c>.</summary>
public sealed class QuiverAIImageAttributes
{
    /// <summary>Maps to <c>viewBox</c>.</summary>
    public QuiverAIViewBox? ViewBox { get; set; }
}

/// <summary>The <c>quiverai</c> provider options of an image call. Maps to <c>QuiverAIImageModelOptions</c>.</summary>
public sealed class QuiverAIImageModelOptions
{
    /// <summary>The operation to perform. Defaults to <c>generate</c>.</summary>
    public QuiverAIImageOperation? Operation { get; set; }

    /// <summary>Extra style guidance for prompt-based generation.</summary>
    public string? Instructions { get; set; }

    /// <summary>Reasoning effort applied to generation or vectorization.</summary>
    public QuiverAIReasoningEffort? ReasoningEffort { get; set; }

    /// <summary>
    /// Optional reference images for SVG editing (at most 4). Use
    /// <see cref="QuiverAIImageReference.Prepare(byte[])"/> to convert binary inputs.
    /// </summary>
    public IReadOnlyList<QuiverAIImageReference>? ReferenceImages { get; set; }

    /// <summary>Maximum number of edit review and redo steps (0-5).</summary>
    public int? MaxReviewSteps { get; set; }

    /// <summary>SVG root attributes requested for generation or vectorization.</summary>
    public QuiverAIImageAttributes? Attributes { get; set; }

    /// <summary>Sampling temperature (0-2).</summary>
    public double? Temperature { get; set; }

    /// <summary>Nucleus sampling top-p (0-1).</summary>
    public double? TopP { get; set; }

    /// <summary>Presence penalty (-2 to 2). Set <see cref="PresencePenaltyIsNull"/> to send an explicit <c>null</c>.</summary>
    public double? PresencePenalty { get; set; }

    /// <summary>Sends <c>presencePenalty: null</c> (upstream's nullable) when <see cref="PresencePenalty"/> is not set.</summary>
    public bool PresencePenaltyIsNull { get; set; }

    /// <summary>Maximum number of output tokens (1 - 65536 for Arrow 2 models, 131072 otherwise).</summary>
    public int? MaxOutputTokens { get; set; }

    /// <summary>Provider orchestrator token budget for SVG editing (1-65536).</summary>
    public int? OrchestratorMaxOutputTokens { get; set; }

    /// <summary>Provider shallow edit token budget for SVG editing (1-65536).</summary>
    public int? ShallowMaxOutputTokens { get; set; }

    /// <summary>Whether to auto-crop the input image before vectorization. Only used when the operation is <c>vectorize</c>.</summary>
    public bool? AutoCrop { get; set; }

    /// <summary>Target canvas size in pixels for vectorization (128 - 4096). Only used when the operation is <c>vectorize</c>.</summary>
    public int? TargetSize { get; set; }

    /// <summary>
    /// Returns the options as call provider options under <paramref name="providerOptionsName"/>
    /// (<c>quiverai</c> by default), ready for <c>ImageModelCallOptions.ProviderOptions</c>.
    /// The model validates the values when it is called.
    /// </summary>
    public IReadOnlyDictionary<string, JsonElement> ToProviderOptions(
        string providerOptionsName = "quiverai"
    )
    {
        var json = new JsonObject();
        if (Operation is { } operation)
        {
            json["operation"] = operation.ToString().ToLowerInvariant();
        }

        if (Instructions is not null)
        {
            json["instructions"] = Instructions;
        }

        if (ReasoningEffort is { } effort)
        {
            json["reasoningEffort"] = effort.ToString().ToLowerInvariant();
        }

        if (ReferenceImages is not null)
        {
            var references = new JsonArray();
            foreach (var reference in ReferenceImages)
            {
                references.Add(
                    reference.Url is not null
                        ? new JsonObject { ["url"] = reference.Url }
                        : new JsonObject { ["base64"] = reference.Base64 }
                );
            }

            json["referenceImages"] = references;
        }

        if (MaxReviewSteps is { } maxReviewSteps)
        {
            json["maxReviewSteps"] = maxReviewSteps;
        }

        if (Attributes is { } attributes)
        {
            var attributesJson = new JsonObject();
            if (attributes.ViewBox is { } viewBox)
            {
                attributesJson["viewBox"] = new JsonObject
                {
                    ["minX"] = viewBox.MinX,
                    ["minY"] = viewBox.MinY,
                    ["width"] = viewBox.Width,
                    ["height"] = viewBox.Height,
                };
            }

            json["attributes"] = attributesJson;
        }

        if (Temperature is { } temperature)
        {
            json["temperature"] = temperature;
        }

        if (TopP is { } topP)
        {
            json["topP"] = topP;
        }

        if (PresencePenalty is { } presencePenalty)
        {
            json["presencePenalty"] = presencePenalty;
        }
        else if (PresencePenaltyIsNull)
        {
            json["presencePenalty"] = null;
        }

        if (MaxOutputTokens is { } maxOutputTokens)
        {
            json["maxOutputTokens"] = maxOutputTokens;
        }

        if (OrchestratorMaxOutputTokens is { } orchestrator)
        {
            json["orchestratorMaxOutputTokens"] = orchestrator;
        }

        if (ShallowMaxOutputTokens is { } shallow)
        {
            json["shallowMaxOutputTokens"] = shallow;
        }

        if (AutoCrop is { } autoCrop)
        {
            json["autoCrop"] = autoCrop;
        }

        if (TargetSize is { } targetSize)
        {
            json["targetSize"] = targetSize;
        }

        return new Dictionary<string, JsonElement>
        {
            [providerOptionsName] = JsonSerializer.SerializeToElement(json),
        };
    }
}
