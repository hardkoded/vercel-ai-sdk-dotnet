// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Baseten;

/// <summary>Baseten provider. OpenAI Chat Completions compatible at <c>https://inference.baseten.co/v1</c>.</summary>
public sealed class BasetenProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "baseten";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://inference.baseten.co/v1";

    /// <summary>Environment variable for the API key.</summary>
    public const string ApiKeyVariable = "BASETEN_API_KEY";

    /// <summary>Creates a provider.</summary>
    public BasetenProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
    }

    /// <inheritdoc />
    public override OpenAICompatibleLanguageModel CreateChatModel(string modelId)
    {
        var url = (Options as BasetenOptions)?.ModelUrl;
        if (!string.IsNullOrEmpty(url) && url!.IndexOf("/predict", StringComparison.Ordinal) >= 0)
        {
            throw new AiSdkException("Not supported. You must use a /sync/v1 endpoint for chat models.");
        }

        var deployment = !string.IsNullOrEmpty(url) && url!.IndexOf("/sync/v1", StringComparison.Ordinal) >= 0;
        var model = base.CreateChatModel(string.IsNullOrEmpty(modelId) ? (deployment ? "placeholder" : "chat") : modelId);
        if (deployment)
        {
            model.Endpoint = ApiKeys.Combine(url!, "chat/completions");
        }

        return model;
    }

    /// <inheritdoc />
    public override IEmbeddingModel EmbeddingModel(string modelId)
    {
        var url = (Options as BasetenOptions)?.ModelUrl;
        if (string.IsNullOrEmpty(url))
        {
            throw new AiSdkException("No model URL provided for embeddings. Please set modelURL option for embeddings.");
        }

        if (url!.IndexOf("/sync", StringComparison.Ordinal) < 0
            || url.IndexOf("/predict", StringComparison.Ordinal) >= 0)
        {
            throw new AiSdkException("Not supported. You must use a /sync or /sync/v1 endpoint for embeddings.");
        }

        var baseUrl = url!;
        if (baseUrl.IndexOf("/sync/v1", StringComparison.Ordinal) < 0)
        {
            baseUrl = baseUrl.TrimEnd('/') + "/v1";
        }

        var model = new OpenAICompatibleEmbeddingModel(this, string.IsNullOrEmpty(modelId) ? "embeddings" : modelId);
        model.Endpoint = ApiKeys.Combine(baseUrl, "embeddings");
        model.MaxEmbeddingsPerCall = 128;
        return model;
    }

    /// <summary>Creates a provider. Pass a handler from tests.</summary>
    public static new BasetenProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new BasetenProvider(client, options);
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

        options.SupportsEmbeddings = true;
        options.SupportsImages = false;
        options.RequireApiKey = true;
        if (string.IsNullOrEmpty(options.UserAgent))
        {
            options.UserAgent = OpenAICompatibleInfo.UserAgent(ProviderId);
        }
        options.IncludeUsage = true;
        options.SelectErrorMessage = OpenAICompatibleTransforms.BasetenError;
        options.SupportsStructuredOutputs = true;
        options.MaxEmbeddingsPerCall = 128;
        var modelUrl = (options as BasetenOptions)?.ModelUrl;
        if (string.IsNullOrEmpty(modelUrl) || modelUrl!.IndexOf("/sync/v1", StringComparison.Ordinal) < 0)
        {
            options.TransformRequestBody = TransformRequestBody;
        }

        return options;
    }

    private const string DeepSeekV4ProModel = "deepseek-ai/DeepSeek-V4-Pro-0813";

    private static JsonObject TransformRequestBody(JsonObject body, IList<CallWarning> warnings)
    {
        var responseFormat = body["response_format"] as JsonObject;
        var formatType = TextOf(responseFormat?["type"]);
        if (TextOf(body["model"]) != DeepSeekV4ProModel
            || TextOf(body["reasoning_effort"]) == "none"
            || (formatType != "json_schema" && formatType != "json_object"))
        {
            return body;
        }

        // With JSON mode enabled, Baseten returns this model's answer as reasoning
        // and leaves content empty. Keep reasoning and prompt for JSON instead.
        warnings.Add(new CallWarning(
            "compatibility",
            "responseFormat",
            "JSON instructions are injected into the system message and response_format is omitted because Baseten DeepSeek V4 Pro 0813 returns JSON as reasoning when JSON mode and reasoning are combined. The response is not constrained by the server."));

        JsonElement? schema = null;
        if (responseFormat!["json_schema"] is JsonObject jsonSchema && jsonSchema["schema"] is { } schemaNode)
        {
            using var document = JsonDocument.Parse(schemaNode.ToJsonString());
            schema = document.RootElement.Clone();
        }

        body.Remove("response_format");
        var messages = body["messages"] as JsonArray ?? new JsonArray();
        body.Remove("messages");
        var rewritten = new JsonArray
        {
            new JsonObject
            {
                ["role"] = "system",
                ["content"] = Vercel.AI.GenerateText.JsonInstructions.InjectJsonInstruction(null, schema),
            },
        };
        foreach (var message in messages)
        {
            rewritten.Add(message?.DeepClone());
        }

        body["messages"] = rewritten;
        return body;
    }

    private static string? TextOf(JsonNode? node)
    {
        return node is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;
    }
}

/// <summary>Baseten settings, including an optional dedicated deployment URL.</summary>
public sealed class BasetenOptions : OpenAICompatibleOptions
{
    /// <summary>Dedicated deployment URL. Chat requires <c>/sync/v1</c>. Embeddings require <c>/sync</c>.</summary>
    public string? ModelUrl { get; set; }
}

/// <summary>Registers <see cref="BasetenProvider"/>.</summary>
public static class BasetenServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddBaseten(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(BasetenProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new BasetenProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(BasetenProvider.ProviderId), options);
        });
        return services;
    }
}
