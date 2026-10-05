// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Google;

/// <summary>Anthropic on Vertex. Builds the rawPredict URL and the <c>anthropic_version</c> body field.</summary>
public sealed class GoogleVertexAnthropicProvider
{
    /// <summary>Creates a provider.</summary>
    public GoogleVertexAnthropicProvider(string? project, string? location, string? baseUrl = null, string? accessToken = null)
    {
        Project = project;
        Location = location;
        BaseUrl = GoogleVertexEndpoints.AnthropicBaseUrl(project, location, baseUrl);
        AccessToken = accessToken;
    }

    /// <summary>Project id.</summary>
    public string? Project { get; }

    /// <summary>Location.</summary>
    public string? Location { get; }

    /// <summary>Publisher base URL.</summary>
    public string BaseUrl { get; }

    /// <summary>Bearer token, when the caller already has one.</summary>
    public string? AccessToken { get; }

    /// <summary>Headers. A caller-supplied Authorization value is kept.</summary>
    public IReadOnlyDictionary<string, string?> Headers(IReadOnlyDictionary<string, string?>? extra = null, Func<string?>? token = null)
    {
        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrEmpty(AccessToken))
        {
            headers["Authorization"] = "Bearer " + AccessToken;
        }
        else if (token != null)
        {
            var value = token();
            if (!string.IsNullOrEmpty(value))
            {
                headers["Authorization"] = "Bearer " + value;
            }
        }

        if (extra != null)
        {
            foreach (var pair in extra)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        return headers;
    }

    /// <summary><c>rawPredict</c> or <c>streamRawPredict</c> URL for a model.</summary>
    public string PredictUrl(string modelId, bool streaming)
    {
        return BaseUrl.TrimEnd('/') + "/" + modelId + ":" + (streaming ? "streamRawPredict" : "rawPredict");
    }

    /// <summary>Removes <c>model</c> and sets <c>anthropic_version</c>.</summary>
    public static JsonObject TransformBody(JsonObject body)
    {
        var clone = (JsonObject)body.DeepClone();
        clone.Remove("model");
        clone["anthropic_version"] = "vertex-2023-10-16";
        return clone;
    }

    /// <summary>Embedding models are not available on this provider.</summary>
    public IEmbeddingModel EmbeddingModel(string modelId)
    {
        throw new AiSdkException("Provider 'google.vertex.anthropic' does not support embedding model '" + modelId + "'.");
    }
}

/// <summary>Vertex MaaS OpenAI-compatible endpoint.</summary>
public sealed class GoogleVertexMaasProvider
{
    private static readonly Dictionary<string, int> MaxTokens = new(StringComparer.Ordinal)
    {
        ["meta/llama-4-maverick-17b-128e-instruct-maas"] = 8192,
        ["meta/llama-4-scout-17b-16e-instruct-maas"] = 8192,
    };

    /// <summary>Creates a provider. The OpenAI-compatible client is created on first use.</summary>
    public GoogleVertexMaasProvider(string project, string? location, string? baseUrl = null, HttpMessageHandler? handler = null)
    {
        Project = project ?? throw new ArgumentNullException(nameof(project));
        Location = string.IsNullOrEmpty(location) ? "global" : location!;
        BaseUrl = GoogleVertexEndpoints.MaasBaseUrl(Project, Location, baseUrl);
        Handler = handler;
    }

    /// <summary>Project id.</summary>
    public string Project { get; }

    /// <summary>Location. Defaults to <c>global</c>.</summary>
    public string Location { get; }

    /// <summary>OpenAI-compatible base URL.</summary>
    public string BaseUrl { get; }

    /// <summary>True after <see cref="LanguageModel"/> has created the inner provider.</summary>
    public bool ClientCreated { get; private set; }

    private HttpMessageHandler? Handler { get; }

    private OpenAICompatibleProvider? _client;

    /// <summary>Returns a chat model and caches the inner provider.</summary>
    public ILanguageModel LanguageModel(string modelId)
    {
        ClientCreated = true;
        _client ??= OpenAICompatibleProvider.Create(new OpenAICompatibleOptions
        {
            BaseUrl = BaseUrl,
            ApiKey = "vertex",
            ProviderName = "vertex.maas",
        }, Handler);
        return _client.LanguageModel(modelId);
    }

    /// <summary>Sets Llama 4 <c>max_tokens</c> when the caller did not.</summary>
    public static JsonObject TransformBody(string modelId, JsonObject body)
    {
        var clone = (JsonObject)body.DeepClone();
        if (MaxTokens.TryGetValue(modelId, out var max) && !clone.ContainsKey("max_tokens"))
        {
            clone["max_tokens"] = max;
        }

        return clone;
    }
}

/// <summary>xAI Grok models hosted on Vertex.</summary>
public sealed class GoogleVertexXaiProvider
{
    /// <summary>Creates a provider.</summary>
    public GoogleVertexXaiProvider(string project, string? location, string? baseUrl = null, HttpMessageHandler? handler = null)
    {
        Project = project ?? throw new ArgumentNullException(nameof(project));
        Location = string.IsNullOrEmpty(location) ? "global" : location!;
        BaseUrl = GoogleVertexEndpoints.XaiBaseUrl(Project, Location, baseUrl);
        Handler = handler;
    }

    /// <summary>Project id.</summary>
    public string Project { get; }

    /// <summary>Location.</summary>
    public string Location { get; }

    /// <summary>OpenAI-compatible base URL.</summary>
    public string BaseUrl { get; }

    /// <summary>True after the inner provider has been created.</summary>
    public bool ClientCreated { get; private set; }

    private HttpMessageHandler? Handler { get; }

    private OpenAICompatibleProvider? _client;

    /// <summary>Returns a chat model.</summary>
    public ILanguageModel LanguageModel(string modelId)
    {
        ClientCreated = true;
        _client ??= OpenAICompatibleProvider.Create(new OpenAICompatibleOptions
        {
            BaseUrl = BaseUrl,
            ApiKey = "vertex",
            ProviderName = "googleVertex.xai",
        }, Handler);
        return _client.LanguageModel(modelId);
    }

    /// <summary>Removes <c>reasoning_effort</c>. Vertex Grok does not accept it.</summary>
    public static JsonObject TransformBody(JsonObject body)
    {
        var clone = (JsonObject)body.DeepClone();
        clone.Remove("reasoning_effort");
        return clone;
    }

    /// <summary>
    /// Counts Grok reasoning tokens separately. Output total is completion tokens plus reasoning tokens.
    /// </summary>
    public static GoogleTokenUsage ConvertUsage(int? promptTokens, int? completionTokens, int? cachedTokens, int? reasoningTokens)
    {
        var prompt = promptTokens ?? 0;
        var completion = completionTokens ?? 0;
        var cached = cachedTokens ?? 0;
        var reasoning = reasoningTokens ?? 0;
        return new GoogleTokenUsage(prompt, prompt - cached, cached, completion + reasoning, completion, reasoning, null);
    }

    /// <summary>Embedding and image models are not available.</summary>
    public IEmbeddingModel EmbeddingModel(string modelId)
    {
        throw new AiSdkException("Provider 'google.vertex.xai' does not support embedding model '" + modelId + "'.");
    }
}
