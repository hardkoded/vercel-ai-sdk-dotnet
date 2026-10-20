// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Mistral;

/// <summary>Mistral provider. OpenAI Chat Completions compatible at <c>https://api.mistral.ai/v1</c>.</summary>
public sealed class MistralProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "mistral";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.mistral.ai/v1";

    /// <summary>Environment variable for the API key.</summary>
    public const string ApiKeyVariable = "MISTRAL_API_KEY";

    /// <summary>Creates a provider.</summary>
    public MistralProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
    }

    /// <summary>Creates a provider. Pass a handler from tests.</summary>
    public static new MistralProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new MistralProvider(client, options);
    }

    private static readonly HashSet<string> ModelsWithoutReasoningEffort = new HashSet<string>
    {
        "codestral-2508",
        "codestral-latest",
        "ministral-14b-2512",
        "ministral-14b-latest",
        "ministral-3b-2512",
        "ministral-3b-latest",
        "ministral-8b-2512",
        "ministral-8b-latest",
        "mistral-code-fim-latest",
        "mistral-code-latest",
        "mistral-large-latest",
        "mistral-large-2512",
        "voxtral-small-2507",
        "voxtral-small-latest",
    };

    private static readonly Dictionary<string, string> EffortMap = new Dictionary<string, string>
    {
        { "minimal", "high" },
        { "low", "high" },
        { "medium", "high" },
        { "high", "high" },
        { "xhigh", "high" },
        { "max", "high" },
    };

    private static string? ResolveReasoningEffort(string modelId, string? reasoning, string? providerEffort, IList<CallWarning> warnings)
    {
        if (ModelsWithoutReasoningEffort.Contains(modelId))
        {
            if (ReasoningMap.IsCustomReasoning(reasoning))
            {
                warnings.Add(new CallWarning("unsupported", "reasoning", "This model does not support reasoning configuration."));
            }

            return null;
        }

        if (providerEffort != null)
        {
            return providerEffort;
        }

        if (!ReasoningMap.IsCustomReasoning(reasoning))
        {
            return null;
        }

        if (reasoning == "none")
        {
            return "none";
        }

        var mappingWarnings = new List<ModelWarning>();
        var effort = ReasoningMap.MapReasoningToProviderEffort(reasoning!, EffortMap, mappingWarnings);
        foreach (var warning in mappingWarnings)
        {
            if (ReasoningMap.ToCallWarning(warning) is { } callWarning)
            {
                warnings.Add(callWarning);
            }
        }

        return effort;
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options = options?.Copy() ?? new OpenAICompatibleOptions();
        if (string.IsNullOrEmpty(options.ProviderName) || options.ProviderName == "openai-compatible")
        {
            options.ProviderName = ProviderId;
        }

        if (options.BaseUrl == "https://api.openai.com/v1")
        {
            options.BaseUrl = DefaultBaseUrl;
        }

        if (options.ApiKeyEnvironmentVariable == "OPENAI_API_KEY")
        {
            options.ApiKeyEnvironmentVariable = ApiKeyVariable;
        }

        options.ResolveReasoningEffort ??= ResolveReasoningEffort;
        options.SupportsEmbeddings = true;
        options.SupportsImages = false;
        options.RequireApiKey = true;
        if (string.IsNullOrEmpty(options.UserAgent))
        {
            options.UserAgent = OpenAICompatibleInfo.UserAgent(ProviderId);
        }
        return options;
    }
}

/// <summary>Registers <see cref="MistralProvider"/>.</summary>
public static class MistralServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddMistral(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(MistralProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new MistralProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(MistralProvider.ProviderId), options);
        });
        return services;
    }
}
