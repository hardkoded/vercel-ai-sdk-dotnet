// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.HeyGen;

/// <summary>Known options accepted under <c>providerOptions.heygen</c>. Upstream <c>heyGenProviderOptions</c>.</summary>
public static class HeyGenProviderOptions
{
    /// <summary>Video options (upstream <c>heyGenProviderOptions.video</c>), keyed by option name.</summary>
    public static readonly IReadOnlyDictionary<string, ProviderOptionDefinition> Video =
        new Dictionary<string, ProviderOptionDefinition>
        {
            ["mode"] = new("Inferred from the supplied image and references when omitted."),
            ["resolution"] = new(
                "Output size class. Takes precedence over the standard resolution option."
            ),
            ["promptEnhancement"] = new(
                "Defaults to quality. Use disabled to preserve the original prompt."
            ),
            ["image"] = new(
                "First frame, including an existing HeyGen asset. Cannot be combined with a standard image."
            ),
            ["referenceImages"] = new(
                "Additional references, appended after standard inputReferences of the same kind."
            ),
            ["referenceVideos"] = new("referenceVideos"),
            ["referenceAudio"] = new("referenceAudio"),
        };
}

/// <summary>Known HeyGen model ids, the adapter limits and the capability catalog. Upstream <c>heygen-model-catalog.ts</c>.</summary>
public static class HeyGenModelCatalog
{
    /// <summary>Known video model ids.</summary>
    public static readonly IReadOnlyList<string> VideoModelIds = ["heygen-video-1"];

    /// <summary>Known decision model ids. Other ids are passed through unchanged.</summary>
    public static readonly IReadOnlyList<string> DecisionModelIds =
    [
        "heygen-decision-1",
        "heygen-decision-1-turbo",
    ];

    /// <summary>Question types HeyGen Decision supports. Upstream <c>heyGenDecisionModelLimits.supportedQuestionTypes</c>.</summary>
    public static readonly IReadOnlyList<string> DecisionSupportedQuestionTypes =
    [
        "choice",
        "score",
        "boolean",
    ];

    /// <summary>Most questions per request. Upstream <c>heyGenDecisionModelLimits.maxQuestions</c>.</summary>
    public const int DecisionMaxQuestions = 400;

    /// <summary>Most options of a choice question. Upstream <c>heyGenDecisionModelLimits.maxChoiceOptions</c>.</summary>
    public const int DecisionMaxChoiceOptions = 255;

    /// <summary>Most levels of a score question. Upstream <c>heyGenDecisionModelLimits.maxScoreLevels</c>.</summary>
    public const int DecisionMaxScoreLevels = 10;

    /// <summary>Most images per request. Upstream <c>heyGenDecisionModelLimits.maxImages</c>.</summary>
    public const int DecisionMaxImages = 4;

    /// <summary>Most videos per request. Upstream <c>heyGenDecisionModelLimits.maxVideos</c>.</summary>
    public const int DecisionMaxVideos = 1;

    /// <summary>
    /// Entries of every known model id. Upstream <c>heyGenModelCatalog</c>; renamed because C# does not allow a member named like its class.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, ModelEntry<object>> Entries = BuildEntries();

    /// <summary>Returns the catalog entry of a video model. Upstream <c>getHeyGenVideoModelEntry</c>.</summary>
    public static ModelEntry<object> GetHeyGenVideoModelEntry(string modelId)
    {
        var options = new Dictionary<string, ModelCapability>();
        foreach (var key in HeyGenProviderOptions.Video.Keys)
        {
            options[key] = ModelCapability.Unknown;
        }

        return new ModelEntry<object>
        {
            Capabilities = new ModelCapabilities
            {
                Video = new VideoCapabilities
                {
                    Inputs = new VideoInputCapabilities
                    {
                        Text = ModelCapability.Supported,
                        Image = ModelCapability.Supported,
                        FirstFrame = ModelCapability.Supported,
                        LastFrame = ModelCapability.Unsupported,
                        ReferenceImage = ModelCapability.Supported,
                        ReferenceVideo = ModelCapability.Supported,
                    },
                    GenerateAudio = ModelCapability.When(
                        "Audio is always generated; disabling it is unsupported."
                    ),
                    AspectRatio = ModelCapability.When(
                        "Supported outside reference-to-video mode."
                    ),
                    Resolution = ModelCapability.When("Supported outside reference-to-video mode."),
                    Duration = ModelCapability.Supported,
                    Fps = ModelCapability.When("Fixed at 24 fps."),
                    Seed = ModelCapability.Supported,
                    MaxVideosPerCall = 1,
                },
            },
            ProviderOptions = new ModelProviderOptions { Video = options },
        };
    }

    /// <summary>Returns the catalog entry of a decision model. Upstream <c>getHeyGenDecisionModelEntry</c>.</summary>
    public static ModelEntry<object> GetHeyGenDecisionModelEntry(string modelId)
    {
        return new ModelEntry<object>
        {
            Capabilities = new ModelCapabilities
            {
                Decision = new DecisionCapabilities
                {
                    QuestionTypes = new DecisionQuestionTypeCapabilities
                    {
                        Boolean = ModelCapability.Supported,
                        Choice = ModelCapability.Supported,
                        Score = ModelCapability.Supported,
                        Box = ModelCapability.Unsupported,
                    },
                    Inputs = new DecisionInputCapabilities { Image = ModelCapability.Supported },
                    Reasoning = ModelCapability.Unsupported,
                    MaxOutputTokens = ModelCapability.Unsupported,
                },
            },
        };
    }

    private static Dictionary<string, ModelEntry<object>> BuildEntries()
    {
        var entries = new Dictionary<string, ModelEntry<object>>(StringComparer.Ordinal);
        foreach (var id in VideoModelIds)
        {
            entries[id] = GetHeyGenVideoModelEntry(id);
        }

        foreach (var id in DecisionModelIds)
        {
            entries[id] = GetHeyGenDecisionModelEntry(id);
        }

        return entries;
    }
}
