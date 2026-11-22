// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Groq;

/// <summary>Groq provider. OpenAI Chat Completions compatible at <c>https://api.groq.com/openai/v1</c>.</summary>
public sealed class GroqProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "groq";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.groq.com/openai/v1";

    /// <summary>Environment variable for the API key.</summary>
    public const string ApiKeyVariable = "GROQ_API_KEY";

    /// <summary>Creates a provider.</summary>
    public GroqProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient) { }

    /// <summary>Creates a provider. Pass a handler from tests.</summary>
    public static new GroqProvider Create(
        OpenAICompatibleOptions? options = null,
        HttpMessageHandler? handler = null
    )
    {
        var client = handler is null
            ? new HttpClient()
            : new HttpClient(handler, disposeHandler: false);
        return new GroqProvider(client, options);
    }

    /// <inheritdoc />
    public override ITranscriptionModel TranscriptionModel(string modelId) =>
        new GroqTranscriptionModel(this, modelId);

    private static string? ResolveReasoningEffort(
        string modelId,
        string? reasoning,
        string? providerEffort,
        IList<CallWarning> warnings
    )
    {
        if (providerEffort != null || !ReasoningMap.IsCustomReasoning(reasoning))
        {
            return providerEffort;
        }

        var supportsNoneReasoning = modelId == "qwen/qwen3.6-27b";

        // GPT-OSS models always reason, so reasoning "none" maps to the lowest effort. Other models do not reason,
        // so there is nothing to disable.
        var alwaysReasons = modelId.StartsWith("openai/gpt-oss", StringComparison.Ordinal);
        if (reasoning == "none" && !supportsNoneReasoning && !alwaysReasons)
        {
            warnings.Add(
                new CallWarning(
                    "unsupported",
                    "reasoning",
                    "reasoning \"" + reasoning + "\" is not supported by this model."
                )
            );
            return null;
        }

        var effortMap = new Dictionary<string, string>
        {
            { "low", "low" },
            { "medium", "medium" },
            { "high", "high" },
        };
        if (supportsNoneReasoning)
        {
            effortMap["none"] = "none";
        }

        var mappingWarnings = new List<ModelWarning>();
        var effort = ReasoningMap.MapReasoningToProviderEffort(
            reasoning,
            effortMap,
            mappingWarnings
        );
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
        options.SupportedUrls ??= new Dictionary<string, string>
        {
            ["image/*"] = "^https?://.*\\z",
        };

        if (
            string.IsNullOrEmpty(options.ProviderName)
            || options.ProviderName == "openai-compatible"
        )
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

        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.InterruptedWithoutFinish = true;
        options.RequireApiKey = true;
        if (string.IsNullOrEmpty(options.UserAgent))
        {
            options.UserAgent = OpenAICompatibleInfo.UserAgent(ProviderId);
        }
        options.SupportsStructuredOutputs = true;
        options.UsesJsonResponseToolWithTools = true;
        options.TransformChatRequest = (_, call, body, _) =>
        {
            body["messages"] = ConvertToGroqChatMessages.Convert(call.Prompt);
            return body;
        };
        options.ResolveReasoningEffort ??= ResolveReasoningEffort;
        return options;
    }
}

/// <summary>Registers <see cref="GroqProvider"/>.</summary>
public static class GroqServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddGroq(
        this IServiceCollection services,
        Action<OpenAICompatibleOptions>? configure = null
    )
    {
        services.AddHttpClient(GroqProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new GroqProvider(
                sp.GetRequiredService<IHttpClientFactory>().CreateClient(GroqProvider.ProviderId),
                options
            );
        });
        return services;
    }
}
