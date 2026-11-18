// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;

namespace Vercel.AI.DeepSeek;

/// <summary>Known options accepted under <c>providerOptions.deepseek</c>. Upstream <c>deepseekProviderOptions</c>.</summary>
public static class DeepSeekProviderOptions
{
    /// <summary>Language options (upstream <c>deepseekProviderOptions.language</c>), keyed by option name.</summary>
    public static readonly IReadOnlyDictionary<string, ProviderOptionDefinition> Language =
        new Dictionary<string, ProviderOptionDefinition>
        {
            ["logprobs"] = new("Return token log probabilities."),
            ["topLogprobs"] = new("Number of token log probabilities to return."),
            ["userId"] = new("End-user identifier for request isolation."),
            ["thinking"] = new("Enable or disable thinking."),
            ["reasoningEffort"] = new("Thinking strength for reasoning models."),
            ["strictJsonSchema"] = new(
                "Use strict JSON schema validation on supporting endpoints."
            ),
        };
}

/// <summary>What the serving endpoint supports. Upstream <c>DeepSeekEndpointOptions</c>.</summary>
public sealed class DeepSeekEndpointOptions
{
    /// <summary>Upstream <c>supportsAssistantPrefixCompletion</c>.</summary>
    public bool? SupportsAssistantPrefixCompletion { get; init; }

    /// <summary>Upstream <c>supportsStrictToolCalls</c>.</summary>
    public bool? SupportsStrictToolCalls { get; init; }

    /// <summary>Upstream <c>supportsPenaltySampling</c>.</summary>
    public bool? SupportsPenaltySampling { get; init; }

    /// <summary>Upstream <c>supportsThinking</c>.</summary>
    public bool? SupportsThinking { get; init; }

    /// <summary>Upstream <c>supportsStructuredOutputs</c>.</summary>
    public bool? SupportsStructuredOutputs { get; init; }
}

/// <summary>Tool support of a DeepSeek language model. Upstream <c>LanguageCapabilities.tools</c>.</summary>
public sealed class DeepSeekToolCapabilities
{
    /// <summary>Upstream <c>usage</c>.</summary>
    public ModelCapability Usage { get; init; } = ModelCapability.Unknown;

    /// <summary>Upstream <c>streaming</c>.</summary>
    public ModelCapability Streaming { get; init; } = ModelCapability.Unknown;

    /// <summary>Upstream <c>strict</c>. <c>Unknown</c> unless the endpoint declares strict tool calls.</summary>
    public ModelCapability Strict { get; init; } = ModelCapability.Unknown;
}

/// <summary>Reasoning support of a DeepSeek language model. Upstream <c>LanguageCapabilities.reasoning</c>.</summary>
public sealed class DeepSeekReasoningCapabilities
{
    /// <summary>Upstream <c>support</c>.</summary>
    public ModelCapability Support { get; init; } = ModelCapability.Unknown;

    /// <summary>Upstream <c>efforts</c>. Null when not declared.</summary>
    public IReadOnlyList<string>? Efforts { get; init; }

    /// <summary>Upstream <c>configurable</c>.</summary>
    public bool Configurable { get; init; }

    /// <summary>Upstream <c>enabledByDefault</c>.</summary>
    public bool EnabledByDefault { get; init; }

    /// <summary>Upstream <c>assistantHistory</c>: whether assistant turns must carry <c>reasoning_content</c>.</summary>
    public bool AssistantHistory { get; init; }
}

/// <summary>Sampling support of a DeepSeek language model. Upstream <c>LanguageCapabilities.sampling</c>.</summary>
public sealed class DeepSeekSamplingCapabilities
{
    /// <summary>Upstream <c>temperature</c>.</summary>
    public ModelCapability Temperature { get; init; } = ModelCapability.Unknown;

    /// <summary>Upstream <c>topP</c>.</summary>
    public ModelCapability TopP { get; init; } = ModelCapability.Unknown;

    /// <summary>Upstream <c>logprobs</c>.</summary>
    public ModelCapability Logprobs { get; init; } = ModelCapability.Unknown;

    /// <summary>Upstream <c>penalties</c>.</summary>
    public bool Penalties { get; init; }

    /// <summary>Upstream <c>topK</c>. Always false.</summary>
    public bool TopK { get; init; }

    /// <summary>Upstream <c>seed</c>. Always false.</summary>
    public bool Seed { get; init; }
}

/// <summary>Structured output support of a DeepSeek language model. Upstream <c>LanguageCapabilities.structuredOutput</c>.</summary>
public sealed class DeepSeekStructuredOutputCapabilities
{
    /// <summary>Upstream <c>jsonSchema</c>.</summary>
    public bool JsonSchema { get; init; }
}

/// <summary>Endpoint support of a DeepSeek language model. Upstream <c>LanguageCapabilities.endpoints</c>.</summary>
public sealed class DeepSeekEndpointCapabilities
{
    /// <summary>Upstream <c>assistantPrefixCompletion</c>.</summary>
    public bool AssistantPrefixCompletion { get; init; }
}

/// <summary>The language capabilities of a DeepSeek model. Upstream <c>LanguageCapabilities</c> of <c>deepseek-model-catalog.ts</c>.</summary>
public sealed class DeepSeekLanguageCapabilities
{
    /// <summary>Upstream <c>textGeneration</c>.</summary>
    public ModelCapability TextGeneration { get; init; } = ModelCapability.Unknown;

    /// <summary>Upstream <c>inputs</c>.</summary>
    public LanguageInputCapabilities Inputs { get; init; } = new();

    /// <summary>Upstream <c>objectGeneration</c>.</summary>
    public ModelCapability ObjectGeneration { get; init; } = ModelCapability.Unknown;

    /// <summary>Upstream <c>tools</c>.</summary>
    public DeepSeekToolCapabilities Tools { get; init; } = new();

    /// <summary>Upstream <c>reasoning</c>.</summary>
    public DeepSeekReasoningCapabilities Reasoning { get; init; } = new();

    /// <summary>Upstream <c>sampling</c>.</summary>
    public DeepSeekSamplingCapabilities Sampling { get; init; } = new();

    /// <summary>Upstream <c>structuredOutput</c>.</summary>
    public DeepSeekStructuredOutputCapabilities StructuredOutput { get; init; } = new();

    /// <summary>Upstream <c>endpoints</c>.</summary>
    public DeepSeekEndpointCapabilities Endpoints { get; init; } = new();
}

/// <summary>A catalog entry of a DeepSeek language model. Upstream <c>DeepSeekLanguageModelEntry</c>.</summary>
public sealed class DeepSeekLanguageModelEntry
{
    /// <summary>Upstream <c>capabilities.language</c>.</summary>
    public DeepSeekLanguageCapabilities Language { get; init; } = new();

    /// <summary>Upstream <c>providerOptions.language</c>: support of each option in <see cref="DeepSeekProviderOptions.Language"/>.</summary>
    public IReadOnlyDictionary<string, ModelCapability> ProviderOptions { get; init; } =
        new Dictionary<string, ModelCapability>();
}

/// <summary>Known DeepSeek model ids per endpoint. Upstream <c>DeepSeekModelIds</c>.</summary>
public static class DeepSeekModelIds
{
    /// <summary>Known Chat Completions model ids. Upstream <c>DeepSeekModelIds['chat']</c>.</summary>
    public static IReadOnlyList<string> Chat { get; } =
    [
        .. DeepSeekModelCatalog.LanguageDefinitions
            .Where(d => d.Endpoints.Contains("chat"))
            .Select(d => d.ModelId),
    ];

    /// <summary>Known Responses model ids. Upstream <c>DeepSeekModelIds['responses']</c>.</summary>
    public static IReadOnlyList<string> Responses { get; } =
    [
        .. DeepSeekModelCatalog.LanguageDefinitions
            .Where(d => d.Endpoints.Contains("responses"))
            .Select(d => d.ModelId),
    ];
}

/// <summary>DeepSeek model catalog. Upstream <c>deepseek-model-catalog.ts</c>.</summary>
public static class DeepSeekModelCatalog
{
    // Legacy aliases and pre-V4 IDs retain their previous protocol. Unknown IDs inherit the current protocol,
    // including assistant reasoning history.
    private static readonly Regex LegacyModelId = new(
        "(^|[/.])deepseek-(?:chat|reasoner|v[1-3](?:\\.\\d+)?)(?=[-/:.]|\\z)",
        RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100)
    );

    // Preserve the provider page's model set and its image-input exception. Upstream languageDefinitions.
    internal static (string ModelId, string[] Endpoints, bool Image)[] LanguageDefinitions { get; } =
    [
        ("deepseek-flash", ["chat", "responses"], false),
        ("deepseek-v4-flash", ["chat"], false),
        ("deepseek-v4-pro", ["chat", "responses"], false),
        ("deepseek-v4-flash-vision-exp", ["chat"], true),
    ];

    /// <summary>
    /// Entries of every known model id. Upstream <c>deepseekModelCatalog</c>; renamed because C# does not allow a member
    /// named like its class.
    /// </summary>
    public static IReadOnlyDictionary<string, DeepSeekLanguageModelEntry> Entries { get; } =
        LanguageDefinitions.ToDictionary(
            d => d.ModelId,
            d => CreateLanguageModelEntry(d.ModelId, d.Image),
            StringComparer.Ordinal
        );

    /// <summary>Returns the entry of a model on an endpoint. Upstream <c>getDeepSeekLanguageModelEntry</c>.</summary>
    public static DeepSeekLanguageModelEntry GetDeepSeekLanguageModelEntry(
        string modelId,
        DeepSeekEndpointOptions? endpoint = null
    )
    {
        endpoint ??= new DeepSeekEndpointOptions();
        var entry = Entries.TryGetValue(modelId, out var known)
            ? known
            : CreateLanguageModelEntry(modelId, null);
        var language = entry.Language;
        return new DeepSeekLanguageModelEntry
        {
            ProviderOptions = entry.ProviderOptions,
            Language = new DeepSeekLanguageCapabilities
            {
                TextGeneration = language.TextGeneration,
                Inputs = language.Inputs,
                ObjectGeneration = language.ObjectGeneration,
                Tools = new DeepSeekToolCapabilities
                {
                    Usage = language.Tools.Usage,
                    Streaming = language.Tools.Streaming,
                    Strict = endpoint.SupportsStrictToolCalls is { } strict
                        ? ModelCapability.From(strict)
                        : ModelCapability.Unknown,
                },
                Reasoning = new DeepSeekReasoningCapabilities
                {
                    Support = language.Reasoning.Support,
                    Efforts = language.Reasoning.Efforts,
                    Configurable = endpoint.SupportsThinking != false,
                    EnabledByDefault = language.Reasoning.EnabledByDefault,
                    AssistantHistory = language.Reasoning.AssistantHistory,
                },
                Sampling = new DeepSeekSamplingCapabilities
                {
                    Temperature = language.Sampling.Temperature,
                    TopP = language.Sampling.TopP,
                    Logprobs = language.Sampling.Logprobs,
                    Penalties = endpoint.SupportsPenaltySampling == true,
                },
                StructuredOutput = new DeepSeekStructuredOutputCapabilities
                {
                    JsonSchema = endpoint.SupportsStructuredOutputs == true,
                },
                Endpoints = new DeepSeekEndpointCapabilities
                {
                    AssistantPrefixCompletion = endpoint.SupportsAssistantPrefixCompletion == true,
                },
            },
        };
    }

    private static DeepSeekLanguageModelEntry CreateLanguageModelEntry(string modelId, bool? image)
    {
        // Legacy aliases and pre-V4 IDs retain their previous protocol. Unknown IDs
        // inherit the current protocol, including assistant reasoning history.
        var assistantHistory = !LegacyModelId.IsMatch(modelId);
        var support = image is not null ? ModelCapability.Supported : ModelCapability.Unknown;
        var thinkingOnly = ModelCapability.When("Only effective when thinking is disabled.");
        var language = new DeepSeekLanguageCapabilities
        {
            TextGeneration = support,
            Inputs = new LanguageInputCapabilities
            {
                Image = image is { } hasImage
                    ? ModelCapability.From(hasImage)
                    : ModelCapability.Unknown,
                Audio = ModelCapability.Unsupported,
            },
            ObjectGeneration = support,
            Tools = new DeepSeekToolCapabilities { Usage = support, Streaming = support },
            Reasoning = new DeepSeekReasoningCapabilities
            {
                Support = support,
                Efforts = image is not null ? ["low", "high", "max"] : null,
                Configurable = true,
                EnabledByDefault = modelId == "deepseek-reasoner" || assistantHistory,
                AssistantHistory = assistantHistory,
            },
            Sampling = new DeepSeekSamplingCapabilities
            {
                Temperature = thinkingOnly,
                TopP = thinkingOnly,
                Logprobs = support,
            },
        };

        return new DeepSeekLanguageModelEntry
        {
            Language = language,
            ProviderOptions = new Dictionary<string, ModelCapability>
            {
                ["logprobs"] = language.Sampling.Logprobs,
                ["topLogprobs"] = language.Sampling.Logprobs,
                ["userId"] = support,
                ["thinking"] = language.Reasoning.Support,
                ["reasoningEffort"] = language.Reasoning.Support,
                ["strictJsonSchema"] = ModelCapability.When(
                    "Requires a serving endpoint with JSON schema response formatting."
                ),
            },
        };
    }
}
