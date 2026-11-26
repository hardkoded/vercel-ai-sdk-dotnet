// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;

namespace Vercel.AI.Moonshot;

/// <summary>Moonshot provider. OpenAI Chat Completions compatible at <c>https://api.moonshot.ai/v1</c>.</summary>
public sealed class MoonshotProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "moonshotai";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.moonshot.ai/v1";

    /// <summary>Environment variable for the API key.</summary>
    public const string ApiKeyVariable = "MOONSHOT_API_KEY";

    /// <summary>Creates a provider.</summary>
    public MoonshotProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient) { }

    /// <summary>Whether <paramref name="modelId"/> accepts JSON schema responses. Every non-empty id does.</summary>
    public static bool SupportsStructuredOutputs(string modelId)
    {
        if (string.IsNullOrEmpty(modelId))
        {
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    public override OpenAICompatibleLanguageModel CreateChatModel(string modelId)
    {
        var model = base.CreateChatModel(modelId);
        model.SupportsStructuredOutputs = SupportsStructuredOutputs(modelId);
        return model;
    }

    /// <summary>Creates a provider. Pass a handler from tests.</summary>
    public static new MoonshotProvider Create(
        OpenAICompatibleOptions? options = null,
        HttpMessageHandler? handler = null
    )
    {
        var client = handler is null
            ? new HttpClient()
            : new HttpClient(handler, disposeHandler: false);
        return new MoonshotProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options = options?.Copy() ?? new OpenAICompatibleOptions();
        options.SupportedUrls ??= new Dictionary<string, string>
        {
            ["image/*"] = "^ms://",
            ["video/*"] = "^ms://",
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
        options.IncludeUsage = true;
        options.ResolveReasoningEffort = (_, _, _, _) => null;
        options.ConvertUsage = usage => MoonshotChatUsage.ConvertMoonshotAIChatUsage(usage);
        options.ExtractResponseHeaderMetadata = MoonshotChatUsage.ExtractCacheWriteTokensByTtl;
        options.TransformChatRequest = MoonshotChatRequest.Apply;
        options.PreparesOwnTools = true;
        options.ConvertChatMessages = (
            modelId,
            call,
            providerOptionsName,
            responseFormatType,
            warnings
        ) =>
        {
            var converted = MoonshotChatMessages.ConvertToMoonshotAIChatMessages(
                call.Prompt,
                modelId,
                providerOptionsName,
                responseFormatType
            );
            foreach (var warning in converted.Warnings)
            {
                warnings.Add(warning);
            }

            return converted.Messages;
        };
        options.ValidateChatResponse = MoonshotChatApiTypes.ValidateChatResponse;
        options.ValidateChatChunk = MoonshotChatApiTypes.ChatChunkError;
        options.NormalizeRequestSchema = MoonshotJsonSchema.NormalizeJsonSchemaForMFJS;
        options.NormalizeResponseSchema = MoonshotJsonSchema.NormalizeResponseSchema;
        return options;
    }
}

/// <summary>Registers <see cref="MoonshotProvider"/>.</summary>
public static class MoonshotServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddMoonshot(
        this IServiceCollection services,
        Action<OpenAICompatibleOptions>? configure = null
    )
    {
        services.AddHttpClient(MoonshotProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new MoonshotProvider(
                sp.GetRequiredService<IHttpClientFactory>()
                    .CreateClient(MoonshotProvider.ProviderId),
                options
            );
        });
        return services;
    }
}
