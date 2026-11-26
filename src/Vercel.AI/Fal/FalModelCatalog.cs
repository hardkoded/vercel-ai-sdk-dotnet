// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;

namespace Vercel.AI.Fal;

/// <summary>Maps to <c>requestConfig.image</c> of a Fal image model entry. Upstream declares it as an empty object.</summary>
internal sealed class FalImageRequestConfig { }

/// <summary>Maps to <c>requestConfig.video</c> of the Fal video model entry.</summary>
internal sealed class FalVideoRequestConfig
{
    /// <summary>The model id without its <c>fal-ai/</c> or <c>fal/</c> prefix. Maps to <c>normalizedModelId</c>.</summary>
    public string NormalizedModelId { get; init; } = "";
}

/// <summary>Maps to the <c>requestConfig</c> of a Fal model entry.</summary>
internal sealed class FalRequestConfig
{
    /// <summary>Maps to <c>requestConfig.image</c>.</summary>
    public FalImageRequestConfig? Image { get; init; }

    /// <summary>Maps to <c>requestConfig.video</c>.</summary>
    public FalVideoRequestConfig? Video { get; init; }
}

/// <summary>Known options accepted under <c>providerOptions.fal</c>. Maps to <c>falProviderOptions</c>.</summary>
internal static class FalProviderOptions
{
    /// <summary>Maps to <c>falProviderOptions.image</c>, keyed by option name.</summary>
    public static readonly IReadOnlyDictionary<string, ProviderOptionDefinition> Image =
        new Dictionary<string, ProviderOptionDefinition>(StringComparer.Ordinal)
        {
            ["imageUrl"] = new("imageUrl"),
            ["maskUrl"] = new("maskUrl"),
            ["guidanceScale"] = new("guidanceScale"),
            ["numInferenceSteps"] = new("numInferenceSteps"),
            ["enableSafetyChecker"] = new("enableSafetyChecker"),
            ["outputFormat"] = new("outputFormat"),
            ["syncMode"] = new("syncMode"),
            ["strength"] = new("strength"),
            ["acceleration"] = new("acceleration"),
            ["safetyTolerance"] = new("safetyTolerance"),
            ["useMultipleImages"] = new(
                "When true, converts multiple input images to `image_urls` array instead of `image_url` string."
            ),
            ["image_url"] = new("image_url"),
            ["mask_url"] = new("mask_url"),
            ["guidance_scale"] = new("guidance_scale"),
            ["num_inference_steps"] = new("num_inference_steps"),
            ["enable_safety_checker"] = new("enable_safety_checker"),
            ["output_format"] = new("output_format"),
            ["sync_mode"] = new("sync_mode"),
            ["safety_tolerance"] = new("safety_tolerance"),
        };

    /// <summary>Maps to <c>falProviderOptions.speech</c>, keyed by option name.</summary>
    public static readonly IReadOnlyDictionary<string, ProviderOptionDefinition> Speech =
        new Dictionary<string, ProviderOptionDefinition>(StringComparer.Ordinal)
        {
            ["voice_setting"] = new("voice_setting"),
            ["audio_setting"] = new("audio_setting"),
            ["language_boost"] = new("language_boost"),
            ["pronunciation_dict"] = new("pronunciation_dict"),
        };

    /// <summary>Maps to <c>falProviderOptions.transcription</c>, keyed by option name.</summary>
    public static readonly IReadOnlyDictionary<string, ProviderOptionDefinition> Transcription =
        new Dictionary<string, ProviderOptionDefinition>(StringComparer.Ordinal)
        {
            ["language"] = new(
                "Language of the audio file. If set to null, the language will be automatically detected. Defaults to null. If translate is selected as the task, the audio will be translated to English, regardless of the language selected."
            ),
            ["diarize"] = new("Whether to diarize the audio file. Defaults to true."),
            ["chunkLevel"] = new(
                "Level of the chunks to return. Either segment or word. Default value: \"segment\""
            ),
            ["version"] = new(
                "Version of the model to use. All of the models are the Whisper large variant. Default value: \"3\""
            ),
            ["batchSize"] = new("Default value: 64"),
            ["numSpeakers"] = new(
                "Number of speakers in the audio file. Defaults to null. If not provided, the number of speakers will be automatically detected."
            ),
        };

    /// <summary>Maps to <c>falProviderOptions.video</c>, keyed by option name.</summary>
    public static readonly IReadOnlyDictionary<string, ProviderOptionDefinition> Video =
        new Dictionary<string, ProviderOptionDefinition>(StringComparer.Ordinal)
        {
            ["loop"] = new("loop"),
            ["motionStrength"] = new("motionStrength"),
            ["resolution"] = new("resolution"),
            ["negativePrompt"] = new("negativePrompt"),
            ["promptOptimizer"] = new("promptOptimizer"),
        };
}

/// <summary>Maps to <c>fal-model-catalog.ts</c>.</summary>
internal static class FalModelCatalog
{
    private static readonly Regex FalAiPrefix = new(
        "^fal-ai/",
        RegexOptions.None,
        TimeSpan.FromSeconds(1)
    );
    private static readonly Regex FalPrefix = new(
        "^fal/",
        RegexOptions.None,
        TimeSpan.FromSeconds(1)
    );

    /// <summary>Maps to <c>imageModelIds</c>.</summary>
    internal static readonly IReadOnlyList<string> FalImageModelIds =
    [
        "fal-ai/aura-sr",
        "fal-ai/bria/background/remove",
        "fal-ai/bria/eraser",
        "fal-ai/bria/product-shot",
        "fal-ai/bria/reimagine",
        "bria/text-to-image/3.2",
        "fal-ai/bria/text-to-image/base",
        "fal-ai/bria/text-to-image/fast",
        "fal-ai/bria/text-to-image/hd",
        "fal-ai/bytedance/dreamina/v3.1/text-to-image",
        "fal-ai/ccsr",
        "fal-ai/clarity-upscaler",
        "fal-ai/creative-upscaler",
        "fal-ai/esrgan",
        "fal-ai/flux-general",
        "fal-ai/flux-general/differential-diffusion",
        "fal-ai/flux-general/image-to-image",
        "fal-ai/flux-general/inpainting",
        "fal-ai/flux-general/rf-inversion",
        "fal-ai/flux-kontext-lora/text-to-image",
        "fal-ai/flux-lora",
        "fal-ai/flux-lora/image-to-image",
        "fal-ai/flux-lora/inpainting",
        "fal-ai/flux-pro/kontext",
        "fal-ai/flux-pro/kontext/max",
        "fal-ai/flux-pro/v1.1",
        "fal-ai/flux-pro/v1.1-ultra",
        "fal-ai/flux-pro/v1.1-ultra-finetuned",
        "fal-ai/flux-pro/v1.1-ultra/redux",
        "fal-ai/flux-pro/v1.1/redux",
        "fal-ai/flux/dev",
        "fal-ai/flux/dev/image-to-image",
        "fal-ai/flux/dev/redux",
        "fal-ai/flux/krea",
        "fal-ai/flux/krea/image-to-image",
        "fal-ai/flux/krea/redux",
        "fal-ai/flux/schnell",
        "fal-ai/flux/schnell/redux",
        "fal-ai/ideogram/character",
        "fal-ai/ideogram/character/edit",
        "fal-ai/ideogram/character/remix",
        "fal-ai/imagen4/preview",
        "fal-ai/luma-photon",
        "fal-ai/luma-photon/flash",
        "fal-ai/object-removal",
        "fal-ai/omnigen-v2",
        "fal-ai/qwen-image",
        "fal-ai/recraft/v3/text-to-image",
        "fal-ai/recraft/v3/image-to-image",
        "fal-ai/sana/sprint",
        "fal-ai/sana/v1.5/4.8b",
        "fal-ai/sana/v1.5/1.6b",
        "fal-ai/sky-raccoon",
        "fal-ai/wan/v2.2-5b/text-to-image",
        "fal-ai/wan/v2.2-a14b/text-to-image",
        "fal-ai/fashn/tryon/v1.6",
    ];

    /// <summary>Maps to <c>speechModelIds</c>.</summary>
    internal static readonly IReadOnlyList<string> FalSpeechModelIds =
    [
        "fal-ai/minimax/voice-clone",
        "fal-ai/minimax/voice-design",
        "fal-ai/dia-tts/voice-clone",
        "fal-ai/minimax/speech-02-hd",
        "fal-ai/minimax/speech-02-turbo",
        "fal-ai/dia-tts",
        "resemble-ai/chatterboxhd/text-to-speech",
    ];

    /// <summary>Maps to <c>transcriptionModelIds</c>.</summary>
    internal static readonly IReadOnlyList<string> FalTranscriptionModelIds = ["whisper", "wizper"];

    /// <summary>Maps to <c>videoModelIds</c>.</summary>
    internal static readonly IReadOnlyList<string> FalVideoModelIds =
    [
        "luma-dream-machine",
        "luma-ray-2",
        "luma-ray-2-flash",
        "minimax-video",
        "minimax-video-01",
        "hunyuan-video",
    ];

    /// <summary>Maps to <c>imageDescriptions</c>.</summary>
    private static readonly IReadOnlyDictionary<string, string> ImageDescriptions = new Dictionary<
        string,
        string
    >(StringComparer.Ordinal)
    {
        ["fal-ai/flux/dev"] = "FLUX.1 [dev] model for high-quality image generation",
        ["fal-ai/flux-pro/kontext"] =
            "FLUX.1 Kontext [pro] handles both text and reference images as inputs, enabling targeted edits and complex transformations",
        ["fal-ai/flux-pro/kontext/max"] =
            "FLUX.1 Kontext [max] with improved prompt adherence and typography generation",
        ["fal-ai/flux-lora"] = "Super fast endpoint for FLUX.1 with LoRA support",
        ["fal-ai/ideogram/character"] =
            "Generate consistent character appearances across multiple images. Maintain facial features, proportions, and distinctive traits",
        ["fal-ai/qwen-image"] =
            "Qwen-Image foundation model with significant advances in complex text rendering and precise image editing",
        ["fal-ai/omnigen-v2"] =
            "Unified image generation model for Image Editing, Personalized Image Generation, Virtual Try-On, Multi Person Generation and more",
        ["fal-ai/bytedance/dreamina/v3.1/text-to-image"] =
            "Dreamina showcases superior picture effects with improvements in aesthetics, precise and diverse styles, and rich details",
        ["fal-ai/recraft/v3/text-to-image"] =
            "SOTA in image generation with vector art and brand style capabilities",
        ["fal-ai/wan/v2.2-a14b/text-to-image"] =
            "High-resolution, photorealistic images with fine-grained detail",
    };

    /// <summary>Maps to <c>speechDescriptions</c>.</summary>
    private static readonly IReadOnlyDictionary<string, string> SpeechDescriptions = new Dictionary<
        string,
        string
    >(StringComparer.Ordinal)
    {
        ["fal-ai/minimax/voice-clone"] =
            "Clone a voice from a sample audio and generate speech from text prompts",
        ["fal-ai/minimax/voice-design"] =
            "Design a personalized voice from a text description and generate speech from text prompts",
        ["fal-ai/dia-tts/voice-clone"] =
            "Clone dialog voices from a sample audio and generate dialogs from text prompts",
        ["fal-ai/minimax/speech-02-hd"] = "Generate speech from text prompts and different voices",
        ["fal-ai/minimax/speech-02-turbo"] =
            "Generate fast speech from text prompts and different voices",
        ["fal-ai/dia-tts"] =
            "Directly generates realistic dialogue from transcripts with audio conditioning for emotion control. Produces natural nonverbals like laughter and throat clearing",
        ["resemble-ai/chatterboxhd/text-to-speech"] =
            "Generate expressive, natural speech with Resemble AI's Chatterbox. Features unique emotion control, instant voice cloning from short audio, and built-in watermarking",
    };

    /// <summary>Maps to <c>falModelCatalog</c>. Renamed because C# does not allow a member named like its class.</summary>
    internal static readonly IReadOnlyDictionary<string, ModelEntry<FalRequestConfig>> Entries =
        BuildEntries();

    /// <summary>Returns the entry of an image model. Maps to <c>getFalImageModelEntry</c>.</summary>
    internal static ModelEntry<FalRequestConfig> GetFalImageModelEntry(string modelId)
    {
        var supportsFileInputs = modelId switch
        {
            "fal-ai/flux-2/edit"
            or "fal-ai/flux-pro/kontext"
            or "fal-ai/flux-pro/kontext/max"
            or "fal-ai/flux-general/image-to-image"
            or "fal-ai/flux-general/inpainting"
            or "fal-ai/flux-lora/image-to-image"
            or "fal-ai/flux-lora/inpainting"
            or "fal-ai/flux/dev/image-to-image"
            or "fal-ai/flux/krea/image-to-image"
            or "fal-ai/recraft/v3/image-to-image" => ModelCapability.Supported,
            "bria/text-to-image/3.2"
            or "fal-ai/bria/text-to-image/base"
            or "fal-ai/bria/text-to-image/fast"
            or "fal-ai/bria/text-to-image/hd"
            or "fal-ai/bytedance/dreamina/v3.1/text-to-image"
            or "fal-ai/flux-kontext-lora/text-to-image"
            or "fal-ai/recraft/v3/text-to-image"
            or "fal-ai/wan/v2.2-5b/text-to-image"
            or "fal-ai/wan/v2.2-a14b/text-to-image" => ModelCapability.Unsupported,
            _ => ModelCapability.Unknown,
        };
        var supportsMaskInputs =
            modelId is "fal-ai/flux-general/inpainting" or "fal-ai/flux-lora/inpainting"
                ? ModelCapability.Supported
            : supportsFileInputs.IsUnknown ? ModelCapability.Unknown
            : ModelCapability.Unsupported;

        return new ModelEntry<FalRequestConfig>
        {
            Capabilities = new ModelCapabilities
            {
                Image = new ImageCapabilities
                {
                    Description = ImageDescriptions.TryGetValue(modelId, out var description)
                        ? description
                        : null,
                    SizesDescription =
                        "Accepted size or aspect ratio depends on the model and operation.",
                    FileInput = supportsFileInputs,
                    MaskInput = supportsMaskInputs,
                    MaxImagesPerCall = 1,
                },
            },
            ProviderOptions = new ModelProviderOptions
            {
                Image = UnknownOptions(FalProviderOptions.Image.Keys),
            },
            RequestConfig = new FalRequestConfig { Image = new FalImageRequestConfig() },
        };
    }

    /// <summary>Returns the entry of a speech model. Maps to <c>getFalSpeechModelEntry</c>.</summary>
    internal static ModelEntry<FalRequestConfig> GetFalSpeechModelEntry(string modelId)
    {
        return new ModelEntry<FalRequestConfig>
        {
            Capabilities = new ModelCapabilities
            {
                Speech = new SpeechCapabilities
                {
                    Description = SpeechDescriptions.TryGetValue(modelId, out var description)
                        ? description
                        : null,
                    Instructions = ModelCapability.Unsupported,
                },
            },
            ProviderOptions = new ModelProviderOptions
            {
                Speech = UnknownOptions(FalProviderOptions.Speech.Keys),
            },
        };
    }

    /// <summary>Returns the entry of a transcription model. Maps to <c>getFalTranscriptionModelEntry</c>.</summary>
    internal static ModelEntry<FalRequestConfig> GetFalTranscriptionModelEntry(string modelId)
    {
        return new ModelEntry<FalRequestConfig>
        {
            Capabilities = new ModelCapabilities
            {
                Transcription = new TranscriptionCapabilities
                {
                    Transcription = ModelCapability.Supported,
                    Streaming = ModelCapability.Unsupported,
                    Duration = ModelCapability.Supported,
                    Segments = ModelCapability.Supported,
                    Language = ModelCapability.Supported,
                },
            },
            ProviderOptions = new ModelProviderOptions
            {
                Transcription = UnknownOptions(FalProviderOptions.Transcription.Keys),
            },
        };
    }

    /// <summary>Returns the entry of a video model. Maps to <c>getFalVideoModelEntry</c>.</summary>
    internal static ModelEntry<FalRequestConfig> GetFalVideoModelEntry(string modelId)
    {
        var endpoint = ModelCapability.When("Supported by the selected model endpoint.");
        return new ModelEntry<FalRequestConfig>
        {
            Capabilities = new ModelCapabilities
            {
                Video = new VideoCapabilities
                {
                    Inputs = new VideoInputCapabilities
                    {
                        Text = ModelCapability.Supported,
                        Image = endpoint,
                        FirstFrame = ModelCapability.Unsupported,
                        LastFrame = ModelCapability.Unsupported,
                        ReferenceImage = ModelCapability.Unsupported,
                        ReferenceVideo = ModelCapability.Unsupported,
                    },
                    GenerateAudio = ModelCapability.Unsupported,
                    AspectRatio = endpoint,
                    Resolution = ModelCapability.When(
                        "Configured with providerOptions.fal.resolution."
                    ),
                    Duration = endpoint,
                    Fps = ModelCapability.Unsupported,
                    Seed = endpoint,
                    MaxVideosPerCall = null,
                },
            },
            ProviderOptions = new ModelProviderOptions
            {
                Video = UnknownOptions(FalProviderOptions.Video.Keys),
            },
            RequestConfig = new FalRequestConfig
            {
                Video = new FalVideoRequestConfig
                {
                    NormalizedModelId = FalPrefix.Replace(FalAiPrefix.Replace(modelId, ""), ""),
                },
            },
        };
    }

    private static Dictionary<string, ModelCapability> UnknownOptions(IEnumerable<string> keys)
    {
        var options = new Dictionary<string, ModelCapability>(StringComparer.Ordinal);
        foreach (var key in keys)
        {
            options[key] = ModelCapability.Unknown;
        }

        return options;
    }

    private static Dictionary<string, ModelEntry<FalRequestConfig>> BuildEntries()
    {
        var entries = new Dictionary<string, ModelEntry<FalRequestConfig>>(StringComparer.Ordinal);
        foreach (var id in FalImageModelIds)
        {
            entries[id] = GetFalImageModelEntry(id);
        }

        foreach (var id in FalSpeechModelIds)
        {
            entries[id] = GetFalSpeechModelEntry(id);
        }

        foreach (var id in FalTranscriptionModelIds)
        {
            entries[id] = GetFalTranscriptionModelEntry(id);
        }

        foreach (var id in FalVideoModelIds)
        {
            entries[id] = GetFalVideoModelEntry(id);
        }

        foreach (var id in ImageDescriptions.Keys)
        {
            entries[id] = GetFalImageModelEntry(id);
        }

        entries["fal-ai/flux-2/edit"] = GetFalImageModelEntry("fal-ai/flux-2/edit");
        return entries;
    }
}
