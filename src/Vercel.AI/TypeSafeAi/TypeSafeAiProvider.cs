// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.TypeSafeAi;

/// <summary>TypeSafe provider settings.</summary>
public sealed class TypeSafeAiOptions
{
    /// <summary>API key. Falls back to <c>TYPESAFE_AI_API_KEY</c> at call time.</summary>
    public string? ApiKey { get; set; }

    /// <summary>API base URL. Defaults to <c>https://api.typesafe.ai/v1</c>.</summary>
    public string? BaseUrl { get; set; }

    /// <summary>Headers sent with every request.</summary>
    public IDictionary<string, string>? Headers { get; set; }
}

/// <summary>TypeSafe's experimental evaluation capability. It has no language, embedding, or image models.</summary>
public sealed class TypeSafeAiProvider
{
    /// <summary>Provider id of the evaluation model.</summary>
    public const string EvaluationProviderId = "typesafe.evaluation";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.typesafe.ai/v1";

    /// <summary>Environment variable for the API key.</summary>
    public const string ApiKeyEnvironmentVariable = "TYPESAFE_AI_API_KEY";

    private readonly TypeSafeAiOptions _options;
    private readonly HttpClient? _httpClient;
    private readonly string _baseUrl;

    /// <summary>Creates a provider.</summary>
    public TypeSafeAiProvider(TypeSafeAiOptions? options = null, HttpClient? httpClient = null)
    {
        _options = options ?? new TypeSafeAiOptions();
        _httpClient = httpClient;
        _baseUrl = string.IsNullOrEmpty(_options.BaseUrl) ? DefaultBaseUrl : _options.BaseUrl!.TrimEnd('/');
    }

    /// <summary>Provider that reads <c>TYPESAFE_AI_API_KEY</c> when a request is made.</summary>
    public static TypeSafeAiProvider Default { get; } = new TypeSafeAiProvider();

    /// <summary>Specification version.</summary>
    public string SpecificationVersion => "v4";

    /// <summary>Creates a provider. Pass a handler from tests.</summary>
    public static TypeSafeAiProvider Create(TypeSafeAiOptions? options = null, HttpMessageHandler? handler = null)
    {
        return new TypeSafeAiProvider(options, handler is null ? null : new HttpClient(handler, disposeHandler: false));
    }

    /// <summary>Returns an evaluation model. The id may be one this library does not know yet.</summary>
    public TypeSafeAiEvaluationModel EvaluationModel(string modelId)
    {
        return new TypeSafeAiEvaluationModel(modelId, new TypeSafeAiEvaluationModelConfig
        {
            Provider = EvaluationProviderId,
            BaseUrl = _baseUrl,
            Headers = Headers,
            HttpClient = _httpClient,
        });
    }

    /// <summary>TypeSafe has no language models.</summary>
    public ILanguageModel LanguageModel(string modelId)
    {
        throw new NoSuchModelError(modelId, "languageModel");
    }

    /// <summary>TypeSafe has no embedding models.</summary>
    public IEmbeddingModel EmbeddingModel(string modelId)
    {
        throw new NoSuchModelError(modelId, "embeddingModel");
    }

    /// <summary>TypeSafe has no image models.</summary>
    public IImageModel ImageModel(string modelId)
    {
        throw new NoSuchModelError(modelId, "imageModel");
    }

    private IReadOnlyDictionary<string, string> Headers()
    {
        var headers = new Dictionary<string, string>
        {
            ["Authorization"] = "Bearer " + ApiKeys.Require(_options.ApiKey, ApiKeyEnvironmentVariable),
        };
        if (_options.Headers != null)
        {
            foreach (var pair in _options.Headers)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        headers["User-Agent"] = ProviderExchange.UserAgent("typesafe-ai");
        return headers;
    }
}
