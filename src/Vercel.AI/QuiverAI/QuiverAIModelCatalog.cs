// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.QuiverAI;

/// <summary>Known options accepted under <c>providerOptions.quiverai</c>. Maps to <c>quiveraiProviderOptions</c>.</summary>
public static class QuiverAIProviderOptions
{
    /// <summary>Maps to <c>quiveraiProviderOptions.language</c>, keyed by option name.</summary>
    public static readonly IReadOnlyDictionary<string, ProviderOptionDefinition> Language =
        new Dictionary<string, ProviderOptionDefinition>
        {
            ["reasoningEffort"] = new("Controls the amount of reasoning used by the model."),
            ["reasoningSummary"] = new("Requests the model's safe reasoning summary."),
        };

    /// <summary>Maps to <c>quiveraiProviderOptions.image</c>, keyed by option name.</summary>
    public static readonly IReadOnlyDictionary<string, ProviderOptionDefinition> Image =
        new Dictionary<string, ProviderOptionDefinition>
        {
            ["operation"] = new(
                "The operation to perform. Defaults to `generate`. - `generate`: Text-to-SVG generation. Requires `prompt`. - `vectorize`: Convert an input raster image into an SVG. Requires a single image in `prompt.images` / `files`. - `animate`: Animate an input SVG. Requires a single SVG in `prompt.images` / `files`; the text prompt is optional. - `edit`: Edit a single SVG from `prompt.images` using `prompt.text` as the required instruction."
            ),
            ["instructions"] = new("Extra style guidance for prompt-based generation."),
            ["reasoningEffort"] = new("Reasoning effort applied to generation or vectorization."),
            ["referenceImages"] = new(
                "Optional reference images for SVG editing. Use `PrepareQuiverAIImageReference` to convert binary inputs."
            ),
            ["maxReviewSteps"] = new("Maximum number of edit review and redo steps (0-5)."),
            ["attributes"] = new("SVG root attributes requested for generation or vectorization."),
            ["temperature"] = new("Sampling temperature (0-2)."),
            ["topP"] = new("Nucleus sampling top-p (0-1)."),
            ["presencePenalty"] = new("Presence penalty (-2 to 2)."),
            ["maxOutputTokens"] = new(
                "Maximum number of output tokens (1 - 65536 for Arrow 2 models). The legacy upper bound of 131072 is retained for other model IDs."
            ),
            ["orchestratorMaxOutputTokens"] = new(
                "Provider orchestrator token budget for SVG editing (1-65536)."
            ),
            ["shallowMaxOutputTokens"] = new(
                "Provider shallow edit token budget for SVG editing (1-65536)."
            ),
            ["autoCrop"] = new(
                "Whether to auto-crop the input image before vectorization. Only used when `operation` is `vectorize`."
            ),
            ["targetSize"] = new(
                "Target canvas size in pixels for vectorization (128 - 4096). Only used when `operation` is `vectorize`."
            ),
        };
}

/// <summary>Request defaults of the language models. Maps to <c>quiveraiRequestDefaults.language</c>.</summary>
public sealed class QuiverAILanguageRequestConfig
{
    /// <summary>Maps to <c>structuredOutputs</c>.</summary>
    public bool StructuredOutputs { get; init; }

    /// <summary>Maps to <c>reasoningEfforts</c>.</summary>
    public IReadOnlyList<string> ReasoningEfforts { get; init; } = [];
}

/// <summary>Request configuration of an image model. Maps to <c>requestConfig.image</c> of <c>getQuiverAIImageModelEntry</c>.</summary>
public sealed class QuiverAIImageRequestConfig
{
    /// <summary>Maps to <c>generateReferenceLimit</c>.</summary>
    public int GenerateReferenceLimit { get; init; }

    /// <summary>Maps to <c>maxOutputTokens</c>.</summary>
    public int? MaxOutputTokens { get; init; }

    /// <summary>Maps to <c>editModel</c>.</summary>
    public bool EditModel { get; init; }
}

/// <summary>The image capabilities of a QuiverAI model: the shared <see cref="ImageCapabilities"/> plus animation and editing. Maps to <c>capabilities.image</c> of <c>getQuiverAIImageModelEntry</c>.</summary>
public sealed class QuiverAIImageCapabilities
{
    /// <summary>Maps to <c>sizes.description</c>.</summary>
    public string SizesDescription { get; init; } = "";

    /// <summary>Maps to <c>fileInput</c>.</summary>
    public ModelCapability FileInput { get; init; } = ModelCapability.Unknown;

    /// <summary>Maps to <c>maskInput</c>.</summary>
    public ModelCapability MaskInput { get; init; } = ModelCapability.Unknown;

    /// <summary>Maps to <c>maxImagesPerCall</c>.</summary>
    public int MaxImagesPerCall { get; init; }

    /// <summary>Maps to <c>animation</c>.</summary>
    public ModelCapability Animation { get; init; } = ModelCapability.Unknown;

    /// <summary>Maps to <c>editing</c>.</summary>
    public ModelCapability Editing { get; init; } = ModelCapability.Unknown;
}

/// <summary>A catalog entry of a QuiverAI language model. Maps to the result of <c>getQuiverAILanguageModelEntry</c>.</summary>
public sealed class QuiverAILanguageModelEntry
{
    /// <summary>Maps to <c>capabilities.language</c>.</summary>
    public LanguageCapabilities Capabilities { get; init; } = new();

    /// <summary>Maps to <c>providerOptions.language</c>.</summary>
    public IReadOnlyDictionary<string, ModelCapability> ProviderOptions { get; init; } =
        new Dictionary<string, ModelCapability>();

    /// <summary>Maps to <c>requestConfig.language</c>.</summary>
    public QuiverAILanguageRequestConfig RequestConfig { get; init; } = new();
}

/// <summary>A catalog entry of a QuiverAI image model. Maps to the result of <c>getQuiverAIImageModelEntry</c>.</summary>
public sealed class QuiverAIImageModelEntry
{
    /// <summary>Maps to <c>capabilities.image</c>.</summary>
    public QuiverAIImageCapabilities Capabilities { get; init; } = new();

    /// <summary>Maps to <c>providerOptions.image</c>.</summary>
    public IReadOnlyDictionary<string, ModelCapability> ProviderOptions { get; init; } =
        new Dictionary<string, ModelCapability>();

    /// <summary>Maps to <c>requestConfig.image</c>.</summary>
    public QuiverAIImageRequestConfig RequestConfig { get; init; } = new();
}

/// <summary>QuiverAI model catalog. Maps to <c>quiverai-model-catalog.ts</c>.</summary>
public static class QuiverAIModelCatalog
{
    private const int MaxImagesPerCall = 16;

    private static readonly string[] ReasoningEfforts = ["low", "medium", "high", "xhigh"];

    private static readonly IReadOnlyDictionary<string, ImagePolicy> ImagePolicies = new Dictionary<
        string,
        ImagePolicy
    >(StringComparer.Ordinal)
    {
        ["arrow-1"] = new(4, null, false),
        ["arrow-1.0"] = new(4, null, false),
        ["arrow-1.1"] = new(4, null, false),
        ["arrow-1.1-max"] = new(16, null, false),
        ["arrow-2"] = new(16, 65536, true),
        ["arrow-2-telos"] = new(16, 65536, true),
    };

    /// <summary>Maps to <c>quiveraiLanguageModelIds</c>.</summary>
    public static IReadOnlyList<string> LanguageModelIds { get; } = ["arrow-2", "arrow-2-telos"];

    /// <summary>Maps to <c>quiveraiImageModelIds</c>.</summary>
    public static IReadOnlyList<string> ImageModelIds { get; } =
    ["arrow-1", "arrow-1.1", "arrow-1.1-max", "arrow-2", "arrow-2-telos"];

    /// <summary>Maps to <c>quiveraiRequestDefaults.language</c>.</summary>
    public static QuiverAILanguageRequestConfig LanguageRequestDefaults { get; } =
        new() { StructuredOutputs = false, ReasoningEfforts = ReasoningEfforts };

    /// <summary>Maps to <c>quiveraiRequestDefaults.image.maxImagesPerCall</c>.</summary>
    public static int ImageMaxImagesPerCall => MaxImagesPerCall;

    /// <summary>Maps to <c>quiveraiRequestDefaults.language.structuredOutputs</c>.</summary>
    internal static bool LanguageStructuredOutputs => LanguageRequestDefaults.StructuredOutputs;

    /// <summary>Maps to <c>quiveraiRequestDefaults.language.reasoningEfforts</c>.</summary>
    internal static IReadOnlyList<string> LanguageReasoningEfforts =>
        LanguageRequestDefaults.ReasoningEfforts;

    /// <summary>Returns the entry of a language model. An unknown model id gets <c>unknown</c> option support. Maps to <c>getQuiverAILanguageModelEntry</c>.</summary>
    public static QuiverAILanguageModelEntry GetQuiverAILanguageModelEntry(string modelId)
    {
        var known = LanguageModelIds.Contains(modelId);
        var optionSupport = known ? ModelCapability.Supported : ModelCapability.Unknown;
        return new QuiverAILanguageModelEntry
        {
            Capabilities = new LanguageCapabilities
            {
                Inputs = new LanguageInputCapabilities
                {
                    Image = ModelCapability.Unknown,
                    Audio = ModelCapability.Unknown,
                },
                ObjectGeneration = ModelCapability.Unsupported,
                Tools = new LanguageToolCapabilities
                {
                    Usage = ModelCapability.Supported,
                    Streaming = ModelCapability.Supported,
                },
                Reasoning = new LanguageReasoningCapabilities
                {
                    Support = ModelCapability.Supported,
                    Efforts = LanguageRequestDefaults.ReasoningEfforts,
                },
                Sampling = new LanguageSamplingCapabilities
                {
                    Temperature = ModelCapability.Unknown,
                    TopP = ModelCapability.Unknown,
                    Logprobs = ModelCapability.Unknown,
                },
            },
            ProviderOptions = new Dictionary<string, ModelCapability>
            {
                ["reasoningEffort"] = optionSupport,
                ["reasoningSummary"] = optionSupport,
            },
            RequestConfig = LanguageRequestDefaults,
        };
    }

    /// <summary>Returns the entry of an image model. An unknown model id gets <c>unknown</c> option support. Maps to <c>getQuiverAIImageModelEntry</c>.</summary>
    public static QuiverAIImageModelEntry GetQuiverAIImageModelEntry(string modelId)
    {
        var known = ImageModelIds.Contains(modelId);
        var policy = ImagePolicies.TryGetValue(modelId, out var found) ? found : null;
        var arrow2 = policy?.Arrow2 ?? false;
        var support = known ? ModelCapability.Supported : ModelCapability.Unknown;
        var editOnly = arrow2
            ? ModelCapability.When("Edit only, with SVG editing enabled for the API key.")
            : ModelCapability.Unsupported;

        var options = new Dictionary<string, ModelCapability>(StringComparer.Ordinal);
        foreach (var key in QuiverAIProviderOptions.Image.Keys)
        {
            options[key] = support;
        }

        options["operation"] = ModelCapability.When(
            "Generate and vectorize use image models; animate and edit require Arrow 2 or Arrow 2 Telos."
        );
        options["instructions"] = ModelCapability.When("Generate only.");
        options["attributes"] = ModelCapability.When("Generate and vectorize only.");
        options["topP"] = ModelCapability.When("Generate and vectorize only.");
        options["presencePenalty"] = ModelCapability.When("Generate and vectorize only.");
        options["autoCrop"] = ModelCapability.When("Vectorize only.");
        options["targetSize"] = ModelCapability.When("Vectorize only.");
        options["referenceImages"] = editOnly;
        options["maxReviewSteps"] = editOnly;
        options["orchestratorMaxOutputTokens"] = editOnly;
        options["shallowMaxOutputTokens"] = editOnly;

        return new QuiverAIImageModelEntry
        {
            Capabilities = new QuiverAIImageCapabilities
            {
                SizesDescription =
                    "SVG canvas dimensions are set through attributes; size and aspectRatio are ignored.",
                FileInput = support,
                MaskInput = known ? ModelCapability.Unsupported : ModelCapability.Unknown,
                MaxImagesPerCall = MaxImagesPerCall,
                Animation = ModelCapability.From(arrow2),
                Editing = arrow2
                    ? ModelCapability.When(
                        "Requires the API key's model inventory to include the svg_edit operation."
                    )
                    : ModelCapability.Unsupported,
            },
            ProviderOptions = options,
            RequestConfig = new QuiverAIImageRequestConfig
            {
                GenerateReferenceLimit = policy?.GenerateReferenceLimit ?? 16,
                MaxOutputTokens = policy?.MaxOutputTokens,
                EditModel = arrow2,
            },
        };
    }

    /// <summary>The reasoning efforts a language model accepts. Maps to <c>getQuiverAILanguageModelEntry(modelId).capabilities.language.reasoning.efforts</c>.</summary>
    internal static IReadOnlyList<string> GetQuiverAILanguageModelReasoningEfforts(string modelId)
    {
        return GetQuiverAILanguageModelEntry(modelId).Capabilities.Reasoning.Efforts!;
    }

    private sealed record ImagePolicy(
        int GenerateReferenceLimit,
        int? MaxOutputTokens,
        bool Arrow2
    );
}
