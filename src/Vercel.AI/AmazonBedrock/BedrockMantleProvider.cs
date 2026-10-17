// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Settings for the OpenAI-compatible Bedrock Mantle endpoint.</summary>
public sealed class BedrockMantleOptions
{
    /// <summary>AWS region. Defaults to <c>us-east-1</c>.</summary>
    public string Region { get; set; } = "us-east-1";

    /// <summary>Bearer token. When set, SigV4 is not used. Falls back to <c>AWS_BEARER_TOKEN_BEDROCK</c>.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Access key. Falls back to <c>AWS_ACCESS_KEY_ID</c>.</summary>
    public string? AccessKeyId { get; set; }

    /// <summary>Secret key. Falls back to <c>AWS_SECRET_ACCESS_KEY</c>.</summary>
    public string? SecretAccessKey { get; set; }

    /// <summary>Session token. Falls back to <c>AWS_SESSION_TOKEN</c>.</summary>
    public string? SessionToken { get; set; }

    /// <summary>Origin including the version path. When empty, the regional Mantle host is used.</summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Supplies SigV4 credentials for each request. When set, its keys are used instead of
    /// <see cref="AccessKeyId"/>, <see cref="SecretAccessKey"/>, and <see cref="SessionToken"/>.
    /// The region always comes from <see cref="Region"/>.
    /// </summary>
    public Func<CancellationToken, Task<AmazonBedrockCredentials>>? CredentialProvider { get; set; }

    /// <summary>Clock used for signing.</summary>
    public Func<DateTimeOffset>? UtcNow { get; set; }

    /// <summary>Extra headers sent on every call. The SDK user-agent suffix is appended.</summary>
    public Dictionary<string, string> Headers { get; } = new();
}

/// <summary>Mantle URL rules. Some model ids are served under <c>/openai/v1</c>.</summary>
public static class BedrockMantleRoutes
{
    /// <summary>True for Mantle models served on the OpenAI route, excluding <c>openai.gpt-oss-</c>.</summary>
    public static bool UsesOpenAiRoute(string modelId)
    {
        if (modelId == null)
        {
            return false;
        }

        if (modelId.StartsWith("openai.gpt-", StringComparison.Ordinal) && !modelId.StartsWith("openai.gpt-oss-", StringComparison.Ordinal))
        {
            return true;
        }

        if (modelId.StartsWith("google.gemma-4", StringComparison.Ordinal) || modelId.StartsWith("xai.", StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }

    /// <summary>Joins the Mantle origin and <paramref name="path"/> for <paramref name="modelId"/>.</summary>
    public static string Url(string region, string? baseUrl, string modelId, string path)
    {
        string origin;
        if (!string.IsNullOrEmpty(baseUrl))
        {
            origin = baseUrl!.TrimEnd('/');
        }
        else
        {
            if (!HostnameParts.IsValidHostnamePart(region))
            {
                throw new ArgumentException("An AWS region must be a single DNS label.", nameof(region));
            }

            origin = "https://bedrock-mantle." + region + ".api.aws/" + (UsesOpenAiRoute(modelId) ? "openai/v1" : "v1");
        }

        var relative = path.StartsWith("/", StringComparison.Ordinal) ? path : "/" + path;
        return origin + relative;
    }
}

/// <summary>Bedrock Mantle provider. Chat calls use <c>/chat/completions</c> and are signed for <c>bedrock-mantle</c>.</summary>
public sealed class BedrockMantleProvider : ProviderBase
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "bedrock-mantle.chat";

    /// <summary>Creates a provider.</summary>
    public BedrockMantleProvider(HttpClient httpClient, BedrockMantleOptions? options = null)
        : base(ProviderId)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        Options = options ?? new BedrockMantleOptions();
    }

    /// <summary>Options.</summary>
    public BedrockMantleOptions Options { get; }

    /// <summary>HTTP client that receives the signed request.</summary>
    public HttpClient HttpClient { get; }

    /// <summary>Creates a provider. Pass a handler from tests.</summary>
    public static BedrockMantleProvider Create(BedrockMantleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new BedrockMantleProvider(client, options);
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        return new BedrockMantleLanguageModel(this, modelId, responses: false);
    }

    /// <summary>Chat Completions model. Same route as <see cref="LanguageModel"/>.</summary>
    public ILanguageModel Chat(string modelId)
    {
        return LanguageModel(modelId);
    }

    /// <summary>Throws because the JavaScript provider function rejects <c>new</c>.</summary>
    public ILanguageModel New(string modelId)
    {
        throw new InvalidOperationException("The Bedrock Mantle model function cannot be called with the new keyword.");
    }

    /// <summary>Mantle has no embedding models.</summary>
    public override IEmbeddingModel EmbeddingModel(string modelId)
    {
        throw new NoSuchModelError(modelId, "embeddingModel");
    }

    /// <summary>Mantle has no image models.</summary>
    public override IImageModel ImageModel(string modelId)
    {
        throw new NoSuchModelError(modelId, "imageModel");
    }

    /// <summary>Responses API model. The path is <c>/responses</c> on the same Mantle origin.</summary>
    public ILanguageModel Responses(string modelId)
    {
        return new BedrockMantleLanguageModel(this, modelId, responses: true);
    }
}

/// <summary>
/// Mantle language model. Delegates to the OpenAI Chat Completions or Responses model, with the request
/// signed for <c>bedrock-mantle</c>.
/// </summary>
public sealed class BedrockMantleLanguageModel : ILanguageModel
{
    private readonly BedrockMantleProvider _provider;
    private readonly bool _responses;

    /// <summary>Creates a model.</summary>
    public BedrockMantleLanguageModel(BedrockMantleProvider provider, string modelId, bool responses)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _responses = responses;
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => _responses ? "bedrock-mantle.responses" : BedrockMantleProvider.ProviderId;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        return Inner().DoGenerateAsync(options, cancellationToken);
    }

    /// <inheritdoc />
    public Task<LanguageModelStreamResult> DoStreamAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        return Inner().DoStreamAsync(options, cancellationToken);
    }

    private ILanguageModel Inner()
    {
        var options = _provider.Options;
        var baseUrl = BedrockMantleRoutes.Url(options.Region, options.BaseUrl, ModelId, string.Empty);
        var apiKey = options.ApiKey ?? Environment.GetEnvironmentVariable("AWS_BEARER_TOKEN_BEDROCK");
        var signing = new AmazonBedrockSigningHandler(
            new HttpClientMessageHandler(_provider.HttpClient),
            options.Region,
            "bedrock-mantle",
            apiKey,
            options.AccessKeyId,
            options.SecretAccessKey,
            options.SessionToken,
            options.UtcNow,
            options.CredentialProvider);
        var openAI = new OpenAIOptions
        {
            ProviderName = "bedrock-mantle",
            BaseUrl = baseUrl,

            // The signing handler replaces the Authorization header, so any non-empty key does.
            ApiKey = "bedrock-mantle",
        };
        var headers = options.Headers.Select(pair => new KeyValuePair<string, string?>(pair.Key, pair.Value));
        foreach (var header in ProviderValues.WithUserAgentSuffix(headers, OpenAICompatibleInfo.UserAgent("amazon-bedrock")))
        {
            openAI.Headers[header.Key] = header.Value;
        }

        var provider = new OpenAIProvider(new HttpClient(signing, disposeHandler: false), openAI);
        return _responses
            ? new OpenAIResponsesLanguageModel(provider, ModelId) { SupportsWebSearchSourcesInclude = false }
            : provider.ChatModel(ModelId);
    }
}
