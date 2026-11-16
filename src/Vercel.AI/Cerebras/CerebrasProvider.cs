// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Cerebras;

/// <summary>Cerebras provider. OpenAI Chat Completions compatible at <c>https://api.cerebras.ai/v1</c>.</summary>
public sealed class CerebrasProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "cerebras";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.cerebras.ai/v1";

    /// <summary>Environment variable for the API key.</summary>
    public const string ApiKeyVariable = "CEREBRAS_API_KEY";

    /// <summary>Creates a provider.</summary>
    public CerebrasProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient) { }

    /// <summary>Creates a provider. Pass a handler from tests.</summary>
    public static new CerebrasProvider Create(
        OpenAICompatibleOptions? options = null,
        HttpMessageHandler? handler = null
    )
    {
        var client = handler is null
            ? new HttpClient()
            : new HttpClient(handler, disposeHandler: false);
        return new CerebrasProvider(client, options);
    }

    // Upstream sends provider options to Cerebras without validating them, so an explicit null reasoningEffort reaches the body.
    private static JsonObject PassThroughNullReasoningEffort(
        string modelId,
        LanguageModelCallOptions options,
        JsonObject body,
        IList<CallWarning> warnings
    )
    {
        if (
            !body.ContainsKey("reasoning_effort")
            && options.ProviderOptions is not null
            && options.ProviderOptions.TryGetValue(ProviderId, out var cerebras)
            && cerebras.ValueKind == JsonValueKind.Object
            && cerebras.TryGetProperty("reasoningEffort", out var effort)
            && effort.ValueKind == JsonValueKind.Null
        )
        {
            body["reasoning_effort"] = null;
        }

        return body;
    }

    // Upstream runs applyQueueThresholdHeader in CerebrasChatLanguageModel. The valid range is 50 to 20000 milliseconds.
    private static IReadOnlyDictionary<string, string?>? ApplyQueueThresholdHeader(
        LanguageModelCallOptions options
    )
    {
        if (
            options.ProviderOptions is null
            || !options.ProviderOptions.TryGetValue(ProviderId, out var cerebras)
            || cerebras.ValueKind != JsonValueKind.Object
            || !cerebras.TryGetProperty("queueThreshold", out var threshold)
            || threshold.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
        )
        {
            return options.Headers;
        }

        if (
            threshold.ValueKind != JsonValueKind.Number
            || !threshold.TryGetDouble(out var value)
            || Math.Floor(value) != value
            || value < 50
            || value > 20000
        )
        {
            throw new InvalidArgumentException(
                "providerOptions",
                threshold.ToString(),
                "queueThreshold must be an integer between 50 and 20000."
            );
        }

        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (options.Headers is not null)
        {
            foreach (var pair in options.Headers)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        headers["queue_threshold"] = ((long)value).ToString(CultureInfo.InvariantCulture);
        return headers;
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options = options?.Copy() ?? new OpenAICompatibleOptions();
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
        options.RequireApiKey = true;
        if (string.IsNullOrEmpty(options.UserAgent))
        {
            options.UserAgent = OpenAICompatibleInfo.UserAgent(ProviderId);
        }
        options.SupportsStructuredOutputs = true;
        options.JsonTextOverridesToolCalls = true;
        options.TransformRequestBody = OpenAICompatibleTransforms.Cerebras;
        options.MetadataExtractor = CerebrasMetadataExtractor.Instance;
        options.PrepareCallHeaders = ApplyQueueThresholdHeader;
        options.TransformChatRequest = PassThroughNullReasoningEffort;
        options.SelectErrorMessage = OpenAICompatibleTransforms.CerebrasError;
        return options;
    }
}

/// <summary>Registers <see cref="CerebrasProvider"/>.</summary>
public static class CerebrasServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddCerebras(
        this IServiceCollection services,
        Action<OpenAICompatibleOptions>? configure = null
    )
    {
        services.AddHttpClient(CerebrasProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new CerebrasProvider(
                sp.GetRequiredService<IHttpClientFactory>()
                    .CreateClient(CerebrasProvider.ProviderId),
                options
            );
        });
        return services;
    }
}
